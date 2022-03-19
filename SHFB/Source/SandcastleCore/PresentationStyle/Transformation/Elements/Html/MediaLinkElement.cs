//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MediaLinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/19/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
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

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles <c>mediaLinkInline</c> elements
    /// </summary>
    public class MediaLinkElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the image caption style
        /// </summary>
        /// <value>The default if not set explicitly is "caption"</value>
        public string ImageCaptionStyle { get; set; } = "caption";

        /// <summary>
        /// This is used to get or set the caption lead text style
        /// </summary>
        /// <value>The default if not set explicitly is "captionLead"</value>
        public string CaptionLeadTextStyle { get; set; } = "captionLead";

        /// <summary>
        /// This is used to get or set the media near style
        /// </summary>
        /// <value>The default if not set explicitly is "mediaNear"</value>
        public string MediaNearStyle { get; set; } = "mediaNear";

        /// <summary>
        /// This is used to get or set the media center style
        /// </summary>
        /// <value>The default if not set explicitly is "mediaCenter"</value>
        public string MediaCenterStyle { get; set; } = "mediaCenter";

        /// <summary>
        /// This is used to get or set the media far style
        /// </summary>
        /// <value>The default if not set explicitly is "mediaFar"</value>
        public string MediaFarStyle { get; set; } = "mediaFar";

        #endregion

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

            XElement captionDiv = null, image = element.Element(Ddue + "image");
            string linkTarget = image?.Attribute(Xlink + "href")?.Value;

            if(!String.IsNullOrWhiteSpace(linkTarget))
            {
                string placement = image.Attribute("placement")?.Value;
                var link = new XElement("div");

                switch(placement)
                {
                    case "center":
                        link.Add(new XAttribute("class", this.MediaCenterStyle));
                        break;

                    case "far":
                        link.Add(new XAttribute("class", this.MediaFarStyle));
                        break;

                    default:
                        link.Add(new XAttribute("class", this.MediaNearStyle));
                        break;
                }

                var caption = element.Element(Ddue + "caption");
                string captionText = caption?.Value.NormalizeWhiteSpace();

                if(caption != null && !String.IsNullOrWhiteSpace(captionText))
                {
                    placement = caption.Attribute("placement")?.Value;
                    string lead = caption.Attribute("lead")?.Value.NormalizeWhiteSpace();
                    captionDiv = new XElement("div", new XAttribute("class", this.ImageCaptionStyle));

                    if(!String.IsNullOrWhiteSpace(lead))
                    {
                        captionDiv.Add(new XElement("span",
                            new XAttribute("class", this.CaptionLeadTextStyle), lead + ": "));
                    }

                    captionDiv.Add(captionText);

                    // If placed after, it is added after the art link
                    if(!placement?.Equals("after", StringComparison.Ordinal) ?? true)
                    {
                        link.Add(captionDiv);
                        captionDiv = null;
                    }
                }

                link.Add(new XElement("artLink",
                    new XAttribute("target", linkTarget)));

                if(captionDiv != null)
                    link.Add(captionDiv);

                transformation.CurrentElement.Add(link);
            }
        }
        #endregion
    }
}
