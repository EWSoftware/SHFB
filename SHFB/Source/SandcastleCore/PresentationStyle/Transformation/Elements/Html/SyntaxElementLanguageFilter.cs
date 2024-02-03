//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SyntaxElementLanguageFilter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/02/2024
// Note    : Copyright 2022-2024, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle syntax section elements in presentation styles that use a
// page-level language filter selector.
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

// Ignore Spelling: plaintext

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.Reflection;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>syntax</c> elements in a topic for presentation styles that use a page-level
    /// language filter selector.
    /// </summary>
    public class SyntaxElementLanguageFilter : Element
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
        /// This is used to get or set the syntax section code template file path
        /// </summary>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string SyntaxSectionCodeTemplatePath { get; set; }

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
        /// <param name="requestExampleUrlArgName">The Request Example URL transformation argument name to use or
        /// null if there isn't one.</param>
        /// <param name="baseSourceCodeUrlArgName">The base source code URL transformation argument name to use
        /// or null if there isn't one.</param>
        public SyntaxElementLanguageFilter(string requestExampleUrlArgName, string baseSourceCodeUrlArgName) : base("syntax")
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

            // Resolve unset paths on first use
            if(String.IsNullOrWhiteSpace(this.SyntaxSectionCodeTemplatePath))
                this.SyntaxSectionCodeTemplatePath = transformation.ResolvePath(@"Templates\SyntaxSectionCodeTemplate.html");

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
                this.RenderSyntaxSections(transformation, element, content);

            if(transformation.ApiMember.ApiGroup == ApiMemberGroup.Type)
                this.InheritanceHierarchyRenderer?.Invoke(transformation, content);

            XElement parameters = transformation.ReferenceNode.Element("parameters"),
                templates = transformation.ReferenceNode.Element("templates"),
                implements = transformation.ReferenceNode.Element("implements"),
                returnValue = transformation.ReferenceNode.Element("returns"),
                eventHandlerType = transformation.ReferenceNode.Element("eventhandler")?.Element("type"),
                value = transformation.CommentsNode.Element("value"),
                returns = transformation.CommentsNode.Element("returns");

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
        private void RenderSyntaxSections(TopicTransformationCore transformation, XElement element, XElement content)
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

            int nodeCount = codeDivs.Count, position = 1;

            if(nodeCount == 0)
                return;

            var template = TopicTransformationCore.LoadTemplateFile(this.SyntaxSectionCodeTemplatePath, null);

            string requestExampleUrl = null, baseSourceCodeUrl = null;
            var sourceContext = transformation.ReferenceNode.Element("sourceContext");

            // Add view source and request example options if wanted.  If not, remove the links.
            if(!String.IsNullOrWhiteSpace(this.RequestExampleUrlArgName))
                requestExampleUrl = transformation.TransformationArguments[this.RequestExampleUrlArgName].Value;

            if(!String.IsNullOrWhiteSpace(this.BaseSourceCodeUrlArgName))
                baseSourceCodeUrl = transformation.TransformationArguments[this.BaseSourceCodeUrlArgName].Value;

            var anchor = template.Descendants("a").FirstOrDefault(
                a => a.Attribute("id")?.Value == "RequestExampleLink");

            // If not found, act like it wasn't specified as it may not be in a custom template
            if(anchor != null)
            {
                if(!String.IsNullOrWhiteSpace(requestExampleUrl))
                {
                    anchor.Attribute("id").Remove();
                    anchor.Attribute("href").Value = requestExampleUrl;

                    transformation.RegisterStartupScriptItem(5000, "requestExampleScript");
                }
                else
                    anchor.Remove();
            }

            anchor = template.Descendants("a").FirstOrDefault(
                a => a.Attribute("id")?.Value == "ViewSourceLink");

            if(anchor != null)
            {
                if(sourceContext != null && !String.IsNullOrWhiteSpace(baseSourceCodeUrl))
                {
                    string file = baseSourceCodeUrl + sourceContext.Attribute("file").Value,
                        lineNumber = sourceContext.Attribute("startLine")?.Value;

                    if(!String.IsNullOrWhiteSpace(lineNumber))
                        file += "#L" + lineNumber;

                    anchor.Attribute("id").Remove();
                    anchor.Attribute("href").Value = file;
                }
                else
                    anchor.Remove();
            }

            foreach(var div in codeDivs)
            {
                var codeBlock = new XElement(template.Root);
                string codeLanguage = div.Attribute("codeLanguage")?.Value,
                    style = div.Attribute("style")?.Value;

                // The snippet language shouldn't be added to single snippets as we never want to hide them
                if(nodeCount != 1)
                    codeBlock.Add(new XAttribute("data-codeSnippetLanguage", style));

                if(position != 1)
                    codeBlock.Attribute("class").Value += " is-hidden";

                var title = codeBlock.Descendants().FirstOrDefault(p => p.Attribute("id")?.Value == "Title");

                if(title == null)
                {
                    throw new InvalidOperationException("Unable to locate the title container with the id " +
                        "'Title' in the syntax section code template");
                }

                title.Attribute("id").Remove();
                title.Add(new XElement("include",
                    new XAttribute("item", $"devlang_{codeLanguage}"),
                    new XAttribute("undefined", codeLanguage)));

                var codeContainer = codeBlock.Descendants("pre").FirstOrDefault(p => p.Attribute("id")?.Value == "CodeBlock");

                if(codeContainer == null)
                {
                    throw new InvalidOperationException("Unable to locate the code container with the id " +
                        "'CodeBlock' in the syntax section code template");
                }

                codeContainer.Attribute("id").Remove();
                codeContainer.RemoveNodes();

                if(div.Attribute("phantom") != null)
                {
                    codeContainer.Add(new XElement("code",
                        new XAttribute("class", "language-plaintext"),
                        new XElement("include", new XAttribute("item", "noCodeExample"))));
                }
                else
                {
                    if(codeLanguage == "XAML")
                        RenderXamlSyntaxBlock(transformation, div, codeContainer);
                    else
                    {
                        if(transformation.UsesLegacyCodeColorizer)
                            codeContainer.Add(div.Nodes());
                        else
                        {
                            if(transformation.CodeSnippetLanguageConversion.TryGetValue(style, out string newId))
                                style = newId;

                            codeContainer.Add(new XElement("code",
                                new XAttribute("class", $"language-{style}"),
                                div.Nodes()));

                            transformation.RegisterStartupScript(10000, "hljs.highlightAll();");
                        }
                    }
                }

                content.Add(codeBlock);
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
            {
                if(transformation.UsesLegacyCodeColorizer)
                    content.Add(block.Nodes());
                else
                {
                    content.Add(new XElement("code", new XAttribute("class", "language-xml"), block.Nodes()));
                    transformation.RegisterStartupScript(10000, "hljs.highlightAll();");
                }
            }
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
