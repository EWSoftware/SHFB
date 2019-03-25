//===============================================================================================================
// System  : Sandcastle Help File Builder Project Launcher
// File    : ProjectLauncherForm.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/22/2019
// Note    : Copyright 2011-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This lets the user choose how to launch help file builder projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/19/2011  EFW  Created the code
// 12/12/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Windows;

using SandcastleBuilder.ProjectLauncher.Properties;

namespace SandcastleBuilder.ProjectLauncher
{
    /// <summary>
    /// This lets the user choose how to launch help file builder projects.
    /// </summary>
    public partial class ProjectLauncherDlg : Window
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectLauncherDlg()
        {
            InitializeComponent();

            if(Settings.Default.UseStandaloneGui || String.IsNullOrWhiteSpace(StartUp.VisualStudioPath))
                rbSHFB.IsChecked = true;
            else
                rbVisualStudio.IsChecked = true;

            chkAlwaysUseSelection.IsChecked = Settings.Default.AlwaysUseSelection;
            btnLaunch.IsEnabled = !String.IsNullOrWhiteSpace(StartUp.ProjectToLoad);

            if(String.IsNullOrWhiteSpace(StartUp.VisualStudioPath))
                rbVisualStudio.IsEnabled = false;
            else
                lblNotInstalled.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close the form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// When closing, save any changes to the user settings
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ProjectLauncherDlg_Closing(object sender, CancelEventArgs e)
        {
            if((rbSHFB.IsChecked.Value && !Settings.Default.UseStandaloneGui) ||
              (!rbSHFB.IsChecked.Value && Settings.Default.UseStandaloneGui) ||
              chkAlwaysUseSelection.IsChecked != Settings.Default.AlwaysUseSelection)
            {
                Settings.Default.AlwaysUseSelection = chkAlwaysUseSelection.IsChecked.Value;
                Settings.Default.UseStandaloneGui = rbSHFB.IsChecked.Value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Open the project with the selected application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnLaunch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(StartUp.LaunchWithSelectedApplication(rbSHFB.IsChecked.Value))
                this.Close();
        }

        /// <summary>
        /// Keep the radio button states consistent
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void AppSelection_Click(object sender, RoutedEventArgs e)
        {
            if(sender == rbSHFB)
            {
                rbSHFB.IsChecked = true;
                rbVisualStudio.IsChecked = false;
            }
            else
            {
                rbSHFB.IsChecked = false;
                rbVisualStudio.IsChecked = true;
            }
        }
        #endregion
    }
}
