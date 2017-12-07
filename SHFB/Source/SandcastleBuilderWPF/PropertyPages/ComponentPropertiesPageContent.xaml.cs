//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ComponentPropertiesPageContent.cs
// Author  : Eric Woodruff
// Updated : 11/03/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Components category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/01/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Components category properties
    /// </summary>
    public partial class ComponentPropertiesPageContent : UserControl
    {
        #region Component configuration list box item
        //=====================================================================

        /// <summary>
        /// This is used to contain the component name and configuration for use with the checked list box
        /// </summary>
        private class ComponentConfig
        {
            /// <summary>
            /// The component name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The current component configuration
            /// </summary>
            public BuildComponentConfiguration Configuration { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private List<Lazy<BuildComponentFactory, IBuildComponentMetadata>> availableComponents;
        private ComponentConfigurationDictionary currentConfigs;
        private ComponentCache componentCache;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the selected component configurations
        /// </summary>
        public ComponentConfigurationDictionary SelectedComponents => currentConfigs;

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is used to signal that the project components have been modified
        /// </summary>
        public event EventHandler ComponentsModified;

        /// <summary>
        /// This event is raised when the control needs the current build component settings from the project
        /// </summary>
        public event EventHandler<ComponentSettingsNeededEventArgs> ComponentSettingsNeeded;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentPropertiesPageContent()
        {
            InitializeComponent();

            lbAvailableComponents.Items.SortDescriptions.Add(
                new SortDescription { Direction = ListSortDirection.Ascending });
            lbProjectComponents.Items.SortDescriptions.Add(new SortDescription("Name",
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
        /// This is used to load the build component settings when necessary
        /// </summary>
        /// <param name="currentProjectFilename">The current project's filename</param>
        /// <param name="componentSearchPaths">The paths to search for components</param>
        public void LoadComponentSettings(string currentProjectFilename, IEnumerable<string> componentSearchPaths)
        {
            if(currentProjectFilename == null)
            {
                lbProjectComponents.Items.Clear();
                gbProjectComponents.IsEnabled = false;
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
        /// Add the selected component when double clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailableComponents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.btnAddComponent_Click(sender, e);
        }

        /// <summary>
        /// Configure the selected component when double clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProjectComponents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.btnConfigure_Click(sender, e);
        }

        /// <summary>
        /// Update the build component details when the selected index changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailableComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;

            txtComponentCopyright.Text = txtComponentVersion.Text = txtComponentDescription.Text = "?? Not Found ??";

            if(availableComponents != null)
            {
                var component = availableComponents.FirstOrDefault(p => p.Metadata.Id == key);

                if(component != null)
                {
                    txtComponentCopyright.Text = component.Metadata.Copyright;
                    txtComponentVersion.Text = $"Version {component.Metadata.Version ?? "0.0.0.0"}";
                    txtComponentDescription.Text = component.Metadata.Description;
                }
            }
        }

        /// <summary>
        /// Notify the parent control when the component enabled state changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkEnabledState_Click(object sender, RoutedEventArgs e)
        {
            this.ComponentsModified?.Invoke(this, EventArgs.Empty);

            // When using the mouse to check/uncheck an item, the selected index in the containing list box
            // doesn't change.  Force it to do so.
            string key = (string)((CheckBox)sender).Content;
            var match = lbProjectComponents.Items.Cast<ComponentConfig>().FirstOrDefault(c => c.Name == key);

            if(match != null)
                lbProjectComponents.SelectedIndex = lbProjectComponents.Items.IndexOf(match);
        }

        /// <summary>
        /// Add the selected build component to the project with a default configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddComponent_Click(object sender, RoutedEventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;
            var match = lbProjectComponents.Items.Cast<ComponentConfig>().FirstOrDefault(c => c.Name == key);

            if(availableComponents != null)
            {
                // No duplicates are allowed
                if(match != null)
                    lbProjectComponents.SelectedIndex = lbProjectComponents.Items.IndexOf(match);
                else
                {
                    var component = availableComponents.FirstOrDefault(p => p.Metadata.Id == key);

                    if(component != null)
                    {
                        try
                        {
                            var c = currentConfigs.Add(key, true, String.Format(CultureInfo.InvariantCulture,
                                "<component id=\"{0}\">{1}</component>", key, component.Value.DefaultConfiguration));
                            match = new ComponentConfig { Name = key, Configuration = c };
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());

                            MessageBox.Show("Unexpected error attempting to add component: " + ex.Message,
                                Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        lbProjectComponents.SelectedIndex = lbProjectComponents.Items.Add(match);
                        lbProjectComponents.Items.Refresh();
                        btnConfigure.IsEnabled = btnDelete.IsEnabled = true;

                        this.ComponentsModified?.Invoke(this, EventArgs.Empty);

                        // Open the configuration dialog to configure it when first added if needed
                        btnConfigure_Click(sender, e);
                    }
                }
            }
        }

        /// <summary>
        /// Edit the selected build component's project configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {
            string newConfig, currentConfig;
            var config = (ComponentConfig)lbProjectComponents.SelectedItem;

            if(config != null && availableComponents != null)
            {
                var component = availableComponents.FirstOrDefault(p => p.Metadata.Id == config.Name);

                if(component != null)
                {
                    if(!component.Metadata.IsConfigurable)
                    {
                        if(sender == btnConfigure)
                            MessageBox.Show("The selected build component contains no editable configuration information",
                                Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    currentConfig = config.Configuration.Configuration;

                    try
                    {
                        newConfig = component.Value.ConfigureComponent(currentConfig,
                            componentCache.ComponentContainer);

                        // Only store it if new or if it changed
                        if(currentConfig != newConfig)
                        {
                            config.Configuration.Configuration = newConfig;
                            this.ComponentsModified?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());

                        MessageBox.Show("Unexpected error attempting to configure component: " + ex.Message,
                            Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                    MessageBox.Show("The selected build component could not be found in any of the component " +
                        "or project folders and cannot be used.", Constants.AppName, MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Delete the selected build component from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var config = (ComponentConfig)lbProjectComponents.SelectedItem;

            if(config != null && currentConfigs.ContainsKey(config.Name))
            {
                currentConfigs.Remove(config.Name);
                this.ComponentsModified?.Invoke(this, EventArgs.Empty);

                int idx = lbProjectComponents.SelectedIndex;
                lbProjectComponents.Items.RemoveAt(idx);

                if(lbProjectComponents.Items.Count == 0)
                    btnConfigure.IsEnabled = btnDelete.IsEnabled = false;
                else
                    if(idx < lbProjectComponents.Items.Count)
                        lbProjectComponents.SelectedIndex = idx;
                    else
                        lbProjectComponents.SelectedIndex = idx - 1;
            }
        }

        /// <summary>
        /// This is called when the component cache is reset prior to loading it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerReset(object sender, EventArgs e)
        {
            gbAvailableComponents.IsEnabled = gbProjectComponents.IsEnabled = false;
            lbAvailableComponents.Items.Clear();
            lbProjectComponents.Items.Clear();
            lbAvailableComponents.Items.Add("Loading...");
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            gbAvailableComponents.IsEnabled = gbProjectComponents.IsEnabled = false;
            lbAvailableComponents.Items.Clear();
            lbProjectComponents.Items.Clear();
            lbAvailableComponents.Items.Add("Unable to load build component information");
            txtComponentDescription.Text = componentCache.LastError.ToString();
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

            HashSet<string> componentIds = new HashSet<string>();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                lbAvailableComponents.Items.Clear();
                lbProjectComponents.Items.Clear();

                availableComponents = componentCache.ComponentContainer.GetExports<BuildComponentFactory,
                    IBuildComponentMetadata>().ToList();

                // Only load those that indicate that they are visible to the property page.  There may be
                // duplicate component IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the first
                // component for a unique ID will be used.
                foreach(var component in availableComponents)
                    if(!componentIds.Contains(component.Metadata.Id) && component.Metadata.IsVisible)
                    {
                        lbAvailableComponents.Items.Add(component.Metadata.Id);
                        componentIds.Add(component.Metadata.Id);
                    }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading build components: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                lbAvailableComponents.Items.Refresh();
                Mouse.OverrideCursor = null;
            }

            if(projectSettings.ProjectLoaded)
            {
                if(lbAvailableComponents.Items.Count != 0)
                {
                    lbAvailableComponents.SelectedIndex = 0;
                    gbAvailableComponents.IsEnabled = gbProjectComponents.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("No valid build components found", Constants.AppName, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    gbAvailableComponents.IsEnabled = gbProjectComponents.IsEnabled = false;
                    return;
                }

                currentConfigs = projectSettings.Components;

                foreach(var kv in currentConfigs)
                    lbProjectComponents.Items.Add(new ComponentConfig { Name = kv.Key, Configuration = kv.Value });

                if(lbProjectComponents.Items.Count != 0)
                {
                    lbProjectComponents.Items.Refresh();
                    lbProjectComponents.SelectedIndex = 0;
                    btnConfigure.IsEnabled = btnDelete.IsEnabled = true;
                }
                else
                    btnConfigure.IsEnabled = btnDelete.IsEnabled = false;
            }
        }
        #endregion
    }
}
