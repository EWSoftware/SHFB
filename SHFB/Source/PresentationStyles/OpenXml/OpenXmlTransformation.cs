//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : OpenXmlTransformation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/04/2024
// Note    : Copyright 2022-2024, Eric Woodruff, All rights reserved
//
// This file contains the class used to generate a MAML or API HTML topic from the raw topic XML data for the
// Open XML presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/25/2022  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: dxa

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle.Transformation;
using Sandcastle.Core.PresentationStyle.Transformation.Elements;
using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;
using Sandcastle.Core.Reflection;

namespace Sandcastle.PresentationStyles.OpenXml
{
    /// <summary>
    /// This class is used to generate a MAML or API HTML topic from the raw topic XML data for the Open XML
    /// presentation style.
    /// </summary>
    public class OpenXmlTransformation : TopicTransformationCore
    {
        #region Private data members
        //=====================================================================

        private XDocument pageTemplate;

        private static readonly HashSet<string> spacePreservedElements = new HashSet<string>(
            new[] { "code", "pre", "snippet" }, StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resolvePath">The function used to resolve content file paths for the presentation style</param>
        public OpenXmlTransformation(Func<string, string> resolvePath) : base(HelpFileFormats.OpenXml, resolvePath)
        {
            this.TopicTemplatePath = this.ResolvePath(@"Templates\TopicTemplate.xml");
            this.UsesLegacyCodeColorizer = true;
        }
        #endregion

        #region Topic transformation argument shortcut properties
        //=====================================================================

        /// <summary>
        /// Maximum version parts
        /// </summary>
        private int MaxVersionParts => Int32.TryParse(this.TransformationArguments[nameof(MaxVersionParts)].Value,
            out int maxVersionParts) ? maxVersionParts : 5;

        /// <summary>
        /// Include enumerated type values
        /// </summary>
        private bool IncludeEnumValues => Boolean.TryParse(this.TransformationArguments[nameof(IncludeEnumValues)].Value,
            out bool includeEnumValues) && includeEnumValues;

        /// <summary>
        /// Enumeration member sort order
        /// </summary>
        private EnumMemberSortOrder EnumMemberSortOrder => Enum.TryParse(this.TransformationArguments[nameof(EnumMemberSortOrder)].Value,
            true, out EnumMemberSortOrder sortOrder) ? sortOrder : EnumMemberSortOrder.Value;

        /// <summary>
        /// Flags enumeration value format
        /// </summary>
        private EnumValueFormat FlagsEnumValueFormat => Enum.TryParse(this.TransformationArguments[nameof(FlagsEnumValueFormat)].Value,
            true, out EnumValueFormat format) ? format : EnumValueFormat.IntegerValue;

        /// <summary>
        /// Flags enumeration value separator group size
        /// </summary>
        private int FlagsEnumSeparatorSize => Int32.TryParse(this.TransformationArguments[nameof(FlagsEnumSeparatorSize)].Value,
            out int groupSize) ? groupSize : 0;

        /// <summary>
        /// Base source code URL
        /// </summary>
        private string BaseSourceCodeUrl => this.TransformationArguments[nameof(BaseSourceCodeUrl)].Value;

        #endregion

        #region TopicTransformationCore implementation
        //=====================================================================

        /// <inheritdoc />
        public override string IconPath { get; set; } = "../media/";

        /// <inheritdoc />
        /// <remarks>Not used by this transformation</remarks>
        public override string StyleSheetPath { get; set; }

        /// <inheritdoc />
        /// <remarks>Not used by this transformation</remarks>
        public override string ScriptPath { get; set; }

        /// <inheritdoc />
        protected override void CreateTransformationArguments()
        {
            this.AddTransformationArgumentRange(new[]
            {
                new TransformationArgument(nameof(BibliographyDataFile), true, true, null,
                    "An optional bibliography data XML file.  Specify the filename with a fully qualified or " +
                    "relative path.  If the path is relative or omitted, it is assumed to be relative to the " +
                    "project folder.\r\n\r\n" +
                    "If blank, no bibliography section will be included in the topics.\r\n\r\n" +
                    "For information on the data file's format, see the bibliography element topic in the " +
                    "Sandcastle MAML Guide or XML Comments Guide."),
                new TransformationArgument(nameof(MaxVersionParts), false, true, null,
                    "The maximum number of assembly version parts to show in API member topics.  Set to 2, " +
                    "3, or 4 to limit it to 2, 3, or 4 parts or leave it blank for all parts including the " +
                    "assembly file version value if specified."),
                new TransformationArgument(nameof(IncludeEnumValues), false, true, "True",
                    "Set this to True to include the column for the numeric value of each field in " +
                    "enumerated type topics.  Set it to False to omit the numeric values column."),
                new TransformationArgument(nameof(EnumMemberSortOrder), false, true, "Value",
                    "The sort order for enumeration members.  Set it to Value to sort by value or Name to sort " +
                    "by name."),
                new TransformationArgument(nameof(FlagsEnumValueFormat), false, true, "IntegerValue",
                    "The format of flags enumeration values: IntegerValue, HexValue, or BitFlags"),
                new TransformationArgument(nameof(FlagsEnumSeparatorSize), false, true, "0",
                    "The separator group size for flags enumeration values (0, 4, or 8).  This determines where " +
                    "separators are placed in the formatted value (e.g. 0b0000_0000, 0x1234_ABCD).  If set to " +
                    "zero, no separators will be inserted."),
                new TransformationArgument(nameof(BaseSourceCodeUrl), false, true, null,
                    "If you set the Source Code Base Path property in the Paths category, specify the URL to " +
                    "the base source code folder on your project's website here.  Some examples for GitHub are " +
                    "shown below.\r\n\r\n" +
                    "Important: Be sure to set the Source Code Base Path property and terminate the URL below with " +
                    "a slash if necessary.\r\n\r\n" +
                    "Format: https://github.com/YourUserID/YourProject/blob/BranchNameOrCommitHash/BaseSourcePath/ \r\n\r\n" +
                    "Master branch: https://github.com/JohnDoe/WidgestProject/blob/master/src/ \r\n" +
                    "A different branch: https://github.com/JohnDoe/WidgestProject/blob/dev-branch/src/ \r\n" +
                    "A specific commit: https://github.com/JohnDoe/WidgestProject/blob/c6e41c4fc2a4a335352d2ae8e7e85a1859751662/src/")
            });
        }

        /// <inheritdoc />
        /// <remarks>This presentation style does not use language specific text</remarks>
        protected override void CreateLanguageSpecificText()
        {
        }

        /// <inheritdoc />
        protected override void CreateElementHandlers()
        {
            this.AddElements(new Element[]
            {
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

                // HTML elements (may occur in XML comments).  Open XML does not support HTML elements but they
                // are prevalent in XML comments so we'll make our best attempt at converting those with Open XML
                // equivalents and removing those that don't but will pull in their content.
                new PassthroughElement("a"),
                new NonRenderedParentElement("abbr"),
                new NonRenderedParentElement("acronym"),
                new NonRenderedParentElement("area"),
                new NonRenderedParentElement("article"),
                new NonRenderedParentElement("aside"),
                new NonRenderedParentElement("audio"),
                new ConvertibleElement("b", "span", "Bold"),
                new NonRenderedParentElement("bdi"),
                new StyledParagraphElement("blockquote", "Quote"),
                new LineBreakElement("br"),
                new NonRenderedParentElement("canvas"),
                new NonRenderedParentElement("datalist"),
                new NonRenderedParentElement("dd"),
                new NonRenderedParentElement("del"),
                new NonRenderedParentElement("details"),
                new NonRenderedParentElement("dialog"),
                new NonRenderedParentElement("div"),
                new NonRenderedParentElement("dl"),
                new DefinedTermElement("dt"),
                new ConvertibleElement("em", "span", "Emphasis"),
                new NonRenderedParentElement("embed"),
                new NonRenderedParentElement("figcaption"),
                new NonRenderedParentElement("figure"),
                new NonRenderedParentElement("font"),
                new NonRenderedParentElement("footer"),
                new StyledParagraphElement("h1", "Heading1"),
                new StyledParagraphElement("h2", "Heading2"),
                new StyledParagraphElement("h3", "Heading3"),
                new StyledParagraphElement("h4", "Heading4"),
                new StyledParagraphElement("h5", "Heading5"),
                new StyledParagraphElement("h6", "Heading6"),
                new NonRenderedParentElement("header"),
                new HorizontalRuleElement(),
                new ConvertibleElement("i", "span", "Emphasis"),
                new PassthroughElement("img"),
                new NonRenderedParentElement("ins"),
                new NonRenderedParentElement("keygen"),
                // The Open XML file builder will convert lists accordingly
                new ConvertibleElement("li", "li"),
                new NonRenderedParentElement("main"),
                new NonRenderedParentElement("map"),
                new NonRenderedParentElement("mark"),
                new NonRenderedParentElement("meter"),
                new NonRenderedParentElement("nav"),
                // The Open XML file builder will convert lists accordingly
                new ConvertibleElement("ol", "ul", "ordered"),
                new NonRenderedParentElement("output"),
                new ParagraphElement("p"),
                // Treat pre elements like code blocks
                new CodeElement("pre"),
                new NonRenderedParentElement("progress"),
                new ConvertibleElement("q", "span", "QuoteChar"),
                new NonRenderedParentElement("rp"),
                new NonRenderedParentElement("rt"),
                new NonRenderedParentElement("ruby"),
                new NonRenderedParentElement("source"),
                new ConvertibleElement("strong", "span", "Bold"),
                new ConvertibleElement("sub", "span", "Subscript"),
                new ConvertibleElement("sup", "span", "Superscript"),
                new NonRenderedParentElement("svg"),
                new NonRenderedParentElement("tbody"),
                new TableCellElement("td"),
                new NonRenderedParentElement("tfoot"),
                new TableCellElement("th"),
                new NonRenderedParentElement("thead"),
                new NonRenderedParentElement("time"),
                new TableRowElement("tr"),
                new NonRenderedParentElement("track"),
                new ConvertibleElement("u", "span", "Underline"),
                // The Open XML file builder will convert lists accordingly
                new ConvertibleElement("ul", "ul", "bullet"),
                new NonRenderedParentElement("video"),
                new NonRenderedParentElement("wbr"),

                // Elements common to HTML, MAML, and/or XML comments.  Processing may differ based on the topic
                // type (API or MAML).
                new BibliographyElement(),
                new CiteElement(),
                new CodeElement("code"),
                new PassthroughElement("include"),
                new PassthroughElement("includeAttribute"),
                new MarkupElement(),
                new ParagraphElement("para"),
                new ListElement(),
                new ParametersElement(),
                new PassthroughElement("referenceLink"),
                // Pass spans through.  They'll be converted by the document builder task.
                new PassthroughElement("span"),
                new CodeElement("snippet"),
                new SummaryElement(),
                new TableElement(),

                // MAML elements
                new NoteElement("alert")
                {
                    CautionAlertTemplatePath = this.ResolvePath(@"Templates\CautionAlertTemplate.xml"),
                    LanguageAlertTemplatePath = this.ResolvePath(@"Templates\LanguageAlertTemplate.xml"),
                    NoteAlertTemplatePath = this.ResolvePath(@"Templates\NoteAlertTemplate.xml"),
                    SecurityAlertTemplatePath = this.ResolvePath(@"Templates\SecurityAlertTemplate.xml"),
                    ToDoAlertTemplatePath = this.ResolvePath(@"Templates\ToDoAlertTemplate.xml")
                },
                new ConvertibleElement("application", "span", "Bold"),
                new NamedSectionElement("appliesTo"),
                new AutoOutlineElement(),
                new NamedSectionElement("background"),
                new NamedSectionElement("buildInstructions"),
                new CodeEntityReferenceElement(),
                new CodeExampleElement(),
                new ConvertibleElement("codeFeaturedElement", "span", "Bold"),
                new ConvertibleElement("codeInline", "span", "CodeInline"),
                new NonRenderedParentElement("codeReference"),
                new ConvertibleElement("command", "span", "Command"),
                new ConvertibleElement("computerOutputInline", "span", "CodeInline"),
                new NonRenderedParentElement("conclusion"),
                new NonRenderedParentElement("content"),
                new CopyrightElement(),
                new NonRenderedParentElement("corporation"),
                new NonRenderedParentElement("country"),
                new ConvertibleElement("database", "span", "Bold"),
                new NonRenderedParentElement("date"),
                new DefinedTermElement("definedTerm"),
                new NonRenderedParentElement("definition"),
                new NonRenderedParentElement("definitionTable"),
                new NamedSectionElement("demonstrates"),
                new NonRenderedParentElement("description"),
                new NamedSectionElement("dotNetFrameworkEquivalent"),
                new ConvertibleElement("embeddedLabel", "span", "Bold"),
                new TableCellElement("entry"),
                new ConvertibleElement("environmentVariable", "span", "CodeInline"),
                new ConvertibleElement("errorInline", "span", "Emphasis"),
                new NamedSectionElement("exceptions"),
                new ExternalLinkElement(),
                new NamedSectionElement("externalResources"),
                new ConvertibleElement("fictitiousUri", "span", "Emphasis"),
                new ConvertibleElement("foreignPhrase", "span", "Emphasis"),
                new GlossaryElement(),
                new ConvertibleElement("hardware", "span", "Bold"),
                new NamedSectionElement("inThisSection"),
                new IntroductionElement(),
                new LanguageKeywordElement(),
                new NamedSectionElement("languageReferenceRemarks"),
                new NonRenderedParentElement("legacy"),
                new ConvertibleElement("legacyBold", "span", "Bold"),
                new ConvertibleElement("legacyItalic", "span", "Emphasis"),
                new LegacyLinkElement(),
                new ConvertibleElement("legacyUnderline", "span", "Underline"),
                new LineBreakElement("lineBreak"),
                new LinkElement(),
                new ConvertibleElement("listItem", "li", true),
                new ConvertibleElement("literal", "span", "Literal"),
                new ConvertibleElement("localUri", "span", "Emphasis"),
                new NonRenderedParentElement("localizedText"),
                new ConvertibleElement("math", "span", "Emphasis"),
                new MediaLinkElement(),
                new MediaLinkInlineElement(),
                new ConvertibleElement("newTerm", "span", "Emphasis"),
                new NamedSectionElement("nextSteps"),
                new ConvertibleElement("parameterReference", "span", "Emphasis"),
                new ConvertibleElement("phrase", "span", "Emphasis"),
                new ConvertibleElement("placeholder", "span", "Emphasis"),
                new NamedSectionElement("prerequisites"),
                new ProcedureElement(),
                new StyledParagraphElement("quote", "Quote"),
                new ConvertibleElement("quoteInline", "span", "QuoteChar"),
                new NamedSectionElement("reference"),
                new NamedSectionElement("relatedSections"),
                new RelatedTopicsElement(),
                new ConvertibleElement("replaceable", "span", "Emphasis"),
                new NamedSectionElement("requirements"),
                new NamedSectionElement("returnValue"),
                new NamedSectionElement("robustProgramming"),
                new TableRowElement("row"),
                new SectionElement(),
                new NonRenderedParentElement("sections"),
                new NamedSectionElement("security"),
                new NonRenderedParentElement("snippets"),
                new ConvertibleElement("step", "li", true),
                new StepsElement(),
                new ConvertibleElement("subscript", "span", "Subscript"),
                new ConvertibleElement("subscriptType", "span", "Subscript"),
                new ConvertibleElement("superscript", "span", "Superscript"),
                new ConvertibleElement("superscriptType", "span", "Superscript"),
                new ConvertibleElement("system", "span", "Bold"),
                new NonRenderedParentElement("tableHeader"),
                new NamedSectionElement("textValue"),
                // The title element is ignored.  The section and table elements handle them as needed.
                new IgnoredElement("title"),
                new NonRenderedParentElement("type"),
                new ConvertibleElement("ui", "span", "Bold"),
                new ConvertibleElement("unmanagedCodeEntityReference", "span", "Bold"),
                new ConvertibleElement("userInput", "span", "Bold"),
                new ConvertibleElement("userInputLocalizable", "span", "Bold"),
                new NamedSectionElement("whatsNew"),

                // XML comments and reflection data file elements
                new ConvertibleElement("c", "span", "CodeInline"),
                new PassthroughElement("conceptualLink"),
                new NamedSectionElement("example"),
                new ImplementsElement(),
                new NoteElement("note")
                {
                    CautionAlertTemplatePath = this.ResolvePath(@"Templates\CautionAlertTemplate.xml"),
                    LanguageAlertTemplatePath = this.ResolvePath(@"Templates\LanguageAlertTemplate.xml"),
                    NoteAlertTemplatePath = this.ResolvePath(@"Templates\NoteAlertTemplate.xml"),
                    SecurityAlertTemplatePath = this.ResolvePath(@"Templates\SecurityAlertTemplate.xml"),
                    ToDoAlertTemplatePath = this.ResolvePath(@"Templates\ToDoAlertTemplate.xml")
                },
                new ConvertibleElement("paramref", "name", "span", "Parameter"),
                new PreliminaryElement(),
                new NamedSectionElement("remarks"),
                new ReturnsElement(),
                new SeeElement(),
                // seeAlso should be a top-level element in the comments but may appear within other elements.
                // We'll ignore it if seen as they'll be handled manually by the See Also section processing.
                new IgnoredElement("seealso"),
                // For this presentation style, namespace/assembly info and inheritance hierarchy are part of
                // the definition (syntax) section.
                new SyntaxElement(nameof(BaseSourceCodeUrl))
                {
                    NamespaceAndAssemblyInfoRenderer = RenderApiNamespaceAndAssemblyInformation,
                    InheritanceHierarchyRenderer = RenderApiInheritanceHierarchy
                },
                new TemplatesElement(),
                new ThreadsafetyElement(),
                new ConvertibleElement("typeparamref", "name", "span", "Parameter"),
                new ValueElement(),
                new VersionsElement()
            });
        }

        /// <inheritdoc />
        protected override void CreateApiTopicSectionHandlers()
        {
            // API Topic sections will be rendered in this order by default
            this.AddApiTopicSectionHandlerRange(new[]
            {
                new ApiTopicSectionHandler(ApiTopicSectionType.Notices, t => RenderNotices(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Summary, t => RenderApiSummarySection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.SyntaxSection, t => RenderApiSyntaxSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Remarks, t => RenderApiRemarksSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Examples, t => RenderApiExamplesSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.MemberList, t => RenderApiMemberList(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Events,
                    t => RenderApiSectionTable(t, "title_events", t.CommentsNode.Elements("event"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.Exceptions,
                    t => RenderApiSectionTable(t, "title_exceptions", this.CommentsNode.Elements("exception"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.Versions, t => RenderApiVersionsSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Permissions,
                    t => RenderApiSectionTable(t, "title_permissions", t.CommentsNode.Elements("permission"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.ThreadSafety,
                    t => t.RenderNode(t.CommentsNode.Element("threadsafety"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.RevisionHistory,
                    t => RenderApiRevisionHistorySection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Bibliography,
                    t => RenderApiBibliographySection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.SeeAlso, t => RenderApiSeeAlsoSection(t))
            });
        }

        /// <inheritdoc />
        protected override XDocument RenderTopic()
        {
            if(pageTemplate == null)
                pageTemplate = LoadTemplateFile(this.TopicTemplatePath, null);

            var document = new XDocument(pageTemplate);

            XElement body = document.Root.Element(OpenXmlElement.WordProcessingML + "body");

            this.CurrentElement = body ?? throw new InvalidOperationException("Page template is missing the \"body\" element");

            if(!this.IsMamlTopic)
            {
                // This is used by the Save Component to get the filename.  It won't end up in the final result.
                document.Root.Add(new XElement("file",
                    new XAttribute("name", this.ReferenceNode.Element("file")?.Attribute("name")?.Value)));
            }

            OpenXmlElement.AddAddressBookmark(body, "Topic");

            // API member list pages are suppressed for this presentation style
            if(this.IsMamlTopic || this.ApiMember.ApiTopicGroup != ApiMemberGroup.List)
            {
                body.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "pStyle",
                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "Heading1"))),
                    new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "t",
                            this.IsMamlTopic ? this.MamlTopicTitle() : this.ApiTopicTitle(true, true)))));
            }

            this.OnRenderStarting(document);

            // Add the topic content.  MAML topics are rendered purely off of the element types.  API topics
            // require custom formatting based on the member type in the topic.
            if(this.IsMamlTopic)
                this.RenderNode(this.TopicNode);
            else
            {
                if(this.ApiMember.ApiTopicGroup != ApiMemberGroup.List)
                {
                    foreach(var section in this.ApiTopicSections)
                    {
                        section.RenderSection(this);
                        this.OnSectionRendered(section.SectionType, section.CustomSectionName);
                    }
                }
                else
                {
                    // Add an attribute to indicate that the page break should be suppressed for list topics.
                    // We still add the bookmark for the list topics so that links to overload lists still work
                    // and will effectively go to the first overloaded member in the set.
                    document.Root.Add(new XAttribute("suppressPageBreak", "true"));
                }
            }

            this.OnRenderCompleted(document);

            return document;
        }

        /// <summary>
        /// This is used to provide additional whitespace handling and normalization for Open XML text elements
        /// </summary>
        /// <param name="content">The content element to which the text is added</param>
        /// <param name="textNode">The text node to render</param>
        public override void RenderTextNode(XElement content, XText textNode)
        {
            if(content != null && textNode != null)
            {
                XElement t = new XElement(OpenXmlElement.WordProcessingML + "t",
                    new XAttribute(Element.XmlSpace, "preserve")),
                    run = new XElement(OpenXmlElement.WordProcessingML + "r", t);

                string text = textNode.Value;

                // If the content element has an xml:space attribute or the parent of the text node is in the
                // list of elements that should preserve space, just add the text as-is.  Otherwise,normalize the
                // whitespace.
                if(text.Length == 0 || content.Attribute(Element.XmlSpace) != null ||
                  spacePreservedElements.Contains(textNode.Parent.Name.LocalName) ||
                  ((textNode.Parent.Name.LocalName == "div" || textNode.Parent.Name.LocalName == "span") &&
                  textNode.Ancestors("syntax").Any()))
                {
                    t.Value = text;
                }
                else
                {
                    // If there is a preceding non-text sibling that isn't a line break and the text started with
                    // a whitespace, add a leading space.
                    if(Char.IsWhiteSpace(text[0]) && textNode.PreviousNode != null &&
                      !(textNode.PreviousNode is XText) && (!(textNode.PreviousNode is XElement pn) ||
                      pn.Name.LocalName != "lineBreak"))
                    {
                        t.Value = " ";
                    }

                    t.Value += text.NormalizeWhiteSpace();

                    // If there is a following non-text sibling and the text ended with a whitespace, add a
                    // trailing space.
                    if(Char.IsWhiteSpace(text[text.Length - 1]) && textNode.NextNode != null &&
                      !(textNode.NextNode is XText))
                    {
                        t.Value += " ";
                    }
                }

                content.Add(run);
            }
        }

        /// <inheritdoc />
        /// <remarks>The returned content element is always null and the content should be inserted into the
        /// transformation's current element after adding the title element.</remarks>
        public override (XElement Title, XElement Content) CreateSection(string uniqueId, bool localizedTitle,
          string title, string linkId)
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

                titleElement = new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "pStyle",
                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "Heading2"))),
                    titleContent);

                // Special case for the See Also section.  Use the unique ID as the link ID.
                if(uniqueId == "seeAlso")
                    linkId = uniqueId;

                if(!String.IsNullOrWhiteSpace(linkId))
                    OpenXmlElement.AddAddressBookmark(this.CurrentElement, linkId);
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

                titleElement = new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "pStyle",
                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "Heading4"))),
                    titleContent);
            }

            return (titleElement, null);
        }
        #endregion

        #region API topic section handlers
        //=====================================================================

        /// <summary>
        /// This is used to render the preliminary and obsolete API notices
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderNotices(TopicTransformationCore transformation)
        {
            var preliminary = transformation.CommentsNode.Element("preliminary");
            var obsolete = transformation.ReferenceNode.AttributeOfType("T:System.ObsoleteAttribute");

            if(preliminary != null || obsolete != null)
            {
                if(preliminary != null)
                    transformation.RenderNode(preliminary);

                if(obsolete != null)
                {
                    transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                        new XElement(OpenXmlElement.WordProcessingML + "r",
                            new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                new XElement(OpenXmlElement.WordProcessingML + "b")),
                            new XElement(OpenXmlElement.WordProcessingML + "t",
                                new XElement("include", new XAttribute("item", "boilerplate_obsoleteLong"))))));
                }
            }
        }

        /// <summary>
        /// This is used to render the summary section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiSummarySection(TopicTransformationCore transformation)
        {
            if(transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                transformation.RenderNode(transformation.CommentsNode.Element("summary"));
            else
            {
                // Render the summary from the first overloads element.  There should only be one.
                var overloads = transformation.ReferenceNode.Descendants("overloads").FirstOrDefault();

                if(overloads != null)
                {
                    var summary = overloads.Element("summary");

                    if(summary != null)
                        transformation.RenderNode(summary);
                    else
                        transformation.RenderChildElements(transformation.CurrentElement, overloads.Nodes());
                }
            }
        }

        /// <summary>
        /// Render the inheritance hierarchy section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="content">The content element to which the information is added</param>
        private static void RenderApiInheritanceHierarchy(TopicTransformationCore transformation, XElement content)
        {
            XElement row, para, family = transformation.ReferenceNode.Element("family"),
                implements = transformation.ReferenceNode.Element("implements");
            bool isFirst = true;

            if(family == null && implements == null)
                return;

            var table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                    new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                        new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTableNoBorder")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblW",
                        new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                        new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))),
                new XElement(OpenXmlElement.WordProcessingML + "tblGrid",
                    new XElement(OpenXmlElement.WordProcessingML + "gridCol",
                        new XAttribute(OpenXmlElement.WordProcessingML + "w", "1728")),
                    new XElement(OpenXmlElement.WordProcessingML + "gridCol",
                        new XAttribute(OpenXmlElement.WordProcessingML + "w", "7632"))));

            content.Add(table);
            content.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

            if(family != null)
            {
                XElement descendants = family.Element("descendents"), ancestors = family.Element("ancestors");

                para = new XElement(OpenXmlElement.WordProcessingML + "p");
                row = new XElement(OpenXmlElement.WordProcessingML + "tr",
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "tcPr",
                            new XElement(OpenXmlElement.WordProcessingML + "tcW",
                                new XAttribute(OpenXmlElement.WordProcessingML + "w", "1728"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "type", "dxa"))),
                        new XElement(OpenXmlElement.WordProcessingML + "p",
                            new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "b")),
                                new XElement(OpenXmlElement.WordProcessingML + "t",
                                    new XElement("include", new XAttribute("item", "text_inheritance")))))),
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "tcPr",
                            new XElement(OpenXmlElement.WordProcessingML + "tcW",
                                new XAttribute(OpenXmlElement.WordProcessingML + "w", "0"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "type", "auto"))),
                        para));

                table.Add(row);

                if(ancestors != null)
                {
                    // Ancestor types are stored nearest to most distant so reverse them
                    foreach(var typeInfo in ancestors.Elements().Reverse())
                    {
                        if(!isFirst)
                        {
                            para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "t",
                                    new XAttribute(Element.XmlSpace, "preserve"), "  \u2192  ")));
                        }

                        transformation.RenderTypeReferenceLink(para, typeInfo, false);
                        isFirst = false;
                    }

                    para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "t",
                                new XAttribute(Element.XmlSpace, "preserve"), "  \u2192  ")));
                }

                para.Add(new XElement("referenceLink",
                        new XAttribute("target", transformation.Key),
                        new XAttribute("show-container", false)));

                if(descendants != null)
                {
                    para = new XElement(OpenXmlElement.WordProcessingML + "p");
                    row = new XElement(OpenXmlElement.WordProcessingML + "tr",
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "tcPr",
                                new XElement(OpenXmlElement.WordProcessingML + "tcW",
                                    new XAttribute(OpenXmlElement.WordProcessingML + "w", "1728"),
                                    new XAttribute(OpenXmlElement.WordProcessingML + "type", "dxa"))),
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                        new XElement(OpenXmlElement.WordProcessingML + "b")),
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "text_derived")))))),
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "tcPr",
                                new XElement(OpenXmlElement.WordProcessingML + "tcW",
                                    new XAttribute(OpenXmlElement.WordProcessingML + "w", "0"),
                                    new XAttribute(OpenXmlElement.WordProcessingML + "type", "auto"))),
                            para));

                    table.Add(row);
                    isFirst = true;

                    foreach(var typeInfo in descendants.Elements().OrderBy(e => e.Attribute("api")?.Value))
                    {
                        if(!isFirst)
                        {
                            para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "br")));
                        }

                        transformation.RenderTypeReferenceLink(para, typeInfo, true);
                        isFirst = false;
                    }
                }
            }

            if(implements != null)
            {
                para = new XElement(OpenXmlElement.WordProcessingML + "p");
                row = new XElement(OpenXmlElement.WordProcessingML + "tr",
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "tcPr",
                            new XElement(OpenXmlElement.WordProcessingML + "tcW",
                                new XAttribute(OpenXmlElement.WordProcessingML + "w", "1728"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "type", "dxa"))),
                        new XElement(OpenXmlElement.WordProcessingML + "p",
                            new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "b")),
                                new XElement(OpenXmlElement.WordProcessingML + "t",
                                    new XElement("include", new XAttribute("item", "text_implements")))))),
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "tcPr",
                            new XElement(OpenXmlElement.WordProcessingML + "tcW",
                                new XAttribute(OpenXmlElement.WordProcessingML + "w", "0"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "type", "auto"))),
                        para));

                table.Add(row);
                isFirst = true;

                foreach(var typeInfo in implements.Elements().OrderBy(e => e.Attribute("api")?.Value))
                {
                    if(!isFirst)
                    {
                        para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                            new XElement(OpenXmlElement.WordProcessingML + "t",
                                new XAttribute(Element.XmlSpace, "preserve"), ", ")));
                    }

                    transformation.RenderTypeReferenceLink(para, typeInfo, false);
                    isFirst = false;
                }
            }
        }

        /// <summary>
        /// This is used to render namespace and assembly information for an API topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="content">The content element to which the information is added</param>
        private static void RenderApiNamespaceAndAssemblyInformation(TopicTransformationCore transformation,
          XElement content)
        {
            // Only API member pages get namespace/assembly info
            if(transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.List ||
               transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.RootGroup ||
               transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Root ||
               transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.NamespaceGroup ||
               transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Namespace)
            {
                return;
            }

            var containers = transformation.ReferenceNode.Element("containers");
            var libraries = containers.Elements("library");
            var para = new XElement(OpenXmlElement.WordProcessingML + "p");

            content.Add(para);

            para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                new XElement(OpenXmlElement.WordProcessingML + "rPr",
                    new XElement(OpenXmlElement.WordProcessingML + "b")),
                new XElement(OpenXmlElement.WordProcessingML + "t",
                    new XElement("include", new XAttribute("item", "boilerplate_requirementsNamespace")))),
                new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t", Element.NonBreakingSpace)),
                new XElement("referenceLink",
                    new XAttribute("target", containers.Element("namespace").Attribute("api").Value)),
                new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "br")));
            int separatorSize = 1;
            bool first = true;

            if(libraries.Count() > 1)
            {
                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                        new XElement(OpenXmlElement.WordProcessingML + "b")),
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        new XElement("include", new XAttribute("item", "boilerplate_requirementsAssemblies")))));
                separatorSize = 2;
            }
            else
            {
                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                        new XElement(OpenXmlElement.WordProcessingML + "b")),
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        new XElement("include", new XAttribute("item", "boilerplate_requirementsAssemblyLabel")))));
            }

            string separator = new String(Element.NonBreakingSpace, separatorSize);
            int maxVersionParts = ((OpenXmlTransformation)transformation).MaxVersionParts;

            foreach(var l in libraries)
            {
                if(!first)
                {
                    para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "br")));
                }

                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t", separator)));

                string version = l.Element("assemblydata").Attribute("version").Value,
                    extension = l.Attribute("kind").Value.Equals(
                        "DynamicallyLinkedLibrary", StringComparison.Ordinal) ? "dll" : "exe";
                string[] versionParts = version.Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);

                // Limit the version number parts if requested
                if(maxVersionParts > 1 && maxVersionParts < 5)
                    version = String.Join(".", versionParts, 0, maxVersionParts);

                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                    new XElement(OpenXmlElement.WordProcessingML + "t",
                        new XElement("include", new XAttribute("item", "assemblyNameAndModule"),
                            new XElement("parameter", l.Attribute("assembly").Value),
                            new XElement("parameter", l.Attribute("module").Value),
                            new XElement("parameter", extension),
                            new XElement("parameter", version)))));

                first = false;
            }

            // Show XAML XML namespaces for APIs that support XAML.  All topics that have auto-generated XAML
            // syntax get an "XMLNS for XAML" line in the Requirements section.  Topics with boilerplate XAML
            // syntax, e.g. "Not applicable", do NOT get this line.
            var xamlCode = transformation.SyntaxNode.Elements("div").Where(d => d.Attribute("codeLanguage")?.Value.Equals(
                "XAML", StringComparison.Ordinal) ?? false);

            if(xamlCode.Any())
            {
                var xamlXmlNS = xamlCode.Elements("div").Where(d => d.Attribute("class")?.Value == "xamlXmlnsUri");

                para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "br")),
                    new XElement(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "rPr",
                            new XElement(OpenXmlElement.WordProcessingML + "b")),
                        new XElement(OpenXmlElement.WordProcessingML + "t",
                                new XElement("include", new XAttribute("item", "boilerplate_xamlXmlnsRequirements"))))),
                    new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "t", Element.NonBreakingSpace)));

                if(xamlXmlNS.Any())
                {
                    first = true;

                    foreach(var d in xamlXmlNS)
                    {
                        if(!first)
                        {
                            content.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "t", ", ")));
                        }

                        para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                            new XElement(OpenXmlElement.WordProcessingML + "t", d.Value.NormalizeWhiteSpace())));
                        first = false;
                    }
                }
                else
                {
                    para.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "t",
                            new XElement("include", new XAttribute("item", "boilerplate_unmappedXamlXmlns")))));
                }
            }
        }

        /// <summary>
        /// This is used to render the syntax section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiSyntaxSection(TopicTransformationCore transformation)
        {
            // Only API member pages get a syntax section
            if(transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.List &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.RootGroup &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.Root &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.NamespaceGroup &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.Namespace)
            {
                transformation.RenderNode(transformation.SyntaxNode);
            }
        }

        /// <summary>
        /// This is used to render a member list topic (root, root group, namespace group, namespace, enumeration,
        /// type, or type member list).
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiMemberList(TopicTransformationCore transformation)
        {
            switch(transformation.ApiMember)
            {
                case var t when t.ApiTopicGroup == ApiMemberGroup.RootGroup || t.ApiTopicGroup == ApiMemberGroup.Root:
                    RenderApiRootList(transformation);
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.NamespaceGroup:
                    RenderApiNamespaceGroupList(transformation);
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Namespace:
                    RenderApiNamespaceList(transformation);
                    break;

                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Enumeration:
                    RenderApiEnumerationMembersList(transformation);
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Type:
                    RenderApiTypeMemberLists(transformation);
                    break;

                default:
                    // Member list pages (t.ApiTopicGroup == ApiMemberGroup.List) are not rendered by this
                    // presentation style.
                    break;
            }
        }

        /// <summary>
        /// Render the list in a root group or root topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiRootList(TopicTransformationCore transformation)
        {
            var elements = transformation.ReferenceNode.Element("elements")?.Elements("element").OrderBy(
                e => e.Element("apidata").Attribute("name").Value).ToList();

            if((elements?.Count ?? 0) == 0)
                return;

            var (title, _) = transformation.CreateSection(elements[0].GenerateUniqueId(), true, "title_namespaces", null);

            transformation.CurrentElement.Add(title);

            XElement table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                    new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                        new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblW",
                        new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                        new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))));

            transformation.CurrentElement.Add(table);
            transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

            foreach(var e in elements)
            {
                string name = e.Element("apidata").Attribute("name").Value;
                var refLink = new XElement("referenceLink",
                    new XAttribute("target", e.Attribute("api").Value),
                    new XAttribute("qualified", "false"));
                var summaryCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                if(name.Length == 0)
                    refLink.Add(new XElement("include", new XAttribute("item", "defaultNamespace")));

                table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "p", refLink)),
                    summaryCell));

                var summary = e.Element("summary");

                if(summary != null)
                    transformation.RenderChildElements(summaryCell, summary.Nodes());
            }
        }

        /// <summary>
        /// Render the list in a namespace group topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiNamespaceGroupList(TopicTransformationCore transformation)
        {
            var elements = transformation.ReferenceNode.Element("elements")?.Elements("element").OrderBy(e =>
            {
                string name = e.Attribute("api").Value;
                return name.Substring(name.IndexOf(':') + 1);
            }).ToList();

            if((elements?.Count ?? 0) == 0)
                return;

            var (title, _) = transformation.CreateSection(elements[0].GenerateUniqueId(), true,
                "tableTitle_namespace", null);

            transformation.CurrentElement.Add(title);

            XElement table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                    new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                        new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblW",
                        new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                        new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))));

            transformation.CurrentElement.Add(table);
            transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

            foreach(var e in elements)
            {
                var summaryCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "p",
                            new XElement("referenceLink",
                                new XAttribute("target", e.Attribute("api").Value),
                                new XAttribute("qualified", "false")))),
                    summaryCell));

                var summary = e.Element("summary");

                if(summary != null)
                    transformation.RenderChildElements(summaryCell, summary.Nodes());
            }
        }

        /// <summary>
        /// Render the category lists in a namespace topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiNamespaceList(TopicTransformationCore transformation)
        {
            var elements = transformation.ReferenceNode.Element("elements").Elements("element").GroupBy(
                e => e.Element("apidata").Attribute("subgroup").Value).ToDictionary(k => k.Key, v => v);

            foreach(string key in new[] { "class", "structure", "interface", "delegate", "enumeration" })
            {
                if(elements.TryGetValue(key, out var group))
                {
                    var (title, _) = transformation.CreateSection(group.First().GenerateUniqueId(), true,
                        "tableTitle_" + key, null);

                    transformation.CurrentElement.Add(title);

                    XElement table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                        new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                            new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                                new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                            new XElement(OpenXmlElement.WordProcessingML + "tblW",
                                new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                            new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                                new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                                new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))));

                    transformation.CurrentElement.Add(table);
                    transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                        new XElement(OpenXmlElement.WordProcessingML + "pPr",
                            new XElement(OpenXmlElement.WordProcessingML + "spacing",
                                new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

                    foreach(var e in group.OrderBy(el => el.Attribute("api").Value))
                    {
                        var summaryCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                        table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                            new XElement(OpenXmlElement.WordProcessingML + "tc",
                                new XElement(OpenXmlElement.WordProcessingML + "p",
                                    new XElement("referenceLink",
                                        new XAttribute("target", e.Attribute("api").Value),
                                        new XAttribute("qualified", "false")))),
                            summaryCell));

                        var summary = e.Element("summary");

                        if(summary != null)
                            transformation.RenderChildElements(summaryCell, summary.Nodes());

                        var obsoleteAttr = e.AttributeOfType("T:System.ObsoleteAttribute");
                        var prelimComment = e.Element("preliminary");

                        if(obsoleteAttr != null || prelimComment != null)
                        {
                            if(!summaryCell.IsEmpty)
                            {
                                summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "br")));
                            }

                            if(obsoleteAttr != null)
                            {
                                summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                        new XElement(OpenXmlElement.WordProcessingML + "b")),
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "boilerplate_obsoleteShort")))));
                            }

                            if(prelimComment != null)
                            {
                                if(obsoleteAttr != null)
                                {
                                    summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                        new XElement(OpenXmlElement.WordProcessingML + "t",
                                            new XAttribute(Element.XmlSpace, "preserve"), "    ")));
                                }

                                summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                        new XElement(OpenXmlElement.WordProcessingML + "i")),
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "preliminaryShort")))));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render the members of an enumeration
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiEnumerationMembersList(TopicTransformationCore transformation)
        {
            // Convert to this type so that we can access the argument shortcuts easily
            var thisTransform = (OpenXmlTransformation)transformation;

            // Sort order is configurable for enumeration members
            EnumMemberSortOrder enumMemberSortOrder = thisTransform.EnumMemberSortOrder;

            var elements = thisTransform.ReferenceNode.Element("elements")?.Elements("element").OrderBy(
                el => enumMemberSortOrder == EnumMemberSortOrder.Name ?
                    el.Element("apidata").Attribute("name").Value :
                    el.Element("value").Value.PadLeft(20, ' ')).ToList();

            if((elements?.Count ?? 0) == 0)
                return;

            var enumValues = elements.Select(e => e.Element("value").Value).ToList();
            bool includeEnumValues = thisTransform.IncludeEnumValues;
            int idx;

            if(includeEnumValues)
            {
                EnumValueFormat enumFormat = thisTransform.FlagsEnumValueFormat;
                int groupSize = 0, minWidth = 0;
                bool signedValues = enumValues.Any(v => v.Length > 0 && v[0] == '-');

                if(enumFormat != EnumValueFormat.IntegerValue &&
                  thisTransform.ReferenceNode.AttributeOfType("T:System.FlagsAttribute") != null)
                {
                    groupSize = thisTransform.FlagsEnumSeparatorSize;

                    if(groupSize != 0 && groupSize != 4 && groupSize != 8)
                        groupSize = 0;

                    // Determine the minimum width of the values
                    if(signedValues)
                    {
                        minWidth = enumValues.Select(v => TopicTransformationExtensions.FormatSignedEnumValue(v,
                            enumFormat, 0, 0)).Max(v => v.Length) - 2;
                    }
                    else
                    {
                        minWidth = enumValues.Select(v => TopicTransformationExtensions.FormatUnsignedEnumValue(v,
                            enumFormat, 0, 0)).Max(v => v.Length) - 2;
                    }

                    if(minWidth < 3)
                        minWidth = 2;
                    else
                    {
                        if((minWidth % 4) != 0)
                            minWidth += 4 - (minWidth % 4);
                    }
                }
                else
                    enumFormat = EnumValueFormat.IntegerValue;   // Enforce integer format for non-flags enums

                for(idx = 0; idx < enumValues.Count; idx++)
                {
                    if(signedValues)
                    {
                        enumValues[idx] = TopicTransformationExtensions.FormatSignedEnumValue(enumValues[idx],
                            enumFormat, minWidth, groupSize);
                    }
                    else
                    {
                        enumValues[idx] = TopicTransformationExtensions.FormatUnsignedEnumValue(enumValues[idx],
                            enumFormat, minWidth, groupSize);
                    }
                }
            }

            var (title, _) = thisTransform.CreateSection(elements.First().GenerateUniqueId(), true,
                "topicTitle_enumMembers", null);

            thisTransform.CurrentElement.Add(title);

            XElement table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                    new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                        new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblW",
                        new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                    new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                        new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                        new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))));

            transformation.CurrentElement.Add(table);
            transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

            idx = 0;

            foreach(var e in elements)
            {
                var summaryCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                XElement valueCell = null;

                if(includeEnumValues)
                {
                    valueCell = new XElement(OpenXmlElement.WordProcessingML + "tc", enumValues[idx]);
                    idx++;
                }

                table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                    new XElement(OpenXmlElement.WordProcessingML + "tc",
                        new XElement(OpenXmlElement.WordProcessingML + "p",
                            new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "t",
                                    e.Element("apidata").Attribute("name").Value)))),
                    valueCell,
                    summaryCell));

                var summary = e.Element("summary");
                var remarks = e.Element("remarks");

                if(summary != null || remarks != null)
                {
                    if(summary != null)
                        thisTransform.RenderChildElements(summaryCell, summary.Nodes());

                    // Enum members may have additional authored content in the remarks node
                    if(remarks != null)
                        thisTransform.RenderChildElements(summaryCell, remarks.Nodes());
                }

                if(e.AttributeOfType("T:System.ObsoleteAttribute") != null)
                {
                    if(!summaryCell.IsEmpty)
                    {
                        summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                            new XElement(OpenXmlElement.WordProcessingML + "br")));
                    }

                    summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                        new XElement(OpenXmlElement.WordProcessingML + "t",
                            new XAttribute(OpenXmlElement.XmlSpace, "preserve"), "    ")));
                }
            }
        }

        /// <summary>
        /// Render type member lists
        /// </summary>
        /// <remarks>This is used for types and the member list subtopics</remarks>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiTypeMemberLists(TopicTransformationCore transformation)
        {
            var allMembers = transformation.ReferenceNode.Element("elements")?.Elements("element").ToList();

            if((allMembers?.Count ?? 0) == 0)
                return;

            var overloads = allMembers.Where(e => e.Attribute("api").Value.StartsWith("Overload:",
                StringComparison.Ordinal)).ToList();

            // Remove overload topics and add their members to the full member list
            foreach(var overload in overloads)
            {
                allMembers.Remove(overload);
                allMembers.AddRange(overload.Elements("element"));
            }

            var memberGroups = new Dictionary<ApiMemberGroup, List<XElement>>
            {
                { ApiMemberGroup.Constructor, new List<XElement>() },
                { ApiMemberGroup.Property, new List<XElement>() },
                { ApiMemberGroup.Method, new List<XElement>() },
                { ApiMemberGroup.Event, new List<XElement>() },
                { ApiMemberGroup.Operator, new List<XElement>() },
                { ApiMemberGroup.Field, new List<XElement>() },
                { ApiMemberGroup.AttachedProperty, new List<XElement>() },
                { ApiMemberGroup.AttachedEvent, new List<XElement>() },
                { ApiMemberGroup.Extension, new List<XElement>() },
                { ApiMemberGroup.ExplicitInterfaceImplementation, new List<XElement>() },
                // Only used for overloads topics.  It won't appear on normal list pages.
                { ApiMemberGroup.Overload, new List<XElement>() },
            };

            if(transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
            {
                // Group the members by section type
                foreach(var m in allMembers)
                {
                    XElement apiData = m.Element("apidata"), memberData = m.Element("memberdata"),
                        procedureData = m.Element("proceduredata");

                    // Some members such as inherited interface members on a derived interface, contain no
                    // metadata and we'll ignore them.
                    if(apiData == null)
                        continue;

                    if(!Enum.TryParse<ApiMemberGroup>(apiData.Attribute("subgroup")?.Value, true, out var subgroup))
                        subgroup = ApiMemberGroup.Unknown;

                    if(!Enum.TryParse<ApiMemberGroup>(apiData.Attribute("subsubgroup")?.Value, true, out var subsubgroup))
                        subsubgroup = ApiMemberGroup.Unknown;

                    switch(m)
                    {
                        // The order of checks is important here and doesn't match the order of the rendered
                        // sections.  It minimizes the conditions we need to check in each subsequent case.
                        case var mbr when procedureData?.Attribute("eii")?.Value == "true":
                            memberGroups[ApiMemberGroup.ExplicitInterfaceImplementation].Add(mbr);
                            break;

                        case var mbr when subgroup == ApiMemberGroup.Constructor:
                            memberGroups[ApiMemberGroup.Constructor].Add(mbr);
                            break;

                        case var mbr when subgroup == ApiMemberGroup.Property && subsubgroup == ApiMemberGroup.Unknown:
                            memberGroups[ApiMemberGroup.Property].Add(mbr);
                            break;

                        case var mbr when subgroup == ApiMemberGroup.Method && subsubgroup == ApiMemberGroup.Unknown:
                            memberGroups[ApiMemberGroup.Method].Add(mbr);
                            break;

                        case var mbr when subgroup == ApiMemberGroup.Event && subsubgroup == ApiMemberGroup.Unknown:
                            memberGroups[ApiMemberGroup.Event].Add(mbr);
                            break;

                        case var mbr when subsubgroup == ApiMemberGroup.Operator:
                            memberGroups[ApiMemberGroup.Operator].Add(mbr);
                            break;

                        case var mbr when subgroup == ApiMemberGroup.Field:
                            memberGroups[ApiMemberGroup.Field].Add(mbr);
                            break;

                        case var mbr when subsubgroup == ApiMemberGroup.AttachedProperty:
                            memberGroups[ApiMemberGroup.AttachedProperty].Add(mbr);
                            break;

                        case var mbr when subsubgroup == ApiMemberGroup.AttachedEvent:
                            memberGroups[ApiMemberGroup.AttachedEvent].Add(mbr);
                            break;

                        case var mbr when subsubgroup == ApiMemberGroup.Extension:
                            memberGroups[ApiMemberGroup.Extension].Add(mbr);
                            break;

                        default:
                            // We shouldn't get here, but just in case...
                            Debug.WriteLine("Unhandled member type Subgroup: {0} Sub-subgroup: {1}", subgroup, subsubgroup);

                            if(Debugger.IsAttached)
                                Debugger.Break();
                            break;
                    }
                }
            }
            else
                memberGroups[ApiMemberGroup.Overload].AddRange(allMembers);

            // Render each section with at least one member
            foreach(var memberType in new[] { ApiMemberGroup.Constructor, ApiMemberGroup.Property,
                ApiMemberGroup.Method, ApiMemberGroup.Event, ApiMemberGroup.Operator, ApiMemberGroup.Field,
                ApiMemberGroup.AttachedProperty, ApiMemberGroup.AttachedEvent, ApiMemberGroup.Extension,
                ApiMemberGroup.ExplicitInterfaceImplementation, ApiMemberGroup.Overload })
            {
                var members = memberGroups[memberType];

                if(members.Count == 0)
                    continue;

                var (title, _) = transformation.CreateSection(members.First().GenerateUniqueId(), true,
                    "tableTitle_" + memberType.ToString(), null);

                transformation.CurrentElement.Add(title);

                XElement table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                    new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                        new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                        new XElement(OpenXmlElement.WordProcessingML + "tblW",
                            new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                        new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                            new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))));

                transformation.CurrentElement.Add(table);
                transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "spacing",
                            new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

                // Sort by EII name if present else the member name and then by template count
                foreach(var e in members.OrderBy(el => el.Element("topicdata")?.Attribute("eiiName")?.Value ??
                    el.Element("apidata")?.Attribute("name").Value ?? String.Empty).ThenBy(
                    el => el.Element("templates")?.Elements()?.Count() ?? 0))
                {
                    XElement referenceLink = new XElement("referenceLink",
                            new XAttribute("target", e.Attribute("api").Value));
                    string showParameters = (transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload &&
                        e.Element("memberdata").Attribute("overload") == null &&
                        !(e.Parent.Attribute("api")?.Value ?? String.Empty).StartsWith(
                            "Overload:", StringComparison.Ordinal)) ? "false" : "true";
                    bool isExtensionMethod = e.AttributeOfType("T:System.Runtime.CompilerServices.ExtensionAttribute") != null;

                    var summaryCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                    switch(memberType)
                    {
                        case var t when t == ApiMemberGroup.Operator &&
                          (e.Element("apidata")?.Attribute("name")?.Value == "Explicit" ||
                          e.Element("apidata")?.Attribute("name")?.Value == "Implicit"):
                            referenceLink.Add(new XAttribute("show-parameters", "true"));
                            break;

                        case var t when t == ApiMemberGroup.Operator:
                            break;

                        case var t when t == ApiMemberGroup.Extension:
                            var extensionMethod = new XElement("extensionMethod");

                            foreach(var attr in e.Attributes())
                                extensionMethod.Add(new XAttribute(attr));

                            foreach(var typeEl in new[] { e.Element("apidata"), e.Element("templates"),
                              e.Element("parameters"), e.Element("containers") })
                            {
                                if(typeEl != null)
                                    extensionMethod.Add(new XElement(typeEl));
                            }

                            referenceLink.Add(new XAttribute("display-target", "extension"),
                                new XAttribute("show-parameters", showParameters), extensionMethod);
                            break;

                        default:
                            referenceLink.Add(new XAttribute("show-parameters", showParameters));
                            break;
                    }

                    table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML +  "p", referenceLink)),
                        summaryCell));

                    var summary = e.Element("summary");

                    if(summary != null)
                        transformation.RenderChildElements(summaryCell, summary.Nodes());

                    if(transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                    {
                        if(memberType == ApiMemberGroup.Extension)
                        {
                            var parameter = new XElement("parameter");

                            summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "br")),
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XAttribute(Element.XmlSpace, "preserve"),
                                        new XElement("include", new XAttribute("item", "definedBy"), parameter))));

                            transformation.RenderTypeReferenceLink(parameter, e.Element("containers").Element("type"), false);
                        }
                        else
                        {
                            if(transformation.ApiMember.TypeTopicId != e.Element("containers").Element("type").Attribute("api").Value)
                            {
                                var parameter = new XElement("parameter");

                                summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                        new XElement(OpenXmlElement.WordProcessingML + "br")),
                                    new XElement(OpenXmlElement.WordProcessingML + "r",
                                        new XElement(OpenXmlElement.WordProcessingML + "t",
                                            new XAttribute(Element.XmlSpace, "preserve"),
                                            new XElement("include", new XAttribute("item", "inheritedFrom"), parameter))));

                                transformation.RenderTypeReferenceLink(parameter, e.Element("containers").Element("type"), false);
                            }
                            else
                            {
                                if(e.Element("overrides")?.Element("member") != null)
                                {
                                    var parameter = new XElement("parameter");

                                    summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                            new XElement(OpenXmlElement.WordProcessingML + "br")),
                                        new XElement(OpenXmlElement.WordProcessingML + "r",
                                            new XElement(OpenXmlElement.WordProcessingML + "t",
                                                new XAttribute(Element.XmlSpace, "preserve"),
                                                new XElement("include", new XAttribute("item", "overridesMember"), parameter))));

                                    transformation.RenderTypeReferenceLink(parameter, e.Element("overrides").Element("member"), true);
                                }
                            }
                        }
                    }

                    var obsoleteAttr = e.AttributeOfType("T:System.ObsoleteAttribute");
                    var prelimComment = e.Element("preliminary");

                    if(obsoleteAttr != null || prelimComment != null)
                    {
                        if(!summaryCell.IsEmpty)
                        {
                            summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "br")));
                        }

                        if(obsoleteAttr != null)
                        {
                            summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "b")),
                                new XElement(OpenXmlElement.WordProcessingML + "t",
                                    new XElement("include", new XAttribute("item", "boilerplate_obsoleteShort")))));
                        }

                        if(prelimComment != null)
                        {
                            if(obsoleteAttr != null)
                            {
                                summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XAttribute(Element.XmlSpace, "preserve"), "    ")));
                            }

                            summaryCell.Add(new XElement(OpenXmlElement.WordProcessingML + "r",
                                new XElement(OpenXmlElement.WordProcessingML + "rPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "i")),
                                new XElement(OpenXmlElement.WordProcessingML + "t",
                                    new XElement("include", new XAttribute("item", "preliminaryShort")))));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render a section with a title and a table containing the element content
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="sectionTitleItem">The section title include item</param>
        /// <param name="sectionElements">An enumerable list of the elements to render in the table</param>
        private static void RenderApiSectionTable(TopicTransformationCore transformation, string sectionTitleItem,
          IEnumerable<XElement> sectionElements)
        {
            if(sectionElements.Any())
            {
                var (title, _) = transformation.CreateSection(sectionElements.First().GenerateUniqueId(), true,
                    sectionTitleItem, null);

                transformation.CurrentElement.Add(title);

                var table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                    new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                        new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                        new XElement(OpenXmlElement.WordProcessingML + "tblW",
                            new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct")),
                        new XElement(OpenXmlElement.WordProcessingML + "tblLook",
                            new XAttribute(OpenXmlElement.WordProcessingML + "firstRow", "0"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "noHBand", "1"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "noVBand", "1"))));

                transformation.CurrentElement.Add(table);
                transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "spacing",
                            new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

                foreach(var se in sectionElements)
                {
                    var descCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                    table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement("referenceLink",
                                    new XAttribute("target", se.Attribute("cref")?.Value ?? String.Empty),
                                    new XAttribute("qualified", "false")))),
                        descCell));

                    transformation.RenderChildElements(descCell, se.Nodes());
                }
            }
        }

        /// <summary>
        /// This is used to render the remarks section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiRemarksSection(TopicTransformationCore transformation)
        {
            // For overloads, render remarks from the first overloads element.  There should only be one.
            if(transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                transformation.RenderNode(transformation.CommentsNode.Element("remarks"));
            else
            {
                var overloads = transformation.ReferenceNode.Descendants("overloads").FirstOrDefault();

                if(overloads != null)
                    transformation.RenderNode(overloads.Element("remarks"));
            }
        }

        /// <summary>
        /// This is used to render the examples section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiExamplesSection(TopicTransformationCore transformation)
        {
            // For overloads, render examples from the overloads element.  There should only be one.
            if(transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                transformation.RenderNode(transformation.CommentsNode.Element("example"));
            else
            {
                var overloads = transformation.ReferenceNode.Descendants("overloads").FirstOrDefault();

                if(overloads != null)
                    transformation.RenderNode(overloads.Element("example"));
            }
        }

        /// <summary>
        /// This is used to render the versions section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiVersionsSection(TopicTransformationCore transformation)
        {
            // Only API member pages get version information
            if(transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.List &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.RootGroup &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.Root &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.NamespaceGroup &&
               transformation.ApiMember.ApiTopicGroup != ApiMemberGroup.Namespace)
            {
                foreach(var v in transformation.ReferenceNode.Elements("versions"))
                    transformation.RenderNode(v);
            }
        }

        /// <summary>
        /// Render the revision history section if applicable
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiRevisionHistorySection(TopicTransformationCore transformation)
        {
            var revisionHistory = transformation.CommentsNode.Element("revisionHistory");

            if(revisionHistory == null || revisionHistory.Attribute("visible")?.Value == "false")
                return;

            var revisions = revisionHistory.Elements("revision").Where(
                h => h.Attribute("visible")?.Value != "false");

            if(revisions.Any())
            {
                var (title, _) = transformation.CreateSection(revisionHistory.GenerateUniqueId(), true,
                    "title_revisionHistory", null);

                transformation.CurrentElement.Add(title);

                var table = new XElement(OpenXmlElement.WordProcessingML + "tbl",
                    new XElement(OpenXmlElement.WordProcessingML + "tblPr",
                        new XElement(OpenXmlElement.WordProcessingML + "tblStyle",
                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "GeneralTable")),
                        new XElement(OpenXmlElement.WordProcessingML + "tblW",
                            new XAttribute(OpenXmlElement.WordProcessingML + "w", "5000"),
                            new XAttribute(OpenXmlElement.WordProcessingML + "type", "pct"))),
                    new XElement(OpenXmlElement.WordProcessingML + "tr",
                        new XElement(OpenXmlElement.WordProcessingML + "trPr",
                            new XElement(OpenXmlElement.WordProcessingML + "cnfStyle",
                                new XAttribute(OpenXmlElement.WordProcessingML + "val", "100000000000"))),
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "keepNext")),
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "header_revHistoryDate")))))),
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "keepNext")),
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "header_revHistoryVersion")))))),
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "keepNext")),
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                        new XElement("include", new XAttribute("item", "header_revHistoryDescription"))))))));

                transformation.CurrentElement.Add(table);
                transformation.CurrentElement.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "spacing",
                            new XAttribute(OpenXmlElement.WordProcessingML + "after", "0")))));

                foreach(var rh in revisions)
                {
                    var descCell = new XElement(OpenXmlElement.WordProcessingML + "tc");

                    table.Add(new XElement(OpenXmlElement.WordProcessingML + "tr",
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                    rh.Attribute("date")?.Value)))),
                        new XElement(OpenXmlElement.WordProcessingML + "tc",
                            new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "r",
                                    new XElement(OpenXmlElement.WordProcessingML + "t",
                                    rh.Attribute("version")?.Value)))),
                        descCell));

                    transformation.RenderChildElements(descCell, rh.Nodes());
                }
            }
        }

        /// <summary>
        /// Render the bibliography section if applicable
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiBibliographySection(TopicTransformationCore transformation)
        {
            if(transformation.ElementHandlerFor("bibliography") is BibliographyElement b)
            {
                if(b.DetermineCitations(transformation).Count != 0)
                {
                    // Use the first citation element as the element for rendering.  It's only needed to create
                    // a unique ID for the section.
                    var cite = transformation.DocumentNode.Descendants("cite").First();

                    b.Render(transformation, cite);
                }
            }
        }

        /// <summary>
        /// This renders the See Also section if applicable
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiSeeAlsoSection(TopicTransformationCore transformation)
        {
            // Render the see and seealso links using the see element handler as the processing is the same
            Element seeHandler = transformation.ElementHandlerFor("see"),
                conceptualLinkHandler = transformation.ElementHandlerFor("conceptualLink");

            // Get see also elements from comments excluding those in overloads comments
            List<XElement> seeAlsoNotInOverloads = transformation.CommentsNode.Descendants("seealso").Where(
                    s => !s.Ancestors("overloads").Any()).ToList(),
                seeAlsoHRef = seeAlsoNotInOverloads.Where(s => s.Attribute("href") != null).ToList(),
                seeAlsoCRef = seeAlsoNotInOverloads.Except(seeAlsoHRef).ToList();

            // Combine those with see also elements from element overloads comments
            var elements = transformation.ReferenceNode.Element("elements") ?? new XElement("elements");
            var elementOverloads = elements.Elements("element").SelectMany(e => e.Descendants("overloads")).ToList();
            
            seeAlsoHRef.AddRange(elementOverloads.Descendants("seealso").Where(s => s.Attribute("href") != null));
            seeAlsoCRef.AddRange(elementOverloads.Descendants("seealso").Where(s => s.Attribute("href") == null));

            // Get unique conceptual links from comments excluding those in overloads comments and combine them
            // with those in element overloads comments.
            var conceptualLinks = transformation.CommentsNode.Descendants("conceptualLink").Where(
                s => !s.Ancestors("overloads").Any()).Concat(
                    elementOverloads.Descendants("conceptualLink")).GroupBy(
                        c => c.Attribute("target")?.Value ?? String.Empty).Where(g => g.Key.Length != 0).Select(
                        g => g.First()).ToList();

            if(seeAlsoCRef.Count != 0 || seeAlsoHRef.Count != 0 || conceptualLinks.Count != 0 ||
              transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Type ||
              transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
              transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
            {
                // This has a fixed ID that matches the one used in MAML topics for the related topics section
                var (title, _) = transformation.CreateSection("seeAlso", true, "title_relatedTopics", null);

                transformation.CurrentElement.Add(title);

                var priorCurrentElement = transformation.CurrentElement;

                if(seeAlsoCRef.Count != 0 || transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Type ||
                  transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
                  transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
                {
                    var (subtitle, subsection) = transformation.CreateSubsection(true, "title_seeAlso_reference");

                    if(subtitle != null)
                        transformation.CurrentElement.Add(subtitle);

                    if(subsection != null)
                        transformation.CurrentElement.Add(subsection);
                    else
                        subsection = transformation.CurrentElement;

                    RenderApiAutoGeneratedSeeAlsoLinks(transformation, subsection);

                    if(seeHandler != null)
                    {
                        foreach(var s in seeAlsoCRef)
                        {
                            var para = new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0"))));
                            subsection.Add(para);

                            transformation.CurrentElement = para;
                            seeHandler.Render(transformation, s);
                        }

                        transformation.CurrentElement = priorCurrentElement;
                    }
                }

                if((seeAlsoHRef.Count != 0 && seeHandler != null) || (conceptualLinks.Count != 0 &&
                  conceptualLinkHandler != null))
                {
                    var (subtitle, subsection) = transformation.CreateSubsection(true, "title_seeAlso_otherResources");

                    if(subtitle != null)
                        transformation.CurrentElement.Add(subtitle);

                    if(subsection != null)
                        transformation.CurrentElement.Add(subsection);
                    else
                        subsection = transformation.CurrentElement;

                    if(seeHandler != null)
                    {
                        foreach(var s in seeAlsoHRef)
                        {
                            var para = new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0"))));
                            subsection.Add(para);

                            transformation.CurrentElement = para;
                            seeHandler.Render(transformation, s);
                        }

                        transformation.CurrentElement = priorCurrentElement;
                    }

                    if(conceptualLinkHandler != null)
                    {
                        foreach(var c in conceptualLinks)
                        {
                            var para = new XElement(OpenXmlElement.WordProcessingML + "p",
                                new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                    new XElement(OpenXmlElement.WordProcessingML + "spacing",
                                        new XAttribute(OpenXmlElement.WordProcessingML + "after", "0"))));
                            subsection.Add(para);

                            transformation.CurrentElement = para;
                            conceptualLinkHandler.Render(transformation, c);
                        }

                        transformation.CurrentElement = priorCurrentElement;
                    }
                }
            }
        }

        /// <summary>
        /// Render auto-generated see also section links based on the API topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="subsection">The subsection to which the links are added</param>
        private static void RenderApiAutoGeneratedSeeAlsoLinks(TopicTransformationCore transformation,
          XElement subsection)
        {
            // Add a link to the containing type on all list and member topics
            if(transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
              transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
            {
                subsection.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "spacing",
                            new XAttribute(OpenXmlElement.WordProcessingML + "after", "0"))),
                    new XElement("referenceLink",
                        new XAttribute("target", transformation.ApiMember.TypeTopicId),
                        new XAttribute("display-target", "format"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoTypeLink"),
                            new XElement("parameter", "{0}"),
                            new XElement("parameter", transformation.ApiMember.TypeApiSubgroup)))));
            }

            // Open XML documents don't contain an overloads topic so we won't render a link to it.  The document
            // will contain link targets for the overload topics that will appear just before the first member
            // of the overload set so that they will work if used in comments.

            // Add a link to the namespace topic
            string namespaceId = transformation.ReferenceNode.Element("containers")?.Element("namespace")?.Attribute("api")?.Value;

            if(!String.IsNullOrWhiteSpace(namespaceId))
            {
                subsection.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                        new XElement(OpenXmlElement.WordProcessingML + "spacing",
                            new XAttribute(OpenXmlElement.WordProcessingML + "after", "0"))),
                    new XElement("referenceLink",
                        new XAttribute("target", namespaceId),
                        new XAttribute("display-target", "format"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoNamespaceLink"),
                            new XElement("parameter", "{0}")))));
            }
        }
        #endregion
    }
}
