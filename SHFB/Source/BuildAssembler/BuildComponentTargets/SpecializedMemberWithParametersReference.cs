// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This represents a specialized member with paramters reference
    /// </summary>
    [Serializable]
    public sealed class SpecializedMemberWithParametersReference : MemberReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the prefix
        /// </summary>
        public string Prefix { get ; private set; }

        /// <summary>
        /// This read-only property returns the specialized type
        /// </summary>
        public SpecializedTypeReference SpecializedType { get; private set; }

        /// <summary>
        /// This read-only property returns the member name
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// This read-only property returns the parameter types
        /// </summary>
        public IList<TypeReference> ParameterTypes { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <param name="specializedType">The specialized type</param>
        /// <param name="memberName">The member name</param>
        /// <param name="parameters">The parameters</param>
        public SpecializedMemberWithParametersReference(string prefix, SpecializedTypeReference specializedType,
          string memberName, IList<TypeReference> parameters)
        {
            if(specializedType == null)
                throw new ArgumentNullException("specializedType");

            if(parameters == null)
                throw new ArgumentNullException("parameters");

            this.Prefix = prefix;
            this.SpecializedType = specializedType;
            this.MemberName = memberName;
            this.ParameterTypes = parameters;
        }
        #endregion
    }
}
