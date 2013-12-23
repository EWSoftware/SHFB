// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/19/2013 - EFW - Moved the class into the Commands namespace, made it public, renamed it from CopyCommand to
// CopyFromIndexCommand and derived it from the new CopyCommand base class.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.Commands
{
    /// <summary>
    /// This represents the copy command for the <see cref="CopyFromIndexComponent"/>
    /// </summary>
    public class CopyFromIndexCommand : CopyCommand
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the source index
        /// </summary>
        public IndexedCache SourceIndex { get; private set; }

        /// <summary>
        /// This read-only property returns the key XPath expression
        /// </summary>
        public XPathExpression Key { get; private set; }

        /// <summary>
        /// This read-only property determines if the source nodes are added to the target as attributes or
        /// as child elements.
        /// </summary>
        public bool IsAttribute { get; private set; }

        /// <summary>
        /// This read-only property determines whether to search for index keys case-insensitively
        /// </summary>
        public bool IgnoreCase { get; private set; }

        /// <summary>
        /// This is used to get or set the message level for missing index entries
        /// </summary>
        /// <value>The default is <c>Ignore</c> to ignore missing index entries without logging a message</value>
        public MessageLevel MissingEntry { get; set; }

        /// <summary>
        /// This is used to get or set the message level for missing source entries
        /// </summary>
        /// <value>The default is <c>Ignore</c> to ignore missing source entries without logging a message</value>
        public MessageLevel MissingSource { get; set; }

        /// <summary>
        /// This is used to get or set the message level for missing target entries
        /// </summary>
        /// <value>The default is <c>Ignore</c> to ignore missing target entries without logging a message</value>
        public MessageLevel MissingTarget { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        /// <param name="sourceIndex">The source index</param>
        /// <param name="keyXPath">The key XPath expression</param>
        /// <param name="sourceXPath">The source XPath expression</param>
        /// <param name="targetXPath">The target XPath expression</param>
        /// <param name="isAttribute">True if the targets are to be added as attributes, false if they are to be
        /// added as elements</param>
        /// <param name="ignoreCase">True to ignore case on the keys when retrieving index values</param>
        public CopyFromIndexCommand(BuildComponentCore parent, IndexedCache sourceIndex, string keyXPath,
          string sourceXPath, string targetXPath, bool isAttribute, bool ignoreCase) :
            base(parent, sourceXPath, targetXPath)
        {
            this.SourceIndex = sourceIndex;
            this.Key = XPathExpression.Compile(keyXPath);
            this.IsAttribute = isAttribute;
            this.IgnoreCase = ignoreCase;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument targetDocument, IXmlNamespaceResolver context)
        {
            // Get the index entry content
            XPathExpression keyExpr = this.Key.Clone();
            keyExpr.SetContext(context);

            string keyValue = (string)targetDocument.CreateNavigator().Evaluate(keyExpr);

            XPathNavigator data = this.SourceIndex[keyValue];

            if(data == null && this.IgnoreCase)
                data = this.SourceIndex[keyValue.ToLowerInvariant()];

            // Notify if no entry
            if(data == null)
            {
                base.ParentComponent.WriteMessage(this.MissingEntry, "No index entry found for key '{0}'.",
                    keyValue);
                return;
            }

            // Get the target node
            XPathExpression targetExpr = this.Target.Clone();
            targetExpr.SetContext(context);
            XPathNavigator target = targetDocument.CreateNavigator().SelectSingleNode(targetExpr);

            // notify if no target found
            if(target == null)
            {
                base.ParentComponent.WriteMessage(this.MissingTarget, "Target node '{0}' not found.",
                    targetExpr.Expression);
                return;
            }

            // get the source nodes
            XPathExpression sourceExpr = this.Source.Clone();
            sourceExpr.SetContext(context);
            XPathNodeIterator sources = data.CreateNavigator().Select(sourceExpr);

            // Append the source nodes to the target node
            foreach(XPathNavigator source in sources)
            {
                // If IsAttribute is true, add the source attributes to current target.  Otherwise append source
                // as a child element to target.
                if(this.IsAttribute && source.HasAttributes)
                {
                    string sourceName = source.LocalName;
                    XmlWriter attributes = target.CreateAttributes();

                    source.MoveToFirstAttribute();

                    do
                    {
                        string attrValue = target.GetAttribute(String.Format(CultureInfo.InvariantCulture,
                            "{0}_{1}", sourceName, source.Name), String.Empty);

                        if(string.IsNullOrEmpty(attrValue))
                            attributes.WriteAttributeString(String.Format(CultureInfo.InvariantCulture,
                                "{0}_{1}", sourceName, source.Name), source.Value);

                    } while(source.MoveToNextAttribute());

                    attributes.Close();
                }
                else
                    target.AppendChild(source);
            }

            // Notify if no source found
            if(sources.Count == 0)
                base.ParentComponent.WriteMessage(this.MissingSource, "Source node '{0}' not found.",
                    sourceExpr.Expression);
        }
        #endregion
    }
}
