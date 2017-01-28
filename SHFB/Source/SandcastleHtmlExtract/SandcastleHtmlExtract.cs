//===============================================================================================================
// System  : Sandcastle Help File Builder - HTML Extract
// File    : SandcastleHtmlExtract.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2017
// Note    : Copyright 2008-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the console mode application used to extract title and keyword information from HTML files
// for use in creating the CHM table of contents and keyword index files.  It can also optionally convert the
// files to a different encoding in order to build HTML Help 1 (.chm) files that use a different language.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/02/2008  EFW  Created the code
// 04/01/2008  EFW  Merged changes from Ferdinand Prantl to add a website keyword index
// 06/14/2008  EFW  Fixed bug in handling of TOC nodes without a file
// 07/14/2008  EFW  Added support for running as an MSBuild task
// 06/12/2010  EFW  Added support for multi-format build output
// 07/26/2012  EFW  Added code to remove Help 2 constructs
// 03/02/2013  EFW  Updated how the keyword index files were created so that each entry has a unique title when
//                  grouped under a common keyword.  Updated to process the files in parallel to improve the
//                  performance.
// 07/31/2014  EFW  Applied fix from Kalyan00 to correctly save files in the localized and original locations
//                  with the proper encodings.
// 09/05/2014  EFW  Added support for setting the maximum degree of parallelism used
//===============================================================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Build.Framework;

namespace SandcastleBuilder.HtmlExtract
{
    /// <summary>
    /// This is the console mode application used to extract title and keyword information from HTML files for
    /// use in creating the CHM table of contents and keyword index files.
    /// </summary>
    public class SandcastleHtmlExtract : Microsoft.Build.Utilities.Task
    {
        #region MSBuild task interface
        //=====================================================================

        /// <summary>
        /// This is used to set the project name
        /// </summary>
        [Required]
        public string ProjectName
        {
            get { return projectName; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    projectName = value;
            }
        }

        /// <summary>
        /// This is used to set the language ID (LCID)
        /// </summary>
        /// <value>This is optional.  If not set, it defaults to 1033.</value>
        public int LanguageId
        {
            get { return langId; }
            set { langId = value; }
        }

        /// <summary>
        /// This is used to set the HTML Help 1 file folder name containing the Help 1 files to be processed.
        /// </summary>
        /// <value>This is optional.  If not set, no HTML help 1 files will be processed.</value>
        public string Help1Folder
        {
            get { return help1Folder; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    help1Folder = value;
            }
        }

        /// <summary>
        /// This is used to set the website file folder name containing the website files to be processed.
        /// </summary>
        /// <value>This is optional.  If not set, no HTML help 1 files will be processed.</value>
        public string WebsiteFolder
        {
            get { return websiteFolder; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    websiteFolder = value;
            }
        }

        /// <summary>
        /// This is used to set the localized output folder name
        /// </summary>
        /// <value>This is optional.  If not set, the HTML files will not be localized.</value>
        public string LocalizedFolder
        {
            get { return localizedFolder; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    localizedFolder = value;
            }
        }

        /// <summary>
        /// This is used to set the general output folder name
        /// </summary>
        /// <value>This is optional.  If not set, it defaults to the current working folder.</value>
        public string OutputFolder
        {
            get { return outputFolder; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    outputFolder = value;
            }
        }

        /// <summary>
        /// This is used to set the table of contents XML filename
        /// </summary>
        /// <value>This is optional.  If not set, it defaults to <b>toc.xml</b>.</value>
        public string TocFile
        {
            get { return tocFile; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    tocFile = value;
            }
        }

        /// <summary>
        /// This is used to get or set the maximum degree of parallelism used to process the HTML files
        /// </summary>
        /// <value>If not set, it defaults to a maximum of 20 threads per processor.  Increase or decrease this
        /// value as needed based on your system.  Setting it to a value less than 1 will allow for an unlimited
        /// number of threads.  However, this is a largely IO-bound process so allowing an excessive number of
        /// threads may slow overall system performance on very large help files.</value>
        public int MaxDegreeOfParallelism
        {
            get { return maxDegreeOfParallelism; }
            set
            {
                if(value < 1)
                    value = -1;

                maxDegreeOfParallelism = value;
            }
        }

        /// <summary>
        /// This is used to execute the task and process the HTML files
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            isMSBuildTask = true;
            return (Main(new string[0]) == 0);
        }
        #endregion

        #region Title information structure
        //=====================================================================

        /// <summary>
        /// This is used to hold title information
        /// </summary>
        private struct TitleInfo
        {
            /// <summary>The topic title</summary>
            public string TopicTitle { get; set; }
            /// <summary>The TOC title</summary>
            public string TocTitle { get; set; }
            /// <summary>The file in which it occurs</summary>
            public string File { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="topicTitle">The topic title</param>
            /// <param name="tocTitle">The TOC title</param>
            /// <param name="filename">The filename</param>
            public TitleInfo(string topicTitle, string tocTitle, string filename) : this()
            {
                this.TopicTitle = topicTitle;
                this.TocTitle = String.IsNullOrWhiteSpace(tocTitle) ? topicTitle : tocTitle;
                this.File = filename;
            }
        }
        #endregion

        #region Keyword information structure
        //=====================================================================

        /// <summary>
        /// This is used to hold keyword information
        /// </summary>
        private struct KeywordInfo
        {
            /// <summary>The main entry</summary>
            public string MainEntry { get; set; }
            /// <summary>An optional sub-entry</summary>
            public string SubEntry { get; set; }
            /// <summary>The file in which it occurs</summary>
            public string File { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        // Options
        private static string outputFolder, help1Folder, websiteFolder, projectName, tocFile,
            localizedFolder, encodingName;
        private static int langId, codePage, maxDegreeOfParallelism = Environment.ProcessorCount * 20;
        private static bool isMSBuildTask;

        // Extracted keyword and title information
        private static List<KeywordInfo> keywords;
        private static ConcurrentBag<KeywordInfo> keywordBag;
        private static ConcurrentDictionary<string, TitleInfo> titles;

        // Regular expressions used for title and keyword extraction and element removal
        private static Regex reTitle = new Regex(@"<title>(.*)</title>", RegexOptions.IgnoreCase);
        private static Regex reTocTitle = new Regex("<mshelp:toctitle\\s+title=\"([^\"]+)\"[^>]+>",
            RegexOptions.IgnoreCase);
        private static Regex reKKeyword = new Regex("<mshelp:keyword\\s+index=\"k\"\\s+term=\"([^\"]+)\"[^>]+>",
            RegexOptions.IgnoreCase);
        private static Regex reSubEntry = new Regex(@",([^\)\>]+|([^\<\>]*" +
            @"\<[^\<\>]*\>[^\<\>]*)?|([^\(\)]*\([^\(\)]*\)[^\(\)]*)?)$");
        private static Regex reXmlIsland = new Regex("<xml>.*?</xml>", RegexOptions.Singleline);
        private static Regex reHxLinkCss = new Regex("<link[^>]*?href=\"ms-help://Hx/HxRuntime/HxLink\\.css\".*?/?>",
            RegexOptions.IgnoreCase);
        private static Regex reHtmlElement = new Regex("\\<html.*?\\>");
        private static Regex reUnusedNamespaces = new Regex("\\s*?xmlns:(MSHelp|mshelp|ddue|xlink|msxsl|xhtml)=\".*?\"");

        // Localization support members
        private static Dictionary<Regex, string> patterns;
        private static Encoding destEncoding;
        #endregion

        #region Main program entry point
        //=====================================================================

        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>Zero on success or a non-zero value on failure</returns>
        public static int Main(string[] args)
        {
            XPathDocument config;
            XPathNavigator nav;
            CultureInfo ci;
            Encoding enc;
            OptionInfo lastOption = null;
            List<OptionInfo> options = new List<OptionInfo>();
            int returnCode = 0;

            if(String.IsNullOrEmpty(outputFolder))
                outputFolder = ".";

            if(String.IsNullOrEmpty(tocFile))
                tocFile = "toc.xml";

            if(langId == 0)
                langId = 1033;

            keywords = new List<KeywordInfo>();
            keywordBag = new ConcurrentBag<KeywordInfo>();
            titles = new ConcurrentDictionary<string, TitleInfo>();

            Assembly asm = Assembly.GetExecutingAssembly();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Console.WriteLine("{0}, version {1}\r\n{2}\r\nE-Mail: Eric@EWoodruff.us\r\n", fvi.ProductName,
                fvi.ProductVersion, fvi.LegalCopyright);

            try
            {
                // Get a list of options
                foreach(string option in args)
                    options.Add(new OptionInfo(option));

                if(options.Count == 0 && !isMSBuildTask)
                {
                    ShowHelp();
                    return 1;
                }

                // Get the settings for this run
                foreach(OptionInfo option in options)
                {
                    lastOption = option;

                    switch(option.Name)
                    {
                        case "help":
                        case "?":
                            ShowHelp();
                            break;

                        case "help1folder":
                            help1Folder = option.Value;

                            if(!Directory.Exists(help1Folder))
                                throw new ArgumentException("The HTML Help 1 folder could not be found");
                            break;

                        case "websitefolder":
                            websiteFolder = option.Value;

                            if(!Directory.Exists(websiteFolder))
                                throw new ArgumentException("The website folder could not be found");
                            break;

                        case "outputfolder":
                            outputFolder = option.Value;
                            break;

                        case "localizedfolder":
                            localizedFolder = option.Value;
                            break;

                        case "lcid":
                            if(!Int32.TryParse(option.Value, out langId))
                                throw new ArgumentException("Invalid LCID value");
                            break;

                        case "projectname":
                            projectName = option.Value;
                            break;

                        case "tocfile":
                            tocFile = option.Value;
                            break;

                        default:
                            throw new ArgumentException("Unknown command line option");
                    }
                }

                lastOption = null;

                // -help1Folder or -websiteFolder must be specified
                if(String.IsNullOrEmpty(help1Folder) && String.IsNullOrEmpty(websiteFolder))
                    throw new InvalidOperationException(isMSBuildTask ?
                        "The Help1Folder and/or WebsiteFolder property must be set to true" :
                        "-help1Folder and/or -websiteFolder must be specified");

                // The project filename must be specified
                if(String.IsNullOrEmpty(projectName))
                    throw new InvalidOperationException(isMSBuildTask ?
                        "A project name must be specified using the ProjectName property" :
                        "A project name must be specified using the /projectName command line option");

                // The TOC file must exist
                if(!File.Exists(tocFile))
                    throw new ArgumentException("TOC file not found");

                outputFolder = Path.GetFullPath(outputFolder);

                if(outputFolder.Length != 0 && outputFolder[outputFolder.Length - 1] == '\\')
                    outputFolder = outputFolder.Substring(0, outputFolder.Length - 1);

                if(!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                // Get the code page to use based on the locale ID
                config = new XPathDocument(Path.GetDirectoryName(asm.Location) + @"\SandcastleHtmlExtract.config");

                nav = config.CreateNavigator().SelectSingleNode(String.Format(CultureInfo.InvariantCulture,
                    "/configuration/languages/language[@id='{0}']", langId));

                // If not found, default to the one for the ANSI code page
                // based on the specified locale ID.
                if(nav == null)
                {
                    ci = new CultureInfo(langId);
                    codePage = ci.TextInfo.ANSICodePage;
                    enc = Encoding.GetEncoding(codePage);
                    encodingName = enc.WebName;

                    Console.WriteLine("SHFB: Warning SHE0001: LCID '{0}' not found in configuration file.  " +
                        "Defaulting to ANSI code page value of '{1}', encoding charset '{2}'.", langId, codePage,
                        encodingName);
                }
                else
                {
                    codePage = Convert.ToInt32(nav.GetAttribute("codepage", String.Empty), CultureInfo.InvariantCulture);
                    encodingName = nav.GetAttribute("charset", String.Empty);

                    Console.WriteLine("Using LCID '{0}', code page '{1}', encoding charset '{2}'.", langId, codePage,
                        encodingName);
                }

                // Parse the HTML Help 1 files and save the keyword index and table of contents
                if(!String.IsNullOrEmpty(help1Folder))
                {
                    help1Folder = Path.GetFullPath(help1Folder);
                    Console.WriteLine("\nProcessing Help 1 files in " + help1Folder);

                    // If localizing, validate the folder and create the regex patterns that
                    // will do the conversions.
                    if(localizedFolder != null)
                    {
                        localizedFolder = Path.GetFullPath(localizedFolder);

                        if(localizedFolder.Length > 0 && localizedFolder[localizedFolder.Length - 1] == '\\')
                            localizedFolder = localizedFolder.Substring(0, localizedFolder.Length - 1);

                        if(!Directory.Exists(localizedFolder))
                            Directory.CreateDirectory(localizedFolder);

                        patterns = new Dictionary<Regex, string>();
                        destEncoding = Encoding.GetEncoding(encodingName);

                        // Convert unsupported high-order characters to 7-bit ASCII equivalents
                        patterns.Add(new Regex(@"\u2018|\u2019"), "'");
                        patterns.Add(new Regex(@"\u201C|\u201D"), "\"");
                        patterns.Add(new Regex(@"\u2026"), "...");

                        if(langId != 1041)
                            patterns.Add(new Regex(@"\u00A0"), "&nbsp;");
                        else
                            patterns.Add(new Regex(@"\u00A0"), " ");

                        if(encodingName != "Windows-1252")
                            patterns.Add(new Regex(@"\u2011|\u2013"), "-");
                        else
                            patterns.Add(new Regex(@"\u2011|\u2013|\u2014"), "-");

                        // Convert other unsupported high-order characters to named entities
                        patterns.Add(new Regex(@"\u00A9"), "&copy;");
                        patterns.Add(new Regex(@"\u00AE"), "&reg;");
                        patterns.Add(new Regex(@"\u2014"), "&mdash;");
                        patterns.Add(new Regex(@"\u2122"), "&trade;");

                        // Replace the charset declaration
                        patterns.Add(new Regex("CHARSET=UTF-8",
                            RegexOptions.IgnoreCase), "CHARSET=" + encodingName);

                        Console.WriteLine("Localized content will be written to '{0}'", localizedFolder);
                    }

                    ParseFiles(help1Folder, localizedFolder);
                    WriteHelp1xKeywordIndex();
                    WriteHelp1xTableOfContents();
                }

                // Parse the website files and save the keyword index and table of contents
                if(!String.IsNullOrEmpty(websiteFolder))
                {
                    websiteFolder = Path.GetFullPath(websiteFolder);
                    Console.WriteLine("\nProcessing website files in " + websiteFolder);

                    ParseFiles(websiteFolder, null);
                    WriteWebsiteKeywordIndex();
                    WriteWebsiteTableOfContents();
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                if(lastOption == null)
                    Console.WriteLine("SHFB: Error SHE0002: Unexpected error: {0}", ex);
                else
                    Console.WriteLine("SHFB: Error SHE0003: Unexpected error applying command line option '{0}': {1}",
                        lastOption.OptionText, ex);

                returnCode = 1;
            }
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Hit ENTER to exit...");
                Console.ReadLine();
            }
#endif
            return returnCode;
        }
        #endregion

        #region Show command line help
        //=====================================================================

        /// <summary>
        /// This is used to show the command line syntax
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine(@"Syntax:
SandcastleHtmlExtract -projectName=ProjectName [-OtherOptions] ...

Prefix options with '-' or '/'.  Names and values are case-insensitive.
Property values should be enclosed in double quotes if they contain spaces,
commas, or other special characters.

-help or -?         Show this help.

-projectName=name   Specify the root project name for the CHM table of contents
                    and keyword index files.  This option is required.

-help1Folder=folder   Generate HTML Help 1 files and/or website files.  At
-websiteFolder=folder least one of these is required.  Both can also be
                      specified.

-lcid=IDvalue       The locale ID to use when saving the table of contents
                    and keyword index files.  This is also used to determine
                    the code page to use when rewriting the HTML files if
                    needed.  If not specified, it defaults to 1033 (0x0409).

-localizedFolder=folder The folder in which to store localized copies of the
                        Help 1 files.  These can be used to build a correctly
                        localized CHM file.  If omitted, no localized files are
                        generated.

-outputFolder=folder The output folder for the table of contents and keyword
                     index files.  If not specified, it defaults to the current
                     folder.

-tocFile=filename   The intermediate table of contents XML file to use for
                    creating the CHM table of contents file.  If not specified,
                    it defaults to toc.xml.");
        }
        #endregion

        #region File parsing methods
        //=====================================================================

        /// <summary>
        /// Parse the given set of files to generate title and keyword info
        /// and localize the files if necessary.
        /// </summary>
        /// <param name="fileFolder">The folder containing the files to parse</param>
        /// <param name="localizedOutputFolder">The folder in which to store localized
        /// output or null for no localized output.</param>
        private static void ParseFiles(string fileFolder, string localizedOutputFolder)
        {
            KeywordInfo kw;
            string mainEntry = String.Empty;
            int htmlFiles = 0;

            keywords.Clear();
            keywordBag = new ConcurrentBag<KeywordInfo>();
            titles.Clear();

            if(fileFolder.Length != 0 && fileFolder[fileFolder.Length - 1] == '\\')
                fileFolder = fileFolder.Substring(0, fileFolder.Length - 1);

            // Process all *.htm and *.html files in the given folder and all of its subfolders.
            Parallel.ForEach(Directory.EnumerateFiles(fileFolder, "*.*", SearchOption.AllDirectories),
              new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, file =>
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();

                if(ext == ".htm" || ext == ".html")
                {
                    ProcessFile(fileFolder, file, localizedOutputFolder);
                    Interlocked.Add(ref htmlFiles, 1);
                }
                else
                    if(localizedOutputFolder != null)
                    {
                        // Copy supporting files only if localizing
                        string destFile = Path.Combine(localizedOutputFolder, file.Substring(fileFolder.Length + 1));
                        string folder = Path.GetDirectoryName(destFile);

                        if(!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        File.Copy(file, destFile, true);
                    }
            });

            Console.WriteLine("Processed {0} HTML files\r\nSorting keywords and generating See Also indices", htmlFiles);

            // Sort the keywords
            keywords.AddRange(keywordBag);
            keywords.Sort((x, y) =>
            {
                string subX, subY;

                if(x.MainEntry != y.MainEntry)
                    return String.Compare(x.MainEntry, y.MainEntry, StringComparison.OrdinalIgnoreCase);

                subX = x.SubEntry;
                subY = y.SubEntry;

                if(subX == null)
                    subX = String.Empty;

                if(subY == null)
                    subY = String.Empty;

                if(subX != subY)
                    return String.Compare(subX, subY, StringComparison.OrdinalIgnoreCase);

                subX = titles[Path.GetFileNameWithoutExtension(x.File)].TopicTitle;
                subY = titles[Path.GetFileNameWithoutExtension(y.File)].TopicTitle;

                return String.Compare(subX, subY, StringComparison.OrdinalIgnoreCase);
            });

            // Insert the See Also indices for each sub-entry
            for(int idx = 0; idx < keywords.Count; idx++)
                if(!String.IsNullOrEmpty(keywords[idx].SubEntry))
                {
                    if(idx > 0)
                        mainEntry = keywords[idx - 1].MainEntry;

                    if(mainEntry != keywords[idx].MainEntry)
                    {
                        kw = new KeywordInfo();
                        kw.MainEntry = keywords[idx].MainEntry;
                        keywords.Insert(idx, kw);
                    }
                }
        }

        /// <summary>
        /// Parse each file looking for the title and index keywords and remove the unnecessary Help 2 constructs
        /// that cause issues in Internet Explorer 10.
        /// </summary>
        /// <param name="basePath">The base folder path</param>
        /// <param name="sourceFile">The file to parse</param>
        /// <param name="localizedOutputFolder">The folder in which to store localized output or null for no
        /// localized output.</param>
        private static void ProcessFile(string basePath, string sourceFile, string localizedOutputFolder)
        {
            Encoding currentEncoding = Encoding.Default;
            MatchCollection matches;
            Match match;
            KeywordInfo keyword;
            string content, topicTitle, tocTitle, term, folder, key, destFile;
            byte[] currentBytes, convertedBytes;

            // Read the file in using the proper encoding
            using(StreamReader sr = new StreamReader(sourceFile, currentEncoding, true))
            {
                content = sr.ReadToEnd();
                currentEncoding = sr.CurrentEncoding;
            }

            topicTitle = tocTitle = String.Empty;

            // Extract the topic title
            match = reTitle.Match(content);

            if(match.Success)
                topicTitle = match.Groups[1].Value;

            // If a TOC title entry is present, get that too
            match = reTocTitle.Match(content);

            if(match.Success)
                tocTitle = match.Groups[1].Value;

            key = Path.GetFileNameWithoutExtension(sourceFile);

            if(!titles.TryAdd(key, new TitleInfo(WebUtility.HtmlDecode(topicTitle),
              WebUtility.HtmlDecode(tocTitle), sourceFile)))
                Console.WriteLine("SHFB: Warning SHE0004: The key '{0}' used for '{1}' is already in use by '{2}'.  " +
                    "'{1}' will be ignored.", key, sourceFile, titles[key].File);

            // Extract K index keywords
            matches = reKKeyword.Matches(content);

            foreach(Match m in matches)
            {
                keyword = new KeywordInfo();
                term = m.Groups[1].Value;

                if(!String.IsNullOrEmpty(term))
                {
                    term = WebUtility.HtmlDecode(term.Replace("%3C", "<").Replace("%3E", ">").Replace("%2C", ","));

                    // See if there is a sub-entry
                    match = reSubEntry.Match(term);

                    if(match.Success)
                    {
                        keyword.MainEntry = term.Substring(0, match.Index);
                        keyword.SubEntry = term.Substring(match.Index + 1).TrimStart(new char[] { ' ' });
                    }
                    else
                        keyword.MainEntry = term;

                    keyword.File = sourceFile;
                    keywordBag.Add(keyword);
                }
            }

            // Remove the XML data island, the MS Help CSS link, and the unused namespaces
            content = reXmlIsland.Replace(content, String.Empty);
            content = reHxLinkCss.Replace(content, String.Empty);

            Match htmlMatch = reHtmlElement.Match(content);
            string htmlElement = reUnusedNamespaces.Replace(htmlMatch.Value, String.Empty);
            
            content = reHtmlElement.Replace(content, htmlElement, 1);

            // If localizing, perform the substitutions, convert the encoding, and save the file to the
            // localized folder using the appropriate encoding.
            if(localizedOutputFolder != null)
            {
                foreach(KeyValuePair<Regex, string> pair in patterns)
                    content = pair.Key.Replace(content, pair.Value);

                currentBytes = currentEncoding.GetBytes(content);
                convertedBytes = Encoding.Convert(currentEncoding, destEncoding, currentBytes);

                destFile = Path.Combine(localizedOutputFolder, sourceFile.Substring(basePath.Length + 1));
                folder = Path.GetDirectoryName(destFile);

                if(!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using(StreamWriter writer = new StreamWriter(destFile, false, destEncoding))
                {
                    writer.Write(destEncoding.GetString(convertedBytes));
                }
            }

            // Save the file to its original location without the Help 2 elements using the original encoding
            using(StreamWriter writer = new StreamWriter(sourceFile, false, currentEncoding))
            {
                writer.Write(content);
            }
        }
        #endregion

        #region Write out the HTML Help 1 table of contents
        //=====================================================================

        /// <summary>
        /// Write out the HTML Help 1 table of contents
        /// </summary>
        private static void WriteHelp1xTableOfContents()
        {
            XmlReaderSettings settings;
            TitleInfo titleInfo;
            string key, title, htmlFile;
            int indentCount, baseFolderLength = help1Folder.Length + 1;

            Console.WriteLine(@"Saving HTML Help 1 table of contents to {0}\{1}.hhc", outputFolder, projectName);

            settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;

            using(var reader = XmlReader.Create(tocFile, settings))
            {
                // Write the table of contents using the appropriate encoding
                using(StreamWriter writer = new StreamWriter(Path.Combine(outputFolder, projectName + ".hhc"), false,
                  Encoding.GetEncoding(codePage)))
                {
                    writer.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">\r\n");
                    writer.WriteLine("<HTML>");
                    writer.WriteLine("  <BODY>");

                    while(reader.Read())
                        switch(reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if(reader.Name == "topic")
                                {
                                    key = reader.GetAttribute("file");

                                    if(!String.IsNullOrEmpty(key) && titles.ContainsKey(key))
                                    {
                                        titleInfo = titles[key];
                                        title = titleInfo.TocTitle;
                                        htmlFile = titleInfo.File.Substring(baseFolderLength);
                                    }
                                    else
                                    {
                                        // Container only topic or unknown element, just use the title or ID attribute
                                        htmlFile = null;
                                        title = reader.GetAttribute("title");

                                        if(String.IsNullOrEmpty(title))
                                            title = reader.GetAttribute("id");

                                        if(String.IsNullOrEmpty(title))
                                            title = key;
                                    }

                                    indentCount = reader.Depth;

                                    WriteContentLine(writer, indentCount, "<UL>");
                                    WriteContentLine(writer, indentCount, "  <LI><OBJECT type=\"text/sitemap\">");
                                    WriteContentLine(writer, indentCount, String.Format(CultureInfo.InvariantCulture,
                                        "    <param name=\"Name\" value=\"{0}\">", WebUtility.HtmlEncode(title)));

                                    if(htmlFile != null)
                                        WriteContentLine(writer, indentCount, String.Format(CultureInfo.InvariantCulture,
                                            "    <param name=\"Local\" value=\"{0}\">", WebUtility.HtmlEncode(htmlFile)));

                                    WriteContentLine(writer, indentCount, "  </OBJECT></LI>");

                                    if(reader.IsEmptyElement)
                                        WriteContentLine(writer, indentCount, "</UL>");
                                }
                                break;

                            case XmlNodeType.EndElement:
                                if(reader.Name == "topic")
                                    WriteContentLine(writer, reader.Depth, "</UL>");
                                break;

                            default:
                                break;
                        }

                    writer.WriteLine();
                    writer.WriteLine("  </BODY>");
                    writer.WriteLine("</HTML>");
                }
            }
        }

        /// <summary>
        /// Write out a table of contents line with indentation
        /// </summary>
        /// <param name="writer">The writer to which the line is saved</param>
        /// <param name="indentCount">The amount of indent to use</param>
        /// <param name="value">The value to write</param>
        private static void WriteContentLine(TextWriter writer,
          int indentCount, string value)
        {
            writer.WriteLine();

            for(int idx = 0; idx < indentCount; idx++)
                writer.Write("  ");

            writer.Write(value);
        }
        #endregion

        #region Write out the HTML Help 1 keyword index
        //=====================================================================

        /// <summary>
        /// Write out the HTML Help 1 keyword index
        /// </summary>
        private static void WriteHelp1xKeywordIndex()
        {
            KeywordInfo kw;
            string title;
            int baseFolderLength = help1Folder.Length + 1;

            Console.WriteLine(@"Saving HTML Help 1 keyword index to {0}\{1}.hhk", outputFolder, projectName);

            // Write the keyword index using the appropriate encoding
            using(StreamWriter writer = new StreamWriter(Path.Combine(outputFolder, projectName + ".hhk"), false,
              Encoding.GetEncoding(codePage)))
            {
                writer.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
                writer.WriteLine("<HTML>");
                writer.WriteLine("  <BODY>");
                writer.Write("    <UL>");

                foreach(var group in keywords.Where(k => !String.IsNullOrEmpty(k.MainEntry)).GroupBy(k => k.MainEntry))
                    if(group.Count() == 1)
                    {
                        kw = group.First();

                        if(!String.IsNullOrEmpty(kw.File))
                            WriteHelp1IndexEntry(kw.MainEntry, kw.File.Substring(baseFolderLength).Replace(
                                '\\', '/'), writer, 3);
                    }
                    else
                    {
                        kw = group.First();
                        WriteHelp1IndexEntry(kw.MainEntry, null, writer, 3);

                        WriteContentLine(writer, 3, "<UL>");

                        foreach(var k in group)
                            if(!String.IsNullOrEmpty(k.File))
                                if(String.IsNullOrEmpty(k.SubEntry))
                                {
                                    // Use the target page's title as the entry's title as it will be fully
                                    // qualified if necessary.
                                    title = titles[Path.GetFileNameWithoutExtension(k.File)].TopicTitle;

                                    WriteHelp1IndexEntry(title, k.File.Substring(baseFolderLength).Replace(
                                        '\\', '/'), writer, 4);
                                }
                                else
                                    WriteHelp1IndexEntry(k.SubEntry, k.File.Substring(baseFolderLength).Replace(
                                        '\\', '/'), writer, 4);

                        WriteContentLine(writer, 3, "</UL>");
                    }

                writer.WriteLine();
                writer.WriteLine("    </UL>");
                writer.WriteLine("  </BODY>");
                writer.WriteLine("</HTML>");
            }
        }

        /// <summary>
        /// This is used to write out a Help 1 index entry
        /// </summary>
        /// <param name="title">The topic title</param>
        /// <param name="file">The target filename</param>
        /// <param name="writer">The stream writer to use</param>
        /// <param name="indent">The indent level</param>
        private static void WriteHelp1IndexEntry(string title, string file, StreamWriter writer, int indent)
        {
            title = WebUtility.HtmlEncode(title);

            WriteContentLine(writer, indent, "<LI><OBJECT type=\"text/sitemap\">");
            WriteContentLine(writer, indent + 1, String.Format(CultureInfo.InvariantCulture,
                "<param name=\"Name\" value=\"{0}\">", title));

            if(String.IsNullOrEmpty(file))
                WriteContentLine(writer, indent + 1, String.Format(CultureInfo.InvariantCulture,
                    "<param name=\"See Also\" value=\"{0}\">", title));
            else
                WriteContentLine(writer, indent + 1, String.Format(CultureInfo.InvariantCulture,
                    "<param name=\"Local\" value=\"{0}\">", WebUtility.HtmlEncode(file)));

            WriteContentLine(writer, indent, "</OBJECT></LI>");
        }
        #endregion

        #region Write out the website table of contents
        //=====================================================================

        /// <summary>
        /// Write out the website table of contents
        /// </summary>
        private static void WriteWebsiteTableOfContents()
        {
            XmlReaderSettings settings;
            TitleInfo titleInfo;
            string key, title, htmlFile;
            int indentCount, baseFolderLength = websiteFolder.Length + 1;

            Console.WriteLine(@"Saving website table of contents to {0}\WebTOC.xml", outputFolder);

            settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;

            using(var reader = XmlReader.Create(tocFile, settings))
            {
                // Write the table of contents with UTF-8 encoding
                using(StreamWriter writer = new StreamWriter(Path.Combine(outputFolder, "WebTOC.xml"), false,
                  Encoding.UTF8))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<HelpTOC>");

                    while(reader.Read())
                        switch(reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if(reader.Name == "topic")
                                {
                                    key = reader.GetAttribute("file");

                                    if(!String.IsNullOrEmpty(key) && titles.ContainsKey(key))
                                    {
                                        titleInfo = titles[key];
                                        title = titleInfo.TocTitle;
                                        htmlFile = titleInfo.File.Substring(baseFolderLength).Replace('\\', '/');
                                    }
                                    else
                                    {
                                        // Container only topic or unknown element, just use the title or ID attribute
                                        htmlFile = null;
                                        title = reader.GetAttribute("title");

                                        if(String.IsNullOrEmpty(title))
                                            title = reader.GetAttribute("id");

                                        if(String.IsNullOrEmpty(title))
                                            title = key;
                                    }

                                    indentCount = reader.Depth;
                                    title = WebUtility.HtmlEncode(title);
                                    htmlFile = WebUtility.HtmlEncode(htmlFile);

                                    if(reader.IsEmptyElement)
                                        WriteContentLine(writer, indentCount, String.Format(CultureInfo.InvariantCulture,
                                            "<HelpTOCNode Title=\"{0}\" Url=\"{1}\" />", title, htmlFile));
                                    else
                                        if(htmlFile != null)
                                        {
                                            WriteContentLine(writer, indentCount, String.Format(CultureInfo.InvariantCulture,
                                                "<HelpTOCNode Id=\"{0}\" Title=\"{1}\" Url=\"{2}\">", Guid.NewGuid(),
                                                title, htmlFile));
                                        }
                                        else
                                            WriteContentLine(writer, indentCount, String.Format(CultureInfo.InvariantCulture,
                                                "<HelpTOCNode Id=\"{0}\" Title=\"{1}\">", Guid.NewGuid(), title));
                                }
                                break;

                            case XmlNodeType.EndElement:
                                if(reader.Name == "topic")
                                    WriteContentLine(writer, reader.Depth, "</HelpTOCNode>");
                                break;

                            default:
                                break;
                        }

                    writer.WriteLine();
                    writer.WriteLine("</HelpTOC>");
                }
            }
        }
        #endregion

        #region Write out the website keyword index
        //=====================================================================

        /// <summary>
        /// Write out the website keyword index
        /// </summary>
        private static void WriteWebsiteKeywordIndex()
        {
            KeywordInfo kw;
            string title;
            int baseFolderLength = websiteFolder.Length + 1;

            Console.WriteLine(@"Saving website keyword index to {0}\WebKI.xml", outputFolder);

            // Write the keyword index with UTF-8 encoding
            using(StreamWriter writer = new StreamWriter(Path.Combine(outputFolder, "WebKI.xml"), false,
              Encoding.UTF8))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.Write("<HelpKI>");

                foreach(var group in keywords.Where(k => !String.IsNullOrEmpty(k.MainEntry)).GroupBy(k => k.MainEntry))
                    if(group.Count() == 1)
                    {
                        kw = group.First();

                        if(!String.IsNullOrEmpty(kw.File))
                            WriteContentLine(writer, 1, String.Format(CultureInfo.InvariantCulture,
                                "<HelpKINode Title=\"{0}\" Url=\"{1}\" />", WebUtility.HtmlEncode(kw.MainEntry),
                                WebUtility.HtmlEncode(kw.File.Substring(baseFolderLength)).Replace('\\', '/')));
                    }
                    else
                    {
                        WriteContentLine(writer, 1, String.Format(CultureInfo.InvariantCulture,
                             "<HelpKINode Title=\"{0}\">", WebUtility.HtmlEncode(group.Key)));

                        foreach(var k in group)
                            if(!String.IsNullOrEmpty(k.File))
                                if(String.IsNullOrEmpty(k.SubEntry))
                                {
                                    // Use the target page's title as the entry's title as it will be fully
                                    // qualified if necessary.
                                    title = titles[Path.GetFileNameWithoutExtension(k.File)].TopicTitle;

                                    WriteContentLine(writer, 2, String.Format(CultureInfo.InvariantCulture,
                                        "<HelpKINode Title=\"{0}\" Url=\"{1}\" />", WebUtility.HtmlEncode(title),
                                        WebUtility.HtmlEncode(k.File.Substring(baseFolderLength)).Replace('\\', '/')));
                                }
                                else
                                    WriteContentLine(writer, 2, String.Format(CultureInfo.InvariantCulture,
                                        "<HelpKINode Title=\"{0}\" Url=\"{1}\" />", WebUtility.HtmlEncode(k.SubEntry),
                                        WebUtility.HtmlEncode(k.File.Substring(baseFolderLength)).Replace('\\', '/')));

                        WriteContentLine(writer, 1, "</HelpKINode>");
                    }

                writer.WriteLine();
                writer.WriteLine("</HelpKI>");
            }
        }
        #endregion
    }
}
