//=============================================================================
// System  : Sandcastle Help File Builder
// File    : TokenEditorWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the token files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  10/15/2008  EFW  Created the code
// 1.9.3.3  12/21/2011  EFW  Rewrote to use the shared WPF Token File Editor
//                           user control.
//=============================================================================

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to edit a token file.
    /// </summary>
    public partial class TokenEditorWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private FileItem tokenFile;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the filename
        /// </summary>
        public string Filename
        {
            get { return ucTokenEditor.Tokens.TokenFilePath; }
        }

        /// <summary>
        /// This read-only property returns the current token collection including any unsaved edits
        /// </summary>
        public TokenCollection Tokens
        {
            get
            {
                ucTokenEditor.CommitChanges();
                return ucTokenEditor.Tokens;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileItem">The project file item to edit</param>
        public TokenEditorWindow(FileItem fileItem)
        {
            InitializeComponent();

            this.Text = Path.GetFileName(fileItem.FullPath);
            this.ToolTipText = fileItem.FullPath;

            tokenFile = fileItem;

            ucTokenEditor.ContentModified += ucTokenEditor_ContentModified;
            ucTokenEditor.LoadTokenFile(tokenFile.FullPath);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Save the topic to a new filename
        /// </summary>
        /// <param name="filename">The new filename</param>
        /// <returns>True if saved successfully, false if not</returns>
        /// <overloads>There are two overloads for this method</overloads>
        public bool Save(string filename)
        {
            string projectPath = Path.GetDirectoryName(tokenFile.ProjectElement.Project.Filename);

            if(!filename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The file must reside in the project folder or a folder below it",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            ucTokenEditor.Tokens.TokenFilePath = filename;

            this.Text = Path.GetFileName(filename);
            this.ToolTipText = filename;
            this.IsDirty = true;

            return this.Save();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Return the string used to store the editor state
        /// </summary>
        /// <returns>A string containing the type name and filename</returns>
        protected override string GetPersistString()
        {
            return GetType().ToString() + "," + this.ToolTipText;
        }

        /// <inheritdoc />
        public override bool CanClose
        {
            get
            {
                // Commit changes before checking the dirty state.  We can't override IsDirty to do it since
                // it gets called a lot when properties change and may trigger subsequent calls and eventually
                // cause a stack overflow.
                ucTokenEditor.CommitChanges();

                if(!this.IsDirty)
                    return true;

                DialogResult dr = MessageBox.Show("Do you want to save your changes to '" +
                    this.ToolTipText + "?  Click YES to to save them, NO to discard them, or " +
                    "CANCEL to stay here and make further changes.", Constants.AppName,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3);

                if(dr == DialogResult.Cancel)
                    return false;

                if(dr == DialogResult.Yes)
                {
                    this.Save();

                    if(this.IsDirty)
                        return false;
                }
                else
                    this.IsDirty = false;    // Don't ask again

                return true;
            }
        }

        /// <inheritdoc />
        public override bool CanSaveContent
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool IsContentDocument
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool Save()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ucTokenEditor.CommitChanges();

                if(this.IsDirty)
                {
                    ucTokenEditor.Tokens.Save();

                    this.Text = Path.GetFileName(this.ToolTipText);
                    this.IsDirty = false;
                }

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Unable to save file.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <inheritdoc />
        public override bool SaveAs()
        {
            using(SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Token File As";
                dlg.Filter = "Token files (*.tokens)|*.tokens|All Files (*.*)|*.*";
                dlg.DefaultExt = Path.GetExtension(this.ToolTipText);
                dlg.InitialDirectory = Path.GetDirectoryName(this.ToolTipText);

                if(dlg.ShowDialog() == DialogResult.OK)
                    return this.Save(dlg.FileName);
            }

            return false;
        }

        /// <summary>
        /// This is overriden to prompt to save changes if necessary
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !this.CanClose;
            base.OnClosing(e);
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTokenEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
        {
            if(!this.IsDirty)
            {
                this.IsDirty = true;
                this.Text += "*";
            }
        }

        /// <summary>
        /// This is used to prompt for save when closing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void TokenEditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !this.CanClose;
        }
        #endregion
    }
}
