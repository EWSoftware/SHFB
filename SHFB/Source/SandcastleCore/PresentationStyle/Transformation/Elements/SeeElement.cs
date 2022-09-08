//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SeeElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/08/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle see/seealso elements
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
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles <c>see</c> and <c>seealso</c> elements
    /// </summary>
    public class SeeElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public SeeElement() : base("see")
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

            string cref = element.Attribute("cref")?.Value,
                href = element.Attribute("href")?.Value,
                langWord = element.Attribute("langword")?.Value;

            if(!String.IsNullOrWhiteSpace(cref))
                RenderApiTopicLink(transformation, element, cref);
            else
            {
                if(!String.IsNullOrWhiteSpace(href))
                    RenderExternalLink(transformation, element, href);
                else
                {
                    if(!String.IsNullOrWhiteSpace(langWord) && element.Name.LocalName == "see")
                        RenderLanguageKeyword(transformation, langWord);
                }
            }
        }

        /// <summary>
        /// Render a link to another API topic
        /// </summary>
        /// <param name="transformation">The topic transformation in use</param>
        /// <param name="element">The element to handle</param>
        /// <param name="apiMemberId">The member ID to use for the link</param>
        private static void RenderApiTopicLink(TopicTransformationCore transformation, XElement element, string apiMemberId)
        {
            bool qualifyHint = element.Attribute("qualifyHint").ToBoolean(),
                autoUpgrade = element.Attribute("autoUpgrade").ToBoolean();

            if(apiMemberId.StartsWith("O:", StringComparison.Ordinal))
                apiMemberId = "Overload:" + apiMemberId.Substring(2);

            var link = new XElement("referenceLink", new XAttribute("target", apiMemberId));

            if(qualifyHint)
                link.Add(new XAttribute("show-container", "true"), new XAttribute("show-parameters", "true"));

            if(autoUpgrade)
                link.Add(new XAttribute("prefer-overload", "true"));

            if(!element.Nodes().Any())
            {
                if(apiMemberId.StartsWith("R:", StringComparison.Ordinal))
                    link.Add(new XElement("include", new XAttribute("item", "topicTitle_Root")));
                else
                {
                    if(apiMemberId.StartsWith("Overload:", StringComparison.Ordinal))
                    {
                        link.Add(new XElement("include", 
                            new XAttribute("item", "boilerplate_seeAlsoOverloadLink"),
                            new XElement("parameter", "{0}")));
                    }
                }
            }

            transformation.CurrentElement.Add(link);
            transformation.RenderChildElements(link, element.Nodes());
        }

        /// <summary>
        /// Render a link to an external URL
        /// </summary>
        /// <param name="transformation">The topic transformation in use</param>
        /// <param name="element">The element to handle</param>
        /// <param name="url">The URL for the external link</param>
        private static void RenderExternalLink(TopicTransformationCore transformation, XElement element, string url)
        {
            string target = element.Attribute("target")?.Value,
                altText = element.Attribute("alt")?.Value;

            if(String.IsNullOrWhiteSpace(target))
                target = "_blank";

            var link = new XElement("a",
                new XAttribute("href", url),
                new XAttribute("target", target),
                new XAttribute("rel", "noopener noreferrer"));

            if(!String.IsNullOrWhiteSpace(altText))
                link.Add(new XAttribute("title", altText));

            transformation.CurrentElement.Add(link);

            if(element.Value.NormalizeWhiteSpace().Length != 0)
                transformation.RenderChildElements(link, element.Nodes());
            else
                link.Add(url);
        }

        /// <summary>
        /// Render a language keyword
        /// </summary>
        /// <param name="transformation">The topic transformation in use</param>
        /// <param name="keyword">The language keyword to render</param>
        private static void RenderLanguageKeyword(TopicTransformationCore transformation, string keyword)
        {
            // If there is a slash, separate the keywords and render each one individually
            bool first = true, isHtml = (transformation.SupportedFormats != HelpFileFormats.OpenXml);

            foreach(string k in keyword.NormalizeWhiteSpace().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
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
                            transformation.CurrentElement.Add(
                                new XElement(OpenXml.OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rPr",
                                        new XElement(OpenXml.OpenXmlElement.WordProcessingML + "rStyle",
                                            new XAttribute(OpenXml.OpenXmlElement.WordProcessingML + "val", "CodeInline"))),
                                    new XElement(OpenXml.OpenXmlElement.WordProcessingML + "t",
                                        new XAttribute(XmlSpace, "preserve"), kw)));
                        }
                    }

                    first = false;
                }
            }
        }
        #endregion
    }
}
