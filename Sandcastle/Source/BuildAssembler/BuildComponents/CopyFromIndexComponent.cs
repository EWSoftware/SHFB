// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/20/2012 - EFW - Reworked the index cache and moved those classes to the Commands namespace and put them
// in their own files.
// 01/24/2012 - EFW - Added a virtual method to create the index caches, added code to dispose of them when done,
// and exposed the context via a protected property.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Commands;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component copies elements from an indexed set of XML files into the target document based on
    /// one or more copy commands that define the elements to copy and where to put them.
    /// </summary>
    public class CopyFromIndexComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        // List of copy components
        private List<CopyComponent> components = new List<CopyComponent>();

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
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public CopyFromIndexComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
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
                BuildComponent.Data.Add(index.Name, index);
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

                IndexedCache index = (IndexedCache)BuildComponent.Data[sourceName];

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
                // Get the data to load the component
                string assemblyPath = componentNode.GetAttribute("assembly", String.Empty);

                if(String.IsNullOrWhiteSpace(assemblyPath))
                    base.WriteMessage(MessageLevel.Error, "Each component element must have an assembly attribute.");

                string typeName = componentNode.GetAttribute("type", String.Empty);

                if(String.IsNullOrWhiteSpace(typeName))
                    base.WriteMessage(MessageLevel.Error, "Each component element must have a type attribute.");

                // expand environment variables in the path
                assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);

                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);

                    CopyComponent component = (CopyComponent)assembly.CreateInstance(typeName, false,
                        BindingFlags.Public | BindingFlags.Instance, null,
                        new object[3] { this, componentNode.Clone(), BuildComponent.Data }, null, null);

                    if(component == null)
                        base.WriteMessage(MessageLevel.Error, "The type '{0}' does not exist in the assembly '{1}'",
                            typeName, assemblyPath);
                    else
                        components.Add(component);
                }
                catch(IOException e)
                {
                    base.WriteMessage(MessageLevel.Error, "A file access error occured while attempting to " +
                        "load the build component '{0}'. The error message is: {1}", assemblyPath, e.Message);
                }
                catch(BadImageFormatException e)
                {
                    base.WriteMessage(MessageLevel.Error, "A syntax generator assembly '{0}' is invalid. The " +
                        "error message is: {1}.", assemblyPath, e.Message);
                }
                catch(TypeLoadException e)
                {
                    base.WriteMessage(MessageLevel.Error, "The type '{0}' does not exist in the assembly " +
                        "'{1}'. The error message is: {2}", typeName, assemblyPath, e.Message);
                }
                catch(MissingMethodException e)
                {
                    base.WriteMessage(MessageLevel.Error, "The type '{0}' in the assembly '{1}' does not have " +
                        "an appropriate constructor. The error message is: {2}", typeName, assemblyPath, e.Message);
                }
                catch(TargetInvocationException e)
                {
                    base.WriteMessage(MessageLevel.Error, "An error occured while attempting to instantiate " +
                        "the type '{0}' in the assembly '{1}'. The error message is: {2}", typeName, assemblyPath,
                        e.InnerException.Message);
                }
                catch(InvalidCastException)
                {
                    base.WriteMessage(MessageLevel.Error, "The type '{0}' in the assembly '{1}' is not a " +
                        "CopyComponent", typeName, assemblyPath);
                }
            }

            if(components.Count != 0)
                base.WriteMessage(MessageLevel.Info, "Loaded {0} copy components", components.Count);
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
        public override void Apply(XmlDocument document, string key)
        {
            // Set the key in the XPath context
            this.Context["key"] = key;

            // Perform each copy command
            foreach(CopyFromIndexCommand copyCommand in copyCommands)
                copyCommand.Apply(document, this.Context);

            // Apply changes for each sub-component, if any
            foreach(CopyComponent component in components)
                component.Apply(document, key);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // Find and dispose of the index caches we own
            if(disposing)
                foreach(var cache in BuildComponent.Data.Values.OfType<IndexedCache>().Where(
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
