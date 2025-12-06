//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LinkInlineRenderer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/04/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a link inline renderer that converts reference links to images, conceptual topics, and
// code entity references to the form recognized by the build components.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/03/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;

using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

using Sandcastle.Core.PresentationStyle.Transformation;

namespace Sandcastle.Core.Markdown.Renderers;

/// <summary>
/// This is a derived link inline renderer that converts reference links to images, conceptual topics, and
/// code entity references to the form recognized by the build components.
/// </summary>
public sealed class LinkInlineRenderer : Markdig.Renderers.Html.Inlines.LinkInlineRenderer
{
    #region Method overrides
    //=====================================================================

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, LinkInline link)
    {
        if(link.Url != null && link.Url.Length > 1)
        {
            var attributes = link.TryGetAttributes();

            if(link.Url[0] == '@')
            {
                if(link.IsImage)
                {
                    // !!TODO: Add support for LiteralInline inner text to override the image alternate text
                    // A new attribute is needed that the ResolveArtLinksComponent can use to set it.
                    renderer.Write($"<artLink target=\"{link.Url.Substring(1)}\" />");
                }
                else
                {
                    if(link.Url[2] == ':')
                        RenderReferenceLink(renderer, link, attributes);
                    else
                        RenderConceptualTopicLink(renderer, link);
                }

                return;
            }

            // External links can pass through as is.  The default for them is to open them in a new tab/window
            // so we'll add the target if not explicitly set.
            if(!link.IsImage && link.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
              (!attributes?.Properties.Any(p => p.Key == "target") ?? true))
            {
                if(attributes == null)
                {
                    attributes = new HtmlAttributes();
                    link.SetAttributes(attributes);
                }

                attributes.AddProperty("target", "_blank");
            }
        }

        base.Write(renderer, link);
    }
    #endregion

    #region Helper methods
    //=====================================================================

    /// <summary>
    /// Render a reference link
    /// </summary>
    /// <param name="renderer">The HTML renderer to use</param>
    /// <param name="link">The link to render</param>
    /// <param name="attributes">The link attributes</param>
    private static void RenderReferenceLink(HtmlRenderer renderer, LinkInline link, HtmlAttributes attributes)
    {
        string linkTarget = link.Url.Substring(1).NormalizeWhiteSpace();

        renderer.Write($"<referenceLink target=\"{linkTarget}\"");

        if(attributes != null)
        {
            string showContainer = attributes.Properties.FirstOrDefault(p => p.Key == "show-container").Value,
                showParameters = attributes.Properties.FirstOrDefault(p => p.Key == "show-parameters").Value,
                preferOverload = attributes.Properties.FirstOrDefault(p => p.Key == "prefer-overload").Value;
            
            if(!String.IsNullOrWhiteSpace(showContainer))
                renderer.Write($" show-container=\"{showContainer}\"");

            if(!String.IsNullOrWhiteSpace(showParameters))
                renderer.Write($" show-parameters=\"{showParameters}\"");

            if(!String.IsNullOrWhiteSpace(preferOverload))
                renderer.Write($" prefer-overload=\"{preferOverload}\"");
        }

        if(link.FirstChild is LiteralInline innerText)
            renderer.Write($">{innerText.Content}</referenceLink>");
        else
        {
            if(linkTarget.StartsWith("R:", StringComparison.Ordinal))
                renderer.Write($"><include item=\"topicTitle_Root\" /></referenceLink>");
            else
                renderer.Write(" />");
        }
    }

    /// <summary>
    /// Render a conceptual topic link
    /// </summary>
    /// <param name="renderer">The HTML renderer to use</param>
    /// <param name="link">The link to render</param>
    private static void RenderConceptualTopicLink(HtmlRenderer renderer, LinkInline link)
    {
        string linkTarget = link.Url.Substring(1).NormalizeWhiteSpace();

        renderer.Write($"<conceptualLink target=\"{linkTarget}\"");

        if(link.FirstChild is not null)
        {
            renderer.Write(">");
            renderer.WriteChildren(link);
            renderer.Write("</conceptualLink>");
        }
        else
            renderer.Write(" />");
    }
    #endregion
}
