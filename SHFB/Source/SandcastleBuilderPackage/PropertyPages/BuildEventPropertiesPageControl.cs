//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildEventPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/20/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Build Events category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/19/2014  EFW  Created the code
// 10/04/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Build Events category project properties
    /// </summary>
    [Guid("88926D2F-6A3F-440C-929D-144888C8EFFD")]
    public partial class BuildEventPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildEventPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Build Events";
            this.HelpKeyword = "682c2e1c-54d2-4128-80ff-f6dc63d2f58d";
            this.MinimumSize = DetermineMinimumSize(ucBuildEventPropertiesContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

#if !STANDALONEGUI
            ucBuildEventPropertiesContent.Project = this.ProjectMgr.BuildProject;
#else
            ucBuildEventPropertiesContent.Project = this.CurrentProject.MSBuildProject;
#endif
        }
        #endregion
    }
}
