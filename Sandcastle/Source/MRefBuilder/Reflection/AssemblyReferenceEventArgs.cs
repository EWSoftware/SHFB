// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class AssemblyReferenceEventArgs : EventArgs {

        private Module module;

        private AssemblyReference reference;

        public AssemblyReferenceEventArgs(AssemblyReference reference, Module module) {
            this.reference = reference;
            this.module = module;
        }

        public AssemblyReference Reference {
            get {
                return (reference);
            }
        }

        public Module Referrer {
            get {
                return (module);
            }
        }
    }

}
