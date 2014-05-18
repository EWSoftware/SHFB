//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : VisualStudio2010.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/17/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the presentation style definition for the Visual Studio 2010 presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/04/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace Sandcastle.PresentationStyles
{
    /// <summary>
    /// This contains the definition for the Visual Studio 2010 presentation style
    /// </summary>
    [PresentationStyleExport("VS2010", "VS2010", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This style is similar to the one use for offline " +
      "content in Microsoft Help Viewer in Visual Studio 2010 and later.")]
    public sealed class VisualStudio2010 : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public VisualStudio2010()
        {
            // The base path of the presentation style files relative to the assembly's location
            this.BasePath = "VS2010";

            this.SupportedFormats = HelpFileFormats.HtmlHelp1 | HelpFileFormats.MSHelp2 |
                HelpFileFormats.MSHelpViewer | HelpFileFormats.Website;

            this.SupportsNamespaceGrouping = this.SupportsCodeSnippetGrouping = true;
            
            // If relative, these paths are relative to the base path
            this.ResourceItemsPath = "Content";
            this.ToolResourceItemsPath = "SHFBContent";

            this.DocumentModelTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\ApplyVSDocModel.xsl", new Dictionary<string, string>
                {
                    { "IncludeAllMembersTopic", "false" },
                    { "IncludeInheritedOverloadTopics", "false" },
                    { "project", "{@ProjectNodeIDOptional}" }
                });

            this.IntermediateTocTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\CreateVSToc.xsl");

            this.ConceptualBuildConfiguration = @"Configuration\SHFBConceptual.config";
            this.ReferenceBuildConfiguration = @"Configuration\SHFBReference.config";

            // Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
            // match the case of the folder and filenames in the literals to their actual casing on the file
            // system.
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"icons\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"scripts\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"styles\*.*"));
            this.ContentFiles.Add(new ContentFiles(HelpFileFormats.Website, null, @"..\LegacyWeb\*.*",
                String.Empty, new[] { ".aspx", ".html", ".htm", ".php" }));

            this.TransformComponentArguments.Add(new TransformComponentArgument("logoFile", true, true, null,
                "An optional logo file to insert into the topic headers.  Specify the filename only, omit " +
                "the path.  Place the file in your project in an icons\\ folder and set the Build Action to " +
                "Content.  If blank, no logo will appear in the topic headers.  If building website output " +
                "and your web server is case-sensitive, be sure to match the case of the folder name in your " +
                "project with that of the presentation style.  The same applies to the logo filename itself."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("logoHeight", true, true, null,
                "An optional logo height.  If left blank, the actual logo image height is used."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("logoWidth", true, true, null,
                "An optional logo width.  If left blank, the actual logo image width is used."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("logoAltText", true, true, null,
                "Optional logo alternate text.  If left blank, no alternate text is added."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("logoPlacement", true, true,
                 "left", "An optional logo placement.  Specify left, right, or above.  If not specified, the " +
                "default is left."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("logoAlignment", true, true,
                "left", "An optional logo alignment when using the 'above' placement option.  Specify left, " +
                "right, or center.  If not specified, the default is left."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("maxVersionParts", false, true,
                null, "The maximum number of assembly version parts to show in API member topics.  Set to 2, " +
                "3, or 4 to limit it to 2, 3, or 4 parts or leave it blank for all parts including the " +
                "assembly file version value if specified."));
            this.TransformComponentArguments.Add(new TransformComponentArgument("defaultLanguage", true, true,
                "cs", "The default language to use for syntax sections, code snippets, and a language-specific " +
                "text.  This should be set to cs, vb, cpp, fs, or the keyword style ID of a third-party syntax " +
                "generator if you want to use a non-standard language as the default."));
        }
    }
}
