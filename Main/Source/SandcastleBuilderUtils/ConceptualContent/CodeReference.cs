//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : CodeReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/12/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content code reference
// stored in an example code file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/12/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.IO;
using System.Xml;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content CodeReference that can be used to insert
    /// a common item, value, or construct into topics.
    /// </summary>
    /// <remarks>This class is serializable so that it can be copied to the
    /// clipboard.</remarks>
    [Serializable, DefaultProperty("Id")]
    public class CodeReference
    {
        #region Private data members
        //=====================================================================

        private string id;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the CodeReference name
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="refId">The reference ID</param>
        public CodeReference(string refId)
        {
            id = refId;
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
            return String.Concat("<codeReference>", id, "</codeReference>");
        }
        #endregion
    }
}
