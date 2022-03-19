﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SummaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/19/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the summary element based on the topic type
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
    /// This handles the <c>summary</c> element based on the topic type
    /// </summary>
    public class SummaryElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the summary style
        /// </summary>
        /// <value>The default if not set explicitly is "summary"</value>
        public string SummaryStyle { get; set; } = "summary";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public SummaryElement() : base("summary")
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

            // The ddue:summary element is redundant since it is optional in the MAML schema but
            // ddue:introduction is not.  Using abstract="true" will prevent the summary from being included in
            // the topic.  If it is true, it will be used for the Abstract help metadata element.
            if(!transformation.IsMamlTopic || !((bool?)element.Attribute("abstract") ?? false))
            {
                var div = new XElement("div", new XAttribute("class", this.SummaryStyle));

                transformation.CurrentElement.Add(div);
                transformation.RenderChildElements(div, element.Nodes());
            }
        }
        #endregion
    }
}
