//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/04/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle link elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/23/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles <c>link</c> elements
    /// </summary>
    public class LinkElement : Element
    {
        /// <inheritdoc />
        public LinkElement() : base("link")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            string linkTarget = element.Attribute(Xlink + "href")?.Value;

            if(!String.IsNullOrWhiteSpace(linkTarget))
            {
                XElement link;

                // In-page link or verified external link?
                if(linkTarget[0] == '#')
                    link = new XElement("a", new XAttribute("href", linkTarget));
                else
                    link = new XElement("conceptualLink", new XAttribute("target", linkTarget));

                transformation.CurrentElement.Add(link);
                transformation.RenderChildElements(link, element.Nodes());
            }
        }
    }
}
