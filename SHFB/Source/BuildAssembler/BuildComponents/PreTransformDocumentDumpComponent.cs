//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : PreTransformDocumentDumpComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/18/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains a build component that is used to save the pre-transform document data for use in testing
// presentation style transformations.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/08/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This build component is a development aid.  It is used to save the pre-transform document data for use
    /// in testing presentation style transformations.
    /// </summary>
    /// <remarks>This is a presentation style development aid.  It saves the pre-transformed content of each
    /// document to a file in a .\RawDocs subfolder in the project's working folder.  These files can be used
    /// for testing presentation style transforms without having to do a full project build.</remarks>
    public class PreTransformDocumentDumpComponent : BuildComponentCore
    {
        #region Private data members
        //=====================================================================

        private XmlWriterSettings settings;
        private string dumpPath;

        #endregion

        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Pre-transform Document Dump Component", IsVisible = true, Version = AssemblyInfo.ProductVersion,
          Copyright = AssemblyInfo.Copyright, Description = "This is a presentation style development aid.  It " +
            "saves the pre-transformed content of each document to a file in a .\\RawDocs subfolder in the " +
            "project's working folder.  These files can be used for testing presentation style transforms " +
            "without having to do a full project build.")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Factory()
            {
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component");
                this.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new PreTransformDocumentDumpComponent(this.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration => @"<dumpPath value=""{@WorkingFolder}\RawDocs"" />";
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected PreTransformDocumentDumpComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Abstract method implementations
        //=====================================================================

        /// <summary>
        /// Initialize the build component
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            this.WriteMessage(MessageLevel.Info, "[{0}, version {1}]\r\n    RawDocumentDumpComponent Component.  {2}",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            dumpPath = configuration.SelectSingleNode("dumpPath").GetAttribute("value", String.Empty);

            if(!Directory.Exists(dumpPath))
                Directory.CreateDirectory(dumpPath);

            settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                CloseOutput = true,
                Indent = true
            };

            // Hook up the event handler to save the content just prior to XSL transformation
            this.BuildAssembler.ComponentEvent += TransformComponent_TopicTransforming;
        }

        /// <summary>
        /// Apply this build component's changes to the document
        /// </summary>
        /// <param name="document">The document to modify</param>
        /// <param name="key">The document's key</param>
        public override void Apply(XmlDocument document, string key)
        {
            // Nothing to do here.  See below.
        }

        /// <summary>
        /// Save the raw content just before transforming so that all other components have had a chance to
        /// add and modify the content such as the Syntax Component.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void TransformComponent_TopicTransforming(object sender, EventArgs e)
        {
            // Don't bother if not a transforming event or not in our group
            if(!(e is ApplyingChangesEventArgs ac) || ac.GroupId != this.GroupId ||
              ac.ComponentId != "XSL Transform Component")
            {
                return;
            }

            if(ac.Document != null)
            {
                StringBuilder filename = new StringBuilder(ac.Key);

                foreach(char c in Path.GetInvalidFileNameChars())
                    if(ac.Key.IndexOf(c) != -1)
                        filename.Replace(c, '_');

                string xmlFile = Path.Combine(dumpPath, filename.ToString());

                if(xmlFile.Length > 250)
                    xmlFile = xmlFile.Substring(0, 250);

                xmlFile += ".xml";

                using(XmlWriter writer = XmlWriter.Create(xmlFile, settings))
                {
                    ac.Document.Save(writer);
                }
            }
        }
        #endregion
    }
}
