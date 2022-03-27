//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TitleAndKeywordHtmlExtract.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/27/2022
// Note    : Copyright 2008-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to extract title and keyword information from HTML files for use in creating
// the Help 1 (CHM) table of contents and keyword index files.  It can also optionally convert the
// files to a different encoding in order to build HTML Help 1 files that use a different language.
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
// 03/02/2022  EFW  Moved the code into the build engine and removed the task.  Updated the code to use the
//                  Help Viewer elements for the keyword info rather than the removed XML data island.
//                  Renamed from SandcastleHtmlExtract to TitleAndKeywordHtmlExtract
//===============================================================================================================

// Ignore Spelling: Prantl jis

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class is used to extract title and keyword information from HTML files for use in creating the Help
    /// 1 (CHM) table of contents and keyword index files.  It can also optionally convert the files to a
    /// different encoding in order to build HTML Help 1 files that use a different language.
    /// </summary>
    public class TitleAndKeywordHtmlExtract
    {
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

        private readonly BuildProcess currentBuild;

        // Options
        private string encodingName;
        private int codePage;

        // Extracted keyword and title information
        private List<KeywordInfo> keywords;
        private ConcurrentBag<KeywordInfo> keywordBag;
        private ConcurrentDictionary<string, TitleInfo> titles;

        // Regular expressions used for title and keyword extraction
        private static readonly Regex reTitle = new Regex(@"<title>(.*)</title>", RegexOptions.IgnoreCase);
        private static readonly Regex reTocTitle = new Regex("<meta\\s+name=\"Title\"\\s+content=\"([^\"]+)\"[^>]+>",
            RegexOptions.IgnoreCase);
        private static readonly Regex reKKeyword = new Regex("<meta\\s+name=\"System.Keywords\"\\s+content=\"([^\"]+)\"[^>]+>",
            RegexOptions.IgnoreCase);
        private static readonly Regex reSubEntry = new Regex(@",([^\)\>]+|([^\<\>]*" +
            @"\<[^\<\>]*\>[^\<\>]*)?|([^\(\)]*\([^\(\)]*\)[^\(\)]*)?)$");

        // Localization support members
        private Dictionary<Regex, string> patterns;
        private Encoding destEncoding;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the HTML Help 1 file folder name containing the Help 1 files to be processed.
        /// </summary>
        /// <value>This is optional.  If not set, no HTML help 1 files will be processed.</value>
        public string Help1Folder { get; set; }

        /// <summary>
        /// This is used to get or set the website file folder name containing the website files to be processed.
        /// </summary>
        /// <value>This is optional.  If not set, no HTML help 1 files will be processed.</value>
        public string WebsiteFolder { get; set; }

        /// <summary>
        /// This is used to get or set the localized output folder name
        /// </summary>
        /// <value>This is optional.  If not set, the HTML files will not be localized.</value>
        public string LocalizedFolder { get; set; }

        /// <summary>
        /// This is used to get or set the maximum degree of parallelism used to process the HTML files
        /// </summary>
        /// <value>If not set, it defaults to a maximum of 20 threads per processor.  Increase or decrease this
        /// value as needed based on your system.  Setting it to a value less than 1 will allow for an unlimited
        /// number of threads.  However, this is a largely IO-bound process so allowing an excessive number of
        /// threads may slow overall system performance on very large help files.</value>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// This read-only property returns the list of language settings
        /// </summary>
        /// <remarks>Additional language settings can be added by plug-ins if needed</remarks>
        public IList<LanguageSettings> LanguageSettings { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentBuild">The current build process</param>
        public TitleAndKeywordHtmlExtract(BuildProcess currentBuild)
        {
            this.currentBuild = currentBuild ?? throw new ArgumentNullException(nameof(currentBuild));

            this.LanguageSettings = new List<LanguageSettings>()
            {
                new LanguageSettings { LocaleId = 1028, CodePage = 950, CharacterSet = "big5" },
                new LanguageSettings { LocaleId = 1029, CodePage = 1250, CharacterSet = "Windows-1250" },
                new LanguageSettings { LocaleId = 1031, CodePage = 1252, CharacterSet = "Windows-1252" },
                new LanguageSettings { LocaleId = 1033, CodePage = 65001, CharacterSet = "UTF-8" },
                new LanguageSettings { LocaleId = 1034, CodePage = 1252, CharacterSet = "Windows-1252" },
                new LanguageSettings { LocaleId = 1036, CodePage = 1252, CharacterSet = "Windows-1252" },
                new LanguageSettings { LocaleId = 1038, CodePage = 1250, CharacterSet = "Windows-1250" },
                new LanguageSettings { LocaleId = 1040, CodePage = 1252, CharacterSet = "Windows-1252" },
                new LanguageSettings { LocaleId = 1041, CodePage = 932, CharacterSet = "shift-jis" },
                new LanguageSettings { LocaleId = 1042, CodePage = 949, CharacterSet = "ks_c_5601-1987" },
                new LanguageSettings { LocaleId = 1045, CodePage = 1250, CharacterSet = "Windows-1250" },
                new LanguageSettings { LocaleId = 1046, CodePage = 1252, CharacterSet = "Windows-1252" },
                new LanguageSettings { LocaleId = 1049, CodePage = 1251, CharacterSet = "Windows-1251" },
                new LanguageSettings { LocaleId = 1055, CodePage = 1254, CharacterSet = "Windows-1254" },
                new LanguageSettings { LocaleId = 2052, CodePage = 936, CharacterSet = "gb2312" },
                new LanguageSettings { LocaleId = 3076, CodePage = 950, CharacterSet = "big5" },
                new LanguageSettings { LocaleId = 3082, CodePage = 65001, CharacterSet = "UTF-8" }
            };
        }
        #endregion

        #region HTML extract method
        //=====================================================================

        /// <summary>
        /// This extracts the information from the HTML files
        /// </summary>
        /// <returns>True on success or false on failure</returns>
        public bool ExtractHtmlInfo()
        {
            bool success = false;

            if(this.MaxDegreeOfParallelism == 0)
                this.MaxDegreeOfParallelism = Environment.ProcessorCount * 20;
            else
            {
                if(this.MaxDegreeOfParallelism < 0)
                    this.MaxDegreeOfParallelism = -1;
            }

            keywords = new List<KeywordInfo>();
            keywordBag = new ConcurrentBag<KeywordInfo>();
            titles = new ConcurrentDictionary<string, TitleInfo>();

            try
            {
                if(!String.IsNullOrWhiteSpace(this.Help1Folder) && !Directory.Exists(this.Help1Folder))
                    throw new ArgumentException("The HTML Help 1 folder could not be found");

                if(!String.IsNullOrWhiteSpace(this.WebsiteFolder) && !Directory.Exists(this.WebsiteFolder))
                    throw new ArgumentException("The website folder could not be found");

                // Help 1 folder or website folder must be specified
                if(String.IsNullOrWhiteSpace(this.Help1Folder) && String.IsNullOrWhiteSpace(this.WebsiteFolder))
                    throw new InvalidOperationException("The Help1Folder and/or WebsiteFolder property must be specified");

                // Get the code page to use based on the locale ID
                var languageSettings = this.LanguageSettings.FirstOrDefault(l => l.LocaleId == currentBuild.Language.LCID);

                // If not found, default to the one for the ANSI code page based on the specified locale ID
                if(languageSettings == null)
                {
                    var ci = new CultureInfo(currentBuild.Language.LCID);
                    codePage = ci.TextInfo.ANSICodePage;

                    var enc = Encoding.GetEncoding(codePage);
                    encodingName = enc.WebName;

                    currentBuild.ReportWarning("SHE0001", "LCID '{0}' not found in configuration file.  " +
                        "Defaulting to ANSI code page value of '{1}', encoding charset '{2}'.",
                        currentBuild.Language.LCID, codePage, encodingName);
                }
                else
                {
                    codePage = languageSettings.CodePage;
                    encodingName = languageSettings.CharacterSet;

                    currentBuild.ReportProgress("Using LCID '{0}', code page '{1}', encoding charset '{2}'.",
                        currentBuild.Language.LCID, codePage, encodingName);
                }

                // Parse the HTML Help 1 files and save the keyword index and table of contents
                if(!String.IsNullOrEmpty(this.Help1Folder))
                {
                    this.Help1Folder = Path.GetFullPath(this.Help1Folder);
                    currentBuild.ReportProgress("Processing Help 1 files in " + this.Help1Folder);

                    // If localizing, validate the folder and create the regex patterns that
                    // will do the conversions.
                    if(this.LocalizedFolder != null)
                    {
                        if(!Directory.Exists(this.LocalizedFolder))
                            Directory.CreateDirectory(this.LocalizedFolder);

                        patterns = new Dictionary<Regex, string>();
                        destEncoding = Encoding.GetEncoding(encodingName);

                        // Convert unsupported high-order characters to 7-bit ASCII equivalents
                        patterns.Add(new Regex(@"\u2018|\u2019"), "'");
                        patterns.Add(new Regex(@"\u201C|\u201D"), "\"");
                        patterns.Add(new Regex(@"\u2026"), "...");

                        if(currentBuild.Language.LCID != 1041)
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

                        currentBuild.ReportProgress("Localized content will be written to '{0}'", this.LocalizedFolder);
                    }

                    ParseFiles(this.Help1Folder, this.LocalizedFolder);
                    WriteHelp1xKeywordIndex();
                    WriteHelp1xTableOfContents();
                }

                // Parse the website files and save the keyword index and table of contents
                if(!String.IsNullOrEmpty(this.WebsiteFolder))
                {
                    this.WebsiteFolder = Path.GetFullPath(this.WebsiteFolder);
                    currentBuild.ReportProgress("Processing website files in " + this.WebsiteFolder);

                    ParseFiles(this.WebsiteFolder, null);
                    WriteWebsiteTableOfContents();
                }

                success = true;
            }
            catch(Exception ex)
            {
                throw new BuilderException("SHE0002", "Unexpected error during title and keyword HTML extract", ex);
            }

            return success;
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
        private void ParseFiles(string fileFolder, string localizedOutputFolder)
        {
            string mainEntry = String.Empty;
            int htmlFiles = 0;

            keywords.Clear();
            keywordBag = new ConcurrentBag<KeywordInfo>();
            titles.Clear();

            // Process all *.htm and *.html files in the given folder and all of its subfolders.
            Parallel.ForEach(Directory.EnumerateFiles(fileFolder, "*.*", SearchOption.AllDirectories),
              new ParallelOptions { MaxDegreeOfParallelism = this.MaxDegreeOfParallelism }, file =>
              {
                  string ext = Path.GetExtension(file).ToLowerInvariant();

                  if(ext == ".htm" || ext == ".html")
                  {
                      ProcessFile(fileFolder, file, localizedOutputFolder);
                      Interlocked.Add(ref htmlFiles, 1);
                  }
                  else
                  {
                      if(localizedOutputFolder != null)
                      {
                          // Copy supporting files only if localizing
                          string destFile = Path.Combine(localizedOutputFolder, file.Substring(fileFolder.Length + 1));
                          string folder = Path.GetDirectoryName(destFile);

                          if(!Directory.Exists(folder))
                              Directory.CreateDirectory(folder);

                          File.Copy(file, destFile, true);
                      }
                  }
              });

            currentBuild.ReportProgress("Processed {0} HTML files.  Sorting keywords and generating See Also indices.",
                htmlFiles);

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
                        keywords.Insert(idx, new KeywordInfo { MainEntry = keywords[idx].MainEntry });
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
        private void ProcessFile(string basePath, string sourceFile, string localizedOutputFolder)
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
            {
                currentBuild.ReportWarning("SHE0004", "The key '{0}' used for '{1}' is already in use by " +
                    "'{2}'.  '{1}' will be ignored.", key, sourceFile, titles[key].File);
            }

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

            // If localizing, perform the substitutions, convert the encoding, and save the file to the localized
            // folder using the appropriate encoding.
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
        private void WriteHelp1xTableOfContents()
        {
            XmlReaderSettings settings;
            TitleInfo titleInfo;
            string key, title, htmlFile;
            int indentCount, baseFolderLength = this.Help1Folder.Length + 1;

            currentBuild.ReportProgress(@"Saving HTML Help 1 table of contents to {0}\{1}.hhc", this.Help1Folder,
                currentBuild.CurrentProject.HtmlHelpName);

            settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            using(var reader = XmlReader.Create(Path.Combine(currentBuild.WorkingFolder, "toc.xml"), settings))
            {
                // Write the table of contents using the appropriate encoding
                using(StreamWriter writer = new StreamWriter(Path.Combine(this.Help1Folder,
                  currentBuild.CurrentProject.HtmlHelpName + ".hhc"), false, Encoding.GetEncoding(codePage)))
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

                                    // Remove any sub-folder if present as the titles are keyed on the name alone
                                    if(!String.IsNullOrWhiteSpace(key))
                                    {
                                        int dirSep = key.IndexOfAny(new[] { '/', '\\' });

                                        if(dirSep != -1)
                                            key = key.Substring(dirSep + 1);
                                    }

                                    if(!String.IsNullOrWhiteSpace(key) && titles.ContainsKey(key))
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
        private static void WriteContentLine(TextWriter writer, int indentCount, string value)
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
        private void WriteHelp1xKeywordIndex()
        {
            KeywordInfo kw;
            string title;
            int baseFolderLength = this.Help1Folder.Length + 1;

            currentBuild.ReportProgress(@"Saving HTML Help 1 keyword index to {0}\{1}.hhk", this.Help1Folder,
                currentBuild.CurrentProject.HtmlHelpName);

            // Write the keyword index using the appropriate encoding
            using(StreamWriter writer = new StreamWriter(Path.Combine(this.Help1Folder,
              currentBuild.CurrentProject.HtmlHelpName + ".hhk"), false, Encoding.GetEncoding(codePage)))
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
        private void WriteWebsiteTableOfContents()
        {
            XmlReaderSettings settings;
            TitleInfo titleInfo;
            string key, title, htmlFile;
            int indentCount, baseFolderLength = this.WebsiteFolder.Length + 1;

            currentBuild.ReportProgress(@"Saving website table of contents to {0}\WebTOC.xml", currentBuild.WorkingFolder);

            settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            using(var reader = XmlReader.Create(Path.Combine(currentBuild.WorkingFolder, "toc.xml"), settings))
            {
                // Write the table of contents with UTF-8 encoding
                using(StreamWriter writer = new StreamWriter(Path.Combine(currentBuild.WorkingFolder, "WebTOC.xml"),
                  false, Encoding.UTF8))
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

                                    // Remove any sub-folder if present as the titles are keyed on the name alone
                                    if(!String.IsNullOrWhiteSpace(key))
                                    {
                                        int dirSep = key.IndexOfAny(new[] { '/', '\\' });

                                        if(dirSep != -1)
                                            key = key.Substring(dirSep + 1);
                                    }

                                    if(!String.IsNullOrWhiteSpace(key) && titles.ContainsKey(key))
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
    }
}
