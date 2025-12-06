//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TableElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/01/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the table element based on the topic type
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

// Ignore Spelling: hoverable

using System;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Conversion;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;

/// <summary>
/// This handles the <c>table</c> element based on the topic type
/// </summary>
public class TableElement : Element
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to get or set the table caption style
    /// </summary>
    /// <value>The default if not set explicitly is <c>caption</c></value>
    public string TableCaptionStyle { get; set; } = "caption";

    /// <summary>
    /// This is used to get or set the overall table style
    /// </summary>
    /// <value>The default if not set explicitly is <c>table is-hoverable</c></value>
    public string TableStyle { get; set; } = "table is-hoverable";

    #endregion

    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public TableElement() : base("table", true)
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

        if(transformation is MarkdownConversionTransformation)
        {
            RenderForConversion(transformation, element);
            return;
        }

        var table = new XElement(this.Name);

        if(!String.IsNullOrWhiteSpace(this.TableStyle) && transformation.SupportedFormats != HelpFileFormats.Markdown)
            table.Add(new XAttribute("class", this.TableStyle));

        if(transformation.IsMamlTopic)
        {
            string title = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

            if((title?.Length ?? 0) != 0)
            {
                if(transformation.SupportedFormats != HelpFileFormats.Markdown)
                {
                    transformation.CurrentElement.Add(new XElement("div",
                        new XAttribute("class", this.TableCaptionStyle), title));
                }
                else
                {
                    transformation.CurrentElement.Add(new XElement("p",
                        new XElement("strong", title)));
                }
            }
        }
        else
        {
            foreach(var attr in element.Attributes())
            {
                // Don't override explicit styling
                if(attr.Name.LocalName != "class" || String.IsNullOrWhiteSpace(this.TableStyle))
                    table.Add(new XAttribute(attr.Name.LocalName, attr.Value));
            }
        }

        transformation.CurrentElement.Add(table);
        transformation.RenderChildElements(table, element.Nodes());
    }

    /// <summary>
    /// Render the table for conversion to a Markdown topic
    /// </summary>
    /// <param name="transformation">The transformation</param>
    /// <param name="element">The table element</param>
    private static void RenderForConversion(TopicTransformationCore transformation, XElement element)
    {
        transformation.CurrentElement.Add("\n");

        string title = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

        if((title?.Length ?? 0) != 0)
            transformation.CurrentElement.Add($"\n*{title}*\n\n");

        // Tables are complicated because the cells may contain other block elements such as nested tables,
        // lists, alerts, etc. which we can't render easily if at all with Markdown and must use HTML.  If
        // it looks like all cells only contains text or a single paragraph with text and inline elements
        // and no line breaks we'll do our best to convert it to Markdown.  If not, use an HTML table.
        // Unlike MAML tables, Markdown tables require a header row.  For those missing a header, an HTML table
        // will be used as well.
        if(element.Element(Ddue + "tableHeader") != null && element.Descendants(Ddue + "entry").All(
          e => e.Nodes().All(n => n is XText t && t.Value.IndexOf('\n') == -1) ||
          (e.Nodes().Count() == 1 && e.Nodes().First() is XElement p && p.Name.LocalName == "para" &&
            p.DescendantNodes().OfType<XText>().All(dt => dt.Value.IndexOf('\n') == -1))))
        {
            RenderAsPipeTable(transformation, element);
        }
        else
            RenderAsHtmlTable(transformation, element);

        transformation.CurrentElement.Add("\n");
    }

    /// <summary>
    /// Convert a MAML table to a Markdown pipe table
    /// </summary>
    /// <param name="transformation">The transformation</param>
    /// <param name="element">The table element</param>
    private static void RenderAsPipeTable(TopicTransformationCore transformation, XElement element)
    {
        var header = element.Element(Ddue + "tableHeader")?.Element(Ddue + "row");
        var rows = element.Elements(Ddue + "row").ToList();
        XElement firstRow;

        if(header != null || rows.Count != 0)
        {
            if(header != null)
            {
                RenderPipeTableRow(transformation, header);
                firstRow = header;
            }
            else
            {
                // Markdown table require a header row even if it's blank
                firstRow = rows[0];

                transformation.CurrentElement.Add("|");

                for(int i = 0; i < firstRow.Elements(Ddue + "entry").Count(); i++)
                    transformation.CurrentElement.Add("  |");

                transformation.CurrentElement.Add("\n");
            }

            transformation.CurrentElement.Add("|");

            for(int i = 0; i < firstRow.Elements(Ddue + "entry").Count(); i++)
                transformation.CurrentElement.Add(" --- |");

            transformation.CurrentElement.Add("\n");

            foreach(var r in rows)
                RenderPipeTableRow(transformation, r);
        }
    }

    /// <summary>
    /// Render a pipe table row
    /// </summary>
    /// <param name="transformation">The transformation</param>
    /// <param name="row">The table row to render</param>
    private static void RenderPipeTableRow(TopicTransformationCore transformation, XElement row)
    {
        transformation.CurrentElement.Add("|");

        foreach(var entry in row.Elements(Ddue + "entry"))
        {
            transformation.CurrentElement.Add(" ");

            var para = entry.Element(Ddue + "para");

            if(para != null)
                transformation.RenderChildElements(transformation.CurrentElement, para.Nodes());
            else
                transformation.RenderChildElements(transformation.CurrentElement, entry.Nodes());

            transformation.CurrentElement.Add(" |");
        }

        transformation.CurrentElement.Add("\n");
    }

    /// <summary>
    /// Convert a MAML table to an HTML table
    /// </summary>
    /// <param name="transformation">The transformation</param>
    /// <param name="element">The table element</param>
    private static void RenderAsHtmlTable(TopicTransformationCore transformation, XElement element)
    {
        var rows = element.Elements(Ddue + "row").ToList();

        if(rows.Count != 0)
        {
            var table = new XElement("table");
            
            transformation.CurrentElement.Add(table);

            var header = element.Element(Ddue + "tableHeader")?.Element(Ddue + "row");

            if(header != null)
            {
                var thead = new XElement("thead");

                table.Add("\n  ");
                table.Add(thead);

                RenderHtmlTableRow(transformation, thead, header, "th");

                thead.Add("\n  ");
            }

            var tbody = new XElement("tbody");

            table.Add("\n  ");
            table.Add(tbody);

            foreach(var r in rows)
                RenderHtmlTableRow(transformation, tbody, r, "td");

            tbody.Add("\n  ");
            table.Add("\n");
        }
    }

    /// <summary>
    /// Render an HTML table row
    /// </summary>
    /// <param name="transformation">The transformation</param>
    /// <param name="rowContainer">The container to which the row is added</param>
    /// <param name="mamlRow">The MAML table row to render</param>
    /// <param name="cellTag">The HTML cell tag to use</param>
    private static void RenderHtmlTableRow(TopicTransformationCore transformation, XElement rowContainer,
      XElement mamlRow, string cellTag)
    {
        var row = new XElement("tr");

        rowContainer.Add("\n    ");
        rowContainer.Add(row);

        foreach(var entry in mamlRow.Elements(Ddue + "entry"))
        {
            var cell = new XElement(cellTag);

            row.Add("\n      ");
            row.Add(cell);

            var para = entry.Element(Ddue + "para");

            if(para != null && entry.Nodes().Count() == 1)
                transformation.RenderChildElements(cell, para.Nodes());
            else
                transformation.RenderChildElements(cell, entry.Nodes());
        }

        row.Add("\n    ");
    }
    #endregion
}
