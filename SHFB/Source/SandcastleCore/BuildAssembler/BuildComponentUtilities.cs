// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/23/2012 - EFW - Updated the utility methods to act as extension methods.  Moved IsLegalXmlText() here
// as it was duplicated in the ExampleComponent and SnippetComponent and replaced it with a modified version
// from MRefBuilder.
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

// Ignore Spelling: xxx aaa bbb

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This class contains a set of utility extension methods that can be used by build components
    /// </summary>
    public static class BuildComponentUtilities
    {
        #region Private data members
        //=====================================================================

        private static readonly Regex reAllXmlChars = new("[\x09\x0A\x0D\u0020-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]");

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to get the message strings from an exception and any of its inner exceptions
        /// </summary>
        /// <param name="exception">The exception from which to get the message</param>
        /// <returns>The exception message along with any inner exception messages</returns>
        /// <remarks><see cref="XmlException"/> and <see cref="XsltException"/> messages will be returned with
        /// line number, line position, and source URI information.</remarks>
        /// <exception cref="ArgumentNullException">This is thrown if the <paramref name="exception"/> argument
        /// is null</exception>
        public static string GetExceptionMessage(this Exception exception)
        {
            if(exception == null)
                throw new ArgumentNullException(nameof(exception));

            if(exception is AggregateException)
                exception = exception.InnerException;

            string message = exception.Message;

            if(exception is XmlException xmlE)
            {
                message = String.Format(CultureInfo.CurrentCulture, "{0} (Line Number: {1}; Line Position: " +
                    "{2}; Source URI: '{3}')", message, xmlE.LineNumber, xmlE.LinePosition, xmlE.SourceUri);
            }

            if(exception is XsltException xslE)
            {
                message = String.Format(CultureInfo.CurrentCulture, "{0} (Line Number: {1}; Line Position: " +
                    "{2}; Source URI: '{3}')", message, xslE.LineNumber, xslE.LinePosition, xslE.SourceUri);
            }

            if(exception.InnerException != null)
            {
                message = String.Format(CultureInfo.CurrentCulture, "{0}\r\nInner Exception: {1}", message,
                    exception.InnerException.GetExceptionMessage());
            }

            return message;
        }

        /// <summary>
        /// This is used to get the inner XML of a node without changing the spacing
        /// </summary>
        /// <param name="node">The node from which to get the inner XML</param>
        /// <returns>The inner XML as a string with its spacing preserved</returns>
        /// <exception cref="ArgumentNullException">This is thrown if the <paramref name="node"/> parameter
        /// is null.</exception>
        public static string GetInnerXml(this XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            // Clone the node so that we don't change the input
            XPathNavigator current = node.Clone();

            XmlWriterSettings settings = new()
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                CloseOutput = true
            };

            StringBuilder builder = new();

            // Don't use a simplified using statement here.  The XML writer needs to be disposed of so that
            // it flushes the content to the string builder.  We could use one but then we have to manually
            // flush the stream before returning.
            using(XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                // write the output
                bool writing = current.MoveToFirstChild();

                while(writing)
                {
                    current.WriteSubtree(writer);
                    writing = current.MoveToNext();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Convert an XPath node iterator to an array
        /// </summary>
        /// <param name="iterator">The XPath iterator to convert to an array</param>
        /// <returns>An array with the cloned nodes from the iterator</returns>
        public static XPathNavigator[] ToArray(this XPathNodeIterator iterator)
        {
            if(iterator == null)
                throw new ArgumentNullException(nameof(iterator));

            XPathNavigator[] result = new XPathNavigator[iterator.Count];

            for(int i = 0; i < result.Length; i++)
            {
                iterator.MoveNext();

                // Clone is required or all entries will equal Current!
                result[i] = iterator.Current.Clone();
            }

            return result;
        }

        /// <summary>
        /// This is used to get the string result from evaluating an XPath expression against the given
        /// document and context.
        /// </summary>
        /// <param name="document">The document to use</param>
        /// <param name="expression">The XPath expression to evaluate</param>
        /// <param name="context">The context to use</param>
        /// <returns>The evaluated expression result</returns>
        /// <overloads>There are two overloads for this method</overloads>
        public static string EvalXPathExpr(this IXPathNavigable document, XPathExpression expression,
          CustomContext context)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            if(expression == null)
                throw new ArgumentNullException(nameof(expression));

            XPathExpression t = expression.Clone();
            t.SetContext(context);

            return document.CreateNavigator().Evaluate(t).ToString();
        }

        /// <summary>
        /// This is used to get the string result from evaluating an XPath expression against the given document
        /// and a context created from a set of key/value pairs.
        /// </summary>
        /// <param name="document">The document to use</param>
        /// <param name="expression">The XPath expression to evaluate</param>
        /// <param name="keyValuePairs">A set of key/value pairs to use when creating the context</param>
        /// <returns>The evaluated expression result</returns>
        /// <example>
        /// <code language="cs">
        /// string result = document.EvalXPathExpr("concat($key, '.htm')", "key", "filename");
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException">This is thrown if the <paramref name="keyValuePairs"/>
        /// parameter contains an odd number of parameters.</exception>
        public static string EvalXPathExpr(this IXPathNavigable document, XPathExpression expression,
          params string[] keyValuePairs)
        {
            CustomContext cc = new();

            if(keyValuePairs.Length % 2 != 0)
                throw new ArgumentException("There must be a value for every key name specified", nameof(keyValuePairs));

            for(int i = 0; i < keyValuePairs.Length; i += 2)
                cc[keyValuePairs[i]] = keyValuePairs[i + 1];

            return document.EvalXPathExpr(expression, cc);
        }

        /// <summary>
        /// This returns the path argument adjusted to be relative to the base path. Absolute path names will
        /// be returned unchanged.
        /// </summary>
        /// <param name="path">The path to adjust including the filename</param>
        /// <param name="basePath">The base path to use including the filename</param>
        /// <returns>The path argument as a path relative to the given base path</returns>
        /// <example>
        /// <code language="none" title=" ">
        /// path:     "xxx/aaa/target.html"
        /// basePath: "xxx/bbb/source.html"
        /// result:   "../aaa/target.html"
        /// </code>
        /// </example>
        /// <remarks>This assumes that the path separator is "/" and that both paths include a filename</remarks>
        public static string GetRelativePath(this string path, string basePath)
        {
            // Ignore absolute path names and an empty basePath
            if(!String.IsNullOrEmpty(path) && path[0] != '/' && !String.IsNullOrEmpty(basePath))
            {
                List<string> pathParts = [.. path.Split('/')];
                List<string> basePathParts = [.. basePath.Split('/')];

                // Remove the base path file name
                if(basePathParts.Count > 0)
                    basePathParts.RemoveAt(basePathParts.Count - 1);

                // Strip common base path bits
                while(pathParts.Count > 0 && basePathParts.Count > 0 &&
                  pathParts[0].Equals(basePathParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    pathParts.RemoveAt(0);
                    basePathParts.RemoveAt(0);
                }

                // Move up one level for each remaining base path part
                for(int i = 0; i < basePathParts.Count; i++)
                    pathParts.Insert(0, "..");

                path = String.Join("/", pathParts);
            }

            return path;
        }

        /// <summary>
        /// This is used to confirm that the specified text only contains legal XML characters
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>True if all characters are legal XML characters, false if not</returns>
        public static bool IsLegalXmlText(this string text)
        {
            if(String.IsNullOrEmpty(text))
                return true;

            return reAllXmlChars.IsMatch(text);
        }
        #endregion
    }
}
