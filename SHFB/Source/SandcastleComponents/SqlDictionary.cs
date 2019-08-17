//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/16/2019
// Note    : Copyright 2013-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a dictionary backed by a SQL Server table.  An optional group ID can be used to segregate
// values within the dictionary.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2013  EFW  Created the code
// 02/15/2013  EFW  Added support for grouping within the dictionary table
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a simple dictionary that stores its data in a SQL Server database table.  An optional group ID
    /// can be used to segregate values within the dictionary.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <remarks>This dictionary is intended for use in Sandcastle build components to store data needed for the
    /// build.  The key type is always a string as is the group ID type if used.  The value type is indicated
    /// using the <typeparamref name="TValue"/> type parameter and is assumed to be either a <see cref="String"/>
    /// or a reference type.</remarks>
    /// <threadsafety static="false" instance="false" />
    public sealed class SqlDictionary<TValue> : IDictionary<string, TValue>, IDisposable
    {
        #region SqlDictionary enumerator
        //=====================================================================

        /// <summary>
        /// This is an enumerator class used to enumerate all key/value pairs in the SQL dictionary table
        /// </summary>
        private class SqlDictionaryEnumerator : IEnumerator<KeyValuePair<string, TValue>>
        {
            #region Private data members
            //=====================================================================

            private SqlConnection cn;
            private SqlCommand cmd;
            private SqlDataReader rdr;
            private BinaryFormatter bf;
            private readonly string keyFieldName, valueFieldName;
            private readonly bool isReferenceType;

            #endregion

            #region Constructor
            //=====================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="owner">The owning <see cref="SqlDictionary{TValue}"/> class</param>
            internal SqlDictionaryEnumerator(SqlDictionary<TValue> owner)
            {
                cn = new SqlConnection(owner.connection.ConnectionString);
                bf = owner.bf;

                keyFieldName = owner.keyFieldName;
                valueFieldName = owner.valueFieldName;
                isReferenceType = owner.isReferenceType;

                if(owner.groupIdFieldName == null)
                    cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture, "Select {0}, {1} From {2}",
                        keyFieldName, valueFieldName, owner.tableName), cn);
                else
                    cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture, "Select {0}, {1} From {2} " +
                        "Where {3} = '{4}'", keyFieldName, valueFieldName, owner.tableName, owner.groupIdFieldName,
                        owner.groupId), cn);

                cn.Open();

                this.Reset();
            }
            #endregion

            #region IEnumerator<KeyValuePair<TKey, TValue>> Members
            //=====================================================================

            /// <inheritdoc />
            public KeyValuePair<string, TValue> Current
            {
                get
                {
                    TValue value;

                    if(!isReferenceType)
                        value = (TValue)rdr[valueFieldName];
                    else
                        using(MemoryStream ms = new MemoryStream((byte[])rdr[valueFieldName]))
                        {
                            value = (TValue)bf.Deserialize(ms);
                        }

                    return new KeyValuePair<string, TValue>((string)rdr[keyFieldName], value);
                }
            }
            #endregion

            #region IEnumerator Members
            //=====================================================================

            /// <inheritdoc />
            object System.Collections.IEnumerator.Current => this.Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if(rdr != null)
                    return rdr.Read();

                return false;
            }

            /// <inheritdoc />
            public void Reset()
            {
                if(rdr != null)
                    rdr.Dispose();

                rdr = cmd.ExecuteReader();
            }
            #endregion

            #region IDisposable Members
            //=====================================================================

            /// <inheritdoc />
            public void Dispose()
            {
                if(rdr != null)
                    rdr.Dispose();

                if(cmd != null)
                    cmd.Dispose();

                if(cn != null)
                    cn.Dispose();
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================

        private string tableName, groupIdFieldName, groupId, keyFieldName, valueFieldName;

        private Dictionary<string, TValue> localCache;
        private int localCacheSize, localCacheFlushCount;

        private SqlConnection connection;
        private SqlCommand cmdRetrieveValue, cmdInsertUpdateValue;
        private BinaryFormatter bf;
        private bool isReferenceType, isDisposed;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the group ID
        /// </summary>
        public string GroupId => groupId;

        /// <summary>
        /// Set this to a non-zero value to enable local caching of values to speed up read-only access
        /// </summary>
        /// <value>If set to zero, the default, the local cache will not be used and all values will be retrieved
        /// from the database.</value>
        public int LocalCacheSize
        {
            get => localCacheSize;
            set
            {
                if(value < 1)
                {
                    localCacheSize = 0;
                    localCache = null;
                }
                else
                {
                    localCacheSize = value;
                    localCache = new Dictionary<string, TValue>();
                }

                localCacheFlushCount = 0;
            }
        }

        /// <summary>
        /// This read-only property returns the number of times the local cache was flushed because it filled up
        /// </summary>
        /// <value>This can help in figuring out an appropriate local cache size</value>
        public int LocalCacheFlushCount => localCacheFlushCount;

        /// <summary>
        /// This read-only property returns the current number of local cache entries in use
        /// </summary>
        public int CurrentLocalCacheCount => (localCache == null) ? 0 : localCache.Count;

        /// <summary>
        /// This read-only property returns whether or not the cache has been disposed of
        /// </summary>
        public bool IsDisposed => isDisposed;

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Simple dictionary constructor
        /// </summary>
        /// <param name="connectionString">The connection string to the SQL database</param>
        /// <param name="tableName">The table name</param>
        /// <param name="keyFieldName">The key field name</param>
        /// <param name="valueFieldName">The value field name</param>
        /// <remarks>Values in dictionaries that use this constructor are not grouped.  All values in the
        /// table are returned.</remarks>
        /// <overloads>There are two overloads for the constructor</overloads>
        public SqlDictionary(string connectionString, string tableName, string keyFieldName,
          string valueFieldName)
        {
            if(String.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A valid connection string is required", connectionString);

            if(String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("A valid table name is required", tableName);

            if(String.IsNullOrWhiteSpace(keyFieldName))
                throw new ArgumentException("A valid key field name is required", keyFieldName);

            if(String.IsNullOrWhiteSpace(valueFieldName))
                throw new ArgumentException("A valid value field name is required", valueFieldName);

            this.tableName = tableName;
            this.keyFieldName = keyFieldName;
            this.valueFieldName = valueFieldName;

            // If not a string, assume it's a reference type
            isReferenceType = !(typeof(TValue) == typeof(string) || !typeof(TValue).IsClass);

            bf = new BinaryFormatter
            {
                Context = new StreamingContext(StreamingContextStates.Persistence)
            };

            connection = new SqlConnection(connectionString);

            cmdRetrieveValue = new SqlCommand(String.Format(CultureInfo.InvariantCulture, "Select {0} From " +
                "{1} Where {2} = @key", valueFieldName, tableName, keyFieldName), connection);
            cmdRetrieveValue.Parameters.Add(new SqlParameter("@key", SqlDbType.VarChar, 768));

            cmdInsertUpdateValue = new SqlCommand(String.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(Select * From {0} Where {1} = @key)
    Insert {0} ({1}, {2}) Values (@key, @value)
ELSE
    Update {0} Set {2} = @value Where {1} = @key", tableName, keyFieldName, valueFieldName), connection);

            cmdInsertUpdateValue.Parameters.Add(new SqlParameter("@key", SqlDbType.VarChar, 768));
            cmdInsertUpdateValue.Parameters.Add(new SqlParameter("@value", isReferenceType ?
                SqlDbType.VarBinary : SqlDbType.VarChar));

            connection.Open();
        }

        /// <summary>
        /// Grouped dictionary constructor
        /// </summary>
        /// <param name="connectionString">The connection string to the SQL database</param>
        /// <param name="tableName">The table name</param>
        /// <param name="groupId">The group ID field name</param>
        /// <param name="keyFieldName">The key field name</param>
        /// <param name="valueFieldName">The value field name</param>
        /// <param name="groupIdFieldName">The group ID to use</param>
        /// <remarks>Values in dictionaries that use this constructor are grouped using an additional field.
        /// This allows multiple related dictionaries to reside within the same table.  Only values from the
        /// dictionary identified by the group ID are returned.</remarks>
        public SqlDictionary(string connectionString, string tableName, string keyFieldName,
          string valueFieldName, string groupIdFieldName, string groupId) :
          this(connectionString, tableName, keyFieldName, valueFieldName)
        {
            if(String.IsNullOrWhiteSpace(groupIdFieldName))
                throw new ArgumentException("A valid group ID field name is required", groupIdFieldName);

            if(String.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("A valid group ID value is required", groupId);

            this.groupIdFieldName = groupIdFieldName;
            this.groupId = groupId;

            cmdRetrieveValue.CommandText = String.Format(CultureInfo.InvariantCulture, "Select {0} From " +
                "{1} Where {2} = '{3}' And {4} = @key", valueFieldName, tableName, groupIdFieldName, groupId,
                keyFieldName);

            cmdInsertUpdateValue.CommandText = String.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(Select * From {0} Where {1} = '{2}' And {3} = @key)
    Insert {0} ({1}, {3}, {4}) Values ('{2}', @key, @value)
ELSE
    Update {0} Set {4} = @value Where {1} = '{2}' And {3} = @key", tableName, groupIdFieldName, groupId,
                keyFieldName, valueFieldName);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to purge all data from the dictionary
        /// </summary>
        public void Purge()
        {
            if(groupIdFieldName == null)
            {
                using(SqlCommand cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture,
                  "Truncate Table {0}", tableName), connection))
                {
                    cmd.ExecuteScalar();
                }
            }
            else
                using(SqlCommand cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture,
                  "Delete {0} Where {1} = '{2}'", tableName, groupIdFieldName, groupId),
                  connection))
                {
                    cmd.ExecuteScalar();
                }
        }
        #endregion

        #region IDisposable Members
        //=====================================================================

        /// <summary>
        /// Dispose of the SQL dictionary
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;

            if(cmdInsertUpdateValue != null)
                cmdInsertUpdateValue.Dispose();

            if(cmdRetrieveValue != null)
                cmdRetrieveValue.Dispose();

            if(connection != null)
                connection.Dispose();
        }
        #endregion

        #region IDictionary<TKey, TValue> Members
        //=====================================================================

        /// <inheritdoc />
        public void Add(string key, TValue value)
        {
            this[key] = value;
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            cmdRetrieveValue.Parameters[0].Value = key;
            object value = cmdRetrieveValue.ExecuteScalar();

            return (value != null && value != DBNull.Value);
        }

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get
            {
                var keys = new HashSet<string>();

                using(var cmd = new SqlCommand { Connection = connection, CommandType = CommandType.Text })
                {
                    if(groupIdFieldName == null)
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "Select {0} From {1}",
                            keyFieldName, tableName);
                    else
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "Select {0} From {1} " +
                            "Where {2} = '{3}'", keyFieldName, tableName, groupIdFieldName, groupId);

                    using(var rdr = cmd.ExecuteReader())
                    {
                        while(rdr.Read())
                            keys.Add((string)rdr[0]);
                    }
                }

                return keys;
            }
        }

        /// <inheritdoc />
        /// <remarks>This method is not implemented</remarks>
        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out TValue value)
        {
            if(localCache != null && localCache.TryGetValue(key, out value))
                return true;

            value = default(TValue);

            cmdRetrieveValue.Parameters[0].Value = key;
            object v = cmdRetrieveValue.ExecuteScalar();

            if(v == null || v == DBNull.Value)
                return false;

            if(!isReferenceType)
                value = (TValue)v;
            else
                using(MemoryStream ms = new MemoryStream((byte[])v))
                {
                    value = (TValue)bf.Deserialize(ms);
                }

            if(localCache != null)
            {
                // If the cache is filled, clear it and start over.  Not the most sophisticated method, but
                // it works.
                if(localCache.Count >= localCacheSize)
                {
                    localCache.Clear();
                    localCacheFlushCount++;
                }

                localCache[key] = value;
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>This property is not implemented</remarks>
        public ICollection<TValue> Values => throw new NotImplementedException();

        /// <inheritdoc />
        public TValue this[string key]
        {
            get
            {
                if(!this.TryGetValue(key, out TValue value))
                    throw new KeyNotFoundException();

                return value;
            }
            set
            {
                cmdInsertUpdateValue.Parameters[0].Value = key;

                if(value != null)
                {
                    if(!isReferenceType)
                        cmdInsertUpdateValue.Parameters[1].Value = value;
                    else
                    {
                        using(MemoryStream ms = new MemoryStream())
                        {
                            bf.Serialize(ms, value);
                            cmdInsertUpdateValue.Parameters[1].Value = ms.GetBuffer();
                        }
                    }
                }
                else
                    cmdInsertUpdateValue.Parameters[1].Value = DBNull.Value;

                cmdInsertUpdateValue.ExecuteNonQuery();
            }
        }
        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>> Members
        //=====================================================================

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item)
        {
            this[item.Key] = item.Value;
        }

        /// <inheritdoc />
        /// <remarks>This method is not implemented</remarks>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return this.TryGetValue(item.Key, out TValue value);
        }

        /// <inheritdoc />
        /// <remarks>This method is not implemented</remarks>
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if(groupIdFieldName == null)
                {
                    using(SqlCommand cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture,
                      "Select COUNT(*) From {0}", tableName), connection))
                    {
                        return (int)cmd.ExecuteScalar();
                    }
                }
                else
                    using(SqlCommand cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture,
                      "Select COUNT(*) From {0} Where {1} = '{2}'", tableName, groupIdFieldName, groupId),
                      connection))
                    {
                        return (int)cmd.ExecuteScalar();
                    }
            }
        }

        /// <inheritdoc />
        /// <value>Always returns false</value>
        public bool IsReadOnly => false;

        /// <inheritdoc />
        /// <remarks>This method is not implemented</remarks>
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>> Members
        //=====================================================================

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return new SqlDictionaryEnumerator(this);
        }

        #endregion

        #region IEnumerable Members
        //=====================================================================

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
