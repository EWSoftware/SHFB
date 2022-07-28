//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ListElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/24/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle list elements based on the topic type
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

// Ignore Spelling: hoverable thead

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles <c>list</c> elements based on the topic type
    /// </summary>
    public class ListElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the "no bullet" list style
        /// </summary>
        /// <value>The default if not set explicitly is <c>noBullet</c></value>
        public string NoBulletStyle { get; set; } = "noBullet";

        /// <summary>
        /// This is used to get or set the overall table style
        /// </summary>
        /// <value>The default if not set explicitly is <c>table is-hoverable"</c></value>
        public string TableStyle { get; set; } = "table is-hoverable";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ListElement() : base("list")
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

            if(transformation.IsMamlTopic)
                this.RenderMamlList(transformation, element);
            else
                this.RenderXmlCommentsList(transformation, element);

            if(transformation.SupportedFormats == HelpFileFormats.Markdown)
                transformation.CurrentElement.Add("\n\n");
        }

        /// <summary>
        /// Render a list element in a MAML topic
        /// </summary>
        /// <param name="transformation">The transformation in use</param>
        /// <param name="element">The element to render</param>
        private void RenderMamlList(TopicTransformationCore transformation, XElement element)
        {
            string elementName = "ul", start = null;
            string styleName = null;

            switch(element.Attribute("class")?.Value)
            {
                case "bullet":
                    break;

                case "ordered":
                    elementName = "ol";
                    start = element.Attribute("start")?.Value;
                    break;

                default:
                    styleName = this.NoBulletStyle;
                    break;
            }

            var list = new XElement(elementName);

            if(styleName != null)
                list.Add(new XAttribute("class", styleName));

            if(!String.IsNullOrWhiteSpace(start))
                list.Add(new XAttribute("start", start));

            transformation.CurrentElement.Add(list);
            transformation.RenderChildElements(list, element.Nodes());
        }

        /// <summary>
        /// Render a list element in XML comments in an API topic
        /// </summary>
        /// <param name="transformation">The transformation in use</param>
        /// <param name="element">The element to render</param>
        private void RenderXmlCommentsList(TopicTransformationCore transformation, XElement element)
        {
            string elementName, start = null;

            switch(element.Attribute("type")?.Value)
            {
                case "definition":
                    elementName = "dl";
                    break;

                case "number":
                    elementName = "ol";
                    start = element.Attribute("start")?.Value;
                    break;

                case "table":
                    elementName = "table";
                    break;

                default:    // "bullet", not specified, or unrecognized
                    elementName = "ul";
                    break;
            }

            var list = new XElement(elementName);

            if(!String.IsNullOrWhiteSpace(start))
                list.Add(new XAttribute("start", start));

            transformation.CurrentElement.Add(list);

            switch(elementName)
            {
                case "dl":
                    foreach(var item in element.Elements("item"))
                    {
                        XElement term = item.Element("term"), description = item.Element("description");

                        if(term != null && description != null)
                        {
                            var dt = new XElement("dt");
                            list.Add(dt);
                            transformation.RenderChildElements(dt, term.Nodes());

                            var dd = new XElement("dd");
                            list.Add(dd);
                            transformation.RenderChildElements(dd, description.Nodes());
                        }
                    }
                    break;

                case "table":
                    XElement listHeader = element.Element("listheader");

                    if(!String.IsNullOrWhiteSpace(this.TableStyle) &&
                      transformation.SupportedFormats != HelpFileFormats.Markdown)
                    {
                        list.Add(new XAttribute("class", this.TableStyle));
                    }

                    if(listHeader != null)
                    {
                        var thead = new XElement("thead");
                        list.Add(thead);
                        
                        var tr = new XElement("tr");
                        thead.Add(tr);

                        foreach(var cell in listHeader.Elements())
                        {
                            var th = new XElement("th");
                            tr.Add(th);
                            transformation.RenderChildElements(th, cell.Nodes());
                        }
                    }

                    foreach(var item in element.Elements("item"))
                    {
                        var tr = new XElement("tr");
                        list.Add(tr);

                        foreach(var cell in item.Elements())
                        {
                            var td = new XElement("td");
                            tr.Add(td);
                            transformation.RenderChildElements(td, cell.Nodes());
                        }
                    }
                    break;

                default:    // ol or ul
                    foreach(var item in element.Elements("item"))
                    {
                        XElement li = new XElement("li"), term = item.Element("term"),
                            description = item.Element("description");
                        list.Add(li);

                        if(term != null || description != null)
                        {
                            if(term != null)
                            {
                                var strong = new XElement("strong");
                                li.Add(strong, " \u2013 ");
                                transformation.RenderChildElements(strong, term.Nodes());
                            }

                            transformation.RenderChildElements(li, description.Nodes());
                        }
                        else
                            transformation.RenderChildElements(li, item.Nodes());
                    }
                    break;
            }
        }
        #endregion
    }
}
