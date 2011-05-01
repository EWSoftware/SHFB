//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : PlugInConfigurationEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to select and edit the plug-in
// configurations.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/10/2007  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to select and edit the plug-in configurations.
    /// </summary>
    /// <remarks>To be editable, the plug-in assembly must be present in
    /// the <b>.\Plug-Ins</b> folder or a subfolder beneath it.  The plug-ins
    /// folder is found under the common application data folder.</remarks>
    internal partial class PlugInConfigurationEditorDlg : Form
    {
        #region Private data members
        //=====================================================================

        private PlugInConfigurationDictionary currentConfigs;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configs">The current configurations</param>
        internal PlugInConfigurationEditorDlg(PlugInConfigurationDictionary configs)
        {
            int idx;

            InitializeComponent();

            currentConfigs = configs;

            try
            {
                foreach(string key in PlugInManager.PlugIns.Keys)
                    lbAvailablePlugIns.Items.Add(key);
            }
            catch(ReflectionTypeLoadException loadEx)
            {
                System.Diagnostics.Debug.WriteLine(loadEx.ToString());
                System.Diagnostics.Debug.WriteLine(loadEx.LoaderExceptions[0].ToString());

                MessageBox.Show("Unexpected error loading plug-ins: " + loadEx.LoaderExceptions[0].Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading plug-ins: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if(lbAvailablePlugIns.Items.Count != 0)
                lbAvailablePlugIns.SelectedIndex = 0;
            else
            {
                MessageBox.Show("No valid plug-ins found", Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                gbAvailablePlugIns.Enabled = gbProjectAddIns.Enabled = false;
            }

            foreach(string key in currentConfigs.Keys)
            {
                idx = lbProjectPlugIns.Items.Add(key);
                lbProjectPlugIns.SetItemChecked(idx, currentConfigs[key].Enabled);
            }

            if(lbProjectPlugIns.Items.Count != 0)
                lbProjectPlugIns.SelectedIndex = 0;
            else
                btnConfigure.Enabled = btnDelete.Enabled = false;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Update the plug-in details when the selected index changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailablePlugIns_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = (string)lbAvailablePlugIns.SelectedItem;

            PlugInInfo info = PlugInManager.PlugIns[key];
            txtPlugInCopyright.Text = info.Copyright;
            txtPlugInVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}", info.Version);
            txtPlugInDescription.Text = info.Description;
        }

        /// <summary>
        /// Update the enabled state of the plug-in based on its checked state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProjectPlugIns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string key = (string)lbProjectPlugIns.Items[e.Index];
            bool newState = (e.NewValue == CheckState.Checked);

            if(currentConfigs[key].Enabled != newState)
                currentConfigs[key].Enabled = newState;
        }

        /// <summary>
        /// Add the selected plug-in to the project with a default
        /// configuration.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddPlugIn_Click(object sender, EventArgs e)
        {
            string key = (string)lbAvailablePlugIns.SelectedItem;
            int idx = lbProjectPlugIns.FindStringExact(key);

            // Currently, no duplicates are allowed
            if(idx != -1)
                lbProjectPlugIns.SelectedIndex = idx;
            else
                if(PlugInManager.IsSupported(key))
                {
                    idx = lbProjectPlugIns.Items.Add(key);

                    if(idx != -1)
                    {
                        currentConfigs.Add(key, true, null);
                        lbProjectPlugIns.SelectedIndex = idx;
                        lbProjectPlugIns.SetItemChecked(idx, true);
                        btnConfigure.Enabled = btnDelete.Enabled = true;

                        currentConfigs.OnDictionaryChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, -1));
                    }
                }
                else
                    MessageBox.Show("The selected plug-in's version is not compatible with this version of the " +
                        "help file builder and cannot be used.", Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
        }

        /// <summary>
        /// Edit the selected plug-in's project configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnConfigure_Click(object sender, EventArgs e)
        {
            PlugInConfiguration plugInConfig;
            string newConfig, currentConfig, key = (string)lbProjectPlugIns.SelectedItem;

            if(PlugInManager.IsSupported(key))
            {
                PlugInInfo info = PlugInManager.PlugIns[key];

                using(IPlugIn plugIn = info.NewInstance())
                {
                    plugInConfig = currentConfigs[key];
                    currentConfig = plugInConfig.Configuration;
                    newConfig = plugIn.ConfigurePlugIn(currentConfigs.ProjectFile, currentConfig);
                }

                // Only store it if new or if it changed
                if(currentConfig != newConfig)
                    plugInConfig.Configuration = newConfig;
            }
            else
                MessageBox.Show("The selected plug-in either does not exist or is of a version that is not " +
                    "compatible with this version of the help file builder and cannot be used.",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Delete the selected plug-in from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string key = (string)lbProjectPlugIns.SelectedItem;
            int idx = lbProjectPlugIns.SelectedIndex;

            if(currentConfigs.ContainsKey(key))
            {
                currentConfigs.Remove(key);
                currentConfigs.OnDictionaryChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, -1));

                lbProjectPlugIns.Items.RemoveAt(idx);

                if(lbProjectPlugIns.Items.Count == 0)
                    btnConfigure.Enabled = btnDelete.Enabled = false;
                else
                    if(idx < lbProjectPlugIns.Items.Count)
                        lbProjectPlugIns.SelectedIndex = idx;
                    else
                        lbProjectPlugIns.SelectedIndex = idx - 1;
            }
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string path = null;

            try
            {
#if DEBUG
                // In debug builds, SHFBROOT points to the .\Debug folder for the SandcastleBuilderGUI project
                path = Path.Combine(@"C:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder\SandcastleBuilder.chm");
#else
                path = Path.Combine(Environment.ExpandEnvironmentVariables("%SHFBROOT%"), "SandcastleBuilder.chm");
#endif
                Form form = new Form();
                form.CreateControl();
                Help.ShowHelp(form, path, HelpNavigator.Topic, "html/e031b14e-42f0-47e1-af4c-9fed2b88cbc7.htm");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
}
