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
    /// This represents a namespace target
    /// </summary>
    [Serializable]
    public sealed class NamespaceTarget : Target
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the namespace name
        /// </summary>
        public string Name { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The namespace name</param>
        public NamespaceTarget(string name)
        {
            this.Name = name;
        }
        #endregion
    }
}
