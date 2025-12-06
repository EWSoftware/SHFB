//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AlertRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to render alert elements from Markdown
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

using System;

using Markdig.Renderers;
using Markdig.Renderers.Html;

using Sandcastle.Core.Markdown.Parsers;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// A MAML renderer for <c>AlertBlock</c> elements.
/// </summary>
/// <remarks>The MAML <c>alert</c> element is used as it allows definition of an optional title and we want the
/// presentation style to render it's actual format.</remarks>
public class AlertBlockRenderer : HtmlObjectRenderer<AlertBlock>
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, AlertBlock obj)
    {
        string noteType = null, title = null;

        if(obj.Kind.Length > 0)
        {
            noteType = obj.Kind.ToString();

            int separator = noteType.IndexOf(',');

            if(separator != -1)
            {
                title = noteType.Substring(separator + 1).Trim();
                noteType = noteType.Substring(0, separator);
            }
        }

        noteType = noteType?.ToLowerInvariant() ?? "note";

        renderer.EnsureLine();
        renderer.Write($"<alert class=\"{noteType}\"");

        if(!String.IsNullOrWhiteSpace(title))
        {
            renderer.Write($" title=\"");
            renderer.WriteEscape(title);
            renderer.Write("\"");
        }

        renderer.WriteLine(">");

        bool priorIPState = renderer.ImplicitParagraph;

        renderer.ImplicitParagraph = false;
        renderer.WriteChildren(obj);
        renderer.ImplicitParagraph = priorIPState;
        renderer.WriteLine("</alert>");

        renderer.EnsureLine();
    }
}