//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentResolveReferenceLinksConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/02/2014
// Note    : Copyright 2013-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the ESENT Resolve Reference Links
// component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  03/12/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

using Sandcastle.Core;

namespace SandcastleBuilder.Components.UI
{
    /// <summary>
    /// This form is used to configure the ESENT Resolve Reference Links component
    /// </summary>
    internal partial class ESentResolveReferenceLinksConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XElement config;     // The configuration
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.ToString(); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public ESentResolveReferenceLinksConfigDlg(string currentConfig)
        {
            XElement node;

            InitializeComponent();

            lnkProjectSite.Links[0].LinkData = "https://GitHub.com/EWSoftware/SHFB";

            // Load the current settings.  Note that there are multiple configurations (one for each help file
            // format).  However, the settings will be the same across all of them.
            config = XElement.Parse(currentConfig);

            node = config.Descendants("msdnContentIdCache").First();
            txtContentIdCachePath.Text = node.Attribute("cachePath").Value;
            udcContentIdLocalCacheSize.Value = ((int?)node.Attribute("localCacheSize") ?? 2500);

            node = config.Descendants("targets").First(d => d.Attribute("id").Value == "FrameworkTargets");
            txtFrameworkTargetsCachePath.Text = node.Attribute("cachePath").Value;
            udcFrameworkTargetsLocalCacheSize.Value = ((int?)node.Attribute("localCacheSize") ?? 2500);

            node = config.Descendants("targets").First(d => d.Attribute("id").Value == "ProjectTargets");
            txtProjectTargetsCachePath.Text = (string)node.Attribute("cachePath");
            udcProjectTargetsLocalCacheSize.Value = ((int?)node.Attribute("localCacheSize") ?? 2500);

            chkEnableLocalCache.Checked = !String.IsNullOrWhiteSpace(txtProjectTargetsCachePath.Text);
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
                path = localDataFolder + "\\" + path.Substring(pos);
            }

            if(path.EndsWith("\\", StringComparison.Ordinal))
                path = path.Substring(0, path.Length - 1);

            if(Directory.Exists(path))
                return path;

            return null;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close without saving
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Go to the Sandcastle Help File Builder project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)e.Link.LinkData);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Select a cache folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectCacheFolder_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            TextBox t;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                if(b == btnSelectContentIdCacheFolder)
                {
                    t = txtContentIdCachePath;
                    dlg.Description = "Select the MSDN content ID cache folder";
                }
                else
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

                dlg.SelectedPath = (ResolvePath(t.Text) ?? Directory.GetCurrentDirectory());

                // If selected, set the new folder
                if(dlg.ShowDialog() == DialogResult.OK)
                    t.Text = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Enable or disable current project target caching
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkEnableLocalCache_CheckedChanged(object sender, EventArgs e)
        {
            txtProjectTargetsCachePath.Enabled = btnSelectProjectTargetsCacheFolder.Enabled =
                udcProjectTargetsLocalCacheSize.Enabled = chkEnableLocalCache.Checked;

            if(!chkEnableLocalCache.Checked)
                txtProjectTargetsCachePath.Text = null;
            else
                txtProjectTargetsCachePath.Text = @"{@WorkingFolder}ProjectTargetsCache";
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            bool isValid = true;

            txtContentIdCachePath.Text = txtContentIdCachePath.Text.Trim();
            txtFrameworkTargetsCachePath.Text = txtFrameworkTargetsCachePath.Text.Trim();
            txtProjectTargetsCachePath.Text = txtProjectTargetsCachePath.Text.Trim();

            epErrors.Clear();

            if(txtContentIdCachePath.Text.Length == 0)
            {
                epErrors.SetError(txtContentIdCachePath, "The content ID cache path is required");
                isValid = false;
            }
            else
                if(txtContentIdCachePath.Text == txtFrameworkTargetsCachePath.Text ||
                  txtContentIdCachePath.Text == txtProjectTargetsCachePath.Text)
                {
                    epErrors.SetError(txtContentIdCachePath, "The content ID cache path must be unique");
                    isValid = false;
                }

            if(txtFrameworkTargetsCachePath.Text.Length == 0)
            {
                epErrors.SetError(txtFrameworkTargetsCachePath, "The Framework target cache path is required");
                isValid = false;
            }
            else
                if(txtFrameworkTargetsCachePath.Text == txtContentIdCachePath.Text ||
                  txtFrameworkTargetsCachePath.Text == txtProjectTargetsCachePath.Text)
                {
                    epErrors.SetError(txtFrameworkTargetsCachePath, "The Framework target cache path must be unique");
                    isValid = false;
                }

            if(chkEnableLocalCache.Checked && txtProjectTargetsCachePath.Text.Length == 0)
            {
                epErrors.SetError(txtProjectTargetsCachePath, "The project target cache path is required " +
                    "if enabled");
                isValid = false;
            }

            if(!isValid)
                return;

            // Update each help format configuration with the same values
            foreach(var n in config.Descendants("msdnContentIdCache"))
            {
                n.Attribute("cachePath").Value = txtContentIdCachePath.Text;
                n.SetAttributeValue("localCacheSize", (int)udcContentIdLocalCacheSize.Value);
            }

            foreach(var n in config.Descendants("targets").Where(d => d.Attribute("id").Value == "FrameworkTargets"))
            {
                n.Attribute("cachePath").Value = txtFrameworkTargetsCachePath.Text;
                n.SetAttributeValue("localCacheSize", (int)udcFrameworkTargetsLocalCacheSize.Value);
            }

            foreach(var n in config.Descendants("targets").Where(d => d.Attribute("id").Value == "ProjectTargets"))
            {
                // These attributes may not exist and will be created if needed
                n.SetAttributeValue("cachePath", txtProjectTargetsCachePath.Text);
                n.SetAttributeValue("localCacheSize", (int)udcProjectTargetsLocalCacheSize.Value);

                // This prevents it reindexing the data in the reference build if it was indexed in the
                // conceptual build.
                n.SetAttributeValue("noReload", true);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Purge the content ID and target cache folders
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnPurge_Click(object sender, EventArgs e)
        {
            string[] allPaths = new[] { txtContentIdCachePath.Text, txtFrameworkTargetsCachePath.Text,
                txtProjectTargetsCachePath.Text };
            string resolvedPath = null;

            if(MessageBox.Show("WARNING: This will delete all of the current ESENT reference link target " +
              "cache folders.  The information will need to be created the next time this project is built.  " +
              "Are you sure you want to delete them?", Constants.AppName, MessageBoxButtons.YesNo,
              MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            try
            {
                foreach(string path in allPaths)
                {
                    resolvedPath = ResolvePath(path);

                    if(resolvedPath != null && Directory.Exists(resolvedPath))
                        Directory.Delete(resolvedPath, true);
                }

                MessageBox.Show("The cache folders have been deleted", Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch(IOException ex)
            {
                MessageBox.Show("Unable to resolve or purge path: " + resolvedPath + "\r\n\r\nReason: " +
                    ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
