//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TableRowElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/09/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle table row elements based on the topic type
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
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml
{
    /// <summary>
    /// This handles table row elements based on the topic type
    /// </summary>
    public class TableRowElement : OpenXmlElement
    {
        /// <inheritdoc />
        public TableRowElement(string name) : base(name)
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var row = new XElement(WordProcessingML + "tr");

            if(element.Element("th") != null || element.Parent.Name.LocalName == "tableHeader")
            {
                row.Add(new XElement(WordProcessingML + "trPr",
                    new XElement(WordProcessingML + "cnfStyle",
                        new XAttribute(WordProcessingML + "val", "100000000000"))));
            }

            transformation.CurrentElement.Add(row);
            transformation.RenderChildElements(row, element.Nodes());
        }
    }
}
