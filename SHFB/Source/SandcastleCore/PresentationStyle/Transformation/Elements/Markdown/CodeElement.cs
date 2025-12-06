//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle code elements in Markdown presentation styles
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/28/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Conversion;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown;

/// <summary>
/// This is used to handle <c>code</c> and <c>snippet</c> elements in a topic for markdown presentation
/// styles.
/// </summary>
public class CodeElement : Element
{
    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    public CodeElement(string name) : base(name, true)
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

        string language = element.Attribute("language")?.Value, title = element.Attribute("title")?.Value;

        // Render based on the attribute count if converting
        if(transformation is MarkdownConversionTransformation)
        {
            int attributeCount = element.Attributes().Count();

            if(attributeCount == 0)
            {
                // Inline
                transformation.CurrentElement.Add("`");
                transformation.RenderChildElements(transformation.CurrentElement, element.Nodes());
                transformation.CurrentElement.Add("`");
            }
            else
            {
                // Fenced.  If nested though, pass it through as an element.
                if(element.Parent.Name.LocalName == this.Name)
                {
                    var el = new XElement(this.Name);

                    foreach(var attr in element.Attributes())
                        el.Add(new XAttribute(attr.Name.LocalName, attr.Value));

                    transformation.CurrentElement.Add("\n");
                    transformation.CurrentElement.Add(el);
                    transformation.CurrentElement.Add("\n");
                    transformation.RenderChildElements(el, element.Nodes());
                }
                else
                {

                    var code = transformation.CurrentElement;

                    code.Add("\n```");

                    if(!String.IsNullOrWhiteSpace(language))
                    {
                        code.Add($" {language}");
                        attributeCount--;
                    }

                    if(attributeCount != 0)
                    {
                        code.Add("{");

                        bool isFirst = true;

                        foreach(var attr in element.Attributes().Where(
                          a => !a.Name.LocalName.Equals("language", StringComparison.OrdinalIgnoreCase)))
                        {
                            if(!isFirst)
                                code.Add(" ");
                            else
                                isFirst = false;

                            code.Add($"{attr.Name.LocalName}=\"{attr.Value}\"");
                        }

                        code.Add("}");
                    }

                    var codeNode = element.FirstNode;

                    if(codeNode is XText t1 && !t1.Value.StartsWith("\n", StringComparison.Ordinal))
                        code.Add("\n");

                    transformation.RenderChildElements(code, element.Nodes());

                    codeNode = element.LastNode;

                    if(codeNode == null || (codeNode is XText t2 && !t2.Value.EndsWith("\n", StringComparison.Ordinal)))
                        code.Add("\n");

                    code.Add("```\n");
                }
            }
        }
        else
        {
            transformation.CurrentElement.Add("\n\n");

            if(!String.IsNullOrWhiteSpace(title) || (title == null && !String.IsNullOrWhiteSpace(language) &&
              !language.Equals("other", StringComparison.OrdinalIgnoreCase) &&
              !language.Equals("none", StringComparison.OrdinalIgnoreCase)))
            {
                XNode content;

                if(title != null)
                    content = new XText(title);
                else
                {
                    content = new XElement("include",
                        new XAttribute("item", $"devlang_{language}"),
                        new XAttribute("undefined", language));
                }

                transformation.CurrentElement.Add("**", content, "**  \n");
            }

            transformation.CurrentElement.Add("```");

            if(!String.IsNullOrWhiteSpace(language) &&
                !language.Equals("other", StringComparison.OrdinalIgnoreCase) &&
                !language.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                transformation.CurrentElement.Add(" ", new XElement("include",
                    new XAttribute("item", $"devlang_{language}"),
                    new XAttribute("undefined", language)), "\n");
            }

            transformation.RenderChildElements(transformation.CurrentElement, element.Nodes());
            transformation.CurrentElement.Add("\n```\n");
        }
    }
    #endregion
}
