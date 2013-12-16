// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This represents a custom XSLT context used by BuildAssembler
    /// </summary>
    public class CustomContext : XsltContext
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, IXsltContextVariable> variables = new Dictionary<string, IXsltContextVariable>();

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomContext() : base()
        {
        }
        #endregion

        #region Variable control
        //=====================================================================

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="variable">The variable value to get or set</param>
        /// <returns>The variable value as a string</returns>
        public string this[string variable]
        {
            get
            {
                return variables[variable].Evaluate(this).ToString();
            }
            set
            {
                variables[variable] = new CustomVariable(value);
            }
        }

        /// <summary>
        /// Clear the named variable
        /// </summary>
        /// <param name="name">The variable to clear</param>
        /// <returns>True if successful, false if not found</returns>
        public bool ClearVariable(string name)
        {
            return variables.Remove(name);
        }

        /// <summary>
        /// Clear all variables
        /// </summary>
        public void ClearVariables()
        {
            variables.Clear();
        }
        #endregion

        #region XsltContext implementation
        //=====================================================================

        /// <inheritdoc />
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return variables[name];
        }

        /// <summary>
        /// This method is not implemented
        /// </summary>
        /// <param name="prefix">Not used</param>
        /// <param name="name">Not used</param>
        /// <param name="argumentTypes">Not used</param>
        /// <returns>Not implemented</returns>
        /// <exception cref="NotImplementedException">This method is not implemented</exception>
        public override IXsltContextFunction ResolveFunction(string prefix, string name,
          XPathResultType[] argumentTypes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <remarks>This implementation always returns zero</remarks>
        public override int CompareDocument(string baseUri, string nextBaseUri)
        {
            return 0;
        }

        /// <inheritdoc />
        /// <value>This implementation always returns true</value>
        public override bool Whitespace
        {
            get { return true; }
        }

        /// <inheritdoc />
        /// <remarks>This implementation always returns true</remarks>
        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return true;
        }
        #endregion
    }
}
