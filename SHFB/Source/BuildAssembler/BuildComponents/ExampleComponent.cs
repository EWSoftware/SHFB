// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 02/14/2013 - EFW - Removed RegexOptions.Compiled from the Regex instances.  It was causing a significant delay
// and a huge memory usage increase that isn't justified based on the way the expressions are used here.
// 03/09/2013 - EFW - Moved the supporting classes to the Snippets namespace

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

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component is used to replace code references with snippets from a file
    /// </summary>
    public class ExampleComponent : BuildComponentCore
    {
        #region Private data members
        //=====================================================================

        // The snippet store
        private Dictionary<SnippetIdentifier, List<StoredSnippet>> snippets =
            new Dictionary<SnippetIdentifier, List<StoredSnippet>>();

        private XPathExpression selector;
        private XmlNamespaceManager context = new CustomContext();

        private static Regex validSnippetReference = new Regex(@"^[^#\a\b\f\n\r\t\v]+#(\w+,)*\w+$");

        private Dictionary<string, List<ColorizationRule>> colorization =
            new Dictionary<string, List<ColorizationRule>>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public ExampleComponent(BuildAssemblerCore assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            XPathNodeIterator contentNodes = configuration.Select("examples");

            foreach(XPathNavigator contentNode in contentNodes)
            {
                string file = contentNode.GetAttribute("file", String.Empty);
                file = Environment.ExpandEnvironmentVariables(file);

                if(String.IsNullOrEmpty(file))
                    WriteMessage(MessageLevel.Error, "Each examples element must contain a file attribute.");

                LoadContent(file);
            }

            WriteMessage(MessageLevel.Info, "Loaded {0} code snippets", snippets.Count);

            XPathNodeIterator colorsNodes = configuration.Select("colors");

            foreach(XPathNavigator colorsNode in colorsNodes)
            {
                string language = colorsNode.GetAttribute("language", String.Empty);
                List<ColorizationRule> rules = new List<ColorizationRule>();

                XPathNodeIterator colorNodes = colorsNode.Select("color");

                foreach(XPathNavigator colorNode in colorNodes)
                {
                    string pattern = colorNode.GetAttribute("pattern", String.Empty);
                    string region = colorNode.GetAttribute("region", String.Empty);
                    string name = colorNode.GetAttribute("class", String.Empty);

                    if(String.IsNullOrEmpty(region))
                        rules.Add(new ColorizationRule(pattern, name));
                    else
                        rules.Add(new ColorizationRule(pattern, region, name));
                }

                colorization[language] = rules;
                WriteMessage(MessageLevel.Info, "Loaded {0} colorization rules for the language '{1}'.", rules.Count, language);
            }

            context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");

            selector = XPathExpression.Compile("//ddue:codeReference");
            selector.SetContext(context);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Snippet loading logic
        /// </summary>
        /// <param name="file">The file from which to load the snippets</param>
        private void LoadContent(string file)
        {
            SnippetIdentifier key = new SnippetIdentifier();
            string language;

            WriteMessage(MessageLevel.Info, "Loading code snippet file '{0}'.", file);
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CheckCharacters = false;
                XmlReader reader = XmlReader.Create(file, settings);

                try
                {
                    reader.MoveToContent();
                    while(!reader.EOF)
                    {
                        if((reader.NodeType == XmlNodeType.Element) && (reader.Name == "item"))
                        {
                            key = new SnippetIdentifier(reader.GetAttribute("id"));
                            reader.Read();
                        }
                        else if((reader.NodeType == XmlNodeType.Element) && (reader.Name == "sampleCode"))
                        {
                            language = reader.GetAttribute("language");

                            string content = reader.ReadString();

                            // If the element is empty, ReadString does not advance the reader, so we must do it manually
                            if(String.IsNullOrEmpty(content))
                                reader.Read();

                            if(!content.IsLegalXmlText())
                                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                    "Snippet '{0}' language '{1}' contains illegal characters.", key, language));

                            content = StripLeadingSpaces(content);

                            StoredSnippet snippet = new StoredSnippet(content, language);
                            List<StoredSnippet> values;

                            if(!snippets.TryGetValue(key, out values))
                            {
                                values = new List<StoredSnippet>();
                                snippets.Add(key, values);
                            }

                            values.Add(snippet);
                        }
                        else
                            reader.Read();
                    }
                }
                catch(XmlException e)
                {
                    WriteMessage(MessageLevel.Warn, "The contents of the snippet file '{0}' are not well-formed XML. The error message is: {1}. Some snippets may be lost.", file, e.Message);
                }
                finally
                {
                    reader.Close();
                }

            }
            catch(IOException e)
            {
                WriteMessage(MessageLevel.Error, "An access error occured while attempting to read the snippet file '{0}'. The error message is: {1}", file, e.Message);
            }
        }

        /// <summary>
        /// Colorization logic
        /// </summary>
        /// <param name="text">The text to colorize</param>
        /// <param name="rules">The colorization rules</param>
        /// <returns>A collection of colorized code regions</returns>
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

                    // we found matches; break the region into colored and uncolored subregions

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

        /// <summary>
        /// Write the colorized code snippet to the output
        /// </summary>
        /// <param name="regions">A collection of colorized code regions</param>
        /// <param name="writer">The XML writer to which the colorized code is written</param>
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

        /// <summary>
        /// Strip a common amount of leading whitespace from each line of the given text block
        /// </summary>
        /// <param name="text">The text from which to strip leading whitespace</param>
        /// <returns>The text with the leading whitespace stripped from each line</returns>
        private static string StripLeadingSpaces(string text)
        {
            if(text == null)
                throw new ArgumentNullException("text");

            // split the text into lines
            string[] lines = text.Split('\n');

            // no need to do this if there is only one line
            if(lines.Length == 1)
                return lines[0];

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
                if(line.Length == 0)
                    result.AppendLine();
                else
                    result.AppendLine(line.Substring(spaces));

            return result.ToString();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            foreach(XPathNavigator node in document.CreateNavigator().Select(selector).ToArray())
            {
                string reference = node.Value;

                // check for validity of reference
                if(validSnippetReference.IsMatch(reference))
                {
                    var identifiers = SnippetIdentifier.ParseReference(reference);

                    if(identifiers.Count() == 1)
                    {
                        // one snippet referenced

                        SnippetIdentifier identifier = identifiers.First();
                        List<StoredSnippet> values;

                        if(snippets.TryGetValue(identifier, out values))
                        {

                            XmlWriter writer = node.InsertAfter();
                            writer.WriteStartElement("snippets");
                            writer.WriteAttributeString("reference", reference);

                            foreach(StoredSnippet value in values)
                            {
                                writer.WriteStartElement("snippet");
                                writer.WriteAttributeString("language", value.Language);

                                if(colorization.ContainsKey(value.Language))
                                    WriteColorizedSnippet(ColorizeSnippet(value.Text, colorization[value.Language]), writer);
                                else
                                    writer.WriteString(value.Text);

                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                            writer.Close();
                        }
                        else
                            base.WriteMessage(key, MessageLevel.Warn, "No snippet with identifier '{0}' was found.", identifier);
                    }
                    else
                    {
                        // multiple snippets referenced

                        // create structure that maps language -> snippets
                        Dictionary<string, List<StoredSnippet>> map = new Dictionary<string, List<StoredSnippet>>();

                        foreach(SnippetIdentifier identifier in identifiers)
                        {
                            List<StoredSnippet> values;

                            if(snippets.TryGetValue(identifier, out values))
                            {
                                foreach(StoredSnippet value in values)
                                {
                                    List<StoredSnippet> pieces;
                                    if(!map.TryGetValue(value.Language, out pieces))
                                    {
                                        pieces = new List<StoredSnippet>();
                                        map.Add(value.Language, pieces);
                                    }
                                    pieces.Add(value);
                                }
                            }
                        }

                        XmlWriter writer = node.InsertAfter();
                        writer.WriteStartElement("snippets");
                        writer.WriteAttributeString("reference", reference);

                        foreach(KeyValuePair<string, List<StoredSnippet>> entry in map)
                        {
                            writer.WriteStartElement("snippet");
                            writer.WriteAttributeString("language", entry.Key);

                            List<StoredSnippet> values = entry.Value;
                            for(int i = 0; i < values.Count; i++)
                            {
                                if(i > 0)
                                    writer.WriteString("\n...\n\n\n");

                                writer.WriteString(values[i].Text);
                            }

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                        writer.Close();
                    }
                }
                else
                    base.WriteMessage(key, MessageLevel.Warn, "The code reference '{0}' is not well-formed", reference);

                node.DeleteSelf();
            }
        }
        #endregion
    }
}
