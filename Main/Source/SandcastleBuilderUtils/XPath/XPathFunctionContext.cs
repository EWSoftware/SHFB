//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : XPathFunctionContext.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/27/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom XPath XSLT context used to provide a regular
// expression search to XPath queries.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.1.0  07/27/2007  EFW  Created the code
//=============================================================================

using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SandcastleBuilder.Utils.XPath
{
    /// <summary>
    /// This class is used to provide a custom regular expression search
    /// function to XPath queries using an XSLT context.
    /// </summary>
    public class XPathFunctionContext : XsltContext
    {
        #region Unused members
        //=====================================================================
        // The following members are not used by this implementation

        /// <summary>
        /// This is not used by this class.
        /// </summary>
        /// <param name="baseUri">The base URI of the first document to compare</param>
        /// <param name="nextbaseUri">The base URI of the second document to compare</param>
        /// <returns>Always returns zero (equal).</returns>
        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            return 0;
        }

        /// <summary>
        /// This is not used by this class.
        /// </summary>
        /// <param name="node">The whitespace node to preserve or strip</param>
        /// <returns>For this class, this always returns true to preserve
        /// whitespace.</returns>
        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return true;
        }

        /// <summary>
        /// This is not used by this class.
        /// </summary>
        /// <param name="prefix">The prefix of the variable as it appears
        /// in the expression</param>
        /// <param name="name">The name of the variable</param>
        /// <returns>Always returns null</returns>
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return null;
        }

        /// <summary>
        /// This is not used by this class.
        /// </summary>
        /// <value>Always returns true to include whitespace in the output</value>
        public override bool Whitespace
        {
            get { return true; }
        }
        #endregion

        /// <summary>
        /// Resolves a function reference and return an
        /// <see cref="IXsltContextFunction"/> representing the function. The
        /// <see cref="IXsltContextFunction"/> is used at execution time to get
        /// the return value of the function.
        /// </summary>
        /// <param name="prefix">The prefix of the function as it appears in
        /// the XPath expression</param>
        /// <param name="name">The name of the function</param>
        /// <param name="ArgTypes">An array of argument types for the function
        /// being resolved. This allows you to select between methods with the
        /// same name (for example, overloaded methods).</param>
        /// <returns>An <see cref="IXsltContextFunction"/> representing the
        /// function</returns>
        public override IXsltContextFunction ResolveFunction(string prefix,
          string name, XPathResultType[] ArgTypes)
        {
            IXsltContextFunction function = null;

            switch(name)
            {
                case "matches-regex":
                    function = new MatchesRegexFunction();
                    break;

                case "resolve-name":
                    function = new ResolveNameFunction();
                    break;

                default:
                    break;
            }

            return function;
        }
    }
}
