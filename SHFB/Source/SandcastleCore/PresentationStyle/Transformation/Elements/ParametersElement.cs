//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ParametersElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
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

using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle <c>parameters</c> elements based on the topic type
    /// </summary>
    public class ParametersElement : NamedSectionElement
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the parameter style for HTML presentation styles
        /// </summary>
        /// <value>The default if not set explicitly is "parameter"</value>
        public string ParameterStyle { get; set; } = "parameter";

        #endregion

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

                if(transformation.SupportedFormats != HelpFileFormats.OpenXml)
                {
                    var dl = new XElement("dl");

                    content.Add(dl);

                    foreach(var p in element.Elements("parameter"))
                    {
                        var dt = new XElement("dt");

                        if(transformation.SupportedFormats != HelpFileFormats.Markdown)
                        {
                            dt.Add(new XAttribute("class", "has-text-weight-normal"),
                                new XElement("span",
                                    new XAttribute("class", this.ParameterStyle),
                                    p.Attribute("name").Value));
                        }

                        dt.Add(NonBreakingSpace, NonBreakingSpace);

                        transformation.RenderTypeReferenceLink(dt, p.Elements().First(), false);

                        if(p.Attribute("optional") != null)
                        {
                            dt.Add(NonBreakingSpace, NonBreakingSpace,
                                new XElement("include", new XAttribute("item", "optionalText")));
                        }

                        var dd = new XElement("dd");
                        var paramComments = transformation.CommentsNode.Elements("param").Where(
                            pc => pc.Attribute("name")?.Value == p.Attribute("name")?.Value).FirstOrDefault();

                        dl.Add(dt, dd);

                        if(paramComments != null)
                            transformation.RenderChildElements(dd, paramComments.Nodes());
                        else
                            dd.Add(NonBreakingSpace);
                    }
                }
                else
                {
                    foreach(var p in element.Elements("parameter"))
                    {
                        var para = new XElement(OpenXml.OpenXmlElement.WordProcessingML + "p",
                            new XElement(OpenXml.OpenXmlElement.WordProcessingML + "pPr",
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "spacing",
                                    new XAttribute(OpenXml.OpenXmlElement.WordProcessingML + "after", "0"))),
                            new XElement(OpenXml.OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rPr",
                                    new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rStyle",
                                        new XAttribute(OpenXml.OpenXmlElement.WordProcessingML + "val", "Parameter"))),
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "t",
                                    new XAttribute(XmlSpace, "preserve"),
                                    p.Attribute("name").Value + "  ")));

                        transformation.RenderTypeReferenceLink(para, p.Elements().First(), false);

                        if(p.Attribute("optional") != null)
                        {
                            para.Add(new XElement(OpenXml.OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "t",
                                    new XAttribute(XmlSpace, "preserve"), "  ")),
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXml.OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "optionalText")))));
                        }

                        content.Add(para);

                        var paramComments = transformation.CommentsNode.Elements("param").Where(
                            pc => pc.Attribute("name")?.Value == p.Attribute("name")?.Value).FirstOrDefault();

                        if(paramComments != null)
                            transformation.RenderChildElements(content, paramComments.Nodes());
                    }
                }
            }
        }
        #endregion
    }
}
