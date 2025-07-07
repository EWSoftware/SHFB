//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : PathPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 06/19/2025
// Note    : Copyright 2017-2025, Eric Woodruff, All rights reserved
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
// 10/31/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Windows.Controls;

using Sandcastle.Core;
using Sandcastle.Core.Project;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Path category properties
    /// </summary>
    public partial class PathPropertiesPageContent : UserControl
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the current component path
        /// </summary>
        public string ComponentPath => fpComponentPath.PersistablePath;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PathPropertiesPageContent()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to set the current project in each of the folder paths
        /// </summary>
        /// <param name="project">The current Sandcastle project</param>
        public void SetCurrentProject(ISandcastleProject project)
        {
            fpHtmlHelp1xCompilerPath.DataContext = new FolderPath(project);
            fpComponentPath.DataContext = new FolderPath(project);
            fpSourceCodeBasePath.DataContext = new FolderPath(project);
            fpOutputPath.DataContext = new FolderPath(project);
            fpWorkingPath.DataContext = new FolderPath(project);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// If the Output Path is blank, default it to the .\Help folder under the project's path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void fpOutputPath_PersistablePathChanged(object sender, EventArgs e)
        {
            if(String.IsNullOrWhiteSpace(fpOutputPath.PersistablePath))
                fpOutputPath.PersistablePath = @".\Help";
        }
        #endregion
    }
}
