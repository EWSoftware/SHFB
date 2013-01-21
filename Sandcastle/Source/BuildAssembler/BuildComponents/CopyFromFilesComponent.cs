// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// 01/18/2013 - EFW - Moved CopyFromFilesCommand into its own file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Commands;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component copies elements from one or more XML files determined using an XPath query into the
    /// target document based on one or more copy commands that define the elements to copy and where to put them.
    /// </summary>
    public class CopyFromFilesComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        private List<CopyFromFilesCommand> copyCommands = new List<CopyFromFilesCommand>();
        private CustomContext context = new CustomContext();
        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public CopyFromFilesComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            XPathNodeIterator copyNodes = configuration.Select("copy");

            foreach(XPathNavigator copyNode in copyNodes)
            {
                string basePath = copyNode.GetAttribute("base", String.Empty);

                if(String.IsNullOrWhiteSpace(basePath))
                    basePath = Environment.CurrentDirectory;

                basePath = Environment.ExpandEnvironmentVariables(basePath);

                if(!Directory.Exists(basePath))
                    base.WriteMessage(MessageLevel.Error, "The base directory '{0}' does not exist", basePath);

                string fileXPath = copyNode.GetAttribute("file", String.Empty);

                if(String.IsNullOrWhiteSpace(fileXPath))
                    base.WriteMessage(MessageLevel.Error, "Each copy element must have a file attribute " +
                        "specifying the file XPath used to get the file from which to copy elements");

                string sourceXPath = copyNode.GetAttribute("source", String.Empty);

                if(String.IsNullOrWhiteSpace(sourceXPath))
                    base.WriteMessage(MessageLevel.Error, "When instantiating a CopyFromFilesComponent, you " +
                        "must specify a source XPath format using the source attribute");

                string targetXPath = copyNode.GetAttribute("target", String.Empty);

                if(String.IsNullOrEmpty(targetXPath))
                    base.WriteMessage(MessageLevel.Error, "When instantiating a CopyFromFilesComponent, you " +
                        "must specify a target XPath format using the target attribute");

                copyCommands.Add(new CopyFromFilesCommand(this, basePath, fileXPath, sourceXPath, targetXPath));
            }

            base.WriteMessage(MessageLevel.Info, "Loaded {0} copy commands", copyCommands.Count);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Set the key in the XPath context
            context["key"] = key;

            // Perform each copy command
            foreach(CopyFromFilesCommand copyCommand in copyCommands)
                copyCommand.Apply(document, context);
        }
        #endregion
    }
}
