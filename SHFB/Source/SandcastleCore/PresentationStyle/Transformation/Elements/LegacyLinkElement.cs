//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LegacyLinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/04/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle legacyLink elements
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
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles <c>legacyLink</c> elements
    /// </summary>
    public class LegacyLinkElement : Element
    {
        /// <inheritdoc />
        public LegacyLinkElement() : base("legacyLink")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var link = new XElement("a",
                new XAttribute("href", element.Attribute(Xlink + "href")?.Value),
                new XAttribute("rel", "noopener noreferrer"));

            transformation.CurrentElement.Add(link);

            if(element.Nodes().Any())
                transformation.RenderChildElements(link, element.Nodes());
            else
                link.Add(element.Attribute(Xlink + "href")?.Value);
        }
    }
}
