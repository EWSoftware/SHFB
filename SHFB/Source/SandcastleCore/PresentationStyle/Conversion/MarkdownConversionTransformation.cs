//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkdownTransformation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to generate a Markdown topic from a MAML topic for the Markdown conversion
// presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Transformation;
using Sandcastle.Core.PresentationStyle.Transformation.Elements;
using Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;
using Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown;
using Sandcastle.Core.Project;

using MarkdownGlossaryElement = Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown.GlossaryElement;

namespace Sandcastle.Core.PresentationStyle.Conversion;

/// <summary>
/// This class is used to generate a Markdown topic from a MAML topic for the Markdown conversion presentation
/// style by utilizing the existing parsing and rendering features of the presentation style transformation.
/// </summary>
public class MarkdownConversionTransformation : TopicTransformationCore
{
    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="resolvePath">The function used to resolve content file paths for the presentation style</param>
    public MarkdownConversionTransformation(Func<string, string> resolvePath) : base(HelpFileFormats.Markdown, resolvePath)
    {
    }
    #endregion

    #region TopicTransformationCore implementation
    //=====================================================================

    /// <inheritdoc />
    public override string IconPath { get; set; } = "media/";

    /// <inheritdoc />
    /// <remarks>Not used by this transformation</remarks>
    public override string StyleSheetPath { get; set; }

    /// <inheritdoc />
    /// <remarks>Not used by this transformation</remarks>
    public override string ScriptPath { get; set; }

    /// <inheritdoc />
    /// <remarks>Not used by this transformation</remarks>
    protected override void CreateTransformationArguments()
    {
    }

    /// <inheritdoc />
    /// <remarks>Not used by this transformation</remarks>
    protected override void CreateLanguageSpecificText()
    {
    }

    /// <inheritdoc />
    protected override void CreateElementHandlers()
    {
        this.AddElements(
        [
            // MAML document root element types
            new NonRenderedParentElement("topic"),
            new NonRenderedParentElement("codeEntityDocument"),
            new NonRenderedParentElement("developerConceptualDocument"),
            new NonRenderedParentElement("developerErrorMessageDocument"),
            new NonRenderedParentElement("developerGlossaryDocument"),
            new NonRenderedParentElement("developerHowToDocument"),
            new NonRenderedParentElement("developerOrientationDocument"),
            new NonRenderedParentElement("developerReferenceWithSyntaxDocument"),
            new NonRenderedParentElement("developerReferenceWithoutSyntaxDocument"),
            new NonRenderedParentElement("developerSDKTechnologyOverviewArchitectureDocument"),
            new NonRenderedParentElement("developerSDKTechnologyOverviewCodeDirectoryDocument"),
            new NonRenderedParentElement("developerSDKTechnologyOverviewOrientationDocument"),
            new NonRenderedParentElement("developerSDKTechnologyOverviewScenariosDocument"),
            new NonRenderedParentElement("developerSDKTechnologyOverviewTechnologySummaryDocument"),
            new NonRenderedParentElement("developerSampleDocument"),
            new NonRenderedParentElement("developerTroubleshootingDocument"),
            new NonRenderedParentElement("developerUIReferenceDocument"),
            new NonRenderedParentElement("developerWalkthroughDocument"),
            new NonRenderedParentElement("developerWhitePaperDocument"),
            new NonRenderedParentElement("developerXmlReference"),

            // HTML elements
            new PassthroughElement("a"),
            new PassthroughElement("abbr"),
            new PassthroughElement("acronym"),
            new PassthroughElement("address", true),
            new PassthroughElement("area"),
            new PassthroughElement("article", true),
            new PassthroughElement("aside", true),
            new PassthroughElement("audio"),
            new MarkdownElement("b", "**", "**", null),
            new PassthroughElement("bdi"),
            new PassthroughElement("blockquote", true),
            new MarkdownElement("br", null, "  \n", null),
            new PassthroughElement("canvas"),
            new PassthroughElement("col"),
            new PassthroughElement("colgroup"),
            new PassthroughElement("datalist"),
            new PassthroughElement("dd", true),
            new MarkdownElement("del", "~~", "~~", null),
            new PassthroughElement("details", true),
            new PassthroughElement("dfn"),
            new PassthroughElement("dialog", true),
            new PassthroughElement("div", true),
            new PassthroughElement("dl", true),
            new PassthroughElement("dt", true),
            new MarkdownElement("em", "*", "*", null),
            new PassthroughElement("embed"),
            new PassthroughElement("fieldset", true),
            new PassthroughElement("figcaption", true),
            new PassthroughElement("figure", true),
            new PassthroughElement("font"),
            new PassthroughElement("footer", true),
            new PassthroughElement("form", true),
            new PassthroughElement("frame"),
            new PassthroughElement("frameset"),
            new MarkdownElement("h1", "# ", null, null, true),
            new MarkdownElement("h2", "## ", null, null, true),
            new MarkdownElement("h3", "### ", null, null, true),
            new MarkdownElement("h4", "#### ", null, null, true),
            new MarkdownElement("h5", "##### ", null, null, true),
            new MarkdownElement("h6", "###### ", null, null, true),
            new PassthroughElement("header", true),
            new MarkdownElement("hr", "---", null, null, true),
            new MarkdownElement("i", "*", "*", null),
            new PassthroughElement("iframe"),
            new PassthroughElement("img"),
            new MarkdownElement("ins", "++", "++", null),
            new PassthroughElement("keygen", true),
            new PassthroughElement("legend", true),
            new PassthroughElement("li", true),
            new PassthroughElement("main", true),
            new PassthroughElement("map"),
            new MarkdownElement("mark", "==", "==", null),
            new PassthroughElement("meter"),
            new PassthroughElement("nav", true),
            new PassthroughElement("ol", true),
            new PassthroughElement("optgroup", true),
            new PassthroughElement("option"),
            new PassthroughElement("output"),
            new MarkdownElement("p", "\n", "\n", null, true),
            new PassthroughElement("pre", true) { DoNotParse = true },
            new PassthroughElement("progress"),
            new PassthroughElement("q"),
            new PassthroughElement("rp"),
            new PassthroughElement("rt"),
            new PassthroughElement("ruby"),
            new PassthroughElement("script", true) { DoNotParse = true },
            new PassthroughElement("search", true),
            new PassthroughElement("select"),
            new PassthroughElement("source", true),
            new MarkdownElement("strong", "**", "**", null),
            new PassthroughElement("style", true) { DoNotParse = true },
            new MarkdownElement("sub", "~", "~", null),
            new MarkdownElement("sup", "^", "^", null),
            new PassthroughElement("svg"),
            new PassthroughElement("tbody", true),
            new PassthroughElement("td", true),
            new PassthroughElement("textarea", true) { DoNotParse = true },
            new PassthroughElement("tfoot", true),
            new PassthroughElement("th", true),
            new PassthroughElement("thead", true),
            new PassthroughElement("time"),
            new PassthroughElement("tr", true),
            new PassthroughElement("track"),
            new PassthroughElement("u"),
            new PassthroughElement("ul", true),
            new PassthroughElement("video"),
            new PassthroughElement("wbr"),

            // Elements common to HTML, MAML, and/or XML comments.  Processing may differ based on the topic
            // type (API or MAML).
            new PassthroughElement("bibliography"),
            new PassthroughElement("cite"),
            new CodeElement("code"),
            new PassthroughElement("include"),
            new PassthroughElement("includeAttribute"),
            new MarkupElement(),
            new MarkdownElement("para", "\n", "\n", null, true),
            new ListElement("list"),
            new ParametersElement(),
            new PassthroughElement("referenceLink"),
            new PassthroughElement("span"),
            new PassthroughElement("snippet"),
            new PassthroughElement("summary"),
            new TableElement(),
            new PassthroughElement("token"),

            // MAML elements
            new NoteElement("alert"),
            new MarkdownElement("application", "**", "**", null),
            new NamedSectionElement("appliesTo"),
            new PassthroughElement("autoOutline", true) { DoNotParse = true },
            new NamedSectionElement("background"),
            new NamedSectionElement("buildInstructions"),
            new CodeEntityReferenceElement(),
            new PassthroughElement("codeExample"),
            new MarkdownElement("codeFeaturedElement", "**", "**", null),
            new MarkdownElement("codeInline", "`", "`", null),
            new PassthroughElement("codeReference", true) { DoNotParse = true },
            // Command may contain nested elements and markdown inline code (`text`) doesn't render nested
            // formatting so we use a code element instead.
            new ConvertibleElement("command", "code"),
            new MarkdownElement("computerOutputInline", "`", "`", null),
            new NonRenderedParentElement("conclusion"),
            new NonRenderedParentElement("content"),
            new PassthroughElement("copyright"),
            new NonRenderedParentElement("corporation"),
            new NonRenderedParentElement("country"),
            new MarkdownElement("database", "**", "**", null),
            new NonRenderedParentElement("date"),
            new PassthroughElement("definedTerm", true),
            new PassthroughElement("definition", true),
            new DefinitionTableElement(),
            new NamedSectionElement("demonstrates"),
            new NonRenderedParentElement("description"),
            new NamedSectionElement("dotNetFrameworkEquivalent"),
            new MarkdownElement("embeddedLabel", "**", "**", null),
            new PassthroughElement("entry", true),
            new MarkdownElement("environmentVariable", "`", "`", null),
            new MarkdownElement("errorInline", "*", "*", null),
            new NamedSectionElement("exceptions"),
            new ExternalLinkElement(),
            new NamedSectionElement("externalResources"),
            new MarkdownElement("fictitiousUri", "*", "*", null),
            new MarkdownElement("foreignPhrase", "*", "*", null),
            new MarkdownGlossaryElement(),
            new MarkdownElement("hardware", "**", "**", null),
            new NamedSectionElement("inThisSection"),
            new IntroductionElement(),
            new LanguageKeywordElement(),
            new NamedSectionElement("languageReferenceRemarks"),
            new NonRenderedParentElement("legacy"),
            new MarkdownElement("legacyBold", "**", "**", null),
            new MarkdownElement("legacyItalic", "*", "*", null),
            new LegacyLinkElement(),
            new ConvertibleElement("legacyUnderline", "u"),
            new MarkdownElement("lineBreak", null, "  \n", null),
            new LinkElement(),
            new PassthroughElement("listItem", true),
            new MarkdownElement("literal", "&#60;span class=\"literal\"&#62;", "&#60;/span&#62;", null),
            new MarkdownElement("localUri", "*", "*", null),
            new NonRenderedParentElement("localizedText"),
            new MarkdownElement("math", "*", "*", null),
            new MediaLinkElement(),
            new MediaLinkInlineElement(),
            new MarkdownElement("newTerm", "*", "*", null),
            new NamedSectionElement("nextSteps"),
            new MarkdownElement("parameterReference", "*", "*", null),
            new MarkdownElement("phrase", "*", "*", null),
            new MarkdownElement("placeholder", "*", "*", null),
            new NamedSectionElement("prerequisites"),
            new ProcedureElement(),
            new ConvertibleElement("quote", "blockquote", false, true),
            new ConvertibleElement("quoteInline", "q"),
            new NamedSectionElement("reference"),
            new NamedSectionElement("relatedSections"),
            new RelatedTopicsElement(),
            new MarkdownElement("replaceable", "*", "*", null),
            new NamedSectionElement("requirements"),
            new NamedSectionElement("returnValue"),
            new NamedSectionElement("robustProgramming"),
            new PassthroughElement("row", true),
            new SectionElement(),
            new NonRenderedParentElement("sections"),
            new NamedSectionElement("security"),
            new NonRenderedParentElement("snippets"),
            new PassthroughElement("step", true),
            new ListElement("steps"),
            new MarkdownElement("subscript", "~", "~", null),
            new MarkdownElement("subscriptType", "~", "~", null),
            new MarkdownElement("superscript", "^", "^", null),
            new MarkdownElement("superscriptType", "^", "^", null),
            new MarkdownElement("system", "**", "**", null),
            new PassthroughElement("tableHeader", true),
            new NamedSectionElement("textValue"),
            // The title element is ignored.  The section and table elements handle them as needed.
            new IgnoredElement("title"),
            new NonRenderedParentElement("type"),
            new MarkdownElement("ui", "**", "**", null),
            new MarkdownElement("unmanagedCodeEntityReference", "**", "**", null),
            new MarkdownElement("userInput", "**", "**", null),
            new MarkdownElement("userInputLocalizable", "**", "**", null),
            new NamedSectionElement("whatsNew")
        ]);
    }

    /// <inheritdoc />
    /// <remarks>Not used by this transformation</remarks>
    protected override void CreateApiTopicSectionHandlers()
    {
    }

    /// <inheritdoc />
    /// <remarks>Not used by this transformation</remarks>
    protected override void CreateNoticeDefinitions()
    {
    }

    /// <inheritdoc />
    protected override XDocument RenderTopic()
    {
        var document = new XDocument(new XElement("document",
            new XAttribute(XNamespace.Xml + "space", "preserve")));

        this.CurrentElement = document.Root;

        // Add YAML Front Matter info
        this.CurrentElement.Add("---\n");
        this.CurrentElement.Add($"uid: {this.MetadataNode.Element("id").Value}\n");
        this.CurrentElement.Add($"alt-uid: {this.MetadataNode.Element("alternateId").Value}\n");

        var title = this.MetadataNode.Element("title");

        if(!title.IsEmpty)
            this.CurrentElement.Add($"title: {title.Value}\n");

        // If a summary element is present and its marked as the abstract, add it to the header and remove it
        var summary = this.TopicNode.Descendants(Element.Ddue + "summary").FirstOrDefault();

        if(summary != null)
        {
            if(summary.Attribute("abstract")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                this.CurrentElement.Add($"abstract: {summary.Value.Trim().NormalizeWhiteSpace()}\n");
                summary.Remove();
            }
        }

        var keywords = this.MetadataNode.Elements("keyword").ToList();

        if(keywords.Count != 0)
        {
            this.CurrentElement.Add("keywords: ");

            bool isFirst = true;

            foreach(var keyword in keywords)
            {
                if(isFirst)
                    isFirst = false;
                else
                    this.CurrentElement.Add(", ");

                string value = keyword.Value.Trim();

                if(value.IndexOf(',') != -1)
                    this.CurrentElement.Add($"\"{keyword.Value.Replace("\"", "\\\"")}\"");
                else
                    this.CurrentElement.Add(keyword.Value);
            }

            this.CurrentElement.Add("\n");
        }

        this.CurrentElement.Add("---\n");

        this.OnRenderStarting(document);

        // Add the topic content.  MAML topics are rendered purely off of the element types.
        this.RenderNode(this.TopicNode);

        // Replace SectionTitle wrapper elements with their content
        foreach(var st in document.Descendants("SectionTitle").ToList())
        {
            foreach(var n in st.Nodes().ToList())
            {
                n.Remove();
                st.AddBeforeSelf(n);
            }

            st.Remove();
        }

        this.OnRenderCompleted(document);

        return document;
    }

    /// <summary>
    /// Render the given XML node based on its node type
    /// </summary>
    /// <param name="node">The node to render</param>
    public override void RenderNode(XNode node)
    {
        switch(node)
        {
            case XText text:
                this.RenderTextNode(this.CurrentElement, text);
                break;

            case XElement element:
                Element handler = this.ElementHandlerFor(element.Name.LocalName);

                // If it has no handler, add a default handler and treat it like a non-rendered parent element
                if(handler == null)
                {
                    // Raise an event to flag unhandled elements
                    this.OnUnhandledElement(element.Name.LocalName, element.Parent.Name.LocalName);

                    handler = new NonRenderedParentElement(element.Name.LocalName);

                    this.AddElement(handler);
                }

                handler.Render(this, element);
                break;

            case XComment comment:
                // Keep comments as they may be relevant in the source document
                this.CurrentElement.Add(comment);
                this.CurrentElement.Add("\n");
                break;

            default:
                if(node != null)
                {
                    Debug.WriteLine("Unhandled node type: {0}", node.NodeType);

                    if(Debugger.IsAttached)
                        Debugger.Break();
                }
                break;
        }
    }

    /// <summary>
    /// This is used to provide additional whitespace handling and normalization for markdown elements
    /// </summary>
    /// <param name="content">The content element to which the text is added</param>
    /// <param name="textNode">The text node to render</param>
    /// <remarks>For conversion, text is passed through as-is without any normalization except for stripping
    /// whitespace at the start of lines.  This may result in some odd formatting but should help preserve
    /// the overall layout of the source document.</remarks>
    public override void RenderTextNode(XElement content, XText textNode)
    {
        var (_, doNotParse) = this.HtmlBlockAndDoNotParseTags;

        // Pass text through as-is within elements that should not be parsed
        if(doNotParse.Any(d => d == textNode.Parent.Name.LocalName))
            content.Add(textNode.Value);
        else
        {
            StringBuilder sb = new(textNode.Value);
            int idx = 0;

            do
            {
                while(idx < sb.Length && (sb[idx] == '\r' || sb[idx] == '\n'))
                    idx++;

                // Don't strip all leading whitespace if it's less than two characters as it may be necessary
                // separating whitespace between elements.  Even then, we'll leave one space just in case if this
                // isn't the first node.
                if(idx + 2 < sb.Length && Char.IsWhiteSpace(sb[idx]) && Char.IsWhiteSpace(sb[idx + 1]))
                {
                    if(textNode.PreviousNode != null)
                        idx++;

                    while(idx < sb.Length && Char.IsWhiteSpace(sb[idx]))
                        sb.Remove(idx, 1);
                }

                while(idx < sb.Length && sb[idx] != '\r' && sb[idx] != '\n')
                    idx++;

            } while(idx < sb.Length);

            content.Add(sb.ToString());
        }
    }

    /// <inheritdoc />
    /// <remarks>The returned content element is always null and the content should be inserted into the
    /// transformation's current element after adding the title element.  The <paramref name="linkId"/>
    /// parameter is not used.</remarks>
    public override (XElement Title, XElement Content) CreateSection(string uniqueId, bool localizedTitle,
      string title, string linkId, int headingLevel)
    {
        XElement titleElement = null;

        if(String.IsNullOrWhiteSpace(title))
        {
            if(localizedTitle)
                throw new ArgumentException("Title cannot be null if it represents a localized item ID", nameof(title));
        }
        else
        {
            XNode titleContent;

            if(localizedTitle)
                titleContent = new XElement("include", new XAttribute("item", title));
            else
                titleContent = new XText(title);

            // Default to a heading level of 2 if not specified since the topic title is level 1
            if(headingLevel < 1)
                headingLevel = 2;
            else
            {
                if(headingLevel > 6)
                    headingLevel = 6;
            }

            string heading = new('#', headingLevel);

            // Wrap the title in a placeholder element.  This container element will be removed by the
            // markdown content generator.
            titleElement = new XElement("SectionTitle",
                new XAttribute(Element.XmlSpace, "preserve"), $"\n\n{heading} ",
                titleContent, "\n");
        }

        return (titleElement, null);
    }

    /// <inheritdoc />
    /// <remarks>The returned content element is always null and the content should be inserted into the
    /// transformation's current element after adding the title element.</remarks>
    public override (XElement Title, XElement Content) CreateSubsection(bool localizedTitle, string title)
    {
        XElement titleElement = null;

        if(String.IsNullOrWhiteSpace(title))
        {
            if(localizedTitle)
                throw new ArgumentException("Title cannot be null if it represents a localized item ID", nameof(title));
        }
        else
        {
            XNode titleContent;

            if(localizedTitle)
                titleContent = new XElement("include", new XAttribute("item", title));
            else
                titleContent = new XText(title);

            // Wrap the title in a placeholder element.  This container element will be removed by the
            // markdown content generator.
            titleElement = new XElement("SectionTitle",
                new XAttribute(Element.XmlSpace, "preserve"), "\n\n#### ",
                titleContent, "\n");
        }

        return (titleElement, null);
    }
    #endregion
}
