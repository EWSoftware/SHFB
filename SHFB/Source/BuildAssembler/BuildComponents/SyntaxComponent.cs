// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project
// 03/17/2013 - EFW - Added support for the syntax writer RenderReferenceLinks property.  Added a condition to
// the Apply method to skip group, project, and namespace pages in which a syntax section is of no use.
// 12/22/2013 - EFW - Updated to use MEF to load the syntax generators
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF
// 01/24/2014 - EFW - Updated the component to be configurable so that the syntax generator configurations can
// be edited.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

using Microsoft.Ddue.Tools.UI;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component is used to generate syntax sections for API member topics
    /// </summary>
    public class SyntaxComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Syntax Component", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This build component is used to create syntax sections in topics using the syntax " +
            "filter languages selected in the project.")]
        public sealed class Factory : BuildComponentFactory
        {
            // This is used to import the list of syntax generator factories that is passed to the build
            // component when it is created.
            [ImportMany(typeof(ISyntaxGeneratorFactory))]
            private List<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> SyntaxGenerators { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public Factory()
            {
                // Replace the existing instance
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace, "Syntax Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SyntaxComponent(base.BuildAssembler, this.SyntaxGenerators);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<syntax input=""/document/reference"" output=""/document/syntax"" renderReferenceLinks=""false"" />
<generators>
    {@SyntaxFilters}
</generators>";
                }
            }

            /// <inheritdoc />
            public override string ConfigureComponent(string currentConfiguration, CompositionContainer container)
            {
                using(var dlg = new SyntaxComponentConfigDlg(currentConfiguration, container))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        currentConfiguration = dlg.Configuration;
                }

                return currentConfiguration;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XPathExpression syntaxInput, syntaxOutput;
        private bool renderReferenceLinks;

        private List<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> generatorFactories;
        private List<SyntaxGeneratorCore> generators = new List<SyntaxGeneratorCore>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        /// <param name="generatorFactories">The list of available syntax generator factory components</param>
        protected SyntaxComponent(BuildAssemblerCore buildAssembler,
          List<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> generatorFactories) : base(buildAssembler)
        {
            this.generatorFactories = generatorFactories;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            XPathNavigator syntaxNode = configuration.SelectSingleNode("syntax");
            string syntaxInputXPath = syntaxNode.GetAttribute("input", String.Empty);

            if(String.IsNullOrEmpty(syntaxInputXPath))
                throw new ConfigurationErrorsException("You must specify an XPath for input in the syntax element");

            syntaxInput = XPathExpression.Compile(syntaxInputXPath);

            string syntaxOutputXPath = syntaxNode.GetAttribute("output", String.Empty);

            if(String.IsNullOrEmpty(syntaxOutputXPath))
                throw new ConfigurationErrorsException("You must specify an XPath for output in the syntax element");

            syntaxOutput = XPathExpression.Compile(syntaxOutputXPath);

            string renderLinks = syntaxNode.GetAttribute("renderReferenceLinks", String.Empty);

            if(String.IsNullOrWhiteSpace(renderLinks) || !Boolean.TryParse(renderLinks, out renderReferenceLinks))
                renderReferenceLinks = false;

            XPathNodeIterator generatorNodes = configuration.Select("generators/generator");

            // Configuration changes are stored separately since the actual generators may be added to the
            // configuration file at build time.  Substitution of the edited configuration is easier to do here.
            var generatorConfigs = configuration.SelectSingleNode("configurations");

            foreach(XPathNavigator generatorNode in generatorNodes)
            {
                // Get the ID of the syntax generator
                string id = generatorNode.GetAttribute("id", String.Empty);

                if(String.IsNullOrWhiteSpace(id))
                    base.WriteMessage(MessageLevel.Error, "Each generator element must have an id attribute");

                var generatorFactory = generatorFactories.FirstOrDefault(g => g.Metadata.Id == id);

                if(generatorFactory == null)
                    base.WriteMessage(MessageLevel.Error, "A syntax generator with the ID '{0}' could not be found", id);

                try
                {
                    var generator = generatorFactory.Value.Create();
                    var configNode = generatorNode.Clone();

                    if(generatorConfigs != null)
                    {
                        var alternateConfig = generatorConfigs.SelectSingleNode("generator[@id='" + id + "']");

                        if(alternateConfig != null)
                        {
                            // Since there may be custom attributes on the generator node, we'll make a copy and
                            // substitute the child elements that make up the configuration.
                            var alternate = XElement.Parse(alternateConfig.OuterXml);
                            var genNode = XElement.Parse(configNode.OuterXml);

                            genNode.RemoveNodes();
                            genNode.Add(alternate.Elements());

                            configNode = genNode.CreateNavigator();
                        }
                    }

                    generator.Initialize(configNode);
                    generators.Add(generator);
                }
                catch(Exception ex)
                {
                    base.WriteMessage(MessageLevel.Error, "An error occurred while attempting to instantiate " +
                        "the '{0}' syntax generator. The error message is: {1}{2}", id, ex.Message,
                        ex.InnerException != null ? "\r\n" + ex.InnerException.Message : String.Empty);
                }
            }

            base.WriteMessage(MessageLevel.Info, "Loaded {0} syntax generators.", generators.Count);
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Don't bother if there is nothing to add (overloads, group, namespace, and project topics)
            if(key[1] == ':' && key[0] != 'G' && key[0] != 'N' && key[0] != 'R')
            {
                XPathNavigator input = document.CreateNavigator().SelectSingleNode(syntaxInput);
                XPathNavigator output = document.CreateNavigator().SelectSingleNode(syntaxOutput);
                SyntaxWriter writer = new ManagedSyntaxWriter(output) { RenderReferenceLinks = renderReferenceLinks };

                foreach(var generator in generators)
                    generator.WriteSyntax(input, writer);
            }
        }
        #endregion
    }
}
