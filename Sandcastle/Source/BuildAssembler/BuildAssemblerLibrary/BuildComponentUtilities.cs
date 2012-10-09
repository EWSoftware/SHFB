// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools {

    public static class BuildComponentUtilities {

        // get the message strings from an exception

        public static string GetExceptionMessage (Exception e) {
            if (e == null) throw new ArgumentNullException("e");

            string message = e.Message;

            XmlException xmlE = e as XmlException;
            if (xmlE != null) {
                message = String.Format("{0} (LineNumber: {1}; LinePosition: {2}; SourceUri: '{3}')", message, xmlE.LineNumber, xmlE.LinePosition, xmlE.SourceUri);
            }

            XsltException xslE = e as XsltException;
            if (xslE != null) {
                message = String.Format("{0} (LineNumber: {1}; LinePosition: {2}; SourceUri: '{3}')", message, xslE.LineNumber, xslE.LinePosition, xslE.SourceUri);
            }

            if (e.InnerException != null) message = String.Format("{0} {1}", message, GetExceptionMessage(e.InnerException));

            return (message);
        }

        // get InnerXml without changing the spacing

        public static string GetInnerXml (XPathNavigator node) {

            // check for null argument, and clone so we don't change input
            if (node == null) throw new ArgumentNullException("node");
	        XPathNavigator current = node.Clone();

		    // create appropriate settings for the output writer
		    XmlWriterSettings settings = new XmlWriterSettings();
		    settings.ConformanceLevel = ConformanceLevel.Fragment;
		    settings.OmitXmlDeclaration = true;

		    // construct a writer for our output
		    StringBuilder builder = new StringBuilder();
		    XmlWriter writer = XmlWriter.Create(builder, settings);

		    // write the output
		    bool writing = current.MoveToFirstChild();
		    while (writing) {
			    current.WriteSubtree(writer);
			    writing = current.MoveToNext();				
		    }

		    // finish up and return the result
		    writer.Close();
		    return(builder.ToString());

        }

        // get an array of nodes matching an XPath expression

        public static XPathNavigator[] ConvertNodeIteratorToArray (XPathNodeIterator iterator) {
            XPathNavigator[] result = new XPathNavigator[iterator.Count];
            for (int i = 0; i < result.Length; i++) {
                iterator.MoveNext();
                result[i] = iterator.Current.Clone();
                // clone is required or all entries will equal Current!
            }
            return(result);
        }


        /// <summary>
        /// Returns the string result from evaluating an xpath expression against the given document and context.
        /// </summary>
        public static string EvalXPathExpr(IXPathNavigable doc, XPathExpression xpe, CustomContext c) {
            XPathExpression t = xpe.Clone();
            t.SetContext(c);
            return doc.CreateNavigator().Evaluate(t).ToString();
        }


        /// <summary>
        /// Returns the string result from evaluating an xpath expression against the given document and
        /// context created from key/value pairs.
        /// </summary>
        /// <example>
        /// string result = BuildComponentUtilities.EvalXPathExpr(doc, "concat($key, '.htm')", "key", "file");
        /// </example>
        public static string EvalXPathExpr(IXPathNavigable doc, XPathExpression xpe, params string[] keyValuePairs) {
            Debug.Assert(keyValuePairs.Length % 2 == 0);
            CustomContext cc = new CustomContext();
            for (int i = 0; i < keyValuePairs.Length; i += 2)
                cc[keyValuePairs[i]] = keyValuePairs[i + 1];
            return EvalXPathExpr(doc, xpe, cc);
        }

        /// <summary>
        /// Returns the path argument adjusted to be relative to the base path. Absolute path names will
        /// be returned unchanged.
        /// </summary>
        /// <example>
        /// path:     "xxx/aaa/target.html"
        /// basePath: "xxx/bbb/source.html"
        /// result:   "../aaa/target.html"
        /// </example>
        public static string GetRelativePath(string path, string basePath) {
            // ignore absolute path names and an empty basePath
            if (!string.IsNullOrEmpty(path) && path[0] != '/' && !string.IsNullOrEmpty(basePath)) {

                List<string> pathParts = new List<string>(path.Split('/'));
                List<string> basePathParts = new List<string>(basePath.Split('/'));

                // remove the base path file name
                if (basePathParts.Count > 0)
                    basePathParts.RemoveAt(basePathParts.Count - 1);

                // strip common base path bits
                while (pathParts.Count > 0 && basePathParts.Count > 0 &&
                    string.Equals(pathParts[0], basePathParts[0], StringComparison.CurrentCultureIgnoreCase)) {
                    pathParts.RemoveAt(0);
                    basePathParts.RemoveAt(0);
                }

                // move up one level for each remaining base path part
                foreach (string s in basePathParts)
                    pathParts.Insert(0, "..");

                path = string.Join("/", pathParts.ToArray());
            }

            return path;
        }
    }

}