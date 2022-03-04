//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CommonStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/20/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type used to define the common style sheet style names
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/20/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This enumerated type defines the common style sheet style names
    /// </summary>
    [Serializable]
    public enum CommonStyle
    {
        // Auto-outline styles

        /// <summary>
        /// Auto-outline
        /// </summary>
        AutoOutline,
        /// <summary>
        /// Outline section entry
        /// </summary>
        OutlineSectionEntry,
        /// <summary>
        /// Outline section entry summary
        /// </summary>
        OutlineSectionEntrySummary,

        // Bibliography styles

        /// <summary>
        /// Bibliography section
        /// </summary>
        BibliographStyle,
        /// <summary>
        /// Bibliography author
        /// </summary>
        BibliographyAuthor,
        /// <summary>
        /// Bibliography number
        /// </summary>
        BibliographyNumber,
        /// <summary>
        /// Bibliography publisher
        /// </summary>
        BibliographyPublisher,
        /// <summary>
        /// Bibliography title
        /// </summary>
        BibliographyTitle,
        /// <summary>
        /// Bibliography citation
        /// </summary>
        Citation,

        // Code snippet and keyword styles

        /// <summary>
        /// Code snippet container
        /// </summary>
        CodeSnippetContainer,
        /// <summary>
        /// Code snippet container code
        /// </summary>
        CodeSnippetContainerCode,
        /// <summary>
        /// Code snippet container code container
        /// </summary>
        CodeSnippetContainerCodeContainer,
        /// <summary>
        /// Code snippet container tabs
        /// </summary>
        CodeSnippetContainerTabs,
        /// <summary>
        /// Code snippet container tab (single snippet)
        /// </summary>
        CodeSnippetContainerTabSingle,
        /// <summary>
        /// Code snippet container tab phantom (no example)
        /// </summary>
        CodeSnippetContainerTabPhantom,
        /// <summary>
        /// Code snippet container tab (multiple snippets)
        /// </summary>
        CodeSnippetContainerTab,
        /// <summary>
        /// Code snippet toolbar
        /// </summary>
        CodeSnippetToolBar,
        /// <summary>
        /// Code snippet toolbar text
        /// </summary>
        CodeSnippetToolBarText,
        /// <summary>
        /// Copy code snippet 
        /// </summary>
        CopyCodeSnippet,
        /// <summary>
        /// Code
        /// </summary>
        Code,
        /// <summary>
        /// Keyword
        /// </summary>
        Keyword,
        /// <summary>
        /// Parameter
        /// </summary>
        Parameter,
        /// <summary>
        /// Type parameter
        /// </summary>
        TypeParameter,

        // Glossary styles

        /// <summary>
        /// Glossary division
        /// </summary>
        GlossaryDiv,
        /// <summary>
        /// Glossary entry
        /// </summary>
        GlossaryEntry,
        /// <summary>
        /// Glossary group
        /// </summary>
        GlossaryGroup,
        /// <summary>
        /// Glossary group heading
        /// </summary>
        GlossaryGroupHeading,
        /// <summary>
        /// Glossary group list
        /// </summary>
        GlossaryGroupList,
        /// <summary>
        /// Glossary horizontal rule
        /// </summary>
        GlossaryRule,
        /// <summary>
        /// Non-link letter
        /// </summary>
        NoLink,
        /// <summary>
        /// Related entry
        /// </summary>
        RelatedEntry,

        // Section styles

        /// <summary>
        /// Alert/note
        /// </summary>
        Alert,
        /// <summary>
        /// Collapse toggle
        /// </summary>
        CollapseToggle,
        /// <summary>
        /// Collapsible area region
        /// </summary>
        CollapsibleAreaRegion,
        /// <summary>
        /// Collapsible region title
        /// </summary>
        CollapsibleRegionTitle,
        /// <summary>
        /// Collapsible section
        /// </summary>
        CollapsibleSection,
        /// <summary>
        /// Introduction
        /// </summary>
        Introduction,
        /// <summary>
        /// Procedure subheading
        /// </summary>
        ProcedureSubHeading,
        /// <summary>
        /// See Also style
        /// </summary>
        SeeAlsoStyle,
        /// <summary>
        /// Subheading
        /// </summary>
        SubHeading,
        /// <summary>
        /// Subsection
        /// </summary>
        SubSection,
        /// <summary>
        /// Summary
        /// </summary>
        Summary,

        // Media styles

        /// <summary>
        /// Caption
        /// </summary>
        Caption,
        /// <summary>
        /// Caption lead text
        /// </summary>
        CaptionLead,
        /// <summary>
        /// Media
        /// </summary>
        Media,
        /// <summary>
        /// Media centered
        /// </summary>
        MediaCenter,
        /// <summary>
        /// Media right-aligned
        /// </summary>
        MediaFar,
        /// <summary>
        /// Media left-aligned
        /// </summary>
        MediaNear,

        // Inline text styles

        /// <summary>
        /// Command
        /// </summary>
        Command,
        /// <summary>
        /// Foreign Phrase
        /// </summary>
        ForeignPhrase,
        /// <summary>
        /// Input
        /// </summary>
        Input,
        /// <summary>
        /// Label
        /// </summary>
        Label,
        /// <summary>
        /// Literal
        /// </summary>
        Literal,
        /// <summary>
        /// Math
        /// </summary>
        Math,
        /// <summary>
        /// Phrase
        /// </summary>
        Phrase,
        /// <summary>
        /// Placeholder
        /// </summary>
        Placeholder,
        /// <summary>
        /// Term
        /// </summary>
        Term,
        /// <summary>
        /// UI
        /// </summary>
        UI,

        // Miscellaneous styles

        /// <summary>
        /// Button
        /// </summary>
        Button,
        /// <summary>
        /// No-bullet list style
        /// </summary>
        NoBullet,
        /// <summary>
        /// Preliminary API
        /// </summary>
        Preliminary,
        /// <summary>
        /// Self-referencing link
        /// </summary>
        SelfLink
    }
}
