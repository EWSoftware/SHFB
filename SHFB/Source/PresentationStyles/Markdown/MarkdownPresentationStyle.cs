//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : MarkdownPresentationStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/20/2022
// Note    : Copyright 2015-2022, Eric Woodruff, All rights reserved
//
// This file contains the presentation style definition for the markdown content presentation style
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/02/2015  EFW  Created the code
//===============================================================================================================

/* TODO: Convert to new presenation style method

using System;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace Sandcastle.PresentationStyles.Markdown
{
    /// <summary>
    /// This defines a presentation style used to generate markdown content (GitHub flavored)
    /// </summary>
    [PresentationStyleExport("Markdown", "Markdown Content", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This generates markdown content (GitHub flavored)")]
    public sealed class MarkdownPresentationStyle : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location => ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly());

        /// <summary>
        /// Constructor
        /// </summary>
        public MarkdownPresentationStyle()
        {
            this.BasePath = "Markdown";

            this.SupportedFormats = HelpFileFormats.Markdown;

            this.SupportsNamespaceGrouping = true;

            this.DocumentModelApplicator = new StandardDocumentModel();
            this.ApiTableOfContentsGenerator = new StandardApiTocGenerator();

            // If relative, these paths are relative to the base path
            this.ResourceItemsPath = "Content";
            this.BuildAssemblerConfiguration = @"Configuration\BuildAssembler.config";

            // Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
            // match the case of the folder and filenames in the literals to their actual casing on the file
            // system.
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"media\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, null, @"MarkdownContent\*.*",
                String.Empty, new[] { ".md" }));

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
*/