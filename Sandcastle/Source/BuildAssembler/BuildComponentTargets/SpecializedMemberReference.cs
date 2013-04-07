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
        public SimpleMemberReference TemplateMember { get; private set; }

        /// <summary>
        /// This read-only property returns the specialized type
        /// </summary>
        public SpecializedTypeReference SpecializedType { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templateMember"></param>
        /// <param name="specializedType"></param>
        public SpecializedMemberReference(SimpleMemberReference templateMember,
          SpecializedTypeReference specializedType)
        {
            if(templateMember == null)
                throw new ArgumentNullException("templateMember");

            if(specializedType == null)
                throw new ArgumentNullException("specializedType");

            this.TemplateMember = templateMember;
            this.SpecializedType = specializedType;
        }
        #endregion
    }
}
