// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;

using System.Compiler;

namespace Sandcastle.Tools.Reflection
{
    /// <summary>
    /// This event arguments class contains information used to resolve assembly references
    /// </summary>
    public class AssemblyReferenceEventArgs : EventArgs
    {
        /// <summary>
        /// This read-only property returns the assembly reference needed
        /// </summary>
        public AssemblyReference Reference { get; }

        /// <summary>
        /// This read-only property returns the module requiring the reference
        /// </summary>
        public Module Referrer { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reference">The assembly reference needed</param>
        /// <param name="referrer">The module requiring the reference</param>
        public AssemblyReferenceEventArgs(AssemblyReference reference, Module referrer)
        {
            this.Reference = reference;
            this.Referrer = referrer;
        }
    }
}
