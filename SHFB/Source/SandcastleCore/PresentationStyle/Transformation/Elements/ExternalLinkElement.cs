//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ExternalLinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/04/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle externalLink elements
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
    /// This handles <c>externalLink</c> elements
    /// </summary>
    public class ExternalLinkElement : Element
    {
        /// <inheritdoc />
        public ExternalLinkElement() : base("externalLink")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            string linkTarget = element.Element(Ddue + "linkTarget")?.Value.NormalizeWhiteSpace(),
                linkAltText = element.Element(Ddue + "linkAlternateText")?.Value.NormalizeWhiteSpace();

            if(String.IsNullOrWhiteSpace(linkTarget))
                linkTarget = "_blank";

            var link = new XElement("a",
                new XAttribute("href", element.Element(Ddue + "linkUri")?.Value.NormalizeWhiteSpace()),
                new XAttribute("target", linkTarget),
                new XAttribute("rel", "noopener noreferrer"),
                element.Element(Ddue + "linkText")?.Value.NormalizeWhiteSpace());

            if(!String.IsNullOrWhiteSpace(linkAltText))
                link.Add(new XAttribute("title", linkAltText));

            transformation.CurrentElement.Add(link);
        }
    }
}
