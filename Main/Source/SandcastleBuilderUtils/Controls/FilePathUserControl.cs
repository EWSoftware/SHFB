//=============================================================================
// System  : EWSoftware File Path User Control
// File    : FilePathUserControl.cs
// Author  : Eric Woodruff
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a user control used to select a file
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
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.Controls
{
    /// <summary>
    /// This user control is used to select a file
    /// </summary>
    [Description("This control is used to select a file"), DefaultEvent("PersistablePathChanged")]
    public partial class FilePathUserControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private FilePath filePath;
        private bool isFixedPath, showFixedPathOption;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the whether to use a File Open dialog
        /// </summary>
        /// <value>If false, a Save As dialog is used instead</value>
        [Category("File Browser"), Description("Specify whether or not to use the File Open dialog.  If false, " +
          "a Save As dialog is used instead"), DefaultValue(true)]
        public bool UseFileOpenDialog { get; set; }

        /// <summary>
        /// This is used to get or set the title of the file dialog
        /// </summary>
        [Category("File Browser"), Description("A title for the file dialog"), DefaultValue("Select a file")]
        public string Title { get; set; }

        /// <summary>
        /// This is used to get or set the filter for the file dialog
        /// </summary>
        [Category("File Browser"), Description("A filter for the file dialog"), DefaultValue("All Files (*.*)|*.*")]
        public string Filter { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to show the fixed path checkbox and expanded path
        /// </summary>
        [Category("File Browser"), Description("Show or hide the Fixed Path option"), DefaultValue(true)]
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
        /// This is used to get or set the default folder from which to start browsing
        /// </summary>
        [Category("File Browser"), Description("The default folder from which to start browsing"),
            DefaultValue(Environment.SpecialFolder.MyDocuments)]
        public Environment.SpecialFolder DefaultFolder { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to use a fixed path
        /// </summary>
        [Category("File Browser"), Description("This specifies whether or not the path is relative or absolute"),
          DefaultValue(false)]
        public bool IsFixedPath
        {
            get { return isFixedPath; }
            set
            {
                filePath.IsFixedPath = isFixedPath = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path in string form
        /// </summary>
        /// <remarks>This is useful for data binding</remarks>
        [Category("File Browser"), Description("This path to use (useful for data binding)"), DefaultValue("")]
        public string PersistablePath
        {
            get
            {
                return filePath.PersistablePath;
            }
            set
            {
                filePath.PersistablePath = value;

                // If set to a rooted path, set the Fixed Path opton
                if(!String.IsNullOrEmpty(value) && Path.IsPathRooted(value))
                    this.IsFixedPath = true;
                else
                    this.IsFixedPath = false;
            }
        }

        /// <summary>
        /// This is used to get or set the file as a <see cref="FilePath"/> object
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FilePath File
        {
            get { return filePath; }
            set
            {
                filePath.PersistablePathChanged -= filePath_PersistablePathChanged;

                if(value == null)
                    filePath = new FilePath(filePath.BasePathProvider);
                else
                    filePath = value;

                filePath.PersistablePathChanged += filePath_PersistablePathChanged;
            }
        }

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when the file path changes
        /// </summary>
        [Category("Action"), Description("Raised when the file path changes")]
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
        public FilePathUserControl()
        {
            InitializeComponent();
            this.Title = "Select a file";
            this.Filter = "All Files (*.*)|*.*";
            this.UseFileOpenDialog = this.ShowFixedPathOption = true;
            this.DefaultFolder = Environment.SpecialFolder.MyDocuments;

            filePath = new FilePath(null);
            filePath.PersistablePathChanged += filePath_PersistablePathChanged;
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
                sizeButton = g.MeasureString(btnSelectFile.Text, this.Font);
            }

            height = (int)sizeText.Height + (SystemInformation.FixedFrameBorderSize.Height * 2);

            btnSelectFile.Height = height + 2;
            btnSelectFile.Width = (int)sizeButton.Width + 15;
            btnSelectFile.Left = this.Width - btnSelectFile.Width;

            txtFile.Height = height;
            txtFile.Width = btnSelectFile.Left - 2;

            lblExpandedPath.Height = txtFile.Height;
            lblExpandedPath.Left = chkFixedPath.Left + chkFixedPath.Width + 6;
            lblExpandedPath.Width = btnSelectFile.Right - lblExpandedPath.Left;

            chkFixedPath.Top = lblExpandedPath.Top = txtFile.Height + 4;

            this.Height = lblExpandedPath.Top + ((this.ShowFixedPathOption) ? lblExpandedPath.Height + 1 : 0);

            if(this.Width < btnSelectFile.Right)
                this.Width = btnSelectFile.Right;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Handle changes to the path object
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void filePath_PersistablePathChanged(object sender, EventArgs e)
        {
            // Keep the fixed path setting the same
            if(filePath.IsFixedPath != isFixedPath)
                filePath.IsFixedPath = isFixedPath;

            txtFile.Text = filePath.PersistablePath;
            chkFixedPath.Checked = filePath.IsFixedPath;

            if(filePath.Path.Length == 0)
                lblExpandedPath.Text = "(Not specified)";
            else
                lblExpandedPath.Text = String.Format(CultureInfo.CurrentCulture, "{0} ({1})", filePath,
                    filePath.Exists ? "Exists" : "Does not exist");

            this.OnPersistablePathChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Prompt the user to select a file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            FileDialog dlg = null;
            string filename, path;

            try
            {
                if(!String.IsNullOrEmpty(filePath.Path))
                {
                    filename = Path.GetFileName(filePath.Path);
                    path = Path.GetDirectoryName(filePath.Path);
                }
                else
                    filename = path = String.Empty;

                // Figure out what type of file dialog to show and set its properties.
                if(this.UseFileOpenDialog)
                    dlg = new OpenFileDialog();
                else
                    dlg = new SaveFileDialog();

                dlg.RestoreDirectory = true;
                dlg.FileName = filename;
                dlg.InitialDirectory = path;
                dlg.Title = !String.IsNullOrEmpty(this.Title) ? this.Title : "Select a file";
                dlg.Filter = !String.IsNullOrEmpty(this.Filter) ? this.Filter : "All files (*.*)|*.*";

                // If selected, set the new file
                if(dlg.ShowDialog() == DialogResult.OK)
                    filePath.Path = dlg.FileName;
            }
            finally
            {
                if(dlg != null)
                    dlg.Dispose();
            }
        }

        /// <summary>
        /// Update the file path when leaving the text box
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFile_Leave(object sender, EventArgs e)
        {
            if(txtFile.Text != filePath.PersistablePath)
                filePath.PersistablePath = txtFile.Text.Trim();
        }

        /// <summary>
        /// Changed the fixed path setting on the file path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkFixedPath_CheckedChanged(object sender, EventArgs e)
        {
            filePath.IsFixedPath = isFixedPath = chkFixedPath.Checked;
        }
        #endregion
    }
}
