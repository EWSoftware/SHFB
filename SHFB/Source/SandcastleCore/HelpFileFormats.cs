//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : HelpFileFormats.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/03/2015
// Note    : Copyright 2006-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the help file format.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.1.0.0  08/28/2006  EFW  Created the code
// 1.3.0.0  09/02/2006  EFW  Added HelpFileFormats.Website support
// 1.4.0.0  02/16/2007  EFW  Added 1x + 2x and 1x + 2x + website combos
// 1.8.0.3  07/05/2009  EFW  Renamed Help 1 and Help 2 members to match the common conventions.  Removed the
//                           combos since they weren't used and are rather unwieldy.  Added MS Help Viewer
//                           option.
// -------  01/04/2014  EFW  Moved the code to Sandcastle.Core
//          02/15/2014  EFW  Added support for the Open XML output format
//          03/30/2015  EFW  Added support for the Markdown output format
//          05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

using System;

namespace Sandcastle.Core
{
    /// <summary>
    /// This public enumerated type defines the type(s) of help file that can be generated
    /// </summary>
    [Flags, Serializable]
    public enum HelpFileFormats
    {
        /// <summary>HTML Help 1 (.chm) format built with HHC.EXE</summary>
        HtmlHelp1    = 0x0001,
        /// <summary>MS Help Viewer (.mshc) format, a ZIP file containing the help content</summary>
        MSHelpViewer = 0x0002,
        /// <summary>A website with a basic TOC and search panes</summary>
        Website      = 0x0004,
        /// <summary>Open XML (.docx) format, compatible with Microsoft Word and Open Office</summary>
        OpenXml      = 0x0008,
        /// <summary>Markdown (.md) format, suitable for use in wikis such as those for GitHub projects</summary>
        Markdown     = 0x0010
    }
}
