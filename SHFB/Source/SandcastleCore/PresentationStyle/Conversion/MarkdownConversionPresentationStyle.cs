//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkdownConversionPresentationStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/26/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the presentation style definition for the markdown conversion presentation style
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.IO;

namespace Sandcastle.Core.PresentationStyle.Conversion;

/// <summary>
/// This defines a presentation style used to convert MAML topics to Markdown topics.
/// </summary>
/// <remarks>This presentation style is only visible in code to allow converting MAML topics to Markdown.  It is
/// not intended to be used as a project presentation style option.</remarks>
public sealed class MarkdownConversionPresentationStyle : PresentationStyleSettings
{
    /// <inheritdoc />
    /// <remarks>This one is in the Sandcastle.Core assembly so we need to ensure it returns the core components
    /// folder so that it finds the resource items files there and not in the tools folder.</remarks>
    public override string Location => ComponentUtilities.CoreComponentsFolder;

    /// <summary>
    /// Constructor
    /// </summary>
    public MarkdownConversionPresentationStyle()
    {
        // The base path of the presentation style files relative to the assembly's location
        this.BasePath = "Markdown";

        this.TopicTransformation = new MarkdownConversionTransformation(this.ResolvePath);
    }

    /// <inheritdoc />
    /// <remarks>This presentation style uses the standard shared content and overrides a few items with
    /// Markdown specific values.</remarks>
    public override IEnumerable<string> ResourceItemFiles(string languageName)
    {
        string filePath = this.ResolvePath(Path.Combine("..", "Shared", "Content")),
            fileSpec = "SharedContent_" + languageName + ".xml";

        if(!File.Exists(Path.Combine(filePath, fileSpec)))
            fileSpec = "SharedContent_en-US.xml";

        yield return Path.Combine(filePath, fileSpec);

        fileSpec = "Markdown_" + languageName + ".xml";

        if(!File.Exists(Path.Combine(filePath, fileSpec)))
            fileSpec = "Markdown_en-US.xml";

        yield return Path.Combine(filePath, fileSpec);

        foreach(string f in this.AdditionalResourceItemsFiles)
            yield return f;
    }
}
