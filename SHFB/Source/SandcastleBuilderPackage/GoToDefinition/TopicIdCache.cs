//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TopicIdCache.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/01/2016
// Note    : Copyright 2014-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to cache information about MAML topic IDs and their related files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
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

using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace SandcastleBuilder.Package.GoToDefinition
{
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

        private static TopicIdCache instance;

        private ConcurrentDictionary<string, TopicInfo> topicInfo;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the singleton instance
        /// </summary>
        public static TopicIdCache Instance
        {
            get
            {
                if(instance == null)
                    instance = new TopicIdCache();

                return instance;
            }
        }

        /// <summary>
        /// This read-only property is used to get the current solution name for which the cache has loaded
        /// topic information.
        /// </summary>
        /// <value>If the returned name does not match the current solution, call <see cref="SetCurrentSolutionAndProjects"/>
        /// to refresh the cache.</value>
        public string CurrentSolutionName { get; private set; }

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
        /// <param name="solutionName">The current solution filename</param>
        /// <param name="projects">The current list of projects</param>
        /// <returns>True if re-indexing was initiated, false if not</returns>
        public bool SetCurrentSolutionAndProjects(string solutionName, IEnumerable<MSBuildProject> projects)
        {
            // If the solution changes, clear all existing topic information
            if(this.CurrentSolutionName != solutionName)
            {
                this.CurrentSolutionName = solutionName;
                topicInfo.Clear();
            }

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
                        // The file content may not be current if the file is open for editing but I can't be
                        // bothered to add the code to go look for the open editor and get it from there yet.
                        var doc = XDocument.Load(filePath);

                        // Add or refresh the title information
                        foreach(var topic in doc.Descendants("Topic"))
                        {
                            TopicInfo info;

                            if(!topicInfo.TryGetValue(topic.Attribute("id").Value, out info))
                                info = new TopicInfo
                                {
                                    TopicId = topic.Attribute("id").Value,
                                };

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
                this.MatchFilesToTopics(projects.Select(p => Path.GetDirectoryName(p.FullPath)).Distinct());
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is used to get the information for the specified topic ID
        /// </summary>
        /// <param name="id">The ID to look up.</param>
        /// <param name="title">On return, contains the topic title if found.</param>
        /// <param name="filename">On return, contains the topic filename if found.</param>
        /// <param name="relativePath">On return, contains the topic file path relative to its project folder.
        /// This can be used for display as it is typically much shorter than the full path.</param>
        /// <returns>True if the topic ID was found in the cache, false if not</returns>
        public bool GetTopicInfo(string id, out string title, out string filename, out string relativePath)
        {
            TopicInfo info;

            if(topicInfo.TryGetValue(id, out info))
            {
                title = info.Title;
                filename = info.Filename;
                relativePath = info.RelativePath;

                return true;
            }

            title = filename = relativePath = "(Not found)";

            return false;
        }

        /// <summary>
        /// Kick off the task that matches topic files to the information loaded from the content layout files
        /// </summary>
        /// <param name="folders">The folders to search for MAML topic (.aml) files</param>
        private void MatchFilesToTopics(IEnumerable<string> folders)
        {
            this.IsIndexingTopics = true;

            Task.Factory.StartNew(() => this.IndexTopics(folders));
        }

        /// <summary>
        /// Search the given set of folders for MAML topics and match them up to the information loaded from the
        /// content layout files.
        /// </summary>
        /// <param name="folders">The folders to search for MAML topic (.aml) files</param>
        private void IndexTopics(IEnumerable<string> folders)
        {
            TopicInfo info;

            foreach(string folder in folders)
                try
                {
                    // We could scan the projects for the files but passing the project folders is less info to
                    // gather when calling this and the Project class doesn't indicate that it is thread-safe.
                    foreach(string file in Directory.EnumerateFiles(folder, "*.aml", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var doc = XDocument.Load(file);

                            string id = (string)doc.Root.Attribute("id") ?? String.Empty;

                            if(topicInfo.TryGetValue(id, out info))
                            {
                                info.Filename = file;
                                info.RelativePath = file.Substring(folder.Length + 1);

                                if(info.Title == "(No title)")
                                    info.Title = Path.GetFileNameWithoutExtension(file);
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

            this.IsIndexingTopics = false;
        }
        #endregion
    }
}
