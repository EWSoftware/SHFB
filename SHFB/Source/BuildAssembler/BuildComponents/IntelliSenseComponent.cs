//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : IntelliSenseComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/09/2016
// Note    : Copyright 2007-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to extract the XML comments into files that can be used for
// IntelliSense.  Only the basic set of tags needed for IntelliSense are exported and only for documented API
// members.  This is based on the Microsoft IntelliSense build component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/09/2007  EFW  Created the code
// 03/24/2008  EFW  Updated it to handle multiple assembly references
// 07/04/2009  EFW  Add parameter to Dispose() to match base class
// 12/21/2012  EFW  Replaced the Microsoft IntelliSense build component with my version
// 11/12/2013  EFW  Added support for exporting code contracts XML comments elements
// 12/23/2013  EFW  Updated the build component to be discoverable via MEF
// 01/07/2016  EFW  Updated to use a pipeline task for writing out the member comments for better performance
//===============================================================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.UI;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This build component is used to generate IntelliSense files based on the documented APIs.
    /// </summary>
    /// <remarks>Only the basic set of tags needed for IntelliSense are exported and only for documented API
    /// members.  This is based on the  Microsoft IntelliSense build component.  That version only works with
    /// Microsoft-specific XML comments files and does not work with general XML comments files created by the
    /// compilers.</remarks>
    /// <example>
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- IntelliSense component configuration.  This must appear
    ///      before the TransformComponent. --&gt;
    /// &lt;component id="IntelliSense Component"&gt;
    ///  &lt;!-- Output options (optional)
    ///       Attributes:
    ///          Include Namespaces (false by default)
    ///          Namespaces filename ("Namespaces" if not specified or empty)
    ///          Directory (current folder if not specified or empty) --&gt;
    ///          Bounded cache capacity (0 if not specified --&gt;
    ///  &lt;output includeNamespaces="false" namespacesFile="Namespaces"
    ///      folder="C:\ProjectDocs\" boundedCapacity="100" /&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class IntelliSenseComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("IntelliSense Component", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This build component is used to extract the XML comments into files that can be used " +
            "for IntelliSense.  Only the basic set of tags needed for IntelliSense are exported and only for " +
            "documented API members.")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Factory()
            {
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.After,
                    "Show Missing Documentation Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new IntelliSenseComponent(this.BuildAssembler);
            }

            /// <inheritdoc />
            public override string ConfigureComponent(string currentConfiguration, CompositionContainer container)
            {
                using(IntelliSenseConfigDlg dlg = new IntelliSenseConfigDlg(currentConfiguration))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        currentConfiguration = dlg.Configuration;
                }

                return currentConfiguration;
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<!-- Output options (optional)
  Attributes:
    Include namespaces (false by default)
    Namespaces comments filename (""Namespaces"" if not specified or empty)
    Output folder (current folder if not specified or empty) -->
<output includeNamespaces=""false"" namespacesFile=""Namespaces"" folder=""{@OutputFolder}"" />";
                }
            }

            /// <inheritdoc />
            /// <remarks>Indicate a dependency on the missing documentation component as it will produce more
            /// complete documentation with all the proper elements present.</remarks>
            public override IEnumerable<string> Dependencies
            {
                get
                {
                    return new List<string> { "Show Missing Documentation Component" };
                }
            }
        }
        #endregion

        #region Comments info
        //=====================================================================

        /// <summary>
        /// This is used to contain the XML comments elements information that will be written to the
        /// IntelliSense files.
        /// </summary>
        private class CommentsInfo
        {
            /// <summary>
            /// The assembly name
            /// </summary>
            public string AssemblyName { get; set; }

            /// <summary>
            /// The member name
            /// </summary>
            public string MemberName { get; set; }

            /// <summary>
            /// The summary element comments
            /// </summary>
            public XPathNavigator Summary { get; set; }

            /// <summary>
            /// The parameter element comments
            /// </summary>
            public XPathNodeIterator Params { get; set; }

            /// <summary>
            /// The type parameter element comments
            /// </summary>
            public XPathNodeIterator TypeParams { get; set; }

            /// <summary>
            /// The returns element comments
            /// </summary>
            public XPathNavigator Returns { get; set; }

            /// <summary>
            /// The exception element comments
            /// </summary>
            public XPathNodeIterator Exceptions { get; set; }

            /// <summary>
            /// The code contracts element comments
            /// </summary>
            public XPathNodeIterator CodeContracts { get; set; }

            /// <summary>
            /// For enumerated types, the enum member element summary comments
            /// </summary>
            public IEnumerable<KeyValuePair<string, XPathNavigator>> EnumElements { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="component">The component creating the instance</param>
            /// <param name="assemblyName">The assembly name</param>
            /// <param name="memberName">The member name</param>
            /// <param name="comments">The XPath navigator from which to extract the comments information</param>
            public CommentsInfo(IntelliSenseComponent component, string assemblyName, string memberName,
              XPathNavigator comments)
            {
                this.AssemblyName = assemblyName;
                this.MemberName = memberName;
                this.Summary = comments.SelectSingleNode(component.summaryExpression);
                this.Params = comments.Select(component.paramExpression);
                this.TypeParams = comments.Select(component.typeparamExpression);
                this.Returns = comments.SelectSingleNode(component.returnsExpression);
                this.Exceptions = comments.Select(component.exceptionExpression);
                this.CodeContracts = comments.Select(component.codeContractsExpression);

                if((string)comments.Evaluate(component.subgroupExpression) == "enumeration")
                {
                    var enums = new List<KeyValuePair<string, XPathNavigator>>();
                    this.EnumElements = enums;

                    foreach(XPathNavigator nav in (XPathNodeIterator)comments.Evaluate(component.elementsExpression))
                    {
                        string enumName = nav.GetAttribute("api", String.Empty);
                        var summaryComments = nav.SelectSingleNode(component.summaryExpression);

                        if(!String.IsNullOrWhiteSpace(enumName) && summaryComments != null)
                            enums.Add(new KeyValuePair<string, XPathNavigator>(enumName, summaryComments));
                    }
                }
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private bool includeNamespaces;
        private string outputFolder, namespacesFilename;

        private XPathExpression assemblyExpression, subgroupExpression, elementsExpression;

        private XPathExpression summaryExpression, paramExpression, typeparamExpression, returnsExpression,
            exceptionExpression, codeContractsExpression;

        private BlockingCollection<CommentsInfo> commentsList;
        private Task commentsWriter;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected IntelliSenseComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
            // No bounded capacity by default
            commentsList = new BlockingCollection<CommentsInfo>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            XPathNavigator nav;
            string attrValue;
            int boundedCapacity;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            this.WriteMessage(MessageLevel.Info, "[{0}, version {1}]\r\n    IntelliSense Component. " +
                "Copyright \xA9 2006-2015, Eric Woodruff, All Rights Reserved.\r\n" +
                "    https://GitHub.com/EWSoftware/SHFB", fvi.ProductName, fvi.ProductVersion);

            outputFolder = String.Empty;
            namespacesFilename = "Namespaces";

            assemblyExpression = XPathExpression.Compile("/document/reference/containers/library/@assembly");
            subgroupExpression = XPathExpression.Compile("string(/document/reference/apidata/@subgroup)");
            elementsExpression = XPathExpression.Compile("/document/reference/elements/element");

            summaryExpression = XPathExpression.Compile("summary");
            paramExpression = XPathExpression.Compile("param");
            typeparamExpression = XPathExpression.Compile("typeparam");
            returnsExpression = XPathExpression.Compile("returns");
            exceptionExpression = XPathExpression.Compile("exception");
            codeContractsExpression = XPathExpression.Compile("requires|ensures|ensuresOnThrow|pure|invariant|" +
                "getter|setter");

            nav = configuration.SelectSingleNode("output");

            if(nav != null)
            {
                attrValue = nav.GetAttribute("includeNamespaces", String.Empty);

                if(!String.IsNullOrEmpty(attrValue) && !Boolean.TryParse(attrValue, out includeNamespaces))
                    throw new ConfigurationErrorsException("You must specify a Boolean value for the <output> " +
                        "'includeNamespaces' attribute.");

                attrValue = nav.GetAttribute("folder", String.Empty);

                if(!String.IsNullOrEmpty(attrValue))
                {
                    outputFolder = Environment.ExpandEnvironmentVariables(attrValue);

                    if(!Directory.Exists(outputFolder))
                        Directory.CreateDirectory(outputFolder);
                }

                attrValue = nav.GetAttribute("namespacesFile", String.Empty);

                if(!String.IsNullOrEmpty(attrValue))
                    namespacesFilename = attrValue;

                // Allow limiting the writer task collection to conserve memory
                attrValue = nav.GetAttribute("boundedCapacity", String.Empty);

                if(!String.IsNullOrWhiteSpace(attrValue) && Int32.TryParse(attrValue, out boundedCapacity) &&
                  boundedCapacity > 0)
                    commentsList = new BlockingCollection<CommentsInfo>(boundedCapacity);
            }

            // Use a pipeline task to allow the actual saving to occur while other topics are being generated
            commentsWriter = Task.Run(() => WriteComments());
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            XPathNavigator navComments;

            // Don't bother if there is nothing to add
            if(key[1] != ':' || ((key[0] == 'G' || key[0] == 'N' || key[0] == 'R') && !includeNamespaces))
                return;

            navComments = document.CreateNavigator().SelectSingleNode("/document/comments");

            // Project and namespace comments go in a separate file.  A member may appear in multiple assemblies
            // so write its comments out to each one.
            if(key[0] == 'G' || key[0] == 'N' || key[0] == 'R')
                commentsList.Add(new CommentsInfo(this, namespacesFilename, key, navComments));
            else
                foreach(XPathNavigator asmName in navComments.Select(assemblyExpression))
                    commentsList.Add(new CommentsInfo(this, asmName.Value, key, navComments));
        }

        /// <summary>
        /// Wait for the comments writer task to complete when disposed
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                commentsList.CompleteAdding();

                if(commentsWriter != null)
                {
                    int count = commentsList.Count;

                    if(count != 0)
                        this.WriteMessage(MessageLevel.Diagnostic, "Waiting for the IntelliSense comments " +
                            "writer task to finish ({0} member(s) remaining)...", count);

                    commentsWriter.Wait();
                }

                commentsList.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to write the comments to the appropriate assembly XML comments file
        /// </summary>
        private void WriteComments()
        {
            CommentsInfo lastComments = null;
            XmlWriter writer;
            string fullPath;

            // Assembly names are compared case insensitively
            var writers = new Dictionary<string, XmlWriter>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach(var comments in commentsList.GetConsumingEnumerable())
                {
                    lastComments = comments;

                    if(String.IsNullOrWhiteSpace(comments.AssemblyName))
                        continue;

                    if(!writers.TryGetValue(comments.AssemblyName, out writer))
                    {
                        fullPath = Path.Combine(outputFolder, comments.AssemblyName + ".xml");
                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;

                        try
                        {
                            writer = XmlWriter.Create(fullPath, settings);
                        }
                        catch(IOException ioEx)
                        {
                            this.WriteMessage(comments.MemberName, MessageLevel.Error, "An access error occurred " +
                                "while attempting to create the IntelliSense output file '{0}'. The error message " +
                                "is: {1}", fullPath, ioEx.Message);
                        }

                        writers.Add(comments.AssemblyName, writer);

                        writer.WriteStartDocument();
                        writer.WriteStartElement("doc");
                        writer.WriteStartElement("assembly");
                        writer.WriteElementString("name", comments.AssemblyName);
                        writer.WriteEndElement();
                        writer.WriteStartElement("members");
                    }

                    writer.WriteStartElement("member");
                    writer.WriteAttributeString("name", comments.MemberName);

                    if(comments.Summary != null)
                        writer.WriteNode(comments.Summary, true);

                    foreach(XPathNavigator nav in comments.Params)
                        writer.WriteNode(nav, true);

                    foreach(XPathNavigator nav in comments.TypeParams)
                        writer.WriteNode(nav, true);

                    if(comments.Returns != null)
                        writer.WriteNode(comments.Returns, true);

                    foreach(XPathNavigator nav in comments.Exceptions)
                        writer.WriteNode(nav, true);

                    foreach(XPathNavigator nav in comments.CodeContracts)
                        writer.WriteNode(nav, true);

                    writer.WriteFullEndElement();

                    if(comments.EnumElements != null)
                        foreach(var kv in comments.EnumElements)
                        {
                            writer.WriteStartElement("member");
                            writer.WriteAttributeString("name", kv.Key);
                            writer.WriteNode(kv.Value, true);
                            writer.WriteFullEndElement();
                        }
                }
            }
            catch(IOException ioEx)
            {
                this.WriteMessage(lastComments.MemberName, MessageLevel.Error, "An access error occurred while " +
                    "attempting to write IntelliSense data. The error message is: {0}", ioEx.Message);
            }
            catch(XmlException xmlEx)
            {
                this.WriteMessage(lastComments.MemberName, MessageLevel.Error, "IntelliSense data was not valid " +
                    "XML. The error message is: {0}", xmlEx.Message);
            }
            finally
            {
                // Write out closing tags and close all open XML writers when done
                foreach(XmlWriter w in writers.Values)
                {
                    w.WriteEndDocument();
                    w.Close();
                }
            }
        }
        #endregion
    }
}
