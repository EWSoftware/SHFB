//===============================================================================================================
// System  : Sandcastle Tools - Windows platform specific code
// File    : FolderPathUserControl.cs
// Author  : Eric Woodruff
// Updated : 05/14/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
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
// 05/10/2021  EFW  Moved the code to the Windows platform assembly from SandcastleBuilder.WPF
//===============================================================================================================

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using SandcastleBuilder.Utils;

namespace Sandcastle.Platform.Windows.UserControls
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

        private bool showFixedPathOption, changingFixedState;

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
            get => showFixedPathOption;
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
        /// <remarks>Changes are applied directly to the data context</remarks>
        [Browsable(false)]
        public bool IsFixedPath
        {
            get
            {
                if(this.DataContext is FolderPath fp)
                    return fp.IsFixedPath;

                return false;
            }
            set
            {
                if(this.DataContext is FolderPath fp)
                    fp.IsFixedPath = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path in string form
        /// </summary>
        /// <remarks>Changes are applied directly to the data context</remarks>
        [Browsable(false)]
        public string PersistablePath
        {
            get
            {
                if(this.DataContext is FolderPath fp)
                    return fp.PersistablePath;

                return null;
            }
            set
            {
                if(this.DataContext is FolderPath fp)
                {
                    fp.PersistablePath = value;

                    // If set to a rooted path, set the Fixed Path option
                    bool isFixed = !String.IsNullOrWhiteSpace(value) && Path.IsPathRooted(value);

                    if(isFixed != fp.IsFixedPath)
                        fp.IsFixedPath = isFixed;
                }
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
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Connect the persistable path changed event when the data context changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void FolderPathUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue is FolderPath op)
                op.PersistablePathChanged -= this.folderPath_PersistablePathChanged;

            if(e.NewValue is FolderPath np)
            {
                np.PersistablePathChanged += this.folderPath_PersistablePathChanged;

                if(String.IsNullOrWhiteSpace(np.PersistablePath))
                    lblExpandedPath.Text = "(Not specified)";
                else
                    lblExpandedPath.Text = $"{np.PersistablePath} ({(np.Exists ? "Exists" : "Does not exist")})";

                changingFixedState = true;
                chkFixedPath.IsChecked = np.IsFixedPath;
                changingFixedState = false;
            }
        }

        /// <summary>
        /// Set the focused control when the user control gains the focus
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void FolderPathUserControl_PreviewGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if(e.NewFocus == this)
            {
                txtFolder.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle changes to the path object
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void folderPath_PersistablePathChanged(object sender, EventArgs e)
        {
            FolderPath fp = (FolderPath)sender;

            if(String.IsNullOrWhiteSpace(fp.PersistablePath))
                lblExpandedPath.Text = "(Not specified)";
            else
                lblExpandedPath.Text = $"{fp.PersistablePath} ({(fp.Exists ? "Exists" : "Does not exist")})";

            // Keep the fixed state in synch.  This isn't bound as I wasn't able to keep it's state consistent
            // when using a binding.
            changingFixedState = true;
            chkFixedPath.IsChecked = fp.IsFixedPath;
            changingFixedState = false;

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

                if(!String.IsNullOrWhiteSpace(this.PersistablePath))
                    dlg.SelectedPath = Path.GetFullPath(this.PersistablePath);

                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if(this.DataContext is FolderPath fp)
                        fp.PersistablePath = dlg.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Keep the fixed path setting in sync with the checkbox
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkFixedPath_Click(object sender, RoutedEventArgs e)
        {
            // I couldn't get it to work consistently with a binding so it's handled manually
            if(!changingFixedState)
                this.IsFixedPath = chkFixedPath.IsChecked ?? false;
        }
        #endregion
    }
}
