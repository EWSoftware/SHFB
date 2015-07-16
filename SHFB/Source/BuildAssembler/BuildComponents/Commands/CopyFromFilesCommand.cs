// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/18/2013 - EFW - Moved the class into the Commands namespace, made it public, and derived it from the new
// CopyCommand base class.

using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using Microsoft.Ddue.Tools.BuildComponent;

namespace Microsoft.Ddue.Tools.Commands
{
    /// <summary>
    /// This represents the copy command for the <see cref="CopyFromFilesComponent"/>
    /// </summary>
    public class CopyFromFilesCommand : CopyCommand
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the base path containing the source files
        /// </summary>
        public string BasePath { get; private set; }

        /// <summary>
        /// This read-only property returns the XPath expression used to get the file from which to copy elements
        /// </summary>
        public XPathExpression SourceFile { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        /// <param name="basePath">The base path in which to find the files</param>
        /// <param name="fileXPath">The file XPath expression used to get the file from which to copy elements</param>
        /// <param name="sourceXPath">The source XPath expression</param>
        /// <param name="targetXPath">The target XPath expression</param>
        public CopyFromFilesCommand(BuildComponentCore parent, string basePath, string fileXPath, string sourceXPath,
          string targetXPath) : base(parent, sourceXPath, targetXPath)
        {
            this.BasePath = basePath;
            this.SourceFile = XPathExpression.Compile(fileXPath);
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
            XPathExpression fileExpr = this.SourceFile.Clone();
            fileExpr.SetContext(context);

            string filename = (string)targetDocument.CreateNavigator().Evaluate(fileExpr);
            string filePath = Path.Combine(this.BasePath, filename);

            if(File.Exists(filePath))
            {
                XPathDocument sourceDocument = new XPathDocument(filePath);

                XPathExpression targetExpr = this.Target.Clone();
                targetExpr.SetContext(context);

                XPathNavigator targetNode = targetDocument.CreateNavigator().SelectSingleNode(targetExpr);

                if(targetNode != null)
                {
                    XPathExpression sourceExpr = this.Source.Clone();
                    sourceExpr.SetContext(context);

                    XPathNodeIterator sourceNodes = sourceDocument.CreateNavigator().Select(sourceExpr);

                    foreach(XPathNavigator sourceNode in sourceNodes)
                        targetNode.AppendChild(sourceNode);

                    // Don't warn or generate an error if no source nodes are found, that may be the case
                }
                else
                    base.ParentComponent.WriteMessage(MessageLevel.Error, "CopyFromFilesCommand target node " +
                        "not found");
            }
            else
                base.ParentComponent.WriteMessage(MessageLevel.Error, "CopyFromFilesCommand source file not found");
        }
        #endregion
    }
}
