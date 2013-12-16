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
    /// This represents a property target
    /// </summary>
    [Serializable]
    public sealed class PropertyTarget : ProcedureTarget
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns an enumerable list of parameters if any
        /// </summary>
        public IList<Parameter> Parameters { get; private set; }

        /// <summary>
        /// This read-only property returns the return type
        /// </summary>
        public TypeReference ReturnType { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">The property parameters</param>
        /// <param name="returnType">The property return type</param>
        public PropertyTarget(IList<Parameter> parameters, TypeReference returnType)
        {
            this.Parameters = parameters;
            this.ReturnType = returnType;
        }
        #endregion
    }
}
