//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PreliminaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/27/2022
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
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the container element that will hold the preliminary text
        /// </summary>
        /// <value>The default if not set explicitly is <c>span</c></value>
        public string PreliminaryContainerElement { get; set; } = "span";

        /// <summary>
        /// This is used to get or set the preliminary text style
        /// </summary>
        /// <value>The default if not set explicitly is <c>tag is-warning is-medium</c></value>
        public string PreliminaryTextStyle { get; set; } = "tag is-warning is-medium";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public PreliminaryElement() : base("preliminary")
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

            var div = new XElement(this.PreliminaryContainerElement, new XAttribute("class", this.PreliminaryTextStyle));
            string text = element.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(text))
                div.Add(text);
            else
                div.Add(new XElement("include", new XAttribute("item", "preliminaryApi")));

            transformation.CurrentElement.Add(div);
        }
        #endregion
    }
}
