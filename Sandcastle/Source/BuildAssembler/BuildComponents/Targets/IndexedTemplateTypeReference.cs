// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class IndexedTemplateTypeReference : TemplateTypeReference
    {
        private string templateId;

        private int index;

        public string TemplateId
        {
            get
            {
                return (templateId);
            }
        }

        public int Index
        {
            get
            {
                return (index);
            }
        }

        internal IndexedTemplateTypeReference(string templateId, int index)
        {
            if(templateId == null)
                throw new ArgumentNullException("templateId");
            if(index < 0)
                throw new ArgumentOutOfRangeException("index");
            this.templateId = templateId;
            this.index = index;
        }

        public override int GetHashCode()
        {
            return (index ^ templateId.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            IndexedTemplateTypeReference other = obj as IndexedTemplateTypeReference;

            if(other == null)
                return false;

            if(this.index == other.index && this.templateId == other.templateId)
                return true;

            return false;
        }
    }
}
