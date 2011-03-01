//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TocEntryCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the table of contents
// entries for additional content items.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.0.0  09/17/2006  EFW  Created the code
// 1.5.0.2  07/03/2007  EFW  Added support for saving as a site map file
// 1.8.0.0  08/11/2008  EFW  Modified to support the new project format
// 1.9.0.0  06/15/2010  EFW  Added support for MS Help Viewer TOC format
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This collection class is used to hold the table of contents entries for
    /// additional content items.
    /// </summary>
    public class TocEntryCollection : BindingList<TocEntry>, ITableOfContents
    {
        #region Private data members
        //=====================================================================

        private FileItem siteMapFile;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the project file item associated
        /// with the collection.
        /// </summary>
        public FileItem FileItem
        {
            get { return siteMapFile; }
        }

        /// <summary>
        /// This is used to get the default topic
        /// </summary>
        /// <value>It returns the default topic or null if one is not set</value>
        public TocEntry DefaultTopic
        {
            get
            {
                foreach(TocEntry t in this)
                {
                    if(t.IsDefaultTopic)
                        return t;

                    var defaultTopic = t.Children.DefaultTopic;

                    if(defaultTopic != null)
                        return defaultTopic;
                }

                return null;
            }
        }

        /// <summary>
        /// This is used to get the topic at which the API table of contents is
        /// to be inserted or parented.
        /// </summary>
        /// <value>This will return null if no parent location has been set</value>
        public TocEntry ApiContentInsertionPoint
        {
            get
            {
                foreach(TocEntry t in this)
                {
                    if(t.ApiParentMode != ApiParentMode.None)
                        return t;

                    var childInsertionPoint = t.Children.ApiContentInsertionPoint;

                    if(childInsertionPoint != null)
                        return childInsertionPoint;
                }

                return null;
            }
        }

        /// <summary>
        /// This is used to get the parent item that will contain the API table
        /// of contents.
        /// </summary>
        /// <returns>The parent item or null if it is the root collection.</returns>
        public TocEntry ApiContentParent
        {
            get
            {
                TocEntry apiParent;

                foreach(TocEntry t in this)
                {
                    if(t.Children.Any(c => c.ApiParentMode != ApiParentMode.None))
                        return t;

                    apiParent = t.Children.ApiContentParent;

                    if(apiParent != null)
                        return apiParent;
                }

                return null;
            }
        }

        /// <summary>
        /// This is used to get the parent collection that contains the item
        /// where the API table of contents is to be inserted.
        /// </summary>
        /// <returns>The parent collection if there is a location defined or
        /// null if there isn't one.</returns>
        public TocEntryCollection ApiContentParentCollection
        {
            get
            {
                TocEntryCollection apiParent;

                foreach(TocEntry t in this)
                {
                    if(t.ApiParentMode != ApiParentMode.None)
                        return this;

                    apiParent = t.Children.ApiContentParentCollection;

                    if(apiParent != null)
                        return apiParent;
                }

                return null;
            }
        }

        /// <summary>
        /// This can be used to get a topic by its unique ID (case-insensitive)
        /// </summary>
        /// <param name="id">The ID of the item to get.</param>
        /// <value>Returns the topic with the specified
        /// <see cref="TocEntry.Id" /> or null if not found.</value>
        public TocEntry this[string id]
        {
            get
            {
                TocEntry found;

                foreach(TocEntry t in this)
                {
                    if(String.Compare(t.Id, id, StringComparison.OrdinalIgnoreCase) == 0)
                        return t;

                    found = t.Children[id];

                    if(found != null)
                        return found;
                }

                return null;
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public TocEntryCollection()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteMap">The site map file associated with the
        /// collection.</param>
        public TocEntryCollection(FileItem siteMap)
        {
            siteMapFile = siteMap;
        }
        #endregion

        #region Sort the collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>All top level items and their children are sorted</remarks>
        public void Sort()
        {
            ((List<TocEntry>)this.Items).Sort((x, y) => Comparer<TocEntry>.Default.Compare(x, y));

            foreach(TocEntry te in this)
                te.Children.Sort();
        }
        #endregion

        #region ToString methods
        //=====================================================================

        /// <summary>
        /// Convert the table of contents entry and its children to a string
        /// </summary>
        /// <returns>The entries in HTML 1.x help format</returns>
        public override string ToString()
        {
            return this.ToString(HelpFileFormat.HtmlHelp1);
        }

        /// <summary>
        /// Convert the table of contents entry and its children to a string
        /// in the specified help file format.
        /// </summary>
        /// <param name="format">The help file format to use</param>
        /// <returns>The entries in the specified help format</returns>
        public string ToString(HelpFileFormat format)
        {
            StringBuilder sb = new StringBuilder(1024);
            this.ConvertToString(format, sb);
            return sb.ToString();
        }

        /// <summary>
        /// This is used to convert the collection to a string and append it
        /// to the specified string builder.
        /// </summary>
        /// <param name="format">The help file format to use</param>
        /// <param name="sb">The string builder to which the information is
        /// appended.</param>
        internal void ConvertToString(HelpFileFormat format, StringBuilder sb)
        {
            foreach(TocEntry te in this)
                te.ConvertToString(format, sb);
        }
        #endregion

        #region Save as intermediate TOC file for conceptual content builds
        //=====================================================================

        /// <summary>
        /// This is used to save the TOC information to an intermediate TOC
        /// file used in the conceptual content build.
        /// </summary>
        /// <param name="rootContainerId">The ID of the root container topic if used</param>
        /// <param name="rootOrder">The TOC order for the root container topic if used</param>
        /// <param name="filename">The filename to use</param>
        public void SaveToIntermediateTocFile(string rootContainerId, int rootOrder, string filename)
        {
            using(StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw.WriteLine("<topics>");

                if(!String.IsNullOrEmpty(rootContainerId))
                    sw.WriteLine("<topic id=\"{0}\" file=\"{0}\" sortOrder=\"{1}\">",
                        rootContainerId, rootOrder);

                sw.WriteLine(this.ToString(HelpFileFormat.MSHelpViewer));

                if(!String.IsNullOrEmpty(rootContainerId))
                    sw.WriteLine("</topic>");

                sw.WriteLine("</topics>");
            }
        }
        #endregion

        #region Site map file methods
        //=====================================================================

        /// <summary>
        /// This is used to locate the default topic if one exists
        /// </summary>
        /// <returns>The default topic if found or null if not found</returns>
        /// <remarks>The first entry found is returned.  Nodes are searched
        /// recursively.</remarks>
        public TocEntry FindDefaultTopic()
        {
            TocEntry defaultTopic;

            foreach(TocEntry t in this)
            {
                if(t.IsDefaultTopic)
                    return t;

                if(t.Children.Count != 0)
                {
                    defaultTopic = t.Children.FindDefaultTopic();

                    if(defaultTopic != null)
                        return defaultTopic;
                }
            }

            return null;
        }

        /// <summary>
        /// This will remove excluded nodes from a TOC created off of the
        /// additional content items in a project.  In addition, it merges
        /// the information from folder entries into the folder nodes.
        /// </summary>
        /// <param name="parent">The parent node</param>
        public void RemoveExcludedNodes(TocEntry parent)
        {
            TocEntry current;
            string source;

            for(int idx = 0; idx < this.Count; idx++)
            {
                current = this[idx];
                source = current.SourceFile;

                if(parent != null && !String.IsNullOrEmpty(source))
                {
                    // If the filename matches the folder, copy the info to
                    // the parent folder item.
                    source = source.ToLower(CultureInfo.InvariantCulture);

                    if(Path.GetDirectoryName(source).EndsWith(
                      Path.GetFileNameWithoutExtension(source),
                      StringComparison.Ordinal))
                    {
                        parent.Title = current.Title;

                        if(current.IncludePage)
                        {
                            parent.SourceFile = current.SourceFile;
                            parent.DestinationFile = current.DestinationFile;
                            parent.IsDefaultTopic = current.IsDefaultTopic;
                        }

                        parent.SortOrder = current.SortOrder;
                        parent.ApiParentMode = current.ApiParentMode;
                        current.IncludePage = false;
                    }
                }

                // If not to be included in the TOC, remove it
                if(!current.IncludePage)
                {
                    this.Remove(current);
                    idx--;
                }
                else                        
                    if(current.Children.Count != 0)
                        current.Children.RemoveExcludedNodes(current);
            }
        }

        /// <summary>
        /// This is used to load the table of contents entries from the site
        /// map file associated with the collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if a
        /// site map has not been associated with the collection.</exception>
        public void Load()
        {
            if(siteMapFile == null)
                throw new InvalidOperationException("A site map has not " +
                    "been associated with the collection");

            XmlDocument siteMap = new XmlDocument();
            TocEntry entry;

            siteMap.Load(siteMapFile.FullPath);

            foreach(XmlNode site in siteMap.ChildNodes[1].ChildNodes)
            {
                entry = new TocEntry(siteMapFile.ProjectElement.Project);
                entry.LoadSiteMapNode(site);
                base.Add(entry);
            }
        }

        /// <summary>
        /// This is used to save the table of contents entries to the site map
        /// file associated with the collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if a
        /// site map has not been associated with the collection.</exception>
        public void Save()
        {
            if(siteMapFile == null)
                throw new InvalidOperationException("A site map has not " +
                    "been associated with the collection");

            XmlDocument siteMap = new XmlDocument();

            siteMap.AppendChild(siteMap.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlNode root = siteMap.CreateNode(XmlNodeType.Element, "siteMap",
                "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");

            siteMap.AppendChild(root);

            foreach(TocEntry te in this)
                te.SaveAsSiteMapNode(root);

            siteMap.Save(siteMapFile.FullPath);
        }

        /// <summary>
        /// Find a TOC entry with the same source filename
        /// </summary>
        /// <param name="sourceFilename">The source filename to match</param>
        /// <returns>The match TOC entry or null if not found</returns>
        public TocEntry Find(string sourceFilename)
        {
            TocEntry match = null;

            foreach(TocEntry entry in this)
            {
                match = entry.ContainsMatch(sourceFilename);

                if(match != null)
                    break;
            }

            return match;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Reset the sort order on all items in the collection
        /// </summary>
        public void ResetSortOrder()
        {
            foreach(TocEntry t in this)
                t.ResetSortOrder();
        }

        /// <summary>
        /// This is used by contained items to notify the parent that a child
        /// list changed and thus the collection should be marked as dirty.
        /// </summary>
        /// <param name="changedItem">The item that changed</param>
        internal void ChildListChanged(TocEntry changedItem)
        {
            this.OnListChanged(new ListChangedEventArgs(
                ListChangedType.ItemChanged, this.IndexOf(changedItem)));
        }
        #endregion

        #region Add all topic files from a folder
        //=====================================================================

        /// <summary>
        /// Add all topics from the specified folder recursively to the
        /// collection and to the given project file.
        /// </summary>
        /// <param name="folder">The folder from which to get the files</param>
        /// <param name="basePath">The base path to remove from files copied
        /// from another folder into the project folder.  On the first call,
        /// this should match the <paramref name="folder"/> value.</param>
        /// <param name="project">The project to which the files are added</param>
        /// <remarks>Only actual HTML content topic files are added.  They must
        /// have a ".htm?" extension.  Folders will be added as sub-topics
        /// recursively.  If a file with the same name as the folder exists,
        /// it will be associated with the container node.  If no such file
        /// exists, an empty container node is created.</remarks>
        public void AddTopicsFromFolder(string folder, string basePath, SandcastleProject project)
        {
            TocEntry topic, removeTopic;
            string name, newPath, projectPath = Path.GetDirectoryName(project.Filename);

            if(basePath.Length != 0 && basePath[basePath.Length - 1] != '\\')
                basePath += "\\";

            // Add files
            foreach(string file in Directory.EnumerateFiles(folder, "*.htm?"))
            {
                // The file must reside under the project path
                if(Path.GetDirectoryName(file).StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                    newPath = file;
                else
                    newPath = Path.Combine(projectPath, file.Substring(basePath.Length));

                // Add the file to the project
                project.AddFileToProject(file, newPath);
                topic = new TocEntry(project);
                topic.SourceFile = new FilePath(newPath, project);
                topic.Title = Path.GetFileNameWithoutExtension(newPath);
                this.Add(topic);
            }

            // Add folders recursively
            foreach(string folderName in Directory.EnumerateDirectories(folder))
            {
                topic = new TocEntry(project);
                topic.Title = name = Path.GetFileName(folderName);
                topic.Children.AddTopicsFromFolder(folderName, basePath, project);

                // Ignore empty folders
                if(topic.Children.Count == 0)
                    continue;

                this.Add(topic);

                // Look for a file with the same name as the folder
                removeTopic = null;

                foreach(TocEntry t in topic.Children)
                    if(Path.GetFileNameWithoutExtension(t.SourceFile) == name)
                    {
                        // If found, remove it as it represents the container
                        // node.
                        topic.Title = name;
                        topic.SourceFile = t.SourceFile;
                        removeTopic = t;
                        break;
                    }

                if(removeTopic != null)
                    topic.Children.Remove(removeTopic);
            }
        }
        #endregion

        #region Overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to set the inserted item's parent to this
        /// collection.
        /// </summary>
        /// <inheritdoc />
        protected override void InsertItem(int index, TocEntry item)
        {
            base.InsertItem(index, item);
            item.Parent = this;
        }

        /// <summary>
        /// This is overridden to set the inserted item's parent to this
        /// collection.
        /// </summary>
        /// <inheritdoc />
        protected override void SetItem(int index, TocEntry item)
        {
            base.SetItem(index, item);
            item.Parent = this;
        }

        /// <summary>
        /// This is overridden to clear the parent on the removed item
        /// </summary>
        /// <param name="index">The index of the item to remove</param>
        protected override void RemoveItem(int index)
        {
            TocEntry item = this[index];
            item.Parent = null;
            base.RemoveItem(index);
        }
        #endregion

        #region ITableOfContents implementation
        //=====================================================================

        /// <summary>
        /// This is used to get the site map file associated with the
        /// collection.
        /// </summary>
        public FileItem ContentLayoutFile
        {
            get { return siteMapFile; }
        }

        /// <summary>
        /// This is used to merge this TOC with another one
        /// </summary>
        /// <param name="toc">The table of contents collection</param>
        /// <param name="pathProvider">The base path provider</param>
        public void GenerateTableOfContents(TocEntryCollection toc,
          IBasePathProvider pathProvider)
        {
            foreach(TocEntry t in this)
                toc.Add(t);
        }
        #endregion
    }
}
