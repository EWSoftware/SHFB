//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : VisualStudio2013.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/27/2016
// Note    : Copyright 2014-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the presentation style definition for the Visual Studio 2013 presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/13/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace Sandcastle.PresentationStyles
{
    /// <summary>
    /// This contains the definition for the Visual Studio 2013 presentation style
    /// </summary>
    [PresentationStyleExport("VS2013", "VS2013", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This style is similar to the one used for Visual " +
        "Studio and MSDN online content.")]
    public sealed class VisualStudio2013 : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location
        {
            get { return ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly()); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public VisualStudio2013()
        {
            // The base path of the presentation style files relative to the assembly's location
            this.BasePath = "VS2013";

            this.SupportedFormats = HelpFileFormats.HtmlHelp1 | HelpFileFormats.MSHelpViewer |
                HelpFileFormats.Website;

            this.SupportsNamespaceGrouping = this.SupportsCodeSnippetGrouping = true;

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

            // Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
            // match the case of the folder and filenames in the literals to their actual casing on the file
            // system.
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"icons\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"scripts\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"styles\*.*"));
            this.ContentFiles.Add(new ContentFiles(HelpFileFormats.Website, null, @"Web\*.*",
                String.Empty, new[] { ".aspx", ".html", ".htm", ".php" }));

            // Define the transform component arguments
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
                "text.  This should be set to cs, vb, cpp, fs, or the keyword style parameter value of a " +
                "third-party syntax generator if you want to use a non-standard language as the default."));
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
            this.TransformComponentArguments.Add(new TransformComponentArgument("requestExampleUrl", false, true,
                null, "To include a link that allows users to request an example for an API topic, set the URL " +
                "to which the request will be sent.  This can be a web page URL or an e-mail URL.  Only include " +
                "the URL as the parameters will be added automatically by the topic.  For example:\r\n\r\n" +
                "Create a new issue on GitHub: https://github.com/YourUserID/YourProject/issues/new \r\n" +
                "Send via e-mail: mailto:YourEmailAddress@Domain.com"));

            // Add the plug-in dependencies
            this.PlugInDependencies.Add(new PlugInDependency("Lightweight Website Style", null));
        }
    }
}
