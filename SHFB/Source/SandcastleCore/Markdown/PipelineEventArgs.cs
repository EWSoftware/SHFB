//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PipelineEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2026
// Note    : Copyright 2026, Eric Woodruff, All rights reserved
//
// This file contains a class used to provide data for Markdown pipeline initialization events
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/20/2026  EFW  Created the code
//===============================================================================================================

using Markdig;

using Sandcastle.Core.Markdown.Renderers;

namespace Sandcastle.Core.Markdown;

/// <summary>
/// This class is used to provide data for Markdown pipeline initialization events
/// </summary>
public sealed class PipelineEventArgs
{
    /// <summary>
    /// This read-only property returns the Markdown pipeline builder that can be used to add extensions and
    /// configure the pipeline prior to setting it up.
    /// </summary>
    public MarkdownPipelineBuilder Builder { get; }

    /// <summary>
    /// This read-only property returns the Markdown renderer that will be used for the build.  It is not set
    /// until after the pipeline has been created.
    /// </summary>
    public MamlTopicRenderer Renderer { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="builder">The builder instance</param>
    /// <param name="renderer">The renderer instance if applicable</param>
    internal PipelineEventArgs(MarkdownPipelineBuilder builder, MamlTopicRenderer renderer)
    {
        this.Builder = builder;
        this.Renderer = renderer;
    }
}
