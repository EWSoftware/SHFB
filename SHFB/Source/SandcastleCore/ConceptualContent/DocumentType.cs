//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : DocumentType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/21/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the conceptual document types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all// applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/25/2008  EFW  Created the code
// 05/08/2015  EFW  Removed support raw HTML files
//===============================================================================================================

using System;

namespace Sandcastle.Core.ConceptualContent;

/// <summary>
/// This public enumerated type defines the conceptual document types
/// </summary>
[Serializable]
public enum DocumentType
{
    /// <summary>Unknown document type or empty container node</summary>
    None,
    /// <summary>File not found</summary>
    NotFound,
    /// <summary>Invalid document (i.e. bad format, parsing errors)</summary>
    Invalid,
    /// <summary>Conceptual document</summary>
    DeveloperConceptualDocument,
    /// <summary>Error Message document</summary>
    DeveloperErrorMessageDocument,
    /// <summary>Glossary document</summary>
    DeveloperGlossaryDocument,
    /// <summary>How To document</summary>
    DeveloperHowToDocument,
    /// <summary>Orientation document</summary>
    DeveloperOrientationDocument,
    /// <summary>Reference List document</summary>
    CodeEntityDocument,
    /// <summary>Reference With Syntax document</summary>
    DeveloperReferenceWithSyntaxDocument,
    /// <summary>Reference Without Syntax document</summary>
    DeveloperReferenceWithoutSyntaxDocument,
    /// <summary>Sample Document</summary>
    DeveloperSampleDocument,
    /// <summary>SDK Technology Overview Architecture document</summary>
    DeveloperSDKTechnologyOverviewArchitectureDocument,
    /// <summary>SDK Technology Overview Code Directory document</summary>
    DeveloperSDKTechnologyOverviewCodeDirectoryDocument,
    /// <summary>SDK Technology Overview Orientation document</summary>
    DeveloperSDKTechnologyOverviewOrientationDocument,
    /// <summary>SDK Technology Overview Scenarios document</summary>
    DeveloperSDKTechnologyOverviewScenariosDocument,
    /// <summary>SDK Technology Overview Technology Summary document</summary>
    DeveloperSDKTechnologyOverviewTechnologySummaryDocument,
    /// <summary>Troubleshooting document</summary>
    DeveloperTroubleshootingDocument,
    /// <summary>User Interface Reference document</summary>
    DeveloperUIReferenceDocument,
    /// <summary>Walkthrough document</summary>
    DeveloperWalkthroughDocument,
    /// <summary>Whitepaper document</summary>
    DeveloperWhitePaperDocument,
    /// <summary>XML Reference document</summary>
    DeveloperXmlReference,
    /// <summary>Markdown</summary>
    Markdown
}
