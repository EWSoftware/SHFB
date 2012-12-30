//===============================================================================================================
// System  : Sandcastle Build Components
// File    : DisplayOptions.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains an enumerated type that defines the display options for reference content links
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/26/2012 - EFW - Moved the enum into its own file
//===============================================================================================================

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This public enumerated type defines the display options for reference content links
    /// </summary>
    [Flags, Serializable]
    public enum DisplayOptions
    {
        /// <summary>Show the containing namespace/class</summary>
        ShowContainer = 1,
        /// <summary>Show template types</summary>
        ShowTemplates = 2,
        /// <summary>Show parameters</summary>
        ShowParameters = 4,
        /// <summary>Default (show template types and parameters)</summary>
        Default = 6,
        /// <summary>Show all</summary>
        All = 7
    }
}
