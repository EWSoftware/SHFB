// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class SimpleMemberReference : MemberReference
    {
        private string memberId;

        public string Id
        {
            get
            {
                return (memberId);
            }
        }

        public Target Resolve(TargetCollection targets)
        {
            return (targets[memberId]);
        }

        internal SimpleMemberReference(string id)
        {
            if(id == null)
                throw new ArgumentNullException("id");

            this.memberId = id;
        }
    }
}
