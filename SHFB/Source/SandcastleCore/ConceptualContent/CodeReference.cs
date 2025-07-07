//=============================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a conceptual content code reference stored in an example code file
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
    /// This represents a conceptual content CodeReference that can be used to insert a common item, value, or
    /// construct into topics.
    /// </summary>
    public class CodeReference
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the CodeReference name
        /// </summary>
        public string Id { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The reference ID</param>
        public CodeReference(string id)
        {
            this.Id = id;
        }
        #endregion

        #region Convert to code reference element format
        //=====================================================================

        /// <summary>
        /// Convert the token to its <c>&lt;codeReference&gt;</c> element form
        /// </summary>
        /// <returns>The token in its <c>&lt;codeReference&gt;</c> element form</returns>
        public string ToCodeReference()
        {
            return String.Concat("<codeReference>", this.Id, "</codeReference>");
        }
        #endregion
    }
}
