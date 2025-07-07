//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : IntelliSenseConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the IntelliSense build component
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/07/2007  EFW  Created the code
// 12/21/2012  EFW  Moved the configuration dialog into the Sandcastle build components assembly
// 12/14/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
// 04/11/2021  EFW  Moved the form to a separate platform-specific assembly
// 04/24/2021  EFW  Added MEF configuration editor export 
//===============================================================================================================

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;

using Sandcastle.Platform.Windows;

namespace Sandcastle.Tools.BuildComponents.UI
{
    /// <summary>
    /// This form is used to configure the settings for the IntelliSense component
    /// </summary>
    public partial class IntelliSenseConfigDlg : Window
    {
        #region Build component configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the component configuration
        /// </summary>
        [ConfigurationEditorExport("IntelliSense Component")]
        public sealed class Factory : IConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(XElement configuration, CompositionContainer container)
            {
                var dlg = new IntelliSenseConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region private data members
        //=====================================================================

        private readonly XElement configuration;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The current configuration element</param>
        public IntelliSenseConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            var settings = configuration.Element("output");

            if(settings != null)
            {
                chkIncludeNamespaces.IsChecked = (bool?)settings.Attribute("includeNamespaces") ?? false;
                txtNamespacesFile.Text = (string)settings.Attribute("namespacesFile");
                txtFolder.Text = (string)settings.Attribute("folder");

                int boundedCapacity = (int?)settings.Attribute("boundedCapacity") ?? -1;

                if(boundedCapacity < 0 || boundedCapacity > 9999)
                    boundedCapacity = 100;

                udcBoundedCapacity.Value = boundedCapacity;
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this component
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("5d546511-6aec-455a-ba09-9daffb124c6d");
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            txtFolder.Text = txtFolder.Text.Trim();
            txtNamespacesFile.Text = txtNamespacesFile.Text.Trim();

            // Store the changes
            configuration.RemoveNodes();

            this.configuration.Add(new XElement("output",
                new XAttribute("includeNamespaces", chkIncludeNamespaces.IsChecked.Value),
                new XAttribute("namespacesFile", txtNamespacesFile.Text),
                new XAttribute("folder", txtFolder.Text),
                new XAttribute("boundedCapacity", udcBoundedCapacity.Value)));

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Select the output folder for the IntelliSense files
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog();
            
            dlg.Description = "Select the IntelliSense output folder";
            dlg.SelectedPath = Directory.GetCurrentDirectory();

            // If selected, set the new folder
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtFolder.Text = dlg.SelectedPath;
        }

        /// <summary>
        /// Go to the Sandcastle Help File Builder project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, RequestNavigateEventArgs e)
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
        #endregion
    }
}
