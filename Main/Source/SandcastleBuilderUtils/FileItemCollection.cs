//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FileItemCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold build items from the
// project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  08/07/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using Microsoft.Build.BuildEngine;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold build items from a project.
    /// </summary>
    public class FileItemCollection : BindingList<FileItem>
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private BuildAction buildAction;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project file containing the build items</param>
        /// <param name="action">The build action for the items to hold in
        /// the collection.</param>
        public FileItemCollection(SandcastleProject project, BuildAction action)
        {
            projectFile = project;
            buildAction = action;
            this.Refresh();
        }
        #endregion

        #region Sort collection
        //=====================================================================
        // Sort the collection

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by display title and ID.  Comparisons
        /// are case-insensitive.</remarks>
        public void Sort()
        {
            Comparison<FileItem> sort;

            switch(buildAction)
            {
                case BuildAction.ContentLayout:
                case BuildAction.SiteMap:
                    sort = new Comparison<FileItem>(this.SortContentLayout);
                    break;

                default:
                    sort = new Comparison<FileItem>(this.GeneralSort);
                    break;
            }

            ((List<FileItem>)base.Items).Sort(sort);
        }

        /// <summary>
        /// Sort content layout files
        /// </summary>
        /// <param name="x">The first file item</param>
        /// <param name="y">The second file item</param>
        private int SortContentLayout(FileItem x, FileItem y)
        {
            int result;

            if(x.SortOrder < y.SortOrder)
                result = -1;
            else
                if(x.SortOrder > y.SortOrder)
                    result = 1;
                else
                    result = String.Compare(x.Link.Path, y.Link.Path,
                        StringComparison.OrdinalIgnoreCase);

            return result;
        }

        /// <summary>
        /// General sort
        /// </summary>
        /// <param name="x">The first file item</param>
        /// <param name="y">The second file item</param>
        private int GeneralSort(FileItem x, FileItem y)
        {
            return String.Compare(x.Link.Path, y.Link.Path,
                StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to refresh the collection by loading the build items
        /// from the project.
        /// </summary>
        public void Refresh()
        {
            this.Clear();

            foreach(BuildItem item in
              projectFile.MSBuildProject.GetEvaluatedItemsByName(buildAction.ToString()))
                this.Add(new FileItem(new ProjectElement(projectFile, item)));

            this.Sort();
        }
        #endregion
    }
}
