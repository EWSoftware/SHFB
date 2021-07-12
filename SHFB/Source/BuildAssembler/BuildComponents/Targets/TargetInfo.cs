//===============================================================================================================
// System  : Sandcastle Build Components
// File    : TargetInfo.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the TargetInfo class used to represent a resolved target containing all the information
// necessary to actually write out the link by using the ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 12/26/2012 - EFW - Moved the class into the Targets namespace
//===============================================================================================================

using System;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This class is used to represent a resolved target containing all the information necessary to actually
    /// write out the link by using the <see cref="ResolveConceptualLinksComponent"/>.
    /// </summary>
    public class TargetInfo
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// This read-only property is used to get the link type
        /// </summary>
        public ConceptualLinkType LinkType { get; }

        /// <summary>
        /// This read-only property is used to get the URL
        /// </summary>
        public string Url { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetUrl">The target URL</param>
        /// <param name="targetText">The target's link text</param>
        /// <param name="typeOfLink">The type of link</param>
        /// <exception cref="ArgumentNullException">This is thrown if the target URL or text is null</exception>
        public TargetInfo(string targetUrl, string targetText, ConceptualLinkType typeOfLink)
        {
            if(targetText == null)
                throw new ArgumentNullException(nameof(targetText));

            this.Url = targetUrl ?? throw new ArgumentNullException(nameof(targetUrl));
            this.Text = targetText.Trim();
            this.LinkType = typeOfLink;
        }
        #endregion
    }
}
