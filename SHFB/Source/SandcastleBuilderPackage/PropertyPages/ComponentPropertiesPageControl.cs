//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ComponentPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 12/27/2013
// Note    : Copyright 2011-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Components category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.6.0  10/28/2012  EFW  Updated for use in the standalone GUI
// -------  12/26/2013  EFW  Updated to use MEF for the build components
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Sandcastle.Core.BuildAssembler.BuildComponent;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.Properties;

using SandcastleBuilder.Utils;
#endif

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Components category project properties
    /// </summary>
    [Guid("F3BA863D-9E18-477E-A62F-DFD679C8FEF7")]
    public partial class ComponentPropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private CompositionContainer componentContainer;
        private List<Lazy<BuildComponentFactory, IBuildComponentMetadata>> availableComponents;
        private ComponentConfigurationDictionary currentConfigs;
        private string messageBoxTitle, lastProjectName;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentPropertiesPageControl()
        {
            InitializeComponent();

#if !STANDALONEGUI
            messageBoxTitle = Resources.PackageTitle;
#else
            messageBoxTitle = SandcastleBuilder.Utils.Constants.AppName;
#endif
            this.Title = "Components";
            this.HelpKeyword = "d1ec47f6-b611-41cf-a78c-f68e01d6ae9e";
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Try to load information about all available build components so that they can be added to the project
        /// </summary>
        /// <returns>True on success, false on failure or if no project is loaded</returns>
        private void LoadAvailableBuildComponentMetadata()
        {
            HashSet<string> componentIds = new HashSet<string>();

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if(componentContainer != null)
                {
                    componentContainer.Dispose();
                    componentContainer = null;
                    availableComponents = null;
                }

#if !STANDALONEGUI
                SandcastleProject currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;

                lastProjectName = currentProject == null ? null : currentProject.Filename;
                componentContainer = BuildComponentManager.GetComponentContainer(currentProject);
#else
                lastProjectName = base.CurrentProject == null ? null : base.CurrentProject.Filename;
                componentContainer = BuildComponentManager.GetComponentContainer(base.CurrentProject);
#endif
                lbProjectComponents.Items.Clear();

                availableComponents = componentContainer.GetExports<BuildComponentFactory, IBuildComponentMetadata>().ToList();

                // Only load those that indicate that they are visible to the designer.  There may be duplicate
                // component IDs across the assemblies found.  See BuildComponentManger.GetComponentContainer()
                // for the folder search precedence.  Only the first component for a unique ID will be used.
                foreach(var component in availableComponents)
                    if(!componentIds.Contains(component.Metadata.Id) && component.Metadata.DesignerVisible)
                    {
                        lbAvailableComponents.Items.Add(component.Metadata.Id);
                        componentIds.Add(component.Metadata.Id);
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

            if(lbAvailableComponents.Items.Count != 0)
            {
                lbAvailableComponents.SelectedIndex = 0;
                gbAvailableComponents.Enabled = gbProjectAddIns.Enabled = true;
            }
            else
            {
                MessageBox.Show("No valid build components found", messageBoxTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                gbAvailableComponents.Enabled = gbProjectAddIns.Enabled = false;
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

            lbProjectComponents.Items.Clear();
            btnConfigure.Enabled = btnDelete.Enabled = false;

#if !STANDALONEGUI
            SandcastleProject currentProject = null;

            if(base.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;

            if(componentContainer == null || currentProject == null || currentProject.Filename != lastProjectName)
                this.LoadAvailableBuildComponentMetadata();

            if(this.ProjectMgr == null || currentProject == null)
                return false;

            currentConfigs = new ComponentConfigurationDictionary(currentProject);
            projProp = this.ProjectMgr.BuildProject.GetProperty("ComponentConfigurations");
#else
            if(componentContainer == null || base.CurrentProject == null ||
              base.CurrentProject.Filename != lastProjectName)
                this.LoadAvailableBuildComponentMetadata();

            if(base.CurrentProject == null)
                return false;

            currentConfigs = new ComponentConfigurationDictionary(base.CurrentProject);
            projProp = base.CurrentProject.MSBuildProject.GetProperty("ComponentConfigurations");
#endif
            if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                currentConfigs.FromXml(projProp.UnevaluatedValue);

            foreach(string key in currentConfigs.Keys)
            {
                idx = lbProjectComponents.Items.Add(key);
                lbProjectComponents.SetItemChecked(idx, currentConfigs[key].Enabled);
            }

            if(lbProjectComponents.Items.Count != 0)
            {
                lbProjectComponents.SelectedIndex = 0;
                btnConfigure.Enabled = btnDelete.Enabled = true;
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control control)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("ComponentConfigurations", currentConfigs.ToXml());
#else
            if(base.CurrentProject == null)
                return false;

            base.CurrentProject.MSBuildProject.SetProperty("ComponentConfigurations", currentConfigs.ToXml());
#endif
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Update the build component details when the selected index changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailableComponents_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;

            txtComponentCopyright.Text = txtComponentVersion.Text = txtComponentDescription.Text = "?? Not Found ??";

            if(availableComponents != null)
            {
                var component = availableComponents.FirstOrDefault(p => p.Metadata.Id == key);

                if(component != null)
                {
                    txtComponentCopyright.Text = component.Metadata.Copyright;
                    txtComponentVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}",
                        component.Metadata.Version ?? "0.0.0.0");
                    txtComponentDescription.Text = component.Metadata.Description;
                }
            }
        }

        /// <summary>
        /// Update the enabled state of the build component based on its checked state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProjectComponents_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            string key = (string)lbProjectComponents.Items[e.Index];
            bool newState = (e.NewValue == CheckState.Checked);

            if(currentConfigs[key].Enabled != newState)
            {
                currentConfigs[key].Enabled = newState;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Add the selected build component to the project with a default configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddComponent_Click(object sender, EventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;
            int idx = lbProjectComponents.FindStringExact(key);

            if(availableComponents != null)
            {
                // Currently, no duplicates are allowed
                if(idx != -1)
                    lbProjectComponents.SelectedIndex = idx;
                else
                {
                    var component = availableComponents.FirstOrDefault(p => p.Metadata.Id == key);

                    if(component != null)
                    {
                        idx = lbProjectComponents.Items.Add(key);

                        if(idx != -1)
                        {
                            try
                            {
                                currentConfigs.Add(key, true, String.Format(CultureInfo.InvariantCulture,
                                    "<component id=\"{0}\">{1}</component>", key, component.Value.DefaultConfiguration));
                            }
                            catch(Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex.ToString());

                                MessageBox.Show("Unexpected error attempting to add component: " + ex.Message,
                                    messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            lbProjectComponents.SelectedIndex = idx;
                            lbProjectComponents.SetItemChecked(idx, true);
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
        /// Edit the selected build component's project configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnConfigure_Click(object sender, EventArgs e)
        {
            BuildComponentConfiguration componentConfig;
            string newConfig, currentConfig, key = (string)lbProjectComponents.SelectedItem;

            if(availableComponents != null)
            {
                var component = availableComponents.FirstOrDefault(p => p.Metadata.Id == key);

                if(component != null)
                {
                    if(!component.Metadata.IsConfigurable)
                    {
                        if(sender == btnConfigure)
                            MessageBox.Show("The selected build component contains no editable configuration information",
                                messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    componentConfig = currentConfigs[key];
                    currentConfig = componentConfig.Configuration;

                    try
                    {
                        newConfig = component.Value.ConfigureComponent(currentConfig);

                        // Only store it if new or if it changed
                        if(currentConfig != newConfig)
                        {
                            componentConfig.Configuration = newConfig;
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
                    MessageBox.Show("The selected build component could not be found in any of the component " +
                        "or project folders and cannot be used.", messageBoxTitle, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Delete the selected build component from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string key = (string)lbProjectComponents.SelectedItem;
            int idx = lbProjectComponents.SelectedIndex;

            if(currentConfigs.ContainsKey(key))
            {
                currentConfigs.Remove(key);
                this.IsDirty = true;

                lbProjectComponents.Items.RemoveAt(idx);

                if(lbProjectComponents.Items.Count == 0)
                    btnConfigure.Enabled = btnDelete.Enabled = false;
                else
                    if(idx < lbProjectComponents.Items.Count)
                        lbProjectComponents.SelectedIndex = idx;
                    else
                        lbProjectComponents.SelectedIndex = idx - 1;
            }
        }
        #endregion
    }
}
