//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : CodeEntityReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/12/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content code entity
// reference link.
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
    /// This represents a conceptual content CodeEntityReference that can be used to insert
    /// a common item, value, or construct into topics.
    /// </summary>
    /// <remarks>This class is serializable so that it can be copied to the
    /// clipboard.</remarks>
    [Serializable, DefaultProperty("Id")]
    public class CodeEntityReference
    {
        #region Private data members
        //=====================================================================

        private string id;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the CodeEntityReference name
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
        public CodeEntityReference(string refId)
        {
            id = refId;
        }
        #endregion

        #region Convert to code reference element format
        //=====================================================================

        /// <summary>
        /// Convert the entity to its <c>&lt;codeEntityReference&gt;</c> element
        /// form
        /// </summary>
        /// <returns>The entity in its <c>&lt;codeEntityReference&gt;</c>
        /// element form</returns>
        public string ToCodeEntityReference()
        {
            string autoUpgrade;

            // If it's a member, add the autoUpgrade attribute so that
            // overloads are preferred if found.
            if(id[0] == 'M')
                autoUpgrade = " autoUpgrade=\"true\"";
            else
                autoUpgrade = String.Empty;

            // The name will not be fully qualified by default
            return String.Concat("<codeEntityReference qualifyHint=\"false\"",
                autoUpgrade, ">", id, "</codeEntityReference>");
        }

        /// <summary>
        /// Convert the entity to its <c>&lt;see&gt;</c> element form
        /// </summary>
        /// <returns>The token in its <c>&lt;see&gt;</c> element form</returns>
        public string ToSee()
        {
            return String.Concat("<see cref=\"", id, "\" />");
        }
        #endregion
    }
}
