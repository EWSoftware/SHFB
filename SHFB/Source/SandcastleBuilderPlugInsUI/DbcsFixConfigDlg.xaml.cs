//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DbcsFixConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/05/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the DBCS Fix plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/18/2008  EFW  Created the code
// 07/31/2014  EFW  Made the localize app optional
// 05/05/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

// Ignore Spelling: exe

using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to configure the settings for the DBCS Fix for CHM Builds plug-in
    /// </summary>
    public partial class DbcsFixConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("DBCS Fix for CHM Builds")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new DbcsFixConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The current configuration element</param>
        public DbcsFixConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            var node = configuration.Element("sbAppLocale");

            if(node != null)
                txtSBAppLocalePath.Text = node.Attribute("path")?.Value;
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
            UiUtility.ShowHelpTopic("31696f39-8f4e-4c4d-ab08-41a40793df03");
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
        /// Select the path to the SBAppLocale tool
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectLocation_Click(object sender, RoutedEventArgs e)
        {
            using(var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = "Select the Steel Bytes AppLocale Executable";
                dlg.Filter = "Executable files (*.exe)|*.exe|All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "exe";

                // If one is selected, use that file
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    txtSBAppLocalePath.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            txtSBAppLocalePath.Text = txtSBAppLocalePath.Text.Trim();

            if(txtSBAppLocalePath.Text.Length == 0)
            {
                txtSBAppLocalePath.SetValidationState(false, "The path to the bibliography XML file is required");
                return;
            }

            txtSBAppLocalePath.SetValidationState(true, null);

            // Store the changes
            var node = configuration.Element("sbAppLocale");

            if(node == null)
            {
                node = new XElement("sbAppLocale", new XAttribute("path", String.Empty));
                configuration.Add(node);
            }

            node.Attribute("path").Value = txtSBAppLocalePath.Text;

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
