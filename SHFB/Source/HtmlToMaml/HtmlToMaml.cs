//===============================================================================================================
// System  : HTML to MAML Converter
// File    : HtmlToMaml.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to convert HTML files to MAML.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/12/2008  EFW  Created the code
// 07/23/2010  EFW  Added code to handle invalid HREF paths
// 08/07/2012  EFW  Incorporated various changes from Dany R
//===============================================================================================================

// Ignore Spelling: Dany cref img href src utf xml xmlns ddue xlink apos lt nbsp mailto html

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This class is used to convert a set of HTML file to their close equivalent as conceptual MAML topic files
    /// </summary>
    public class HtmlToMaml : IBasePathProvider
    {
        #region Private data members
        //=====================================================================

        private readonly TopicCollection topics;
        private readonly ImageReferenceCollection images;
        private Topic currentTopic;
        private readonly string sourcePath, destPath;

        private readonly Dictionary<FilePath, Topic> topicDictionary;
        private readonly Dictionary<FilePath, ImageReference> imageDictionary;

        private static HtmlToMaml pathProvider;
        private readonly Dictionary<string, string> entities, tokens;
        private readonly Dictionary<string, TagOptions> conversionRules;
        private readonly List<string> markupSections;
        private int sectionCount;
        private bool isFirstHeading;
        private readonly bool createCompanionFile, replaceIntro;

        private readonly Regex reMarkupWrapper, reRemoveTag, reReplaceTag, reReplaceMarker;
        private readonly MatchEvaluator matchMarkupWrapper, matchReplace, matchMarker,
            matchCode, matchSee, matchAnchor, matchImage, matchHeading,
            matchEntity, matchToken, matchIntroduction;

        private static readonly Regex reAllTags = new Regex(
            @"<\s*(?<Closing>/?)\s*(?<Tag>[a-z0-9]*?)\s*?(?<Attributes>(\s|/)[^>]*?)?>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reReplaceTokens = new Regex(
            @"<@\s*(?<Value>(?<Name>\w+)[^>/]*?)(/)?\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reReplaceEntity = new Regex(@"&(\w+);");

        private static readonly Regex reReplaceCode = new Regex(
            @"<\s*(code)\s*?(?<Attributes>\s[^>]*?)?(/>|>(?<Code>.*?)<\s*/\s*(\1)[^>]?>)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reReplaceSee = new Regex(
            "(<\\s*(?<Tag>see)(?<PreAttrs>\\s+[^>]*)cref\\s*=\\s*\"(?<Link>.+?)\"(?<PostAttrs>.*?))(/>|(>(?<Content>.*?)" +
            "<\\s*/see\\s*>))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reReplaceAnchor = new Regex(
            "<\\s*a\\s+[^>]*?>(?<LinkText>.*?)<\\s*/\\s*a[^>]?>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reReplaceImage = new Regex("<\\s*img\\s+[^>]*?>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reLinkOpts = new Regex(
            "(href\\s*=\\s*(\"|')(?<HRef>.*?)(\"|'))|(name\\s*=\\s*(\"|')" +
            "(?<Name>.*?)(\"|'))|(target\\s*=\\s*(\"|')(?<Target>.*?)" +
            "(\"|'))|(title\\s*=\\s*(\"|')(?<Title>.*?)(\"|'))|" +
            "(alt\\s*=\\s*(\"|')(?<AltText>.*?)(\"|'))|(src\\s*=\\s*(\"|')" +
            "(?<Src>.*?)(\"|')|(align\\s*=\\s*(\"|')(?<Align>.*?)(\"|')))", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static readonly Regex reReplaceHeading = new Regex(
            "<\\s*(?<Level>h[1-6])\\s*>(<!-- TODO: Add named anchor: " +
            "(?<AnchorName>[^ ]+) -->)?(?<Title>.*?)<\\s*/\\s*\\k<Level>\\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reReplaceIntroduction = new Regex(
            @"^\s*(?<Introduction>.*?)\s*(?(<\s*section[^>]*?>)(?=<\s*section[^>]*?>)|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        #endregion

        #region IBasePathProvider implementation
        //=====================================================================

        /// <summary>
        /// This returns the source path for use as the base path
        /// </summary>
        public string BasePath => sourcePath;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the base path provider for the other classes involved
        /// in the conversion that need relative paths.
        /// </summary>
        /// <returns>The <see cref="HtmlToMaml"/> object performing the
        /// conversion.</returns>
        public static IBasePathProvider PathProvider => pathProvider;

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised to report progress information throughout
        /// the conversion process.
        /// </summary>
        public event EventHandler<ConversionProgressEventArgs> ConversionProgress;

        /// <summary>
        /// This raises the <see cref="ConversionProgress"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnConversionProgress(ConversionProgressEventArgs e)
        {
            ConversionProgress?.Invoke(this, e);
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source path containing the HTML files</param>
        /// <param name="dest">The destination path for the MAML topics and
        /// supporting files</param>
        /// <param name="createCompanion">True to create companion files for
        /// all topic files or false to not create them.</param>
        /// <param name="moveIntroText">If true, move text before the first section into an introduction element.
        /// If false, insert a place holder introduction element.</param>
        public HtmlToMaml(string source, string dest, bool createCompanion, bool moveIntroText)
        {
            XPathDocument rulesFile;
            XPathNavigator navRules;
            StringBuilder sb;
            string name;

            sourcePath = source;
            destPath = dest;
            createCompanionFile = createCompanion;
            replaceIntro = moveIntroText;
            pathProvider = this;

            if(sourcePath.EndsWith("\\", StringComparison.Ordinal))
                sourcePath = sourcePath.Substring(0, sourcePath.Length - 1);

            if(destPath.EndsWith("\\", StringComparison.Ordinal))
                destPath = destPath.Substring(0, destPath.Length - 1);

            sourcePath = Path.GetFullPath(sourcePath);
            destPath = Path.GetFullPath(destPath);

            topics = new TopicCollection();
            images = new ImageReferenceCollection();

            topicDictionary = new Dictionary<FilePath, Topic>();
            imageDictionary = new Dictionary<FilePath, ImageReference>();

            conversionRules = new Dictionary<string, TagOptions>();
            markupSections = new List<string>();
            entities = new Dictionary<string, string>();
            tokens = new Dictionary<string, string>();

            // Load the conversion rules
            rulesFile = new XPathDocument(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "ConversionRules.xml"));
            navRules = rulesFile.CreateNavigator();

            XPathNavigator bodyExpr = navRules.SelectSingleNode("//BodyExtract");

            if(bodyExpr != null)
                FileParser.BodyExtractExpression = bodyExpr.GetAttribute("expression", String.Empty);

            // Add the tags we will handle internally
            conversionRules.Add("a", null);
            conversionRules.Add("code", null);
            conversionRules.Add("h1", null);
            conversionRules.Add("h2", null);
            conversionRules.Add("h3", null);
            conversionRules.Add("h4", null);
            conversionRules.Add("h5", null);
            conversionRules.Add("h6", null);
            conversionRules.Add("img", null);
            conversionRules.Add("see", null);

            // Get the rules to process
            sb = new StringBuilder();

            foreach(XPathNavigator nav in navRules.Select("//Entities/Entity"))
                entities.Add(nav.GetAttribute("name", String.Empty), nav.GetAttribute("value", String.Empty));

            foreach(XPathNavigator nav in navRules.Select("//MarkupWrapper/Tag"))
            {
                if(sb.Length != 0)
                    sb.Append('|');

                name = nav.GetAttribute("name", String.Empty).ToLowerInvariant();
                conversionRules.Add(name, null);
                sb.Append(name);
            }

            name = sb.ToString();
            sb.Insert(0, @"<\s*(");
            sb.Append(@")[^>]*?>.*?<\s*/\s*(\1)[^>]*?>|<\s*(");
            sb.Append(name);
            sb.Append(@")\s*?((\s|/)[^>]*?)?>");
            reMarkupWrapper = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline);
            reReplaceMarker= new Regex("\xFF");
            matchMarkupWrapper = new MatchEvaluator(OnMatchMarkupWrapper);
            matchMarker = new MatchEvaluator(OnMatchMarker);
            sb.Remove(0, sb.Length);

            foreach(XPathNavigator nav in navRules.Select("//Remove/Tag"))
            {
                if(sb.Length != 0)
                    sb.Append('|');

                name = nav.GetAttribute("name", String.Empty).ToLowerInvariant();
                conversionRules.Add(name, null);
                sb.Append(name);
            }

            sb.Insert(0, @"<\s*/?\s*(");
            sb.Append(@")\s*?((\s|/)[^>]*?)?>");
            reRemoveTag = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sb.Remove(0, sb.Length);

            foreach(XPathNavigator nav in navRules.Select("//Replace/Tag"))
            {
                if(sb.Length != 0)
                    sb.Append('|');

                name = nav.GetAttribute("name", String.Empty).ToLowerInvariant();
                conversionRules.Add(name, new TagOptions(nav));
                sb.Append(name);
            }

            sb.Insert(0, @"<\s*(?<Closing>/?)\s*(?<Tag>");
            sb.Append(@")\s*(?<Attributes>(\s|/)[^>]*?)?>");
            reReplaceTag = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline);
            matchReplace = new MatchEvaluator(OnMatchReplace);

            matchCode = new MatchEvaluator(OnMatchCode);
            matchSee = new MatchEvaluator(OnMatchSee);
            matchAnchor = new MatchEvaluator(OnMatchAnchor);
            matchImage = new MatchEvaluator(OnMatchImage);
            matchHeading = new MatchEvaluator(OnMatchHeading);
            matchIntroduction = new MatchEvaluator(OnMatchIntroduction);
            matchEntity = new MatchEvaluator(OnMatchEntity);
            matchToken = new MatchEvaluator(OnMatchToken);
        }
        #endregion

        #region Public methods
        //=====================================================================

        /// <summary>
        /// This is called to perform the actual conversion
        /// </summary>
        public void ConvertTopics()
        {
            FileParser fileParser = new FileParser();
            string filename;

            this.ReportProgress("Conversion started at {0:MM/dd/yyyy hh:mm tt}\r\n", DateTime.Now);

            topics.AddTopicsFromFolder(sourcePath, topicDictionary);
            topics.ParseFiles(fileParser, imageDictionary);

            foreach(Topic t in topics)
                this.ConvertTopic(t);

            // Save the content layout and media content files
            filename = Path.Combine(destPath, "_ContentLayout.content");
            this.ReportProgress("Saving content layout file: {0}", filename);
            topics.Save(filename);

            if(images.Count != 0)
            {
                filename = Path.Combine(destPath, @"Media\_MediaFiles.xml");

                if(!Directory.Exists(Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                this.ReportProgress("Saving media content file: {0}", filename);
                images.Save(filename);
            }

            if(tokens.Count != 0)
            {
                filename = Path.Combine(destPath, "_ContentTokens.tokens");
                this.ReportProgress("Saving tokens file: {0}", filename);
                
                using(StreamWriter sw = new StreamWriter(filename, false,
                  Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine("<content xml:space=\"preserve\" " +
                        "xmlns:ddue=\"http://ddue.schemas.microsoft.com/" +
                        "authoring/2003/5\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">");

                    foreach(string key in tokens.Keys)
                        sw.WriteLine("  <item id=\"{0}\">{{@{1}}}</item>", key, WebUtility.HtmlEncode(tokens[key]));

                    sw.WriteLine("</content>");
                }
            }

            this.ReportProgress("\r\nConversion finished successfully at {0:MM/dd/yyyy hh:mm tt}", DateTime.Now);
        }

        /// <summary>
        /// This is called to convert a single topic and its children
        /// </summary>
        /// <param name="topic">The topic to convert</param>
        public void ConvertTopic(Topic topic)
        {
            StringBuilder sb = new StringBuilder();
            List<string> extras = new List<string>();
            string destFile, tagName, body = topic.Body;
            Regex reRemoveExtra;

            if(topic.SourceFile != null)
            {
                // Save the topic to the destination folder
                destFile = Path.Combine(destPath, topic.SourceFile.Path.Substring(sourcePath.Length + 1));
                destFile = Path.ChangeExtension(destFile, ".aml");

                if(!Directory.Exists(Path.GetDirectoryName(destFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                this.ReportProgress("{0} -> {1}", topic.SourceFile, destFile);
                currentTopic = topic;

                if(!String.IsNullOrEmpty(body))
                {
                    markupSections.Clear();
                    sectionCount = 0;

                    foreach(Match m in reAllTags.Matches(body))
                    {
                        tagName = m.Groups["Tag"].Value.ToLowerInvariant();

                        if(!conversionRules.ContainsKey(tagName) && extras.IndexOf(tagName) == -1)
                        {
                            if(sb.Length != 0)
                                sb.Append('|');

                            sb.Append(tagName);
                            extras.Add(tagName);

                            this.ReportProgress("    Warning: Found unknown tag '{0}' which will be removed", tagName);
                        }
                    }

                    // Replace unrecognized entities
                    body = reReplaceEntity.Replace(body, matchEntity);

                    // Replace markup wrappers with a placeholder
                    body = reMarkupWrapper.Replace(body, matchMarkupWrapper);

                    // Remove tags with no MAML equivalent
                    body = reRemoveTag.Replace(body, String.Empty);

                    if(sb.Length != 0)
                    {
                        sb.Insert(0, @"<\s*/?\s*(");
                        sb.Append(@")\s*?((\s|/)[^>]*?)?>");
                        reRemoveExtra = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        body = reRemoveExtra.Replace(body, String.Empty);
                    }

                    // Replace tokens
                    body = reReplaceTokens.Replace(body, matchToken);

                    // Replace tags with their MAML equivalent
                    body = reReplaceTag.Replace(body, matchReplace);

                    // Update <code> tags with the appropriate MAML tag
                    body = reReplaceCode.Replace(body, matchCode);

                    // Replace <see> tags with <codeEntityReference> tags
                    body = reReplaceSee.Replace(body, matchSee);

                    // Replace <a> tags with an appropriate MAML link
                    body = reReplaceAnchor.Replace(body, matchAnchor);

                    // Replace <img> tags with an appropriate MAML media link
                    body = reReplaceImage.Replace(body, matchImage);

                    // Wrap heading tags (h1-h6) in sections
                    isFirstHeading = true;
                    body = reReplaceHeading.Replace(body, matchHeading);

                    if(!isFirstHeading)
                        body += "\r\n  </content>\r\n</section>\r\n";

                    // If wanted, move the leading text before the first section tag into an introduction
                    // element.  This doesn't always produce good results across all topics depending on how
                    // they are formatted so it is optional.
                    if(replaceIntro)
                        body = reReplaceIntroduction.Replace(body, matchIntroduction);

                    // Put the markup wrappers back in the body
                    body = reReplaceMarker.Replace(body, matchMarker);

                    topic.Body = body;

                    // The converted HTML may not be valid XML so we'll write it
                    // out as text rather than using an XML document object.
                    using(StreamWriter sw = new StreamWriter(destFile, false,
                      Encoding.UTF8))
                    {
                        sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        sw.WriteLine("<topic id=\"{0}\" revisionNumber=\"{1}\">",
                            topic.Id, topic.RevisionNumber);  //DR[12-07-27]:

                        sw.WriteLine("  <developerConceptualDocument\r\n" +
                            "    xmlns=\"http://ddue.schemas.microsoft.com/authoring/2003/5\"\r\n" +
                            "    xmlns:xlink=\"http://www.w3.org/1999/xlink\">\r\n");

                        // If it had an abstract, write that out
                        if(!String.IsNullOrEmpty(topic.TopicAbstract))
                            sw.WriteLine("    <summary abstract=\"true\">\r\n" +
                                topic.TopicAbstract + "\r\n    </summary>");

                        // Insert a default introduction if leading text was not moved above
                        if(!replaceIntro)
                            sw.WriteLine("    <introduction>\r\n      <para>TODO: " +
                                "Move introduction text here</para>\r\n    " +
                                "</introduction>");

                        sw.WriteLine(topic.Body);
                        sw.WriteLine("    <relatedTopics>\r\n    </relatedTopics>\r\n");
                        sw.WriteLine("  </developerConceptualDocument>\r\n</topic>");
                    }

                    if(createCompanionFile)
                        CreateCompanionFile(Path.ChangeExtension(destFile, ".cmp"), topic);
                }
            }

            foreach(Topic t in topic.Subtopics)
                this.ConvertTopic(t);
        }

        /// <summary>
        /// Create a companion file for a topic
        /// </summary>
        /// <param name="filename">The companion filename.</param>
        /// <param name="topic">The topic</param>
        public static void CreateCompanionFile(string filename, Topic topic)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(filename, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("fileAssetGuid", topic.Id.ToString());
                writer.WriteAttributeString("assetTypeId", "CompanionFile");

                writer.WriteStartElement("topic");
                writer.WriteAttributeString("id", topic.Id.ToString());

                writer.WriteStartElement("title");
                if(!String.IsNullOrEmpty(topic.Title))
                    writer.WriteValue(topic.Title);
                else
                    writer.WriteValue(Path.GetFileNameWithoutExtension(filename));

                writer.WriteEndElement();   // </title>

                writer.WriteStartElement("tableOfContentsTitle");
                if(!String.IsNullOrEmpty(topic.Title))
                    writer.WriteValue(topic.Title);
                else
                    writer.WriteValue(Path.GetFileNameWithoutExtension(filename));

                writer.WriteEndElement();   // </tableOfContentsTitle>

                foreach(MSHelpAttr attr in topic.HelpAttributes)
                {
                    writer.WriteStartElement("attribute");
                    writer.WriteAttributeString("name", attr.AttributeName);
                    writer.WriteValue(attr.AttributeValue);
                    writer.WriteEndElement();
                }

                foreach(MSHelpKeyword kw in topic.HelpKeywords)
                {
                    writer.WriteStartElement("keyword");
                    writer.WriteAttributeString("index", kw.Index);
                    writer.WriteValue(kw.Term);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();   // </topic>

                writer.WriteEndElement();   // </metadata>
                writer.WriteEndDocument();
            }
            finally
            {
                if(writer != null)
                    writer.Close();
            }
        }
        #endregion

        #region Private conversion methods
        //=====================================================================

        /// <summary>
        /// This is used to report progress during the conversion
        /// </summary>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the
        /// message text</param>
        protected void ReportProgress(string message, params object[] args)
        {
            this.OnConversionProgress(new ConversionProgressEventArgs(
                String.Format(CultureInfo.CurrentCulture, message, args)));
        }

        /// <summary>
        /// This replaces named entities with their numeric equivalents
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks>The .NET XML parser only recognizes the common ASCII named
        /// entities.  As such, we need to replace all others with their
        /// numeric equivalents.</remarks>
        private string OnMatchEntity(Match match)
        {
            string entity = match.Groups[1].Value;

            if(!entities.TryGetValue(entity, out string value))
            {
                switch(entity)
                {
                    case "amp":     // These are okay
                    case "apos":
                    case "gt":
                    case "lt":
                    case "nbsp":
                    case "quot":
                        break;

                    default:
                        this.ReportProgress("    Warning: Unknown entity encountered: {0}", match.Value);
                        break;
                }

                return match.Value;
            }

            return String.Concat("&#", value, ";");
        }

        /// <summary>
        /// This replaces tokens with a MAML token element
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        private string OnMatchToken(Match match)
        {
            string name, value, newKey = String.Empty;
            int mod = 1;

            name = match.Groups["Name"].Value.Trim();
            value = match.Groups["Value"].Value.Trim();

            if(!tokens.ContainsKey(name))
                tokens.Add(name, value);
            else
                if(tokens[name] != value)
                {
                    // If we get a duplicate key name with differing values,
                    // try to find the value under another key name.
                    foreach(string key in tokens.Keys)
                        if(tokens[key] == value)
                        {
                            newKey = key;
                            break;
                        }

                    // If not found, add a new entry with a unique key
                    if(newKey.Length == 0)
                    {
                        do
                        {
                            newKey = name + "_" + mod.ToString(CultureInfo.InvariantCulture);
                            mod++;

                        } while(tokens.ContainsKey(newKey));

                        tokens.Add(newKey, value);
                    }

                    name = newKey;
                }

            return String.Concat("<token>", name, "</token>");
        }

        /// <summary>
        /// This replaces markup sections with a placeholder
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks>This is done so that the content of the markup is not
        /// affected by other changes made when processing the other
        /// rules.</remarks>
        private string OnMatchMarkupWrapper(Match match)
        {
            markupSections.Add(String.Concat("<!-- TODO: Review markup -->" +
                "\r\n<markup>\r\n", match.Value, "\r\n</markup>"));
            return "\xFF";
        }

        /// <summary>
        /// This replaces the markup placeholders with their markup
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        private string OnMatchMarker(Match match)
        {
            return markupSections[sectionCount++];
        }

        /// <summary>
        /// This replaces tags with their closes MAML equivalent
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The equivalent MAML tag</returns>
        private string OnMatchReplace(Match match)
        {
            TagOptions tagOpts;
            string tag;

            tag = match.Groups["Tag"].Value.ToLowerInvariant();
            tagOpts = conversionRules[tag];
            tagOpts.Evaluate(match);

            // Return the replacement
            if(String.IsNullOrEmpty(tagOpts.Tag) || tagOpts.Tag.IndexOfAny(new char[] { '<', '>' }) != -1)
                return tagOpts.Tag;

            return String.Format(CultureInfo.InvariantCulture, "<{0}{1}{2}>", tagOpts.Closing, tagOpts.Tag,
                tagOpts.Attributes);
        }

        /// <summary>
        /// This replaces <c>code</c> tags with their equivalent MAML tag
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        private string OnMatchCode(Match match)
        {
            string attrs, code, tag;

            attrs = match.Groups["Attributes"].Value.Trim();
            code = match.Groups["Code"].Value;

            if(!String.IsNullOrEmpty(attrs) || code.IndexOfAny(new[] { '\r', '\n' }) != -1)
            {
                tag = "code";

                if(!String.IsNullOrEmpty(attrs))
                    attrs = " " + attrs;
                else
                {
                    // Special case:  If there are no attributes and there is only one CR/LF pair, it's probably
                    // a codeInline tag that spans a line.
                    if(code.IndexOf('\r') == code.LastIndexOf('\r') && code.IndexOf('\n') == code.LastIndexOf('\n'))
                        tag = "codeInline";
                }
            }
            else
                tag = "codeInline";

            return String.Concat("<", tag, attrs, ">", code, "</", tag, ">");
        }

        /// <summary>
        /// This replaces <c>see</c> tags with a roughly equivalent <c>codeEntityReference</c> tag.
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks><c>see</c> tags in HTML may not be fully qualified so these may require review to fix up the
        /// references with fully qualified names.</remarks>
        private string OnMatchSee(Match match)
        {
            string link, autoUpgrade, notQualified = String.Empty;

            link = match.Groups["Link"].Value;

            if(link.Length > 2 && link[1] != ':')
            {
                notQualified = "<!-- TODO: Reference not fully qualified -->";
                this.ReportProgress("    Warning: Reference to code entity '{0}' is not fully qualified", link);
            }

            // If it's a member, add the autoUpgrade attribute so that overloads are preferred if found
            if(link.StartsWith("M:", StringComparison.OrdinalIgnoreCase))
                autoUpgrade = " autoUpgrade=\"true\"";
            else
                autoUpgrade = String.Empty;

            return String.Concat(notQualified, "<codeEntityReference qualifyHint=\"false\"",
                autoUpgrade, ">", link, "</codeEntityReference>");
        }

        /// <summary>
        /// This replaces <c>a</c> tags with their equivalent MAML link tag
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks>Named anchors are a bit of a problem since in MAML, they equate to an address attribute on
        /// an element so they are just marked for review.</remarks>
        private string OnMatchAnchor(Match match)
        {
            Topic topic;
            string href, name, target, linkText, title, inPageLink;
            int pos;

            href = name = target = title = inPageLink = String.Empty;

            foreach(Match opt in reLinkOpts.Matches(match.Value))
                if(opt.Groups["HRef"].Value.Length != 0)
                    href = opt.Groups["HRef"].Value.Trim();
                else
                    if(opt.Groups["Name"].Value.Length != 0)
                        name = opt.Groups["Name"].Value;
                    else
                        if(opt.Groups["Target"].Value.Length != 0)
                            target = opt.Groups["Target"].Value;
                        else
                            if(opt.Groups["Title"].Value.Length != 0)
                                title = opt.Groups["Title"].Value;

            linkText = match.Groups["LinkText"].Value;

            if(linkText.Length != 0)
                linkText = linkText.Replace("\r\n", " ");

            if(!String.IsNullOrEmpty(name))
            {
                this.ReportProgress("    Warning: Named anchor '{0}' " +
                    "needs review", name);
                return String.Concat("<!-- TODO: Add named anchor: ", name, " -->", linkText);
            }

            if(!String.IsNullOrEmpty(target))
                target = String.Concat("  <linkTarget>", target, "</linkTarget>\r\n");

            if(!String.IsNullOrEmpty(title))
                title = String.Concat("  <linkAlternateText>", title, "</linkAlternateText>\r\n");

            // If it contains "://" or starts with "mailto:", it's automatically an external link
            if(href.IndexOf("://", StringComparison.Ordinal) != -1 ||
              href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                href = WebUtility.HtmlEncode(href);
                return String.Concat("<externalLink>\r\n  <linkText>", linkText, "</linkText>\r\n", title,
                    "  <linkUri>", href, "</linkUri>\r\n", target, "</externalLink>");
            }

            // Split off any in-page link part
            pos = href.IndexOf('#');

            if(pos != -1)
            {
                // If it's just an in-page link, return that now
                if(pos == 0)
                    return String.Concat("<link xlink:href=\"", href, "\">", linkText, "</link>");

                inPageLink = href.Substring(pos);
                href = href.Substring(0, pos);
            }

            href = href.Replace('/', '\\');

            try
            {
                // Try to find it by the actual path first.  If not found, fully
                // qualify it based on the current topic's base path.
                if(!topicDictionary.TryGetValue(new FilePath(href, HtmlToMaml.PathProvider), out topic))
                    if(!topicDictionary.TryGetValue(new FilePath(Path.GetFullPath(Path.Combine(
                      Path.GetDirectoryName(currentTopic.SourceFile), href)), PathProvider), out topic))
                    {
                        this.ReportProgress("    Warning: Unable to resolve topic link to '{0}'", href);
                        return String.Concat("<!-- TODO: Unknown topic link: ", href, " -->", linkText);
                    }
            }
            catch(NotSupportedException )
            {
                // Invalid file path format
                this.ReportProgress("    Warning: Unable to resolve topic link to '{0}'", href);
                return String.Concat("<!-- TODO: Unknown topic link: ", href, " -->", linkText);
            }

            // If a target is specified, make it an external link
            if(!String.IsNullOrEmpty(target))
                return String.Concat("<externalLink>\r\n  <linkText>", linkText,
                    "</linkText>\r\n", title, "  <linkUri>html/", topic.Id,
                    inPageLink, ".htm</linkUri>\r\n", target, "</externalLink>");

            // Only use the link text if it doesn't match the topic title
            if(!String.IsNullOrEmpty(linkText) && String.Compare(WebUtility.HtmlDecode(linkText), topic.Title,
              StringComparison.OrdinalIgnoreCase) != 0)
                return String.Concat("<link xlink:href=\"", topic.Id, inPageLink, "\">", linkText, "</link>");

            return String.Concat("<link xlink:href=\"", topic.Id, inPageLink, "\" />");
        }

        /// <summary>
        /// This replaces <c>img</c> tags with their equivalent MAML media link tag
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks>Since we can't really tell whether to use <c>mediaLink</c> or <c>mediaLinkInline</c>, we'll
        /// always use <c>mediaLink</c>.  Inline links will need to be updated during the review process.  If the
        /// link is in the form of a URL, it will be converted to an <c>externalLink</c> element.</remarks>
        private string OnMatchImage(Match match)
        {
            string src, altText, mediaFolder, mediaFile, missing = String.Empty;
            string placement = String.Empty;

            src = altText = String.Empty;

            foreach(Match opt in reLinkOpts.Matches(match.Value))
                if(opt.Groups["Src"].Value.Length != 0)
                    src = opt.Groups["Src"].Value.Trim();
                else
                    if(opt.Groups["AltText"].Value.Length != 0)
                        altText = opt.Groups["AltText"].Value;
                    else
                        if(opt.Groups["Align"].Value.Length != 0)
                            placement = opt.Groups["Align"].Value;

            if(!String.IsNullOrEmpty(placement))
                if(placement.Equals("middle", StringComparison.OrdinalIgnoreCase))
                    placement = " placement=\"center\"";
                else
                    if(placement.Equals("right", StringComparison.OrdinalIgnoreCase))
                        placement = " placement=\"right\"";
                    else
                        placement = String.Empty;

            // If it contains "://", it's automatically an external link
            if(src.IndexOf("://", StringComparison.Ordinal) != -1)
            {
                src = WebUtility.HtmlEncode(src);

                if(String.IsNullOrEmpty(altText))
                    altText = src;

                return String.Concat("<externalLink>\r\n  <linkText>", altText, "</linkText>\r\n  <linkUri>",
                    src, "</linkUri>\r\n</externalLink>");
            }

            // Fully qualify the path as it is most likely relative to the
            // topic file.
            src = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentTopic.SourceFile),
                src.Replace('/', '\\')));

            // If not found, we'll assume it's a new image reference
            if(!imageDictionary.TryGetValue(new FilePath(src, PathProvider), out ImageReference image))
            {
                image = new ImageReference(src) { AlternateText = altText };
                images.Add(image);
                imageDictionary.Add(image.SourceFile, image);

                this.ReportProgress("    Info: Added new image reference: {0}", image.SourceFile.PersistablePath);

                // Copy it to the destination folder or add a note if missing
                if(File.Exists(image.SourceFile))
                {
                    mediaFolder = Path.Combine(destPath, "Media");
                    mediaFile = Path.Combine(mediaFolder, Path.GetFileName(image.SourceFile));

                    if(!Directory.Exists(mediaFolder))
                        Directory.CreateDirectory(mediaFolder);

                    File.Copy(image.SourceFile, mediaFile, true);
                    File.SetAttributes(mediaFile, FileAttributes.Normal);
                }
                else
                {
                    missing = "<!-- TODO: Missing source image: " + image.SourceFile.PersistablePath + " -->";
                    this.ReportProgress("    Warning: Image file '{0}' not found", image.SourceFile);
                }
            }

            return String.Concat(missing, "<mediaLink><image", placement, " xlink:href=\"", image.Id,
                "\" /></mediaLink>");
        }

        /// <summary>
        /// This replaces <c>h1</c>-<c>h6</c> tags with a MAML <c>section</c>
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks>If a "named anchor" comment is found, the named anchor is added as the section's
        /// <c>address</c> attribute.  Closing <c>section</c> tags are added on subsequent matches.  The final
        /// one is added after all of the matches are done.  The heading tag is noted in a comment to aid in
        /// deciding how or if section nesting is needed during the review.</remarks>
        private string OnMatchHeading(Match match)
        {
            string level, anchorName, title, closeSection;

            level = match.Groups["Level"].Value;
            anchorName = match.Groups["AnchorName"].Value;
            title = match.Groups["Title"].Value;

            if(isFirstHeading)
            {
                isFirstHeading = false;
                closeSection = String.Empty;
            }
            else
                closeSection = "  </content>\r\n</section>\r\n\r\n";

            if(!String.IsNullOrEmpty(anchorName))
                anchorName = " address=\"" + anchorName + "\"";

            // If not empty and not a non-breaking space, include the title element
            if(!String.IsNullOrWhiteSpace(title) && title.Trim() != "&#160;")
                return String.Concat(closeSection, "<section", anchorName, "><!--", level,
                    "-->\r\n  <title>", title, "</title>\r\n  <content>");

            // No title, so comment it out
            return String.Concat(closeSection, "<section", anchorName, "><!--", level,
                "-->\r\n  <!--<title>", title, "</title>-->\r\n  <content>");
        }

        /// <summary>
        /// This puts everything before the first <c>section</c> into an <c>introduction</c>
        /// </summary>
        /// <param name="match">The match to replace</param>
        /// <returns>The replacement text</returns>
        /// <remarks>If text is found before the first <c>section</c> tag or when none is available,
        /// it will be put between <c>introduction</c> tags.  If no text is found before the first
        /// <c>section</c> tag, a TODO <c>introduction</c> is returned.</remarks>
        private string OnMatchIntroduction(Match match)
        {
            string introduction;

            introduction = match.Groups["Introduction"].Value;

            if(!String.IsNullOrWhiteSpace(introduction))
                return String.Concat("    <introduction>\r\n      ", introduction,
                    "\r\n    </introduction>\r\n");

            return "    <introduction>\r\n      <para>TODO: Move introduction text here</para>\r\n" +
                "    </introduction>\r\n\r\n";
        }
        #endregion
    }
}
