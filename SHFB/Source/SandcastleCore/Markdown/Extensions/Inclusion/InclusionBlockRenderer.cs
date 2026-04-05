//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : InclusionBlockRenderer.cs
// Author  : Jason Curl (jcurl@arcor.de)
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// Renders an inclusion block.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/05/2025  JMC  Created the code
//===============================================================================================================

using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// Block renderer for the <see cref="InclusionBlock"/>.
/// </summary>
/// <remarks>
/// It delegates rendering to all the blocks the inclusion block contains.
/// </remarks>
public class InclusionBlockRenderer : HtmlObjectRenderer<InclusionBlock>
{
    private readonly MarkdownPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="InclusionBlockRenderer"/> class.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    public InclusionBlockRenderer(MarkdownPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// Renders the inclusion block.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="inclusion">The inclusion block.</param>
    protected override void Write(HtmlRenderer renderer, InclusionBlock inclusion)
    {
        if(!inclusion.Loaded)
        {
            renderer.Write(inclusion.GetRawToken());
        }
        else
        {
            foreach(Block block in inclusion)
            {
                renderer.Write(block);
            }
        }
    }
}
