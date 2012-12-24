// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    public class CopyFromFilesComponent : BuildComponent
    {
        public CopyFromFilesComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            XPathNodeIterator copy_nodes = configuration.Select("copy");

            foreach(XPathNavigator copy_node in copy_nodes)
            {

                string root_value = copy_node.GetAttribute("base", String.Empty);
                if(String.IsNullOrEmpty(root_value))
                    root_value = Environment.CurrentDirectory;
                root_value = Environment.ExpandEnvironmentVariables(root_value);

                if(!Directory.Exists(root_value))
                    WriteMessage(MessageLevel.Error, "The base directory '{0}' does not exist.", root_value);

                string file_value = copy_node.GetAttribute("file", String.Empty);
                if(String.IsNullOrEmpty(file_value))
                    WriteMessage(MessageLevel.Error, "Each copy element must have a file attribute specifying the file to copy from.");

                string source_value = copy_node.GetAttribute("source", String.Empty);
                string target_value = copy_node.GetAttribute("target", String.Empty);

                CopyFromFilesCommand copy_command = new CopyFromFilesCommand(root_value, file_value, source_value, target_value);
                copy_commands.Add(copy_command);
            }

            WriteMessage(MessageLevel.Info, "Loaded {0} copy commands.", copy_commands.Count);
        }

        List<CopyFromFilesCommand> copy_commands = new List<CopyFromFilesCommand>();

        private CustomContext context = new CustomContext();

        public override void Apply(XmlDocument document, string key)
        {
            context["key"] = key;
            foreach(CopyFromFilesCommand copy_command in copy_commands)
            {
                copy_command.Apply(document, context);
            }
        }

    }

    internal class CopyFromFilesCommand
    {

        public CopyFromFilesCommand(string root, string file, string source, string target)
        {
            root_directory = root;
            file_expression = XPathExpression.Compile(file);
            source_expression = XPathExpression.Compile(source);
            target_expression = XPathExpression.Compile(target);
        }

        private string root_directory;

        private XPathExpression file_expression;

        private XPathExpression source_expression;

        private XPathExpression target_expression;

        public void Apply(XmlDocument targetDocument, IXmlNamespaceResolver context)
        {

            XPathExpression local_file_expression = file_expression.Clone();
            local_file_expression.SetContext(context);

            XPathExpression local_source_expression = source_expression.Clone();
            local_source_expression.SetContext(context);

            XPathExpression local_target_expression = target_expression.Clone();
            local_target_expression.SetContext(context);

            string file_name = (string)targetDocument.CreateNavigator().Evaluate(local_file_expression);
            string file_path = Path.Combine(root_directory, file_name);

            if(!File.Exists(file_path))
                return;
            XPathDocument sourceDocument = new XPathDocument(file_path);

            XPathNavigator target_node = targetDocument.CreateNavigator().SelectSingleNode(local_target_expression);
            if(target_node == null)
                return;

            XPathNodeIterator source_nodes = sourceDocument.CreateNavigator().Select(local_source_expression);
            foreach(XPathNavigator source_node in source_nodes)
            {
                target_node.AppendChild(source_node);
            }

        }
    }
}