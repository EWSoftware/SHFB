//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeInlineElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class that handles codeInline elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/24/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

/// <summary>
/// This handles <c>codeInline</c> elements
/// </summary>
public class CodeInlineElement : Element
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property returns the element name that will be rendered in the topic
    /// </summary>
    public string RenderedName { get; }

    /// <summary>
    /// This read-only property returns the style name if specified
    /// </summary>
    /// <value>If null, no style attribute will be rendered</value>
    public string StyleName { get; }

    #endregion

    #region Constructors
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderedElement">The element name to use in the rendered topic</param>
    /// <param name="styleName">The style name to use in the style attribute</param>
    public CodeInlineElement(string renderedElement, string styleName) : base("codeInline")
    {
        this.RenderedName = renderedElement;
        this.StyleName = styleName;
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

        string keyword = element.Value.NormalizeWhiteSpace();

        if(!String.IsNullOrWhiteSpace(keyword))
        {
            if(keyword[0] == ':')
            {
                RenderLST(transformation, element);
                return;
            }

            if(transformation.SupportedFormats == HelpFileFormats.Markdown)
            {
                // Markdown typically doesn't get rendered if nested within another HTML element.  In some cases
                // it does but only if the opening element is not immediately followed by a new line.  If a
                // parent element other than the document is found, use the fallback HTML element instead.
                if(transformation.CurrentElement.Parent != null)
                {
                    var el = new XElement("code");

                    transformation.CurrentElement.Add(el);
                    transformation.RenderChildElements(el, element.Nodes());
                }
                else
                {
                    transformation.CurrentElement.Add("`");

                    foreach(var child in element.Nodes())
                        transformation.RenderNode(child);

                    transformation.CurrentElement.Add("`");
                }
            }
            else
            {
                var el = new XElement(this.RenderedName);

                if(!String.IsNullOrWhiteSpace(this.StyleName))
                    el.Add(new XAttribute("class", this.StyleName));

                transformation.CurrentElement.Add(el);

                transformation.RenderChildElements(el, element.Nodes());
            }
        }
    }

    /// <summary>
    /// If possible, render language-specific text for the keyword.  If not, fallback and render it as a normal
    /// keyword.
    /// </summary>
    /// <param name="transformation">The topic transformation to use</param>
    /// <param name="element">The element to convert</param>
    /// <returns>True if language-specific text was rendered, false if not</returns>
    private static void RenderLST(TopicTransformationCore transformation, XElement element)
    {
        string keyword = element.Value?.NormalizeWhiteSpace().Substring(1);

        // If there is a slash, separate the keywords and render each one individually
        bool first = true, isMarkdown = transformation.SupportedFormats == HelpFileFormats.Markdown,
            isHtml = (transformation.SupportedFormats != HelpFileFormats.OpenXml && !isMarkdown);

        foreach(string k in keyword.Split(['/'], StringSplitOptions.RemoveEmptyEntries))
        {
            string kw = k.Trim();

            if(!String.IsNullOrWhiteSpace(kw))
            {
                if(!first)
                    transformation.CurrentElement.Add('/');

                if(isHtml)
                {
                    var lst = transformation.LanguageSpecificTextFor(kw);

                    if(lst != null)
                        transformation.CurrentElement.Add(lst.Render());
                    else
                        transformation.CurrentElement.Add(Html.LanguageSpecificText.RenderKeyword(keyword));
                }
                else
                {
                    string includeItem = null;

                    switch(kw)
                    {
                        case "null":
                        case "Nothing":
                        case "nullptr":
                            includeItem = "devlang_nullKeyword";
                            break;

                        case "static":
                        case "Shared":
                            includeItem = "devlang_staticKeyword";
                            break;

                        case "virtual":
                        case "Overridable":
                            includeItem = "devlang_virtualKeyword";
                            break;

                        case "true":
                        case "True":
                            includeItem = "devlang_trueKeyword";
                            break;

                        case "false":
                        case "False":
                            includeItem = "devlang_falseKeyword";
                            break;

                        case "abstract":
                        case "MustInherit":
                            includeItem = "devlang_abstractKeyword";
                            break;

                        case "sealed":
                        case "NotInheritable":
                            includeItem = "devlang_sealedKeyword";
                            break;

                        case "async":
                        case "Async":
                            includeItem = "devlang_asyncKeyword";
                            break;

                        case "await":
                        case "Await":
                            includeItem = "devlang_awaitKeyword";
                            break;

                        case "this":
                        case "Me":
                            includeItem = "devlang_thisKeyword";
                            break;

                        default:
                            break;
                    }

                    if(includeItem != null)
                    {
                        transformation.CurrentElement.Add(new XElement("include",
                            new XAttribute("item", includeItem)));
                    }
                    else
                    {
                        if(isMarkdown)
                            transformation.CurrentElement.Add($"`{kw}`");
                        else
                        {
                            transformation.CurrentElement.Add(
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rPr",
                                        new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rStyle",
                                            new XAttribute(OpenXml.OpenXmlElement.WordProcessingML + "val", "CodeInline"))),
                                    new XElement(OpenXml.OpenXmlElement.WordProcessingML + "t",
                                        new XAttribute(XmlSpace, "preserve"), kw)));
                        }
                    }
                }

                first = false;
            }
        }
    }
    #endregion
}
