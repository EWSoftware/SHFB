//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : Token.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/25/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content token that can
// be used to insert a common item, value, or construct into topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/24/2008  EFW  Created the code
// 1.8.0.0  07/25/2008  EFW  Reworked to support new MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content token that can be used to insert
    /// a common item, value, or construct into topics.
    /// </summary>
    /// <remarks>This class is serializable so that it can be copied to the
    /// clipboard.</remarks>
    [Serializable, DefaultProperty("TokenName")]
    public class Token
    {
        #region Private data members
        //=====================================================================

        private string tokenName, tokenValue;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the token name
        /// </summary>
        [Category("Token"), Description("The name of the token"),
          DefaultValue(null)]
        public string TokenName
        {
            get { return tokenName; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    tokenName = "NoName";
                else
                    tokenName = value;
            }
        }

        /// <summary>
        /// This is used to get or set the token value
        /// </summary>
        /// <value>The value can contain help file builder replacement tags.
        /// These will be replaced at build time with the appropriate project
        /// value.</value>
        [Category("Token"), Description("The value of the token"),
          DefaultValue(null),
          Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string TokenValue
        {
            get { return tokenValue; }
            set { tokenValue = value; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>The token name defaults to "NoName"</remarks>
        public Token()
        {
            this.TokenName = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The token name</param>
        /// <param name="value">The token value</param>
        public Token(string name, string value)
        {
            this.TokenName = name;
            this.TokenValue = value;
        }
        #endregion

        #region Convert to token element format
        //=====================================================================

        /// <summary>
        /// Convert the token to its <c>&lt;token&gt;</c> element form
        /// </summary>
        /// <returns>The token in its <c>&lt;token&gt;</c> element form</returns>
        public string ToToken()
        {
            return String.Concat("<token>", tokenName, "</token>");
        }
        #endregion
    }
}
