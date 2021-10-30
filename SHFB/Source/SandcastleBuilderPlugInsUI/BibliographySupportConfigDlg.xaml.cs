//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BibliographySupportConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/27/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the bibliography support plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/18/2008  EFW  Created the code
// 04/27/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to configure the settings for the Bibliography Support plug-in
    /// </summary>
    public partial class BibliographySupportConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Bibliography Support")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new BibliographySupportConfigDlg(configuration);

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
        public BibliographySupportConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            var node = configuration.Element("bibliography");

            if(node != null)
                txtBibliographyFile.Text = node.Attribute("path")?.Value;
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
            UiUtility.ShowHelpTopic("161537d9-6f89-42ef-9c51-3a15ef94df65");
        }

        /// <summary>
        /// Go to the Sandcastle Help File Builder project site
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
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, "Bibliography Support Plug-In",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Select the path to the bibliography file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectLocation_Click(object sender, RoutedEventArgs e)
        {
            using(var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = "Select the bibliography file";
                dlg.Filter = "Bibliography files (*.xml)|*.xml|All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "xml";

                // If one is selected, use that file
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    txtBibliographyFile.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            txtBibliographyFile.Text = txtBibliographyFile.Text.Trim();

            if(txtBibliographyFile.Text.Length == 0)
            {
                txtBibliographyFile.SetValidationState(false, "The path to the bibliography XML file is required");
                return;
            }

            txtBibliographyFile.SetValidationState(true, null);

            // Store the changes
            var node = configuration.Element("bibliography");

            if(node == null)
            {
                node = new XElement("bibliography", new XAttribute("path", String.Empty));
                configuration.Add(node);
            }

            node.Attribute("path").Value = txtBibliographyFile.Text;

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
