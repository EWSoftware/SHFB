//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentIndexCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/12/2021
//
// This is a version of the InMemoryIndexCache that adds the ability to store index information in one or more
// persistent ESENT databases.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/20/2013  EFW  Created the code
// 04/12/2021  EFW  Merged SHFB build components into the main build components assembly
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;

using Microsoft.Isam.Esent.Collections.Generic;

namespace Sandcastle.Tools.BuildComponents.Commands
{
    /// <summary>
    /// This is a version of the <c>InMemoryIndexCache</c> that adds the ability to store index information in
    /// one or more persistent ESENT databases.
    /// </summary>
    public class ESentIndexedCache : InMemoryIndexedCache
    {
        #region Private data members
        //=====================================================================

        private readonly List<PersistentDictionary<string, string>> esentCaches;
        private readonly XmlReaderSettings settings;

        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override int Count => base.Count + esentCaches.Sum(c => c.Count);

        /// <inheritdoc />
        public override XPathNavigator this[string key]
        {
            get
            {
                // Try the in-memory cache first
                XPathNavigator content = base[key];

                // If not found there, try the database caches if there are any
                if(content == null && esentCaches.Count != 0)
                    foreach(var c in esentCaches)
                        if(c.TryGetValue(key, out string xml))
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
            settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };

            esentCaches = new List<PersistentDictionary<string, string>>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If a cache path attribute is found and is not empty, the given database cache is used rather
        /// than an in-memory cache for the file set.  If not found or empty, the index information is added to
        /// the standard in-memory cache.</remarks>
        public override void AddDocuments(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var cache = this.CreateCache(configuration);

            // ESENT caches are inserted in reverse order as later caches take precedence over earlier ones
            if(cache == null)
                base.AddDocuments(configuration);
            else
                esentCaches.Insert(0, cache);
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

            this.Component.WriteMessage(MessageLevel.Diagnostic, "\"{0}\" highest ESENT local cache flush " +
                "count: {1}.  Highest ESENT current local cache usage: {2} of {3}.", base.Name, flushCount,
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
            PersistentDictionary<string, string> cache = null;
            HashSet<string> namespaceFileFilter = new HashSet<string>();
            string groupId, cachePathAttrName, cachePath, baseDirectory, wildcardPath, recurseValue, dupWarning,
                fullPath, directoryPart, filePart;
            bool isProjectData = false;
            int filesToLoad;

            var parent = configuration.Clone();
            parent.MoveToParent();

            groupId = configuration.GetAttribute("groupId", String.Empty);

            // If caching project data, they will all go into a common index
            if(groupId.StartsWith("Project_", StringComparison.OrdinalIgnoreCase))
            {
                cachePathAttrName = "projectCachePath";
                isProjectData = true;
            }
            else
                cachePathAttrName = "frameworkCachePath";

            cache = esentCaches.FirstOrDefault(c => c.DatabasePath.Contains(groupId));

            if(cache != null)
                cachePath = cache.DatabasePath;
            else
            {
                cachePath = parent.GetAttribute(cachePathAttrName, String.Empty);

                if(String.IsNullOrWhiteSpace(cachePath))
                    return null;

                cachePath = Path.Combine(cachePath, groupId);

                string cacheSize = parent.GetAttribute("localCacheSize", String.Empty);

                if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out int localCacheSize))
                    localCacheSize = 2500;

                // Column compression is left on here as it does benefit the string data
                cache = new PersistentDictionary<string, string>(cachePath) { LocalCacheSize = localCacheSize };
            }

            baseDirectory = configuration.GetAttribute("base", String.Empty);

            if(!String.IsNullOrWhiteSpace(baseDirectory))
                baseDirectory = Environment.ExpandEnvironmentVariables(baseDirectory);

            wildcardPath = configuration.GetAttribute("files", String.Empty);

            if(String.IsNullOrWhiteSpace(wildcardPath))
                base.Component.WriteMessage(MessageLevel.Error, "Each data element must have a files attribute " +
                    "specifying which files to index.");

            wildcardPath = Environment.ExpandEnvironmentVariables(wildcardPath);

            recurseValue = configuration.GetAttribute("recurse", String.Empty);

            if(String.IsNullOrWhiteSpace(recurseValue) || !Boolean.TryParse(recurseValue, out bool recurse))
                recurse = false;

            // Support suppression of duplicate ID warnings.  This can happen a lot when common classes appear in
            // multiple assemblies.
            dupWarning = configuration.GetAttribute("duplicateWarning", String.Empty);

            if(String.IsNullOrWhiteSpace(dupWarning) || !Boolean.TryParse(dupWarning, out bool reportDuplicateIds))
                reportDuplicateIds = true;

            if(String.IsNullOrEmpty(baseDirectory))
                fullPath = wildcardPath;
            else
            {
                // Verify that the directory exists
                if(!Directory.Exists(baseDirectory))
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The targets " +
                        "directory '{0}' does not exist.  The configuration is most likely out of date.  " +
                        "Please delete this component from the project, add it back, and reconfigure it.",
                        baseDirectory), nameof(configuration));

                fullPath = Path.Combine(baseDirectory, wildcardPath);
            }

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
                // empty, it has already been loaded unless it's a project comments file list.  In that case,
                // we will merge them if necessary.
                if(isProjectData || cache.Count == 0)
                    filesToLoad = Directory.EnumerateFiles(directoryPart, filePart, recurse ?
                        SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Count();
            }

            if(filesToLoad != 0)
            {
                // The time estimate is a ballpark figure and depends on the system
                base.Component.WriteMessage(MessageLevel.Diagnostic, "{0} file(s) need to be added to the " +
                    "ESENT index cache database.  Indexing them will take about {1:N0} minute(s), please be " +
                    "patient.  Cache location: {2}", filesToLoad, Math.Ceiling(filesToLoad / 60.0), cachePath);

                // Limit the degree of parallelism or it overwhelms the ESENT version store
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
