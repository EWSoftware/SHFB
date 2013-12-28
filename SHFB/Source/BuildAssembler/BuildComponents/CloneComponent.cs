// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/23/2012 - EFW - Added a dispose method to properly dispose of all components in each branch
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

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
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Clone Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new CloneComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private List<IEnumerable<BuildComponentCore>> branches = new List<IEnumerable<BuildComponentCore>>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected CloneComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>Multiple <c>branch</c> elements are specified as the configuration.  Each <c>branch</c>
        /// element can contain one or more <c>component</c> definitions that will be created and executed when
        /// this component is applied.  Each branch receives a clone of the document.  This may be useful for
        /// generating multiple help output formats in one build configuration.</remarks>
        public override void Initialize(XPathNavigator configuration)
        {
            XPathNodeIterator branchNodes = configuration.Select("branch");

            foreach(XPathNavigator branchNode in branchNodes)
                branches.Add(this.BuildAssembler.LoadComponents(branchNode));
        }

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
