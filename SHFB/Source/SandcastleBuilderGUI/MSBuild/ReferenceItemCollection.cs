//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ReferenceItemCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/22/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the reference item information
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/23/2006  EFW  Created the code
// 06/23/2008  EFW  Rewrote to support MSBuild project format
// 07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 05/13/2015  EFW  Moved the file to the GUI project as it is only used there
//===============================================================================================================

using System;
using System.ComponentModel;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;

using SandcastleBuilder.MSBuild.HelpProject;

namespace SandcastleBuilder.Gui.MSBuild
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

        private readonly SandcastleProject projectFile;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        public ReferenceItemCollection(SandcastleProject project)
        {
            projectFile = project;

            Project msBuildProject = projectFile.MSBuildProject;

            foreach(ProjectItem item in msBuildProject.GetItems(ReferenceType))
                this.Add(new ReferenceItem(projectFile, item));

            foreach(ProjectItem item in msBuildProject.GetItems(ProjectReferenceType))
                this.Add(new ProjectReferenceItem(projectFile, item));

            foreach(ProjectItem item in msBuildProject.GetItems(COMReferenceType))
                this.Add(new COMReferenceItem(projectFile, item));
        }
        #endregion

        #region Add and remove elements from the project
        //=====================================================================

        // Note that adding COM references is not supported in the standalone GUI as I haven't found a way to get
        // a list of COM objects and generate the necessary metadata for the reference items.  It does support
        // COM references added via Visual Studio.

        /// <summary>
        /// Add a new GAC or file reference item to the collection
        /// </summary>
        /// <param name="referenceName">The reference name.  This will be the GAC name or the base filename for
        /// file reference.</param>
        /// <param name="hintPath">The hint path for file references.  For GAC references, this should be null.</param>
        /// <returns>The <see cref="ReferenceItem" /> added to the project.  If the named item already exists in
        /// the collection a reference to the existing item is returned.</returns>
        /// <remarks>The <see cref="ReferenceItem" /> constructor is internal so that we control creation of the
        /// items and can associate them with a project element.</remarks>
        public ReferenceItem AddReference(string referenceName, string hintPath)
        {
            ReferenceItem item = new(projectFile, ReferenceType, referenceName);

            if(!String.IsNullOrEmpty(hintPath))
                item.HintPath = new FilePath(hintPath, projectFile);

            int idx = this.IndexOf(item);

            if(idx == -1)
                this.Add(item);
            else
                item = base[idx];

            return item;
        }

        /// <summary>
        /// Add a new project reference item to the collection
        /// </summary>
        /// <param name="projectPath">The path to the project</param>
        /// <returns>The <see cref="ProjectReferenceItem" /> added to the project.  If the named item already
        /// exists in the collection a reference to the existing item is returned.</returns>
        public ProjectReferenceItem AddProjectReference(string projectPath)
        {
            ProjectReferenceItem item = new(projectFile, ProjectReferenceType, projectPath);

            int idx = this.IndexOf(item);

            if(idx == -1)
                this.Add(item);
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
            item.RemoveFromProjectFile();
            base.RemoveItem(index);
        }
        #endregion
    }
}
