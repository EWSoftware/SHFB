//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SyntaxElementTabbed.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle syntax section elements in legacy presentation styles such as
// the VS2013 presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/10/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.Reflection;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>syntax</c> elements in a topic for legacy presentation styles such as the
    /// VS2013 presentation style.
    /// </summary>
    public class SyntaxElementTabbed : Element
    {
        #region Private data members
        //=====================================================================

        private static readonly HashSet<string> includedXamlSyntax = new HashSet<string>(new[] {
            "xamlAttributeUsageHeading", "xamlObjectElementUsageHeading", "xamlContentElementUsageHeading",
            "xamlPropertyElementUsageHeading" });

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the Request Example URL transformation argument name
        /// </summary>
        public string RequestExampleUrlArgName { get; }

        /// <summary>
        /// This read-only property returns the base source code URL transformation argument name
        /// </summary>
        public string BaseSourceCodeUrlArgName { get; }

        /// <summary>
        /// This is used to get or set the link button style
        /// </summary>
        /// <value>The default if not set explicitly is "button"</value>
        public string LinkButtonStyle { get; set; } = "button";

        /// <summary>
        /// This is used to get or set the code snippet container style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainer"</value>
        public string CodeSnippetContainerStyle { get; set; } = "codeSnippetContainer";

        /// <summary>
        /// This is used to get or set the code snippet container tabs style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainerTabs"</value>
        public string CodeSnippetContainerTabsStyle { get; set; } = "codeSnippetContainerTabs";

        /// <summary>
        /// This is used to get or set the code snippet container tab style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainerTab"</value>
        public string CodeSnippetContainerTabStyle { get; set; } = "codeSnippetContainerTab";

        /// <summary>
        /// This is used to get or set the code snippet container tab single style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainerTabSingle"</value>
        public string CodeSnippetContainerTabSingleStyle { get; set; } = "codeSnippetContainerTabSingle";

        /// <summary>
        /// This is used to get or set the code snippet container tab phantom style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainerTabPhantom"</value>
        public string CodeSnippetContainerTabPhantomStyle { get; set; } = "codeSnippetContainerTabPhantom";

        /// <summary>
        /// This is used to get or set the code snippet container code container style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainerCodeContainer"</value>
        public string CodeSnippetContainerCodeContainerStyle { get; set; } = "codeSnippetContainerCodeContainer";

        /// <summary>
        /// This is used to get or set the code snippet toolbar style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetToolBar"</value>
        public string CodeSnippetToolBarStyle { get; set; } = "codeSnippetToolBar";

        /// <summary>
        /// This is used to get or set the code snippet toolbar text style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetToolBarText"</value>
        public string CodeSnippetToolBarTextStyle { get; set; } = "codeSnippetToolBarText";

        /// <summary>
        /// This is used to get or set the copy code snippet style
        /// </summary>
        /// <value>The default if not set explicitly is "copyCodeSnippet"</value>
        public string CopyCodeSnippetStyle { get; set; } = "copyCodeSnippet";

        /// <summary>
        /// This is used to get or set the code snippet container code style
        /// </summary>
        /// <value>The default if not set explicitly is "codeSnippetContainerCode"</value>
        public string CodeSnippetContainerCodeStyle { get; set; } = "codeSnippetContainerCode";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestExampleUrlArgName">The Request Example URL transformation argument name to use or
        /// null if there isn't one.</param>
        /// <param name="baseSourceCodeUrlArgName">The base source code URL transformation argument name to use
        /// or null if there isn't one.</param>
        public SyntaxElementTabbed(string requestExampleUrlArgName, string baseSourceCodeUrlArgName) : base("syntax")
        {
            this.RequestExampleUrlArgName = requestExampleUrlArgName;
            this.BaseSourceCodeUrlArgName = baseSourceCodeUrlArgName;
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

            if(!element.Elements().Any())
                return;

            var (title, content) = transformation.CreateSection(element.GenerateUniqueId(), true,
                "title_syntax", null);

            transformation.CurrentElement.Add(title);
            transformation.CurrentElement.Add(content);

            this.RenderSyntaxSections(transformation, element, content);

            XElement sourceContext = transformation.ReferenceNode.Element("sourceContext"),
                parameters = transformation.ReferenceNode.Element("parameters"),
                templates = transformation.ReferenceNode.Element("templates"),
                implements = transformation.ReferenceNode.Element("implements"),
                returnValue = transformation.ReferenceNode.Element("returns"),
                eventHandlerType = transformation.ReferenceNode.Element("eventhandler")?.Element("type"),
                value = transformation.CommentsNode.Element("value"),
                returns = transformation.CommentsNode.Element("returns");

            this.RenderSourceContextElement(transformation, sourceContext, content);

            transformation.RenderChildElements(content, new[] { parameters, templates, value, returns });

            // If there were no value or returns comments and there is a return value, add a default return
            // value section.
            if(value == null && returns == null && (returnValue != null || eventHandlerType != null))
                RenderDefaultReturnSection(transformation, content);

            transformation.RenderChildElements(content, new[] { implements });

            // Add a usage note for extension methods
            if(transformation.ApiMember.ApiSubgroup == ApiMemberGroup.Method &&
              transformation.ReferenceNode.AttributeOfType("T:System.Runtime.CompilerServices.ExtensionAttribute") != null)
            {
                var (subtitle, subcontent) = transformation.CreateSubsection(true, "title_extensionUsage");

                if(subtitle != null)
                    content.Add(subtitle);

                if(subcontent != null)
                {
                    content.Add(subcontent);
                    content = subcontent;
                }

                var parameter = new XElement("parameter");
                var paramType = transformation.ReferenceNode.Element("parameters").Element("parameter").Element("type");

                content.Add(new XElement("include", new XAttribute("item", "text_extensionUsage"), parameter));

                transformation.RenderTypeReferenceLink(parameter, paramType, false);
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Render the syntax sections
        /// </summary>
        /// <param name="transformation">The transformation to use</param>
        /// <param name="element">The element containing the syntax sections</param>
        /// <param name="content">The content element to which the sections will be added</param>
        private void RenderSyntaxSections(TopicTransformationCore transformation, XElement element, XElement content)
        {
            var codeDivs = element.Elements("div").Where(d =>
            {
                var lang = d.Attribute("codeLanguage");

                if(lang == null)
                    return false;

                if(lang.Value != "XAML")
                    return true;

                // Suppress tabs containing only boilerplate XAML which isn't currently shown
                return d.Elements("div").Any(subDiv => includedXamlSyntax.Contains(subDiv.Attribute("class").Value));

            }).ToList();

            int nodeCount = codeDivs.Count, position = 1;

            if(nodeCount == 0)
                return;

            string id = codeDivs.First().GenerateUniqueId();

            var codeSnippetContainer = new XElement("div",
                new XAttribute("id", id),
                new XAttribute("class", this.CodeSnippetContainerStyle));

            content.Add(codeSnippetContainer);

            var tabDiv = new XElement("div", new XAttribute("class", this.CodeSnippetContainerTabsStyle));

            codeSnippetContainer.Add(tabDiv);

            foreach(var div in codeDivs)
            {
                // Single snippet, phantom (placeholder for a non-represented language), or multi-snippet
                string style = (nodeCount == 1) ? this.CodeSnippetContainerTabSingleStyle :
                    div.Attribute("phantom") != null ? this.CodeSnippetContainerTabPhantomStyle :
                    this.CodeSnippetContainerTabStyle;
                string codeLanguage = div.Attribute("codeLanguage")?.Value;

                var tab = new XElement("div",
                    new XAttribute("id", $"{id}_tab{position}"),
                    new XAttribute("class", style));
                tabDiv.Add(tab);

                if(nodeCount == 1)
                {
                    tab.Add(new XElement("include",
                        new XAttribute("item", $"devlang_{codeLanguage}"),
                        new XAttribute("undefined", codeLanguage)));
                }
                else
                {
                    // Use onclick rather than href or Help Viewer 2 messes up the link
                    tab.Add(new XElement("a",
                        new XAttribute("href", "#"),
                        new XAttribute("onclick", $"ChangeTab('{id}','{div.Attribute("style")?.Value}','{position}','{nodeCount}');return false;"),
                            new XElement("include",
                                new XAttribute("item", $"devlang_{codeLanguage}"),
                                new XAttribute("undefined", codeLanguage))));
                }

                position++;
            }

            var codeDiv = new XElement("div", new XAttribute("class", this.CodeSnippetContainerCodeContainerStyle));

            codeSnippetContainer.Add(codeDiv);

            // Code snippet toolbar (Copy Code link)
            codeDiv.Add(new XElement("div",
                new XAttribute("class", this.CodeSnippetToolBarStyle),
                new XElement("div",
                    new XAttribute("class", this.CodeSnippetToolBarTextStyle),
                    new XElement("a",
                        new XAttribute("id", $"{id}_copyCode"),
                        new XAttribute("href", "#"),
                        new XAttribute("class", this.CopyCodeSnippetStyle),
                        new XAttribute("onclick", $"CopyToClipboard('{id}');return false;"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "title"),
                                new XAttribute("item", "copyCode")),
                            new XElement("include",
                                new XAttribute("item", "copyCode"))))));

            position = 1;

            foreach(var div in codeDivs)
            {
                var snippetDiv = new XElement("div",
                    new XAttribute("id", $"{id}_code_Div{position}"),
                    new XAttribute("class", this.CodeSnippetContainerCodeStyle));
                codeDiv.Add(snippetDiv);

                if(position == 1)
                    snippetDiv.Add(new XAttribute("style", "display: block"));
                else
                    snippetDiv.Add(new XAttribute("style", "display: none"));

                if(div.Attribute("phantom") != null)
                    snippetDiv.Add(new XElement("p", new XElement("include", new XAttribute("item", "noCodeExample"))));
                else
                {
                    if(div.Attribute("codeLanguage").Value == "XAML")
                    {
                        RenderXamlSyntaxBlock(transformation, div, snippetDiv);
                    }
                    else
                    {
                        snippetDiv.Add(new XElement("pre",
                            new XAttribute(XmlSpace, "preserve"), div.Nodes()));
                    }
                }

                position++;
            }
        }

        /// <summary>
        /// Render XAML syntax blocks based on the API member type
        /// </summary>
        /// <param name="transformation">The transformation to use</param>
        /// <param name="xamlSnippet">The XAML snippet to render</param>
        /// <param name="content">The content element to which the syntax will be added</param>
        private static void RenderXamlSyntaxBlock(TopicTransformationCore transformation, XElement xamlSnippet,
          XElement content)
        {
            var syntaxBlocks = new List<XElement>();

            switch(transformation.ApiMember)
            {
                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Interface ||
                  t.ApiTopicSubgroup == ApiMemberGroup.Constructor ||
                  t.ApiTopicSubgroup == ApiMemberGroup.Method ||
                  t.ApiTopicSubgroup == ApiMemberGroup.Delegate ||
                  t.ApiTopicSubgroup == ApiMemberGroup.Field:
                    // Show nothing for page types that cannot be used in XAML
                    break;

                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Class || t.ApiTopicSubgroup == ApiMemberGroup.Structure:
                    // Class and structure
                    syntaxBlocks.AddRange(xamlSnippet.Elements("div").Where(
                        d => d.Attribute("class").Value == "xamlObjectElementUsageHeading"));
                    break;

                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Enumeration:
                    // Enumeration
                    content.Add(new XElement("pre",
                        new XAttribute(XmlSpace, "preserve"),
                        new XElement("include", new XAttribute("item", "enumerationOverviewXamlSyntax"))));
                    break;

                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Property || t.TopicSubSubgroup == ApiMemberGroup.AttachedProperty:
                    // Property or attached property

                    // Property element usage
                    syntaxBlocks.AddRange(xamlSnippet.Elements("div").Where(
                        d => d.Attribute("class").Value == "xamlPropertyElementUsageHeading"));
                    syntaxBlocks.AddRange(xamlSnippet.Elements("div").Where(
                        d => d.Attribute("class").Value == "xamlContentElementUsageHeading"));

                    // Attribute usage
                    syntaxBlocks.AddRange(xamlSnippet.Elements("div").Where(
                        d => d.Attribute("class").Value == "xamlAttributeUsageHeading"));
                    break;

                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Event || t.TopicSubSubgroup == ApiMemberGroup.AttachedEvent:
                    // Event or attached event
                    syntaxBlocks.AddRange(xamlSnippet.Elements("div").Where(
                        d => d.Attribute("class").Value == "xamlAttributeUsageHeading"));
                    break;

                default:
                    break;
            }

            foreach(var block in syntaxBlocks)
                content.Add(new XElement("pre", new XAttribute(XmlSpace, "preserve"), new XElement(block)));
        }

        /// <summary>
        /// Render a default return section for a member without any explicit comments based on the member's
        /// return value type.
        /// </summary>
        /// <param name="transformation">The transformation to use</param>
        /// <param name="content">The content element to which the section will be added</param>
        private static void RenderDefaultReturnSection(TopicTransformationCore transformation, XElement content)
        {
            string titleIncludeItem;

            switch(transformation.ApiMember.ApiSubgroup)
            {
                case ApiMemberGroup.Property:
                    titleIncludeItem = "title_propertyValue";
                    break;

                case ApiMemberGroup.Field:
                    titleIncludeItem = "title_fieldValue";
                    break;

                case ApiMemberGroup.Event:
                    titleIncludeItem = "title_value";
                    break;

                default:
                    titleIncludeItem = "title_methodValue";
                    break;
            }

            var (subtitle, subcontent) = transformation.CreateSubsection(true, titleIncludeItem);

            if(subtitle != null)
                content.Add(subtitle);

            if(subcontent != null)
            {
                content.Add(subcontent);
                content = subcontent;
            }

            XElement fixedBufferAttr = transformation.ReferenceNode.AttributeOfType("T:System.Runtime.CompilerServices.FixedBufferAttribute"),
                isReadOnlyAttr = transformation.ReferenceNode.AttributeOfType("T:System.Runtime.CompilerServices.IsReadOnlyAttribute"),
                isByRefLikeAttr = transformation.ReferenceNode.AttributeOfType("T:System.Runtime.CompilerServices.IsByRefLikeAttribute");
            XElement typeInfo;

            if(fixedBufferAttr != null)
                typeInfo = fixedBufferAttr.Element("argument").Element("typeValue").Element("type");
            else if(isReadOnlyAttr != null)
                typeInfo = fixedBufferAttr.Element("argument").Element("typeValue").Element("type");
            else if(isByRefLikeAttr != null)
                typeInfo = isByRefLikeAttr.Element("argument").Element("typeValue").Element("type");
            else if(transformation.ApiMember.ApiSubgroup == ApiMemberGroup.Event)
                typeInfo = transformation.ReferenceNode.Element("eventhandler").Element("type");
            else
                typeInfo = transformation.ReferenceNode.Element("returns").Elements().First();

            transformation.RenderTypeReferenceLink(content, typeInfo, false);
        }

        /// <summary>
        /// Render the View Source and Request Example buttons for the source context element
        /// </summary>
        /// <param name="transformation">The transformation to use</param>
        /// <param name="sourceContext">The element containing the source context information</param>
        /// <param name="content">The content element to which the sections will be added</param>
        private void RenderSourceContextElement(TopicTransformationCore transformation, XElement sourceContext,
          XElement content)
        {
            string requestExampleUrl = null, baseSourceCodeUrl = null;

            if(!String.IsNullOrWhiteSpace(this.RequestExampleUrlArgName))
                requestExampleUrl = transformation.TransformationArguments[this.RequestExampleUrlArgName].Value;

            if(!String.IsNullOrWhiteSpace(this.BaseSourceCodeUrlArgName))
                baseSourceCodeUrl = transformation.TransformationArguments[this.BaseSourceCodeUrlArgName].Value;

            if(!String.IsNullOrWhiteSpace(requestExampleUrl))
            {
                content.Add(new XElement("include",
                    new XAttribute("item", "requestExample"),
                    new XElement("parameter", requestExampleUrl),
                    new XElement("parameter", this.LinkButtonStyle)));
            }

            if(sourceContext != null && !String.IsNullOrWhiteSpace(baseSourceCodeUrl))
            {
                string file = baseSourceCodeUrl + sourceContext.Attribute("file").Value,
                    lineNumber = sourceContext.Attribute("startLine")?.Value;

                if(!String.IsNullOrWhiteSpace(lineNumber))
                    file += "#L" + lineNumber;

                content.Add(new XElement("a",
                        new XAttribute("target", "_blank"),
                        new XAttribute("class", this.LinkButtonStyle),
                        new XAttribute("href", file),
                        new XAttribute("rel", "noopener noreferrer"),
                    new XElement("includeAttribute",
                        new XAttribute("name", "title"),
                        new XAttribute("item", "sourceCodeLinkTitle")),
                    new XElement("include",
                        new XAttribute("item", "sourceCodeLinkText"))));
            }
        }
        #endregion
    }
}
