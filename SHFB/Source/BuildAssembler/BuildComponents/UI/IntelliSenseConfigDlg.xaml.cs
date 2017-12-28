//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : IntelliSenseConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/20/2017
// Note    : Copyright 2006-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
//===============================================================================================================

using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Forms;
using System.Xml.Linq;

using Microsoft.Ddue.Tools.BuildComponent;

namespace Microsoft.Ddue.Tools.UI
{
    /// <summary>
    /// This form is used to configure the settings for the <see cref="IntelliSenseComponent"/>
    /// </summary>
    public partial class IntelliSenseConfigDlg : Window
    {
        #region Private data members
        //=====================================================================

        private XElement config;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration => config.ToString();

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public IntelliSenseConfigDlg(string currentConfig)
        {
            InitializeComponent();

            // Load the current settings
            config = XElement.Parse(currentConfig);
            var settings = config.Element("output");

            if(settings != null)
            {
                chkIncludeNamespaces.IsChecked = ((bool?)settings.Attribute("includeNamespaces") ?? false);
                txtNamespacesFile.Text = (string)settings.Attribute("namespacesFile");
                txtFolder.Text = (string)settings.Attribute("folder");

                int boundedCapacity = ((int?)settings.Attribute("boundedCapacity") ?? -1);

                if(boundedCapacity < 0 || boundedCapacity > 9999)
                    boundedCapacity = 100;

                udcBoundedCapacity.Value = boundedCapacity;
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

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
            config.RemoveNodes();

            config.Add(new XElement("output",
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
            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the IntelliSense output folder";
                dlg.SelectedPath = Directory.GetCurrentDirectory();

                // If selected, set the new folder
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    txtFolder.Text = dlg.SelectedPath + @"\";
            }
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
                System.Windows.MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    "IntelliSense Component", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
