//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : InMemoryTargetDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains a target dictionary backed by a simple Dictionary<TKey, TValue> instance.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/31/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This contains a collection of targets indexed by member ID stored in a simple
    /// <see cref="ConcurrentDictionary{TKey, TValue}"/> entirely in memory.
    /// </summary>
    /// <remarks>The behavior of this dictionary is to return null if a target ID is not found and to replace
    /// existing entries if a duplicate ID is added.  All targets are stored in memory.  Since it must load all
    /// targets the first time the files are encountered, it can slow down initialization.  The trade off is that
    /// it can run faster than database-backed implementations that look up the items rather than storing them
    /// in memory.
    /// 
    /// <para>This implementation does not offer the option for a persistent cache as streaming the entire
    /// dictionary in and out takes several times longer than just loading the source XML data.</para></remarks>
    [Serializable]
    public sealed class InMemoryTargetDictionary : TargetDictionary
    {
        #region Private data members
        //=====================================================================

        private readonly ConcurrentDictionary<string, Target> index;

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
        public InMemoryTargetDictionary(BuildComponentCore component, XPathNavigator configuration) :
          base(component, configuration)
        {
            index = new ConcurrentDictionary<string, Target>();

            this.LoadTargetDictionary();
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
