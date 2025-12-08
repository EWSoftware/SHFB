//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AutoOutlineElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/07/2025
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
// 01/26/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;

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
public class AutoOutlineElement : Element
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to set a style class name on top-level auto-outlines
    /// </summary>
    /// <remarks>This is useful for hiding top-level auto-outlines on all but certain devices if another
    /// element on the page displays section headers such as In This Article quick links.  Custom lead text,
    /// if specified, will still be rendered as it may not refer to the auto-outline.</remarks>
    public string TopLevelStyleName { get; set; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public AutoOutlineElement() : base("autoOutline", true)
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

        string leadIn = element.Attribute("lead")?.Value.NormalizeWhiteSpace();
        AutoOutlineType outlineType = element.Attribute("excludeRelatedTopics").ToBoolean() ?
            AutoOutlineType.TopNoRelated : AutoOutlineType.TopLevel;

        if(!Int32.TryParse(element.Value, out int maxDepth))
            maxDepth = 0;

        var intro = element.Ancestors(Ddue + "introduction").FirstOrDefault();

        // If there are title elements with a level, it's a Markdown topic so we have to build it differently
        if(transformation.TopicNode.Descendants(Ddue + "title").Any(t => t.Attribute("level") != null))
        {
            this.InsertAutoOutlineFromMarkdown(transformation, intro != null ? null : element.Parent.Parent,
                leadIn, maxDepth, outlineType == AutoOutlineType.TopNoRelated);
        }
        else
        {
            if(intro != null)
            {
                // If in an introduction, it outlines the top level sections
                this.InsertAutoOutline(transformation, transformation.CurrentElement, intro.Parent, leadIn,
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
                        this.InsertAutoOutline(transformation, transformation.CurrentElement, sections, leadIn,
                            AutoOutlineType.Subsection, 0, maxDepth);
                    }
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
    private void InsertAutoOutline(TopicTransformationCore transformation, XElement renderTo,
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
                // No class on this one as the text may not refer to the auto-outline so it should always
                // be visible.
                renderTo.Add(new XElement("p", leadIn));
            }
            else
            {
                if(outlineType < AutoOutlineType.Subsection)
                {
                    var para = new XElement("p",
                        new XElement("include",
                            new XAttribute("item", "boilerplate_autoOutlineTopLevelIntro")));

                    // Apply the top level style so that the lead-in text is hidden when the outline is hidden
                    if(!String.IsNullOrWhiteSpace(this.TopLevelStyleName))
                        para.Add(new XAttribute("class", this.TopLevelStyleName));

                    renderTo.Add(para);
                }
                else
                {
                    if(outlineType == AutoOutlineType.Subsection)
                    {
                        renderTo.Add(new XElement("p",
                            new XElement("include",
                                new XAttribute("item", "boilerplate_autoOutlineSubsectionIntro"))));
                    }
                }
            }
        }

        var ul = new XElement("ul");

        if(outlineType <= AutoOutlineType.TopNoRelated && !String.IsNullOrWhiteSpace(this.TopLevelStyleName))
            ul.Add(new XAttribute("class", this.TopLevelStyleName));

        renderTo.Add(ul);

        foreach(var s in content.Elements(Ddue + "section"))
        {
            string title = s.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

            // Ignore sections without a title
            if(String.IsNullOrWhiteSpace(title))
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
                var div = new XElement("div");

                li.Add(div);

                transformation.RenderChildElements(div, summary.Nodes());
            }

            ul.Add(li);

            // Expand subsections if wanted
            var subsections = s.Element(Ddue + "sections");

            if(subsections != null && currentDepth < maxDepth)
            {
                this.InsertAutoOutline(transformation, li, subsections, null, AutoOutlineType.SubSubsection,
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
                            new XAttribute("href", "#seeAlsoSection"),
                        new XElement("include",
                            new XAttribute("item", "title_relatedTopics")))));
            }
        }
    }

    /// <summary>
    /// This is used to add auto-outline links from a Markdown topic
    /// </summary>
    /// <param name="transformation">The transformation to use</param>
    /// <param name="start">Null for a top-level outline, or the starting section for a sub-section outline</param>
    /// <param name="leadIn">The lead in text</param>
    /// <param name="maxDepth">The maximum depth of nested sections</param>
    /// <param name="excludeRelatedTopics">True to exclude the related topics link, false to include it</param>
    /// <returns>A list element containing the outline or null if no valid sections where found</returns>
    private void InsertAutoOutlineFromMarkdown(TopicTransformationCore transformation, XElement start,
      string leadIn, int maxDepth, bool excludeRelatedTopics)
    {
        int idx, level;

        if(leadIn == null || !leadIn.Equals("none", StringComparison.Ordinal))
        {
            if(!String.IsNullOrWhiteSpace(leadIn))
            {
                // No class on this one as the text may not refer to the auto-outline so it should always
                // be visible.
                transformation.CurrentElement.Add(new XElement("p", leadIn));
            }
            else
            {
                if(start == null)
                {
                    var para = new XElement("p",
                        new XElement("include",
                            new XAttribute("item", "boilerplate_autoOutlineTopLevelIntro")));

                    // Apply the top level style so that the lead-in text is hidden when the outline is hidden
                    if(!String.IsNullOrWhiteSpace(this.TopLevelStyleName))
                        para.Add(new XAttribute("class", this.TopLevelStyleName));

                    transformation.CurrentElement.Add(para);
                }
                else
                {
                    transformation.CurrentElement.Add(new XElement("p",
                        new XElement("include",
                            new XAttribute("item", "boilerplate_autoOutlineSubsectionIntro"))));
                }
            }
        }

        // Get all title elements with level attributes from the section
        var titleElements = transformation.TopicNode.DescendantNodes().OfType<XElement>().Where(
            e => e.Name.LocalName == "title" && e.Attribute("level") != null).ToList();

        // If we have starting element, it's a sub-section outline for that element so remove it and everything
        // before it and all elements after it that aren't at a higher level.
        if(start != null)
        {
            while(titleElements.Count != 0 && titleElements[0].Parent != start)
                titleElements.RemoveAt(0);

            idx = 0;
            level = (int?)start.Attribute("level") ?? 2;

            if(titleElements.Count != 0)
                titleElements.RemoveAt(0);

            while(idx < titleElements.Count &&
              ((int?)titleElements[idx].Attribute("level") ?? 2) > level)
            {
                idx++;
            }

            while(idx < titleElements.Count)
                titleElements.RemoveAt(idx);
        }
        else
        {
            var seeAlsoSection = titleElements.FirstOrDefault(
                t => t.Parent?.Attribute("id")?.Value == "see-also");

            if(seeAlsoSection != null)
            {
                idx = titleElements.IndexOf(seeAlsoSection);
                level = (int?)seeAlsoSection.Attribute("level") ?? 2;

                // If not removing the See Also section, just exclude any child elements after it
                if(!excludeRelatedTopics)
                    idx++;
                else
                    titleElements.RemoveAt(idx);

                while(idx < titleElements.Count &&
                  ((int?)titleElements[idx].Attribute("level") ?? 2) > level)
                {
                    titleElements.RemoveAt(idx);
                }
            }
        }

        if(titleElements.Count == 0)
            return;

        // Heading levels can vary so we'll track depth independent of heading level
        int baseDepth = 0;
        XElement list = new("ul");

        if(start == null && !String.IsNullOrWhiteSpace(this.TopLevelStyleName))
            list.Add(new XAttribute("class", this.TopLevelStyleName));

        transformation.CurrentElement.Add(list);

        AddMarkdownTitlesToList(list, titleElements, 0, 0, baseDepth, maxDepth);
    }

    /// <summary>
    /// Recursively add title elements to the list based on their level
    /// </summary>
    /// <param name="currentList">The current list to add items to</param>
    /// <param name="titleElements">All title elements</param>
    /// <param name="startIndex">The starting index in the title elements list</param>
    /// <param name="currentDepth">The current depth being processed</param>
    /// <param name="currentLevel">The current heading level</param>
    /// <param name="maxDepth">The maximum depth to process</param>
    /// <returns>The index of the next unprocessed title element</returns>
    private static int AddMarkdownTitlesToList(XElement currentList, List<XElement> titleElements,
        int startIndex, int currentLevel, int currentDepth, int maxDepth)
    {
        int i = startIndex;

        while(i < titleElements.Count)
        {
            var titleElement = titleElements[i];
            int level = (int?)titleElement.Attribute("level") ?? 2;

            // If this title is at a lower level, return to parent
            if(level < currentLevel)
                return i;

            string title = titleElement.Value.NormalizeWhiteSpace();

            if(String.IsNullOrEmpty(title))
            {
                i++;
                continue;
            }

            var address = titleElement.Parent?.Attribute("id")?.Value;
            XElement listItem = new("li");
            currentList.Add(listItem);

            if(address == null)
                listItem.Add(title);
            else
                listItem.Add(new XElement("a", new XAttribute("href", "#" + address), title));

            i++;

            // Check if the next title is at a higher level (nested)
            if(i < titleElements.Count)
            {
                int nextLevel = (int?)titleElements[i].Attribute("level") ?? 2;

                if(nextLevel > level)
                {
                    if(currentDepth + 1 < maxDepth)
                    {
                        // Create nested list for higher level titles
                        XElement nestedList = new("ul");

                        i = AddMarkdownTitlesToList(nestedList, titleElements, i, nextLevel, currentDepth + 1, maxDepth);

                        listItem.Add(nestedList);
                    }
                    else
                    {
                        // Skip titles at a higher level
                        while(i < titleElements.Count)
                        {
                            nextLevel = (int?)titleElements[i].Attribute("level") ?? 2;

                            if(nextLevel <= level)
                                break;

                            i++;
                        }
                    }
                }
            }
        }

        return i;
    }
    #endregion
}
