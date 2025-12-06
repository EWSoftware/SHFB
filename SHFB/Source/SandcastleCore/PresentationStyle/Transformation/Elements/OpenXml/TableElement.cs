//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TableElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/22/2025
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
// 05/09/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This handles the <c>table</c> element based on the topic type
/// </summary>
public class TableElement : OpenXmlElement
{
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

        if(transformation.IsMamlTopic)
        {
            string title = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

            if((title?.Length ?? 0) != 0)
            {
                transformation.CurrentElement.Add(new XElement(WordProcessingML + "p",
                    new XElement(WordProcessingML + "pPr",
                        new XElement(WordProcessingML + "pStyle",
                            new XAttribute(WordProcessingML + "val", "Heading3"))),
                    new XElement(WordProcessingML + "r",
                        new XElement(WordProcessingML + "t", title))));
            }
        }

        XElement table = new(WordProcessingML + "tbl"),
            tableProperties = new(WordProcessingML + "tblPr",
                new XElement(WordProcessingML + "tblStyle",
                    new XAttribute(WordProcessingML + "val", "GeneralTable")),
                new XElement(WordProcessingML + "tblW",
                    new XAttribute(WordProcessingML + "w", "5000"),
                    new XAttribute(WordProcessingML + "type", "pct")));

        table.Add(tableProperties);

        if(!element.Descendants(Ddue + "tableHeader").Any() && !element.Descendants(Ddue + "th").Any())
        {
            // Turn off first row formatting if there is no table header
            tableProperties.Add(new XElement(WordProcessingML + "tblLook",
                    new XAttribute(WordProcessingML + "firstRow", "0"),
                    new XAttribute(WordProcessingML + "noHBand", "1"),
                    new XAttribute(WordProcessingML + "noVBand", "1")));
        }

        transformation.CurrentElement.Add(table);

        transformation.RenderChildElements(table, element.Nodes());

        transformation.CurrentElement.Add(new XElement(WordProcessingML + "p",
            new XElement(WordProcessingML + "pPr",
                new XElement(WordProcessingML + "spacing",
                    new XAttribute(WordProcessingML + "after", "0")))));
    }
    #endregion
}
