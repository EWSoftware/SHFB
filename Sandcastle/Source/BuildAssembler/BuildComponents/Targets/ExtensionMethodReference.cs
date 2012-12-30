// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// Contains the information to generate the display string for an extension method link
    /// </summary>
    public class ExtensionMethodReference : Reference
    {
        private string methodName;

        public string Name
        {
            get
            {
                return (methodName);
            }
        }

        private Parameter[] parameters;

        public Parameter[] Parameters
        {
            get
            {
                return (parameters);
            }
        }

        private TypeReference[] templateArgs;

        public TypeReference[] TemplateArgs
        {
            get
            {
                return (templateArgs);
            }
        }

        internal ExtensionMethodReference(string methodName, Parameter[] parameters, TypeReference[] templateArgs)
        {
            if(methodName == null)
                throw new ArgumentNullException("methodName");

            this.methodName = methodName;
            this.parameters = parameters;
            this.templateArgs = templateArgs;
        }
    }
}
