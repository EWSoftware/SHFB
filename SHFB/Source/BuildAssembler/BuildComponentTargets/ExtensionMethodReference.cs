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
    /// This represents and extension method reference
    /// </summary>
    [Serializable]
    public sealed class ExtensionMethodReference : Reference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the extension method name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This read-only property returns the extension method parameters
        /// </summary>
        public IList<Parameter> Parameters { get; }

        /// <summary>
        /// This read-only property returns the template arguments if any
        /// </summary>
        public IList<TypeReference> TemplateArgs { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodName">The extension method name</param>
        /// <param name="parameters">The extension method parameters</param>
        /// <param name="templateArgs">The extension method template arguments if any</param>
        public ExtensionMethodReference(string methodName, IList<Parameter> parameters,
          IList<TypeReference> templateArgs)
        {
            this.Name = methodName ?? throw new ArgumentNullException(nameof(methodName));
            this.Parameters = parameters;
            this.TemplateArgs = (templateArgs ?? new List<TypeReference>());
        }
        #endregion
    }
}
