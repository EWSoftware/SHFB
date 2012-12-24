// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;

// still have problems with spaces

namespace Microsoft.Ddue.Tools
{

    public class SharedContentComponent : BuildComponent
    {

        public SharedContentComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {

            // get context
            context = GetContext(configuration);

            // get the tags to be resolved
            XPathNodeIterator resolve_nodes = configuration.Select("replace");
            foreach(XPathNavigator resolve_node in resolve_nodes)
            {
                string path = resolve_node.GetAttribute("elements", String.Empty);
                if(String.IsNullOrEmpty(path))
                    path = "//(include|includeAttribute)";
                // if (String.IsNullOrEmpty(path)) WriteMessage(MessageLevel.Error, "Each resolve element must contain a path attribute specifying an XPath expression for shared content elements.");
                try
                {
                    XPathExpression path_expresion = XPathExpression.Compile(path, context);
                }
                catch(XPathException)
                {
                    WriteMessage(MessageLevel.Error, "The elements expression '{0}' is not a valid XPath.", path);
                }

                string item = resolve_node.GetAttribute("item", String.Empty);
                if(String.IsNullOrEmpty(item))
                    item = "string(@item)";
                try
                {
                    XPathExpression item_expression = XPathExpression.Compile(item, context);
                }
                catch(XPathException)
                {
                    WriteMessage(MessageLevel.Error, "The item expression '{0}' is not a valid XPath.", item);
                }

                string parameters = resolve_node.GetAttribute("parameters", String.Empty);
                if(String.IsNullOrEmpty(parameters))
                    parameters = "parameter";

                string attribute = resolve_node.GetAttribute("attribute", String.Empty);
                if(String.IsNullOrEmpty(attribute))
                    attribute = "string(@name)";

                elements.Add(new SharedContentElement(path, item, parameters, attribute, context));
            }

            if(elements.Count == 0)
                elements.Add(new SharedContentElement(@"//include | //includeAttribute", "string(@item)", "parameter", "string(@name)", context));

            // get the source and target formats
            XPathNodeIterator content_nodes = configuration.Select("content");
            foreach(XPathNavigator content_node in content_nodes)
            {
                // get the files				
                string sharedContentFiles = content_node.GetAttribute("file", String.Empty);
                if(String.IsNullOrEmpty(sharedContentFiles))
                    WriteMessage(MessageLevel.Error, "The content/@file attribute must specify a path.");
                ParseDocuments(sharedContentFiles);
            }

            WriteMessage(MessageLevel.Info, "Loaded {0} shared content items.", content.Count);
        }

        public void ParseDocuments(string wildcardPath)
        {
            string sharedContentFiles = Environment.ExpandEnvironmentVariables(wildcardPath);
            if(String.IsNullOrEmpty(sharedContentFiles))
                WriteMessage(MessageLevel.Error, "The content/@file attribute specifies an empty string.");

            WriteMessage(MessageLevel.Info, "Searching for files that match '{0}'.", sharedContentFiles);
            string directoryPart = Path.GetDirectoryName(sharedContentFiles);
            if(String.IsNullOrEmpty(directoryPart))
                directoryPart = Environment.CurrentDirectory;
            directoryPart = Path.GetFullPath(directoryPart);
            string filePart = Path.GetFileName(sharedContentFiles);
            string[] files = Directory.GetFiles(directoryPart, filePart);
            foreach(string file in files)
                LoadContent(file);
            WriteMessage(MessageLevel.Info, "Found {0} files in {1}.", files.Length, sharedContentFiles);
        }

        private void LoadContent(string file)
        {

            WriteMessage(MessageLevel.Info, "Loading shared content file '{0}'.", file);

            try
            {
                XmlReader reader = XmlReader.Create(file);

                try
                {
                    reader.MoveToContent();
                    while(!reader.EOF)
                    {

                        if((reader.NodeType == XmlNodeType.Element) && (reader.Name == "item"))
                        {

                            string key = reader.GetAttribute("id").ToLowerInvariant();
                            string value = reader.ReadInnerXml();

                            if(content.ContainsKey(key))
                                WriteMessage(MessageLevel.Info, "Overriding shared content item '{0}' with value in file '{1}'.", key, file);
                            content[key] = value;
                            // content.Add(key, value);
                        }
                        else
                        {
                            reader.Read();
                        }

                    }
                }
                finally
                {
                    reader.Close();
                }

            }
            catch(IOException e)
            {
                WriteMessage(MessageLevel.Error, "The shared content file '{0}' could not be opened. The " +
                    "error message is: {1}", file, e.GetExceptionMessage());
            }
            catch(XmlException e)
            {
                WriteMessage(MessageLevel.Error, "The shared content file '{0}' is not well-formed. The " +
                    "error message is: {1}", file, e.GetExceptionMessage());
            }
            catch(XmlSchemaException e)
            {
                WriteMessage(MessageLevel.Error, "The shared content file '{0}' is not valid. The error " +
                    "message is: {1}", file, e.GetExceptionMessage());
            }
        }

        // Stored data

        // The context
        private CustomContext context = new CustomContext();

        // The shared content items
        private Dictionary<string, string> content = new Dictionary<string, string>();

        // THe shared content elements
        private List<SharedContentElement> elements = new List<SharedContentElement>();

        public override void Apply(XmlDocument document, string key)
        {
            ResolveContent(document);
        }

        // private XPathExpression expression = XPathExpression.Compile("//include | //includeAttribute");

        private void ResolveContent(XmlDocument document)
        {
            ResolveContent(document, document.CreateNavigator());
        }

        private void ResolveContent(XmlDocument document, XPathNavigator start)
        {

            // for each kind of shared content element
            foreach(SharedContentElement element in elements)
            {

                // find all such elements
                XPathNodeIterator nodeIterator = start.Select(element.Path);

                // convert to an array so as not to cause an error when manipulating the document
                XPathNavigator[] nodes = ConvertIteratorToArray(nodeIterator);

                // process each element
                foreach(XPathNavigator node in nodes)
                {
                    // get the key
                    string item = node.Evaluate(element.Item).ToString().ToLowerInvariant();

                    // check for missing key
                    if(String.IsNullOrEmpty(item))
                    {
                        WriteMessage(MessageLevel.Warn, "A shared content element did not specify an item.");
                    }
                    else
                    {
                        // extract parameters
                        List<string> parameters = new List<string>();

                        XPathNodeIterator parameter_nodes = node.Select(element.Parameters);

                        foreach(XPathNavigator parameter_node in parameter_nodes)
                            parameters.Add(parameter_node.GetInnerXml());

                        // get the content
                        string content = GetContent(item, parameters.ToArray());

                        // check for missing content
                        if(content == null)
                            WriteMessage(MessageLevel.Warn, "Missing shared content item. Tag:'{0}'; Id:'{1}'.", node.LocalName, item);
                        else
                        {
                            // store the content in a document fragment
                            XmlDocumentFragment fragment = document.CreateDocumentFragment();
                            fragment.InnerXml = content;

                            // resolve any shared content in the fragment
                            ResolveContent(document, fragment.CreateNavigator());

                            // look for an attribute name
                            string attribute = node.Evaluate(element.Attribute).ToString();

                            // insert the resolved content
                            if(String.IsNullOrEmpty(attribute))
                            {
                                // as mixed content
                                // node.InsertAfter(resolvedContent);
                                XmlWriter writer = node.InsertAfter();
                                fragment.WriteTo(writer);
                                writer.Close();
                            }
                            else
                            {
                                // as an attribute
                                XPathNavigator parent = node.CreateNavigator();
                                parent.MoveToParent();
                                parent.CreateAttribute(String.Empty, attribute, String.Empty, fragment.InnerText);
                            }
                        }
                    }

                    // keep a reference to the parent element
                    XPathNavigator parentElement = node.CreateNavigator();
                    parentElement.MoveToParent();

                    // remove the node
                    node.DeleteSelf();

                    // if there is no content left in the parent element, make sure it is self-closing
                    if(!parentElement.HasChildren && !parentElement.IsEmptyElement)
                    {

                        //If 'node' was already the root then we will have a blank node now and 
                        //doing an InsertAfter() will throw an exception.
                        if(parentElement.Name.Length > 0)
                        {
                            // create a new element
                            XmlWriter attributeWriter = parentElement.InsertAfter();
                            attributeWriter.WriteStartElement(parentElement.Prefix, parentElement.LocalName, parentElement.NamespaceURI);

                            // copy attributes to it
                            XmlReader attributeReader = parentElement.ReadSubtree();
                            attributeReader.Read();
                            attributeWriter.WriteAttributes(attributeReader, false);
                            attributeReader.Close();

                            // close it
                            attributeWriter.WriteEndElement();
                            attributeWriter.Close();

                            // delete the old element
                            parentElement.DeleteSelf();
                        }
                        else
                        {
                            //if we are inside a tag such as title, removing the content will make it in the
                            //form of <title /> which is not allowed in html. 
                            //Since this usually means there is a problem with the shared content or the transforms
                            //leading up to this we will just report the error here.
                            WriteMessage(MessageLevel.Error, "Error replacing item.");
                        }
                    }
                }
            }
        }

        // look up shared content
        private string GetContent(string key, string[] parameters)
        {
            string value;

            if(content.TryGetValue(key, out value))
            {
                try
                {
                    value = String.Format(CultureInfo.InvariantCulture, value, parameters);
                }
                catch(FormatException)
                {
                    WriteMessage(MessageLevel.Error, "The shared content item '{0}' could not be formatted with {1} parameters.", key, parameters.Length);
                }

                return (value);
            }
            else
            {
                return (null);
            }

        }

        private static XPathNavigator[] ConvertIteratorToArray(XPathNodeIterator iterator)
        {
            XPathNavigator[] result = new XPathNavigator[iterator.Count];
            for(int i = 0; i < result.Length; i++)
            {
                iterator.MoveNext();
                result[i] = iterator.Current.Clone();
            }
            return (result);
        }

        private static CustomContext GetContext(XPathNavigator configuration)
        {

            CustomContext context = new CustomContext();

            XPathNodeIterator context_nodes = configuration.Select("context");
            foreach(XPathNavigator context_node in context_nodes)
            {
                string prefix = context_node.GetAttribute("prefix", String.Empty);
                string name = context_node.GetAttribute("name", String.Empty);
                context.AddNamespace(prefix, name);
            }

            return (context);

        }

    }

    internal class SharedContentElement
    {

        public SharedContentElement(string path, string item, string parameters, string attribute, IXmlNamespaceResolver context)
        {
            this.path = XPathExpression.Compile(path, context);
            this.item = XPathExpression.Compile(item, context);
            this.parameters = XPathExpression.Compile(parameters, context);
            this.attribute = XPathExpression.Compile(attribute, context);
        }

        private XPathExpression path;

        private XPathExpression item;

        private XPathExpression parameters;

        private XPathExpression attribute;

        public XPathExpression Path
        {
            get
            {
                return (path);
            }
        }

        public XPathExpression Item
        {
            get
            {
                return (item);
            }
        }

        public XPathExpression Parameters
        {
            get
            {
                return (parameters);
            }
        }

        public XPathExpression Attribute
        {
            get
            {
                return (attribute);
            }
        }
    }
}
