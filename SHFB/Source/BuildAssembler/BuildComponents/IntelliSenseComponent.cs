//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : IntelliSenseComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/24/2014
// Note    : Copyright 2007-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to extract the XML comments into files that can be used for
// IntelliSense.  Only the basic set of tags needed for IntelliSense are exported and only for documented API
// members.  This is based on the Microsoft IntelliSense build component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.1  10/09/2007  EFW  Created the code
// 1.6.0.7  03/24/2008  EFW  Updated it to handle multiple assembly references
// 1.8.0.3  07/04/2009  EFW  Add parameter to Dispose() to match base class
// 2.7.3.0  12/21/2012  EFW  Replaced the Microsoft IntelliSense build component with my version
// 2.7.5.0  11/12/2013  EFW  Added support for exporting code contracts XML comments elements
// -------  12/23/2013  EFW  Updated the build component to be discoverable via MEF
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using Microsoft.Ddue.Tools.UI;

namespace Microsoft.Ddue.Tools
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
    ///  &lt;output includeNamespaces="false" namespacesFile="Namespaces"
    ///      folder="C:\ProjectDocs\" /&gt;
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
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.After,
                    "Show Missing Documentation Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new IntelliSenseComponent(base.BuildAssembler);
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

        #region Private data members
        //=====================================================================

        private bool includeNamespaces;
        private string outputFolder, namespacesFilename;

        private XPathExpression assemblyExpression, subgroupExpression, elementsExpression;

        private XPathExpression summaryExpression, paramExpression, typeparamExpression, returnsExpression,
            exceptionExpression, codeContractsExpression;

        private Dictionary<string, XmlWriter> writers;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected IntelliSenseComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            XPathNavigator nav;
            string attrValue;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, "[{0}, version {1}]\r\n    IntelliSense Component. " +
                "Copyright \xA9 2006-2012, Eric Woodruff, All Rights Reserved.\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion);

            outputFolder = String.Empty;
            namespacesFilename = "Namespaces";

            // Assembly names are compared case insensitively
            this.writers = new Dictionary<string, XmlWriter>(StringComparer.OrdinalIgnoreCase);

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
            }
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            XPathNavigator navComments;

            // Don't bother if there is nothing to add
            if(key[1] != ':' || ((key[0] == 'R' || key[0] == 'N') && !includeNamespaces))
                return;

            navComments = document.CreateNavigator().SelectSingleNode("/document/comments");

            // Project and namespace comments go in a separate file.  A member may appear in multiple assemblies
            // so write its comments out to each one.
            if(key[0] == 'R' || key[0] == 'N')
                this.WriteComments(key, namespacesFilename, navComments);
            else
                foreach(XPathNavigator asmName in navComments.Select(assemblyExpression))
                    this.WriteComments(key, asmName.Value, navComments);
        }

        /// <summary>
        /// Write out closing tags and close all open XML writers when disposed.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(XmlWriter writer in writers.Values)
                {
                    writer.WriteEndDocument();
                    writer.Close();
                }

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Write the comments to the assembly's XML comments file
        /// </summary>
        /// <param name="key">The key (member name) of the item being documented.</param>
        /// <param name="filename">The assembly filename</param>
        /// <param name="navComments">The comments XPath navigator</param>
        private void WriteComments(string key, string filename, XPathNavigator navComments)
        {
            XmlWriter writer;
            XPathNavigator navTag;
            XPathNodeIterator iterator;
            string fullPath;

            if(String.IsNullOrEmpty(filename))
                return;

            try
            {
                if(!writers.TryGetValue(filename, out writer))
                {
                    fullPath = Path.Combine(outputFolder, filename + ".xml");
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;

                    try
                    {
                        writer = XmlWriter.Create(fullPath, settings);
                    }
                    catch(IOException ioEx)
                    {
                        base.WriteMessage(key, MessageLevel.Error, "An access error occurred while attempting " +
                            "to create the IntelliSense output file '{0}'. The error message is: {1}", fullPath,
                            ioEx.Message);
                    }

                    writers.Add(filename, writer);

                    writer.WriteStartDocument();
                    writer.WriteStartElement("doc");
                    writer.WriteStartElement("assembly");
                    writer.WriteElementString("name", filename);
                    writer.WriteEndElement();
                    writer.WriteStartElement("members");
                }

                writer.WriteStartElement("member");
                writer.WriteAttributeString("name", key);

                navTag = navComments.SelectSingleNode(summaryExpression);

                if(navTag != null)
                    writer.WriteNode(navTag, true);

                iterator = navComments.Select(paramExpression);

                foreach(XPathNavigator nav in iterator)
                    writer.WriteNode(nav, true);

                iterator = navComments.Select(typeparamExpression);

                foreach(XPathNavigator nav in iterator)
                    writer.WriteNode(nav, true);

                navTag = navComments.SelectSingleNode(returnsExpression);

                if(navTag != null)
                    writer.WriteNode(navTag, true);

                iterator = navComments.Select(exceptionExpression);

                foreach(XPathNavigator nav in iterator)
                    writer.WriteNode(nav, true);

                iterator = navComments.Select(codeContractsExpression);

                foreach(XPathNavigator nav in iterator)
                    writer.WriteNode(nav, true);

                writer.WriteFullEndElement();

                // Write out enumeration members?
                if((string)navComments.Evaluate(subgroupExpression) == "enumeration")
                {
                    iterator = (XPathNodeIterator)navComments.Evaluate(elementsExpression);

                    foreach(XPathNavigator nav in iterator)
                    {
                        string attribute = nav.GetAttribute("api", String.Empty);
                        writer.WriteStartElement("member");
                        writer.WriteAttributeString("name", attribute);

                        navTag = nav.SelectSingleNode(summaryExpression);

                        if(navTag != null)
                            writer.WriteNode(navTag, true);

                        writer.WriteFullEndElement();
                    }
                }
            }
            catch(IOException ioEx)
            {
                base.WriteMessage(key, MessageLevel.Error, "An access error occurred while attempting to write " +
                    "IntelliSense data. The error message is: {0}", ioEx.Message);
            }
            catch(XmlException xmlEx)
            {
                base.WriteMessage(key, MessageLevel.Error, "IntelliSense data was not valid XML. The error " +
                    "message is: {0}", xmlEx.Message);
            }
        }
        #endregion
    }
}
