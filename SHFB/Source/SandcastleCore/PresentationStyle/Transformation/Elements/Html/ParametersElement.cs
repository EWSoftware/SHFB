//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ParametersElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/05/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle parameters elements based on the topic type
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
    /// This is used to handle <c>parameters</c> elements based on the topic type
    /// </summary>
    public class ParametersElement : NamedSectionElement
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ParametersElement() : base("parameters")
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

            // Sandcastle has never defined how to handle the parameters element in MAML topics so we'll just
            // treat it as a named section element.
            if(transformation.IsMamlTopic)
                base.Render(transformation, element);
            else
            {
                // For API topics, we do have a defined structure for the parameters element in a syntax section
                var (titleElement, contentElement) = transformation.CreateSubsection(true, "title_parameters");
                var content = transformation.CurrentElement;

                if(titleElement != null)
                    content.Add(titleElement);

                if(contentElement != null)
                {
                    content.Add(contentElement);
                    content = contentElement;
                }

                var dl = new XElement("dl");
                
                content.Add(dl);

                foreach(var p in element.Elements("parameter"))
                {
                    string optional = null;

                    if(p.Attribute("optional") != null)
                        optional = " (Optional)";

                    dl.Add(new XElement("dt",
                        new XElement("span",
                            transformation.StyleAttributeFor(CommonStyle.Parameter),
                            p.Attribute("name").Value), optional));

                    var parameter = new XElement("parameter");
                    var dd = new XElement("dd",
                        new XElement("include",
                            new XAttribute("item", "typeLink"), parameter),
                        new XElement("br"));
                    dl.Add(dd);

                    transformation.RenderTypeReferenceLink(parameter, p.Elements().First(), true);

                    var paramComments = transformation.CommentsNode.Elements("param").Where(
                        pc => pc.Attribute("name")?.Value == p.Attribute("name")?.Value).FirstOrDefault();

                    if(paramComments != null)
                        transformation.RenderChildElements(dd, paramComments.Nodes());
                }
            }
        }
        #endregion
    }
}
