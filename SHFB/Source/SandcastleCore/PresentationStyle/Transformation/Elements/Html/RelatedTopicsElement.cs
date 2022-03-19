﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : RelatedTopicsElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/19/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle relatedTopics elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/27/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>relatedTopics</c> elements in a topic
    /// </summary>
    public class RelatedTopicsElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public RelatedTopicsElement() : base("relatedTopics")
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

            if(element.Elements().Any() || element.Value.NormalizeWhiteSpace().Length != 0)
            {
                // This has a fixed ID used by the auto-outline element to link to the section
                var (title, content) = transformation.CreateSection("seeAlso", true, "title_relatedTopics", null);

                transformation.CurrentElement.Add(title);
                transformation.CurrentElement.Add(content);

                var linkGroups = new Dictionary<TopicTypeGroup, List<XElement>>
                {
                    { TopicTypeGroup.Tasks, new List<XElement>() },
                    { TopicTypeGroup.Reference, new List<XElement>() },
                    { TopicTypeGroup.Concepts, new List<XElement>() },
                    { TopicTypeGroup.OtherResources, new List<XElement>() },
                };

                // Group the links by See Also group type
                foreach(var link in element.Elements())
                {
                    var topicType = TopicType.FromUniqueId(link.Attribute("topicType_id")?.Value);
                    TopicTypeGroup groupType = topicType?.SeeAlsoGroup ?? TopicTypeGroup.OtherResources;

                    if(link.Name.LocalName == "codeEntityReference")
                        groupType = TopicTypeGroup.Reference;

                    linkGroups[groupType].Add(link);
                }

                RenderLinkSubsection(transformation, content, "title_seeAlso_tasks",
                    linkGroups[TopicTypeGroup.Tasks]);
                RenderLinkSubsection(transformation, content, "title_seeAlso_reference",
                    linkGroups[TopicTypeGroup.Reference]);
                RenderLinkSubsection(transformation, content, "title_seeAlso_concepts",
                    linkGroups[TopicTypeGroup.Concepts]);
                RenderLinkSubsection(transformation, content, "title_seeAlso_otherResources",
                    linkGroups[TopicTypeGroup.OtherResources]);
            }
        }

        /// <summary>
        /// Render the links in the given topic type group to their own subsection
        /// </summary>
        /// <param name="transformation">The topic transformation in use</param>
        /// <param name="content">The parent content element to which the links are rendered</param>
        /// <param name="titleIncludeItem">The include item name for the title</param>
        /// <param name="links">The links to render</param>
        private static void RenderLinkSubsection(TopicTransformationCore transformation, XElement content,
          string titleIncludeItem, IEnumerable<XElement> links)
        {
            if(links.Any())
            {
                var (titleElement, contentElement) = transformation.CreateSubsection(true, titleIncludeItem);

                if(titleElement != null)
                    content.Add(titleElement);

                if(contentElement != null)
                {
                    content.Add(contentElement);
                    content = contentElement;
                }

                foreach(var l in links)
                {
                    var div = new XElement("div");

                    content.Add(div);
                    transformation.RenderChildElements(div, new[] { l });
                }
            }
        }
        #endregion
    }
}
