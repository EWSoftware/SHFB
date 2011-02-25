//=============================================================================
// System  : Code Colorizer Library
// File    : CodeColorizer.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 06/26/2009
// Compiler: Microsoft Visual C#
//
// This is used to colorize blocks of code for output as HTML.  The original
// Code Project article by Jonathan can be found at:
// http://www.codeproject.com/csharp/highlightcs.asp.
//
#region Modifications by Eric Woodruff
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006-06/2009:
//
// Overall:
//      Updated for use with .NET 2.0 and rewrote various parts to streamline
//      the code and improve performance.  See source files for details.
//
//      Removed all dead code from the project and flattened the namespace
//      hierarchy.
//
//      Reworked the colorizer to load the default configuration files on
//      first use if not specified.  Also switched to using the built in
//      System.Configuration.DictionarySectionHandler for the config section.
//
//      Made various modifications to allow exceptions to propagate up to
//      the caller.  Helps indicate when you've hosed up the configuration.
//
//      Wrapped timer stuff in conditional blocks to remove it from release
//      builds.
//
//      Gave the assembly a strong name.
//
// In this file:
//      Converted the public fields to properties.
//
//      Fixed bug in FindRule that caused it to return null if there was only
//      one keyword set in a context.
//
//      Added missing support for the <code lang="xxx"> tag.  Also, "language"
//      is an acceptable alternative for the "lang" attribute in either the
//      <code> or <pre> tag.
//
//      Added support for the tabSize attribute to convert tabs to X number of
//      spaces to ensure code layout consistency.
//
//      Added code to strip any common leading whitespace from all lines.
//
//      Added support for line numbering and folding of #region and
//      #if/#else/#endif statements using numberLines and outlining attributes.
//
//      Added support for a title attribute.
//
//      Added support for a keepSeeTags attribute that preserves <see> tags
//      within the code so that they can be processed later by a documentation
//      tool such as the Sandcastle Help File Builder.
//
//      Added a property to return a dictionary that maps the language IDs to
//      the friendly names.  Also added support for default titles based on
//      the language.
//
//      Added support for mapping variations on language IDs to actual entries
//      in the syntax file to make it more flexible in finding rule sets.
//
//      Incorporated changes from Joey Bradshaw to support turning off the
//      word boundaries on language keywords.
//
//  Changes to highlight.css/.xml/.xsl:
//      Reworked the keyword lists to share common keywords amongst similar
//      languages.
//
//      Added support for C#, VB.NET, and J#.  Also added "name" attribute to
//      each "language" element to specify a friendly name.
//
//      Added a <map> element to allow mapping of language name variations to
//      existing entries in the syntax file.
//
//      Removed the preprocessor keyword list and added a generic handler for
//      preprocessor directives in each language that needed it.
//
//      Added <include.file> handler for C and C++.
//
//      Added number handler to each languages and the XSL transformation.
//
//      Reworked the VB comment detection to prevent it double-spacing the
//      comments and moved REM keyword into it as well.
//
//      Modified the style names to be generic.
//
//      Incorporated changes from Joey Bradshaw to support colorization of
//      PowerShell scripts.
//
//      Incorporated changes from Golo Roden to support colorization of
//      Python code.
//
#endregion
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Xsl;

namespace ColorizerLibrary
{
	/// <summary>
	/// Code colorizer provides a flexible solution for colorizing code
	/// </summary>
    /// <remarks>Original Author: Jonathan de Halleux, dehalleux@pelikhan.com,
    /// 2003.
    /// <p/>Modified by Eric Woodruff (Eric@EWoodruff.us) 11/2006. The original
    /// Code Project article and code by Jonathan can be found at:
    /// <a href="http://www.codeproject.com/csharp/highlightcs.asp">
    /// http://www.codeproject.com/csharp/highlightcs.asp</a>.</remarks>
    public class CodeColorizer
    {
        #region Collapsible region tracking class
        private sealed class CollapsibleRegion
        {
            private int lineNo, nestingLevel;
            private string regionType, description;
            private bool isStart;

            #region Properties
            /// <summary>
            /// The line number on which the region part occurs
            /// </summary>
            internal int LineNumber
            {
                get { return lineNo; }
                set { lineNo = value; }
            }

            /// <summary>
            /// This returns the nesting level for the entry
            /// </summary>
            internal int NestingLevel
            {
                get { return nestingLevel; }
            }

            /// <summary>
            /// The region type
            /// </summary>
            internal string RegionType
            {
                get { return regionType; }
            }

            /// <summary>
            /// The description for the collapsed text
            /// </summary>
            internal string Description
            {
                get { return description; }
            }

            /// <summary>
            /// This returns true if this entry represents the start of a
            /// region.
            /// </summary>
            internal bool IsStart
            {
                get { return isStart; }
            }
            #endregion

            #region Constructors
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="line">The line number</param>
            /// <param name="match">The match</param>
            /// <param name="nesting">The nesting level</param>
            internal CollapsibleRegion(int line, Match match, int nesting)
            {
                int idx;

                lineNo = line;
                regionType = match.Groups[1].Value.ToLower(
                    CultureInfo.InvariantCulture);
                nestingLevel = nesting;

                // Remove spaces from VB.NET style end statements
                if(regionType.StartsWith("#end ", StringComparison.Ordinal))
                    regionType = regionType.Replace(" ", String.Empty);
                else
                {
                    // Remove name or condition
                    idx = regionType.IndexOf(' ');

                    if(idx != -1)
                        regionType = regionType.Substring(0, idx);
                }

                // Certain collapsed blocks have a description.  Also, certain
                // blocks start or end on the previous or next line.
                switch(regionType)
                {
                    case "#region":
                        description = HttpUtility.HtmlEncode(
                            match.Groups[2].Value.Trim());

                        if(description.Length == 0)
                            description = "...";

                        nestingLevel++;
                        isStart = true;
                        break;

                    case "#if":
                        lineNo++;
                        description = "...";
                        nestingLevel++;
                        isStart = true;
                        break;

                    case "#else":
                        lineNo++;
                        description = "...";
                        nestingLevel++;
                        isStart = true;
                        break;

                    case "#endif":
                        lineNo--;
                        nestingLevel--;
                        break;
                }
            }

            /// <summary>
            /// Constructor.  This version assumes it's an #endif
            /// </summary>
            /// <param name="line">The line number</param>
            /// <param name="nesting">The nesting level</param>
            internal CollapsibleRegion(int line, int nesting)
            {
                lineNo = line - 1;
                regionType = "#endif";
                nestingLevel = nesting - 1;
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================
        // Private data members

        // This regular expression is used to search for and colorize the
        // code blocks.
        private static Regex reColorize = new Regex(
            "<\\s*(pre|code)\\s+([^>]*?)(lang(?:uage)?\\s*=\\s*(\"|')?" +
            "([a-z0-9\\-+#]+)(\"|')?)([^>]*?)>((.|\\n)*?)<\\s*/\\s*" +
            "(pre|code)\\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // This is used to find option overrides
        private static Regex reOptOverrides = new Regex(
            "((numberLines\\s*=\\s*(\"|')?(true|false)(\"|')?))|" +
            "((outlining\\s*=\\s*(\"|')?(true|false)(\"|')?))|" +
            "((tabSize\\s*=\\s*(\"|')?([0-9]+)(\"|')?))|" +
            "((title\\s*=\\s*(\"([^\"]+)\"|\\'([^\\']+)\\')))|" +
            "((keepSeeTags\\s*=\\s*(\"|')?(true|false)(\"|')?))",
            RegexOptions.IgnoreCase);

        // These are used to preserve <see> tags so that they aren't colorized
        private static Regex reExtractSeeTags = new Regex(
            "(<\\s*see(?<PreAttrs>\\s+[^>]*)[a-z]ref\\s*=" +
            "\\s*\"(?<Link>.+?)\"(?<PostAttrs>.*?))(/>|(>(?<Content>.*?)" +
            "<\\s*/\\s*see\\s*>))", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        // The marker is HTML encoded after colorization
        private static Regex reReplaceMarker = new Regex("&#255;");

        private List<string> seeTags;
        private int seeTagIndex;
        private MatchEvaluator onSeeTagFound, onMarkerFound;

        // This is used to find the collapsible region boundaries
        private static Regex reCollapseMarkers = new Regex(
            @"^\s*(#region\s(.*)|#if\s(.*)|#else|#end\s?if|#end\s?region)",
            RegexOptions.IgnoreCase);

        private List<CollapsibleRegion> regions;

		// Syntax description file, friendly name dictionary, and alternate
        // IDs dictionary.
		private XmlDocument languageSyntax;
        private XmlNode languages, keywordLists;
        private Dictionary<string, string> friendlyNames, alternateIds;

		// Code stylesheet
		private XslCompiledTransform languageStyle;

        // Delegate for Regex.Replace
		private MatchEvaluator replaceByCodeDelegate;

        private bool inBox,             // Boxed, non-boxed code flag
                     numberLines,       // Line numbering flag
                     outlining,         // Outlining flag
                     useDefaultTitle,   // Use default title
                     keepSeeTags;       // Keep <see> tags in the code

        private int  defaultTabSize;    // Tab size override

        // Text of the Copy link and the location of the image file
        private string copyText, copyImageUrl;

        // Regular expressions cache
        private RegexDictionary rxDic;

        // Language syntax filename and style filename
		private string languageSyntaxFileName, languageStyleFileName;

#if DEBUG
		/// <summary>
		/// A timer
		/// </summary>
		private AvgTimer timer;
#endif
		#endregion

		#region Properties
		/// <summary>
		/// The syntax file name
		/// </summary>
		public string LanguageSyntaxFileName
        {
            get
            {
                if(languageSyntaxFileName == null)
                    this.LoadDefaultConfigFiles();

                return languageSyntaxFileName;
            }
            set { languageSyntaxFileName = value; }
        }

		/// <summary>
		/// The style file name
		/// </summary>
		public string LanguageStyleFileName
        {
            get
            {
                if(languageStyleFileName == null)
                    this.LoadDefaultConfigFiles();

                return languageStyleFileName;
            }
            set { languageStyleFileName = value; }
        }

        /// <summary>
        /// This is used to set or get whether or not lines will be
        /// numbered in code blocks without a <b>numberLines</b> attribute.
        /// </summary>
        /// <value>The default is false</value>
        public bool NumberLines
        {
            get { return numberLines; }
            set { numberLines = value; }
        }

        /// <summary>
        /// This is used to set or get whether or not outlining of #region
        /// and #if/#else#/endif blocks is enabled in blocks without an
        /// <b>outlining</b> attribute.
        /// </summary>
        /// <value>The default is false.  Note that if enabled, space for the
        /// outline markers will only be reserved if the code contains any
        /// collapsible regions.</value>
        public bool OutliningEnabled
        {
            get { return outlining; }
            set { outlining = value; }
        }

        /// <summary>
        /// This is used to set or get whether or not <c>&lt;see&gt;</c> tags
        /// are preserved within the code.
        /// </summary>
        /// <value>The default is false to treat them as part of the colorized
        /// code.  If set to true, any <c>&lt;see&gt;</c> tags are preserved so
        /// that they may be processed as normal HTML tags.  This is useful when
        /// using the code colorizer in conjunction with a documentation tool
        /// such as the <see href="http://SHFB.CodePlex.com">Sandcastle
        /// Help File Builder</see>.</value>
        public bool KeepSeeTags
        {
            get { return keepSeeTags; }
            set { keepSeeTags = value; }
        }

        /// <summary>
        /// This is used to set or get whether or not to use a default title
        /// based on the language name if a title is not specified.
        /// </summary>
        /// <value>The default is true.  If set to false, no title will appear
        /// if one is not specified.</value>
        public bool UseDefaultTitle
        {
            get { return useDefaultTitle; }
            set { useDefaultTitle = value; }
        }

        /// <summary>
        /// This is used to set or get the tab size override for the colorizer
        /// for code blocks without a <b>tabSize</b> attribute and no tab size
        /// defined in the syntax file for the selected language.
        /// </summary>
        /// <value>The default is eight.</value>
        public int TabSize
        {
            get { return defaultTabSize; }
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
        public string CopyText
        {
            get { return copyText; }
            set { copyText = value; }
        }

        /// <summary>
        /// This is used to set or get the image URL for the Copy link.
        /// </summary>
        /// <value>The default is "CopyCode.gif".  A copy of this file with
        /// the same name suffixed with "_h" should exist to use as the image
        /// when the link is highlighted (i.e. CopyCode_h.gif).</value>
        public string CopyImageUrl
        {
            get { return copyImageUrl; }
            set { copyImageUrl = value; }
        }

        /// <summary>
        /// This is used to return a dictionary that maps the language IDs to
        /// friendly names.
        /// </summary>
        public Dictionary<string, string> FriendlyNames
        {
            get { return friendlyNames; }
        }

        /// <summary>
        /// This is used to return a dictionary that maps the alternate IDs
        /// to actual IDs present in the syntax file.
        /// </summary>
        public Dictionary<string, string> AlternateIds
        {
            get { return alternateIds; }
        }
        #endregion

#if DEBUG
        #region Benchmarking properties
        /// <summary>
        /// Returns the computation time of the last call
        /// </summary>
        public double BenchmarkSec
        {
            get { return timer.Duration; }
        }

		/// <summary>
		/// Returns the computation time per character of the last call
		/// </summary>
		public double BenchmarkSecPerChar
		{
			get { return timer.DurationPerQuantity; }
		}

		/// <summary>
		/// Returns the average computation time
		/// </summary>
		public double BenchmarkAvgSec
		{
			get { return timer.DurationPerRun; }
		}
        #endregion
#endif

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>Unless specified via the properties, the default language
        /// and style files will be retrieved from the application
        /// configuration file on first use.</remarks>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public CodeColorizer()
        {
#if DEBUG
			timer = new AvgTimer();
#endif
            // Replace function delegate
            replaceByCodeDelegate = new MatchEvaluator(ReplaceByCode);

            // Collapsible region tracking list
            regions = new List<CollapsibleRegion>();

            // See tag preservation
            seeTags = new List<string>();
            onSeeTagFound = new MatchEvaluator(OnSeeTagFound);
            onMarkerFound = new MatchEvaluator(OnMarkerFound);

            // Friendly name dictionary
            friendlyNames = new Dictionary<string, string>();
            alternateIds = new Dictionary<string, string>();
        }

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="languageSyntax">XML syntax file name</param>
		/// <param name="languageStyle">XSL style file name</param>
		public CodeColorizer(string languageSyntax, string languageStyle) :
          this()
		{
			this.LanguageSyntaxFileName = languageSyntax;
			this.LanguageStyleFileName = languageStyle;

			// Load the language file
			this.Init();
		}

		#endregion

		#region Overrides
		/// <summary>
		/// Converting to string
		/// </summary>
		/// <returns>Returns the syntax and style in a string</returns>
		public override string ToString()
		{
			return "<Syntax>" + languageSyntax.ToString() +
				"</syntax>\n<style>" + languageStyle.ToString() + "</style>";
		}
		#endregion

		#region Public Methods
		/// <summary>
        /// Load the language file and preprocess it. Also loads the XSL file.
        /// </summary>
		/// <remarks>Call this method to reload the language files and reset
        /// the colorizer to its default state.</remarks>
		public void Init()
		{
            numberLines = outlining = false;
            useDefaultTitle = true;
            defaultTabSize = 8;
            copyText = "Copy";
            copyImageUrl = "CopyCode.gif";

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

		/// <summary>
        /// Processes HTML and highlight code in
        /// <b>&lt;code&gt;...&lt;/code&gt;</b> and
        /// <b>&lt;pre&gt;...&lt;/pre&gt;</b> tags.
        /// </summary>
		/// <param name="htmlText">The HTML text to colorize</param>
		/// <returns>HTML with colorized code blocks</returns>
        /// <remarks>See highlight.xml for a list of available languages.</remarks>
		public string ProcessAndHighlightText(string htmlText)
		{
            if(String.IsNullOrEmpty(htmlText))
                return htmlText;

            // Load the configuration files if not done already
            if(languageSyntax == null)
                this.LoadDefaultConfigFiles();

#if DEBUG
			timer.Start(htmlText.Length);
#endif
			// Colorize the text
			string result = reColorize.Replace(htmlText, replaceByCodeDelegate);

#if DEBUG
			timer.Stop();
#endif
			return result;
		}
        #endregion

		#region Private class methods
        //=====================================================================
        // Private class methods

        /// <summary>
        /// Loads the configuration from the App.config/Web.config and create a
        /// CodeColorizer.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if the
        /// configuration file could not be loaded, if the
        /// <b>ColorizerLibrary/syntax</b> node is not found, or if the
        /// <b>ColorizerLibrary/style</b> node is not found.</exception>
        private void LoadDefaultConfigFiles()
        {
            Hashtable config = (Hashtable)ConfigurationManager.GetSection(
                "ColorizerLibrary");

            if(config == null)
                throw new InvalidOperationException("Could not load " +
                    "configuration file section");

            // These are required
            if(config["syntax"] == null)
                throw new InvalidOperationException("Could not find " +
                    "<syntax> parameter in configuration file");

            if(config["style"] == null)
                throw new InvalidOperationException("Could not find " +
                    "<style> parameter in configuration file");

            this.LanguageSyntaxFileName = (string)config["syntax"];
            this.LanguageStyleFileName = (string)config["style"];
            this.Init();

            // These are optional
            if(config["numberLines"] != null)
                this.NumberLines = Convert.ToBoolean(config["numberLines"],
                    CultureInfo.InvariantCulture);

            if(config["outliningEnabled"] != null)
                this.OutliningEnabled = Convert.ToBoolean(
                    config["outliningEnabled"], CultureInfo.InvariantCulture);

            if(config["tabSize"] != null)
                this.TabSize = Convert.ToInt32(
                    config["tabSize"], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This is used to strip a common amount of leading whitespace on all
        /// lines of code in the block to colorize and to convert tabs to a
        /// consistent number of spaces.
        /// </summary>
        /// <param name="text">The text containing the lines to clean up.</param>
        /// <param name="tabSize">The number of spaces to which tab characters
        /// are converted.</param>
        private string StripLeadingWhitespace(string text, int tabSize)
        {
            Match m;
            string[] lines;
            string currentLine, tabsToSpaces = new String(' ', tabSize);
            int minSpaces, spaceCount, line, nestingLevel = 0;

            if(String.IsNullOrEmpty(text))
                return text;

            // Replace "\r\n" with "\n" first for consistency
            lines = text.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // If only one line, just trim it and convert tabs to spaces
            if(lines.Length == 1)
                return lines[0].Trim().Replace("\t", tabsToSpaces);

            minSpaces = Int32.MaxValue;
            regions.Clear();

            // The first pass is used to determine the minimum number of
            // spaces and to truncate blank lines.
            for(line = 0; line < lines.Length; line++)
            {
                currentLine = lines[line].Replace("\t", tabsToSpaces);

                if(currentLine.Length != 0)
                {
                    // Add a region boundary?
                    if(outlining)
                    {
                        m = reCollapseMarkers.Match(currentLine);

                        if(m.Success)
                        {
                            // If it's an #else, insert a pseudo-#endif
                            if(m.Groups[1].Value.StartsWith("#else",
                              StringComparison.OrdinalIgnoreCase))
                            {
                                regions.Add(new CollapsibleRegion(line,
                                    nestingLevel));
                                nestingLevel--;
                            }

                            regions.Add(new CollapsibleRegion(line, m,
                                nestingLevel));

                            nestingLevel = regions[regions.Count - 1].NestingLevel;
                        }
                    }

                    for(spaceCount = 0; spaceCount < currentLine.Length;
                      spaceCount++)
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

            // The second pass joins them back together less the determined
            // amount of leading whitespace.
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
        /// This is used to number lines and/or add collapsible sections for
        /// #region and #if/#else/#endif blocks based on the current settings.
        /// </summary>
        /// <param name="text">The text containing the lines to modify.</param>
        private string NumberAndOutline(string text)
        {
            string[] lines;
            string numPad, spacer, lineInfo, regionType, linePrefix;
            int regionIdx = 0, nestingLevel = 0, mod = 10, line;

            if(String.IsNullOrEmpty(text))
                return text;

            // Replace "\r\n" with "\n" first for consistency
            lines = text.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // Figure out the starting padding for the line numbers
            if(!numberLines || lines.Length < 10)
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

            if(outlining && regions.Count != 0)
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
                if(regionIdx < regions.Count &&
                  regions[regionIdx].LineNumber == line)
                {
                    regionType = regions[regionIdx].RegionType;

                    switch(regionType)
                    {
                        case "#region":
                        case "#if":
                        case "#else":
                            nestingLevel++;

                            if(!numberLines)
                                lineInfo = String.Empty;
                            else
                                lineInfo = String.Format(
                                    CultureInfo.InvariantCulture,
                                    "<span class=\"highlight-lineno\">" +
                                    "{0}{1}</span><span " +
                                    "class=\"highlight-lnborder\"></span>",
                                    numPad, line + 1);

                            linePrefix = String.Format(
                                CultureInfo.InvariantCulture,
                                "<span id=\"hrCol{0}\" " +
                                "style=\"display: none;\">{1}<span " +
                                "class=\"highlight-collapsebox\" " +
                                "onclick=\"javascript: " +
                                "HighlightExpandCollapse('hrExp{0}', " +
                                "'hrCol{0}');\">+</span><span " +
                                "class=\"highlight-collapsed\">{2}</span>" +
                                "</span><span id=\"hrExp{0}\" " +
                                "style=\"display: inline;\">{1}<span " +
                                "class=\"highlight-collapsebox\" " +
                                "onclick=\"javascript: " +
                                "HighlightExpandCollapse('hrCol{0}', " +
                                "'hrExp{0}');\">-</span>", line, lineInfo,
                                regions[regionIdx].Description);

                            regionIdx++;

                            // Append collapse boxes for other regions on the
                            // same line.
                            while(regionIdx < regions.Count &&
                              regions[regionIdx].LineNumber == line &&
                              regions[regionIdx].IsStart)
                            {
                                linePrefix += String.Format(
                                    CultureInfo.InvariantCulture,
                                    "<span id=\"hrCol{0}_{1}\" " +
                                    "style=\"display: none;\">" +
                                    "<span class=\"highlight-collapsebox\" " +
                                    "onclick=\"javascript: " +
                                    "HighlightExpandCollapse('hrExp{0}_{1}', " +
                                    "'hrCol{0}_{1}');\">+</span><span " +
                                    "class=\"highlight-collapsed\">{2}</span>" +
                                    "</span><span id=\"hrExp{0}_{1}\" " +
                                    "style=\"display: inline;\">" +
                                    "<span class=\"highlight-collapsebox\" " +
                                    "onclick=\"javascript: " +
                                    "HighlightExpandCollapse('hrCol{0}_{1}', " +
                                    "'hrExp{0}_{1}');\">-</span>", line, regionIdx,
                                    regions[regionIdx].Description);

                                regionIdx++;
                            }

                            lines[line] = linePrefix + lines[line];

                            // Close out regions ending on this line
                            while(regionIdx < regions.Count &&
                              regions[regionIdx].LineNumber == line &&
                              !regions[regionIdx].IsStart)
                            {
                                regionIdx++;
                                nestingLevel--;
                                lines[line] += "</span>";
                            }

                            spacer = "<span class=\"highlight-expanded\">" +
                                "&nbsp;</span>";
                            break;

                        case "#endif":
                        case "#endregion":
                            // Mismatched?
                            if(nestingLevel == 0)
                            {
                                regionIdx++;

                                if(numberLines)
                                    lines[line] = String.Format(
                                        CultureInfo.InvariantCulture,
                                        "<span class=\"highlight-lineno\">" +
                                        "{0}{1}</span><span " +
                                        "class=\"highlight-lnborder\"></span>" +
                                        "{2}{3}", numPad, line + 1, spacer,
                                        lines[line]);
                                else
                                    lines[line] = spacer + lines[line];
                                break;
                            }

                            nestingLevel--;

                            if(!numberLines)
                                lineInfo = "<span class=\"highlight-" +
                                    "endblock\">&nbsp;</span>";
                            else
                                lineInfo = String.Format(
                                    CultureInfo.InvariantCulture,
                                    "<span class=\"highlight-lineno\">" +
                                    "{0}{1}</span><span " +
                                    "class=\"highlight-lnborder\"></span>" +
                                    "<span class=\"highlight-endblock\">" +
                                    "&nbsp;</span>", numPad, line + 1);

                            lines[line] = lineInfo + lines[line] + "</span>";
                            regionIdx++;

                            // Close out regions ending on the same line
                            while(regionIdx < regions.Count &&
                               regions[regionIdx].LineNumber == line)
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
                    if(numberLines)     // Format the line number part
                        lines[line] = String.Format(
                            CultureInfo.InvariantCulture,
                            "<span class=\"highlight-lineno\">{0}{1}</span>" +
                            "<span class=\"highlight-lnborder\"></span>" +
                            "{2}{3}", numPad, line + 1, spacer, lines[line]);
                    else
                        lines[line] = spacer + lines[line];
            }

            // Close out mismatched regions
            while(nestingLevel-- > 0)
                lines[line - 1] += "</span>";

            return String.Join("\r\n", lines);
        }

		/// <summary>
        /// Build the keyword family regular expressions.
        /// </summary>
		/// <remarks>This method creates regular expression that match a whole
        /// keyword family and adds it as a parameter "regexp" to the
        /// keywordlist node.</remarks>
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
			foreach(XmlNode keywordListNode in keywordLists.SelectNodes(
              "keywordlist"))
			{
                // Skip any that don't have keywords
                keywords = keywordListNode.SelectNodes("kw");

                if(keywords.Count == 0)
                    continue;

                // See if word boundaries should be used
                wordBound = keywordListNode.Attributes["use-word-boundary"];

                if(wordBound == null || Convert.ToBoolean(wordBound.Value,
                  CultureInfo.InvariantCulture))
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
				XmlHelper.XmlSetAttribute(keywordListNode, "regexp",
                    String.Join(null, expression, 0, 7));
			}
		}

		/// <summary>
        /// This builds regular expressions out of the context node
        /// </summary>
		/// <param name="languageNode">The language node.</param>
		/// <param name="contextNode">The context node.</param>
		/// <remarks>This method creates regular expressions that match all the
        /// context rules and adds it as a parameter "regexp" to the context
        /// node.</remarks>
        /// <returns>The regular expression string for the context node.</returns>
		/// <exception cref="InvalidOperationException">This is thrown if the
        /// regular expression rule is missing the expression attribute, if the
		/// keyword list could not be found, or if the keyword list family or
        /// regular expression could not be found.</exception>
		private string BuildRuleRegExp(XmlNode languageNode,
          XmlNode contextNode)
		{
            StringBuilder sb = new StringBuilder("(", 1024);
            XmlNode familyNode, kwList, node;

			foreach(XmlNode ruleNode in contextNode.ChildNodes)
			{
				if (ruleNode.Name == "#comment")
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
				else if(ruleNode.Name == "regexp" )
				{
					node = ruleNode.Attributes["expression"];

					if(node == null)
						throw new InvalidOperationException(
                            "Regular expression rule missing expression " +
                            "attribute");

				    // Append the regular expression
					sb.Append(node.Value);

					// Add the regular expression to the dictionnary
					rxDic.AddKey(languageNode, contextNode, ruleNode,
                        node.Value);
				}
				else if(ruleNode.Name == "keyword")
				{
					// Find the keyword list
					familyNode = ruleNode.Attributes["family"];

					if(familyNode == null)
						throw new InvalidOperationException(
                            "Keyword rule missing family");

					kwList = keywordLists.SelectSingleNode("keywordlist[" +
                        "@id=\"" + familyNode.Value + "\"]");

					if(kwList == null)
						throw new InvalidOperationException("Could not find " +
                            "keywordlist (family: " + familyNode.Value + ")");

					node = kwList.Attributes["regexp"];

					if(node == null)
						throw new InvalidOperationException("Could not find " +
                            "keywordlist regular expression");

					// Append the regular expression
					sb.Append(node.Value);

					// Add it to the dictionnary too
					rxDic.AddKey(languageNode, contextNode, kwList,
                        "(" + node.Value + ")");
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
        /// This precompiles regular expressions and search strings and
        /// prepares rules attribute.
        /// </summary>
		/// <param name="languageNode">The context node</param>
		private void BuildRules(XmlNode languageNode)
		{
			string regExp;

            // Create regular expressions for each context
            foreach(XmlNode contextNode in languageNode.SelectNodes(
              "contexts/context"))
			{
				regExp = this.BuildRuleRegExp(languageNode, contextNode);

				// Add or update the attribute
				XmlHelper.XmlSetAttribute(contextNode, "regexp", regExp);	

				// Create regular expression and add to dictionary
				rxDic.AddKey(languageNode, contextNode, regExp);
			}
		}

        /// <summary>
        /// This prepares the syntax XML file for use.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if the
        /// highlight node could not be found in the configuration file.
        /// </exception>
		private void BuildSyntax()
		{
			XmlNode needBuildNode, highlightNode;

			languageSyntax.Load(this.LanguageSyntaxFileName);

			// Check to see if a build is needed
			highlightNode = languageSyntax.SelectSingleNode("highlight");

			if(highlightNode == null)
				throw new InvalidOperationException(
                    "Could not find highlight node");

            languages = highlightNode.SelectSingleNode("languages");

            if(highlightNode == null)
                throw new InvalidOperationException(
                    "Could not find languages node");

            keywordLists = highlightNode.SelectSingleNode("keywordlists");

            if(keywordLists == null)
                throw new InvalidOperationException(
                    "Could not find keywordlists node");

            needBuildNode = highlightNode.Attributes["needs-build"];

			if(needBuildNode == null || needBuildNode.Value == "yes")
			{
                // First, build keyword regexp
                this.BuildKeywordRegExp();

				// Iterate languages and pre-build the regular expressions
                foreach(XmlNode languageNode in languages.SelectNodes(
                  "language"))
                {
                    friendlyNames.Add(languageNode.Attributes["id"].Value,
                        languageNode.Attributes["name"].Value);
                    this.BuildRules(languageNode);
                }

                // Add the mapped language names to the friendly list too
                foreach(XmlNode mapEntry in highlightNode.SelectNodes(
                  "map/language"))
                {
                    friendlyNames.Add(mapEntry.Attributes["from"].Value,
                        friendlyNames[mapEntry.Attributes["to"].Value]);
                    alternateIds.Add(mapEntry.Attributes["from"].Value,
                        mapEntry.Attributes["to"].Value);
                }

				// Update node to reflect that it has been built
				XmlHelper.XmlSetAttribute(highlightNode, "needs-build", "no");
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
		/// <param name="matchText">The text that matched the context regular
        /// expression</param>
		/// <remarks>If the Regex finds a rule occurrence, this method is used
        /// to find which rule has been triggered.</remarks>
        /// <returns>The node that triggered the match or null if no node is
        /// found.</returns>
		private XmlNode FindRule(XmlNode languageNode, XmlNode contextNode,
          string matchText)
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
					if(matchText == ruleNode.Attributes["char"].Value +
                      ruleNode.Attributes["char1"].Value)
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
        /// This is used to apply the context rules successively to the code
        /// string that needs colorizing.
        /// </summary>
		/// <param name="languageNode">The language node.</param>
		/// <param name="contextNode">The context node.</param>
		/// <param name="code">The code block to parse and convert.</param>
		/// <param name="parsedCodeNode">Parent node that will contain the
        /// parsed code.</param>
		/// <remarks>This method uses the pre-computed regular expressions of
        /// the context rules, rule matching, etc.  The results are added to
        /// the XML document in the parsedcodeNode.</remarks>
        /// <exception cref="InvalidOperationException">This is thrown if a
        /// rule match cannot be found or if a matching context node cannot
        /// be found.</exception>
		private void ApplyRules(XmlNode languageNode, XmlNode contextNode,
          string code, XmlNode parsedCodeNode)
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
					XmlHelper.XmlAddChildCDATAElem(parsedCodeNode,
						contextNode.Attributes["attribute"].Value, code);

					// finished parsing
					break;
				}

				// Add the skipped text if not empty
				if(m.Index != 0)
                {
                    attributeNode = contextNode.Attributes["attribute"];

                    if(attributeNode != null && attributeNode.Value != "hidden")
					    XmlHelper.XmlAddChildCDATAElem(parsedCodeNode,
						    attributeNode.Value, code.Substring(0, m.Index));
                }

				// Find the rule that caused the match
				XmlNode ruleNode = this.FindRule(languageNode, contextNode,
                    m.Value);

				if(ruleNode == null)
					throw new InvalidOperationException("Didn't find " +
                        "matching rule, regular expression false? (context: " +
                        contextNode.Attributes["id"].Value + ")");

				// Check to see if the match needs to be added to the result
				attributeNode = ruleNode.Attributes["attribute"];

				if(attributeNode != null && attributeNode.Value != "hidden")
					XmlHelper.XmlAddChildCDATAElem(parsedCodeNode,
						attributeNode.Value, m.Value);

				// Update the context if necessary
				if(contextNode.Attributes["id"].Value != ruleNode.Attributes[
                  "context"].Value)
				{
					// Get the new context
                    contextNode = languageNode.SelectSingleNode(
                        "contexts/context[@id=\"" + ruleNode.Attributes[
                        "context"].Value + "\"]");

					if(contextNode == null)
						throw new InvalidOperationException(
                            "Didn't find matching context, error in XML " +
                            "specification?");

					// Get the new regular expression
					regExp = rxDic.GetKey(languageNode, contextNode);
				}

                // Removed the parsed code and carry on
				code = code.Substring(m.Index + m.Length);			
			}
		}

		/// <summary>
        /// Create and populate an xml document with the corresponding parsed
        /// language tree for highlighting.
        /// </summary>
		/// <param name="languageNode">The language node to use for parsing the
        /// code.</param> 
		/// <param name="rootTag">Root tag (under parsed code) for the
        /// generated xml tree.</param> 
		/// <param name="code">The code to parse</param>
		/// <returns>Returns an <seealso cref="XmlDocument"/> document
        /// containing the parsed nodes.</returns>
		/// <remarks>This method builds an XML tree containing context node.
        /// Use an XSL file to render it.</remarks>
        /// <exception cref="ArgumentException">This is thrown if the
        /// language node does not contain any contexts or if there is no
        /// default context node.</exception>
        /// <exception cref="InvalidOperationException">This is thrown if the
        /// main node of the parsed code XML document cannot be created.
        /// </exception>
		private XmlDocument BuildHighlightTree(XmlNode languageNode,
          string rootTag, string code)
		{
            XmlDocument xmlResult;
            XmlNode contextNode, resultMainNode;
            string defaultContext;

            // Get the contexts
            contextNode = languageNode.SelectSingleNode("contexts");

            if(contextNode == null)
                throw new ArgumentException("Could not find contexts node " +
                    "for " + rootTag + "language");

            // Getting the default context
            defaultContext = contextNode.Attributes["default"].Value;

            contextNode = contextNode.SelectSingleNode("context[@id=\"" +
                defaultContext + "\"]");

            if(contextNode == null)
            	throw new ArgumentException("Could not find the default " +
                    "context for " + rootTag + " language (default = " +
                    defaultContext + ")");

            // Create the XML result and add the main node
            xmlResult = new XmlDocument();

            resultMainNode = xmlResult.CreateElement("parsedcode");
            if(resultMainNode == null)
            	throw new InvalidOperationException(
                    "Could not create main node parsedcode");

            xmlResult.AppendChild(resultMainNode);

            // Add the language and in-box attributes to it
            XmlHelper.XmlSetAttribute(resultMainNode,"lang", rootTag );
            XmlHelper.XmlSetAttribute(resultMainNode, "in-box",
                (inBox) ? "1" : "0");

            // Parse the code and populate the XML document
            this.ApplyRules(languageNode, contextNode, code, resultMainNode);

            return xmlResult;
		}

        /// <summary>
        /// This method is used as the match evaluator for the main colorizer
        /// regular expression</summary>
		/// <param name="match">Full match</param>
		private String ReplaceByCode(Match match)
		{
			string tag = match.Groups[1].Value,
                   language = match.Groups[5].Value,
			       matchText = HttpUtility.HtmlDecode(match.Groups[8].Value);

			XmlNode languageNode;
            MatchCollection otherOpts;

            int tabSize = defaultTabSize;
            bool preserveNumbering = numberLines, preserveOutlining = outlining,
                 tabSizeOverridden = false, preserveKeepSeeTags = keepSeeTags;
            string altLanguage, title = null;

            try
            {
                // See if the other options have been specified
                otherOpts = reOptOverrides.Matches(match.Groups[2].Value +
                    " " + match.Groups[7].Value);

                foreach(Match m in otherOpts)
                {
                    if(m.Groups[1].Value.Length != 0)
                        numberLines = Convert.ToBoolean(m.Groups[4].Value,
                            CultureInfo.InvariantCulture);

                    if(m.Groups[6].Value.Length != 0)
                        outlining = Convert.ToBoolean(m.Groups[9].Value,
                            CultureInfo.InvariantCulture);

                    if(m.Groups[11].Value.Length != 0)
                    {
                        tabSize = Convert.ToInt32(m.Groups[14].Value,
                            CultureInfo.InvariantCulture);

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
                        keepSeeTags = Convert.ToBoolean(m.Groups[24].Value,
                            CultureInfo.InvariantCulture);
                }

                // Find the language in the language file by ID or by name
    			languageNode = languages.SelectSingleNode("language[@id=\"" +
                    language + "\" or @name=\"" + language + "\"]");

                // If not found, try matching a variation
                if(languageNode == null)
                {
                    // Try lower case first
                    language = language.ToLower(CultureInfo.InvariantCulture);

    			    languageNode = languages.SelectSingleNode(
                        "language[@id=\"" + language + "\" or @name=\"" +
                        language + "\"]");

                    // Try to map it to an ID if not found
                    if(languageNode == null && alternateIds.TryGetValue(
                      language, out altLanguage))
                    {
                        language = altLanguage;
        			    languageNode = languages.SelectSingleNode(
                            "language[@id=\"" + language + "\"]");
                    }
                }

                // If not found, we'll just format it neatly and optionally
                // number it and add collapsible regions.
			    if(languageNode == null)
                {
                    inBox = true;

                    // Tidy up the block by stripping any common leading
                    // whitespace and converting tabs to spaces.
                    matchText = this.StripLeadingWhitespace(
                        match.Groups[8].Value, tabSize);
                }
                else
                {
                    // Replace tabs with the specified number of spaces to
                    // ensure consistency of layout.  Default to eight if not
                    // specified.
                    if(languageNode.Attributes["tabSize"] != null && !tabSizeOverridden)
                        tabSize = Convert.ToInt32(languageNode.Attributes[
                            "tabSize"].Value, CultureInfo.InvariantCulture);

                    // Tidy up the block by stripping any common leading
                    // whitespace and converting tabs to spaces.
                    matchText = this.StripLeadingWhitespace(matchText, tabSize);

                    // This is used to determine whether to render the
                    // colorized code in a <span> or a <pre> tag.
                    inBox = (String.Compare(tag, "pre",
                      StringComparison.OrdinalIgnoreCase) == 0);

                    // If keeping see tags, replace them with a marker
                    // character so that they aren't colorized.
                    if(keepSeeTags)
                    {
                        seeTags.Clear();
                        matchText = reExtractSeeTags.Replace(matchText,
                            onSeeTagFound);
                    }

                    // Apply syntax matching to text with the corresponding
                    // language.
                    XmlDocument xmlResult = this.BuildHighlightTree(
                        languageNode, language, matchText);

                    // Transform the XML to HTML and return the results
                    using(StringWriter sw = new StringWriter(CultureInfo.CurrentCulture))
                    {
                        languageStyle.Transform(xmlResult, null, sw);

                        matchText = sw.ToString();
                    }

                    // Replace the markers with the see tags if they were kept
                    if(keepSeeTags)
                    {
                        seeTagIndex = 0;
                        matchText = reReplaceMarker.Replace(matchText,
                            onMarkerFound);
                    }
                }

                // Add line numbers and/or outlining if needed
                if(inBox)
                {
                    if(numberLines || outlining)
                        matchText = this.NumberAndOutline(matchText);

                    // Add the <pre> tag to it
                    matchText = "<pre class=\"highlight-pre\">" + matchText +
                        "</pre>";

                    // Use a default title if necessary
                    if(String.IsNullOrEmpty(title))
                    {
                        if(useDefaultTitle)
                            if(!friendlyNames.TryGetValue(language, out title) &&
                              languageNode != null)
                                title = languageNode.Attributes["name"].Value;

                        if(String.IsNullOrEmpty(title))
                            title = "&#xa0;";
                    }

                    // Add the title div with the title text and Copy span
                    matchText = String.Format(CultureInfo.InvariantCulture,
                        "<div class=\"highlight-title\">" +
                        "<span class=\"highlight-copycode\" " +
                        "onkeypress=\"javascript:CopyColorizedCodeCheckKey(" +
                        "this.parentNode, event);\" tabindex=\"0\" " +
                        "onmouseover=\"CopyCodeChangeIcon(this)\" " +
                        "onmouseout=\"CopyCodeChangeIcon(this)\" " +
                        "onclick=\"javascript:CopyColorizedCode(" +
                        "this.parentNode);\"><img src=\"{0}\" " +
                        "style=\"margin-right: 5px;\" />{1}</span>{2}</div>{3}",
                        copyImageUrl, copyText, title, matchText);
                }
            }
            finally
            {
                // Restore the defaults
                numberLines = preserveNumbering;
                outlining = preserveOutlining;
                keepSeeTags = preserveKeepSeeTags;
            }

            return matchText;
		}

        /// <summary>
        /// Replace a see tag with a marker and store it in the list
        /// </summary>
        private string OnSeeTagFound(Match match)
        {
            seeTags.Add(match.Value);
            return "\xFF";
        }

        /// <summary>
        /// Replace a see tag marker with the next entry from the saved list
        /// of tags.
        /// </summary>
        private string OnMarkerFound(Match match)
        {
            return seeTags[seeTagIndex++];
        }
		#endregion
	}
}
