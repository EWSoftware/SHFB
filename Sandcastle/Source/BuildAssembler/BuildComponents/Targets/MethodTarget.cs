// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

namespace Microsoft.Ddue.Tools.Targets
{
    public class MethodTarget : ProcedureTarget
    {
        internal Parameter[] parameters;

        internal TypeReference returnType;

        internal string[] templates;

        public Parameter[] Parameters
        {
            get
            {
                return (parameters);
            }
        }

        public string[] Templates
        {
            get
            {
                return (templates);
            }
        }

        // property to hold specialized template arguments (used with extension methods)
        internal TypeReference[] templateArgs;

        public TypeReference[] TemplateArgs
        {
            get
            {
                return (templateArgs);
            }
        }
    }
}
