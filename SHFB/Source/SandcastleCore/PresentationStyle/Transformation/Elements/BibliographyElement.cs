//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : BibliographyElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle bibliography elements based on the topic type
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
using System.Collections.Generic;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle general <c>bibliography</c> elements based on the topic type
    /// </summary>
    public class BibliographyElement : Element
    {
        #region Private data members
        //=====================================================================

        private string lastTopicKey;
        private List<string> citations;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the bibliography author style for HTML output formats
        /// </summary>
        /// <value>The default if not set explicitly is "BibliographyAuthor"</value>
        public string BibliographyAuthorStyle { get; set; } = "bibliographyAuthor";

        /// <summary>
        /// This is used to get or set the bibliography title style for HTML output formats
        /// </summary>
        /// <value>The default if not set explicitly is "BibliographyTitle"</value>
        public string BibliographyTitleStyle { get; set; } = "bibliographyTitle";

        /// <summary>
        /// This is used to get or set the bibliography publisher style for HTML output formats
        /// </summary>
        /// <value>The default if not set explicitly is "BibliographyPublisher"</value>
        public string BibliographyPublisherStyle { get; set; } = "bibliographyPublisher";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public BibliographyElement() : base("bibliography")
        {
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Determine unique citations in the current topic if not already done
        /// </summary>
        /// <param name="transformation">The topic transformation from which to get the citations</param>
        /// <returns>The unique citations in the current topic in the order that they were encountered</returns>
        public IReadOnlyCollection<string> DetermineCitations(TopicTransformationCore transformation)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(lastTopicKey != transformation.Key)
            {
                lastTopicKey = transformation.Key;

                if(citations == null)
                    citations = [];
                else
                    citations.Clear();

                var keysSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                XName citeName;

                if(transformation.IsMamlTopic)
                    citeName = Ddue + "cite";
                else
                    citeName = "cite";

                foreach(var cite in transformation.DocumentNode.Descendants(citeName))
                {
                    string key = cite.Value?.NormalizeWhiteSpace();

                    if(!String.IsNullOrWhiteSpace(key) && !keysSeen.Contains(key) &&
                      transformation.BibliographyData.ContainsKey(key))
                    {
                        citations.Add(key);
                        keysSeen.Add(key);
                    }
                }
            }

            return citations;
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            this.DetermineCitations(transformation);

            if(citations.Count != 0)
            {
                var (title, content) = transformation.CreateSection(element.GenerateUniqueId(), true,
                    "bibliographyTitle", null);

                if(title != null)
                    transformation.CurrentElement.Add(title);

                if(content != null)
                    transformation.CurrentElement.Add(content);
                else
                    content = transformation.CurrentElement;

                int idx = 1;

                foreach(var citation in citations)
                {
                    var reference = transformation.BibliographyData[citation];

                    switch(transformation.SupportedFormats)
                    {
                        case HelpFileFormats.OpenXml:
                            RenderOpenXmlCitation(reference, idx, content);
                            break;

                        case HelpFileFormats.Markdown:
                            RenderMarkdownCitation(reference, idx, content);
                            break;

                        default:
                            this.RenderHtmlCitation(reference, idx, content);
                            break;
                    }

                    idx++;
                }
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Render a citation for HTML output formats
        /// </summary>
        /// <param name="reference">The citation reference to render</param>
        /// <param name="citationNumber">The citation number</param>
        /// <param name="content">The content element to which the citation is added</param>
        private void RenderHtmlCitation(XElement reference, int citationNumber, XElement content)
        {
            var div = new XElement("div",
                new XElement("span",
                    new XAttribute("id", $"cite{citationNumber}"),
                    $"[{citationNumber}] "),
                new XElement("span",
                    new XAttribute("class", this.BibliographyAuthorStyle),
                    reference.Element("author")?.Value.NormalizeWhiteSpace()),
                ", ",
                new XElement("span",
                    new XAttribute("class", this.BibliographyTitleStyle),
                    reference.Element("title")?.Value.NormalizeWhiteSpace()));

            string publisher = reference.Element("publisher")?.Value.NormalizeWhiteSpace(),
                link = reference.Element("link")?.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(publisher))
            {
                div.Add(", ", new XElement("span",
                    new XAttribute("class", this.BibliographyPublisherStyle), publisher));
            }

            if(!String.IsNullOrWhiteSpace(link))
            {
                div.Add(", ",
                    new XElement("a",
                        new XAttribute("href", link),
                        new XAttribute("target", "_blank"),
                        new XAttribute("rel", "noopener noreferrer"),
                        link));
            }

            content.Add(div);
        }

        /// <summary>
        /// Render a citation for markdown output formats
        /// </summary>
        /// <param name="reference">The citation reference to render</param>
        /// <param name="citationNumber">The citation number</param>
        /// <param name="content">The content element to which the citation is added</param>
        private static void RenderMarkdownCitation(XElement reference, int citationNumber, XElement content)
        {
            content.Add(new XElement("span",
                    new XAttribute("id", $"cite{citationNumber}"), $"\\[{citationNumber}\\] "),
                $"**{reference.Element("author")?.Value.NormalizeWhiteSpace()}**, ",
                $"*{reference.Element("title")?.Value.NormalizeWhiteSpace()}*");

            string publisher = reference.Element("publisher")?.Value.NormalizeWhiteSpace(),
                link = reference.Element("link")?.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(publisher))
                content.Add(", ", publisher);

            if(!String.IsNullOrWhiteSpace(link))
            {
                content.Add(", ",
                    new XElement("a",
                        new XAttribute("href", link),
                        new XAttribute("target", "_blank"),
                        new XAttribute("rel", "noopener noreferrer"),
                        link));
            }

            content.Add("  \n");
        }

        /// <summary>
        /// Render a citation for Open XML output formats
        /// </summary>
        /// <param name="reference">The citation reference to render</param>
        /// <param name="citationNumber">The citation number</param>
        /// <param name="content">The content element to which the citation is added</param>
        private static void RenderOpenXmlCitation(XElement reference, int citationNumber, XElement content)
        {
            OpenXmlElement.AddAddressBookmark(content, $"cite{citationNumber}");

            var para = new XElement(OpenXmlElement.WordProcessingML + "p",
                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0"))));

            para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        new XAttribute(XmlSpace, "preserve"),
                        $"[{citationNumber}] ")),
                new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                        new XElement(OpenXmlElement.WordProcessingML + "b")),
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        reference.Element("author")?.Value.NormalizeWhiteSpace())),
                new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        new XAttribute(XmlSpace, "preserve"), ", ")),
                new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                        new XElement(OpenXmlElement.WordProcessingML + "i")),
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        reference.Element("title")?.Value.NormalizeWhiteSpace())));

            string publisher = reference.Element("publisher")?.Value.NormalizeWhiteSpace(),
                link = reference.Element("link")?.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(publisher))
            {
                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t", $", {publisher}")));
            }

            if(!String.IsNullOrWhiteSpace(link))
            {
                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        new XAttribute(XmlSpace, "preserve"), ", ")));
                para.Add(new XElement("a", new XAttribute("href", link), link));
            }

            content.Add(para);
        }
        #endregion
    }
}
