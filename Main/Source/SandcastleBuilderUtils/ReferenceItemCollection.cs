//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ReferenceItemCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/10/2010
// Note    : Copyright 2006-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the reference item
// information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.1.0.0  08/23/2006  EFW  Created the code
// 1.8.0.0  06/23/2008  EFW  Rewrote to support MSBuild project format
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

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the reference items for a project
    /// </summary>
    public class ReferenceItemCollection : BindingList<ReferenceItem>
    {
        #region Private data members
        //=====================================================================

        private const string ReferenceType = "Reference";
        private const string ProjectReferenceType = "ProjectReference";
        private const string COMReferenceType = "COMReference";

        private SandcastleProject projectFile;
        private DateTime timeOfLastDirty;
        private bool loadingItems;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        internal ReferenceItemCollection(SandcastleProject project)
        {
            projectFile = project;
        }
        #endregion

        #region Sort the collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection in ascending order.
        /// </summary>
        public void Sort()
        {
            ((List<ReferenceItem>)base.Items).Sort((x, y) =>
            {
                return String.Compare(x.Reference, y.Reference,
                    StringComparison.CurrentCultureIgnoreCase);
            });
        }
        #endregion

        #region Load the build items from the project
        //=====================================================================

        /// <summary>
        /// This is used to ensure that the collection has up to date
        /// information about the reference build items in the project.
        /// </summary>
        /// <param name="refresh">True to refresh if out of date or false
        /// to only load the references if not already done.</param>
        /// <remarks>The collection is only loaded when necessary</remarks>
        public void EnsureCurrent(bool refresh)
        {
            Project project = projectFile.MSBuildProject;

            if(project.TimeOfLastDirty == timeOfLastDirty || (!refresh &&
              timeOfLastDirty != DateTime.MinValue))
                return;

            try
            {
                loadingItems = true;
                this.Clear();
                timeOfLastDirty = project.TimeOfLastDirty;

                BuildItemGroup referenceGroup = project.GetEvaluatedItemsByName(ReferenceType);

                foreach(BuildItem item in referenceGroup)
                    this.Add(new ReferenceItem(new ProjectElement(projectFile, item)));

                referenceGroup = project.GetEvaluatedItemsByName(ProjectReferenceType);

                foreach(BuildItem item in referenceGroup)
                    this.Add(new ProjectReferenceItem(new ProjectElement(projectFile, item)));

                referenceGroup = project.GetEvaluatedItemsByName(COMReferenceType);

                foreach(BuildItem item in referenceGroup)
                    this.Add(new COMReferenceItem(new ProjectElement(projectFile, item)));
            }
            finally
            {
                loadingItems = false;
            }
        }
        #endregion

        #region Add and remove elements from the project
        //=====================================================================

        // Note that adding COM references is not supported in the standalone
        // GUI as I haven't found a way to get a list of COM objects and
        // generate the necessary metadata for the reference items.  It does
        // support COM references added via Visual Studio.

        /// <summary>
        /// Add a new GAC or file reference item to the collection
        /// </summary>
        /// <param name="referenceName">The reference name.  This will be the
        /// GAC name or the base filename for file reference.</param>
        /// <param name="hintPath">The hint path for file references.  For
        /// GAC references, this should be null.</param>
        /// <returns>The <see cref="ReferenceItem" /> added to the
        /// project.  If the named item already exists in the collection
        /// a reference to the existing item is returned.</returns>
        /// <remarks>The <see cref="ReferenceItem" /> constructor is internal
        /// so that we control creation of the items and can associate them
        /// with a project element.</remarks>
        public ReferenceItem AddReference(string referenceName, string hintPath)
        {
            ReferenceItem item = new ReferenceItem(new ProjectElement(
                projectFile, ReferenceType, referenceName));

            if(!String.IsNullOrEmpty(hintPath))
                item.HintPath = new FilePath(hintPath, projectFile);

            int idx = base.IndexOf(item);

            if(idx == -1)
                base.Add(item);
            else
                item = base[idx];

            return item;
        }

        /// <summary>
        /// Add a new project reference item to the collection
        /// </summary>
        /// <param name="projectPath">The path to the project</param>
        /// <returns>The <see cref="ProjectReferenceItem" /> added to the
        /// project.  If the named item already exists in the collection
        /// a reference to the existing item is returned.</returns>
        public ProjectReferenceItem AddProjectReference(string projectPath)
        {
            ProjectReferenceItem item = new ProjectReferenceItem(
                new ProjectElement(projectFile, ProjectReferenceType, projectPath));

            int idx = base.IndexOf(item);

            if(idx == -1)
                base.Add(item);
            else
                item = (ProjectReferenceItem)base[idx];

            return item;
        }

        /// <summary>
        /// Remove an item from the collection and from the project file
        /// </summary>
        /// <param name="index">The index of the item to remove</param>
        protected override void RemoveItem(int index)
        {
            ReferenceItem item = this[index];
            item.ProjectElement.RemoveFromProjectFile();
            base.RemoveItem(index);
        }

        /// <summary>
        /// This is overridden to suppress the event when loading references
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if(!loadingItems)
                base.OnListChanged(e);
        }
        #endregion
    }
}
