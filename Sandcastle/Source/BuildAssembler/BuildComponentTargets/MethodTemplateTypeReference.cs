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
    /// This represents a method template type reference
    /// </summary>
    [Serializable]
    public sealed class MethodTemplateTypeReference : TemplateTypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the template method
        /// </summary>
        public MemberReference TemplateMethod { get; private set; }

        /// <summary>
        /// This read only property returns the position
        /// </summary>
        public int Position { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="template">The template method</param>
        /// <param name="position">The position</param>
        public MethodTemplateTypeReference(MemberReference template, int position)
        {
            this.TemplateMethod = template;
            this.Position = position;
        }
        #endregion
    }
}
