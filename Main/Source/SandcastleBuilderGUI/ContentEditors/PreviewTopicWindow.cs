//=============================================================================
// System  : Sandcastle Help File Builder
// File    : PreviewTopicWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/05/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to preview a topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/27/2008  EFW  Created the code
// 1.8.0.0  07/26/2008  EFW  Reworked for use with the new project format
// 1.9.0.0  06/07/2010  EFW  Added support for multi-format build output
// 1.9.3.4  01/18/2012  EFW  Rewrote to use the shared WPF Topic Previewer
//                           user control.
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using WinFormsMessageBox = System.Windows.Forms.MessageBox;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF.Commands;
using SandcastleBuilder.WPF.UserControls;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to preview a topic
    /// </summary>
    public partial class PreviewTopicWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private TopicPreviewerControl ucTopicPreviewer;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PreviewTopicWindow()
        {
            InitializeComponent();

            ucTopicPreviewer = new TopicPreviewerControl();
            ehTopicPreviewerHost.Child = ucTopicPreviewer;

            // Hook up the command bindings and event handlers
            ucTopicPreviewer.CommandBindings.Add(new CommandBinding(EditorCommands.Edit,
                cmdEdit_Executed, cmdEdit_CanExecute));

            ucTopicPreviewer.FileContentNeeded += ucTopicPreviewer_FileContentNeeded;
            ucTopicPreviewer.TopicContentNeeded += ucTopicPreviewer_TopicContentNeeded;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load the conceptual content information and preview the topics
        /// </summary>
        /// <param name="project">The current project</param>
        /// <param name="previewTopic">The filename of the topic to show as the starting topic or null for the
        /// first topic.</param>
        public void PreviewTopic(SandcastleProject project, string previewTopic)
        {
            if(project == null || ucTopicPreviewer.CurrentProject == null ||
              ucTopicPreviewer.CurrentProject.Filename != project.Filename)
                ucTopicPreviewer.CurrentProject = project;

            ucTopicPreviewer.Refresh(false);
            ucTopicPreviewer.FindAndDisplay(previewTopic);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to ignore Ctrl+F4 which closes the window rather
        /// than hide it when docked as a document.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing && this.DockState == DockState.Document)
            {
                this.Hide();
                e.Cancel = true;
            }
            else
                base.OnFormClosing(e);
        }
        #endregion

        #region Routed event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get information from token and content layoutfiles open in editors
        /// so that current information is displayed for them in the topic previewer control.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>Site maps are ignored as they aren't supported by the topic previewer</remarks>
        private void ucTopicPreviewer_FileContentNeeded(object sender, FileContentNeededEventArgs e)
        {
            ContentLayoutWindow contentLayoutWindow;
            TokenEditorWindow tokenEditorWindow;

            foreach(IDockContent content in this.DockPanel.Documents)
            {
                contentLayoutWindow = content as ContentLayoutWindow;

                if(contentLayoutWindow != null)
                    e.ContentLayoutFiles.Add(contentLayoutWindow.Filename, contentLayoutWindow.Topics);
                else
                {
                    tokenEditorWindow = content as TokenEditorWindow;

                    if(tokenEditorWindow != null)
                        e.TokenFiles.Add(tokenEditorWindow.Filename, tokenEditorWindow.Tokens);
                }
            }
        }

        /// <summary>
        /// This is used to get the content of a specific topic file if it is open in an editor so that the
        /// current content is displayed for it in the topic previewer control.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTopicPreviewer_TopicContentNeeded(object sender, TopicContentNeededEventArgs e)
        {
            TopicEditorWindow topicEditor;

            foreach(IDockContent content in this.DockPanel.Documents)
            {
                topicEditor = content as TopicEditorWindow;

                if(topicEditor != null && topicEditor.Filename.Equals(e.TopicFilename, StringComparison.OrdinalIgnoreCase))
                    e.TopicContent = topicEditor.FileContent;
            }
        }
        #endregion

        #region Command event handlers
        //=====================================================================

        /// <summary>
        /// Determine whether or not the command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            TocEntry t = ucTopicPreviewer.CurrentTopic;

            e.CanExecute = (t != null && t.SourceFile.Exists);
        }

        /// <summary>
        /// Open the selected file for editing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TocEntry t = ucTopicPreviewer.CurrentTopic;

            if(t.SourceFile.Exists)
            {
                string fullName = t.SourceFile;

                // If the document is already open, just activate it
                foreach(IDockContent content in this.DockPanel.Documents)
                    if(String.Compare(content.DockHandler.ToolTipText, fullName, true,
                      CultureInfo.CurrentCulture) == 0)
                    {
                        content.DockHandler.Activate();
                        return;
                    }

                if(File.Exists(fullName))
                {
                    TopicEditorWindow editor = new TopicEditorWindow(fullName);
                    editor.Show(this.DockPanel);
                }
                else
                    WinFormsMessageBox.Show("File does not exist: " + fullName, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                WinFormsMessageBox.Show("No file is associated with this topic", Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
}
