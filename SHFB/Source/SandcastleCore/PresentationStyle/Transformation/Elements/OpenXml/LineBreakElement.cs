//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LineBreakElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/09/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle line break elements in Open XMl topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/04/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml
{
    /// <summary>
    /// This handles line break elements in Open XMl topics
    /// </summary>
    public class LineBreakElement : OpenXmlElement
    {
        /// <inheritdoc />
        public LineBreakElement(string name) : base(name)
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            transformation.CurrentElement.Add(new XElement(WordProcessingML + "r",
                new XElement(WordProcessingML + "br")));
        }
    }
}
