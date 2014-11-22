//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : SelectProjectsDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/25/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form used to indicate whether to add the solution or just selected projects from within
// it as documentation sources.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/25/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This form is used to indicate whether to add the solution or just selected projects from within it as
    /// documentation sources.
    /// </summary>
    public partial class SelectProjectsDlg : Form
    {
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

                if(rbAddSolution.Checked)
                    yield return lblSolutionName.Text;
                else
                {
                    if(!Path.IsPathRooted(solutionPath))
                        solutionPath = Path.GetFullPath(solutionPath);

                    foreach(string projectName in cblProjects.CheckedItems)
                        if(!Path.IsPathRooted(projectName))
                            yield return Path.GetFullPath(Path.Combine(solutionPath, projectName));
                        else
                            yield return projectName;
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

            foreach(string project in DocumentationSource.ProjectsIn(solutionName).OrderBy(p => p))
                cblProjects.Items.Add(project);
        }

        /// <summary>
        /// This is used to prompt the user whether to add the solution or just selected projects from it as
        /// documentation sources.
        /// </summary>
        /// <param name="solutionName">The solution filename to use</param>
        /// <returns>An enumerable list containing either the solution name or the selected projects</returns>
        public static IEnumerable<string> SelectSolutionOrProjects(string solutionName)
        {
            using(SelectProjectsDlg dlg = new SelectProjectsDlg())
            {
                dlg.LoadSolutionProjectNames(solutionName);

                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach(string filename in dlg.SolutionOrProjectNames)
                        yield return filename;
                }
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// When an item is checked, select the "projects" radio button by default
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cblProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cblProjects.CheckedIndices.Contains(cblProjects.SelectedIndex))
                rbAddProjects.Checked = true;
        }
        #endregion
    }
}
