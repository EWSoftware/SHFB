// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class ReferenceTypeReference : TypeReference
    {
        private TypeReference referredToType;

        public TypeReference ReferredToType
        {
            get
            {
                return referredToType;
            }
        }

        internal ReferenceTypeReference(TypeReference referredToType)
        {
            if(referredToType == null)
                throw new ArgumentNullException("referredToType");
            this.referredToType = referredToType;
        }
    }
}
