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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Commands;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
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
                return new CopyFromIndexComponent(base.BuildAssembler, this.CopyComponents);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        // List of copy components
        private List<Lazy<ICopyComponentFactory, ICopyComponentMetadata>> copyComponentFactories;
        private List<CopyComponentCore> components = new List<CopyComponentCore>();

        // What to copy
        private List<CopyFromIndexCommand> copyCommands = new List<CopyFromIndexCommand>();

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the context to use for the index when evaluating XPath expressions
        /// </summary>
        protected CustomContext Context { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        /// <param name="copyComponentFactories">The list of available copy component factory components</param>
        protected CopyFromIndexComponent(BuildAssemblerCore buildAssembler,
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
            MessageLevel level;
            bool isAttribute, ignoreCase;

            // Set up the context
            this.Context = new CustomContext();

            XPathNodeIterator contextNodes = configuration.Select("context");

            foreach(XPathNavigator contextNode in contextNodes)
                this.Context.AddNamespace(contextNode.GetAttribute("prefix", String.Empty),
                    contextNode.GetAttribute("name", String.Empty));

            // Set up the indices
            XPathNodeIterator indexNodes = configuration.Select("index");

            foreach(XPathNavigator indexNode in indexNodes)
            {
                // Create the index
                IndexedCache index = this.CreateIndex(indexNode);
#if DEBUG
                base.WriteMessage(MessageLevel.Diagnostic, "Loading {0} index", index.Name);

                DateTime startLoad = DateTime.Now;
#endif
                // Search the data directories for entries
                XPathNodeIterator dataNodes = indexNode.Select("data");

                foreach(XPathNavigator dataNode in dataNodes)
                    index.AddDocuments(dataNode);

                // Getting the count from a database cache can be expensive so only report it if it will be seen
                if(base.BuildAssembler.VerbosityLevel == MessageLevel.Info)
                    base.WriteMessage(MessageLevel.Info, "Indexed {0} elements", index.Count);
#if DEBUG
                TimeSpan loadTime = (DateTime.Now - startLoad);
                base.WriteMessage(MessageLevel.Diagnostic, "Load time: {0} seconds", loadTime.TotalSeconds);
#endif
                BuildComponentCore.Data.Add(index.Name, index);
            }

            // Get the copy commands
            XPathNodeIterator copyNodes = configuration.Select("copy");

            foreach(XPathNavigator copyNode in copyNodes)
            {
                string sourceName = copyNode.GetAttribute("name", String.Empty);

                if(String.IsNullOrWhiteSpace(sourceName))
                    base.WriteMessage(MessageLevel.Error, "Each copy command must specify an index to copy from");

                string keyXPath = copyNode.GetAttribute("key", String.Empty);

                if(String.IsNullOrWhiteSpace(keyXPath))
                    keyXPath = "string($key)";

                string sourceXPath = copyNode.GetAttribute("source", String.Empty);

                if(String.IsNullOrWhiteSpace(sourceXPath))
                    base.WriteMessage(MessageLevel.Error, "When instantiating a CopyFromIndex component, you " +
                        "must specify a source XPath format using the source attribute");

                string targetXPath = copyNode.GetAttribute("target", String.Empty);

                if(String.IsNullOrWhiteSpace(targetXPath))
                    base.WriteMessage(MessageLevel.Error, "When instantiating a CopyFromIndex component, you " +
                        "must specify a target XPath format using the target attribute");

                isAttribute = ignoreCase = false;

                string boolValue = copyNode.GetAttribute("attribute", String.Empty);

                if(!String.IsNullOrWhiteSpace(boolValue) && !Boolean.TryParse(boolValue, out isAttribute))
                    base.WriteMessage(MessageLevel.Error, "The 'attribute' attribute value is not a valid Boolean");

                boolValue = copyNode.GetAttribute("ignoreCase", String.Empty);

                if(!String.IsNullOrWhiteSpace(boolValue) && !Boolean.TryParse(boolValue, out ignoreCase))
                    base.WriteMessage(MessageLevel.Error, "The ignoreCase attribute value is not a valid Boolean");

                IndexedCache index = (IndexedCache)BuildComponentCore.Data[sourceName];

                CopyFromIndexCommand copyCommand = new CopyFromIndexCommand(this, index, keyXPath, sourceXPath,
                    targetXPath, isAttribute, ignoreCase);

                string messageLevel = copyNode.GetAttribute("missing-entry", String.Empty);

                if(!String.IsNullOrWhiteSpace(messageLevel))
                    if(Enum.TryParse<MessageLevel>(messageLevel, true, out level))
                        copyCommand.MissingEntry = level;
                    else
                        base.WriteMessage(MessageLevel.Error, "'{0}' is not a message level.", messageLevel);

                messageLevel = copyNode.GetAttribute("missing-source", String.Empty);

                if(!String.IsNullOrWhiteSpace(messageLevel))
                    if(Enum.TryParse<MessageLevel>(messageLevel, true, out level))
                        copyCommand.MissingSource = level;
                    else
                        base.WriteMessage(MessageLevel.Error, "'{0}' is not a message level.", messageLevel);

                messageLevel = copyNode.GetAttribute("missing-target", String.Empty);

                if(!String.IsNullOrWhiteSpace(messageLevel))
                    if(Enum.TryParse<MessageLevel>(messageLevel, true, out level))
                        copyCommand.MissingTarget = level;
                    else
                        base.WriteMessage(MessageLevel.Error, "'{0}' is not a message level.", messageLevel);

                copyCommands.Add(copyCommand);
            }

            XPathNodeIterator componentNodes = configuration.Select("components/component");

            foreach(XPathNavigator componentNode in componentNodes)
            {
                // Get the ID of the copy component
                string id = componentNode.GetAttribute("id", String.Empty);

                if(String.IsNullOrWhiteSpace(id))
                    base.WriteMessage(MessageLevel.Error, "Each copy component element must have an id attribute");

                var copyComponentFactory = copyComponentFactories.FirstOrDefault(g => g.Metadata.Id == id);

                if(copyComponentFactory == null)
                    base.WriteMessage(MessageLevel.Error, "A copy component with the ID '{0}' could not be found", id);

                try
                {
                    var copyComponent = copyComponentFactory.Value.Create(this);

                    copyComponent.Initialize(componentNode.Clone(), BuildComponentCore.Data);
                    components.Add(copyComponent);
                }
                catch(Exception ex)
                {
                    base.WriteMessage(MessageLevel.Error, "An error occurred while attempting to instantiate " +
                        "the '{0}' copy component. The error message is: {1}{2}", id, ex.Message,
                        ex.InnerException != null ? "\r\n" + ex.InnerException.Message : String.Empty);
                }
            }

            if(components.Count != 0)
                base.WriteMessage(MessageLevel.Info, "Loaded {0} copy components", components.Count);
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Set the key in the XPath context
            this.Context["key"] = key;

            // Perform each copy command
            foreach(CopyFromIndexCommand copyCommand in copyCommands)
                copyCommand.Apply(document, this.Context);

            // Apply changes for each sub-component, if any
            foreach(CopyComponentCore component in components)
                component.Apply(document, key);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // Find and dispose of the index caches we own
            if(disposing)
                foreach(var cache in BuildComponentCore.Data.Values.OfType<IndexedCache>().Where(
                  c => c.Component == this))
                {
                    cache.ReportCacheStatistics();
                    cache.Dispose();
                }

            base.Dispose(disposing);
        }
        #endregion
    }
}
