// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
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
                return new IfThenComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XPathExpression condition;

        private readonly Dictionary<string, string> contextNamespaces = new Dictionary<string, string>();
        private IEnumerable<BuildComponentCore> trueBranch = new List<BuildComponentCore>();
        private IEnumerable<BuildComponentCore> falseBranch = new List<BuildComponentCore>();

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
            get => base.GroupId;
            set
            {
                base.GroupId = value;

                string groupId = value + "/True";

                foreach(var component in trueBranch)
                    component.GroupId = groupId;

                groupId = value + "/False";

                foreach(var component in falseBranch)
                    component.GroupId = groupId;
            }
        }

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Get the context namespaces
            XPathNodeIterator contextNodes = configuration.Select("context");

            foreach(XPathNavigator contextNode in contextNodes)
                contextNamespaces[contextNode.GetAttribute("prefix", String.Empty)] =
                    contextNode.GetAttribute("name", String.Empty);

            // Get the condition
            XPathNavigator ifNode = configuration.SelectSingleNode("if");

            if(ifNode == null)
                throw new ArgumentException("You must specify a condition using the <if> element.", nameof(configuration));

            string conditionXPath = ifNode.GetAttribute("condition", String.Empty);

            if(String.IsNullOrEmpty(conditionXPath))
                throw new ArgumentException("You must define a condition attribute on the <if> element", nameof(configuration));

            condition = XPathExpression.Compile(conditionXPath);

            // Construct the true branch
            XPathNavigator thenNode = configuration.SelectSingleNode("then");

            if(thenNode != null)
                trueBranch = BuildAssembler.LoadComponents(thenNode);

            // Construct the false branch
            XPathNavigator elseNode = configuration.SelectSingleNode("else");

            if(elseNode != null)
                falseBranch = BuildAssembler.LoadComponents(elseNode);

            // Set a default group ID
            this.GroupId = null;
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            // Set up the test
            CustomContext context = new CustomContext(contextNamespaces);
            context["key"] = key;

            XPathExpression test = condition.Clone();
            test.SetContext(context);

            // Evaluate the condition
            bool result = (bool)document.CreateNavigator().Evaluate(test);

            // On the basis of the condition, execute either the true or the false branch
            if(result)
            {
                foreach(BuildComponentCore component in trueBranch)
                    component.Apply(document, key);
            }
            else
            {
                foreach(BuildComponentCore component in falseBranch)
                    component.Apply(document, key);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(BuildComponentCore component in trueBranch)
                    component.Dispose();

                foreach(BuildComponentCore component in falseBranch)
                    component.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
