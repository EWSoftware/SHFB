//=============================================================================
// System  : Sandcastle Help File Builder
// File    : ContentLayoutWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/25/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the conceptual content items.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/24/2008  EFW  Created the code
// 1.8.0.0  09/04/2008  EFW  Reworked for use with the new project format
// 1.9.0.0  07/10/2010  EFW  Added support for parenting API content anywhere
// 1.9.3.3  12/19/2011  EFW  Rewrote to use the shared WPF Content Layout
//                           Editor user control.
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MouseCursor = System.Windows.Forms.Cursor;
using MouseCursors = System.Windows.Forms.Cursors;
using WinFormsMessageBox = System.Windows.Forms.MessageBox;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF;
using SandcastleBuilder.WPF.Commands;
using SandcastleBuilder.WPF.UserControls;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to edit the conceptual content items
    /// </summary>
    public partial class ContentLayoutWindow : BaseContentEditor
    {
        #region Private data Members
        //=====================================================================

        private FileItem contentLayoutFile;
        private ContentLayoutEditorControl ucContentLayoutEditor;

        //=====================================================================
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the filename
        /// </summary>
        public string Filename
        {
            get { return contentLayoutFile.FullPath; }
        }

        /// <summary>
        /// This read-only property returns the current topic collection including any unsaved edits
        /// </summary>
        public TopicCollection Topics
        {
            get
            {
                ucContentLayoutEditor.CommitChanges();
                return ucContentLayoutEditor.Topics;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileItem">The project file item to edit</param>
        public ContentLayoutWindow(FileItem fileItem)
        {
            InitializeComponent();

            this.Text = Path.GetFileName(fileItem.FullPath);
            this.ToolTipText = fileItem.FullPath;

            contentLayoutFile = fileItem;

            ucContentLayoutEditor = new ContentLayoutEditorControl();
            ehContentLayoutEditorHost.Child = ucContentLayoutEditor;

            // Hook up the command bindings and event handlers
            ucContentLayoutEditor.CommandBindings.Add(new CommandBinding(EditorCommands.Edit,
                cmdEdit_Executed, cmdEdit_CanExecute));
            ucContentLayoutEditor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddFromTemplate,
                cmdAddFromTemplate_Executed));
            ucContentLayoutEditor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddExistingFile,
                cmdAddExistingFile_Executed));
            ucContentLayoutEditor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddAllFromFolder,
                cmdAddAllFromFolder_Executed));

            ucContentLayoutEditor.ContentModified += ucContentLayoutEditor_ContentModified;
            ucContentLayoutEditor.AssociateTopic += ucContentLayoutEditor_AssociateTopic;

            // Load the content layout file
            ucContentLayoutEditor.LoadContentLayoutFile(contentLayoutFile);
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
            string projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);

            if(!filename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                WinFormsMessageBox.Show("The file must reside in the project folder or a folder below it",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            contentLayoutFile.Include = new FilePath(filename, contentLayoutFile.ProjectElement.Project);

            this.Text = Path.GetFileName(filename);
            this.ToolTipText = filename;
            this.IsDirty = true;

            return this.Save();
        }

        /// <summary>
        /// Add a new topic file to the project and the editor
        /// </summary>
        /// <param name="filename">The filename of the topic to add</param>
        /// <param name="addAsChild">True to add as a child of the selected
        /// topic or false to add it as a sibling.</param>
        /// <returns>The topic that was just added</returns>
        private Topic AddTopicFile(string filename, bool addAsChild)
        {
            Topic newTopic, currentTopic = ucContentLayoutEditor.CurrentTopic;
            string newPath = filename, projectPath = Path.GetDirectoryName(
                contentLayoutFile.ProjectElement.Project.Filename);

            // The file must reside under the project path
            if(!Path.GetDirectoryName(filename).StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                newPath = Path.Combine(projectPath, Path.GetFileName(filename));

            // Add the file to the project if not already there
            FileItem newItem = contentLayoutFile.ProjectElement.Project.AddFileToProject(filename, newPath);

            // Add the topic to the editor's collection
            newTopic = new Topic
            {
                TopicFile = new TopicFile(newItem)
            };

            if(addAsChild && currentTopic != null)
            {
                currentTopic.Subtopics.Add(newTopic);
                currentTopic.IsExpanded = true;
            }
            else
                if(currentTopic == null)
                    ucContentLayoutEditor.Topics.Add(newTopic);
                else
                    currentTopic.Parent.Insert(currentTopic.Parent.IndexOf(currentTopic) + 1, newTopic);

            newTopic.IsSelected = true;

            return newTopic;
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
                ucContentLayoutEditor.CommitChanges();

                if(!this.IsDirty)
                    return true;

                DialogResult dr = WinFormsMessageBox.Show("Do you want to save your changes to '" +
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
                MouseCursor.Current = MouseCursors.WaitCursor;
                ucContentLayoutEditor.CommitChanges();

                if(this.IsDirty)
                {
                    ucContentLayoutEditor.Topics.Save();

                    this.Text = Path.GetFileName(this.ToolTipText);
                    this.IsDirty = false;
                }

                return true;
            }
            catch(Exception ex)
            {
                WinFormsMessageBox.Show("Unable to save file.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                MouseCursor.Current = MouseCursors.Default;
            }
        }

        /// <inheritdoc />
        public override bool SaveAs()
        {
            using(SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Content Layout File As";
                dlg.Filter = "Content layout files (*.content)|*.content|All Files (*.*)|*.*";
                dlg.DefaultExt = Path.GetExtension(this.ToolTipText);
                dlg.InitialDirectory = Path.GetDirectoryName(this.ToolTipText);

                if(dlg.ShowDialog() == DialogResult.OK)
                    return this.Save(dlg.FileName);
            }

            return false;
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to prompt for save when closing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ContentLayoutWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !this.CanClose;
        }

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucContentLayoutEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
        {
            if(!this.IsDirty)
            {
                this.IsDirty = true;
                this.Text += "*";
            }
        }

        /// <summary>
        /// Associate a project file with the current topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucContentLayoutEditor_AssociateTopic(object sender, RoutedEventArgs e)
        {
            FileItem newItem;
            Topic t = ucContentLayoutEditor.CurrentTopic;
            string newPath, projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);

            if(t != null)
                using(OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select the conceptual topic file";
                    dlg.Filter = "Conceptual Topics (*.aml)|*.aml|All files (*.*)|*.*";
                    dlg.DefaultExt = "aml";
                    dlg.InitialDirectory = projectPath;
                    dlg.CheckFileExists = true;

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        // The file must reside under the project path
                        newPath = dlg.FileName;

                        if(!Path.GetDirectoryName(newPath).StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                            newPath = Path.Combine(projectPath, Path.GetFileName(newPath));

                        // Add the file to the project if not already there
                        newItem = contentLayoutFile.ProjectElement.Project.AddFileToProject(
                            dlg.FileName, newPath);

                        t.TopicFile = new TopicFile(newItem);

                        // Let the caller know we associated a file with the topic
                        e.Handled = true;

                        MainForm.Host.ProjectExplorer.RefreshProject();
                    }
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
            Topic t = ucContentLayoutEditor.CurrentTopic;

            e.CanExecute = (t != null && t.TopicFile != null && t.TopicFile.DocumentType > DocumentType.Invalid);
        }

        /// <summary>
        /// Edit the selected file for editing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // If the sender is a topic, use that instead.  Due to the way the WPF tree view works, the
            // selected topic isn't always the one we just added when it's the first child of a parent topic.
            Topic t = sender as Topic;

            if(t == null)
                t = ucContentLayoutEditor.CurrentTopic;

            if(t.TopicFile != null)
            {
                string fullName = t.TopicFile.FullPath;

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

        /// <summary>
        /// Add a new topic from a template file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddFromTemplate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string newFilePath = Path.GetDirectoryName(contentLayoutFile.FullPath);

            if(ucContentLayoutEditor.CurrentTopic != null && ucContentLayoutEditor.CurrentTopic.TopicFile != null)
                newFilePath = Path.GetDirectoryName(ucContentLayoutEditor.CurrentTopic.TopicFile.FullPath);

            using(SelectFileTemplateDlg dlg = new SelectFileTemplateDlg(true, newFilePath))
            {
                // If created, add it to the project, refresh the Project Explorer, and open the file for editing
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    Topic t = this.AddTopicFile(dlg.NewFilename, e.Parameter != null);
                    MainForm.Host.ProjectExplorer.RefreshProject();

                    // Ensure that the topic we just added is opened by passing it as the sender
                    cmdEdit_Executed(t, e);
                }
            }
        }

        /// <summary>
        /// Add a new topic from an existing file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddExistingFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic t = ucContentLayoutEditor.CurrentTopic;
            string projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the conceptual topic file(s)";
                dlg.Filter = "Conceptual Topics (*.aml)|*.aml|All files (*.*)|*.*";
                dlg.DefaultExt = "aml";
                dlg.InitialDirectory = (t != null && t.TopicFile != null) ?
                    Path.GetDirectoryName(t.TopicFile.FullPath) : projectPath;
                dlg.Multiselect = true;

                // If selected, add the new file(s).  Filenames that are
                // already in the collection are ignored.
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach(string filename in dlg.FileNames)
                    {
                        this.AddTopicFile(filename, e.Parameter != null);

                        if(t != null)
                            t.IsSelected = true;
                    }

                    MainForm.Host.ProjectExplorer.RefreshProject();
                }
            }
        }

        /// <summary>
        /// Add new topics from all files in a selected folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddAllFromFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TopicCollection parent, newTopics = new TopicCollection(null);
            Topic selectedTopic = ucContentLayoutEditor.CurrentTopic;
            string projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);
            int idx;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select a folder to add all of its content";
                dlg.SelectedPath = (selectedTopic != null && selectedTopic.TopicFile != null) ?
                    Path.GetDirectoryName(selectedTopic.TopicFile.FullPath) : projectPath;

                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        MouseCursor.Current = MouseCursors.WaitCursor;

                        newTopics.AddTopicsFromFolder(dlg.SelectedPath, dlg.SelectedPath,
                            contentLayoutFile.ProjectElement.Project);

                        MainForm.Host.ProjectExplorer.RefreshProject();
                    }
                    finally
                    {
                        MouseCursor.Current = MouseCursors.Default;
                    }
                }

                if(newTopics.Count != 0)
                    if(e.Parameter == null || selectedTopic == null)
                    {
                        // Insert as siblings
                        if(selectedTopic == null)
                        {
                            parent = ucContentLayoutEditor.Topics;
                            idx = 0;
                        }
                        else
                        {
                            parent = selectedTopic.Parent;
                            idx = parent.IndexOf(selectedTopic) + 1;
                        }

                        foreach(Topic t in newTopics)
                            parent.Insert(idx++, t);
                    }
                    else
                    {
                        // Insert as children
                        parent = selectedTopic.Subtopics;

                        foreach(Topic t in newTopics)
                            parent.Add(t);

                        selectedTopic.IsExpanded = true;
                    }
            }
        }
        #endregion
    }
}
