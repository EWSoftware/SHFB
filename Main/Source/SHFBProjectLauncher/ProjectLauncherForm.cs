//=============================================================================
// System  : Sandcastle Help File Builder Project Launcher
// File    : ProjectLauncherForm.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This lets the user choose how to launch help file builder projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.1  04/19/2011  EFW  Created the code
//=============================================================================

using System;
using System.Windows.Forms;

using SandcastleBuilder.ProjectLauncher.Properties;

namespace SandcastleBuilder.ProjectLauncher
{
    /// <summary>
    /// This lets the user choose how to launch help file builder projects.
    /// </summary>
    public partial class ProjectLauncherForm : Form
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectLauncherForm()
        {
            InitializeComponent();

            if(Settings.Default.UseStandaloneGui || String.IsNullOrEmpty(StartUp.VisualStudioPath))
                rbSHFB.Checked = true;
            else
                rbVisualStudio.Checked = true;

            chkAlwaysUseSelection.Checked = Settings.Default.AlwaysUseSelection;
            btnLaunch.Enabled = !String.IsNullOrEmpty(StartUp.ProjectToLoad);

            if(String.IsNullOrEmpty(StartUp.VisualStudioPath))
            {
                rbVisualStudio.Enabled = false;
                lblNotInstalled.Visible = true;
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// When closing, save any changes to the user settings
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ProjectLauncherForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if((rbSHFB.Checked && !Settings.Default.UseStandaloneGui) ||
              (!rbSHFB.Checked && Settings.Default.UseStandaloneGui) ||
              chkAlwaysUseSelection.Checked != Settings.Default.AlwaysUseSelection)
            {
                Settings.Default.AlwaysUseSelection = chkAlwaysUseSelection.Checked;
                Settings.Default.UseStandaloneGui = rbSHFB.Checked;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Close without opening the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Open the project with the selected application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if(StartUp.LaunchWithSelectedApplication(rbSHFB.Checked))
                this.Close();
        }
        #endregion
    }
}
