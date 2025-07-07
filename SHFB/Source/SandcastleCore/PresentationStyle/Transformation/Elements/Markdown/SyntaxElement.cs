//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SyntaxElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle syntax section elements in markdown presentation styles
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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.Reflection;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown
{
    /// <summary>
    /// This is used to handle <c>syntax</c> elements in markdown presentation styles topics
    /// </summary>
    public class SyntaxElement : Element
    {
        #region Private data members
        //=====================================================================

        private static readonly HashSet<string> includedXamlSyntax = [.. new[] {
            "xamlAttributeUsageHeading", "xamlObjectElementUsageHeading", "xamlContentElementUsageHeading",
            "xamlPropertyElementUsageHeading" }];

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the base source code URL transformation argument name
        /// </summary>
        public string BaseSourceCodeUrlArgName { get; }

        /// <summary>
        /// This is used to get or set the action delegate use to render the namespace and assembly information
        /// </summary>
        public Action<TopicTransformationCore, XElement> NamespaceAndAssemblyInfoRenderer { get; set; }

        /// <summary>
        /// This is used to get or set the action delegate use to render the inheritance hierarchy information
        /// </summary>
        public Action<TopicTransformationCore, XElement> InheritanceHierarchyRenderer { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseSourceCodeUrlArgName">The base source code URL transformation argument name to use
        /// or null if there isn't one.</param>
        public SyntaxElement(string baseSourceCodeUrlArgName) : base("syntax")
        {
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

            var (title, content) = transformation.CreateSection(element.GenerateUniqueId(), true,
                "title_definition", null);

            if(title != null)
                transformation.CurrentElement.Add(title);

            if(content != null)
                transformation.CurrentElement.Add(content);
            else
                content = transformation.CurrentElement;

            this.NamespaceAndAssemblyInfoRenderer?.Invoke(transformation, content);

            if(element.Elements().Any())
                RenderSyntaxSections(transformation, element, content);

            string baseSourceCodeUrl = null;
            var sourceContext = transformation.ReferenceNode.Element("sourceContext");

            // Add the view source option if wanted.  If not, remove the link.
            if(!String.IsNullOrWhiteSpace(this.BaseSourceCodeUrlArgName))
                baseSourceCodeUrl = transformation.TransformationArguments[this.BaseSourceCodeUrlArgName].Value;

            if(sourceContext != null && !String.IsNullOrWhiteSpace(baseSourceCodeUrl))
            {
                string file = baseSourceCodeUrl + sourceContext.Attribute("file").Value,
                    lineNumber = sourceContext.Attribute("startLine")?.Value;

                if(!String.IsNullOrWhiteSpace(lineNumber))
                    file += "#L" + lineNumber;

                content.Add("\n\n",
                    new XElement("a",
                        new XAttribute("href", file),
                        new XElement("includeAttribute",
                            new XAttribute("name", "title"),
                            new XAttribute("item", "sourceCodeLinkTitle")),
                        new XElement("include",
                            new XAttribute("item", "sourceCodeLinkText"))), "\n\n");
            }

            if(transformation.ApiMember.ApiGroup == ApiMemberGroup.Type)
                this.InheritanceHierarchyRenderer?.Invoke(transformation, content);

            XElement parameters = transformation.ReferenceNode.Element("parameters"),
                templates = transformation.ReferenceNode.Element("templates"),
                implements = transformation.ReferenceNode.Element("implements"),
                returnValue = transformation.ReferenceNode.Element("returns"),
                eventHandlerType = transformation.ReferenceNode.Element("eventhandler")?.Element("type"),
                value = transformation.CommentsNode.Element("value"),
                returns = transformation.CommentsNode.Element("returns");

            transformation.RenderChildElements(content, [parameters, templates, value, returns]);

            // If there were no value or returns comments and there is a return value, add a default return
            // value section.
            if(value == null && returns == null && (returnValue != null || eventHandlerType != null))
                RenderDefaultReturnSection(transformation, content);

            transformation.RenderChildElements(content, [implements]);

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
                var paramType = transformation.ReferenceNode.Element("parameters").Element("parameter").Elements().First();

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
        private static void RenderSyntaxSections(TopicTransformationCore transformation, XElement element, XElement content)
        {
            var codeDivs = element.Elements("div").Where(d =>
            {
                var lang = d.Attribute("codeLanguage");

                if(lang == null)
                    return false;

                if(lang.Value != "XAML")
                    return true;

                // Suppress snippets containing only boilerplate XAML which isn't currently shown
                return d.Elements("div").Any(subDiv => includedXamlSyntax.Contains(subDiv.Attribute("class").Value));

            }).ToList();

            if(codeDivs.Count == 0)
                return;

            content.Add("\n\n");

            foreach(var div in codeDivs)
            {
                string codeLanguage = div.Attribute("codeLanguage")?.Value;

                content.Add("**",
                    new XElement("include",
                        new XAttribute("item", $"devlang_{codeLanguage}"),
                        new XAttribute("undefined", codeLanguage)), "**\n",
                    "``` ",
                    new XElement("include",
                        new XAttribute("item", $"devlang_{codeLanguage}"),
                        new XAttribute("undefined", codeLanguage)), "\n");

                if(div.Attribute("phantom") != null)
                    content.Add(new XElement("include", new XAttribute("item", "noCodeExample")));
                else
                {
                    if(div.Attribute("codeLanguage").Value == "XAML")
                        RenderXamlSyntaxBlock(transformation, div, content);
                    else
                        transformation.RenderChildElements(content, div.Nodes());
                }

                content.Add("\n```\n");
            }

            content.Add("\n");
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
                transformation.RenderChildElements(content, block.Nodes());
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
                typeInfo = isReadOnlyAttr.Element("argument").Element("typeValue").Element("type");
            else if(isByRefLikeAttr != null)
                typeInfo = isByRefLikeAttr.Element("argument").Element("typeValue").Element("type");
            else if(transformation.ApiMember.ApiSubgroup == ApiMemberGroup.Event)
                typeInfo = transformation.ReferenceNode.Element("eventhandler").Element("type");
            else
                typeInfo = transformation.ReferenceNode.Element("returns").Elements().First();

            transformation.RenderTypeReferenceLink(content, typeInfo, false);
        }
        #endregion
    }
}
