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
    /// This represents a constructor target
    /// </summary>
    [Serializable]
    public class ConstructorTarget : MemberTarget
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns an enumerable list of parameters if any
        /// </summary>
        public IList<Parameter> Parameters { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">The list of constructor parameters if any</param>
        internal ConstructorTarget(IList<Parameter> parameters)
        {
            this.Parameters = (parameters ?? new List<Parameter>());
        }
        #endregion
    }
}
