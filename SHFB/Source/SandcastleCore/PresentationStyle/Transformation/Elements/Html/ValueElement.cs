//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ValueElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle value elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/11/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>value</c> elements
    /// </summary>
    public class ValueElement : NamedSectionElement
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ValueElement() : base("value")
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

            string titleIncludeItem = transformation.ApiMember.ApiSubgroup == Reflection.ApiMemberGroup.Property ?
                "title_propertyValue" : "title_fieldValue";

            var (titleElement, contentElement) = transformation.CreateSubsection(true, titleIncludeItem);
            var content = transformation.CurrentElement;

            if(titleElement != null)
                content.Add(titleElement);

            if(contentElement != null)
            {
                content.Add(contentElement);
                content = contentElement;
            }

            var typeInfo = transformation.ReferenceNode.Element("returns")?.Elements().First();

            if(typeInfo != null)
            {
                var parameter = new XElement("parameter");
                content.Add(new XElement("include",
                        new XAttribute("item", "typeLink"), parameter),
                    new XElement("br"));

                transformation.RenderTypeReferenceLink(parameter, typeInfo, true);
            }

            transformation.RenderChildElements(content, element.Nodes());
        }
        #endregion
    }
}
