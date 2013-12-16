//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ConceptualLinkInfo.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the ConceptualLinkInfo class used to hold conceptual link information used by the
// ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 12/26/2012 - EFW - Moved the class into the Targets namespace.  Removed the static Create() method and moved
// the code it contained into the constructor and made it public.
//===============================================================================================================

using System;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This class is used to hold conceptual link information used by by the
    /// <see cref="ResolveConceptualLinksComponent"/>.
    /// </summary>
    public class ConceptualLinkInfo
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the target of the link
        /// </summary>
        public string Target { get; private set; }

        // EFW - Added support for an anchor name
        /// <summary>
        /// This read-only property returns the optional anchor name within the target
        /// </summary>
        public string Anchor { get; private set; }

        /// <summary>
        /// This read-only property returns the text to show for the link
        /// </summary>
        public string Text { get; private set; }

        #endregion

        #region Methods, etc
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">The XPath navigator node from which to load the link settings</param>
        /// <exception cref="ArgumentNullException">This is thrown if the node parameters is null</exception>
        public ConceptualLinkInfo(XPathNavigator node)
        {
            string target;
            int pos;

            if(node == null)
                throw new ArgumentNullException("node");

            target = node.GetAttribute("target", string.Empty);

            // EFW - Added support for an optional anchor name in the target
            pos = target.IndexOf('#');

            if(pos != -1)
            {
                this.Anchor = target.Substring(pos);
                target = target.Substring(0, pos);
            }

            this.Target = target.ToLowerInvariant();

            // EFW - Trim off unwanted whitespace
            this.Text = node.ToString().Trim();
        }
        #endregion
    }
}
