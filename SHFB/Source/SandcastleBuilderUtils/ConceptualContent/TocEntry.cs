//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TocEntry.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a table of contents entry.  This is used to build the table of
// contents entries for content layout and site map files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/17/2006  EFW  Created the code
// 12/08/2006  EFW  Added NeedsColorizing property
// 03/09/2007  EFW  Added support for <code source="file" /> tags
// 07/03/2007  EFW  Added support for saving as a site map file
// 04/12/2008  EFW  Added support for a split table of contents
// 08/11/2008  EFW  Modified to support the new project format
// 06/15/2010  EFW  Added support for MS Help Viewer TOC format
// 12/20/2011  EFW  Updated for use with the new content layout editor
// 05/07/2015  EFW  Removed all deprecated code
//===============================================================================================================

// Ignore Spelling: url

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a table of contents entry.  This is used to build the table of contents entries for
    /// content layout and site map files.
    /// </summary>
    public class TocEntry : IComparable<TocEntry>, ICloneable, INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private FilePath sourceFile;
        private ApiParentMode apiParentMode;
        private string title;
        private bool isDefaultTopic, isSelected, isExpanded;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to track the topic's parent collection
        /// </summary>
        /// <remarks>This is used by the site map editor to move items around within the collection</remarks>
        public TocEntryCollection Parent { get; internal set; }

        /// <summary>
        /// This returns the <see cref="IBasePathProvider" /> for the entry.
        /// </summary>
        public IBasePathProvider BasePathProvider { get; }

        /// <summary>
        /// This returns the child table of contents collection for this entry
        /// </summary>
        /// <value>If empty, this is a single item in the table of contents.  If it has children, they are listed
        /// below this one.  A file may or may not be associated with this entry if it is a root node.</value>
        public TocEntryCollection Children { get; } = new TocEntryCollection();

        /// <summary>
        /// This is used to get or set the entry's source file path.
        /// </summary>
        public FilePath SourceFile
        {
            get => sourceFile;
            set
            {
                if(value == null)
                    sourceFile = new FilePath(this.BasePathProvider);
                else
                    sourceFile = value;
            }
        }

        /// <summary>
        /// This is used to get or set the entry's destination file path.
        /// </summary>
        public string DestinationFile { get; set; }

        /// <summary>
        /// The ID of the item when it represents a TOC entry from a content layout file
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The display title for the topic previewer
        /// </summary>
        public string PreviewerTitle { get; set; }

        /// <summary>
        /// The link text for the topic previewer
        /// </summary>
        public string LinkText { get; set; }

        /// <summary>
        /// This is used to get or set the entry's title in the table of contents
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                if(value != title)
                {
                    if(String.IsNullOrWhiteSpace(value))
                        value = Path.GetFileNameWithoutExtension(this.SourceFile);

                    title = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the item is the default topic for the help file
        /// </summary>
        public bool IsDefaultTopic
        {
            get => isDefaultTopic;
            set
            {
                if(value != isDefaultTopic)
                {
                    isDefaultTopic = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too
                }
            }
        }

        /// <summary>
        /// This is used to get or set the sort order for the entry within its group
        /// </summary>
        /// <value>Entries with identical sort order values will sort by title as well.  Items with no specific
        /// sort order will sort below those with a defined sort order.</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// This is used to specify how API content is parented to this topic or the topic's parent
        /// </summary>
        public ApiParentMode ApiParentMode
        {
            get => apiParentMode;
            set
            {
                if(value != apiParentMode)
                {
                    apiParentMode = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is selected
        /// </summary>
        /// <remarks>Used by the editor for binding in the tree view.  The value is serialized when saved so that
        /// its state is remembered when reloaded.</remarks>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if(value != isSelected)
                {
                    isSelected = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is expanded
        /// </summary>
        /// <remarks>Used by the editor for binding in the tree view.  The value is serialized when saved so that
        /// its state is remembered when reloaded.</remarks>
        public bool IsExpanded
        {
            get => isExpanded && this.Children.Count != 0;
            set
            {
                if(value != isExpanded)
                {
                    isExpanded = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This returns a description of the topic that can be used as a tool tip
        /// </summary>
        public string ToolTip
        {
            get
            {
                string description = this.Title;

                if(isDefaultTopic)
                    description += "\nDefault topic";

                if(apiParentMode != ApiParentMode.None)
                {
                    if(isDefaultTopic)
                        description += " / ";
                    else
                        description += "\n";

                    switch(apiParentMode)
                    {
                        case ApiParentMode.InsertAfter:
                            description += "Insert API content after topic";
                            break;

                        case ApiParentMode.InsertBefore:
                            description += "Insert API content before topic";
                            break;

                        default:
                            description += "Insert API content as child of topic";
                            break;
                    }
                }

                return description;
            }
        }

        /// <summary>
        /// This is used to get or set a unique ID to work around a legacy additional content support issue
        /// </summary>
        /// <value>The site map editor assigns each topic its own unique ID to work around object equality issues
        /// caused by legacy support for file system based additional content.</value>
        public Guid UniqueId { get; set; }

        #endregion

        #region IComparable<TocEntry> Members
        //=====================================================================

        /// <summary>
        /// Compares this instance to another instance and returns an indication of their relative values
        /// </summary>
        /// <param name="other">A TocEntry object to compare</param>
        /// <returns>Returns -1 if this instance is less than the value, 0 if they are equal, or 1 if this
        /// instance is greater than the value or the value is null.</returns>
        /// <remarks>The <see cref="SortOrder"/> property is compared first.  If equal, the <see cref="Title"/>
        /// property is used.</remarks>
        public int CompareTo(TocEntry other)
        {
            if(other == null)
                return 1;

            if(this.SortOrder < other.SortOrder)
                return -1;

            if(this.SortOrder > other.SortOrder)
                return 1;

            return String.Compare(this.Title, other.Title, StringComparison.Ordinal);
        }
        #endregion

        #region ICloneable Members
        //=====================================================================

        /// <summary>
        /// Clone this table of contents entry
        /// </summary>
        /// <returns>A clone of this table of contents entry</returns>
        public object Clone()
        {
            TocEntry clone = new TocEntry(this.BasePathProvider)
            {
                DestinationFile = this.DestinationFile,
                IsDefaultTopic = false,   // Can't have more than one
                SortOrder = this.SortOrder,
                SourceFile = (FilePath)sourceFile.Clone(),
                Title = this.Title
            };

            foreach(TocEntry child in this.Children)
                clone.Children.Add((TocEntry)child.Clone());

            return clone;
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="basePathProvider">The base path provider</param>
        public TocEntry(IBasePathProvider basePathProvider)
        {
            this.BasePathProvider = basePathProvider;
            this.SortOrder = Int32.MaxValue;

            sourceFile = new FilePath(basePathProvider);

            this.Children.ListChanged += childList_ListChanged;
        }
        #endregion

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Reset the sort order on this item and its children
        /// </summary>
        public void ResetSortOrder()
        {
            this.SortOrder = -1;

            foreach(TocEntry t in this.Children)
                t.ResetSortOrder();
        }

        /// <summary>
        /// Mark the project as dirty if the contained lists change
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>This may not be the best way to handle this.</remarks>
        private void childList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(this.Parent != null)
                this.Parent.ChildListChanged(this, e);
        }
        #endregion

        #region Equality, hash code, and To String
        //=====================================================================

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            TocEntry other = obj as TocEntry;

            if(other == null)
                return false;

            // Work around legacy support equality issue
            if(this.UniqueId != other.UniqueId)
                return false;

            return (this.SourceFile == other.SourceFile && this.DestinationFile == other.DestinationFile &&
                this.Title == other.Title && this.SortOrder == other.SortOrder &&
                this.IsDefaultTopic == other.IsDefaultTopic && this.ApiParentMode == other.ApiParentMode);
        }

        /// <summary>
        /// Overload for equal operator
        /// </summary>
        /// <param name="t1">The first TOC entry object</param>
        /// <param name="t2">The second TOC entry object</param>
        /// <returns>True if equal, false if not.</returns>
        public static bool operator ==(TocEntry t1, TocEntry t2)
        {
            // Do they reference the same entry?
            if(ReferenceEquals(t1, t2))
                return true;

            // Check null reference first
            if(t1 is null || t2 is null)
                return false;

            return t1.Equals(t2);
        }

        /// <summary>
        /// Overload for not equal operator
        /// </summary>
        /// <param name="t1">The first TOC entry object</param>
        /// <param name="t2">The second TOC entry object</param>
        /// <returns>True if not equal, false if they are equal.</returns>
        public static bool operator !=(TocEntry t1, TocEntry t2)
        {
            return !(t1 == t2);
        }

        /// <summary>
        /// Overload for less than operator
        /// </summary>
        /// <param name="t1">The first TOC entry object</param>
        /// <param name="t2">The second TOC entry object</param>
        /// <returns>True if t1 is less than t2, false if not.</returns>
        public static bool operator <(TocEntry t1, TocEntry t2)
        {
            return (t1 == null && t2 != null) || (t1 != null && t1.CompareTo(t2) < 0);
        }

        /// <summary>
        /// Overload for greater than operator
        /// </summary>
        /// <param name="t1">The first TOC entry object</param>
        /// <param name="t2">The second TOC entry object</param>
        /// <returns>True if t1 is greater than t2, false if not.</returns>
        public static bool operator >(TocEntry t1, TocEntry t2)
        {
            return t1 != null && t1.CompareTo(t2) > 0;
        }  

        /// <summary>
        /// Get a hash code for this item
        /// </summary>
        /// <returns>Returns the hash code for the <see cref="ToString()" />
        /// value.</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Convert the table of contents entry and its children to a string
        /// </summary>
        /// <returns>The entries in HTML 1.x help format</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(1024);

            this.ConvertToString(sb);

            return sb.ToString();
        }

        /// <summary>
        /// This is used to convert the entry to a string and append it to the specified string builder
        /// </summary>
        /// <param name="sb">The string builder to which the entry is appended</param>
        internal void ConvertToString(StringBuilder sb)
        {
            string url, orderAttr, titleAttr;

            if(String.IsNullOrEmpty(this.DestinationFile))
            {
                url = WebUtility.HtmlEncode(this.Id);
                titleAttr = String.Format(CultureInfo.InvariantCulture, " title=\"{0}\"",
                    WebUtility.HtmlEncode(this.Title));
            }
            else
            {
                url = WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(this.DestinationFile));
                titleAttr = String.Empty;
            }

            if(this.SortOrder != -1)
                orderAttr = String.Format(CultureInfo.InvariantCulture, " sortOrder=\"{0}\"", this.SortOrder);
            else
                orderAttr = String.Empty;

            if(this.Children.Count == 0)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<topic id=\"{0}\" file=\"{0}\"{1}{2} />\r\n", url,
                    orderAttr, titleAttr);
            }
            else
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<topic id=\"{0}\" file=\"{0}\"{1}{2}>\r\n", url,
                    orderAttr, titleAttr);

                this.Children.ConvertToString(sb);
                sb.Append("</topic>\r\n");
            }
        }
        #endregion

        #region Convert to link element format
        //=====================================================================

        /// <summary>
        /// Convert the entry to its <c>&lt;a&gt;</c> element form
        /// </summary>
        /// <param name="innerText">Optional inner text</param>
        /// <returns>The topic in its <c>&lt;a&gt;</c> element form</returns>
        public string ToAnchor(string innerText)
        {
            string path = sourceFile.PersistablePath.Replace('\\', '/');

            if(String.IsNullOrEmpty(path))
                path = "#";

            if(String.IsNullOrWhiteSpace(innerText))
                return String.Format(CultureInfo.CurrentCulture, "<a href=\"{0}\">{1}</a>", path, this.Title);

            return String.Format(CultureInfo.CurrentCulture, "<a href=\"{0}\">{1}</a>", path, innerText);
        }
        #endregion

        #region Site map methods
        //=====================================================================

        /// <summary>
        /// This will load information about the entry from the node and will also load all child nodes
        /// </summary>
        /// <param name="site">The site map node to use for this entry</param>
        public void LoadSiteMapNode(XmlNode site)
        {
            TocEntry child;

            if(site != null)
            {
                this.Title = site.Attributes["title"].Value;

                if(site.Attributes["url"] == null)
                    sourceFile = new FilePath(this.BasePathProvider);
                else
                    sourceFile = new FilePath(site.Attributes["url"].Value, this.BasePathProvider);

                if(site.Attributes["isDefault"] != null)
                    this.IsDefaultTopic = true;

                if(site.Attributes["isExpanded"] != null)
                    this.IsExpanded = true;

                if(site.Attributes["isSelected"] != null)
                    this.IsSelected = true;

                // Site maps only support None or After
                if(site.Attributes["splitToc"] != null)
                    this.ApiParentMode = ApiParentMode.InsertAfter;

                if(site.ChildNodes.Count != 0)
                    foreach(XmlNode childSite in site.ChildNodes)
                    {
                        child = new TocEntry(this.BasePathProvider);
                        child.LoadSiteMapNode(childSite);
                        
                        this.Children.Add(child);
                    }
            }
        }

        /// <summary>
        /// Save this node and its children to the specified root node as site map nodes
        /// </summary>
        /// <param name="root">The root node to which the current entry is added</param>
        public void SaveAsSiteMapNode(XmlNode root)
        {
            XmlAttribute attr;

            if(root == null)
                throw new ArgumentNullException(nameof(root));

            XmlNode child = root.OwnerDocument.CreateNode(XmlNodeType.Element, "siteMapNode", root.NamespaceURI);

            attr = root.OwnerDocument.CreateAttribute("title");
            attr.Value = this.Title;
            child.Attributes.Append(attr);

            attr = root.OwnerDocument.CreateAttribute("url");

            if(sourceFile.Path.Length == 0)
                attr.Value = String.Empty;
            else
                attr.Value = sourceFile.PersistablePath;

            child.Attributes.Append(attr);

            if(this.IsDefaultTopic)
            {
                attr = root.OwnerDocument.CreateAttribute("isDefault");
                attr.Value = "true";
                child.Attributes.Append(attr);
            }

            if(this.IsExpanded)
            {
                attr = root.OwnerDocument.CreateAttribute("isExpanded");
                attr.Value = "true";
                child.Attributes.Append(attr);
            }

            if(this.IsSelected)
            {
                attr = root.OwnerDocument.CreateAttribute("isSelected");
                attr.Value = "true";
                child.Attributes.Append(attr);
            }

            // Site maps only supports None or After
            if(this.ApiParentMode == ApiParentMode.InsertAfter)
            {
                attr = root.OwnerDocument.CreateAttribute("splitToc");
                attr.Value = "true";
                child.Attributes.Append(attr);
            }

            root.AppendChild(child);

            if(this.Children.Count != 0)
                foreach(TocEntry te in this.Children)
                    te.SaveAsSiteMapNode(child);
        }

        /// <summary>
        /// See if this entry or one of its children is a match to the specified source filename
        /// </summary>
        /// <param name="sourceFilename">The source filename to match</param>
        /// <returns>The match TOC entry or null if not found</returns>
        public TocEntry ContainsMatch(string sourceFilename)
        {
            TocEntry match = null;

            if(sourceFile == sourceFilename)
                return this;

            if(this.Children.Count != 0)
                foreach(TocEntry entry in this.Children)
                {
                    match = entry.ContainsMatch(sourceFilename);

                    if(match != null)
                        break;
                }

            return match;
        }
        #endregion
    }
}
