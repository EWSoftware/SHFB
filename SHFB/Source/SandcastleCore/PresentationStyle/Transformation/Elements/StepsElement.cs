//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : StepsElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/10/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle steps elements
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
    /// This is used to handle general <c>steps</c> elements in a topic
    /// </summary>
    public class StepsElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public StepsElement() : base("steps")
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

            string style = element.Attribute("class")?.Value;
            int steps = element.Elements(Ddue + "step").Count();
            XElement list;

            // Open XML always uses an unordered list element and applies a class attribute to let the Open XML
            // file generator know how to style the list.
            if(steps > 1 && style.Equals("ordered", StringComparison.Ordinal) &&
              transformation.SupportedFormats != HelpFileFormats.OpenXml)
            {
                list = new XElement("ol");
            }
            else
                list = new XElement("ul");

            if(transformation.SupportedFormats == HelpFileFormats.OpenXml)
                list.Add(new XAttribute("class", steps > 1 ? style : "bullet"));

            transformation.CurrentElement.Add(list);
            transformation.RenderChildElements(list, element.Elements(Ddue + "step"));
        }
        #endregion
    }
}
