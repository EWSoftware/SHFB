// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

namespace Microsoft.Ddue.Tools.Targets
{
    public class MethodTemplateTypeReference : TemplateTypeReference
    {
        private MemberReference template;

        private int position;

        public MemberReference TemplateMethod
        {
            get
            {
                return (template);
            }
        }

        public int Position
        {
            get
            {
                return (position);
            }
        }

        internal MethodTemplateTypeReference(MemberReference template, int position)
        {
            this.template = template;
            this.position = position;
        }
    }
}
