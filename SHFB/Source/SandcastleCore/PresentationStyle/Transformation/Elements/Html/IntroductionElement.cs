//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IntroductionElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/23/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle introduction elements
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>introduction</c> elements in a topic
    /// </summary>
    public class IntroductionElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public IntroductionElement() : base("introduction")
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

            if(element.Elements().Any() || element.Value.NormalizeWhiteSpace().Length != 0)
            {
                var intro = new XElement("div", transformation.StyleAttributeFor(CommonStyle.Introduction));

                string address = element.Attribute("address")?.Value;

                if(!String.IsNullOrWhiteSpace(address))
                    intro.Add(new XAttribute("id", address));

                transformation.CurrentElement.Add(intro);
                transformation.RenderChildElements(intro, element.Nodes());
            }
        }
        #endregion
    }
}
