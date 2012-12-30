// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

namespace Microsoft.Ddue.Tools.Targets
{
    public class SimpleTypeReference : TypeReference
    {
        private string typeId;

        public string Id
        {
            get
            {
                return (typeId);
            }
        }

        public Target Resolve(TargetCollection targets)
        {
            return (targets[typeId]);
        }

        internal SimpleTypeReference(string id)
        {
            this.typeId = id;
        }
    }
}
