// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the class into the Snippets namespace and made it public

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Ddue.Tools.Snippets
{
    /// <summary>
    /// This defines a colorization rule
    /// </summary>
    public class ColorizationRule
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the regular expression used by the rule
        /// </summary>
        public Regex Pattern { get; private set; }

        /// <summary>
        /// This read-only property returns the optional region name used to limit the part of the match that
        /// is returned from the matches.
        /// </summary>
        public string Region { get; private set; }

        /// <summary>
        /// This read-only property returns the class name to use for the matched region
        /// </summary>
        public string ClassName { get; private set; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor.  Defines a rule with a pattern and a class name but no region name
        /// </summary>
        /// <param name="pattern">The regular expression used to match the region text</param>
        /// <param name="className">The class name to apply to the region</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public ColorizationRule(string pattern, string className) : this(pattern, null, className)
        {
        }

        /// <summary>
        /// Constructor.  Defines a rule with a pattern, a region name to limit the match to a specific
        /// part of the pattern, and a class name.
        /// </summary>
        /// <param name="pattern">The regular expression used to match the region text</param>
        /// <param name="region">The region name that defines the named part of the regular expression to return
        /// for each match.</param>
        /// <param name="className">The class name to apply to the region</param>
        public ColorizationRule(string pattern, string region, string className)
        {
            this.Pattern = new Regex(pattern, RegexOptions.Multiline);
            this.Region = region;
            this.ClassName = className;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This returns an enumerable list of captures that match the pattern, if any
        /// </summary>
        /// <param name="text">The text to search</param>
        /// <returns>An enumerable list of captures that match the pattern</returns>
        public IEnumerable<Capture> Apply(string text)
        {
            MatchCollection matches = this.Pattern.Matches(text);

            foreach(Match m in matches)
                if(this.Region == null)
                    yield return m;
                else
                    yield return m.Groups[this.Region];
        }
        #endregion
    }
}
