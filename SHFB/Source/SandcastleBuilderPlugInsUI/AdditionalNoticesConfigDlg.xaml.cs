//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : AdditionalNoticesConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/26/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the additional notices plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/26/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle.Transformation;
using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to edit the additional notices plug-in configuration
    /// </summary>
    public partial class AdditionalNoticesConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Additional Notices")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new AdditionalNoticesConfigDlg(project, configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;
        private readonly SandcastleProject project;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The current project</param>
        /// <param name="configuration">The current configuration element</param>
        public AdditionalNoticesConfigDlg(SandcastleProject project, XElement configuration)
        {
            InitializeComponent();

            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if(!configuration.IsEmpty)
            {
                // Load the current settings
                foreach(var n in configuration.Descendants("Notice"))
                    lbNotices.Items.Add(Notice.FromXml(n));
            }

            btnDeleteNotice.IsEnabled = grpNoticeProps.IsEnabled = lbNotices.Items.Count != 0;

            if(lbNotices.Items.Count != 0)
                lbNotices.SelectedIndex = 0;
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
            UiUtility.ShowHelpTopic("f238b7a7-24eb-4163-8083-f35b6a60a3d1");
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
        /// Enable or disable controls based on whether or not there are notice definitions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbNotices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDeleteNotice.IsEnabled = grpNoticeProps.IsEnabled = lbNotices.SelectedIndex != -1;
        }

        /// <summary>
        /// Add a new notice definition
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddNotice_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new Notice
            {
                AttributeTypeName = "T:MyNamespace.MyAttribute",
                NoticeMessage = "A long description for topics",
                TagText = "List Tag",
            };

            lbNotices.SelectedIndex = lbNotices.Items.Add(newItem);
            btnDeleteNotice.IsEnabled = grpNoticeProps.IsEnabled = true;
        }

        /// <summary>
        /// Delete the selected notice definition
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteNotice_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbNotices.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the notice definition '" +
              ((Notice)lbNotices.SelectedItem).NoticeDescription + "'?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
              MessageBoxResult.Yes)
            {
                lbNotices.Items.RemoveAt(idx);

                if(lbNotices.Items.Count == 0)
                    btnDeleteNotice.IsEnabled = grpNoticeProps.IsEnabled = false;
                else
                {
                    if(idx < lbNotices.Items.Count)
                        lbNotices.SelectedIndex = idx;
                    else
                        lbNotices.SelectedIndex = lbNotices.Items.Count - 1;
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
            var notices = lbNotices.Items.Cast<Notice>().ToList();

            if(notices.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more notice definitions are invalid.  Please fix them before saving them.",
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(new XElement("Notices", notices.Select(r => r.ToXml())));

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
