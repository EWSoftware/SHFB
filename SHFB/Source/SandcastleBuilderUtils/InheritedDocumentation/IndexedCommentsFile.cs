//=============================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : IndexedCommentsFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/06/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is represents an indexed XML comments file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.InheritedDocumentation
{
    /// <summary>
    /// This represents an indexed XML comments file
    /// </summary>
    public class IndexedCommentsFile
    {
        #region Private data members
        //=====================================================================

        // These are used frequently so they are static
        private static XPathExpression memberListExpr = XPathExpression.Compile(
            "/doc/members/member");
        private static XPathExpression keyExpr = XPathExpression.Compile(
            "@name");

        // Comments filename and the index
        private string commentsFile;
        private Dictionary<string, XPathNavigator> index;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the comments filename
        /// </summary>
        public string Filename
        {
            get { return commentsFile; }
        }

        /// <summary>
        /// This read-only property returns the number of items in the index
        /// </summary>
        public int Count
        {
            get { return index.Count; }
        }
        #endregion

        #region Methods, etc.
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The name of the XML comments file to index</param>
        public IndexedCommentsFile(string filename)
        {
            XPathDocument comments;
            XPathNodeIterator members;
            XPathNavigator key;

            if(String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            index = new Dictionary<string, XPathNavigator>();
            commentsFile = filename;

            try
            {
                comments = new XPathDocument(filename, XmlSpace.Preserve);
                members = comments.CreateNavigator().Select(memberListExpr);

                foreach(XPathNavigator member in members)
                {
                    key = member.SelectSingleNode(keyExpr);

                    if(key != null)
                    {
                        index[key.Value] = member;

                        // Also add a namespace entry for NamespaceDoc classes
                        if(key.Value.EndsWith(".NamespaceDoc",
                          StringComparison.Ordinal))
                            index["N:" + key.Value.Substring(2,
                                key.Value.Length - 15)] = member;
                    }
                }
            }
            catch(IOException ioEx)
            {
                throw new InheritedDocsException("An access error occured " +
                    "while attempting to load the file '{0}'", ioEx);
            }
            catch(XmlException ex)
            {
                throw new InheritedDocsException("The indexed document '" +
                    filename + "' is not a valid XML document.", ex);
            }
        }

        /// <summary>
        /// Get the XML comments for the given key
        /// </summary>
        /// <param name="key">The key for the comments</param>
        /// <returns>An <see cref="XPathNavigator"/> for the comments or null
        /// if not found.</returns>
        public XPathNavigator GetContent(string key)
        {
            XPathNavigator navigator = index[key];

            return (navigator == null) ? null : navigator.Clone();
        }

        /// <summary>
        /// Return all keys in this file
        /// </summary>
        /// <returns>A string array containing the keys</returns>
        public string[] GetKeys()
        {
            string[] keys = new string[index.Count];

            index.Keys.CopyTo(keys, 0);
            return keys;
        }
        #endregion
    }
}
