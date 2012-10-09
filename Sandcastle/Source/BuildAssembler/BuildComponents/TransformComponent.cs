// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools {

	public class TransformComponent : BuildComponent {
		
		public TransformComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {

			// load the transforms
			XPathNodeIterator transform_nodes = configuration.Select("transform");
			foreach (XPathNavigator transform_node in transform_nodes) {

				// load the transform
				string file = transform_node.GetAttribute("file", String.Empty);
				if (String.IsNullOrEmpty(file)) WriteMessage(MessageLevel.Error, "Each transform element must specify a file attribute.");
				file = Environment.ExpandEnvironmentVariables(file);

				Transform transform = null;
				try {
					transform = new Transform(file);
				} catch (IOException e) {
					WriteMessage(MessageLevel.Error, String.Format("The transform file '{0}' could not be loaded. The error message is: {1}", file, BuildComponentUtilities.GetExceptionMessage(e)));
				} catch (XmlException e) {
					WriteMessage(MessageLevel.Error, String.Format("The transform file '{0}' is not a valid XML file. The error message is: {1}", file, BuildComponentUtilities.GetExceptionMessage(e)));
				} catch (XsltException e) {
					WriteMessage(MessageLevel.Error, String.Format("The XSL transform '{0}' contains an error. The error message is: {1}", file, BuildComponentUtilities.GetExceptionMessage(e))); 
				}


				transforms.Add(transform);


				// load any arguments
				XPathNodeIterator argument_nodes = transform_node.Select("argument");
				foreach (XPathNavigator argument_node in argument_nodes) {
					string key = argument_node.GetAttribute("key", String.Empty);
					if ((key == null) || (key.Length == 0)) WriteMessage(MessageLevel.Error, "When creating a transform argument, you must specify a key using the key attribute");

                    // set "expand-value" attribute to true to expand environment variables embedded in "value".
                    string expand_attr = argument_node.GetAttribute("expand-value", String.Empty);
                    bool expand_value = String.IsNullOrEmpty(expand_attr) ? false : Convert.ToBoolean(expand_attr);

					string value = argument_node.GetAttribute("value", String.Empty);
					if ((value != null) && (value.Length > 0)) {
                        transform.Arguments.AddParam(key, String.Empty, expand_value ? Environment.ExpandEnvironmentVariables(value) : value);
                    }
                    else {
						transform.Arguments.AddParam(key, String.Empty, argument_node.Clone());
					}
				}			

			}

		}

		// the stored transforms

		private List<Transform> transforms = new List<Transform>();

		// the action of the component

		public override void Apply (XmlDocument document, string key) {

			// iterate over transforms
			foreach (Transform transform in transforms) {

				// add the key as a parameter to the arguments
				transform.Arguments.RemoveParam("key", String.Empty);
				transform.Arguments.AddParam("key", String.Empty, key);

				// create a buffer into which output can be written
				using (MemoryStream buffer = new MemoryStream()) {


					// do the transform, routing output to the buffer
                    XmlWriterSettings settings = transform.Xslt.OutputSettings;
                    XmlWriter writer = XmlWriter.Create(buffer, settings);
                    try {
                        transform.Xslt.Transform(document, transform.Arguments, writer);
                    } catch (XsltException e) {
                        WriteMessage(MessageLevel.Error, String.Format("A error ocurred while executing the transform '{0}', on line {1}, at position {2}. The error message was: {3}", e.SourceUri, e.LineNumber, e.LinePosition, (e.InnerException == null) ? e.Message : e.InnerException.Message));
                    } catch (XmlException e) {
                        WriteMessage(MessageLevel.Error, String.Format("A error ocurred while executing the transform '{0}', on line {1}, at position {2}. The error message was: {3}", e.SourceUri, e.LineNumber, e.LinePosition, (e.InnerException == null) ? e.Message : e.InnerException.Message));
                    } finally {
						writer.Close();
					}

					// replace the document by the contents of the buffer
					buffer.Seek(0, SeekOrigin.Begin);

                    // some settings to ensure that we don't try to go get, parse, and validate using any referenced schemas or DTDs
                    XmlReaderSettings readerSettings = new XmlReaderSettings();

                    //!EFW - Update. Uses DtdProcessing property now rather than the obsolete ProhibitDtd property.
                    // The old property value was false which translates to Parse for the new property.
                    readerSettings.DtdProcessing = DtdProcessing.Parse;
                    readerSettings.XmlResolver = null;

                    XmlReader reader = XmlReader.Create(buffer, readerSettings);
                    try {
                        document.Load(reader);
                    } catch (XmlException e) {
                        WriteMessage(MessageLevel.Error, String.Format("A error ocurred while executing the transform '{0}', on line {1}, at position {2}. The error message was: {3}", e.SourceUri, e.LineNumber, e.LinePosition, (e.InnerException == null) ? e.Message : e.InnerException.Message));
                    } finally {
                        reader.Close();
                    }

				}
			}
	
		}

	}


	// a represenataion of a transform action

	internal class Transform {

		public Transform (string file) {
            // The transforms presumably come from a trusted source, so there's no reason
            // not to enable scripting and the document function. The latter is used to read topic
            // info files for the conceptual WebDocs build.
			xslt.Load(file, new XsltSettings(true, true), new XmlUrlResolver());
		}

		private XslCompiledTransform xslt = new XslCompiledTransform();

		private XsltArgumentList arguments = new XsltArgumentList();

		public XslCompiledTransform Xslt {
			get {
				return(xslt);
			}
		}

		public XsltArgumentList Arguments {
			get {
				return(arguments);
			}
		}

	}

}
