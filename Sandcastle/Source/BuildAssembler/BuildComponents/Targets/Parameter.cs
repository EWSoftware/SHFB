// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

namespace Microsoft.Ddue.Tools.Targets
{
    public class Parameter
    {
        private string name;

        private TypeReference type;

        public string Name
        {
            get
            {
                return (name);
            }
        }

        public TypeReference Type
        {
            get
            {
                return (type);
            }
        }

        internal Parameter(string name, TypeReference type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
