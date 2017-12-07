//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : PlugInPropertiesPageContent.cs
// Author  : Eric Woodruff
// Updated : 11/07/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
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
// 11/07/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sandcastle.Core;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// Interaction logic for PlugInPropertiesPageContent.xaml
    /// </summary>
    public partial class PlugInPropertiesPageContent : UserControl
    {
        #region Plug-in configuration list box item
        //=====================================================================

        /// <summary>
        /// This is used to contain the plug-in name and configuration for use with the checked list box
        /// </summary>
        private class PlugInConfig
        {
            /// <summary>
            /// The plug-in name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The current plug-in configuration
            /// </summary>
            public PlugInConfiguration Configuration { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private List<Lazy<IPlugIn, IPlugInMetadata>> availablePlugIns;
        private PlugInConfigurationDictionary currentConfigs;
        private ComponentCache componentCache;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns a reference to the underlying Sandcastle project
        /// </summary>
        public SandcastleProject Project { get; set; }

        /// <summary>
        /// This read-only property returns the selected plug-in configurations
        /// </summary>
        public PlugInConfigurationDictionary SelectedPlugIns => currentConfigs;

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is used to signal that the project plug-ins have been modified
        /// </summary>
        public event EventHandler PlugInsModified;

        /// <summary>
        /// This event is raised when the control needs the current plug-in settings from the project
        /// </summary>
        public event EventHandler<ComponentSettingsNeededEventArgs> ComponentSettingsNeeded;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PlugInPropertiesPageContent()
        {
            InitializeComponent();

            lbAvailablePlugIns.Items.SortDescriptions.Add(
                new SortDescription { Direction = ListSortDirection.Ascending });
            lbProjectPlugIns.Items.SortDescriptions.Add(new SortDescription("Name",
                ListSortDirection.Ascending));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to disconnect from the component cache when the control is no longer needed
        /// </summary>
        public void Dispose()
        {
            if(componentCache != null)
            {
                componentCache.ComponentContainerLoaded -= componentCache_ComponentContainerLoaded;
                componentCache.ComponentContainerLoadFailed -= componentCache_ComponentContainerLoadFailed;
                componentCache.ComponentContainerReset -= componentCache_ComponentContainerReset;
            }
        }

        /// <summary>
        /// This is used to load the plug-in settings when necessary
        /// </summary>
        /// <param name="currentProjectFilename">The current project's filename</param>
        /// <param name="componentSearchPaths">The paths to search for components</param>
        public void LoadPlugInSettings(string currentProjectFilename, IEnumerable<string> componentSearchPaths)
        {
            if(currentProjectFilename == null)
            {
                lbProjectPlugIns.Items.Clear();
                gbProjectPlugIns.IsEnabled = false;
            }
            else
            {
                if(componentCache == null)
                {
                    componentCache = ComponentCache.CreateComponentCache(currentProjectFilename);

                    componentCache.ComponentContainerLoaded += componentCache_ComponentContainerLoaded;
                    componentCache.ComponentContainerLoadFailed += componentCache_ComponentContainerLoadFailed;
                    componentCache.ComponentContainerReset += componentCache_ComponentContainerReset;
                }

                if(componentCache.LoadComponentContainer(componentSearchPaths))
                    this.componentCache_ComponentContainerLoaded(this, EventArgs.Empty);
                else
                    this.componentCache_ComponentContainerReset(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Add the selected plug-in when double clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailablePlugIns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.btnAddPlugIn_Click(sender, e);
        }

        /// <summary>
        /// Configure the selected plug-in when double clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProjectPlugIns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.btnConfigure_Click(sender, e);
        }

        /// <summary>
        /// Update the plug-in details when the selected index changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailablePlugIns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string key = (string)lbAvailablePlugIns.SelectedItem;

            txtPlugInCopyright.Text = txtPlugInVersion.Text = txtPlugInDescription.Text = "?? Not Found ??";

            if(availablePlugIns != null)
            {
                var plugIn = availablePlugIns.FirstOrDefault(p => p.Metadata.Id == key);

                if(plugIn != null)
                {
                    txtPlugInCopyright.Text = plugIn.Metadata.Copyright;
                    txtPlugInVersion.Text = $"Version {plugIn.Metadata.Version ?? "0.0.0.0"}";
                    txtPlugInDescription.Text = plugIn.Metadata.Description;
                }
            }
        }

        /// <summary>
        /// Notify the parent control when the plug-in's enabled state changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkEnabledState_Click(object sender, RoutedEventArgs e)
        {
            this.PlugInsModified?.Invoke(this, EventArgs.Empty);

            // When using the mouse to check/uncheck an item, the selected index in the containing list box
            // doesn't change.  Force it to do so.
            string key = (string)((CheckBox)sender).Content;
            var match = lbProjectPlugIns.Items.Cast<PlugInConfig>().FirstOrDefault(c => c.Name == key);

            if(match != null)
                lbProjectPlugIns.SelectedIndex = lbProjectPlugIns.Items.IndexOf(match);
        }

        /// <summary>
        /// Add the selected plug-in to the project with a default configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddPlugIn_Click(object sender, RoutedEventArgs e)
        {
            string key = (string)lbAvailablePlugIns.SelectedItem;
            var match = lbProjectPlugIns.Items.Cast<PlugInConfig>().FirstOrDefault(c => c.Name == key);

            if(availablePlugIns != null)
            {
                // No duplicates are allowed
                if(match != null)
                    lbProjectPlugIns.SelectedIndex = lbProjectPlugIns.Items.IndexOf(match);
                else
                {
                    var plugIn = availablePlugIns.FirstOrDefault(p => p.Metadata.Id == key);

                    if(plugIn != null)
                    {
                        try
                        {
                            var c = currentConfigs.Add(key, true, null);
                            match = new PlugInConfig { Name = key, Configuration = c };
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());

                            MessageBox.Show("Unexpected error attempting to add plug-in: " + ex.Message,
                                Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        lbProjectPlugIns.SelectedIndex = lbProjectPlugIns.Items.Add(match);
                        lbProjectPlugIns.Items.Refresh();
                        btnConfigure.IsEnabled = btnDelete.IsEnabled = true;

                        this.PlugInsModified?.Invoke(this, EventArgs.Empty);

                        // Open the configuration dialog to configure it when first added if needed
                        btnConfigure_Click(sender, e);
                    }
                }
            }
        }

        /// <summary>
        /// Edit the selected plug-in's project configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {
            string newConfig, currentConfig;
            var config = (PlugInConfig)lbProjectPlugIns.SelectedItem;

            if(config != null && availablePlugIns != null)
            {
                var plugIn = availablePlugIns.FirstOrDefault(p => p.Metadata.Id == config.Name);

                if(plugIn != null)
                {
                    if(!plugIn.Metadata.IsConfigurable)
                    {
                        if(sender == btnConfigure)
                            MessageBox.Show("The selected plug-in contains no editable configuration information",
                                Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    currentConfig = config.Configuration.Configuration;

                    try
                    {
                        // Plug-in instances are shared.  The container will dispose of the plug-in when it is
                        // disposed of.
                        newConfig = plugIn.Value.ConfigurePlugIn(this.Project, currentConfig);

                        // Only store it if new or if it changed
                        if(currentConfig != newConfig)
                        {
                            config.Configuration.Configuration = newConfig;
                            this.PlugInsModified?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());

                        MessageBox.Show("Unexpected error attempting to configure plug-in: " + ex.Message,
                            Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                    MessageBox.Show("The selected plug-in could not be found in any of the component " +
                        "or project folders and cannot be used.", Constants.AppName, MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Delete the selected plug-in from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var config = (PlugInConfig)lbProjectPlugIns.SelectedItem;

            if(config != null && currentConfigs.ContainsKey(config.Name))
            {
                currentConfigs.Remove(config.Name);
                this.PlugInsModified?.Invoke(this, EventArgs.Empty);

                int idx = lbProjectPlugIns.SelectedIndex;
                lbProjectPlugIns.Items.RemoveAt(idx);

                if(lbProjectPlugIns.Items.Count == 0)
                    btnConfigure.IsEnabled = btnDelete.IsEnabled = false;
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
            gbAvailablePlugIns.IsEnabled = gbProjectPlugIns.IsEnabled = false;
            lbAvailablePlugIns.Items.Clear();
            lbProjectPlugIns.Items.Clear();
            lbAvailablePlugIns.Items.Add("Loading...");
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            gbAvailablePlugIns.IsEnabled = gbProjectPlugIns.IsEnabled = false;
            lbAvailablePlugIns.Items.Clear();
            lbProjectPlugIns.Items.Clear();
            lbAvailablePlugIns.Items.Add("Unable to load plug-in information");
            txtPlugInDescription.Text = componentCache.LastError.ToString();
        }

        /// <summary>
        /// This is called when the component cache has finished being loaded and is available for use
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoaded(object sender, EventArgs e)
        {
            ComponentSettingsNeededEventArgs projectSettings = new ComponentSettingsNeededEventArgs();

            this.ComponentSettingsNeeded?.Invoke(this, projectSettings);

            HashSet<string> plugInIds = new HashSet<string>();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                lbAvailablePlugIns.Items.Clear();
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

                MessageBox.Show("Unexpected error loading plug-ins: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                lbAvailablePlugIns.Items.Refresh();
                Mouse.OverrideCursor = null;
            }

            if(projectSettings.ProjectLoaded)
            {
                if(lbAvailablePlugIns.Items.Count != 0)
                {
                    lbAvailablePlugIns.SelectedIndex = 0;
                    gbAvailablePlugIns.IsEnabled = gbProjectPlugIns.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("No valid plug-ins found", Constants.AppName, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    gbAvailablePlugIns.IsEnabled = gbProjectPlugIns.IsEnabled = false;
                    return;
                }

                currentConfigs = projectSettings.PlugIns;

                foreach(var kv in currentConfigs)
                    lbProjectPlugIns.Items.Add(new PlugInConfig { Name = kv.Key, Configuration = kv.Value });

                if(lbProjectPlugIns.Items.Count != 0)
                {
                    lbProjectPlugIns.Items.Refresh();
                    lbProjectPlugIns.SelectedIndex = 0;
                    btnConfigure.IsEnabled = btnDelete.IsEnabled = true;
                }
                else
                    btnConfigure.IsEnabled = btnDelete.IsEnabled = false;
            }
        }
        #endregion
    }
}
