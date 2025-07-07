//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : SelectProjectsDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains a form used to indicate whether to add the solution or just selected projects from within
// it as documentation sources.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and ca be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/25/2014  EFW  Created the code
// 12/05/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.MSBuild.HelpProject;

namespace SandcastleBuilder.WPF.UI
{
    /// <summary>
    /// This form is used to indicate whether to add the solution or just selected projects from within it as
    /// documentation sources.
    /// </summary>
    public partial class SelectProjectsDlg : Window
    {
        #region Project item for the list box
        //=====================================================================

        /// <summary>
        /// This is used to select projects in the checked list box
        /// </summary>
        private class ProjectItem
        {
            /// <summary>
            /// True if selected, false if not
            /// </summary>
            public bool IsSelected { get; set; }

            /// <summary>
            /// The project filename
            /// </summary>
            public string ProjectFilename { get; set; }
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get an enumerable list containing either the solution name or the selected projects
        /// </summary>
        public IEnumerable<string> SolutionOrProjectNames
        {
            get
            {
                string solutionPath = Path.GetDirectoryName(lblSolutionName.Text);

                if(rbAddSolution.IsChecked ?? false)
                    yield return lblSolutionName.Text;
                else
                {
                    if(!Path.IsPathRooted(solutionPath))
                        solutionPath = Path.GetFullPath(solutionPath);

                    foreach(ProjectItem project in lbProjects.Items)
                    {
                        if(project.IsSelected)
                        {
                            if(!Path.IsPathRooted(project.ProjectFilename))
                                yield return Path.GetFullPath(Path.Combine(solutionPath, project.ProjectFilename));
                            else
                                yield return project.ProjectFilename;
                        }
                    }
                }
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SelectProjectsDlg()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This loads the dialog with the available projects from the given solution file
        /// </summary>
        /// <param name="solutionName">The solution filename to use</param>
        public void LoadSolutionProjectNames(string solutionName)
        {
            lblSolutionName.Text = solutionName;

            var sf = new SolutionFile(solutionName);

            foreach(var p in sf.EnumerateProjectFiles().OrderBy(p => p.ProjectFileName))
                lbProjects.Items.Add(new ProjectItem { ProjectFilename = p.ProjectFileName });
        }

        /// <summary>
        /// This is used to prompt the user whether to add the solution or just selected projects from it as
        /// documentation sources.
        /// </summary>
        /// <param name="solutionName">The solution filename to use</param>
        /// <returns>An enumerable list containing either the solution name or the selected projects</returns>
        public static IEnumerable<string> SelectSolutionOrProjects(string solutionName)
        {
            SelectProjectsDlg dlg = new();

            dlg.LoadSolutionProjectNames(solutionName);

            if(dlg.ShowModalDialog() ?? false)
            {
                foreach(string filename in dlg.SolutionOrProjectNames)
                    yield return filename;
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// When a project is selected, select the "projects" radio button by default
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void project_Checked(object sender, RoutedEventArgs e)
        {
            rbAddProjects.IsChecked = true;
        }

        /// <summary>
        /// Close the form and use the selection(s)
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        #endregion
    }
}
