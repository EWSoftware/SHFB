//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ApiFilterCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/03/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the API filter entries
// for MRefBuilder to remove.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/16/2007  EFW  Created the code
// 1.8.0.0  07/03/2008  EFW  Rewrote to support MSBuild project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the API filter entries for
    /// MRefBuilder to remove.
    /// </summary>
    /// <remarks><note type="note">Unlike other collections in the project,
    /// this one is cleared and rebuilt if it changes.  As such, the contained
    /// items do not notify the project when they change as they are created
    /// anew each time the collection is rebuilt.</note></remarks>
    [TypeConverter(typeof(ApiFilterCollectionTypeConverter)),
      Editor(typeof(ApiFilterEditor), typeof(UITypeEditor))]
    public class ApiFilterCollection : BindingList<ApiFilter>, ICloneable
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private bool isDirty;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the dirty state of the collection
        /// </summary>
        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }

        /// <summary>
        /// This is used to get a reference to the project that owns the
        /// collection.
        /// </summary>
        /// <remarks>Child collections do not contain a reference to the
        /// project file.</remarks>
        public SandcastleProject Project
        {
            get { return projectFile; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        /// <remarks>Child collections do not contain a reference to the
        /// project file.</remarks>
        internal ApiFilterCollection(SandcastleProject project)
        {
            projectFile = project;
        }
        #endregion

        #region Sort collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>All top level items and their children are sorted by
        /// API entry type and then by name</remarks>
        public void Sort()
        {
            ((List<ApiFilter>)base.Items).Sort(
                delegate(ApiFilter x, ApiFilter y)
                {
                    return Comparer<ApiFilter>.Default.Compare(x, y);
                });

            foreach(ApiFilter te in this)
                te.Children.Sort();
        }
        #endregion

        #region Read/write API filter items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing API filter items from the project
        /// file.
        /// </summary>
        /// <param name="apiFilter">The API filter items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        internal void FromXml(string apiFilter)
        {
            ApiFilter filter;
            XmlTextReader xr = null;

            try
            {
                xr = new XmlTextReader(apiFilter, XmlNodeType.Element,
                    new XmlParserContext(null, null, null, XmlSpace.Default));
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "Filter")
                    {
                        filter = new ApiFilter();
                        filter.FromXml(xr);
                        this.Add(filter);
                    }

                    xr.Read();
                }
            }
            finally
            {
                if(xr != null)
                    xr.Close();

                isDirty = false;
            }
        }

        /// <summary>
        /// This is used to write the API filter info to an XML fragment ready
        /// for storing in the project file.
        /// </summary>
        /// <returns>The XML fragment containing the help attribute info</returns>
        internal string ToXml()
        {
            MemoryStream ms = new MemoryStream(10240);
            XmlTextWriter xw = null;

            try
            {
                xw = new XmlTextWriter(ms, new UTF8Encoding(false));
                xw.Formatting = Formatting.Indented;

                foreach(ApiFilter filter in this)
                    filter.ToXml(xw);

                xw.Flush();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            finally
            {
                if(xw != null)
                    xw.Close();

                ms.Dispose();
            }
        }
        #endregion

        #region Add/merge child members
        //=====================================================================
        // Add or merge child members to the collection based on namespace
        // comment or <exclude/> tag exclusions.

        /// <summary>
        /// This is used to merge an entry with the filter collection
        /// </summary>
        /// <param name="entryType">The entry type</param>
        /// <param name="fullName">The member's full name</param>
        /// <param name="isExposed">True to expose it, false to remove it</param>
        /// <param name="isProjectExclude">True if this is a project exclude
        /// (currently this will always be true).</param>
        /// <returns>True if merged without conflict or false if the merged
        /// member conflicted with an existing entry.  The existing entry
        /// will take precedence.</returns>
        public bool MergeEntry(ApiEntryType entryType, string fullName,
          bool isExposed, bool isProjectExclude)
        {
            ApiFilter newEntry;

            foreach(ApiFilter child in this)
                if(child.FullName == fullName)
                {
                    // If the exposure doesn't match, use the existing
                    // entry and ignore the merged entry
                    if(child.IsExposed != isExposed)
                        return false;

                    child.IsProjectExclude = isProjectExclude;
                    return true;
                }

            // It's a new one
            newEntry = new ApiFilter(entryType, fullName, isExposed);
            newEntry.IsProjectExclude = isProjectExclude;
            this.Add(newEntry);

            return true;
        }

        /// <summary>
        /// Add a new type entry to this namespace collection
        /// </summary>
        /// <param name="fullName">The full name of the entry</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="typeName">The type name</param>
        /// <param name="memberName">The member</param>
        /// <returns>True if merged without conflict or false if the merged
        /// member conflicted with an existing entry.  The existing entry
        /// will take precedence.</returns>
        /// <remarks>Entries added by this method are exclusions based on
        /// namespace comment or &lt;exclude/&gt; tag exclusions.</remarks>
        public bool AddNamespaceChild(string fullName, string nameSpace,
          string typeName, string memberName)
        {
            ApiFilter newEntry;

            // Find the namespace.  The entry is only added if the namespace
            // is exposed.
            foreach(ApiFilter entry in this)
                if(entry.FullName == nameSpace)
                {
                    if(entry.IsExposed)
                    {
                        if(memberName != null)
                            return entry.Children.AddTypeChild(fullName,
                                typeName, memberName);

                        return entry.Children.MergeEntry(ApiEntryType.Class,
                            fullName.Substring(2), false, true);
                    }

                    return true;    // Excluded by default
                }

            // New namespace
            newEntry = new ApiFilter(ApiEntryType.Namespace, nameSpace, true);
            newEntry.IsProjectExclude = true;
            base.Add(newEntry);
            newEntry.Children.AddTypeChild(fullName, typeName, memberName);

            return true;
        }

        /// <summary>
        /// Add a new member entry to this type collection
        /// </summary>
        /// <param name="fullName">The full name of the entry</param>
        /// <param name="typeName">The type name</param>
        /// <param name="memberName">The member</param>
        /// <returns>True if merged without conflict or false if the merged
        /// member conflicted with an existing entry.  The existing entry
        /// will take precedence.</returns>
        /// <remarks>Entries added by this method are exclusions based on
        /// namespace comment or &lt;exclude/&gt; tag exclusions.</remarks>
        public bool AddTypeChild(string fullName, string typeName,
          string memberName)
        {
            ApiFilter newEntry, childEntry;

            // Find the type
            foreach(ApiFilter entry in this)
                if(entry.FullName == typeName)
                {
                    // The entry is only added if the namespace is exposed
                    if(entry.IsExposed)
                        return entry.Children.MergeEntry(
                            ApiFilter.ApiEntryTypeFromLetter(fullName[0]),
                            fullName.Substring(2), false, true);

                    return true;    // Excluded by default
                }

            // New type
            newEntry = new ApiFilter(ApiEntryType.Class, typeName,
                (memberName != null));
            newEntry.IsProjectExclude = true;
            base.Add(newEntry);

            if(memberName != null)
            {
                childEntry = new ApiFilter(ApiFilter.ApiEntryTypeFromLetter(
                    fullName[0]), fullName.Substring(2), false);
                childEntry.IsProjectExclude = true;
                newEntry.Children.Add(childEntry);
            }

            return true;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to mark the collection as dirty when it changes
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            isDirty = true;
            base.OnListChanged(e);
        }

        /// <summary>
        /// Convert the API filter entry and its children to a string
        /// </summary>
        /// <returns>The entries in the MRefBuilder API filter XML format</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(10240);

            sb.Append("<apiFilter expose=\"true\">\r\n");

            foreach(ApiFilter entry in this)
                entry.ConvertToString(sb);

            sb.Append("</apiFilter>\r\n");

            return sb.ToString();
        }
        #endregion

        #region ICloneable Members
        //=====================================================================

        /// <summary>
        /// Clone the API filter collection
        /// </summary>
        /// <returns>A clone of the collection</returns>
        public object Clone()
        {
            ApiFilterCollection clone = new ApiFilterCollection(projectFile);

            foreach(ApiFilter filter in this)
                clone.Add((ApiFilter)filter.Clone());

            return clone;
        }
        #endregion
    }
}
