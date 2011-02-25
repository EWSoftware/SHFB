//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : ConceptualLinkInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/08/2008
// Note    : This is a slightly modified version of the Microsoft
//           ConceptualLinkInfo class (Copyright 2007-2008 Microsoft
//           Corporation).  My changes are indicated by my initials "EFW" in a
//           comment on the changes.
// Compiler: Microsoft Visual C#
//
// This file contains a reimplementation of the ConceptualLinkInfo class used
// by the ResolveConceptualLinksComponent.
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
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a reimplementation of the <c>ConceptualLinkInfo</c> class used
    /// by <see cref="ResolveConceptualLinksComponent"/>.
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
