//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : MainWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2015-2025, Eric Woodruff, All rights reserved
//
// This file contains the main window for the application and is used to edit the reflection data set files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/21/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using Microsoft.Win32;

using Sandcastle.Core.Reflection;

namespace ReflectionDataManager
{
    /// <summary>
    /// This is the main window for the application and is used to edit the reflection data set files
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private data members
        //=====================================================================

        private ReflectionDataSet dataSet;
        private ICollectionView locationsView;
        private bool hasChanges;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            cboPlatform.ItemsSource = PlatformType.PlatformTypes;

            this.Load(null);
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load a reflection data set file
        /// </summary>
        /// <param name="filename">The reflection data set file to load</param>
        private void Load(string filename)
        {
            if(dataSet != null)
                dataSet.PropertyChanged -= dataSet_PropertyChanged;

            if(filename == null)
            {
                this.DataContext = dataSet = new ReflectionDataSet();
                bdAssemblyLocation.DataContext = locationsView = CollectionViewSource.GetDefaultView(dataSet.AssemblyLocations);
            }
            else
            {
                this.DataContext = dataSet = new ReflectionDataSet(filename);
                bdAssemblyLocation.DataContext = locationsView = CollectionViewSource.GetDefaultView(dataSet.AssemblyLocations);
            }

            dataSet.PropertyChanged += dataSet_PropertyChanged;
            hasChanges = false;

            this.Title = "Reflection Data Manager - [" + ((filename == null) ? "New" : Path.GetFileName(filename)) + "]";
        }

        /// <summary>
        /// This is used to check for changes and prompt the user to save them if necessary
        /// </summary>
        /// <returns>Returns <c>OK</c> if changes were saved or discarded or <c>Cancel</c> if the caller should
        /// cancel their operation.</returns>
        private MessageBoxResult CheckForChanges()
        {
            MessageBoxResult result = MessageBoxResult.OK;

            if(hasChanges)
            {
                result = MessageBox.Show("Do you want to save your changes to this reflection data file?",
                  "Reflection Data Manager", MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                  MessageBoxResult.Cancel);

                if(result == MessageBoxResult.Yes)
                {
                    this.cmdSave_Executed(this, null);

                    result = hasChanges ? MessageBoxResult.Cancel : MessageBoxResult.OK;
                }
                else
                    if(result == MessageBoxResult.No)
                        result = MessageBoxResult.OK;
            }

            return result;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is called when a change is made to note that the file needs to be saved
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dataSet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            hasChanges = true;

            if(this.Title.Length > 0 && this.Title[this.Title.Length - 1] != '*')
                this.Title += "*";
        }

        /// <summary>
        /// Check for changes and prompt to save when closing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = (this.CheckForChanges() != MessageBoxResult.OK);
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Show the About dialog box
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var aboutDlg = new AboutDlg();

            aboutDlg.ShowDialog();
        }

        /// <summary>
        /// Create a new reflection data definition file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(this.CheckForChanges() == MessageBoxResult.OK)
                this.Load(null);
        }

        /// <summary>
        /// Open an existing reflection data definition file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(this.CheckForChanges() == MessageBoxResult.OK)
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "Reflection data files (*.reflection)|*.reflection|All files (*.*)|*.*"
                };

                if((dlg.ShowDialog() ?? false))
                    this.Load(dlg.FileName);
            }
        }

        /// <summary>
        /// Save the reflection data definition file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(dataSet.Filename))
                cmdSaveAs_Executed(sender, e);
            else
            {
                try
                {
                    dataSet.Save();
                    hasChanges = false;

                    this.Title = "Reflection Data Manager - [" + Path.GetFileName(dataSet.Filename) + "]";
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to save changes: " + ex.Message, "Reflection Data Manager",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        /// <summary>
        /// Save the reflection data definition file to a new name
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Reflection data files (*.reflection)|*.reflection|All files (*.*)|*.*"
            };

            if((dlg.ShowDialog() ?? false))
            {
                dataSet.Filename = dlg.FileName;
                hasChanges = true;
                this.cmdSave_Executed(sender, e);
            }
        }

        /// <summary>
        /// Open the build form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdBuild_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(dataSet.AssemblyLocations.Count == 0 || !dataSet.IncludedAssemblies.Any())
            {
                MessageBox.Show("You must add at least one location with included assemblies to build",
                    "Reflection Data Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var buildDlg = new BuildReflectionDataDlg(dataSet);

            buildDlg.ShowDialog();
        }

        /// <summary>
        /// Determine whether the commands can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(e.Command == NavigationCommands.NextPage)
            {
                e.CanExecute = (locationsView != null &&
                    locationsView.CurrentPosition < locationsView.SourceCollection.OfType<AssemblyLocation>().Count() - 1);
            }
            else if(e.Command == NavigationCommands.PreviousPage)
            {
                e.CanExecute = (locationsView != null && locationsView.CurrentPosition > 0);
            }
            else if(e.Command == ApplicationCommands.Delete || e.Command == ApplicationCommands.Replace ||
              e.Command == NavigationCommands.Refresh)
            {
                e.CanExecute = (locationsView != null && locationsView.CurrentPosition != -1);
            }
        }

        /// <summary>
        /// Move to the previous assembly location entry
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdPreviousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            locationsView.MoveCurrentToPrevious();
        }

        /// <summary>
        /// Move to the next assembly location entry
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdNextPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            locationsView.MoveCurrentToNext();
        }

        /// <summary>
        /// Add a new assembly location
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            dataSet.AssemblyLocations.Add(new AssemblyLocation());
            locationsView.MoveCurrentToLast();
        }

        /// <summary>
        /// Delete the selected assembly location
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to remove the current assembly location?",
              "Reflection Data Manager", MessageBoxButton.YesNo, MessageBoxImage.Question,
              MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                dataSet.AssemblyLocations.Remove((AssemblyLocation)locationsView.CurrentItem);
            }
        }

        /// <summary>
        /// Select the folder that contains the assemblies
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdReplace_Executed(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.FolderBrowserDialog dlg = new();
            
            var location = (AssemblyLocation)locationsView.CurrentItem;

            dlg.Description = "Select the folder containing the assemblies";

            if(!String.IsNullOrWhiteSpace(location.StoredPath))
                dlg.SelectedPath = location.Path;
            else
                dlg.SelectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Reference Assemblies\Microsoft\Framework");

            // If selected, set the new folder
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                location.StoredPath = dlg.SelectedPath;
        }

        /// <summary>
        /// Refresh the current set of assemblies in the selected location
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdRefresh_Executed(object sender, RoutedEventArgs e)
        {
            ((AssemblyLocation)locationsView.CurrentItem).DetermineAssemblyDetails(false);
        }
        #endregion
    }
}
