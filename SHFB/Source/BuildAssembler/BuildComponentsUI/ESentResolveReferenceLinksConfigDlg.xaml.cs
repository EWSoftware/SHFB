//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentResolveReferenceLinksConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the ESENT Resolve Reference Links
// component.
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
    /// Interaction logic for ESentResolveReferenceLinksConfigDlg.xaml
    /// </summary>
    public partial class ESentResolveReferenceLinksConfigDlg : Window
    {
        #region Build component configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the component configuration
        /// </summary>
        [ConfigurationEditorExport("Resolve Reference Links (ESENT Cache)")]
        public sealed class Factory : IConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(XElement configuration, CompositionContainer container)
            {
                var dlg = new ESentResolveReferenceLinksConfigDlg(configuration);

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
        public ESentResolveReferenceLinksConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings.  Note that there are multiple configurations (one for each help file
            // format).  However, the settings will be the same across all of them.
            var node = configuration.Descendants("memberIdUrlCache").First();

            txtMemberIdUrlCachePath.Text = node.Attribute("cachePath").Value;
            udcMemberIdUrlLocalCacheSize.Value = (int?)node.Attribute("localCacheSize") ?? 2500;

            node = configuration.Descendants("targets").First(d => d.Attribute("id").Value == "FrameworkTargets");
            txtFrameworkTargetsCachePath.Text = node.Attribute("cachePath").Value;
            udcFrameworkTargetsLocalCacheSize.Value = (int?)node.Attribute("localCacheSize") ?? 2500;

            node = configuration.Descendants("targets").First(d => d.Attribute("id").Value == "ProjectTargets");

            // Set this first as it may override the value set below
            chkEnableLocalCache.IsChecked = !String.IsNullOrWhiteSpace((string)node.Attribute("cachePath"));

            if(!(chkEnableLocalCache.IsChecked ?? false))
                this.chkEnableLocalCache_CheckedChanged(this, null);

            txtProjectTargetsCachePath.Text = (string)node.Attribute("cachePath");
            udcProjectTargetsLocalCacheSize.Value = (int?)node.Attribute("localCacheSize") ?? 2500;
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
                if(b == btnSelectMemberIdUrlCacheFolder)
                {
                    t = txtMemberIdUrlCachePath;
                    dlg.Description = "Select the member ID URL cache folder";
                }
                else
                {
                    if(b == btnSelectFrameworkTargetsCacheFolder)
                    {
                        t = txtFrameworkTargetsCachePath;
                        dlg.Description = "Select the Framework targets cache folder";
                    }
                    else
                    {
                        t = txtProjectTargetsCachePath;
                        dlg.Description = "Select the current project targets cache folder";
                    }
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
            txtProjectTargetsCachePath.IsEnabled = btnSelectProjectTargetsCacheFolder.IsEnabled =
                udcProjectTargetsLocalCacheSize.IsEnabled = chkEnableLocalCache.IsChecked ?? false;

            if(!chkEnableLocalCache.IsChecked ?? false)
                txtProjectTargetsCachePath.Text = null;
            else
                txtProjectTargetsCachePath.Text = "{@WorkingFolder}ProjectTargetsCache";
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            txtMemberIdUrlCachePath.Text = txtMemberIdUrlCachePath.Text.Trim();
            txtFrameworkTargetsCachePath.Text = txtFrameworkTargetsCachePath.Text.Trim();
            txtProjectTargetsCachePath.Text = txtProjectTargetsCachePath.Text.Trim();

            txtMemberIdUrlCachePath.SetValidationState(true, null);
            txtFrameworkTargetsCachePath.SetValidationState(true, null);
            txtProjectTargetsCachePath.SetValidationState(true, null);

            if(txtMemberIdUrlCachePath.Text.Length == 0)
            {
                txtMemberIdUrlCachePath.SetValidationState(false, "The content ID cache path is required");
                isValid = false;
            }
            else
            {
                if(txtMemberIdUrlCachePath.Text == txtFrameworkTargetsCachePath.Text ||
                  txtMemberIdUrlCachePath.Text == txtProjectTargetsCachePath.Text)
                {
                    txtMemberIdUrlCachePath.SetValidationState(false, "The content ID cache path must be unique");
                    isValid = false;
                }
            }

            if(txtFrameworkTargetsCachePath.Text.Length == 0)
            {
                txtFrameworkTargetsCachePath.SetValidationState(false, "The Framework target cache path is required");
                isValid = false;
            }
            else
            {
                if(txtFrameworkTargetsCachePath.Text == txtMemberIdUrlCachePath.Text ||
                  txtFrameworkTargetsCachePath.Text == txtProjectTargetsCachePath.Text)
                {
                    txtFrameworkTargetsCachePath.SetValidationState(false, "The Framework target cache path must be unique");
                    isValid = false;
                }
            }

            if((chkEnableLocalCache.IsChecked ?? false) && txtProjectTargetsCachePath.Text.Length == 0)
            {
                txtProjectTargetsCachePath.SetValidationState(false, "The project target cache path is required " +
                    "if enabled");
                isValid = false;
            }

            if(isValid)
            {
                // Update each help format configuration with the same values
                foreach(var n in configuration.Descendants("memberIdUrlCache"))
                {
                    n.Attribute("cachePath").Value = txtMemberIdUrlCachePath.Text;
                    n.SetAttributeValue("localCacheSize", (int)udcMemberIdUrlLocalCacheSize.Value);
                }

                foreach(var n in configuration.Descendants("targets").Where(
                  d => d.Attribute("id").Value == "FrameworkTargets"))
                {
                    n.Attribute("cachePath").Value = txtFrameworkTargetsCachePath.Text;
                    n.SetAttributeValue("localCacheSize", (int)udcFrameworkTargetsLocalCacheSize.Value);
                }

                foreach(var n in configuration.Descendants("targets").Where(
                  d => d.Attribute("id").Value == "ProjectTargets"))
                {
                    // These attributes may not exist and will be created if needed
                    n.SetAttributeValue("cachePath", txtProjectTargetsCachePath.Text);
                    n.SetAttributeValue("localCacheSize", (int)udcProjectTargetsLocalCacheSize.Value);

                    // This prevents it reindexing the data in the reference build if it was indexed in the
                    // conceptual build.
                    n.SetAttributeValue("noReload", true);
                }

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
            string[] allPaths = new[] { txtMemberIdUrlCachePath.Text, txtFrameworkTargetsCachePath.Text,
                txtProjectTargetsCachePath.Text };
            string resolvedPath = null;

            if(MessageBox.Show("WARNING: This will delete all of the current ESENT reference link target " +
              "cache folders.  The information will need to be created the next time this project is built.  " +
              "Are you sure you want to delete them?", Constants.AppName, MessageBoxButton.YesNo,
              MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                return;

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
