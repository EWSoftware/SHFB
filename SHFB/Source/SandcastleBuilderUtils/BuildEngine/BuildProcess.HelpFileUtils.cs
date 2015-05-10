//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.HelpFileUtils.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/05/2015
// Note    : Copyright 2006-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to modify the help file project files to create a better table of contents
// and find the default help file page
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/07/2006  EFW  Created the code
// 09/06/2006  EFW  Added support for TOC content placement
// 09/09/2006  EFW  Added support for website output
// 10/02/2006  EFW  Added support for the September CTP
// 11/04/2006  EFW  Added support for the NamingMethod property
// 06/19/2007  EFW  Various additions and updates for the June CTP
// 09/13/2007  EFW  Added support for calling plug-ins
// 02/04/2008  EFW  Adjusted loading of Help 1 TOC to use an encoding based on the chosen language
// 04/12/2007  EFW  Added support for a split table of contents
// 06/06/2010  EFW  Added support for multi-format build output
// 06/30/2010  EFW  Reworked TOC handling to support parenting of API content to a conceptual topic for all
//                  formats.
// 02/19/2012  EFW  Added support for PHP website files.  Merged changes for VS2010 style from Don Fehr.
// 10/25/2012  EFW  Updated to use the new presentation style definition files
// 06/21/2013  EFW  Added support for format-specific help content files.  Removed
//                  ModifyHelpTopicFilenames() as naming is now handled entirely by AddFilenames.xsl.
// 01/09/2013  EFW  Removed copying of branding files.  They are part of the presentation style now.
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

using Sandcastle.Core;

using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        // The table of contents entries for the additional and conceptual content
        private TocEntryCollection toc;

        #endregion

        #region Table of content helper methods
        //=====================================================================

        /// <summary>
        /// This is used to determine the best placement for the API content based on the project settings
        /// </summary>
        private void DetermineApiContentPlacement()
        {
            TocEntryCollection parentCollection;
            TocEntry apiInsertionPoint, parentTopic;
            XmlDocument tocXml;
            int tocOrder = project.TocOrder;

            this.ApiTocParentId = null;
            this.ApiTocOrder = -1;

            tocXml = new XmlDocument();
            tocXml.Load(workingFolder + "toc.xml");

            XmlNodeList topics = tocXml.SelectNodes("topics/topic");

            // Note that in all cases, we only have to set the order on the item where it changes.
            // The sort order on all subsequent items will increment from there.
            if(toc != null && toc.Count != 0)
            {
                toc.ResetSortOrder();

                // See if a root content container is defined for MSHV output
                var root = this.ConceptualContent.Topics.FirstOrDefault(t => t.MSHVRootContentContainer != null);

                if(root != null)
                    this.RootContentContainerId = root.MSHVRootContentContainer.Id;

                if(tocOrder == -1 || root != null)
                    tocOrder = 0;

                // If building MS Help Viewer output, ensure that the root container ID is valid
                // and not visible in the TOC if defined.
                if((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0 &&
                  !String.IsNullOrEmpty(this.RootContentContainerId) && toc[this.RootContentContainerId] != null)
                    throw new BuilderException("BE0069", String.Format(CultureInfo.CurrentCulture,
                        "The project's root content container topic (ID={0}) must be have its Visible property " +
                        "set to False in the content layout file.", this.RootContentContainerId));

                // Was an insertion point defined in the content layout?
                apiInsertionPoint = toc.ApiContentInsertionPoint;

                if(apiInsertionPoint != null)
                {
                    parentCollection = toc.ApiContentParentCollection;
                    toc[0].SortOrder = tocOrder;

                    // Insert the API content before, after, or as a child of a conceptual topic
                    switch(apiInsertionPoint.ApiParentMode)
                    {
                        case ApiParentMode.InsertBefore:
                            parentTopic = toc.ApiContentParent;

                            if(parentTopic == null)
                            {
                                this.ApiTocParentId = "**Root**";
                                this.ApiTocOrder = tocOrder + parentCollection.IndexOf(apiInsertionPoint);
                            }
                            else
                            {
                                this.ApiTocParentId = toc.ApiContentParent.Id;
                                this.ApiTocOrder = parentCollection.IndexOf(apiInsertionPoint);
                            }

                            apiInsertionPoint.SortOrder = this.ApiTocOrder + topics.Count;
                            break;

                        case ApiParentMode.InsertAfter:
                            parentTopic = toc.ApiContentParent;
                            this.ApiTocOrder = parentCollection.IndexOf(apiInsertionPoint) + 1;

                            if(parentTopic == null)
                            {
                                this.ApiTocParentId = "**Root**";

                                if(this.ApiTocOrder < parentCollection.Count)
                                    parentCollection[this.ApiTocOrder].SortOrder = tocOrder + this.ApiTocOrder + topics.Count;

                                this.ApiTocOrder += tocOrder;
                            }
                            else
                            {
                                this.ApiTocParentId = parentTopic.Id;

                                if(this.ApiTocOrder < parentCollection.Count)
                                    parentCollection[this.ApiTocOrder].SortOrder = this.ApiTocOrder + topics.Count;
                            }
                            break;

                        case ApiParentMode.InsertAsChild:
                            this.ApiTocParentId = apiInsertionPoint.Id;
                            this.ApiTocOrder = parentCollection.Count;
                            break;

                        default:    // Unknown
                            break;
                    }
                }
                else
                {
                    // Base the API sort order on the ContentPlacement property
                    if(project.ContentPlacement == ContentPlacement.AboveNamespaces)
                    {
                        toc[0].SortOrder = tocOrder;
                        this.ApiTocOrder = tocOrder + toc.Count;
                    }
                    else
                    {
                        this.ApiTocOrder = tocOrder;
                        toc[0].SortOrder = tocOrder + topics.Count;
                    }
                }
            }
            else
                this.ApiTocOrder = project.TocOrder;

            // Set the sort order on the first API topic if defined
            if(this.ApiTocOrder != -1 && topics.Count != 0)
            {
                XmlAttribute attr = tocXml.CreateAttribute("sortOrder");
                attr.Value = this.ApiTocOrder.ToString(CultureInfo.InvariantCulture);
                topics[0].Attributes.Append(attr);

                tocXml.Save(workingFolder + "toc.xml");
            }
        }

        /// <summary>
        /// This is used to merge the conceptual content table of contents with any additional content table of
        /// contents information.
        /// </summary>
        private void MergeConceptualAndAdditionalContentTocInfo()
        {
            List<ITableOfContents> tocFiles;
            TocEntryCollection siteMap;
            TocEntry tocEntry;

            this.ReportProgress(BuildStep.MergeTablesOfContents,
                "Merging conceptual and additional tables of contents...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            // Add the conceptual content layout files
            tocFiles = new List<ITableOfContents>();

            foreach(TopicCollection topics in this.ConceptualContent.Topics)
                tocFiles.Add(topics);

            // Load all site maps and add them to the list
            foreach(var contentFile in project.ContentFiles(BuildAction.SiteMap))
            {
                this.ReportProgress("    Loading site map '{0}'", contentFile.FullPath);
                siteMap = new TocEntryCollection(contentFile);
                siteMap.Load();

                // Copy site map files to the help format folders
                foreach(TocEntry site in siteMap)
                    this.CopySiteMapFiles(site);

                tocFiles.Add(siteMap);
            }

            // Sort the files
            tocFiles.Sort((x, y) =>
            {
                ContentFile fx = x.ContentLayoutFile, fy = y.ContentLayoutFile;

                if(fx.SortOrder < fy.SortOrder)
                    return -1;

                if(fx.SortOrder > fy.SortOrder)
                    return 1;

                return String.Compare(fx.Filename, fy.Filename, StringComparison.OrdinalIgnoreCase);
            });

            // Create the merged TOC.  Invisible items are excluded.
            toc = new TocEntryCollection();

            foreach(ITableOfContents file in tocFiles)
                file.GenerateTableOfContents(toc, false);

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
        /// This is used to copy site map files to the help format output folders including those for any child
        /// site map entries.
        /// </summary>
        /// <param name="site">The site entry containing the files to copy</param>
        private void CopySiteMapFiles(TocEntry site)
        {
            if(site.SourceFile.Path.Length != 0)
            {
                // Set the destination filename which will always match the source filename for site map files.
                site.DestinationFile = site.SourceFile.PersistablePath;

                foreach(string baseFolder in this.HelpFormatOutputFolders)
                    if(!File.Exists(baseFolder + site.DestinationFile))
                    {
                        this.ReportProgress("{0} -> {1}{2}", site.SourceFile, baseFolder, site.DestinationFile);

                        // All attributes are turned off so that we can delete it later
                        File.Copy(site.SourceFile, baseFolder + site.DestinationFile, true);
                        File.SetAttributes(baseFolder + site.DestinationFile, FileAttributes.Normal);
                    }
            }

            if(site.Children.Count != 0)
                foreach(TocEntry entry in site.Children)
                    this.CopySiteMapFiles(entry);
        }

        /// <summary>
        /// This combines the conceptual and API intermediate TOC files into one file ready for transformation to
        /// the help format-specific TOC file formats and, if necessary, determines the default topic.
        /// </summary>
        private void CombineIntermediateTocFiles()
        {
            XmlAttribute attr;
            XmlDocument conceptualXml = null, tocXml;
            XmlElement docElement;
            XmlNodeList allNodes;
            XmlNode node, parent;
            int insertionPoint;

            this.ReportProgress(BuildStep.CombiningIntermediateTocFiles,
                "Combining conceptual and API intermediate TOC files...");

            if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
            {
                this.ExecutePlugIns(ExecutionBehaviors.Before);

                // Load the TOC files
                if(toc != null && toc.Count != 0)
                {
                    conceptualXml = new XmlDocument();
                    conceptualXml.Load(workingFolder + "_ConceptualTOC_.xml");
                }

                tocXml = new XmlDocument();
                tocXml.Load(workingFolder + "toc.xml");

                // Merge the conceptual and API TOCs into one?
                if(conceptualXml != null)
                {
                    // Remove the root content container if present as we don't need it for the other formats
                    if((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0 &&
                      !String.IsNullOrEmpty(this.RootContentContainerId))
                    {
                        docElement = conceptualXml.DocumentElement;
                        node = docElement.FirstChild;
                        allNodes = node.SelectNodes("topic");

                        foreach(XmlNode n in allNodes)
                        {
                            n.ParentNode.RemoveChild(n);
                            docElement.AppendChild(n);
                        }

                        node.ParentNode.RemoveChild(node);
                    }

                    if(String.IsNullOrEmpty(this.ApiTocParentId))
                    {
                        // If not parented, the API content is placed above or below the conceptual content based
                        // on the project's ContentPlacement setting.
                        if(project.ContentPlacement == ContentPlacement.AboveNamespaces)
                        {
                            docElement = conceptualXml.DocumentElement;

                            foreach(XmlNode n in tocXml.SelectNodes("topics/topic"))
                            {
                                node = conceptualXml.ImportNode(n, true);
                                docElement.AppendChild(node);
                            }

                            tocXml = conceptualXml;
                        }
                        else
                        {
                            docElement = tocXml.DocumentElement;

                            foreach(XmlNode n in conceptualXml.SelectNodes("topics/topic"))
                            {
                                node = tocXml.ImportNode(n, true);
                                docElement.AppendChild(node);
                            }
                        }
                    }
                    else
                    {
                        // Parent the API content to a conceptual topic
                        parent = conceptualXml.SelectSingleNode("//topic[@id='" + this.ApiTocParentId + "']");

                        // If not found, parent it to the root
                        if(parent == null)
                            parent = conceptualXml.DocumentElement;

                        insertionPoint = this.ApiTocOrder;

                        if(insertionPoint == -1 || insertionPoint >= parent.ChildNodes.Count)
                            insertionPoint = parent.ChildNodes.Count;

                        foreach(XmlNode n in tocXml.SelectNodes("topics/topic"))
                        {
                            node = conceptualXml.ImportNode(n, true);

                            if(insertionPoint >= parent.ChildNodes.Count)
                                parent.AppendChild(node);
                            else
                                parent.InsertBefore(node, parent.ChildNodes[insertionPoint]);

                            insertionPoint++;
                        }

                        tocXml = conceptualXml;
                    }

                    // Fix up empty container nodes by removing the file attribute and setting the ID attribute to
                    // the title attribute value.
                    foreach(XmlNode n in tocXml.SelectNodes("//topic[@title]"))
                    {
                        attr = n.Attributes["file"];

                        if(attr != null)
                            n.Attributes.Remove(attr);

                        attr = n.Attributes["id"];

                        if(attr != null)
                            attr.Value = n.Attributes["title"].Value;
                    }

                    tocXml.Save(workingFolder + "toc.xml");
                }

                this.ExecutePlugIns(ExecutionBehaviors.After);
            }

            // Determine the default topic for Help 1, website, and markdown output if one was not specified in a
            // site map or content layout file.
            if(defaultTopic == null && (project.HelpFileFormat & (HelpFileFormats.HtmlHelp1 |
              HelpFileFormats.Website | HelpFileFormats.Markdown)) != 0)
            {
                var defTopic = ComponentUtilities.XmlStreamAxis(workingFolder + "toc.xml", "topic").FirstOrDefault(
                    t => t.Attribute("file") != null);

                if(defTopic != null)
                {
                    // Find the file.  Could be .htm, .html, or .md so just look for any file with the given name.
                    defaultTopic = Directory.EnumerateFiles(workingFolder + "Output",
                        defTopic.Attribute("file").Value + ".*", SearchOption.AllDirectories).FirstOrDefault();

                    if(defaultTopic != null)
                    {
                        defaultTopic = defaultTopic.Substring(workingFolder.Length + 7);

                        if(defaultTopic.IndexOf('\\') != -1)
                            defaultTopic = defaultTopic.Substring(defaultTopic.IndexOf('\\') + 1);
                    }
                }

                // This shouldn't happen anymore, but just in case...
                if(defaultTopic == null)
                    throw new BuilderException("BE0026", "Unable to determine default topic in toc.xml.  Mark " +
                        "one as the default topic manually in the content layout file.");
            }
        }
        #endregion

        #region Content copying methods
        //=====================================================================

        /// <summary>
        /// This is called to copy the additional content files to the help format content folders
        /// </summary>
        private void CopyAdditionalContent()
        {
            string projectPath, source, filename, dirName;

            this.ReportProgress(BuildStep.CopyAdditionalContent, "Copying additional content files...");

            if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
            {
                // Plug-ins might add or remove additional content so call them before checking to see if there
                // is anything to copy.
                this.ExecutePlugIns(ExecutionBehaviors.Before);

                if(!project.HasItems(BuildAction.Content))
                    this.ReportProgress("No additional content to copy");
                else
                {
                    // Now copy the content files
                    projectPath = FolderPath.TerminatePath(Path.GetDirectoryName(originalProjectName));

                    foreach(var fileItem in project.ContentFiles(BuildAction.Content))
                    {
                        source = fileItem.FullPath;
                        dirName = Path.GetDirectoryName(fileItem.LinkPath.Substring(projectPath.Length));
                        filename = Path.Combine(dirName, Path.GetFileName(source));

                        this.EnsureOutputFoldersExist(dirName);

                        foreach(string baseFolder in this.HelpFormatOutputFolders)
                        {
                            this.ReportProgress("{0} -> {1}{2}", source, baseFolder, filename);

                            // All attributes are turned off so that we can delete it later
                            File.Copy(source, baseFolder + filename, true);
                            File.SetAttributes(baseFolder + filename, FileAttributes.Normal);
                        }
                    }
                }

                this.ExecutePlugIns(ExecutionBehaviors.After);
            }
        }

        /// <summary>
        /// This is called to copy the standard content files (icons, scripts, style sheets, and other standard
        /// presentation style content) to the help output folders.
        /// </summary>
        /// <remarks>This creates the base folder <strong>Output\</strong> and one folder for each help file
        /// format.  It then copies the stock icon, script, and style sheet files from the defined presentation
        /// style help content folders.</remarks>
        private void CopyStandardHelpContent()
        {
            int idx = 0;

            this.ReportProgress(BuildStep.CopyStandardHelpContent, "Copying standard help content...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);
            this.EnsureOutputFoldersExist(null);

            foreach(HelpFileFormats value in Enum.GetValues(typeof(HelpFileFormats)))
                if((project.HelpFileFormat & value) != 0)
                {
                    // EnsureOutputFoldersExist adds the folders to HelpFormatOutputFolders in the same order as
                    // the values so we can index it here.
                    presentationStyle.CopyHelpContent(value, this.HelpFormatOutputFolders[idx],
                        this.ReportProgress, (name, source, dest) => substitutionTags.TransformTemplate(name, source, dest));
                    idx++;
                }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// This copies files from the specified source folder to the specified destination folder.  If any
        /// subfolders are found below the source folder and the wildcard is "*.*", the subfolders are also
        /// copied recursively.
        /// </summary>
        /// <param name="sourcePath">The source path from which to copy</param>
        /// <param name="destPath">The destination path to which to copy</param>
        /// <param name="fileCount">A reference to the file count variable</param>
        private void RecursiveCopy(string sourcePath, string destPath, ref int fileCount)
        {
            if(sourcePath == null)
                throw new ArgumentNullException("sourcePath");

            if(destPath == null)
                throw new ArgumentNullException("destPath");

            int idx = sourcePath.LastIndexOf('\\');

            string dirName = sourcePath.Substring(0, idx), fileSpec = sourcePath.Substring(idx + 1), filename;

            foreach(string name in Directory.EnumerateFiles(dirName, fileSpec))
            {
                filename = destPath + Path.GetFileName(name);

                if(!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                // All attributes are turned off so that we can delete it later
                File.Copy(name, filename, true);
                File.SetAttributes(filename, FileAttributes.Normal);

                fileCount++;

                if((fileCount % 500) == 0)
                    this.ReportProgress("Copied {0} files", fileCount);
            }

            // For "*.*", copy subfolders too
            if(fileSpec == "*.*")
            {
                // Ignore hidden folders as they may be under source control and are not wanted
                foreach(string folder in Directory.EnumerateDirectories(dirName))
                    if((File.GetAttributes(folder) & FileAttributes.Hidden) != FileAttributes.Hidden)
                        this.RecursiveCopy(folder + @"\*.*", destPath + folder.Substring(dirName.Length + 1) + @"\",
                            ref fileCount);
            }
        }
        #endregion

        #region Other stuff
        //=====================================================================

        /// <summary>
        /// This is used to generate the website helper files and copy the output to the project output folder
        /// ready for use as a website.
        /// </summary>
        private void GenerateWebsite()
        {
            string webWorkingFolder = String.Format(CultureInfo.InvariantCulture, "{0}Output\\{1}",
                workingFolder, HelpFileFormats.Website);
            int fileCount = 0;

            // Generate the full-text index for the ASP.NET search option
            this.ReportProgress(BuildStep.GenerateFullTextIndex, "Generating full-text index for the website...\r\n");

            if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
            {
                this.ExecutePlugIns(ExecutionBehaviors.Before);

                FullTextIndex index = new FullTextIndex(workingFolder + "StopWordList.txt", language);
                index.CreateFullTextIndex(webWorkingFolder);
                index.SaveIndex(webWorkingFolder + @"\fti\");

                this.ExecutePlugIns(ExecutionBehaviors.After);
            }

            this.ReportProgress(BuildStep.CopyingWebsiteFiles, "Copying website files to output folder...\r\n");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            // Copy the TOC, keyword index, index pages, and tree view stuff
            File.Copy(workingFolder + "WebTOC.xml", outputFolder + "WebTOC.xml");
            File.Copy(workingFolder + "WebKI.xml", outputFolder + "WebKI.xml");

            // Copy the help pages and related content
            this.RecursiveCopy(webWorkingFolder + @"\*.*", outputFolder, ref fileCount);
            this.ReportProgress("Copied {0} files for the website content", fileCount);

            this.GatherBuildOutputFilenames();
            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// This is used to ensure that all output folders exist based on the selected help file format(s)
        /// </summary>
        /// <param name="subFolder">The subfolder name or null to ensure that the base folders exist.</param>
        /// <remarks>This creates the named folder under the help format specific folder beneath the
        /// <strong>.\Output</strong> folder.</remarks>
        public void EnsureOutputFoldersExist(string subFolder)
        {
            if(this.HelpFormatOutputFolders.Count == 0)
            {
                foreach(HelpFileFormats value in Enum.GetValues(typeof(HelpFileFormats)))
                    if((project.HelpFileFormat & value) != 0)
                        this.HelpFormatOutputFolders.Add(String.Format(CultureInfo.InvariantCulture,
                            @"{0}Output\{1}\", workingFolder, value));
            }

            foreach(string baseFolder in this.HelpFormatOutputFolders)
                if(!Directory.Exists(baseFolder + subFolder))
                    Directory.CreateDirectory(baseFolder + subFolder);
        }
        #endregion
    }
}
