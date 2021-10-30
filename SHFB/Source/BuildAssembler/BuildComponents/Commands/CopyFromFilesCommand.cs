// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/18/2013 - EFW - Moved the class into the Commands namespace, made it public, and derived it from the new
// CopyCommand base class.

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents.Commands
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
        public string BasePath { get; }

        /// <summary>
        /// This read-only property returns the XPath expression used to get the file from which to copy elements
        /// </summary>
        public XPathExpression SourceFile { get; }

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
            if(targetDocument == null)
                throw new ArgumentNullException(nameof(targetDocument));

            XPathExpression fileExpr = this.SourceFile.Clone();
            fileExpr.SetContext(context);

            string filename = (string)targetDocument.CreateNavigator().Evaluate(fileExpr);
            string filePath = Path.Combine(this.BasePath, filename);

            if(File.Exists(filePath))
            {
                XPathExpression targetExpr = this.Target.Clone();
                targetExpr.SetContext(context);

                XPathNavigator targetNode = targetDocument.CreateNavigator().SelectSingleNode(targetExpr);

                if(targetNode != null)
                {
                    XPathExpression sourceExpr = this.Source.Clone();
                    sourceExpr.SetContext(context);

                    using(var reader = XmlReader.Create(filePath, new XmlReaderSettings { CloseInput = true }))
                    {
                        XPathDocument sourceDocument = new XPathDocument(reader);
                        XPathNodeIterator sourceNodes = sourceDocument.CreateNavigator().Select(sourceExpr);

                        foreach(XPathNavigator sourceNode in sourceNodes)
                            targetNode.AppendChild(sourceNode);
                    }

                    // Don't warn or generate an error if no source nodes are found, that may be the case
                }
                else
                    this.ParentComponent.WriteMessage(MessageLevel.Error, "CopyFromFilesCommand target node " +
                        "not found: {0}", targetExpr.Expression);
            }
            else
                this.ParentComponent.WriteMessage(MessageLevel.Error, "CopyFromFilesCommand source file not " +
                    "found: {0}", filePath);
        }
        #endregion
    }
}
