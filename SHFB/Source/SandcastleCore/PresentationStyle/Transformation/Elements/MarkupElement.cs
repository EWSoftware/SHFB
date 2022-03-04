//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkupElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/13/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the markup element, a parent element that does not itself have any
// rendered representation.  It just passes through clones of its child elements without any namespaces.
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
    /// This handles the <c>markup</c> element, a parent element that does not itself have any rendered
    /// representation.  It just clones the child nodes, removes any XML namespaces, and passes them through
    /// as-is.
    /// </summary>
    /// <remarks>This will allow build components and topic authors to add HTML or other elements such as
    /// <c>include</c> elements for localized shared content to a pre-transformed document.  This prevents it
    /// from being removed as unrecognized content by the transformations.</remarks>
    public class MarkupElement : Element
    {
        /// <inheritdoc />
        public MarkupElement() : base("markup")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            XElement clone = new XElement(element);

            clone.RemoveNamespaces();

            foreach(var child in clone.Nodes())
                transformation.CurrentElement.Add(child);
        }
    }
}
