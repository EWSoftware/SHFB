﻿// Copyright © Microsoft Corporation.
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
    /// This represents a specialization
    /// </summary>
    [Serializable]
    public sealed class Specialization
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the template type
        /// </summary>
        public SimpleTypeReference TemplateType { get; }

        /// <summary>
        /// This read-only property returns the arguments
        /// </summary>
        public IList<TypeReference> Arguments { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templateType">The template type</param>
        /// <param name="arguments">The arguments</param>
        public Specialization(SimpleTypeReference templateType, IList<TypeReference> arguments)
        {
            this.TemplateType = templateType ?? throw new ArgumentNullException(nameof(templateType));
            this.Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }
        #endregion
    }
}
