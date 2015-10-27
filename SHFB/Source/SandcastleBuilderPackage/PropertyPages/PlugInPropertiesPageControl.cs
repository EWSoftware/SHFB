//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PlugInPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 10/26/2015
// Note    : Copyright 2011-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Plug-Ins category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/28/2012  EFW  Updated for use in the standalone GUI
// 12/18/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

#if STANDALONEGUI
using Sandcastle.Core;
#else
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.Properties;
#endif

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

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

        private List<Lazy<IPlugIn, IPlugInMetadata>> availablePlugIns;
        private PlugInConfigurationDictionary currentConfigs;
        private ComponentCache componentCache;

        private string messageBoxTitle;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PlugInPropertiesPageControl()
        {
            InitializeComponent();

#if !STANDALONEGUI
            messageBoxTitle = Resources.PackageTitle;
#else
            messageBoxTitle = Constants.AppName;
#endif
            this.Title = "Plug-Ins";
            this.HelpKeyword = "be2b5b09-cf5f-4fc3-be8c-f6d8a27c3691";
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool BindControlValue(Control control)
        {
            SandcastleProject currentProject = null;
            string[] searchFolders;

#if !STANDALONEGUI
            if(this.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
            currentProject = this.CurrentProject;
#endif
            if(currentProject == null)
            {
                lbProjectPlugIns.Items.Clear();
                gbProjectAddIns.Enabled = false;
            }
            else
            {
                searchFolders = new[] { currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) };

                if(componentCache == null)
                {
                    componentCache = ComponentCache.CreateComponentCache(currentProject.Filename);

                    componentCache.ComponentContainerLoaded += componentCache_ComponentContainerLoaded;
                    componentCache.ComponentContainerLoadFailed += componentCache_ComponentContainerLoadFailed;
                    componentCache.ComponentContainerReset += componentCache_ComponentContainerReset;
                }

                if(componentCache.LoadComponentContainer(searchFolders))
                    this.componentCache_ComponentContainerLoaded(this, EventArgs.Empty);
                else
                    this.componentCache_ComponentContainerReset(this, EventArgs.Empty);
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control control)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("PlugInConfigurations", currentConfigs.ToXml());
#else
            if(this.CurrentProject == null)
                return false;

            this.CurrentProject.MSBuildProject.SetProperty("PlugInConfigurations", currentConfigs.ToXml());
#endif
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

            txtPlugInCopyright.Text = txtPlugInVersion.Text = txtPlugInDescription.Text = "?? Not Found ??";

            if(availablePlugIns != null)
            {
                var plugIn = availablePlugIns.FirstOrDefault(p => p.Metadata.Id == key);

                if(plugIn != null)
                {
                    txtPlugInCopyright.Text = plugIn.Metadata.Copyright;
                    txtPlugInVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}",
                        plugIn.Metadata.Version ?? "0.0.0.0");
                    txtPlugInDescription.Text = plugIn.Metadata.Description;
                }
            }
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

            if(availablePlugIns != null)
            {
                // Currently, no duplicates are allowed
                if(idx != -1)
                    lbProjectPlugIns.SelectedIndex = idx;
                else
                {
                    var plugIn = availablePlugIns.FirstOrDefault(p => p.Metadata.Id == key);

                    if(plugIn != null)
                    {
                        idx = lbProjectPlugIns.Items.Add(key);

                        if(idx != -1)
                        {
                            currentConfigs.Add(key, true, null);
                            lbProjectPlugIns.SelectedIndex = idx;
                            lbProjectPlugIns.SetItemChecked(idx, true);
                            btnConfigure.Enabled = btnDelete.Enabled = true;

                            this.IsDirty = true;

                            // Open the configuration dialog to configure it when first added if needed
                            btnConfigure_Click(sender, e);
                        }
                    }
                }
            }
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

#if !STANDALONEGUI
            SandcastleProject currentProject = null;

            if(this.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;

#else
            SandcastleProject currentProject = this.CurrentProject;
#endif
            if(currentProject == null)
                return;

            if(availablePlugIns != null)
            {
                var plugIn = availablePlugIns.FirstOrDefault(p => p.Metadata.Id == key);

                if(plugIn != null)
                {
                    if(!plugIn.Metadata.IsConfigurable)
                    {
                        if(sender == btnConfigure)
                            MessageBox.Show("The selected plug-in contains no editable configuration information",
                                messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    plugInConfig = currentConfigs[key];
                    currentConfig = plugInConfig.Configuration;

                    try
                    {
                        // Plug-in instances are shared.  The container will dispose of the plug-in when it is
                        // disposed of.
                        newConfig = plugIn.Value.ConfigurePlugIn(currentProject, currentConfig);

                        // Only store it if new or if it changed
                        if(currentConfig != newConfig)
                        {
                            plugInConfig.Configuration = newConfig;
                            this.IsDirty = true;
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());

                        MessageBox.Show("Unexpected error attempting to configure component: " + ex.Message,
                            messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    MessageBox.Show("The selected plug-in could not be found in any of the component or " +
                        "project folders and cannot be used.", messageBoxTitle, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
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

        /// <summary>
        /// This is called when the component cache is reset prior to loading it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerReset(object sender, EventArgs e)
        {
            if(!this.IsDisposed)
            {
                gbAvailablePlugIns.Enabled = gbProjectAddIns.Enabled = false;
                lbAvailablePlugIns.Items.Clear();
                lbProjectPlugIns.Items.Clear();
                lbAvailablePlugIns.Items.Add("Loading...");
            }
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            if(!this.IsDisposed)
            {
                gbAvailablePlugIns.Enabled = gbProjectAddIns.Enabled = false;
                lbAvailablePlugIns.Items.Clear();
                lbProjectPlugIns.Items.Clear();
                lbAvailablePlugIns.Items.Add("Unable to load transform component arguments");
                txtPlugInDescription.Text = componentCache.LastError.ToString();
            }
        }

        /// <summary>
        /// This is called when the component cache has finished being loaded and is available for use
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoaded(object sender, EventArgs e)
        {
            ProjectProperty projProp;
            int idx;

            if(this.IsDisposed)
                return;

            lbAvailablePlugIns.Items.Clear();

            HashSet<string> plugInIds = new HashSet<string>();

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                lbProjectPlugIns.Items.Clear();

                availablePlugIns = componentCache.ComponentContainer.GetExports<IPlugIn, IPlugInMetadata>().ToList();

                // There may be duplicate component IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the first
                // component for a unique ID will be used.  We also ignore hidden plug-ins.
                foreach(var plugIn in availablePlugIns)
                    if(!plugIn.Metadata.IsHidden && !plugInIds.Contains(plugIn.Metadata.Id))
                    {
                        lbAvailablePlugIns.Items.Add(plugIn.Metadata.Id);
                        plugInIds.Add(plugIn.Metadata.Id);
                    }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading build components: " + ex.Message, messageBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if(lbAvailablePlugIns.Items.Count != 0)
            {
                lbAvailablePlugIns.SelectedIndex = 0;
                gbAvailablePlugIns.Enabled = gbProjectAddIns.Enabled = true;
            }
            else
            {
                MessageBox.Show("No valid build components found", messageBoxTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                gbAvailablePlugIns.Enabled = gbProjectAddIns.Enabled = false;
                return;
            }

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return;

            projProp = this.ProjectMgr.BuildProject.GetProperty("PlugInConfigurations");
#else
            if(this.CurrentProject == null)
                return;

            projProp = this.CurrentProject.MSBuildProject.GetProperty("PlugInConfigurations");
#endif
            currentConfigs = new PlugInConfigurationDictionary();

            if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                currentConfigs.FromXml(projProp.UnevaluatedValue);

            // May already be binding so preserve the original state
            bool isBinding = this.IsBinding;

            try
            {
                this.IsBinding = true;

                foreach(string key in currentConfigs.Keys)
                {
                    idx = lbProjectPlugIns.Items.Add(key);
                    lbProjectPlugIns.SetItemChecked(idx, currentConfigs[key].Enabled);
                }

                if(lbProjectPlugIns.Items.Count != 0)
                {
                    lbProjectPlugIns.SelectedIndex = 0;
                    btnConfigure.Enabled = btnDelete.Enabled = true;
                }
                else
                    btnConfigure.Enabled = btnDelete.Enabled = false;
            }
            finally
            {
                this.IsBinding = isBinding;
            }
        }
        #endregion
    }
}
