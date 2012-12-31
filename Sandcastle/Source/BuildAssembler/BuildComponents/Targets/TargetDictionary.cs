// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/28/2012 - EFW - Made the class derive from IDictionary<TKey, TValue> to make it easier to work with and
// renamed it TargetDictionary to reflect its true purpose.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Targets
{
    // The basic object model here is this:
    //  * Target objects represent files that can be targeted by a reference link
    //  * Different child objects of Target represent different sorts of API targets: Namespace, Type, Member, etc.
    //  * Targets are stored in a TargetDictionary
    // To indicate relationships between targets (e.g. a Method takes a particular type parameter), we
    // introduce another set of classes:
    //  * Reference objects refer to a specific target
    //  * Objects like SpecializedTypeReference and ArrayTypeReference that represent decorated types
    // There are two ways to construct such objects:
    //  * XML from a reflection information file defines Target and Reference objects.
    //    XmlTargetDictionaryUtilities does this.
    //  * Code entity reference strings construct Reference objects. TextReferenceUtilities does this.
    // Finally, we need a way to write the link text corresponding to a reference:
    //  * LinkTextResolver contains routines that, given a reference, writes the corresponding link text

    // All arguments of public methods are verified

    // The fact that the creation methods (via XML or CER strings) for references and their rendering methods
    // are separated from the declarations of the reference types goes against OO principals. (The consequent
    // absence of virtual methods also makes for a lot of ugly casting to figure out what method to call.)
    // But there is a reason for it: I wanted all the code that interpreted XML together, all the code that
    // interpreted CER strings together, and all the code that did link text rendering together, and I wanted
    // them all separate from each other. I belive this is extremely important for maintainability. It may
    // be possible to leverage partial classes to do this in a more OO fashion.

    /// <summary>
    /// This contains a collection of targets indexed by member ID
    /// </summary>
    /// <remarks>The behavior of this dictionary is to return null if a target ID is not found and to replace
    /// existing entries if a duplicate ID is added.</remarks>
    public class TargetDictionary : IDictionary<string, Target>
    {
        #region Private data members
        //=====================================================================

        private IDictionary<string, Target> index;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the target dictionary's unique ID
        /// </summary>
        public string DictionaryId { get; protected set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration used to create the target dictionary</param>
        public TargetDictionary(XPathNavigator configuration)
        {
            index = this.CreateTargetDictionary(configuration);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create a target dictionary from the given configuration information
        /// </summary>
        /// <param name="configuration">The target dictionary configuration</param>
        /// <returns>A target dictionary instance</returns>
        /// <remarks>This can be overridden in derived classes to provide persistent caches with backing stores
        /// other than the default in-memory dictionary.  The default implementation always creates a
        /// unique GUID ID if an <c>id</c> attribute is not found in the configuration.  Using a common ID
        /// across instances allows sharing of the target dicitonary data store.</remarks>
        protected virtual IDictionary<string, Target> CreateTargetDictionary(XPathNavigator configuration)
        {
            var td = new Dictionary<string, Target>();

            string id = configuration.GetAttribute("id", String.Empty);

            this.DictionaryId = String.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;

            // Get base directory
            string baseValue = configuration.GetAttribute("base", String.Empty);

            // Get file pattern
            string filesValue = configuration.GetAttribute("files", String.Empty);

            if(String.IsNullOrEmpty(filesValue))
                throw new ArgumentException("Each targets element must have a files attribute " +
                    "specifying which target files to load.", "configuration");

            // Determine whether to search recursively
            bool recurse = false;
            string recurseValue = configuration.GetAttribute("recurse", String.Empty);

            if(!String.IsNullOrEmpty(recurseValue) && !Boolean.TryParse(recurseValue, out recurse))
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "On the targets " +
                    "element, recurse='{0}' is not an allowed value.", recurseValue), "configuration");

            // Turn baseValue and filesValue into directoryPath and filePattern
            string fullPath;

            if(String.IsNullOrEmpty(baseValue))
                fullPath = filesValue;
            else
                fullPath = Path.Combine(baseValue, filesValue);

            fullPath = Environment.ExpandEnvironmentVariables(fullPath);

            string directoryPath = Path.GetDirectoryName(fullPath);

            if(String.IsNullOrEmpty(directoryPath))
                directoryPath = Environment.CurrentDirectory;

            string filePattern = Path.GetFileName(fullPath);

            // Verify that the directory exists
            if(!Directory.Exists(directoryPath))
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The targets " +
                    "directory '{0}' does not exist.", directoryPath), "configuration");

            foreach(string file in Directory.EnumerateFiles(directoryPath, filePattern, recurse ?
              SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                try
                {
                    XPathDocument document = new XPathDocument(file);

                    // We use the Target.Add() method so that any of its dependencies are added too
                    foreach(var t in XmlTargetDictionaryUtilities.EnumerateTargets(document.CreateNavigator()))
                        t.Add(td);
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
                        "An access error occured while opening the reference targets file '{0}'. The error " +
                        "message is: {1}", file, e.GetExceptionMessage()));
                }
            }

            return td;
        }
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
    }
}
