//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeInlineRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to render codeInline elements from Markdown
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

using Markdig.Syntax.Inlines;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// A MAML renderer for <c>CodeInline</c> elements.
/// </summary>
/// <remarks>The MAML <c>codeInline</c> element is used rather than the HTML <c>code</c> element as it
/// conflicts with the MAML and XML comments <c>code</c> element used by the presentation styles to render
/// code blocks.</remarks>
public class CodeInlineRenderer : Markdig.Renderers.Html.HtmlObjectRenderer<CodeInline>
{
    /// <inheritdoc />
    protected override void Write(Markdig.Renderers.HtmlRenderer renderer, CodeInline obj)
    {
        if(renderer.EnableHtmlForInline)
            renderer.Write("<codeInline>");

        if(renderer.EnableHtmlEscape)
            renderer.WriteEscape(obj.ContentSpan);
        else
            renderer.Write(obj.ContentSpan);

        if(renderer.EnableHtmlForInline)
            renderer.Writer.Write("</codeInline>");
    }
}
