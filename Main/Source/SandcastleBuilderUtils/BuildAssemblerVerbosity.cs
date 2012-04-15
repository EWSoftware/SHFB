//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildAssemblerVerbosity.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/31/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an enumerated type that defines the build assembler tool
// verbosity levels.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.4.0  03/31/2012  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the build assembler tool verbosity levels
    /// </summary>
    [Serializable]
    public enum BuildAssemblerVerbosity
    {
        /// <summary>Report all messages (the default)</summary>
        AllMessages,
        /// <summary>Only warning and error messages</summary>
        OnlyWarningsAndErrors,
        /// <summary>Only error messages</summary>
        OnlyErrors
    }
}
