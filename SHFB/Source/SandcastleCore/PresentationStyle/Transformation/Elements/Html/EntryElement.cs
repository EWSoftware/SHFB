//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : EntryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/23/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the entry element based on the parent element
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// Handle the <c>entry</c> element based on the parent element
    /// </summary>
    public class EntryElement : Element
    {
        /// <inheritdoc />
        public EntryElement() : base("entry")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            XElement cell;

            if(element.Parent.Parent.Name.LocalName == "tableHeader")
                cell = new XElement("th");
            else
            {
                cell = new XElement("td");

                string address = element.Attribute("address")?.Value;

                if(!String.IsNullOrWhiteSpace(address))
                    cell.Add(new XAttribute("id", address));
            }

            transformation.CurrentElement.Add(cell);
            transformation.RenderChildElements(cell, element.Nodes());

            if(transformation.SupportedFormats == HelpFileFormats.Markdown)
                transformation.CurrentElement.Add("\n");
        }
    }
}
