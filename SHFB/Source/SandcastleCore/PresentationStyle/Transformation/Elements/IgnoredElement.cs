//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IgnoredElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/21/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to completely ignore an element and any child elements it contains so that
// they are not rendered in the topic at all.
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

using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This can be used to completely ignore an element and any child elements it contains so that they are not
    /// rendered in the topic at all.
    /// </summary>
    public class IgnoredElement : Element
    {
        /// <inheritdoc />
        public IgnoredElement(string name) : base(name)
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            // Do nothing so that they are omitted completely
        }
    }
}
