//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ProjectReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a project reference item that can be used by MRefBuilder to locate
// assembly dependencies for the assemblies being documented.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/23/2006  EFW  Created the code
// 06/30/2008  EFW  Rewrote to support the MSBuild project format
// 12/29/2013  EFW  Added support for the ReferenceOutputAssembly metadata item
// 05/13/2015  EFW  Moved the file to the GUI project as it is only used there
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;

using Microsoft.Build.Evaluation;

using Sandcastle.Platform.Windows.Design;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.MSBuild;

namespace SandcastleBuilder.Gui.MSBuild
{
    /// <summary>
    /// This represents a project reference item that can be used by <strong>MRefBuilder</strong> to locate
    /// assembly dependencies for the assemblies being documented.
    /// </summary>
    public class ProjectReferenceItem : ReferenceItem
    {
        #region Private data members
        //=====================================================================

        private FilePath projectPath;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or path to the project
        /// </summary>
        [Category("Metadata"), Description("The path to the referenced project."),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All), MergableProperty(false),
          FileDialog("Select the reference project", "Visual Studio Projects (*.*proj)|*.*proj|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public FilePath ProjectPath
        {
            get => projectPath;
            set
            {
                if(value == null || value.Path.Length == 0 || value.Path.IndexOfAny(new char[] { '*', '?' }) != -1)
                    throw new ArgumentException("A project path must be specified and cannot contain wildcards " +
                        "(* or ?)", nameof(value));

                // Do this first in case the project isn't editable
                this.Include = value.PersistablePath;

                projectPath = value;
                projectPath.PersistablePathChanging += projectPath_PersistablePathChanging;
                this.GetProjectMetadata(true);
            }
        }

        /// <summary>
        /// Hint path isn't applicable to project references
        /// </summary>
        [Browsable(false)]
        public override FilePath HintPath
        {
            get => base.HintPath;
            set { }
        }

        /// <summary>
        /// This is overridden to return the project name rather than the file path
        /// </summary>
        public override string Reference => this.Name;

        /// <summary>
        /// This is used to get the project reference's GUID
        /// </summary>
        [Category("Metadata"), Description("The project reference's GUID")]
        public string ProjectGuid => this.GetMetadata(BuildItemMetadata.ProjectGuid);

        /// <summary>
        /// This is used to get the project name
        /// </summary>
        [Category("Metadata"), Description("The project name")]
        public string Name => this.GetMetadata(BuildItemMetadata.Name);

        /// <summary>
        /// This is used to get or set whether or not to use the project as a reference or just for MSBuild
        /// dependency determination.
        /// </summary>
        [Category("Metadata"), Description("True to reference the output assembly or false to only use it for " +
          "MSBuild dependency determination"), DefaultValue(true)]
        public bool ReferenceOutputAssembly
        {
            get
            {
                // If not present or valid, default to true
                if(!Boolean.TryParse(this.GetMetadata(BuildItemMetadata.ReferenceOutputAssembly), out bool value))
                    value = true;

                return value;
            }
            set => this.SetMetadata(BuildItemMetadata.ReferenceOutputAssembly, value.ToString().ToLowerInvariant());
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle changes in the <see cref="ProjectPath" /> properties such that the hint path
        /// gets stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void projectPath_PersistablePathChanging(object sender, EventArgs e)
        {
            this.Include = projectPath.PersistablePath;
            this.GetProjectMetadata(true);
            this.OnPropertyChanged(nameof(ProjectPath));
        }

        /// <summary>
        /// This is used to get the build item meta data from the referenced project
        /// </summary>
        /// <param name="refresh">True to force the metadata to be refreshed, false to only retrieve it if it
        /// doesn't exist.</param>
        private void GetProjectMetadata(bool refresh)
        {
            string name;

            if(!refresh && this.HasMetadata(BuildItemMetadata.Name) &&
              this.HasMetadata(BuildItemMetadata.ProjectGuid))
                return;

            using(MSBuildProject project = new MSBuildProject(projectPath))
            {
                project.SetConfiguration(SandcastleProject.DefaultConfiguration,
                    SandcastleProject.DefaultPlatform, null, false);

                name = Path.GetFileNameWithoutExtension(project.AssemblyName);

                if(!String.IsNullOrEmpty(name))
                    this.SetMetadata(BuildItemMetadata.Name, name);
                else
                    this.SetMetadata(BuildItemMetadata.Name, "(Invalid project type)");

                this.SetMetadata(BuildItemMetadata.ProjectGuid, project.ProjectGuid);
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing reference
        /// </summary>
        /// <param name="project">The project that owns the reference</param>
        /// <param name="existingItem">The existing reference</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        internal ProjectReferenceItem(SandcastleProject project, ProjectItem existingItem) : base(project, existingItem)
        {
            projectPath = new FilePath(this.Include, this.Project);
            projectPath.PersistablePathChanging += projectPath_PersistablePathChanging;
            this.GetProjectMetadata(false);
            this.Include = projectPath.PersistablePath;
        }

        /// <summary>
        /// This constructor is used to create a new reference and add it to the project
        /// </summary>
        /// <param name="project">The project that will own the reference</param>
        /// <param name="itemType">The type of reference to create</param>
        /// <param name="itemPath">The path to the reference</param>
        internal ProjectReferenceItem(SandcastleProject project, string itemType, string itemPath) :
          base(project, itemType, itemPath)
        {
            projectPath = new FilePath(this.Include, this.Project);
            projectPath.PersistablePathChanging += projectPath_PersistablePathChanging;
            this.GetProjectMetadata(false);
            this.Include = projectPath.PersistablePath;
        }
        #endregion
    }
}
