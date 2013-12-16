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
    /// This represents a simple type reference
    /// </summary>
    [Serializable]
    public sealed class SimpleTypeReference : TypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the type ID
        /// </summary>
        public string Id { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The type ID</param>
        public SimpleTypeReference(string id)
        {
            if(id == null)
                throw new ArgumentNullException("id");

            this.Id = id;
        }
        #endregion
    }
}
