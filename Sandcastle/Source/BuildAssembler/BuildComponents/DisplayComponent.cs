// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {


	public class DisplayComponent : BuildComponent {

		private string xpath_format = "/";

		public DisplayComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {
			XPathNavigator xpath_format_node = configuration.SelectSingleNode("xpath");
			if (xpath_format_node != null) xpath_format = xpath_format_node.Value;
		}

		public override void Apply (XmlDocument document, string key) {
			string xpath = String.Format(xpath_format, key);

			Object result = document.CreateNavigator().Evaluate(xpath);

			if (result == null) {
				Console.WriteLine("null result");
				return;
			}

			XPathNodeIterator nodes = result as XPathNodeIterator;
			if (nodes != null) {
				foreach (XPathNavigator node in nodes) {
					Console.WriteLine(node.OuterXml);
				}
				return;				
			}
			
			Console.WriteLine(result.ToString());

		}

	}

}
