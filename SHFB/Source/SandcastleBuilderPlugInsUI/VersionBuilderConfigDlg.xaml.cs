//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : VersionBuilderConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the version builder plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/01/2007  EFW  Created the code
// 08/13/2008  EFW  Updated to support the new project format
// 06/27/2010  EFW  Added support for /rip option
// 04/27/2021  EFW  Added the configuration editor MEF export attribute
// 05/25/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

using Sandcastle.Platform.Windows;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to edit the version builder plug-in configuration
    /// </summary>
    public partial class VersionBuilderConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Version Builder")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(ISandcastleProject project, XElement configuration)
            {
                var dlg = new VersionBuilderConfigDlg(project, configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;
        private readonly ISandcastleProject project;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The current project</param>
        /// <param name="configuration">The current configuration element</param>
        public VersionBuilderConfigDlg(ISandcastleProject project, XElement configuration)
        {
            InitializeComponent();

            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if(!configuration.IsEmpty)
            {
                var currentProject = configuration.Element("currentProject");

                if(currentProject != null)
                {
                    txtLabel.Text = currentProject.Attribute("label").Value;
                    txtVersion.Text = currentProject.Attribute("version").Value;
                    chkRipOldAPIs.IsChecked = (bool)currentProject.Attribute("ripOldApis");
                }

                // Load the current settings
                foreach(var reference in configuration.Descendants("version"))
                    lbVersionInfo.Items.Add(VersionSettings.FromXml(project, reference));
            }

            btnDeleteProject.IsEnabled = grpVersionInfoProps.IsEnabled = lbVersionInfo.Items.Count != 0;

            if(lbVersionInfo.Items.Count != 0)
                lbVersionInfo.SelectedIndex = 0;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this plug-in
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("6c03afba-18d0-4270-b521-c2015c4d97b3");
        }

        /// <summary>
        /// Go to the project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Enable or disable controls based on whether or not there are references
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbVersionInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDeleteProject.IsEnabled = grpVersionInfoProps.IsEnabled = lbVersionInfo.SelectedIndex != -1;
        }

        /// <summary>
        /// Add a new version info project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddProject_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new System.Windows.Forms.OpenFileDialog();
            
            dlg.Title = "Select the help file builder project(s)";
            dlg.Filter = "Sandcastle Help File Builder Project Files " +
                "(*.shfbproj)|*.shfbproj|All Files (*.*)|*.*";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.DefaultExt = "shfbproj";
            dlg.Multiselect = true;

            // If selected, add the file(s)
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach(string file in dlg.FileNames)
                {
                    var newItem = new VersionSettings(project)
                    {
                        HelpFileProject = new FilePath(file, project),
                        FrameworkLabel = txtLabel.Text,
                        Version = "1.0"
                    };

                    // It will end up on the last one added
                    lbVersionInfo.SelectedIndex = lbVersionInfo.Items.Add(newItem);
                }

                btnDeleteProject.IsEnabled = grpVersionInfoProps.IsEnabled = true;
            }
        }

        /// <summary>
        /// Delete the selected version info project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbVersionInfo.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the version information project '" +
              ((ReferenceLinkSettings)lbVersionInfo.SelectedItem).HelpFileProject + "'?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
              MessageBoxResult.Yes)
            {
                lbVersionInfo.Items.RemoveAt(idx);

                if(lbVersionInfo.Items.Count == 0)
                    btnDeleteProject.IsEnabled = grpVersionInfoProps.IsEnabled = false;
                else
                {
                    if(idx < lbVersionInfo.Items.Count)
                        lbVersionInfo.SelectedIndex = idx;
                    else
                        lbVersionInfo.SelectedIndex = lbVersionInfo.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Validate and save the settings
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var versionInfo = lbVersionInfo.Items.Cast<VersionSettings>().ToList();

            if(versionInfo.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more reference link projects are invalid.  Please fix them before saving them.",
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // There can't be any duplicate projects or label/version combinations
            var duplicateProjects = versionInfo.GroupBy(r => r.HelpFileProject.Path).Where(g => g.Count() != 1).ToList();
            var duplicateIDs = versionInfo.GroupBy(r => r.UniqueId).Where(g => g.Count() != 1).ToList();
            var keys = new HashSet<int>(duplicateIDs.Select(g => g.Key));
            var dups = versionInfo.Where(v => keys.Contains(v.UniqueId)).Select(
                v => $"{v.FrameworkLabel} {v.Version}").Distinct().ToList();

            if(versionInfo.Any(v => v.FrameworkLabel.Equals(txtLabel.Text, StringComparison.Ordinal) &&
              v.Version.Equals(txtVersion.Text, StringComparison.Ordinal)))
            {
                keys.Add((txtLabel.Text + txtVersion.Text).GetHashCode());
                dups.Insert(0, $"{txtLabel.Text} {txtVersion.Text}");
            }

            if(duplicateProjects.Count != 0)
            {
                MessageBox.Show("The same help file project cannot be specified more than once: " +
                    String.Join(", ", duplicateProjects.Select(g => g.Key)), Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            if(keys.Count != 0)
            {
                MessageBox.Show("The same framework label and version cannot be specified more than once: " +
                    String.Join(", ", dups), Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(
                new XElement("currentProject",
                    new XAttribute("label", txtLabel.Text),
                    new XAttribute("version", txtVersion.Text),
                    new XAttribute("ripOldApis", chkRipOldAPIs.IsChecked)),
                new XElement("versions", versionInfo.Select(r => r.ToXml())));

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
