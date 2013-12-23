// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/23/2012 - EFW - Added a dispose method to properly dispose of all components in each branch

using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component is used to clone the topic for each set of build components and execute them
    /// on the cloned topic.
    /// </summary>
    public class CloneComponent : BuildComponentCore
    {
        #region Private data members
        //=====================================================================

        private List<IEnumerable<BuildComponentCore>> branches = new List<IEnumerable<BuildComponentCore>>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        /// <remarks>Multiple <c>branch</c> elements are specified as the configuration.  Each <c>branch</c>
        /// element can contain one or more <c>component</c> definitions that will be created and executed when
        /// this component is applied.  Each branch receives a clone of the document.  This may be useful for
        /// generating multiple help output formats in one build configuration.</remarks>
        public CloneComponent(BuildAssemblerCore assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            XPathNodeIterator branchNodes = configuration.Select("branch");

            foreach(XPathNavigator branchNode in branchNodes)
                branches.Add(BuildAssembler.LoadComponents(branchNode));
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            foreach(IEnumerable<BuildComponentCore> branch in branches)
            {
                XmlDocument subdocument = document.Clone() as XmlDocument;

                foreach(BuildComponentCore component in branch)
                    component.Apply(subdocument, key);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(var branch in branches)
                    foreach(var component in branch)
                        component.Dispose();

            base.Dispose(disposing);
        }
        #endregion
    }
}
