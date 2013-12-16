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
    /// This represents a type target
    /// </summary>
    [Serializable]
    public class TypeTarget : Target
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This is used to get or set the containing namespace
        /// </summary>
        public NamespaceReference ContainingNamespace { get; set; }

        /// <summary>
        /// This is used to get or set the containing type
        /// </summary>
        public SimpleTypeReference ContainingType { get; set; }

        /// <summary>
        /// This is used to get or set the templates
        /// </summary>
        public IList<string> Templates { get; set; }
        #endregion
    }
}
