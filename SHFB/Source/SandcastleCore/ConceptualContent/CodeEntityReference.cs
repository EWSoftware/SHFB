//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeEntityReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a conceptual content code entity reference link
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/12/2008  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content CodeEntityReference that can be used to insert a common item, value,
    /// or construct into topics.
    /// </summary>
    public class CodeEntityReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the CodeEntityReference name
        /// </summary>
        public string Id { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The reference ID</param>
        public CodeEntityReference(string id)
        {
            this.Id = id;
        }
        #endregion

        #region Convert to code reference element format
        //=====================================================================

        /// <summary>
        /// Convert the entity to its <c>&lt;codeEntityReference&gt;</c> element form
        /// </summary>
        /// <returns>The entity in its <c>&lt;codeEntityReference&gt;</c> element form</returns>
        public string ToCodeEntityReference()
        {
            string autoUpgrade;

            // If it's a member, add the autoUpgrade attribute so that
            // overloads are preferred if found.
            if(this.Id[0] == 'M')
                autoUpgrade = " autoUpgrade=\"true\"";
            else
                autoUpgrade = String.Empty;

            // The name will not be fully qualified by default
            return String.Concat("<codeEntityReference qualifyHint=\"false\"", autoUpgrade, ">", this.Id,
                "</codeEntityReference>");
        }

        /// <summary>
        /// Convert the entity to its <c>&lt;see&gt;</c> element form
        /// </summary>
        /// <returns>The token in its <c>&lt;see&gt;</c> element form</returns>
        public string ToSee()
        {
            return String.Concat("<see cref=\"", this.Id, "\" />");
        }
        #endregion
    }
}
