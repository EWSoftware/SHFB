//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LanguageKeywordElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle languageKeyword elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/25/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Conversion;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

/// <summary>
/// This handles <c>languageKeyword</c> elements
/// </summary>
public class LanguageKeywordElement : Element
{
    /// <inheritdoc />
    public LanguageKeywordElement() : base("languageKeyword")
    {
    }

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        string keyword = element.Value?.NormalizeWhiteSpace();

        // If there is a slash, separate the keywords and render each one individually
        bool first = true, isMarkdown = transformation.SupportedFormats == HelpFileFormats.Markdown,
            isHtml = (transformation.SupportedFormats != HelpFileFormats.OpenXml && !isMarkdown);

        // If converting, use the language keyword format
        if(transformation is MarkdownConversionTransformation)
        {
            foreach(string k in keyword.Split(['/'], StringSplitOptions.RemoveEmptyEntries))
            {
                string kw = k.Trim();
                char? languageSpecificText;

                if(!String.IsNullOrWhiteSpace(kw))
                {
                    if(!first)
                        transformation.CurrentElement.Add('/');
                    else
                        first = false;

                    // If it's an LST keyword, prefix it so that the codeInline handler tries to use LST for it
                    switch(kw)
                    {
                        case "null":
                        case "Nothing":
                        case "nullptr":
                        case "static":
                        case "Shared":
                        case "virtual":
                        case "Overridable":
                        case "true":
                        case "True":
                        case "false":
                        case "False":
                        case "abstract":
                        case "MustInherit":
                        case "sealed":
                        case "NotInheritable":
                        case "async":
                        case "Async":
                        case "await":
                        case "Await":
                        case "let!":
                        case "this":
                        case "Me":
                            languageSpecificText = ':';
                            break;

                        default:
                            languageSpecificText = null;
                            break;
                    }

                    transformation.CurrentElement.Add($"`{languageSpecificText}{kw}`");
                }
            }
        }
        else
        {
            foreach(string k in keyword.Split(['/'], StringSplitOptions.RemoveEmptyEntries))
            {
                string kw = k.Trim();

                if(!String.IsNullOrWhiteSpace(kw))
                {
                    if(!first)
                        transformation.CurrentElement.Add('/');
                    else
                        first = false;

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
                            case "let!":
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
                }
            }
        }
    }
}
