//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PreliminaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/21/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the preliminary element
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

using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles the <c>preliminary</c> element
    /// </summary>
    public class PreliminaryElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the container element that will hold the preliminary text for HTML output
        /// </summary>
        /// <value>The default if not set explicitly is <c>span</c></value>
        public string PreliminaryContainerElement { get; set; } = "span";

        /// <summary>
        /// This is used to get or set the preliminary text style for HTML output
        /// </summary>
        /// <value>The default if not set explicitly is <c>tag is-warning is-medium</c></value>
        public string PreliminaryTextStyle { get; set; } = "tag is-warning is-medium";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public PreliminaryElement() : base("preliminary")
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

            string text = element.Value.NormalizeWhiteSpace();

            XElement container;

            if(transformation.SupportedFormats != HelpFileFormats.OpenXml)
            {
                container = new XElement(this.PreliminaryContainerElement,
                    new XAttribute("class", this.PreliminaryTextStyle));

                if(!String.IsNullOrWhiteSpace(text))
                    container.Add(text);
                else
                    container.Add(new XElement("include", new XAttribute("item", "preliminaryApi")));
            }
            else
            {
                container = new XElement(OpenXmlElement.WordProcessingML + "p");

                if(!String.IsNullOrWhiteSpace(text))
                {
                    container.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "rPr",
                            new XElement(OpenXmlElement.WordProcessingML + "i")),
                        new XElement(OpenXmlElement.WordProcessingML + "t", text)));
                }
                else
                {
                    container.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "rPr",
                            new XElement(OpenXmlElement.WordProcessingML + "i")),
                        new XElement(OpenXmlElement.WordProcessingML + "t",
                            new XElement("include", new XAttribute("item", "preliminaryApi")))));
                }
            }

            transformation.CurrentElement.Add(container);
        }
        #endregion
    }
}
