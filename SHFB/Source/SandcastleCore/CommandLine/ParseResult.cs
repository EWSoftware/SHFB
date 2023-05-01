// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

namespace Sandcastle.Core.CommandLine
{
    /// <summary>
    /// This enumerated type defines the command line option parsing results
    /// </summary>
    internal enum ParseResult
    {
        /// <summary>Success</summary>
        Success,
        /// <summary>Argument not allowed</summary>
        ArgumentNotAllowed,
        /// <summary>Malformed argument</summary>
        MalformedArgument,
        /// <summary>Missing option</summary>
        MissingOption,
        /// <summary>Unrecognized option</summary>
        UnrecognizedOption,
        /// <summary>A single-use option appeared multiple times</summary>
        MultipleOccurrence
    }
}
