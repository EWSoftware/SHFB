// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using System.IO;

namespace Microsoft.Ddue.Tools
{

    public class SaveComponent : BuildComponent
    {

        private CustomContext context = new CustomContext();

        private XPathExpression path_expression;

        private XPathExpression select_expression;

        private XmlWriterSettings settings = new XmlWriterSettings();

        public SaveComponent(BuildAssembler assembler, XPathNavigator configuration)
            : base(assembler, configuration)
        {

            // load the target path format
            XPathNavigator save_node = configuration.SelectSingleNode("save");
            if(save_node == null)
                throw new ConfigurationErrorsException("When instantiating a save component, you must specify a the target file using the <save> element.");

            string base_value = save_node.GetAttribute("base", String.Empty);
            if(!String.IsNullOrEmpty(base_value))
            {
                basePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(base_value));
            }

            string path_value = save_node.GetAttribute("path", String.Empty);
            if(String.IsNullOrEmpty(path_value))
                WriteMessage(MessageLevel.Error, "Each save element must have a path attribute specifying an XPath that evaluates to the location to save the file.");
            path_expression = XPathExpression.Compile(path_value);

            string select_value = save_node.GetAttribute("select", String.Empty);
            if(!String.IsNullOrEmpty(select_value))
                select_expression = XPathExpression.Compile(select_value);

            settings.Encoding = Encoding.UTF8;

            string indent_value = save_node.GetAttribute("indent", String.Empty);
            if(!String.IsNullOrEmpty(indent_value))
                settings.Indent = Convert.ToBoolean(indent_value);

            string omit_value = save_node.GetAttribute("omit-xml-declaration", String.Empty);
            if(!String.IsNullOrEmpty(omit_value))
                settings.OmitXmlDeclaration = Convert.ToBoolean(omit_value);

            linkPath = save_node.GetAttribute("link", String.Empty);
            if(String.IsNullOrEmpty(linkPath))
                linkPath = "../html";

            // add-xhtml-namespace adds a default namespace for xhtml. Required by Help3 documentation.
            string addXhtmlDeclaration = save_node.GetAttribute("add-xhtml-namespace", String.Empty);
            if(!String.IsNullOrEmpty(addXhtmlDeclaration))
                writeXhtmlNamespace = Convert.ToBoolean(addXhtmlDeclaration);


            // encoding

            settings.CloseOutput = true;

        }

        private string basePath = null;

        private string linkPath = null;

        private bool writeXhtmlNamespace = false;


        public override void Apply(XmlDocument document, string key)
        {

            // set the evaluation context
            context["key"] = key;

            XPathExpression path_xpath = path_expression.Clone();
            path_xpath.SetContext(context);

            // evaluate the path
            string path = document.CreateNavigator().Evaluate(path_xpath).ToString();
            string file = Path.GetFileName(path);

            string fileLinkPath = Path.Combine(linkPath, file);
            if(basePath != null)
                path = Path.Combine(basePath, path);

            string targetDirectory = Path.GetDirectoryName(path);
            if(!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            if(writeXhtmlNamespace)
            {
                document.DocumentElement.SetAttribute("xmlns", "http://www.w3.org/1999/xhtml");
                document.LoadXml(document.OuterXml);
            }

            // save the document
            // select_expression determines which nodes get saved. If there is no select_expression
            // we simply save the root node as before. If there is a select_expression, we evaluate the
            // xpath expression and save the resulting node set. The select expression also enables the 
            // "literal-text" processing instruction, which outputs its content as unescaped text.
            if(select_expression == null)
            {
                XmlNode doctype = document.DocumentType;
                try
                {
                    //Console.WriteLine("path = '{0}'", path);
                    //document.Save(path);

                    using(XmlWriter writer = XmlWriter.Create(path, settings))
                    {
                        document.Save(writer);
                    }

                }
                catch(IOException e)
                {
                    base.WriteMessage(key, MessageLevel.Error, "An access error occured while attempting to " +
                        "save to the file '{0}'. The error message is: {1}", path,
                        BuildComponentUtilities.GetExceptionMessage(e));
                }
                catch(XmlException e)
                {
                    base.WriteMessage(key, MessageLevel.Error, "Invalid XML was written to the output " +
                        "file '{0}'. The error message is: '{1}'", path,
                        BuildComponentUtilities.GetExceptionMessage(e));
                }

                // Get the relative html path for HXF generation.
                int index = fileLinkPath.IndexOf('/');
                string htmlPath = fileLinkPath.Substring(index + 1, fileLinkPath.Length - (index + 1));

                FileCreatedEventArgs fe = new FileCreatedEventArgs(htmlPath, Path.GetDirectoryName(targetDirectory));
                OnComponentEvent(fe);
            }
            else
            {
                // IMPLEMENTATION NOTE: The separate StreamWriter is used to maintain XML indenting.
                // Without it the XmlWriter won't honor our indent settings after plain text nodes have been
                // written.
                settings.ConformanceLevel = ConformanceLevel.Auto;
                using(StreamWriter output = File.CreateText(path))
                {
                    using(XmlWriter writer = XmlWriter.Create(output, settings))
                    {
                        XPathExpression select_xpath = select_expression.Clone();
                        select_xpath.SetContext(context);
                        XPathNodeIterator ni = document.CreateNavigator().Select(select_expression);
                        while(ni.MoveNext())
                        {
                            if(ni.Current.NodeType == XPathNodeType.ProcessingInstruction && ni.Current.Name.Equals("literal-text"))
                            {
                                writer.Flush();
                                output.Write(ni.Current.Value);
                            }
                            else
                                ni.Current.WriteSubtree(writer);
                        }
                    }
                }
            }
        }
    }
}
