//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : VisualStudio2013Transformation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/24/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to generate a MAML or API HTML topic from the raw topic XML data for the
// Visual Studio 2013 presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle.Transformation;
using Sandcastle.Core.PresentationStyle.Transformation.Elements;
using Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;
using Sandcastle.Core.Reflection;

namespace Sandcastle.PresentationStyles.VS2013
{
    /// <summary>
    /// This class is used to generate a MAML or API HTML topic from the raw topic XML data for the Visual
    /// Studio 2013 presentation style.
    /// </summary>
    public class VisualStudio2013Transformation : TopicTransformationCore
    {
        #region Private data members
        //=====================================================================

        private XDocument pageTemplate;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="supportedFormats">The help file formats supported by the presentation style</param>
        /// <param name="resolvePath">The function used to resolve content file paths for the presentation style</param>
        public VisualStudio2013Transformation(HelpFileFormats supportedFormats, Func<string, string> resolvePath) :
          base(supportedFormats, resolvePath)
        {
            this.TopicTemplatePath = this.ResolvePath(@"Templates\TopicTemplate.html");
            this.UsesLegacyCodeColorizer = true;
        }
        #endregion

        #region Topic transformation argument shortcut properties
        //=====================================================================

        /// <summary>
        /// Robots metadata
        /// </summary>
        private string RobotsMetadata => this.TransformationArguments[nameof(RobotsMetadata)].Value;

        /// <summary>
        /// Logo file
        /// </summary>
        private string LogoFile => this.TransformationArguments[nameof(LogoFile)].Value;

        /// <summary>
        /// Logo height
        /// </summary>
        private int LogoHeight => Int32.TryParse(this.TransformationArguments[nameof(LogoHeight)].Value,
            out int height) ? height : 0;

        /// <summary>
        /// Logo width
        /// </summary>
        private int LogoWidth => Int32.TryParse(this.TransformationArguments[nameof(LogoWidth)].Value,
            out int width) ? width : 0;

        /// <summary>
        /// Logo alternate text
        /// </summary>
        private string LogoAltText => this.TransformationArguments[nameof(LogoAltText)].Value;

        /// <summary>
        /// Logo placement
        /// </summary>
        private LogoPlacement LogoPlacement => Enum.TryParse(this.TransformationArguments[nameof(LogoPlacement)].Value,
            true, out LogoPlacement placement) ? placement : LogoPlacement.Left ;

        /// <summary>
        /// Logo alignment
        /// </summary>
        private LogoAlignment LogoAlignment => Enum.TryParse(this.TransformationArguments[nameof(LogoAlignment)].Value,
            true, out LogoAlignment alignment) ? alignment : LogoAlignment.Left;

        /// <summary>
        /// Logo URL
        /// </summary>
        private string LogoUrl => this.TransformationArguments[nameof(LogoUrl)].Value;

        /// <summary>
        /// Maximum version parts
        /// </summary>
        private int MaxVersionParts => Int32.TryParse(this.TransformationArguments[nameof(MaxVersionParts)].Value,
            out int maxVersionParts) ? maxVersionParts : 5;

        /// <summary>
        /// Default language
        /// </summary>
        private string DefaultLanguage => !String.IsNullOrWhiteSpace(this.TransformationArguments[nameof(DefaultLanguage)].Value) ?
            this.TransformationArguments[nameof(DefaultLanguage)].Value : this.TransformationArguments[nameof(DefaultLanguage)].DefaultValue;

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
        /// Include separators for integer enumeration values
        /// </summary>
        private bool IncludeIntegerEnumSeparators => Boolean.TryParse(this.TransformationArguments[nameof(IncludeIntegerEnumSeparators)].Value,
            out bool includeSeparators) && includeSeparators;

        /// <summary>
        /// Base source code URL
        /// </summary>
        private string BaseSourceCodeUrl => this.TransformationArguments[nameof(BaseSourceCodeUrl)].Value;

        /// <summary>
        /// Request example URL
        /// </summary>
        private string RequestExampleUrl => this.TransformationArguments[nameof(RequestExampleUrl)].Value;

        /// <summary>
        /// Show parameters on all methods on the member list page, not just on overloads
        /// </summary>
        private bool ShowParametersOnAllMethods => Boolean.TryParse(this.TransformationArguments[nameof(ShowParametersOnAllMethods)].Value,
            out bool showParameters) && showParameters;

        #endregion

        #region TopicTransformationCore implementation
        //=====================================================================

        /// <inheritdoc />
        public override string IconPath { get; set; } = "../icons/";

        /// <inheritdoc />
        public override string StyleSheetPath { get; set; } = "../styles/";

        /// <inheritdoc />
        public override string ScriptPath { get; set; } = "../scripts/";

        /// <inheritdoc />
        protected override void CreateTransformationArguments()
        {
            this.AddTransformationArgumentRange(new[]
            {
                new TransformationArgument(nameof(RobotsMetadata), true, true, null,
                    "An optional robots metadata value (e.g. noindex, nofollow).  If left blank, the robots " +
                    "metadata element will be omitted from the topics."),
                new TransformationArgument(nameof(BibliographyDataFile), true, true, null,
                    "An optional bibliography data XML file.  Specify the filename with a fully qualified or " +
                    "relative path.  If the path is relative or omitted, it is assumed to be relative to the " +
                    "project folder.\r\n\r\n" +
                    "If blank, no bibliography section will be included in the topics.\r\n\r\n" +
                    "For information on the data file's format, see the bibliography element topic in the " +
                    "Sandcastle MAML Guide or XML Comments Guide."),
                new TransformationArgument(nameof(LogoFile), true, true, null,
                    "An optional logo file to insert into the topic headers.  Specify the filename only, omit " +
                    "the path.\r\n\r\n" +
                    "Important: Add a folder called \"icons\\\" to the root of your help file builder project and " +
                    "place the logo file in the icons\\ folder.  Set the Build Action property to Content on the " +
                    "logo file's properties.\r\n\r\n" +
                    "If blank, no logo will appear in the topic headers.  If building website output and your web " +
                    "server is case-sensitive, be sure to match the case of the folder name in your project with " +
                    "that of the presentation style.  The same applies to the logo filename itself."),
                new TransformationArgument(nameof(LogoHeight), true, true, null,
                    "An optional logo height.  If left blank, the actual logo image height is used."),
                new TransformationArgument(nameof(LogoWidth), true, true, null,
                    "An optional logo width.  If left blank, the actual logo image width is used."),
                new TransformationArgument(nameof(LogoAltText), true, true, null,
                    "Optional logo alternate text.  If left blank, no alternate text is added."),
                new TransformationArgument(nameof(LogoPlacement), true, true, "Left",
                    "An optional logo placement.  Specify Left, Right, or Above.  If not specified, the " +
                    "default is Left."),
                new TransformationArgument(nameof(LogoAlignment), true, true, "Left",
                    "An optional logo alignment when using the 'Above' placement option.  Specify Left, " +
                    "Right, or Center.  If not specified, the default is Left."),
                new TransformationArgument(nameof(LogoUrl), true, true, null,
                    "An optional logo URL to navigate to when the logo is clicked."),
                new TransformationArgument(nameof(MaxVersionParts), false, true, null,
                    "The maximum number of assembly version parts to show in API member topics.  Set to 2, " +
                    "3, or 4 to limit it to 2, 3, or 4 parts or leave it blank for all parts including the " +
                    "assembly file version value if specified."),
                new TransformationArgument(nameof(DefaultLanguage), true, true, "cs",
                    "The default language to use for syntax sections, code snippets, and a language-specific " +
                    "text.  This should be set to cs, vb, cpp, fs, or the keyword style parameter value of a " +
                    "third-party syntax generator if you want to use a non-standard language as the default."),
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
                new TransformationArgument(nameof(IncludeIntegerEnumSeparators), false, true, "true",
                    "Set this to true to include separators in integer enumeration values (1,000 vs 1000)."),
                new TransformationArgument(nameof(BaseSourceCodeUrl), false, true, null,
                    "If you set the Source Code Base Path property in the Paths category, specify the URL to " +
                    "the base source code folder on your project's website here.  Some examples for GitHub are " +
                    "shown below.\r\n\r\n" +
                    "Important: Be sure to set the Source Code Base Path property and terminate the URL below with " +
                    "a slash if necessary.\r\n\r\n" +
                    "Format: https://github.com/YourUserID/YourProject/blob/BranchNameOrCommitHash/BaseSourcePath/ \r\n\r\n" +
                    "Master branch: https://github.com/JohnDoe/WidgestProject/blob/master/src/ \r\n" +
                    "A different branch: https://github.com/JohnDoe/WidgestProject/blob/dev-branch/src/ \r\n" +
                    "A specific commit: https://github.com/JohnDoe/WidgestProject/blob/c6e41c4fc2a4a335352d2ae8e7e85a1859751662/src/"),
                new TransformationArgument(nameof(RequestExampleUrl), false, true, null,
                    "To include a link that allows users to request an example for an API topic, set the URL " +
                    "to which the request will be sent.  This can be a web page URL or an e-mail URL.  Only include " +
                    "the URL as the parameters will be added automatically by the topic.  For example:\r\n\r\n" +
                    "Create a new issue on GitHub: https://github.com/YourUserID/YourProject/issues/new \r\n" +
                    "Send via e-mail: mailto:YourEmailAddress@Domain.com"),
                new TransformationArgument(nameof(ShowParametersOnAllMethods), false, true, "False",
                    "If false, the default, parameters are hidden on all but overloaded methods on the member " +
                    "list pages.  If set to true, parameters are shown on all methods.")
            });
        }

        /// <inheritdoc />
        protected override void CreateLanguageSpecificText()
        {
            this.AddLanguageSpecificTextRange(new[]
            {
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.CPlusPlus, "nullptr"),
                    (LanguageSpecificText.VisualBasic, "Nothing"),
                    (LanguageSpecificText.Neutral, "null"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "Shared"),
                    (LanguageSpecificText.Neutral, "static"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "Overridable"),
                    (LanguageSpecificText.Neutral, "virtual"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "True"),
                    (LanguageSpecificText.Neutral, "true"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "False"),
                    (LanguageSpecificText.Neutral, "false"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "MustInherit"),
                    (LanguageSpecificText.Neutral, "abstract"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "NotInheritable"),
                    (LanguageSpecificText.Neutral, "sealed"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "In"),
                    (LanguageSpecificText.FSharp, String.Empty),
                    (LanguageSpecificText.Neutral, "in"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "Out"),
                    (LanguageSpecificText.FSharp, String.Empty),
                    (LanguageSpecificText.Neutral, "out"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "Async"),
                    (LanguageSpecificText.Neutral, "async"),
                }),
                new LanguageSpecificText(true, new[]
                {
                    (LanguageSpecificText.VisualBasic, "Await"),
                    (LanguageSpecificText.FSharp, "let!"),
                    (LanguageSpecificText.Neutral, "await"),
                })
            });
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

                // HTML elements (may occur in XML comments)
                new PassthroughElement("a"),
                new PassthroughElement("abbr"),
                new PassthroughElement("acronym"),
                new PassthroughElement("area"),
                new PassthroughElement("b"),
                new PassthroughElement("blockquote"),
                new PassthroughElement("br"),
                new PassthroughElement("dd"),
                new PassthroughElement("del"),
                new PassthroughElement("div"),
                new PassthroughElement("dl"),
                new PassthroughElement("dt"),
                new PassthroughElement("em"),
                new PassthroughElement("font"),
                new PassthroughElement("h1"),
                new PassthroughElement("h2"),
                new PassthroughElement("h3"),
                new PassthroughElement("h4"),
                new PassthroughElement("h5"),
                new PassthroughElement("h6"),
                new PassthroughElement("hr"),
                new PassthroughElement("i"),
                new PassthroughElement("img"),
                new PassthroughElement("ins"),
                new PassthroughElement("li"),
                new PassthroughElement("map"),
                new PassthroughElement("ol"),
                new PassthroughElement("p"),
                new PassthroughElement("pre"),
                new PassthroughElement("q"),
                new PassthroughElement("strong"),
                new PassthroughElement("sub"),
                new PassthroughElement("sup"),
                new PassthroughElement("td"),
                new PassthroughElement("th"),
                new PassthroughElement("tr"),
                new PassthroughElement("u"),
                new PassthroughElement("ul"),

                // Elements common to HTML, MAML, and/or XML comments.  Processing may differ based on the topic
                // type (API or MAML).
                new BibliographyElement(),
                new CiteElement(),
                new CodeSnippetGroupElementTabbed(),
                new PassthroughElement("include"),
                new PassthroughElement("includeAttribute"),
                new MarkupElement(),
                new ConvertibleElement("para", "p"),
                new ListElement { TableStyle = null },
                new ParametersElement(),
                new PassthroughElement("referenceLink"),
                new PassthroughElement("span"),
                new SummaryElement(),
                new TableElement { TableStyle = null },

                // MAML elements
                new NoteElement("alert"),
                new ConvertibleElement("application", "strong"),
                new NamedSectionElement("appliesTo"),
                new AutoOutlineElement(),
                new NamedSectionElement("background"),
                new NamedSectionElement("buildInstructions"),
                new CodeEntityReferenceElement(),
                new CodeExampleElement(),
                new ConvertibleElement("codeFeaturedElement", "span", "label"),
                new ConvertibleElement("codeInline", "span", "code"),
                new NonRenderedParentElement("codeReference"),
                new ConvertibleElement("command", "span", "command"),
                new ConvertibleElement("computerOutputInline", "span", "code"),
                new NonRenderedParentElement("conclusion"),
                new NonRenderedParentElement("content"),
                new CopyrightElement(),
                new NonRenderedParentElement("corporation"),
                new NonRenderedParentElement("country"),
                new ConvertibleElement("database", "strong"),
                new NonRenderedParentElement("date"),
                new ConvertibleElement("definedTerm", "dt", true),
                new ConvertibleElement("definition", "dd"),
                new ConvertibleElement("definitionTable", "dl"),
                new NamedSectionElement("demonstrates"),
                new NonRenderedParentElement("description"),
                new NamedSectionElement("dotNetFrameworkEquivalent"),
                new ConvertibleElement("embeddedLabel", "span", "label"),
                new EntryElement(),
                new ConvertibleElement("environmentVariable", "span", "code"),
                new ConvertibleElement("errorInline", "em"),
                new NamedSectionElement("exceptions"),
                new ExternalLinkElement(),
                new NamedSectionElement("externalResources"),
                new ConvertibleElement("fictitiousUri", "em"),
                new ConvertibleElement("foreignPhrase", "span", "foreignPhrase"),
                new GlossaryElement(),
                new ConvertibleElement("hardware", "strong"),
                new NamedSectionElement("inThisSection"),
                new IntroductionElement(),
                new LanguageKeywordElement(),
                new NamedSectionElement("languageReferenceRemarks"),
                new NonRenderedParentElement("legacy"),
                new ConvertibleElement("legacyBold", "strong"),
                new ConvertibleElement("legacyItalic", "em"),
                new LegacyLinkElement(),
                new ConvertibleElement("legacyUnderline", "u"),
                new ConvertibleElement("lineBreak", "br"),
                new LinkElement(),
                new ConvertibleElement("listItem", "li", true),
                new ConvertibleElement("literal", "span", "literal"),
                new ConvertibleElement("localUri", "em"),
                new NonRenderedParentElement("localizedText"),
                new ConvertibleElement("math", "span", "math"),
                new MediaLinkElement
                {
                    MediaElement = "div",
                    MediaCaptionElement = "div",
                    CaptionStyle = "caption",
                    CaptionLeadTextStyle = "captionLead"
                },
                new MediaLinkInlineElement(),
                new ConvertibleElement("newTerm", "span", "term"),
                new NamedSectionElement("nextSteps"),
                new ConvertibleElement("parameterReference", "span", "parameter"),
                new ConvertibleElement("phrase", "span", "phrase"),
                new ConvertibleElement("placeholder", "span", "placeholder"),
                new NamedSectionElement("prerequisites"),
                new ProcedureElement(),
                new ConvertibleElement("quote", "blockquote"),
                new ConvertibleElement("quoteInline", "q"),
                new NamedSectionElement("reference"),
                new NamedSectionElement("relatedSections"),
                new RelatedTopicsElement(),
                new ConvertibleElement("replaceable", "span", "placeholder"),
                new NamedSectionElement("requirements"),
                new NamedSectionElement("returnValue"),
                new NamedSectionElement("robustProgramming"),
                new ConvertibleElement("row", "tr"),
                new SectionElement(),
                new NonRenderedParentElement("sections"),
                new NamedSectionElement("security"),
                new NonRenderedParentElement("snippets"),
                new ConvertibleElement("step", "li", true),
                new StepsElement(),
                new ConvertibleElement("subscript", "sub"),
                new ConvertibleElement("subscriptType", "sub"),
                new ConvertibleElement("superscript", "sup"),
                new ConvertibleElement("superscriptType", "sup"),
                new ConvertibleElement("system", "strong"),
                new NonRenderedParentElement("tableHeader"),
                new NamedSectionElement("textValue"),
                // The title element is ignored.  The section and table elements handle them as needed.
                new IgnoredElement("title"),
                new NonRenderedParentElement("type"),
                new ConvertibleElement("ui", "span", "ui"),
                new ConvertibleElement("unmanagedCodeEntityReference", "strong"),
                new ConvertibleElement("userInput", "span", "input"),
                new ConvertibleElement("userInputLocalizable", "span", "input"),
                new NamedSectionElement("whatsNew"),

                // XML comments and reflection data file elements
                new ConvertibleElement("c", "span", "code"),
                new PassthroughElement("conceptualLink"),
                new NamedSectionElement("example"),
                new ImplementsElement(),
                new NoteElement("note"),
                new ConvertibleElement("paramref", "name", "span", "parameter"),
                // This is handled as a notice
                new IgnoredElement("preliminary"),
                new NamedSectionElement("remarks"),
                new ReturnsElement(),
                new SeeElement(),
                // seeAlso should be a top-level element in the comments but may appear within other elements.
                // We'll ignore it if seen as they'll be handled manually by the See Also section processing.
                new IgnoredElement("seealso"),
                new SyntaxElementTabbed(nameof(RequestExampleUrl), nameof(BaseSourceCodeUrl)),
                new TemplatesElement(),
                new ThreadsafetyElement(),
                new ConvertibleElement("typeparamref", "name", "span", "typeparameter"),
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
                new ApiTopicSectionHandler(ApiTopicSectionType.InheritanceHierarchyAbbreviated,
                    t => RenderApiInheritanceHierarchy(t, false)),
                new ApiTopicSectionHandler(ApiTopicSectionType.NamespaceAndAssemblyInfo,
                    t => RenderApiNamespaceAndAssemblyInformation(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.SyntaxSection, t => RenderApiSyntaxSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.MemberList, t => RenderApiMemberList(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Events,
                    t => RenderApiSectionTable(t, "title_events", "header_eventType", "header_eventReason",
                         t.CommentsNode.Elements("event"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.Exceptions,
                    t => RenderApiSectionTable(t, "title_exceptions", "header_exceptionName",
                         "header_exceptionCondition", this.CommentsNode.Elements("exception"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.Remarks, t => RenderApiRemarksSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Examples, t => RenderApiExamplesSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Versions, t => RenderApiVersionsSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Permissions,
                    t => RenderApiSectionTable(t, "title_permissions", "header_permissionName",
                         "header_permissionDescription", t.CommentsNode.Elements("permission"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.ThreadSafety,
                    t => t.RenderNode(t.CommentsNode.Element("threadsafety"))),
                new ApiTopicSectionHandler(ApiTopicSectionType.RevisionHistory,
                    t => RenderApiRevisionHistorySection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.Bibliography,
                    t => RenderApiBibliographySection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.SeeAlso, t => RenderApiSeeAlsoSection(t)),
                new ApiTopicSectionHandler(ApiTopicSectionType.InheritanceHierarchyFull,
                    t => RenderApiInheritanceHierarchy(t, true))
            });
        }

        /// <inheritdoc />
        protected override void CreateNoticeDefinitions()
        {
            Notice preliminary = Notice.PreliminaryNotice, obsolete = Notice.ObsoleteNotice,
                experimental = Notice.ExperimentalNotice;

            preliminary.NoticeStyleClasses = experimental.NoticeStyleClasses = "preliminary";
            preliminary.TagStyleClasses = experimental.TagStyleClasses = "preliminary tag";
            obsolete.NoticeStyleClasses = "obsolete";
            obsolete.TagStyleClasses = "tag";
                
            this.AddNoticeDefinitions(new[] { preliminary, obsolete, experimental });
        }

        /// <inheritdoc />
        protected override XDocument RenderTopic()
        {
            if(pageTemplate == null)
            {
                pageTemplate = LoadTemplateFile(this.TopicTemplatePath, new[] {
                    ("{@DefaultLanguage}", this.DefaultLanguage),
                    ("{@Locale}", this.Locale),
                    ("{@LocaleLowercase}", this.Locale.ToLowerInvariant()),
                    ("{@IconPath}", this.IconPath),
                    ("{@StyleSheetPath}", this.StyleSheetPath),
                    ("{@ScriptPath}", this.ScriptPath)
                });
            }

            var document = new XDocument(pageTemplate);

            XElement html = document.Root, head = html.Element("head"),
                topicContent = html.Descendants().Where(d => d.Attribute("id")?.Value == "TopicContent").FirstOrDefault();

            this.CurrentElement = head ?? throw new InvalidOperationException("Page template is missing the head element");
            this.RenderHeaderMetadata();

            this.CurrentElement = topicContent ?? throw new InvalidOperationException("Page template is missing the \"TopicContent\" element");
            this.RenderPageTitleAndLogo();

            if(this.HasHeaderText)
            {
                topicContent.Add(new XElement("span",
                    new XAttribute("class", "introStyle"),
                    new XElement("include", new XAttribute("item", "headerText"))));
            }

            if(this.IsPreliminaryDocumentation)
            {
                topicContent.Add(new XElement("p",
                    new XAttribute("class", "preliminary"),
                    new XElement("include", new XAttribute("item", "preliminaryDocs"))));
            }

            this.OnRenderStarting(document);

            // Add the topic content.  MAML topics are rendered purely off of the element types.  API topics
            // require custom formatting based on the member type in the topic.
            if(this.IsMamlTopic)
                this.RenderNode(this.TopicNode);
            else
            {
                foreach(var section in this.ApiTopicSections)
                {
                    section.RenderSection(this);
                    this.OnSectionRendered(section.SectionType, section.CustomSectionName);
                }
            }

            var body = html.Element("body") ?? throw new InvalidOperationException("Body element not found");

            if(this.StartupScriptBlocks.Any())
                body.Add(new XElement("script", $"$(function(){{\r\n{String.Join("\r\n", this.StartupScriptBlocks)}\r\n}});"));

            if(this.StartupScriptBlockItemIds.Any())
            {
                var scriptItems = new XElement("script");

                body.Add(scriptItems);

                foreach(string id in this.StartupScriptBlockItemIds)
                    scriptItems.Add(new XElement("include", new XAttribute("item", id)));
            }

            this.OnRenderCompleted(document);

            return document;
        }

        /// <inheritdoc />
        public override (XElement Title, XElement Content) CreateSection(string uniqueId, bool localizedTitle,
          string title, string linkId)
        {
            string toggleImageId = uniqueId + "Toggle", toggleSectionId = uniqueId + "Section";
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

                titleElement = new XElement("div",
                        new XAttribute("class","collapsibleAreaRegion"),
                    new XElement("span",
                        new XAttribute("class", "collapsibleRegionTitle"),
                        new XAttribute("onclick", "SectionExpandCollapse('" + uniqueId + "')"),
                        new XAttribute("onkeypress", "SectionExpandCollapse_CheckKey('" + uniqueId + "', event)"),
                        new XAttribute("tabindex", "0"),
                            new XElement("img",
                                new XAttribute("id", toggleImageId),
                                new XAttribute("class", "collapseToggle"),
                                new XAttribute("src", this.IconPath + "SectionExpanded.png")),
                            titleContent));

                if(!String.IsNullOrWhiteSpace(linkId))
                    titleElement.Add(new XAttribute("id", linkId));
            }

            var contentElement = new XElement("div",
                new XAttribute("id", toggleSectionId),
                new XAttribute("class", "collapsibleSection"));

            return (titleElement, contentElement);
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

                titleElement = new XElement("h4", titleContent);
            }

            return (titleElement, null);
        }
        #endregion

        #region General topic rendering helper methods
        //=====================================================================

        /// <summary>
        /// This is used to add topic metadata to the <c>head</c> element
        /// </summary>
        /// <remarks>The <see cref="TopicTransformationCore.CurrentElement" /> should be set to the <c>head</c>
        /// element before calling this.</remarks>
        private void RenderHeaderMetadata()
        {
            string topicDesc;
            
            if(!String.IsNullOrWhiteSpace(this.RobotsMetadata))
            {
                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "robots"),
                    new XAttribute("content", this.RobotsMetadata)));
            }

            this.CurrentElement.Add(new XElement("title", this.IsMamlTopic ? this.MamlTopicTitle() :
                this.ApiTopicTitle(true, true)));

            if(this.IsMamlTopic)
            {
                string tocTitle = this.MetadataNode.Element("tableOfContentsTitle")?.Value.NormalizeWhiteSpace();

                if(!String.IsNullOrWhiteSpace(tocTitle))
                {
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Title"),
                        new XAttribute("content", tocTitle)));
                }
            }
            else
            {
                XNode title;

                if(this.ApiMember.ApiGroup == ApiMemberGroup.Namespace)
                {
                    // For namespaces only show the title without any descriptive text as the TOC title
                    title = new XText(this.ApiMember.Name);
                }
                else
                    title = this.ApiTopicTitle(false, true);

                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Title"),
                    new XElement("includeAttribute",
                        new XAttribute("name", "content"),
                        new XAttribute("item", "tocTitle"),
                        new XElement("parameter", title))));
            }

            if(this.IsMamlTopic)
            {
                topicDesc = this.TopicNode.Descendants(Element.Ddue + "para").FirstOrDefault()?.Value.NormalizeWhiteSpace();

                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.Id"),
                    new XAttribute("content", this.TopicNode.Attribute("id").Value)));

                var topicType = TopicType.FromElementName(this.TopicNode.Elements().First().Name.LocalName);

                if(topicType != null)
                {
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.ContentType"),
                        new XAttribute("content", TopicType.DescriptionForTopicTypeGroup(topicType.ContentType))));
                }
                else
                {
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.ContentType"),
                        new XAttribute("content", TopicType.DescriptionForTopicTypeGroup(TopicTypeGroup.Concepts))));
                }

                foreach(var keyword in this.MetadataNode.Elements("keyword").Where(
                  k => k.Attribute("index")?.Value == "K"))
                {
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "System.Keywords"),
                        new XAttribute("content", keyword.Value)));
                }

                foreach(var f1Help in this.MetadataNode.Elements("keyword").Where(
                  k => k.Attribute("index")?.Value == "F"))
                {
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", f1Help.Value)));
                }
            }
            else
            {
                topicDesc = this.CommentsNode.Element("summary")?.Value.NormalizeWhiteSpace();

                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.Id"),
                    new XAttribute("content", this.Key)));
                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.ContentType"),
                    new XAttribute("content", TopicType.DescriptionForTopicTypeGroup(TopicTypeGroup.Reference))));

                this.AddIndexMetadata();
                this.AddF1HelpMetadata();

                // Insert container and filename metadata for an API topic
                string namespaceId = this.ReferenceNode.Element("containers")?.Element(
                    "namespace").Attribute("api").Value, namespaceName = "(Default Namespace)";

                // Get the namespace from the container node for most members
                if(namespaceId != null && namespaceId.Length > 2 && namespaceId[1] == ':')
                    namespaceName = namespaceId.Substring(2);
                else
                {
                    if(String.IsNullOrWhiteSpace(namespaceId))
                    {
                        // If it's a namespace, get the name from the API data node.  For all others, assume it's
                        // the default namespace
                        if((this.ApiMember.ApiGroup == ApiMemberGroup.NamespaceGroup ||
                          this.ApiMember.ApiGroup == ApiMemberGroup.Namespace) &&
                          !String.IsNullOrWhiteSpace(this.ApiMember.Name))
                        {
                            namespaceName = this.ApiMember.Name;
                        }
                    }
                }

                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "container"),
                    new XAttribute("content", namespaceName)));

                if(!String.IsNullOrWhiteSpace(this.ApiMember.TopicFilename))
                {
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "file"),
                        new XAttribute("content", this.ApiMember.TopicFilename)));
                    this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "guid"),
                        new XAttribute("content", this.ApiMember.TopicFilename)));
                }
            }

            foreach(var language in this.DocumentNode.Descendants().Where(
              d => d.Attribute("language") != null && d.Attribute("phantom") == null).Select(
              d => d.Attribute("language").Value).Distinct().OrderBy(l => l))
            {
                string devLang = LanguageIdFor(language);

                if(!devLang.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                  !devLang.Equals("other", StringComparison.OrdinalIgnoreCase))
                {
                    this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.Category"),
                        new XElement("includeAttribute",
                            new XAttribute("name", "content"),
                            new XAttribute("item", $"metaLang_{devLang}"),
                            new XAttribute("undefined", devLang),
                                new XElement("parameter", "DevLang:"))));
                }
            }

            if(!String.IsNullOrWhiteSpace(topicDesc))
            {
                if(topicDesc.Length > 256)
                    topicDesc = topicDesc.Substring(0, 256);

                int pos = topicDesc.LastIndexOf('.');

                if(pos != -1)
                    topicDesc = topicDesc.Substring(0, pos + 1);

                this.CurrentElement.Add(new XElement("meta", new XAttribute("name", "Description"),
                    new XAttribute("content", topicDesc)));
            }
        }

        /// <summary>
        /// Add index metadata to the header
        /// </summary>
        private void AddIndexMetadata()
        {
            string namespaceName;

            // The ordering of the cases is important to ensure members are evaluated correctly
            switch(this.ApiMember)
            {
                case var t when t.ApiTopicGroup == ApiMemberGroup.Namespace:
                    // Namespace topics get one unqualified index entry
                    namespaceName = this.ReferenceNode.Element("apidata").Attribute("name").Value;

                    if(!String.IsNullOrWhiteSpace(namespaceName))
                    {
                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "System.Keywords"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "content"),
                                new XAttribute("item", "indexEntry_namespace"),
                                new XElement("parameter", namespaceName))));
                    }
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Type:
                    // Type overview topics get Namespace.Type and type keywords
                    namespaceName = this.ReferenceNode.Element("containers").Element("namespace").Element(
                        "apidata").Attribute("name").Value;

                    foreach(string apiName in this.LanguageSpecificApiNames(this.ReferenceNode))
                    {
                        if(!String.IsNullOrWhiteSpace(namespaceName))
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", $"indexEntry_{t.ApiSubgroup}"),
                                    new XElement("parameter", String.Join(".", namespaceName, apiName)))));
                        }

                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "System.Keywords"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "content"),
                                new XAttribute("item", $"indexEntry_{t.ApiSubgroup}"),
                                new XElement("parameter", apiName))));
                    }

                    // Enumerations get an entry for each member
                    if(t.ApiSubgroup == ApiMemberGroup.Enumeration && this.ReferenceNode.Element("elements") != null)
                    {
                        foreach(var enumMember in this.ReferenceNode.Element("elements").Elements("element"))
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", "indexEntry_EnumerationMember"),
                                    new XElement("parameter", enumMember.Element("apidata").Attribute("name").Value))));
                        }
                    }
                    break;

                case var t when(t.TopicGroup == ApiMemberGroup.Api && t.ApiSubgroup == ApiMemberGroup.Constructor &&
                  t.OverloadTopicId == null) || (t.TopicSubgroup == ApiMemberGroup.Overload &&
                  t.ApiSubgroup == ApiMemberGroup.Constructor):
                    // Constructor and constructor overload topics get unqualified sub-entries using the type names
                    var typeInfo = this.ReferenceNode.Element("containers").Element("type");
                    string typeSubGroup = typeInfo.Element("apidata").Attribute("subgroup").Value,
                        typeName = ApiTypeNameWithTemplateCount(typeInfo);
                    int tickMark = typeName.IndexOf('`');

                    if(tickMark != -1)
                        typeName = typeName.Substring(0, tickMark);

                    foreach(string apiName in this.LanguageSpecificApiNames(typeInfo))
                    {
                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "System.Keywords"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "content"),
                                new XAttribute("item", "indexEntry_constructor"),
                                new XElement("parameter",
                                    new XElement("include",
                                        new XAttribute("item", $"indexEntry_{typeSubGroup}"),
                                        new XElement("parameter", apiName))))));

                        if(typeName == apiName)
                        {
                            // Omit the type name on nested types
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", "indexEntry_constructorType"),
                                    new XElement("parameter", String.Join(".", apiName,
                                    typeName.Substring(typeName.LastIndexOf('.') + 1))))));
                        }
                        else
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", "indexEntry_constructorType"),
                                    new XElement("parameter", String.Join(".", apiName, typeName)))));
                        }
                    }
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.List && t.ApiTopicSubgroup != ApiMemberGroup.Overload:
                    // Member list pages, except overloads, get unqualified sub-entries.  The topic subgroup
                    // (e.g. "Methods") determines the title.
                    foreach(string apiName in this.LanguageSpecificApiNames(this.ReferenceNode))
                    {
                        string titleItem;

                        if(t.TopicSubgroup == ApiMemberGroup.Operators)
                        {
                            int operatorCount = this.ReferenceNode.Element("elements").Elements("element").Where(
                                el => !(el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                    "Explicit", StringComparison.Ordinal) &&
                                    !(el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                    "Implicit", StringComparison.Ordinal)).Count();
                            int conversionCount = this.ReferenceNode.Element("elements").Elements("element").Where(
                                el => (el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                    "Explicit", StringComparison.Ordinal) ||
                                    (el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                    "Implicit", StringComparison.Ordinal)).Count();

                            if(operatorCount > 0 && conversionCount > 0)
                                titleItem = "OperatorsAndTypeConversions";
                            else
                            {
                                if(operatorCount == 0 && conversionCount > 0)
                                    titleItem = "TypeConversions";
                                else
                                    titleItem = t.TopicSubgroup.ToString();
                            }
                        }
                        else
                            titleItem = t.TopicSubgroup.ToString();

                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "System.Keywords"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "content"),
                                new XAttribute("item", $"indexEntry_{titleItem}"),
                                new XElement("parameter",
                                    new XElement("include",
                                        new XAttribute("item", $"indexEntry_{t.ApiSubgroup}"),
                                        new XElement("parameter", apiName))))));
                    }
                    break;

                case var t when t.TopicGroup == ApiMemberGroup.Api && t.ApiSubSubgroup == ApiMemberGroup.Operator &&
                  (t.Name.Equals("Implicit", StringComparison.Ordinal) ||
                   t.Name.Equals("Explicit", StringComparison.Ordinal)):
                    // Explicit and implicit operators get an unqualified entry
                    if(this.ApiTopicTitle(true, true) is XElement titleInfo)
                    {
                        var lastParameter = titleInfo.Elements("parameter").LastOrDefault()?.Value;

                        if(!String.IsNullOrWhiteSpace(lastParameter))
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", "indexEntry_conversionOperator"),
                                    new XElement("parameter", lastParameter.Substring(1, lastParameter.Length - 2)))));
                        }
                    }
                    break;

                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiGroup == ApiMemberGroup.Member &&
                  t.OverloadTopicId == null) || t.TopicSubgroup == ApiMemberGroup.Overload:
                    // Other member or overload topics get qualified and unqualified entries using the member names
                    string entryType;

                    if(t.IsExplicitlyImplemented)
                    {
                        if(t.ApiTopicSubSubgroup != ApiMemberGroup.None)
                            entryType = t.ApiTopicSubSubgroup.ToString();
                        else
                        {
                            if(t.ApiTopicSubgroup == ApiMemberGroup.Overload)
                                entryType = t.ApiSubgroup.ToString();
                            else
                                entryType = t.ApiTopicSubgroup.ToString();
                        }

                        foreach(string apiName in this.LanguageSpecificApiNames(this.ReferenceNode.Element(
                          "implements").Element("member")))
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", $"indexEntry_{entryType}Explicit"),
                                    new XElement("parameter", apiName))));

                            foreach(string ns in this.LanguageSpecificApiNames(this.ReferenceNode.Element(
                              "containers").Element("type")))
                            {
                                this.CurrentElement.Add(new XElement("meta",
                                    new XAttribute("name", "System.Keywords"),
                                    new XElement("includeAttribute",
                                        new XAttribute("name", "content"),
                                        new XAttribute("item", $"indexEntry_{entryType}Explicit"),
                                        new XElement("parameter", String.Join(".", ns, apiName)))));
                            }
                        }
                    }
                    else
                    {
                        if(t.ApiTopicSubSubgroup != ApiMemberGroup.None)
                            entryType = t.ApiTopicSubSubgroup.ToString();
                        else
                        {
                            if(t.ApiSubSubgroup == ApiMemberGroup.Operator)
                                entryType = t.ApiSubSubgroup.ToString();
                            else
                            {
                                if(t.ApiTopicSubgroup == ApiMemberGroup.Overload)
                                    entryType = t.ApiSubgroup.ToString();
                                else
                                    entryType = t.ApiTopicSubgroup.ToString();
                            }
                        }

                        HashSet<string> keywords = new HashSet<string>();
                        var typeNode = this.ReferenceNode;

                        // For overloads topics, use the type from container type
                        //if(t.ApiTopicSubgroup == ApiMemberGroup.Overload)
                        //    typeNode = this.ReferenceNode.Element("containers").Element("type");

                        foreach(string apiName in this.LanguageSpecificApiNames(this.ReferenceNode))
                        {
                            keywords.Add(apiName);

                            int pos = apiName.LastIndexOf('.');

                            if(pos != -1)
                            {
                                keywords.Add(apiName.Substring(pos + 1));
                            }
                        }

                        foreach(string apiName in keywords)
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", $"indexEntry_{entryType}"),
                                    new XElement("parameter", apiName))));
                        }
                    }
                    break;

                default:
                    // All other pages do not get any F1 help keywords
                    break;
            }
        }

        /// <summary>
        /// Add F1 help metadata to the header
        /// </summary>
        private void AddF1HelpMetadata()
        {
            string namespaceName, typeName, memberName;

            switch(this.ApiMember)
            {
                case var t when t.ApiTopicGroup == ApiMemberGroup.Namespace:
                    // Namespace pages get the namespace keyword, if it exists
                    namespaceName = this.ReferenceNode.Element("apidata").Attribute("name").Value;

                    if(!String.IsNullOrWhiteSpace(namespaceName))
                    {
                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", namespaceName)));
                    }
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Type:
                    // Type overview pages get Namespace.Type and type keywords
                    namespaceName = this.ReferenceNode.Element("containers").Element("namespace").Element(
                        "apidata").Attribute("name").Value;
                    typeName = ApiTypeNameWithTemplateCount(this.ReferenceNode);

                    if(!String.IsNullOrWhiteSpace(namespaceName))
                    {
                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", String.Join(".", namespaceName, typeName))));
                    }

                    this.CurrentElement.Add(new XElement("meta",
                        new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", typeName)));

                    // Enumerations get an entry for each member (Namespace.Type.Member or Type.Member)
                    if(t.ApiSubgroup == ApiMemberGroup.Enumeration && this.ReferenceNode.Element("elements") != null)
                    {
                        foreach(var enumMember in this.ReferenceNode.Element("elements").Elements("element"))
                        {
                            if(!String.IsNullOrWhiteSpace(namespaceName))
                            {
                                this.CurrentElement.Add(new XElement("meta",
                                    new XAttribute("name", "Microsoft.Help.F1"),
                                    new XAttribute("content", String.Join(".", namespaceName, typeName,
                                        enumMember.Element("apidata").Attribute("name").Value))));
                            }
                            else
                            {
                                this.CurrentElement.Add(new XElement("meta",
                                    new XAttribute("name", "Microsoft.Help.F1"),
                                    new XAttribute("content", String.Join(".", typeName,
                                        enumMember.Element("apidata").Attribute("name").Value))));
                            }
                        }
                    }
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.List && t.ApiTopicSubgroup == ApiMemberGroup.Overload:
                    // Overload list pages get Namepsace.Type.Member, Type.Member, and member keywords
                    namespaceName = this.ReferenceNode.Element("containers").Element("namespace").Element(
                        "apidata").Attribute("name").Value;
                    typeName = ApiTypeNameWithTemplateCount(this.ReferenceNode.Element("containers").Element("type"));

                    if(t.ApiSubgroup == ApiMemberGroup.Constructor)
                        memberName = typeName;
                    else
                        memberName = t.Name;

                    if(!String.IsNullOrWhiteSpace(namespaceName))
                    {
                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", String.Join(".", namespaceName, typeName, memberName))));
                    }

                    this.CurrentElement.Add(new XElement("meta",
                        new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", String.Join(".", typeName, memberName))));
                    this.CurrentElement.Add(new XElement("meta",
                        new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", memberName)));
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Member:
                    // Member pages get Namepsace.Type.Member, Type.Member, and member keywords.  Overload
                    // signature topics do not.
                    if(this.ReferenceNode.Element("memberdata").Attribute("overload") == null)
                    {
                        // Overload list pages get Namepsace.Type.Member, Type.Member, and member keywords
                        namespaceName = this.ReferenceNode.Element("containers").Element("namespace").Element(
                            "apidata").Attribute("name").Value;
                        typeName = ApiTypeNameWithTemplateCount(this.ReferenceNode.Element("containers").Element("type"));

                        if(t.ApiSubgroup == ApiMemberGroup.Constructor)
                            memberName = typeName;
                        else
                        {
                            if(t.IsExplicitlyImplemented)
                                memberName = ApiTypeNameWithTemplateCount(this.ReferenceNode.Element("implements").Element("member"));
                            else
                                memberName = t.Name;
                        }

                        if(!String.IsNullOrWhiteSpace(namespaceName))
                        {
                            this.CurrentElement.Add(new XElement("meta",
                                new XAttribute("name", "Microsoft.Help.F1"),
                                new XAttribute("content", String.Join(".", namespaceName, typeName, memberName))));
                        }

                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", String.Join(".", typeName, memberName))));
                        this.CurrentElement.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", memberName)));
                    }
                    break;

                default:
                    // All other pages do not get any F1 help keywords
                    break;
            }
        }

        /// <summary>
        /// This is used to render the page title and optional logo
        /// </summary>
        /// <remarks>The <see cref="TopicTransformationCore.CurrentElement" /> should be set to the topic content
        /// element before calling this.</remarks>
        private void RenderPageTitleAndLogo()
        {
            var table = new XElement("table", new XAttribute("class", "titleTable"));

            XElement image = null;
            string logoFile = this.LogoFile;
            LogoPlacement placement = this.LogoPlacement;
            LogoAlignment alignment = this.LogoAlignment;

            if(!String.IsNullOrWhiteSpace(logoFile))
            {
                string logoAltText = this.LogoAltText, logoUrl = this.LogoUrl;
                int logoWidth = this.LogoWidth, logoHeight = this.LogoHeight;

                image = new XElement("img");

                if(!String.IsNullOrWhiteSpace(logoAltText))
                    image.Add(new XAttribute("alt", logoAltText));

                if(logoWidth > 0)
                    image.Add(new XAttribute("width", logoWidth));

                if(logoHeight > 0)
                    image.Add(new XAttribute("height", logoHeight));

                image.Add(new XAttribute("src", this.IconPath + logoFile));

                if(!String.IsNullOrWhiteSpace(logoUrl))
                {
                    image = new XElement("a",
                        new XAttribute("target", "_blank"),
                        new XAttribute("rel", "noopener noreferrer"),
                        new XAttribute("href", logoUrl), new XElement(image));
                }
            }

            if(image != null && placement == LogoPlacement.Above)
            {
                table.Add(new XElement("tr",
                    new XElement("td",
                        new XAttribute("colspan", "2"),
                        new XAttribute("class", "logoColumnAbove"),
                        new XAttribute("align", alignment.ToString().ToLowerInvariant()),
                    image)));
            }

            var tr = new XElement("tr");
            
            table.Add(tr);

            if(image != null && placement == LogoPlacement.Left)
            {
                tr.Add(new XElement("td",
                    new XAttribute("class", "logoColumn"),
                    image));
            }

            tr.Add(new XElement("td",
                new XAttribute("class", "titleColumn"),
                new XElement("h1", this.IsMamlTopic ? this.MamlTopicTitle() : this.ApiTopicTitle(false, false))));

            if(image != null && placement == LogoPlacement.Right)
            {
                tr.Add(new XElement("td",
                    new XAttribute("class", "logoColumn"),
                    image));
            }

            this.CurrentElement.Add(table);
        }
        #endregion

        #region API topic section handlers
        //=====================================================================

        /// <summary>
        /// This is used to render the notices at the top of each topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderNotices(TopicTransformationCore transformation)
        {
            var notices = new List<XElement>();

            foreach(var n in transformation.NoticeDefinitions)
            {
                string noticeText = null;

                if(!String.IsNullOrWhiteSpace(n.ElementName))
                {
                    var element = transformation.CommentsNode.Element(n.ElementName);

                    if(element != null)
                    {
                        if(n.UseValueForText)
                            noticeText = element.Value?.NormalizeWhiteSpace();

                        if(String.IsNullOrWhiteSpace(noticeText))
                            noticeText = n.NoticeMessage;
                    }
                }

                if(noticeText == null && !String.IsNullOrWhiteSpace(n.AttributeTypeName))
                {
                    string attrName = n.AttributeTypeName;

                    // Add the "T:" prefix if not specified
                    if(attrName.Length > 2 && attrName[1] != ':')
                        attrName = "T:" + attrName;

                    var attr = transformation.ReferenceNode.AttributeOfType(attrName);

                    if(attr != null)
                    {
                        if(n.UseValueForText)
                            noticeText = attr.Element("argument")?.Element("value")?.Value?.NormalizeWhiteSpace();

                        if(String.IsNullOrWhiteSpace(noticeText))
                            noticeText = n.NoticeMessage;
                    }
                }

                if(!String.IsNullOrWhiteSpace(noticeText))
                {
                    string style = n.NoticeStyleClasses;

                    if(String.IsNullOrWhiteSpace(style))
                        style = "tag";

                    var div = new XElement("div", new XAttribute("class", style));
                    var para = new XElement("p");

                    div.Add(para);

                    // If the notice text starts with '@', it's a content item
                    if(noticeText[0] == '@')
                        para.Add(new XElement("include", new XAttribute("item", noticeText.Substring(1))));
                    else
                        para.Add(noticeText);

                    notices.Add(div);
                }
            }

            if(notices.Count != 0)
            {
                var notes = new XElement("div", new XAttribute("id", "TopicNotices"));

                transformation.CurrentElement.Add(notes);

                foreach(var n in notices)
                    notes.Add(n);
            }
        }

        /// <summary>
        /// This is used to render the notice tags within a member list entry
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="apiMember">The API member information element</param>
        /// <param name="parent">The parent element that will contain the notice tags</param>
        private static void RenderNoticeTags(TopicTransformationCore transformation, XElement apiMember,
           XElement parent)
        {
            var notices = new List<XElement>();

            foreach(var n in transformation.NoticeDefinitions)
            {
                string noticeText = null;

                if(!String.IsNullOrWhiteSpace(n.ElementName))
                {
                    var element = apiMember.Element(n.ElementName);

                    if(element != null)
                        noticeText = n.TagText;
                }

                if(noticeText == null && !String.IsNullOrWhiteSpace(n.AttributeTypeName))
                {
                    string attrName = n.AttributeTypeName;

                    // Add the "T:" prefix if not specified
                    if(attrName.Length > 2 && attrName[1] != ':')
                        attrName = "T:" + attrName;

                    var attr = apiMember.AttributeOfType(attrName);

                    if(attr != null)
                        noticeText = n.TagText;
                }

                if(!String.IsNullOrWhiteSpace(noticeText))
                {
                    string style = n.TagStyleClasses;

                    if(String.IsNullOrWhiteSpace(style))
                        style = "tag";

                    var tag = new XElement("strong", new XAttribute("class", style));

                    // If the notice text starts with '@', it's a content item
                    if(noticeText[0] == '@')
                        tag.Add(new XElement("include", new XAttribute("item", noticeText.Substring(1))));
                    else
                        tag.Add(noticeText);

                    notices.Add(tag);
                }
            }

            if(notices.Count != 0)
            {
                if(!parent.IsEmpty)
                    parent.Add(new XElement("br"));

                foreach(var n in notices)
                    parent.Add(n, " ");
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
                    {
                        var div = new XElement("div", new XAttribute("class", "summary"));

                        transformation.CurrentElement.Add(div);
                        transformation.RenderChildElements(div, overloads.Nodes());
                    }
                }
            }
        }

        /// <summary>
        /// Render the inheritance hierarchy section
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="fullHierarchy">True for a full hierarchy, false for an abbreviated one (four descendants
        /// maximum).  If full, it will only render a full one if the abbreviated hierarchy section is not
        /// present or if the descendant count exceeds four.  The abbreviated hierarchy renders a "More..." link
        /// to a full hierarchy section with a <c>fullInheritance</c> ID that is assumed to be elsewhere in the
        /// topic that shows all descendants.</param>
        private static void RenderApiInheritanceHierarchy(TopicTransformationCore transformation, bool fullHierarchy)
        {
            var family = transformation.ReferenceNode.Element("family");

            if(family == null)
                return;

            var descendants = family.Element("descendents");
            int descendantCount = descendants?.Elements().Count() ?? 0;

            // If an abbreviated hierarchy section is present and there are less than 5 descendants, skip the
            // full hierarchy.
            if(fullHierarchy && descendantCount < 5 && transformation.ApiTopicSectionHandlerFor(
              ApiTopicSectionType.InheritanceHierarchyAbbreviated, null) != null)
            {
                return;
            }

            var (title, content) = transformation.CreateSection(family.GenerateUniqueId(), true,
                "title_family", fullHierarchy ? "fullInheritance" : null);

            transformation.CurrentElement.Add(title);
            transformation.CurrentElement.Add(content);

            var ancestors = family.Element("ancestors");
            int indent = 0;

            if(ancestors != null)
            {
                // Ancestor types are stored nearest to most distant so reverse them
                foreach(var typeInfo in ancestors.Elements().Reverse())
                {
                    if(indent > 0)
                        content.Add(indent.ToIndent());

                    transformation.RenderTypeReferenceLink(content, typeInfo, true);
                    content.Add(new XElement("br"));

                    indent++;
                }
            }

            if(indent > 0)
            {
                content.Add(indent.ToIndent());
                indent++;
            }

            content.Add(new XElement("referenceLink",
                    new XAttribute("target", transformation.Key),
                    new XAttribute("show-container", true)),
                new XElement("br"));

            if(descendants != null)
            {
                if(!fullHierarchy && descendantCount > 4)
                {
                    content.Add(indent.ToIndent());
                    content.Add(new XElement("a",
                        new XAttribute("href", "#fullInheritance"),
                        new XElement("include",
                            new XAttribute("item", "text_moreInheritance"))));
                }
                else
                {
                    foreach(var typeInfo in descendants.Elements().OrderBy(e => e.Attribute("api")?.Value))
                    {
                        content.Add(indent.ToIndent());
                        transformation.RenderTypeReferenceLink(content, typeInfo, true);
                        content.Add(new XElement("br"));
                    }
                }
            }
        }

        /// <summary>
        /// This is used to render namespace and assembly information for an API topic
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        private static void RenderApiNamespaceAndAssemblyInformation(TopicTransformationCore transformation)
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

            transformation.CurrentElement.Add(new XElement("br"),
                new XElement("strong", new XElement("include", new XAttribute("item", "boilerplate_requirementsNamespace"))),
                Element.NonBreakingSpace,
                new XElement("referenceLink",
                    new XAttribute("target", containers.Element("namespace").Attribute("api").Value)),
                new XElement("br"));

            int separatorSize = 1;
            bool first = true;

            if(libraries.Count() > 1)
            {
                transformation.CurrentElement.Add(new XElement("strong",
                    new XElement("include", new XAttribute("item", "boilerplate_requirementsAssemblies"))));
                separatorSize = 2;
            }
            else
            {
                transformation.CurrentElement.Add(new XElement("strong",
                    new XElement("include", new XAttribute("item", "boilerplate_requirementsAssemblyLabel"))));
            }

            string separator = new String(Element.NonBreakingSpace, separatorSize);
            int maxVersionParts = ((VisualStudio2013Transformation)transformation).MaxVersionParts;

            foreach(var l in libraries)
            {
                if(!first)
                    transformation.CurrentElement.Add(new XElement("br"));

                transformation.CurrentElement.Add(separator);

                string version = l.Element("assemblydata").Attribute("version").Value,
                    extension = l.Attribute("kind").Value.Equals(
                        "DynamicallyLinkedLibrary", StringComparison.Ordinal) ? "dll" : "exe";
                string[] versionParts = version.Split(VersionNumberSeparators, StringSplitOptions.RemoveEmptyEntries);

                // Limit the version number parts if requested
                if(maxVersionParts > 1 && maxVersionParts < 5)
                    version = String.Join(".", versionParts, 0, maxVersionParts);

                transformation.CurrentElement.Add(new XElement("include",
                        new XAttribute("item", "assemblyNameAndModule"),
                    new XElement("parameter", l.Attribute("assembly").Value),
                    new XElement("parameter", l.Attribute("module").Value),
                    new XElement("parameter", extension),
                    new XElement("parameter", version)));

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

                transformation.CurrentElement.Add(new XElement("br"),
                    new XElement("strong",
                        new XElement("include", new XAttribute("item", "boilerplate_xamlXmlnsRequirements"))),
                    Element.NonBreakingSpace);

                if(xamlXmlNS.Any())
                {
                    first = true;

                    foreach(var d in xamlXmlNS)
                    {
                        if(!first)
                            transformation.CurrentElement.Add(", ");

                        transformation.CurrentElement.Add(d.Value.NormalizeWhiteSpace());
                        first = false;
                    }
                }
                else
                {
                    transformation.CurrentElement.Add(new XElement("include",
                        new XAttribute("item", "boilerplate_unmappedXamlXmlns")));
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

                case var t when t.ApiTopicGroup == ApiMemberGroup.Type || t.ApiTopicGroup == ApiMemberGroup.List:
                    RenderApiTypeMemberLists(transformation);
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

            var (title, content) = transformation.CreateSection(elements[0].GenerateUniqueId(), true, "title_namespaces", null);

            transformation.CurrentElement.Add(title);
            transformation.CurrentElement.Add(content);

            var table = new XElement("table",
                    new XAttribute("id", "namespaceList"),
                    new XAttribute("class", "members"),
                new XElement("tr",
                    new XElement("th",
                        new XElement("include", new XAttribute("item", "header_namespaceName"))),
                    new XElement("th",
                        new XElement("include", new XAttribute("item", "header_namespaceDescription")))));

            content.Add(table);

            foreach(var e in elements)
            {
                string name = e.Element("apidata").Attribute("name").Value;
                var refLink = new XElement("referenceLink",
                    new XAttribute("target", e.Attribute("api").Value),
                    new XAttribute("qualified", "false"));
                var summaryCell = new XElement("td");

                if(name.Length == 0)
                    refLink.Add(new XElement("include", new XAttribute("item", "defaultNamespace")));

                table.Add(new XElement("tr",
                    new XElement("td", refLink),
                    summaryCell));

                var summary = e.Element("summary");

                if(summary != null)
                    transformation.RenderChildElements(summaryCell, summary.Nodes());
                else
                    summaryCell.Add(Element.NonBreakingSpace);
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

            var (title, content) = transformation.CreateSection(elements[0].GenerateUniqueId(), true,
                "tableTitle_namespace", null);

            transformation.CurrentElement.Add(title);
            transformation.CurrentElement.Add(content);

            var table = new XElement("table",
                    new XAttribute("id", "namespaceList"),
                    new XAttribute("class", "members"),
                new XElement("tr",
                    new XElement("th",
                        new XElement("include", new XAttribute("item", "header_namespaceName"))),
                    new XElement("th",
                        new XElement("include", new XAttribute("item", "header_namespaceDescription")))));

            content.Add(table);

            foreach(var e in elements)
            {
                var summaryCell = new XElement("td");

                table.Add(new XElement("tr",
                    new XElement("td",
                        new XElement("referenceLink",
                            new XAttribute("target", e.Attribute("api").Value),
                            new XAttribute("qualified", "false"))),
                    summaryCell));

                var summary = e.Element("summary");

                if(summary != null)
                    transformation.RenderChildElements(summaryCell, summary.Nodes());
                else
                    summaryCell.Add(Element.NonBreakingSpace);
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
                    var (title, content) = transformation.CreateSection(group.First().GenerateUniqueId(), true,
                        "tableTitle_" + key, null);

                    transformation.CurrentElement.Add(title);
                    transformation.CurrentElement.Add(content);

                    var table = new XElement("table",
                            new XAttribute("id", key + "List"),
                            new XAttribute("class", "members"),
                        new XElement("tr",
                            new XElement("th",
                                new XAttribute("class", "iconColumn"), Element.NonBreakingSpace),
                            new XElement("th",
                                new XElement("include", new XAttribute("item", $"header_{key}Name"))),
                            new XElement("th",
                                new XElement("include", new XAttribute("item", "header_typeDescription")))));

                    content.Add(table);

                    foreach(var e in group.OrderBy(el => el.Attribute("api").Value))
                    {
                        string visibility;

                        switch(e.Element("typedata").Attribute("visibility").Value)
                        {
                            case "family":
                            case "family or assembly":
                            case "assembly":
                                visibility = "prot";
                                break;

                            case "private":
                                visibility = "priv";
                                break;

                            default:
                                visibility = "pub";
                                break;
                        }

                        XElement codeExampleImage = null;

                        if(e.Descendants("example").Any())
                        {
                            codeExampleImage = new XElement("img",
                                new XAttribute("src", $"{transformation.IconPath}CodeExample.png"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "alt"),
                                    new XAttribute("item", "altText_CodeExample")),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "title"),
                                    new XAttribute("item", "altText_CodeExample")));
                        }

                        var summaryCell = new XElement("td");

                        table.Add(new XElement("tr",
                            new XElement("td",
                                new XElement("img",
                                    new XAttribute("src",
                                        $"{transformation.IconPath}{visibility}{Char.ToUpperInvariant(key[0])}{key.Substring(1)}.gif"),
                                    new XElement("includeAttribute",
                                        new XAttribute("name", "alt"),
                                        new XAttribute("item", $"altText_{visibility}{key}")),
                                    new XElement("includeAttribute",
                                        new XAttribute("name", "title"),
                                        new XAttribute("item", $"altText_{visibility}{key}"))),
                                codeExampleImage),
                            new XElement("td",
                                new XElement("referenceLink",
                                    new XAttribute("target", e.Attribute("api").Value),
                                    new XAttribute("qualified", "false"))),
                            summaryCell));

                        var summary = e.Element("summary");

                        if(summary != null)
                            transformation.RenderChildElements(summaryCell, summary.Nodes());

                        RenderNoticeTags(transformation, e, summaryCell);
                        
                        if(summaryCell.IsEmpty)
                            summaryCell.Add(Element.NonBreakingSpace);
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
            var thisTransform = (VisualStudio2013Transformation)transformation;

            var allMembers = transformation.ReferenceNode.Element("elements")?.Elements("element").ToList();

            if(allMembers == null)
                return;

            List<XElement> fieldMembers = new List<XElement>(), extensionsMethods = new List<XElement>();

            // Enumerations can have extension methods which need to be rendered in a separate section
            foreach(var m in allMembers)
            {
                XElement apiData = m.Element("apidata");

                // Some members such as inherited interface members on a derived interface, contain no
                // metadata and we'll ignore them.
                if(apiData == null)
                    continue;

                if(Enum.TryParse<ApiMemberGroup>(apiData.Attribute("subgroup")?.Value, true, out var subgroup) &&
                  subgroup == ApiMemberGroup.Field)
                {
                    fieldMembers.Add(m);
                }
                else
                    extensionsMethods.Add(m);
            }

            if(fieldMembers.Count != 0)
            {
                // Sort order is configurable for enumeration members
                EnumMemberSortOrder enumMemberSortOrder = thisTransform.EnumMemberSortOrder;

                var elements = fieldMembers.OrderBy(el => enumMemberSortOrder == EnumMemberSortOrder.Name ?
                    el.Element("apidata").Attribute("name").Value :
                    el.Element("value").Value.PadLeft(20, ' ')).ToList();

                var enumValues = elements.Select(e => e.Element("value").Value).ToList();
                bool includeEnumValues = thisTransform.IncludeEnumValues;
                int idx;

                if(includeEnumValues)
                {
                    EnumValueFormat enumFormat = thisTransform.FlagsEnumValueFormat;
                    int groupSize = thisTransform.IncludeIntegerEnumSeparators ? 3 : 0, minWidth = 0;
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

                var (title, content) = thisTransform.CreateSection(elements.First().GenerateUniqueId(), true,
                    "topicTitle_enumMembers", null);

                thisTransform.CurrentElement.Add(title);
                thisTransform.CurrentElement.Add(content);

                XElement valueHeaderCell = null;

                if(includeEnumValues)
                {
                    valueHeaderCell = new XElement("th",
                        new XElement("include", new XAttribute("item", "header_memberValue")));
                }

                var table = new XElement("table",
                        new XAttribute("id", "enumMemberList"),
                        new XAttribute("class", "members"),
                    new XElement("tr",
                        new XElement("th",
                            new XElement("include", new XAttribute("item", "header_memberName"))),
                        valueHeaderCell,
                        new XElement("th",
                            new XElement("include", new XAttribute("item", "header_memberDescription")))));

                content.Add(table);
                idx = 0;

                foreach(var e in elements)
                {
                    var summaryCell = new XElement("td");
                    XElement valueCell = null;

                    if(includeEnumValues)
                    {
                        valueCell = new XElement("td", enumValues[idx]);
                        idx++;
                    }

                    table.Add(new XElement("tr",
                        new XElement("td", e.Element("apidata").Attribute("name").Value),
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

                    RenderNoticeTags(transformation, e, summaryCell);

                    if(summaryCell.IsEmpty)
                        summaryCell.Add(Element.NonBreakingSpace);
                }
            }

            if(extensionsMethods.Count != 0)
                RenderApiTypeMemberLists(transformation);
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
                transformation.CurrentElement.Add(new XElement("p",
                    new XElement("include",
                        new XAttribute("item", "exposedMembersTableText"),
                        new XElement("parameter",
                            new XElement("referenceLink", new XAttribute("target",
                                transformation.ApiMember.TypeTopicId))))));

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

            // When called for an enumeration's extension methods, ignore fields as they've already been rendered
            if(transformation.ApiMember.ApiTopicSubgroup == ApiMemberGroup.Enumeration)
                memberGroups[ApiMemberGroup.Field].Clear();

            // Render each section with at least one member
            foreach(var memberType in new[] { ApiMemberGroup.Constructor, ApiMemberGroup.Property,
                ApiMemberGroup.Method, ApiMemberGroup.Event, ApiMemberGroup.Operator, ApiMemberGroup.Field,
                ApiMemberGroup.AttachedProperty, ApiMemberGroup.AttachedEvent, ApiMemberGroup.Extension,
                ApiMemberGroup.ExplicitInterfaceImplementation, ApiMemberGroup.Overload })
            {
                var members = memberGroups[memberType];

                if(members.Count == 0)
                    continue;

                var (title, content) = transformation.CreateSection(members.First().GenerateUniqueId(), true,
                    "tableTitle_" + memberType.ToString(), null);

                transformation.CurrentElement.Add(title);
                transformation.CurrentElement.Add(content);

                var table = new XElement("table",
                        new XAttribute("id", memberType + "List"),
                        new XAttribute("class", "members"),
                    new XElement("tr",
                        new XElement("th",
                            new XAttribute("class", "iconColumn"), Element.NonBreakingSpace),
                        new XElement("th",
                            new XElement("include", new XAttribute("item", $"header_typeName"))),
                        new XElement("th",
                            new XElement("include", new XAttribute("item", "header_typeDescription")))));

                content.Add(table);

                // Sort by EII name if present else the member name and then by template count
                foreach(var e in members.OrderBy(el => el.Element("topicdata")?.Attribute("eiiName")?.Value ??
                    el.Element("apidata")?.Attribute("name").Value ?? String.Empty).ThenBy(
                    el => el.Element("templates")?.Elements()?.Count() ?? 0))
                {
                    string visibility;

                    switch(e.Element("memberdata")?.Attribute("visibility").Value)
                    {
                        case "family":
                        case "family or assembly":
                        case "assembly":
                            visibility = "prot";
                            break;

                        case "private":
                            visibility = "priv";
                            break;

                        default:
                            visibility = "pub";
                            break;
                    }

                    XElement codeExampleImage = null, staticImage = null, eiiImage = null,
                        referenceLink = new XElement("referenceLink",
                            new XAttribute("target", e.Attribute("api").Value));
                    string showParameters = (!((VisualStudio2013Transformation)transformation).ShowParametersOnAllMethods &&
                        transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload &&
                        e.Element("memberdata").Attribute("overload") == null &&
                        !(e.Parent.Attribute("api")?.Value ?? String.Empty).StartsWith(
                            "Overload:", StringComparison.Ordinal)) ? "false" : "true";
                    bool isExtensionMethod = e.AttributeOfType("T:System.Runtime.CompilerServices.ExtensionAttribute") != null;

                    if(e.Descendants("example").Any())
                    {
                        codeExampleImage = new XElement("img",
                            new XAttribute("src", $"{transformation.IconPath}CodeExample.png"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "alt"),
                                new XAttribute("item", "altText_CodeExample")),
                            new XElement("includeAttribute",
                                new XAttribute("name", "title"),
                                new XAttribute("item", "altText_CodeExample")));
                    }

                    if(!isExtensionMethod && memberType != ApiMemberGroup.AttachedEvent &&
                      memberType != ApiMemberGroup.AttachedProperty &&
                      e.Element("memberdata")?.Attribute("static")?.Value == "true")
                    {
                        staticImage = new XElement("img",
                            new XAttribute("src", $"{transformation.IconPath}Static.gif"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "alt"),
                                new XAttribute("item", "altText_Static")),
                            new XElement("includeAttribute",
                                new XAttribute("name", "title"),
                                new XAttribute("item", "altText_Static")));
                    }

                    if(memberType == ApiMemberGroup.ExplicitInterfaceImplementation)
                    {
                        eiiImage = new XElement("img",
                            new XAttribute("src", $"{transformation.IconPath}pubInterface.gif"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "alt"),
                                new XAttribute("item", "altText_ExplicitInterface")),
                            new XElement("includeAttribute",
                                new XAttribute("name", "title"),
                                new XAttribute("item", "altText_ExplicitInterface")));
                    }

                    var summaryCell = new XElement("td");

                    if(!Enum.TryParse(e.Element("apidata")?.Attribute("subgroup")?.Value, true,
                      out ApiMemberGroup imageMemberType))
                    {
                        imageMemberType = memberType;
                    }

                    switch(memberType)
                    {
                        case var t when t == ApiMemberGroup.Operator &&
                          (e.Element("apidata")?.Attribute("name")?.Value == "Explicit" ||
                          e.Element("apidata")?.Attribute("name")?.Value == "Implicit"):
                            referenceLink.Add(new XAttribute("show-parameters", "true"));
                            imageMemberType = ApiMemberGroup.Operator;
                            break;

                        case var t when t == ApiMemberGroup.Operator:
                            imageMemberType = ApiMemberGroup.Operator;
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

                            imageMemberType = ApiMemberGroup.Extension;
                            break;

                        default:
                            if(imageMemberType == ApiMemberGroup.Constructor)
                                imageMemberType = ApiMemberGroup.Method;
                            else
                            {
                                if(imageMemberType == ApiMemberGroup.Method && Enum.TryParse(
                                  e.Element("apidata")?.Attribute("subsubgroup")?.Value, true,
                                  out ApiMemberGroup subGroupType) && subGroupType == ApiMemberGroup.Operator)
                                {
                                    imageMemberType = subGroupType;
                                }
                                else
                                {
                                    // Show the extension method icon for extension methods in their containing
                                    // type to match IntelliSense.
                                    if(isExtensionMethod && visibility == "pub")
                                        imageMemberType = ApiMemberGroup.Extension;
                                }
                            }

                            referenceLink.Add(new XAttribute("show-parameters", showParameters));
                            break;
                    }

                    table.Add(new XElement("tr",
                        new XElement("td",
                            eiiImage,
                            new XElement("img",
                                new XAttribute("src", $"{transformation.IconPath}{visibility}{imageMemberType}.gif"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "alt"),
                                    new XAttribute("item", $"altText_{visibility}{imageMemberType}")),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "title"),
                                    new XAttribute("item", $"altText_{visibility}{imageMemberType}"))),
                            staticImage,
                            codeExampleImage),
                        new XElement("td", referenceLink),
                        summaryCell));

                    var summary = e.Element("summary");

                    if(summary != null)
                        transformation.RenderChildElements(summaryCell, summary.Nodes());

                    if(transformation.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                    {
                        if(memberType == ApiMemberGroup.Extension)
                        {
                            var parameter = new XElement("parameter");

                            summaryCell.Add(new XElement("br"),
                                new XElement("include", new XAttribute("item", "definedBy"),
                                parameter));

                            transformation.RenderTypeReferenceLink(parameter, e.Element("containers").Element("type"), false);
                        }
                        else
                        {
                            if(transformation.ApiMember.TypeTopicId != e.Element("containers").Element("type").Attribute("api").Value)
                            {
                                var parameter = new XElement("parameter");

                                summaryCell.Add(new XElement("br"),
                                    new XElement("include", new XAttribute("item", "inheritedFrom"),
                                    parameter));

                                transformation.RenderTypeReferenceLink(parameter, e.Element("containers").Element("type"), false);
                            }
                            else
                            {
                                if(e.Element("overrides")?.Element("member") != null)
                                {
                                    var parameter = new XElement("parameter");

                                    summaryCell.Add(new XElement("br"),
                                        new XElement("include", new XAttribute("item", "overridesMember"),
                                        parameter));

                                    transformation.RenderTypeReferenceLink(parameter, e.Element("overrides").Element("member"), true);
                                }
                            }
                        }
                    }

                    RenderNoticeTags(transformation, e, summaryCell);

                    if(summaryCell.IsEmpty)
                        summaryCell.Add(Element.NonBreakingSpace);
                }

                content.Add(new XElement("a",
                    new XAttribute("href", "#PageHeader"),
                    new XElement("include", new XAttribute("item", "top"))));
            }
        }

        /// <summary>
        /// Render a section with a title and a table containing the element content
        /// </summary>
        /// <param name="transformation">The topic transformation to use</param>
        /// <param name="sectionTitleItem">The section title include item</param>
        /// <param name="typeColumnHeaderItem">The type column header item</param>
        /// <param name="descriptionColumnHeaderItem">The description column header item</param>
        /// <param name="sectionElements">An enumerable list of the elements to render in the table</param>
        private static void RenderApiSectionTable(TopicTransformationCore transformation, string sectionTitleItem,
          string typeColumnHeaderItem, string descriptionColumnHeaderItem, IEnumerable<XElement> sectionElements)
        {
            if(sectionElements.Any())
            {
                var (title, content) = transformation.CreateSection(sectionElements.First().GenerateUniqueId(), true,
                    sectionTitleItem, null);

                transformation.CurrentElement.Add(title);
                transformation.CurrentElement.Add(content);

                var table = new XElement("table",
                    new XElement("tr",
                        new XElement("th",
                            new XElement("include", new XAttribute("item", typeColumnHeaderItem))),
                        new XElement("th",
                            new XElement("include", new XAttribute("item", descriptionColumnHeaderItem)))));

                content.Add(table);

                foreach(var se in sectionElements)
                {
                    var descCell = new XElement("td");

                    table.Add(new XElement("tr",
                        new XElement("td",
                            new XElement("referenceLink",
                                new XAttribute("target", se.Attribute("cref")?.Value ?? String.Empty),
                                new XAttribute("qualified", "false"))),
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
                var (title, content) = transformation.CreateSection(revisionHistory.GenerateUniqueId(), true,
                    "title_revisionHistory", null);

                transformation.CurrentElement.Add(title);
                transformation.CurrentElement.Add(content);

                var table = new XElement("table",
                    new XElement("tr",
                        new XElement("th",
                            new XElement("include", new XAttribute("item", "header_revHistoryDate"))),
                        new XElement("th",
                            new XElement("include", new XAttribute("item", "header_revHistoryVersion"))),
                        new XElement("th",
                            new XElement("include", new XAttribute("item", "header_revHistoryDescription")))));

                content.Add(table);

                foreach(var rh in revisions)
                {
                    var descCell = new XElement("td");

                    table.Add(new XElement("tr",
                        new XElement("td", rh.Attribute("date")?.Value),
                        new XElement("td", rh.Attribute("version")?.Value),
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
                var (title, content) = transformation.CreateSection("seeAlso", true, "title_relatedTopics", null);

                transformation.CurrentElement.Add(title);
                transformation.CurrentElement.Add(content);

                var priorCurrentElement = transformation.CurrentElement;

                if(seeAlsoCRef.Count != 0 || transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Type ||
                  transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
                  transformation.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
                {
                    var (subtitle, subsection) = transformation.CreateSubsection(true, "title_seeAlso_reference");

                    if(subtitle != null)
                        content.Add(subtitle);

                    if(subsection != null)
                        content.Add(subsection);
                    else
                        subsection = content;

                    RenderApiAutoGeneratedSeeAlsoLinks(transformation, subsection);

                    if(seeHandler != null)
                    {
                        foreach(var s in seeAlsoCRef)
                        {
                            var div = new XElement("div");
                            subsection.Add(div);

                            transformation.CurrentElement = div;
                            seeHandler.Render(transformation, s);
                        }
                    }
                }

                if((seeAlsoHRef.Count != 0 && seeHandler != null) || (conceptualLinks.Count != 0 &&
                  conceptualLinkHandler != null))
                {
                    var (subtitle, subsection) = transformation.CreateSubsection(true, "title_seeAlso_otherResources");

                    if(subtitle != null)
                        content.Add(subtitle);

                    if(subsection != null)
                        content.Add(subsection);
                    else
                        subsection = content;

                    if(seeHandler != null)
                    {
                        foreach(var s in seeAlsoHRef)
                        {
                            var div = new XElement("div");
                            subsection.Add(div);

                            transformation.CurrentElement = div;
                            seeHandler.Render(transformation, s);
                        }
                    }

                    if(conceptualLinkHandler != null)
                    {
                        foreach(var c in conceptualLinks)
                        {
                            var div = new XElement("div");
                            subsection.Add(div);

                            transformation.CurrentElement = div;
                            conceptualLinkHandler.Render(transformation, c);
                        }
                    }
                }

                transformation.CurrentElement = priorCurrentElement;
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
                subsection.Add(new XElement("div",
                    new XElement("referenceLink",
                        new XAttribute("target", transformation.ApiMember.TypeTopicId),
                        new XAttribute("display-target", "format"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoTypeLink"),
                            new XElement("parameter", "{0}"),
                            new XElement("parameter", transformation.ApiMember.TypeApiSubgroup)))));
            }

            // Add a link to the overload topic
            if(!String.IsNullOrWhiteSpace(transformation.ApiMember.OverloadTopicId))
            {
                subsection.Add(new XElement("div",
                    new XElement("referenceLink",
                        new XAttribute("target", transformation.ApiMember.OverloadTopicId),
                        new XAttribute("display-target", "format"),
                        new XAttribute("show-parameters", "false"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoOverloadLink"),
                            new XElement("parameter", "{0}")))));
            }

            // Add a link to the namespace topic
            string namespaceId = transformation.ReferenceNode.Element("containers")?.Element("namespace")?.Attribute("api")?.Value;

            if(!String.IsNullOrWhiteSpace(namespaceId))
            {
                subsection.Add(new XElement("div",
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
