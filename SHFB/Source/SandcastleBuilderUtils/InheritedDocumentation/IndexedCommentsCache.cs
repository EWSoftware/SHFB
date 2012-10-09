//=============================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : IndexedCommentsCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to cache indexed XML comments files
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.InheritedDocumentation
{
    /// <summary>
    /// This is used to cache indexed XML comments files
    /// </summary>
    public class IndexedCommentsCache
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, string> index;
        private Dictionary<string, IndexedCommentsFile> cache;
        private List<string> lruList;
        private int cacheSize, filesIndexed;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the number of items indexed
        /// </summary>
        public int IndexCount
        {
            get { return index.Count; }
        }

        /// <summary>
        /// This read-only property returns the number of comments files
        /// that were indexed.
        /// </summary>
        public int FilesIndexed
        {
            get
            {
                return filesIndexed;
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This is used by the cache to report duplicate key warnings
        /// </summary>
        public event EventHandler<CommentsCacheEventArgs> ReportWarning;

        /// <summary>
        /// This is used to raise the <see cref="ReportWarning" /> event
        /// </summary>
        /// <param name="args">The event arguments</param>
        protected virtual void OnReportWarning(CommentsCacheEventArgs args)
        {
            var handler = ReportWarning;

            if(handler != null)
                handler(this, args);
        }
        #endregion

        #region Methods, etc
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">The maximum size of the cache</param>
        public IndexedCommentsCache(int size)
        {
            if(size < 0)
                throw new ArgumentOutOfRangeException("size");

            cacheSize = size;
            index = new Dictionary<string, string>();
            cache = new Dictionary<string, IndexedCommentsFile>(cacheSize);
            lruList = new List<string>(cacheSize);
        }

        /// <summary>
        /// Index all comments files found in the specified folder.
        /// </summary>
        /// <param name="path">The path to search.  If null or empty, the
        /// current directory is assumed.</param>
        /// <param name="wildcard">The wildcard to use.  If null or empty,
        /// "*.xml" is assumed.</param>
        /// <param name="recurse">True to recurse subfolders or false to only
        /// use the given folder.</param>
        /// <param name="commentsFiles">Optional.  If not null, an
        /// <see cref="XPathDocument"/> is added to the collection for each
        /// file indexed.</param>
        public void IndexCommentsFiles(string path, string wildcard,
          bool recurse, Collection<XPathNavigator> commentsFiles)
        {
            XPathDocument xpathDoc;
            string[] keys;

            if(String.IsNullOrEmpty(path))
                path = Environment.CurrentDirectory;
            else
                path = Path.GetFullPath(path);

            if(String.IsNullOrEmpty(wildcard))
                wildcard = "*.xml";

            // Index the file
            foreach(string filename in Directory.EnumerateFiles(path, wildcard))
            {
                if(!filename.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    this.OnReportWarning(new CommentsCacheEventArgs(
                        "SHFB: Warning GID0007: Ignoring non-XML comments file: " + filename));
                    continue;
                }

                keys = new IndexedCommentsFile(filename).GetKeys();

                if(commentsFiles != null)
                {
                    xpathDoc = new XPathDocument(filename);
                    commentsFiles.Add(xpathDoc.CreateNavigator());
                }

                // Check for duplicates.  If found, the last one in wins.
                foreach(string key in keys)
                {
                    if(index.ContainsKey(key))
                        this.OnReportWarning(new CommentsCacheEventArgs(String.Format(
                            CultureInfo.InvariantCulture, "SHFB: Warning GID0008: Entries for the key " +
                            "'{0}' occur in both '{1}' and '{2}'.  The entries in '{2}' will be used.", key,
                            index[key], filename)));

                    index[key] = filename;
                }

                filesIndexed++;
            }

            if(recurse)
                foreach(string folder in Directory.EnumerateDirectories(path))
                    this.IndexCommentsFiles(folder, wildcard, recurse, commentsFiles);
        }

        /// <summary>
        /// Get the comments for the specified key
        /// </summary>
        /// <param name="key">The key for which to retrieve comments</param>
        /// <returns>An <see cref="XPathNavigator"/> for the comments or null
        /// if not found.</returns>
        public XPathNavigator GetComments(string key)
        {
            IndexedCommentsFile document = this.GetCommentsFile(key);

            if(document == null)
                return null;

            return document.GetContent(key);
        }

        /// <summary>
        /// Get the comments file from the index cache that contains the given
        /// key.
        /// </summary>
        /// <param name="key">The key for which to retrieve the file</param>
        /// <returns>The indexed comments file or null if not found</returns>
        public IndexedCommentsFile GetCommentsFile(string key)
        {
            IndexedCommentsFile document;
            string filename;

            if(index.TryGetValue(key, out filename))
            {
                if(!cache.TryGetValue(filename, out document))
                {
                    document = new IndexedCommentsFile(filename);

                    if(cache.Count >= cacheSize)
                    {
                        cache.Remove(lruList[0]);
                        lruList.RemoveAt(0);
                    }

                    cache.Add(filename, document);
                    lruList.Add(filename);
                }
                else
                {
                    // Since it got used, move it to the end of the list
                    // so that it stays around longer.  This is a really
                    // basic Least Recently Used list.
                    lruList.Remove(filename);
                    lruList.Add(filename);
                }

                return document;
            }

            return null;
        }

        /// <summary>
        /// Return all keys in this index
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
