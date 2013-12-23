// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component executes a set of build components on the topic based on the result of an XPath
    /// expression.
    /// </summary>
    public class SwitchComponent : BuildComponentCore
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public SwitchComponent(BuildAssemblerCore assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            // get the condition
            XPathNavigator condition_element = configuration.SelectSingleNode("switch");

            if(condition_element == null)
                throw new ConfigurationErrorsException("You must specify a condition using the <switch> statement with a 'value' attribute.");

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

        private Dictionary<string, IEnumerable<BuildComponentCore>> cases = new Dictionary<string, IEnumerable<BuildComponentCore>>();

        // the action of the component

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // evaluate the condition
            string result = document.CreateNavigator().Evaluate(condition).ToString();

            // get the corresponding component stack
            IEnumerable<BuildComponentCore> components;

            if(cases.TryGetValue(result, out components))
            {
                // apply it
                foreach(BuildComponentCore component in components)
                    component.Apply(document, key);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(IEnumerable<BuildComponentCore> components in cases.Values)
                    foreach(BuildComponentCore component in components)
                        component.Dispose();

            base.Dispose(disposing);
        }
    }
}
