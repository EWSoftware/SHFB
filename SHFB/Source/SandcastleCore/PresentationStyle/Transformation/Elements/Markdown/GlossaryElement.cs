//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : GlossaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle glossary elements for markdown presentation styles
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/24/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown;

/// <summary>
/// This is used to handle <c>glossary</c> elements in a topic for markdown presentation styles
/// </summary>
public class GlossaryElement : Element
{
    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public GlossaryElement() : base("glossary", true)
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

        string title = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

        if(!String.IsNullOrWhiteSpace(title))
            transformation.CurrentElement.Add("## ", title, "\n");

        if(element.Element(Ddue + "glossaryDiv") != null)
        {
            // An organized glossary with glossaryDiv elements.  Render links to each titled division.
            bool addSeparator = false;

            foreach(var div in element.Elements(Ddue + "glossaryDiv"))
            {
                title = div.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

                if(!String.IsNullOrWhiteSpace(title))
                {
                    if(addSeparator)
                        transformation.CurrentElement.Add(" | ");

                    string address = div.Attribute("address")?.Value;

                    if(!String.IsNullOrWhiteSpace(address))
                        transformation.CurrentElement.Add($"[{title}](#{address})");
                    else
                        transformation.CurrentElement.Add(title);

                    addSeparator = true;
                }
            }

            transformation.CurrentElement.Add("\n");

            foreach(var gd in element.Elements(Ddue + "glossaryDiv"))
                RenderGlossaryDivision(transformation, gd);
        }
        else
        {
            // A simple glossary consisting of nothing but glossaryEntry elements
            RenderGlossaryLetterBar(element, null, transformation.CurrentElement);

            transformation.CurrentElement.Add("\n");

            RenderGlossaryEntries(transformation, element, null, transformation.CurrentElement);
        }
    }

    /// <summary>
    /// Render a glossary division
    /// </summary>
    /// <param name="transformation">The topic transformation in use</param>
    /// <param name="glossaryDiv">The glossary division to render</param>
    private static void RenderGlossaryDivision(TopicTransformationCore transformation, XElement glossaryDiv)
    {
        string address = glossaryDiv.Attribute("address")?.Value,
            title = glossaryDiv.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

        if(!String.IsNullOrWhiteSpace(title))
            transformation.CurrentElement.Add($"\n\n### {title}");

        // The section will automatically get an ID so use address instead
        if(!String.IsNullOrWhiteSpace(address))
            transformation.CurrentElement.Add($"{{address=\"{address}\"}}");

        transformation.CurrentElement.Add("\n");

        string divId = glossaryDiv.GenerateUniqueId();

        RenderGlossaryLetterBar(glossaryDiv, divId, transformation.CurrentElement);

        transformation.CurrentElement.Add("\n");

        RenderGlossaryEntries(transformation, glossaryDiv, divId, transformation.CurrentElement);
    }

    /// <summary>
    /// Render a glossary letter bar
    /// </summary>
    /// <param name="glossaryDiv">The glossary division for which to render the letter bar</param>
    /// <param name="id">An optional ID for the section containing the letter bar</param>
    /// <param name="content">The content element to which the letter bar is rendered</param>
    private static void RenderGlossaryLetterBar(XElement glossaryDiv, string id, XElement content)
    {
        // Sort the terms elements by their first term element and determine what letters are present
        var terms = glossaryDiv.Descendants(Ddue + "terms").Select(t => t.Elements(Ddue + "term").Where(
            term => term.Value.NormalizeWhiteSpace().Length != 0).OrderBy(
            term => term.Value.NormalizeWhiteSpace()).First().Value.NormalizeWhiteSpace());

        var letters = new HashSet<char>(terms.Select(t => Char.ToUpperInvariant(t[0])).Distinct());

        for(char letter = 'A'; letter <= 'Z'; letter++)
        {
            if(letter != 'A')
                content.Add(" | ");

            if(letters.Contains(letter))
                content.Add($"[{letter}](#{id}{letter})");
            else
                content.Add(letter);
        }
    }

    /// <summary>
    /// Render the glossary division entries
    /// </summary>
    /// <param name="transformation">The topic transformation in use</param>
    /// <param name="glossaryDiv">The glossary division for which to render the entries</param>
    /// <param name="id">An optional ID for the section containing the entries</param>
    /// <param name="content">The content element to which the glossary entries are rendered</param>
    private static void RenderGlossaryEntries(TopicTransformationCore transformation, XElement glossaryDiv, string id,
        XElement content)
    {
        // Sort the terms elements by their first term element and group the glossary entries by letter
        var letterGroups = glossaryDiv.Descendants(Ddue + "terms").Select(t => t.Elements(Ddue + "term").Where(
            term => term.Value.NormalizeWhiteSpace().Length != 0).OrderBy(
            term => term.Value.NormalizeWhiteSpace()).First()).GroupBy(
            term => Char.ToUpperInvariant(term.Value.NormalizeWhiteSpace()[0]));

        // [Letter]
        //    [Terms, ...]
        //    [Definition]
        //    [See Also: [Related entries, ...]
        // 
        //    ... Repeat for each term in the letter group ...
        //
        // ... Repeat for each letter represented ...
        foreach(var letter in letterGroups)
        {
            // The section will automatically get an ID so use address instead
            content.Add($"\n\n### {letter.Key}{{address=\"{id}{letter.Key}\"}}\n");

            var dl = transformation.CurrentElement;

            // The group only contains the first term for each glossary entry in the letter group
            foreach(var term in letter)
            {
                // Get the parent glossary entry
                var entry = term.Parent.Parent;
                dl.Add("\n");

                string termId = null;
                bool firstTerm = true;

                // Terms
                foreach(var t in term.Parent.Elements(Ddue + "term").OrderBy(t => t.Value.NormalizeWhiteSpace()))
                {
                    if(!firstTerm)
                        dl.Add(", ");
                    else
                        firstTerm = false;

                    string termText = t.Value.NormalizeWhiteSpace();
                    
                    // It's possible multiple terms can have an ID, so just use the first one found
                    termId ??= t.Attribute("termId")?.Value;

                    dl.Add(termText);
                }

                if(termId != null)
                    dl.Add($"{{id=\"{termId}\"}}");

                dl.Add("\n");

                // Definition
                var definition = entry.Element(Ddue + "definition");

                if(definition != null)
                {
                    // This is a bit tricky as we have to prefix the first line with ":   " and each subsequent line
                    // with four spaces.  So, render it to a temporary element and then copy all of the text out
                    // prefixing as we go.
                    var dd = new XElement("dd");

                    transformation.RenderChildElements(dd, definition.Nodes());

                    // First move the content of paragraphs into the parent element.
                    foreach(var para in dd.Elements().Where(e => e.Name.LocalName == "para").ToList())
                    {
                        var priorNode = para.PreviousNode;

                        foreach(var node in para.Nodes().ToList())
                        {
                            node.Remove();
                            para.AddAfterSelf(node);
                        }

                        para.Remove();
                    }

                    // Prefix each line as needed
                    StringBuilder sb = new(10240);
                    bool isFirst = true;

                    foreach(var b in dd.Nodes())
                    {
                        if(b is XText textBlock)
                        {
                            sb.Clear();
                            sb.Append(textBlock.Value);

                            int idx = 0;

                            do
                            {
                                while(idx < sb.Length && (sb[idx] == '\r' || sb[idx] == '\n'))
                                {
                                    if(isFirst)
                                        sb.Remove(idx, 1);
                                    else
                                    {
                                        if(idx > 0 && sb[idx - 1] == '\n')
                                            break;

                                        idx++;
                                    }
                                }

                                // Don't insert a prefix if there is an inline prior to the text with no line break after it
                                if(!isFirst && (textBlock.PreviousNode is not XElement || idx != 0))
                                    sb.Insert(idx, "    ");
                                else
                                {
                                    sb.Insert(idx, ":   ");
                                    isFirst = false;
                                }

                                while(idx < sb.Length && sb[idx] != '\r' && sb[idx] != '\n')
                                    idx++;

                            } while(idx < sb.Length);

                            // Remove any trailing spaces if this is the last text node in the block
                            if(textBlock.NextNode is null)
                            {
                                idx--;

                                while(idx >= 0 && sb[idx] == ' ')
                                {
                                    sb.Remove(idx, 1);
                                    idx--;
                                }
                            }

                            textBlock.Value = sb.ToString();
                        }
                    }

                    // And finally, move the nodes to the current element.
                    foreach(var n in dd.Nodes().ToList())
                    {
                        n.Remove();
                        dl.Add(n);
                    }

                    dl.Add("    \n");
                }

                // Related entries
                if(entry.Elements(Ddue + "relatedEntry").Any())
                {
                    bool firstRelated = true;

                    dl.Add(new XElement("include",
                        new XAttribute("item", "text_relatedEntries")), " ");

                    foreach(var re in entry.Elements(Ddue + "relatedEntry"))
                    {
                        if(!firstRelated)
                            dl.Add(", ");

                        termId = re.Attribute("termId")?.Value;

                        if(!String.IsNullOrWhiteSpace(termId))
                        {
                            var relTerm = entry.Ancestors(Ddue + "glossary").First().Descendants(Ddue + "term").Where(
                                t => t.Attribute("termId")?.Value == termId).FirstOrDefault();

                            if(relTerm != null)
                                dl.Add($"[{relTerm.Value.NormalizeWhiteSpace()}](#{relTerm.Attribute("termId").Value})");

                            firstRelated = false;
                        }
                    }

                    dl.Add("\n");
                }
            }
        }
    }
    #endregion
}
