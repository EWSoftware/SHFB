//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalReferenceLinksConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the additional reference links plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/25/2008  EFW  Created the code
// 08/13/2008  EFW  Updated to support the new project format
// 04/26/2021  EFW  Added the configuration editor MEF export attribute
// 05/21/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
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
    /// This form is used to edit the additional reference links plug-in configuration
    /// </summary>
    public partial class AdditionalReferenceLinksConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Additional Reference Links")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(ISandcastleProject project, XElement configuration)
            {
                var dlg = new AdditionalReferenceLinksConfigDlg(project, configuration);

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
        public AdditionalReferenceLinksConfigDlg(ISandcastleProject project, XElement configuration)
        {
            InitializeComponent();

            cboHtmlSdkLinkType.ItemsSource = cboWebsiteSdkLinkType.ItemsSource = (new Dictionary<string, string> {
                { HtmlSdkLinkType.Msdn.ToString(), "Links to online help topics" },
                { HtmlSdkLinkType.None.ToString(), "No SDK links" } }).ToList();
            cboMSHelpViewerSdkLinkType.ItemsSource = (new Dictionary<string, string> {
                { MSHelpViewerSdkLinkType.Msdn.ToString(), "Links to online help topics" },
                { MSHelpViewerSdkLinkType.Id.ToString(), "ID links within the collection" },
                { MSHelpViewerSdkLinkType.None.ToString(), "No SDK links" } }).ToList();

            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if(!configuration.IsEmpty)
            {
                // Load the current settings
                foreach(var reference in configuration.Descendants("target"))
                    lbReferences.Items.Add(ReferenceLinkSettings.FromXml(project, reference));
            }

            btnDeleteReferenceProject.IsEnabled = grpReferenceProps.IsEnabled = lbReferences.Items.Count != 0;

            if(lbReferences.Items.Count != 0)
                lbReferences.SelectedIndex = 0;
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
            UiUtility.ShowHelpTopic("15b6b7be-3778-4487-b524-d558d02e6460");
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
        private void lbReferences_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDeleteReferenceProject.IsEnabled = grpReferenceProps.IsEnabled = lbReferences.SelectedIndex != -1;
        }

        /// <summary>
        /// Add a new reference links project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddReferenceProject_Click(object sender, RoutedEventArgs e)
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
                    var newItem = new ReferenceLinkSettings(project)
                    {
                        HelpFileProject = new FilePath(file, project)
                    };

                    // It will end up on the last one added
                    lbReferences.SelectedIndex = lbReferences.Items.Add(newItem);
                }

                btnDeleteReferenceProject.IsEnabled = grpReferenceProps.IsEnabled = true;
            }
        }

        /// <summary>
        /// Delete the selected reference links project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteReferenceProject_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbReferences.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the reference links project '" +
              ((ReferenceLinkSettings)lbReferences.SelectedItem).HelpFileProject + "'?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
              MessageBoxResult.Yes)
            {
                lbReferences.Items.RemoveAt(idx);

                if(lbReferences.Items.Count == 0)
                    btnDeleteReferenceProject.IsEnabled = grpReferenceProps.IsEnabled = false;
                else
                {
                    if(idx < lbReferences.Items.Count)
                        lbReferences.SelectedIndex = idx;
                    else
                        lbReferences.SelectedIndex = lbReferences.Items.Count - 1;
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
            var references = lbReferences.Items.Cast<ReferenceLinkSettings>().ToList();

            if(references.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more reference link projects are invalid.  Please fix them before saving them.",
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // There can't be duplicate IDs or projects
            var duplicates = references.GroupBy(r => r.HelpFileProject.Path).Where(g => g.Count() != 1).ToList();

            if(duplicates.Count != 0)
            {
                MessageBox.Show("The same help file project cannot be specified more than once: " +
                    String.Join(", ", duplicates.Select(g => g.Key)), Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(new XElement("targets", references.Select(r => r.ToXml())));

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
