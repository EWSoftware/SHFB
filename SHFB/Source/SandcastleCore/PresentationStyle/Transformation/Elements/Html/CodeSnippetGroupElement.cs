﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeSnippetGroupElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/12/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle codeSnippetGroup elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/29/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle general <c>codeSnippetGroup</c> elements in a topic
    /// </summary>
    public class CodeSnippetGroupElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public CodeSnippetGroupElement() : base("codeSnippetGroup")
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

            var snippets = element.Elements().Where(e => e.Name.LocalName == "code" ||
                e.Name.LocalName == "snippet");

            if(snippets.Any())
            {
                string id = element.GenerateUniqueId();
                int nodeCount = snippets.Count(), position = 1;

                var codeSnippetContainer = new XElement("div",
                    transformation.StyleAttributeFor(CommonStyle.CodeSnippetContainer));
                transformation.CurrentElement.Add(codeSnippetContainer);

                // Only render the title tabs if there is more than one snippet or the title is not a single
                // space indicating that it should be omitted.
                if(nodeCount > 1 || snippets.First().Attribute("title")?.Value != " ")
                {
                    var tabDiv = new XElement("div",
                        transformation.StyleAttributeFor(CommonStyle.CodeSnippetContainerTabs));

                    codeSnippetContainer.Add(tabDiv);

                    foreach(var snippet in snippets)
                    {
                        // Single snippet, phantom (placeholder for a non-represented language), or multi-snippet
                        CommonStyle style = (nodeCount == 1) ? CommonStyle.CodeSnippetContainerTabSingle :
                            snippet.Attribute("phantom") != null ? CommonStyle.CodeSnippetContainerTabPhantom :
                            CommonStyle.CodeSnippetContainerTab;
                        string title = snippet.Attribute("title")?.Value.NormalizeWhiteSpace(),
                            codeLanguage = snippet.Attribute("codeLanguage")?.Value;

                        var tab = new XElement("div",
                            new XAttribute("id", $"{id}_tab{position}"),
                            transformation.StyleAttributeFor(style));
                        tabDiv.Add(tab);

                        if(nodeCount == 1)
                        {
                            if(!String.IsNullOrWhiteSpace(title))
                                tab.Add(title);
                            else
                            {
                                tab.Add(new XElement("include",
                                    new XAttribute("item", $"devlang_{codeLanguage}"),
                                    new XAttribute("undefined", codeLanguage)));
                            }
                        }
                        else
                        {
                            // Use onclick rather than href or Help Viewer 2 messes up the link
                            tab.Add(new XElement("a",
                                new XAttribute("href", "#"),
                                new XAttribute("onclick", $"javascript:ChangeTab('{id}','{snippet.Attribute("style")?.Value}','{position}','{nodeCount}');return false;"),
                                    new XElement("include",
                                        new XAttribute("item", $"devlang_{codeLanguage}"),
                                        new XAttribute("undefined", codeLanguage))));
                        }

                        position++;
                    }
                }

                var codeDiv = new XElement("div",
                    transformation.StyleAttributeFor(CommonStyle.CodeSnippetContainerCodeContainer));

                codeSnippetContainer.Add(codeDiv);

                // Code snippet toolbar (Copy Code link)
                codeDiv.Add(new XElement("div",
                    transformation.StyleAttributeFor(CommonStyle.CodeSnippetToolBar),
                    new XElement("div",
                        transformation.StyleAttributeFor(CommonStyle.CodeSnippetToolBarText),
                        new XElement("a",
                            new XAttribute("id", $"{id}_copyCode"),
                            new XAttribute("href", "#"),
                            transformation.StyleAttributeFor(CommonStyle.CopyCodeSnippet),
                            new XAttribute("onclick", $"javascript:CopyToClipboard('{id}');return false;"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "title"),
                                    new XAttribute("item", "copyCode")),
                                new XElement("include",
                                    new XAttribute("item", "copyCode"))))));

                position = 1;

                foreach(var snippet in snippets)
                {
                    var snippetDiv = new XElement("div",
                        new XAttribute("id", $"{id}_code_Div{position}"),
                        transformation.StyleAttributeFor(CommonStyle.CodeSnippetContainerCode));
                    codeDiv.Add(snippetDiv);

                    if(position == 1)
                        snippetDiv.Add(new XAttribute("style", "display: block"));
                    else
                        snippetDiv.Add(new XAttribute("style", "display: none"));

                    if(snippet.Attribute("phantom") != null)
                        snippetDiv.Add(new XElement("include", new XAttribute("item", "noCodeExample")));
                    else
                        snippetDiv.Add(new XElement("pre",
                            new XAttribute(XmlSpace, "preserve"), snippet.Nodes()));

                    position++;
                }

                // Register the tab set even for single tabs as we may need to hide the Copy link
                codeSnippetContainer.Add(new XElement("script",
                    new XAttribute("type", "text/javascript"),
                    $"AddLanguageTabSet(\"{id}\")"));
            }
        }
        #endregion
    }
}