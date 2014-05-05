// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/24/2012 - Moved FileCreatedEventArgs out into its own file and simplified it.  The HxFGeneratorComponent
// will figure out where it needs to put its file rather than relying on the SaveComponent to tell it.  This
// makes the event more generic so that it can be used by other components.  Fixed the creation of the
// XML writer so that it uses the indentation.  Fixed the component so that it is really capable of generating
// multiple HxF files concurrently.  This seems like an odd feature considering that there's typically only
// one Help 2 output file set but since the original component made an attempt to support it, I've kept the
// ability to do so.
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component is used to generate an HxF file for the Help 2 compiler that contains all of the
    /// content files to compile into it.
    /// </summary>
    /// <remarks>Rather than modifying the document, this responds to component events with an event argument
    /// type of <see cref="FileCreatedEventArgs"/> to save file information to the HxF file.</remarks>
    public class HxfGeneratorComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("HxF File Generator Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new HxfGeneratorComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private string inputFile, outputFilename;

        // Path names are compared case insensitively
        private Dictionary<string, XmlWriter> writers = new Dictionary<string, XmlWriter>(
            StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected HxfGeneratorComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            inputFile = configuration.GetAttribute("input", String.Empty);

            if(!String.IsNullOrEmpty(inputFile))
                inputFile = Environment.ExpandEnvironmentVariables(inputFile);

            outputFilename = configuration.GetAttribute("output", String.Empty);

            if(!String.IsNullOrEmpty(outputFilename))
                outputFilename = Environment.ExpandEnvironmentVariables(outputFilename);

            // Subscribe to component events
            base.BuildAssembler.ComponentEvent += FileCreatedHandler;
        }

        /// <summary>
        /// This component does not change the document
        /// </summary>
        /// <param name="document">Not used</param>
        /// <param name="key">Not used</param>
        public override void Apply(XmlDocument document, string key)
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(var w in writers.Values)
                {
                    w.WriteEndDocument();
                    w.Close();
                }

            base.Dispose(disposing);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to create the HxF files as needed and write the filenames to them
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void FileCreatedHandler(object sender, EventArgs e)
        {
            XmlWriter writer;

            FileCreatedEventArgs fe = e as FileCreatedEventArgs;

            // Ignore if not our event or if it's not a content file
            if(fe == null || !fe.IsContentFile)
                return;

            // The HxF files should go in the parent folder of the content files
            string basePath = Path.GetDirectoryName(Path.GetDirectoryName(fe.FilePath));

            string path = Path.Combine(basePath, outputFilename);

            // This allows for multiple concurrent output files in different locations
            if(!writers.TryGetValue(path, out writer))
            {
                writer = this.WriteFile(path);
                writers.Add(path, writer);
            }

            if(basePath.Length > 1)
                WriteFileElement(writer, fe.FilePath.Substring(basePath.Length + 1));
            else
                WriteFileElement(writer, fe.FilePath);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This creates a new HxF file and, if an input seed file has been specified, it writes out the
        /// filenames from it to the new HxF file.
        /// </summary>
        /// <param name="path">The path to the new HxF file</param>
        /// <returns>The Xml writer to use when adding more file elements to the HxF file</returns>
        public XmlWriter WriteFile(string path)
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;

            XmlWriter writer = XmlWriter.Create(path, writerSettings);

            writer.WriteStartDocument();
            writer.WriteDocType("HelpFileList", null, "MS-Help://Hx/Resources/HelpFileList.dtd", null);
            writer.WriteStartElement("HelpFileList");
            writer.WriteAttributeString("DTDVersion", "1.0");

            // Use the input to seed the output
            if(!String.IsNullOrEmpty(inputFile))
            {
                try
                {
                    foreach(string line in File.ReadLines(inputFile))
                        WriteFileElement(writer, line);
                }
                catch(Exception ex)
                {
                    base.WriteMessage(MessageLevel.Error, "An access error occurred while attempting to copy " +
                        "the input HxF data. The error message is:", ex.Message);
                }
            }

            return writer;
        }

        /// <summary>
        /// This adds a file element to the HxF file
        /// </summary>
        /// <param name="writer">The XML writer to use</param>
        /// <param name="url">The URL of the file</param>
        private static void WriteFileElement(XmlWriter writer, string url)
        {
            writer.WriteStartElement("File");
            writer.WriteAttributeString("Url", url);
            writer.WriteEndElement();
        }
        #endregion
    }
}
