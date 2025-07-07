//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ApiFilterCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the API filter entries for MRefBuilder to remove.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/16/2007  EFW  Created the code
// 07/03/2008  EFW  Rewrote to support MSBuild project format
// 04/07/2011  EFW  Made the constructor and from/to XML members public so that it can be used from the VSPackage
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Sandcastle.Core.Project
{
    /// <summary>
    /// This collection class is used to hold the API filter entries for MRefBuilder to remove
    /// </summary>
    /// <remarks><note type="note">Unlike other collections in the project, this one is cleared and rebuilt if it
    /// changes.  As such, the contained items do not notify the project when they change as they are created
    /// anew each time the collection is rebuilt.</note></remarks>
    public class ApiFilterCollection : List<ApiFilter>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set a reference to the project that owns the collection
        /// </summary>
        /// <remarks>This is used by collection editors to get a reference to the owning project.  Child
        /// collections do not contain a reference to the project file.</remarks>
        public ISandcastleProject Project { get; set; }

        #endregion

        #region Read/write API filter items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing API filter items from the project file
        /// </summary>
        /// <param name="apiFilter">The API filter items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string apiFilter)
        {
            ApiFilter filter;

            using var xr = new XmlTextReader(apiFilter, XmlNodeType.Element,
              new XmlParserContext(null, null, null, XmlSpace.Default));
            
            xr.MoveToContent();

            while(!xr.EOF)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "Filter")
                {
                    filter = new ApiFilter();
                    filter.FromXml(xr);
                    this.Add(filter);
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to write the API filter info to an XML fragment ready for storing in the project file
        /// </summary>
        /// <returns>The XML fragment containing the API filter information</returns>
        public string ToXml()
        {
            using var ms = new MemoryStream(10240);
            using var xw = new XmlTextWriter(ms, new UTF8Encoding(false));

            xw.Formatting = Formatting.Indented;

            foreach(ApiFilter filter in this)
                filter.ToXml(xw);

            xw.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        #endregion

        #region Add/merge exclusion child members based on namespace comments and <exclude /> tags
        //=====================================================================

        /// <summary>
        /// This is used to merge an exclusion entry with the filter collection
        /// </summary>
        /// <param name="entryType">The entry type</param>
        /// <param name="fullName">The member's full name</param>
        /// <returns>True if merged without conflict or false if the merged member conflicted with an existing
        /// entry.  The existing entry will take precedence.</returns>
        public bool MergeExclusionEntry(ApiEntryType entryType, string fullName)
        {
            ApiFilter newEntry;

            foreach(ApiFilter child in this)
            {
                if(child.FullName == fullName)
                {
                    // If the exposure doesn't match, use the existing entry and ignore the merged entry
                    if(child.IsExposed)
                        return false;

                    child.IsProjectExclude = true;
                    return true;
                }
            }

            // It's a new one
            newEntry = new ApiFilter(entryType, fullName, false) { IsProjectExclude = true };

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
        /// <returns>True if merged without conflict or false if the merged member conflicted with an existing
        /// entry.  The existing entry will take precedence.</returns>
        /// <remarks>Entries added by this method are exclusions based on namespace comment or &lt;exclude/&gt;
        /// tag exclusions.</remarks>
        public bool AddNamespaceChild(string fullName, string nameSpace, string typeName, string memberName)
        {
            if(fullName == null)
                throw new ArgumentNullException(nameof(fullName));

            // Find the namespace.  The entry is only added if the namespace is exposed.
            foreach(ApiFilter entry in this)
            {
                if(entry.EntryType == ApiEntryType.Namespace && entry.FullName == nameSpace)
                {
                    if(entry.IsExposed)
                    {
                        if(memberName != null)
                            return entry.Children.AddTypeChild(fullName, typeName, memberName);

                        return entry.Children.MergeExclusionEntry(ApiEntryType.Class, fullName.Substring(2));
                    }

                    return true;    // Excluded by default
                }
            }

            // New namespace
            ApiFilter newEntry = new(ApiEntryType.Namespace, nameSpace, true) { IsProjectExclude = true };

            this.Add(newEntry);

            newEntry.Children.AddTypeChild(fullName, typeName, memberName);

            return true;
        }

        /// <summary>
        /// Add a new member entry to this type collection
        /// </summary>
        /// <param name="fullName">The full name of the entry</param>
        /// <param name="typeName">The type name</param>
        /// <param name="memberName">The member</param>
        /// <returns>True if merged without conflict or false if the merged member conflicted with an existing
        /// entry.  The existing entry will take precedence.</returns>
        /// <remarks>Entries added by this method are exclusions based on namespace comment or &lt;exclude/&gt;
        /// tag exclusions.</remarks>
        public bool AddTypeChild(string fullName, string typeName, string memberName)
        {
            if(fullName == null)
                throw new ArgumentNullException(nameof(fullName));

            // Find the type
            foreach(ApiFilter entry in this)
            {
                if(entry.FullName == typeName)
                {
                    // The entry is only added if the namespace is exposed
                    if(entry.IsExposed)
                    {
                        return entry.Children.MergeExclusionEntry(ApiFilter.ApiEntryTypeFromLetter(fullName[0]),
                            fullName.Substring(2));
                    }

                    return true;    // Excluded by default
                }
            }

            // New type
            ApiFilter newEntry = new(ApiEntryType.Class, typeName, (memberName != null)) { IsProjectExclude = true };

            this.Add(newEntry);

            if(memberName != null)
            {
                ApiFilter childEntry = new(ApiFilter.ApiEntryTypeFromLetter(fullName[0]),
                  fullName.Substring(2), false)
                {
                    IsProjectExclude = true
                };

                newEntry.Children.Add(childEntry);
            }

            return true;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Convert the API filter entry and its children to a string
        /// </summary>
        /// <returns>The entries in the MRefBuilder API filter XML format</returns>
        public override string ToString()
        {
            StringBuilder sb = new(10240);

            sb.Append("<apiFilter expose=\"true\">\r\n");

            foreach(ApiFilter entry in this)
                entry.ConvertToString(sb);

            sb.Append("</apiFilter>\r\n");

            return sb.ToString();
        }
        #endregion
    }
}
