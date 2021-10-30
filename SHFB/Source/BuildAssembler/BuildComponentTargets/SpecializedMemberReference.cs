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
    /// This represents a specialized member reference
    /// </summary>
    [Serializable]
    public sealed class SpecializedMemberReference : MemberReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the template member
        /// </summary>
        public SimpleMemberReference TemplateMember { get; }

        /// <summary>
        /// This read-only property returns the specialized type
        /// </summary>
        public SpecializedTypeReference SpecializedType { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templateMember">The template member</param>
        /// <param name="specializedType">The specialized type</param>
        public SpecializedMemberReference(SimpleMemberReference templateMember,
          SpecializedTypeReference specializedType)
        {
            this.TemplateMember = templateMember ?? throw new ArgumentNullException(nameof(templateMember));
            this.SpecializedType = specializedType ?? throw new ArgumentNullException(nameof(specializedType));
        }
        #endregion
    }
}
