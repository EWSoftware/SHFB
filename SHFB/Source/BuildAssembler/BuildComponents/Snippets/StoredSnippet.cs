// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the class into the Snippets namespace and made it public

namespace Microsoft.Ddue.Tools.Snippets
{
    /// <summary>
    /// This represents a stored snippet
    /// </summary>
    public class StoredSnippet
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the snippet text
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// This read-only property returns the snippet language
        /// </summary>
        public string Language { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">The snippet text</param>
        /// <param name="language">The snippet language</param>
        public StoredSnippet(string text, string language)
        {
            this.Text = text;
            this.Language = language;
        }
        #endregion
    }
}
