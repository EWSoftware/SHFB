//=============================================================================
// System  : Sandcastle Help File Builder
// File    : TokenEditorWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/28/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
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
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.Design;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to edit a token file.
    /// </summary>
    public partial class TokenEditorWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private TokenCollection tokens;
        private bool isDeleting;
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

            sbStatusBarText.InstanceStatusBar = MainForm.Host.StatusBarTextLabel;

            editor.TextEditorProperties.Font = Settings.Default.TextEditorFont;
            editor.TextEditorProperties.ShowLineNumbers = Settings.Default.ShowLineNumbers;
            editor.SetHighlighting("XML");

            tokens = new TokenCollection(fileItem);
            tokens.Load();
            tokens.ListChanged += new ListChangedEventHandler(tokens_ListChanged);

            this.Text = Path.GetFileName(fileItem.FullPath);
            this.ToolTipText = fileItem.FullPath;
            this.LoadTokens();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Update the editor font used based on the selected user settings
        /// </summary>
        public void UpdateFont()
        {
            editor.TextEditorProperties.Font = Settings.Default.TextEditorFont;
            editor.TextEditorProperties.ShowLineNumbers = Settings.Default.ShowLineNumbers;
        }

        /// <summary>
        /// Save the topic to a new filename
        /// </summary>
        /// <param name="filename">The new filename</param>
        /// <returns>True if saved successfully, false if not</returns>
        /// <overloads>There are two overloads for this method</overloads>
        public bool Save(string filename)
        {
            string projectPath = Path.GetDirectoryName(
                tokens.FileItem.ProjectElement.Project.Filename);

            if(!filename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The file must reside in the project folder " +
                    "or a folder below it", Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            tokens.FileItem.Include = new FilePath(filename,
                tokens.FileItem.ProjectElement.Project);
            this.Text = Path.GetFileName(filename);
            this.ToolTipText = filename;
            this.IsDirty = true;
            return this.Save();
        }

        /// <summary>
        /// Load the tree view with the tokens and set the form up to edit them
        /// </summary>
        private void LoadTokens()
        {
            TreeNode node;

            foreach(Token t in tokens)
            {
                node = tvTokens.Nodes.Add(t.TokenName);
                node.Tag = t;
            }

            if(tokens.Count != 0)
                tvTokens.SelectedNode = tvTokens.Nodes[0];
            else
                txtTokenID.Enabled = editor.Enabled = btnDelete.Enabled = false;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override bool CanClose
        {
            get
            {
                tvTokens_BeforeSelect(tvTokens, new TreeViewCancelEventArgs(
                    tvTokens.SelectedNode, false, TreeViewAction.Unknown));

                if(!this.IsDirty)
                    return true;

                DialogResult dr = MessageBox.Show("Do you want to save your " +
                    "changes to '" + this.ToolTipText + "?  Click YES to " +
                    "to save them, NO to discard them, or CANCEL to stay " +
                    "here and make further changes.", "Topic Editor",
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

                tvTokens_BeforeSelect(tvTokens, new TreeViewCancelEventArgs(
                    tvTokens.SelectedNode, false, TreeViewAction.Unknown));

                if(this.IsDirty)
                {
                    tokens.Save();
                    this.Text = Path.GetFileName(this.ToolTipText);
                    this.IsDirty = false;
                }

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Unable to save file.  Reason: " + ex.Message,
                    "Token File Editor", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                dlg.Filter = "Token files (*.tokens)|*.tokens|" +
                    "All Files (*.*)|*.*";
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
        void tokens_ListChanged(object sender, ListChangedEventArgs e)
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
        private void TokenEditorWindow_FormClosing(object sender,
          FormClosingEventArgs e)
        {
            e.Cancel = !this.CanClose;
        }

        /// <summary>
        /// Add a new token
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            Token t = new Token("New Token", "Token value");
            TreeNode node = tvTokens.Nodes.Add(t.TokenName);
            node.Tag = t;
            tokens.Add(t);
            tvTokens.SelectedNode = node;

            if(tokens.Count == 1)
                txtTokenID.Enabled = editor.Enabled = btnDelete.Enabled = true;

            txtTokenID.Focus();
        }

        /// <summary>
        /// Delete the selected token
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            TreeNode node = tvTokens.SelectedNode;
            Token t = (Token)node.Tag;

            if(MessageBox.Show("Do you want to delete the token '" +
              t.TokenName + "'?", Constants.AppName, MessageBoxButtons.YesNo,
              MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) ==
              DialogResult.Yes)
            {
                isDeleting = true;
                tokens.Remove(t);
                tvTokens.Nodes.Remove(node);

                if(tokens.Count == 0)
                {
                    txtTokenID.Text = editor.Text = String.Empty;
                    txtTokenID.Enabled = editor.Enabled = btnDelete.Enabled = false;
                    editor.Refresh();
                }

                isDeleting = false;
                tvTokens.Focus();
            }
        }
        #endregion

        #region Tree view event handlers
        //=====================================================================

        /// <summary>
        /// This validates the token ID and value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvTokens_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = tvTokens.SelectedNode;

            if(node == null || isDeleting)
                return;

            Token t = (Token)node.Tag;

            if(txtTokenID.Text.Trim().Length == 0)
                txtTokenID.Text = Guid.NewGuid().ToString();

            if(editor.Text.Trim().Length == 0)
                editor.Text = "(No token content)";

            if(txtTokenID.Text.Trim() != t.TokenName ||
              editor.Text != t.TokenValue)
            {
                // Store new values in token
                t.TokenName = txtTokenID.Text.Trim();
                t.TokenValue = editor.Text;
                node.Text = t.TokenName;

                if(!this.IsDirty)
                {
                    this.IsDirty = true;
                    this.Text += "*";
                }
            }
        }

        /// <summary>
        /// This loads the selected token ID and text into the editor fields
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvTokens_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Token t = (Token)e.Node.Tag;

            txtTokenID.Text = t.TokenName;
            editor.Text = t.TokenValue;
            editor.Refresh();
        }
        #endregion
    }
}
