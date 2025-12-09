//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ListElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
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

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This handles <c>list</c> elements based on the topic type
/// </summary>
public class ListElement : OpenXmlElement
{
    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public ListElement() : base("list", true)
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
            RenderMamlList(transformation, element);
        else
            RenderXmlCommentsList(transformation, element);
    }

    /// <summary>
    /// Render a list element in a MAML topic
    /// </summary>
    /// <param name="transformation">The transformation in use</param>
    /// <param name="element">The element to render</param>
    private static void RenderMamlList(TopicTransformationCore transformation, XElement element)
    {
        string start = null, listType = element.Attribute("class")?.Value.ToLowerInvariant();

        if(listType == "ordered")
            start = element.Attribute("start")?.Value;

        var list = new XElement("ul");

        if(listType != null)
            list.Add(new XAttribute("class", listType));

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
    private static void RenderXmlCommentsList(TopicTransformationCore transformation, XElement element)
    {
        XElement container, bold;
        string start, listType = element.Attribute("type")?.Value.ToLowerInvariant();

        switch(listType)
        {
            case "definition":
                foreach(var item in element.Elements("item"))
                {
                    XElement term = item.Element("term"), description = item.Element("description");

                    if(term != null && description != null)
                    {
                        container = new XElement(WordProcessingML + "p",
                            new XElement(WordProcessingML + "pPr",
                                new XElement(WordProcessingML + "spacing",
                                    new XAttribute(WordProcessingML + "after", "0"))));
                        bold = new XElement("span", new XAttribute("class", "Bold"));
                        container.Add(bold);

                        transformation.CurrentElement.Add(container);
                        transformation.RenderChildElements(bold, term.Nodes());

                        transformation.RenderChildElements(transformation.CurrentElement, description.Nodes());
                    }
                }
                break;

            case "table":
                container = new XElement(WordProcessingML + "tbl");
                var tableProps = new XElement(WordProcessingML + "tblPr",
                        new XElement(WordProcessingML + "tblStyle",
                            new XAttribute(WordProcessingML + "val", "GeneralTable")),
                        new XElement(WordProcessingML + "tblW",
                            new XAttribute(WordProcessingML + "w", "5000"),
                            new XAttribute(WordProcessingML + "type", "pct")));

                container.Add(tableProps);
                transformation.CurrentElement.Add(container);
                transformation.CurrentElement.Add(new XElement(WordProcessingML + "p",
                    new XElement(WordProcessingML + "pPr",
                        new XElement(WordProcessingML + "spacing",
                            new XAttribute(WordProcessingML + "after", "0")))));

                XElement listHeader = element.Element("listheader"), tr, tc;

                if(listHeader != null)
                {
                    tr = new XElement(WordProcessingML + "tr");
                    container.Add(tr);

                    foreach(var cell in listHeader.Elements())
                    {
                        tc = new XElement(WordProcessingML + "tc");
                        tr.Add(tc);
                        transformation.RenderChildElements(tc, cell.Nodes());
                    }
                }
                else
                {
                    // Turn off first row formatting if there is no list header
                    tableProps.Add(new XElement(WordProcessingML + "tblLook",
                            new XAttribute(WordProcessingML + "firstRow", "0"),
                            new XAttribute(WordProcessingML + "noHBand", "1"),
                            new XAttribute(WordProcessingML + "noVBand", "1")));
                }

                foreach(var item in element.Elements("item"))
                {
                    tr = new XElement(WordProcessingML + "tr");
                    container.Add(tr);

                    foreach(var cell in item.Elements())
                    {
                        tc = new XElement(WordProcessingML + "tc");
                        tr.Add(tc);
                        transformation.RenderChildElements(tc, cell.Nodes());
                    }
                }
                break;

            default:    // "bullet", "nobullet", "number", not specified, or unrecognized
                start = element.Attribute("start")?.Value;

                var list = new XElement("ul", new XAttribute("class", listType));

                if(!String.IsNullOrWhiteSpace(start))
                    list.Add(new XAttribute("start", start));

                transformation.CurrentElement.Add(list);

                foreach(var item in element.Elements("item"))
                {
                    XElement li = new("li"), term = item.Element("term"),
                        description = item.Element("description");
                    list.Add(li);

                    if(term != null || description != null)
                    {
                        container = new XElement(WordProcessingML + "p");
                        li.Add(container);

                        // If there's a term but no description, render the term as the description
                        if(term != null && description == null)
                        {
                            description = term;
                            term = null;
                        }

                        if(term != null)
                        {
                            bold = new XElement("span", new XAttribute("class", "Bold"));
                            container.Add(bold, " \u2013 ");

                            transformation.RenderChildElements(bold, term.Nodes());
                        }

                        if(description != null)
                            transformation.RenderChildElements(container, description.Nodes());
                    }
                    else
                        transformation.RenderChildElements(li, item.Nodes());
                }
                break;
        }
    }
    #endregion
}
