//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : MemberIdMatchExpression.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/14/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to contain the member ID match expression settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// =====================================================================================================
// 11/14/2014  EFW  Created the code
//===============================================================================================================

using System.Text.RegularExpressions;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This is used to hold the member ID match expression settings
    /// </summary>
    public class MemberIdMatchExpression
    {
        #region Private data members
        //=====================================================================

        private Regex regex;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The member ID expression to match
        /// </summary>
        public string MatchExpression { get; set; }

        /// <summary>
        /// The replacement member ID value
        /// </summary>
        public string ReplacementValue { get; set; }

        /// <summary>
        /// Match and replace the member ID using a regular expression
        /// </summary>
        /// <value>True to match and replace as a regular expression, false to use literal match and replace</value>
        public bool MatchAsRegEx { get; set; }

        /// <summary>
        /// This read-only property returns a <see cref="Regex"/> for the expression if it should be matched as a
        /// regular expression or null if not.
        /// </summary>
        /// <remarks>The regular expression is cached for future use</remarks>
        public Regex RegularExpression
        {
            get
            {
                if(this.MatchAsRegEx && regex == null)
                    regex = new Regex(this.MatchExpression);

                return regex;
            }
        }
        #endregion
    }
}
