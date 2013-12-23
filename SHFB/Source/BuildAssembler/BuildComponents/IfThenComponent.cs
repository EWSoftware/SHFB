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
    /// This component is used to conditionally execute a set of components based on an XPath condition
    /// </summary>
    public class IfThenComponent : BuildComponentCore
    {
        #region Private data members
        //=====================================================================

        private XPathExpression condition;
        private IEnumerable<BuildComponentCore> true_branch = new List<BuildComponentCore>();
        private IEnumerable<BuildComponentCore> false_branch = new List<BuildComponentCore>();
        private BuildContext context;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public IfThenComponent(BuildAssemblerCore assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            // Get the condition
            XPathNavigator if_node = configuration.SelectSingleNode("if");

            if(if_node == null)
                throw new ConfigurationErrorsException("You must specify a condition using the <if> element.");

            string condition_xpath = if_node.GetAttribute("condition", String.Empty);

            if(String.IsNullOrEmpty(condition_xpath))
                throw new ConfigurationErrorsException("You must define a condition attribute on the <if> element");

            condition = XPathExpression.Compile(condition_xpath);

            // Construct the true branch
            XPathNavigator then_node = configuration.SelectSingleNode("then");

            if(then_node != null)
                true_branch = BuildAssembler.LoadComponents(then_node);

            // Construct the false branch
            XPathNavigator else_node = configuration.SelectSingleNode("else");

            if(else_node != null)
                false_branch = BuildAssembler.LoadComponents(else_node);

            // Keep a pointer to the context for future use
            context = assembler.Context;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Set up the test
            context["key"] = key;
            XPathExpression test = condition.Clone();
            test.SetContext(context.XsltContext);

            // Evaluate the condition
            bool result = (bool)document.CreateNavigator().Evaluate(test);

            // On the basis of the condition, execute either the true or the false branch
            if(result)
            {
                foreach(BuildComponentCore component in true_branch)
                    component.Apply(document, key);
            }
            else
            {
                foreach(BuildComponentCore component in false_branch)
                    component.Apply(document, key);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(BuildComponentCore component in true_branch)
                    component.Dispose();

                foreach(BuildComponentCore component in false_branch)
                    component.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
