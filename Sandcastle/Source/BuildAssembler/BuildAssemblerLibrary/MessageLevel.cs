// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added Diagnostic message level type for diagnostic messages.  This allows messages
// to appear regardless of the verbosity level.
// 10/14/2012 - EFW - Added support for topic ID and message parameters in the message logging methods.

using System;

namespace Microsoft.Ddue.Tools
{
    #region Message level enumeration
    //=====================================================================

    /// <summary>
    /// This enumerated type defines the message logging levels
    /// </summary>
    public enum MessageLevel
    {
        /// <summary>Do not show at all</summary>
        Ignore,
        /// <summary>Informational message</summary>
        Info,
        /// <summary>A warning message (a minor problem)</summary>
        Warn,
        /// <summary>An error message (a major problem that will stop the build)</summary>
        Error,
        /// <summary>A diagnostic message, useful for debugging</summary>
        Diagnostic
    }
    #endregion
}
