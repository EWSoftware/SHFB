// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/24/2012 - Removed support for the link configuration attribute and simplified the FileCreatedEventArgs
// class to make it more generic.  Components that handle the event arguments are responsible for determining
// additional file locations rather than relying on this component to tell them.
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF
// 03/18/2014 - EFW - Added support for the outputMethod attribute although the default output will remain XML
// 01/06/2016 - EFW - Updated to use a pipeline task for writing out the files for better performance

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This build component is used to save the generated document or parts of it to a file
    /// </summary>
    public class SaveComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Save Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SaveComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XmlWriterSettings settings = new XmlWriterSettings();
        private XPathExpression pathExpression, selectExpression;

        private string basePath, groupId;
        private bool writeXhtmlNamespace;

        private BlockingCollection<KeyValuePair<string, XmlDocument>> documentList;
        private Task documentWriter;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected SaveComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
            // No bounded capacity by default
            documentList = new BlockingCollection<KeyValuePair<string, XmlDocument>>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            int boundedCapacity;

            settings.Encoding = Encoding.UTF8;
            settings.CloseOutput = true;

            // Load the target path format
            XPathNavigator saveNode = configuration.SelectSingleNode("save");

            if(saveNode == null)
                throw new ConfigurationErrorsException("When instantiating a save component, you must specify " +
                    "a the target file using the <save> element.");

            string baseValue = saveNode.GetAttribute("base", String.Empty);

            if(!String.IsNullOrWhiteSpace(baseValue))
                basePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(baseValue));

            string pathValue = saveNode.GetAttribute("path", String.Empty);

            if(String.IsNullOrWhiteSpace(pathValue))
                this.WriteMessage(MessageLevel.Error, "Each save element must have a path attribute specifying " +
                    "an XPath expression that evaluates to the location to save the file.");

            pathExpression = XPathExpression.Compile(pathValue);

            string selectValue = saveNode.GetAttribute("select", String.Empty);

            if(!String.IsNullOrWhiteSpace(selectValue))
            {
                settings.ConformanceLevel = ConformanceLevel.Auto;
                selectExpression = XPathExpression.Compile(selectValue);
            }

            // If true, indent the rendered HTML (debugging aid)
            string indentValue = saveNode.GetAttribute("indent", String.Empty);

            if(!String.IsNullOrWhiteSpace(indentValue))
                settings.Indent = Convert.ToBoolean(indentValue, CultureInfo.InvariantCulture);

            // If true or not set, omit the XML declaration
            string omitValue = saveNode.GetAttribute("omit-xml-declaration", String.Empty);

            if(!String.IsNullOrWhiteSpace(omitValue))
                settings.OmitXmlDeclaration = Convert.ToBoolean(omitValue, CultureInfo.InvariantCulture);

            // If true, this adds a default namespace for XHTML.  Required by Help Viewer documentation.
            string addXhtmlDeclaration = saveNode.GetAttribute("add-xhtml-namespace", String.Empty);

            if(!String.IsNullOrWhiteSpace(addXhtmlDeclaration))
                writeXhtmlNamespace = Convert.ToBoolean(addXhtmlDeclaration, CultureInfo.InvariantCulture);

            // By default, the output is serialized using the default XML rules.  This attribute can be set to
            // override that with a different output method such as HTML.  While this will prevent it from
            // converting empty elements to self-closing elements, it can have unintended side-effects such as
            // converting self-closing elements to full opening/closing empty elements and using non-XHTML
            // compliant versions of elements such as <br>, <hr>, and <link>.  The empty element issue can be
            // solved using an "<xsl:comment />" or "<xsl:text> </xsl:text>" element in the XSL transformations
            // thus avoiding the other side-effects of enabling the HTML output method.
            string outputMethod = saveNode.GetAttribute("outputMethod", String.Empty);

            if(!String.IsNullOrWhiteSpace(outputMethod))
            {
                XmlOutputMethod method;

                if(!Enum.TryParse(outputMethod, true, out method))
                    throw new ConfigurationErrorsException("The specified output method is not valid for the " +
                        "save component");

                // This isn't a publicly settable property so we have to resort to reflection
                var propertyInfo = typeof(XmlWriterSettings).GetProperty("OutputMethod", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic);
                propertyInfo.SetValue(settings, method, null);
            }

            // Get an optional group ID for reporting documents remaining when disposed
            groupId = saveNode.GetAttribute("groupId", String.Empty);

            if(!String.IsNullOrWhiteSpace(groupId))
                groupId += " ";
            else
                groupId = String.Empty;

            // Allow limiting the writer task collection to conserve memory
            string capacity = saveNode.GetAttribute("boundedCapacity", String.Empty);

            if(!String.IsNullOrWhiteSpace(capacity) && Int32.TryParse(capacity, out boundedCapacity) &&
              boundedCapacity > 0)
                documentList = new BlockingCollection<KeyValuePair<string, XmlDocument>>(boundedCapacity);

            // Use a pipeline task to allow the actual saving to occur while other topics are being generated
            documentWriter = Task.Run(() => WriteDocuments());
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            this.documentList.Add(new KeyValuePair<string, XmlDocument>(key, document));
        }

        /// <summary>
        /// Wait for the document writer task to complete when disposed
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                documentList.CompleteAdding();

                if(documentWriter != null && documentWriter.Status != TaskStatus.Faulted)
                {
                    int count = documentList.Count;

                    if(count != 0)
                        this.WriteMessage(MessageLevel.Diagnostic, "Waiting for the document writer task to " +
                            "finish ({0} {1}file(s) remaining)...", count, groupId);

                    documentWriter.Wait();
                }

                documentList.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to write the document to its destination file
        /// </summary>
        private void WriteDocuments()
        {
            string lastKey = "??", path = null;

            try
            {
                foreach(var kv in documentList.GetConsumingEnumerable())
                {
                    var document = kv.Value;

                    lastKey = kv.Key;

                    // Set the evaluation context
                    CustomContext context = new CustomContext { ["key"] = lastKey };

                    XPathExpression xpath = pathExpression.Clone();
                    xpath.SetContext(context);

                    // Evaluate the path
                    path = document.CreateNavigator().Evaluate(xpath).ToString();

                    if(basePath != null)
                        path = Path.Combine(basePath, path);

                    string targetDirectory = Path.GetDirectoryName(path);

                    if(!Directory.Exists(targetDirectory))
                        Directory.CreateDirectory(targetDirectory);

                    if(writeXhtmlNamespace)
                    {
                        document.DocumentElement.SetAttribute("xmlns", "http://www.w3.org/1999/xhtml");
                        document.LoadXml(document.OuterXml);
                    }

                    // selectExpression determines which nodes get saved. If there is no selectExpression we simply
                    // save the root node as before. If there is a selectExpression, we evaluate the XPath expression
                    // and save the resulting node set. The select expression also enables the "literal-text" processing
                    // instruction, which outputs its content as unescaped text.
                    if(selectExpression == null)
                    {
                        using(XmlWriter writer = XmlWriter.Create(path, settings))
                        {
                            document.Save(writer);
                        }
                    }
                    else
                    {
                        // IMPLEMENTATION NOTE: The separate StreamWriter is used to maintain XML indenting.  Without it
                        // the XmlWriter won't honor our indent settings after plain text nodes have been written.
                        using(StreamWriter output = File.CreateText(path))
                        {
                            using(XmlWriter writer = XmlWriter.Create(output, settings))
                            {
                                XPathExpression selectXPath = selectExpression.Clone();
                                selectXPath.SetContext(context);

                                XPathNodeIterator ni = document.CreateNavigator().Select(selectExpression);

                                while(ni.MoveNext())
                                {
                                    if(ni.Current.NodeType == XPathNodeType.ProcessingInstruction &&
                                        ni.Current.Name.Equals("literal-text"))
                                    {
                                        writer.Flush();
                                        output.Write(ni.Current.Value);
                                    }
                                    else
                                        ni.Current.WriteSubtree(writer);
                                }
                            }
                        }
                    }

                    // Raise an event to indicate that a file was created
                    this.OnComponentEvent(new FileCreatedEventArgs(path, true));
                }
            }
            catch(IOException e)
            {
                documentList.CompleteAdding();
                this.WriteMessage(lastKey, MessageLevel.Error, "An access error occurred while attempting to " +
                    "save to the file '{0}'. The error message is '{1}'", path, e.GetExceptionMessage());
            }
            catch(XmlException e)
            {
                documentList.CompleteAdding();
                this.WriteMessage(lastKey, MessageLevel.Error, "Invalid XML was written to the output " +
                    "file '{0}'. The error message is '{1}'", path, e.GetExceptionMessage());
            }
            catch(Exception e)
            {
                documentList.CompleteAdding();
                this.WriteMessage(lastKey, MessageLevel.Error, "An error occurred trying to save the topic " +
                    "content. The error message is '{0}'", e.GetExceptionMessage());
            }
        }
        #endregion
    }
}
