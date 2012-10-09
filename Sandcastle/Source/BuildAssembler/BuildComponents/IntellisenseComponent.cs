// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {

	public class IntellisenseComponent : BuildComponent {

		public IntellisenseComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {

			XPathNavigator output_node = configuration.SelectSingleNode("output");
			if (output_node != null) {
                
				string directory_value = output_node.GetAttribute("directory", String.Empty);
				if (!String.IsNullOrEmpty(directory_value)) {
					directory = Environment.ExpandEnvironmentVariables(directory_value);
					if (!Directory.Exists(directory)) WriteMessage(MessageLevel.Error, String.Format("The output directory '{0}' does not exist.", directory));
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

			context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");

			summaryExpression.SetContext(context);
            memberSummaryExpression.SetContext(context);
			returnsExpression.SetContext(context);
			parametersExpression.SetContext(context);
			parameterNameExpression.SetContext(context);
            templatesExpression.SetContext(context);
            templateNameExpression.SetContext(context);
            exceptionExpression.SetContext(context);
            exceptionCrefExpression.SetContext(context);
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
            
   			XPathNavigator root = document.CreateNavigator().SelectSingleNode("/document/comments");

            string assembly;
                      
            if ((string)root.Evaluate(groupExpression) == "namespace") {
                // get the assembly for the namespace
                //assembly = (string) root.Evaluate(namespaceAssemblyExpression);
                // Assign general name for namespace assemblies since they do not belong to any specific assembly
                assembly = "namespaces";
            } else {
			    // get the assembly for the API
			    assembly = (string) root.Evaluate(assemblyExpression);
            }
                       
			if (String.IsNullOrEmpty(assembly)) return;

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
                    if ((string)root.Evaluate(groupExpression) != "namespace")
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

                    string name = (string)parameter.Evaluate(parameterNameExpression);

                    XmlReader reader = parameter.ReadSubtree();

                    writer.WriteStartElement("param");
                    writer.WriteAttributeString("name", name);
                    CopyContent(reader, writer);
                    writer.WriteEndElement();

                    reader.Close();
                }

                // templates
                XPathNodeIterator templates = root.Select(templatesExpression);
                foreach (XPathNavigator template in templates) {

                    string name = (string)template.Evaluate(templateNameExpression);

                    XmlReader reader = template.ReadSubtree();

                    writer.WriteStartElement("typeparam");
                    writer.WriteAttributeString("name", name);
                    CopyContent(reader, writer);
                    writer.WriteEndElement();

                    reader.Close();
                }

                // exceptions
                XPathNodeIterator exceptions = root.Select(exceptionExpression);
                foreach (XPathNavigator exception in exceptions) {

                    string exceptionCref = (string)exception.Evaluate(exceptionCrefExpression);

                    XmlReader reader = exception.ReadSubtree();

                    writer.WriteStartElement("exception");
                    writer.WriteAttributeString("cref", exceptionCref);
                    CopyContent(reader, writer);
                    writer.WriteEndElement();

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
                string subgroup = (string)root.Evaluate(subgroupExpression);
                if (subgroup == "enumeration") {

                    XPathNodeIterator elements = (XPathNodeIterator)root.Evaluate(elementsExpression);
                    foreach (XPathNavigator element in elements) {

                        string api = (string)element.GetAttribute("api", string.Empty);
                        writer.WriteStartElement("member");
                        writer.WriteAttributeString("name", api);
                        
                        //summary
                        WriteSummary(element, memberSummaryExpression, writer);
                                                
                        writer.WriteFullEndElement();
                    }
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
            }
            else {
                // Console.WriteLine("no summary");
            }
        }

		private void CopyContent (XmlReader reader, XmlWriter writer) {
			reader.MoveToContent();
			while (true) {

				//Console.WriteLine("{0} {1}", reader.ReadState, reader.NodeType);

				if (reader.NodeType == XmlNodeType.Text) {
					writer.WriteString(reader.ReadString());
				} else if (reader.NodeType == XmlNodeType.Element) {
					//Console.WriteLine(reader.LocalName);
					if (reader.LocalName == "codeEntityReference") {
						writer.WriteStartElement("see");
						writer.WriteAttributeString("cref", reader.ReadElementString());
						writer.WriteEndElement();
					} else if (reader.LocalName == "parameterReference") {
						writer.WriteStartElement("paramref");
						writer.WriteAttributeString("name", reader.ReadElementString());
						writer.WriteEndElement();
                    } else if (reader.LocalName == "link") {
                        string displayText = reader.ReadElementString();
                        if (displayText.StartsWith("GTMT#")) {
                           writer.WriteString(displayText.Substring(displayText.IndexOf("#") + 1));
                        } else {
                            writer.WriteString(displayText);
                        }
                    } else {
						reader.Read();
					}
				} else {
					if (!reader.Read()) break;
				}

			}
			
		}

		private string directory = String.Empty;

		private Dictionary<string,XmlWriter> writers = new Dictionary<string,XmlWriter>();

		private XPathExpression assemblyExpression = XPathExpression.Compile("string(/document/reference/containers/library/@assembly)");

        private XPathExpression namespaceAssemblyExpression = XPathExpression.Compile("string(/document/reference/elements/element/containers/library/@assembly)");
             
     	private XPathExpression summaryExpression = XPathExpression.Compile("ddue:dduexml/ddue:summary");
               
        private XPathExpression returnsExpression = XPathExpression.Compile("ddue:dduexml/ddue:returnValue");
		
		private XPathExpression parametersExpression = XPathExpression.Compile("ddue:dduexml/ddue:parameters/ddue:parameter/ddue:content");

		private XPathExpression parameterNameExpression = XPathExpression.Compile("string(../ddue:parameterReference)");

        private XPathExpression templatesExpression = XPathExpression.Compile("ddue:dduexml/ddue:genericParameters/ddue:genericParameter/ddue:content");

        private XPathExpression templateNameExpression = XPathExpression.Compile("string(../ddue:parameterReference)");
        
        private XPathExpression exceptionExpression = XPathExpression.Compile("ddue:dduexml/ddue:exceptions/ddue:exception/ddue:content");

        private XPathExpression exceptionCrefExpression = XPathExpression.Compile("string(../ddue:codeEntityReference)");

        private XPathExpression subgroupExpression = XPathExpression.Compile("string(/document/reference/apidata/@subgroup)");

        private XPathExpression groupExpression = XPathExpression.Compile("string(/document/reference/apidata/@group)");

        private XPathExpression elementsExpression = XPathExpression.Compile("/document/reference/elements/element");

        private XPathExpression memberSummaryExpression = XPathExpression.Compile("ddue:summary");
        
        private XmlNamespaceManager context = new CustomContext();

	}

}