//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PreliminaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/01/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the preliminary element
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles the <c>preliminary</c> element
    /// </summary>
    public class PreliminaryElement : Element
    {
        /// <inheritdoc />
        public PreliminaryElement() : base("preliminary")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var div = new XElement("div", transformation.StyleAttributeFor(CommonStyle.Preliminary));
            string text = element.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(text))
                div.Add(text);
            else
                div.Add(new XElement("include", new XAttribute("item", "preliminaryText")));

            transformation.CurrentElement.Add(div);
        }
    }
}
