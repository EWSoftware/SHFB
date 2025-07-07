//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TemplatesElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle templates elements
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

using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle <c>templates</c> elements
    /// </summary>
    public class TemplatesElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the template parameter style for HTML presentation styles
        /// </summary>
        /// <value>The default if not set explicitly is "parameter"</value>
        public string TemplateParameterStyle { get; set; } = "parameter";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public TemplatesElement() : base("templates")
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

            var (titleElement, contentElement) = transformation.CreateSubsection(true, "title_templates");
            var content = transformation.CurrentElement;

            if(titleElement != null)
                content.Add(titleElement);

            if(contentElement != null)
            {
                content.Add(contentElement);
                content = contentElement;
            }

            if(transformation.SupportedFormats != HelpFileFormats.OpenXml)
            {
                var dl = new XElement("dl");

                content.Add(dl);

                foreach(var t in element.Elements("template"))
                {
                    var dt = new XElement("dt");

                    dl.Add(dt);

                    if(transformation.SupportedFormats != HelpFileFormats.Markdown)
                    {
                        dt.Add(new XAttribute("class", "has-text-weight-normal"),
                            new XElement("span",
                                new XAttribute("class", this.TemplateParameterStyle),
                                t.Attribute("name").Value));
                    }

                    var dd = new XElement("dd");
                    dl.Add(dd);

                    var typeParamComments = transformation.CommentsNode.Elements("typeparam").Where(
                        tpc => tpc.Attribute("name")?.Value == t.Attribute("name")?.Value).FirstOrDefault();

                    if(typeParamComments != null)
                        transformation.RenderChildElements(dd, typeParamComments.Nodes());
                }
            }
            else
            {
                foreach(var t in element.Elements("template"))
                {
                    content.Add(new XElement(OpenXml.OpenXmlElement.WordProcessingML + "p",
                        new XElement(OpenXml.OpenXmlElement.WordProcessingML + "pPr",
                            new XElement(OpenXml.OpenXmlElement.WordProcessingML + "spacing",
                                new XAttribute(OpenXml.OpenXmlElement.WordProcessingML + "after", "0"))),
                        new XElement(OpenXml.OpenXmlElement.WordProcessingML + "r",
                            new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rPr",
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rStyle",
                                    new XAttribute(OpenXml.OpenXmlElement.WordProcessingML + "val", "Parameter"))),
                            new XElement(OpenXml.OpenXmlElement.WordProcessingML + "t",
                                t.Attribute("name").Value))));

                    var typeParamComments = transformation.CommentsNode.Elements("typeparam").Where(
                        tpc => tpc.Attribute("name")?.Value == t.Attribute("name")?.Value).FirstOrDefault();

                    if(typeParamComments != null)
                        transformation.RenderChildElements(content, typeParamComments.Nodes());
                }
            }
        }
        #endregion
    }
}
