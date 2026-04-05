//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : InclusionBlockExtension.cs
// Author  : .NET Foundation
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// An inclusion block added to the Markdig AST.
//
// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you
// under the MIT license.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/05/2025  JMC  Created the code based on https://github.com/dotnet/docfx/commits/2a436495ed0716eebbc7eaad84d652f18600c2f6/
//===============================================================================================================

using System.Linq;

using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// Markdown inclusion extension to add to a pipeline.
/// </summary>
public class InclusionExtension : IMarkdownExtension
{
    private MarkdownPipeline _pipeline;

    /// <summary>
    /// Setups this extension for the specified pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <remarks>
    /// To load the inclusion blocks when loading the file, you must also set up the renderer. The setup doesn't provide
    /// the pipeline which is required when loading a new markdown file. The renderer does have the pipeline. This may
    /// be strange, Markdig never intended to load markdown files during parsing. Other porjects (docfx) loads the file
    /// during rendering that prevents processing before rendering, needing a lot of invasive changes in the caller.
    /// </remarks>
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline
            .BlockParsers
            .AddIfNotAlready(new InclusionBlockParser());

        pipeline.DocumentProcessed += document =>
        {
            // Pipeline set up when renderer is configured.
            ProcessInclusions(document, _pipeline);
        };
    }

    /// <summary>
    /// Setups this extension for the specified renderer.
    /// </summary>
    /// <param name="pipeline">The pipeline used to parse the document.</param>
    /// <param name="renderer">The renderer.</param>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        _pipeline = pipeline;

        if(renderer is HtmlRenderer htmlRenderer)
        {
            if(!htmlRenderer.ObjectRenderers.Contains<InclusionBlockRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, new InclusionBlockRenderer(pipeline));
            }
        }
    }

    private static void ProcessInclusions(MarkdownDocument document, MarkdownPipeline pipeline)
    {
        var inclusions = document.Descendants<InclusionBlock>().ToList();

        foreach(var inclusion in inclusions)
        {
            inclusion.Load(pipeline);
        }
    }
}
