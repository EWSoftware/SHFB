//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : OpenXmlPresentationStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/08/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the presentation style definition for the Open XML presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/15/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;
using Sandcastle.Core.Project;

namespace Sandcastle.PresentationStyles.OpenXml
{
    /// <summary>
    /// This defines a presentation style used to generate Open XML documents
    /// </summary>
    /// <remarks>
    /// <note type="important">Three of the content files that make up the document parts,
    /// "DocumentParts\_rels\rels.xml_rels", "DocumentParts\Content_Types.xml",
    /// and "DocumentParts\word\_rels\document.xml_rels", are renamed by the build
    /// process to "DocumentParts\_rels\.rels", "DocumentParts\[Content_Types].xml",
    /// and "DocumentParts\word\_rels\document.xml.rels".  Those three filenames are reserved as part of the Open
    /// Packaging Conventions and when deployed as part of a NuGet package, NuGet tends to ignore them when
    /// extracting the package content or it causes problems reading the package when using third-party tools.
    /// As such, we give them non-reserved names for inclusion in the package and rename them to their actual
    /// names at build time.</note></remarks>
    [PresentationStyleExport("OpenXML", "Open XML Document", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This generates an Open XML document containing the " +
        "help content.\r\nOpen XML documents can be converted to other formats such as PDF files.")]
    public sealed class OpenXmlPresentationStyle : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location => ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly());

        /// <summary>
        /// Constructor
        /// </summary>
        public OpenXmlPresentationStyle()
        {
            // The base path of the presentation style files relative to the assembly's location
            this.BasePath = "OpenXML";

            this.SupportedFormats = HelpFileFormats.OpenXml;

            this.SupportsNamespaceGrouping = true;

            this.DocumentModelApplicator = new StandardDocumentModel();
            this.ApiTableOfContentsGenerator = new StandardApiTocGenerator();
            this.TopicTransformation = new OpenXmlTransformation(this.ResolvePath);

            // If relative, these paths are relative to the base path
            this.BuildAssemblerConfiguration = Path.Combine("Configuration", "BuildAssembler.config");

            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, null,
                Path.Combine("DocumentParts", "*.*"), String.Empty, [".xml"]));
        }

        /// <inheritdoc />
        /// <remarks>This presentation style uses the standard shared content and overrides a few items with
        /// Open XML specific values.</remarks>
        public override IEnumerable<string> ResourceItemFiles(string languageName)
        {
            string filePath = this.ResolvePath(Path.Combine("..", "Shared", "Content")),
                fileSpec = "SharedContent_" + languageName + ".xml";

            if(!File.Exists(Path.Combine(filePath, fileSpec)))
                fileSpec = "SharedContent_en-US.xml";

            yield return Path.Combine(filePath, fileSpec);

            fileSpec = "OpenXml_" + languageName + ".xml";

            if(!File.Exists(Path.Combine(filePath, fileSpec)))
                fileSpec = "OpenXml_en-US.xml";

            yield return Path.Combine(filePath, fileSpec);

            foreach(string f in this.AdditionalResourceItemsFiles)
                yield return f;
        }
    }
}
