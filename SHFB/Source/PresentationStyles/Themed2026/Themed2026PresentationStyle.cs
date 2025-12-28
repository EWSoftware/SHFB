//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : Themed2026PresentationStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the presentation style definition for the Themed 2026 presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/19/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;
using Sandcastle.Core.Project;

namespace Sandcastle.PresentationStyles.Themed2026;

/// <summary>
/// This contains the definition for the Themed 2026 presentation style
/// </summary>
[PresentationStyleExport("Themed2026", "Themed 2026", Version = AssemblyInfo.ProductVersion,
  Copyright = AssemblyInfo.Copyright, Description = "This is the default presentation style.  It generates " +
    "website output with a responsive layout and supports switching between light and dark themes.")]
public sealed class Themed2026PresentationStyle : PresentationStyleSettings
{
    /// <inheritdoc />
    public override string Location => ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly());

    /// <summary>
    /// Constructor
    /// </summary>
    public Themed2026PresentationStyle()
    {
        // The base path of the presentation style files relative to the assembly's location
        this.BasePath = "Themed2026";

        this.SupportedFormats = HelpFileFormats.Website;

        this.SupportsNamespaceGrouping = this.SupportsCodeSnippetGrouping = true;

        this.DocumentModelApplicator = new StandardDocumentModel();
        this.ApiTableOfContentsGenerator = new StandardApiTocGenerator();
        this.TopicTransformation = new Themed2026Transformation(this);

        // If relative, these paths are relative to the base path
        this.BuildAssemblerConfiguration = Path.Combine("Configuration", "BuildAssembler.config");

        // Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
        // match the case of the folder and filenames in the literals to their actual casing on the file
        // system.
        this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, Path.Combine("css", "*.*")));
        this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, Path.Combine("icons", "*.*")));
        this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, null, Path.Combine("scripts", "*.*"),
            "scripts", [".js"]));
        this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, Path.Combine("webfonts", "*.*")));
        this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, null,
            Path.Combine("RootWebsiteContent", "*.*"), String.Empty, [".aspx", ".htm", ".html", ".php"]));

        // Add the plug-in dependencies
        this.PlugInDependencies.Add(new PlugInDependency("Website Table of Contents Generator", null));
    }

    /// <inheritdoc />
    /// <remarks>This presentation style only uses the standard shared content</remarks>
    public override IEnumerable<string> ResourceItemFiles(string languageName)
    {
        string filePath = this.ResolvePath(Path.Combine("..", "Shared", "Content")),
            fileSpec = "SharedContent_" + languageName + ".xml";

        if(!File.Exists(Path.Combine(filePath, fileSpec)))
            fileSpec = "SharedContent_en-US.xml";

        yield return Path.Combine(filePath, fileSpec);

        foreach(string f in this.AdditionalResourceItemsFiles)
            yield return f;
    }
}
