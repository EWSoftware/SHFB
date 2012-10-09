// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public abstract class ApiNamer {

        public virtual string GetApiName(Member api) {

            Namespace space = api as Namespace;
            if (space != null) return (GetNamespaceName(space));

            TypeNode type = api as TypeNode;
            if (type != null) return (GetTypeName(type));

            return (GetMemberName(api));

        }

        public abstract string GetMemberName(Member member);

        public abstract string GetNamespaceName(Namespace space);

        public abstract string GetTypeName(TypeNode type);

    }

}
