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
    /// This represents a reference type reference
    /// </summary>
    [Serializable]
    public sealed class ReferenceTypeReference : TypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the referred to type
        /// </summary>
        public TypeReference ReferredToType { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referredToType">The referred to type</param>
        public ReferenceTypeReference(TypeReference referredToType)
        {
            this.ReferredToType = referredToType ?? throw new ArgumentNullException(nameof(referredToType));
        }
        #endregion
    }
}
