// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    // exposes all apis for which a topic exists
    // this includes all documented members (see DocumentFilter) except for enumeration members

    public class AllTopicFilter : AllDocumentedFilter {

        public override bool IsExposedMember(Member member) {
            // don't expose members of enumerations
            if (member.DeclaringType.NodeType == NodeType.EnumNode) return (false);
            // otherwise, agree with DocumentedFilter
            return (base.IsExposedMember(member));
        }

    }

}
