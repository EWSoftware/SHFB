// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the class into the Snippets namespace and made it public

namespace Sandcastle.Tools.BuildComponents.Snippets
{
    /// <summary>
    /// This defines a region of colorized code
    /// </summary>
    public struct Region
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the class name used to colorize the text
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// This read-only property returns the text in the region
        /// </summary>
        public string Text { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor.  A region of text with no class name.
        /// </summary>
        /// <param name="text">The text in the region</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public Region(string text) : this(null, text)
        {
        }

        /// <summary>
        /// Constructor.  A region of text with a class name.
        /// </summary>
        /// <param name="className">The class name to apply to the region</param>
        /// <param name="text">The text in the region</param>
        public Region(string className, string text)
        {
            this.ClassName = className;
            this.Text = text;
        }
        #endregion
    }
}
