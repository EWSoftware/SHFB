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
    /// This represents a reference type reference
    /// </summary>
    [Serializable]
    public class ReferenceTypeReference : TypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the referred to type
        /// </summary>
        public TypeReference ReferredToType { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referredToType">The referred to type</param>
        internal ReferenceTypeReference(TypeReference referredToType)
        {
            if(referredToType == null)
                throw new ArgumentNullException("referredToType");

            this.ReferredToType = referredToType;
        }
        #endregion
    }
}
