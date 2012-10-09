// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class NoFilter : ApiFilter {

        public override bool IsExposedApi(Member api) {
            return (true);
        }

        public override bool IsExposedMember(Member member) {
            return (true);
        }

        public override bool IsExposedNamespace(Namespace space) {
            return (true);
        }

        public override bool IsExposedType(TypeNode type) {
            return (true);
        }

    }

}
