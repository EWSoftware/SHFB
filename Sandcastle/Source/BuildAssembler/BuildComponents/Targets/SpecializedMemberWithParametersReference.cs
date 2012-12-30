// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class SpecializedMemberWithParametersReference : MemberReference
    {
        private string prefix;

        private SpecializedTypeReference type;

        private string member;

        private TypeReference[] parameters;

        public string Prefix
        {
            get
            {
                return (prefix);
            }
        }

        public SpecializedTypeReference SpecializedType
        {
            get
            {
                return (type);
            }
        }

        public string MemberName
        {
            get
            {
                return (member);
            }
        }

        public TypeReference[] ParameterTypes
        {
            get
            {
                return (parameters);
            }
        }

        internal SpecializedMemberWithParametersReference(string prefix, SpecializedTypeReference type,
          string member, TypeReference[] parameters)
        {
            if(type == null)
                throw new ArgumentNullException("type");
            if(parameters == null)
                throw new ArgumentNullException("parameters");
            this.prefix = prefix;
            this.type = type;
            this.member = member;
            this.parameters = parameters;
        }
    }
}
