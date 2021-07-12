// Copyright � Microsoft Corporation.
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
// 03/28/2018 - EFW - Made some changes to set the title to the language ID for unrecognized languages

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

using Sandcastle.Tools.BuildComponents.Snippets;

namespace Sandcastle.Tools.BuildComponents
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
        [BuildComponentExport("Syntax Component", IsVisible = true, Version = AssemblyInfo.ProductVersion,
          Copyright = AssemblyInfo.Copyright, Description = "This build component is used to create syntax " +
            "sections in topics using the syntax filter languages selected in the project.  It can also group " +
            "and sort code snippets based on the order of the defined syntax generators.")]
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
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace, "Syntax Component");

                // Place it before the transform component in conceptual builds if not there already
                this.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SyntaxComponent(this.BuildAssembler, this.SyntaxGenerators);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration =>
@"<syntax input=""/document/reference"" output=""/document/syntax"" renderReferenceLinks=""false"" />
<generators>
    {@SyntaxFilters}
</generators>
<containerElement name=""codeSnippetGroup"" addNoExampleTabs=""true"" includeOnSingleSnippets=""false""
    groupingEnabled=""{@CodeSnippetGrouping}"" />";
        }
        #endregion

        #region Private data members
        //=====================================================================

        // Syntax generator data
        private XPathExpression syntaxInput, syntaxOutput;
        private bool renderReferenceLinks, addNoExampleTabs, includeOnSingleSnippets;

        private readonly List<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> generatorFactories;
        private readonly List<SyntaxGeneratorCore> generators = new List<SyntaxGeneratorCore>();

        // Code snippet grouping and sorting members
        private XPathExpression referenceRoot, referenceCode, conceptualRoot, conceptualCode;
        private string containerElementName;
        private readonly Dictionary<string, ISyntaxGeneratorMetadata> codeSnippetLanguages;
        private readonly Dictionary<string, int> languageOrder;
        private readonly HashSet<string> generatorLanguages;
        private readonly List<ISyntaxGeneratorMetadata> languageSet;

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
            this.generatorFactories = generatorFactories ?? throw new ArgumentNullException(nameof(generatorFactories));

            generatorLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            languageSet = new List<ISyntaxGeneratorMetadata>();
            languageOrder = new Dictionary<string, int>();

            // Generate a list of all possible language IDs for code snippets from the available generators
            codeSnippetLanguages = new Dictionary<string, ISyntaxGeneratorMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach(var factory in generatorFactories)
            {
                codeSnippetLanguages[factory.Metadata.Id] = factory.Metadata;

                foreach(string alternateId in (factory.Metadata.AlternateIds ?? String.Empty).Split(
                  new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    codeSnippetLanguages[alternateId] = factory.Metadata;
                }
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            XPathNavigator syntaxNode = configuration.SelectSingleNode("syntax");
            string syntaxInputXPath = syntaxNode.GetAttribute("input", String.Empty);

            if(String.IsNullOrEmpty(syntaxInputXPath))
                throw new ArgumentException("You must specify an XPath for input in the syntax element",
                    nameof(configuration));

            syntaxInput = XPathExpression.Compile(syntaxInputXPath);

            string syntaxOutputXPath = syntaxNode.GetAttribute("output", String.Empty);

            if(String.IsNullOrEmpty(syntaxOutputXPath))
                throw new ArgumentException("You must specify an XPath for output in the syntax element",
                    nameof(configuration));

            syntaxOutput = XPathExpression.Compile(syntaxOutputXPath);

            string attrValue = syntaxNode.GetAttribute("renderReferenceLinks", String.Empty);

            if(String.IsNullOrWhiteSpace(attrValue) || !Boolean.TryParse(attrValue, out renderReferenceLinks))
                renderReferenceLinks = false;

            XPathNodeIterator generatorNodes = configuration.Select("generators/generator");

            // Configuration changes are stored separately since the actual generators may be added to the
            // configuration file at build time.  Substitution of the edited configuration is easier to do here.
            var generatorConfigs = configuration.SelectSingleNode("configurations");

            // If we have configuration nodes, note the order of the syntax generators.  These will be used to
            // order the snippets.
            if(generatorConfigs != null)
            {
                int order = 1;

                foreach(XPathNavigator id in generatorConfigs.Select("generator/@id"))
                    languageOrder.Add(id.Value, order++);
            }

            foreach(XPathNavigator generatorNode in generatorNodes)
            {
                // Get the ID of the syntax generator
                string id = generatorNode.GetAttribute("id", String.Empty);

                if(String.IsNullOrWhiteSpace(id))
                    this.WriteMessage(MessageLevel.Error, "Each generator element must have an id attribute");

                var generatorFactory = generatorFactories.FirstOrDefault(g => g.Metadata.Id == id);

                if(generatorFactory == null)
                    this.WriteMessage(MessageLevel.Error, "A syntax generator with the ID '{0}' could not be found", id);

                // Track the languages for grouping
                generatorLanguages.Add(generatorFactory.Metadata.Id);
                languageSet.Add(generatorFactory.Metadata);

                foreach(string alternateId in (generatorFactory.Metadata.AlternateIds ?? String.Empty).Split(
                  new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    generatorLanguages.Add(alternateId);

                try
                {
                    var generator = generatorFactory.Value.Create();
                    var configNode = generatorNode.Clone();

                    if(generatorConfigs != null)
                    {
                        var alternateConfig = generatorConfigs.SelectSingleNode("generator[@id='" + id + "']");

                        if(alternateConfig != null && alternateConfig.HasChildren)
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
                    this.WriteMessage(MessageLevel.Error, "An error occurred while attempting to instantiate " +
                        "the '{0}' syntax generator. The error message is: {1}{2}", id, ex.Message,
                        ex.InnerException != null ? "\r\n" + ex.InnerException.Message : String.Empty);
                }
            }

            this.WriteMessage(MessageLevel.Info, "Loaded {0} syntax generators.", generators.Count);

            // If this is not found or set, we'll assume the presentation style does not support grouping
            var containerElement = configuration.SelectSingleNode("containerElement");

            if(containerElement != null)
            {
                // If grouping is disabled, skip the remainder of the set up.  This will happen if the user adds
                // a custom configuration to a project for a presentation style that doesn't support it.
                containerElementName = containerElement.GetAttribute("name", String.Empty);
                attrValue = containerElement.GetAttribute("groupingEnabled", String.Empty);

                if(String.IsNullOrWhiteSpace(attrValue) || !Boolean.TryParse(attrValue, out bool groupingEnabled))
                    groupingEnabled = false;

                if(!groupingEnabled || String.IsNullOrWhiteSpace(containerElementName))
                    return;

                // Get the "no example tab" options
                attrValue = containerElement.GetAttribute("addNoExampleTabs", String.Empty);

                if(String.IsNullOrWhiteSpace(attrValue) || !Boolean.TryParse(attrValue, out addNoExampleTabs))
                    addNoExampleTabs = true;

                attrValue = containerElement.GetAttribute("includeOnSingleSnippets", String.Empty);

                if(String.IsNullOrWhiteSpace(attrValue) || !Boolean.TryParse(attrValue, out includeOnSingleSnippets))
                    includeOnSingleSnippets = false;

                // Create the XPath queries used for code snippet grouping and sorting
                var context = new CustomContext();

                context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");

                referenceRoot = XPathExpression.Compile("document/comments|document/syntax");
                referenceCode = XPathExpression.Compile("//code|//div[@codeLanguage]");

                conceptualRoot = XPathExpression.Compile("document/topic");
                conceptualCode = XPathExpression.Compile("//ddue:code|//ddue:snippet");
                conceptualCode.SetContext(context);

                // Hook up the event handler to group and sort code snippets just prior to XSL transformation
                this.BuildAssembler.ComponentEvent += TransformComponent_TopicTransforming;
            }
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            if(key == null)
                throw new ArgumentNullException(nameof(key));

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
            XmlAttribute attribute;
            XmlNode code, nextNode;
            CodeSnippetGroup snippetGroup;
            string namespaceUri;
            int order;

            // Don't bother if not a transforming event or not in our group
            if(!(e is TransformingTopicEventArgs tt) || ((BuildComponentCore)sender).GroupId != this.GroupId)
                return;

            XmlDocument document = tt.Document;
            XPathNavigator root, navDoc = document.CreateNavigator();
            List<CodeSnippetGroup> allGroups = new List<CodeSnippetGroup>();
            List<List<CodeSnippet>> extraGroups = new List<List<CodeSnippet>>();

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
                    this.WriteMessage(tt.Key, MessageLevel.Warn, "Root content node not found.  Cannot group " +
                        "and sort code snippets.");
                    return;
                }

                codeList = root.Select(conceptualCode).ToArray();
                namespaceUri = "http://ddue.schemas.microsoft.com/authoring/2003/5";
            }

            if(codeList.Length == 0)
                return;

            // In the first pass, group all consecutive code snippets
            foreach(XPathNavigator navCode in codeList)
            {
                code = ((IHasXmlNode)navCode).GetNode();

                // If the parent is null, it was a sibling of a lead node and has already been handled.  If
                // the parent is another code element, ignore it as it's a nested code element which will be
                // replaced later with actual code by the Code Block Component.
                if(code.ParentNode == null || code.ParentNode.LocalName == "code")
                    continue;

                if(code.LocalName != "div")
                {
                    snippetGroup = new CodeSnippetGroup(document.CreateElement(containerElementName, namespaceUri));
                    allGroups.Add(snippetGroup);

                    code.ParentNode.InsertBefore(snippetGroup.SnippetGroupElement, code);
                }
                else
                {
                    // A div indicates a syntax section so we must reuse the parent element rather than replace
                    // it as they are already grouped.
                    snippetGroup = new CodeSnippetGroup((XmlElement)code.ParentNode) { IsSyntaxSection = true };
                    allGroups.Add(snippetGroup);
                }

                // Keep going until we hit a text node or a non-code element
                while(code != null && (code.LocalName == "code" || code.LocalName == "snippet" ||
                  code.LocalName == "div"))
                {
                    snippetGroup.CodeSnippets.Add(new CodeSnippet((XmlElement)code));

                    nextNode = code.NextSibling;
                    code.ParentNode.RemoveChild(code);

                    // Skip intervening comment nodes as they may incorrectly split a group
                    while(nextNode is XmlComment)
                        nextNode = nextNode.NextSibling;

                    code = nextNode as XmlElement;
                }
            }

            foreach(var group in allGroups.ToList())
            {
                // Get the group insertion point and snippet count up front as they may change below
                int groupInsertionPoint = allGroups.IndexOf(group) + 1, snippetCount = group.CodeSnippets.Count;
                CodeSnippetGroup insertionPoint = group;

                // For each group, move snippets with a title or a language not in the syntax set into a new
                // group by themselves following the containing group and remove them from the containing group.
                foreach(var snippet in group.CodeSnippets.ToList())
                {
                    if(!codeSnippetLanguages.TryGetValue(snippet.Language, out ISyntaxGeneratorMetadata metadata))
                    {
                        // If the language is not set or not recognized, put it in a standalone group without
                        // a title if one hasn't been set already.
                        snippet.LanguageElementName = snippet.KeywordStyleParameter = "Other";

                        if(snippet.CodeElement.Attributes["title"] == null)
                        {
                            string friendlyName = " ";

                            if(!snippet.Language.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                              !snippet.Language.Equals("other", StringComparison.OrdinalIgnoreCase))
                            {
                                // If we have a set of shared language IDs, see if it's in there so that we can
                                // get a human-readable name.  If not, we'll use the language ID as the title.
                                var languageIds = (IReadOnlyDictionary<string, string>)BuildComponentCore.Data["LanguageIds"];

                                if(languageIds == null || !languageIds.TryGetValue(snippet.Language, out friendlyName))
                                    friendlyName = snippet.Language;
                            }

                            attribute = document.CreateAttribute("title");
                            attribute.Value = friendlyName;
                            snippet.CodeElement.Attributes.Append(attribute);
                        }
                    }
                    else
                    {
                        snippet.LanguageElementName = metadata.LanguageElementName;
                        snippet.KeywordStyleParameter = metadata.KeywordStyleParameter;

                        if(!languageOrder.TryGetValue(metadata.Id, out order))
                            order = metadata.SortOrder;

                        snippet.SortOrder = order;
                    }

                    attribute = document.CreateAttribute("codeLanguage");
                    attribute.Value = snippet.LanguageElementName;
                    snippet.CodeElement.Attributes.Append(attribute);

                    attribute = document.CreateAttribute("style");
                    attribute.Value = snippet.KeywordStyleParameter;
                    snippet.CodeElement.Attributes.Append(attribute);

                    if(snippet.CodeElement.Attributes["title"] != null || !generatorLanguages.Contains(snippet.Language))
                        if(snippetCount != 1)
                        {
                            snippetGroup = new CodeSnippetGroup(document.CreateElement(containerElementName,
                                namespaceUri)) { IsStandalone = true };

                            allGroups.Insert(groupInsertionPoint++, snippetGroup);

                            insertionPoint.SnippetGroupElement.ParentNode.InsertAfter(snippetGroup.SnippetGroupElement,
                                insertionPoint.SnippetGroupElement);
                            insertionPoint = snippetGroup;

                            group.CodeSnippets.Remove(snippet);
                            snippetGroup.CodeSnippets.Add(snippet);
                        }
                        else
                            group.IsStandalone = true;
                }

                // If all snippets were converted into standalone snippets, remove the group
                if(group.CodeSnippets.Count == 0)
                {
                    group.SnippetGroupElement.ParentNode.RemoveChild(group.SnippetGroupElement);
                    allGroups.Remove(group);
                    continue;
                }

                extraGroups.Clear();
                groupInsertionPoint = allGroups.IndexOf(group) + 1;
                insertionPoint = group;

                // If there are multiple snippets for a language in a group, move the duplicates into a new group
                if(!group.IsStandalone && group.CodeSnippets.Count != 1)
                {
                    // There can be multiple language IDs that map to the same keyword style so group by that
                    foreach(var langSet in group.CodeSnippets.GroupBy(c => c.KeywordStyleParameter).Where(
                      g => g.Count() != 1).ToList())
                    {
                        // For syntax sections, merge common language snippets into a single section
                        if(group.IsSyntaxSection)
                        {
                            var firstSnippet = langSet.First();

                            foreach(var snippet in langSet.Skip(1))
                            {
                                firstSnippet.CodeElement.InnerXml += "\r\n\r\n" + snippet.CodeElement.InnerXml;
                                group.CodeSnippets.Remove(snippet);
                            }

                            continue;
                        }

                        // For each duplicate, find a group that doesn't contain the language or create
                        // a new group and put it in that one.
                        foreach(var snippet in langSet.Skip(1))
                        {
                            var moveToGroup = extraGroups.FirstOrDefault(eg => !eg.Any(
                                c => c.Language == snippet.Language));

                            if(moveToGroup == null)
                            {
                                moveToGroup = new List<CodeSnippet>();
                                extraGroups.Add(moveToGroup);
                            }

                            moveToGroup.Add(snippet);
                            group.CodeSnippets.Remove(snippet);
                        }
                    }

                    foreach(var set in extraGroups)
                    {
                        snippetGroup = new CodeSnippetGroup(document.CreateElement(containerElementName,
                            namespaceUri));

                        allGroups.Insert(groupInsertionPoint++, snippetGroup);

                        insertionPoint.SnippetGroupElement.ParentNode.InsertAfter(snippetGroup.SnippetGroupElement,
                            insertionPoint.SnippetGroupElement);
                        insertionPoint = snippetGroup;
                        snippetGroup.CodeSnippets.AddRange(set);
                    }
                }
            }

            // We've got all the necessary groups now so make the final pass
            foreach(var group in allGroups)
            {
                // If wanted, add "no example" snippets to groups that don't contain all of the syntax languages.
                // Standalone groups are always excluded.  Single snippets are excluded if so indicated.
                if(addNoExampleTabs && !group.IsStandalone && (group.CodeSnippets.Count > 1 || includeOnSingleSnippets))
                    foreach(var style in languageSet.Select(l => l.KeywordStyleParameter).Except(
                      group.CodeSnippets.Select(c => c.KeywordStyleParameter)))
                    {
                        var language = languageSet.First(l => l.KeywordStyleParameter == style);

                        var noExample = document.CreateElement(group.CodeSnippets[0].CodeElement.LocalName,
                            namespaceUri);

                        attribute = document.CreateAttribute("language");
                        attribute.Value = language.KeywordStyleParameter;
                        noExample.Attributes.Append(attribute);

                        attribute = document.CreateAttribute("codeLanguage");
                        attribute.Value = language.LanguageElementName;
                        noExample.Attributes.Append(attribute);

                        attribute = document.CreateAttribute("style");
                        attribute.Value = language.KeywordStyleParameter;
                        noExample.Attributes.Append(attribute);

                        attribute = document.CreateAttribute("phantom");
                        attribute.Value = "true";
                        noExample.Attributes.Append(attribute);

                        if(!languageOrder.TryGetValue(language.Id, out order))
                            order = language.SortOrder;

                        group.CodeSnippets.Add(new CodeSnippet(noExample) { SortOrder = order });
                    }

                // And finally, add the snippets to the group container in sorted order
                foreach(var snippet in group.CodeSnippets.OrderBy(c => c.SortOrder))
                    group.SnippetGroupElement.AppendChild(snippet.CodeElement);
            }
        }
        #endregion
    }
}
