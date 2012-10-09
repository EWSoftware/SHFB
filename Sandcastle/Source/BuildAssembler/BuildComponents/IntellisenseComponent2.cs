// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {

	public class IntellisenseComponent2 : BuildComponent {

		public IntellisenseComponent2 (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {

			XPathNavigator output_node = configuration.SelectSingleNode("output");
			if (output_node != null) {
                
				string directory_value = output_node.GetAttribute("directory", String.Empty);
				if (!String.IsNullOrEmpty(directory_value)) {
					directory = Environment.ExpandEnvironmentVariables(directory_value);
					if (!Directory.Exists(directory)) WriteMessage(MessageLevel.Error, String.Format("The output directory '{0}' does not exist.", directory));
				}
			}

            XPathNavigator expression_node = configuration.SelectSingleNode("expressions");
            if (expression_node != null) {

                string root = expression_node.GetAttribute("root", string.Empty);
                try {
                    rootExpression = XPathExpression.Compile(root);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", root));
                }

                string assembly = expression_node.GetAttribute("assembly", string.Empty);
                try {
                    assemblyExpression = XPathExpression.Compile(assembly);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", assembly));
                }

                string summary = expression_node.GetAttribute("summary", string.Empty);
                try {
                    summaryExpression = XPathExpression.Compile(summary);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", summary));
                }

                string parameters = expression_node.GetAttribute("parameters", string.Empty);
                try {
                    parametersExpression = XPathExpression.Compile(parameters);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", parameters));
                }

                string parameterContent = expression_node.GetAttribute("parameterContent", string.Empty);
                try {
                    parameterContentExpression = XPathExpression.Compile(parameterContent);
                } catch (XPathException ) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", parameterContent));
                }

                string templates = expression_node.GetAttribute("templates", string.Empty);
                try {
                    templatesExpression = XPathExpression.Compile(templates);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", templates));
                }

                string templateContent = expression_node.GetAttribute("templateContent", string.Empty);
                try {
                    templateContentExpression = XPathExpression.Compile(templateContent);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", templateContent));
                }

                string returns = expression_node.GetAttribute("returns", string.Empty);
                try {
                    returnsExpression = XPathExpression.Compile(returns);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", returns));
                }

                string exception = expression_node.GetAttribute("exception", string.Empty);
                try {
                    exceptionExpression = XPathExpression.Compile(exception);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", exception));
                }

                string exceptionCref = expression_node.GetAttribute("exceptionCref", string.Empty);
                try {
                    exceptionCrefExpression = XPathExpression.Compile(exceptionCref);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", exceptionCref));
                }

                string enumeration = expression_node.GetAttribute("enumeration", string.Empty);
                try {
                    enumerationExpression = XPathExpression.Compile(enumeration);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", enumeration));
                }

                string enumerationApi = expression_node.GetAttribute("enumerationApi", string.Empty);
                try {
                    enumerationApiExpression = XPathExpression.Compile(enumerationApi);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", enumerationApi));
                }

                string memberSummary = expression_node.GetAttribute("memberSummary", string.Empty);
                try {
                    memberSummaryExpression = XPathExpression.Compile(memberSummary);
                } catch (XPathException) {
                    WriteMessage(MessageLevel.Error, String.Format("The expression '{0}' is not a valid XPath expression.", memberSummary));
                }

            }

            // a way to get additional information into the intellisense file
            XPathNodeIterator input_nodes = configuration.Select("input");
            foreach (XPathNavigator input_node in input_nodes) {
                string file_value = input_node.GetAttribute("file", String.Empty);
                if (!String.IsNullOrEmpty(file_value)) {
                    string file = Environment.ExpandEnvironmentVariables(file_value);
                    ReadInputFile(file);
                }
            }
        }

        // input content store

        private void ReadInputFile (string file) {
           try {
                XPathDocument document = new XPathDocument(file);

                XPathNodeIterator member_nodes = document.CreateNavigator().Select("/metadata/topic[@id]");
                foreach (XPathNavigator member_node in member_nodes) {
                    string id = member_node.GetAttribute("id", String.Empty);
                    content[id] = member_node.Clone();
                }

                WriteMessage(MessageLevel.Info, String.Format("Read {0} input content nodes.", member_nodes.Count));

            } catch (XmlException e) {
                WriteMessage(MessageLevel.Error, String.Format("The input file '{0}' is not a well-formed XML file. The error message is: {1}", file, e.Message));
            } catch (IOException e) {
                WriteMessage(MessageLevel.Error, String.Format("An error occured while attempting to access the fileThe input file '{0}'. The error message is: {1}", file, e.Message));
            }


        }
        private Dictionary<string, XPathNavigator> content = new Dictionary<string, XPathNavigator>();

        // the action of the component

		public override void Apply (XmlDocument document, string id) {
            
            // only generate intellisense if id corresponds to an allowed intellisense ID
            if (id.Length < 2) return;
            if (id[1] != ':') return;
            if (!((id[0] == 'T') || (id[0] == 'M') || (id[0] == 'P') || (id[0] == 'F') || (id[0] == 'E') || (id[0] == 'N'))) return;

            XPathNavigator root = document.CreateNavigator().SelectSingleNode(rootExpression);
            
            // get the assembly information
            string assembly = (string) root.Evaluate(assemblyExpression);
                      
            if (String.IsNullOrEmpty(assembly)) {
                    assembly = "namespaces";
            }
            
            // try/catch block for capturing errors
            try {

                // get the writer for the assembly
                XmlWriter writer;
                if (!writers.TryGetValue(assembly, out writer)) {

                    // create a writer for the assembly
                    string name = Path.Combine(directory, assembly + ".xml");
                    // Console.WriteLine("creating {0}", name);

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;

                    try {
                        writer = XmlWriter.Create(name, settings);
                    } catch (IOException e) {
                        WriteMessage(MessageLevel.Error, String.Format("An access error occured while attempting to create the intellisense output file '{0}'. The error message is: {1}", name, e.Message));
                    }

                    writers.Add(assembly, writer);

                    // write out the initial data
                    writer.WriteStartDocument();
                    writer.WriteStartElement("doc");
                    //do not generate assembly nodes for namespace topics
                    if (assembly != "namespaces")
                    {
                        writer.WriteStartElement("assembly");
                        writer.WriteElementString("name", assembly);
                        writer.WriteEndElement();
                    }
                    writer.WriteStartElement("members");
                }

                writer.WriteStartElement("member");
                writer.WriteAttributeString("name", id);
                                
                // summary
                WriteSummary(root, summaryExpression, writer);
                                                
                // return value
                XPathNavigator returns = root.SelectSingleNode(returnsExpression);
                if (returns != null) {
                    writer.WriteStartElement("returns");
                    XmlReader reader = returns.ReadSubtree();
                    
                    CopyContent(reader, writer);
                    reader.Close();

                    writer.WriteEndElement();
                }

                // parameters
                XPathNodeIterator parameters = root.Select(parametersExpression);
                foreach (XPathNavigator parameter in parameters) {

                    string name = (string)parameter.GetAttribute("paramName", string.Empty);
                                        
                    XPathNavigator parameterContent = parameter.SelectSingleNode(parameterContentExpression);
                    
                    if (parameterContent == null) continue;
                    
                    XmlReader reader = parameterContent.ReadSubtree();

                    writer.WriteStartElement("param");
                    writer.WriteAttributeString("name", name);
                    CopyContent(reader, writer);
                    writer.WriteFullEndElement();

                    reader.Close();
                }
                
                // templates
                XPathNodeIterator templates = root.Select(templatesExpression);
                foreach (XPathNavigator template in templates) {

                    string name = (string)template.GetAttribute("paramName", string.Empty);
                    
                    XPathNavigator templateContent = template.SelectSingleNode(templateContentExpression);

                    if (templateContent == null) continue;

                    XmlReader reader = templateContent.ReadSubtree();

                    writer.WriteStartElement("typeparam");
                    writer.WriteAttributeString("name", name);
                    CopyContent(reader, writer);
                    writer.WriteFullEndElement();

                    reader.Close();
                }

                // exceptions
                XPathNodeIterator exceptions = root.Select(exceptionExpression);
                foreach (XPathNavigator exception in exceptions) {
                                        
                    XPathNavigator exceptionCref = exception.SelectSingleNode(exceptionCrefExpression);
                                     
                    if (exceptionCref == null) continue;
                   
                    string cref = exceptionCref.GetAttribute("target", string.Empty);
                    XmlReader reader = exception.ReadSubtree();

                    writer.WriteStartElement("exception");
                    writer.WriteAttributeString("cref", cref);
                    CopyContent(reader, writer);
                    writer.WriteFullEndElement();

                    reader.Close();
                }

                // stored contents
                XPathNavigator input;
                if (content.TryGetValue(id, out input)) {
                    XPathNodeIterator input_nodes = input.SelectChildren(XPathNodeType.Element);
                    foreach (XPathNavigator input_node in input_nodes) {
                        input_node.WriteSubtree(writer);
                    }
                }
                                
                writer.WriteFullEndElement();
                
                // enumeration members
                XPathNodeIterator enumerationIterator = root.Select(enumerationExpression);

                foreach (XPathNavigator enumeration in enumerationIterator) {

                    XPathNavigator enumApi = enumeration.SelectSingleNode(enumerationApiExpression);

                    if (enumApi == null) continue;

                    string api = (string)enumApi.GetAttribute("target", string.Empty);
                    writer.WriteStartElement("member");
                    writer.WriteAttributeString("name", api);

                    //summary
                    WriteSummary(enumeration, memberSummaryExpression, writer);

                    writer.WriteFullEndElement();
                }
            } catch (IOException e) {
                WriteMessage(MessageLevel.Error, String.Format("An access error occured while attempting to write intellisense data. The error message is: {0}", e.Message));
            } catch (XmlException e) {
                WriteMessage(MessageLevel.Error, String.Format("Intellisense data was not valid XML. The error message is: {0}", e.Message));
            }

		}

        protected override void Dispose(bool disposing) {
            if (disposing) {
			    foreach (XmlWriter writer in writers.Values) {
				    writer.WriteEndDocument();
				    writer.Close();
			    }
            }
            base.Dispose(disposing);
        }
        
        private void WriteSummary(XPathNavigator node, XPathExpression expression, XmlWriter writer) {
            
            XPathNavigator summary = node.SelectSingleNode(expression);
            if (summary != null) {
                writer.WriteStartElement("summary");

                XmlReader reader = summary.ReadSubtree();

                CopyContent(reader, writer);
                reader.Close();

                writer.WriteEndElement();
            } else {
                    // Console.WriteLine("no summary");
            }
        }
        
		private void CopyContent (XmlReader reader, XmlWriter writer) {
			reader.MoveToContent();
			while (true) {
                
				if (reader.NodeType == XmlNodeType.Text) {
					writer.WriteString(reader.ReadString());
				} else if (reader.NodeType == XmlNodeType.Element) {
					if (reader.LocalName == "span" && (reader.GetAttribute("sdata",string.Empty) == "cer")) {
                        writer.WriteStartElement("see");
						writer.WriteAttributeString("cref", reader.GetAttribute("target", string.Empty));
						writer.WriteEndElement();
                        reader.Skip();
                    }
                    else if (reader.LocalName == "span" && (reader.GetAttribute("sdata", string.Empty) == "paramReference"))
                    {
						writer.WriteStartElement("paramref");
						writer.WriteAttributeString("name", reader.ReadElementString());
						writer.WriteEndElement();
                    } else if (reader.LocalName == "span" && (reader.GetAttribute("sdata",string.Empty) == "link")) {
                        writer.WriteString(reader.ReadElementString());
                    }
                    else if (reader.LocalName == "span" && (reader.GetAttribute("sdata", string.Empty) == "langKeyword"))
                    {
                        string keyword = reader.GetAttribute("value", string.Empty);
                        writer.WriteString(keyword);
                        reader.Skip();
                    }
                    else {
						reader.Read();
                    }
				} else {
					if (!reader.Read()) break;
				}

			}
			
		}

		private string directory = String.Empty;

		private Dictionary<string,XmlWriter> writers = new Dictionary<string,XmlWriter>();
        		
        private XPathExpression rootExpression;

        private XPathExpression assemblyExpression;

        private XPathExpression summaryExpression;

        private XPathExpression parametersExpression;

        private XPathExpression parameterContentExpression;

        private XPathExpression templatesExpression;

        private XPathExpression templateContentExpression;

        private XPathExpression returnsExpression;

        private XPathExpression exceptionExpression;

        private XPathExpression exceptionCrefExpression;

        private XPathExpression enumerationExpression;

        private XPathExpression enumerationApiExpression;

        private XPathExpression memberSummaryExpression;
     	
	}

}