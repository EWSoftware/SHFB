//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PlugInPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Plug-In category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Plug-In category project properties
    /// </summary>
    [Guid("8FB53BCE-82A8-4207-9DB6-7D30696C780C")]
    public partial class PlugInPropertiesPageControl : BasePropertyPage
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
        public PlugInPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Plug-Ins";
            this.HelpKeyword = "e031b14e-42f0-47e1-af4c-9fed2b88cbc7";

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
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading plug-ins: " + ex.Message, Resources.PackageTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if(lbAvailablePlugIns.Items.Count != 0)
                lbAvailablePlugIns.SelectedIndex = 0;
            else
            {
                MessageBox.Show("No valid plug-ins found", Resources.PackageTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                gbAvailablePlugIns.Enabled = gbProjectAddIns.Enabled = false;
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool BindControlValue(Control control)
        {
            ProjectProperty projProp;
            int idx;

            currentConfigs = new PlugInConfigurationDictionary(null);
            lbProjectPlugIns .Items.Clear();

            if(this.ProjectMgr == null)
                return false;

            projProp = this.ProjectMgr.BuildProject.GetProperty("PlugInConfigurations");

            if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                currentConfigs.FromXml(projProp.UnevaluatedValue);

            foreach(string key in currentConfigs.Keys)
            {
                idx = lbProjectPlugIns.Items.Add(key);
                lbProjectPlugIns.SetItemChecked(idx, currentConfigs[key].Enabled);
            }

            if(lbProjectPlugIns.Items.Count != 0)
                lbProjectPlugIns.SelectedIndex = 0;
            else
                btnConfigure.Enabled = btnDelete.Enabled = false;

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control control)
        {
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("PlugInConfigurations", currentConfigs.ToXml());
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

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
            {
                currentConfigs[key].Enabled = newState;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Add the selected plug-in to the project with a default configuration
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

                        this.IsDirty = true;
                    }
                }
                else
                    MessageBox.Show("The selected plug-in's version is not compatible with this version of the " +
                        "help file builder and cannot be used.", Resources.PackageTitle, MessageBoxButtons.OK,
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
                {
                    plugInConfig.Configuration = newConfig;
                    this.IsDirty = true;
                }
            }
            else
                MessageBox.Show("The selected plug-in either does not exist or is of a version that is not " +
                    "compatible with this version of the help file builder and cannot be used.",
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                this.IsDirty = true;

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
        #endregion
    }
}
