// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This represents an invalid reference
    /// </summary>
    [Serializable]
    public sealed class InvalidReference : Reference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the ID
        /// </summary>
        public string Id { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The invalid reference ID</param>
        public InvalidReference(string id)
        {
            this.Id = id;
        }
        #endregion
    }
}
