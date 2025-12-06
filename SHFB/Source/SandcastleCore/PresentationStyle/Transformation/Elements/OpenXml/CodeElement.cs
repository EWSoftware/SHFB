//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle code elements in Open XML presentation styles
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/08/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This is used to handle <c>code</c> and <c>snippet</c> elements in a topic for Open XML presentation
/// styles.
/// </summary>
public class CodeElement : OpenXmlElement
{
    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    public CodeElement(string name) : base(name, true)
    {
        this.DoNotParse = true;
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

        if(!element.Attributes().Any())
        {
            var el = new XElement("span", new XAttribute("class", "CodeInline"));

            transformation.CurrentElement.Add(el);
            transformation.RenderChildElements(el, element.Nodes());
            return;
        }

        string language = element.Attribute("language")?.Value, title = element.Attribute("title")?.Value;

        XElement row, table = new(WordProcessingML + "tbl"),
            tableProperties = new(WordProcessingML + "tblPr",
                new XElement(WordProcessingML + "tblStyle",
                    new XAttribute(WordProcessingML + "val", "CodeTable")),
                new XElement(WordProcessingML + "tblW",
                    new XAttribute(WordProcessingML + "w", "5000"),
                    new XAttribute(WordProcessingML + "type", "pct")));

        table.Add(tableProperties);

        if(!String.IsNullOrWhiteSpace(title) || (title == null && !String.IsNullOrWhiteSpace(language) &&
          !language.Equals("other", StringComparison.OrdinalIgnoreCase) &&
          !language.Equals("none", StringComparison.OrdinalIgnoreCase)))
        {
            XNode content;

            if(title != null)
                content = new XText(title);
            else
            {
                content = new XElement("include",
                    new XAttribute("item", $"devlang_{language}"),
                    new XAttribute("undefined", language));
            }

            row = new XElement(WordProcessingML + "tr",
                new XElement(WordProcessingML + "trPr",
                    new XElement(WordProcessingML + "cnfStyle",
                        new XAttribute(WordProcessingML + "val", "100000000000"))),
                new XElement(WordProcessingML + "tc",
                    new XElement(WordProcessingML + "p",
                        new XElement(WordProcessingML + "pPr",
                            new XElement(WordProcessingML + "keepNext")),
                        new XElement(WordProcessingML + "r",
                            new XElement(WordProcessingML + "t", content)))));

            table.Add(row);
        }
        else
        {
            // Turn off first row formatting
            tableProperties.Add(new XElement(WordProcessingML + "tblLook",
                    new XAttribute(WordProcessingML + "firstRow", "0"),
                    new XAttribute(WordProcessingML + "noHBand", "1"),
                    new XAttribute(WordProcessingML + "noVBand", "1")));
        }

        var para = new XElement(WordProcessingML + "p");

        table.Add(new XElement(WordProcessingML + "tr", new XElement(WordProcessingML + "tc", para)));

        transformation.RenderChildElements(para, element.Nodes());

        transformation.CurrentElement.Add(table);
        transformation.CurrentElement.Add(new XElement(WordProcessingML + "p",
            new XElement(WordProcessingML + "pPr",
                new XElement(WordProcessingML + "spacing",
                    new XAttribute(WordProcessingML + "after", "0")))));
    }
    #endregion
}
