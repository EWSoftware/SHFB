//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : FigureElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/04/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle figure elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/02/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;

/// <summary>
/// This handles figure elements
/// </summary>
public class FigureElement : PassthroughElement
{
    #region Properties
    //=====================================================================

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
    public FigureElement() : base("figure", true)
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

        // If a placement attribute is present, convert it to a class attribute.  The placement attribute
        // may be on the figure caption element if coming from a parsed Markdown figure.
        var placement = element.Attribute("placement") ?? element.Element(Ddue + "figcaption")?.Attribute("placement");

        if(placement != null)
        {
            switch(placement.Value)
            {
                case "center":
                    element.Add(new XAttribute("class", this.MediaCenterStyle));
                    break;

                case "far":
                    element.Add(new XAttribute("class", this.MediaFarStyle));
                    break;

                default:
                    element.Add(new XAttribute("class", this.MediaNearStyle));
                    break;
            }

            // If converted from Markdown, an extra empty figcaption element may be added if the placement
            // attribute is before the image.  It needs to be removed.  Otherwise, just remove the attribute.
            if(placement.Parent.Name.LocalName == "figcaption" && placement.Parent.FirstNode == null)
                placement.Parent.Remove();
            else
                placement.Remove();
        }
        else
            element.Add(new XAttribute("class", this.MediaNearStyle));

        // If a paragraph element exists, it's from the Markdown conversion so move the content up to replace it
        var para = element.Element(Ddue + "para");

        if(para != null)
        {
            while(para.FirstNode != null)
            {
                var node = para.FirstNode;

                node.Remove();
                para.AddBeforeSelf(node);
            }

            para.Remove();
        }

        base.Render(transformation, element);
    }
    #endregion
}
