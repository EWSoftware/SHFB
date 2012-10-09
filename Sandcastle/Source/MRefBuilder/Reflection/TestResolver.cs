// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class TestResolver : AssemblyResolver {

        public TestResolver(XPathNavigator configuration) : base(configuration) { }

        public override AssemblyNode ResolveReference(AssemblyReference reference, Module module) {
            Console.WriteLine("test resolver: {0}", reference.StrongName);
            return (base.ResolveReference(reference, module));
        }
    }

}
