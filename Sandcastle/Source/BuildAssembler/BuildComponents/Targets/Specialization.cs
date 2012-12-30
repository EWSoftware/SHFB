// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class Specialization
    {
        private SimpleTypeReference template;

        private TypeReference[] arguments;

        public SimpleTypeReference TemplateType
        {
            get
            {
                return (template);
            }
        }

        public TypeReference[] Arguments
        {
            get
            {
                return arguments;
            }
        }

        internal Specialization(SimpleTypeReference template, TypeReference[] arguments)
        {
            if(template == null)
                throw new ArgumentNullException("template");
            if(arguments == null)
                throw new ArgumentNullException("arguments");
            this.template = template;
            this.arguments = arguments;
        }
    }
}
