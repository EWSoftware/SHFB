//===============================================================================================================
// System  : Code Colorizer Library
// File    : CodeColorizer.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 04/06/2021
//
// This is used to colorize blocks of code for output as HTML.  The original Code Project article by Jonathan
// can be found at: http://www.codeproject.com/Articles/3767/Multiple-Language-Syntax-Highlighting-Part-2-C-Con.
//
#region Modifications by Eric Woodruff
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006-01/2016:
//
// Overall:
//      Updated for use with .NET 4.0 and rewrote various parts to streamline the code and improve performance.
//      See source files for details.
//
//      Removed all dead code from the project and flattened the namespace hierarchy.
//
//      Reworked the colorizer to load the default configuration files on first use if not specified.  Also
//      switched to using the built in System.Configuration.DictionarySectionHandler for the config section.
//
//      Made various modifications to allow exceptions to propagate up to the caller.  Helps indicate when
//      you've hosed up the configuration.
//
//      Wrapped timer stuff in conditional blocks to remove it from release builds.
//
//      Gave the assembly a strong name.
//
// In this file:
//      Converted the public fields to properties.
//
//      Fixed bug in FindRule that caused it to return null if there was only one keyword set in a context.
//
//      Added missing support for the <code lang="xxx"> tag.  Also, "language" is an acceptable alternative for
//      the "lang" attribute in either the <code> or <pre> tag.
//
//      Added support for the tabSize attribute to convert tabs to X number of spaces to ensure code layout
//      consistency.
//
//      Added code to strip any common leading whitespace from all lines.
//
//      Added support for line numbering and folding of #region and #if/#else/#endif statements using numberLines
//      and outlining attributes.
//
//      Added support for a title attribute.
//
//      Added support for a keepSeeTags attribute that preserves <see> tags within the code so that they can be
//      processed later by a documentation tool such as the Sandcastle Help File Builder.
//
//      Added a property to return a dictionary that maps the language IDs to the friendly names.  Also added
//      support for default titles based on the language.
//
//      Added support for mapping variations on language IDs to actual entries in the syntax file to make it more
//      flexible in finding rule sets.
//
//      Incorporated changes from Joey Bradshaw to support turning off the word boundaries on language keywords.
//
//      Added the ability to disable all options except leading whitespace normalization.
//
//      Made various updates to make the code colorizer thread safe.
//
//  Changes to highlight.css/.xml/.xsl:
//      Reworked the keyword lists to share common keywords amongst similar languages.
//
//      Added support for C#, VB.NET, and J#.  Also added "name" attribute to each "language" element to specify
//      a friendly name.
//
//      Added a <map> element to allow mapping of language name variations to existing entries in the syntax
//      file.
//
//      Removed the preprocessor keyword list and added a generic handler for preprocessor directives in each
//      language that needed it.
//
//      Added <include.file> handler for C and C++.
//
//      Added number handler to each languages and the XSL transformation.
//
//      Reworked the VB comment detection to prevent it double-spacing the comments and moved REM keyword into
//      it as well.
//
//      Modified the style names to be generic.
//
//      Incorporated changes from Joey Bradshaw to support colorization of PowerShell scripts.
//
//      Incorporated changes from Golo Roden to support colorization of Python code.
//
#endregion
//===============================================================================================================

// Ignore Spelling: Golo Roden uage collapsebox onclick lnborder keywordlist keywordlists regexp detectchar
// Ignore Spelling: linecontinue parsedcode copycode onmouseover onmouseout onkeypress

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

namespace ColorizerLibrary
{
    /// <summary>
    /// Code colorizer provides a flexible solution for colorizing code
    /// </summary>
    /// <remarks>Original Author: Jonathan de Halleux, dehalleux@pelikhan.com, 2003.
    /// <p/>Modified by Eric Woodruff (Eric@EWoodruff.us) 11/2006. The original Code Project article and code by
    /// Jonathan can be found at:
    /// <see href="http://www.codeproject.com/Articles/3767/Multiple-Language-Syntax-Highlighting-Part-2-C-Con"/>.</remarks>
    /// <threadsafety>The <see cref="ProcessAndHighlightText"/> and <see cref="ColorizePlainText"/> methods are
    /// thread safe.  All other public instance properties and methods are not guaranteed to be thread safe
    /// and are intended for setting the defaults prior to colorizing code.</threadsafety>
    public class CodeColorizer
    {
        #region Collapsible region tracking class
        //=====================================================================

        /// <summary>
        /// This is used to track a collapsible region within the code block
        /// </summary>
        private sealed class CollapsibleRegion
        {
            #region Properties
            //=================================================================

            /// <summary>
            /// The line number on which the region part occurs
            /// </summary>
            internal int LineNumber { get; set; }

            /// <summary>
            /// This returns the nesting level for the entry
            /// </summary>
            internal int NestingLevel { get; }

            /// <summary>
            /// The region type
            /// </summary>
            internal string RegionType { get; }

            /// <summary>
            /// The description for the collapsed text
            /// </summary>
            internal string Description { get; }

            /// <summary>
            /// This returns true if this entry represents the start of a
            /// region.
            /// </summary>
            internal bool IsStart { get; }

            #endregion

            #region Constructors
            //=================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="lineNumber">The line number</param>
            /// <param name="match">The match</param>
            /// <param name="nestingLevel">The nesting level</param>
            internal CollapsibleRegion(int lineNumber, Match match, int nestingLevel)
            {
                this.LineNumber = lineNumber;
                this.RegionType = match.Groups[1].Value.ToLowerInvariant();
                this.NestingLevel = nestingLevel;

                // Remove spaces from VB.NET style end statements
                if(this.RegionType.StartsWith("#end ", StringComparison.Ordinal))
                    this.RegionType = this.RegionType.Replace(" ", String.Empty);
                else
                {
                    // Remove name or condition
                    int idx = this.RegionType.IndexOf(' ');

                    if(idx != -1)
                        this.RegionType = this.RegionType.Substring(0, idx);
                }

                // Certain collapsed blocks have a description.  Also, certain blocks start or end on the
                // previous or next line.
                switch(this.RegionType)
                {
                    case "#region":
                        this.Description = WebUtility.HtmlEncode(match.Groups[2].Value.Trim());

                        if(this.Description.Length == 0)
                            this.Description = "...";

                        this.NestingLevel++;
                        this.IsStart = true;
                        break;

                    case "#if":
                        this.LineNumber++;
                        this.Description = "...";
                        this.NestingLevel++;
                        this.IsStart = true;
                        break;

                    case "#else":
                        this.LineNumber++;
                        this.Description = "...";
                        this.NestingLevel++;
                        this.IsStart = true;
                        break;

                    case "#endif":
                        this.LineNumber--;
                        this.NestingLevel--;
                        break;
                }
            }

            /// <summary>
            /// Constructor.  This version assumes it's an #endif
            /// </summary>
            /// <param name="lineNumber">The line number</param>
            /// <param name="nestingLevel">The nesting level</param>
            internal CollapsibleRegion(int lineNumber, int nestingLevel)
            {
                this.LineNumber = lineNumber - 1;
                this.RegionType = "#endif";
                this.NestingLevel = nestingLevel - 1;
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================

        // This regular expression is used to search for and colorize the code blocks
        private static readonly Regex reColorize = new Regex("<\\s*(pre|code)\\s+([^>]*?)(lang(?:uage)?\\s*=\\s*(\"|')?" +
            "([a-z0-9\\-+#]+)(\"|')?)([^>]*?)>((.|\\n)*?)<\\s*/\\s*(pre|code)\\s*>", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        // This is used to find option overrides
        private static readonly Regex reOptOverrides = new Regex("((numberLines\\s*=\\s*(\"|')?(true|false)(\"|')?))|" +
            "((outlining\\s*=\\s*(\"|')?(true|false)(\"|')?))|" +
            "((tabSize\\s*=\\s*(\"|')?([0-9]+)(\"|')?))|" +
            "((title\\s*=\\s*(\"([^\"]+)\"|\\'([^\\']+)\\')))|" +
            "((keepSeeTags\\s*=\\s*(\"|')?(true|false)(\"|')?))|" +
            "((disabled\\s*=\\s*(\"|')?(true|false)(\"|')?))", RegexOptions.IgnoreCase);

        // These are used to preserve <see> tags so that they aren't colorized
        private static readonly Regex reExtractSeeTags = new Regex("(<\\s*see(?<PreAttrs>\\s+[^>]*)[a-z]ref\\s*=" +
            "\\s*\"(?<Link>.+?)\"(?<PostAttrs>.*?))(/>|(>(?<Content>.*?)<\\s*/\\s*see\\s*>))",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // The marker is HTML encoded after colorization
        private static readonly Regex reReplaceMarker = new Regex("&#255;");

        // This is used to find the collapsible region boundaries
        private static readonly Regex reCollapseMarkers = new Regex(
            @"^\s*(#region\s(.*)|#if\s(.*)|#else|#end\s?if|#end\s?region)", RegexOptions.IgnoreCase);

        private static int uniqueRegionId;

        // Syntax description file, friendly name dictionary, and alternate IDs dictionary
        private readonly XmlDocument languageSyntax;
        private XmlNode languages, keywordLists;
        private readonly Dictionary<string, string> friendlyNames, alternateIds;

        // Code style sheet
        private readonly XslCompiledTransform languageStyle;

        // Delegate for Regex.Replace
        private readonly MatchEvaluator replaceByCodeDelegate;

        private int defaultTabSize;    // Tab size override

        // Regular expressions cache
        private readonly RegexDictionary rxDic;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to specify the output format used by the <see cref="LanguageStyleFileName"/>
        /// </summary>
        /// <remarks>This is used to determine the supported options when colorizing plain text with the
        /// <see cref="ColorizePlainText"/> method.</remarks>
        public OutputFormat OutputFormat { get; set; }

        /// <summary>
        /// The syntax file name
        /// </summary>
        public string LanguageSyntaxFileName { get; }

        /// <summary>
        /// The style file name
        /// </summary>
        public string LanguageStyleFileName { get; }

        /// <summary>
        /// This is used to set or get whether or not lines will be numbered in code blocks without a
        /// <b>numberLines</b> attribute.
        /// </summary>
        /// <value>The default is false</value>
        public bool NumberLines { get; set; }

        /// <summary>
        /// This is used to set or get whether or not outlining of #region and #if/#else#/endif blocks is
        /// enabled in blocks without an <b>outlining</b> attribute.
        /// </summary>
        /// <value>The default is false.  Note that if enabled, space for the outline markers will only be
        /// reserved if the code contains any collapsible regions.</value>
        public bool OutliningEnabled { get; set; }

        /// <summary>
        /// This is used to set or get whether or not <c>&lt;see&gt;</c> tags are preserved within the code.
        /// </summary>
        /// <value>The default is false to treat them as part of the colorized code.  If set to true, any
        /// <c>&lt;see&gt;</c> tags are preserved so that they may be processed as normal HTML tags.  This is
        /// useful when using the code colorizer in conjunction with a documentation tool such as the
        /// <see href="https://GitHub.com/EWSoftware/SHFB">Sandcastle Help File Builder</see>.</value>
        public bool KeepSeeTags { get; set; }

        /// <summary>
        /// This is used to set or get whether or not to use a default title based on the language name if a
        /// title is not specified.
        /// </summary>
        /// <value>The default is true.  If set to false, no title will appear if one is not specified.</value>
        public bool UseDefaultTitle { get; set; }

        /// <summary>
        /// This is used to set or get the tab size override for the colorizer for code blocks without a
        /// <b>tabSize</b> attribute and no tab size defined in the syntax file for the selected language.
        /// </summary>
        /// <value>The default is eight.</value>
        public int TabSize
        {
            get => defaultTabSize;
            set
            {
                if(value > 0)
                    defaultTabSize = value;
                else
                    defaultTabSize = 8;
            }
        }

        /// <summary>
        /// This is used to set or get the text for the Copy link
        /// </summary>
        /// <value>The default is "Copy".</value>
        public string CopyText { get; set; }

        /// <summary>
        /// This is used to set or get the image URL for the Copy link.
        /// </summary>
        /// <value>The default is "CopyCode.gif".  A copy of this file with the same name suffixed with "_h"
        /// should exist to use as the image when the link is highlighted (i.e. CopyCode_h.gif).</value>
        public string CopyImageUrl { get; set; }

        /// <summary>
        /// This is used to return a read-only dictionary that maps the language IDs to friendly names.
        /// </summary>
        public IReadOnlyDictionary<string, string> FriendlyNames => friendlyNames;

        /// <summary>
        /// This is used to return a read-only dictionary that maps the alternate IDs to actual IDs present in
        /// the syntax file.
        /// </summary>
        public IReadOnlyDictionary<string, string> AlternateIds => alternateIds;

        #endregion

#if DEBUG && BENCHMARK
        #region Benchmarking properties
        //=====================================================================

        private long lastRun, totalTime, totalBytes;
        private int runCount;

        /// <summary>
        /// Returns the computation time of the last call
        /// </summary>
        public double BenchmarkSec => new TimeSpan(lastRun).TotalSeconds;

        /// <summary>
        /// Returns the computation time per character overall
        /// </summary>
        public double BenchmarkSecPerChar => (new TimeSpan(totalTime)).TotalSeconds / totalBytes;

        /// <summary>
        /// Returns the average computation time overall
        /// </summary>
        public double BenchmarkAvgSec => (new TimeSpan(totalTime)).TotalSeconds / runCount;

        #endregion
#endif

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="languageSyntaxFilename">The XML syntax file name</param>
        /// <param name="languageStyleFilename">The XSL style file name</param>
        public CodeColorizer(string languageSyntaxFilename, string languageStyleFilename)
        {
            if(String.IsNullOrWhiteSpace(languageSyntaxFilename))
                throw new ArgumentException("A language syntax filename must be specified", nameof(languageSyntaxFilename));

            if(String.IsNullOrWhiteSpace(languageStyleFilename))
                throw new ArgumentException("A language style filename must be specified", nameof(languageStyleFilename));

            this.LanguageSyntaxFileName = languageSyntaxFilename;
            this.LanguageStyleFileName = languageStyleFilename;

            // Replace function delegate
            replaceByCodeDelegate = new MatchEvaluator(ReplaceByCode);

            // Friendly name dictionary
            friendlyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            alternateIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            this.UseDefaultTitle = true;
            defaultTabSize = 8;
            this.CopyText = "Copy";
            this.CopyImageUrl = "CopyCode.gif";

            rxDic = new RegexDictionary();

            // Load and preprocess language data
            languageSyntax = new XmlDocument();
            friendlyNames.Clear();
            alternateIds.Clear();

            this.BuildSyntax();

            // Load XSL
            languageStyle = new XslCompiledTransform();
            languageStyle.Load(this.LanguageStyleFileName);
        }
        #endregion

        #region Overrides
        //=====================================================================

        /// <summary>
        /// Converting to string
        /// </summary>
        /// <returns>Returns the syntax and style in a string</returns>
        public override string ToString()
        {
            return "<Syntax>" + languageSyntax.ToString() + "</syntax>\n<style>" + languageStyle.ToString() +
                "</style>";
        }
        #endregion

        #region Public Methods
        //=====================================================================

        /// <summary>
        /// Processes HTML and highlight code in <c>&lt;code&gt;...&lt;/code&gt;</c> and
        /// <c>&lt;pre&gt;...&lt;/pre&gt;</c> tags.
        /// </summary>
        /// <param name="htmlText">The HTML text to colorize</param>
        /// <returns>The HTML with colorized code blocks</returns>
        /// <remarks>See highlight.xml for a list of available languages.</remarks>
        public string ProcessAndHighlightText(string htmlText)
        {
            if(String.IsNullOrEmpty(htmlText))
                return htmlText;
            
            if(languageSyntax == null)
                throw new InvalidOperationException("Call Initialize() to load the defaults before attempting " +
                    "to colorize code");

#if DEBUG && BENCHMARK
            var timer = System.Diagnostics.Stopwatch.StartNew();
#endif
            // Colorize the text
            string result = reColorize.Replace(htmlText, replaceByCodeDelegate);

#if DEBUG && BENCHMARK
            timer.Stop();

            System.Threading.Interlocked.Add(ref totalBytes, htmlText.Length);
            System.Threading.Interlocked.Increment(ref runCount);
            System.Threading.Interlocked.Exchange(ref lastRun, timer.Elapsed.Ticks);
            System.Threading.Interlocked.Add(ref totalTime, timer.Elapsed.Ticks);
#endif
            return result;
        }

        /// <summary>
        /// This method is used to colorize a block of plain text using the given rules
        /// </summary>
        /// <param name="plainText">The plain text to colorize</param>
        /// <param name="language">The language to use when colorizing the text</param>
        /// <param name="onlyNormalizeWhiteSpace">False to colorize the text, true to only normalize whitespace
        /// with no colorization</param>
        /// <param name="tabSize">Null for the default tab size or a positive non-zero value to specify a
        /// different tab size.  This will override the language's default tab size if there is one.</param>
        /// <returns>The text with colorized code blocks</returns>
        public string ColorizePlainText(string plainText, string language, bool onlyNormalizeWhiteSpace = false,
          int? tabSize = null)
        {
            List<string> seeTags = null;
            IList<CollapsibleRegion> regions;
            XmlNode languageNode;
            bool tabSizeOverridden = false;
            int seeTagIndex;

            if(languageSyntax == null)
                throw new InvalidOperationException("Call Initialize() to load the defaults before attempting " +
                    "to colorize code");

            if(tabSize == null || tabSize < 1)
                tabSize = defaultTabSize;
            else
                tabSizeOverridden = true;

            // Find the language in the language file by ID or by name
            languageNode = languages.SelectSingleNode("language[@id=\"" + language + "\" or @name=\"" +
                language + "\"]");

            // If not found, try matching a variation
            if(languageNode == null)
            {
                // Try lower case first
                language = language.ToLowerInvariant();

                languageNode = languages.SelectSingleNode("language[@id=\"" + language + "\" or @name=\"" +
                    language + "\"]");

                // Try to map it to an ID if not found
                if(languageNode == null && alternateIds.TryGetValue(language, out string altLanguage))
                {
                    language = altLanguage;
                    languageNode = languages.SelectSingleNode("language[@id=\"" + language + "\"]");
                }
            }

            // If not found, we'll just format it neatly and optionally number it and add collapsible regions
            if(languageNode == null || onlyNormalizeWhiteSpace)
            {
                // Replace tabs with the specified number of spaces to ensure consistency of layout.  Default
                // to eight if not specified.
                if(languageNode != null && languageNode.Attributes["tabSize"] != null && !tabSizeOverridden)
                    tabSize = Convert.ToInt32(languageNode.Attributes["tabSize"].Value,
                        CultureInfo.InvariantCulture);

                // Tidy up the block by stripping any common leading whitespace and converting tabs to spaces
                plainText = WebUtility.HtmlEncode(StripLeadingWhitespace(plainText, tabSize.Value,
                    this.OutliningEnabled, out regions));
            }
            else
            {
                // Replace tabs with the specified number of spaces to ensure consistency of layout.  Default
                // to eight if not specified.
                if(languageNode.Attributes["tabSize"] != null && !tabSizeOverridden)
                    tabSize = Convert.ToInt32(languageNode.Attributes["tabSize"].Value,
                        CultureInfo.InvariantCulture);

                // Tidy up the block by stripping any common leading whitespace and converting tabs to spaces
                plainText = StripLeadingWhitespace(plainText, tabSize.Value, this.OutliningEnabled, out regions);

                // If keeping see tags and it is supported, replace them with a marker character so that they
                // aren't colorized.
                if(this.KeepSeeTags && this.OutputFormat == OutputFormat.Html)
                {
                    seeTags = new List<string>();

                    // Typically, this would be done with a match evaluator but we need it to be thread safe so
                    // we'll manage the matches locally.  Process the list in reverse to preserve the text
                    // indices.  While the matches do appear to come back in order, we'll play it safe and sort
                    // in descending order rather than just reversing the results.
                    foreach(Match m in reExtractSeeTags.Matches(plainText).Cast<Match>().OrderByDescending(m => m.Index))
                    {
                        seeTags.Add(m.Value);
                        plainText = plainText.Substring(0, m.Index) + "\xFF" + plainText.Substring(m.Index + m.Length);
                    }
                }

                // Apply syntax matching to text with the corresponding language
                XmlDocument xmlResult = this.BuildHighlightTree(languageNode, language, false, plainText);

                // Transform the XML to the output format and return the results
                using(StringWriter sw = new StringWriter(CultureInfo.CurrentCulture))
                {
                    languageStyle.Transform(xmlResult, null, sw);

                    plainText = sw.ToString();
                }

                // Replace the markers with the see tags if they were kept
                if(this.KeepSeeTags && seeTags.Count != 0)
                {
                    seeTagIndex = 0;

                    // As above
                    foreach(Match m in reReplaceMarker.Matches(plainText).Cast<Match>().OrderByDescending(m => m.Index))
                    {
                        plainText = plainText.Substring(0, m.Index) + seeTags[seeTagIndex] +
                            plainText.Substring(m.Index + m.Length);
                        seeTagIndex++;
                    }
                }
            }

            // If supported and wanted, add line numbering and/or outlining
            if((this.NumberLines || this.OutliningEnabled) && this.OutputFormat == OutputFormat.Html)
                plainText = NumberAndOutlineHtml(plainText, this.NumberLines, this.OutliningEnabled, regions);
            else
                if(this.NumberLines && this.OutputFormat == OutputFormat.FlowDocument)
                    plainText = NumberFlowDocument(plainText);

            return plainText;
        }
        #endregion

        #region Private class methods
        //=====================================================================

        /// <summary>
        /// This is used to strip a common amount of leading whitespace on all lines of code in the block to
        /// colorize and to convert tabs to a consistent number of spaces.
        /// </summary>
        /// <param name="text">The text containing the lines to clean up.</param>
        /// <param name="tabSize">The number of spaces to which tab characters are converted.</param>
        /// <param name="willAddOutlining">True if outlining will be added, false if not</param>
        /// <param name="regions">On return, if outlining is to be added, this will contain an enumerable list
        /// of collapsible regions.</param>
        private static string StripLeadingWhitespace(string text, int tabSize, bool willAddOutlining,
          out IList<CollapsibleRegion> regions)
        {
            Match m;
            string[] lines;
            string currentLine, tabsToSpaces = new String(' ', tabSize);
            int minSpaces, spaceCount, line, nestingLevel = 0;

            regions = new List<CollapsibleRegion>();

            if(String.IsNullOrEmpty(text))
                return text;

            // Replace "\r\n" with "\n" first for consistency
            lines = text.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // If only one line, just trim it and convert tabs to spaces
            if(lines.Length == 1)
                return lines[0].Trim().Replace("\t", tabsToSpaces);

            minSpaces = Int32.MaxValue;

            // The first pass is used to determine the minimum number of spaces and to truncate blank lines
            for(line = 0; line < lines.Length; line++)
            {
                currentLine = lines[line].Replace("\t", tabsToSpaces);

                if(currentLine.Length != 0)
                {
                    // Add a region boundary?
                    if(willAddOutlining)
                    {
                        m = reCollapseMarkers.Match(currentLine);

                        if(m.Success)
                        {
                            // If it's an #else, insert a pseudo-#endif
                            if(m.Groups[1].Value.StartsWith("#else", StringComparison.OrdinalIgnoreCase))
                            {
                                regions.Add(new CollapsibleRegion(line, nestingLevel));
                                nestingLevel--;
                            }

                            regions.Add(new CollapsibleRegion(line, m, nestingLevel));

                            nestingLevel = regions[regions.Count - 1].NestingLevel;
                        }
                    }

                    for(spaceCount = 0; spaceCount < currentLine.Length; spaceCount++)
                        if(currentLine[spaceCount] != ' ')
                            break;

                    if(spaceCount == currentLine.Length)
                        currentLine = String.Empty;
                    else
                        if(spaceCount < minSpaces)
                            minSpaces = spaceCount;

                    lines[line] = currentLine;
                }
            }

            // Unlikely, but it could happen...
            if(minSpaces == Int32.MaxValue)
                minSpaces = 0;

            // The second pass joins them back together less the determined amount of leading whitespace.
            StringBuilder sb = new StringBuilder(text.Length);

            for(line = 0; line < lines.Length; line++)
            {
                currentLine = lines[line];

                if(currentLine.Length != 0)
                    sb.AppendLine(currentLine.Substring(minSpaces));
                else
                    if(sb.Length > 0)   // Skip leading blank lines
                        sb.AppendLine();
                    else
                        foreach(CollapsibleRegion cr in regions)
                            cr.LineNumber--;
            }

            // Trim off trailing blank lines too
            return sb.ToString().TrimEnd(new char[] { ' ', '\r', '\n' });
        }

        /// <summary>
        /// This is used to number lines and/or add collapsible sections for #region and #if/#else/#endif blocks
        /// based on the current settings for HTML.
        /// </summary>
        /// <param name="text">The text containing the lines to modify.</param>
        /// <param name="addLineNumbers">True to add line numbers, false if not</param>
        /// <param name="addOutlining">True to add outlining, false if not</param>
        /// <param name="regions">If adding outlining, this will contain an enumerable list of the collapsible
        /// regions.</param>
        private static string NumberAndOutlineHtml(string text, bool addLineNumbers, bool addOutlining,
          IList<CollapsibleRegion> regions)
        {
            string[] lines;
            string numPad, spacer, lineInfo, regionType, linePrefix;
            int regionIdx = 0, nestingLevel = 0, mod = 10, line;

            if(String.IsNullOrEmpty(text))
                return text;

            // Replace "\r\n" with "\n" first for consistency
            lines = text.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // Figure out the starting padding for the line numbers
            if(!addLineNumbers || lines.Length < 10)
                numPad = String.Empty;
            else
                if(lines.Length < 100)
                    numPad = "&nbsp;";
                else
                    if(lines.Length < 999)
                        numPad = "&nbsp;&nbsp;";
                    else
                        if(lines.Length < 9999)
                            numPad = "&nbsp;&nbsp;&nbsp;";
                        else
                            numPad = "&nbsp;&nbsp;&nbsp;&nbsp;";

            if(addOutlining && regions.Count != 0)
                spacer = "<span class=\"highlight-spacer\"></span>";
            else
                spacer = "<span class=\"highlight-spacerShort\"></span>";

            for(line = 0; line < lines.Length; line++)
            {
                // Adjust the line number padding?
                if(numPad.Length != 0 && ((line + 1) % mod) == 0)
                {
                    numPad = numPad.Remove(0, 6);
                    mod *= 10;
                }

                // Starting or ending a collapsible region?
                if(regionIdx < regions.Count && regions[regionIdx].LineNumber == line)
                {
                    regionType = regions[regionIdx].RegionType;

                    switch(regionType)
                    {
                        case "#region":
                        case "#if":
                        case "#else":
                            uniqueRegionId++;
                            nestingLevel++;

                            if(!addLineNumbers)
                                lineInfo = String.Empty;
                            else
                                lineInfo = String.Format(CultureInfo.InvariantCulture,
                                    "<span class=\"highlight-lineno\">{0}{1}</span>" +
                                    "<span class=\"highlight-lnborder\"></span>", numPad, line + 1);

                            linePrefix = String.Format(CultureInfo.InvariantCulture,
                                "<span id=\"hrCol{0}\" style=\"display: none;\">{1}<span " +
                                "class=\"highlight-collapsebox\" onclick=\"" +
                                "HighlightExpandCollapse('hrExp{0}', 'hrCol{0}');\">+</span><span " +
                                "class=\"highlight-collapsed\">{2}</span></span><span id=\"hrExp{0}\" " +
                                "style=\"display: inline;\">{1}<span class=\"highlight-collapsebox\" " +
                                "onclick=\"HighlightExpandCollapse('hrCol{0}', " +
                                "'hrExp{0}');\">-</span>", uniqueRegionId, lineInfo, regions[regionIdx].Description);

                            regionIdx++;

                            // Append collapse boxes for other regions on the same line
                            while(regionIdx < regions.Count && regions[regionIdx].LineNumber == line &&
                              regions[regionIdx].IsStart)
                            {
                                linePrefix += String.Format(CultureInfo.InvariantCulture,
                                    "<span id=\"hrCol{0}_{1}\" style=\"display: none;\">" +
                                    "<span class=\"highlight-collapsebox\" onclick=\"" +
                                    "HighlightExpandCollapse('hrExp{0}_{1}', 'hrCol{0}_{1}');\">+</span><span " +
                                    "class=\"highlight-collapsed\">{2}</span></span><span id=\"hrExp{0}_{1}\" " +
                                    "style=\"display: inline;\"><span class=\"highlight-collapsebox\" " +
                                    "onclick=\"HighlightExpandCollapse('hrCol{0}_{1}', " +
                                    "'hrExp{0}_{1}');\">-</span>", uniqueRegionId, regionIdx, regions[regionIdx].Description);

                                regionIdx++;
                            }

                            lines[line] = linePrefix + lines[line];

                            // Close out regions ending on this line
                            while(regionIdx < regions.Count && regions[regionIdx].LineNumber == line &&
                              !regions[regionIdx].IsStart)
                            {
                                regionIdx++;
                                nestingLevel--;
                                lines[line] += "</span>";
                            }

                            spacer = "<span class=\"highlight-expanded\">&nbsp;</span>";
                            break;

                        case "#endif":
                        case "#endregion":
                            // Mismatched?
                            if(nestingLevel == 0)
                            {
                                regionIdx++;

                                if(addLineNumbers)
                                    lines[line] = String.Format(CultureInfo.InvariantCulture,
                                        "<span class=\"highlight-lineno\">{0}{1}</span><span " +
                                        "class=\"highlight-lnborder\"></span>{2}{3}", numPad, line + 1,
                                        spacer, lines[line]);
                                else
                                    lines[line] = spacer + lines[line];
                                break;
                            }

                            nestingLevel--;

                            if(!addLineNumbers)
                                lineInfo = "<span class=\"highlight-endblock\">&nbsp;</span>";
                            else
                                lineInfo = String.Format(CultureInfo.InvariantCulture,
                                    "<span class=\"highlight-lineno\">{0}{1}</span><span " +
                                    "class=\"highlight-lnborder\"></span><span class=\"highlight-endblock\">" +
                                    "&nbsp;</span>", numPad, line + 1);

                            lines[line] = lineInfo + lines[line] + "</span>";
                            regionIdx++;

                            // Close out regions ending on the same line
                            while(regionIdx < regions.Count && regions[regionIdx].LineNumber == line)
                            {
                                regionIdx++;
                                lines[line] += "</span>";
                            }

                            if(nestingLevel == 0)
                                spacer = "<span class=\"highlight-spacer\"></span>";
                            break;
                    }
                }
                else
                    if(addLineNumbers)     // Format the line number part
                        lines[line] = String.Format(CultureInfo.InvariantCulture,
                            "<span class=\"highlight-lineno\">{0}{1}</span>" +
                            "<span class=\"highlight-lnborder\"></span>{2}{3}", numPad, line + 1,
                            spacer, lines[line]);
                    else
                        lines[line] = spacer + lines[line];
            }

            // Close out mismatched regions
            while(nestingLevel-- > 0)
                lines[line - 1] += "</span>";

            return String.Join("\r\n", lines);
        }

        /// <summary>
        /// This is used to number lines for flow documents
        /// </summary>
        /// <param name="text">The text containing the lines to modify.</param>
        private static string NumberFlowDocument(string text)
        {
            string[] lines;
            string lineNum;
            int totalWidth, line;

            if(String.IsNullOrEmpty(text))
                return text;

            // Replace "\r\n" with "\n" first for consistency
            lines = text.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // Figure out the starting padding for the line numbers
            if(lines.Length < 10)
                totalWidth = 1;
            else
                if(lines.Length < 100)
                    totalWidth = 2;
                else
                    if(lines.Length < 999)
                        totalWidth = 3;
                    else
                        if(lines.Length < 9999)
                            totalWidth = 4;
                        else
                            totalWidth = 5;

            for(line = 0; line < lines.Length; line++)
            {
                lineNum = (line + 1).ToString(CultureInfo.InvariantCulture);

                // Adjust the line number padding if necessary
                if(totalWidth != 1)
                    lineNum = lineNum.PadLeft(totalWidth);

                lines[line] = String.Format(CultureInfo.InvariantCulture,
                    "<Span Style=\"{{DynamicResource HighlightLineNo}}\" xml:space=\"preserve\">{0}| </Span>{1}",
                    lineNum, lines[line]);
            }

            return String.Join("\r\n", lines);
        }

        /// <summary>
        /// Build the keyword family regular expressions.
        /// </summary>
        /// <remarks>This method creates regular expression that match a whole keyword family and adds it as a
        /// parameter <c>regexp</c> to the <c>keywordlist</c> node.</remarks>
        private void BuildKeywordRegExp()
        {
            XmlNodeList keywords;
            XmlNode preNode, postNode, wordBound;
            string[] words = new string[100], expression = new string[7];
            int idx;

            // Set the common regular expression parts
            expression[2] = "(";        // Opening
            expression[4] = ")";        // Closing

            // Iterate keyword lists
            foreach(XmlNode keywordListNode in keywordLists.SelectNodes("keywordlist"))
            {
                // Skip any that don't have keywords
                keywords = keywordListNode.SelectNodes("kw");

                if(keywords.Count == 0)
                    continue;

                // See if word boundaries should be used
                wordBound = keywordListNode.Attributes["use-word-boundary"];

                if(wordBound == null || Convert.ToBoolean(wordBound.Value, CultureInfo.InvariantCulture))
                    expression[0] = expression[6] = @"\b";
                else
                    expression[0] = expression[6] = String.Empty;

                // Add pre-expression
                preNode = keywordListNode.Attributes["pre"];

                if(preNode != null)
                    expression[1] = preNode.Value;
                else
                    expression[1] = null;

                // Resize the array if necessary
                if(keywords.Count > words.Length)
                    words = new string[keywords.Count];

                // Add kw elements to the regular expression
                for(idx = 0; idx < keywords.Count; idx++)
                    words[idx] = Regex.Escape(keywords[idx].InnerText);

                expression[3] = String.Join("|", words, 0, idx);

                // Add post-expression
                postNode = keywordListNode.Attributes["post"];

                if(postNode != null)
                    expression[5] = postNode.Value;
                else
                    expression[5] = null;

                // Add to keywordListNode
                keywordListNode.XmlSetAttribute("regexp", String.Join(null, expression, 0, 7));
            }
        }

        /// <summary>
        /// This builds regular expressions out of the context node
        /// </summary>
        /// <param name="languageNode">The language node.</param>
        /// <param name="contextNode">The context node.</param>
        /// <remarks>This method creates regular expressions that match all the context rules and adds it as a
        /// parameter <c>regexp</c> to the context node.</remarks>
        /// <returns>The regular expression string for the context node.</returns>
        /// <exception cref="InvalidOperationException">This is thrown if the regular expression rule is missing
        /// the expression attribute, if the keyword list could not be found, or if the keyword list family or
        /// regular expression could not be found.</exception>
        private string BuildRuleRegExp(XmlNode languageNode, XmlNode contextNode)
        {
            StringBuilder sb = new StringBuilder("(", 1024);
            XmlNode familyNode, kwList, node;

            foreach(XmlNode ruleNode in contextNode.ChildNodes)
            {
                if(ruleNode.Name == "#comment")
                    continue;

                // Apply the rule
                if(ruleNode.Name == "detect2chars")
                {
                    sb.Append(Regex.Escape(ruleNode.Attributes["char"].Value));
                    sb.Append(Regex.Escape(ruleNode.Attributes["char1"].Value));
                }
                else if(ruleNode.Name == "detectchar")
                {
                    sb.Append(Regex.Escape(ruleNode.Attributes["char"].Value));
                }
                else if(ruleNode.Name == "linecontinue")
                {
                    sb.Append('\n');
                }
                else if(ruleNode.Name == "regexp")
                {
                    node = ruleNode.Attributes["expression"];

                    if(node == null)
                        throw new InvalidOperationException("Regular expression rule missing expression attribute");

                    // Append the regular expression
                    sb.Append(node.Value);

                    // Add the regular expression to the dictionary
                    rxDic.AddKey(languageNode, contextNode, ruleNode, node.Value);
                }
                else if(ruleNode.Name == "keyword")
                {
                    // Find the keyword list
                    familyNode = ruleNode.Attributes["family"];

                    if(familyNode == null)
                        throw new InvalidOperationException("Keyword rule missing family");

                    kwList = keywordLists.SelectSingleNode("keywordlist[@id=\"" + familyNode.Value + "\"]");

                    if(kwList == null)
                        throw new InvalidOperationException("Could not find keywordlist (family: " +
                            familyNode.Value + ")");

                    node = kwList.Attributes["regexp"];

                    if(node == null)
                        throw new InvalidOperationException("Could not find keywordlist regular expression");

                    // Append the regular expression
                    sb.Append(node.Value);

                    // Add it to the dictionary too
                    rxDic.AddKey(languageNode, contextNode, kwList, "(" + node.Value + ")");
                }

                sb.Append('|');
            }

            if(sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append(')');
                return sb.ToString();
            }

            return String.Empty;
        }

        /// <summary>
        /// This precompiles regular expressions and search strings and prepares rules attribute.
        /// </summary>
        /// <param name="languageNode">The context node</param>
        private void BuildRules(XmlNode languageNode)
        {
            string regExp;

            // Create regular expressions for each context
            foreach(XmlNode contextNode in languageNode.SelectNodes("contexts/context"))
            {
                regExp = this.BuildRuleRegExp(languageNode, contextNode);

                // Add or update the attribute
                contextNode.XmlSetAttribute("regexp", regExp);

                // Create regular expression and add to dictionary
                rxDic.AddKey(languageNode, contextNode, regExp);
            }
        }

        /// <summary>
        /// This prepares the syntax XML file for use.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if the highlight node could not be found
        /// in the configuration file.
        /// </exception>
        private void BuildSyntax()
        {
            XmlNode needBuildNode, highlightNode;

            using(var reader = XmlReader.Create(this.LanguageSyntaxFileName, new XmlReaderSettings()))
            {
                languageSyntax.Load(reader);
            }

            // Check to see if a build is needed
            highlightNode = languageSyntax.SelectSingleNode("highlight");

            if(highlightNode == null)
                throw new InvalidOperationException("Could not find highlight node");

            languages = highlightNode.SelectSingleNode("languages");

            if(languages == null)
                throw new InvalidOperationException("Could not find languages node");

            keywordLists = highlightNode.SelectSingleNode("keywordlists");

            if(keywordLists == null)
                throw new InvalidOperationException("Could not find keywordlists node");

            needBuildNode = highlightNode.Attributes["needs-build"];

            if(needBuildNode == null || needBuildNode.Value == "yes")
            {
                // First, build keyword regexp
                this.BuildKeywordRegExp();

                // Iterate languages and pre-build the regular expressions
                foreach(XmlNode languageNode in languages.SelectNodes("language"))
                {
                    friendlyNames.Add(languageNode.Attributes["id"].Value, languageNode.Attributes["name"].Value);
                    this.BuildRules(languageNode);
                }

                // Add the mapped language names to the friendly list too
                foreach(XmlNode mapEntry in highlightNode.SelectNodes("map/language"))
                {
                    friendlyNames.Add(mapEntry.Attributes["from"].Value,
                        friendlyNames[mapEntry.Attributes["to"].Value]);
                    alternateIds.Add(mapEntry.Attributes["from"].Value, mapEntry.Attributes["to"].Value);
                }

                // Update node to reflect that it has been built
                highlightNode.XmlSetAttribute("needs-build", "no");
            }

            // Save the file if asked
            XmlNode saveBuildNode = highlightNode.Attributes["save-build"];

            if(saveBuildNode != null && saveBuildNode.Value == "yes")
                languageSyntax.Save(this.LanguageSyntaxFileName);
        }

        /// <summary>
        /// This is used to find the rule that triggered the match.
        /// </summary>
        /// <param name="languageNode">The language node.</param>
        /// <param name="contextNode">The context node.</param>
        /// <param name="matchText">The text that matched the context regular expression</param>
        /// <remarks>If the Regex finds a rule occurrence, this method is used to find which rule has been
        /// triggered.</remarks>
        /// <returns>The node that triggered the match or null if no node is found.</returns>
        private XmlNode FindRule(XmlNode languageNode, XmlNode contextNode, string matchText)
        {
            XmlNode node;
            Regex regExp;
            Match m;

            // Match the regular expression
            foreach(XmlNode ruleNode in contextNode.ChildNodes)
            {
                if(ruleNode.Name == "#comment")
                    continue;

                if(ruleNode.Name == "detect2chars")
                {
                    if(matchText == ruleNode.Attributes["char"].Value + ruleNode.Attributes["char1"].Value)
                        return ruleNode;
                }
                else if(ruleNode.Name == "detectchar")
                {
                    if(matchText == ruleNode.Attributes["char"].Value)
                        return ruleNode;
                }
                else if(ruleNode.Name == "linecontinue")
                {
                    if(matchText == "\n")
                        return ruleNode;
                }
                else if(ruleNode.Name == "regexp")
                {
                    regExp = rxDic.GetKey(languageNode, contextNode, ruleNode);

                    m = regExp.Match(matchText);

                    if(m.Success)
                        return ruleNode;
                }
                else if(ruleNode.Name == "keyword")
                {
                    node = keywordLists.SelectSingleNode("keywordlist[@id=\"" +
                        ruleNode.Attributes["family"].Value + "\"]");

                    regExp = rxDic.GetKey(languageNode, contextNode, node);

                    m = regExp.Match(matchText);

                    if(m.Success)
                        return ruleNode;
                }
            }

            return null;
        }

        /// <summary>
        /// This is used to apply the context rules successively to the code string that needs colorizing.
        /// </summary>
        /// <param name="languageNode">The language node.</param>
        /// <param name="contextNode">The context node.</param>
        /// <param name="code">The code block to parse and convert.</param>
        /// <param name="parsedCodeNode">Parent node that will contain the parsed code.</param>
        /// <remarks>This method uses the pre-computed regular expressions of the context rules, rule matching,
        /// etc.  The results are added to the XML document in the parsedcodeNode.</remarks>
        /// <exception cref="InvalidOperationException">This is thrown if a rule match cannot be found or if a
        /// matching context node cannot be found.</exception>
        private void ApplyRules(XmlNode languageNode, XmlNode contextNode, string code, XmlNode parsedCodeNode)
        {
            XmlNode attributeNode;
            Regex regExp;
            Match m;

            // Get the regular expression for the default context
            regExp = rxDic.GetKey(languageNode, contextNode);

            while(code.Length > 0)
            {
                // Apply
                m = regExp.Match(code);

                if(!m.Success)
                {
                    parsedCodeNode.XmlAddChildCDATAElement(contextNode.Attributes["attribute"].Value, code);

                    // Finished parsing
                    break;
                }

                // Add the skipped text if not empty
                if(m.Index != 0)
                {
                    attributeNode = contextNode.Attributes["attribute"];

                    if(attributeNode != null && attributeNode.Value != "hidden")
                        parsedCodeNode.XmlAddChildCDATAElement(attributeNode.Value, code.Substring(0, m.Index));
                }

                // Find the rule that caused the match
                XmlNode ruleNode = this.FindRule(languageNode, contextNode, m.Value);

                if(ruleNode == null)
                    throw new InvalidOperationException("Didn't find matching rule, regular expression false? " +
                        "(context: " + contextNode.Attributes["id"].Value + ")");

                // Check to see if the match needs to be added to the result
                attributeNode = ruleNode.Attributes["attribute"];

                if(attributeNode != null && attributeNode.Value != "hidden")
                    parsedCodeNode.XmlAddChildCDATAElement(attributeNode.Value, m.Value);

                // Update the context if necessary
                if(contextNode.Attributes["id"].Value != ruleNode.Attributes["context"].Value)
                {
                    // Get the new context
                    contextNode = languageNode.SelectSingleNode("contexts/context[@id=\"" + ruleNode.Attributes[
                        "context"].Value + "\"]");

                    if(contextNode == null)
                        throw new InvalidOperationException("Didn't find matching context, error in XML " +
                            "specification?");

                    // Get the new regular expression
                    regExp = rxDic.GetKey(languageNode, contextNode);
                }

                // Removed the parsed code and carry on
                code = code.Substring(m.Index + m.Length);
            }
        }

        /// <summary>
        /// Create and populate an xml document with the corresponding parsed language tree for highlighting.
        /// </summary>
        /// <param name="languageNode">The language node to use for parsing the code.</param> 
        /// <param name="rootTag">Root tag (under parsed code) for the generated xml tree.</param>
        /// <param name="inBox">True if the code should be rendered in a box (<c>pre</c> element) or inline
        /// (a <c>span</c> element).</param>
        /// <param name="code">The code to parse</param>
        /// <returns>Returns an <seealso cref="XmlDocument"/> document containing the parsed nodes.</returns>
        /// <remarks>This method builds an XML tree containing context node.  Use an XSL file to render it.</remarks>
        /// <exception cref="ArgumentException">This is thrown if the language node does not contain any
        /// contexts or if there is no default context node.</exception>
        /// <exception cref="InvalidOperationException">This is thrown if the main node of the parsed code XML
        /// document cannot be created.</exception>
        private XmlDocument BuildHighlightTree(XmlNode languageNode, string rootTag, bool inBox, string code)
        {
            XmlDocument xmlResult;
            XmlNode contextNode, resultMainNode;
            string defaultContext;

            // Get the contexts
            contextNode = languageNode.SelectSingleNode("contexts");

            if(contextNode == null)
                throw new ArgumentException("Could not find contexts node for " + rootTag + "language");

            // Getting the default context
            defaultContext = contextNode.Attributes["default"].Value;

            contextNode = contextNode.SelectSingleNode("context[@id=\"" + defaultContext + "\"]");

            if(contextNode == null)
                throw new ArgumentException("Could not find the default context for " + rootTag +
                    " language (default = " + defaultContext + ")");

            // Create the XML result and add the main node
            xmlResult = new XmlDocument();

            resultMainNode = xmlResult.CreateElement("parsedcode");

            if(resultMainNode == null)
                throw new InvalidOperationException("Could not create main node parsedcode");

            xmlResult.AppendChild(resultMainNode);

            // Add the language and in-box attributes to it
            resultMainNode.XmlSetAttribute("lang", rootTag);
            resultMainNode.XmlSetAttribute("in-box", inBox ? "1" : "0");

            // Parse the code and populate the XML document
            this.ApplyRules(languageNode, contextNode, code, resultMainNode);

            return xmlResult;
        }

        /// <summary>
        /// This method is used as the match evaluator for the main colorizer regular expression
        /// </summary>
        /// <param name="match">Full match</param>
        private String ReplaceByCode(Match match)
        {
            List<string> seeTags = null;
            string tag = match.Groups[1].Value, language = match.Groups[5].Value,
                   matchText = WebUtility.HtmlDecode(match.Groups[8].Value);

            XmlNode languageNode;
            MatchCollection otherOpts;
            IList<CollapsibleRegion> regions;

            int seeTagIndex, tabSize = defaultTabSize;
            bool numberLinesLocal = this.NumberLines, outliningLocal = this.OutliningEnabled,
                tabSizeOverridden = false, keepSeeTagsLocal = this.KeepSeeTags, isDisabled = false, inBox = false;
            string title = null;

            // See if the other options have been specified
            otherOpts = reOptOverrides.Matches(match.Groups[2].Value + " " + match.Groups[7].Value);

            foreach(Match m in otherOpts)
            {
                if(m.Groups[1].Value.Length != 0)
                    numberLinesLocal = Convert.ToBoolean(m.Groups[4].Value, CultureInfo.InvariantCulture);

                if(m.Groups[6].Value.Length != 0)
                    outliningLocal = Convert.ToBoolean(m.Groups[9].Value, CultureInfo.InvariantCulture);

                if(m.Groups[11].Value.Length != 0)
                {
                    tabSize = Convert.ToInt32(m.Groups[14].Value, CultureInfo.InvariantCulture);

                    if(tabSize < 1)
                        tabSize = defaultTabSize;
                    else
                        tabSizeOverridden = true;
                }

                if(m.Groups[16].Value.Length != 0)
                {
                    title = m.Groups[19].Value.Trim();

                    if(title.Length == 0)
                        title = "&#xa0;";
                }

                if(m.Groups[21].Value.Length != 0)
                    keepSeeTagsLocal = Convert.ToBoolean(m.Groups[24].Value, CultureInfo.InvariantCulture);

                if(m.Groups[26].Value.Length != 0)
                    isDisabled = Convert.ToBoolean(m.Groups[29].Value, CultureInfo.InvariantCulture);
            }

            // Find the language in the language file by ID or by name
            languageNode = languages.SelectSingleNode("language[@id=\"" + language + "\" or @name=\"" +
                language + "\"]");

            // If not found, try matching a variation
            if(languageNode == null)
            {
                // Try lower case first
                language = language.ToLowerInvariant();

                languageNode = languages.SelectSingleNode("language[@id=\"" + language + "\" or @name=\"" +
                    language + "\"]");

                // Try to map it to an ID if not found
                if(languageNode == null && alternateIds.TryGetValue(language, out string altLanguage))
                {
                    language = altLanguage;
                    languageNode = languages.SelectSingleNode("language[@id=\"" + language + "\"]");
                }
            }

            // If not found, we'll just format it neatly and optionally number it and add collapsible regions
            if(languageNode == null || isDisabled)
            {
                // Replace tabs with the specified number of spaces to ensure consistency of layout.  Default
                // to eight if not specified.
                if(languageNode != null && languageNode.Attributes["tabSize"] != null && !tabSizeOverridden)
                    tabSize = Convert.ToInt32(languageNode.Attributes["tabSize"].Value,
                        CultureInfo.InvariantCulture);

                // This is used to determine whether to render the colorized code in a <span> or a <pre> tag
                inBox = tag.Equals("pre", StringComparison.OrdinalIgnoreCase);

                // Tidy up the block by stripping any common leading whitespace and converting tabs to spaces
                matchText = StripLeadingWhitespace(match.Groups[8].Value, tabSize, outliningLocal, out regions);
            }
            else
            {
                // Replace tabs with the specified number of spaces to ensure consistency of layout.  Default
                // to eight if not specified.
                if(languageNode.Attributes["tabSize"] != null && !tabSizeOverridden)
                    tabSize = Convert.ToInt32(languageNode.Attributes["tabSize"].Value,
                        CultureInfo.InvariantCulture);

                // Tidy up the block by stripping any common leading whitespace and converting tabs to spaces
                matchText = StripLeadingWhitespace(matchText, tabSize, outliningLocal, out regions);

                // This is used to determine whether to render the colorized code in a <span> or a <pre> tag
                inBox = tag.Equals("pre", StringComparison.OrdinalIgnoreCase);

                // If keeping see tags, replace them with a marker character so that they aren't colorized
                if(keepSeeTagsLocal)
                {
                    seeTags = new List<string>();

                    // Typically, this would be done with a match evaluator but we need it to be thread safe so
                    // we'll manage the matches locally.  Process the list in reverse to preserve the text
                    // indices.  While the matches do appear to come back in order, we'll play it safe and sort
                    // in descending order rather than just reversing the results.
                    foreach(Match m in reExtractSeeTags.Matches(matchText).Cast<Match>().OrderByDescending(m => m.Index))
                    {
                        seeTags.Add(m.Value);
                        matchText = matchText.Substring(0, m.Index) + "\xFF" + matchText.Substring(m.Index + m.Length);
                    }
                }

                // Apply syntax matching to text with the corresponding language
                XmlDocument xmlResult = this.BuildHighlightTree(languageNode, language, inBox, matchText);

                // Transform the XML to HTML and return the results
                using(StringWriter sw = new StringWriter(CultureInfo.CurrentCulture))
                {
                    languageStyle.Transform(xmlResult, null, sw);

                    matchText = sw.ToString();
                }

                // Replace the markers with the see tags if they were kept
                if(keepSeeTagsLocal && seeTags.Count != 0)
                {
                    seeTagIndex = 0;

                    // As above
                    foreach(Match m in reReplaceMarker.Matches(matchText).Cast<Match>().OrderByDescending(m => m.Index))
                    {
                        matchText = matchText.Substring(0, m.Index) + seeTags[seeTagIndex] +
                            matchText.Substring(m.Index + m.Length);
                        seeTagIndex++;
                    }
                }
            }

            // Add line numbers and/or outlining if needed
            if(inBox)
            {
                if(numberLinesLocal || outliningLocal)
                    matchText = NumberAndOutlineHtml(matchText, numberLinesLocal, outliningLocal, regions);

                // Add the <pre> tag to it
                matchText = "<pre class=\"highlight-pre\">" + matchText + "</pre>";

                // Use a default title if necessary
                if(String.IsNullOrEmpty(title))
                {
                    if(this.UseDefaultTitle && !friendlyNames.TryGetValue(language, out title) && languageNode != null)
                        title = languageNode.Attributes["name"].Value;

                    if(String.IsNullOrEmpty(title))
                        title = "&#xa0;";
                }

                // Add the title div with the title text and Copy span
                matchText = String.Format(CultureInfo.InvariantCulture,
                    "<div class=\"highlight-title\"><span class=\"highlight-copycode\" " +
                    "onkeypress=\"CopyColorizedCodeCheckKey(this.parentNode, event);\" " +
                    "onmouseover=\"CopyCodeChangeIcon(this)\" " +
                    "onmouseout=\"CopyCodeChangeIcon(this)\" " +
                    "onclick=\"CopyColorizedCode(this.parentNode);\"><img src=\"{0}\" " +
                    "style=\"margin-right: 5px;\" />{1}</span>{2}</div>{3}", this.CopyImageUrl, this.CopyText,
                    title, matchText);
            }

            return matchText;
        }
        #endregion
    }
}
