// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This represents a custom XSLT context used by BuildAssembler
    /// </summary>
    public class CustomContext : XsltContext
    {
        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, IXsltContextVariable> variables = [];

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public CustomContext() : base()
        {
        }

        /// <summary>
        /// This constructor takes a dictionary containing the namespaces (the key is the prefix, the value is
        /// the namespace URI).
        /// </summary>
        /// <param name="namespaces">A dictionary containing the namespaces to add to the context</param>
        public CustomContext(IDictionary<string, string> namespaces) : this()
        {
            if(namespaces != null)
            {
                foreach(var kv in namespaces)
                    this.AddNamespace(kv.Key, kv.Value);
            }
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
            get => variables[variable].Evaluate(this).ToString();
            set => variables[variable] = new CustomVariable(value);
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

        /// <summary>
        /// This method is not used
        /// </summary>
        /// <param name="baseUri">Not used</param>
        /// <param name="nextBaseUri">Not used</param>
        /// <returns>This implementation always returns zero</returns>
        public override int CompareDocument(string baseUri, string nextBaseUri)
        {
            return 0;
        }

        /// <inheritdoc />
        /// <value>This implementation always returns true</value>
        public override bool Whitespace => true;

        /// <inheritdoc />
        /// <remarks>This implementation always returns true</remarks>
        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return true;
        }
        #endregion
    }
}
