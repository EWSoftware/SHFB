//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProjectReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/16/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a project reference item that can be
// used by MRefBuilder to locate assembly dependencies for the assemblies being
// documented.
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
// 1.8.0.0  06/30/2008  EFW  Rewrote to support the MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;

using SandcastleBuilder.Utils.Design;
using SandcastleBuilder.Utils.MSBuild;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents a project reference item that can be used by
    /// <b>MRefBuilder</b> to locate assembly dependencies for the assemblies
    /// being documented.
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
          FileDialog("Select the reference project",
            "Visual Studio Projects (*.*proj)|*.*proj|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public FilePath ProjectPath
        {
            get { return projectPath; }
            set
            {
                if(value == null || value.Path.Length == 0 ||
                  value.Path.IndexOfAny(new char[] { '*', '?' }) != -1)
                    throw new ArgumentException("A project path must be " +
                        "specified and cannot contain wildcards (* or ?)",
                        "value");

                // Do this first in case the project isn't editable
                base.ProjectElement.Include = value.PersistablePath;

                projectPath = value;
                projectPath.PersistablePathChanging += new EventHandler(
                    projectPath_PersistablePathChanging);
                this.GetProjectMetadata(true);
            }
        }

        /// <summary>
        /// Hint path isn't applicable to project references
        /// </summary>
        [Browsable(false)]
        public override FilePath HintPath
        {
            get { return base.HintPath; }
            set { }
        }

        /// <summary>
        /// This is overridden to return the project name rather than the
        /// file path.
        /// </summary>
        public override string Reference
        {
            get { return this.Name; }
        }

        /// <summary>
        /// This is used to get the project reference's GUID
        /// </summary>
        [Category("Metadata"), Description("The project reference's GUID")]
        public string Project
        {
            get
            {
                return base.ProjectElement.GetMetadata(
                    ProjectElement.ProjectGuid);
            }
        }

        /// <summary>
        /// This is used to get the project name
        /// </summary>
        [Category("Metadata"), Description("The project name")]
        public string Name
        {
            get
            {
                return base.ProjectElement.GetMetadata(ProjectElement.Name);
            }
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle changes in the <see cref="ProjectPath" />
        /// properties such that the hint path gets stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void projectPath_PersistablePathChanging(object sender,
          EventArgs e)
        {
            base.ProjectElement.Include = projectPath.PersistablePath;
            this.GetProjectMetadata(true);
        }

        /// <summary>
        /// This is used to get the build item meta data from the referenced
        /// project.
        /// </summary>
        /// <param name="refresh">True to force the metadata to be refreshed,
        /// false to only retrieve it if it doesn't exist.</param>
        private void GetProjectMetadata(bool refresh)
        {
            string name;

            if(!refresh && base.ProjectElement.HasMetadata(
              ProjectElement.Name) && base.ProjectElement.HasMetadata(
              ProjectElement.ProjectGuid))
                return;

            MSBuildProject project = new MSBuildProject(projectPath);
            project.SetConfiguration(SandcastleProject.DefaultConfiguration,
                SandcastleProject.DefaultPlatform, null);

            name = Path.GetFileNameWithoutExtension(project.AssemblyName);

            if(!String.IsNullOrEmpty(name))
                base.ProjectElement.SetMetadata(ProjectElement.Name, name);
            else
                base.ProjectElement.SetMetadata(ProjectElement.Name,
                    "(Invalid project type)");

            base.ProjectElement.SetMetadata(ProjectElement.ProjectGuid,
                project.ProjectGuid);
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal Constructor
        /// </summary>
        /// <param name="element">The project element</param>
        internal ProjectReferenceItem(ProjectElement element) : base(element)
        {
            projectPath = new FilePath(base.ProjectElement.Include,
                base.ProjectElement.Project);
            projectPath.PersistablePathChanging += new EventHandler(
                projectPath_PersistablePathChanging);
            this.GetProjectMetadata(false);
            base.ProjectElement.Include = projectPath.PersistablePath;
        }
        #endregion
    }
}
