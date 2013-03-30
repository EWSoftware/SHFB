// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This component is used to execute a set of components on the topic
    /// </summary>
    public class ForEachComponent : BuildComponent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler instance</param>
        /// <param name="configuration">The component configuration</param>
        public ForEachComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            // set up the context
            XPathNodeIterator context_nodes = configuration.Select("context");

            foreach(XPathNavigator context_node in context_nodes)
            {
                string prefix = context_node.GetAttribute("prefix", String.Empty);
                string name = context_node.GetAttribute("name", String.Empty);
                context.AddNamespace(prefix, name);
            }

            // load the expression format
            XPathNavigator variable_node = configuration.SelectSingleNode("variable");

            if(variable_node == null)
                throw new ConfigurationErrorsException("When instantiating a ForEach component, you must specify a variable using the <variable> element.");

            string xpath_format = variable_node.GetAttribute("expression", String.Empty);

            if((xpath_format == null) || (xpath_format.Length == 0))
                throw new ConfigurationErrorsException("When instantiating a ForEach component, you must specify a variable expression using the expression attribute");

            xpath = XPathExpression.Compile(xpath_format);

            // load the subcomponents
            WriteMessage(MessageLevel.Info, "Loading subcomponents.");
            XPathNavigator components_node = configuration.SelectSingleNode("components");

            if(components_node == null)
                throw new ConfigurationErrorsException("When instantiating a ForEach component, you must specify subcomponents using the <components> element.");

            components = BuildAssembler.LoadComponents(components_node);

            WriteMessage(MessageLevel.Info, "Loaded {0} subcomponents.", components.Count());
        }

        // the format string for the variable expression
        private XPathExpression xpath;

        // the xpath context
        private CustomContext context = new CustomContext();

        // the subcomponents
        private IEnumerable<BuildComponent> components;

        // the work of the component

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // adjust the context
            context["key"] = key;

            // evaluate the condition
            XPathExpression xpath_local = xpath.Clone();
            xpath_local.SetContext(context);

            Object result = document.CreateNavigator().Evaluate(xpath_local);

            // try to intrepret the result as a node set
            XPathNodeIterator result_node_iterator = result as XPathNodeIterator;

            if(result_node_iterator != null)
            {
                // if it is, apply the child components to each node value
                foreach(XPathNavigator result_node in result_node_iterator.ToArray())
                    ApplyComponents(document, result_node.Value);
            }
            else
            {
                // if it isn't, apply the child components to the string value of the result
                ApplyComponents(document, result.ToString());
            }
        }

        /// <summary>
        /// Apply the components to the document
        /// </summary>
        /// <param name="document">The document to which the topics are applied</param>
        /// <param name="key">The document key</param>
        private void ApplyComponents(XmlDocument document, string key)
        {
            foreach(BuildComponent component in components)
                component.Apply(document, key);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(BuildComponent component in components)
                    component.Dispose();

            base.Dispose(disposing);
        }
    }
}
