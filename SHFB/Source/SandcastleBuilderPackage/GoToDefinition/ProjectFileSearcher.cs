//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ProjectFileSearcher.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/12/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
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

using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This class is used to search for an open files related to MAML link-type elements such as for topics,
/// images, code references, and tokens.
/// </summary>
internal class ProjectFileSearcher
{
    #region Private data members
    //=====================================================================

    private readonly SVsServiceProvider serviceProvider;
    private readonly List<MSBuildProject> shfbProjects;

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
    /// <param name="serviceProvider">The service provider used to get the current solution or null if not needed</param>
    public ProjectFileSearcher(SVsServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        shfbProjects = [];

        foreach(var p in ProjectCollection.GlobalProjectCollection.LoadedProjects)
        {
            if(p.FullPath.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase) &&
              !shfbProjects.Any(sp => sp.FullPath.Equals(p.FullPath, StringComparison.OrdinalIgnoreCase)))
            {
                shfbProjects.Add(p);
            }
        }
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
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            // Topic IDs must be matched to files so a separate cache is maintained for them
            if(idType == IdType.Link)
            {
                // Ignore the request if still indexing
                if(TopicIdCache.Instance.IsIndexingTopics)
                    return true;

                string anchor = null;

                // Remove anchor name references.  We'll try to select it in the opened file.
                if(id.IndexOf('#') != -1)
                {
                    anchor = "address=\"" + id.Substring(id.IndexOf('#') + 1) + "\"";
                    id = id.Substring(0, id.IndexOf('#'));
                }

                var (found, _, topicFilename, _) = TopicIdCache.Instance.GetTopicInfo(id);

                // If not found, try re-indexing to update the info
                if(!found)
                    return TopicIdCache.Instance.IndexConceptualContentTopics(shfbProjects);

                if(this.serviceProvider.GetService(typeof(SDTE)) is not DTE2 dte2 || dte2.Solution == null)
                    return false;

                var item = dte2.Solution.FindProjectItem(topicFilename);

                if(item != null)
                {
                    var window = item.Open();

                    if(window != null)
                    {
                        window.Activate();

                        if(!String.IsNullOrWhiteSpace(anchor))
                        {
                            if(window.Selection is TextSelection selection)
                                selection.FindText(anchor);
                        }

                        return true;
                    }
                }

                return false;
            }

            // All other files are few an are searched for when requested
            foreach(var currentProject in shfbProjects)
            {
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
                            if(this.serviceProvider.GetService(typeof(SDTE)) is not DTE2 dte2 || dte2.Solution == null)
                                return false;

                            var item = dte2.Solution.FindProjectItem(image.EvaluatedInclude);

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
    /// <returns>Returns a tuple containing a flag indicating true if successful along with the title of the item,
    /// the filename, and the topic file path relative to its project folder.  This can be used for display as it
    /// is typically much shorter than the full path.  If not found, it returns false along with messages in the
    /// title, filename, and relative path indicating the reason.</returns>
    public (bool Found, string Title, string Filename, string RelativePath) GetInfoForId(IdType idType, string id)
    {
        string title, filename, relativePath;
        bool found = false;

        title = filename = relativePath = "(Not found)";

        try
        {
            // Topic IDs must be matched to files so a separate cache is maintained for them
            if(idType == IdType.Link)
            {
                // Remove anchor name reference
                if(id.IndexOf('#') != -1)
                    id = id.Substring(0, id.IndexOf('#'));

                (found, title, filename, relativePath) = TopicIdCache.Instance.GetTopicInfo(id);

                // Tell the user if still indexing.  We may not have a filename yet but may have a title.
                if(TopicIdCache.Instance.IsIndexingTopics)
                {
                    filename = relativePath = "Topic ID cache is being built.  Try again in a few seconds.";
                    found = false;
                }
                else
                {
                    if(!found)
                    {
                        // Not found, try re-indexing to update the info
                        TopicIdCache.Instance.IndexConceptualContentTopics(shfbProjects);

                        // If it exists, we should at least have a title at this point so go get it
                        (_, title, _, _) = TopicIdCache.Instance.GetTopicInfo(id);

                        filename = relativePath = "Topic ID cache is being built.  Try again in a few seconds.";
                    }
                }

                return (found, title, filename, relativePath);
            }

            if(idType == IdType.Image)
            {
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

                        return (true, title, filename, relativePath);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            // Ignore exceptions, we'll just fail the search
            System.Diagnostics.Debug.WriteLine(ex);
        }

        return (false, title, filename, relativePath);
    }

    /// <summary>
    /// Open a code snippet file by finding the one containing the given ID
    /// </summary>
    /// <param name="currentProject">The current project used to find files</param>
    /// <param name="id">The ID of the snippet to search for in the code snippet files</param>
    /// <returns>True if opened, false if not</returns>
    private bool OpenCodeSnippetFile(MSBuildProject currentProject, string id)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

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
                    if(this.serviceProvider.GetService(typeof(SDTE)) is not DTE2 dte2 || dte2.Solution == null)
                        return false;

                    var item = dte2.Solution.FindProjectItem(snippetFile.EvaluatedInclude);

                    if(item != null)
                    {
                        var window = item.Open();

                        if(window != null)
                        {
                            window.Activate();

                            if(window.Selection is TextSelection selection)
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
        ThreadHelper.ThrowIfNotOnUIThread();

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
                    if(this.serviceProvider.GetService(typeof(SDTE)) is not DTE2 dte2 || dte2.Solution == null)
                        return false;

                    var item = dte2.Solution.FindProjectItem(contentLayoutFile.EvaluatedInclude);

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
