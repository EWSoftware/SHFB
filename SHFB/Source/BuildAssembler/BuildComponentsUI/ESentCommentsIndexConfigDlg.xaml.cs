//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentCommentsIndexConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the ESENT Copy From Index component
// (Comments Index Data).
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/12/2013  EFW  Created the code
// 04/26/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;

using Sandcastle.Platform.Windows;

namespace Sandcastle.Tools.BuildComponents.UI
{
    /// <summary>
    /// This form is used to configure the ESENT Copy From Index component (Comments Index Data)
    /// </summary>
    public partial class ESentCommentsIndexConfigDlg : Window
    {
        #region Build component configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the component configuration
        /// </summary>
        [ConfigurationEditorExport("Comments Index Data (ESENT Cache)")]
        public sealed class CommentsIndexDataFactory : IConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(XElement configuration, CompositionContainer container)
            {
                var dlg = new ESentCommentsIndexConfigDlg(configuration);

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
        public ESentCommentsIndexConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            var node = configuration.Descendants("index").First();

            // Set this first as it may override the value set below
            chkEnableLocalCache.IsChecked = !String.IsNullOrWhiteSpace(txtProjectIndexCachePath.Text);

            if(!(chkEnableLocalCache.IsChecked ?? false))
                this.chkEnableLocalCache_CheckedChanged(this, null);

            udcInMemoryCacheSize.Value = (int?)node.Attribute("cache") ?? 15;
            udcLocalCacheSize.Value = (int?)node.Attribute("localCacheSize") ?? 2500;
            txtFrameworkIndexCachePath.Text = node.Attribute("frameworkCachePath").Value;
            txtProjectIndexCachePath.Text = (string)node.Attribute("projectCachePath");
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Resolve the given path by replacing LocalDataFolder with the actual path and seeing if it exists
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>The resolved path if it exists, or null if it does not</returns>
        private static string ResolvePath(string path)
        {
            string localDataFolder = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), Constants.ProgramDataFolder);
            int pos = path.IndexOf("{@LocalDataFolder}", StringComparison.OrdinalIgnoreCase);

            if(pos != -1)
            {
                pos += 18;
                path = Path.Combine(localDataFolder, path.Substring(pos));
            }

            if(path.Length != 0 && path[path.Length - 1] == Path.DirectorySeparatorChar)
                path = path.Substring(0, path.Length - 1);

            if(Directory.Exists(path))
                return path;

            return null;
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
            UiUtility.ShowHelpTopic("3a1c4bf1-8ecf-4ab3-8010-277bed8d3819");
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
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Select a cache folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectCacheFolder_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            TextBox t;

            using(var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if(b == btnSelectFrameworkIndexCacheFolder)
                {
                    t = txtFrameworkIndexCachePath;
                    dlg.Description = "Select the Framework comments index cache folder";
                }
                else
                {
                    t = txtProjectIndexCachePath;
                    dlg.Description = "Select the current project comments index cache folder";
                }

                dlg.SelectedPath = (ResolvePath(t.Text) ?? Directory.GetCurrentDirectory());

                // If selected, set the new folder
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    t.Text = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Enable or disable current project target caching
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkEnableLocalCache_CheckedChanged(object sender, RoutedEventArgs e)
        {
            txtProjectIndexCachePath.IsEnabled = btnSelectProjectIndexCacheFolder.IsEnabled =
                chkEnableLocalCache.IsChecked ?? false;

            if(!chkEnableLocalCache.IsChecked ?? false)
                txtProjectIndexCachePath.Text = null;
            else
                txtProjectIndexCachePath.Text = "{@WorkingFolder}CommentsIndexCache";
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            txtFrameworkIndexCachePath.Text = txtFrameworkIndexCachePath.Text.Trim();
            txtProjectIndexCachePath.Text = txtProjectIndexCachePath.Text.Trim();

            txtFrameworkIndexCachePath.SetValidationState(true, null);
            txtProjectIndexCachePath.SetValidationState(true, null);

            if(txtFrameworkIndexCachePath.Text.Length == 0)
            {
                txtFrameworkIndexCachePath.SetValidationState(false, "The Framework comments index cache path " +
                    "is required");
                isValid = false;
            }
            else
            {
                if(txtFrameworkIndexCachePath.Text == txtProjectIndexCachePath.Text)
                {
                    txtFrameworkIndexCachePath.SetValidationState(false, "The Framework comments index cache " +
                        "path must be unique");
                    isValid = false;
                }
            }

            if((chkEnableLocalCache.IsChecked ?? false) && txtProjectIndexCachePath.Text.Length == 0)
            {
                txtProjectIndexCachePath.SetValidationState(false, "The project comments index cache path is " +
                    "required if enabled");
                isValid = false;
            }

            if(isValid)
            {
                var node = configuration.Descendants("index").First();

                node.SetAttributeValue("cache", (int)udcInMemoryCacheSize.Value);
                node.SetAttributeValue("localCacheSize", (int)udcLocalCacheSize.Value);
                node.SetAttributeValue("frameworkCachePath", txtFrameworkIndexCachePath.Text);
                node.SetAttributeValue("projectCachePath", txtProjectIndexCachePath.Text);

                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Purge the content ID and target cache folders
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnPurge_Click(object sender, RoutedEventArgs e)
        {
            string[] allPaths = new[] { txtFrameworkIndexCachePath.Text, txtProjectIndexCachePath.Text };
            string resolvedPath = null;

            if(MessageBox.Show("WARNING: This will delete all of the current ESENT comments index cache " +
              "folders.  The information will need to be created the next time this project is built.  " +
              "Are you sure you want to delete them?", Constants.AppName, MessageBoxButton.YesNo,
              MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                foreach(string path in allPaths)
                {
                    resolvedPath = ResolvePath(path);

                    if(resolvedPath != null && Directory.Exists(resolvedPath))
                        Directory.Delete(resolvedPath, true);
                }

                MessageBox.Show("The cache folders have been deleted", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch(IOException ex)
            {
                MessageBox.Show("Unable to resolve or purge path: " + resolvedPath + "\r\n\r\nReason: " +
                    ex.Message, Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
