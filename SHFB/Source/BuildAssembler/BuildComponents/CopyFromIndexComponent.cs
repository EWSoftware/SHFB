// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/20/2012 - EFW - Reworked the index cache and moved those classes to the Commands namespace and put them
// in their own files.
// 01/24/2012 - EFW - Added a virtual method to create the index caches, added code to dispose of them when done,
// and exposed the context via a protected property.
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Tools.BuildComponents.Commands;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This build component copies elements from an indexed set of XML files into the target document based on
    /// one or more copy commands that define the elements to copy and where to put them.
    /// </summary>
    public class CopyFromIndexComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Copy From Index Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <summary>
            /// This is used to import the list of copy component factories that is passed to the build component
            /// when it is created.
            /// </summary>
            [ImportMany(typeof(ICopyComponentFactory))]
            private List<Lazy<ICopyComponentFactory, ICopyComponentMetadata>> CopyComponents { get; set; }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new CopyFromIndexComponent(this.BuildAssembler, this.CopyComponents);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, string> contextNamespaces = [];

        // List of copy components
        private readonly List<Lazy<ICopyComponentFactory, ICopyComponentMetadata>> copyComponentFactories;
        private readonly List<CopyComponentCore> components = [];

        // What to copy
        private readonly List<CopyFromIndexCommand> copyCommands = [];

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the context to use for the index when evaluating XPath expressions
        /// </summary>
        /// <remarks>Since this is only for providing XML namespaces, it can be shared amongst all indexes</remarks>
        protected CustomContext Context { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        /// <param name="copyComponentFactories">The list of available copy component factory components</param>
        protected CopyFromIndexComponent(IBuildAssembler buildAssembler,
          List<Lazy<ICopyComponentFactory, ICopyComponentMetadata>> copyComponentFactories) : base(buildAssembler)
        {
            this.copyComponentFactories = copyComponentFactories;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create the index cache
        /// </summary>
        /// <param name="configuration">The index configuration</param>
        /// <returns>An instance of an <see cref="IndexedCache"/> derived class</returns>
        protected virtual IndexedCache CreateIndex(XPathNavigator configuration)
        {
            return new InMemoryIndexedCache(this, this.Context, configuration);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            MessageLevel level;
            bool isAttribute, ignoreCase;

            // Set up the context
            XPathNodeIterator contextNodes = configuration.Select("context");

            foreach(XPathNavigator contextNode in contextNodes)
            {
                contextNamespaces[contextNode.GetAttribute("prefix", String.Empty)] =
                    contextNode.GetAttribute("name", String.Empty);
            }

            // This is only used by the indices and won't change
            this.Context = new CustomContext(contextNamespaces);

            // Set up the indices
            XPathNodeIterator indexNodes = configuration.Select("index");

            foreach(XPathNavigator indexNode in indexNodes)
            {
                // Create the index
                IndexedCache index = this.CreateIndex(indexNode);
#if DEBUG
                this.WriteMessage(MessageLevel.Diagnostic, "Loading {0} index", index.Name);

                DateTime startLoad = DateTime.Now;
#endif
                // Search the data directories for entries
                XPathNodeIterator dataNodes = indexNode.Select("data");

                foreach(XPathNavigator dataNode in dataNodes)
                    index.AddDocuments(dataNode);

                // Getting the count from a database cache can be expensive so only report it if it will be seen
                if(this.BuildAssembler.VerbosityLevel == MessageLevel.Info)
                    this.WriteMessage(MessageLevel.Info, "Indexed {0} elements", index.Count);
#if DEBUG
                TimeSpan loadTime = (DateTime.Now - startLoad);
                this.WriteMessage(MessageLevel.Diagnostic, "Load time: {0} seconds", loadTime.TotalSeconds);
#endif
                this.BuildAssembler.Data.Add(index.Name, index);
            }

            // Get the copy commands
            XPathNodeIterator copyNodes = configuration.Select("copy");

            foreach(XPathNavigator copyNode in copyNodes)
            {
                string sourceName = copyNode.GetAttribute("name", String.Empty);

                if(String.IsNullOrWhiteSpace(sourceName))
                    this.WriteMessage(MessageLevel.Error, "Each copy command must specify an index to copy from");

                string keyXPath = copyNode.GetAttribute("key", String.Empty);

                if(String.IsNullOrWhiteSpace(keyXPath))
                    keyXPath = "string($key)";

                string sourceXPath = copyNode.GetAttribute("source", String.Empty);

                if(String.IsNullOrWhiteSpace(sourceXPath))
                    this.WriteMessage(MessageLevel.Error, "When instantiating a CopyFromIndex component, you " +
                        "must specify a source XPath format using the source attribute");

                string targetXPath = copyNode.GetAttribute("target", String.Empty);

                if(String.IsNullOrWhiteSpace(targetXPath))
                    this.WriteMessage(MessageLevel.Error, "When instantiating a CopyFromIndex component, you " +
                        "must specify a target XPath format using the target attribute");

                isAttribute = ignoreCase = false;

                string boolValue = copyNode.GetAttribute("attribute", String.Empty);

                if(!String.IsNullOrWhiteSpace(boolValue) && !Boolean.TryParse(boolValue, out isAttribute))
                    this.WriteMessage(MessageLevel.Error, "The 'attribute' attribute value is not a valid Boolean");

                boolValue = copyNode.GetAttribute("ignoreCase", String.Empty);

                if(!String.IsNullOrWhiteSpace(boolValue) && !Boolean.TryParse(boolValue, out ignoreCase))
                    this.WriteMessage(MessageLevel.Error, "The ignoreCase attribute value is not a valid Boolean");

                IndexedCache index = (IndexedCache)this.BuildAssembler.Data[sourceName];

                CopyFromIndexCommand copyCommand = new(this, index, keyXPath, sourceXPath,
                    targetXPath, isAttribute, ignoreCase);

                string messageLevel = copyNode.GetAttribute("missing-entry", String.Empty);

                if(!String.IsNullOrWhiteSpace(messageLevel))
                {
                    if(Enum.TryParse(messageLevel, true, out level))
                        copyCommand.MissingEntry = level;
                    else
                        this.WriteMessage(MessageLevel.Error, "'{0}' is not a message level.", messageLevel);
                }

                messageLevel = copyNode.GetAttribute("missing-source", String.Empty);

                if(!String.IsNullOrWhiteSpace(messageLevel))
                {
                    if(Enum.TryParse(messageLevel, true, out level))
                        copyCommand.MissingSource = level;
                    else
                        this.WriteMessage(MessageLevel.Error, "'{0}' is not a message level.", messageLevel);
                }

                messageLevel = copyNode.GetAttribute("missing-target", String.Empty);

                if(!String.IsNullOrWhiteSpace(messageLevel))
                {
                    if(Enum.TryParse(messageLevel, true, out level))
                        copyCommand.MissingTarget = level;
                    else
                        this.WriteMessage(MessageLevel.Error, "'{0}' is not a message level.", messageLevel);
                }

                copyCommands.Add(copyCommand);
            }

            XPathNodeIterator componentNodes = configuration.Select("components/component");

            foreach(XPathNavigator componentNode in componentNodes)
            {
                // Get the ID of the copy component
                string id = componentNode.GetAttribute("id", String.Empty);

                if(String.IsNullOrWhiteSpace(id))
                    this.WriteMessage(MessageLevel.Error, "Each copy component element must have an id attribute");

                var copyComponentFactory = copyComponentFactories.FirstOrDefault(g => g.Metadata.Id == id);

                if(copyComponentFactory == null)
                    this.WriteMessage(MessageLevel.Error, "A copy component with the ID '{0}' could not be found", id);

                try
                {
                    var copyComponent = copyComponentFactory.Value.Create(this);

                    copyComponent.Initialize(componentNode.Clone(), this.BuildAssembler.Data);
                    components.Add(copyComponent);
                }
                catch(Exception ex)
                {
                    this.WriteMessage(MessageLevel.Error, "An error occurred while attempting to instantiate " +
                        "the '{0}' copy component. The error message is: {1}{2}", id, ex.Message,
                        ex.InnerException != null ? "\r\n" + ex.InnerException.Message : String.Empty);
                }
            }

            if(components.Count != 0)
                this.WriteMessage(MessageLevel.Info, "Loaded {0} copy components", components.Count);
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Set the key in the XPath context
            var context = new CustomContext(contextNamespaces);
            context["key"] = key;

            // Perform each copy command
            foreach(CopyFromIndexCommand copyCommand in copyCommands)
                copyCommand.Apply(document, context);

            // Apply changes for each sub-component, if any
            foreach(CopyComponentCore component in components)
                component.Apply(document, key);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // Find and dispose of the index caches we own
            if(disposing)
            {
                foreach(var cache in this.BuildAssembler.Data.Values.OfType<IndexedCache>().Where(
                  c => c.Component == this))
                {
                    cache.ReportCacheStatistics();
                    cache.Dispose();
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
