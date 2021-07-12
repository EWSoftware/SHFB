//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MatchesRegexFunction.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a custom XPath function used to perform a regular expression search in XPath queries
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/27/2007  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: utils Filt Excep

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SandcastleBuilder.WPF.XPath
{
    /// <summary>
    /// This class is a custom XPath function used to perform a regular expression search in XPath queries
    /// </summary>
    /// <remarks>The function should be passed a value to compare, the regular expression to use, and a Boolean
    /// indicating whether or not to do a case-insensitive match.</remarks>
    /// <example>
    /// Some examples of XPath queries using the function:
    /// 
    /// <code language="none">
    /// //apis/api[matches-regex(string(@id), 'utils.*proj', boolean(true))
    ///
    /// //apis/api[matches-regex(string(@id), 'Proj|Filt|Excep', boolean(false))
    /// </code>
    /// </example>
    internal sealed class MatchesRegexFunction : IXsltContextFunction
    {
        #region Private data members
        //=====================================================================

        // This is used to contain prior regular expressions so that they don't have to be created on each call
        private static readonly Dictionary<string, Regex> regexDictionary = new Dictionary<string, Regex>();

        #endregion

        #region IXsltContextFunction Members
        //=====================================================================

        /// <summary>
        /// Gets the supplied XPath types for the function's argument list.  This information can be used to
        /// discover the signature of the function which allows you to differentiate between overloaded
        /// functions.
        /// </summary>
        /// <value>Always returns an array with two <c>String</c> types and a <c>Boolean</c> type specified</value>
        public XPathResultType[] ArgTypes => new XPathResultType[] { XPathResultType.String,
            XPathResultType.String, XPathResultType.Boolean };

        /// <summary>
        /// Gets the minimum number of arguments for the function. This enables the user to differentiate between
        /// overloaded functions.
        /// </summary>
        /// <value>Always returns three</value>
        public int Minargs => 3;

        /// <summary>
        /// Gets the maximum number of arguments for the function. This enables the user to differentiate between
        /// overloaded functions.
        /// </summary>
        /// <value>Always returns three</value>
        public int Maxargs => 3;

        /// <summary>
        /// Gets the XPath type returned by the function
        /// </summary>
        /// <value>Always returns <c>Boolean</c></value>
        public XPathResultType ReturnType => XPathResultType.Boolean;

        /// <summary>
        /// This is called to invoke the <c>matches-regex</c> method
        /// </summary>
        /// <param name="xsltContext">The XSLT context for the function call</param>
        /// <param name="args">The arguments for the function call</param>
        /// <param name="docContext">The context node for the function call</param>
        /// <returns>An object representing the return value of the function (true for a match, false for no
        /// match).</returns>
        /// <exception cref="ArgumentException">This is thrown if the number of arguments for the function is not
        /// three.</exception>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            string value, expression, id;
            bool ignoreCase;

            if(args.Length != 3)
                throw new ArgumentException("There must be three parameters passed to the 'matches-regex' function",
                    nameof(args));

            value = args[0].ToString();
            expression = args[1].ToString();
            ignoreCase = Convert.ToBoolean(args[2], CultureInfo.InvariantCulture);

            // Create a unique ID for each expression.  We'll add a marker for case-insensitive ones to keep them
            // separate from any matching case-sensitive ones.
            id = expression;

            if(ignoreCase)
                id += "_\xFF\xFF";

            // Try to get the regex from the dictionary first
            if(!regexDictionary.TryGetValue(id, out Regex reMatch))
            {
                reMatch = new Regex(expression, (ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None);
                regexDictionary.Add(id, reMatch);
            }

            return reMatch.IsMatch(value);
        }
        #endregion
    }
}
