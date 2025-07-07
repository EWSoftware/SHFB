//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : WildcardReferencesConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the wildcard references plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/17/2011  EFW  Created the code
// 05/14/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
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
    /// This form is used to edit the wildcard references plug-in configuration
    /// </summary>
    public partial class WildcardReferencesConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Wildcard Assembly References")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(ISandcastleProject project, XElement configuration)
            {
                var dlg = new WildcardReferencesConfigDlg(project, configuration);

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
        public WildcardReferencesConfigDlg(ISandcastleProject project, XElement configuration)
        {
            InitializeComponent();

            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if(!configuration.IsEmpty)
            {
                // Load the current settings
                foreach(var reference in configuration.Descendants("reference"))
                    lbReferences.Items.Add(WildcardReferenceSettings.FromXml(project, reference));
            }

            btnDeleteReferencePath.IsEnabled = grpReferenceProps.IsEnabled = lbReferences.Items.Count != 0;

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
            UiUtility.ShowHelpTopic("96557037-c19e-4183-bcf1-f42d7018de9f");
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
            btnDeleteReferencePath.IsEnabled = grpReferenceProps.IsEnabled = lbReferences.SelectedIndex != -1;
        }

        /// <summary>
        /// Add a new wildcard reference path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddReferencePath_Click(object sender, RoutedEventArgs e)
        {
            WildcardReferenceSettings newItem;

            using var dlg = new System.Windows.Forms.FolderBrowserDialog();
            
            dlg.Description = "Select the folder for the new project";
            dlg.SelectedPath = Directory.GetCurrentDirectory();

            // If selected, add the file(s)
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                newItem = new WildcardReferenceSettings
                {
                    ReferencePath = new FolderPath(dlg.SelectedPath, project)
                };

                lbReferences.SelectedIndex = lbReferences.Items.Add(newItem);
                btnDeleteReferencePath.IsEnabled = grpReferenceProps.IsEnabled = true;
            }
        }

        /// <summary>
        /// Delete the selected wildcard reference path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteReferencePath_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbReferences.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the wildcard reference '" +
              ((WildcardReferenceSettings)lbReferences.SelectedItem).ReferenceDescription + "'?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
              MessageBoxResult.Yes)
            {
                lbReferences.Items.RemoveAt(idx);

                if(lbReferences.Items.Count == 0)
                    btnDeleteReferencePath.IsEnabled = grpReferenceProps.IsEnabled = false;
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
            var references = lbReferences.Items.Cast<WildcardReferenceSettings>().ToList();

            if(references.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more wildcard references are invalid.  Please fix them before saving them.",
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(new XElement("references", references.Select(r => r.ToXml())));

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
