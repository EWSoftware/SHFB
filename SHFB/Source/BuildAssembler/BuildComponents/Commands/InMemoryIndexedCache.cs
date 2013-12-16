// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/20/2013 - EFW - Moved the IndexedDocumentCache into its own file in the Commands namespace and renamed it
// InMemoryIndexedCache to reflect its usage.  The IndexedDocument class was made a private nested class within
// InMemoryIndexedCache as it serves no purpose outside of it.  Added support for suppressing duplicate ID
// warnings.  This prevents lots of unnecessary warnings about duplicate IDs in comments files. Added support for
// filtering loaded index data by namespace.  This eliminates loading of unnecessary reflection data saving time
// and memory.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Commands
{
    /// <summary>
    /// This contains a collection of XPath navigators indexed by member ID stored in a simple
    /// <see cref="ConcurrentDictionary{TKey, TValue}"/> entirely in memory.
    /// </summary>
    /// <remarks>This index maps the element keys to the files in which they are found.  As needed, the files
    /// containing the requested keys are loaded and cached in memory.  When the cache fills, the oldest file is
    /// unloaded to make room for a new file.</remarks>
    public class InMemoryIndexedCache : IndexedCache
    {
        #region IndexedDocument
        //=====================================================================

        /// <summary>
        /// This is used by the <see cref="InMemoryIndexedCache"/> to contain index information for an individual
        /// XML file.
        /// </summary>
        /// <remarks>Instances of this class are created and cached as needed</remarks>
        private class IndexedDocument
        {
            #region Private data members
            //=====================================================================

            // The index that maps keys to XPath navigators containing the data
            Dictionary<string, XPathNavigator> index = new Dictionary<string, XPathNavigator>();
            #endregion

            #region Properties
            //=====================================================================

            /// <summary>
            /// This read-only property returns the XPath navigator for the specified key
            /// </summary>
            /// <param name="key">The key to look up</param>
            /// <returns>The XPath navigagor associated with the key</returns>
            public XPathNavigator this[string key]
            {
                get { return index[key].Clone(); }
            }
            #endregion

            #region Constructor
            //=====================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">The cache with which this indexed document is associated</param>
            /// <param name="file">The file to index</param>
            public IndexedDocument(IndexedCache cache, string file)
            {
                foreach(var kv in cache.GetValues(file))
                    index[kv.Key] = kv.Value;
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================

        // An index mapping keys to the files that contain them
        private ConcurrentDictionary<string, string> index = new ConcurrentDictionary<string, string>();

        // A simple caching mechanism.
        // This cache keeps track of the order that files are loaded in and always unloads the oldest one.
        // This is better, but a document that is often accessed gets no "points" so it will eventualy be
        // thrown out even if it is used regularly.
        private int cacheSize;
        private Queue<string> queue;
        private Dictionary<string, IndexedDocument> cache;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override int Count
        {
            get { return index.Count; }
        }

        /// <inheritdoc />
        public override XPathNavigator this[string key]
        {
            get
            {
                IndexedDocument document;
                string file;

                // Look up the file corresponding to the key
                if(index.TryGetValue(key, out file))
                {
                    // Now look for that file in the cache
                    if(!cache.TryGetValue(file, out document))
                    {
                        // Not in the cache, so load it
                        document = new IndexedDocument(this, file);

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

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The <see cref="CopyFromIndexComponent"/> to which the indexed cache belongs</param>
        /// <param name="context">A context to use with the key and value XPath expressions</param>
        /// <param name="configuration">The indexed cache configuration</param>
        public InMemoryIndexedCache(CopyFromIndexComponent component, XmlNamespaceManager context,
          XPathNavigator configuration) : base(component, context, configuration)
        {
            string cacheValue = configuration.GetAttribute("cache", String.Empty);
            int size;

            if(String.IsNullOrWhiteSpace(cacheValue) || !Int32.TryParse(cacheValue, out size) || size < 1)
                size = 15;

            this.cacheSize = size;

            // set up the cache
            queue = new Queue<string>(size);
            cache = new Dictionary<string, IndexedDocument>(size);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void AddDocuments(XPathNavigator configuration)
        {
            HashSet<string> namespaceFileFilter = new HashSet<string>();
            string baseDirectory, wildcardPath, recurseValue, dupWarning, fullPath, directoryPart,
                filePart;
            bool recurse, reportDuplicateIds;

            baseDirectory = configuration.GetAttribute("base", String.Empty);

            if(!String.IsNullOrWhiteSpace(baseDirectory))
                baseDirectory = Environment.ExpandEnvironmentVariables(baseDirectory);

            wildcardPath = configuration.GetAttribute("files", String.Empty);

            if(String.IsNullOrWhiteSpace(wildcardPath))
                base.Component.WriteMessage(MessageLevel.Error, "Each data element must have a files attribute " +
                    "specifying which files to index.");

            wildcardPath = Environment.ExpandEnvironmentVariables(wildcardPath);

            recurseValue = configuration.GetAttribute("recurse", String.Empty);

            if(String.IsNullOrWhiteSpace(recurseValue) || !Boolean.TryParse(recurseValue, out recurse))
                recurse = false;

            // Support suppression of duplicate ID warnings.  This can happen a lot when common classes appear in
            // multiple assemblies.
            dupWarning = configuration.GetAttribute("duplicateWarning", String.Empty);

            if(String.IsNullOrWhiteSpace(dupWarning) || !Boolean.TryParse(dupWarning, out reportDuplicateIds))
                reportDuplicateIds = true;

            if(String.IsNullOrEmpty(baseDirectory))
                fullPath = wildcardPath;
            else
                fullPath = Path.Combine(baseDirectory, wildcardPath);

            fullPath = Environment.ExpandEnvironmentVariables(fullPath);

            directoryPart = Path.GetDirectoryName(fullPath);

            if(String.IsNullOrEmpty(directoryPart))
                directoryPart = Environment.CurrentDirectory;

            filePart = Path.GetFileName(fullPath);

            // Filtering reduces the number of files to load, especially for the core reflection data files
            namespaceFileFilter.Clear();

            foreach(XPathNavigator filter in configuration.Select("namespace/@file"))
                namespaceFileFilter.Add(filter.Value);

            base.Component.WriteMessage(MessageLevel.Info, "Searching for files that match '{0}' in '{1}'",
                filePart, directoryPart);

            Parallel.ForEach(Directory.EnumerateFiles(directoryPart, filePart,
              recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly), file =>
              {
                  // Skip the file if not in a defined filter
                  if(namespaceFileFilter.Count != 0 && !namespaceFileFilter.Contains(Path.GetFileName(file)))
                      return;

                  // Get the keys from the file and add them to the index
                  foreach(string key in this.GetKeys(file))
                  {
                      // Only report the warning if wanted
                      if(index.ContainsKey(key) && reportDuplicateIds)
                          this.Component.WriteMessage(MessageLevel.Warn, "Entries for the key '{0}' occur in " +
                              "both '{1}' and '{2}'. The last entry will be used.", key, index[key], file);

                      index[key] = file;
                  }
              });
        }

        /// <summary>
        /// Report the cache usage for the build
        /// </summary>
        public override void ReportCacheStatistics()
        {
            this.Component.WriteMessage(MessageLevel.Diagnostic, "\"{0}\" in-memory cache entries used: " +
                "{1} of {2}.", base.Name, cache.Count, cacheSize);
        }
        #endregion
    }
}
