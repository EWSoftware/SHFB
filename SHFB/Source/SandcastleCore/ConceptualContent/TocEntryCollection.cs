//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TocEntryCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/08/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the table of contents entries for content layout and site
// map files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/17/2006  EFW  Created the code
// 07/03/2007  EFW  Added support for saving as a site map file
// 08/11/2008  EFW  Modified to support the new project format
// 06/15/2010  EFW  Added support for MS Help Viewer TOC format
// 12/20/2011  EFW  Updated for use with the new content layout editor
// 05/07/2015  EFW  Removed all deprecated code
//===============================================================================================================

// Ignore Spelling: utf

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

using Sandcastle.Core.Project;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This collection class is used to hold the table of contents entries for content layout and site map files
    /// </summary>
    public class TocEntryCollection : BindingList<TocEntry>, ITableOfContents
    {
        #region Private data members
        //=====================================================================

        private readonly ContentFile siteMapFile;

        #endregion

        #region Properties
        //=====================================================================

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
        /// This is used to get the topic at which the API table of contents is to be inserted or parented
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
        /// This is used to get the parent item that will contain the API table of contents
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
        /// This is used to get the parent collection that contains the item where the API table of contents is
        /// to be inserted.
        /// </summary>
        /// <returns>The parent collection if there is a location defined or null if there isn't one</returns>
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
        /// <value>Returns the topic with the specified <see cref="TocEntry.Id" /> or null if not found</value>
        public TocEntry this[string id]
        {
            get
            {
                TocEntry found;

                foreach(TocEntry t in this)
                {
                    if(String.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase))
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
        /// <param name="siteMapFile">The site map file associated with the collection</param>
        public TocEntryCollection(ContentFile siteMapFile)
        {
            this.siteMapFile = siteMapFile;
        }
        #endregion

        #region Sort and find methods
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

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// This is used to enumerate all topics recursively
        /// </summary>
        /// <returns>An enumerable list of all topics and sub-topics</returns>
        public IEnumerable<TocEntry> All()
        {
            foreach(var t in this)
            {
                yield return t;

                if(t.Children.Count != 0)
                    foreach(var st in t.Children.All())
                        yield return st;
            }
        }

        /// <summary>
        /// This is used to find all topics and sub-topics that match the specified predicate recursively
        /// </summary>
        /// <param name="match">The match predicate</param>
        /// <param name="expandParentIfFound">True to expand the parent if a child node matches or false to leave
        /// it as is.  Expanding the node ensures it is visible in the bound tree view.</param>
        /// <returns>An enumerable list of all matches</returns>
        public IEnumerable<TocEntry> Find(Predicate<TocEntry> match, bool expandParentIfFound)
        {
            if(match == null)
                throw new ArgumentNullException(nameof(match));

            foreach(var t in this)
            {
                if(match(t))
                    yield return t;

                var matches = t.Children.Find(match, expandParentIfFound);

                if(matches.Any())
                {
                    // If requested, make sure the topic is expanded so that we can move to it in the tree view
                    if(expandParentIfFound)
                        t.IsExpanded = true;

                    foreach(var m in matches)
                        yield return m;
                }
            }
        }
        #endregion

        #region ToString methods
        //=====================================================================

        /// <summary>
        /// Convert the table of contents entry and its children to a string
        /// </summary>
        /// <returns>The entries in string format</returns>
        public override string ToString()
        {
            StringBuilder sb = new(1024);

            this.ConvertToString(sb);

            return sb.ToString();
        }

        /// <summary>
        /// This is used to convert the collection to a string and append it to the specified string builder
        /// </summary>
        /// <param name="sb">The string builder to which the collection is appended</param>
        internal void ConvertToString(StringBuilder sb)
        {
            foreach(TocEntry te in this)
                te.ConvertToString(sb);
        }
        #endregion

        #region Save as intermediate TOC file for conceptual content builds
        //=====================================================================

        /// <summary>
        /// This is used to save the TOC information to an intermediate TOC file used in the conceptual content
        /// build.
        /// </summary>
        /// <param name="rootContainerId">The ID of the root container topic if used</param>
        /// <param name="rootOrder">The TOC order for the root container topic if used</param>
        /// <param name="filename">The filename to use</param>
        public void SaveToIntermediateTocFile(string rootContainerId, int rootOrder, string filename)
        {
            using StreamWriter sw = new(filename);
            
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sw.WriteLine("<topics>");

            if(!String.IsNullOrEmpty(rootContainerId))
            {
                sw.WriteLine("<topic id=\"{0}\" file=\"{0}\" sortOrder=\"{1}\">",
                    WebUtility.HtmlEncode(rootContainerId), rootOrder);
            }

            sw.WriteLine(this.ToString());

            if(!String.IsNullOrEmpty(rootContainerId))
                sw.WriteLine("</topic>");

            sw.WriteLine("</topics>");
        }
        #endregion

        #region Site map file methods
        //=====================================================================

        /// <summary>
        /// This is used to locate the default topic if one exists
        /// </summary>
        /// <returns>The default topic if found or null if not found</returns>
        /// <remarks>The first entry found is returned.  Nodes are searched recursively.</remarks>
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
        /// This is used to load the table of contents entries from the site map file associated with the
        /// collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if a site map has not been associated with
        /// the collection.</exception>
        public void Load()
        {
            if(siteMapFile == null)
                throw new InvalidOperationException("A site map has not been associated with the collection");

            XmlDocument siteMap = new();
            TocEntry entry;

            siteMap.Load(siteMapFile.FullPath);

            foreach(XmlNode site in siteMap.ChildNodes[1].ChildNodes)
            {
                entry = new TocEntry(siteMapFile.BasePathProvider);
                entry.LoadSiteMapNode(site);
                this.Add(entry);
            }
        }

        /// <summary>
        /// This is used to save the table of contents entries to the site map file associated with the
        /// collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if a site map has not been associated with
        /// the collection.</exception>
        public void Save()
        {
            if(siteMapFile == null)
                throw new InvalidOperationException("A site map has not been associated with the collection");

            XmlDocument siteMap = new();

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
        /// This is used by contained items to notify the parent that a child list changed and thus the
        /// collection should be marked as dirty.
        /// </summary>
        /// <param name="t">The item that changed</param>
        /// <param name="e">The list change event arguments from the child collection</param>
        internal void ChildListChanged(TocEntry t, ListChangedEventArgs e)
        {
            int idx = this.IndexOf(t);

            if(idx != -1)
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, idx, e.PropertyDescriptor));
        }
        #endregion

        #region Add all topic files from a folder
        //=====================================================================

        /// <summary>
        /// Add all topics from the specified folder recursively to the collection and to the given project file
        /// </summary>
        /// <param name="folder">The folder from which to get the files</param>
        /// <param name="basePath">The base path to remove from files copied from another folder into the project
        /// folder.  On the first call, this should match the <paramref name="folder"/> value.</param>
        /// <param name="project">The project to which the files are added</param>
        /// <remarks>Only actual HTML and markdown content topic files are added.  They must have a ".htm?" or
        /// "*.md" extension.  Folders will be added as sub-topics recursively.  If a file with the same name as
        /// the folder exists, it will be associated with the container node.  If no such file exists, an empty
        /// container node is created.</remarks>
        public void AddTopicsFromFolder(string folder, string basePath, ISandcastleProject project)
        {
            if(basePath == null)
                throw new ArgumentNullException(nameof(project));

            if(project == null)
                throw new ArgumentNullException(nameof(project));

            TocEntry topic, removeTopic;
            string name, newPath, projectPath = Path.GetDirectoryName(project.Filename);

            if(basePath.Length != 0 && basePath[basePath.Length - 1] != Path.DirectorySeparatorChar)
                basePath += Path.DirectorySeparatorChar;

            // Add files
            foreach(string file in Directory.EnumerateFiles(folder, "*.htm?").Concat(
              Directory.EnumerateFiles(folder, "*.md")))
            {
                // The file must reside under the project path
                if(Path.GetDirectoryName(file).StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                    newPath = file;
                else
                    newPath = Path.Combine(projectPath, file.Substring(basePath.Length));

                // Add the file to the project
                project.AddFileToProject(file, newPath);

                topic = new TocEntry(project)
                {
                    SourceFile = new FilePath(newPath, project),
                    Title = Path.GetFileNameWithoutExtension(newPath)
                };

                this.Add(topic);
            }

            // Add folders recursively
            foreach(string folderName in Directory.EnumerateDirectories(folder))
            {
                topic = new TocEntry(project) { Title = name = Path.GetFileName(folderName) };
                topic.Children.AddTopicsFromFolder(folderName, basePath, project);

                // Ignore empty folders
                if(topic.Children.Count == 0)
                    continue;

                this.Add(topic);

                // Look for a file with the same name as the folder
                removeTopic = null;

                foreach(TocEntry t in topic.Children)
                {
                    if(Path.GetFileNameWithoutExtension(t.SourceFile) == name)
                    {
                        // If found, remove it as it represents the container node
                        topic.Title = name;
                        topic.SourceFile = t.SourceFile;
                        removeTopic = t;
                        break;
                    }
                }

                if(removeTopic != null)
                    topic.Children.Remove(removeTopic);
            }
        }
        #endregion

        #region Overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to set the inserted item's parent to this collection
        /// </summary>
        /// <inheritdoc />
        protected override void InsertItem(int index, TocEntry item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));

            base.InsertItem(index, item);
            item.Parent = this;
        }

        /// <summary>
        /// This is overridden to set the inserted item's parent to this collection
        /// </summary>
        /// <inheritdoc />
        protected override void SetItem(int index, TocEntry item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));

            base.SetItem(index, item);
            item.Parent = this;
        }

        /// <summary>
        /// This is overridden to clear the parent on the removed item
        /// </summary>
        /// <param name="index">The index of the item to remove</param>
        protected override void RemoveItem(int index)
        {
            this[index].Parent = null;
            base.RemoveItem(index);
        }
        #endregion

        #region ITableOfContents implementation
        //=====================================================================

        /// <inheritdoc />
        public ContentFile ContentLayoutFile => siteMapFile;

        /// <inheritdoc />
        /// <remarks>The <paramref name="includeInvisibleItems"/> parameter is ignored as site maps do not
        /// support them.</remarks>
        public void GenerateTableOfContents(TocEntryCollection toc, bool includeInvisibleItems)
        {
            if(toc == null)
                throw new ArgumentNullException(nameof(toc));

            foreach(TocEntry t in this)
                toc.Add(t);
        }
        #endregion
    }
}
