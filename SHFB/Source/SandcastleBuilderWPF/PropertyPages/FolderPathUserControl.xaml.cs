//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : FolderPathUserControl.cs
// Author  : Eric Woodruff
// Updated : 10/30/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a user control used to select a folder
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/15/2011  EFW  Created the code
// 10/30/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to select a folder
    /// </summary>
    [Description("This control is used to select a folder"), DefaultProperty("PersistablePath"),
      DefaultEvent("PersistablePathChanged")]
    public partial class FolderPathUserControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private FolderPath folderPath;
        private bool isFixedPath, showFixedPathOption;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the title of the folder dialog
        /// </summary>
        [Category("Folder Browser"), Description("A title for the Folder Browser dialog"), DefaultValue("Select a folder")]
        public string Title { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to show the New Folder button
        /// </summary>
        [Category("Folder Browser"), Description("Show or hide the New Folder button"), DefaultValue(false)]
        public bool ShowNewFolderButton { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to show the fixed path checkbox and expanded path
        /// </summary>
        [Category("Folder Browser"), Description("Show or hide the Fixed Path option"), DefaultValue(true)]
        public bool ShowFixedPathOption
        {
            get { return showFixedPathOption; }
            set
            {
                showFixedPathOption = value;
                grdFixPathInfo.Visibility = showFixedPathOption ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// This is used to get or set the root folder used to limit browsing
        /// </summary>
        [Category("Folder Browser"), Description("The root folder used to limit browsing"),
            DefaultValue(Environment.SpecialFolder.Desktop)]
        public Environment.SpecialFolder RootFolder { get; set; }

        /// <summary>
        /// This is used to get or set the default folder from which to start browsing
        /// </summary>
        [Category("Folder Browser"), Description("The default folder from which to start browsing"),
            DefaultValue(Environment.SpecialFolder.Desktop)]
        public Environment.SpecialFolder DefaultFolder { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to use a fixed path
        /// </summary>
        [Category("Folder Browser"), Description("This specifies whether or not the path is relative or absolute"),
          DefaultValue(false)]
        public bool IsFixedPath
        {
            get { return isFixedPath; }
            set { folderPath.IsFixedPath = isFixedPath = value; }
        }

        /// <summary>
        /// This is used to get or set the path in string form
        /// </summary>
        /// <remarks>This is useful for data binding</remarks>
        [Category("Folder Browser"), Description("This path to use (useful for data binding)"), DefaultValue("")]
        public string PersistablePath
        {
            get { return folderPath.PersistablePath; }
            set
            {
                folderPath.PersistablePath = value;

                // If set to a rooted path, set the Fixed Path option
                if(!String.IsNullOrWhiteSpace(value) && Path.IsPathRooted(value))
                    this.IsFixedPath = true;
                else
                    this.IsFixedPath = false;
            }
        }

        /// <summary>
        /// This is used to get or set the folder as a <see cref="FolderPath"/> object
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FolderPath Folder
        {
            get { return folderPath; }
            set
            {
                folderPath.PersistablePathChanged -= folderPath_PersistablePathChanged;

                if(value == null)
                    folderPath = new FolderPath(folderPath.BasePathProvider);
                else
                    folderPath = value;

                folderPath.PersistablePathChanged += folderPath_PersistablePathChanged;
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when the folder path changes
        /// </summary>
        [Category("Action"), Description("Raised when the folder path changes")]
        public event EventHandler PersistablePathChanged;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public FolderPathUserControl()
        {
            InitializeComponent();

            this.Title = "Select a folder";
            this.ShowFixedPathOption = true;

            folderPath = new FolderPath(null);
            folderPath.PersistablePathChanged += folderPath_PersistablePathChanged;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Handle changes to the path object
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void folderPath_PersistablePathChanged(object sender, EventArgs e)
        {
            // Keep the fixed path setting the same
            if(folderPath.IsFixedPath != isFixedPath)
                folderPath.IsFixedPath = isFixedPath;

            txtFolder.Text = folderPath.PersistablePath;
            chkFixedPath.IsChecked = folderPath.IsFixedPath;

            if(folderPath.Path.Length == 0)
                lblExpandedPath.Text = "(Not specified)";
            else
                lblExpandedPath.Text = $"{folderPath} ({(folderPath.Exists ? "Exists" : "Does not exist")})";

            this.PersistablePathChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Prompt the user to select a folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using(var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = this.Title;
                dlg.ShowNewFolderButton = this.ShowNewFolderButton;
                dlg.RootFolder = this.RootFolder;
                dlg.SelectedPath = Environment.GetFolderPath(this.DefaultFolder);

                if(!String.IsNullOrWhiteSpace(folderPath.Path))
                    dlg.SelectedPath = folderPath.Path;

                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    folderPath.Path = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Update the folder path when leaving the text box
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFolder_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(txtFolder.Text != folderPath.PersistablePath)
                folderPath.PersistablePath = txtFolder.Text.Trim();
        }

        /// <summary>
        /// Change the fixed path setting on the folder path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkFixedPath_Click(object sender, RoutedEventArgs e)
        {
            folderPath.IsFixedPath = isFixedPath = (chkFixedPath.IsChecked ?? false);
        }
        #endregion
    }
}
