﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : MarkdownContentGenerator.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/30/2022
// Note    : Copyright 2015-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to finish up creation of the markdown content and copy it to the
// output folder.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/30/2015  EFW  Created the code
// 07/04/2022  EFW  Moved the code into the build engine and removed the task
//===============================================================================================================

// Ignore Spelling: noscript fieldset iframe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class is used to finish up creation of the markdown content and copy it to the output folder
    /// </summary>
    public class MarkdownContentGenerator
    {
        #region Private data members
        //=====================================================================

        private static readonly Regex reAddNewLinesBefore = new Regex(@"(\w)(\<(p|div|h[1-6]|blockquote|pre|table|dl|" +
            "ol|ul|address|script|noscript|form|fieldset|iframe|math|article|aside|canvas|details|dialog|" +
            "figcaption|figure|footer|header|main|nav))");
        private static readonly Regex reAddNewLinesAfter = new Regex(@"(/(p|div|h[1-6]|blockquote|pre|table|dl|" +
            "ol|ul|address|script|noscript|form|fieldset|iframe|math|article|aside|canvas|details|dialog|" +
            @"figcaption|figure|footer|header|main|nav).*?\>)(\s*?\w)");
        private static readonly Regex reTrimNbsp = new Regex(@"\s*&nbsp;\s*");
        private static readonly Regex reDecodeEntities = new Regex("```.*?```", RegexOptions.Singleline);
        private static readonly MatchEvaluator meDecodeEntities = new MatchEvaluator(
            m => WebUtility.HtmlDecode(m.Value));

        private readonly BuildProcess buildProcess;
        private readonly string workingFolder;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProcess">The build process to use</param>
        public MarkdownContentGenerator(BuildProcess buildProcess)
        {
            this.buildProcess = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));

            workingFolder = Path.Combine(buildProcess.WorkingFolder, @"Output\Markdown");
        }
        #endregion

        #region Execute methods
        //=====================================================================

        /// <summary>
        /// This is used to execute the task and perform the build
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public bool Execute()
        {
            XDocument topic;
            string key, title,
                extension = buildProcess.CurrentProject.AppendMarkdownFileExtensionsToUrls ? ".md" : String.Empty;
            int topicCount = 0;

            // Load the TOC file and process the topics in TOC order.  This generates the sidebar TOC as well.
            using(var tocReader = XmlReader.Create(Path.Combine(workingFolder, @"..\..\toc.xml"), new XmlReaderSettings { CloseInput = true }))
            using(StreamWriter sidebar = new StreamWriter(Path.Combine(workingFolder, "_Sidebar.md")))
            {
                while(tocReader.Read())
                    if(tocReader.NodeType == XmlNodeType.Element && tocReader.Name == "topic")
                    {
                        key = tocReader.GetAttribute("file");

                        if(!String.IsNullOrWhiteSpace(key))
                        {
                            string topicFile = Path.Combine(workingFolder, key + ".md");

                            // The topics are easier to update as XDocuments as we can use LINQ to XML to
                            // find stuff.  Not all topics may have been generated by the presentation style.
                            // Ignore those that won't load.
                            try
                            {
                                topic = XDocument.Load(topicFile);
                            }
                            catch(XmlException)
                            {
                                // If it's an additional topic added by the user, wrap it in a document
                                // element and try again.  We still need the title for the TOC.
                                string content = File.ReadAllText(topicFile);

                                try
                                {
                                    topic = XDocument.Parse("<document xml:space=\"preserve\">\r\n" + content +
                                        "\r\n</document>");
                                }
                                catch
                                {
                                    topic = null;
                                }
                            }

                            if(topic != null)
                            {
                                title = ApplyChanges(key, topic) ?? key;

                                // Remove the containing document element and save the inner content
                                string content = topic.ToString(buildProcess.CurrentProject.IndentHtml ?
                                    SaveOptions.None : SaveOptions.DisableFormatting);

                                int pos = content.IndexOf('>');

                                if(pos != -1)
                                    content = content.Substring(pos + 1).TrimStart();

                                pos = content.IndexOf("</document>", StringComparison.Ordinal);

                                if(pos != -1)
                                    content = content.Substring(0, pos);

                                // A few final fix ups:

                                // Insert line breaks between literal text and block level elements where needed.
                                // If not, it tends not to render any markdown before/after them.
                                content = reAddNewLinesBefore.Replace(content, "$1\r\n\r\n$2");
                                content = reAddNewLinesAfter.Replace(content, "$1\r\n\r\n$3");

                                // Decode entities within code blocks
                                content = reDecodeEntities.Replace(content, meDecodeEntities);

                                // Trim whitespace around non-breaking spaces to get rid of excess blank
                                // lines except in a cases where we need to keep it so that the content is
                                // converted from markdown to HTML properly.
                                content = reTrimNbsp.Replace(content, new MatchEvaluator(m =>
                                {
                                    if(m.Index + m.Length < content.Length && content[m.Index + m.Length] == '#')
                                        return "\r\n\r\n";

                                    if(m.Value.Length > 6 && m.Value[0] == '&' && Char.IsWhiteSpace(m.Value[6]))
                                        return "&nbsp;\r\n";

                                    if(Char.IsWhiteSpace(m.Value[0]))
                                        return "\r\n&nbsp;";

                                    return m.Value;
                                }));

                                File.WriteAllText(topicFile, content);

                                if(tocReader.Depth > 1)
                                    sidebar.Write(new String(' ', (tocReader.Depth - 1) * 2));

                                sidebar.WriteLine("- [{0}]({1}{2})", title, key, extension);

                                topicCount++;

                                if((topicCount % 500) == 0)
                                    buildProcess.ReportProgress("{0} topics generated", topicCount);
                            }
                        }
                    }
            }

            buildProcess.ReportProgress("Finished generating {0} topics", topicCount);

            string homeTopic = Path.Combine(workingFolder, "Home.md");

            if(!File.Exists(homeTopic) && !String.IsNullOrWhiteSpace(buildProcess.DefaultTopicFile))
            {
                string defaultTopic = Path.Combine(workingFolder,
                    Path.GetFileNameWithoutExtension(buildProcess.DefaultTopicFile) + ".md");

                if(File.Exists(defaultTopic))
                    File.Copy(defaultTopic, homeTopic);
            }

            // Copy the working folder content to the output folder
            int fileCount = 0;

            buildProcess.ReportProgress("Copying content to output folder...");

            this.RecursiveCopy(workingFolder, buildProcess.OutputFolder, ref fileCount);

            buildProcess.ReportProgress("Finished copying {0} files", fileCount);

            return true;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This applies the changes needed to convert the XML to a markdown topic file
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="topic">The topic to which the changes are applied</param>
        /// <returns>The page title if one could be found</returns>
        private string ApplyChanges(string key, XDocument topic)
        {
            string topicTitle = null;
            var root = topic.Root;

            // Remove the filename element from API topics
            var filename = root.Element("file");

            if(filename != null)
                filename.Remove();

            // Replace SectionTitle elements with their content
            foreach(var st in topic.Descendants("SectionTitle").ToList())
            {
                foreach(var n in st.Nodes().ToList())
                {
                    n.Remove();
                    st.AddBeforeSelf(n);
                }

                st.Remove();
            }

            foreach(var span in topic.Descendants("span").Where(s => s.Attribute("class") != null).ToList())
            {
                string spanClass = span.Attribute("class").Value;

                switch(spanClass)
                {
                    case "languageSpecificText":
                        // Replace language-specific text with the neutral text sub-entry.  If not found,
                        // remove it.
                        var genericText = span.Elements("span").FirstOrDefault(
                            s => (string)s.Attribute("class") == "nu");

                        if(genericText != null)
                            span.ReplaceWith(genericText.Value);
                        else
                            span.Remove();
                        break;

                    default:
                        // All other formatting spans are removed by moving the content up to the parent element.
                        // The children of the LST spans are ignored since we've already handled them.
                        if(span.Parent.Name == "span" && (string)span.Parent.Attribute("class") == "languageSpecificText")
                            break;

                        foreach(var child in span.Nodes().ToList())
                        {
                            child.Remove();
                            span.AddBeforeSelf(child);
                        }

                        span.Remove();
                        break;
                }
            }

            var linkTargets = new Dictionary<string, string>();

            // Remove link ID spans and change any links to them to use the page/section title instead.  Note
            // that cross-page anchor references (PageName#Anchor) won't work and I'm not going to attempt to
            // support them since it would be more complicated.  Likewise, links to elements without a title
            // such as list items and table cells won't work either.
            foreach(var span in topic.Descendants("span").Where(s => s.Attribute("id") != null &&
              String.IsNullOrWhiteSpace(s.Value)).ToList())
            {
                string id = span.Attribute("id").Value;

                if(span.PreviousNode is XText sectionTitle)
                {
                    if(String.IsNullOrWhiteSpace(sectionTitle.Value) && sectionTitle.PreviousNode is XText prevText)
                        sectionTitle = prevText;

                    // We may get more than one line so find the last one with a section title which will be
                    // the closest to the span.
                    string title = sectionTitle.Value.Split(new[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries).Reverse().FirstOrDefault(
                            t => t.Trim().Length > 2 && t[0] == '#');

                    if(title != null)
                    {
                        int pos = title.IndexOf(' ');

                        if(pos != -1)
                        {
                            title = title.Substring(pos + 1).Trim();

                            // Extract the topic title for the sidebar TOC
                            if(id == "PageHeader")
                                topicTitle = title;

                            // Convert the title ID to the expected format
                            title = title.ToLowerInvariant().Replace(' ', '-').Replace("#", String.Empty);

                            // For intro links, link to the page header title since intro sections have no title
                            // themselves.  The transformations always add a PageHeader link span after the page
                            // title (or should).
                            if(id.StartsWith("@pageHeader_", StringComparison.Ordinal))
                            {
                                if(linkTargets.ContainsKey(id.Substring(12)))
                                {
                                    buildProcess.ReportWarning("GMC0001", "Duplicate in-page link ID found: " +
                                        "Topic ID: {0}  Link ID: {1}", key, id);
                                }

                                linkTargets[id.Substring(12)] = "PageHeader";
                            }
                            else
                            {
                                if(linkTargets.ContainsKey(id))
                                {
                                    buildProcess.ReportWarning("GMC0001", "Duplicate in-page link ID found: " +
                                        "Topic ID: {0}  Link ID: {1}", key, id);
                                }

                                linkTargets[id] = "#" + title;
                            }
                        }

                        span.Remove();
                    }
                }
            }

            // Update in-page link targets
            foreach(var anchor in topic.Descendants("a").ToList())
            {
                var href = anchor.Attribute("href");

                if(href.Value.Length > 1 && href.Value[0] == '#')
                {
                    string id = href.Value.Substring(1).Trim();

                    // Special case for the See Also section link
                    if(id == "seeAlsoSection")
                        id = "seeAlso";

                    if(linkTargets.TryGetValue(id, out string target))
                    {
                        if(target == "PageHeader")
                        {
                            if(!linkTargets.TryGetValue("PageHeader", out target))
                                target = "#";
                        }

                        href.Value = target;
                    }
                }
            }

            // If we couldn't find a topic title, try to get the first section header title.  It's probably a
            // user-added file.
            if(topicTitle == null)
            {
                var textBlock = topic.DescendantNodes().OfType<XText>().FirstOrDefault();

                if(textBlock != null)
                {
                    string title = textBlock.Value.Split(new[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(t => t.Trim().Length > 2 && t[0] == '#');

                    if(title != null)
                    {
                        int pos = title.IndexOf(' ');

                        if(pos != -1)
                            topicTitle = title.Substring(pos + 1).Trim();
                    }
                }
            }

            return topicTitle;
        }

        /// <summary>
        /// This copies files from the specified source folder to the specified destination folder.  If any
        /// subfolders are found below the source folder, the subfolders are also copied recursively.
        /// </summary>
        /// <param name="sourcePath">The source path from which to copy</param>
        /// <param name="destPath">The destination path to which to copy</param>
        /// <param name="fileCount">The file count used for logging progress</param>
        private void RecursiveCopy(string sourcePath, string destPath, ref int fileCount)
        {
            if(sourcePath == null)
                throw new ArgumentNullException(nameof(sourcePath));

            if(destPath == null)
                throw new ArgumentNullException(nameof(destPath));

            if(destPath[destPath.Length - 1] != '\\')
                destPath += @"\";

            foreach(string name in Directory.EnumerateFiles(sourcePath, "*.*"))
            {
                string filename = destPath + Path.GetFileName(name);

                if(!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                File.Copy(name, filename, true);

                // All attributes are turned off so that we can delete it later
                File.SetAttributes(filename, FileAttributes.Normal);

                fileCount++;

                if((fileCount % 500) == 0)
                    buildProcess.ReportProgress("Copied {0} files", fileCount);
            }

            // Ignore hidden folders as they may be under source control and are not wanted
            foreach(string folder in Directory.EnumerateDirectories(sourcePath))
                if((File.GetAttributes(folder) & FileAttributes.Hidden) != FileAttributes.Hidden)
                    RecursiveCopy(folder, destPath + folder.Substring(sourcePath.Length + 1), ref fileCount);
        }
        #endregion
    }
}
