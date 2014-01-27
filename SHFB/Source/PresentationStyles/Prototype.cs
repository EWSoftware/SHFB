//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : Prototype.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/11/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the presentation style definition for the Prototype presentation style.
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

using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace Sandcastle.PresentationStyles
{
    /// <summary>
    /// This contains the definition for the Prototype presentation style
    /// </summary>
    [PresentationStyleExport("Prototype", "Prototype (Deprecated)", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This style has been deprecated and is no longer " +
      "supported.  It was the first style provided with Sandcastle.")]
    public sealed class Prototype : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Prototype()
        {
            // The base path of the presentation style files relative to the assembly's location
            this.BasePath = "Prototype";

            // This deprecated style does not support the MS Help Viewer format or namespace grouping
            this.SupportedFormats = HelpFileFormats.HtmlHelp1 | HelpFileFormats.MSHelp2 | HelpFileFormats.Website;

            // If relative, these paths are relative to the base path
            this.ResourceItemsPath = "Content";
            this.ToolResourceItemsPath = "SHFBContent";

            // This presentation style requires a project node ID in order to generate the root namespaces node
			// which is used to generate the root namespaces topic page.
            this.DocumentModelTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\ApplyPrototypeDocModel.xsl", new Dictionary<string, string>
                {
                    { "project", "{@ProjectNodeIDRequired}" }
                });

            // This transformation is passed a parameter to indicate whether or not to create a root namespace
            // container.
            this.IntermediateTocTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\CreatePrototypeToc.xsl", new Dictionary<string, string>
                {
                    { "rootNamespaceContainer", "{@RootNamespaceContainer}" }
                });

            this.ConceptualBuildConfiguration = @"Configuration\SHFBConceptual.config";
            this.ReferenceBuildConfiguration = @"Configuration\SHFBReference.config";

            // Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
            // match the case of the folder and filenames in the literals to their actual casing on the file
            // system.
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"icons\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"scripts\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"styles\*.*"));
            this.ContentFiles.Add(new ContentFiles(HelpFileFormats.Website, "%SHFBROOT%", @"Web\*.*", @".\",
                new[] { ".aspx", ".html", ".htm", ".php" }));

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
        }
    }
}
