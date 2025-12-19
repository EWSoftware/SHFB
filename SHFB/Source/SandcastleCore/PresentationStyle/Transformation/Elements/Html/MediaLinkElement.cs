//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MediaLinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/22/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle mediaLink elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/24/2022  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: figcaption

using System;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Conversion;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;

/// <summary>
/// This handles <c>mediaLinkInline</c> elements
/// </summary>
public class MediaLinkElement : Element
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// The element used to contain the media link
    /// </summary>
    /// <value>The default if not set explicitly is <c>figure</c></value>
    public string MediaElement { get; set; } = "figure";

    /// <summary>
    /// The element used to contain the media link caption
    /// </summary>
    /// <value>The default if not set explicitly is <c>figcaption</c></value>
    public string MediaCaptionElement { get; set; } = "figcaption";

    /// <summary>
    /// This is used to get or set the image caption style
    /// </summary>
    /// <value>No style is applied if not set explicitly</value>
    public string CaptionStyle { get; set; }

    /// <summary>
    /// This is used to get or set the caption lead text style
    /// </summary>
    /// <value>No style is applied if not set explicitly</value>
    public string CaptionLeadTextStyle { get; set; }

    /// <summary>
    /// This is used to get or set the media near style
    /// </summary>
    /// <value>The default if not set explicitly is <c>mediaNear</c></value>
    public string MediaNearStyle { get; set; } = "mediaNear";

    /// <summary>
    /// This is used to get or set the media center style
    /// </summary>
    /// <value>The default if not set explicitly is <c>mediaCenter</c></value>
    public string MediaCenterStyle { get; set; } = "mediaCenter";

    /// <summary>
    /// This is used to get or set the media far style
    /// </summary>
    /// <value>The default if not set explicitly is <c>mediaFar</c></value>
    public string MediaFarStyle { get; set; } = "mediaFar";

    #endregion

    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public MediaLinkElement() : base("mediaLink", true)
    {
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        XElement image = element.Element(Ddue + "image");
        string linkTarget = image?.Attribute(Xlink + "href")?.Value;

        if(String.IsNullOrWhiteSpace(linkTarget))
            return;

        XElement mediaLink, captionDiv = null, caption = element.Element(Ddue + "caption");
        string placement = image.Attribute("placement")?.Value, captionText = caption?.Value.NormalizeWhiteSpace(),
            captionPlacement = caption?.Attribute("placement")?.Value,
            lead = caption?.Attribute("lead")?.Value.NormalizeWhiteSpace();

        if(transformation is MarkdownConversionTransformation || transformation.SupportedFormats == HelpFileFormats.Markdown)
        {
            mediaLink = transformation.CurrentElement;

            mediaLink.Add("\n^^^");

            if(!String.IsNullOrWhiteSpace(captionText))
            {
                if(!String.IsNullOrWhiteSpace(lead))
                    captionText = String.Concat(" ", lead, ": ", captionText);
                else
                    captionText = String.Concat(" ", captionText);

                // If placed after, it is added after the art link
                if(!captionPlacement?.Equals("after", StringComparison.Ordinal) ?? true)
                {
                    mediaLink.Add(captionText);
                    captionText = null;
                }
            }

            if(!String.IsNullOrWhiteSpace(placement))
                mediaLink.Add($"{{placement=\"{placement}\"}}");

            mediaLink.Add($"\n![](@{linkTarget})\n^^^");

            if(captionText != null)
                mediaLink.Add(captionText);

            mediaLink.Add("\n\n");
        }
        else
        {
            mediaLink = new(this.MediaElement);

            switch(placement)
            {
                case "center":
                    mediaLink.Add(new XAttribute("class", this.MediaCenterStyle));
                    break;

                case "far":
                    mediaLink.Add(new XAttribute("class", this.MediaFarStyle));
                    break;

                default:
                    mediaLink.Add(new XAttribute("class", this.MediaNearStyle));
                    break;
            }

            if(!String.IsNullOrWhiteSpace(captionText))
            {
                captionDiv = new XElement(MediaCaptionElement);

                if(!String.IsNullOrWhiteSpace(this.CaptionStyle))
                    captionDiv.Add(new XAttribute("class", this.CaptionStyle));

                if(!String.IsNullOrWhiteSpace(lead))
                {
                    if(!String.IsNullOrWhiteSpace(this.CaptionLeadTextStyle))
                    {
                        captionDiv.Add(new XElement("span",
                            new XAttribute("class", this.CaptionLeadTextStyle), lead + ": "));
                    }
                    else
                        captionDiv.Add(lead, ": ");
                }

                captionDiv.Add(captionText);

                // If placed after, it is added after the art link
                if(!captionPlacement?.Equals("after", StringComparison.Ordinal) ?? true)
                {
                    mediaLink.Add(captionDiv);
                    captionDiv = null;
                }
            }

            mediaLink.Add(new XElement("artLink", new XAttribute("target", linkTarget)));

            if(captionDiv != null)
                mediaLink.Add(captionDiv);

            transformation.CurrentElement.Add(mediaLink);
        }
    }
    #endregion
}
