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
    /// This represents a simple member reference
    /// </summary>
    [Serializable]
    public class SimpleMemberReference : MemberReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the member ID
        /// </summary>
        public string Id { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The member ID</param>
        internal SimpleMemberReference(string id)
        {
            if(id == null)
                throw new ArgumentNullException("id");

            this.Id = id;
        }
        #endregion
    }
}
