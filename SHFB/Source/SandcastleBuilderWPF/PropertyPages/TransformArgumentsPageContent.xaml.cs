//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : TransformArgumentsPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 10/29/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Transform Arguments category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/25/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
// ==============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Transform Arguments category properties
    /// </summary>
    public partial class TransformArgumentsPageContent : UserControl
    {
        #region Private data members
        //=====================================================================

        private bool loadingInfo, refreshingArgs, changingArg;
        private string lastStyle;

        private ComponentCache componentCache;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return an enumerable list of the current transformation component arguments
        /// </summary>
        public IEnumerable<TransformComponentArgument> TransformationArguments
        {
            get
            {
                foreach(TransformComponentArgument tca in lbArguments.Items)
                    yield return tca;
            }
        }

        /// <summary>
        /// This read-only property is use to see if the settings for the currently selected property are valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                var args = new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left);
                this.lbArguments_PreviewMouseDown(this, args);

                return !args.Handled;
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when any of the unbound control property values changes
        /// </summary>
        public event EventHandler PropertyChanged;

        /// <summary>
        /// This event is raised when the control needs the current presentation style settings from the project
        /// </summary>
        public event EventHandler<PresentationStyleSettingsNeededEventArgs> PresentationStyleSettingsNeeded;

        /// <summary>
        /// This is used to refresh the values
        /// </summary>
        /// <remarks>Since we are dependent on the arguments from the selected presentation style, we refresh the
        /// available arguments when necessary whenever one of the controls gains the focus.  There doesn't
        /// seem to be a way to do it when the page gains focus or is made visible in Visual Studio.</remarks>
        public event EventHandler RefreshValues;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TransformArgumentsPageContent()
        {
            InitializeComponent();

            lbArguments.GotKeyboardFocus += (s, e) => this.RefreshValues?.Invoke(s, e);
            txtValue.GotKeyboardFocus += (s, e) => this.RefreshValues?.Invoke(e, e);
            txtDescription.GotKeyboardFocus += (s, e) => this.RefreshValues?.Invoke(e, e);
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
        /// This is used to load the transform component argument settings when necessary
        /// </summary>
        /// <param name="currentProjectFilename">The current project's filename</param>
        /// <param name="componentSearchPaths">The paths to search for components</param>
        public void LoadArgumentSettings(string currentProjectFilename, IEnumerable<string> componentSearchPaths)
        {
            if(!loadingInfo)
            {
                if(currentProjectFilename == null)
                    lbArguments.Items.Clear();
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
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Validate the current transform argument and store its value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbArguments_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(lbArguments.SelectedItem == null || !lbArguments.IsEnabled)
                return;

            var arg = (TransformComponentArgument)lbArguments.SelectedItem;

            if(arg.Value != null || arg.Content == null)
                arg.Value = txtValue.Text;
            else
            {
                // Ensure the content is valid XML
                try
                {
                    arg.Content = XElement.Parse("<Content>" + txtValue.Text + "</Content>");
                }
                catch(XmlException ex)
                {
                    MessageBox.Show("The value does not appear to be valid XML.  Error " + ex.Message,
                        Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Show the selected transformation argument value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbArguments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lbArguments.SelectedItem == null || !lbArguments.IsEnabled)
                return;

            var arg = (TransformComponentArgument)lbArguments.SelectedItem;

            txtDescription.Text = arg.Description;
            chkIsForConceptualBuild.IsChecked = arg.IsForConceptualBuild;
            chkIsForReferenceBuild.IsChecked = arg.IsForReferenceBuild;

            changingArg = true;

            if(arg.Value != null || arg.Content == null)
            {
                txtValue.Height = 20;
                txtValue.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                txtValue.Text = arg.Value;
            }
            else
            {
                txtValue.Height = Double.NaN;
                txtValue.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

                var reader = arg.Content.CreateReader();
                reader.MoveToContent();
                txtValue.Text = reader.ReadInnerXml();
            }

            changingArg = false;
        }

        /// <summary>
        /// Mark the project as dirty if the transform argument value changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!changingArg && lbArguments.SelectedItem != null)
                this.PropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// This is called when the component cache is reset prior to loading it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerReset(object sender, EventArgs e)
        {
            loadingInfo = true;
            lastStyle = null;
            lbArguments.IsEnabled = false;
            lbArguments.Items.Clear();
            lbArguments.Items.Add("Loading...");
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            lbArguments.IsEnabled = loadingInfo = false;
            lbArguments.Items.Clear();
            lbArguments.Items.Add("Unable to load transform component arguments");
            txtDescription.Text = componentCache.LastError.ToString();
        }

        /// <summary>
        /// This is called when the component cache has finished being loaded and is available for use
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoaded(object sender, EventArgs e)
        {
            PresentationStyleSettingsNeededEventArgs projectSettings = new PresentationStyleSettingsNeededEventArgs();
            PresentationStyleSettings pss = null;
            TransformComponentArgument tca, clone;

            if(refreshingArgs)
                return;

            this.PresentationStyleSettingsNeeded?.Invoke(this, projectSettings);

            // Skip it if there is no project or if already loaded and nothing changed
            if(!projectSettings.ProjectLoaded || (lastStyle != null &&
              (projectSettings.PresentationStyle ?? String.Empty).Equals(lastStyle, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            try
            {
                refreshingArgs = true;

                Mouse.OverrideCursor = Cursors.Wait;

                lbArguments.IsEnabled = true;
                lbArguments.Items.Clear();

                var presentationStyleIds = new HashSet<string>();
                var presentationStyles = new List<Lazy<PresentationStyleSettings, IPresentationStyleMetadata>>();
                var transformComponentArgs = new Dictionary<string, TransformComponentArgument>();

                // There may be duplicate presentation style IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                // first component for a unique ID will be used.
                foreach(var style in componentCache.ComponentContainer.GetExports<PresentationStyleSettings,
                  IPresentationStyleMetadata>())
                    if(!presentationStyleIds.Contains(style.Metadata.Id))
                    {
                        presentationStyles.Add(style);
                        presentationStyleIds.Add(style.Metadata.Id);
                    }

                // Get the transform component arguments defined in the project if any
                if(!String.IsNullOrEmpty(projectSettings.TransformComponentArguments))
                {
                    using(var xr = new XmlTextReader("<Args>" + projectSettings.TransformComponentArguments + "</Args>",
                      XmlNodeType.Element, new XmlParserContext(null, null, null, XmlSpace.Preserve)))
                    {
                        xr.Namespaces = false;
                        xr.MoveToContent();

                        foreach(var arg in XElement.Load(xr, LoadOptions.PreserveWhitespace).Descendants("Argument"))
                        {
                            tca = new TransformComponentArgument(arg);
                            transformComponentArgs.Add(tca.Key, tca);
                        }
                    }
                }

                if(!String.IsNullOrWhiteSpace(projectSettings.PresentationStyle))
                {
                    var style = presentationStyles.FirstOrDefault(s => s.Metadata.Id.Equals(
                        projectSettings.PresentationStyle, StringComparison.OrdinalIgnoreCase));

                    if(style != null)
                        pss = style.Value;
                }

                if(pss == null)
                {
                    var style = presentationStyles.FirstOrDefault(s => s.Metadata.Id.Equals(
                        Constants.DefaultPresentationStyle, StringComparison.OrdinalIgnoreCase));

                    if(style != null)
                        pss = style.Value;
                    else
                        pss = presentationStyles.First().Value;
                }

                lastStyle = projectSettings.PresentationStyle;

                // Create an entry for each transform component argument in the presentation style
                foreach(var arg in pss.TransformComponentArguments)
                {
                    clone = arg.Clone();

                    // Use the value from the project or the cloned default if not present
                    if(transformComponentArgs.TryGetValue(arg.Key, out tca))
                    {
                        clone.Value = tca.Value;
                        clone.Content = tca.Content;
                    }

                    lbArguments.Items.Add(clone);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                MessageBox.Show("Unable to load transform component arguments.  Error " + ex.Message,
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                refreshingArgs = false;
            }

            if(lbArguments.Items.Count != 0)
            {
                lbArguments.SelectedIndex = 0;
                txtValue.IsEnabled = true;
            }
            else
                txtValue.IsEnabled = false;

            loadingInfo = false;
        }
        #endregion
    }
}
