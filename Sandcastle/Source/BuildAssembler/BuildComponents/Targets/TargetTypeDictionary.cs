//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : TargetTypeDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/30/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a target type dictionary used to contain common target dictionaries with their associated
// link type.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 2.7.3.0  12/29/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This is a dictionary used to contain common target dictionaries with their associated link type
    /// </summary>
    /// <remarks>The behavior of this dictionary is to return null if a target ID is not found and to replace
    /// existing entries if a duplicate ID is added.  The structure allows access to all reference link targets
    /// within a set of multiple target dictionaries, each with a different reference link type.  The target
    /// dictionary instances can be easily shared across multiple instances of the reference link components.</remarks>
    public class TargetTypeDictionary : IDictionary<string, Target>
    {
        #region Private data members
        //=====================================================================

        private List<KeyValuePair<ReferenceLinkType, TargetDictionary>> targetDictionaries;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TargetTypeDictionary()
        {
            targetDictionaries = new List<KeyValuePair<ReferenceLinkType, TargetDictionary>>();
        }
        #endregion

        #region IDictionary<string,Target> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This method is not implemented.  Use <see cref="Add(ReferenceLinkType, TargetDictionary"/>
        /// to add target dictionaries and their associated reference link type</remarks>
        public void Add(string key, Target value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            foreach(var kp in targetDictionaries)
                if(kp.Value.ContainsKey(key))
                    return true;

            return false;
        }

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get
            {
                return targetDictionaries.Select(kv => kv.Value.Keys).SelectMany(k => k).ToList();
            }
        }

        /// <inheritdoc />
        /// <remarks>This method is not implemented as targets are never removed</remarks>
        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out Target value)
        {
            value = null;

            foreach(var kp in targetDictionaries)
                if(kp.Value.TryGetValue(key, out value))
                    return true;

            return false;
        }

        /// <summary>
        /// This attempts to retrieve the item with the specified key, returning it along with the reference
        /// link type with which it is associated.
        /// </summary>
        /// <param name="key">The item key to look up</param>
        /// <param name="value">On return, this will contain the item value if found</param>
        /// <param name="linkType">On return, this will contain the link type of the item if found</param>
        /// <returns>True if the item was found, false if not</returns>
        public bool TryGetValue(string key, out Target value, out ReferenceLinkType linkType)
        {
            value = null;
            linkType = ReferenceLinkType.None;

            foreach(var kp in targetDictionaries)
                if(kp.Value.TryGetValue(key, out value))
                {
                    linkType = value.IsInvalidLink ? ReferenceLinkType.None : kp.Key;
                    return true;
                }

            return false;
        }

        /// <inheritdoc />
        public ICollection<Target> Values
        {
            get
            {
                return targetDictionaries.Select(kv => kv.Value.Values).SelectMany(v => v).ToList();
            }
        }

        /// <inheritdoc />
        /// <returns>If not found, this implementation returns null.</returns>
        public Target this[string key]
        {
            get
            {
                Target t;

                this.TryGetValue(key, out t);

                return t;
            }
            set
            {
                // Can't set a value since we don't know what type of link is wanted
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,Target>> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This method is not implemented.  Use <see cref="Add(ReferenceLinkType, TargetDictionary"/>
        /// to add target dictionaries and their associated reference link type</remarks>
        public void Add(KeyValuePair<string, Target> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            targetDictionaries.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, Target> item)
        {
            foreach(var kv in targetDictionaries)
                if(((ICollection<KeyValuePair<string, Target>>)kv.Value).Contains(item))
                    return true;

            return false;
        }

        /// <inheritdoc />
        /// <remarks>This method is not currently implemented</remarks>
        public void CopyTo(KeyValuePair<string, Target>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return targetDictionaries.Sum(kv => kv.Value.Count);
            }
        }

        /// <inheritdoc />
        /// <value>This always returns false</value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        /// <remarks>This method is not implemented as targets are never removed</remarks>
        public bool Remove(KeyValuePair<string, Target> item)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEnumerable<KeyValuePair<string,Target>> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This method is not currently implemented</remarks>
        public IEnumerator<KeyValuePair<string, Target>> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEnumerable Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This method is not currently implemented</remarks>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Target type methods
        //=====================================================================

        /// <summary>
        /// This read-only property is used to determine if any of the target dictionaries require the
        /// <see cref="MsdnResolver"/> to look up links.
        /// </summary>
        public bool NeedsMsdnResolver
        {
            get
            {
                return targetDictionaries.Any(kv => kv.Key == ReferenceLinkType.Msdn);
            }
        }

        /// <summary>
        /// Add a target type dictionary to the collection
        /// </summary>
        /// <param name="type">The reference link type to use for targets in the given dictionary</param>
        /// <param name="dictionary">The target dictionary to add</param>
        public void Add(ReferenceLinkType type, TargetDictionary dictionary)
        {
            // Dictionaries are stored in reverse order to allow entries from later dictionaries to override
            // one from earlier dictionaries
            targetDictionaries.Insert(0, new KeyValuePair<ReferenceLinkType, TargetDictionary>(type, dictionary));
        }
        #endregion

#if DEBUG
        /// <summary>
        /// Dump the references to an XML file
        /// </summary>
        /// <param name="targetFile">A targets file to load and from which to generate reference links</param>
        /// <remarks>This is used as a debugging aid to compare the resolved references to prior versions and
        /// ensure that they match.</remarks>
        public void DumpTargetDictionary(string targetsFile)
        {
            LinkTextResolver resolver = new LinkTextResolver(this);

            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
            XmlWriter writer = XmlWriter.Create(targetsFile, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("References");
            writer.WriteAttributeString("Count", this.Count.ToString());

            foreach(var td in targetDictionaries.Reverse<KeyValuePair<ReferenceLinkType, TargetDictionary>>())
                foreach(var target in td.Value)
                {
                    writer.WriteStartElement("Reference");
                    writer.WriteAttributeString("Id", target.Key);

                    resolver.WriteTarget(target.Value, DisplayOptions.All, writer);

                    writer.WriteEndElement();
                }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
#endif
    }
}
