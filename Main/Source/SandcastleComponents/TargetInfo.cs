//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : TargetInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/08/2008
// Note    : This is a slightly modified version of the Microsoft
//           TargetInfo class (Copyright 2007-2008 Microsoft Corporation).
//           My changes are indicated by my initials "EFW" in a comment on the
//           changes.
// Compiler: Microsoft Visual C#
//
// This file contains a reimplementation of the TargetInfo class used by the
// ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/07/2008  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a reimplementation of the <c>TargetInfo</c> class used by
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetUrl">The target URL</param>
        /// <param name="targetText">The target's link text</param>
        /// <param name="typeOfLink">The type of link</param>
        /// <exception cref="ArgumentNullException">This is thrown if the
        /// target URL or text is null.</exception>
        public TargetInfo(string targetUrl, string targetText,
          LinkType typeOfLink)
        {
            if(targetUrl == null)
                throw new ArgumentNullException("targetUrl");

            if(targetText == null)
                throw new ArgumentNullException("targetText");

            url = targetUrl;

            // EFW - Use String.Empty or trim off unwanted whitespace
            text = String.IsNullOrEmpty(targetText) ? String.Empty :
                targetText.Trim();
            linkType = typeOfLink;
        }
    }
}
