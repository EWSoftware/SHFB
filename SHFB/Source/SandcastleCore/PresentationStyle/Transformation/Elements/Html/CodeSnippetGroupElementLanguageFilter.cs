//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeSnippetGroupElementLanguageFilter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle codeSnippetGroup elements in presentation styles that use a
// page-level language filter selector.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle general <c>codeSnippetGroup</c> elements in a topic for presentation styles that
    /// use a page-level language filter selector.
    /// </summary>
    public class CodeSnippetGroupElementLanguageFilter : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the code snippet template file path
        /// </summary>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string CodeSnippetTemplatePath { get; set; }

        /// <summary>
        /// This is used to get or set the code snippet template file path that contains an element block for
        /// line numbers.
        /// </summary>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string CodeSnippetNumberedTemplatePath { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public CodeSnippetGroupElementLanguageFilter() : base("codeSnippetGroup")
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
                int nodeCount = snippets.Count(), position = 1;

                // Resolve unset paths on first use
                if(String.IsNullOrWhiteSpace(this.CodeSnippetTemplatePath))
                {
                    this.CodeSnippetTemplatePath = transformation.ResolvePath(
                        Path.Combine("Templates", "CodeSnippetTemplate.html"));
                }

                if(String.IsNullOrWhiteSpace(this.CodeSnippetNumberedTemplatePath))
                {
                    this.CodeSnippetNumberedTemplatePath = transformation.ResolvePath(
                        Path.Combine("Templates", "CodeSnippetNumberedTemplate.html"));
                }

                XDocument codeOnlyTemplate = TopicTransformationCore.LoadTemplateFile(this.CodeSnippetTemplatePath, null),
                    withNumbersTemplate = null;

                if(File.Exists(this.CodeSnippetNumberedTemplatePath))
                    withNumbersTemplate = TopicTransformationCore.LoadTemplateFile(this.CodeSnippetNumberedTemplatePath, null);

                TopicTransformationCore.LoadTemplateFile(this.CodeSnippetNumberedTemplatePath, null);

                foreach(var snippet in snippets)
                {
                    string numberLines = snippet.Attribute("numberLines")?.Value;
                    XElement codeBlock;

                    if(withNumbersTemplate == null || transformation.UsesLegacyCodeColorizer ||
                      numberLines == null || !numberLines.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        codeBlock = new XElement(codeOnlyTemplate.Root);
                        numberLines = null;
                    }
                    else
                        codeBlock = new XElement(withNumbersTemplate.Root);

                    string titleText = snippet.Attribute("title")?.Value,
                        codeLanguage = snippet.Attribute("codeLanguage")?.Value,
                        style = snippet.Attribute("style")?.Value;

                    // The snippet language shouldn't be added to single snippets as we never want to hide them
                    if(nodeCount != 1)
                        codeBlock.Add(new XAttribute("data-codeSnippetLanguage", style));

                    if(position != 1)
                        codeBlock.Attribute("class").Value += " is-hidden";

                    var title = codeBlock.Descendants().FirstOrDefault(p => p.Attribute("id")?.Value == "Title") ??
                        throw new InvalidOperationException("Unable to locate the title container with the id " +
                            "'Title' in the syntax section code template");

                    title.Attribute("id").Remove();

                    // A single space is used to suppress the title
                    if(!String.IsNullOrWhiteSpace(titleText) || titleText == " ")
                    {
                        if(titleText == " ")
                            title.Add(NonBreakingSpace);
                        else
                            title.Add(titleText.NormalizeWhiteSpace());
                    }
                    else
                    {
                        title.Add(new XElement("include",
                            new XAttribute("item", $"devlang_{codeLanguage}"),
                            new XAttribute("undefined", codeLanguage)));
                    }

                    var codeContainer = codeBlock.Descendants("pre").FirstOrDefault(
                        p => p.Attribute("id")?.Value == "CodeBlock") ??
                        throw new InvalidOperationException("Unable to locate the code container with the id " +
                            "'CodeBlock' in the code snippet template");

                    codeContainer.Attribute("id").Remove();
                    codeContainer.RemoveNodes();

                    if(snippet.Attribute("phantom") != null)
                    {
                        codeContainer.Add(new XElement("code",
                            new XAttribute("class", "language-plaintext"),
                            new XElement("include", new XAttribute("item", "noCodeExample"))));
                    }
                    else
                    {
                        if(transformation.UsesLegacyCodeColorizer)
                            codeContainer.Add(snippet.Nodes());
                        else
                        {
                            string language = snippet.Attribute("language")?.Value ?? "plaintext";

                            if(transformation.CodeSnippetLanguageConversion.TryGetValue(language, out string newId))
                                language = newId;

                            if(numberLines != null)
                            {
                                var numberContainer = codeBlock.Descendants("pre").FirstOrDefault(
                                    p => p.Attribute("id")?.Value == "LineNumbers") ??
                                    throw new InvalidOperationException("Unable to locate the line number " +
                                        "container with the id 'LineNumbers' in the code snippet template");

                                numberContainer.Attribute("id").Remove();
                                numberContainer.RemoveNodes();

                                int lineCount = snippet.Value.Split(["\r\n"], StringSplitOptions.None).Length;

                                numberContainer.Value = String.Join("\r\n", Enumerable.Range(
                                    1, lineCount).Select(i => i.ToString(CultureInfo.InvariantCulture)));
                            }

                            codeContainer.Add(new XElement("code",
                                new XAttribute("class", $"language-{language}"),
                                snippet.Nodes()));

                            transformation.RegisterStartupScript(10000, "hljs.highlightAll();");
                        }
                    }

                    transformation.CurrentElement.Add(codeBlock);
                    position++;
                }
            }
        }
        #endregion
    }
}
