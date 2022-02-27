// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added Diagnostic message level type for diagnostic messages.  This allows messages
// to appear regardless of the verbosity level.
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly
// 02/25/2022 - EFW - Removed the Ignore value and replaced LogLevel with MessageLevel

namespace Sandcastle.Core
{
    /// <summary>
    /// This enumerated type defines the message logging levels
    /// </summary>
    public enum MessageLevel
    {
        /// <summary>Informational message</summary>
        Info,
        /// <summary>A warning message (a minor problem)</summary>
        Warn,
        /// <summary>An error message (a major problem that will stop the build)</summary>
        Error,
        /// <summary>A diagnostic message, useful for debugging</summary>
        Diagnostic
    }
}
