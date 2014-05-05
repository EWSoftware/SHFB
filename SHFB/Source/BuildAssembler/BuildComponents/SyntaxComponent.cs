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
// 04/27/2014 - EFW - Added support for grouping and sorting code snippets based on the order of the defined
// syntax generators.

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
using Microsoft.Ddue.Tools.Snippets;

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
            "filter languages selected in the project.  It can also group and sort code snippets based on the " +
            "order of the defined syntax generators.")]
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
                // Replace the existing instance for reference builds
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace, "Syntax Component");

                // Place it before the transform component in conceptual builds if not there already
                base.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component");
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
</generators>
<containerElement name=""codeSnippetGroup"" />";
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

        // Syntax generator data
        private XPathExpression syntaxInput, syntaxOutput;
        private bool renderReferenceLinks;

        private List<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> generatorFactories;
        private List<SyntaxGeneratorCore> generators = new List<SyntaxGeneratorCore>();

        // Code snippet grouping and sorting members
        private XmlNamespaceManager context;
        private XPathExpression referenceRoot, referenceCode, conceptualRoot, conceptualCode;
        private string containerElementName;
        private Dictionary<string, ISyntaxGeneratorMetadata> codeSnippetLanguages;
        private HashSet<string> generatorLanguages;

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

            generatorLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Generate a list of all possible language IDs for code snippets from the available generators
            codeSnippetLanguages = new Dictionary<string, ISyntaxGeneratorMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach(var factory in generatorFactories)
            {
                codeSnippetLanguages[factory.Metadata.Id] = factory.Metadata;

                foreach(string alternateId in factory.Metadata.AlternateIds.Split(new[] { ',', ' ', '\t' },
                  StringSplitOptions.RemoveEmptyEntries))
                    codeSnippetLanguages[alternateId] = factory.Metadata;
            }
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

                // Track the languages for grouping
                generatorLanguages.Add(generatorFactory.Metadata.Id);

                foreach(string alternateId in generatorFactory.Metadata.AlternateIds.Split(new[] { ',', ' ', '\t' },
                  StringSplitOptions.RemoveEmptyEntries))
                    generatorLanguages.Add(alternateId);

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

            var containerElement = configuration.SelectSingleNode("containerElement/@name");

            if(containerElement != null && !String.IsNullOrWhiteSpace(containerElement.Value))
            {
                containerElementName = containerElement.Value;

                // Create the XPath queries used for code snippet grouping and sorting
                context = new CustomContext();
                context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");
                referenceRoot = XPathExpression.Compile("document/comments");
                referenceCode = XPathExpression.Compile("//code");
                conceptualRoot = XPathExpression.Compile("document/topic");
                conceptualCode = XPathExpression.Compile("//ddue:code|//ddue:snippet");
                conceptualCode.SetContext(context);

                // Hook up the event handler to group and sort code snippets just prior to XSL transformation
                base.BuildAssembler.ComponentEvent += TransformComponent_TopicTransforming;
            }
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Don't bother if there is nothing to add (conceptual topics, overloads, group, namespace, and
            // project summary topics).
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

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to group and sort code snippets based on the order of the defined syntax generators
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>A two-phase approach is used to ensure that we don't have to be concerned about the
        /// placement of the syntax component in relation to other components that may insert code snippets.
        /// By running just prior to XSL transformation all other components that may insert code snippets will
        /// have been executed.</remarks>
        private void TransformComponent_TopicTransforming(object sender, EventArgs e)
        {
            XPathNavigator[] codeList;
            XmlElement nextElement;
            XmlAttribute attribute;
            XmlNode code;
            CodeSnippetGroup snippetGroup;
            ISyntaxGeneratorMetadata metadata;
            string namespaceUri;

            // Don't bother if not a transforming event
            TransformingTopicEventArgs tt = e as TransformingTopicEventArgs;

            if(tt == null)
                return;

            XPathNavigator root, navDoc = tt.Document.CreateNavigator();
            List<CodeSnippetGroup> allGroups = new List<CodeSnippetGroup>();

            // Select all code nodes.  The location depends on the build type.
            root = navDoc.SelectSingleNode(referenceRoot);

            // If not null, it's a reference (API) build.  If null, it's a conceptual build.
            if(root != null)
            {
                codeList = root.Select(referenceCode).ToArray();
                namespaceUri = String.Empty;
            }
            else
            {
                root = navDoc.SelectSingleNode(conceptualRoot);

                if(root == null)
                {
                    base.WriteMessage(tt.Key, MessageLevel.Warn, "Root content node not found.  Cannot group " +
                        "and sort code snippets.");
                    return;
                }

                codeList = root.Select(conceptualCode).ToArray();
                namespaceUri = "http://ddue.schemas.microsoft.com/authoring/2003/5";
            }

            // In the first pass, group all consecutive code snippets
            foreach(XPathNavigator navCode in codeList)
            {
                code = ((IHasXmlNode)navCode).GetNode();

                // If the parent is null, it was a sibling of a lead node and has already been handled.  If
                // the parent is another code element, ignore it as it's a nested code element which will be
                // replaced later with actual code by the Code Block Component.
                if(code.ParentNode == null || code.ParentNode.LocalName == "code")
                    continue;

                snippetGroup = new CodeSnippetGroup(tt.Document.CreateElement(containerElementName, namespaceUri));
                allGroups.Add(snippetGroup);

                code.ParentNode.InsertBefore(snippetGroup.SnippetGroupElement, code);

                while(code != null && (code.LocalName == "code" || code.LocalName == "snippet"))
                {
                    snippetGroup.CodeSnippets.Add(new CodeSnippet((XmlElement)code));

                    nextElement = code.NextSibling as XmlElement;
                    code.ParentNode.RemoveChild(code);
                    code = nextElement;
                }
            }

            // For each group, move snippets with a title or a language not in the syntax set into a new group by
            // themselves following the containing group and remove them from the containing group.
            foreach(var group in allGroups.ToList())
            {
                // Get the group insertion point and snippet count up front as they may change below
                int groupInsertionPoint = allGroups.IndexOf(group) + 1, snippetCount = group.CodeSnippets.Count;
                CodeSnippetGroup insertionPoint = group;

                foreach(var snippet in group.CodeSnippets.ToList())
                {
                    if(!codeSnippetLanguages.TryGetValue(snippet.Language, out metadata))
                    {
                        // If the language is not set or not recognized, put it in a standalone group without
                        // a title if one hasn't been set already.
                        snippet.LanguageElementName = snippet.KeywordStyleParameter = "Other";

                        if(snippet.CodeElement.Attributes["title"] == null)
                        {
                            attribute = tt.Document.CreateAttribute("title");
                            attribute.Value = " ";
                            snippet.CodeElement.Attributes.Append(attribute);
                        }
                    }
                    else
                    {
                        snippet.LanguageElementName = metadata.LanguageElementName;
                        snippet.KeywordStyleParameter = metadata.KeywordStyleParameter;
                        snippet.SortOrder = metadata.SortOrder;
                    }

                    attribute = tt.Document.CreateAttribute("codeLanguage");
                    attribute.Value = snippet.LanguageElementName;
                    snippet.CodeElement.Attributes.Append(attribute);

                    attribute = tt.Document.CreateAttribute("style");
                    attribute.Value = snippet.KeywordStyleParameter;
                    snippet.CodeElement.Attributes.Append(attribute);

                    if(snippetCount != 1 && (snippet.CodeElement.Attributes["title"] != null ||
                      !generatorLanguages.Contains(snippet.Language)))
                    {
                        snippetGroup = new CodeSnippetGroup(tt.Document.CreateElement(containerElementName, namespaceUri));
                        allGroups.Insert(groupInsertionPoint++, snippetGroup);

                        insertionPoint.SnippetGroupElement.ParentNode.InsertAfter(snippetGroup.SnippetGroupElement,
                            insertionPoint.SnippetGroupElement);
                        insertionPoint = snippetGroup;

                        group.CodeSnippets.Remove(snippet);
                        snippetGroup.CodeSnippets.Add(snippet);
                    }
                }

                // If all snippets were converted into standalone snippets, remove the group
                if(group.CodeSnippets.Count == 0)
                {
                    group.SnippetGroupElement.ParentNode.RemoveChild(group.SnippetGroupElement);
                    allGroups.Remove(group);
                }
            }

            // TODO: If there are multiple snippets in a group for a language, move duplicates into a new group.

            // TODO: Add "no example" snippets to groups that don't contain all of the syntax languages.  Perhaps
            // make this a configurable option.

            // And finally, add the snippets to the group container in sorted order
            foreach(var group in allGroups)
                foreach(var snippet in group.CodeSnippets.OrderBy(c => c.SortOrder))
                    group.SnippetGroupElement.AppendChild(snippet.CodeElement);
        }
        #endregion
    }
}
