//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ParagraphRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Based on Markdig.Renderers.Html.ParagraphRenderer.  Copyright (c) Alexandre Mutel. All rights
//           reserved.  Licensed under the BSD-Clause 2 license.  https://github.com/xoofx/markdig
//
// This file contains a class used to render paragraph elements from Markdown
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// A MAML renderer for <c>ParagraphBlock</c>.
/// </summary>
/// <remarks>The MAML <c>para</c> element is used rather than the HTML <c>p</c> element as the presentation
/// styles have some dependencies on the <c>para</c> element.</remarks>
public sealed class ParagraphRenderer : Markdig.Renderers.Html.HtmlObjectRenderer<ParagraphBlock>
{
    /// <inheritdoc />
    protected override void Write(Markdig.Renderers.HtmlRenderer renderer, ParagraphBlock obj)
    {
        if(!renderer.ImplicitParagraph && renderer.EnableHtmlForBlock)
        {
            if(!renderer.IsFirstInContainer)
                renderer.EnsureLine();

            renderer.Write("<para").WriteAttributes(obj);
            renderer.Writer.Write('>');
        }

        renderer.WriteLeafInline(obj);

        if(!renderer.ImplicitParagraph)
        {
            if(renderer.EnableHtmlForBlock)
                renderer.WriteLine("</para>");

            renderer.EnsureLine();
        }
    }
}
