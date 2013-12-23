// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project
// 03/17/2013 - EFW - Added support for the syntax writer RenderReferenceLinks property.  Added a condition to
// the Apply method to skip group, project, and namespace pages in which a syntax section is of no use.
// 12/22/2013 - EFW - Updated to use MEF to load the syntax generators

using System;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
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
        #region Private data members
        //=====================================================================

        private XPathExpression syntaxInput, syntaxOutput;
        private bool renderReferenceLinks;
        private List<SyntaxGeneratorBase> generators = new List<SyntaxGeneratorBase>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public SyntaxComponent(BuildAssemblerCore assembler, XPathNavigator configuration) :
          base(assembler, configuration)
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

            // TODO: Once SyntaxComponent is itself a MEF component, the syntax generators will be loaded
            // via an import.  For now, we need to get them manually.
            using(var container = new CompositionContainer(new DirectoryCatalog(Path.GetDirectoryName(
              this.GetType().Assembly.Location))))
            {
                var syntaxGenerators = container.GetExports<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>();

                foreach(XPathNavigator generatorNode in generatorNodes)
                {
                    // Get the ID of the syntax generator
                    string id = generatorNode.GetAttribute("id", String.Empty);

                    if(String.IsNullOrWhiteSpace(id))
                        base.WriteMessage(MessageLevel.Error, "Each generator element must have an id attribute");

                    var generatorFactory = syntaxGenerators.FirstOrDefault(g => g.Metadata.Id == id);

                    if(generatorFactory == null)
                        base.WriteMessage(MessageLevel.Error, "A syntax generator with the ID '{0}' could not" +
                            "be found");

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
            }

            base.WriteMessage(MessageLevel.Info, "Loaded {0} syntax generators.", generators.Count);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Don't bother if there is nothing to add (group, namespace, and project topics)
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
