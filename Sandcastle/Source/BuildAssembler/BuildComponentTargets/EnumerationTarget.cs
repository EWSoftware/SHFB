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
    /// This represents an enumeration target
    /// </summary>
    [Serializable]
    public sealed class EnumerationTarget : TypeTarget
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns a list of enumeration elements
        /// </summary>
        public IList<MemberTarget> Elements { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elements">The list of enumeration members</param>
        public EnumerationTarget(IList<MemberTarget> elements)
        {
            this.Elements = elements;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to add the enumeration along with all of its elements to the target dictionary
        /// </summary>
        /// <param name="targets">The target dictionary to which the enumeration and its elements are added</param>
        public override void Add(IDictionary<string, Target> targets)
        {
            base.Add(targets);

            foreach(MemberTarget element in this.Elements)
                element.Add(targets);
        }
        #endregion
    }
}
