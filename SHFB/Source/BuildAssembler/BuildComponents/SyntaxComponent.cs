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

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

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
        [BuildComponentExport("Syntax Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <summary>
            /// This is used to import the list of syntax generator factories that is passed to the build
            /// component when it is created.
            /// </summary>
            [ImportMany(typeof(ISyntaxGeneratorFactory))]
            private List<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> SyntaxGenerators { get; set; }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SyntaxComponent(base.BuildAssembler, this.SyntaxGenerators);
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

                    generator.Initialize(generatorNode.Clone());
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
