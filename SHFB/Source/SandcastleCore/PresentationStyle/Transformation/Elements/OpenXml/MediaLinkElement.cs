//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MediaLinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/10/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle mediaLink elements in Open XML topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/09/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml
{
    /// <summary>
    /// This handles <c>mediaLinkInline</c> elements in Open XML topics
    /// </summary>
    public class MediaLinkElement : OpenXmlElement
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public MediaLinkElement() : base("mediaLink")
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

            XElement captionPara = null, image = element.Element(Ddue + "image");
            string linkTarget = image?.Attribute(Xlink + "href")?.Value;

            if(!String.IsNullOrWhiteSpace(linkTarget))
            {
                string placement = image.Attribute("placement")?.Value;
                var properties = new XElement(WordProcessingML + "pPr");

                switch(placement)
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

                var caption = element.Element(Ddue + "caption");
                string captionText = caption?.Value.NormalizeWhiteSpace();

                if(caption != null && !String.IsNullOrWhiteSpace(captionText))
                {
                    placement = caption.Attribute("placement")?.Value;
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
                    if(!placement?.Equals("after", StringComparison.Ordinal) ?? true)
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
}
