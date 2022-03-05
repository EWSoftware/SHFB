//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : VisualStudio2013Transformation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/04/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
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

        private readonly Dictionary<CommonStyle, string> commonStyles = new Dictionary<CommonStyle, string>();

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="supportedFormats">The help file formats supported by the presentation style</param>
        public VisualStudio2013Transformation(HelpFileFormats supportedFormats) : base(supportedFormats)
        {
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
        /// Base source code URL
        /// </summary>
        private string BaseSourceCodeUrl => this.TransformationArguments[nameof(BaseSourceCodeUrl)].Value;

        /// <summary>
        /// Request example URL
        /// </summary>
        private string RequestExampleUrl => this.TransformationArguments[nameof(RequestExampleUrl)].Value;

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
            this.AddTransformationArgumentRange(new[] {
                new TransformationArgument(nameof(RobotsMetadata), true, true, null,
                    "An optional robots metadata value (e.g. noindex, nofollow).  If left blank, the robots " +
                    "metadata element will be omitted from the topics."),
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
                new TransformationArgument(nameof(LogoPlacement), true, true, "left",
                    "An optional logo placement.  Specify left, right, or above.  If not specified, the " +
                    "default is left."),
                new TransformationArgument(nameof(LogoAlignment), true, true, "left",
                    "An optional logo alignment when using the 'above' placement option.  Specify left, " +
                    "right, or center.  If not specified, the default is left."),
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
                new TransformationArgument(nameof(IncludeEnumValues), false, true, "true",
                    "Set this to 'true' to include the column for the numeric value of each field in " +
                    "enumerated type topics.  Set it to 'false' to omit the numeric values column."),
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
                    "A specific commit: https://github.com/JohnDoe/WidgestProject/blob/c6e41c4fc2a4a335352d2ae8e7e85a1859751662/src/"),
                new TransformationArgument(nameof(RequestExampleUrl), false, true, null,
                    "To include a link that allows users to request an example for an API topic, set the URL " +
                    "to which the request will be sent.  This can be a web page URL or an e-mail URL.  Only include " +
                    "the URL as the parameters will be added automatically by the topic.  For example:\r\n\r\n" +
                    "Create a new issue on GitHub: https://github.com/YourUserID/YourProject/issues/new \r\n" +
                    "Send via e-mail: mailto:YourEmailAddress@Domain.com") });
        }

        /// <inheritdoc />
        protected override void CreateLanguageSpecificText()
        {
            LanguageSpecificText.KeywordStyleName = this.StyleNameFor(CommonStyle.Keyword);

            this.AddLanguageSpecificTextRange(new[] {
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
            this.AddElements(new Element[] {
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
                new CodeSnippetGroupElement(),
                new MarkupElement(),
                new ConvertibleElement("para", "p"),
                new ListElement(),
                new ParametersElement(),
                new PassthroughElement("span"),
                new SummaryElement(),
                new TableElement(),

                // MAML elements
                new NoteElement("alert"),
                new ConvertibleElement("application", "strong"),
                new NamedSectionElement("appliesTo"),
                new NonRenderedParentElement("attribute", 4, this.StyleNameFor(CommonStyle.SubHeading), null),
                new NonRenderedParentElement("attributes", 4, this.StyleNameFor(CommonStyle.SubHeading), "title_attributes"),
                new NamedSectionElement("attributesandElements"),
                new AutoOutlineElement(),
                new NamedSectionElement("background"),
                new NamedSectionElement("buildInstructions"),
                new NonRenderedParentElement("childElement", 4, this.StyleNameFor(CommonStyle.SubHeading), "title_childElement"),
                new CodeEntityReferenceElement(),
                new CodeExampleElement(),
                new ConvertibleElement("codeFeaturedElement", "span", this.StyleNameFor(CommonStyle.Label)),
                new ConvertibleElement("codeInline", "span", this.StyleNameFor(CommonStyle.Code)),
                new NonRenderedParentElement("codeReference"),
                new ConvertibleElement("command", "span", this.StyleNameFor(CommonStyle.Command)),
                new ConvertibleElement("computerOutputInline", "span", this.StyleNameFor(CommonStyle.Code)),
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
                new NamedSectionElement("elementInformation"),
                new ConvertibleElement("embeddedLabel", "span", this.StyleNameFor(CommonStyle.Label)),
                new EntryElement(),
                new ConvertibleElement("environmentVariable", "span", this.StyleNameFor(CommonStyle.Code)),
                new ConvertibleElement("errorInline", "em"),
                new NamedSectionElement("exceptions"),
                new ExternalLinkElement(),
                new NamedSectionElement("externalResources"),
                new ConvertibleElement("fictitiousUri", "em"),
                new ConvertibleElement("foreignPhrase", "span", this.StyleNameFor(CommonStyle.ForeignPhrase)),
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
                new ConvertibleElement("literal", "span", this.StyleNameFor(CommonStyle.Literal)),
                new ConvertibleElement("localUri", "em"),
                new NonRenderedParentElement("localizedText"),
                new ConvertibleElement("math", "span", this.StyleNameFor(CommonStyle.Math)),
                new MediaLinkElement(),
                new MediaLinkInlineElement(),
                new ConvertibleElement("newTerm", "span", this.StyleNameFor(CommonStyle.Term)),
                new NamedSectionElement("nextSteps"),
                new ConvertibleElement("parameterReference", "span", this.StyleNameFor(CommonStyle.Parameter)),
                new NonRenderedParentElement("parentElement", 4, this.StyleNameFor(CommonStyle.SubHeading), "title_parentElement"),
                new ConvertibleElement("phrase", "span", this.StyleNameFor(CommonStyle.Phrase)),
                new ConvertibleElement("placeholder", "span", this.StyleNameFor(CommonStyle.Placeholder)),
                new NamedSectionElement("prerequisites"),
                new ProcedureElement(),
                new ConvertibleElement("quote", "blockquote"),
                new ConvertibleElement("quoteInline", "q"),
                new NamedSectionElement("reference"),
                new NamedSectionElement("relatedSections"),
                new RelatedTopicsElement(),
                new ConvertibleElement("replaceable", "span", this.StyleNameFor(CommonStyle.Placeholder)),
                new NamedSectionElement("requirements"),
                new NamedSectionElement("returnValue"),
                new NamedSectionElement("robustProgramming"),
                new ConvertibleElement("row", "tr"),
                new SchemaHierarchyElement(),
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
                new IgnoredElement("title"),
                new NonRenderedParentElement("type"),
                new ConvertibleElement("ui", "span", this.StyleNameFor(CommonStyle.UI)),
                new ConvertibleElement("unmanagedCodeEntityReference", "strong"),
                new ConvertibleElement("userInput", "span", this.StyleNameFor(CommonStyle.Input)),
                new ConvertibleElement("userInputLocalizable", "span", this.StyleNameFor(CommonStyle.Input)),
                new NamedSectionElement("whatsNew"),

                // XML comments and reflection data file elements
                new ConvertibleElement("c", "span", this.StyleNameFor(CommonStyle.Code)),
                new PassthroughElement("conceptualLink"),
                new NamedSectionElement("example"),
                new ImplementsElement(),
                new NoteElement("note"),
                new ConvertibleElement("paramref", "name", "span", this.StyleNameFor(CommonStyle.Parameter)),
                new PreliminaryElement(),
                new NamedSectionElement("remarks"),
                new ReturnsElement(),
                new SeeElement("see"),
                // seeAlso should be a top-level element in the comments but may appear within other elements.
                // We'll ignore it if seen as they'll be handled manually by the See Also section processing.
                new IgnoredElement("seealso"),
                new SourceContextElement(nameof(RequestExampleUrl), nameof(BaseSourceCodeUrl)),
                new SyntaxElement(),
                new TemplatesElement(),
                new ThreadsafetyElement(),
                new ConvertibleElement("typeparamref", "name", "span", this.StyleNameFor(CommonStyle.TypeParameter)),
                new ValueElement(),
                new VersionsElement()
            });
        }

        /// <inheritdoc />
        public override void RenderTypeReferenceLink(XElement content, XElement typeInfo, bool qualified)
        {
            if(content == null)
                throw new ArgumentNullException(nameof(content));

            if(typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            var specialization = typeInfo.Element("specialization");
            string api = typeInfo.Attribute("api")?.Value, name = typeInfo.Attribute("name")?.Value,
                displayApi = typeInfo.Attribute("display-api")?.Value;
            bool first = true;

            switch(typeInfo.Name.LocalName)
            {
                case "type":
                    content.Add(new XElement("referenceLink",
                        new XAttribute("target", api),
                        new XAttribute("prefer-overload", false),
                        new XAttribute("show-templates", specialization == null),
                        new XAttribute("show-container", qualified)));

                    if(specialization != null)
                        this.RenderTypeReferenceLink(content, specialization, false);
                    break;

                case "specialization":
                case "templates":
                    content.Add(LanguageSpecificText.TypeSpecializationOpening.Render());

                    foreach(var t in typeInfo.Elements())
                    {
                        if(!first)
                            content.Add(", ");

                        this.RenderTypeReferenceLink(content, t, false);

                        first = false;
                    }

                    content.Add(LanguageSpecificText.TypeSpecializationClosing.Render());
                    break;

                case "template":
                    if(!String.IsNullOrWhiteSpace(api))
                    {
                        content.Add(new XElement("referenceLink",
                                new XAttribute("target", api),
                            new XElement("span",
                                this.StyleAttributeFor(CommonStyle.TypeParameter),
                            name)));
                    }
                    else
                        content.Add(new XElement("span", this.StyleAttributeFor(CommonStyle.TypeParameter), name));
                    break;

                case "arrayOf":
                    LanguageSpecificText arrayOfClosing;

                    if(Int32.TryParse(typeInfo.Attribute("rank")?.Value, out int rank) && rank > 1)
                    {
                        arrayOfClosing = new LanguageSpecificText(false, new[]
                        {
                            (LanguageSpecificText.CPlusPlus, $",{rank}>"),
                            (LanguageSpecificText.VisualBasic, $"({rank})"),
                            (LanguageSpecificText.Neutral, $"[{rank}]")
                        });
                    }
                    else
                    {
                        arrayOfClosing = new LanguageSpecificText(false, new[]
                        {
                            (LanguageSpecificText.CPlusPlus, ">"),
                            (LanguageSpecificText.VisualBasic, "()"),
                            (LanguageSpecificText.Neutral, "[]")
                        });
                    }

                    content.Add(LanguageSpecificText.ArrayOfOpening.Render());
                    this.RenderTypeReferenceLink(content, typeInfo.Elements().First(), true);
                    content.Add(arrayOfClosing.Render());
                    break;

                case "pointerTo":
                    this.RenderTypeReferenceLink(content, typeInfo.Elements().First(), true);
                    content.Add("*");
                    break;

                case "referenceTo":
                    this.RenderTypeReferenceLink(content, typeInfo.Elements().First(), true);
                    content.Add(LanguageSpecificText.ReferenceTo.Render());
                    break;

                case "member":
                    if(!String.IsNullOrWhiteSpace(displayApi))
                    {
                        content.Add(new XElement("referenceLink",
                            new XAttribute("target", api),
                            new XAttribute("display-target", displayApi),
                            new XAttribute("show-container", qualified)));
                    }
                    else
                    {
                        content.Add(new XElement("referenceLink",
                            new XAttribute("target", api),
                            new XAttribute("show-container", qualified)));
                    }
                    break;

                default:
                    Debug.WriteLine("Unhandled type element: {0}", typeInfo.Name.LocalName);

                    if(Debugger.IsAttached)
                        Debugger.Break();
                    break;
            }
        }

        /// <inheritdoc />
        protected override XDocument RenderTopic()
        {
            // Create the document, header content, and body containing the topic header content
            var document = new XDocument();
            var html = new XElement("html",
                this.RenderHeaderMetadata());
            var body = new XElement("body",
                    new XAttribute("onload", $"OnLoad('{this.DefaultLanguage}')"),
                new XElement("input",
                    new XAttribute("type", "hidden"),
                    new XAttribute("id", "userDataCache"),
                    new XAttribute("class", "userDataStyle")),
                new XElement("div",
                    new XAttribute("class", "pageHeader"),
                    new XAttribute("id", "PageHeader"),
                        new XElement("include",
                            new XAttribute("item", "runningHeaderText"))));
            var pageBody = new XElement("div",
                new XAttribute("class", "pageBody"));
            var topicContent = new XElement("div",
                new XAttribute("class", "topicContent"),
                new XAttribute("id", "TopicContent"),
                    this.RenderPageTitleAndLogo());

            document.Add(html);
            html.Add(body);
            body.Add(pageBody);
            pageBody.Add(topicContent);

            topicContent.Add(new XElement("include",
                new XAttribute("item", "header")));

            // Add the topic content.  MAML topics are rendered purely off of the element types.  API topics
            // require custom formatting based on the member type in the topic.
            this.CurrentElement = topicContent;

            if(this.IsMamlTopic)
                this.RenderNode(this.TopicNode);
            else
                this.RenderApiTopicBody();

            // Add the topic footer content
            body.Add(new XElement("div",
                new XAttribute("id", "pageFooter"),
                new XAttribute("class", "pageFooter"),
                    new XElement("include",
                        new XAttribute("item", "footer_content"))),
                new XElement("include",
                    new XAttribute("item", "websiteAdContent")));

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
                        this.StyleAttributeFor(CommonStyle.CollapsibleAreaRegion),
                    new XElement("span",
                        this.StyleAttributeFor(CommonStyle.CollapsibleRegionTitle),
                        new XAttribute("onclick", "SectionExpandCollapse('" + uniqueId + "')"),
                        new XAttribute("onkeypress", "SectionExpandCollapse_CheckKey('" + uniqueId + "', event)"),
                        new XAttribute("tabindex", "0"),
                            new XElement("img",
                                new XAttribute("id", toggleImageId),
                                this.StyleAttributeFor(CommonStyle.CollapseToggle),
                                new XAttribute("src", this.IconPath + "SectionExpanded.png")),
                            titleContent));

                if(!String.IsNullOrWhiteSpace(linkId))
                    titleElement.Add(new XAttribute("id", linkId));
            }

            var contentElement = new XElement("div",
                new XAttribute("id", toggleSectionId),
                this.StyleAttributeFor(CommonStyle.CollapsibleSection));

            return (titleElement, contentElement);
        }

        /// <inheritdoc />
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

                titleElement = new XElement("h4",
                    this.StyleAttributeFor(CommonStyle.SubHeading),
                    titleContent);
            }

            // Content for subsections can be added to the current element immediately after the title element
            // by the caller.
            return (titleElement, null);
        }

        /// <inheritdoc />
        protected override IEnumerable<XNode> ApiTopicShortNameDecorated()
        {
            // This isn't returned, just its content
            XElement nameElement = new XElement("name");

            switch(this.ApiMember)
            {
                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiGroup == ApiMemberGroup.Type) ||
                  (t.TopicGroup == ApiMemberGroup.List && t.TopicSubgroup != ApiMemberGroup.Overload):
                    // Type overview pages and member list pages get the type name
                    this.ApiTypeNameDecorated(nameElement, this.ReferenceNode);
                    break;

                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiSubgroup == ApiMemberGroup.Constructor) ||
                  (t.TopicSubgroup == ApiMemberGroup.Overload && t.ApiSubgroup == ApiMemberGroup.Constructor):
                    // Constructors and member list pages also use the type name
                    this.ApiTypeNameDecorated(nameElement, this.ReferenceNode.Element("containers").Element("type"));
                    break;

                case var t when t.IsExplicitlyImplemented:
                    // EII members
                    this.ApiTypeNameDecorated(nameElement, this.ReferenceNode.Element("containers").Element("type"));
                    
                    nameElement.Add(LanguageSpecificText.NameSeparator.Render());

                    var member = this.ReferenceNode.Element("implements").Element("member");

                    this.ApiTypeNameDecorated(nameElement, member.Element("type"));

                    nameElement.Add(LanguageSpecificText.NameSeparator.Render());

                    // If the API element is not present (unresolved type), show the type name from the type element
                    if(!String.IsNullOrWhiteSpace(t.Name))
                        nameElement.Add(t.Name);
                    else
                    {
                        string name = member.Attribute("api")?.Value;

                        if(name != null)
                        {
                            int pos = name.LastIndexOf('.');

                            if(pos != -1)
                                name = name.Substring(pos + 1);

                            nameElement.Add(name);
                        }
                    }

                    var templates = member.Element("templates");

                    if(templates != null)
                        this.ApiTypeNameDecorated(nameElement, templates);
                    break;

                case var t when t.TopicGroup == ApiMemberGroup.List && t.TopicSubgroup == ApiMemberGroup.Overload &&
                  this.ReferenceNode.Element("templates") != null:
                    // Use just the plain, unadorned Type.API name for overload pages with templates
                    this.ApiTypeNameDecorated(nameElement, this.ReferenceNode.Element("containers").Element("type"));

                    nameElement.Add(LanguageSpecificText.NameSeparator.Render());
                    nameElement.Add(t.Name);
                    break;

                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiGroup == ApiMemberGroup.Member) ||
                  (t.TopicSubgroup == ApiMemberGroup.Overload && t.ApiGroup == ApiMemberGroup.Member):
                    // Normal member pages use the qualified member name
                    this.ApiTypeNameDecorated(nameElement, this.ReferenceNode.Element("containers").Element("type"));

                    if(t.ApiSubSubgroup == ApiMemberGroup.Operator &&
                      (t.Name.Equals("Explicit", StringComparison.Ordinal) ||
                       t.Name.Equals("Implicit", StringComparison.Ordinal)))
                    {
                        nameElement.Add(" ",
                            new XElement("span",
                                new XAttribute("class", LanguageSpecificText.LanguageSpecificTextStyleName),
                                new XElement("span",
                                    new XAttribute("class", LanguageSpecificText.VisualBasic),
                                    t.Name.Equals("Explicit", StringComparison.Ordinal) ? "Narrowing" : "Widening",
                                    " ", t.Name),
                                new XElement("span",
                                    new XAttribute("class", LanguageSpecificText.Neutral),
                                    t.Name)));
                    }
                    else
                    {
                        nameElement.Add(LanguageSpecificText.NameSeparator.Render(), t.Name);
                    }

                    templates = this.ReferenceNode.Element("templates");

                    if(templates != null)
                        this.ApiTypeNameDecorated(nameElement, templates);
                    break;

                case var t when (String.IsNullOrWhiteSpace(t.Name)):
                    // Default namespace
                    nameElement.Add(new XElement("include", new XAttribute("item", "defaultNamespace")));
                    break;

                default:
                    // Namespaces and other members just use the name
                    nameElement.Add(this.ApiMember.Name);
                    break;
            }

            return nameElement.Nodes();
        }

        /// <inheritdoc />
        protected override void ApiTypeNameDecorated(XElement memberName, XElement typeInfo)
        {
            if(memberName == null)
                throw new ArgumentNullException(nameof(memberName));

            if(typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            var specialization = typeInfo.Element("specialization");
            var templates = typeInfo.Element("templates");
            string name = typeInfo.Attribute("name")?.Value, api = typeInfo.Attribute("api")?.Value,
                apiDataName = typeInfo.Element("apidata")?.Attribute("name")?.Value;
            bool first = true;

            switch(typeInfo.Name.LocalName)
            {
                case "reference":
                case "type":
                    if(typeInfo.Name.LocalName == "reference")
                    {
                        // Don't show the type on list pages
                        if(this.ApiMember.TopicGroup != ApiMemberGroup.List)
                        {
                            var typeNode = typeInfo.Element("type");

                            if(typeNode == null)
                            {
                                typeNode = typeInfo.Element("containers")?.Element("type");

                                if(typeNode != null)
                                {
                                    this.ApiTypeNameDecorated(memberName, typeNode);
                                    memberName.Add(LanguageSpecificText.NameSeparator.Render());
                                }
                            }
                        }
                    }
                    else
                    {
                        // Add nested type name if necessary
                        var nestedType = typeInfo.Element("type") ?? typeInfo.Element("container")?.Element("type");

                        if(nestedType != null)
                        {
                            this.ApiTypeNameDecorated(memberName, nestedType);
                            memberName.Add(LanguageSpecificText.NameSeparator.Render());
                        }
                    }

                    // If the API element is not present (unresolved type), show the type name from the type element
                    if(!String.IsNullOrWhiteSpace(apiDataName))
                        memberName.Add(apiDataName);
                    else
                    {
                        if(api != null)
                        {
                            int pos = api.LastIndexOf('.');

                            if(pos != -1)
                                api = api.Substring(pos + 1);

                            memberName.Add(api);
                        }
                    }

                    if(specialization != null)
                        this.ApiTypeNameDecorated(memberName, specialization);
                    else
                    {
                        if(templates != null)
                            this.ApiTypeNameDecorated(memberName, templates);
                    }
                    break;

                case "specialization":
                case "templates":
                    memberName.Add(LanguageSpecificText.TypeSpecializationOpening.Render());

                    foreach(var t in typeInfo.Elements())
                    {
                        if(!first)
                            memberName.Add(", ");

                        this.ApiTypeNameDecorated(memberName, t);

                        first = false;
                    }

                    memberName.Add(LanguageSpecificText.TypeSpecializationClosing.Render());
                    break;

                case "template":
                    memberName.Add(new XElement("span", this.StyleAttributeFor(CommonStyle.TypeParameter), name));
                    break;

                case "arrayOf":
                    LanguageSpecificText arrayOfClosing;

                    if(Int32.TryParse(typeInfo.Attribute("rank")?.Value, out int rank) && rank > 1)
                    {
                        arrayOfClosing = new LanguageSpecificText(false, new[]
                        {
                            (LanguageSpecificText.CPlusPlus, $",{rank}>"),
                            (LanguageSpecificText.VisualBasic, $"({rank})"),
                            (LanguageSpecificText.Neutral, $"[{rank}]")
                        });
                    }
                    else
                    {
                        arrayOfClosing = new LanguageSpecificText(false, new[]
                        {
                            (LanguageSpecificText.CPlusPlus, ">"),
                            (LanguageSpecificText.VisualBasic, "()"),
                            (LanguageSpecificText.Neutral, "[]")
                        });
                    }

                    memberName.Add(LanguageSpecificText.ArrayOfOpening.Render());
                    this.ApiTypeNameDecorated(memberName, typeInfo.Elements().First());
                    memberName.Add(arrayOfClosing.Render());
                    break;

                case "pointerTo":
                    this.ApiTypeNameDecorated(memberName, typeInfo.Elements().First());
                    memberName.Add("*");
                    break;

                case "referenceTo":
                    this.ApiTypeNameDecorated(memberName, typeInfo.Elements().First());
                    memberName.Add(LanguageSpecificText.ReferenceTo.Render());
                    break;

                default:
                    Debug.WriteLine("Unhandled type element: {0}", typeInfo.Name.LocalName);

                    if(Debugger.IsAttached)
                        Debugger.Break();
                    break;
            }
        }

        /// <inheritdoc />
        /// <remarks>For the most part, we just convert the first letter to lowercase but a few are all lowercase</remarks>
        public override string StyleNameFor(CommonStyle style)
        {
            if(!commonStyles.TryGetValue(style, out string styleName))
            {
                styleName = style.ToString();

                switch(style)
                {
                    case CommonStyle.TypeParameter:
                    case CommonStyle.NoBullet:
                    case CommonStyle.NoLink:
                    case CommonStyle.SelfLink:
                    case CommonStyle.UI:
                        styleName = styleName.ToLowerInvariant();
                        break;

                    default:
                        var styleChars = styleName.ToCharArray();
                        styleChars[0] = Char.ToLowerInvariant(styleChars[0]);
                        styleName = new string(styleChars);
                        break;
                }

                commonStyles.Add(style, styleName);
            }

            return styleName;
        }

        /// <inheritdoc />
        public override XAttribute StyleAttributeFor(CommonStyle style)
        {
            return new XAttribute("class", this.StyleNameFor(style));
        }
        #endregion

        #region General topic rendering helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create the <c>head</c> element containing all of the topic metadata
        /// </summary>
        private XElement RenderHeaderMetadata()
        {
            string topicDesc;
            
            var head = new XElement("head");

            head.Add(new XElement("link",
                    new XAttribute("rel", "shortcut icon"),
                    new XAttribute("href", this.IconPath + "favicon.ico")),
                new XElement("link",
                    new XAttribute("rel", "stylesheet"),
                    new XAttribute("type", "text/css"),
                    new XAttribute("href", this.StyleSheetPath + "branding.css")),
                new XElement("link",
                    new XAttribute("rel", "stylesheet"),
                    new XAttribute("type", "text/css"),
                    new XAttribute("href", this.StyleSheetPath + $"branding-{this.Locale}.css")),
                new XElement("script",
                    new XAttribute("type", "text/javascript"),
                    new XAttribute("src", this.ScriptPath + "branding.js"), String.Empty),
                new XElement("meta",
                    new XAttribute("http-equiv", "Content-Type"),
                    new XAttribute("content", "text/html; charset=UTF-8")));

            if(!String.IsNullOrWhiteSpace(this.RobotsMetadata))
                head.Add(new XElement("meta", new XAttribute("name", "robots"), new XAttribute("content", this.RobotsMetadata)));

            head.Add(new XElement("title", this.IsMamlTopic ? this.MamlTopicTitle() : this.ApiTopicTitle(true, true)));

            if(this.IsMamlTopic)
            {
                string tocTitle = this.MetadataNode.Element("tableOfContentsTitle")?.Value.NormalizeWhiteSpace();

                if(!String.IsNullOrWhiteSpace(tocTitle))
                    head.Add(new XElement("meta", new XAttribute("name", "Title"), new XAttribute("content", tocTitle)));
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

                head.Add(new XElement("meta", new XAttribute("name", "Title"),
                    new XElement("includeAttribute",
                        new XAttribute("name", "content"),
                        new XAttribute("item", "meta_mshelp_tocTitle"),
                        new XElement("parameter", title))));
            }

            head.Add(new XElement("meta", new XAttribute("name", "Language"),
                new XAttribute("content", this.Locale.ToLowerInvariant())));
            head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.Locale"),
                new XAttribute("content", this.Locale.ToLowerInvariant())));
            head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.TopicLocale"),
                new XAttribute("content", this.Locale.ToLowerInvariant())));

            head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.SelfBranded"),
                new XAttribute("content", "true")));

            if(this.IsMamlTopic)
            {
                topicDesc = this.TopicNode.Descendants(Element.Ddue + "para").FirstOrDefault()?.Value.NormalizeWhiteSpace();

                head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.Id"),
                    new XAttribute("content", this.TopicNode.Attribute("id").Value)));

                var topicType = TopicType.FromElementName(this.TopicNode.Elements().First().Name.LocalName);

                if(topicType != null)
                {
                    head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.ContentType"),
                        new XAttribute("content", TopicType.DescriptionForTopicTypeGroup(topicType.ContentType))));
                }
                else
                {
                    head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.ContentType"),
                        new XAttribute("content", TopicType.DescriptionForTopicTypeGroup(TopicTypeGroup.Concepts))));
                }

                foreach(var keyword in this.MetadataNode.Elements("keyword").Where(
                  k => k.Attribute("index")?.Value == "K"))
                {
                    head.Add(new XElement("meta", new XAttribute("name", "System.Keywords"),
                        new XAttribute("content", keyword.Value)));
                }

                foreach(var f1Help in this.MetadataNode.Elements("keyword").Where(
                  k => k.Attribute("index")?.Value == "F"))
                {
                    head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", f1Help.Value)));
                }
            }
            else
            {
                topicDesc = this.CommentsNode.Element("summary")?.Value.NormalizeWhiteSpace();

                head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.Id"),
                    new XAttribute("content", this.Key)));
                head.Add(new XElement("meta", new XAttribute("name", "Microsoft.Help.ContentType"),
                    new XAttribute("content", TopicType.DescriptionForTopicTypeGroup(TopicTypeGroup.Reference))));

                this.AddIndexMetadata(head);
                this.AddF1HelpMetadata(head);
            }

            foreach(var language in this.DocumentNode.Descendants().Where(
              d => d.Attribute("language") != null && d.Attribute("phantom") == null).Select(
              d => d.Attribute("language").Value).Distinct().OrderBy(l => l))
            {
                string devLang = LanguageIdFor(language);

                if(!devLang.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                  !devLang.Equals("other", StringComparison.OrdinalIgnoreCase))
                {
                    head.Add(new XElement("meta",
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

                head.Add(new XElement("meta", new XAttribute("name", "Description"),
                    new XAttribute("content", topicDesc)));
            }

            if(!this.IsMamlTopic)
            {
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

                head.Add(new XElement("meta", new XAttribute("name", "container"),
                    new XAttribute("content", namespaceName)));

                if(!String.IsNullOrWhiteSpace(this.ApiMember.TopicFilename))
                {
                    head.Add(new XElement("meta", new XAttribute("name", "file"),
                        new XAttribute("content", this.ApiMember.TopicFilename)));
                    head.Add(new XElement("meta", new XAttribute("name", "guid"),
                        new XAttribute("content", this.ApiMember.TopicFilename)));
                }
            }

            return head;
        }

        /// <summary>
        /// Add index metadata to the header
        /// </summary>
        /// <param name="head">The header element to which the index metadata is added</param>
        private void AddIndexMetadata(XElement head)
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
                        head.Add(new XElement("meta",
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
                            head.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", $"indexEntry_{t.ApiSubgroup}"),
                                    new XElement("parameter", String.Join(".", namespaceName, apiName)))));
                        }

                        head.Add(new XElement("meta",
                            new XAttribute("name", "System.Keywords"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "content"),
                                new XAttribute("item", $"indexEntry_{t.ApiSubgroup}"),
                                new XElement("parameter", apiName))));
                    }

                    // Enumerations get an entry for each member
                    if(t.ApiSubgroup == ApiMemberGroup.Enumeration)
                    {
                        foreach(var enumMember in this.ReferenceNode.Element("elements").Elements("element"))
                        {
                            head.Add(new XElement("meta",
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
                        head.Add(new XElement("meta",
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
                            head.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", "indexEntry_constructorType"),
                                    new XElement("parameter", String.Join(".", apiName,
                                    typeName.Substring(typeName.LastIndexOf('.') + 1))))));
                        }
                        else
                        {
                            head.Add(new XElement("meta",
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

                        head.Add(new XElement("meta",
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
                            head.Add(new XElement("meta",
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
                            head.Add(new XElement("meta",
                                new XAttribute("name", "System.Keywords"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "content"),
                                    new XAttribute("item", $"indexEntry_{entryType}Explicit"),
                                    new XElement("parameter", apiName))));

                            foreach(string ns in this.LanguageSpecificApiNames(this.ReferenceNode.Element(
                              "containers").Element("type")))
                            {
                                head.Add(new XElement("meta",
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
                            head.Add(new XElement("meta",
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
        /// <param name="head">The header element to which the F1 help metadata is added</param>
        private void AddF1HelpMetadata(XElement head)
        {
            string namespaceName, typeName, memberName;

            switch(this.ApiMember)
            {
                case var t when t.ApiTopicGroup == ApiMemberGroup.Namespace:
                    // Namespace pages get the namespace keyword, if it exists
                    namespaceName = this.ReferenceNode.Element("apidata").Attribute("name").Value;

                    if(!String.IsNullOrWhiteSpace(namespaceName))
                    {
                        head.Add(new XElement("meta",
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
                        head.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", String.Join(".", namespaceName, typeName))));
                    }

                    head.Add(new XElement("meta",
                        new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", typeName)));

                    // Enumerations get an entry for each member (Namespace.Type.Member or Type.Member)
                    if(t.ApiSubgroup == ApiMemberGroup.Enumeration)
                    {
                        foreach(var enumMember in this.ReferenceNode.Element("elements").Elements("element"))
                        {
                            if(!String.IsNullOrWhiteSpace(namespaceName))
                            {
                                head.Add(new XElement("meta",
                                    new XAttribute("name", "Microsoft.Help.F1"),
                                    new XAttribute("content", String.Join(".", namespaceName, typeName,
                                        enumMember.Element("apidata").Attribute("name").Value))));
                            }
                            else
                            {
                                head.Add(new XElement("meta",
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
                        head.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", String.Join(".", namespaceName, typeName, memberName))));
                    }

                    head.Add(new XElement("meta",
                        new XAttribute("name", "Microsoft.Help.F1"),
                        new XAttribute("content", String.Join(".", typeName, memberName))));
                    head.Add(new XElement("meta",
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
                            head.Add(new XElement("meta",
                                new XAttribute("name", "Microsoft.Help.F1"),
                                new XAttribute("content", String.Join(".", namespaceName, typeName, memberName))));
                        }

                        head.Add(new XElement("meta",
                            new XAttribute("name", "Microsoft.Help.F1"),
                            new XAttribute("content", String.Join(".", typeName, memberName))));
                        head.Add(new XElement("meta",
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
        /// <returns>An XML element containing the page title elements</returns>
        private XElement RenderPageTitleAndLogo()
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

            if(!String.IsNullOrWhiteSpace(logoFile) && placement == LogoPlacement.Above)
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

            if(!String.IsNullOrWhiteSpace(logoFile) && placement == LogoPlacement.Left)
            {
                tr.Add(new XElement("td",
                    new XAttribute("class", "logoColumn"),
                    image));
            }

            tr.Add(new XElement("td",
                new XAttribute("class", "titleColumn"),
                new XElement("h1", this.IsMamlTopic ? this.MamlTopicTitle() : this.ApiTopicTitle(false, false))));

            if(!String.IsNullOrWhiteSpace(logoFile) && placement == LogoPlacement.Right)
            {
                tr.Add(new XElement("td",
                    new XAttribute("class", "logoColumn"),
                    image));
            }

            return table;
        }

        /// <summary>
        /// Render the content of an API topic's body
        /// </summary>
        /// <remarks>This is mostly custom rendering of the various sections based on the member type</remarks>
        private void RenderApiTopicBody()
        {
            this.RenderNode(this.CommentsNode.Element("preliminary"));

            if(this.ReferenceNode.AttributeOfType("T:System.ObsoleteAttribute") != null)
            {
                this.CurrentElement.Add(new XElement("p",
                    new XElement("include", new XAttribute("item", "boilerplate_obsoleteLong"))));
            }

            if(this.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                this.RenderNode(this.CommentsNode.Element("summary"));
            else
            {
                // Render the summary from the first overloads element.  There should only be one.
                var overloads = this.ReferenceNode.Descendants("overloads").FirstOrDefault();

                if(overloads != null)
                {
                    var summary = overloads.Element("summary");

                    if(summary != null)
                        this.RenderNode(summary);
                    else
                    {
                        var div = new XElement("div", this.StyleAttributeFor(CommonStyle.Summary));

                        this.CurrentElement.Add(div);
                        this.RenderChildElements(div, overloads.Nodes());
                    }
                }
            }

            // Render a minimal inheritance hierarchy at the top
            int descendants = this.RenderInheritanceHierarchy(4);

            // Only API member pages get namespace/assembly info and a syntax section
            if(this.ApiMember.ApiTopicGroup != ApiMemberGroup.List &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.RootGroup &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.Root &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.NamespaceGroup &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.Namespace)
            {
                this.RenderNamespaceAndAssemblyInformation();
                this.RenderNode(this.SyntaxNode);
            }

            // Element lists
            switch(this.ApiMember)
            {
                case var t when t.ApiTopicGroup == ApiMemberGroup.RootGroup || t.ApiTopicGroup == ApiMemberGroup.Root:
                    this.RenderRootList();
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.NamespaceGroup:
                    this.RenderNamespaceGroupList();
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Namespace:
                    this.RenderNamespaceList();
                    break;

                case var t when t.ApiTopicSubgroup == ApiMemberGroup.Enumeration:
                    this.RenderEnumerationMembersList();
                    break;

                case var t when t.ApiTopicGroup == ApiMemberGroup.Type || t.ApiTopicGroup == ApiMemberGroup.List:
                    this.RenderTypeMemberLists();
                    break;
            }

            this.RenderSectionTable("title_events", "header_eventType", "header_eventReason",
                this.CommentsNode.Elements("event"));
            this.RenderSectionTable("title_exceptions", "header_exceptionName", "header_exceptionCondition",
                this.CommentsNode.Elements("exception"));

            this.RenderNode(this.CommentsNode.Element("remarks"));
            this.RenderNode(this.CommentsNode.Element("example"));

            // Possible TODO: The Code Contracts project is dead so this is being omitted.  This would be a good
            // test for coming up with a way to extending the presentation style with an extra section without
            // having to redo the whole presentation style.
            // this.RenderContractsSection();

            // Only API member pages get version information
            if(this.ApiMember.ApiTopicGroup != ApiMemberGroup.List &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.RootGroup &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.Root &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.NamespaceGroup &&
               this.ApiMember.ApiTopicGroup != ApiMemberGroup.Namespace)
            {
                foreach(var v in this.ReferenceNode.Elements("versions"))
                    this.RenderNode(v);
            }

            this.RenderSectionTable("title_permissions", "header_permissionName", "header_permissionDescription",
                this.CommentsNode.Elements("permission"));

            this.RenderRevisionHistory();

            // Render the bibliography section if needed.  By now, any citation elements in the summary, remarks,
            // or other sections should have been seen.
            if(this.ElementHandlerFor("bibliography") is BibliographyElement b)
            {
                if(b.DetermineCitations(this).Count != 0)
                {
                    // Use the first citation element as the element for rendering.  It's only needed to create
                    // a unique ID for the section.
                    var cite = this.DocumentNode.Descendants("cite").First();

                    b.Render(this, cite);
                }
            }

            this.RenderSeeAlsoSection();

            // Render a full inheritance hierarchy at the bottom if needed
            if(descendants > 4)
                this.RenderInheritanceHierarchy(0);
        }

        /// <summary>
        /// Render the inheritance hierarchy section
        /// </summary>
        /// <param name="maxDescendants">The maximum number of descendents to show or zero to show all
        /// descendants.  If a non-zero value is passed, and the number of descendants is greater than it, a
        /// "More..." link is generated to a section with a <c>fullInheritance</c> ID that is assumed to be
        /// elsewhere in the topic that shows all descendants.</param>
        /// <returns>The number of descendants.  This can be used to determine if a full hierarchy is needed at
        /// the end of the topic.</returns>
        public int RenderInheritanceHierarchy(int maxDescendants)
        {
            var family = this.ReferenceNode.Element("family");
            int descendantCount = 0;

            if(family == null)
                return 0;

            var (title, content) = this.CreateSection(family.GenerateUniqueId(), true,
                "title_family", maxDescendants == 0 ? "fullInheritance" : null);

            this.CurrentElement.Add(title);
            this.CurrentElement.Add(content);

            var ancestors = family.Element("ancestors");
            int indent = 0;

            if(ancestors != null)
            {
                // Ancestor types are stored nearest to most distant so reverse them
                foreach(var typeInfo in ancestors.Elements().Reverse())
                {
                    if(indent > 0)
                        content.Add(indent.ToIndent());

                    this.RenderTypeReferenceLink(content, typeInfo, true);
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
                    new XAttribute("target", this.Key),
                    new XAttribute("show-container", true)),
                new XElement("br"));

            var descendants = family.Element("descendents");

            // descendents
            if(descendants != null)
            {

                descendantCount = descendants.Elements().Count();

                if(maxDescendants > 0 && descendantCount > maxDescendants)
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
                        this.RenderTypeReferenceLink(content, typeInfo, true);
                        content.Add(new XElement("br"));
                    }
                }
            }

            return descendantCount;
        }

        /// <summary>
        /// This is used to render namespace and assembly information for an API topic
        /// </summary>
        private void RenderNamespaceAndAssemblyInformation()
        {
            var containers = this.ReferenceNode.Element("containers");
            var libraries = containers.Elements("library");

            this.CurrentElement.Add(new XElement("p", " "),
                new XElement("include", new XAttribute("item", "boilerplate_requirementsNamespace")),
                Element.NonBreakingSpace,
                new XElement("referenceLink",
                    new XAttribute("target", containers.Element("namespace").Attribute("api").Value)),
                new XElement("br"));

            int separatorSize = 1;
            bool first = true;

            if(libraries.Count() > 1)
            {
                this.CurrentElement.Add(new XElement("include",
                    new XAttribute("item", "boilerplate_requirementsAssemblies")));
                separatorSize = 2;
            }
            else
            {
                this.CurrentElement.Add(new XElement("include",
                    new XAttribute("item", "boilerplate_requirementsAssemblyLabel")));
            }

            string separator = new String(Element.NonBreakingSpace, separatorSize);

            foreach(var l in libraries)
            {
                if(!first)
                    this.CurrentElement.Add(new XElement("br"));

                this.CurrentElement.Add(separator);

                string version = l.Element("assemblydata").Attribute("version").Value,
                    extension = l.Attribute("kind").Value.Equals(
                        "DynamicallyLinkedLibrary", StringComparison.Ordinal) ? "dll" : "exe";
                string[] versionParts = version.Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);

                // Limit the version number parts if requested
                if(this.MaxVersionParts > 1 && this.MaxVersionParts < 5)
                    version = String.Join(".", versionParts, 0, this.MaxVersionParts);

                this.CurrentElement.Add(new XElement("include",
                        new XAttribute("item", "assemblyNameAndModule"),
                    new XElement("parameter", l.Attribute("assembly").Value),
                    new XElement("parameter", l.Attribute("module").Value),
                    new XElement("parameter", extension),
                    new XElement("parameter", version)));

                first = false;
            }

            // Show XAML XML namespace for APIs that support XAML.  All topics that have auto-generated XAML
            // syntax get an "XMLNS for XAML" line in the Requirements section.  Topics with boilerplate XAML
            // syntax, e.g. "Not applicable", do NOT get this line.
            var xamlCode = this.SyntaxNode.Elements("div").Where(d => d.Attribute("codeLanguage")?.Value.Equals(
                "XAML", StringComparison.Ordinal) ?? false);

            if(xamlCode.Any())
            {
                var xamlXmlNS = xamlCode.Elements("div").Where(d => d.Attribute("xamlXmlnsUri")?.Value != null);

                this.CurrentElement.Add(new XElement("br"));

                XElement parameter = new XElement("parameter"),
                    xamlNS = new XElement("include",
                        new XAttribute("item", "boilerplate_xamlXmlnsRequirements"), parameter);

                this.CurrentElement.Add(xamlNS);

                if(xamlXmlNS.Any())
                {
                    first = true;

                    foreach(var d in xamlXmlNS)
                    {
                        if(!first)
                            this.CurrentElement.Add(", ");

                        parameter.Add(new XElement(d));
                        first = false;
                    }
                }
                else
                {
                    parameter.Add(new XElement("include",
                        new XAttribute("item", "boilerplate_unmappedXamlXmlns")));
                }
            }
        }

        /// <summary>
        /// Render a section with a title and a table containing the element content
        /// </summary>
        /// <param name="sectionTitleItem">The section title include item</param>
        /// <param name="typeColumnHeaderItem">The type column header item</param>
        /// <param name="descriptionColumnHeaderItem">The description column header item</param>
        /// <param name="sectionElements">An enumerable list of the elements to render in the table</param>
        private void RenderSectionTable(string sectionTitleItem, string typeColumnHeaderItem,
          string descriptionColumnHeaderItem, IEnumerable<XElement> sectionElements)
        {
            if(sectionElements.Any())
            {
                var (title, content) = this.CreateSection(sectionElements.First().GenerateUniqueId(), true,
                    sectionTitleItem, null);

                this.CurrentElement.Add(title);
                this.CurrentElement.Add(content);

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
                                new XAttribute("target", se.Attribute("cref")?.Value),
                                new XAttribute("qualified", "true"))),
                        descCell));

                    this.RenderChildElements(descCell, se.Nodes());
                }
            }
        }

        /// <summary>
        /// Render the revision history section if applicable
        /// </summary>
        private void RenderRevisionHistory()
        {
            var revisionHistory = this.CommentsNode.Element("revisionHistory");

            if(revisionHistory == null || revisionHistory.Attribute("visible")?.Value == "false")
                return;

            var revisions = revisionHistory.Elements("revision").Where(
                h => h.Attribute("visible")?.Value != "false");

            if(revisions.Any())
            {
                var (title, content) = this.CreateSection(revisionHistory.GenerateUniqueId(), true,
                    "title_revisionHistory", null);

                this.CurrentElement.Add(title);
                this.CurrentElement.Add(content);

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

                    this.RenderChildElements(descCell, rh.Nodes());
                }
            }
        }

        /// <summary>
        /// This renders the See Also section if applicable
        /// </summary>
        private void RenderSeeAlsoSection()
        {
            // Render the see and seealso links using the see element handler as the processing is the same
            Element seeHandler = this.ElementHandlerFor("see"),
                conceptualLinkHandler = this.ElementHandlerFor("conceptualLink");

            // Get see also elements from comments excluding those in overloads comments
            List<XElement>  seeAlsoNotInOverloads = this.CommentsNode.Descendants("seealso").Where(
                    s => !s.Ancestors("overloads").Any()).ToList(),
                seeAlsoHRef = seeAlsoNotInOverloads.Where(s => s.Attribute("href") != null).ToList(),
                seeAlsoCRef = seeAlsoNotInOverloads.Except(seeAlsoHRef).ToList();

            // Combine those with see also elements from element overloads comments
            var elements = this.ReferenceNode.Element("elements");

            if(elements == null)
                elements = new XElement("elements");

            var elementOverloads = elements.Elements("element").SelectMany(e => e.Descendants("overloads")).ToList();
            seeAlsoHRef.AddRange(elementOverloads.Descendants("seealso").Where(s => s.Attribute("href") != null));
            seeAlsoCRef.AddRange(elementOverloads.Descendants("seealso").Where(s => s.Attribute("href") == null));

            // Get conceptual links from comments excluding those in overloads comments and combine them with
            // those in element overloads comments.
            var conceptualLinks = this.CommentsNode.Descendants("conceptualLink").Where(
                s => !s.Ancestors("overloads").Any()).Concat(elementOverloads.Descendants("conceptualLink")).ToList();

            if(seeAlsoCRef.Count != 0 || seeAlsoHRef.Count != 0 || conceptualLinks.Count != 0 ||
              this.ApiMember.ApiTopicGroup == ApiMemberGroup.Type ||
              this.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
              this.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
            {
                var (title, content) = this.CreateSection("seeAlsoSection", true, "title_relatedTopics", null);

                this.CurrentElement.Add(title);
                this.CurrentElement.Add(content);

                var priorCurrentElement = this.CurrentElement;

                if(seeAlsoCRef.Count != 0 || this.ApiMember.ApiTopicGroup == ApiMemberGroup.Type ||
                  this.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
                  this.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
                {
                    var (subtitle, subsection) = this.CreateSubsection(true, "title_seeAlso_reference");

                    if(subtitle != null)
                        content.Add(subtitle);

                    if(subsection != null)
                        content.Add(subsection);
                    else
                        subsection = content;

                    this.RenderAutoGeneratedSeeAlsoLinks(subsection);

                    if(seeHandler != null)
                    {
                        foreach(var s in seeAlsoCRef)
                        {
                            var div = new XElement("div", this.StyleAttributeFor(CommonStyle.SeeAlsoStyle));
                            subsection.Add(div);

                            this.CurrentElement = div;
                            seeHandler.Render(this, s);
                        }
                    }
                }

                if((seeAlsoHRef.Count != 0 && seeHandler != null) || (conceptualLinks.Count != 0 &&
                  conceptualLinkHandler != null))
                {
                    var (subtitle, subsection) = this.CreateSubsection(true, "title_seeAlso_otherResources");

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
                            var div = new XElement("div", this.StyleAttributeFor(CommonStyle.SeeAlsoStyle));
                            subsection.Add(div);

                            this.CurrentElement = div;
                            seeHandler.Render(this, s);
                        }
                    }

                    if(conceptualLinkHandler != null)
                    {
                        foreach(var c in conceptualLinks)
                        {
                            var div = new XElement("div", this.StyleAttributeFor(CommonStyle.SeeAlsoStyle));
                            subsection.Add(div);

                            this.CurrentElement = div;
                            conceptualLinkHandler.Render(this, c);
                        }
                    }
                }

                this.CurrentElement = priorCurrentElement;
            }
        }

        /// <summary>
        /// Render auto-generated see also section links based on the API topic
        /// </summary>
        /// <param name="subsection">The subsection to which the links are added</param>
        private void RenderAutoGeneratedSeeAlsoLinks(XElement subsection)
        {
            // Adda a link to the containing type on all list and member topics
            if(this.ApiMember.ApiTopicGroup == ApiMemberGroup.Member ||
              this.ApiMember.ApiTopicGroup == ApiMemberGroup.List)
            {
                subsection.Add(new XElement("div",
                        this.StyleAttributeFor(CommonStyle.SeeAlsoStyle),
                    new XElement("referenceLink",
                        new XAttribute("target", this.ApiMember.TypeTopicId),
                        new XAttribute("display-target", "format"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoTypeLink"),
                            new XElement("parameter", "{0}"),
                            new XElement("parameter", this.ApiMember.TypeApiSubgroup)))));
            }

            // Add a link to the overload topic
            if(!String.IsNullOrWhiteSpace(this.ApiMember.OverloadTopicId))
            {
                subsection.Add(new XElement("div",
                        this.StyleAttributeFor(CommonStyle.SeeAlsoStyle),
                    new XElement("referenceLink",
                        new XAttribute("target", this.ApiMember.OverloadTopicId),
                        new XAttribute("display-target", "format"),
                        new XAttribute("show-parameters", "false"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoOverloadLink"),
                            new XElement("parameter", "{0}")))));
            }

            // Add a link to the namespace topic
            string namespaceId = this.ReferenceNode.Element("containers")?.Element("namespace")?.Attribute("api")?.Value;

            if(!String.IsNullOrWhiteSpace(namespaceId))
            {
                subsection.Add(new XElement("div",
                        this.StyleAttributeFor(CommonStyle.SeeAlsoStyle),
                    new XElement("referenceLink",
                        new XAttribute("target", namespaceId),
                        new XAttribute("display-target", "format"),
                        new XElement("include",
                            new XAttribute("item", "boilerplate_seeAlsoNamespaceLink"),
                            new XElement("parameter", "{0}")))));
            }
        }

        /// <summary>
        /// Render the list in a root group or root topic
        /// </summary>
        private void RenderRootList()
        {
            var elements = this.ReferenceNode.Element("elements").Elements("element").OrderBy(
                e => e.Element("apidata").Attribute("name").Value).ToList();

            if(elements.Count == 0)
                return;

            var (title, content) = this.CreateSection(elements[0].GenerateUniqueId(), true, "title_namespaces", null);

            this.CurrentElement.Add(title);
            this.CurrentElement.Add(content);

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
                    this.RenderChildElements(summaryCell, summary.Nodes());
                else
                    summaryCell.Add(Element.NonBreakingSpace);
            }
        }

        /// <summary>
        /// Render the list in a namespace group topic
        /// </summary>
        private void RenderNamespaceGroupList()
        {
            var elements = this.ReferenceNode.Element("elements").Elements("element").OrderBy(e =>
            {
                string name = e.Attribute("api").Value;
                return name.Substring(name.IndexOf(':') + 1);
            }).ToList();

            if(elements.Count == 0)
                return;

            var (title, content) = this.CreateSection(elements[0].GenerateUniqueId(), true,
                "tableTitle_namespace", null);

            this.CurrentElement.Add(title);
            this.CurrentElement.Add(content);

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
                    this.RenderChildElements(summaryCell, summary.Nodes());
                else
                    summaryCell.Add(Element.NonBreakingSpace);
            }
        }

        /// <summary>
        /// Render the category lists in a namespace topic
        /// </summary>
        private void RenderNamespaceList()
        {
            var elements = this.ReferenceNode.Element("elements").Elements("element").GroupBy(
                e => e.Element("apidata").Attribute("subgroup").Value).ToDictionary(k => k.Key, v => v);

            foreach(string key in new[] { "class", "structure", "interface", "delegate", "enumeration" })
            {
                if(elements.TryGetValue(key, out var group))
                {
                    var (title, content) = this.CreateSection(group.First().GenerateUniqueId(), true,
                        "tableTitle_" + key, null);

                    this.CurrentElement.Add(title);
                    this.CurrentElement.Add(content);

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
                                new XAttribute("src", $"{this.IconPath}CodeExample.png"),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "alt"),
                                    new XAttribute("item", "altText_CodeExample")),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "title"),
                                    new XAttribute("item", "altText_CodeExample")));
                        }

                        var summaryCell = new XElement("td");

                        if(e.AttributeOfType("T:System.ObsoleteAttribute") != null)
                        {
                            summaryCell.Add(new XElement("include",
                                new XAttribute("item", "boilerplate_obsoleteShort")));
                        }

                        table.Add(new XElement("tr",
                            new XElement("td",
                                new XElement("img",
                                    new XAttribute("src", $"{this.IconPath}{visibility}{key}.gif"),
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
                            this.RenderChildElements(summaryCell, summary.Nodes());
                        else
                        {
                            if(summaryCell.IsEmpty)
                                summaryCell.Add(Element.NonBreakingSpace);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render the members of an enumeration
        /// </summary>
        private void RenderEnumerationMembersList()
        {
            // Sort order is configurable for enumeration members
            EnumMemberSortOrder enumMemberSortOrder = this.EnumMemberSortOrder;

            var elements = this.ReferenceNode.Element("elements").Elements("element").OrderBy(
                el => enumMemberSortOrder == EnumMemberSortOrder.Name ?
                    el.Element("apidata").Attribute("name").Value :
                    el.Element("value").Value.PadLeft(20, ' ')).ToList();

            if(elements.Count == 0)
                return;

            var enumValues = elements.Select(e => e.Element("value").Value).ToList();
            bool includeEnumValues = this.IncludeEnumValues;
            int idx;

            if(includeEnumValues)
            {
                EnumValueFormat enumFormat = this.FlagsEnumValueFormat;
                int groupSize = 0, minWidth = 0;
                bool signedValues = enumValues.Any(v => v.Length > 0 && v[0] == '-');

                if(enumFormat != EnumValueFormat.IntegerValue &&
                  this.ReferenceNode.AttributeOfType("T:System.FlagsAttribute") != null)
                {
                    groupSize = this.FlagsEnumSeparatorSize;

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

            var (title, content) = this.CreateSection(elements.First().GenerateUniqueId(), true,
                "topicTitle_enumMembers", null);

            this.CurrentElement.Add(title);
            this.CurrentElement.Add(content);

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

                if(e.AttributeOfType("T:System.ObsoleteAttribute") != null)
                    summaryCell.Add(new XElement("include", new XAttribute("item", "boilerplate_obsoleteShort")));

                XElement valueCell = null;

                if(includeEnumValues)
                {
                    valueCell = new XElement("td", enumValues[idx]);
                    idx++;
                }

                table.Add(new XElement("tr",
                    new XElement("td",
                        new XElement("span",
                            this.StyleAttributeFor(CommonStyle.SelfLink),
                            e.Element("apidata").Attribute("name").Value)),
                    valueCell,
                    summaryCell));

                var summary = e.Element("summary");
                var remarks = e.Element("remarks");

                if(summary != null || remarks != null)
                {
                    if(summary != null)
                        this.RenderChildElements(summaryCell, summary.Nodes());

                    // Enum members may have additional authored content in the remarks node
                    if(remarks != null)
                        this.RenderChildElements(summaryCell, remarks.Nodes());
                }
                else
                {
                    if(summaryCell.IsEmpty)
                        summaryCell.Add(Element.NonBreakingSpace);
                }
            }
        }

        /// <summary>
        /// Render type member lists
        /// </summary>
        /// <remarks>This is used for types and the member list subtopics</remarks>
        private void RenderTypeMemberLists()
        {
            var allMembers = this.ReferenceNode.Element("elements")?.Elements("element").ToList();

            if(allMembers == null || allMembers.Count == 0)
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

            if(this.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
            {
                this.CurrentElement.Add(new XElement("p",
                    new XElement("include",
                        new XAttribute("item", "exposedMembersTableText"),
                        new XElement("parameter",
                            new XElement("referenceLink", new XAttribute("target", this.ApiMember.TypeTopicId))))));

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
                        case var mbr when memberData.Attribute("visibility").Value == "private" &&
                          procedureData.Attribute("virtual").Value == "true":
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

                var (title, content) = this.CreateSection(members.First().GenerateUniqueId(), true,
                    "tableTitle_" + memberType.ToString(), null);

                this.CurrentElement.Add(title);
                this.CurrentElement.Add(content);

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
                    el.Element("apidata").Attribute("name").Value).ThenBy(
                    el => el.Element("templates")?.Elements()?.Count() ?? 0))
                {
                    string visibility;

                    switch(e.Element("memberdata").Attribute("visibility").Value)
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
                    string showParameters = (this.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload &&
                        e.Element("memberdata").Attribute("overload") == null) ? "false" : "true";

                    if(e.Descendants("example").Any())
                    {
                        codeExampleImage = new XElement("img",
                            new XAttribute("src", $"{this.IconPath}CodeExample.png"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "alt"),
                                new XAttribute("item", "altText_CodeExample")),
                            new XElement("includeAttribute",
                                new XAttribute("name", "title"),
                                new XAttribute("item", "altText_CodeExample")));
                    }

                    if(memberType != ApiMemberGroup.AttachedEvent && memberType != ApiMemberGroup.AttachedProperty &&
                      e.Element("memberdata").Attribute("static")?.Value == "true")
                    {
                        staticImage = new XElement("img",
                            new XAttribute("src", $"{this.IconPath}Static.gif"),
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
                            new XAttribute("src", $"{this.IconPath}pubInterface.gif"),
                            new XElement("includeAttribute",
                                new XAttribute("name", "alt"),
                                new XAttribute("item", "altText_ExplicitInterface")),
                            new XElement("includeAttribute",
                                new XAttribute("name", "title"),
                                new XAttribute("item", "altText_ExplicitInterface")));
                    }

                    var summaryCell = new XElement("td");

                    if(e.AttributeOfType("T:System.ObsoleteAttribute") != null)
                        summaryCell.Add(new XElement("include", new XAttribute("item", "boilerplate_obsoleteShort")));

                    if(!Enum.TryParse(e.Element("apidata")?.Attribute("subgroup")?.Value, true, out ApiMemberGroup imageMemberType))
                        imageMemberType = memberType;

                    if(imageMemberType == ApiMemberGroup.Constructor)
                        imageMemberType = ApiMemberGroup.Method;

                    switch(memberType)
                    {
                        case var t when t == ApiMemberGroup.Operator &&
                            (e.Element("apidata")?.Attribute("name")?.Value == "Explicit" ||
                            e.Element("apidata")?.Attribute("name")?.Value == "Implicit"):
                            referenceLink.Add(new XAttribute("show-parameters", "true"));
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

                    table.Add(new XElement("tr",
                        new XElement("td",
                            eiiImage,
                            new XElement("img",
                                new XAttribute("src", $"{this.IconPath}{visibility}{imageMemberType}.gif"),
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
                        this.RenderChildElements(summaryCell, summary.Nodes());

                    if(this.ApiMember.ApiTopicSubgroup != ApiMemberGroup.Overload)
                    {
                        if(memberType == ApiMemberGroup.Extension)
                        {
                            var parameter = new XElement("parameter");

                            summaryCell.Add(new XElement("br"),
                                new XElement("include", new XAttribute("item", "definedBy"),
                                parameter));

                            this.RenderTypeReferenceLink(parameter, e.Element("containers").Element("type"), false);
                        }
                        else
                        {
                            if(e.Element("overrides")?.Element("member") != null)
                            {
                                var parameter = new XElement("parameter");

                                summaryCell.Add(new XElement("br"),
                                    new XElement("include", new XAttribute("item", "overridesMember"),
                                    parameter));

                                this.RenderTypeReferenceLink(parameter, e.Element("overrides").Element("member"), true);
                            }
                            else
                            {
                                if(this.ApiMember.TypeTopicId != e.Element("containers").Element("type").Attribute("api").Value)
                                {
                                    var parameter = new XElement("parameter");

                                    summaryCell.Add(new XElement("br"),
                                        new XElement("include", new XAttribute("item", "inheritedFrom"),
                                        parameter));

                                    this.RenderTypeReferenceLink(parameter, e.Element("containers").Element("type"), false);
                                }
                            }
                        }
                    }

                    if(summaryCell.IsEmpty)
                        summaryCell.Add(Element.NonBreakingSpace);
                }

                content.Add(new XElement("a",
                    new XAttribute("href", "#PageHeader"),
                    new XElement("include", new XAttribute("item", "top"))));
            }
        }
        #endregion
    }
}
