// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This build component executes a set of build components on the topic based on the result of an XPath
    /// expression.
    /// </summary>
    public class SwitchComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Switch Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SwitchComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XPathExpression condition;
        private readonly Dictionary<string, IEnumerable<BuildComponentCore>> cases = [];

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected SwitchComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
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

                foreach(var keyValue in cases)
                {
                    string caseGroupId = value + "/" + keyValue.Key;

                    foreach(var component in keyValue.Value)
                        component.GroupId = caseGroupId;
                }
            }
        }

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Get the condition
            XPathNavigator conditionElement = configuration.SelectSingleNode("switch") ??
                throw new ArgumentException("You must specify a condition using the <switch> statement with " +
                    "a 'value' attribute.", nameof(configuration));

            string conditionValue = conditionElement.GetAttribute("value", String.Empty);

            if(String.IsNullOrEmpty(conditionValue))
            {
                throw new ArgumentException("The switch statement must have a 'value' attribute, which is " +
                    "an xpath expression.", nameof(configuration));
            }

            condition = XPathExpression.Compile(conditionValue);

            // Load the component stacks for each case
            XPathNodeIterator caseElements = configuration.Select("case");

            foreach(XPathNavigator caseElement in caseElements)
            {
                string caseValue = caseElement.GetAttribute("value", String.Empty);

                this.WriteMessage(MessageLevel.Info, "Loading components for " + caseValue + " case");

                cases.Add(caseValue, BuildAssembler.LoadComponents(caseElement));
            }

            // Set a default group ID
            this.GroupId = null;
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            // Evaluate the condition
            string result = document.CreateNavigator().Evaluate(condition).ToString();

            // Get the corresponding component stack
            if(cases.TryGetValue(result, out IEnumerable<BuildComponentCore> components))
            {
                // Apply it
                foreach(BuildComponentCore component in components)
                    component.Apply(document, key);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(IEnumerable<BuildComponentCore> components in cases.Values)
                {
                    foreach(BuildComponentCore component in components)
                        component.Dispose();
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
