﻿// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/24/2012 - EFW - Move this class into its own file in the Targets namespace and made it public.
// 03/28/2018 - EFW - Added support for specifying a value to use when the item is undefined

using System.Xml;
using System.Xml.XPath;

namespace Sandcastle.Tools.BuildComponents.Targets
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
        public XPathExpression Path { get; }

        /// <summary>
        /// This read-only property returns the item XPath expression used to get the name of the content item
        /// that will replace the element.
        /// </summary>
        public XPathExpression Item { get; }

        /// <summary>
        /// This read-only property returns the item XPath expression used to get the value used to replace the
        /// item if the named item is not found.
        /// </summary>
        public XPathExpression Undefined { get; }

        /// <summary>
        /// This read-only property returns the XPth expression used to select parameter elements
        /// </summary>
        public XPathExpression Parameters { get; }

        /// <summary>
        /// This read-only property returns the XPath expression used to get an attribute name if the content
        /// value is to be added as an attribute.
        /// </summary>
        public XPathExpression Attribute { get; }

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
            this.Undefined = XPathExpression.Compile("string(@undefined)", context);
        }
        #endregion
    }
}
