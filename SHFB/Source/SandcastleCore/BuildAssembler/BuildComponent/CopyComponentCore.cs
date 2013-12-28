// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/19/2013 - EFW - Moved the class out into its own file.  Added a parent parameter to the constructor to
// allow passing the parent component reference to the copy component so that it can log messages.
// 12/27/2013 - EFW - Updated the copy component to be discoverable via MEF.  Moved it to Sandcastle.Core.

using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This abstract class is used to create copy components used in other build components such as
    /// <c>CopyFromIndexComponent</c>
    /// </summary>
    public abstract class CopyComponentCore
    {
        /// <summary>
        /// This read-only property returns a reference to the parent build component
        /// </summary>
        protected BuildComponentCore ParentBuildComponent { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        protected CopyComponentCore(BuildComponentCore parent)
        {
            this.ParentBuildComponent = parent;
        }

        /// <summary>
        /// This abstract method must be overridden to initialize the copy component
        /// </summary>
        /// <param name="configuration">The copy component configuration</param>
        /// <param name="data">A dictionary object with string as key and object as value.</param>
        public abstract void Initialize(XPathNavigator configuration, IDictionary<string, object> data);

        /// <summary>
        /// This abstract method must be overridden to apply the copy component's changes to the specified
        /// document.
        /// </summary>
        /// <param name="document">The document that the build component can modify</param>
        /// <param name="key">The key that uniquely identifies the document</param>
        public abstract void Apply(XmlDocument document, string key);
    }
}
