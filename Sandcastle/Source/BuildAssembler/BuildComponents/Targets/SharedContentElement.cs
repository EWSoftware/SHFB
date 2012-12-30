// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/24/2012 - EFW - Move this class into its own file in the Targets namespace and made it public.

using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This class holds the properties of a shared content element that needs to be replaced in a topic
    /// </summary>
    public class SharedContentElement
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the XPath expression used to find elements to be replaced
        /// </summary>
        public XPathExpression Path { get; private set; }

        /// <summary>
        /// This read-only property returns the item XPath expression used to get the name of the content item
        /// that will replace the element.
        /// </summary>
        public XPathExpression Item { get; private set; }

        /// <summary>
        /// This read-only property returns the XPth expression used to select parameter elements
        /// </summary>
        public XPathExpression Parameters { get; private set; }

        /// <summary>
        /// This read-only property returns the XPath expression used to get an attribute name if the content
        /// value is to be added as an attribute.
        /// </summary>
        public XPathExpression Attribute { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The path expression</param>
        /// <param name="item">The item name expression</param>
        /// <param name="parameters">The parameters expression</param>
        /// <param name="attribute">The attribute name expression</param>
        /// <param name="context">The context to use for the XPath expressions</param>
        public SharedContentElement(string path, string item, string parameters, string attribute,
          IXmlNamespaceResolver context)
        {
            this.Path = XPathExpression.Compile(path, context);
            this.Item = XPathExpression.Compile(item, context);
            this.Parameters = XPathExpression.Compile(parameters, context);
            this.Attribute = XPathExpression.Compile(attribute, context);
        }
        #endregion
    }
}
