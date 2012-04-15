//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.AdditionalContent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to merge the additional content into the
// working folder and build the table of contents entries for it.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.0.0  08/07/2006  EFW  Created the code
// 1.3.3.1  12/08/2006  EFW  Added support for colorizing <pre> tags in
//                           additional content files.
// 1.3.3.2  12/20/2006  EFW  Added support for project property and shared
//                           content substitution.
// 1.4.0.0  02/23/2007  EFW  Added support for Exclude content items and
//                           support for <code source="file"/> tags.
// 1.4.0.2  06/12/2007  EFW  Added support for nested code blocks.
// 1.5.0.0  06/19/2007  EFW  Various additions and updates for the June CTP
// 1.5.0.2  07/03/2007  EFW  Added support for content site map file
// 1.5.2.0  09/13/2007  EFW  Added support for calling plug-ins
// 1.6.0.0  09/28/2007  EFW  Added support for transforming *.topic files
// 1.6.0.1  10/29/2007  EFW  Link resolution now works on any tag with a cref
//                           attribute in it.
// 1.6.0.7  04/12/2007  EFW  Added support for a split table of contents
// 1.8.0.0  07/26/2008  EFW  Modified to support the new project format
// 1.8.0.1  01/21/2009  EFW  Added support for removeRegionMarkers option on
//                           imported code blocks.
// 1.9.0.0  06/06/2010  EFW  Added support for multi-format build output
// 1.9.0.0  06/30/2010  EFW  Removed splitting of TOC collection
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using ColorizerLibrary;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        // The table of contents entries for the additional and conceptual content
        private TocEntryCollection toc;

        // Regular expressions used to match table of contents options and to
        // resolve namespace references in additional content files.
        private static Regex reTocExclude = new Regex(
            @"<!--\s*@TOCExclude\s*-->", RegexOptions.IgnoreCase);

        internal static Regex reIsDefaultTopic = new Regex(
            @"<!--\s*@DefaultTopic\s*-->", RegexOptions.IgnoreCase);

        internal static Regex reSplitToc = new Regex(
            @"<!--\s*@SplitTOC\s*-->", RegexOptions.IgnoreCase);

        internal static Regex reSortOrder = new Regex(@"<!--\s*@SortOrder\s*" +
            @"(?<SortOrder>\d{1,5})\s*-->",
            RegexOptions.IgnoreCase);

        private static Regex rePageTitle = new Regex(
            @"<title>(?<Title>.*)</title>", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reResolveLinks = new Regex(
            "(<\\s*(?<Tag>\\w*)(?<PreAttrs>\\s+[^>]*)cref\\s*=" +
            "\\s*\"(?<Link>.+?)\"(?<PostAttrs>.*?))(/>|(>(?<Content>.*?)" +
            "<\\s*/(\\k<Tag>)\\s*>))", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reColorizeCheck = new Regex(
            @"<pre\s+[^>]*?lang(uage)?\s*=", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reProjectTags = new Regex(
            @"<\s*@(?<Field>\w*?)(:(?<Format>.*?))?\s*/?>");

        private static Regex reSharedContent = new Regex(
            "<\\s*include\\s*item\\s*=\\s*\"(?<Item>.*?)\"\\s*/\\s*>",
            RegexOptions.IgnoreCase);

        private static Regex reCodeBlock = new Regex(
            "<code([^>]+?)source\\s*=\\s*\"(.*?)\"(.*?)(/>|>\\s*?</code>)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static Regex reCodeRegion = new Regex(
            "region\\s*=\\s*\"(.*?)\"", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reIsNested = new Regex("nested\\s*=\\s*(\"|')true",
            RegexOptions.IgnoreCase);

        private static Regex reWillRemoveMarkers = new Regex(
            "removeRegionMarkers\\s*=\\s*(\"|')true", RegexOptions.IgnoreCase);

        private static Regex reRemoveRegionMarkers = new Regex(
            @"^.*?#(pragma\s+)?(region|end\s?region).*?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex reSpanScript = new Regex(
            "<(span|script)([^>]*?)(/>)", RegexOptions.IgnoreCase);

        // NOTE: This same expression is used in the CodeBlockComponent.
        // See it for details on what it is and what it does.
        private static Regex reMatchRegion = new Regex(
            @"\#(pragma\s+)?region\s+(.*?(((?<Open>\#(pragma\s+)?region\s+).*?)+" +
            @"((?<Close-Open>\#(pragma\s+)?end\s?region).*?)+)*(?(Open)(?!)))" +
            @"\#(pragma\s+)?end\s?region", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private MatchEvaluator linkMatchEval, contentMatchEval,
            codeBlockMatchEval;

        // The XML documents used to resolve shared content
        private XPathNavigator sharedContent, sharedBuilderContent, styleContent;

        private string pathToRoot;      // Path to root for resolved links

        private CodeColorizer codeColorizer;    // The code colorizer

        // XSL transformation variables
        private string xslStylesheet;
        private XslCompiledTransform xslTransform;
        private XsltArgumentList xslArguments;
        #endregion

        /// <summary>
        /// This is called to copy the additional content files and build a
        /// list of them for the help file project.
        /// </summary>
        /// <remarks>Note that for wilcard content items, the folders are
        /// copied recursively.</remarks>
        private void CopyAdditionalContent()
        {
            Dictionary<string, TocEntryCollection> tocItems =
                new Dictionary<string, TocEntryCollection>();
            TocEntryCollection parentToc;
            TocEntry tocEntry, tocFolder;
            FileItemCollection contentItems;
            string projectPath, source, filename, dirName;
            string[] parts;
            int part;

            this.ReportProgress(BuildStep.CopyAdditionalContent, "Copying additional content files...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            // A plug-in might add or remove additional content so call
            // them before checking to see if there is anything to copy.
            this.ExecutePlugIns(ExecutionBehaviors.Before);

            if(!project.HasItems(BuildAction.Content) && !project.HasItems(BuildAction.SiteMap))
            {
                this.ReportProgress("No additional content to copy");
                this.ExecutePlugIns(ExecutionBehaviors.After);
                return;
            }

            toc = new TocEntryCollection();
            tocItems.Add(String.Empty, toc);

            // Now copy the content files
            contentItems = new FileItemCollection(project, BuildAction.Content);
            projectPath = FolderPath.TerminatePath(Path.GetDirectoryName(originalProjectName));

            foreach(FileItem fileItem in contentItems)
            {
                source = fileItem.Include;
                dirName = Path.GetDirectoryName(fileItem.Link.ToString().Substring(projectPath.Length));
                filename = Path.Combine(dirName, Path.GetFileName(source));

                if(source.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
                  source.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                  source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                {
                    tocEntry = BuildProcess.GetTocInfo(source);

                    // Exclude the page if so indicated via the item metadata
                    if(fileItem.ExcludeFromToc)
                        tocEntry.IncludePage = false;

                    // .topic files get transformed into .html files
                    if(source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                        filename = Path.ChangeExtension(filename, ".html");

                    tocEntry.SourceFile = new FilePath(source, project);
                    tocEntry.DestinationFile = filename;

                    // Figure out where to add the entry
                    parts = tocEntry.DestinationFile.Split('\\');
                    pathToRoot = String.Empty;
                    parentToc = toc;

                    for(part = 0; part < parts.Length - 1; part++)
                    {
                        pathToRoot += parts[part] + @"\";

                        // Create place holders if necessary
                        if(!tocItems.TryGetValue(pathToRoot, out parentToc))
                        {
                            tocFolder = new TocEntry(project);
                            tocFolder.Title = parts[part];

                            if(part == 0)
                                toc.Add(tocFolder);
                            else
                                tocItems[String.Join(@"\", parts, 0, part) + @"\"].Add(tocFolder);

                            parentToc = tocFolder.Children;
                            tocItems.Add(pathToRoot, parentToc);
                        }
                    }

                    parentToc.Add(tocEntry);

                    if(tocEntry.IncludePage && tocEntry.IsDefaultTopic)
                        defaultTopic = tocEntry.DestinationFile;
                }
                else
                    tocEntry = null;

                this.EnsureOutputFoldersExist(dirName);

                foreach(string baseFolder in this.HelpFormatOutputFolders)
                {
                    // If the file contains items that need to be resolved,
                    // it is handled separately.
                    if(tocEntry != null &&
                      (tocEntry.HasLinks || tocEntry.HasCodeBlocks ||
                      tocEntry.NeedsColorizing || tocEntry.HasProjectTags ||
                      source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase)))
                    {
                        // Figure out the path to the root if needed
                        parts = tocEntry.DestinationFile.Split('\\');
                        pathToRoot = String.Empty;

                        for(part = 0; part < parts.Length - 1; part++)
                            pathToRoot += "../";

                        this.ResolveLinksAndCopy(source, baseFolder + filename, tocEntry);
                    }
                    else
                    {
                        this.ReportProgress("{0} -> {1}{2}", source, baseFolder, filename);

                        // All attributes are turned off so that we can delete it later
                        File.Copy(source, baseFolder + filename, true);
                        File.SetAttributes(baseFolder + filename, FileAttributes.Normal);
                    }
                }
            }

            // Remove excluded nodes, merge folder item info into the root
            // nodes, and sort the items.  If a site map isn't defined, this
            // will define the layout of the items.
            toc.RemoveExcludedNodes(null);
            toc.Sort();

            codeColorizer = null;
            sharedContent = sharedBuilderContent = styleContent = null;

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// This is used to merge the conceptual content table of contents with
        /// any additional content table of contents information.
        /// </summary>
        /// <remarks>This will also split the table of contents if any entry
        /// has the "split" option.  A split in the conceptual content will
        /// take precedence as additional content is always appended to
        /// the end of the conceptual content.  Likewise, a default topic in
        /// the conceptual content will take precedence over a default topic
        /// in the additional content.</remarks>
        private void MergeConceptualAndAdditionalContentTocInfo()
        {
            FileItemCollection siteMapFiles;
            List<ITableOfContents> tocFiles;
            TocEntryCollection siteMap, mergedToc;
            TocEntry tocEntry;

            this.ReportProgress(BuildStep.MergeTablesOfContents,
                "Merging conceptual and additional tables of contents...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            // Add the conceptual content layout files
            tocFiles = new List<ITableOfContents>();

            foreach(TopicCollection topics in conceptualContent.Topics)
                tocFiles.Add(topics);

            // Load all site maps and add them to the list
            siteMapFiles = new FileItemCollection(project, BuildAction.SiteMap);

            foreach(FileItem fileItem in siteMapFiles)
            {
                this.ReportProgress("    Loading site map '{0}'", fileItem.FullPath);
                siteMap = new TocEntryCollection(fileItem);
                siteMap.Load();

                // Merge destination file information into the site map
                foreach(TocEntry site in siteMap)
                    this.MergeTocInfo(site);

                tocFiles.Add(siteMap);
            }

            // Sort the files
            tocFiles.Sort((x, y) =>
            {
                FileItem fx = x.ContentLayoutFile, fy = y.ContentLayoutFile;

                if(fx.SortOrder < fy.SortOrder)
                    return -1;

                if(fx.SortOrder > fy.SortOrder)
                    return 1;

                return String.Compare(fx.Name, fy.Name, StringComparison.OrdinalIgnoreCase);
            });

            // Create the merged TOC.  Invisible items are excluded.
            mergedToc = new TocEntryCollection();

            foreach(ITableOfContents file in tocFiles)
                file.GenerateTableOfContents(mergedToc, project, false);

            // If there were no site maps, add items copied from the project.
            // Empty container nodes are ignored.
            if(siteMapFiles.Count == 0 && toc != null && toc.Count != 0)
                foreach(TocEntry t in toc)
                    if(t.DestinationFile != null || t.Children.Count != 0)
                        mergedToc.Add(t);

            toc = mergedToc;

            if(toc.Count != 0)
            {
                // Look for the default topic
                tocEntry = toc.FindDefaultTopic();

                if(tocEntry != null)
                    defaultTopic = tocEntry.DestinationFile;
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// This is used to merge destination file information into the site
        /// map TOC.
        /// </summary>
        /// <param name="site">The site entry to update</param>
        /// <remarks>In addition, files in the site map that do not exist in
        /// the TOC built from the defined content will be processed and
        /// copied to the root folder.</remarks>
        private void MergeTocInfo(TocEntry site)
        {
            TocEntry match;
            string source, filename;

            if(site.SourceFile.Path.Length != 0)
            {
                match = toc.Find(site.SourceFile);

                if(match != null)
                    site.DestinationFile = match.DestinationFile;
                else
                {
                    source = site.SourceFile;
                    site.DestinationFile = Path.GetFileName(source);
                    filename = site.DestinationFile;

                    // .topic files get transformed into .html files
                    if(source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                        site.DestinationFile = Path.ChangeExtension(site.DestinationFile, ".html");

                    // Check to see if anything needs resolving
                    if(source.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
                      source.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                      source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                        match = BuildProcess.GetTocInfo(source);

                    foreach(string baseFolder in this.HelpFormatOutputFolders)
                    {
                        // If the file contains items that need to be resolved, it is handled separately
                        if(match != null && (match.HasLinks || match.HasCodeBlocks ||
                          match.NeedsColorizing || match.HasProjectTags ||
                          source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase)))
                        {
                            // Files are always copied to the root
                            pathToRoot = String.Empty;

                            this.ResolveLinksAndCopy(source, baseFolder + filename, match);
                        }
                        else
                        {
                            this.ReportProgress("{0} -> {1}{2}", source, baseFolder, filename);

                            // All attributes are turned off so that we can delete it later
                            File.Copy(source, baseFolder + filename, true);
                            File.SetAttributes(baseFolder + filename, FileAttributes.Normal);
                        }
                    }
                }
            }

            if(site.Children.Count != 0)
                foreach(TocEntry entry in site.Children)
                    this.MergeTocInfo(entry);
        }

        /// <summary>
        /// This is used to extract table of contents information from a file
        /// that will appear in the help file's table of contents.
        /// </summary>
        /// <param name="filename">The file from which to extract the 
        /// information</param>
        /// <returns>The table of contents entry</returns>
        internal static TocEntry GetTocInfo(string filename)
        {
            TocEntry tocEntry;
            Encoding enc = Encoding.Default;
            string content;

            content = BuildProcess.ReadWithEncoding(filename, ref enc);

            tocEntry = new TocEntry(null);
            tocEntry.IncludePage = !reTocExclude.IsMatch(content);
            tocEntry.IsDefaultTopic = reIsDefaultTopic.IsMatch(content);

            if(reSplitToc.IsMatch(content))
                tocEntry.ApiParentMode = ApiParentMode.InsertAfter;

            Match m = reSortOrder.Match(content);
            if(m.Success)
                tocEntry.SortOrder = Convert.ToInt32(m.Groups["SortOrder"].Value, CultureInfo.InvariantCulture);

            // Get the page title if possible.  If not found, use the filename
            // without the path or extension as the page title.
            m = rePageTitle.Match(content);
            if(!m.Success)
                tocEntry.Title = Path.GetFileNameWithoutExtension(filename);
            else
                tocEntry.Title = HttpUtility.HtmlDecode(m.Groups["Title"].Value).Replace(
                    "\r", String.Empty).Replace("\n", String.Empty);

            // Since we've got the file loaded, see if there are links
            // that need to be resolved when the file is copied, if it
            // contains <pre> blocks that should be colorized, or if it
            // contains tags or shared content items that need replacing.
            tocEntry.HasLinks = reResolveLinks.IsMatch(content);
            tocEntry.HasCodeBlocks = reCodeBlock.IsMatch(content);
            tocEntry.NeedsColorizing = reColorizeCheck.IsMatch(content);
            tocEntry.HasProjectTags = (reProjectTags.IsMatch(content) || reSharedContent.IsMatch(content));

            return tocEntry;
        }

        /// <summary>
        /// This is called to load an additional content file, resolve links
        /// to namespace content and copy it to the output folder.
        /// </summary>
        /// <param name="sourceFile">The source filename to copy</param>
        /// <param name="destFile">The destination filename</param>
        /// <param name="entry">The entry being resolved.</param>
        internal void ResolveLinksAndCopy(string sourceFile, string destFile,
          TocEntry entry)
        {
            Encoding enc = Encoding.Default;
            string content, script, syntaxFile;
            int pos;

            // For topics, change the extenstion back to ".topic".  It's
            // ".html" in the TOC as that's what it ends up as after
            // transformation.
            if(sourceFile.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                destFile = Path.ChangeExtension(destFile, ".topic");

            this.ReportProgress("{0} -> {1}", sourceFile, destFile);

            // When reading the file, use the default encoding but detect the
            // encoding if byte order marks are present.
            content = BuildProcess.ReadWithEncoding(sourceFile, ref enc);

            // Expand <code> tags if necessary
            if(entry.HasCodeBlocks)
                content = reCodeBlock.Replace(content, codeBlockMatchEval);

            // Colorize <pre> tags if necessary
            if(entry.NeedsColorizing || entry.HasCodeBlocks)
            {
                // Initialize code colorizer on first use
                if(codeColorizer == null)
                    codeColorizer = new CodeColorizer(shfbFolder + @"Colorizer\highlight.xml",
                        shfbFolder + @"Colorizer\highlight.xsl");

                // Set the path the "Copy" image
                codeColorizer.CopyImageUrl = pathToRoot + "icons/CopyCode.gif";

                // Colorize it and replace the "Copy" literal text with the
                // shared content include item so that it gets localized.
                content = codeColorizer.ProcessAndHighlightText(content);
                content = content.Replace(codeColorizer.CopyText + "</span",
                    "<include item=\"copyCode\"/></span");
                entry.HasProjectTags = true;

                // Add the links to the colorizer stylesheet and script files
                // unless it's going to be transformed.  In which case, the
                // links should be in the XSL stylesheet.
                if(!sourceFile.EndsWith(".topic", StringComparison.OrdinalIgnoreCase) &&
                  !sourceFile.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase))
                {
                    script = String.Format(CultureInfo.InvariantCulture,
                        "<link type='text/css' rel='stylesheet' href='{0}styles/highlight.css' />" +
                        "<script type='text/javascript' src='{0}scripts/highlight.js'></script>", pathToRoot);

                    pos = content.IndexOf("</head>", StringComparison.Ordinal);

                    // Create a <head> section if one doesn't exist
                    if(pos == -1)
                    {
                        script = "<head>" + script + "</head>";
                        pos = content.IndexOf("<html>", StringComparison.Ordinal);

                        if(pos != -1)
                            pos += 6;
                        else
                            pos = 0;
                    }

                    content = content.Insert(pos, script);
                }

                // Copy the colorizer files if not already there
                this.EnsureOutputFoldersExist("icons");
                this.EnsureOutputFoldersExist("styles");
                this.EnsureOutputFoldersExist("scripts");

                foreach(string baseFolder in this.HelpFormatOutputFolders)
                    if(!File.Exists(baseFolder + @"styles\highlight.css"))
                    {
                        syntaxFile = baseFolder + @"styles\highlight.css";
                        File.Copy(shfbFolder + @"Colorizer\highlight.css", syntaxFile);
                        File.SetAttributes(syntaxFile, FileAttributes.Normal);

                        syntaxFile = baseFolder + @"scripts\highlight.js";
                        File.Copy(shfbFolder + @"Colorizer\highlight.js", syntaxFile);
                        File.SetAttributes(syntaxFile, FileAttributes.Normal);

                        // Always copy the image files, they may be different.  Also, delete the
                        // destination file first if it exists as the filename casing may be different.
                        syntaxFile = baseFolder + @"icons\CopyCode.gif";

                        if(File.Exists(syntaxFile))
                        {
                            File.SetAttributes(syntaxFile, FileAttributes.Normal);
                            File.Delete(syntaxFile);
                        }

                        File.Copy(shfbFolder + @"Colorizer\CopyCode.gif", syntaxFile);
                        File.SetAttributes(syntaxFile, FileAttributes.Normal);

                        syntaxFile = baseFolder + @"icons\CopyCode_h.gif";

                        if(File.Exists(syntaxFile))
                        {
                            File.SetAttributes(syntaxFile, FileAttributes.Normal);
                            File.Delete(syntaxFile);
                        }

                        File.Copy(shfbFolder + @"Colorizer\CopyCode_h.gif", syntaxFile);
                        File.SetAttributes(syntaxFile, FileAttributes.Normal);
                    }
            }

            // Use a regular expression to find and replace all tags with
            // cref attributes with a link to the help file content.  This
            // needs to happen after the code block processing as they
            // may contain <see> tags that need to be resolved.
            if(entry.HasLinks || entry.HasCodeBlocks)
                content = reResolveLinks.Replace(content, linkMatchEval);

            // Replace project option tags with project option values
            if(entry.HasProjectTags)
            {
                // Project tags can be nested
                while(reProjectTags.IsMatch(content))
                    content = reProjectTags.Replace(content, fieldMatchEval);

                // Shared content items can be nested
                while(reSharedContent.IsMatch(content))
                    content = reSharedContent.Replace(content, contentMatchEval);
            }

            // Write the file back out with the appropriate encoding
            using(StreamWriter sw = new StreamWriter(destFile, false, enc))
            {
                sw.Write(content);
            }

            // Transform .topic files into .html files
            if(sourceFile.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                this.XslTransform(destFile);
        }

        /// <summary>
        /// Replace a link to a namespace item with a link to the HTML page
        /// for it.
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnLinkMatch(Match match)
        {
            XmlNodeList elements;
            string tag, preAttrs, link, postAttrs, content, href;

            tag = match.Groups["Tag"].Value;
            preAttrs = match.Groups["PreAttrs"].Value;
            link = match.Groups["Link"].Value;
            postAttrs = match.Groups["PostAttrs"].Value;
            content = match.Groups["Content"].Value;

            // If the tag is "see", change it to an anchor tag ("a")
            if(tag == "see")
            {
                tag = "a";

                // Use the link as the content if no text is specified
                if(String.IsNullOrEmpty(content))
                    content = link;
            }

            // If it looks like we've got a prefix, try for a "starts with"
            // match.  If not, try for a substring match after the prefix.
            // This should give the best results before going more general.
            if(link.IndexOf(':') != -1)
                elements = apisNode.SelectNodes("api[starts-with(@id,'" +
                    link + "')]/file");
            else
                elements = apisNode.SelectNodes(
                    "api[substring-after(@id,':') = '" + link + "']/file");

            // Find all nodes containing the text if the above didn't find
            // anything and use the first one found.
            if(elements.Count == 0)
                elements = apisNode.SelectNodes("api[contains(@id,'" + link +
                    "')]/file");

            // Anything found?
            if(elements.Count == 0)
            {
                this.ReportProgress("\tResolve Links: No matches found " +
                    "for '{0}'", link);

                // If it's an anchor tag, show it as a bold, non-clickable link
                if(tag == "a")
                    return String.Format(CultureInfo.InvariantCulture,
                        "<b>{0}</b>", content);

                // All other tags will use a dummy href value
                href = "#";
            }
            else
            {
                // If one match is found, use it as the href value
                if(elements.Count == 1)
                    this.ReportProgress("\tResolve Links: Matched '{0}' to '{1}'",
                        link, elements[0].ParentNode.Attributes["id"].Value);
                else
                {
                    // If multiple matches are found, issue a warning, dump all
                    // matches, and then use first match as the href value.
                    this.ReportProgress("\tResolve Links: Multiple matches " +
                        "found for '{0}':", link);

                    foreach(XmlNode n in elements)
                        this.ReportProgress("\t\t{0}",
                            n.ParentNode.Attributes["id"].Value);

                    this.ReportProgress("\t\tUsing '{0}' for link",
                        elements[0].ParentNode.Attributes["id"].Value);
                }

                href = String.Format(CultureInfo.InvariantCulture,
                    "{0}html/{1}.htm", pathToRoot,
                    elements[0].Attributes["name"].Value);
            }

            if(!String.IsNullOrEmpty(content))
                return String.Format(CultureInfo.InvariantCulture,
                    "<{0} {1} href=\"{2}\" {3}>{4}</{0}>", tag, preAttrs, href,
                    postAttrs, content);

            return String.Format(CultureInfo.InvariantCulture,
                "<{0} {1} href=\"{2}\" {3}/>", tag, preAttrs, href, postAttrs);
        }

        /// <summary>
        /// Replace a shared content item with it's value.  Note that these
        /// may be nested.
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnContentMatch(Match match)
        {
            XPathDocument contentFile;
            XPathNavigator item;
            string content = String.Empty;

            // Load the shared content files on first use
            if(sharedContent == null)
            {
                contentFile = new XPathDocument(presentationFolder +
                    @"content\shared_content.xml");
                sharedContent = contentFile.CreateNavigator();

                contentFile = new XPathDocument(workingFolder +
                    "SharedBuilderContent.xml");
                sharedBuilderContent = contentFile.CreateNavigator();

                contentFile = new XPathDocument(workingFolder +
                    "PresentationStyleBuilderContent.xml");
                styleContent = contentFile.CreateNavigator();
            }

            // Give preference to the help file builder's shared content files
            item = sharedBuilderContent.SelectSingleNode("content/item[@id='" +
                match.Groups["Item"] + "']");

            if(item == null)
                item = styleContent.SelectSingleNode("content/item[@id='" +
                    match.Groups["Item"] + "']");

            if(item == null)
                item = sharedContent.SelectSingleNode("content/item[@id='" +
                    match.Groups["Item"] + "']");

            if(item != null)
                content = item.InnerXml;

            return content;
        }

        /// <summary>
        /// This is used to load a code block from an external file.
        /// </summary>
        /// <returns>The HTML encoded block extracted from the file and
        /// wrapped in a &lt;pre&gt; tag ready for colorizing.</returns>
        /// <remarks>If a region attribute is found, only the named region
        /// is returned.  If n region attribute is found, the whole file is
        /// returned.  Relative paths are assumed to be relative to the
        /// project folder.</remarks>
        private string OnCodeBlockMatch(Match match)
        {
            Regex reFindRegion;
            Match find, m;

            string sourceFile = null, region = null, codeBlock = null,
                options = match.Groups[1].Value + match.Groups[3].Value;

            sourceFile = match.Groups[2].Value;

            try
            {
                sourceFile = Environment.ExpandEnvironmentVariables(sourceFile);

                if(!Path.IsPathRooted(sourceFile))
                    sourceFile = Path.GetFullPath(projectFolder + sourceFile);

                using(StreamReader sr = new StreamReader(sourceFile))
                {
                    codeBlock = sr.ReadToEnd();
                }
            }
            catch(ArgumentException argEx)
            {
                throw new BuilderException("BE0013", String.Format(
                    CultureInfo.InvariantCulture, "Possible invalid path " +
                    "'{0}{1}'.", projectFolder, sourceFile), argEx);
            }
            catch(IOException ioEx)
            {
                throw new BuilderException("BE0014", String.Format(
                    CultureInfo.InvariantCulture, "Unable to load source " +
                    "file '{0}'.", sourceFile), ioEx);
            }

            // If no region is specified, the whole file is included
            m = reCodeRegion.Match(options);

            if(m.Success)
            {
                region = m.Groups[1].Value;
                options = options.Remove(m.Index, m.Length);

                // Find the start of the region.  This gives us an immediate
                // starting match on the second search and we can look for the
                // matching #endregion without caring about the region name.
                // Otherwise, nested regions get in the way and complicate
                // things.
                reFindRegion = new Regex("\\#(pragma\\s+)?region\\s+\"?" +
                    Regex.Escape(region), RegexOptions.IgnoreCase);

                find = reFindRegion.Match(codeBlock);

                if(!find.Success)
                    throw new BuilderException("BE0015", String.Format(
                        CultureInfo.InvariantCulture, "Unable to locate " +
                        "start of region '{0}' in source file '{1}'", region,
                        sourceFile));

                // Find the end of the region taking into account any
                // nested regions.
                m = reMatchRegion.Match(codeBlock, find.Index);

                if(!m.Success)
                    throw new BuilderException("BE0016", String.Format(
                        CultureInfo.InvariantCulture, "Unable to extract " +
                        "region '{0}' in source file '{1}{2}' (missing " +
                        "#endregion?)", region, projectFolder, sourceFile));

                // Extract just the specified region starting after the
                // description.
                codeBlock = m.Groups[2].Value.Substring(
                    m.Groups[2].Value.IndexOf('\n') + 1);

                // Strip off the trailing comment characters if present
                if(codeBlock[codeBlock.Length - 1] == ' ')
                    codeBlock = codeBlock.TrimEnd();

                // VB commented #End Region statement within a method body
                if(codeBlock[codeBlock.Length - 1] == '\'')
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 1);

                // XML/XAML commented #endregion statement
                if(codeBlock.EndsWith("<!--", StringComparison.Ordinal))
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 4);

                // C or SQL style commented #endregion statement
                if(codeBlock.EndsWith("/*", StringComparison.Ordinal) ||
                  codeBlock.EndsWith("--", StringComparison.Ordinal))
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 2);
            }

            // Remove nested region markers?
            if(reWillRemoveMarkers.IsMatch(options))
            {
                codeBlock = reRemoveRegionMarkers.Replace(codeBlock, String.Empty);
                codeBlock = codeBlock.Replace("\r\n\n", "\r\n");
            }

            // If nested, don't wrap it in a <pre> tag
            if(reIsNested.IsMatch(options))
                return codeBlock;

            // Return the HTML encoded block
            return "<pre xml:space=\"preserve\" " + options + ">" + HttpUtility.HtmlEncode(
                codeBlock) + "</pre>";
        }

        /// <summary>
        /// This is used to transform a *.topic file into a *.html file using
        /// an XSLT transformation based on the presentation style.
        /// </summary>
        /// <param name="sourceFile">The source topic filename</param>
        private void XslTransform(string sourceFile)
        {
            TocEntry tocInfo;
            XmlReader reader = null;
            XmlWriter writer = null;
            XsltSettings settings;
            XmlReaderSettings readerSettings;
            XmlWriterSettings writerSettings;
            Encoding enc = Encoding.Default;
            FileItemCollection transforms;
            string content;

            string sourceStylesheet, destFile = Path.ChangeExtension(sourceFile, ".html");

            try
            {
                readerSettings = new XmlReaderSettings();
                readerSettings.CloseInput = true;
                readerSettings.DtdProcessing = DtdProcessing.Parse;

                // Create the transform on first use
                if(xslTransform == null)
                {
                    transforms = new FileItemCollection(project, BuildAction.TopicTransform);

                    if(transforms.Count != 0)
                    {
                        if(transforms.Count > 1)
                            this.ReportWarning("BE0011", "Multiple topic " +
                                "transformations found.  Using '{0}'",
                                transforms[0].FullPath);

                        sourceStylesheet = transforms[0].FullPath;
                    }
                    else
                        sourceStylesheet = templateFolder + presentationParam + ".xsl";

                    xslStylesheet = workingFolder + Path.GetFileName(sourceStylesheet);
                    tocInfo = BuildProcess.GetTocInfo(sourceStylesheet);

                    // The stylesheet may contain shared content items so we
                    // must resolve it this way rather than using
                    // TransformTemplate.
                    this.ResolveLinksAndCopy(sourceStylesheet, xslStylesheet, tocInfo);

                    xslTransform = new XslCompiledTransform();
                    settings = new XsltSettings(true, true);
                    xslArguments = new XsltArgumentList();

                    xslTransform.Load(XmlReader.Create(xslStylesheet,
                        readerSettings), settings, new XmlUrlResolver());
                }

                this.ReportProgress("Applying XSL transformation '{0}' to '{1}'.", xslStylesheet, sourceFile);

                reader = XmlReader.Create(sourceFile, readerSettings);
                writerSettings = xslTransform.OutputSettings.Clone();
                writerSettings.CloseOutput = true;
                writerSettings.Indent = false;

                writer = XmlWriter.Create(destFile, writerSettings);

                xslArguments.Clear();
                xslArguments.AddParam("pathToRoot", String.Empty, pathToRoot);
                xslTransform.Transform(reader, xslArguments, writer);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0017", String.Format(
                    CultureInfo.InvariantCulture, "Unexpected error " +
                    "using '{0}' to transform additional content file '{1}' " +
                    "to '{2}'.  The error is: {3}\r\n{4}", xslStylesheet,
                    sourceFile, destFile, ex.Message,
                    (ex.InnerException == null) ? String.Empty :
                        ex.InnerException.Message));
            }
            finally
            {
                if(reader != null)
                    reader.Close();

                if(writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }
            }

            // The source topic file is deleted as the transformed file
            // takes its place.
            File.Delete(sourceFile);

            // <span> and <script> tags cannot be self-closing if empty.
            // The template may contain them correctly but when written out
            // as XML, they get converted to self-closing tags which breaks
            // them.  To fix them, convert them to full start and close tags.
            content = BuildProcess.ReadWithEncoding(destFile, ref enc);
            content = reSpanScript.Replace(content, "<$1$2></$1>");

            // An XSL transform might have added tags and include items that
            // need replacing so run it through those options if needed.
            tocInfo = BuildProcess.GetTocInfo(destFile);

            // Expand <code> tags if necessary
            if(tocInfo.HasCodeBlocks)
                content = reCodeBlock.Replace(content, codeBlockMatchEval);

            // Colorize <pre> tags if necessary
            if(tocInfo.NeedsColorizing || tocInfo.HasCodeBlocks)
            {
                // Initialize code colorizer on first use
                if(codeColorizer == null)
                    codeColorizer = new CodeColorizer(shfbFolder + @"Colorizer\highlight.xml",
                        shfbFolder + @"Colorizer\highlight.xsl");

                // Set the path the "Copy" image
                codeColorizer.CopyImageUrl = pathToRoot + "icons/CopyCode.gif";

                // Colorize it and replace the "Copy" literal text with the
                // shared content include item so that it gets localized.
                content = codeColorizer.ProcessAndHighlightText(content);
                content = content.Replace(codeColorizer.CopyText + "</span",
                    "<include item=\"copyCode\"/></span");
                tocInfo.HasProjectTags = true;
            }

            // Use a regular expression to find and replace all tags with
            // cref attributes with a link to the help file content.  This
            // needs to happen after the code block processing as they
            // may contain <see> tags that need to be resolved.
            if(tocInfo.HasLinks || tocInfo.HasCodeBlocks)
                content = reResolveLinks.Replace(content, linkMatchEval);

            // Replace project option tags with project option values
            if(tocInfo.HasProjectTags)
            {
                // Project tags can be nested
                while(reProjectTags.IsMatch(content))
                    content = reProjectTags.Replace(content, fieldMatchEval);

                // Shared content items can be nested
                while(reSharedContent.IsMatch(content))
                    content = reSharedContent.Replace(content, contentMatchEval);
            }

            // Write the file back out with the appropriate encoding
            using(StreamWriter sw = new StreamWriter(destFile, false, enc))
            {
                sw.Write(content);
            }
        }
    }
}
