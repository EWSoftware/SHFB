//=============================================================================
// System  : Sandcastle Build Components
// File    : TargetInfo.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the TargetInfo class used to represent a resolved target
// containing all the information necessary to actually write out the link by
// using the ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
//=============================================================================

using System;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class is used to represent a resolved target containing all the
    /// information necessary to actually write out the link by using the
    /// <see cref="ResolveConceptualLinksComponent"/>.
    /// </summary>
    public class TargetInfo
    {
        #region Private data members
        //=====================================================================

        private string text;
        private LinkType linkType;
        private string url;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the text
        /// </summary>
        public string Text
        {
            get { return text; }
        }

        /// <summary>
        /// This read-only property is used to get the link type
        /// </summary>
        public LinkType LinkType
        {
            get { return linkType; }
        }

        /// <summary>
        /// This read-only property is used to get the URL
        /// </summary>
        public string Url
        {
            get { return url; }
        }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetUrl">The target URL</param>
        /// <param name="targetText">The target's link text</param>
        /// <param name="typeOfLink">The type of link</param>
        /// <exception cref="ArgumentNullException">This is thrown if the
        /// target URL or text is null.</exception>
        public TargetInfo(string targetUrl, string targetText, LinkType typeOfLink)
        {
            if(targetUrl == null)
                throw new ArgumentNullException("targetUrl");

            if(targetText == null)
                throw new ArgumentNullException("targetText");

            url = targetUrl;

            // EFW - Use String.Empty or trim off unwanted whitespace
            text = String.IsNullOrEmpty(targetText) ? String.Empty : targetText.Trim();
            linkType = typeOfLink;
        }
        #endregion
    }
}
