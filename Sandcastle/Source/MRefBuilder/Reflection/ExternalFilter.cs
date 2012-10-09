// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    // exposes all APIs that are visible from outside the assembly

    // this includes property and event accessors (e.g. get_), delegate methods (e.g. Invoke), as well as enumeration members

    public class ExternalFilter : ApiFilter {

        public override bool IsExposedMember(Member member) {
            if (member == null) throw new ArgumentNullException("member");
            // expose all visible members
            return (member.IsVisibleOutsideAssembly);
        }

        // we are satistied with the default namespace expose test, so don't override it

        public override bool IsExposedType(TypeNode type) {
            if (type == null) throw new ArgumentNullException("type");
            // expose all visible types
            return (type.IsVisibleOutsideAssembly);
        }

    }

}
