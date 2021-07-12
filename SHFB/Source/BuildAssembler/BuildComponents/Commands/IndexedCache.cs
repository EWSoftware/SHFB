﻿// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/19/2013 - EFW - Created a new abstract base class for indexed cache classes
// 01/24/2013 - EFW - Added IDisposable implementation
// 05/18/2018 - EFW - Added code to handle invalid and redirected XML comments files

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;

namespace Sandcastle.Tools.BuildComponents.Commands
{
    /// <summary>
    /// This abstract base class is used to create indexed caches of information represented by XPathNavigators
    /// such as reflection information and XML comments.
    /// </summary>
    public abstract class IndexedCache : IDisposable
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns a reference to the <see cref="CopyFromIndexComponent"/> that owns it
        /// </summary>
        public CopyFromIndexComponent Component { get; }

        /// <summary>
        /// This read-only property returns the name of the index cache
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This read-only property returns the XPath expression used to search for values in the XML files
        /// </summary>
        public XPathExpression ValueExpression { get; }

        /// <summary>
        /// This read-only property returns the XPath expression used to extract the key from values
        /// </summary>
        /// <value>The key expression is always relative to the index value node</value>
        public XPathExpression KeyExpression { get; }

        /// <summary>
        /// This read-only property can be used to determine whether or not the indexed cache has been disposed
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// This read-only property returns a count of the items in the indexed cache
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// This read-only property returns the value in the indexed cache for the given key
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>The value associated with the key or null if it was not found</returns>
        public abstract XPathNavigator this[string key] { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The <see cref="CopyFromIndexComponent"/> to which the indexed cache belongs</param>
        /// <param name="context">A context to use with the key and value XPath expressions</param>
        /// <param name="configuration">The configuration to use</param>
        protected IndexedCache(CopyFromIndexComponent component, XmlNamespaceManager context,
          XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.Component = component ?? throw new ArgumentNullException(nameof(component));

            // Get the name of the index
            this.Name = configuration.GetAttribute("name", String.Empty);

            if(String.IsNullOrWhiteSpace(this.Name))
                component.WriteMessage(MessageLevel.Error, "Each index must have a unique name");

            // Get the XPath for keys (relative to value nodes)
            string keyXPath = configuration.GetAttribute("key", String.Empty);

            if(String.IsNullOrWhiteSpace(keyXPath))
                component.WriteMessage(MessageLevel.Error, "Each index element must have a key attribute " +
                    "containing an XPath (relative to the value XPath) that evaluates to the entry key");

            // Get the XPath for value nodes
            string valueXPath = configuration.GetAttribute("value", String.Empty);

            if(String.IsNullOrWhiteSpace(valueXPath))
                component.WriteMessage(MessageLevel.Error, "Each index element must have a value attribute " +
                    "containing an XPath that describes index entries");

            try
            {
                this.KeyExpression = XPathExpression.Compile(keyXPath);
            }
            catch(XPathException)
            {
                component.WriteMessage(MessageLevel.Error, "The key expression '{0}' is not a valid XPath " +
                    "expression", keyXPath);
            }

            this.KeyExpression.SetContext(context);

            try
            {
                this.ValueExpression = XPathExpression.Compile(valueXPath);
            }
            catch(XPathException)
            {
                component.WriteMessage(MessageLevel.Error, "The value expression '{0}' is not a valid XPath " +
                    "expression", valueXPath);
            }

            this.ValueExpression.SetContext(context);
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the indexed cache if not done
        /// explicitly with <see cref="Dispose()"/>.
        /// </summary>
        ~IndexedCache()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the indexed cache
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

        #region Methods
        //=====================================================================

        /// <summary>
        /// This loads an XML file and handles redirection in XML comments files
        /// </summary>
        /// <param name="filename">The XML file to load</param>
        /// <returns>An <see cref="XPathDocument"/> instance for the loaded XML file or null if it could not be
        /// loaded.</returns>
        private XPathDocument LoadXmlCommentsFile(string filename)
        {
            XPathDocument document = null;

            try
            {
                document = new XPathDocument(XmlReader.Create(filename, new XmlReaderSettings { CloseInput = true }));

                // For XML comments files, some versions of the framework redirect the comments files to a
                // common location.
                var redirect = document.CreateNavigator().SelectSingleNode("doc/@redirect");

                if(redirect != null)
                {
                    string path = Environment.ExpandEnvironmentVariables(redirect.Value);

                    // If it still starts with a variable reference, it's typically this one which we have to
                    // handle ourselves.
                    if(path.StartsWith("%PROGRAMFILESDIR%", StringComparison.Ordinal))
                    {
                        string programFiles = Environment.GetFolderPath(Environment.Is64BitProcess ?
                            Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);

                        path = path.Replace("%PROGRAMFILESDIR%", programFiles + @"\");
                    }

                    if(!Path.IsPathRooted(path) || !File.Exists(path))
                    {
                        this.Component.WriteMessage(MessageLevel.Warn, "Ignoring invalid XML comments file " +
                            $"'{filename}'.  Reason: Unable to locate redirected file {path}");
                        document = null;
                    }
                    else
                        document = new XPathDocument(XmlReader.Create(path, new XmlReaderSettings { CloseInput = true }));
                }
            }
            catch(IOException e)
            {
                this.Component.WriteMessage(MessageLevel.Error, "An access error occurred while attempting to " +
                    "load the file '{0}'. The error message is: {1}", filename, e.Message);
            }
            catch(XmlException e)
            {
                this.Component.WriteMessage(MessageLevel.Warn,
                    $"Ignoring invalid XML comments file '{filename}'.  Reason: {e.Message}");
            }

            return document;
        }

        /// <summary>
        /// This is used to index documents and add their key/file mappings to the cache
        /// </summary>
        /// <param name="configuration">The configuration used to add documents</param>
        public abstract void AddDocuments(XPathNavigator configuration);

        /// <summary>
        /// This can be overridden in derived classes to report cache usage statistics after the build
        /// </summary>
        /// <remarks>The default implementation does nothing.  You can override this to provide information that
        /// can help adjust the cache size to make it more efficient.</remarks>
        public virtual void ReportCacheStatistics()
        {
        }

        /// <summary>
        /// This returns an enumerable list of all key values from the specified XML file based on the
        /// expressions for this cache.
        /// </summary>
        /// <param name="file">The XML file from which to obtain the keys</param>
        /// <returns>An enumerable list of the key values in the given file</returns>
        public IEnumerable<string> GetKeys(string file)
        {
            XPathDocument document = this.LoadXmlCommentsFile(file);

            if(document != null)
            {
                XPathNodeIterator valueNodes = document.CreateNavigator().Select(this.ValueExpression);

                foreach(XPathNavigator valueNode in valueNodes)
                {
                    XPathNavigator keyNode = valueNode.SelectSingleNode(this.KeyExpression);

                    // Only return found key values
                    if(keyNode != null)
                        yield return keyNode.Value;
                }
            }
        }

        /// <summary>
        /// This returns an enumerable list of all key/value pairs from the specified XML file based on the
        /// expressions for this cache.
        /// </summary>
        /// <param name="file">The XML file from which to obtain the keys</param>
        /// <returns>An enumerable list of the key/value values in the given file</returns>
        public IEnumerable<KeyValuePair<string, XPathNavigator>> GetValues(string file)
        {
            XPathDocument document = this.LoadXmlCommentsFile(file);

            if(document != null)
            {
                XPathNodeIterator valueNodes = document.CreateNavigator().Select(this.ValueExpression);

                foreach(XPathNavigator valueNode in valueNodes)
                {
                    XPathNavigator keyNode = valueNode.SelectSingleNode(this.KeyExpression);

                    // Only return values that have a key
                    if(keyNode != null)
                        yield return new KeyValuePair<string, XPathNavigator>(keyNode.Value, valueNode);
                }
            }
        }
        #endregion
    }
}
