//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/16/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a dictionary backed by a SQL Server table.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  01/14/2013  EFW  Created the code
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
    /// This is a simple dictionary that stores its data in a SQL Server database table
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <remarks>This dictionary is intended for use in Sandcastle build components to store data needed for the
    /// build.  The key type is always a string.  The value type is indicated using the
    /// <typeparamref name="TValue"/> type parameter and is assumed to be either a <see cref="String"/> or a
    /// reference type.</remarks>
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
            private string tableName, keyFieldName, valueFieldName;
            private bool isReferenceType;
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

                tableName = owner.tableName;
                keyFieldName = owner.keyFieldName;
                valueFieldName = owner.valueFieldName;
                isReferenceType = owner.isReferenceType;

                cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture, "Select {0}, {1} From {2}",
                    keyFieldName, valueFieldName, tableName), cn);

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
            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

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

        private string connectionString, tableName, keyFieldName, valueFieldName;

        private Dictionary<string, TValue> localCache;
        private int localCacheSize;

        private SqlConnection connection;
        private SqlCommand cmdRetrieveValue, cmdInsertUpdateValue;
        private BinaryFormatter bf;
        private bool isReferenceType;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Set this to a non-zero value to enable local caching of values to speed up read-only access
        /// </summary>
        /// <value>If set to zero, the default, the local cache will not be used and all values will be retrieved
        /// from the database.</value>
        public int LocalCacheSize
        {
            get { return localCacheSize; }
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
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">The connection string to the SQL database</param>
        /// <param name="tableName">The table name</param>
        /// <param name="keyFieldName">The key field name</param>
        /// <param name="valueFieldName">The value field name</param>
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

            this.connectionString = connectionString;
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
        #endregion

        #region IDisposable Members
        //=====================================================================

        /// <summary>
        /// Dispose of the SQL dictionary
        /// </summary>
        public void Dispose()
        {
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
            return (cmdRetrieveValue.ExecuteScalar() != null);
        }

        /// <inheritdoc />
        /// <remarks>This property is not implemented</remarks>
        public ICollection<string> Keys
        {
            get { throw new NotImplementedException(); }
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

            if(v == null)
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
                    localCache.Clear();

                localCache[key] = value;
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>This property is not implemented</remarks>
        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public TValue this[string key]
        {
            get
            {
                TValue value;

                if(!this.TryGetValue(key, out value))
                    throw new KeyNotFoundException();

                return value;
            }
            set
            {
                cmdInsertUpdateValue.Parameters[0].Value = key;

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
            TValue value;

            return this.TryGetValue(item.Key, out value);
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
                using(SqlCommand cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture,
                    "Select COUNT(*) From {0}", tableName), connection))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <inheritdoc />
        /// <value>Always returns false</value>
        public bool IsReadOnly
        {
            get { return false; }
        }

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
