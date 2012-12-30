// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.Targets
{
    public class SpecializedTypeReference : TypeReference
    {
        private Specialization[] specializations;

        public Specialization[] Specializations
        {
            get
            {
                return (specializations);
            }
        }

        internal SpecializedTypeReference(Specialization[] specializations)
        {
            if(specializations == null)
                throw new ArgumentNullException("specializations");
            this.specializations = specializations;
        }

        public Dictionary<IndexedTemplateTypeReference, TypeReference> GetSpecializationDictionary()
        {
            Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary =
                new Dictionary<IndexedTemplateTypeReference, TypeReference>();

            foreach(Specialization specialization in specializations)
            {
                for(int index = 0; index < specialization.Arguments.Length; index++)
                {
                    IndexedTemplateTypeReference template = new IndexedTemplateTypeReference(
                        specialization.TemplateType.Id, index);

                    dictionary.Add(template, specialization.Arguments[index]);
                }
            }

            return dictionary;
        }
    }
}
