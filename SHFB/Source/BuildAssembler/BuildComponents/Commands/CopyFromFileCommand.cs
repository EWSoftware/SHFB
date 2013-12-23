// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/18/2013 - EFW - Moved the class into the Commands namespace, made it public, and derived it from the new
// CopyCommand base class.

using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.Commands
{
    /// <summary>
    /// This represents the copy command for the <see cref="CopyFromFileComponent"/>
    /// </summary>
    public class CopyFromFileCommand : CopyCommand
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the source document from which to copy data
        /// </summary>
        public XPathDocument SourceDocument { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        /// <param name="sourceDocument">The source XPath document</param>
        /// <param name="sourceXPath">The source XPath expression</param>
        /// <param name="targetXPath">The target XPath expression</param>
        public CopyFromFileCommand(BuildComponentCore parent, XPathDocument sourceDocument, string sourceXPath,
          string targetXPath) : base(parent, sourceXPath, targetXPath)
        {
            this.SourceDocument = sourceDocument;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Apply the copy command to the specified target document using the specified context
        /// </summary>
        /// <param name="targetDocument">The target document</param>
        /// <param name="context">The context to use</param>
        public override void Apply(XmlDocument targetDocument, IXmlNamespaceResolver context)
        {
            // Extract the target node
            XPathExpression targetXPath = this.Target.Clone();
            targetXPath.SetContext(context);

            XPathNavigator target = targetDocument.CreateNavigator().SelectSingleNode(targetXPath);

            if(target != null)
            {
                // Extract the source nodes
                XPathExpression sourceXPath = this.Source.Clone();
                sourceXPath.SetContext(context);

                XPathNodeIterator sources = this.SourceDocument.CreateNavigator().Select(sourceXPath);

                // Append the source nodes to the target node
                foreach(XPathNavigator source in sources)
                    target.AppendChild(source);

                // Don't warn or generate an error if no source nodes are found, that may be the case
            }
            else
                base.ParentComponent.WriteMessage(MessageLevel.Error, "CopyFromFileCommand target node not found");
        }
        #endregion
    }
}
