//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : GlossaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/22/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle glossary elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/18/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This is used to handle <c>glossary</c> elements in a topic
/// </summary>
public class GlossaryElement : OpenXmlElement
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
        {
            transformation.CurrentElement.Add(new XElement(WordProcessingML + "p",
                new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "pStyle",
                        new XAttribute(WordProcessingML + "val", "Heading1"))),
                new XElement(WordProcessingML + "r",
                    new XElement(WordProcessingML + "t", title))));
        }

        if(element.Element(Ddue + "glossaryDiv") != null)
        {
            // An organized glossary with glossaryDiv elements.  Render links to each titled division.
            bool addSeparator = false;
            var para = new XElement(WordProcessingML + "p");
            
            transformation.CurrentElement.Add(para);

            foreach(var div in element.Elements(Ddue + "glossaryDiv"))
            {
                title = div.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

                if(!String.IsNullOrWhiteSpace(title))
                {
                    if(addSeparator)
                    {
                        para.Add(new XElement(WordProcessingML + "r",
                            new XElement(WordProcessingML + "t",
                                new XAttribute(XmlSpace, "preserve"), " | ")));
                    }

                    string address = div.Attribute("address")?.Value;

                    if(!String.IsNullOrWhiteSpace(address))
                    {
                        para.Add(new XElement(WordProcessingML + "hyperlink",
                                new XAttribute(WordProcessingML + "history", "1"),
                                new XAttribute(WordProcessingML + "anchor", "_" + address),
                            new XElement(WordProcessingML + "r",
                                new XElement(WordProcessingML + "rPr",
                                    new XElement(WordProcessingML + "rStyle",
                                        new XAttribute(WordProcessingML + "val", "Hyperlink"))),
                                new XElement(WordProcessingML + "t", title))));
                    }
                    else
                    {
                        para.Add(new XElement(WordProcessingML + "r",
                            new XElement(WordProcessingML + "t", title)));
                    }

                    addSeparator = true;
                }
            }

            foreach(var gd in element.Elements(Ddue + "glossaryDiv"))
                RenderGlossaryDivision(transformation, gd);
        }
        else
        {
            // A simple glossary consisting of nothing but glossaryEntry elements
            RenderGlossaryLetterBar(element, null, transformation.CurrentElement);
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
        string address = glossaryDiv.Attribute("address")?.Value;

        if(!String.IsNullOrWhiteSpace(address))
            AddAddressBookmark(transformation.CurrentElement, address);

        string title = glossaryDiv.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

        if(!String.IsNullOrWhiteSpace(title))
        {
            transformation.CurrentElement.Add(new XElement(WordProcessingML + "p",
                new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "pStyle",
                        new XAttribute(WordProcessingML + "val", "Heading2"))),
                new XElement(WordProcessingML + "r",
                    new XElement(WordProcessingML + "t", title))));
        }

        string divId = glossaryDiv.GenerateUniqueId();

        RenderGlossaryLetterBar(glossaryDiv, divId, transformation.CurrentElement);
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
            {
                content.Add(new XElement(WordProcessingML + "r",
                    new XElement(WordProcessingML + "t",
                        new XAttribute(XmlSpace, "preserve"), " | ")));
            }

            if(letters.Contains(letter))
            {
                content.Add(new XElement(WordProcessingML + "hyperlink",
                        new XAttribute(WordProcessingML + "history", "1"),
                        new XAttribute(WordProcessingML + "anchor", $"_{id}{letter}"),
                    new XElement(WordProcessingML + "r",
                        new XElement(WordProcessingML + "rPr",
                            new XElement(WordProcessingML + "rStyle",
                                new XAttribute(WordProcessingML + "val", "Hyperlink"))),
                        new XElement(WordProcessingML + "t", letter))));
            }
            else
            {
                content.Add(new XElement(WordProcessingML + "r",
                    new XElement(WordProcessingML + "rPr",
                        new XElement(WordProcessingML + "b")),
                    new XElement(WordProcessingML + "t", letter)));
            }
        }

        content.Add(new XElement("br"));
    }

    /// <summary>
    /// Render the glossary division entries
    /// </summary>
    /// <param name="transformation">The topic transformation in use</param>
    /// <param name="glossaryDiv">The glossary division for which to render the entries</param>
    /// <param name="id">An optional ID for the section containing the entries</param>
    /// <param name="content">The content element to which the glossary entries are rendered</param>
    private static void RenderGlossaryEntries(TopicTransformationCore transformation, XElement glossaryDiv,
        string id, XElement content)
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
            AddAddressBookmark(content, $"{id}{letter.Key}");

            content.Add(new XElement(WordProcessingML + "p",
                new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "pStyle",
                        new XAttribute(WordProcessingML + "val", "Heading3"))),
                new XElement(WordProcessingML + "r",
                    new XElement(WordProcessingML + "t", letter.Key))));

            // The group only contains the first term for each glossary entry in the letter group
            foreach(var term in letter)
            {
                // Get the parent glossary entry
                var entry = term.Parent.Parent;
                var para = new XElement(WordProcessingML + "p",
                    new XElement(WordProcessingML + "pPr",
                        new XElement(WordProcessingML + "spacing",
                            new XAttribute(WordProcessingML + "after", "0"))));

                content.Add(para);

                string address = entry.Attribute("address")?.Value;

                if(!String.IsNullOrWhiteSpace(address))
                    AddAddressBookmark(para, address);

                bool firstTerm = true;

                // Terms
                foreach(var t in term.Parent.Elements(Ddue + "term").OrderBy(t => t.Value.NormalizeWhiteSpace()))
                {
                    if(!firstTerm)
                    {
                        para.Add(new XElement(WordProcessingML + "r",
                            new XElement(WordProcessingML + "t",
                                new XAttribute(XmlSpace, "preserve"), ", ")));
                    }

                    string termText = t.Value.NormalizeWhiteSpace();
                    address = t.Attribute("termId")?.Value;

                    if(!String.IsNullOrWhiteSpace(address))
                        AddAddressBookmark(para, address);
                    
                    para.Add(new XElement("span", new XAttribute("class", "Bold"), termText));

                    firstTerm = false;
                }

                // Definition
                var definition = entry.Element(Ddue + "definition");

                if(definition != null)
                    transformation.RenderChildElements(content, definition.Nodes());

                // Related entries
                if(entry.Elements(Ddue + "relatedEntry").Any())
                {
                    bool firstRelated = true;

                    para = new XElement(WordProcessingML + "p",
                        new XElement(WordProcessingML + "pPr",
                            new XElement(WordProcessingML + "ind",
                                new XAttribute(WordProcessingML + "left", "432"))),
                        new XElement(WordProcessingML + "r",
                            new XElement(WordProcessingML + "t",
                                new XElement("include",
                                    new XAttribute("item", "text_relatedEntries")), NonBreakingSpace)));

                    content.Add(para);

                    foreach(var re in entry.Elements(Ddue + "relatedEntry"))
                    {
                        if(!firstRelated)
                        {
                            para.Add(new XElement(WordProcessingML + "r",
                                new XElement(WordProcessingML + "t",
                                    new XAttribute(XmlSpace, "preserve"), ", ")));
                        }

                        string termId = re.Attribute("termId")?.Value;

                        if(!String.IsNullOrWhiteSpace(termId))
                        {
                            var relTerm = entry.Ancestors(Ddue + "glossary").First().Descendants(Ddue + "term").Where(
                                t => t.Attribute("termId")?.Value == termId).FirstOrDefault();

                            if(relTerm != null)
                            {
                                para.Add(new XElement(WordProcessingML + "hyperlink",
                                    new XAttribute(WordProcessingML + "history", "1"),
                                    new XAttribute(WordProcessingML + "anchor",
                                        "_" + relTerm.Attribute("termId").Value),
                                    new XElement(WordProcessingML + "r",
                                        new XElement(WordProcessingML + "rPr",
                                            new XElement(WordProcessingML + "rStyle",
                                                new XAttribute(WordProcessingML + "val", "Hyperlink"))),
                                        new XElement(WordProcessingML + "t",
                                            relTerm.Value.NormalizeWhiteSpace()))));
                            }

                            firstRelated = false;
                        }
                    }
                }
            }
        }
    }
    #endregion
}
