//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlTargetDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/12/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a target dictionary backed by a SqlDictionary<TKey, TValue> instance.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2013  EFW  Created the code
// 04/12/2021  EFW  Merged SHFB build components into the main build components assembly
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This contains a collection of targets indexed by member ID stored in a <see cref="SqlDictionary{TValue}"/>
    /// </summary>
    /// <remarks>The behavior of this dictionary is to return null if a target ID is not found and to replace
    /// existing entries if a duplicate ID is added.  All targets are stored in a SQL database.  The
    /// initial use will create the database slowing down initialization on first use.  Subsequent uses will not
    /// need to recreate it.  The trade off is that it can use much less memory at the expense of some build
    /// speed for each topic.  The speed difference is offset somewhat by the shorter initialization time.</remarks>
    /// <threadsafety static="false" instance="false" />
    public sealed class SqlTargetDictionary : TargetDictionary
    {
        #region Private data members
        //=====================================================================

        private readonly SqlDictionary<Target> index;
        private readonly string connectionString;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The build component that owns the dictionary.  This is useful for logging
        /// messages during initialization.</param>
        /// <param name="configuration">The target dictionary configuration</param>
        /// <param name="connectionString">The connection string to use</param>
        /// <param name="groupId">The group ID to use</param>
        /// <param name="localCacheSize">The local cache size to use</param>
        /// <param name="reload">True to reload the cache or false to leave it alone.  This is used to reload
        /// project data so that it is always current.</param>
        /// <returns>A target dictionary instance that uses a simple in-memory
        /// <see cref="Dictionary{TKey, TValue}"/> instance to store the targets.</returns>
        public SqlTargetDictionary(BuildComponentCore component, XPathNavigator configuration,
          string connectionString, string groupId, int localCacheSize, bool reload) :
          base(component, configuration)
        {
            this.connectionString = connectionString;
            base.DictionaryId = groupId;

            index = new SqlDictionary<Target>(connectionString, "Targets", "TargetKey", "TargetValue",
              "GroupId", groupId)
            {
                LocalCacheSize = localCacheSize
            };

            int filesToLoad = 0;

            if(reload)
            {
                filesToLoad = Directory.EnumerateFiles(this.DirectoryPath, this.FilePattern, this.Recurse ?
                  SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Count();
            }
            else
                foreach(string file in Directory.EnumerateFiles(this.DirectoryPath, this.FilePattern, this.Recurse ?
                  SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    if((this.NamespaceFileFilter.Count == 0 || this.NamespaceFileFilter.Contains(
                      Path.GetFileName(file))) && !this.ContainsKey("N:" + Path.GetFileNameWithoutExtension(file)))
                        filesToLoad++;

            // Loading new targets can take a while so issue a diagnostic message as an alert.  The time estimate
            // is a ballpark figure and depends on the system.
            if(filesToLoad != 0)
            {
                component.WriteMessage(MessageLevel.Diagnostic, "{0} file(s) need to be added to the SQL " +
                    "reflection target cache database.  Indexing them will take about {1:N0} minute(s), " +
                    "please be patient.  Cache location: {2}", filesToLoad, Math.Ceiling(filesToLoad / 60.0),
                    connectionString);

                this.LoadTargetDictionary();
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
                if(index != null)
                    index.Dispose();

                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// This is overridden to load the SQL dictionary in a thread-safe manner
        /// </summary>
        /// <param name="maxDegreeOfParallelism">This can be used to override the maximum degree of parallelism.
        /// By default, it is -1 to allow as many threads as possible.</param>
        protected override void LoadTargetDictionary(int maxDegreeOfParallelism = -1)
        {
            var namespaceFileFilter = this.NamespaceFileFilter;

            Parallel.ForEach(Directory.EnumerateFiles(this.DirectoryPath, this.FilePattern, this.Recurse ?
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly),
                new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                file =>
                {
                    using(SqlConnection cn = new SqlConnection(connectionString))
                    using(SqlCommand cmd = new SqlCommand())
                    {
                        cn.Open();

                        cmd.Connection = cn;
                        cmd.Parameters.Add(new SqlParameter("@key", SqlDbType.VarChar, 768));

                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                            "Select TargetKey From Targets Where GroupId = '{0}' And TargetKey = @key",
                            base.DictionaryId);
                        cmd.Parameters[0].Value = "N:" + Path.GetFileNameWithoutExtension(file);

                        // Skip the file if not in a defined filter or if it's already in the dictionary
                        if((namespaceFileFilter.Count != 0 && !namespaceFileFilter.Contains(Path.GetFileName(file))) ||
                          cmd.ExecuteScalar() != null)
                            return;

                        this.BuildComponent.WriteMessage(MessageLevel.Info, "Indexing targets in {0}", file);

                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                            "IF NOT EXISTS(Select * From Targets Where GroupId = '{0}' And TargetKey = @key) " +
                            "Insert Targets (GroupId, TargetKey, TargetValue) Values ('{0}', @key, @value) " +
                            "ELSE Update Targets Set TargetValue = @value Where GroupId = '{0}' " +
                            "And TargetKey = @key", base.DictionaryId);

                        cmd.Parameters.Add(new SqlParameter("@value", SqlDbType.VarBinary));

                        BinaryFormatter bf = new BinaryFormatter
                        {
                            Context = new StreamingContext(StreamingContextStates.Persistence)
                        };

                        try
                        {
                            using(var reader = XmlReader.Create(file, new XmlReaderSettings { CloseInput = true }))
                            {
                                XPathDocument document = new XPathDocument(reader);

                                foreach(var t in XmlTargetDictionaryUtilities.EnumerateTargets(document.CreateNavigator()))
                                {
                                    cmd.Parameters[0].Value = t.Id;

                                    using(MemoryStream ms = new MemoryStream())
                                    {
                                        bf.Serialize(ms, t);
                                        cmd.Parameters[1].Value = ms.GetBuffer();
                                        cmd.ExecuteNonQuery();
                                    }

                                    // Enumeration targets have members we need to add too.  We can't use the
                                    // Target.Add() method here so we must do it manually.
                                    if(t is EnumerationTarget et)
                                        foreach(var el in et.Elements)
                                        {
                                            cmd.Parameters[0].Value = el.Id;

                                            using(MemoryStream ms = new MemoryStream())
                                            {
                                                bf.Serialize(ms, el);
                                                cmd.Parameters[1].Value = ms.GetBuffer();
                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                }
                            }
                        }
                        catch(XmlSchemaException e)
                        {
                            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                "The reference targets file '{0}' is not valid. The error message is: {1}", file,
                                e.GetExceptionMessage()));
                        }
                        catch(XmlException e)
                        {
                            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                "The reference targets file '{0}' is not well-formed XML.  The error message is: {1}",
                                file, e.GetExceptionMessage()));
                        }
                        catch(IOException e)
                        {
                            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                "An access error occurred while opening the reference targets file '{0}'. The error " +
                                "message is: {1}", file, e.GetExceptionMessage()));
                        }
                    }
                });
        }

        /// <summary>
        /// Report the cache usage for the build
        /// </summary>
        public override void ReportCacheStatistics()
        {
            this.BuildComponent.WriteMessage(MessageLevel.Diagnostic, "\"{0}\" SQL local cache flushed {1} " +
                "time(s).  Current SQL local cache usage: {2} of {3}.", base.DictionaryId,
                index.LocalCacheFlushCount, index.CurrentLocalCacheCount, index.LocalCacheSize);

            base.ReportCacheStatistics();
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
        public override ICollection<string> Keys => index.Keys;

        /// <inheritdoc />
        public override bool TryGetValue(string key, out Target value)
        {
            return index.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public override ICollection<Target> Values => index.Values;

        /// <inheritdoc />
        /// <returns>If not found, this implementation returns null.</returns>
        public override Target this[string key]
        {
            get
            {
                index.TryGetValue(key, out Target t);

                return t;
            }
            set => index[key] = value;
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
        public override int Count => index.Count;

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
