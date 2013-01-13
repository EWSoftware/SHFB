// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This represents a method target
    /// </summary>
    [Serializable]
    public class MethodTarget : ProcedureTarget
    {
        #region Properties
        //=====================================================================

        public IList<Parameter> Parameters { get; private set; }

        public TypeReference ReturnType { get; private set; }

        public IList<string> Templates { get; internal set; }

        // property to hold specialized template arguments (used with extension methods)
        public IList<TypeReference> TemplateArgs { get; internal set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">Method parameters if any</param>
        /// <param name="returnType">The method return type</param>
        public MethodTarget(IList<Parameter> parameters, TypeReference returnType)
        {
            this.Parameters = (parameters ?? new List<Parameter>());
            this.ReturnType = returnType;
        }
        #endregion
    }
}
