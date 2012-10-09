//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : NamespaceSummaryItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/01/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a namespace summary item that can
// be used to add comments to a namespace in the help file or exclude it
// completely from the help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  08/02/2006  EFW  Created the code
// 1.8.0.0  06/30/2008  EFW  Rewrote to support the MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents a a namespace summary item that can be used to add
    /// comments to a namespace in the help file or exclude it completely
    /// from the help file.
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
        /// This is used to get or set whether or not the namespace is included
        /// in the help file.
        /// </summary>
        [Category("Summary"), Description("If true, the namespace and its " +
          "contents will appear in the help file.  If false, it is excluded."),
          DefaultValue(true)]
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
        [Category("Summary"), Description("The summary comments for the " +
          "namespace."), DefaultValue("")]
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
        [Category("Summary"), Description("The namespace's name."),
          DefaultValue("(global)")]
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
        /// <param name="documented">The flag indicating whether or not the
        /// namespace is to be documented.</param>
        /// <param name="summaryText">The summary text</param>
        /// <param name="project">The owning project</param>
        internal NamespaceSummaryItem(string itemName, bool documented,
          string summaryText, SandcastleProject project) : base(project)
        {
            name = itemName;
            summary = summaryText;
            isDocumented = documented;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Return the namespace name as the string representation of the
        /// object.
        /// </summary>
        /// <returns>The namespace name</returns>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }
}
