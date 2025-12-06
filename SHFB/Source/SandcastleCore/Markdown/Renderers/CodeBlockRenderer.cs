//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeBlockRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Based on Markdig.Renderers.Html.CodeBlockRenderer.  Copyright (c) Alexandre Mutel. All rights
//           reserved.  Licensed under the BSD-Clause 2 license.  https://github.com/xoofx/markdig
//
// This file contains a class used to render code elements from Markdown
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/03/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// A MAML renderer for <c>CodeBlock</c>.
/// </summary>
/// <remarks>The MAML <c>code</c> element is not nested within a <c>pre</c> element and the language name is
/// never prefixed.</remarks>
public class CodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockRenderer"/> class.
    /// </summary>
    public CodeBlockRenderer() { }

    /// <summary>
    /// Gets a map of fenced code block info that should be rendered as div blocks instead of code blocks.
    /// </summary>
    public HashSet<string> BlocksAsDiv { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a map of custom block mapping to render as custom blocks instead of code blocks.
    /// For example defining {"mermaid", "pre"} will render a block with info `mermaid` as a `pre` block but
    /// without the code HTML element.
    /// </summary>
    public Dictionary<string, string> BlockMapping { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private HashSet<string> SpecialBlockMapping
    {
        get
        {
            field ??= new HashSet<string>([.. BlocksAsDiv, .. BlockMapping.Keys], StringComparer.OrdinalIgnoreCase);
            
            return field;
        }
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        renderer.EnsureLine();

        var infoPrefix = (obj.Parser as FencedCodeBlockParser)?.InfoPrefix ?? FencedCodeBlockParser.DefaultInfoPrefix;
        var attributes = obj.TryGetAttributes();

        if(attributes != null)
        {
            var language = attributes.Classes.FirstOrDefault(c => c.StartsWith(infoPrefix, StringComparison.Ordinal));

            if(language != null)
            {
                attributes.Classes.Remove(language);

                language = language.Substring(infoPrefix.Length);
                attributes.AddProperty("language", language);
            }
        }

        if(obj is FencedCodeBlock { Info: string info } && SpecialBlockMapping.Contains(info))
        {
            var htmlBlock = BlockMapping.TryGetValue(info, out var blockType) ? blockType : "div";

            if(renderer.EnableHtmlForBlock)
            {
                renderer.Writer.Write('<');
                renderer.Write(htmlBlock).WriteAttributes(obj).Writer.Write('>');
            }

            renderer.WriteLeafRawLines(obj, true, true, true);

            if(renderer.EnableHtmlForBlock)
                renderer.Write("</").Write(htmlBlock).WriteLine(">");
        }
        else
        {
            if(renderer.EnableHtmlForBlock)
            {
                renderer.Writer.Write("<code");
                renderer.WriteAttributes(obj);
                renderer.Writer.Write('>');
            }

            renderer.WriteLeafRawLines(obj, true, renderer.EnableHtmlEscape);

            if(renderer.EnableHtmlForBlock)
                renderer.WriteLine("</code>");
        }

        renderer.EnsureLine();
    }
}
