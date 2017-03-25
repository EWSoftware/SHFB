//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ReferenceLinkInfo.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the ReferenceLinkInfo class used to hold reference link information used by the
// ResolveReferenceLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/28/2012 - EFW - Moved the class into the Targets namespace.  Removed the static Create() method and moved
// the code it contained into the constructor and made it public.
// 03/17/2013 - EFW - Added support for the syntax writer renderAsLink attribute
//===============================================================================================================

using System;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.BuildComponent;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This class is used to hold reference link information used by the
    /// <see cref="ResolveReferenceLinksComponent"/>.
    /// </summary>
    public class ReferenceLinkInfo
    {
        #region Properties
        //=====================================================================

        /// <summary>This read-only property returns the target of the link</summary>
        public string Target { get; private set; }

        /// <summary>This read-only property returns the display target of the link</summary>
        public string DisplayTarget { get; internal set; }

        /// <summary>This read-only property returns the display options for the link</summary>
        public DisplayOptions DisplayOptions { get; private set; }

        /// <summary>This read-only property indicates whether or not to prefer the overload topic</summary>
        public bool PreferOverload { get; private set; }

        /// <summary>
        /// This read-only property indicates whether or not to render the element as an actual link
        /// </summary>
        /// <value>If true, it is rendered as a link.  If false, it will be rendered as an identifier.</value>
        public bool RenderAsLink { get; private set; }

        /// <summary>This read-only property returns the contents of the link</summary>
        public XPathNavigator Contents { get; internal set; }
        #endregion

        #region Methods, etc.
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">The XPath navigator from which to load the link settings</param>
        /// <exception cref="ArgumentNullException">This is thrown if the element parameters is null</exception>
        /// <exception cref="InvalidOperationException">This is thrown if the element contains invalid
        /// configuration information.</exception>
        public ReferenceLinkInfo(XPathNavigator element)
        {
            bool attrValue;

            if(element == null)
                throw new ArgumentNullException("element");

            this.DisplayOptions = DisplayOptions.Default;
            this.Target = element.GetAttribute("target", String.Empty);
            this.DisplayTarget = element.GetAttribute("display-target", String.Empty);

            string showContainer = element.GetAttribute("show-container", String.Empty);

            if(String.IsNullOrEmpty(showContainer))
                showContainer = element.GetAttribute("qualified", String.Empty);

            if(!String.IsNullOrEmpty(showContainer))
            {
                if(!Boolean.TryParse(showContainer, out attrValue))
                    throw new InvalidOperationException("The show-container or qualified attribute does not " +
                        "contain a valid Boolean value");

                if(attrValue)
                    this.DisplayOptions |= DisplayOptions.ShowContainer;
                else
                    this.DisplayOptions &= ~DisplayOptions.ShowContainer;
            }

            string showTemplates = element.GetAttribute("show-templates", String.Empty);

            if(!String.IsNullOrEmpty(showTemplates))
            {
                if(!Boolean.TryParse(showTemplates, out attrValue))
                    throw new InvalidOperationException("The show-templates attribute does not contain a " +
                        "valid Boolean value");

                if(attrValue)
                    this.DisplayOptions |= DisplayOptions.ShowTemplates;
                else
                    this.DisplayOptions &= ~DisplayOptions.ShowTemplates;
            }

            string showParameters = element.GetAttribute("show-parameters", String.Empty);

            if(!String.IsNullOrEmpty(showParameters))
            {
                if(!Boolean.TryParse(showParameters, out attrValue))
                    throw new InvalidOperationException("The show-parameters attribute does not contain a " +
                        "valid Boolean value");

                if(attrValue)
                    this.DisplayOptions |= DisplayOptions.ShowParameters;
                else
                    this.DisplayOptions &= ~DisplayOptions.ShowParameters;
            }

            string preferOverload = element.GetAttribute("prefer-overload", String.Empty);

            if(String.IsNullOrEmpty(preferOverload))
                preferOverload = element.GetAttribute("auto-upgrade", String.Empty);

            if(!String.IsNullOrEmpty(preferOverload))
            {
                if(!Boolean.TryParse(preferOverload, out attrValue))
                    throw new InvalidOperationException("The prefer-overload or auto-upgrade attribute does " +
                        "not contain a valid Boolean value");

                if(attrValue)
                    this.PreferOverload = true;
                else
                    this.PreferOverload = false;
            }

            string renderAsLink = element.GetAttribute("renderAsLink", String.Empty);

            if(String.IsNullOrWhiteSpace(renderAsLink) || !Boolean.TryParse(renderAsLink, out attrValue))
                this.RenderAsLink = true;
            else
                this.RenderAsLink = attrValue;

            this.Contents = element.Clone();

            if(!this.Contents.MoveToFirstChild())
                this.Contents = null;
        }
        #endregion
    }
}
