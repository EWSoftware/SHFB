// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class SpecializedMemberReference : MemberReference
    {
        private SimpleMemberReference member;

        private SpecializedTypeReference type;

        public SimpleMemberReference TemplateMember
        {
            get
            {
                return (member);
            }
        }

        public SpecializedTypeReference SpecializedType
        {
            get
            {
                return (type);
            }
        }

        internal SpecializedMemberReference(SimpleMemberReference member, SpecializedTypeReference type)
        {
            if(member == null)
                throw new ArgumentNullException("member");
            if(type == null)
                throw new ArgumentNullException("type");
            this.member = member;
            this.type = type;
        }
    }
}
