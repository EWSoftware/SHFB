//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : OpenXml.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/12/2016
// Note    : Copyright 2014-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace Sandcastle.PresentationStyles
{
    /// <summary>
    /// This defines a presentation style used to generate Open XML documents
    /// </summary>
    /// <remarks>
    /// <note type="important">Two of the content files that make up the document parts,
    /// "DocumentParts\_rels\rels.xml.rels" and "DocumentParts\Content_Types.xml", are renamed by the build
    /// process to "DocumentParts\_rels\.rels" and "DocumentParts\[Content_Types].xml".  Those two names are
    /// reserved as part of the Open Packaging Conventions and when deployed as part of a NuGet package, NuGet
    /// tends to ignore them when extracting the package content.  As such, we give them non-reserved names for
    /// inclusion in the package and rename them to their actual names at build time.</note></remarks>
    [PresentationStyleExport("OpenXML", "Open XML Document", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This generates an Open XML document containing the " +
        "help content.\r\nOpen XML documents can be converted to other formats such as PDF files.")]
    public sealed class OpenXML : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location
        {
            get { return ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly()); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OpenXML()
        {
            this.BasePath = "OpenXML";

            this.SupportedFormats = HelpFileFormats.OpenXml;

            this.SupportsNamespaceGrouping = true;

            // If relative, these paths are relative to the base path
            this.ResourceItemsPath = "Content";
            this.ToolResourceItemsPath = "SHFBContent";

            this.DocumentModelTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\ApplyVSDocModel.xsl", new Dictionary<string, string>
                {
                    { "IncludeAllMembersTopic", "false" },
                    { "project", "{@ProjectNodeIDOptional}" }
                });

            this.IntermediateTocTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\CreateVSToc.xsl");

            this.BuildAssemblerConfiguration = @"Configuration\BuildAssembler.config";

            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, null, @"DocumentParts\*.*",
                String.Empty, new[] { ".xml" } ));

            this.TransformComponentArguments.Add(new TransformComponentArgument("maxVersionParts", false, true,
                null, "The maximum number of assembly version parts to show in API member topics.  Set to 2, " +
                "3, or 4 to limit it to 2, 3, or 4 parts or leave it blank for all parts including the " +
                "assembly file version value if specified."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("includeEnumValues", false, true,
                "true", "Set this to 'true' to include the column for the numeric value of each field in " +
                "enumerated type topics.  Set it to 'false' to omit the numeric values column."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("baseSourceCodeUrl", false, true,
                null, "If you set the Source Code Base Path property in the Paths category, specify the URL to " +
                "the base source code folder on your project's website here.  Some examples for GitHub are " +
                "shown below.\r\n\r\n" +
                "Important: Be sure to set the Source Code Base Path property and terminate the URL below with " +
                "a slash if necessary.\r\n\r\n" +
                "Format: https://github.com/YourUserID/YourProject/blob/BranchNameOrCommitHash/BaseSourcePath/ \r\n\r\n" +
                "Master branch: https://github.com/JohnDoe/WidgestProject/blob/master/src/ \r\n" +
                "A different branch: https://github.com/JohnDoe/WidgestProject/blob/dev-branch/src/ \r\n" +
                "A specific commit: https://github.com/JohnDoe/WidgestProject/blob/c6e41c4fc2a4a335352d2ae8e7e85a1859751662/src/"));
        }
    }
}
