// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.Targets
{
    public class EnumerationTarget : TypeTarget
    {
        internal MemberTarget[] elements;

        public override void Add(IDictionary<string, Target> targets)
        {
            base.Add(targets);

            foreach(MemberTarget element in elements)
                element.Add(targets);
        }
    }
}
