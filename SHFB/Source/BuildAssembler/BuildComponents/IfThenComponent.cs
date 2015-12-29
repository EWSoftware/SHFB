// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This component is used to conditionally execute a set of components based on an XPath condition
    /// </summary>
    public class IfThenComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("If Then Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new IfThenComponent(base.BuildAssembler);
            }
        }
        #endregion

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
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected IfThenComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This sets a unique group ID for each branch</remarks>
        public override string GroupId
        {
            get { return base.GroupId; }
            set
            {
                base.GroupId = value;

                string groupId = value + "/True";

                foreach(var component in true_branch)
                    component.GroupId = groupId;

                groupId = value + "/False";

                foreach(var component in false_branch)
                    component.GroupId = groupId;
            }
        }

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
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
            context = this.BuildAssembler.Context;

            // Set a default group ID
            this.GroupId = null;
        }

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
