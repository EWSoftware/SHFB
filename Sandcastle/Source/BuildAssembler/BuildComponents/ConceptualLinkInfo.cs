//=============================================================================
// System  : Sandcastle Build Components
// File    : ConceptualLinkInfo.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the ConceptualLinkInfo class used to hold conceptual link
// information used by the ResolveConceptualLinksComponent.
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
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class is used to hold conceptual link information used by
    /// by the <see cref="ResolveConceptualLinksComponent"/>.
    /// </summary>
    public class ConceptualLinkInfo
    {
        #region Private data members
        //=====================================================================

        // EFW - Added support for an anchor name
        private string target, anchor;
        private string text;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The target of the link
        /// </summary>
        public string Target
        {
            get { return target; }
        }

        /// <summary>
        /// The optional anchor name within the target
        /// </summary>
        public string Anchor
        {
            get { return anchor; }
        }

        /// <summary>
        /// The text to show for the link
        /// </summary>
        public string Text
        {
            get { return text; }
        }
        #endregion

        #region Methods, etc
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        private ConceptualLinkInfo()
        {
        }

        /// <summary>
        /// This is used to create a conceptual link based on the information
        /// in the supplied XPath navigator.
        /// </summary>
        /// <param name="node">The XPath navigator node from which to load the
        /// link settings.</param>
        /// <returns>A conceptual link info object</returns>
        /// <exception cref="ArgumentNullException">This is thrown if the
        /// node parameters is null.</exception>
        public static ConceptualLinkInfo Create(XPathNavigator node)
        {
            string target;
            int pos;

            if(node == null)
                throw new ArgumentNullException("node");

            ConceptualLinkInfo info = new ConceptualLinkInfo();
            target = node.GetAttribute("target", string.Empty);

            // EFW - Added support for an optional anchor name in the target
            pos = target.IndexOf('#');
            if(pos != -1)
            {
                info.anchor = target.Substring(pos);
                target = target.Substring(0, pos);
            }

            info.target = target;

            // EFW - Trim off unwanted whitespace
            info.text = node.ToString().Trim();
            return info;
        }
        #endregion
    }
}
