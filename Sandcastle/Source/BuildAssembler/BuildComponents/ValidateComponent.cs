// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {

	public class ValidateComponent : BuildComponent {

		private XmlSchemaSet schemas = new XmlSchemaSet();

		public ValidateComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {

			XPathNodeIterator schema_nodes = configuration.Select("schema");
			foreach (XPathNavigator schema_node in schema_nodes) {
				string file = schema_node.GetAttribute("file", String.Empty);
				schemas.Add(null, file);
			}
					
		}

		public override void Apply (XmlDocument document, string key) {

			// set the validate schema
			document.Schemas = schemas;

			// create a validation handler
			ValidationEventHandler handler = new ValidationEventHandler(LogValidationError);

			// validate the document
			document.Validate(handler);

		}

		private void LogValidationError (Object o, ValidationEventArgs e) {
			string message = String.Format("ValidationError: {0}", e.Message);
			WriteMessage(MessageLevel.Warn, message);
		}

	}

}