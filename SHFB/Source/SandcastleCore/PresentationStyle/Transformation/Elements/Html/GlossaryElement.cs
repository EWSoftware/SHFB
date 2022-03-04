//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : GlossaryElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/01/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
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
// 01/28/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>glossary</c> elements in a topic
    /// </summary>
    public class GlossaryElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public GlossaryElement() : base("glossary")
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
                transformation.CurrentElement.Add(new XElement("h2", title));

            if(element.Element(Ddue + "glossaryDiv") != null)
            {
                // An organized glossary with glossaryDiv elements
                transformation.CurrentElement.Add(new XElement("br"));

                // Render links to each titled division
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
                        {
                            transformation.CurrentElement.Add(new XElement("a",
                                new XAttribute("href", "#" + address), title));
                        }
                        else
                            transformation.CurrentElement.Add(title);

                        addSeparator = true;
                    }
                }

                foreach(var gd in element.Elements(Ddue + "glossaryDiv"))
                    RenderGlossaryDivision(transformation, gd);
            }
            else
            {
                // A simple glossary consisting of nothing bu glossaryEntry elements
                transformation.CurrentElement.Add(new XElement("br"));

                RenderGlossaryLetterBar(transformation, element, null, transformation.CurrentElement);

                transformation.CurrentElement.Add(new XElement("br"));

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
            var div = new XElement("div", transformation.StyleAttributeFor(CommonStyle.GlossaryDiv));

            transformation.CurrentElement.Add(div);

            string address = glossaryDiv.Attribute("address")?.Value;

            if(!String.IsNullOrWhiteSpace(address))
                div.Add(new XAttribute("id", address));

            string title = glossaryDiv.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

            if(!String.IsNullOrWhiteSpace(title))
                div.Add(new XElement("h3", title));

            div.Add(new XElement("hr", transformation.StyleAttributeFor(CommonStyle.GlossaryRule)));

            RenderGlossaryLetterBar(transformation, glossaryDiv, glossaryDiv.GenerateUniqueId(), div);

            div.Add(new XElement("br"));

            RenderGlossaryEntries(transformation, glossaryDiv, null, div);
        }

        /// <summary>
        /// Render a glossary letter bar
        /// </summary>
        /// <param name="transformation">The transformation to use</param>
        /// <param name="glossaryDiv">The glossary division for which to render the letter bar</param>
        /// <param name="id">An optional ID for the section containing the letter bar</param>
        /// <param name="content">The content element to which the letter bar is rendered</param>
        private static void RenderGlossaryLetterBar(TopicTransformationCore transformation, XElement glossaryDiv,
          string id, XElement content)
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
                    content.Add(new XElement("a", new XAttribute("href", $"#{id}{letter}"), letter));
                else
                    content.Add(new XElement("span", transformation.StyleAttributeFor(CommonStyle.NoLink), letter));
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
                var group = new XElement("div",
                        transformation.StyleAttributeFor(CommonStyle.GlossaryGroup),
                    new XElement("h3",
                        transformation.StyleAttributeFor(CommonStyle.GlossaryGroupHeading),
                        new XAttribute("id", $"{id}{letter.Key}"), letter.Key));
                    
                content.Add(group);

                var dl = new XElement("dl", transformation.StyleAttributeFor(CommonStyle.GlossaryGroupList));
                group.Add(dl);

                // The group only contains the first term for each glossary entry in the letter group
                foreach(var term in letter)
                {
                    // Get the parent glossary entry
                    var entry = term.Parent.Parent;
                    var dt = new XElement("dt", transformation.StyleAttributeFor(CommonStyle.GlossaryEntry));
                    dl.Add(dt);

                    string address = entry.Attribute("address")?.Value;

                    if(!String.IsNullOrWhiteSpace(address))
                        dt.Add(new XAttribute("id", address));

                    bool firstTerm = true;

                    // Terms
                    foreach(var t in term.Parent.Elements(Ddue + "term").OrderBy(t => t.Value.NormalizeWhiteSpace()))
                    {
                        if(!firstTerm)
                            dt.Add(", ");

                        string termText = t.Value.NormalizeWhiteSpace();
                        address = t.Attribute("termId")?.Value;

                        if(!String.IsNullOrWhiteSpace(address))
                            dt.Add(new XElement("span", new XAttribute("id", address), termText));
                        else
                            dt.Add(termText);

                        firstTerm = false;
                    }

                    // Definition
                    var dd = new XElement("dd", transformation.StyleAttributeFor(CommonStyle.GlossaryEntry));
                    dl.Add(dd);

                    var definition = entry.Element(Ddue + "definition");

                    if(definition != null)
                        transformation.RenderChildElements(dd, definition.Nodes());

                    // Related entries
                    if(entry.Elements(Ddue + "relatedEntry").Any())
                    {
                        bool firstRelated = true;

                        var div = new XElement("div",
                                transformation.StyleAttributeFor(CommonStyle.RelatedEntry),
                            new XElement("include",
                                new XAttribute("item", "text_relatedEntries")), NonBreakingSpace);
                        dd.Add(div);

                        foreach(var re in entry.Elements(Ddue + "relatedEntry"))
                        {
                            if(!firstRelated)
                                div.Add(", ");

                            string termId = re.Attribute("termId")?.Value;

                            if(!String.IsNullOrWhiteSpace(termId))
                            {
                                var relTerm = entry.Ancestors(Ddue + "glossary").First().Descendants(Ddue + "term").Where(
                                    t => t.Attribute("termId")?.Value == termId).FirstOrDefault();

                                if(relTerm != null)
                                {
                                    div.Add(new XElement("a",
                                        new XAttribute("href", "#" + relTerm.Attribute("termId").Value),
                                        relTerm.Value.NormalizeWhiteSpace()));
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
}
