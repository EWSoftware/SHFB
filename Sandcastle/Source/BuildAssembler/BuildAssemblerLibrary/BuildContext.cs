// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// 12/23/2012 - EFW - Cleaned up the code and split the CustomContext and CustomVariable classes out into their
// own source code files.

using System;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This represents the build context used by BuildAssembler
    /// </summary>
    public class BuildContext
    {
        #region Private data members
        //=====================================================================

        private CustomContext context = new CustomContext();
        #endregion

        #region Namespace control
        //=====================================================================

        /// <summary>
        /// This adds an XML namespace to the context
        /// </summary>
        /// <param name="prefix">The namespace prefix</param>
        /// <param name="uri">The namespace URI</param>
        public void AddNamespace(string prefix, string uri)
        {
            context.AddNamespace(prefix, uri);
        }

        /// <summary>
        /// This looks up a namespace by prefix
        /// </summary>
        /// <param name="prefix">The namespace prefix to look up</param>
        /// <returns>The URI if found or null if not found</returns>
        public string LookupNamespace(string prefix)
        {
            return context.LookupNamespace(prefix);
        }

        /// <summary>
        /// This removes a namespace from the context
        /// </summary>
        /// <param name="prefix">The namespace prefix to remove</param>
        /// <returns>True if found and removed or false if not found</returns>
        public bool RemoveNamespace(string prefix)
        {
            string uri = LookupNamespace(prefix);

            if(uri == null)
                return false;

            context.RemoveNamespace(prefix, uri);
            return true;
        }
        #endregion

        #region Variable control
        //=====================================================================

        /// <summary>
        /// Add a variable to the context with the specified value
        /// </summary>
        /// <param name="name">The variable to add</param>
        /// <param name="value">The value to assign the variable</param>
        public void AddVariable(string name, string value)
        {
            context[name] = value;
        }

        /// <summary>
        /// Look up the named variable
        /// </summary>
        /// <param name="name">The variable to look up</param>
        /// <returns>The variable value if found or null if not found</returns>
        public string LookupVariable(string name)
        {
            return context[name];
        }

        /// <summary>
        /// Remove the named variable from the context
        /// </summary>
        /// <param name="name">The variable to remove</param>
        /// <returns>True if found and removed or false if not found</returns>
        public bool RemoveVariable(string name)
        {
            return context.ClearVariable(name);
        }

        /// <summary>
        /// Remove all variables from the context
        /// </summary>
        public void ClearVariables()
        {
            context.ClearVariables();
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="name">The variable for which to get or set the value</param>
        /// <returns>The variable value if found or null if the named variable was not found</returns>
        public string this[string name]
        {
            get { return context[name]; }
            set { context[name] = value; }
        }
        #endregion

        #region Function control
        //=====================================================================

        /// <summary>
        /// This read-only property returns the context used for XPath queries
        /// </summary>
        public XsltContext XsltContext
        {
            get { return context; }
        }
        #endregion

        #region Configuration
        //=====================================================================

        /// <summary>
        /// This is used to load the context settings from the given configuration information
        /// </summary>
        /// <param name="configuration">The configuration information from which to load the context
        /// settings.</param>
        /// <remarks>There should be one ore more <c>namespace</c> elements with a <c>prefix</c> and a
        /// <c>uri</c> attribute that identify the namespace to add to the context.</remarks>
        public void Load(XPathNavigator configuration)
        {
            XPathNodeIterator namespaceNodes = configuration.Select("namespace");

            foreach(XPathNavigator namespaceNode in namespaceNodes)
            {
                string prefixValue = namespaceNode.GetAttribute("prefix", String.Empty);
                string uriValue = namespaceNode.GetAttribute("uri", String.Empty);

                AddNamespace(prefixValue, uriValue);
            }
        }
        #endregion
    }
}
