//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : NamespaceSummaryItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/30/2013
// Note    : Copyright 2006-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a namespace summary item that can be used to add comments to a
// namespace in the help file or exclude it completely from the help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.2.0.0  08/02/2006  EFW  Created the code
// 1.8.0.0  06/30/2008  EFW  Rewrote to support the MSBuild project format
// 1.9.9.0  11/30/2013  EFW  Merged changes from Stazzz to support namespace grouping
//===============================================================================================================

using System;
using System.ComponentModel;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents a a namespace summary item that can be used to add comments to a namespace in the help
    /// file or exclude it completely from the help file.
    /// </summary>
    [DefaultProperty("Summary")]
    public class NamespaceSummaryItem : PropertyBasedCollectionItem
    {
        #region Private data members
        //=====================================================================

        private bool isDocumented;
        private string name, summary;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to check whether or not this is a namespace group
        /// </summary>
        [Category("Summary"), Description("If true, this is a namespace group.  If false, it is a normal namespace"),
          DefaultValue(false)]
        public Boolean IsGroup { get; private set; }

        /// <summary>
        /// This is used to get or set whether or not the namespace is included in the help file
        /// </summary>
        [Category("Summary"), Description("If true, the namespace and its contents will appear in the help " +
          "file.  If false, it is excluded."), DefaultValue(true)]
        public bool IsDocumented
        {
            get { return isDocumented; }
            set
            {
                base.CheckProjectIsEditable();
                isDocumented = value;
            }
        }

        /// <summary>
        /// This is used to get or set the namespace summary comments
        /// </summary>
        [Category("Summary"), Description("The summary comments for the namespace."), DefaultValue("")]
        public string Summary
        {
            get { return summary; }
            set
            {
                base.CheckProjectIsEditable();

                if(value == null || value.Trim().Length == 0)
                    summary = String.Empty;
                else
                    summary = value.Trim();
            }
        }

        /// <summary>
        /// This read-only property is used to get the namespace name
        /// </summary>
        [Category("Summary"), Description("The namespace's name."), DefaultValue("(global)")]
        public string Name
        {
            get { return (name.Length == 0) ? "(global)" : name; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="itemName">The namespace's name</param>
        /// <param name="isGroup">This indicates whether or not the namespace is a group namespace</param>
        /// <param name="isDocumented">This indicates whether or not the namespace is to be documented</param>
        /// <param name="summaryText">The summary text</param>
        /// <param name="project">The owning project</param>
        internal NamespaceSummaryItem(string itemName, bool isGroup, bool isDocumented, string summaryText,
          SandcastleProject project) : base(project)
        {
            this.IsGroup = isGroup;
            this.name = itemName;
            this.summary = summaryText;
            this.isDocumented = isDocumented;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Return the namespace name as the string representation of the object
        /// </summary>
        /// <returns>The namespace name</returns>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }
}
