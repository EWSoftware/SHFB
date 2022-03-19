//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ProcedureElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/16/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle procedure elements
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
    /// This is used to handle general <c>procedure</c> elements in a topic
    /// </summary>
    public class ProcedureElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ProcedureElement() : base("procedure")
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

            string address = element.Attribute("address")?.Value;
            var titleText = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(titleText))
            {
                var title = new XElement("h3", titleText);

                if(!String.IsNullOrWhiteSpace(address))
                    title.Add(new XAttribute("id", address));

                transformation.CurrentElement.Add(title);
            }

            var content = new XElement("div");

            if(!String.IsNullOrWhiteSpace(address) && String.IsNullOrWhiteSpace(titleText))
                content.Add(new XAttribute("id", address));

            transformation.CurrentElement.Add(content);

            // Render the steps and conclusion
            transformation.RenderChildElements(content, element.Elements(Ddue + "steps").Concat(
                element.Elements(Ddue + "conclusion")));
        }
        #endregion
    }
}
