//=============================================================================
// System  : EWSoftware Folder Path User Control
// File    : FolderPathUserControl.cs
// Author  : Eric Woodruff
// Updated : 04/16/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a user control used to select a folder
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in
// all applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  04/15/2011  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.Controls
{
    /// <summary>
    /// This user control is used to select a folder
    /// </summary>
    [Description("This control is used to select a folder"), DefaultEvent("PersistablePathChanged")]
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
                this.PerformLayout();
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
            set
            {
                folderPath.IsFixedPath = isFixedPath = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path in string form
        /// </summary>
        /// <remarks>This is useful for data binding</remarks>
        [Category("Folder Browser"), Description("This path to use (useful for data binding)"), DefaultValue("")]
        public string PersistablePath
        {
            get
            {
                return folderPath.PersistablePath;
            }
            set
            {
                folderPath.PersistablePath = value;

                // If set to a rooted path, set the Fixed Path opton
                if(!String.IsNullOrEmpty(value) && Path.IsPathRooted(value))
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

        /// <summary>
        /// This raises the <see cref="PersistablePathChanged"/> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected void OnPersistablePathChanged(EventArgs e)
        {
            var handler = PersistablePathChanged;

            if(handler != null)
                handler(this, e);
        }
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

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to handle laying out the control elements
        /// </summary>
        /// <param name="levent">The event parameters</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            SizeF sizeText, sizeButton;
            int height;

            base.OnLayout(levent);

            using(Graphics g = Graphics.FromHwnd(this.Handle))
            {
                sizeText = g.MeasureString("0", this.Font);
                sizeButton = g.MeasureString(btnSelectFolder.Text, this.Font);
            }

            height = (int)sizeText.Height + (SystemInformation.FixedFrameBorderSize.Height * 2);

            btnSelectFolder.Height = height + 2;
            btnSelectFolder.Width = (int)sizeButton.Width + 15;
            btnSelectFolder.Left = this.Width - btnSelectFolder.Width;

            txtFolder.Height = height;
            txtFolder.Width = btnSelectFolder.Left - 2;

            lblExpandedPath.Height = txtFolder.Height;
            lblExpandedPath.Left = chkFixedPath.Left + chkFixedPath.Width + 6;
            lblExpandedPath.Width = btnSelectFolder.Right - lblExpandedPath.Left;

            chkFixedPath.Top = lblExpandedPath.Top = txtFolder.Height + 4;

            this.Height = lblExpandedPath.Top + ((this.ShowFixedPathOption) ? lblExpandedPath.Height + 1 : 0);

            if(this.Width < btnSelectFolder.Right)
                this.Width = btnSelectFolder.Right;
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
            chkFixedPath.Checked = folderPath.IsFixedPath;

            if(folderPath.Path.Length == 0)
                lblExpandedPath.Text = "(Not specified)";
            else
                lblExpandedPath.Text = String.Format("{0} ({1})", folderPath,
                    folderPath.Exists ? "Exists" : "Does not exist");

            this.OnPersistablePathChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Prompt the user to select a folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = this.Title;
                dlg.ShowNewFolderButton = this.ShowNewFolderButton;
                dlg.RootFolder = this.RootFolder;
                dlg.SelectedPath = Environment.GetFolderPath(this.DefaultFolder);

                if(!String.IsNullOrEmpty(folderPath.Path))
                    dlg.SelectedPath = folderPath.Path;

                // If selected, set the new folder
                if(dlg.ShowDialog() == DialogResult.OK)
                    folderPath.Path = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Update the folder path when leaving the text box
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFolder_Leave(object sender, EventArgs e)
        {
            if(txtFolder.Text != folderPath.PersistablePath)
                folderPath.PersistablePath = txtFolder.Text.Trim();
        }

        /// <summary>
        /// Changed the fixed path setting on the folder path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkFixedPath_CheckedChanged(object sender, EventArgs e)
        {
            folderPath.IsFixedPath = isFixedPath = chkFixedPath.Checked;
        }
        #endregion
    }
}
