// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Snippets;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// SnippetComponent class to replace the snippet code references.
    /// </summary>
    public class SnippetComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Snippet Code Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SnippetComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private members
        /// <summary>
        /// Regex to validate the snippet references.
        /// </summary>
        private static Regex validSnippetReference = new Regex(
            @"^[^#\a\b\f\n\r\t\v]+#(\w+,)*\w+$",
            RegexOptions.Compiled);

        /// <summary>
        /// Dictionary to map language folder names to language id.
        /// </summary>
        private static Dictionary<string, string> languageMap = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// List that controls the order in which languages snippets are displayed.
        /// </summary>
        private static List<string> languageList = new List<string>();

        /// <summary>
        /// Dictionary consisting of example name as key and example path as value.
        /// </summary>
        private Dictionary<string, string> exampleIndex = new Dictionary<string, string>();

        /// <summary>
        /// Dictionary consisting of exampleName\unitName as key with a null value.
        /// </summary>
        private Dictionary<string, string> approvedSnippetIndex = new Dictionary<string, string>();

        /// <summary>
        /// Dictionary containing the example name as key and list of rejected language snippets as values.
        /// </summary>
        private Dictionary<string, List<string>> rejectedSnippetIndex = new Dictionary<string, List<string>>();

        /// <summary>
        /// List of unit folder names to exclude from sample parsing.
        /// </summary>
        private Dictionary<string, Object> excludedUnits = new Dictionary<string, Object>();

        /// <summary>
        /// Dictionary consisting of exampleName\unitName as key with a null value.
        /// </summary>
        private SnippetCache snippetCache = null;

        /// <summary>
        /// XPathExpression to look for snippet references in the topics.
        /// </summary>
        private XPathExpression selector;

        /// <summary>
        /// XmlNamespaceManager to set the context.
        /// </summary>
        private XmlNamespaceManager context = new CustomContext();

        /// <summary>
        /// List of languages.
        /// </summary>
        private List<Language> languages = new List<Language>();

        /// <summary>
        /// snippet store.
        /// </summary>
        private Dictionary<SnippetIdentifier, List<Snippet>> snippets = new Dictionary<SnippetIdentifier, List<Snippet>>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected SnippetComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            // Get the parsnip examples location.
            XPathNodeIterator examplesNode = configuration.Select("examples/example");

            if(examplesNode.Count == 0)
                WriteMessage(MessageLevel.Error, "Each snippet component element must have a child element named 'examples' containing an element named 'example' with an attribute named 'directory', whose value is a path to the directory containing examples.");

            foreach(XPathNavigator exampleNode in examplesNode)
            {
                string rootDirectory = exampleNode.GetAttribute("directory", string.Empty);

                if(string.IsNullOrEmpty(rootDirectory))
                    WriteMessage(MessageLevel.Error, "Each examples element must have a directory attribute specifying a directory containing parsnip samples.");

                rootDirectory = Environment.ExpandEnvironmentVariables(rootDirectory);
                if(!Directory.Exists(rootDirectory))
                    WriteMessage(MessageLevel.Error, "The examples/@directory attribute specified a directory that doesn't exist: '{0}'", rootDirectory);

                // create a dictionary that maps the example names to the example path under the root directory
                this.LoadExamples(rootDirectory);
            }

            // Get the approved log files location.
            XPathNodeIterator approvedSnippetsNode = configuration.Select("approvalLogs/approvalLog");

            if(approvedSnippetsNode.Count == 0)
                WriteMessage(MessageLevel.Warn, "The config did not have an 'approvalLogs' node to specify a snippet approval logs.");

            foreach(XPathNavigator node in approvedSnippetsNode)
            {
                string approvalLogFile = node.GetAttribute("file", string.Empty);

                if(string.IsNullOrEmpty(approvalLogFile))
                    WriteMessage(MessageLevel.Error, "The approvalLog node must have a 'file' attribute specifying the path to a snippet approval log.");

                approvalLogFile = Environment.ExpandEnvironmentVariables(approvalLogFile);
                if(!File.Exists(approvalLogFile))
                    WriteMessage(MessageLevel.Error, "The approvalLog/@file attribute specified a file that doesn't exist: '{0}'", approvalLogFile);

                // load the approval log into the approvedSnippetIndex dictionary
                this.ParseApprovalLogFiles(approvalLogFile);
            }

            // Get the names of the unit directories in the sample tree to exclude from parsing
            //     <excludedUnits><unitFolder name="CPP_OLD" /></excludedUnits>
            XPathNodeIterator excludedUnitNodes = configuration.Select("excludedUnits/unitFolder");
            foreach(XPathNavigator unitFolder in excludedUnitNodes)
            {
                string folderName = unitFolder.GetAttribute("name", string.Empty);

                if(string.IsNullOrEmpty(folderName))
                    WriteMessage(MessageLevel.Error, "Each excludedUnits/unitFolder node must have a 'name' attribute specifying the name of a folder name to exclude.");

                folderName = Environment.ExpandEnvironmentVariables(folderName);

                // add the folderName to the list of names to be excluded
                this.excludedUnits.Add(folderName.ToLowerInvariant(), null);
            }

            // Get the languages defined.
            XPathNodeIterator languageNodes = configuration.Select("languages/language");
            foreach(XPathNavigator languageNode in languageNodes)
            {
                // read the @languageId, @unit, and @extension attributes
                string languageId = languageNode.GetAttribute("languageId", string.Empty);
                if(string.IsNullOrEmpty(languageId))
                    WriteMessage(MessageLevel.Error, "Each language node must specify an @languageId attribute.");

                string unit = languageNode.GetAttribute("unit", string.Empty);

                // if both @languageId and @unit are specified, add this language to the language map
                if(!string.IsNullOrEmpty(unit))
                    languageMap.Add(unit.ToLowerInvariant(), languageId);

                // add languageId to the languageList for purpose of ordering snippets in the output
                if(!languageList.Contains(languageId))
                    languageList.Add(languageId.ToLowerInvariant());

                string extension = languageNode.GetAttribute("extension", string.Empty);
                if(!string.IsNullOrEmpty(extension))
                {
                    if(!extension.Contains("."))
                    {
                        extension = "." + extension;
                        WriteMessage(MessageLevel.Warn, "The @extension value must begin with a period. Adding a period to the extension value '{0}' of the {1} language.", extension, languageId);
                    }
                    else
                    {
                        int indexOfPeriod = extension.IndexOf('.');
                        if(indexOfPeriod != 0)
                        {
                            extension = extension.Substring(indexOfPeriod);
                            WriteMessage(MessageLevel.Warn, "The @extension value must begin with a period. Using the substring beginning with the first period of the specified extension value '{0}' of the {1} language.", extension, languageId);
                        }
                    }
                }

                // read the color nodes, if any, and add them to the list of colorization rules
                List<ColorizationRule> rules = new List<ColorizationRule>();

                XPathNodeIterator colorNodes = languageNode.Select("color");
                foreach(XPathNavigator colorNode in colorNodes)
                {
                    string pattern = colorNode.GetAttribute("pattern", String.Empty);
                    string region = colorNode.GetAttribute("region", String.Empty);
                    string name = colorNode.GetAttribute("class", String.Empty);
                    if(String.IsNullOrEmpty(region))
                    {
                        rules.Add(new ColorizationRule(pattern, name));
                    }
                    else
                    {
                        rules.Add(new ColorizationRule(pattern, region, name));
                    }
                }

                this.languages.Add(new Language(languageId, extension, rules));
                WriteMessage(MessageLevel.Info, "Loaded {0} colorization rules for the language '{1}', extension '{2}.", rules.Count, languageId, extension);
            }

            this.context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");
            this.selector = XPathExpression.Compile("//ddue:codeReference");
            this.selector.SetContext(this.context);

            // create the snippet cache
            snippetCache = new SnippetCache(100, approvedSnippetIndex, languageMap, languages, excludedUnits);
        }

        /// <summary>
        /// Apply method to perform the actual work of the component.
        /// </summary>
        /// <param name="document">document to be parsed for snippet references</param>
        /// <param name="key">Id of a topic</param>
        public override void Apply(XmlDocument document, string key)
        {
            // clear out the snippets dictionary of any snippets from the previous document
            snippets.Clear();

            foreach(XPathNavigator node in document.CreateNavigator().Select(this.selector).ToArray())
            {
                // get the snippet reference, which can contain one or more snippet ids
                string reference = node.Value;

                // check for validity of reference
                if(!validSnippetReference.IsMatch(reference))
                {
                    base.WriteMessage(key, MessageLevel.Warn, "Skipping invalid snippet reference: " + reference);
                    continue;
                }

                // get the identifiers from the codeReference
                var identifiers = SnippetIdentifier.ParseReference(reference);

                // load the language-specific snippets for each of the specified identifiers
                foreach(SnippetIdentifier identifier in identifiers)
                {
                    if(snippets.ContainsKey(identifier))
                        continue;

                    // look up the snippets example path
                    string examplePath = string.Empty;
                    if(!this.exampleIndex.TryGetValue(identifier.ExampleId, out examplePath))
                    {
                        base.WriteMessage(key, MessageLevel.Warn, "Snippet with identifier '{0}' was not found. " +
                            "The '{1}' example was not found in the examples directory.", identifier.ToString(),
                            identifier.ExampleId);
                        continue;
                    }

                    // get the snippet from the snippet cache 
                    List<Snippet> snippetList = snippetCache.GetContent(examplePath, identifier);
                    if(snippetList != null)
                    {
                        snippets.Add(identifier, snippetList);
                    }
                    else
                    {
                        // if no approval log was specified in the config, all snippets are treated as approved by default
                        // so show an warning message that the snippet was not found
                        if(approvedSnippetIndex.Count == 0)
                            base.WriteMessage(key, MessageLevel.Warn, "No Snippet with identifier '{0}' was found.",
                                identifier.ToString());
                        else
                        {
                            // show a warning message: either snippet not found, or snippet not approved.
                            bool isApproved = false;

                            foreach(string snippetIndex in this.approvedSnippetIndex.Keys)
                            {
                                string[] splitSnippet = snippetIndex.Split('\\');
                                if(splitSnippet[0] == identifier.ExampleId)
                                {
                                    isApproved = true;
                                    break;
                                }
                            }

                            // check whether snippets are present in parsnip approval logs and throw warnings accordingly.
                            if(!isApproved || !rejectedSnippetIndex.ContainsKey(identifier.ExampleId))
                                base.WriteMessage(key, MessageLevel.Warn, "The snippet with identifier '{0}' " +
                                    "was omitted because it is not present in parsnip approval logs.",
                                    identifier.ToString());
                            else
                                base.WriteMessage(key, MessageLevel.Warn, "No Snippet with identifier '{0}' " +
                                    "was found.", identifier.ToString());
                        }

                        continue;
                    }

                    // write warning messages for any rejected units for this example
                    List<string> rejectedUnits;
                    if(rejectedSnippetIndex.TryGetValue(identifier.ExampleId, out rejectedUnits))
                    {
                        foreach(string rejectedUnit in rejectedUnits)
                            base.WriteMessage(key, MessageLevel.Warn, "The '{0}' snippet with identifier " +
                                "'{1}' was omitted because the {2}\\{0} unit did not pass Parsnip testing.",
                                rejectedUnit, identifier.ToString(), identifier.ExampleId);
                    }
                }

                if(identifiers.Count() == 1)
                {
                    // one snippet referenced
                    SnippetIdentifier identifier = identifiers.First();

                    if(snippets.ContainsKey(identifier))
                        WriteSnippetContent(node, snippets[identifier]);
                }
                else
                {
                    // handle case where codeReference contains multiple identifiers
                    // Each language's set of snippets from multiple identifiers are displayed in a single block; 

                    // create dictionary that maps each language to its set of snippets
                    Dictionary<string, List<Snippet>> map = new Dictionary<string, List<Snippet>>();
                    foreach(SnippetIdentifier identifier in identifiers)
                    {
                        List<Snippet> values;
                        if(snippets.TryGetValue(identifier, out values))
                        {
                            foreach(Snippet value in values)
                            {
                                List<Snippet> pieces;
                                if(!map.TryGetValue(value.Language.LanguageId, out pieces))
                                {
                                    pieces = new List<Snippet>();
                                    map.Add(value.Language.LanguageId, pieces);
                                }
                                pieces.Add(value);
                            }
                        }
                    }

                    // now write the collection of snippet pieces to the document
                    XmlWriter writer = node.InsertAfter();
                    writer.WriteStartElement("snippets");
                    writer.WriteAttributeString("reference", reference);

                    // first write the snippets in the order their language shows up in the language map (if any)
                    foreach(string devlang in languageList)
                    {
                        foreach(KeyValuePair<string, List<Snippet>> entry in map)
                        {
                            if(!(devlang == entry.Key.ToLowerInvariant()))
                                continue;
                            writer.WriteStartElement("snippet");
                            writer.WriteAttributeString("language", entry.Key);
                            writer.WriteString("\n");

                            // write the set of snippets for this language
                            List<Snippet> values = entry.Value;
                            for(int i = 0; i < values.Count; i++)
                            {
                                if(i > 0)
                                    writer.WriteString("\n...\n\n\n");
                                // write the colorized or plaintext snippet text
                                WriteSnippetText(values[i], writer);
                            }

                            writer.WriteEndElement();
                        }
                    }

                    // now write any snippets whose language isn't in the language map
                    foreach(KeyValuePair<string, List<Snippet>> entry in map)
                    {
                        if(languageList.Contains(entry.Key.ToLowerInvariant()))
                            continue;
                        writer.WriteStartElement("snippet");
                        writer.WriteAttributeString("language", entry.Key);
                        writer.WriteString("\n");

                        // write the set of snippets for this language
                        List<Snippet> values = entry.Value;
                        for(int i = 0; i < values.Count; i++)
                        {
                            if(i > 0)
                                writer.WriteString("\n...\n\n\n");
                            // write the colorized or plaintext snippet text
                            WriteSnippetText(values[i], writer);
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.Close();
                }
                node.DeleteSelf();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Index the example names to paths.
        /// </summary>
        /// <param name="rootDirectory">root directory location of parsnip samples</param>
        private void LoadExamples(string rootDirectory)
        {
            try
            {
                DirectoryInfo root = new DirectoryInfo(rootDirectory);
                DirectoryInfo[] areaDirectories = root.GetDirectories();

                foreach(DirectoryInfo area in areaDirectories)
                {
                    DirectoryInfo[] exampleDirectories = area.GetDirectories();

                    foreach(DirectoryInfo example in exampleDirectories)
                    {
                        string path;
                        if(this.exampleIndex.TryGetValue(example.Name.ToLowerInvariant(), out path))
                            WriteMessage(MessageLevel.Warn, "The example '{0}' under folder '{1}' already exists under '{2}'", example.Name, example.FullName, path);

                        this.exampleIndex[example.Name.ToLowerInvariant()] = example.FullName;
                    }
                }
            }
            catch(Exception e)
            {
                WriteMessage(MessageLevel.Error, "The loading of examples failed:{0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Index the approved snippets.
        /// </summary>
        /// <param name="file">approved snippets log file</param>
        private void ParseApprovalLogFiles(string file)
        {
            string sampleName = string.Empty;
            string unitName = string.Empty;
            List<string> rejectedUnits = null;

            XmlReader reader = XmlReader.Create(file);
            try
            {
                while(reader.Read())
                {
                    if(reader.NodeType == XmlNodeType.Element)
                    {
                        if(reader.Name == "Sample")
                        {
                            sampleName = reader.GetAttribute("name").ToLowerInvariant();
                            //create a new rejectedUnits list for this sample
                            rejectedUnits = null;
                        }

                        if(reader.Name == "Unit")
                        {
                            unitName = reader.GetAttribute("name").ToLowerInvariant();

                            bool include = Convert.ToBoolean(reader.GetAttribute("include"),
                                CultureInfo.InvariantCulture);

                            if(include)
                            {
                                if(this.approvedSnippetIndex.ContainsKey(Path.Combine(sampleName, unitName)))
                                    WriteMessage(MessageLevel.Warn, "Sample '{0}' already exists in the approval log files.", sampleName);
                                this.approvedSnippetIndex[Path.Combine(sampleName, unitName)] = null;
                            }
                            else
                            {
                                if(rejectedUnits == null)
                                {
                                    rejectedUnits = new List<string>();
                                    rejectedSnippetIndex[sampleName] = rejectedUnits;
                                }
                                rejectedUnits.Add(unitName);
                            }
                        }
                    }
                }
            }
            catch(XmlException e)
            {
                WriteMessage(MessageLevel.Error, "The specified approval log file is not well-formed. The error message is: {0}", e.Message);
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Write the snippet content to output files.
        /// </summary>
        /// <param name="node">code reference node</param>
        /// <param name="snippetList">List of snippets</param>
        private void WriteSnippetContent(XPathNavigator node, List<Snippet> snippetList)
        {
            if(snippetList == null || snippetList.Count == 0)
            {
                WriteMessage(MessageLevel.Warn, "Empty snippet list past for node " + node.Name);
                return;
            }

            XmlWriter writer = node.InsertAfter();
            writer.WriteStartElement("snippets");
            writer.WriteAttributeString("reference", node.Value);

            // first write the snippets in the order their language shows up in the language map (if any)
            foreach(string devlang in languageList)
            {
                foreach(Snippet snippet in snippetList)
                {
                    if(!(devlang == snippet.Language.LanguageId.ToLowerInvariant()))
                        continue;
                    writer.WriteStartElement("snippet");
                    writer.WriteAttributeString("language", snippet.Language.LanguageId);
                    writer.WriteString("\n");
                    // write the colorized or plaintext snippet text
                    WriteSnippetText(snippet, writer);
                    writer.WriteEndElement();
                }
            }

            // now write any snippets whose language isn't in the language map
            foreach(Snippet snippet in snippetList)
            {
                if(languageList.Contains(snippet.Language.LanguageId.ToLowerInvariant()))
                    continue;
                writer.WriteStartElement("snippet");
                writer.WriteAttributeString("language", snippet.Language.LanguageId);
                writer.WriteString("\n");
                // write the colorized or plaintext snippet text
                WriteSnippetText(snippet, writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Close();
        }

        private static void WriteSnippetText(Snippet snippet, XmlWriter writer)
        {
            // if colorization rules are defined, then colorize the snippet.
            if(snippet.Language.ColorizationRules != null)
                WriteColorizedSnippet(ColorizeSnippet(snippet.Content, snippet.Language.ColorizationRules), writer);
            else
                writer.WriteString(snippet.Content);
        }

        private static ICollection<Region> ColorizeSnippet(string text, List<ColorizationRule> rules)
        {
            // create a linked list consiting entirely of one uncolored region
            LinkedList<Region> regions = new LinkedList<Region>();
            regions.AddFirst(new Region(text));

            // loop over colorization rules
            foreach(ColorizationRule rule in rules)
            {
                // loop over regions
                LinkedListNode<Region> node = regions.First;
                while(node != null)
                {
                    // only try to colorize uncolored regions
                    if(node.Value.ClassName != null)
                    {
                        node = node.Next;
                        continue;
                    }

                    // find matches in the region
                    string regionText = node.Value.Text;
                    var matches = rule.Apply(regionText);

                    // if no matches were found, continue to the next region
                    if(matches.Count() == 0)
                    {
                        node = node.Next;
                        continue;
                    }

                    // we found matches; break the region into colored and uncolered subregions
                    // index is where we are looking from; index-1 is the end of the last match
                    int index = 0;

                    LinkedListNode<Region> referenceNode = node;

                    foreach(Capture match in matches)
                    {
                        // create a leading uncolored region 
                        if(match.Index > index)
                        {
                            Region uncoloredRegion = new Region(regionText.Substring(index, match.Index - index));
                            referenceNode = regions.AddAfter(referenceNode, uncoloredRegion);
                        }

                        // create a colored region
                        Region coloredRegion = new Region(rule.ClassName, regionText.Substring(match.Index, match.Length));
                        referenceNode = regions.AddAfter(referenceNode, coloredRegion);

                        index = match.Index + match.Length;
                    }

                    // create a trailing uncolored region
                    if(index < regionText.Length)
                    {
                        Region uncoloredRegion = new Region(regionText.Substring(index));
                        referenceNode = regions.AddAfter(referenceNode, uncoloredRegion);
                    }

                    // remove the original node
                    regions.Remove(node);
                    node = referenceNode.Next;
                }
            }
            return (regions);
        }

        private static void WriteColorizedSnippet(ICollection<Region> regions, XmlWriter writer)
        {
            foreach(Region region in regions)
                if(region.ClassName == null)
                    writer.WriteString(region.Text);
                else
                {
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", region.ClassName);
                    writer.WriteString(region.Text);
                    writer.WriteEndElement();
                }
        }
        #endregion
    }

    /// <summary>
    /// Language class.
    /// </summary>
    internal class Language
    {
        #region Private members
        /// <summary>
        /// The id of the programming language.
        /// </summary>
        private string languageId;

        /// <summary>
        /// Language file extension.
        /// </summary>
        private string extension;

        /// <summary>
        /// List of colorization rules.
        /// </summary>
        private List<ColorizationRule> colorizationRules;
        #endregion

        #region Constructor
        /// <summary>
        /// Language Constructor
        /// </summary>
        /// <param name="languageId">language id</param>
        /// <param name="extension">language file extension</param>
        /// <param name="rules">colorization rules</param>
        public Language(string languageId, string extension, List<ColorizationRule> rules)
        {
            this.languageId = languageId;
            this.extension = extension;
            this.colorizationRules = rules;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the languageId.
        /// </summary>
        public string LanguageId
        {
            get
            {
                return this.languageId;
            }
        }

        /// <summary>
        /// Gets the file extension
        /// </summary>
        public string Extension
        {
            get
            {
                return this.extension;
            }
        }

        /// <summary>
        /// Gets the colorization rules
        /// </summary>
        public List<ColorizationRule> ColorizationRules
        {
            get
            {
                return this.colorizationRules;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Check if the language is defined.
        /// </summary>
        /// <param name="languageId">language id</param>
        /// <param name="extension">file extension</param>
        /// <returns>boolean indicating if a language is defined</returns>
        public bool IsMatch(string languageId, string extension)
        {
            if(this.languageId == languageId)
            {
                if(this.extension == extension)
                {
                    return true;
                }
                else if(this.extension == "*")
                {
                    return true;
                }
            }
            else if(this.languageId == "*")
            {
                if(this.extension == extension)
                {
                    return true;
                }

                if(this.extension == "*")
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Snippet class.
    /// </summary>
    internal class Snippet
    {
        #region Private Members
        /// <summary>
        /// snippet content.
        /// </summary>
        private string content;

        /// <summary>
        /// snippet language
        /// </summary>
        private Language language;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for Snippet class.
        /// </summary>
        /// <param name="content">snippet content</param>
        /// <param name="language">snippet language</param>
        public Snippet(string content, Language language)
        {
            this.content = content;
            this.language = language;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the snippet content.
        /// </summary>
        public string Content
        {
            get
            {
                return this.content;
            }
        }

        /// <summary>
        /// Gets the snippet language.
        /// </summary>
        public Language Language
        {
            get
            {
                return this.language;
            }
        }
        #endregion
    }

    internal class SnippetCache
    {
        private int _cacheSize = 100;

        private LinkedList<String> lruLinkedList;

        private Dictionary<string, IndexedExample> cache;

        private Dictionary<string, string> _approvalIndex;
        private Dictionary<string, string> _languageMap;
        private List<Language> _languages;
        private Dictionary<string, Object> _excludedUnits;

        public SnippetCache(int cacheSize, Dictionary<string, string> approvalIndex, Dictionary<string, string> languageMap, List<Language> languages, Dictionary<string, Object> excludedUnits)
        {
            _cacheSize = cacheSize;
            _approvalIndex = approvalIndex;
            _languageMap = languageMap;
            _languages = languages;
            _excludedUnits = excludedUnits;

            cache = new Dictionary<string, IndexedExample>(_cacheSize);

            lruLinkedList = new LinkedList<string>();
        }

        public List<Snippet> GetContent(string examplePath, SnippetIdentifier snippetId)
        {

            // get the example containing the identifier
            IndexedExample exampleIndex = GetCachedExample(examplePath);
            if(exampleIndex == null)
                return (null);

            // 
            return exampleIndex.GetContent(snippetId);
        }

        private IndexedExample GetCachedExample(string examplePath)
        {
            IndexedExample exampleIndex;
            if(cache.TryGetValue(examplePath, out exampleIndex))
            {
                // move the file from its current position to the head of the lru linked list
                lruLinkedList.Remove(exampleIndex.ListNode);
                lruLinkedList.AddFirst(exampleIndex.ListNode);
            }
            else
            {
                // not in the cache, so load and index a new example
                exampleIndex = new IndexedExample(examplePath, _approvalIndex, _languageMap, _languages, _excludedUnits);
                if(cache.Count >= _cacheSize)
                {
                    // the cache is full
                    // the last node in the linked list has the path of the next file to remove from the cache
                    if(lruLinkedList.Last != null)
                    {
                        cache.Remove(lruLinkedList.Last.Value);
                        lruLinkedList.RemoveLast();
                    }
                }
                // add the new file to the cache and to the head of the lru linked list
                cache.Add(examplePath, exampleIndex);
                exampleIndex.ListNode = lruLinkedList.AddFirst(examplePath);
            }
            return (exampleIndex);
        }


    }

    internal class IndexedExample
    {
        /// <summary>
        /// snippet store.
        /// </summary>
        private Dictionary<SnippetIdentifier, List<Snippet>> exampleSnippets = new Dictionary<SnippetIdentifier, List<Snippet>>();
        private Dictionary<string, string> _approvalIndex;
        private Dictionary<string, string> _languageMap;
        private List<Language> _languages;
        private Dictionary<string, Object> _excludedUnits;

        public IndexedExample(string examplePath, Dictionary<string, string> approvalIndex, Dictionary<string, string> languageMap, List<Language> languages, Dictionary<string, Object> excludedUnits)
        {
            _approvalIndex = approvalIndex;
            _languageMap = languageMap;
            _languages = languages;
            _excludedUnits = excludedUnits;

            // load all the snippets under the specified example path
            this.ParseExample(new DirectoryInfo(examplePath));
        }

        public List<Snippet> GetContent(SnippetIdentifier identifier)
        {
            if(exampleSnippets.ContainsKey(identifier))
                return exampleSnippets[identifier];
            else
                return null;
        }

        private LinkedListNode<string> listNode;
        public LinkedListNode<string> ListNode
        {
            get
            {
                return (listNode);
            }
            set
            {
                listNode = value;
            }
        }

        /// <summary>
        /// Check whether the snippet unit is approved
        /// </summary>
        /// <param name="unit">unit directory</param>
        /// <returns>boolean indicating whether the snippet unit is approved</returns>
        private bool IsApprovedUnit(DirectoryInfo unit)
        {
            string sampleName = unit.Parent.Name.ToLowerInvariant();
            string unitName = unit.Name.ToLowerInvariant();

            // return false if the unit name is in the list of names to exclude
            if(_excludedUnits.ContainsKey(unitName))
                return false;

            // if no approval log is specified, all snippets are approved by default
            if(_approvalIndex.Count == 0)
                return true;

            if(_approvalIndex.ContainsKey(Path.Combine(sampleName, unitName)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse the example directory.
        /// </summary>
        /// <param name="exampleDirectory">The example directory</param>
        private void ParseExample(DirectoryInfo exampleDirectory)
        {
            // process the approved language-specific unit directories for this example
            DirectoryInfo[] unitDirectories = exampleDirectory.GetDirectories();

            foreach(DirectoryInfo unit in unitDirectories)
            {
                if(this.IsApprovedUnit(unit))
                    this.ParseUnit(unit);
            }
        }

        /// <summary>
        /// Parse the unit directory for language files.
        /// </summary>
        /// <param name="unit">unit directory containing a language-specific version of the example</param>
        private void ParseUnit(DirectoryInfo unit)
        {
            // the language is the Unit Directory name, or the language id mapped to that name
            string language = unit.Name;
            if(_languageMap.ContainsKey(language.ToLowerInvariant()))
                language = _languageMap[language.ToLowerInvariant()];

            ParseDirectory(unit, language, unit.Parent.Name);
        }

        /// <summary>
        /// Parse an example subdir looking for source files containing snipppets.
        /// </summary>
        /// <param name="directory">The directory to parse</param>
        /// <param name="language">the id of a programming language</param>
        /// <param name="exampleName">the name of the example</param>
        private void ParseDirectory(DirectoryInfo directory, string language, string exampleName)
        {
            // parse the files in this directory
            FileInfo[] files = directory.GetFiles();
            foreach(FileInfo file in files)
                ParseFile(file, language, exampleName);

            // recurse to get files in any subdirectories
            DirectoryInfo[] subdirectories = directory.GetDirectories();
            foreach(DirectoryInfo subdirectory in subdirectories)
                ParseDirectory(subdirectory, language, exampleName);
        }

        /// <summary>
        /// Parse the language files to retrieve the snippet content.
        /// </summary>
        /// <param name="file">The snippet file</param>
        /// <param name="language">The snippet language</param>
        /// <param name="exampleName">The name of the example that contains this file</param>
        private void ParseFile(FileInfo file, string language, string exampleName)
        {
            string snippetLanguage = string.Empty;

            // The snippet language is the name (or id mapping) of the Unit folder
            // unless the file extension is .xaml
            // NOTE: this is just preserving the way ExampleBuilder handled it (which we can change when we're confident there are no unwanted side-effects)
            if(file.Extension.ToLowerInvariant() == ".xaml")
                snippetLanguage = "XAML";
            else
                snippetLanguage = language;

            // get the text in the file
            StreamReader reader = file.OpenText();
            string text = reader.ReadToEnd();
            reader.Close();

            this.ParseSnippetContent(text, snippetLanguage, file.Extension, exampleName);
        }

        /// <summary>
        /// Parse the snippet content.
        /// </summary>
        /// <param name="text">content to be parsed</param>
        /// <param name="language">snippet language</param>
        /// <param name="extension">file extension</param>
        /// <param name="example">snippet example</param>
        private void ParseSnippetContent(string text, string language, string extension, string example)
        {
            // parse the text for snippets
            for(Match match = find.Match(text); match.Success; match = find.Match(text, match.Index + 10))
            {
                string snippetIdentifier = match.Groups["id"].Value;
                string snippetContent = match.Groups["tx"].Value;
                snippetContent = clean.Replace(snippetContent, "\n");

                //if necessary, clean one more time to catch snippet comments on consecutive lines
                if(clean.Match(snippetContent).Success)
                {
                    snippetContent = clean.Replace(snippetContent, "\n");
                }

                snippetContent = cleanAtStart.Replace(snippetContent, "");
                snippetContent = cleanAtEnd.Replace(snippetContent, "");

                // get the language/extension from our languages List, which may contain colorization rules for the language
                Language snippetLanguage = new Language(language, extension, null);
                foreach(Language lang in _languages)
                {
                    if(!lang.IsMatch(language, extension))
                        continue;
                    snippetLanguage = lang;
                    break;
                }

                SnippetIdentifier identifier = new SnippetIdentifier(example, snippetIdentifier);

                // BUGBUG: i don't think this ever happens, but if it did we should write an error
                if(!snippetContent.IsLegalXmlText())
                {
                    // WriteMessage(MessageLevel.Warn, "Snippet '{0}' language '{1}' contains illegal characters.", identifier.ToString(), snippetLanguage.LanguageId);
                    continue;
                }

                snippetContent = StripLeadingSpaces(snippetContent);

                // Add the snippet information to dictionary
                Snippet snippet = new Snippet(snippetContent, snippetLanguage);
                List<Snippet> values;

                if(!this.exampleSnippets.TryGetValue(identifier, out values))
                {
                    values = new List<Snippet>();
                    this.exampleSnippets.Add(identifier, values);
                }
                values.Add(snippet);
            }
        }

        private static string StripLeadingSpaces(string text)
        {

            if(text == null)
                throw new ArgumentNullException("text");

            // split the text into lines
            string[] stringSeparators = new string[] { "\r\n" };
            string[] lines = text.Split(stringSeparators, StringSplitOptions.None);

            // figure out how many leading spaces to delete
            int spaces = Int32.MaxValue;
            for(int i = 0; i < lines.Length; i++)
            {

                string line = lines[i];

                // skip empty lines
                if(line.Length == 0)
                    continue;

                // determine the number of leading spaces
                int index = 0;
                while(index < line.Length)
                {
                    if(line[index] != ' ')
                        break;
                    index++;
                }

                if(index == line.Length)
                {
                    // lines that are all spaces should just be treated as empty
                    lines[i] = String.Empty;
                }
                else
                {
                    // otherwise, keep track of the minimum number of leading spaces				
                    if(index < spaces)
                        spaces = index;
                }

            }

            // re-form the string with leading spaces deleted
            StringBuilder result = new StringBuilder();

            foreach(string line in lines)
            {
                if(line.Length == 0)
                {
                    result.AppendLine();
                }
                else
                {
                    result.AppendLine(line.Substring(spaces));
                }
            }

            return (result.ToString());
        }

        /// <summary>
        /// Regex to find the snippet content.
        /// </summary>
        private static Regex find = new Regex(@"<snippet(?<id>\w+)>.*\n(?<tx>(.|\n)*?)\n.*</snippet(\k<id>)>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Regex to clean the snippet content.
        /// </summary>
        private static Regex clean = new Regex(@"\n[^\n]*?<(/?)snippet(\w+)>[^\n]*?\n",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Regex to clean the start of the snippet.
        /// </summary>
        private static Regex cleanAtStart = new Regex(@"^[^\n]*?<(/?)snippet(\w+)>[^\n]*?\n",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Regex to clean the end of the snippet.
        /// </summary>
        private static Regex cleanAtEnd = new Regex(@"\n[^\n]*?<(/?)snippet(\w+)>[^\n]*?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}

