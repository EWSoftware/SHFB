// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/18/2013 - EFW - Factored the common copy command code out into an abstract base class to allow for the
// creation of new copy commands.

using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.Commands
{
    /// <summary>
    /// This is an abstract base class used for copy commands
    /// </summary>
    public abstract class CopyCommand
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the parent component
        /// </summary>
        /// <value>This can be used to log messages</value>
        public BuildComponentCore ParentComponent { get; private set; }

        /// <summary>
        /// This read-only property returns the XPath expression used to get the source elements
        /// </summary>
        public XPathExpression Source { get; private set; }

        /// <summary>
        /// This read-only property returns the XPath expression used to get the target element to which the
        /// source elements are copied
        /// </summary>
        public XPathExpression Target { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        /// <param name="sourceXPath">The source XPath expression</param>
        /// <param name="targetXPath">The target XPath expression</param>
        protected CopyCommand(BuildComponentCore parent, string sourceXPath, string targetXPath)
        {
            this.ParentComponent = parent;
            this.Source = XPathExpression.Compile(sourceXPath);
            this.Target = XPathExpression.Compile(targetXPath);
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Implement this method to apply the copy command to the given target document using the given context
        /// </summary>
        /// <param name="targetDocument">The target document</param>
        /// <param name="context">The context to use</param>
        public abstract void Apply(XmlDocument targetDocument, IXmlNamespaceResolver context);

        #endregion
    }
}
