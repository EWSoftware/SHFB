// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/28/2012 - EFW - Made the class derive from IDictionary<TKey, TValue> to make it easier to work with and
// renamed it to reflect its true purpose.

// TODO: Rename the class and the file TargetDictionary to reflect its true nature

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Targets
{
    // The basic object model here is this:
    //  * Target objects represent files that can be targeted by a reference link
    //  * Different child objects of Target represent different sorts of API targets: Namespace, Type, Member, etc.
    //  * Targets are stored in a TargetCollection
    // To indicate relationships between targets (e.g. a Method takes a particular type parameter), we
    // introduce another set of classes:
    //  * Reference objects refer to a specific target
    //  * Objects like SpecializedTypeReference and ArrayTypeReference that represent decorated types
    // There are two ways to construct such objects:
    //  * XML from a reflection information file defines Target and Reference objects. XmlUtilities does this.
    //  * Code entity reference strings construct Reference objecs. CerUtilities does this.
    // Finally, we need a way to write the link text corresponding to a reference:
    //  * LinkTextResolver contains routines that, given a reference, writes the corresponding link text

    // All arguments of public methods are verified

    // The fact that the creation methods (via XML or CER strings) for references and their rendering methods
    // are separated from the declarations of the reference types goes against OO principals. (The consequent
    // absence of virtual methods also makes for a lot of ugly casting to figure out what method to call.)
    // But there is a reason for it: I wanted all the code that intrepreted XML together, all the code that
    // intrepreted CER strings together, and all the code that did link text renderig together, and I wanted
    // them all separate from each other. I belive this is extremely important for maintainability. It may
    // be possible to leverage partial classes to do this in a more OO fashion.

    /// <summary>
    /// This contains a collection of targets indexed by member ID
    /// </summary>
    /// <remarks>The behavior of this dictionary is to return null if a target ID is not found and to replace
    /// existing entries if a duplicate ID is added.</remarks>
    public class TargetCollection : IDictionary<string, Target>
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, Target> index = new Dictionary<string, Target>();
        #endregion

        #region IDictionary<string,Target> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If the key already exists, the existing value is replaced</remarks>
        public void Add(string key, Target value)
        {
            index[key] = value;
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return index.ContainsKey(key);
        }

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get { return index.Keys; }
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
            return index.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public ICollection<Target> Values
        {
            get { return index.Values; }
        }

        /// <inheritdoc />
        /// <returns>If not found, this implementation returns null.</returns>
        public Target this[string key]
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
        public void Add(KeyValuePair<string, Target> item)
        {
            index[item.Key] = item.Value;
        }

        /// <inheritdoc />
        public void Clear()
        {
            index.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, Target> item)
        {
            return ((ICollection<KeyValuePair<string, Target>>)index).Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, Target>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, Target>>)index).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public int Count
        {
            get { return index.Count; }
        }

        /// <inheritdoc />
        /// <value>This always returns false</value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        /// <remarks>This method is not implemented</remarks>
        public bool Remove(KeyValuePair<string, Target> item)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEnumerable<KeyValuePair<string,Target>> Members
        //=====================================================================

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, Target>> GetEnumerator()
        {
            return index.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        //=====================================================================

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return index.GetEnumerator();
        }
        #endregion

#if DEBUG
        /// <summary>
        /// Dump the references to an XML file
        /// </summary>
        /// <param name="targetFile">A targets file to load and from which to generate reference links</param>
        /// <remarks>This is used as a debugging aid to compare the resolved references to prior versions and
        /// ensure that they match.</remarks>
        public void DumpTargetCollection(string targetsFile)
        {
            LinkTextResolver resolver = new LinkTextResolver(this);

            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
            XmlWriter writer = XmlWriter.Create(targetsFile, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("References");
            writer.WriteAttributeString("Count", this.Count.ToString());

            foreach(var target in this)
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
