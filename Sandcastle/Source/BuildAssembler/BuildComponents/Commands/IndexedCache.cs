// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 01/19/2013 - EFW - Created a new abstract base class for indexed cache classes

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Commands
{
    /// <summary>
    /// This abstract base class is used to create indexed caches of information represented by XPathNavigators
    /// such as reflection information and XML comments.
    /// </summary>
    public abstract class IndexedCache
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns a reference to the <see cref="CopyFromIndexComponent"/> that owns it
        /// </summary>
        public CopyFromIndexComponent Component { get; private set; }

        /// <summary>
        /// This read-only property returns the name of the index cache
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This read-only property returns the XPath expression used to search for values in the XML files
        /// </summary>
        public XPathExpression ValueExpression { get; private set; }

        /// <summary>
        /// This read-only property returns the XPath expression used to extract the key from values
        /// </summary>
        /// <value>The key expression is always relative to the index value node</value>
        public XPathExpression KeyExpression { get; private set; }

        /// <summary>
        /// This read-only property returns a count of the items in the indexed cache
        /// </summary>
        public abstract int IndexCount { get; }

        /// <summary>
        /// For indexes that cache information in memory, this property can be used to report how many cache
        /// entries were utilized.
        /// </summary>
        /// <value>This can be used to help size a local cache appropriately to conserve memory and/or improve
        /// performance.</value>
        public abstract int CacheEntriesUsed { get; }

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
            if(component == null)
                throw new ArgumentNullException("component");

            this.Component = component;

            // Get the name of the index
            this.Name = configuration.GetAttribute("name", String.Empty);

            if(String.IsNullOrWhiteSpace(this.Name))
                component.WriteMessage(MessageLevel.Error, "Each index must have a unique name");

            // Get the xpath for keys (relative to value nodes)
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

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to index documents and add their key/file mappings to the cache
        /// </summary>
        /// <param name="configuration">The configuration used to add documents</param>
        public abstract void AddDocuments(XPathNavigator configuration);

        /// <summary>
        /// This returns an enumerable list of all key values from the specified XML file based on the
        /// expressions for this cache.
        /// </summary>
        /// <param name="file">The XML file from which to obtain the keys</param>
        /// <returns>An enumerable list of the key values in the given file</returns>
        public IEnumerable<string> GetKeys(string file)
        {
            XPathDocument document = null;

            try
            {
                document = new XPathDocument(file);
            }
            catch(IOException e)
            {
                this.Component.WriteMessage(MessageLevel.Error, "An access error occured while attempting to " +
                    "load the file '{0}'. The error message is: {1}", file, e.Message);
            }
            catch(XmlException e)
            {
                this.Component.WriteMessage(MessageLevel.Error, "The indexed document '{0}' is not a valid " +
                    "XML document. The error message is: {1}", file, e.Message);
            }

            XPathNodeIterator valueNodes = document.CreateNavigator().Select(this.ValueExpression);

            foreach(XPathNavigator valueNode in valueNodes)
            {
                XPathNavigator keyNode = valueNode.SelectSingleNode(this.KeyExpression);

                // Only return found key values
                if(keyNode != null)
                    yield return keyNode.Value;
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
            XPathDocument document = null;

            try
            {
                document = new XPathDocument(file);
            }
            catch(IOException e)
            {
                this.Component.WriteMessage(MessageLevel.Error, "An access error occured while attempting to " +
                    "load the file '{0}'. The error message is: {1}", file, e.Message);
            }
            catch(XmlException e)
            {
                this.Component.WriteMessage(MessageLevel.Error, "The indexed document '{0}' is not a valid " +
                    "XML document. The error message is: {1}", file, e.Message);
            }

            XPathNodeIterator valueNodes = document.CreateNavigator().Select(this.ValueExpression);

            foreach(XPathNavigator valueNode in valueNodes)
            {
                XPathNavigator keyNode = valueNode.SelectSingleNode(this.KeyExpression);

                // Only return values that have a key
                if(keyNode != null)
                    yield return new KeyValuePair<string, XPathNavigator>(keyNode.Value, valueNode);
            }
        }
        #endregion
    }
}
