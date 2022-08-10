//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PassthroughElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/13/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle passthrough elements by writing them out along with any attributes
// and then parsing any child nodes.
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles passthrough elements such as HTML elements by writing them out along with any attributes and
    /// then parsing any child nodes.
    /// </summary>
    public class PassthroughElement : Element
    {
        /// <inheritdoc />
        public PassthroughElement(string name) : base(name)
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var el = new XElement(this.Name);

            foreach(var attr in element.Attributes())
                el.Add(new XAttribute(attr.Name.LocalName, attr.Value));

            transformation.CurrentElement.Add(el);
            transformation.RenderChildElements(el, element.Nodes());
        }
    }
}
