//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : FigureElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle figure elements in Open XML topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/05/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This handles <c>figure</c> elements in Open XML topics
/// </summary>
public class FigureElement : OpenXmlElement
{
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

        XElement captionPara = null, image = element.Element(Ddue + "img") ??
            element.Descendants(Ddue + "artLink").FirstOrDefault();
        string linkTarget = (image?.Attribute("src") ?? image?.Attribute("target"))?.Value;

        if(!String.IsNullOrWhiteSpace(linkTarget))
        {
            // If a placement attribute is present, convert it to a class attribute.  The placement attribute
            // may be on the figure caption element if coming from a parsed Markdown figure.
            var placement = element.Attribute("placement") ?? element.Element(Ddue + "figcaption")?.Attribute("placement");
            var properties = new XElement(WordProcessingML + "pPr");

            switch(placement?.Value)
            {
                case "center":
                    properties.Add(new XElement(WordProcessingML + "jc",
                        new XAttribute(WordProcessingML + "val", "center")));
                    break;

                case "far":
                    properties.Add(new XElement(WordProcessingML + "jc",
                        new XAttribute(WordProcessingML + "val", "right")));
                    break;

                default:
                    properties.Add(new XElement(WordProcessingML + "jc",
                        new XAttribute(WordProcessingML + "val", "left")));
                    break;
            }

            // If converted from Markdown, an extra empty figcaption element may be added if the placement
            // attribute is before the image.  It needs to be removed.  Otherwise, just remove the attribute.
            if(placement?.Parent.Name.LocalName == "figcaption" && placement?.Parent.FirstNode == null)
                placement.Parent.Remove();

            var caption = element.Element(Ddue + "figcaption");
            string captionText = caption?.Value.NormalizeWhiteSpace();

            if(caption != null && !String.IsNullOrWhiteSpace(captionText))
            {
                string lead = caption.Attribute("lead")?.Value.NormalizeWhiteSpace();

                var captionProps = new XElement(properties);
                captionProps.AddFirst(new XElement(WordProcessingML + "pStyle",
                    new XAttribute(WordProcessingML + "val", "Caption")));

                captionPara = new XElement(WordProcessingML + "p", captionProps);

                if(!String.IsNullOrWhiteSpace(lead))
                {
                    captionPara.Add(new XElement(WordProcessingML + "r",
                        new XElement(WordProcessingML + "rPr",
                            new XElement(WordProcessingML + "b")),
                        new XElement(WordProcessingML + "t",
                            new XAttribute(XmlSpace, "preserve"), lead, ": ")));
                }

                captionPara.Add(new XElement(WordProcessingML + "r",
                    new XElement(WordProcessingML + "t", captionText)));

                // If placed after, it is added after the art link
                if(caption.NextNode != null)
                {
                    captionProps.Element(WordProcessingML + "pStyle").AddAfterSelf(
                        new XElement(WordProcessingML + "keepNext"));
                    transformation.CurrentElement.Add(captionPara);
                    captionPara = null;
                }
            }

            transformation.CurrentElement.Add(new XElement(WordProcessingML + "p", properties,
                new XElement(WordProcessingML + "r",
                    new XElement("artLink", new XAttribute("target", linkTarget)))));

            if(captionPara != null)
                transformation.CurrentElement.Add(captionPara);
        }
    }
    #endregion
}
