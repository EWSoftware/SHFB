//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MamlToFlowDocumentConverter.Static.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/18/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the static data definitions used by the MAML to flow
// document converter class.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/02/2012  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml
{
    // This contains the static data definitions used by the MAML to flow
    // document converter.
    partial class MamlToFlowDocumentConverter
    {
        #region XML namespaces
        //=====================================================================

        // XML namespaces used by the parser
        internal static XNamespace ddue = "http://ddue.schemas.microsoft.com/authoring/2003/5";
        internal static XNamespace xlink = "http://www.w3.org/1999/xlink";
        #endregion

        #region See Also section topic ID GUIDs
        //=====================================================================
        // These are used to categorize links in the relatedTopics element

        // Tasks
        private static Guid HowToId = new Guid("DAC3A6A0-C863-4E5B-8F65-79EFC6A4BA09");
        private static Guid WalkthroughId = new Guid("4779DD54-5D0C-4CC3-9DB3-BF1C90B721B3");
        private static Guid SampleId = new Guid("069EFD88-412D-4E2F-8848-2D5C3AD56BDE");
        private static Guid TroubleshootingId = new Guid("38C8E0D1-D601-4DBA-AE1B-5BEC16CD9B01");

        // Reference
        private static Guid ReferenceWithoutSyntaxId = new Guid("F9205737-4DEC-4A58-AA69-0E621B1236BD");
        private static Guid ReferenceWithSyntaxId = new Guid("95DADC4C-A2A6-447A-AA36-B6BE3A4F8DEC");
        private static Guid XmlReferenceId = new Guid("3272D745-2FFC-48C4-9E9D-CF2B2B784D5F");
        private static Guid ErrorMessageId = new Guid("A635375F-98C2-4241-94E7-E427B47C20B6");
        private static Guid UIReferenceId = new Guid("B8ED9F21-39A4-4967-928D-160CD2ED9DCE");
        
        // Concepts
        private static Guid ConceptualId = new Guid("1FE70836-AA7D-4515-B54B-E10C4B516E50");
        private static Guid SdkTechnologyOverviewArchitectureId = new Guid("68F07632-C4C5-4645-8DFA-AC87DCB4BD54");
        private static Guid SdkTechnologyOverviewCodeDirectoryId = new Guid("4BBAAF90-0E5F-4C86-9D31-A5CAEE35A416");
        private static Guid SdkTechnologyOverviewScenariosId = new Guid("356C57C4-384D-4AF2-A637-FDD6F088A033");
        private static Guid SdkTechnologyOverviewTechnologySummaryId = new Guid("19F1BB0E-F32A-4D5F-80A9-211D92A8A715");
        
        // Other resources
        private static Guid OrientationId = new Guid("B137C930-7BF7-48A2-A329-3ADCAEF8868E");
        private static Guid WhitePaperId = new Guid("56DB00EC-28BA-4C0D-8694-28E8B244E236");
        private static Guid CodeEntityId = new Guid("4A273212-0AC8-4D72-8349-EC11CD2FF8CD");
        private static Guid GlossaryId = new Guid("A689E19C-2687-4881-8CE1-652FF60CF46C");
        private static Guid SDKTechnologyOverviewOrientationId = new Guid("CDB8C120-888F-447B-8AF8-F9540562E7CA");
        #endregion

        #region Regular expressions
        //=====================================================================

        // Regular expressions used by the parser.
        private static Regex reCondenseWhitespace = new Regex(@"\s+");
        private static Regex reRemoveNamespace = new Regex(" xmlns=\".+?\"");
        #endregion

        #region Alert class name to display title dictionary
        //=====================================================================

        // This is used to map alert class names to titles.  We could use a case-insensitive comparer here but
        // the XML is case-sensitive so we'll stay case sensitive too so that erros in the title value show up.
        // These could be localized but we're not doing that for now.
        private static Dictionary<string, string> alertTitles = new Dictionary<string, string>()
        {
            { "c#", "C# Note" },
            { "C#", "C# Note" },
            { "c++", "C++ Note" },
            { "C++", "C++ Note" },
            { "caller", "Notes to Callers" },
            { "caution", "Caution" },
            { "cpp", "C++ Note" },
            { "CPP", "C++ Note" },
            { "cs", "C# Note" },
            { "csharp", "C# Note" },
            { "implement", "Notes to Implementers" },
            { "important", "Important" },
            { "inherit", "Notes to Inheritors" },
            { "j#", "J# Note" },
            { "J#", "J# Note" },
            { "JSharp", "J# Note" },
            { "note", "Note" },
            { "security", "Security Note" },
            { "security note", "Security Note" },
            { "tip", "Tip" },
            { "vb", "Visual Basic Note" },
            { "VB", "Visual Basic Note" },
            { "visual basic", "Visual Basic Note" },
            { "visual basic note", "Visual Basic Note" },
            { "visual c# note", "C# Note" },
            { "visual c++ note", "C++ Note" },
            { "visual j# note", "J# Note" },
            { "warning", "Caution" }
        };
        #endregion

        #region Alert class name to icon ID dictionary
        //=====================================================================

        // This is used to map alert class names to icons
        private static Dictionary<string, string> alertIcons = new Dictionary<string, string>()
        {
            { "c#", "AlertNote" },
            { "C#", "AlertNote" },
            { "c++", "AlertNote" },
            { "C++", "AlertNote" },
            { "CPP", "AlertNote" },
            { "caller", "AlertNote" },
            { "caution", "AlertCaution" },
            { "cpp", "AlertNote" },
            { "cs", "AlertNote" },
            { "csharp", "AlertNote" },
            { "implement", "AlertNote" },
            { "important", "AlertCaution" },
            { "inherit", "AlertNote" },
            { "j#", "AlertNote" },
            { "J#", "AlertNote" },
            { "JSharp", "AlertNote" },
            { "note", "AlertNote" },
            { "security", "AlertSecurity" },
            { "security note", "AlertSecurity" },
            { "tip", "AlertNote" },
            { "vb", "AlertNote" },
            { "VB", "AlertNote" },
            { "visual basic", "AlertNote" },
            { "visual basic note", "AlertNote" },
            { "visual c# note", "AlertNote" },
            { "visual c++ note", "AlertNote" },
            { "visual j# note", "AlertNote" },
            { "warning", "AlertCaution" }
        };
        #endregion

        #region MAML named section to display title dictionary
        //=====================================================================

        // This is used to map named section elements to display titles.
        // These could be localized but we're not doing that for now.
        private static Dictionary<string, string> namedSectionTitles = new Dictionary<string, string>()
        {
            { "appliesTo", "Applies To" },
            { "attributes", "Attributes" },
            { "attributesAndElements", "Attributes and Elements" },
            { "background", "Background" },
            { "buildInstructions", "Compiling the Code" },
            { "childElement", "Child Elements" },
            { "codeExample", "Example" },
            { "codeExamples", "Examples" },
            { "demonstrates", "Demonstrates" },
            { "dotNetFrameworkEquivalent", ".NET Framework Equivalent" },
            { "elementInformation", "Element Information" },
            { "exceptions", "Exceptions" },
            { "externalResources", "External Resources" },
            { "inThisSection", "In This Section" },
            { "languageReferenceRemarks", "Remarks" },
            { "nextSteps", "Next Steps" },
            { "parameters", "Parameters" },
            { "parentElement", "Parent Elements" },
            { "prerequisites", "Prerequisites" },
            { "reference", "Reference" },
            { "relatedTopics", "See Also" },
            { "relatedSections", "Related Sections" },
            { "requirements", "Requirements" },
            { "robustProgramming", "Robust Programming" },
            { "security", "Security" },
            { "textValue", "Text Value" },
            { "whatsNew", "What's New" }
        };
        #endregion

        #region Code block language ID to display title dictionary
        //=====================================================================

        // This is used to map code block language IDs to display titles.  These are case-insensitive.  They will
        // be loaded on first use by the caller using the code colorizer definitions.
        private static Dictionary<string, string> languageTitles = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        #endregion

        #region MAML element method handler dictionary
        //=====================================================================

        // This maps known element types to their respective handler methods
        private static Dictionary<string, Action<ElementProperties>> elementHandlers =
          new Dictionary<string, Action<ElementProperties>>
        {
            #region Unsupported elements
            //=================================================================

            // These rarely used elements are being ignored for now.  Support for them will be added if
            // requested or as time permits.
            //
            // changeHistory
            // copyright
            // legacySyntax
            // nonLocErrorTitle
            // notesForCallers
            // notesForImplementers
            // notesForInheritors
            // platformNotes
            // remarks
            // returnValue
            // schemaHierarchy
            // secondaryErrorTitle
            #endregion

            // Top level topic element
            { "topic", IgnoredElement },

            // Document type elements
            { "codeEntityDocument", IgnoredElement },
            { "developerConceptualDocument", IgnoredElement },
            { "developerErrorMessageDocument", IgnoredElement },
            { "developerGlossaryDocument", IgnoredElement },
            { "developerHowToDocument", IgnoredElement },
            { "developerOrientationDocument", IgnoredElement },
            { "developerReferenceWithSyntaxDocument", IgnoredElement },
            { "developerReferenceWithoutSyntaxDocument", IgnoredElement },
            { "developerSDKTechnologyOverviewArchitectureDocument", IgnoredElement },
            { "developerSDKTechnologyOverviewCodeDirectoryDocument", IgnoredElement },
            { "developerSDKTechnologyOverviewOrientationDocument", IgnoredElement },
            { "developerSDKTechnologyOverviewScenariosDocument", IgnoredElement },
            { "developerSDKTechnologyOverviewTechnologySummaryDocument", IgnoredElement },
            { "developerSampleDocument", IgnoredElement },
            { "developerTroubleshootingDocument", IgnoredElement },
            { "developerUIReferenceDocument", IgnoredElement },
            { "developerWalkthroughDocument", IgnoredElement },
            { "developerWhitePaperDocument", IgnoredElement },
            { "developerXmlReference", IgnoredElement },

            // Ignored elements.  These are either containers, have no special formatting, or are being ignored
            // due to external requirements that make them unsuitable for rendering in the preview. The
            // IgnoredElement handler will allow processing of child elements.  The IgnoredElementWithChildren
            // handler will omit the element and all of its child elements as well.
            { "attribute", IgnoredElement },
            { "bibliography", IgnoredElementWithChildren },
            { "cite", IgnoredElement },
            { "conclusion", IgnoredElement },
            { "content", IgnoredElement },
            { "corporation", IgnoredElement },
            { "country", IgnoredElement },
            { "date", IgnoredElement },
            { "definitionTable", IgnoredElement },
            { "description", IgnoredElement },
            { "dynamicLink", IgnoredElementWithChildren },
            { "legacy", IgnoredElement },
            { "localizedText", IgnoredElement },
            { "platformNote", IgnoredElement },
            { "sections", IgnoredElement },
            { "type", IgnoredElement },

            // General section, named section, and formatted block elements
            { "alert", AlertElement },
            { "appliesTo", SectionElement },
            { "attributesandElements", SectionElement },
            { "attributes", SectionElement },
            { "background", SectionElement },
            { "buildInstructions", SectionElement },
            { "childElement", SectionElement },
            { "code", CodeElement },
            { "codeExamples", SectionElement },
            { "codeExample", SectionElement },
            { "codeReference", CodeElement },
            { "demonstrates", SectionElement },
            { "dotNetFrameworkEquivalent", SectionElement },
            { "elementInformation", SectionElement },
            { "exceptions", SectionElement },
            { "externalResources", SectionElement },
            { "inThisSection", SectionElement },
            { "introduction", SectionElement },
            { "languageReferenceRemarks", SectionElement },
            { "nextSteps", SectionElement },
            { "para", ParagraphElement },
            { "parameters", SectionElement },
            { "parentElement", SectionElement },
            { "prerequisites", SectionElement },
            { "procedure", SectionElement },
            { "quote", QuoteElement },
            { "reference", SectionElement },
            { "relatedSections", SectionElement },
            { "requirements", SectionElement },
            { "robustProgramming", SectionElement },
            { "sampleCode", CodeElement },
            { "section", SectionElement },
            { "security", SectionElement },
            { "snippet", CodeElement },
            { "snippets", SectionElement },
            { "summary", SummaryElement },
            { "syntaxSection", SectionElement },
            { "textValue", SectionElement },
            { "title", TitleElement },
            { "whatsNew", SectionElement },

            // List elements
            { "definedTerm", DefinedTermElement },
            { "definition", DefinitionElement },
            { "list", ListElement },
            { "listItem", ListItemElement },
            { "relatedTopics", RelatedTopicsElement },
            { "steps", ListElement },
            { "step", ListItemElement },

            // Media elements
            { "mediaLink", MediaLinkElement },
            { "mediaLinkInline", MediaLinkInlineElement },

            // Table elements
            { "table", TableElement },
            { "tableHeader", TableHeaderElement },
            { "row", RowElement },
            { "entry", EntryElement },

            // Formatted inline elements
            { "application", BoldElement },
            { "codeFeaturedElement", BoldElement },
            { "codeInline", InlineCodeElement },
            { "computerOutputInline", InlineCodeElement },
            { "command", InlineCodeElement },
            { "database", BoldElement },
            { "embeddedLabel", BoldElement },
            { "environmentVariable", InlineCodeElement },
            { "errorInline", ItalicElement },
            { "fictitiousUri", ItalicElement },
            { "foreignPhrase", ItalicElement },
            { "hardware", BoldElement },
            { "languageKeyword", InlineCodeElement },
            { "legacyBold", BoldElement },
            { "legacyItalic", ItalicElement },
            { "legacyUnderline", UnderlineElement },
            { "literal", LiteralElement },
            { "localUri", ItalicElement },
            { "math", MathElement },
            { "newTerm", ItalicElement },
            { "parameter", ItalicElement },
            { "parameterReference", ItalicElement },
            { "placeholder", ItalicElement },
            { "phrase", ItalicElement },
            { "quoteInline", ItalicElement },
            { "replaceable", ItalicElement },
            { "span", InlineCodeElement },
            { "subscript", SubscriptElement },
            { "subscriptType", SubscriptElement },
            { "superscript", SuperscriptElement },
            { "superscriptType", SuperscriptElement },
            { "system", BoldElement },
            { "ui", BoldElement },
            { "unmanagedCodeEntityReference", BoldElement },
            { "userInput", BoldElement },
            { "userInputLocalizable", BoldElement },
            { "xmlEntityReference", BoldElement },  // XSD build component extension element (http://xsddoc.CodePlex.com)

            // Inline link elements
            { "codeEntityReference", CodeEntityReferenceElement },
            { "externalLink", ExternalLinkElement },
            { "legacyLink", LinkElement },
            { "link", LinkElement },

            // Miscellaneous elements
            { "autoOutline", AutoOutlineElement },
            { "markup", MarkupElement },
            { "token", TokenElement },

            // Glossary element.  This handles all aspects of producing the glossary topic.
            { "glossary", GlossaryElement }
        };
        #endregion
    }
}
