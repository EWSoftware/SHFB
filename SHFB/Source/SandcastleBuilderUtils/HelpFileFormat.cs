//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : HelpFileFormat.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/27/2009
// Note    : Copyright 2006-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the help file format.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.1.0.0  08/28/2006  EFW  Created the code
// 1.3.0.0  09/02/2006  EFW  Added HelpFileFormat.Website support
// 1.4.0.0  02/16/2007  EFW  Added 1x + 2x and 1x + 2x + website combos
// 1.8.0.3  07/05/2009  EFW  Renamed Help 1 and Help 2 members to match
//                           the common conventions.  Removed the combos since
//                           they weren't used and are rather unwieldy. Added
//                           MS Help Viewer option.
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the type(s) of help file generated
    /// </summary>
    [Serializable, Flags]
    public enum HelpFileFormat
    {
        /// <summary>HTML Help 1 (.chm) format built with HHC.EXE.</summary>
        HtmlHelp1    = 0x0001,
        /// <summary>MS Help 2 (.HxS) format built with HXCOMP.EXE.</summary>
        MSHelp2      = 0x0002,
        /// <summary>MS Help Viewer (.mshc) format, a ZIP file containing the
        /// help content.</summary>
        MSHelpViewer = 0x0004,
        /// <summary>A website with a basic TOC and search panes.</summary>
        Website      = 0x0008
    }
}
