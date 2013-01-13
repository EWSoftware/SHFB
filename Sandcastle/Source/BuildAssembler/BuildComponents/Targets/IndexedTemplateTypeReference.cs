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
    /// This represents an indexed template type reference
    /// </summary>
    [Serializable]
    public class IndexedTemplateTypeReference : TemplateTypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the template ID
        /// </summary>
        public string TemplateId { get; private set; }

        /// <summary>
        /// This read-only property returns the index
        /// </summary>
        public int Index { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templateId">The template ID</param>
        /// <param name="index">The index</param>
        internal IndexedTemplateTypeReference(string templateId, int index)
        {
            if(templateId == null)
                throw new ArgumentNullException("templateId");

            if(index < 0)
                throw new ArgumentOutOfRangeException("index");

            this.TemplateId = templateId;
            this.Index = index;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to support equality comparisons
        /// </summary>
        /// <returns>The hash code for the indexed template type reference</returns>
        public override int GetHashCode()
        {
            return (this.Index ^ this.TemplateId.GetHashCode());
        }

        /// <summary>
        /// This is overridden to allow comparison of to indexed template type references for equality
        /// </summary>
        /// <param name="obj">The instance to compare</param>
        /// <returns>True if equal, false if not</returns>
        public override bool Equals(object obj)
        {
            IndexedTemplateTypeReference other = obj as IndexedTemplateTypeReference;

            return (other != null && this.Index == other.Index && this.TemplateId == other.TemplateId);
        }
        #endregion
    }
}
