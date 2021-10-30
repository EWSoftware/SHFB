// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This represents a type template type reference
    /// </summary>
    [Serializable]
    public sealed class TypeTemplateTypeReference : TemplateTypeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the template type
        /// </summary>
        public SimpleTypeReference TemplateType { get; }

        /// <summary>
        /// This read-only property returns the position
        /// </summary>
        public int Position { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templateType">The template type</param>
        /// <param name="position">The position</param>
        public TypeTemplateTypeReference(SimpleTypeReference templateType, int position)
        {
            if(position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            this.TemplateType = templateType ?? throw new ArgumentNullException(nameof(templateType));
            this.Position = position;
        }
        #endregion
    }
}
