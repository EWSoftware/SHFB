//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TopicIdCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/07/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to cache information about MAML topic IDs and their related files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/06/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Sandcastle.Core.Markdown;

using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This class is used to cache information about MAML topic IDs and their related files
/// </summary>
/// <remarks>This class is a singleton</remarks>
internal class TopicIdCache
{
    #region Topic info class
    //=====================================================================

    /// <summary>
    /// This class is used to hold information about the topics
    /// </summary>
    internal class TopicInfo
    {
        /// <summary>
        /// This is used to get or set the topic ID
        /// </summary>
        public string TopicId { get; set; }

        /// <summary>
        /// This is used to get or set the topic title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// This is used to get or set the related topic file from the project
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// This is used to get or set the related topic file's relative path
        /// </summary>
        /// <value>This omit's the project path and makes it shorter and more convenient for display</value>
        public string RelativePath { get; set; }

    }
    #endregion

    #region Private data members
    //=====================================================================

    private readonly ConcurrentDictionary<string, TopicInfo> topicInfo;

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property is used to get the singleton instance
    /// </summary>
    public static TopicIdCache Instance { get; } = new TopicIdCache();

    /// <summary>
    /// This read-only property is used to see if the topics are currently being indexed
    /// </summary>
    /// <value>When true, topic information may not be current and should not be used</value>
    public bool IsIndexingTopics { get; private set; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Private constructor
    /// </summary>
    private TopicIdCache()
    {
        topicInfo = new ConcurrentDictionary<string, TopicInfo>(StringComparer.OrdinalIgnoreCase);
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <summary>
    /// This is used to set the current solution name and the projects
    /// </summary>
    /// <param name="projects">The current list of projects</param>
    /// <returns>True if re-indexing was initiated, false if not</returns>
    public bool IndexConceptualContentTopics(IEnumerable<MSBuildProject> projects)
    {
        if(this.IsIndexingTopics)
            return false;

        topicInfo.Clear();

        // Index the content layout files in each project.  This should happen quickly as there usually
        // aren't that many content layout files.
        foreach(var project in projects)
        {
            string projectPath = Path.GetDirectoryName(project.FullPath), filePath;

            foreach(var contentLayoutFile in project.GetItems("ContentLayout"))
            {
                var link = contentLayoutFile.Metadata.FirstOrDefault(m => m.Name == "Link");

                if(link == null)
                    filePath = Path.Combine(projectPath, contentLayoutFile.EvaluatedInclude);
                else
                    filePath = Path.Combine(projectPath, link.EvaluatedValue);

                if(File.Exists(filePath))
                {
                    // The file content may not be current if the file is open for editing but since we're
                    // running in the background, we can't get that info anyway.
                    var doc = XDocument.Load(filePath);

                    // Add or refresh the title information
                    foreach(var topic in doc.Descendants("Topic"))
                    {
                        if(!topicInfo.TryGetValue(topic.Attribute("id").Value, out TopicInfo info))
                        {
                            info = new TopicInfo
                            {
                                TopicId = topic.Attribute("id").Value,
                            };
                        }

                        info.Title = (string)topic.Attribute("title") ?? info.Title ?? "(No title)";

                        topicInfo.AddOrUpdate(info.TopicId, info, (key, value) => value);
                    }
                }
            }
        }

        // If any new topics are found, match the ID to actual files.  This is done in the background as
        // large projects may have hundreds of topics.  This may not be necessary but it saves blocking the
        // IDE while it does it just in case.
        if(topicInfo.Any(t => t.Value.Filename == null))
        {
            this.IsIndexingTopics = true;

            // Fire and forget
            Task.Run(() => this.IndexTopics(projects.Select(p => Path.GetDirectoryName(p.FullPath)).Distinct()));

            return true;
        }

        return false;
    }

    /// <summary>
    /// This is used to get the information for the specified topic ID
    /// </summary>
    /// <param name="id">The ID to look up.</param>
    /// <returns>Returns a tuple containing a flag indicating true if found in the cache along with the title of
    /// the topic, the filename, and the topic file path relative to its project folder.  This can be used for
    /// display as it is typically much shorter than the full path.  If not found, it returns false along with
    /// a "Not found" message in the other values.</returns>
    public (bool Found, string Title, string Filename, string RelativePath) GetTopicInfo(string id)
    {
        if(topicInfo.TryGetValue(id, out TopicInfo info))
            return (true, info.Title, info.Filename, info.RelativePath);

        return (false, "(Not found)", "(Not found)", "(Not found)");
    }

    /// <summary>
    /// Search the given set of folders for MAML topics and match them up to the information loaded from the
    /// content layout files.
    /// </summary>
    /// <param name="folders">The folders to search for MAML topic (.aml) files</param>
    private void IndexTopics(IEnumerable<string> folders)
    {
        foreach(string folder in folders)
        {
            try
            {
                // We could scan the projects for the files but passing the project folders is less info to
                // gather when calling this and the Project class doesn't indicate that it is thread-safe.
                foreach(string file in Directory.EnumerateFiles(folder, "*.aml", SearchOption.AllDirectories).Concat(
                  Directory.EnumerateFiles(folder, "*.md", SearchOption.AllDirectories)))
                {
                    try
                    {
                        string id, title = null;

                        if(Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase))
                        {
                            var md = new MarkdownFile(file);
                            id = md.UniqueId;
                            title = md.Title;
                        }
                        else
                        {
                            var doc = XDocument.Load(file);
                            id = (string)doc.Root.Attribute("id") ?? String.Empty;
                        }

                        if(topicInfo.TryGetValue(id, out TopicInfo info))
                        {
                            info.Filename = file;
                            info.RelativePath = file.Substring(folder.Length + 1);

                            if(info.Title == "(No title)")
                                info.Title = title ?? Path.GetFileNameWithoutExtension(file);
                        }
                    }
                    catch(Exception ex)
                    {
                        // Ignore exceptions, we just won't be able to get info for the file
                        System.Diagnostics.Debug.WriteLine("File: {0}\r\n{1}", file, ex);
                    }
                }
            }
            catch(Exception ex)
            {
                // Ignore exceptions, we just won't be able to get info for the file
                System.Diagnostics.Debug.WriteLine("Folder: {0}\r\n{1}", folder, ex);
            }
        }

        this.IsIndexingTopics = false;
    }
    #endregion
}
