// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;
using System.Collections.Generic;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This represents a method target
    /// </summary>
    [Serializable]
    public sealed class MethodTarget : ProcedureTarget
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns an enumerable list of parameters if any
        /// </summary>
        public IList<Parameter> Parameters { get; }

        /// <summary>
        /// This read-only property returns the return type
        /// </summary>
        public TypeReference ReturnType { get; }

        /// <summary>
        /// This is used to get or set an enumerable list of the template types if any
        /// </summary>
        public IList<string> Templates { get; set; }

        /// <summary>
        /// This is used to get or set specialized template arguments if any (used with extension methods)
        /// </summary>
        public IList<TypeReference> TemplateArgs { get; set; }

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
