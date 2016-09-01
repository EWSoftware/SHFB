//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ProjectFileSearcher.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/01/2016
// Note    : Copyright 2014-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to search for an open files related to MAML link-type elements such as for
// topics, images, code references, and tokens.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to search for an open files related to MAML link-type elements such as for topics,
    /// images, code references, and tokens.
    /// </summary>
    internal class ProjectFileSearcher
    {
        #region Private data members
        //=====================================================================

        private SVsServiceProvider serviceProvider;
        private ITextView textView;
        private Solution currentSolution;
        private List<MSBuildProject> shfbProjects;

        internal static string tokenId;

        #endregion

        #region ID type enumeration
        //=====================================================================

        /// <summary>
        /// This defines the valid ID types that are related to file types that can be searched for and opened
        /// </summary>
        public enum IdType
        {
            /// <summary>
            /// An unknown ID type
            /// </summary>
            Unknown,
            /// <summary>
            /// A code snippet reference
            /// </summary>
            CodeReference,
            /// <summary>
            /// An image file reference
            /// </summary>
            Image,
            /// <summary>
            /// A topic link
            /// </summary>
            Link,
            /// <summary>
            /// A token reference
            /// </summary>
            Token,
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">The service provider used to get the object manager</param>
        /// <param name="textView">The current text view used to determine the current project.  If null, the
        /// active project is determined by using the DTE object which may not be correct (the active project may
        /// not contain the current document).  This is only used to try and figure out which project to search
        /// first.</param>
        public ProjectFileSearcher(SVsServiceProvider serviceProvider, ITextView textView)
        {
            this.serviceProvider = serviceProvider;
            this.textView = textView;
            shfbProjects = new List<MSBuildProject>();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Search for and go to the definition of the given member ID
        /// </summary>
        /// <param name="idType">The ID type</param>
        /// <param name="id">The ID of the item for which to open a file</param>
        /// <returns>True if successful, false if not</returns>
        public bool OpenFileFor(IdType idType, string id)
        {
            try
            {
                if(!this.DetermineCurrentSolutionAndProjects())
                    return false;

                // Topic IDs must be matched to files so a separate cache is maintained for them
                if(idType == IdType.Link)
                {
                    // Ignore the request if still indexing
                    if(TopicIdCache.Instance.IsIndexingTopics)
                        return true;

                    string topicTitle, topicFilename, relativePath, anchor = null;

                    // Remove anchor name references.  We'll try to select it in the opened file.
                    if(id.IndexOf('#') != -1)
                    {
                        anchor = "address=\"" + id.Substring(id.IndexOf('#') + 1) + "\"";
                        id = id.Substring(0, id.IndexOf('#'));
                    }

                    if(!TopicIdCache.Instance.GetTopicInfo(id, out topicTitle, out topicFilename, out relativePath))
                    {
                        // Not found, try re-indexing to update the info
                        return TopicIdCache.Instance.SetCurrentSolutionAndProjects(currentSolution.FullName, shfbProjects);
                    }

                    var item = currentSolution.FindProjectItem(topicFilename);

                    if(item != null)
                    {
                        var window = item.Open();

                        if(window != null)
                        {
                            window.Activate();

                            if(!String.IsNullOrWhiteSpace(anchor))
                            {
                                var selection = window.Selection as TextSelection;

                                if(selection != null)
                                    selection.FindText(anchor);
                            }

                            return true;
                        }
                    }

                    return false;
                }

                // All other files are few an are searched for when requested
                foreach(var currentProject in shfbProjects)
                    switch(idType)
                    {
                        case IdType.CodeReference:
                            if(this.OpenCodeSnippetFile(currentProject, id))
                                return true;
                            break;

                        case IdType.Image:
                            var image = currentProject.GetItems("Image").FirstOrDefault(pi =>
                                pi.Metadata.Any(m => m.Name == "ImageId" &&
                                    m.EvaluatedValue.Equals(id, StringComparison.OrdinalIgnoreCase)));

                            if(image != null)
                            {
                                var item = currentSolution.FindProjectItem(image.EvaluatedInclude);

                                if(item != null)
                                {
                                    var window = item.Open();

                                    if(window != null)
                                    {
                                        window.Activate();
                                        return true;
                                    }
                                }
                            }
                            break;

                        case IdType.Token:
                            if(this.OpenTokenFile(currentProject, id))
                                return true;

                            break;

                        default:    // Unknown
                            break;
                    }
            }
            catch(Exception ex)
            {
                // Ignore exceptions, we'll just fail the search
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return false;
        }

        /// <summary>
        /// Search for and return the title and filename for the given ID
        /// </summary>
        /// <param name="idType">The ID type.</param>
        /// <param name="id">The ID of the item for which to get information.</param>
        /// <param name="title">On return, contains the title of the item if found.</param>
        /// <param name="filename">On return, contains the filename of the item if found.</param>
        /// <param name="relativePath">On return, contains the topic file path relative to its project folder.
        /// This can be used for display as it is typically much shorter than the full path.</param>
        /// <returns>True if successful, false if not</returns>
        public bool GetInfoFor(IdType idType, string id, out string title, out string filename,
          out string relativePath)
        {
            title = filename = relativePath = "(Not found)";

            try
            {
                if(!this.DetermineCurrentSolutionAndProjects())
                {
                    filename = relativePath = "Unable to determine current solution and/or projects";
                    return false;
                }

                // Topic IDs must be matched to files so a separate cache is maintained for them
                if(idType == IdType.Link)
                {
                    // Remove anchor name reference
                    if(id.IndexOf('#') != -1)
                        id = id.Substring(0, id.IndexOf('#'));

                    bool found = TopicIdCache.Instance.GetTopicInfo(id, out title, out filename, out relativePath);

                    // Tell the user if still indexing.  We may not have a filename yet.
                    if(TopicIdCache.Instance.IsIndexingTopics)
                    {
                        filename = relativePath = "Topic ID cache is being built.  Try again in a few seconds.";
                        found = false;
                    }
                    else
                        if(!found)
                        {
                            // Not found, try re-indexing to update the info
                            TopicIdCache.Instance.SetCurrentSolutionAndProjects(currentSolution.FullName, shfbProjects);

                            // If it exists, we should at least have a title at this point so go get it
                            TopicIdCache.Instance.GetTopicInfo(id, out title, out filename, out relativePath);

                            filename = relativePath = "Topic ID cache is being built.  Try again in a few seconds.";
                        }

                    return found;
                }

                if(idType == IdType.Image)
                    foreach(var currentProject in shfbProjects)
                    {
                        var image = currentProject.GetItems("Image").FirstOrDefault(pi =>
                            pi.Metadata.Any(m => m.Name == "ImageId" &&
                                m.EvaluatedValue.Equals(id, StringComparison.OrdinalIgnoreCase)));

                        if(image != null)
                        {
                            var altText = image.Metadata.FirstOrDefault(m => m.Name == "AlternateText");

                            if(altText != null && !String.IsNullOrWhiteSpace(altText.EvaluatedValue))
                                title = altText.EvaluatedValue;
                            else
                                title = "(Not set)";

                            filename = relativePath = image.EvaluatedInclude;
                            return true;
                        }
                    }
            }
            catch(Exception ex)
            {
                // Ignore exceptions, we'll just fail the search
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return false;
        }


        /// <summary>
        /// This is used to determine the current solution and the SHFB projects it contains
        /// </summary>
        /// <returns>True if determined, false if not</returns>
        private bool DetermineCurrentSolutionAndProjects()
        {
            ITextDocument document;

            // Determine the current project as it will be searched first
            if(textView != null && textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(
              typeof(ITextDocument), out document))
            {
                if(document != null && document.TextBuffer != null)
                {
                    var dte2 = this.serviceProvider.GetService(typeof(SDTE)) as DTE2;

                    if(dte2 != null && dte2.Solution != null)
                    {
                        currentSolution = dte2.Solution;
                        var prjItem = dte2.Solution.FindProjectItem(document.FilePath);

                        // Seems rather odd but there's no way to get to the underlying MSBuild project through
                        // any of the Visual Studio interfaces.  As such, we have to resort to using the
                        // global project collection to find it by name.  We could use the COM objects to search
                        // for the files but that's much more convoluted as the items are nested.  Using the
                        // MSBuild project is much simpler.
                        if(prjItem != null && prjItem.ContainingProject != null)
                        {
                            var currentProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(
                                prjItem.ContainingProject.FullName).FirstOrDefault(
                                    p => p.FullPath == prjItem.ContainingProject.FullName);

                            if(currentProject != null &&
                              currentProject.FullPath.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase))
                                shfbProjects.Add(currentProject);
                        }
                    }
                }
            }
            else
            {
                    var dte2 = this.serviceProvider.GetService(typeof(SDTE)) as DTE2;

                    if(dte2 != null && dte2.Solution != null)
                    {
                        currentSolution = dte2.Solution;

                        var activeSolutionProjects = dte2.ActiveSolutionProjects as Array;

                        if(activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                        {
                            var activeProject = activeSolutionProjects.GetValue(0) as EnvDTE.Project;

                            var currentProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(
                                activeProject.FullName).FirstOrDefault(p => p.FullPath == activeProject.FullName);

                            if(currentProject != null &&
                              currentProject.FullPath.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase))
                                shfbProjects.Add(currentProject);
                        }
                    }
            }

            // Add all other SHFB projects in the solution excluding the current project if it is one
            if(currentSolution != null)
                foreach(var p in ProjectCollection.GlobalProjectCollection.LoadedProjects)
                    if(p.FullPath.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase) &&
                      !shfbProjects.Any(sp => sp.FullPath.Equals(p.FullPath, StringComparison.OrdinalIgnoreCase)))
                        shfbProjects.Add(p);

            if(currentSolution != null && TopicIdCache.Instance.CurrentSolutionName != currentSolution.FullName)
                TopicIdCache.Instance.SetCurrentSolutionAndProjects(currentSolution.FullName, shfbProjects);

            return (currentSolution != null && shfbProjects.Count != 0);
        }

        /// <summary>
        /// Open a code snippet file by finding the one containing the given ID
        /// </summary>
        /// <param name="currentProject">The current project used to find files</param>
        /// <param name="id">The ID of the snippet to search for in the code snippet files</param>
        /// <returns>True if opened, false if not</returns>
        private bool OpenCodeSnippetFile(MSBuildProject currentProject, string id)
        {
            string projectPath = Path.GetDirectoryName(currentProject.FullPath), filePath;

            int pos = id.IndexOf(',');

            // If it's a combined ID, find the first one
            if(pos != -1)
                id = id.Substring(0, pos);

            foreach(var snippetFile in currentProject.GetItems("CodeSnippets"))
            {
                var link = snippetFile.Metadata.FirstOrDefault(m => m.Name == "Link");

                if(link == null)
                    filePath = Path.Combine(projectPath, snippetFile.EvaluatedInclude);
                else
                    filePath = Path.Combine(projectPath, link.EvaluatedValue);

                if(File.Exists(filePath))
                {
                    // The file content may not be current if the file is open for editing but I can't be
                    // bothered to add the code to go look for the open editor and get it from there yet.
                    var doc = XDocument.Load(filePath);

                    if(doc.Descendants("item").Any(t => t.Attribute("id").Value == id))
                    {
                        var item = currentSolution.FindProjectItem(snippetFile.EvaluatedInclude);

                        if(item != null)
                        {
                            var window = item.Open();

                            if(window != null)
                            {
                                window.Activate();

                                var selection = window.Selection as TextSelection;

                                if(selection != null)
                                    selection.FindText(id);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Open a token file by finding the one containing the given ID
        /// </summary>
        /// <param name="currentProject">The current project used to find files</param>
        /// <param name="id">The ID of the token to search for in the token files</param>
        /// <returns>True if opened, false if not</returns>
        private bool OpenTokenFile(MSBuildProject currentProject, string id)
        {
            string projectPath = Path.GetDirectoryName(currentProject.FullPath), filePath;

            foreach(var contentLayoutFile in currentProject.GetItems("Tokens"))
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

                    if(doc.Descendants("item").Any(t => t.Attribute("id").Value == id))
                    {
                        var item = currentSolution.FindProjectItem(contentLayoutFile.EvaluatedInclude);

                        if(item != null)
                        {
                            // Bit of a hack.  The token editor is a custom editor and I couldn't find a way
                            // to get to it from the window.  So, set the ID and the token editor pane will
                            // select this as the default token when the editor is opened.  However, if already
                            // open, it won't have any effect.
                            try
                            {
                                tokenId = id;

                                var window = item.Open();

                                if(window != null)
                                {
                                    window.Activate();
                                    return true;
                                }
                            }
                            finally
                            {
                                tokenId = null;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
