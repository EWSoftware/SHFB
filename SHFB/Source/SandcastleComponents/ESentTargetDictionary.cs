//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentTargetDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/16/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a target dictionary backed by a Managed ESent PersistentDictionary<TKey, TValue> instance.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  01/01/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Targets;

using Microsoft.Isam.Esent.Collections.Generic;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This contains a collection of targets indexed by member ID stored in a Managed ESent
    /// <c>PersistentDictionary&lt;TKey, TValue&gt;</c>.
    /// </summary>
    /// <remarks>The behavior of this dictionary is to return null if a target ID is not found and to replace
    /// existing entries if a duplicate ID is added.  All targets are stored in a Managed ESent database.  The
    /// initial use will create the database slowing down initialization on first use.  Subsequent uses will not
    /// need to recreate it.  The trade off is that it can use much less memory at the expense of some build
    /// speed for each topic.  The speed difference is offset somewhat by the shorter initialization time.</remarks>
    [Serializable]
    public sealed class ESentTargetDictionary : TargetDictionary
    {
        #region Private data members
        //=====================================================================

        private PersistentDictionary<string, Target> index;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The build component that owns the dictionary.  This is useful for logging
        /// messages during initialization.</param>
        /// <param name="configuration">The target dictionary configuration</param>
        /// <returns>A target dictionary instance that uses a simple in-memory
        /// <see cref="Dictionary{TKey, TValue}"/> instance to store the targets.</returns>
        public ESentTargetDictionary(BuildComponent component, XPathNavigator configuration) :
          base(component, configuration)
        {
            bool compressColumns = false;
            int localCacheSize;

            string dbPath = configuration.GetAttribute("dbPath", String.Empty);

            if(String.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentException("The dbPath attribute must contain a value", "configuration");

            string compress = configuration.GetAttribute("compressColumns", String.Empty);

            // If compressing columns, suffix the folder to allow switching back and forth
            if(!String.IsNullOrWhiteSpace(compress) && Boolean.TryParse(compress, out compressColumns) &&
              compressColumns)
                dbPath += "_Compressed";

            string cacheSize = configuration.GetAttribute("localCacheSize", String.Empty);

            if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out localCacheSize))
                localCacheSize = 1000;

            // This is a slightly modified version of Managed ESent that provides the option to serialize
            // reference types.  In this case, we don't care about potential issues of persisted copies not
            // matching the original if modified as they are never updated once written to the cache.  We can
            // also turn off column compression for a slight performance increase.
            PersistentDictionaryFile.AllowReferenceTypeSerialization = true;

            index = new PersistentDictionary<string, Target>(dbPath, compressColumns)
            { LocalCacheSize = localCacheSize };

            // Loading new targets can take a while so issue a diagnostic message as an alert
            int filesToLoad = 0;

            foreach(string file in Directory.EnumerateFiles(this.DirectoryPath, this.FilePattern, this.Recurse ?
              SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                if((this.NamespaceFileFilter.Count == 0 || this.NamespaceFileFilter.Contains(
                  Path.GetFileName(file))) && !this.ContainsKey("N:" + Path.GetFileNameWithoutExtension(file)))
                    filesToLoad++;

            // The time estimate is a ballpark figure and depends on the system
            if(filesToLoad != 0)
            {
                component.WriteMessage(MessageLevel.Diagnostic, "{0} target files need to be added to the " +
                    "ESent reflection target cache database.  Indexing them will take about {1:N0} minute(s), " +
                    "please be patient.  Cache location: {2}", filesToLoad, filesToLoad * 10 / 60.0, dbPath);

                // Limit the degree of parallelism or it overwhelms the ESent version store
                this.LoadTargetDictionary(3);
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(!base.IsDisposed)
            {
                index.Dispose();
                base.Dispose(disposing);
            }
        }
        #endregion

        #region IDictionary<string,Target> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If the key already exists, the existing value is replaced</remarks>
        public override void Add(string key, Target value)
        {
            index[key] = value;
        }

        /// <inheritdoc />
        public override bool ContainsKey(string key)
        {
            return index.ContainsKey(key);
        }

        /// <inheritdoc />
        public override ICollection<string> Keys
        {
            get { return index.Keys; }
        }

        /// <inheritdoc />
        public override bool TryGetValue(string key, out Target value)
        {
            return index.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public override ICollection<Target> Values
        {
            get { return index.Values; }
        }

        /// <inheritdoc />
        /// <returns>If not found, this implementation returns null.</returns>
        public override Target this[string key]
        {
            get
            {
                Target t;

                index.TryGetValue(key, out t);

                return t;
            }
            set
            {
                index[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,Target>> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If the key already exists, the existing value is replaced</remarks>
        public override void Add(KeyValuePair<string, Target> item)
        {
            index[item.Key] = item.Value;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            index.Clear();
        }

        /// <inheritdoc />
        public override bool Contains(KeyValuePair<string, Target> item)
        {
            return ((ICollection<KeyValuePair<string, Target>>)index).Contains(item);
        }

        /// <inheritdoc />
        public override void CopyTo(KeyValuePair<string, Target>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, Target>>)index).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public override int Count
        {
            get { return index.Count; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Target>> Members
        //=====================================================================

        /// <inheritdoc />
        public override IEnumerator<KeyValuePair<string, Target>> GetEnumerator()
        {
            return index.GetEnumerator();
        }
        #endregion
    }
}
