// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;
using System.Collections.Generic;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This represents a specialized type reference
    /// </summary>
    [Serializable]
    public sealed class SpecializedTypeReference : TypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the specializations
        /// </summary>
        public IList<Specialization> Specializations { get; }

        /// <summary>
        /// This read-only property is used to create and return a specialization dictionary
        /// </summary>
        /// <returns>The specialization dictionary</returns>
        public Dictionary<IndexedTemplateTypeReference, TypeReference> SpecializationDictionary
        {
            get
            {
                var dictionary = new Dictionary<IndexedTemplateTypeReference, TypeReference>();

                foreach(Specialization specialization in this.Specializations)
                    for(int index = 0; index < specialization.Arguments.Count; index++)
                    {
                        dictionary.Add(new IndexedTemplateTypeReference(specialization.TemplateType.Id, index),
                            specialization.Arguments[index]);
                    }

                return dictionary;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="specializations">The specializations</param>
        public SpecializedTypeReference(IList<Specialization> specializations)
        {
            this.Specializations = specializations ?? throw new ArgumentNullException(nameof(specializations));
        }
        #endregion
    }
}
