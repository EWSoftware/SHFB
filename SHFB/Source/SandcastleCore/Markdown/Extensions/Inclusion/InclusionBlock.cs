//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : InclusionBlock.cs
// Author  : .NET Foundation
// Updated : 04/54/2026
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

using System;
using System.IO;

using Markdig;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// An inclusion block, creating while parsing from <see cref="InclusionBlockParser"/>.
/// </summary>
public class InclusionBlock : ContainerBlock
{
    /// <summary>
    /// Gets or sets the title by the inclusion block.
    /// </summary>
    /// <value>The title in the inclusion block text.</value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the included file path given by the user.
    /// </summary>
    /// <value>The included file path.</value>
    public string IncludedFilePath { get; set; }

    /// <summary>
    /// Gets the resolved file path as obtained by this block.
    /// </summary>
    /// <value>The resolved file path.</value>
    public string ResolvedFilePath { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="InclusionBlock"/> is loaded.
    /// </summary>
    /// <value><see langword="true"/> if loaded; otherwise, <see langword="false"/>.</value>
    public bool Loaded { get; set; }

    /// <summary>
    /// Gets the raw token.
    /// </summary>
    /// <returns>The raw token, useful for errors.</returns>
    public string GetRawToken() => $"[!INCLUDE[{Title}]({IncludedFilePath})]";

    /// <summary>
    /// Initializes a new instance of the <see cref="InclusionBlock"/> class.
    /// </summary>
    /// <param name="parser">The parser used to create this block.</param>
    public InclusionBlock(BlockParser parser) : base(parser)
    {

    }

    /// <summary>
    /// Loads the markdown file that this inclusion block points to, and adds the blocks in the file to this block.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    public void Load(MarkdownPipeline pipeline)
    {
        if(Loaded)
        {
            return;
        }

        try
        {
            using var path = InclusionFiles.PushDependency(IncludedFilePath);

            ResolvedFilePath = path.FilePath;
            string content = ReadFile(ResolvedFilePath);
            MarkdownDocument document = Markdig.Markdown.Parse(content, pipeline);

            // A foreach() won't work as expected, skipping some blocks.
            while(document.Count > 0)
            {
                Block block = document[0];
                document.RemoveAt(0);
                this.Add(block);
            }
            Loaded = true;
        }
        catch(ArgumentException ex)
        {
            return;
        }
        catch(Exception ex)
        {
            InclusionFiles.Warnings.Add(new("BE0076", ex.Message));
            return;
        }
    }

    private static string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }
}
