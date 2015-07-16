// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/24/2012 - EFW - Moved SharedContentElement into its own file and inlined a couple of methods.  Looked into
// sharing the common content across all instances with local instance overrides but it loads fast and it
// typically loads under 60KB of data per instance so it's not worth the extra overhead.
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;

using Microsoft.Ddue.Tools.Targets;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This build component is used to replace a given set of elements with the content of shared content items
    /// loaded from XML files.
    /// </summary>
    public class SharedContentComponent : BuildComponentCore
    {
        #region Build component factory for MEF - Standard version
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Shared Content Component")]
        public sealed class DefaultFactory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SharedContentComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Build component factory for MEF - API token resolution version
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used for API token resolution
        /// </summary>
        [BuildComponentExport("API Token Resolution", IsVisible = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This build component is used to resolve tokens in XML comments files.")]
        public sealed class ApiTokenResolutionComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ApiTokenResolutionComponentFactory()
            {
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "Show Missing Documentation Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SharedContentComponent(base.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"{@TokenFiles}
<replace elements=""/*//token"" item=""string(.)"" />";
                }
            }

            /// <inheritdoc />
            /// <remarks>Indicate a dependency on the missing documentation component as that's the best
            /// placement if the IntelliSense component is used too.</remarks>
            public override IEnumerable<string> Dependencies
            {
                get
                {
                    return new List<string> { "Show Missing Documentation Component" };
                }
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private CustomContext context;
        private Dictionary<string, string> content;
        private List<SharedContentElement> elements;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected SharedContentComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            // Get the context.  This will contain namespaces that prefix the elements to find.
            context = new CustomContext();

            XPathNodeIterator contextNodes = configuration.Select("context");

            foreach(XPathNavigator cn in contextNodes)
                context.AddNamespace(cn.GetAttribute("prefix", String.Empty),
                    cn.GetAttribute("name", String.Empty));

            // Item keys are compared case-insensitively
            content = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            elements = new List<SharedContentElement>();

            // Get the elements to be resolved
            XPathNodeIterator resolve_nodes = configuration.Select("replace");

            foreach(XPathNavigator resolve_node in resolve_nodes)
            {
                // Get the XPath expression used to find the elements to replace
                string path = resolve_node.GetAttribute("elements", String.Empty);

                // If not defined, assume include and includeAttribute are to be replaced
                if(String.IsNullOrEmpty(path))
                    path = "//include | //includeAttribute";

                try
                {
                    XPathExpression path_expresion = XPathExpression.Compile(path, context);
                }
                catch(XPathException)
                {
                    base.WriteMessage(MessageLevel.Error, "The elements expression '{0}' is not a valid XPath",
                        path);
                }

                // Get the XPath expression used to get the item name to insert
                string item = resolve_node.GetAttribute("item", String.Empty);

                if(String.IsNullOrEmpty(item))
                    item = "string(@item)";
                try
                {
                    XPathExpression item_expression = XPathExpression.Compile(item, context);
                }
                catch(XPathException)
                {
                    base.WriteMessage(MessageLevel.Error, "The item expression '{0}' is not a valid XPath", item);
                }

                // Get the XPath expression used to find parameter elements
                string parameters = resolve_node.GetAttribute("parameters", String.Empty);

                if(String.IsNullOrEmpty(parameters))
                    parameters = "parameter";

                // Get the XPath expression used to get the attribute name for attribute items
                string attribute = resolve_node.GetAttribute("attribute", String.Empty);

                if(String.IsNullOrEmpty(attribute))
                    attribute = "string(@name)";

                elements.Add(new SharedContentElement(path, item, parameters, attribute, context));
            }

            // If not defined, assume include and includeAttribute are to be replaced using the default names
            if(elements.Count == 0)
                elements.Add(new SharedContentElement("//include | //includeAttribute", "string(@item)",
                    "parameter", "string(@name)", context));

            // Load the content item files
            XPathNodeIterator content_nodes = configuration.Select("content");

            foreach(XPathNavigator content_node in content_nodes)
            {
                string sharedContentFiles = content_node.GetAttribute("file", String.Empty);

                if(String.IsNullOrEmpty(sharedContentFiles))
                    base.WriteMessage(MessageLevel.Error, "The content/@file attribute must specify a path.");

                this.ParseDocuments(sharedContentFiles);
            }

            base.WriteMessage(MessageLevel.Info, "Loaded {0} shared content items.", content.Count);
        }

        /// <summary>
        /// Search for elements to replace and insert the shared content in their place
        /// </summary>
        /// <param name="document">The document in which to replace the elements</param>
        /// <param name="key">The document key</param>
        /// <remarks>Shared content items are replaced recursively</remarks>
        public override void Apply(XmlDocument document, string key)
        {
            this.ResolveContent(key, document, document.CreateNavigator());
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Find content files using the given wildcard and load all of the content items in them
        /// </summary>
        /// <param name="wildcardPath">The wildcard path used to locate content item files</param>
        public void ParseDocuments(string wildcardPath)
        {
            string sharedContentFiles = Environment.ExpandEnvironmentVariables(wildcardPath);

            if(String.IsNullOrWhiteSpace(sharedContentFiles))
                base.WriteMessage(MessageLevel.Error, "The content/@file attribute specifies an empty string.");

            base.WriteMessage(MessageLevel.Info, "Searching for files that match '{0}'.", sharedContentFiles);

            string directoryPart = Path.GetDirectoryName(sharedContentFiles);

            if(String.IsNullOrEmpty(directoryPart))
                directoryPart = Environment.CurrentDirectory;

            directoryPart = Path.GetFullPath(directoryPart);

            string filePart = Path.GetFileName(sharedContentFiles);
            string[] files = Directory.GetFiles(directoryPart, filePart);

            foreach(string file in files)
                this.LoadContent(file);

            base.WriteMessage(MessageLevel.Info, "Found {0} files in {1}.", files.Length, sharedContentFiles);
        }

        /// <summary>
        /// Load all shared content items from the specified file
        /// </summary>
        /// <param name="file">The shared content file to load</param>
        private void LoadContent(string file)
        {
            base.WriteMessage(MessageLevel.Info, "Loading shared content file '{0}'.", file);

            try
            {
                using(XmlReader reader = XmlReader.Create(file))
                {
                    reader.MoveToContent();

                    while(!reader.EOF)
                        if(reader.NodeType == XmlNodeType.Element && reader.Name == "item")
                        {
                            string key = reader.GetAttribute("id");
                            string value = reader.ReadInnerXml();

                            if(content.ContainsKey(key))
                                base.WriteMessage(MessageLevel.Info, "Overriding shared content item '{0}' " +
                                    "with value in file '{1}'.", key, file);

                            content[key] = value;
                        }
                        else
                            reader.Read();
                }
            }
            catch(IOException e)
            {
                base.WriteMessage(MessageLevel.Error, "The shared content file '{0}' could not be opened. The " +
                    "error message is: {1}", file, e.GetExceptionMessage());
            }
            catch(XmlException e)
            {
                base.WriteMessage(MessageLevel.Error, "The shared content file '{0}' is not well-formed. The " +
                    "error message is: {1}", file, e.GetExceptionMessage());
            }
            catch(XmlSchemaException e)
            {
                base.WriteMessage(MessageLevel.Error, "The shared content file '{0}' is not valid. The error " +
                    "message is: {1}", file, e.GetExceptionMessage());
            }
        }

        /// <summary>
        /// Look up the shared content elements, find their corresponding shared content item and replace the
        /// elements with the content item value.
        /// </summary>
        /// <param name="key">The document key</param>
        /// <param name="document">The document containing the topic</param>
        /// <param name="start">The XPath navigator to search for content elements</param>
        /// <remarks>This method will replace content items within other content items recursively</remarks>
        private void ResolveContent(string key, XmlDocument document, XPathNavigator start)
        {
            List<string> parameters = new List<string>();

            // For each kind of shared content element...
            foreach(SharedContentElement element in elements)
            {
                // Find all such elements, convert to an array so as not to cause an error when manipulating the
                // document, and process each element.
                foreach(XPathNavigator node in start.Select(element.Path).ToArray())
                {
                    // Get the item key
                    string item = node.Evaluate(element.Item).ToString();

                    // Check for a missing item key
                    if(String.IsNullOrEmpty(item))
                        base.WriteMessage(key, MessageLevel.Warn, "A shared content element did not specify an item");
                    else
                    {
                        // Extract any parameters
                        parameters.Clear();

                        XPathNodeIterator parameterNodes = node.Select(element.Parameters);

                        foreach(XPathNavigator parameterNode in parameterNodes)
                            parameters.Add(parameterNode.GetInnerXml());

                        // Find the content item and format the parameters into the value
                        string contentValue = null;

                        if(content.TryGetValue(item, out contentValue))
                        {
                            try
                            {
                                contentValue = String.Format(CultureInfo.InvariantCulture, contentValue,
                                    parameters.ToArray());
                            }
                            catch(FormatException)
                            {
                                base.WriteMessage(key, MessageLevel.Error, "The shared content item '{0}' " +
                                    "could not be formatted with {1} parameters.", item, parameters.Count);
                            }
                        }

                        // Check for missing content
                        if(contentValue == null)
                        {
                            base.WriteMessage(key, MessageLevel.Warn, "Missing shared content item. Tag: " +
                                "'{0}'; Id:'{1}'.", node.LocalName, item);
                        }
                        else
                        {
                            // Store the content in a document fragment
                            XmlDocumentFragment fragment = document.CreateDocumentFragment();
                            fragment.InnerXml = contentValue;

                            // Resolve any shared content in the fragment
                            this.ResolveContent(key, document, fragment.CreateNavigator());

                            // Look for an attribute name
                            string attribute = node.Evaluate(element.Attribute).ToString();

                            // Insert the resolved content...
                            if(String.IsNullOrEmpty(attribute))
                            {
                                // ...as mixed content
                                XmlWriter writer = node.InsertAfter();
                                fragment.WriteTo(writer);
                                writer.Close();
                            }
                            else
                            {
                                // ...as an attribute
                                XPathNavigator parent = node.CreateNavigator();
                                parent.MoveToParent();
                                parent.CreateAttribute(String.Empty, attribute, String.Empty, fragment.InnerText);
                            }
                        }
                    }

                    // Keep a reference to the parent element
                    XPathNavigator parentElement = node.CreateNavigator();
                    parentElement.MoveToParent();

                    // Remove the node
                    node.DeleteSelf();

                    // If there is no content left in the parent element, make sure it is self-closing
                    if(!parentElement.HasChildren && !parentElement.IsEmptyElement)
                    {
                        // If the node was already the root then we will have a blank node now and 
                        // doing an InsertAfter() will throw an exception.
                        if(parentElement.Name.Length > 0)
                        {
                            // Create a new element
                            XmlWriter attributeWriter = parentElement.InsertAfter();
                            attributeWriter.WriteStartElement(parentElement.Prefix, parentElement.LocalName,
                                parentElement.NamespaceURI);

                            // Copy attributes to it
                            XmlReader attributeReader = parentElement.ReadSubtree();
                            attributeReader.Read();
                            attributeWriter.WriteAttributes(attributeReader, false);
                            attributeReader.Close();

                            // Close it
                            attributeWriter.WriteEndElement();
                            attributeWriter.Close();

                            // Delete the old element
                            parentElement.DeleteSelf();
                        }
                        else
                        {
                            // If we are inside a tag such as title, removing the content will leave it in the
                            // form "<title />" which is not allowed in HTML.  Since this usually means there is
                            // a problem with the shared content or the transforms leading up to this, we will
                            // just report the error here.
                            base.WriteMessage(key, MessageLevel.Error, "Error replacing item.  Root document " +
                                "element encountered.");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
