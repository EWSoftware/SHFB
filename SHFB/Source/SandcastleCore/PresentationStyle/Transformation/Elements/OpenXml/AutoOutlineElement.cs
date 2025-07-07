//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AutoOutlineElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle autoOutline elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/12/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml
{
    /// <summary>
    /// This is used to handle <c>autoOutline</c> elements in a topic
    /// </summary>
    /// <remarks><para>This element inserts a bullet list of links to the topic's sections or a section's
    /// sub-sections with optional support for limiting the expansion down to a specific level.  Authors can use
    /// the tag directly or specify a token (defined in a token file) in a topic's introduction to get a bullet
    /// list of the sections; or in a <c>ddue:section</c>/<c>ddue:content</c> to get a bullet list of the
    /// section's sub-sections.  If the token is used, the shared content component replaces
    /// <c>&lt;token&gt;autoOutline&lt;/token&gt;</c> with an <c>&lt;autoOutline/&gt;</c> node that you specify
    /// in the token's content.  This was the old way of doing it but this version allows it to be specified
    /// directly like any other MAML tag.</para>
    /// 
    /// <para>The <c>excludeRelatedTopics</c> attribute can be set to true to exclude the link to the related
    /// topics section if present or set to false to omit it.  If not specified, the default is true to include
    /// it.</para>
    /// 
    /// <para>The <c>lead</c> attribute can be included to specify the lead in text.  It can be set to "none"
    /// to omit the lead in text.  If omitted, default lead in text will be used.</para>
    /// 
    /// <para>Examples:</para></remarks>
    /// <example>
    /// <code language="xml">
    /// &lt;!-- Show only top-level topic titles, default lead-in text, includes related
    /// topics link --&gt;
    /// &lt;autoOutline/&gt;
    /// 
    /// &lt;!-- Show only top-level topic titles without the related topics link, default
    /// lead-in text --&gt;
    /// &lt;autoOutline excludeRelatedTopics="true"/&gt;
    /// 
    /// &lt;!-- Add custom lead-in text --&gt;
    /// &lt;autoOutline excludeRelatedTopics="true" lead="Changes in this release" /&gt;
    /// 
    /// &lt;!-- Show top-level titles and titles for one level down, default lead-in text,
    /// includes related topics link --&gt;
    /// &lt;autoOutline&gt;1&lt;/autoOutline&gt;
    ///
    /// &lt;-- Show top-level titles and titles for one level down, omit the lead-in text
    /// &lt;autoOutline lead="none"&gt;1&lt;/autoOutline&gt; --&gt;
    ///
    /// &lt;-- Show titles from the top down to three levels, default lead-in text, includes
    /// related topics link --&gt;
    /// &lt;autoOutline&gt;3&lt;/autoOutline&gt;
    /// </code>
    /// </example>
    public class AutoOutlineElement : OpenXmlElement
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public AutoOutlineElement() : base("autoOutline")
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

            string leadIn = element.Attribute("lead")?.Value.NormalizeWhiteSpace();
            AutoOutlineType outlineType = element.Attribute("excludeRelatedTopics").ToBoolean() ?
                AutoOutlineType.TopNoRelated : AutoOutlineType.TopLevel;

            if(!Int32.TryParse(element.Value, out int maxDepth))
                maxDepth = 0;

            var intro = element.Ancestors(Ddue + "introduction").FirstOrDefault();

            if(intro != null)
            {
                // If in an introduction, it outlines the top level sections
                InsertAutoOutline(transformation, transformation.CurrentElement, intro.Parent, leadIn,
                    outlineType, 0, maxDepth);
            }
            else
            {
                // If in a section/content element, it outlines the subsections of the parent section element
                // if it has any.
                var content = element.Ancestors(Ddue + "content").Where(
                    c => c.Parent.Name == Ddue + "section").FirstOrDefault();

                if(content != null)
                {
                    var sections = content.Parent.Element(Ddue + "sections");

                    if(sections != null)
                    {
                        InsertAutoOutline(transformation, transformation.CurrentElement, sections, leadIn,
                            AutoOutlineType.Subsection, 0, maxDepth);
                    }
                }
            }
        }

        /// <summary>
        /// Insert an auto-outline
        /// </summary>
        /// <param name="transformation">The topic transformation in use</param>
        /// <param name="renderTo">The parent element to which the outline will be rendered</param>
        /// <param name="content">The content from which to get the auto-outline sections</param>
        /// <param name="leadIn">The lead in text</param>
        /// <param name="outlineType">The outline type</param>
        /// <param name="currentDepth">The current outline depth</param>
        /// <param name="maxDepth">The maximum outline depth</param>
        private static void InsertAutoOutline(TopicTransformationCore transformation, XElement renderTo,
          XElement content, string leadIn, AutoOutlineType outlineType, int currentDepth, int maxDepth)
        {
            // Ignore any without a title section
            if(!content.Elements(Ddue + "section").Any(
              s => (s.Element(Ddue + "title")?.Value ?? String.Empty).NormalizeWhiteSpace().Length != 0))
            {
                return;
            }

            if(leadIn == null || !leadIn.Equals("none", StringComparison.Ordinal))
            {
                if(!String.IsNullOrWhiteSpace(leadIn))
                {
                    renderTo.Add(new XElement(WordProcessingML + "p",
                        new XElement(WordProcessingML + "r",
                            new XElement(WordProcessingML + "t", leadIn))));
                }
                else
                {
                    if(outlineType < AutoOutlineType.Subsection)
                    {
                        renderTo.Add(new XElement(WordProcessingML + "p",
                            new XElement(WordProcessingML + "r",
                                new XElement(WordProcessingML + "t",
                                    new XElement("include",
                                        new XAttribute("item", "boilerplate_autoOutlineTopLevelIntro"))))));
                    }
                    else
                    {
                        if(outlineType == AutoOutlineType.Subsection)
                        {
                            renderTo.Add(new XElement(WordProcessingML + "p",
                                new XElement(WordProcessingML + "r",
                                    new XElement(WordProcessingML + "t",
                                        new XElement("include",
                                            new XAttribute("item", "boilerplate_autoOutlineSubsectionIntro"))))));
                        }
                    }
                }
            }

            var ul = new XElement("ul");

            renderTo.Add(ul);

            foreach(var s in content.Elements(Ddue + "section"))
            {
                string title = s.Element(Ddue + "title").Value.NormalizeWhiteSpace();

                // Ignore sections without a title
                if(title.Length == 0)
                    continue;

                var li = new XElement("li");
                string address = s.Attribute("address")?.Value;
                var summary = s.Element(Ddue + "summary");

                if(!String.IsNullOrWhiteSpace(address))
                    li.Add(new XElement("a", new XAttribute("href", "#" + address), title));
                else
                    li.Add(title);

                if(summary != null && summary.Value.NormalizeWhiteSpace().Length != 0)
                {
                    var para = new XElement(WordProcessingML + "p");

                    li.Add(para);

                    transformation.RenderChildElements(para, summary.Nodes());
                }

                ul.Add(li);

                // Expand subsections if wanted
                var subsections = s.Element(Ddue + "sections");

                if(subsections != null && currentDepth < maxDepth)
                {
                    InsertAutoOutline(transformation, li, subsections, null, AutoOutlineType.SubSubsection,
                        currentDepth + 1, maxDepth);
                }
            }

            // Add See Also section link for top level outlines but only if it has related topics
            if(outlineType == AutoOutlineType.TopLevel)
            {
                var relatedTopics = transformation.DocumentNode.Descendants(Ddue + "relatedTopics").FirstOrDefault();

                if(relatedTopics != null && relatedTopics.HasElements)
                {
                    ul.Add(new XElement("li",
                        new XElement("a",
                                new XAttribute("href", "#seeAlso"),
                            new XElement("include",
                                new XAttribute("item", "title_relatedTopics")))));
                }
            }
        }
        #endregion
    }
}
