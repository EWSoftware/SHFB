// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    public class CopyFromFileComponent : BuildComponent
    {
        public CopyFromFileComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException("configuration");

            string data_name = null;

            // get information about the data file
            XPathNodeIterator data_nodes = configuration.Select("data");
            foreach(XPathNavigator data_node in data_nodes)
            {
                string data_file = data_node.GetAttribute("file", String.Empty);
                if(String.IsNullOrEmpty(data_file))
                    WriteMessage(MessageLevel.Error, "Data elements must have a file attribute specifying a file from which to load data.");
                data_file = Environment.ExpandEnvironmentVariables(data_file);

                data_name = data_node.GetAttribute("name", String.Empty);
                if(String.IsNullOrEmpty(data_name))
                    data_name = Guid.NewGuid().ToString();

                // load a schema, if one is specified
                string schema_file = data_node.GetAttribute("schema", String.Empty);
                XmlReaderSettings settings = new XmlReaderSettings();
                if(!String.IsNullOrEmpty(schema_file))
                {
                    settings.Schemas.Add(null, schema_file);
                }

                // load the document
                WriteMessage(MessageLevel.Info, "Loading data file '{0}'.", data_file);
                using(XmlReader reader = XmlReader.Create(data_file, settings))
                {
                    XPathDocument data_document = new XPathDocument(reader);
                    Data.Add(data_name, data_document);
                }
            }


            // get the source and target expressions for each copy command
            XPathNodeIterator copy_nodes = configuration.Select("copy");
            foreach(XPathNavigator copy_node in copy_nodes)
            {
                string source_name = copy_node.GetAttribute("name", String.Empty);
                if(String.IsNullOrEmpty(source_name))
                    source_name = data_name;

                XPathDocument source_document = (XPathDocument)Data[source_name];

                string source_xpath = copy_node.GetAttribute("source", String.Empty);
                if(String.IsNullOrEmpty(source_xpath))
                    throw new ConfigurationErrorsException("When instantiating a CopyFromFile component, you must specify a source xpath format using the source attribute.");
                string target_xpath = copy_node.GetAttribute("target", String.Empty);
                if(String.IsNullOrEmpty(target_xpath))
                    throw new ConfigurationErrorsException("When instantiating a CopyFromFile component, you must specify a target xpath format using the target attribute.");
                copy_commands.Add(new CopyFromFileCommand(source_document, source_xpath, target_xpath));
            }

        }

        // private XPathDocument data_document;

        private List<CopyFromFileCommand> copy_commands = new List<CopyFromFileCommand>();

        private CustomContext context = new CustomContext();

        // the work of the component

        public override void Apply(XmlDocument document, string key)
        {

            // set the key in the XPath context
            context["key"] = key;

            // iterate over the copy commands
            foreach(CopyFromFileCommand copy_command in copy_commands)
            {

                // extract the target node
                XPathExpression target_xpath = copy_command.Target.Clone();
                target_xpath.SetContext(context);
                XPathNavigator target = document.CreateNavigator().SelectSingleNode(target_xpath);

                // warn if target not found?
                if(target == null)
                {
                    continue;
                }

                // extract the source nodes
                XPathExpression source_xpath = copy_command.Source.Clone();
                source_xpath.SetContext(context);
                XPathNodeIterator sources = copy_command.SourceDocument.CreateNavigator().Select(source_xpath);

                // warn if source not found?

                // append the source nodes to the target node
                foreach(XPathNavigator source in sources)
                {
                    target.AppendChild(source);
                }

            }

        }

    }


    // a representation of a copying operation

    internal class CopyFromFileCommand
    {

        public CopyFromFileCommand(XPathDocument source_document, string source_xpath, string target_xpath)
        {
            this.source_document = source_document;
            source = XPathExpression.Compile(source_xpath);
            target = XPathExpression.Compile(target_xpath);
        }

        private XPathDocument source_document;

        private XPathExpression source;

        private XPathExpression target;

        public XPathDocument SourceDocument
        {
            get
            {
                return (source_document);
            }
        }

        public XPathExpression Source
        {
            get
            {
                return (source);
            }
        }

        public XPathExpression Target
        {
            get
            {
                return (target);
            }
        }

    }

}
