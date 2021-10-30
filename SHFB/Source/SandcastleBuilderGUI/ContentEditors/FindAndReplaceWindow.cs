//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : FindAndReplaceWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the form used to handle search and replace in the text editor windows
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/09/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows.Forms;

using Sandcastle.Core;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to handle search and replace in the text editor windows
    /// </summary>
    /// <remarks>This is rather crude but it works.  It's the best I could do after poking around in the editor
    /// code.  It will do for the time being even if it isn't the most efficient way of doing it.</remarks>
    public partial class FindAndReplaceWindow : BaseContentEditor
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public FindAndReplaceWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the find text
        /// </summary>
        public string FindText
        {
            get => txtFindText.Text;
            set => txtFindText.Text = value;
        }

        /// <summary>
        /// This is used to get or set the replacement text
        /// </summary>
        public string ReplaceWith
        {
            get => txtReplaceWith.Text;
            set => txtReplaceWith.Text = value;
        }

        /// <summary>
        /// This is used to get or set whether the search is case-sensitive
        /// </summary>
        public bool CaseSensitive
        {
            get => chkCaseSensitive.Checked;
            set => chkCaseSensitive.Checked = value;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Get the active document window
        /// </summary>
        /// <returns>The active topic editor window or null if not found</returns>
        private TopicEditorWindow FindActiveDocumentWindow()
        {
            TopicEditorWindow topicWindow = this.DockPanel.ActiveDocument as TopicEditorWindow;

            if(topicWindow == null)
                foreach(IDockContent content in this.DockPanel.Documents)
                {
                    topicWindow = content as TopicEditorWindow;

                    if(topicWindow != null)
                        break;
                }

            return topicWindow;
        }

        /// <summary>
        /// This is used to show or hide the Replace controls
        /// </summary>
        /// <param name="show">True to show them, false to hide them</param>
        /// <returns>The prior state of the controls (false for hidden,
        /// true for visible).</returns>
        public bool ShowReplaceControls(bool show)
        {
            bool priorState = lblReplaceWith.Visible;

            lblReplaceWith.Visible = txtReplaceWith.Visible = btnReplace.Visible = btnReplaceAll.Visible = show;

            txtFindText.Focus();
            txtFindText.SelectAll();
            return priorState;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Find the selected text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnFind_Click(object sender, EventArgs e)
        {
            TopicEditorWindow topicWindow = this.FindActiveDocumentWindow();

            epErrors.Clear();

            if(txtFindText.Text.Length == 0)
            {
                epErrors.SetError(txtFindText, "Enter some text to find");
                return;
            }

            if(topicWindow != null)
                if(!topicWindow.FindText(txtFindText.Text, chkCaseSensitive.Checked))
                    MessageBox.Show("The specified text was not found", Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Find and replace the next occurrence of the search text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReplace_Click(object sender, EventArgs e)
        {
            TopicEditorWindow topicWindow = this.FindActiveDocumentWindow();

            epErrors.Clear();

            if(txtFindText.Text.Length == 0)
            {
                epErrors.SetError(txtFindText, "Enter some text to find");
                return;
            }

            if(topicWindow != null)
                if(!topicWindow.ReplaceText(txtFindText.Text, txtReplaceWith.Text, chkCaseSensitive.Checked))
                    MessageBox.Show("The specified text was not found", Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Find and replace all occurrences of the search text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            TopicEditorWindow topicWindow = this.FindActiveDocumentWindow();

            epErrors.Clear();

            if(txtFindText.Text.Length == 0)
            {
                epErrors.SetError(txtFindText, "Enter some text to find");
                return;
            }

            if(topicWindow != null)
                if(!topicWindow.ReplaceAll(txtFindText.Text, txtReplaceWith.Text, chkCaseSensitive.Checked))
                    MessageBox.Show("The specified text was not found", Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        }
        #endregion
    }
}
