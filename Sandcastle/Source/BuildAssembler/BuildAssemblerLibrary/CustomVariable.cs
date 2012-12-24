// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This represents a custom XSLT variable used by a BuildAssembler <see cref="CustomContext"/>
    /// </summary>
    internal struct CustomVariable : IXsltContextVariable
    {
        #region Private data members
        //=====================================================================

        private string value;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The variable's value</param>
        public CustomVariable(string value)
        {
            this.value = value;
        }
        #endregion

        #region  IXsltContextVariable implementation
        //=====================================================================

        /// <inheritdoc />
        /// <value>This implementation always returns false</value>
        public bool IsLocal
        {
            get { return false; }
        }

        /// <inheritdoc />
        /// <value>This implementation always returns false</value>
        public bool IsParam
        {
            get { return false; }
        }

        /// <inheritdoc />
        /// <value>This implementation always returns <see cref="XPathResultType.String"/></value>
        public XPathResultType VariableType
        {
            get { return XPathResultType.String; }
        }

        /// <inheritdoc />
        /// <remarks>This always returns the value of the variable assigned in the constructor</remarks>
        public object Evaluate(XsltContext context)
        {
            return value;
        }
        #endregion
    }
}
