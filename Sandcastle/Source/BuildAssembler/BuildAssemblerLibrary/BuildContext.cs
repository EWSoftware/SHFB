// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools {

	public class BuildContext {

		private CustomContext context = new CustomContext();

		// Namespace control

		public void AddNamespace (string prefix, string uri) {
			context.AddNamespace(prefix, uri);
		}

		public string LookupNamespace (string prefix) {
			return( context.LookupNamespace(prefix) );
		}

		public bool RemoveNamespace (string prefix) {
			string uri = LookupNamespace(prefix);
			if (uri == null) {
				return(false);
			} else {
				context.RemoveNamespace(prefix, uri);
				return(true);
			}
		}

		public void ClearNamespaces () {
		}

		// Variable control

		public void AddVariable (string name, string value) {
			context[name] = value;
		}

		public string LookupVariable (string name) {
			return( context[name] );
		}

		public bool RemoveVariable (string name) {
			return( context.ClearVariable(name) );
		}

		public void ClearVariables () {
			context.ClearVariables();
		}

		public string this [string name] {
			get {
				return(context[name]);
			}
			set {
				context[name] = value;
			}
		}

		// Function control

		// The context for use in XPath queries

		public XsltContext XsltContext {
			get {
				return(context);
			}
		}

        // Load data from config

        public void Load (XPathNavigator configuration) {
            XPathNodeIterator namespaceNodes = configuration.Select("namespace");
            foreach (XPathNavigator namespaceNode in namespaceNodes) {
                string prefixValue = namespaceNode.GetAttribute("prefix", String.Empty);
                string uriValue = namespaceNode.GetAttribute("uri", String.Empty);
                AddNamespace(prefixValue, uriValue);
            }
        }

	}

	 public class CustomContext : XsltContext {

		public CustomContext() : base() {}

		// variable control

		private Dictionary<string, IXsltContextVariable> variables = new Dictionary<string,IXsltContextVariable>();

		public string this [string variable] {
			get {
				return(variables[variable].Evaluate(this).ToString());
			}
			set {
				variables[variable] = new CustomVariable(value);
			}
		}

		public bool ClearVariable (string name) {
			return( variables.Remove(name) );
		}

		public void ClearVariables () {
			variables.Clear();
		}

		// Implementation of XsltContext methods

		public override IXsltContextVariable ResolveVariable (string prefix, string name) {
			return( variables[name] );
		}

		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType[] argumentTypes) {
			throw new NotImplementedException();
		}		

		public override int CompareDocument (string baseUri, string nextBaseUri) {
			return(0);
		}

		public override bool Whitespace {
			get {
				return(true);
			}
		}

		public override bool PreserveWhitespace (XPathNavigator node) {
			return(true);
		}

	}


	internal struct CustomVariable : IXsltContextVariable {

		public CustomVariable (string value) {
			this.value = value;
		}

		private string value;

		public bool IsLocal {
			get {
				return(false);
			}
		}

		public bool IsParam {
			get {
				return(false);
			}
		}

		public XPathResultType VariableType {
			get {
				return(XPathResultType.String);
			}
		}

		public Object Evaluate (XsltContext context) {
			return(value);
		}

	}
	

}