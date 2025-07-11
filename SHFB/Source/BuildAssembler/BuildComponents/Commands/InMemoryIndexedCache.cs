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

using Sandcastle.Core;

namespace Sandcastle.Tools.BuildComponents.Commands
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
            private readonly Dictionary<string, XPathNavigator> index = [];

            #endregion

            #region Properties
            //=====================================================================

            /// <summary>
            /// This read-only property returns the XPath navigator for the specified key
            /// </summary>
            /// <param name="key">The key to look up</param>
            /// <returns>The XPath navigator associated with the key</returns>
            public XPathNavigator this[string key] => index[key].Clone();

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
        private readonly ConcurrentDictionary<string, string> index = new();

        // A simple caching mechanism.
        // This cache keeps track of the order that files are loaded in and always unloads the oldest one.
        // This is better, but a document that is often accessed gets no "points" so it will eventually be
        // thrown out even if it is used regularly.
        private readonly int cacheSize;
        private readonly ConcurrentQueue<string> queue;
        private readonly ConcurrentDictionary<string, IndexedDocument> cache;

        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override int Count => index.Count;

        /// <inheritdoc />
        public override XPathNavigator this[string key]
        {
            get
            {
                // Look up the file corresponding to the key
                if(index.TryGetValue(key, out string file))
                {
                    // Now look for that file in the cache
                    if(!cache.TryGetValue(file, out IndexedDocument document))
                    {
                        // Not in the cache, so load it
                        document = new IndexedDocument(this, file);

                        // If the cache is full, remove a document
                        if(cache.Count >= cacheSize)
                        {
                            if(queue.TryDequeue(out string cacheFile))
                                cache.TryRemove(cacheFile, out _);
                        }

                        // Add the new document to the cache
                        cache.TryAdd(file, document);
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
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string cacheValue = configuration.GetAttribute("cache", String.Empty);

            if(String.IsNullOrWhiteSpace(cacheValue) || !Int32.TryParse(cacheValue, out int size) || size < 1)
                size = 15;

            this.cacheSize = size;

            // Set up the cache
            queue = new ConcurrentQueue<string>();
            cache = new ConcurrentDictionary<string, IndexedDocument>(4 * Environment.ProcessorCount, size);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void AddDocuments(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            HashSet<string> namespaceFileFilter = [];
            string baseDirectory, wildcardPath, recurseValue, dupWarning, fullPath, directoryPart, filePart;

            baseDirectory = configuration.GetAttribute("base", String.Empty).CorrectFilePathSeparators();

            if(!String.IsNullOrWhiteSpace(baseDirectory))
                baseDirectory = Environment.ExpandEnvironmentVariables(baseDirectory);

            wildcardPath = configuration.GetAttribute("files", String.Empty).CorrectFilePathSeparators();

            if(String.IsNullOrWhiteSpace(wildcardPath))
            {
                this.Component.WriteMessage(MessageLevel.Error, "Each data element must have a files attribute " +
                    "specifying which files to index.");
            }

            wildcardPath = Environment.ExpandEnvironmentVariables(wildcardPath);

            recurseValue = configuration.GetAttribute("recurse", String.Empty);

            if(String.IsNullOrWhiteSpace(recurseValue) || !Boolean.TryParse(recurseValue, out bool recurse))
                recurse = false;

            // Support suppression of duplicate ID warnings.  This can happen a lot when common classes appear in
            // multiple assemblies.
            dupWarning = configuration.GetAttribute("duplicateWarning", String.Empty);

            if(String.IsNullOrWhiteSpace(dupWarning) || !Boolean.TryParse(dupWarning, out bool reportDuplicateIds))
                reportDuplicateIds = true;

            if(String.IsNullOrWhiteSpace(baseDirectory))
                fullPath = wildcardPath;
            else
                fullPath = Path.Combine(baseDirectory, wildcardPath);

            fullPath = Environment.ExpandEnvironmentVariables(fullPath);

            directoryPart = Path.GetDirectoryName(fullPath);

            if(String.IsNullOrWhiteSpace(directoryPart))
                directoryPart = Environment.CurrentDirectory;

            filePart = Path.GetFileName(fullPath);

            // Filtering reduces the number of files to load, especially for the core reflection data files
            namespaceFileFilter.Clear();

            foreach(XPathNavigator filter in configuration.Select("namespace/@file"))
                namespaceFileFilter.Add(filter.Value);

            this.Component.WriteMessage(MessageLevel.Info, "Searching for files that match '{0}' in '{1}'",
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
                      if(index.TryGetValue(key, out string duplicate) && reportDuplicateIds)
                      {
                          this.Component.WriteMessage(MessageLevel.Warn, "Entries for the key '{0}' occur in " +
                              "both '{1}' and '{2}'. The last entry will be used.", key, duplicate, file);
                      }

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
                "{1} of {2}.", this.Name, cache.Count, cacheSize);
        }
        #endregion
    }
}
