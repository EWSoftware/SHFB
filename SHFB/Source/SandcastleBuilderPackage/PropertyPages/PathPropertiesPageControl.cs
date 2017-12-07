//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PathPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 10/31/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Path category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/28/2012  EFW  Updated for use in the standalone GUI
// 12/20/2013  EFW  Added support for the ComponentPath project property
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
// 10/31/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.IO;
using System.Runtime.InteropServices;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.Utils;
using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Path category project properties
    /// </summary>
    [Guid("70E698CB-B4C8-4746-B36F-983D72763543")]
    public partial class PathPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PathPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Paths";
            this.HelpKeyword = "e6fcfa33-e7ee-430a-abfe-6b7962e6d068";
            this.MinimumSize = DetermineMinimumSize(ucPathPropertiesPageContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                SandcastleProject currentProject = null;
                string[] searchFolders;

#if !STANDALONEGUI
                if(this.ProjectMgr != null)
                    currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
                currentProject = this.CurrentProject;
#endif
                // If necessary, force a reset of the shared component cache to reflect changes made to the
                // component path project property.
                if(currentProject != null)
                {
                    FolderPath componentPath = new FolderPath(ucPathPropertiesPageContent.ComponentPath,
                        currentProject);

                    searchFolders = new[] { componentPath.ToString(), Path.GetDirectoryName(currentProject.Filename) };

                    var componentCache = ComponentCache.CreateComponentCache(currentProject.Filename);

                    componentCache.LoadComponentContainer(searchFolders);
                }

                return true;
            }
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            // Set the project as the base path provider so that the folder is correct
#if !STANDALONEGUI
            if(base.ProjectMgr != null)
            {
                SandcastleProject project = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;
#else
            if(base.CurrentProject != null)
            {
                SandcastleProject project = base.CurrentProject;
#endif
                ucPathPropertiesPageContent.SetCurrentProject(project);
            }
        }
        #endregion
    }
}
