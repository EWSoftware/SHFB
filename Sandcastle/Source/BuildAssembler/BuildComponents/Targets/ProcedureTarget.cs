// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

namespace Microsoft.Ddue.Tools.Targets
{
    public class ProcedureTarget : MemberTarget
    {
        internal bool conversionOperator;

        internal MemberReference explicitlyImplements = null;

        public bool ConversionOperator
        {
            get
            {
                return (conversionOperator);
            }
        }

        public MemberReference ExplicitlyImplements
        {
            get
            {
                return (explicitlyImplements);
            }
        }
    }
}
