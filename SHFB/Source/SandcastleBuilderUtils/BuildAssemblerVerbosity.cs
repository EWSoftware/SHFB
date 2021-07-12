﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildAssemblerVerbosity.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type that defines the build assembler tool verbosity levels
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/31/2012  EFW  Created the code
//===============================================================================================================

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
