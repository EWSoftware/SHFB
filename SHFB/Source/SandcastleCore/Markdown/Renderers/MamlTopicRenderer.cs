//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MamlTopicRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/03/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to render paragraph elements from Markdown
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
using System.Linq;

using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// This is a modified version of the HTML renderer that converts a markdown file to a MAML topic for the
/// Sandcastle Help File Builder build process.
/// </summary>
/// <remarks>The result is not a pure MAML document but rather a hybrid of MAML and HTML that can be parsed
/// and rendered to a supported output format by a Sandcastle Help File Builder presentation style.</remarks>
public class MamlTopicRenderer : HtmlRenderer
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to get or set the current topic ID
    /// </summary>
    public string Id { get; set; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="forPreview">True if rendering for preview, false if for a build</param>
    /// <param name="writer">The text writer to use</param>
    public MamlTopicRenderer(bool forPreview, TextWriter writer) : base(writer)
    {
        // These renders have specific requirements for MAML
        this.ObjectRenderers.Replace<Markdig.Renderers.Html.ParagraphRenderer>(new ParagraphRenderer());
        this.ObjectRenderers.Replace<Markdig.Renderers.Html.Inlines.CodeInlineRenderer>(new CodeInlineRenderer());

        // If rendering for preview, leave the links as is for the previewer to handle
        if(!forPreview)
            this.ObjectRenderers.Replace<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(new LinkInlineRenderer());

        this.ObjectRenderers.Replace<Markdig.Renderers.Html.CodeBlockRenderer>(new CodeBlockRenderer());
        this.ObjectRenderers.InsertBefore<Markdig.Renderers.Html.QuoteBlockRenderer>(new AlertBlockRenderer());
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <inheritdoc />
    /// <returns>The rendered Markdown object</returns>
    public override object Render(MarkdownObject markdownObject)
    {
        if(markdownObject is not MarkdownDocument document)
        {
            this.Write(markdownObject);

            return this.Writer;
        }

        // Remove any Front Matter blocks as we don't handle them
        var frontMatterBlocks = document.Where(m => m is YamlFrontMatterBlock).ToList();

        foreach(var b in frontMatterBlocks)
            document.Remove(b);

        // We still need the MAML wrapper elements.  The ID and document type don't matter though.
        this.Writer.WriteLine($@"<?xml version=""1.0"" encoding=""utf-8""?>
<topic id=""{this.Id ?? "IDNotSet"}"" revisionNumber=""1"">
  <developerConceptualDocument
    xmlns=""http://ddue.schemas.microsoft.com/authoring/2003/5""
    xmlns:xlink=""http://www.w3.org/1999/xlink"">");

        // If there are no headings, we can wrap everything in a single untitled section without an introduction
        if(!document.Any(m => m is HeadingBlock))
        {
            this.WriteLine("<section>").WriteLine("<content>");

            foreach(MarkdownObject m in document)
                this.Write(m);

            this.Writer.WriteLine("</content>\n</section>");
        }
        else
        {
            // Render an optional introduction followed by one or more sections.  Nesting sections would get
            // rather complicated so we just render all sections at the top level.  An attribute on the title
            // element is used to render the proper heading level tag.
            bool isFirstSection = true, hasIntro = document[0] is not HeadingBlock;

            if(hasIntro)
                this.Writer.Write("<introduction>");

            foreach(MarkdownObject m in document)
            {
                if(m is HeadingBlock h)
                {
                    if(hasIntro)
                    {
                        this.Writer.WriteLine("</introduction>");

                        hasIntro = false;
                    }
                    else
                    {
                        if(!isFirstSection)
                            this.Writer.WriteLine("</content>");
                    }

                    if(!isFirstSection)
                        this.Writer.WriteLine("</section>");

                    isFirstSection = false;

                    this.Write("<section").WriteAttributes(h).WriteLine('>');
                    this.Write($"<title level=\"{h.Level}\">").WriteLeafInline(h).WriteLine("</title>").WriteLine("<content>");
                }
                else
                    this.Write(m);
            }

            if(hasIntro)
                this.Writer.Write("</introduction>");
            else
            {
                this.Writer.WriteLine("</content>\n</section>");
            }
        }

        this.Writer.WriteLine("</developerConceptualDocument>\r\n</topic>\r\n");

        return this.Writer;
    }

    /// <summary>
    /// This is used to reset the renderer
    /// </summary>
    public void ResetRenderer()
    {
        this.Reset();
    }
    #endregion
}
