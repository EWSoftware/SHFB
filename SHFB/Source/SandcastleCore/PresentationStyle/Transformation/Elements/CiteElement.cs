//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CiteElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle cite elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/23/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle general <c>cite</c> elements in a topic
    /// </summary>
    public class CiteElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the citation style for HTML output formats
        /// </summary>
        /// <value>The default if not set explicitly is "citation"</value>
        public string CitationStyle { get; set; } = "citation";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public CiteElement() : base("cite")
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

            string cite = element.Value?.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(cite) &&
              transformation.ElementHandlerFor("bibliography") is BibliographyElement handler)
            {
                // Determine the citation index
                var citations = handler.DetermineCitations(transformation);
                int idx = 1, foundIdx = -1;

                foreach(string c in citations)
                {
                    if(c.Equals(cite, StringComparison.OrdinalIgnoreCase))
                    {
                        foundIdx = idx;
                        break;
                    }

                    idx++;
                }

                if(foundIdx != -1)
                {
                    XElement c;

                    if(transformation.SupportedFormats != HelpFileFormats.OpenXml)
                    {
                        c = new XElement("sup",
                            new XElement("a",
                                new XAttribute("href", $"#cite{foundIdx}"),
                                $"[{foundIdx}]"));

                        if(transformation.SupportedFormats != HelpFileFormats.Markdown)
                            c.Add(new XAttribute("class", this.CitationStyle));
                    }
                    else
                    {
                        c = new XElement("a",
                                new XAttribute("href", $"#cite{foundIdx}"),
                            new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "vertAlign",
                                        new XAttribute(OpenXmlElement.WordProcessingML + "val", "superscript"))),
                                new XElement(OpenXmlElement.WordProcessingML + "t", $"[{foundIdx}]")));
                    }

                    transformation.CurrentElement.Add(c);
                }
            }
        }
        #endregion
    }
}
