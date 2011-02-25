//=============================================================================
// System  : Code Colorizer Library
// File    : CodeColorizer.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 11/21/2006
// Compiler: Microsoft Visual C#
//
// This is used to cache regular expressions to improve performance.  The
// original Code Project article by Jonathan can be found at:
// http://www.codeproject.com/csharp/highlightcs.asp.
//
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006:
//
//      Removed the RegexOptions.Compiled option as it doesn't appear to
//      improve performance all that much in average use.
//
//      Modified to use a generic Dictionary<string, Regex> for the collection.
//
//      Made the class internal and sealed as it serves no purpose outside of
//      the colorizer.
//
//=============================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace ColorizerLibrary
{
	/// <summary>
	///  Dictionary associating string to Regex
	/// </summary>
	/// <remarks>This implementation uses a
    /// <see cref="Dictionary{TValue, KValue}" /> to store the Regex
    /// objects.</remarks>
	internal sealed class RegexDictionary
	{
        private Dictionary<string, Regex> dictionary;

		/// <summary>
		/// Default constructor.
		/// </summary>
		internal RegexDictionary()
		{
            dictionary = new Dictionary<string, Regex>();
		}

		#region Private Methods
        /// <summary>
        /// This returns the key name based on the IDs of the two specified
        /// nodes.
        /// </summary>
        /// <param name="node1">The first node</param>
        /// <param name="node2">The second node</param>
        /// <overloads>There are two overloads for this method</overloads>
        private static string KeyName(XmlNode node1, XmlNode node2)
        {
            XmlNode attr1, attr2;

            if(node1 == null)
                throw new ArgumentNullException("node1");

            if(node2 == null)
                throw new ArgumentNullException("node2");

            attr1 = node1.Attributes["id"];
            attr2 = node2.Attributes["id"];

            if(attr1 == null)
                throw new ArgumentException("node1 has no 'id' attribute");

            if(attr2 == null)
                throw new ArgumentException("node2 has no 'id' attribute");

            return attr1.Value + "." + attr2.Value;
        }

        /// <summary>
        /// This returns the key name based on the IDs of the three specified
        /// nodes.
        /// </summary>
        /// <param name="node1">The first node</param>
        /// <param name="node2">The second node</param>
        /// <param name="node3">The third node</param>
        private static string KeyName(XmlNode node1, XmlNode node2,
          XmlNode node3)
        {
            XmlNode attr1, attr2, attr3;

            if(node1 == null)
                throw new ArgumentNullException("node1");

            if(node2 == null)
                throw new ArgumentNullException("node2");

            if(node3 == null)
                throw new ArgumentNullException("node3");

            attr1 = node1.Attributes["id"];
            attr2 = node2.Attributes["id"];
            attr3 = node3.Attributes["id"];

            if(attr1 == null)
                throw new ArgumentException("node1 has no 'id' attribute");

            if(attr2 == null)
                throw new ArgumentException("node2 has no 'id' attribute");

            if(attr3 == null)
                throw new ArgumentException("node3 has no 'id' attribute");

            return attr1.Value + "." + attr2.Value + "." + attr3.Value;
        }

		/// <summary>
		/// Retrieve the regular expression options from the language node
		/// </summary>
		/// <param name="languageNode">langue name</param>
		/// <returns>RegexOptions enumeration combination</returns>
        private static RegexOptions GetRegexOptions(XmlNode languageNode)
		{
			RegexOptions regOp = RegexOptions.Multiline;

			// Check if case sensitive...
			XmlNode caseNode = languageNode.Attributes["not-case-sensitive"];

			if(caseNode != null && caseNode.Value == "yes")
				regOp |= RegexOptions.IgnoreCase;

			return regOp;
		}
		#endregion

		#region Key add and retrieve methods
		/// <summary>
		/// Add a regex depending on two nodes
		/// </summary>
        /// <param name="languageNode">The language node</param>
        /// <param name="subNode">The sub-node</param>
        /// <param name="sRegExp">The regular expression string</param>
		/// <exception cref="ArgumentNullException">This is thrown if a node
        /// parameter is null or the regular expression is null.</exception>
		/// <exception cref="ArgumentException">This is thrown if a node
        /// parameter does not have an 'id' attribute or if the regular
        /// expression could not be created.</exception>
        /// <overloads>There are two overloads for this method</overloads>
		internal void AddKey(XmlNode languageNode, XmlNode subNode,
          string sRegExp)
		{
			Regex regExp = new Regex(sRegExp,
                RegexDictionary.GetRegexOptions(languageNode));

            if(regExp == null)
				throw new ArgumentException(
                    "Could not create regular expression");

			dictionary.Add(RegexDictionary.KeyName(languageNode, subNode),
                regExp);
		}

		/// <summary>
		/// Add a regex depending on three nodes
		/// </summary>
		/// <param name="languageNode">The language node</param>
		/// <param name="subNode">The first sub-node</param>
		/// <param name="subNode2">The second sub-node</param>
		/// <param name="sRegExp">The regular expression string</param>
		/// <exception cref="ArgumentNullException">This is thrown if a node
        /// parameter is null or the regular expression is null.</exception>
		/// <exception cref="ArgumentException">This is thrown if a node
        /// parameter does not have an 'id' attribute or if the regular
        /// expression could not be created.</exception>
		internal void AddKey(XmlNode languageNode, XmlNode subNode,
          XmlNode subNode2, string sRegExp)
		{
			Regex regExp = new Regex(sRegExp,
                RegexDictionary.GetRegexOptions(languageNode));

			if(regExp == null)
				throw new ArgumentException(
                    "Could not create regular expression");

			dictionary.Add(RegexDictionary.KeyName(languageNode, subNode,
                subNode2), regExp);
		}

		/// <summary>
		/// Retrieves the regular expression out of 2 nodes
		/// </summary>
        /// <param name="languageNode">The language node</param>
        /// <param name="subNode">The sub-node</param>
        /// <returns>The regular expression</returns>
		/// <exception cref="ArgumentNullException">This is thrown if a node
        /// parameter is null or the regular expression is null.</exception>
		/// <exception cref="ArgumentException">This is thrown if a node
        /// parameter does not have an 'id' attribute.</exception>
		internal Regex GetKey(XmlNode languageNode, XmlNode subNode)
		{
			return dictionary[RegexDictionary.KeyName(languageNode, subNode)];
		}

		/// <summary>
		/// Retrieves the regular expression out of 3 nodes
		/// </summary>
		/// <param name="languageNode">The language node</param>
		/// <param name="subNode">The first sub-node</param>
		/// <param name="subNode2">The second sub-node</param>
		/// <returns>The regular expression</returns>
		/// <exception cref="ArgumentNullException">This is thrown if a node
        /// parameter is null or the regular expression is null.</exception>
		/// <exception cref="ArgumentException">This is thrown if a node
        /// parameter does not have an 'id' attribute.</exception>
		internal Regex GetKey(XmlNode languageNode, XmlNode subNode,
          XmlNode subNode2)
		{
			return dictionary[RegexDictionary.KeyName(languageNode, subNode,
                subNode2)];
		}
		#endregion
	}
}
