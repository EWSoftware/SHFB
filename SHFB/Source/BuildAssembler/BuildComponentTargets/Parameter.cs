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
    /// This represents a parameter
    /// </summary>
    [Serializable]
    public sealed class Parameter
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the parameter name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This read-only property returns the parameter type
        /// </summary>
        public TypeReference ParameterType { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="type">The parameter type</param>
        public Parameter(string name, TypeReference type)
        {
            this.Name = name;
            this.ParameterType = type;
        }
        #endregion
    }
}
