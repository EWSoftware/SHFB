//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : BuildPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 04/17/2021
// Note    : Copyright 2017-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Build category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 11/15/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

// Ignore Spelling: mshc docx md

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;
using Sandcastle.Core.PresentationStyle;
using Sandcastle.Core.Reflection;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Build category properties
    /// </summary>
    public partial class BuildPropertiesPageContent : UserControl
    {
        #region Help file format item
        //=====================================================================

        /// <summary>
        /// This is used to select help file formats
        /// </summary>
        private sealed class HelpFileFormatItem
        {
            /// <summary>The help file format</summary>
            public HelpFileFormats Format { get; set; }

            /// <summary>The description</summary>
            public string Description { get; set; }

            /// <summary>
            /// This is used to indicate whether or not the format is currently selected
            /// </summary>
            public bool IsSelected { get; set; }

            /// <summary>
            /// This is used to indicate whether or not the format is currently active
            /// </summary>
            public bool IsActive { get; set; }
        }
        #endregion

        #region Syntax filter item
        //=====================================================================

        /// <summary>
        /// This is used to select syntax filters
        /// </summary>
        private sealed class SyntaxFilterItem
        {
            /// <summary>The syntax filter ID</summary>
            public string Id { get; set; }

            /// <summary>
            /// This is used to indicate whether or not the filter is currently selected
            /// </summary>
            public bool IsSelected { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private ReflectionDataSetDictionary reflectionDataSets;
        private List<ISyntaxGeneratorMetadata> syntaxGenerators;
        private List<IPresentationStyleMetadata> presentationStyles;
        private readonly List<HelpFileFormatItem> allHelpFileFormats;
        private List<SyntaxFilterItem> allSyntaxFilters;
        private ComponentCache componentCache;

        private string lastProjectName;
        private bool isBinding;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the currently selected help file formats
        /// </summary>
        public HelpFileFormats SelectedHelpFileFormats
        {
            get
            {
                HelpFileFormats formats = 0;

                foreach(var f in allHelpFileFormats.Where(f => f.IsActive && f.IsSelected).Select(f => f.Format))
                    formats |= f;

                if(formats == 0)
                {
                    var firstActive = allHelpFileFormats.FirstOrDefault(f => f.IsActive);

                    if(firstActive != null)
                        formats = firstActive.Format;
                    else
                        formats = allHelpFileFormats[0].Format;
                }

                return formats;
            }
            set
            {
                foreach(var f in allHelpFileFormats)
                    f.IsSelected = ((value & f.Format) == f.Format);
            }
        }

        /// <summary>
        /// This read-only property is used to get the currently selected syntax filters
        /// </summary>
        public string SelectedSyntaxFilters => ComponentUtilities.ToRecognizedSyntaxFilterIds(syntaxGenerators,
            String.Join(", ", allSyntaxFilters.Where(f => f.IsSelected).Select(f => f.Id)));

        /// <summary>
        /// This read-only property is used to get the currently selected presentation style
        /// </summary>
        public string SelectedPresentationStyle => (string)cboPresentationStyle.SelectedValue;

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is used to signal that the build formats or syntax components project property has
        /// been modified.
        /// </summary>
        public event EventHandler PropertyChanged;

        /// <summary>
        /// This event is raised when the control needs the current build property settings from the project
        /// </summary>
        public event EventHandler<BuildPropertiesNeededEventArgs> BuildPropertiesNeeded;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildPropertiesPageContent()
        {
            InitializeComponent();

            allHelpFileFormats = new List<HelpFileFormatItem>
            {
                new HelpFileFormatItem { Format = HelpFileFormats.HtmlHelp1, Description = "HTML Help 1 (chm)" },
                new HelpFileFormatItem { Format = HelpFileFormats.MSHelpViewer, Description = "MS Help Viewer (mshc)" },
                new HelpFileFormatItem { Format = HelpFileFormats.OpenXml, Description = "Open XML (docx)" },
                new HelpFileFormatItem { Format = HelpFileFormats.Markdown, Description = "Markdown (md)" },
                new HelpFileFormatItem { Format = HelpFileFormats.Website, Description = "Website (HTML/ASP.NET)" }
            };

            cboBuildAssemblerVerbosity.ItemsSource = (new Dictionary<string, string> {
                { BuildAssemblerVerbosity.AllMessages.ToString(), "All Messages" },
                { BuildAssemblerVerbosity.OnlyWarningsAndErrors.ToString(), "Only warnings and errors" },
                { BuildAssemblerVerbosity.OnlyErrors.ToString(), "Only Errors" } }).ToList();

            cboBuildAssemblerVerbosity.SelectedIndex = 1;
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
        /// This is used to set the current project for the build log file path
        /// </summary>
        /// <param name="project">The current Sandcastle project</param>
        public void SetCurrentProject(SandcastleProject project)
        {
            fpBuildLogFile.DataContext = new FilePath(project);
        }

        /// <summary>
        /// Try to load information about all available framework reflection data sets
        /// </summary>
        /// <param name="currentProject">The current Sandcastle project</param>
        public void LoadReflectionDataSetInfo(SandcastleProject currentProject)
        {
            if(reflectionDataSets != null && currentProject != null && currentProject.Filename != lastProjectName)
                return;

            lastProjectName = currentProject?.Filename;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if(currentProject != null)
                    reflectionDataSets = new ReflectionDataSetDictionary(new[] {
                        currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) });
                else
                    reflectionDataSets = new ReflectionDataSetDictionary(null);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading reflection data set info: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            cboFrameworkVersion.Items.Clear();

            if(reflectionDataSets.Keys.Count == 0)
            {
                imgFrameworkWarning.Visibility = Visibility.Visible;

                MessageBox.Show("No valid reflection data sets found", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                reflectionDataSets.Add(ReflectionDataSetDictionary.DefaultFrameworkTitle,
                    new ReflectionDataSet { Title = ReflectionDataSetDictionary.DefaultFrameworkTitle });
            }
            else
            {
                imgFrameworkWarning.Visibility = Visibility.Hidden;

                foreach(string dataSetName in reflectionDataSets.Keys.OrderBy(k => k))
                    cboFrameworkVersion.Items.Add(dataSetName);

                cboFrameworkVersion.SelectedItem = ReflectionDataSetDictionary.DefaultFrameworkTitle;
            }
        }

        /// <summary>
        /// This is used to load the build format settings when necessary
        /// </summary>
        /// <param name="currentProjectFilename">The current project's filename</param>
        /// <param name="componentSearchPaths">The paths to search for components</param>
        public void LoadBuildFormatInfo(string currentProjectFilename, IEnumerable<string> componentSearchPaths)
        {
            if(currentProjectFilename == null)
            {
                cboPresentationStyle.ItemsSource = lbSyntaxFilters.ItemsSource = lbHelpFileFormat.ItemsSource = null;
                cboPresentationStyle.IsEnabled = lbSyntaxFilters.IsEnabled = lbHelpFileFormat.IsEnabled = false;
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
        /// When a build format or syntax filter state changes, raise the property changed event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Selection_Click(object sender, RoutedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Update the info provider text and available help file formats when the presentation style changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboPresentationStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IPresentationStyleMetadata pss;

            if(presentationStyles == null || cboPresentationStyle.SelectedItem == null)
                return;

            pss = (IPresentationStyleMetadata)cboPresentationStyle.SelectedItem;

            imgPresentationStyleInfo.ToolTip = String.Format(CultureInfo.InvariantCulture,
                "{0}\r\n\r\nVersion {1}\r\n{2}", pss.Description, pss.Version, pss.Copyright);

            // Filter the help file formats based on what is supported by the presentation style
            HelpFileFormats supportedFormats = HelpFileFormats.HtmlHelp1;

            var style = componentCache.ComponentContainer.GetExports<PresentationStyleSettings,
                IPresentationStyleMetadata>().FirstOrDefault(s => s.Metadata.Id.Equals(pss.Id, StringComparison.OrdinalIgnoreCase));

            if(style != null)
                supportedFormats = style.Value.SupportedFormats;

            allHelpFileFormats.ForEach(f => f.IsActive = false);

            var styleFormats = allHelpFileFormats.Where(f => (supportedFormats & f.Format) != 0).ToList();

            if(styleFormats.Count != 0 && !styleFormats.Any(f => f.IsSelected))
                styleFormats[0].IsSelected = true;

            styleFormats.ForEach(f => f.IsActive = true);

            lbHelpFileFormat.ItemsSource = styleFormats;

            if(!isBinding)
                this.PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Update the info provider text when the syntax filter selection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbSyntaxFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(syntaxGenerators != null && lbSyntaxFilters.SelectedIndex != -1)
            {
                var generator = syntaxGenerators.FirstOrDefault(sf => sf.Id.Equals(lbSyntaxFilters.SelectedValue));

                if(generator == null)
                    imgSyntaxFilterInfo.ToolTip = null;
                else
                    imgSyntaxFilterInfo.ToolTip = String.Format(CultureInfo.InvariantCulture,
                        "{0}\r\n\r\nVersion {1}\r\n{2}", generator.Description, generator.Version,
                        generator.Copyright);
            }
        }

        /// <summary>
        /// This is called when the component cache is reset prior to loading it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerReset(object sender, EventArgs e)
        {
            try
            {
                syntaxGenerators = null;
                presentationStyles = null;

                cboPresentationStyle.IsEnabled = lbSyntaxFilters.IsEnabled = lbHelpFileFormat.IsEnabled = false;

                lbHelpFileFormat.ItemsSource = null;
                lbHelpFileFormat.Items.Clear();
                lbHelpFileFormat.Items.Add(new HelpFileFormatItem { Description = "Loading..." });

                lbSyntaxFilters.ItemsSource = null;
                lbSyntaxFilters.Items.Clear();
                lbSyntaxFilters.Items.Add(new SyntaxFilterItem { Id = "Loading..." });

                cboPresentationStyle.ItemsSource = null;
                cboPresentationStyle.Items.Clear();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            try
            {
                lbHelpFileFormat.Items.Clear();

                lbSyntaxFilters.Items.Clear();
                lbSyntaxFilters.Items.Add("Unable to load syntax filters");
                imgSyntaxFilterInfo.ToolTip = componentCache.LastError.ToString();

                cboPresentationStyle.Items.Clear();
                cboPresentationStyle.Items.Add("Unable to load presentation styles");
                cboPresentationStyle.SelectedIndex = 0;
                imgPresentationStyleInfo.ToolTip =componentCache.LastError.ToString();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// This is called when the component cache has finished being loaded and is available for use
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoaded(object sender, EventArgs e)
        {
            BuildPropertiesNeededEventArgs projectSettings = new BuildPropertiesNeededEventArgs();

            this.BuildPropertiesNeeded?.Invoke(this, projectSettings);

            HashSet<string> generatorIds = new HashSet<string>(), presentationStyleIds = new HashSet<string>();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                isBinding = true;

                cboPresentationStyle.IsEnabled = lbSyntaxFilters.IsEnabled = lbHelpFileFormat.IsEnabled = true;
                cboPresentationStyle.ItemsSource = lbHelpFileFormat.ItemsSource = lbSyntaxFilters.ItemsSource = null;

                if(cboPresentationStyle.Items.Count != 0)
                    cboPresentationStyle.Items.Clear();

                if(lbHelpFileFormat.Items.Count != 0)
                    lbHelpFileFormat.Items.Clear();

                if(lbSyntaxFilters.Items.Count != 0)
                    lbSyntaxFilters.Items.Clear();

                syntaxGenerators = new List<ISyntaxGeneratorMetadata>();
                presentationStyles = new List<IPresentationStyleMetadata>();

                var generators = componentCache.ComponentContainer.GetExports<ISyntaxGeneratorFactory,
                    ISyntaxGeneratorMetadata>().Select(g => g.Metadata).ToList();

                // There may be duplicate generator IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                // first component for a unique ID will be used.
                foreach(var generator in generators)
                    if(!generatorIds.Contains(generator.Id))
                    {
                        syntaxGenerators.Add(generator);
                        generatorIds.Add(generator.Id);
                    }

                var styles = componentCache.ComponentContainer.GetExports<PresentationStyleSettings,
                    IPresentationStyleMetadata>().Select(g => g.Metadata).ToList();

                // As above for duplicates
                foreach(var style in styles)
                    if(!presentationStyleIds.Contains(style.Id))
                    {
                        presentationStyles.Add(style);
                        presentationStyleIds.Add(style.Id);
                    }

                allSyntaxFilters = new List<SyntaxFilterItem>();

                foreach(var filter in syntaxGenerators.OrderBy(f => f.Id))
                    allSyntaxFilters.Add(new SyntaxFilterItem { Id = filter.Id });

                lbSyntaxFilters.ItemsSource = allSyntaxFilters;    

                cboPresentationStyle.ItemsSource = presentationStyles.OrderBy(s => s.IsDeprecated ? 1 : 0).ThenBy(
                    s => s.Id).ToList();
                cboPresentationStyle.SelectedValue = Constants.DefaultPresentationStyle;

                if(lbSyntaxFilters.Items.Count != 0)
                    lbSyntaxFilters.SelectedIndex = 0;
                else
                    MessageBox.Show("No valid syntax generators found", Constants.AppName, MessageBoxButton.OK,
                        MessageBoxImage.Information);

                if(cboPresentationStyle.Items.Count == 0)
                    MessageBox.Show("No valid presentation styles found", Constants.AppName, MessageBoxButton.OK,
                        MessageBoxImage.Information);

                if(projectSettings.ProjectLoaded)
                {
                    if(!String.IsNullOrWhiteSpace(projectSettings.PresentationStyle))
                    {
                        var match = cboPresentationStyle.Items.Cast<IPresentationStyleMetadata>().FirstOrDefault(p =>
                            p.Id.Equals(projectSettings.PresentationStyle, StringComparison.OrdinalIgnoreCase));

                        if(match != null)
                            cboPresentationStyle.SelectedValue = match.Id;
                    }

                    if(!String.IsNullOrWhiteSpace(projectSettings.SyntaxFilters))
                    {
                        foreach(string filter in ComponentUtilities.SyntaxFiltersFrom(syntaxGenerators,
                          projectSettings.SyntaxFilters).Select(f => f.Id))
                        {
                            var match = allSyntaxFilters.FirstOrDefault(f => f.Id == filter);

                            if(match != null)
                                match.IsSelected = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading syntax generators and presentation styles: " +
                    ex.Message, Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                isBinding = false;
            }
        }

        /// <summary>
        /// View information about other reflection data sets available as NuGet packages
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkOtherDataSets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(lnkOtherDataSets.NavigateUri.AbsoluteUri);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to navigate to website.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
