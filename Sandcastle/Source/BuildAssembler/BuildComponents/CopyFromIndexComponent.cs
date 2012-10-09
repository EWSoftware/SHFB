// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/27/2012 - EFW - Added support for suppressing duplicate ID warnings.  This prevents lots of unnecessary
// warnings about duplicate IDs in comments files.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;

namespace Microsoft.Ddue.Tools
{
    public class CopyFromIndexComponent : BuildComponent
    {
        // List of copy components
        private List<CopyComponent> components = new List<CopyComponent>();

        // what to copy
        private List<CopyCommand> copy_commands = new List<CopyCommand>();

        // a context in which to evaluate XPath expressions
        private CustomContext context = new CustomContext();

        // !EFW - Added to support supression of duplicate ID warnings in comment files
        /// <summary>
        /// This is used to get or set whether duplicate IDs in the index files result in a warning
        /// </summary>
        /// <value>The default is true to report warnings for duplicate ID values in the index files</value>
        public bool DuplicateIdWarnings { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public CopyFromIndexComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            this.DuplicateIdWarnings = true;

            // set up the context
            XPathNodeIterator context_nodes = configuration.Select("context");
            foreach(XPathNavigator context_node in context_nodes)
            {
                string prefix = context_node.GetAttribute("prefix", String.Empty);
                string name = context_node.GetAttribute("name", String.Empty);
                context.AddNamespace(prefix, name);
            }

            // set up the indices
            XPathNodeIterator index_nodes = configuration.Select("index");

            foreach(XPathNavigator index_node in index_nodes)
            {
                // get the name of the index
                string name = index_node.GetAttribute("name", String.Empty);
                if(String.IsNullOrEmpty(name))
                    throw new ConfigurationErrorsException("Each index must have a unique name.");

                // get the xpath for value nodes
                string value_xpath = index_node.GetAttribute("value", String.Empty);
                if(String.IsNullOrEmpty(value_xpath))
                    WriteMessage(MessageLevel.Error, "Each index element must have a value attribute containing an XPath that describes index entries.");

                // get the xpath for keys (relative to value nodes)
                string key_xpath = index_node.GetAttribute("key", String.Empty);
                if(String.IsNullOrEmpty(key_xpath))
                    WriteMessage(MessageLevel.Error, "Each index element must have a key attribute containing an XPath (relative to the value XPath) that evaluates to the entry key.");

                // get the cache size
                int cache = 10;
                string cache_value = index_node.GetAttribute("cache", String.Empty);
                if(!String.IsNullOrEmpty(cache_value))
                    cache = Convert.ToInt32(cache_value);

                // create the index
                IndexedDocumentCache index = new IndexedDocumentCache(this, key_xpath, value_xpath, context, cache);

                // search the data directories for entries
                XPathNodeIterator data_nodes = index_node.Select("data");
                foreach(XPathNavigator data_node in data_nodes)
                {
                    string base_value = data_node.GetAttribute("base", String.Empty);

                    if(!String.IsNullOrEmpty(base_value))
                        base_value = Environment.ExpandEnvironmentVariables(base_value);

                    bool recurse = false;
                    string recurse_value = data_node.GetAttribute("recurse", String.Empty);

                    if(!String.IsNullOrEmpty(recurse_value))
                        recurse = Convert.ToBoolean(recurse_value);

                    // !EFW - Added to support suppression of duplicate warnings on comment files
                    string duplicateWarning = data_node.GetAttribute("duplicateWarning", String.Empty);

                    if(!String.IsNullOrEmpty(duplicateWarning))
                        this.DuplicateIdWarnings = Convert.ToBoolean(duplicateWarning);

                    // get the files				
                    string files = data_node.GetAttribute("files", String.Empty);

                    if(String.IsNullOrEmpty(files))
                        WriteMessage(MessageLevel.Error, "Each data element must have a files attribute specifying which files to index.");

                    files = Environment.ExpandEnvironmentVariables(files);

                    if(!String.IsNullOrEmpty(base_value))
                        WriteMessage(MessageLevel.Info, String.Format("Searching for files that match '{0}' in '{1}'.", files, base_value));
                    else
                        WriteMessage(MessageLevel.Info, String.Format("Searching for files that match '{0}'.", files));

                    index.AddDocuments(base_value, files, recurse);
                }

                WriteMessage(MessageLevel.Info, String.Format("Indexed {0} elements in {1} files.", index.Count, index.DocumentCount));

                BuildComponent.Data.Add(name, index);
            }

            // get the copy commands
            XPathNodeIterator copy_nodes = configuration.Select("copy");

            foreach(XPathNavigator copy_node in copy_nodes)
            {

                string source_name = copy_node.GetAttribute("name", String.Empty);
                if(String.IsNullOrEmpty(source_name))
                    throw new ConfigurationErrorsException("Each copy command must specify an index to copy from.");

                string key_xpath = copy_node.GetAttribute("key", String.Empty);

                string source_xpath = copy_node.GetAttribute("source", String.Empty);
                if(String.IsNullOrEmpty(source_xpath))
                    throw new ConfigurationErrorsException("When instantiating a CopyFromDirectory component, you must specify a source xpath format using the source attribute.");

                string target_xpath = copy_node.GetAttribute("target", String.Empty);
                if(String.IsNullOrEmpty(target_xpath))
                    throw new ConfigurationErrorsException("When instantiating a CopyFromDirectory component, you must specify a target xpath format using the target attribute.");

                string attribute_value = copy_node.GetAttribute("attribute", String.Empty);

                string ignoreCase_value = copy_node.GetAttribute("ignoreCase", String.Empty);

                string missingEntryValue = copy_node.GetAttribute("missing-entry", String.Empty);
                string missingSourceValue = copy_node.GetAttribute("missing-source", String.Empty);
                string missingTargetValue = copy_node.GetAttribute("missing-target", String.Empty);

                IndexedDocumentCache index = (IndexedDocumentCache)Data[source_name];

                CopyCommand copyCommand = new CopyCommand(index, key_xpath, source_xpath, target_xpath, attribute_value, ignoreCase_value);
                
                if(!String.IsNullOrEmpty(missingEntryValue))
                {
                    try
                    {
                        copyCommand.MissingEntry = (MessageLevel)Enum.Parse(typeof(MessageLevel), missingEntryValue, true);
                    }
                    catch(ArgumentException)
                    {
                        WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a message level.", missingEntryValue));
                    }
                }
                
                if(!String.IsNullOrEmpty(missingSourceValue))
                {
                    try
                    {
                        copyCommand.MissingSource = (MessageLevel)Enum.Parse(typeof(MessageLevel), missingSourceValue, true);
                    }
                    catch(ArgumentException)
                    {
                        WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a message level.", missingSourceValue));
                    }
                }
                
                if(!String.IsNullOrEmpty(missingTargetValue))
                {
                    try
                    {
                        copyCommand.MissingTarget = (MessageLevel)Enum.Parse(typeof(MessageLevel), missingTargetValue, true);
                    }
                    catch(ArgumentException)
                    {
                        WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a message level.", missingTargetValue));
                    }
                }

                copy_commands.Add(copyCommand);


            }

            XPathNodeIterator component_nodes = configuration.Select("components/component");
            foreach(XPathNavigator component_node in component_nodes)
            {

                // get the data to load the component
                string assembly_path = component_node.GetAttribute("assembly", String.Empty);
                if(String.IsNullOrEmpty(assembly_path))
                    WriteMessage(MessageLevel.Error, "Each component element must have an assembly attribute.");
                string type_name = component_node.GetAttribute("type", String.Empty);
                if(String.IsNullOrEmpty(type_name))
                    WriteMessage(MessageLevel.Error, "Each component element must have a type attribute.");

                // expand environment variables in the path
                assembly_path = Environment.ExpandEnvironmentVariables(assembly_path);

                //Console.WriteLine("loading {0} from {1}", type_name, assembly_path);
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assembly_path);
                    CopyComponent component = (CopyComponent)assembly.CreateInstance(type_name, false, BindingFlags.Public | BindingFlags.Instance, null, new Object[2] { component_node.Clone(), Data }, null, null);

                    if(component == null)
                    {
                        WriteMessage(MessageLevel.Error, String.Format("The type '{0}' does not exist in the assembly '{1}'.", type_name, assembly_path));
                    }
                    else
                    {
                        components.Add(component);
                    }

                }
                catch(IOException e)
                {
                    WriteMessage(MessageLevel.Error, String.Format("A file access error occured while attempting to load the build component '{0}'. The error message is: {1}", assembly_path, e.Message));
                }
                catch(BadImageFormatException e)
                {
                    WriteMessage(MessageLevel.Error, String.Format("A syntax generator assembly '{0}' is invalid. The error message is: {1}.", assembly_path, e.Message));
                }
                catch(TypeLoadException e)
                {
                    WriteMessage(MessageLevel.Error, String.Format("The type '{0}' does not exist in the assembly '{1}'. The error message is: {2}", type_name, assembly_path, e.Message));
                }
                catch(MissingMethodException e)
                {
                    WriteMessage(MessageLevel.Error, String.Format("The type '{0}' in the assembly '{1}' does not have an appropriate constructor. The error message is: {2}", type_name, assembly_path, e.Message));
                }
                catch(TargetInvocationException e)
                {
                    WriteMessage(MessageLevel.Error, String.Format("An error occured while attempting to instantiate the type '{0}' in the assembly '{1}'. The error message is: {2}", type_name, assembly_path, e.InnerException.Message));
                }
                catch(InvalidCastException)
                {
                    WriteMessage(MessageLevel.Error, String.Format("The type '{0}' in the assembly '{1}' is not a SyntaxGenerator.", type_name, assembly_path));
                }
            }

            WriteMessage(MessageLevel.Info, String.Format("Loaded {0} copy components.", components.Count));

        }

        // the actual work of the component

        public override void Apply(XmlDocument document, string key)
        {

            // set the key in the XPath context
            context["key"] = key;

            // perform each copy action
            foreach(CopyCommand copy_command in copy_commands)
            {

                // get the source comment
                XPathExpression key_expression = copy_command.Key.Clone();
                key_expression.SetContext(context);
                // Console.WriteLine(key_expression.Expression);
                string key_value = (string)document.CreateNavigator().Evaluate(key_expression);
                // Console.WriteLine("got key '{0}'", key_value);
                XPathNavigator data = copy_command.Index.GetContent(key_value);

                if(data == null && copy_command.IgnoreCase == "true")
                    data = copy_command.Index.GetContent(key_value.ToLower());

                // notify if no entry
                if(data == null)
                {
                    WriteMessage(copy_command.MissingEntry, String.Format("No index entry found for key '{0}'.", key_value));
                    continue;
                }

                // get the target node
                String target_xpath = copy_command.Target.Clone().ToString();
                XPathExpression target_expression = XPathExpression.Compile(string.Format(target_xpath, key_value));
                target_expression.SetContext(context);

                XPathNavigator target = document.CreateNavigator().SelectSingleNode(target_expression);

                // notify if no target found
                if(target == null)
                {
                    WriteMessage(copy_command.MissingTarget, String.Format("Target node '{0}' not found.", target_expression.Expression));
                    continue;
                }

                // get the source nodes
                XPathExpression source_expression = copy_command.Source.Clone();
                source_expression.SetContext(context);
                XPathNodeIterator sources = data.CreateNavigator().Select(source_expression);

                // append the source nodes to the target node
                int source_count = 0;
                foreach(XPathNavigator source in sources)
                {
                    source_count++;

                    // If attribute=true, add the source attributes to current target. 
                    // Otherwise append source as a child element to target
                    if(copy_command.Attribute == "true" && source.HasAttributes)
                    {
                        string source_name = source.LocalName;
                        XmlWriter attributes = target.CreateAttributes();

                        source.MoveToFirstAttribute();
                        string attrFirst = target.GetAttribute(string.Format("{0}_{1}", source_name, source.Name), string.Empty);
                        if(string.IsNullOrEmpty(attrFirst))
                            attributes.WriteAttributeString(string.Format("{0}_{1}", source_name, source.Name), source.Value);

                        while(source.MoveToNextAttribute())
                        {
                            string attrNext = target.GetAttribute(string.Format("{0}_{1}", source_name, source.Name), string.Empty);
                            if(string.IsNullOrEmpty(attrNext))
                                attributes.WriteAttributeString(string.Format("{0}_{1}", source_name, source.Name), source.Value);
                        }
                        attributes.Close();
                    }
                    else
                        target.AppendChild(source);
                }

                // notify if no source found
                if(source_count == 0)
                {
                    WriteMessage(copy_command.MissingSource, String.Format("Source node '{0}' not found.", source_expression.Expression));
                }

                foreach(CopyComponent component in components)
                {
                    component.Apply(document, key);
                }
            }
        }

        internal void WriteHelperMessage(MessageLevel level, string message)
        {
            WriteMessage(level, message);
        }

    }

    // the storage system

    public class IndexedDocumentCache
    {

        public IndexedDocumentCache(CopyFromIndexComponent component, string keyXPath, string valueXPath, XmlNamespaceManager context, int cacheSize)
        {

            if(component == null)
                throw new ArgumentNullException("component");
            if(cacheSize < 0)
                throw new ArgumentOutOfRangeException("cacheSize");

            this.component = component;

            try
            {
                keyExpression = XPathExpression.Compile(keyXPath);
            }
            catch(XPathException)
            {
                component.WriteHelperMessage(MessageLevel.Error, String.Format("The key expression '{0}' is not a valid XPath expression.", keyXPath));
            }
            keyExpression.SetContext(context);

            try
            {
                valueExpression = XPathExpression.Compile(valueXPath);
            }
            catch(XPathException)
            {
                component.WriteHelperMessage(MessageLevel.Error, String.Format("The value expression '{0}' is not a valid XPath expression.", valueXPath));
            }
            valueExpression.SetContext(context);

            this.cacheSize = cacheSize;

            // set up the cache
            cache = new Dictionary<string, IndexedDocument>(cacheSize);
            queue = new Queue<string>(cacheSize);
        }

        // index component to which the cache belongs
        private CopyFromIndexComponent component;

        public CopyFromIndexComponent Component
        {
            get
            {
                return (component);
            }
        }

        // search pattern for index values
        private XPathExpression valueExpression;

        public XPathExpression ValueExpression
        {
            get
            {
                return (valueExpression);
            }
        }

        // search pattern for the index keys (relative to the index value node)
        private XPathExpression keyExpression;

        public XPathExpression KeyExpression
        {
            get
            {
                return (keyExpression);
            }
        }

        // a index mapping keys to the files that contain them
        private Dictionary<string, string> index = new Dictionary<string, string>();

        public void AddDocument(string file)
        {
            // load the document
            IndexedDocument document = new IndexedDocument(this, file);

            // record the keys
            string[] keys = document.GetKeys();

            foreach(string key in keys)
            {
                // !EFW - Only report the warning if wanted
                if(index.ContainsKey(key) && component.DuplicateIdWarnings)
                    component.WriteHelperMessage(MessageLevel.Warn, String.Format("Entries for the key " +
                        "'{0}' occur in both '{1}' and '{2}'. The last entry will be used.", key,
                        index[key], file));

                index[key] = file;
            }
        }

        public void AddDocuments(string wildcardPath)
        {
            string directory_part = Path.GetDirectoryName(wildcardPath);
            if(String.IsNullOrEmpty(directory_part))
                directory_part = Environment.CurrentDirectory;
            directory_part = Path.GetFullPath(directory_part);
            string file_part = Path.GetFileName(wildcardPath);
            //Console.WriteLine("{0}::{1}", directory_part, file_part);
            string[] files = Directory.GetFiles(directory_part, file_part);
            foreach(string file in files)
            {
                AddDocument(file);
            }

            //Console.WriteLine(files.Length);
            documentCount += files.Length;
        }

        public void AddDocuments(string baseDirectory, string wildcardPath, bool recurse)
        {

            string path;
            if(String.IsNullOrEmpty(baseDirectory))
            {
                path = wildcardPath;
            }
            else
            {
                path = Path.Combine(baseDirectory, wildcardPath);
            }

            AddDocuments(path);

            if(recurse)
            {
                string[] subDirectories = Directory.GetDirectories(baseDirectory);
                foreach(string subDirectory in subDirectories)
                    AddDocuments(subDirectory, wildcardPath, recurse);
            }
        }

        private int documentCount;

        public int DocumentCount
        {
            get
            {
                return (documentCount);
            }
        }

        // a simple caching mechanism

        int cacheSize;

        // an improved cache

        // this cache keeps track of the order that files are loaded in, and always unloads the oldest one
        // this is better, but a document that is often accessed gets no "points", so it will eventualy be
        // thrown out even if it is used regularly

        private Dictionary<string, IndexedDocument> cache;

        private Queue<string> queue;

        public IndexedDocument GetDocument(string key)
        {

            // look up the file corresponding to the key
            string file;
            if(index.TryGetValue(key, out file))
            {

                // now look for that file in the cache
                IndexedDocument document;
                if(!cache.TryGetValue(file, out document))
                {

                    // not in the cache, so load it
                    document = new IndexedDocument(this, file);

                    // if the cache is full, remove a document
                    if(cache.Count >= cacheSize)
                    {
                        string fileToUnload = queue.Dequeue();
                        cache.Remove(fileToUnload);
                    }

                    // add it to the cache
                    cache.Add(file, document);
                    queue.Enqueue(file);

                }

                // XPathNavigator content = document.GetContent(key);
                return (document);

            }
            else
            {
                // there is no such key
                return (null);
            }

        }


        public XPathNavigator GetContent(string key)
        {

            IndexedDocument document = GetDocument(key);
            if(document == null)
            {
                return (null);
            }
            else
            {
                return (document.GetContent(key));
            }

        }

        public int Count
        {
            get
            {
                return (index.Count);
            }
        }

    }

    // a file that we have indexed

    public class IndexedDocument
    {

        public IndexedDocument(IndexedDocumentCache cache, string file)
        {

            if(cache == null)
                throw new ArgumentNullException("cache");
            if(file == null)
                throw new ArgumentNullException("file");

            // remember the file
            this.file = file;

            // load the document
            try
            {
                //XPathDocument document = new XPathDocument(file, XmlSpace.Preserve);
                XPathDocument document = new XPathDocument(file);

                // search for value nodes
                XPathNodeIterator valueNodes = document.CreateNavigator().Select(cache.ValueExpression);
                // Console.WriteLine("found {0} instances of '{1}' (key xpath is '{2}')", valueNodes.Count, valueExpression.Expression, keyExpression.Expression);

                // get the key string for each value node and record it in the index
                foreach(XPathNavigator valueNode in valueNodes)
                {

                    XPathNavigator keyNode = valueNode.SelectSingleNode(cache.KeyExpression);
                    if(keyNode == null)
                    {
                        // Console.WriteLine("null key");
                        continue;
                    }

                    string key = keyNode.Value;
                    index[key] = valueNode;
                    if(!index.ContainsKey(key))
                    {
                        //index.Add(key, valueNode);
                    }
                    else
                    {
                        // Console.WriteLine("Repeat key '{0}'", key);
                    }
                }

            }
            catch(IOException e)
            {
                cache.Component.WriteHelperMessage(MessageLevel.Error, String.Format("An access error occured while attempting to load the file '{0}'. The error message is: {1}", file, e.Message));
            }
            catch(XmlException e)
            {
                cache.Component.WriteHelperMessage(MessageLevel.Error, String.Format("The indexed document '{0}' is not a valid XML document. The error message is: {1}", file, e.Message));
            }
            // Console.WriteLine("indexed {0} keys", index.Count);


        }

        // the indexed file

        private string file;

        // the index that maps keys to positions in the file		

        Dictionary<string, XPathNavigator> index = new Dictionary<string, XPathNavigator>();

        // public methods

        public string File
        {
            get
            {
                return (file);
            }
        }

        public XPathNavigator GetContent(string key)
        {
            XPathNavigator value = index[key];
            if(value == null)
            {
                return (null);
            }
            else
            {
                return (value.Clone());
            }
        }

        public string[] GetKeys()
        {
            string[] keys = new string[Count];
            index.Keys.CopyTo(keys, 0);
            return (keys);
        }

        public int Count
        {
            get
            {
                return (index.Count);
            }
        }

    }

    internal class CopyCommand
    {

        public CopyCommand(IndexedDocumentCache source_index, string key_xpath, string source_xpath, string target_xpath, string attribute_value, string ignoreCase_value)
        {
            this.cache = source_index;
            if(String.IsNullOrEmpty(key_xpath))
            {
                // Console.WriteLine("null key xpath");
                key = XPathExpression.Compile("string($key)");
            }
            else
            {
                // Console.WriteLine("compiling key xpath '{0}'", key_xpath);
                key = XPathExpression.Compile(key_xpath);
            }
            source = XPathExpression.Compile(source_xpath);
            target = target_xpath;
            attribute = attribute_value;
            ignoreCase = ignoreCase_value;
        }

        private IndexedDocumentCache cache;

        private XPathExpression key;

        private XPathExpression source;

        private String target;

        private String attribute;

        private String ignoreCase;

        private MessageLevel missingEntry = MessageLevel.Ignore;

        private MessageLevel missingSource = MessageLevel.Ignore;

        private MessageLevel missingTarget = MessageLevel.Ignore;

        public IndexedDocumentCache Index
        {
            get
            {
                return (cache);
            }
        }

        public XPathExpression Key
        {
            get
            {
                return (key);
            }
        }

        public XPathExpression Source
        {
            get
            {
                return (source);
            }
        }

        public String Target
        {
            get
            {
                return (target);
            }
        }

        public String Attribute
        {
            get
            {
                return (attribute);
            }
        }

        public String IgnoreCase
        {
            get
            {
                return (ignoreCase);
            }
        }

        public MessageLevel MissingEntry
        {
            get
            {
                return (missingEntry);
            }
            set
            {
                missingEntry = value;
            }
        }

        public MessageLevel MissingSource
        {
            get
            {
                return (missingSource);
            }
            set
            {
                missingSource = value;
            }
        }

        public MessageLevel MissingTarget
        {
            get
            {
                return (missingTarget);
            }
            set
            {
                missingTarget = value;
            }
        }

    }

    // the abstract CopyComponent
    public abstract class CopyComponent
    {

        public CopyComponent(XPathNavigator configuration, Dictionary<string, object> data) { }

        public abstract void Apply(XmlDocument document, string key);

    }
}
