// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {

	// a component to replace code references with snippets from a file

	public class ExampleComponent : BuildComponent {

		// instantiation logic

		public ExampleComponent(BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {

            XPathNodeIterator contentNodes = configuration.Select("examples");
			foreach (XPathNavigator contentNode in contentNodes) {
				string file = contentNode.GetAttribute("file", String.Empty);
                file = Environment.ExpandEnvironmentVariables(file);
                if (String.IsNullOrEmpty(file)) WriteMessage(MessageLevel.Error, String.Format("Each examples element must contain a file attribute."));
				LoadContent(file);
			}

			WriteMessage(MessageLevel.Info, String.Format("Loaded {0} code snippets", snippets.Count));

			XPathNodeIterator colorsNodes = configuration.Select("colors");
			foreach (XPathNavigator colorsNode in colorsNodes) {
				string language = colorsNode.GetAttribute("language", String.Empty);
				List<ColorizationRule> rules = new List<ColorizationRule>();

				XPathNodeIterator colorNodes = colorsNode.Select("color");
				foreach (XPathNavigator colorNode in colorNodes) {
					string pattern = colorNode.GetAttribute("pattern", String.Empty);
					string region = colorNode.GetAttribute("region", String.Empty);
					string name = colorNode.GetAttribute("class", String.Empty);
					if (String.IsNullOrEmpty(region)) {
						rules.Add( new ColorizationRule(pattern, name) );						
					} else {
						rules.Add( new ColorizationRule(pattern, region, name) );
					}
				}

				colorization[language] = rules;
				WriteMessage(MessageLevel.Info, String.Format("Loaded {0} colorization rules for the language '{1}'.", rules.Count, language));
			}

			context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");

			selector = XPathExpression.Compile("//ddue:codeReference");
			selector.SetContext(context);
		}

		// snippet loading logic

		private void LoadContent(string file) {

			SnippetIdentifier key = new SnippetIdentifier();
			string language;

			WriteMessage(MessageLevel.Info, String.Format("Loading code snippet file '{0}'.", file));
            try {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CheckCharacters = false;
                XmlReader reader = XmlReader.Create(file, settings);

                try {
                    reader.MoveToContent();
                    while (!reader.EOF) {
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "item")) {
                            key = new SnippetIdentifier(reader.GetAttribute("id"));
                            reader.Read();
                        } else if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "sampleCode")) {
                            language = reader.GetAttribute("language");

                            string content = reader.ReadString();

                            // If the element is empty, ReadString does not advance the reader, so we must do it manually
                            if (String.IsNullOrEmpty(content)) reader.Read();

                            if (!IsLegalXmlText(content))
                                throw new InvalidOperationException(String.Format("Snippet '{0}' language " +
                                    "'{1}' contains illegal characters.", key, language));

                            content = StripLeadingSpaces(content);

                            StoredSnippet snippet = new StoredSnippet(content, language);
                            List<StoredSnippet> values;
                            if (!snippets.TryGetValue(key, out values)) {
                                values = new List<StoredSnippet>();
                                snippets.Add(key, values);
                            }
                            values.Add(snippet);
                        } else {
                            reader.Read();
                        }
                    }
                } catch (XmlException e) {
                    WriteMessage(MessageLevel.Warn, String.Format("The contents of the snippet file '{0}' are not well-formed XML. The error message is: {1}. Some snippets may be lost.", file, e.Message));
                } finally {
			    	reader.Close();
			    }

            } catch (IOException e) {
                WriteMessage(MessageLevel.Error, String.Format("An access error occured while attempting to read the snippet file '{0}'. The error message is: {1}", file, e.Message));
            }

		}

        // logic for checking XML

        private bool IsLegalXmlCharacter (char c) {
            if (c < 0x20) {
                return ( (c == 0x09) || (c == 0x0A) || (c == 0x0D) );
            } else {
                if (c < 0xD800) {
                    return(true);
                } else {
                    return( (c >= 0xE000) && (c <= 0xFFFD) );
                }
            }
        }

        private bool IsLegalXmlText (string text) {
            foreach (char c in text) {
                if (!IsLegalXmlCharacter(c)) return (false);
            }
            return (true);
        }

		// the snippet store

		private Dictionary<SnippetIdentifier,List<StoredSnippet>> snippets = new Dictionary<SnippetIdentifier,List<StoredSnippet>>();

        // the actual work of the component

		public override void Apply(XmlDocument document, string key) {
            XPathNodeIterator nodesIterator = document.CreateNavigator().Select(selector);
            XPathNavigator[] nodes = BuildComponentUtilities.ConvertNodeIteratorToArray(nodesIterator);
            foreach (XPathNavigator node in nodes) {

				string reference = node.Value;

				// check for validity of reference
				if (validSnippetReference.IsMatch(reference)) {
					

				SnippetIdentifier[] identifiers = SnippetIdentifier.ParseReference(reference);

				if (identifiers.Length == 1) {
					// one snippet referenced

					SnippetIdentifier identifier = identifiers[0];
					List<StoredSnippet> values;
					if (snippets.TryGetValue(identifier, out values)) {

						XmlWriter writer = node.InsertAfter();
						writer.WriteStartElement("snippets");
						writer.WriteAttributeString("reference", reference);

						foreach (StoredSnippet value in values) {
							writer.WriteStartElement("snippet");
							writer.WriteAttributeString("language", value.Language);

							if (colorization.ContainsKey(value.Language)) {
								WriteColorizedSnippet(ColorizeSnippet(value.Text, colorization[value.Language]), writer);

							} else {
                                writer.WriteString(value.Text);
                                //writer.WriteString(System.Web.HttpUtility.HtmlDecode(value.Text));
							}
							writer.WriteEndElement();
						}

						writer.WriteEndElement();
						writer.Close();

					} else {
						base.WriteMessage(key, MessageLevel.Warn, "No snippet with identifier '{0}' was found.", identifier);
					}
				} else {
					// multiple snippets referenced

					// create structure that maps language -> snippets
					Dictionary<string,List<StoredSnippet>> map = new Dictionary<string,List<StoredSnippet>>();
					foreach (SnippetIdentifier identifier in identifiers) {
						List<StoredSnippet> values;
						if (snippets.TryGetValue(identifier, out values)) {
							foreach (StoredSnippet value in values) {
								List<StoredSnippet> pieces;
								if (!map.TryGetValue(value.Language, out pieces)) {
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
					
					foreach (KeyValuePair<string,List<StoredSnippet>> entry in map) {
						writer.WriteStartElement("snippet");
						writer.WriteAttributeString("language", entry.Key);

						List<StoredSnippet> values = entry.Value;
                        for (int i=0; i<values.Count; i++) {
							if (i>0) writer.WriteString("\n...\n\n\n");
                            writer.WriteString(values[i].Text);
                            // writer.WriteString(System.Web.HttpUtility.HtmlDecode(values[i].Text));
						}

						writer.WriteEndElement();
					}

					writer.WriteEndElement();
					writer.Close();
						
				}

				} else {
					base.WriteMessage(key, MessageLevel.Warn, "The code reference '{0}' is not well-formed", reference);
				}

				node.DeleteSelf();

			}
		}

		private XPathExpression selector;

		private XmlNamespaceManager context = new CustomContext();

		private static Regex validSnippetReference = new Regex(@"^[^#\a\b\f\n\r\t\v]+#(\w+,)*\w+$", RegexOptions.Compiled);

		// colorization logic

		private Dictionary<string,List<ColorizationRule>> colorization = new Dictionary<string,List<ColorizationRule>>();

		private static ICollection<Region> ColorizeSnippet (string text, List<ColorizationRule> rules) {

			// Console.WriteLine("colorizing: '{0}'", text);

			// create a linked list consiting entirely of one uncolored region
			LinkedList<Region> regions = new LinkedList<Region>();
			regions.AddFirst( new Region(text) );

			// loop over colorization rules
			foreach (ColorizationRule rule in rules) {

				// loop over regions

				LinkedListNode<Region> node = regions.First;				
				while(node != null) {

					// only try to colorize uncolored regions
					if (node.Value.ClassName != null) {
						node = node.Next;
						continue;
					}

					// find matches in the region
					string regionText = node.Value.Text;
					Capture[] matches = rule.Apply(regionText);

					// if no matches were found, continue to the next region
					if (matches.Length == 0) {
						node = node.Next;
						continue;						
					}

					// we found matches; break the region into colored and uncolered subregions

                    // index is where we are looking from; index-1 is the end of the last match
					int index = 0;

					LinkedListNode<Region> referenceNode = node;
					
					foreach (Capture match in matches) {

						// create a leading uncolored region 
						if (match.Index > index) {
                            //Console.WriteLine("uncolored: {0} '{1}' -> {2} '{3}'", index, regionText[index], match.Index - 1, regionText[match.Index - 1]); 
							Region uncoloredRegion = new Region(regionText.Substring(index, match.Index-index));
							referenceNode = regions.AddAfter(referenceNode, uncoloredRegion);
						}

						// create a colored region
						// Console.WriteLine("name = {0}", rule.ClassName);
                        //Console.WriteLine("colored: {0} '{1}' -> {2} '{3}'", match.Index, regionText[match.Index], match.Index + match.Length - 1, regionText[match.Index + match.Length - 1]);
                        Region coloredRegion = new Region(rule.ClassName, regionText.Substring(match.Index, match.Length));
						referenceNode = regions.AddAfter(referenceNode, coloredRegion);

						index = match.Index + match.Length;						

					}

					// create a trailing uncolored region
					if (index < regionText.Length) {
						Region uncoloredRegion = new Region(regionText.Substring(index));
						referenceNode = regions.AddAfter(referenceNode, uncoloredRegion);
					}

					// remove the original node
					regions.Remove(node);

					node = referenceNode.Next;
				}


			}
			return(regions);

		}

		private static void WriteColorizedSnippet (ICollection<Region> regions, XmlWriter writer) {
			foreach (Region region in regions) {
				// Console.WriteLine("writing {0}", region.ClassName);
				if (region.ClassName == null) {
					writer.WriteString(region.Text);
				} else {
					writer.WriteStartElement("span");
					writer.WriteAttributeString("class", region.ClassName);
					writer.WriteString(region.Text);
					writer.WriteEndElement();
				}
			}
		}

		private static string StripLeadingSpaces (string text) {

			if (text == null) throw new ArgumentNullException("text");

			// Console.WriteLine("BEFORE:");
			// Console.WriteLine(text);

			// split the text into lines
			string[] lines = text.Split('\n');

			// no need to do this if there is only one line
			if (lines.Length == 1) return(lines[0]);

			// figure out how many leading spaces to delete
			int spaces = Int32.MaxValue;
			for (int i=0; i<lines.Length; i++) {

				string line = lines[i];

				// skip empty lines
				if (line.Length == 0) continue;

				// determine the number of leading spaces
				int index = 0;
				while (index < line.Length) {
					if (line[index] != ' ') break;
					index++;
				}

				if (index == line.Length) {
					// lines that are all spaces should just be treated as empty
					lines[i] = String.Empty;
				} else {
					// otherwise, keep track of the minimum number of leading spaces				
					if (index < spaces) spaces = index;
				}

			}

			// Console.WriteLine("SPACES = {0}", spaces);

			// re-form the string with leading spaces deleted
			StringBuilder result = new StringBuilder();
			foreach (string line in lines) {
				if (line.Length == 0) {
					result.AppendLine();
				} else {
					result.AppendLine(line.Substring(spaces));
				}
			}
			// Console.WriteLine("AFTER:");
			// Console.WriteLine(result.ToString());
			return(result.ToString());			

		}		
			
	}

	internal struct SnippetIdentifier {

		public SnippetIdentifier (string exampleId, string snippetId) {
			this.exampleId = exampleId.ToLower();
			this.snippetId = snippetId.ToLower();
		}

		public SnippetIdentifier (string identifier) {
			int index = identifier.LastIndexOf('#');
			exampleId = identifier.Substring(0,index).ToLower();
			snippetId = identifier.Substring(index+1).ToLower();
		}

		private string exampleId;

		private string snippetId;

		public string Example {
			get {
				return(exampleId);
			}
		}

		public string Snippet {
			get {
				return(snippetId);
			}
		}

		public override string ToString() {
			return(String.Format("{0}#{1}", exampleId, snippetId));
		}

		public static SnippetIdentifier[] ParseReference (string reference) {

			int index = reference.IndexOf('#');
			if (index < 0) return(new SnippetIdentifier[0]);

			string example = reference.Substring(0,index);
			string[] snippets = reference.Substring(index+1).Split(',');

			SnippetIdentifier[] identifiers = new SnippetIdentifier[snippets.Length];
			for (int i=0; i<snippets.Length; i++) {
				identifiers[i] = new SnippetIdentifier(example, snippets[i]);
			}
			return(identifiers);

		}

	}

	internal class StoredSnippet {

		public StoredSnippet (string text, string language) {
			this.text = text;
			this.language = language;
		}

		private string text;

		private string language;

		public string Text {
			get {
				return(text);
			}
		}

		public string Language {
			get {
				return(language);
			}
		}
	}

	internal class ColorizationRule {

		public ColorizationRule (string pattern, string className) : this(pattern, null, className) {}

		public ColorizationRule (string pattern, string region, string className) {
			this.pattern = new Regex(pattern, RegexOptions.Compiled|RegexOptions.Multiline);
			this.region = region;
			this.className = className;
		}		

		private Regex pattern;

		private string region;

		private string className;

		public string ClassName {
			get {
				return(className);
			}
		}

		public Capture[] Apply (string text) {

			MatchCollection matches = pattern.Matches(text);
			Capture[] captures = new Capture[matches.Count];

			if (region == null) {
				matches.CopyTo(captures, 0);
				return(captures);
			} else {
				for (int i=0; i<captures.Length; i++) {
					captures[i] = matches[i].Groups[region];
				}
				return(captures);
			}

		}

	}

	internal struct Region {

		public Region (string text) : this(null, text) {}

		public Region (string className, string text) {
			this.className = className;
			this.text = text;
		}

		private string className;

		private string text;


		public string ClassName {
			get {
				return(className);
			}
		}

		public string Text {
			get {
				return(text);
			}
		}

	}

}
