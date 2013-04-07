// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This represents a member target
    /// </summary>
    [Serializable]
    public class MemberTarget : Target
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This is used to get or set the containing type
        /// </summary>
        public TypeReference ContainingType { get; set; }

        /// <summary>
        /// This is used to get or set the overload ID if applicable
        /// </summary>
        public string OverloadId { get; set; }

        #endregion
    }
}
