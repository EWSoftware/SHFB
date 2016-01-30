// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This component is used to execute a set of components on the topic
    /// </summary>
    public class ForEachComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("For Each Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ForEachComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XPathExpression xPath;

        private Dictionary<string, string> contextNamespaces = new Dictionary<string, string>();
        private IEnumerable<BuildComponentCore> components;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected ForEachComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This sets the group ID for each subcomponent</remarks>
        public override string GroupId
        {
            get { return base.GroupId; }
            set
            {
                base.GroupId = value;

                foreach(var component in components)
                    component.GroupId = value;
            }
        }

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            // Get the context namespaces
            XPathNodeIterator contextNodes = configuration.Select("context");

            foreach(XPathNavigator contextNode in contextNodes)
                contextNamespaces[contextNode.GetAttribute("prefix", String.Empty)] =
                    contextNode.GetAttribute("name", String.Empty);

            // Load the expression format
            XPathNavigator variableNode = configuration.SelectSingleNode("variable");

            if(variableNode == null)
                throw new ConfigurationErrorsException("When instantiating a ForEach component, you must " +
                    "specify a variable using the <variable> element.");

            string xpathFormat = variableNode.GetAttribute("expression", String.Empty);

            if(String.IsNullOrWhiteSpace(xpathFormat))
                throw new ConfigurationErrorsException("When instantiating a ForEach component, you must " +
                    "specify a variable expression using the expression attribute");

            xPath = XPathExpression.Compile(xpathFormat);

            // Load the subcomponents
            WriteMessage(MessageLevel.Info, "Loading subcomponents.");
            XPathNavigator componentsNode = configuration.SelectSingleNode("components");

            if(componentsNode == null)
                throw new ConfigurationErrorsException("When instantiating a ForEach component, you must " +
                    "specify subcomponents using the <components> element.");

            components = BuildAssembler.LoadComponents(componentsNode);

            WriteMessage(MessageLevel.Info, "Loaded {0} subcomponents.", components.Count());
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Set the context
            CustomContext context = new CustomContext(contextNamespaces);
            context["key"] = key;

            // Evaluate the condition
            XPathExpression xPathLocal = xPath.Clone();
            xPathLocal.SetContext(context);

            object result = document.CreateNavigator().Evaluate(xPathLocal);

            // Try to interpret the result as a node set
            XPathNodeIterator resultNodeIterator = result as XPathNodeIterator;

            if(resultNodeIterator != null)
            {
                // If it is, apply the child components to each node value
                foreach(XPathNavigator resultNode in resultNodeIterator.ToArray())
                    ApplyComponents(document, resultNode.Value);
            }
            else
            {
                // If it isn't, apply the child components to the string value of the result
                ApplyComponents(document, result.ToString());
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(BuildComponentCore component in components)
                    component.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Apply the components to the document
        /// </summary>
        /// <param name="document">The document to which the topics are applied</param>
        /// <param name="key">The document key</param>
        private void ApplyComponents(XmlDocument document, string key)
        {
            foreach(BuildComponentCore component in components)
                component.Apply(document, key);
        }
        #endregion
    }
}
