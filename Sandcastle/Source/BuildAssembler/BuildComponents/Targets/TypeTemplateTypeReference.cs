// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class TypeTemplateTypeReference : TemplateTypeReference
    {
        private SimpleTypeReference template;

        private int position;

        public SimpleTypeReference TemplateType
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

        internal TypeTemplateTypeReference(SimpleTypeReference template, int position)
        {
            if(template == null)
                throw new ArgumentNullException("template");
            if(position < 0)
                throw new ArgumentOutOfRangeException("position");
            this.template = template;
            this.position = position;
        }
    }
}
