//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : BibliographyElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/19/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
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
        /// This is used to get or set the bibliography author style
        /// </summary>
        /// <value>The default if not set explicitly is "BibliographyAuthor"</value>
        public string BibliographyAuthorStyle { get; set; } = "bibliographyAuthor";

        /// <summary>
        /// This is used to get or set the bibliography title style
        /// </summary>
        /// <value>The default if not set explicitly is "BibliographyTitle"</value>
        public string BibliographyTitleStyle { get; set; } = "bibliographyTitle";

        /// <summary>
        /// This is used to get or set the bibliography publisher style
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
                    citations = new List<string>();
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

                transformation.CurrentElement.Add(title);
                transformation.CurrentElement.Add(content);

                int idx = 1;

                foreach(var citation in citations)
                {
                    var reference = transformation.BibliographyData[citation];

                    var div = new XElement("div",
                        new XElement("span",
                            new XAttribute("id", $"cite{idx}"),
                            $"[{idx}] "),
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

                    idx++;

                    content.Add(div);
                }
            }
        }
        #endregion
    }
}
