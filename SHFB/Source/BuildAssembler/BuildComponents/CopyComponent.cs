// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/19/2013 - EFW - Moved the class out into its own file.  Added a parent parameter to the constructor to
// allow passing the parent component reference to the copy component so that it can log messages.

using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This abstract class is used to create copy components used in other build components such as
    /// <see cref="CopyFromIndexComponent"/>
    /// </summary>
    public abstract class CopyComponent
    {
        /// <summary>
        /// This read-only property returns a reference to the parent build component
        /// </summary>
        protected BuildComponentCore ParentBuildComponent { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        protected CopyComponent(BuildComponentCore parent)
        {
            this.ParentBuildComponent = parent;
        }

        /// <summary>
        /// This abstract method must be overridden to apply the copy component's changes to the specified
        /// document.
        /// </summary>
        /// <param name="document">The document that the build component can modify</param>
        /// <param name="key">The key that uniquely identifies the document</param>
        public abstract void Apply(XmlDocument document, string key);
    }
}
