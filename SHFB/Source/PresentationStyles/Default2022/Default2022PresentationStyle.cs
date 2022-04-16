//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : Default2022PresentationStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/16/2022
// Note    : Copyright 2014-2022, Eric Woodruff, All rights reserved
//
// This file contains the presentation style definition for the Default 2022 presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/16/2022  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: webfonts

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace Sandcastle.PresentationStyles.Default2022
{
    /// <summary>
    /// This contains the definition for the Default 2022 presentation style
    /// </summary>
    [PresentationStyleExport("Default2022", "Default2022", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This presentation style has a table of contents and " +
        "topic structure similar to the ones used for Microsoft Docs online content as of 2022.  It also has " +
        "a responsive layout.")]
    public sealed class Default2022PresentationStyle : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location => ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly());

        /// <summary>
        /// Constructor
        /// </summary>
        public Default2022PresentationStyle()
        {
            // The base path of the presentation style files relative to the assembly's location
            this.BasePath = "Default2022";

            this.SupportedFormats = HelpFileFormats.Website;

            this.SupportsNamespaceGrouping = this.SupportsCodeSnippetGrouping = true;

            this.DocumentModelApplicator = new StandardDocumentModel();
            this.ApiTableOfContentsGenerator = new StandardApiTocGenerator();
            this.TopicTranformation = new Default2022Transformation(this);

            // If relative, these paths are relative to the base path
            this.BuildAssemblerConfiguration = @"Configuration\BuildAssembler.config";

            // Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
            // match the case of the folder and filenames in the literals to their actual casing on the file
            // system.
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"css\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"icons\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"languages\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"scripts\*.*"));
            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, @"webfonts\*.*"));
            this.ContentFiles.Add(new ContentFiles(HelpFileFormats.Website, null, @"RootWebsiteContent\*.*",
                String.Empty, new[] { ".aspx", ".htm", ".html", ".php" }));

            // Add the plug-in dependencies
            this.PlugInDependencies.Add(new PlugInDependency("Website Table of Contents Generator", null));
        }

        /// <inheritdoc />
        public override IEnumerable<string> ResourceItemFiles(string languageName)
        {
            // This presentation style only uses the standard shared content
            string filePath = this.ResolvePath(@"..\Shared\Content\SharedContent"),
                fileSpec = "*" + languageName + "*.xml";

            if(!Directory.EnumerateFiles(filePath, fileSpec).Any())
                fileSpec = "*en-US*.xml";

            foreach(var file in Directory.EnumerateFiles(filePath, fileSpec))
                yield return file;
        }
    }
}
