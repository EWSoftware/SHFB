// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component executes a set of build components on the topic based on the result of an XPath
    /// expression.
    /// </summary>
    public class SwitchComponent : BuildComponent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public SwitchComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            // get the condition
            XPathNavigator condition_element = configuration.SelectSingleNode("switch");

            if(condition_element == null)
                throw new ConfigurationErrorsException("You must specifiy a condition using the <switch> statement with a 'value' attribute.");

            string condition_value = condition_element.GetAttribute("value", String.Empty);

            if(String.IsNullOrEmpty(condition_value))
                throw new ConfigurationErrorsException("The switch statement must have a 'value' attribute, which is an xpath expression.");

            condition = XPathExpression.Compile(condition_value);

            // load the component stacks for each case
            XPathNodeIterator case_elements = configuration.Select("case");

            foreach(XPathNavigator case_element in case_elements)
            {
                string case_value = case_element.GetAttribute("value", String.Empty);

                cases.Add(case_value, BuildAssembler.LoadComponents(case_element));
            }
        }

        // data held by the component

        private XPathExpression condition;

        private Dictionary<string, IEnumerable<BuildComponent>> cases = new Dictionary<string, IEnumerable<BuildComponent>>();

        // the action of the component

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // evaluate the condition
            string result = document.CreateNavigator().Evaluate(condition).ToString();

            // get the corresponding component stack
            IEnumerable<BuildComponent> components;

            if(cases.TryGetValue(result, out components))
            {
                // apply it
                foreach(BuildComponent component in components)
                    component.Apply(document, key);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(IEnumerable<BuildComponent> components in cases.Values)
                    foreach(BuildComponent component in components)
                        component.Dispose();

            base.Dispose(disposing);
        }
    }
}
