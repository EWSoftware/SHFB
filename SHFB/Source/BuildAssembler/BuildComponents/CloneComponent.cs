// Copyright � Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/23/2012 - EFW - Added a dispose method to properly dispose of all components in each branch
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
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
                return new CloneComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly List<IEnumerable<BuildComponentCore>> branches = new List<IEnumerable<BuildComponentCore>>();

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
        /// <remarks>This sets a unique group ID for each branch</remarks>
        public override string GroupId
        {
            get => base.GroupId;
            set
            {
                base.GroupId = value;

                int branchId = 1;

                foreach(var branch in branches)
                {
                    string branchGroupId = value + "/" + branchId.ToString(CultureInfo.InvariantCulture);
                    branchId++;

                    foreach(var component in branch)
                        component.GroupId = branchGroupId;
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>Multiple <c>branch</c> elements are specified as the configuration.  Each <c>branch</c>
        /// element can contain one or more <c>component</c> definitions that will be created and executed when
        /// this component is applied.  Each branch receives a clone of the document.  This may be useful for
        /// generating multiple help output formats in one build configuration.</remarks>
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            XPathNodeIterator branchNodes = configuration.Select("branch");

            foreach(XPathNavigator branchNode in branchNodes)
                branches.Add(this.BuildAssembler.LoadComponents(branchNode));

            // Set a default group ID
            this.GroupId = null;
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            foreach(var branch in branches)
            {
                XmlDocument subdocument = (XmlDocument)document.Clone();

                foreach(var component in branch)
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
