// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/28/2012 - EFW - Made the class derive from IDictionary<TKey, TValue> to make it easier to work with and
// renamed it TargetDictionary to reflect its true purpose.
// 12/31/2012 - EFW - Converted to abstract base class to allow for various implementations that utilize
// different backing stores including those with support for persistence.
// 01/13/2013 - EFW - Reworked LoadTargetDictionary() to use Parallel.For() by default to load the targets.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

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

    // The fact that the creation methods (via XML or CER (code entity reference) strings) for references and
    // their rendering methods are separated from the declarations of the reference types goes against OO
    // principals. (The consequent absence of virtual methods also makes for a lot of ugly casting to figure out
    // what method to call.)

    // But there is a reason for it: I wanted all the code that interpreted XML together, all the code that
    // interpreted CER strings together, and all the code that did link text rendering together, and I wanted
    // them all separate from each other. I believe this is extremely important for maintainability. It may
    // be possible to leverage partial classes to do this in a more OO fashion.

    /// <summary>
    /// This is a base class used for a collection of targets indexed by member ID
    /// </summary>
    [Serializable]
    public abstract class TargetDictionary : IDictionary<string, Target>, IDisposable
    {
        #region Private data members
        //=====================================================================

        private HashSet<string> namespaceFileFilter;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the build component that owns the target dictionary
        /// </summary>
        /// <value>This is useful for logging information during initialization</value>
        public BuildComponentCore BuildComponent { get; private set; }

        /// <summary>
        /// This is used to get or set the target dictionary's unique ID
        /// </summary>
        public string DictionaryId { get; protected set; }

        /// <summary>
        /// This is used to get or set the path to the target files
        /// </summary>
        public string DirectoryPath { get; protected set; }

        /// <summary>
        /// This is used to get or set the file pattern to use when searching for target files
        /// </summary>
        public string FilePattern { get; protected set; }

        /// <summary>
        /// This is used to get or set whether to recurse into subfolders of <see cref="DirectoryPath"/> when
        /// loading target files.
        /// </summary>
        public bool Recurse { get; protected set; }

        /// <summary>
        /// This read-only property returns any optional namespace files used to filter what gets loaded
        /// </summary>
        public HashSet<string> NamespaceFileFilter
        {
            get { return namespaceFileFilter; }
        }

        /// <summary>
        /// This read-only property can be used to determine whether or not the target dictionary has been
        /// disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The build component that owns the dictionary.  This is useful for logging
        /// messages during initialization.</param>
        /// <param name="configuration">The configuration used to create the target dictionary</param>
        /// <remarks>The default implementation always creates a unique ID based on the directory path and file
        /// pattern if an <c>id</c> attribute is not found in the configuration.  Using a common ID across
        /// instances allows sharing of the target dictionary data store.</remarks>
        protected TargetDictionary(BuildComponentCore component, XPathNavigator configuration)
        {
            this.BuildComponent = component;

            string id = configuration.GetAttribute("id", String.Empty);

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

            this.Recurse = recurse;

            // Turn baseValue and filesValue into directoryPath and filePattern
            string fullPath;

            if(String.IsNullOrEmpty(baseValue))
                fullPath = filesValue;
            else
                fullPath = Path.Combine(baseValue, filesValue);

            fullPath = Environment.ExpandEnvironmentVariables(fullPath);

            this.DirectoryPath = Path.GetDirectoryName(fullPath);

            if(String.IsNullOrEmpty(this.DirectoryPath))
                this.DirectoryPath = Environment.CurrentDirectory;

            this.FilePattern = Path.GetFileName(fullPath);

            // Verify that the directory exists
            if(!Directory.Exists(this.DirectoryPath))
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The targets " +
                    "directory '{0}' does not exist.", this.DirectoryPath), "configuration");

            if(String.IsNullOrWhiteSpace(id))
                id = Path.Combine(this.DirectoryPath, this.FilePattern).GetHashCode().ToString("X",
                    CultureInfo.InvariantCulture);

            this.DictionaryId = id;

            namespaceFileFilter = new HashSet<string>();

            foreach(XPathNavigator filter in configuration.Select("namespace/@file"))
                namespaceFileFilter.Add(filter.Value);
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the target dictionary if not done
        /// explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~TargetDictionary()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the target dictionary
        /// </summary>
        /// <overloads>There are two overloads for this method</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose of in this one
            this.IsDisposed = true;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This helper method can be called to find all target files and load them into the dictionary
        /// </summary>
        /// <param name="maxDegreeOfParallelism">This can be used to override the maximum degree of parallelism.
        /// By default, it is -1 to allow as many threads as possible.</param>
        /// <remarks>This method assumes that the dictionary is thread-safe and supports parallel loading of
        /// target data.  If not, override this method to load the data synchronously.</remarks>
        protected virtual void LoadTargetDictionary(int maxDegreeOfParallelism = -1)
        {
            Parallel.ForEach(Directory.EnumerateFiles(this.DirectoryPath, this.FilePattern, this.Recurse ?
              SearchOption.AllDirectories : SearchOption.TopDirectoryOnly),
              new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
              file =>
            {
                // Skip the file if not in a defined filter or if it's already in the dictionary
                if((namespaceFileFilter.Count != 0 && !namespaceFileFilter.Contains(Path.GetFileName(file))) ||
                  this.ContainsKey("N:" + Path.GetFileNameWithoutExtension(file)))
                    return;

                this.BuildComponent.WriteMessage(MessageLevel.Info, "Indexing targets in {0}", file);

                try
                {
                    XPathDocument document = new XPathDocument(file);

                    // We use the Target.Add() method so that any of its dependencies are added too
                    foreach(var t in XmlTargetDictionaryUtilities.EnumerateTargets(document.CreateNavigator()))
                        t.Add(this);
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
            });
        }

        /// <summary>
        /// This can be overridden in derived classes to report cache usage statistics after the build
        /// </summary>
        /// <remarks>The default implementation does nothing.  You can override this to provide information that
        /// can help adjust the cache size to make it more efficient.</remarks>
        public virtual void ReportCacheStatistics()
        {
        }
        #endregion

        #region IDictionary<string,Target> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If the key already exists, the existing value is replaced</remarks>
        public abstract void Add(string key, Target value);

        /// <inheritdoc />
        public abstract bool ContainsKey(string key);

        /// <inheritdoc />
        public abstract ICollection<string> Keys { get; }

        /// <inheritdoc />
        /// <remarks>This method is not implemented as targets are never removed</remarks>
        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract bool TryGetValue(string key, out Target value);

        /// <inheritdoc />
        public abstract ICollection<Target> Values { get; }

        /// <inheritdoc />
        /// <returns>If not found, this implementation returns null.</returns>
        public abstract Target this[string key] { get; set; }

        #endregion

        #region ICollection<KeyValuePair<string,Target>> Members
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If the key already exists, the existing value is replaced</remarks>
        public abstract void Add(KeyValuePair<string, Target> item);

        /// <inheritdoc />
        public abstract void Clear();

        /// <inheritdoc />
        public abstract bool Contains(KeyValuePair<string, Target> item);

        /// <inheritdoc />
        public abstract void CopyTo(KeyValuePair<string, Target>[] array, int arrayIndex);

        /// <inheritdoc />
        public abstract int Count { get; }

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
        public abstract IEnumerator<KeyValuePair<string, Target>> GetEnumerator();

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
