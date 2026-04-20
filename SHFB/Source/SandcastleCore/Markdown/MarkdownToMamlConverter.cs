//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkdownToMamlConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2026
// Note    : Copyright 2025-2026, Eric Woodruff, All rights reserved
//
// This file contains a class used to convert Markdown content to MAML format for the build
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/25/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;

using Markdig;
using Markdig.Parsers;

using Sandcastle.Core.Markdown.Extensions;
using Sandcastle.Core.Markdown.Parsers;
using Sandcastle.Core.Markdown.Renderers;

namespace Sandcastle.Core.Markdown;

/// <summary>
/// This class is used to convert Markdown content to MAML format for the build
/// </summary>
public sealed class MarkdownToMamlConverter
{
    #region Private data members
    //=====================================================================

    private readonly IEnumerable<string> blockTags, doNotParseTags;
    private readonly MarkdownPipelineBuilder builder;

    private MarkdownPipeline pipeline;
    private MamlTopicRenderer renderer;
    private HtmlBlockMarkdownExtension htmlBlockExtension;
    private readonly bool forPreview;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="forPreview">True if rendering for preview, false if for a build</param>
    /// <param name="blockTags">An enumerable list of tags that should be treated as blocks</param>
    /// <param name="doNotParseTags">An enumerable list of tags that should not be parsed for Markdown
    /// content (code, scripts, pre, style, textarea, etc.).</param>
    public MarkdownToMamlConverter(bool forPreview, IEnumerable<string> blockTags, IEnumerable<string> doNotParseTags)
    {
        this.blockTags = blockTags;
        this.doNotParseTags = doNotParseTags;
        this.forPreview = forPreview;

        builder = new MarkdownPipelineBuilder();
    }
    #endregion

    #region Events
    //=====================================================================

    /// <summary>
    /// This event is raised prior to building and setting up the Markdown pipeline.  It can be used to add
    /// additional extensions or take other actions before the pipeline is built.
    /// </summary>
    public event EventHandler<PipelineEventArgs> BeforePipelineBuild;

    /// <summary>
    /// This event is raised after the pipeline is set up for rendering.  It can be used to make changes to the
    /// pipeline or take other actions after the pipeline is set up for rendering.
    /// </summary>
    public event EventHandler<PipelineEventArgs> AfterPipelineSetup;

    #endregion

    #region Methods
    //=====================================================================

    /// <summary>
    /// Set up the Markdown pipeline
    /// </summary>
    /// <remarks>This should only be called once prior to converting any topics</remarks>
    private void SetUpPipeline()
    {
        builder
            .UseYamlFrontMatter()
            .UseAbbreviations()
            .UseAutoIdentifiers()
            .UseCitations()
            .UseCustomContainers()
            .UseDefinitionLists()
            .UseEmphasisExtras()
            .UseFigures()
            .UseFooters()
            .UseFootnotes()
            .UseGridTables()
            .UseMediaLinks()
            .UsePipeTables()
            .UseListExtras()
            .UseTaskLists()
            .UseAutoLinks()
            .UseReferralLinks("noopener", "noreferrer")
            .UseEmojiAndSmiley()
            .UseGenericAttributes();    // This one must be added last

        // Register the new extension to post-process HtmlBlock contents
        htmlBlockExtension = new HtmlBlockMarkdownExtension(blockTags, doNotParseTags);
        
        builder.Extensions.Add(htmlBlockExtension);

        // Insert our alert block parser before the default QuoteBlockParser so it is tried first.
        builder.BlockParsers.InsertBefore<QuoteBlockParser>(new AlertBlockParser());

        // Allow other components to make changes to the pipeline before it is built
        this.BeforePipelineBuild?.Invoke(this, new PipelineEventArgs(builder, null));

        pipeline = builder.Build();

        renderer = new MamlTopicRenderer(forPreview, new StringWriter());
        pipeline.Setup(renderer);

        // Replace the alert block renderer with our custom one
        renderer.ObjectRenderers.Add(new AlertBlockRenderer());

        // Allow other components to make changes to the renderer after it is built
        this.AfterPipelineSetup?.Invoke(this, new PipelineEventArgs(builder, renderer));
    }

    /// <summary>
    /// Convert the given Markdown file to MAML and return the results
    /// </summary>
    /// <param name="id">The topic ID</param>
    /// <param name="markdownFile">The Markdown file to convert</param>
    /// <returns>The MAML representation of the Markdown content</returns>
    public string ConvertFromFile(string id, string markdownFile)
    {
        var m = new MarkdownFile(markdownFile);

        return ConvertFromMarkdown(id, m.Content);
    }

    /// <summary>
    /// Convert the given Markdown content to MAML and return the results
    /// </summary>
    /// <param name="id">The topic ID</param>
    /// <param name="markdown">The Markdown content to convert</param>
    /// <returns>The MAML representation of the Markdown content</returns>
    public string ConvertFromMarkdown(string id, string markdown)
    {
        if(pipeline == null)
            this.SetUpPipeline();

        var document = MarkdownParser.Parse(markdown, pipeline);

        renderer.Id = id;
        renderer.Render(document);
        renderer.Writer.Flush();

        var result = renderer.Writer.ToString()!;

        renderer.ResetRenderer();

        return result;
    }
    #endregion
}
