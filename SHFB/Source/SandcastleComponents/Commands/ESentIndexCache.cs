//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentIndexCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/28/2013
// Compiler: Microsoft Visual C#
//
// This is a version of the InMemoryIndexCache that adds the ability to store index information in one or more
// persistent ESent databases.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  01/20/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Commands;

using Microsoft.Isam.Esent.Collections.Generic;

namespace SandcastleBuilder.Components.Commands
{
    /// <summary>
    /// This is a version of the <c>InMemoryIndexCache</c> that adds the ability to store index information in
    /// one or more persistent ESent databases.
    /// </summary>
    public class ESentIndexedCache : InMemoryIndexedCache
    {
        #region Private data members
        //=====================================================================

        private List<PersistentDictionary<string, string>> esentCaches;
        private XmlReaderSettings settings;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override int IndexCount
        {
            get
            {
                return base.IndexCount + esentCaches.Sum(c => c.Count);
            }
        }

        /// <inheritdoc />
        public override XPathNavigator this[string key]
        {
            get
            {
                string xml;

                // Try the in-memory cache first
                XPathNavigator content = base[key];

                // If not found there, try the database caches if there are any
                if(content == null && esentCaches.Count != 0)
                    foreach(var c in esentCaches)
                        if(c.TryGetValue(key, out xml))
                        {
                            // Convert the XML to an XPath navigator
                            using(StringReader textReader = new StringReader(xml))
                                using(XmlReader reader = XmlReader.Create(textReader, settings))
                                {
                                    XPathDocument document = new XPathDocument(reader);
                                    content = document.CreateNavigator();
                                }
                        }

                return content;
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
        /// <param name="configuration">The configuration to use</param>
        public ESentIndexedCache(CopyFromIndexComponent component, XmlNamespaceManager context,
          XPathNavigator configuration) : base(component, context, configuration)
        {
            settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            esentCaches = new List<PersistentDictionary<string, string>>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If a <c>cachePath</c> attribute is found, the given database cache is used rather than an
        /// in-memory cache for the file set.  If not found, the index information is added to the standard
        /// in-memory cache.</remarks>
        public override void AddDocuments(XPathNavigator configuration)
        {
            string cachePath = configuration.GetAttribute("cachePath", String.Empty);

            // ESent caches are inserted in reverse order as later caches take precedence over earlier ones
            if(String.IsNullOrWhiteSpace(cachePath))
                base.AddDocuments(configuration);
            else
                esentCaches.Insert(0, this.CreateCache(configuration));
        }

        /// <summary>
        /// Report the cache usage for the build
        /// </summary>
        public override void ReportCacheStatistics()
        {
            int flushCount = 0, currentCount = 0, cacheSize = 0;

            // Get the highest local cache flush count and current count
            foreach(var c in esentCaches)
            {
                cacheSize = c.LocalCacheSize;   // These are all the same

                if(c.LocalCacheFlushCount > flushCount)
                    flushCount = c.LocalCacheFlushCount;

                if(c.CurrentLocalCacheCount > currentCount)
                    currentCount = c.CurrentLocalCacheCount;
            }

            this.Component.WriteMessage(MessageLevel.Diagnostic, "\"{0}\" highest ESent local cache flush " +
                "count: {1}.  Highest ESent current local cache usage: {2} of {3}.", base.Name, flushCount,
                currentCount, cacheSize);

            base.ReportCacheStatistics();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing && esentCaches != null)
                foreach(var c in esentCaches)
                    c.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create the index cache database
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        private PersistentDictionary<string, string> CreateCache(XPathNavigator configuration)
        {
            HashSet<string> namespaceFileFilter = new HashSet<string>();
            string cachePath, compress, baseDirectory, wildcardPath, recurseValue, dupWarning, fullPath,
                directoryPart, filePart;
            bool recurse, reportDuplicateIds, compressColumns = false;
            int localCacheSize, filesToLoad;

            cachePath = configuration.GetAttribute("cachePath", String.Empty);
            compress = configuration.GetAttribute("compressColumns", String.Empty);

            // If compressing columns, suffix the folder to allow switching back and forth
            if(!String.IsNullOrWhiteSpace(compress) && Boolean.TryParse(compress, out compressColumns) &&
              compressColumns)
                cachePath += "_Compressed";

            string cacheSize = configuration.GetAttribute("localCacheSize", String.Empty);

            if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out localCacheSize))
                localCacheSize = 1000;

            var cache = new PersistentDictionary<string, string>(cachePath, compressColumns)
                { LocalCacheSize = localCacheSize };

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

            // Loading new targets can take a while so issue a diagnostic message as an alert
            filesToLoad = 0;

            if(namespaceFileFilter.Count != 0)
            {
                // Reflection data can be filtered by namespace so some or all of it may already be there
                foreach(string file in Directory.EnumerateFiles(directoryPart, filePart, recurse ?
                  SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    if((namespaceFileFilter.Count == 0 || namespaceFileFilter.Contains(Path.GetFileName(file))) &&
                      !cache.ContainsKey("N:" + Path.GetFileNameWithoutExtension(file)))
                        filesToLoad++;
            }
            else
            {
                // Comments files can't be filtered by namespace so we'll assume that if the collection is not
                // empty, it has already been loaded.
                if(cache.Count == 0)
                    filesToLoad = Directory.EnumerateFiles(directoryPart, filePart, recurse ?
                        SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Count();
            }

            if(filesToLoad != 0)
            {
                // The time estimate is a ballpark figure and depends on the system
                base.Component.WriteMessage(MessageLevel.Diagnostic, "{0} files need to be added to the ESent " +
                    "index cache database.  Indexing them will take about {1:N0} minute(s), please be " +
                    "patient.  Cache location: {2}", filesToLoad, Math.Ceiling(filesToLoad / 60.0), cachePath);

                // Limit the degree of parallelism or it overwhelms the ESent version store
                Parallel.ForEach(Directory.EnumerateFiles(directoryPart, filePart,
                  recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly),
                  new ParallelOptions { MaxDegreeOfParallelism = 3 },
                  file =>
                  {
                      // Skip the file if not in a defined filter
                      if(namespaceFileFilter.Count != 0 && !namespaceFileFilter.Contains(Path.GetFileName(file)))
                          return;

                      // Get the keys from the file and add them to the index
                      foreach(var kv in base.GetValues(file))
                      {
                          // Only report the warning if wanted
                          if(cache.ContainsKey(kv.Key) && reportDuplicateIds)
                              this.Component.WriteMessage(MessageLevel.Warn, "An entry for the key '{0}' " +
                                  "already exists and will be replaced by the one from '{1}'", kv.Key, file);

                          cache[kv.Key] = kv.Value.InnerXml;
                      }
                  });
            }

            return cache;
        }
        #endregion
    }
}
