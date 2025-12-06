//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : HtmlBlockRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to render HTML blocks from Markdown
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

using System.IO;

using Markdig.Renderers;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// This class is used to render HTML blocks from Markdown
/// </summary>
public sealed class HtmlBlockRenderer : HtmlRenderer
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="writer">The text writer to use</param>
    public HtmlBlockRenderer(TextWriter writer) : base(writer)
    {
    }

    /// <summary>
    /// This is used to reset the renderer
    /// </summary>
    public void ResetRenderer()
    {
        this.Reset();
    }
}
