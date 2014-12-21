//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlIndexCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/16/2014
// Compiler: Microsoft Visual C#
//
// This is a version of the InMemoryIndexCache that adds the ability to store index information in a persistent
// SQL database.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/15/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Commands;

namespace SandcastleBuilder.Components.Commands
{
    /// <summary>
    /// This is a version of the <c>InMemoryIndexCache</c> that adds the ability to store index information in a
    /// persistent SQL database.
    /// </summary>
    public class SqlIndexedCache : InMemoryIndexedCache
    {
        #region Private data members
        //=====================================================================

        private List<SqlDictionary<string>> sqlCaches;
        private XmlReaderSettings settings;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override int Count
        {
            get
            {
                return base.Count + sqlCaches.Sum(c => c.Count);
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
                if(content == null && sqlCaches.Count != 0)
                    foreach(var c in sqlCaches)
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
        public SqlIndexedCache(CopyFromIndexComponent component, XmlNamespaceManager context,
          XPathNavigator configuration) : base(component, context, configuration)
        {
            settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            sqlCaches = new List<SqlDictionary<string>>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If a <c>groupId</c> attribute is found, the given database cache is used rather than an
        /// in-memory cache for the file set.  If not found, the index information is added to the standard
        /// in-memory cache.</remarks>
        public override void AddDocuments(XPathNavigator configuration)
        {
            var cache = this.CreateCache(configuration);

            // SQL caches are inserted in reverse order as later caches take precedence over earlier ones
            if(cache == null)
                base.AddDocuments(configuration);
            else
                sqlCaches.Insert(0, cache);
        }

        /// <summary>
        /// Report the cache usage for the build
        /// </summary>
        public override void ReportCacheStatistics()
        {
            int flushCount = 0, currentCount = 0, cacheSize = 0;

            // Get the highest local cache flush count and current count
            foreach(var c in sqlCaches)
            {
                cacheSize = c.LocalCacheSize;   // These are all the same

                if(c.LocalCacheFlushCount > flushCount)
                    flushCount = c.LocalCacheFlushCount;

                if(c.CurrentLocalCacheCount > currentCount)
                    currentCount = c.CurrentLocalCacheCount;
            }

            this.Component.WriteMessage(MessageLevel.Diagnostic, "\"{0}\" highest SQL local cache flush " +
                "count: {1}.  Highest SQL current local cache usage: {2} of {3}.", base.Name, flushCount,
                currentCount, cacheSize);

            base.ReportCacheStatistics();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing && sqlCaches != null)
                foreach(var c in sqlCaches)
                    c.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create the index cache
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        private SqlDictionary<string> CreateCache(XPathNavigator configuration)
        {
            SqlDictionary<string> cache = null;
            HashSet<string> namespaceFileFilter = new HashSet<string>();
            string groupId, connectionString, baseDirectory, wildcardPath, recurseValue, dupWarning, fullPath,
                directoryPart, filePart;
            bool recurse, reportDuplicateIds, cacheProject, isProjectData = false;
            int localCacheSize, filesToLoad;

            var parent = configuration.Clone();
            parent.MoveToParent();

            connectionString = parent.GetAttribute("connectionString", String.Empty);
            groupId = configuration.GetAttribute("groupId", String.Empty);

            // If caching project data, they will all go into a common index
            if(groupId.StartsWith("Project_", StringComparison.OrdinalIgnoreCase))
                isProjectData = true;

            cache = sqlCaches.FirstOrDefault(c => c.GroupId == groupId);

            if(cache == null)
            {
                if(!Boolean.TryParse(parent.GetAttribute("cacheProject", String.Empty), out cacheProject))
                    cacheProject = false;

                if((isProjectData && !cacheProject) || String.IsNullOrWhiteSpace(connectionString))
                    return null;

                string cacheSize = configuration.GetAttribute("localCacheSize", String.Empty);

                if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out localCacheSize))
                    localCacheSize = 2500;

                cache = new SqlDictionary<string>(connectionString, "IndexData", "IndexKey", "IndexValue",
                  "GroupId", groupId)
                {
                    LocalCacheSize = localCacheSize
                };
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
            {
                // Verify that the directory exists
                if(!Directory.Exists(baseDirectory))
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The targets " +
                        "directory '{0}' does not exist.  The configuration is most likely out of date.  " +
                        "Please delete this component from the project, add it back, and reconfigure it.",
                        baseDirectory), "configuration");

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
                base.Component.WriteMessage(MessageLevel.Diagnostic, "{0} file(s) need to be added to the SQL " +
                    "index cache database.  Indexing them will take about {1:N0} minute(s), please be " +
                    "patient.  Group ID: {2}  Cache location: {3}", filesToLoad, Math.Ceiling(filesToLoad / 60.0),
                    groupId, connectionString);

                Parallel.ForEach(Directory.EnumerateFiles(directoryPart, filePart,
                    recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly),
                    file =>
                    {
                        // Skip the file if not in a defined filter
                        if(namespaceFileFilter.Count != 0 && !namespaceFileFilter.Contains(Path.GetFileName(file)))
                            return;

                        base.Component.WriteMessage(MessageLevel.Info, "Indexing targets in {0}", file);

                        using(SqlConnection cn = new SqlConnection(connectionString))
                        using(SqlCommand cmdSearch = new SqlCommand())
                        using(SqlCommand cmdAdd = new SqlCommand())
                        {
                            cn.Open();

                            cmdSearch.Connection = cmdAdd.Connection = cn;

                            cmdSearch.CommandText = String.Format(CultureInfo.InvariantCulture,
                                "Select IndexKey From IndexData Where GroupId = '{0}' And IndexKey = @key",
                                groupId);
                            cmdSearch.Parameters.Add(new SqlParameter("@key", SqlDbType.VarChar, 768));

                            cmdAdd.CommandText = String.Format(CultureInfo.InvariantCulture,
                                "IF NOT EXISTS(Select * From IndexData Where GroupId = '{0}' And IndexKey = @key) " +
                                "Insert IndexData (GroupId, IndexKey, IndexValue) Values('{0}', @key, @value) " +
                                "ELSE " +
                                "Update IndexData Set IndexValue = @value Where GroupId = '{0}' And IndexKey = @key",
                                groupId);

                            cmdAdd.Parameters.Add(new SqlParameter("@key", SqlDbType.VarChar, 768));
                            cmdAdd.Parameters.Add(new SqlParameter("@value", SqlDbType.Text));

                            // Get the keys from the file and add them to the index
                            foreach(var kv in base.GetValues(file))
                            {
                                cmdSearch.Parameters[0].Value = cmdAdd.Parameters[0].Value = kv.Key;

                                // Only report the warning if wanted
                                if(cmdSearch.ExecuteScalar() != null && reportDuplicateIds)
                                    this.Component.WriteMessage(MessageLevel.Warn, "An entry for the key '{0}' " +
                                        "already exists and will be replaced by the one from '{1}'", kv.Key, file);

                                cmdAdd.Parameters[1].Value = kv.Value.InnerXml;
                                cmdAdd.ExecuteNonQuery();
                            }
                        }
                    });
            }

            return cache;
        }
        #endregion
    }
}
