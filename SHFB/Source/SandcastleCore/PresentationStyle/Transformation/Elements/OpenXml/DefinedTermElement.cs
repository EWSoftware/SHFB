//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : DefinedTermElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle defined term elements based on the topic type
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
    /// This handles defined term elements based on the topic type
    /// </summary>
    public class DefinedTermElement : OpenXmlElement
    {
        /// <inheritdoc />
        public DefinedTermElement(string name) : base(name)
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            XElement span = new("span", new XAttribute("class", "Bold")),
                para = new(WordProcessingML + "p",
                new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "spacing",
                        new XAttribute(WordProcessingML + "after", "0"))), span);

            string address = element.Attribute("address")?.Value;

            if(!String.IsNullOrWhiteSpace(address))
                AddAddressBookmark(transformation.CurrentElement, address);

            transformation.CurrentElement.Add(para);
            transformation.RenderChildElements(span, element.Nodes());
        }
    }
}
