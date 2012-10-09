//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : DocumentType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/25/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the conceptual document
// types.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/25/2008  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
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
        /// <summary>SDK Technology Overview Architechture document</summary>
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
        /// <summary>Raw HTML</summary>
        Html
    }
}
