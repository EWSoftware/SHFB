// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using System.Reflection;

namespace Microsoft.Ddue.Tools {

	public class CloneComponent : BuildComponent {

		private List<IEnumerable<BuildComponent>> branches = new List<IEnumerable<BuildComponent>>();

		public CloneComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {
			
			XPathNodeIterator branch_nodes = configuration.Select("branch");
			foreach (XPathNavigator branch_node in branch_nodes) {
				BuildComponent[] branch = BuildAssembler.LoadComponents(branch_node);
				branches.Add(branch);
			}

		}

		public override void Apply (XmlDocument document, string key) {

			foreach (IEnumerable<BuildComponent> branch in branches) {
				XmlDocument subdocument = document.Clone() as XmlDocument;
				foreach(BuildComponent component in branch) {
					component.Apply(subdocument, key);
				}
			}

		}

	}

}