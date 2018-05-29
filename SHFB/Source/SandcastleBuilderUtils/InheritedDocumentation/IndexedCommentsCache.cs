//===============================================================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : IndexedCommentsCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/18/2018
// Note    : Copyright 2008-2018, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to cache indexed XML comments files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/27/2008  EFW  Created the code
// 02/28/2013  EFW  Made updates based on changes in the related Sandcastle index cache classes
//===============================================================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.InheritedDocumentation
{
    /// <summary>
    /// This is used to cache indexed XML comments files
    /// </summary>
    public class IndexedCommentsCache
    {
        #region IndexedCommentsFile
        //=====================================================================

        /// <summary>
        /// This represents an indexed XML comments file
        /// </summary>
        private class IndexedCommentsFile
        {
            #region Private data members
            //=====================================================================

            // The index that maps keys to XPath navigators containing the comments
            private Dictionary<string, XPathNavigator> index = new Dictionary<string, XPathNavigator>();

            #endregion

            #region Properties
            //=====================================================================

            /// <summary>
            /// This read-only property returns the XPath navigator for the specified key
            /// </summary>
            /// <param name="key">The key to look up</param>
            /// <returns>The XPath navigator associated with the key</returns>
            public XPathNavigator this[string key]
            {
                get { return index[key].Clone(); }
            }
            #endregion

            #region Methods, etc.
            //=====================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">The cache with which this indexed document is associated</param>
            /// <param name="filename">The name of the XML comments file to index</param>
            public IndexedCommentsFile(IndexedCommentsCache cache, string filename)
            {
                foreach(var kv in cache.GetValues(filename))
                    index[kv.Key] = kv.Value;
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XPathExpression memberListExpr;
        private XPathExpression keyExpr;

        private ConcurrentDictionary<string, string> index;
        private Queue<string> queue;
        private Dictionary<string, IndexedCommentsFile> cache;
        private int cacheSize, filesIndexed;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the number of items indexed
        /// </summary>
        public int IndexCount => index.Count;

        /// <summary>
        /// This read-only property returns the number of comments files that were indexed
        /// </summary>
        public int FilesIndexed => filesIndexed;

        /// <summary>
        /// This is used to get or set whether or not duplicate entry warnings are generated
        /// </summary>
        public bool ShowDuplicatesWarning { get; set; }

        /// <summary>
        /// This read-only property returns all keys in the index
        /// </summary>
        public IEnumerable<string> AllKeys => index.Keys;

        /// <summary>
        /// This read-only property returns the comments for the specified key
        /// </summary>
        /// <param name="key">The key for which to retrieve comments</param>
        /// <returns>An <see cref="XPathNavigator"/> for the comments or null if not found.</returns>
        public XPathNavigator this[string key]
        {
            get
            {
                IndexedCommentsFile document;
                string file;

                // Look up the file corresponding to the key
                if(index.TryGetValue(key, out file))
                {
                    // Now look for that file in the cache
                    if(!cache.TryGetValue(file, out document))
                    {
                        // Not in the cache, so load it
                        document = new IndexedCommentsFile(this, file);

                        // If the cache is full, remove a document
                        if(cache.Count >= cacheSize)
                            cache.Remove(queue.Dequeue());

                        // Add the new document to the cache
                        cache.Add(file, document);
                        queue.Enqueue(file);
                    }

                    return document[key];
                }

                return null;
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
            ReportWarning?.Invoke(this, args);
        }
        #endregion

        #region Constructor
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
            index = new ConcurrentDictionary<string, string>();
            queue = new Queue<string>(size);
            cache = new Dictionary<string, IndexedCommentsFile>(cacheSize);

            memberListExpr = XPathExpression.Compile("/doc/members/member");
            keyExpr = XPathExpression.Compile("@name");
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This loads an XML comments file and handles redirection
        /// </summary>
        /// <param name="filename">The XML comments file to load</param>
        /// <returns>An <see cref="XPathDocument"/> instance for the loaded XML comments file or null if it could
        /// not be loaded.</returns>
        private XPathDocument LoadXmlCommentsFile(string filename)
        {
            XPathDocument document = null;

            try
            {
                document = new XPathDocument(filename);

                // Some versions of the framework redirect the comments files to a common location
                var redirect = document.CreateNavigator().SelectSingleNode("doc/@redirect");

                if(redirect != null)
                {
                    string path = Environment.ExpandEnvironmentVariables(redirect.Value);

                    // If it still starts with a variable reference, it's typically this one which we have to
                    // handle ourselves.
                    if(path.StartsWith("%PROGRAMFILESDIR%", StringComparison.Ordinal))
                    {
                        string programFiles = Environment.GetFolderPath(Environment.Is64BitProcess ?
                            Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);

                        path = path.Replace("%PROGRAMFILESDIR%", programFiles + @"\");
                    }

                    if(!Path.IsPathRooted(path) || !File.Exists(path))
                    {
                        this.OnReportWarning(new CommentsCacheEventArgs("SHFB: Warning BE0031: Ignoring invalid " +
                            $"XML comments file '{filename}'.  Reason: Unable to locate redirected file {path}"));
                        document = null;
                    }
                    else
                        document = new XPathDocument(path);
                }
            }
            catch(IOException e)
            {
                throw new InheritedDocsException(String.Format(CultureInfo.CurrentCulture,
                    "An access error occurred while attempting to load the file '{0}'. The error message is: {1}",
                    filename, e.Message), e);
            }
            catch(XmlException e)
            {
                System.Diagnostics.Debug.WriteLine("Bad XML comments file '{0}'.  Error: {1}", filename, e);

                this.OnReportWarning(new CommentsCacheEventArgs(
                    $"SHFB: Warning BE0031: Ignoring invalid XML comments file '{filename}'.  Reason: {e.Message}"));
            }

            return document;
        }

        /// <summary>
        /// Index all comments files found in the specified folder.
        /// </summary>
        /// <param name="path">The path to search.  If null or empty, the current directory is assumed.</param>
        /// <param name="wildcard">The wildcard to use.  If null or empty, "*.xml" is assumed.</param>
        /// <param name="recurse">True to recurse subfolders or false to only use the given folder.</param>
        /// <param name="commentsFiles">Optional.  If not null, an <see cref="XPathDocument"/> is added to the
        /// collection for each file indexed.</param>
        /// <remarks>The files are indexed in parallel.</remarks>
        public void IndexCommentsFiles(string path, string wildcard, bool recurse,
          ConcurrentBag<XPathNavigator> commentsFiles)
        {
            XPathDocument xpathDoc;

            if(String.IsNullOrEmpty(path))
                path = Environment.CurrentDirectory;
            else
                path = Path.GetFullPath(path);

            if(String.IsNullOrEmpty(wildcard))
                wildcard = "*.xml";

            // Index the files
            Parallel.ForEach(Directory.EnumerateFiles(path, wildcard,
              recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly), filename =>
            {
                if(!filename.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    this.OnReportWarning(new CommentsCacheEventArgs(
                        "SHFB: Warning GID0007: Ignoring non-XML comments file: " + filename));
                    return;
                }

                if(commentsFiles != null)
                {
                    xpathDoc = this.LoadXmlCommentsFile(filename);

                    if(xpathDoc == null)
                        return;

                    commentsFiles.Add(xpathDoc.CreateNavigator());
                }

                // Get the keys from the file and add them to the index
                foreach(string key in this.GetKeys(filename))
                {
                    // Only report the warning if wanted.  If there are duplicates, the last one found wins.
                    if(this.ShowDuplicatesWarning && index.ContainsKey(key))
                        this.OnReportWarning(new CommentsCacheEventArgs(String.Format(
                            CultureInfo.InvariantCulture, "SHFB: Warning GID0008: Entries for the key " +
                            "'{0}' occur in both '{1}' and '{2}'.  The entries in '{2}' will be used.", key,
                            index[key], filename)));

                    index[key] = filename;
                }

                filesIndexed++;
            });
        }

        /// <summary>
        /// This returns an enumerable list of all key values from the specified XML file based on the
        /// expressions for this cache.
        /// </summary>
        /// <param name="file">The XML file from which to obtain the keys</param>
        /// <returns>An enumerable list of the key values in the given file</returns>
        public IEnumerable<string> GetKeys(string file)
        {
            var document = this.LoadXmlCommentsFile(file);

            if(document != null)
            {
                XPathNodeIterator valueNodes = document.CreateNavigator().Select(memberListExpr);

                foreach(XPathNavigator valueNode in valueNodes)
                {
                    XPathNavigator keyNode = valueNode.SelectSingleNode(keyExpr);

                    // Only return found key values
                    if(keyNode != null)
                    {
                        yield return keyNode.Value;

                        // Also add a namespace entry for NamespaceDoc classes
                        if(keyNode.Value.EndsWith(".NamespaceDoc", StringComparison.Ordinal))
                            yield return "N:" + keyNode.Value.Substring(2, keyNode.Value.Length - 15);

                        // Also add a namespace group entry for NamespaceGroupDoc classes
                        if(keyNode.Value.EndsWith(".NamespaceGroupDoc", StringComparison.Ordinal))
                            yield return "G:" + keyNode.Value.Substring(2, keyNode.Value.Length - 20);
                    }
                }
            }
        }

        /// <summary>
        /// This returns an enumerable list of all key/value pairs from the specified XML file based on the
        /// expressions for this cache.
        /// </summary>
        /// <param name="file">The XML file from which to obtain the keys</param>
        /// <returns>An enumerable list of the key/value values in the given file</returns>
        public IEnumerable<KeyValuePair<string, XPathNavigator>> GetValues(string file)
        {
            var document = this.LoadXmlCommentsFile(file);

            if(document != null)
            {
                XPathNodeIterator valueNodes = document.CreateNavigator().Select(memberListExpr);

                foreach(XPathNavigator valueNode in valueNodes)
                {
                    XPathNavigator keyNode = valueNode.SelectSingleNode(keyExpr);

                    // Only return values that have a key
                    if(keyNode != null)
                    {
                        yield return new KeyValuePair<string, XPathNavigator>(keyNode.Value, valueNode);

                        // Also add a namespace entry for NamespaceDoc classes
                        if(keyNode.Value.EndsWith(".NamespaceDoc", StringComparison.Ordinal))
                            yield return new KeyValuePair<string, XPathNavigator>("N:" + keyNode.Value.Substring(2,
                                keyNode.Value.Length - 15), valueNode);

                        // Also add a namespace group entry for NamespaceGroupDoc classes
                        if(keyNode.Value.EndsWith(".NamespaceGroupDoc", StringComparison.Ordinal))
                            yield return new KeyValuePair<string, XPathNavigator>("G:" + keyNode.Value.Substring(2,
                                keyNode.Value.Length - 20), valueNode);
                    }
                }
            }
        }
        #endregion
    }
}
