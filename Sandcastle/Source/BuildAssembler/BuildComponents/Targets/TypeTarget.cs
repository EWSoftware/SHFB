// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

namespace Microsoft.Ddue.Tools.Targets
{
    public class TypeTarget : Target
    {
        internal string name;

        internal NamespaceReference containingNamespace;

        internal SimpleTypeReference containingType;

        internal string[] templates;

        public string Name
        {
            get
            {
                return (name);
            }
        }

        public NamespaceReference Namespace
        {
            get
            {
                return (containingNamespace);
            }
        }

        public SimpleTypeReference OuterType
        {
            get
            {
                return (containingType);
            }
        }

        public string[] Templates
        {
            get
            {
                return (templates);
            }
        }
    }
}
