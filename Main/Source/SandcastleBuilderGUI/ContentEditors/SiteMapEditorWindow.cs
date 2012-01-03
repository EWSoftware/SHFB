//=============================================================================
// System  : Sandcastle Help File Builder
// File    : SiteMapEditorWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/03/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit site map files that defines the
// table of contents layout for additional content items.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/03/2007  EFW  Created the code
// 1.6.0.7  04/12/2008  EFW  Added support for splitting the table of contents
// 1.8.0.0  09/04/2008  EFW  Reworked for use with the new project format
// 1.9.3.3  12/21/2011  EFW  Rewrote to use the shared WPF Site Map Editor
//                           user control.
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
    /// This form is used to edit site map files that defines the table of
    /// contents layout for additional content items.
    /// </summary>
    public partial class SiteMapEditorWindow : BaseContentEditor
    {
        #region Private data Members
        //=====================================================================

        private FileItem siteMapFile;
        private SiteMapEditorControl ucSiteMapEditor;

        //=====================================================================
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the filename
        /// </summary>
        public string Filename
        {
            get { return siteMapFile.FullPath; }
        }

        /// <summary>
        /// This read-only property returns the current topic collection including any unsaved edits
        /// </summary>
        public TocEntryCollection Topics
        {
            get
            {
                ucSiteMapEditor.CommitChanges();
                return ucSiteMapEditor.Topics;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileItem">The project file item to edit</param>
        public SiteMapEditorWindow(FileItem fileItem)
        {
            InitializeComponent();

            this.Text = Path.GetFileName(fileItem.FullPath);
            this.ToolTipText = fileItem.FullPath;

            siteMapFile = fileItem;

            ucSiteMapEditor = new SiteMapEditorControl();
            ehSiteMapEditorHost.Child = ucSiteMapEditor;

            // Hook up the command bindings and event handlers
            ucSiteMapEditor.CommandBindings.Add(new CommandBinding(EditorCommands.Edit,
                cmdEdit_Executed, cmdEdit_CanExecute));
            ucSiteMapEditor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddFromTemplate,
                cmdAddFromTemplate_Executed));
            ucSiteMapEditor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddExistingFile,
                cmdAddExistingFile_Executed));
            ucSiteMapEditor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddAllFromFolder,
                cmdAddAllFromFolder_Executed));

            ucSiteMapEditor.ContentModified += ucSiteMapEditor_ContentModified;
            ucSiteMapEditor.AssociateTopic += ucSiteMapEditor_AssociateTopic;

            // Load the site map file
            ucSiteMapEditor.LoadSiteMapFile(siteMapFile);
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
            string projectPath = Path.GetDirectoryName(siteMapFile.ProjectElement.Project.Filename);

            if(!filename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                WinFormsMessageBox.Show("The file must reside in the project folder or a folder below it",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            siteMapFile.Include = new FilePath(filename, siteMapFile.ProjectElement.Project);

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
        private TocEntry AddTopicFile(string filename, bool addAsChild)
        {
            TocEntry newTopic, currentTopic = ucSiteMapEditor.CurrentTopic;
            string newPath = filename, projectPath = Path.GetDirectoryName(
                siteMapFile.ProjectElement.Project.Filename);

            // The file must reside under the project path
            if(!Path.GetDirectoryName(filename).StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                newPath = Path.Combine(projectPath, Path.GetFileName(filename));

            // Add the file to the project if not already there
            siteMapFile.ProjectElement.Project.AddFileToProject(filename, newPath);

            // Add the topic to the editor's collection
            newTopic = new TocEntry(siteMapFile.ProjectElement.Project)
            {
                SourceFile = new FilePath(newPath, siteMapFile.ProjectElement.Project),
                Title = Path.GetFileNameWithoutExtension(newPath)
            };

            if(addAsChild && currentTopic != null)
            {
                currentTopic.Children.Add(newTopic);
                currentTopic.IsExpanded = true;
            }
            else
                if(currentTopic == null)
                    ucSiteMapEditor.Topics.Add(newTopic);
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
                ucSiteMapEditor.CommitChanges();

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
                ucSiteMapEditor.CommitChanges();

                if(this.IsDirty)
                {
                    ucSiteMapEditor.Topics.Save();

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
                dlg.Title = "Save Site Map File As";
                dlg.Filter = "Site map files (*.sitemap)|*.sitemap|All Files (*.*)|*.*";
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
        private void SiteMapEditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !this.CanClose;
        }

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucSiteMapEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
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
        private void ucSiteMapEditor_AssociateTopic(object sender, RoutedEventArgs e)
        {
            TocEntry t = ucSiteMapEditor.CurrentTopic;
            string newPath, projectPath = Path.GetDirectoryName(siteMapFile.ProjectElement.Project.Filename);

            if(t != null)
                using(OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select the additional content topic file";
                    dlg.Filter = "Additional Content Topics (*.htm, *.html, *.topic)|*.htm;*.html;*.topic|" +
                        "All files (*.*)|*.*";
                    dlg.DefaultExt = "html";
                    dlg.InitialDirectory = projectPath;
                    dlg.CheckFileExists = true;

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        // The file must reside under the project path
                        newPath = dlg.FileName;

                        if(!Path.GetDirectoryName(newPath).StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                            newPath = Path.Combine(projectPath, Path.GetFileName(newPath));

                        // Add the file to the project if not already there
                        siteMapFile.ProjectElement.Project.AddFileToProject(dlg.FileName, newPath);

                        t.SourceFile = new FilePath(newPath, siteMapFile.ProjectElement.Project);

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
            TocEntry t = ucSiteMapEditor.CurrentTopic;

            e.CanExecute = (t != null && t.SourceFile.Path.Length != 0);
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
            TocEntry t = sender as TocEntry;

            if(t == null)
                t = ucSiteMapEditor.CurrentTopic;

            if(t.SourceFile.Path.Length != 0)
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

        /// <summary>
        /// Add a new topic from a template file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddFromTemplate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string newFilePath = Path.GetDirectoryName(siteMapFile.FullPath);

            if(ucSiteMapEditor.CurrentTopic != null)
                newFilePath = Path.GetDirectoryName(ucSiteMapEditor.CurrentTopic.SourceFile);

            using(SelectFileTemplateDlg dlg = new SelectFileTemplateDlg(false, newFilePath))
            {
                // If created, add it to the project, refresh the Project Explorer, and open the file for editing
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    TocEntry t = this.AddTopicFile(dlg.NewFilename, e.Parameter != null);
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
            TocEntry t = ucSiteMapEditor.CurrentTopic;
            string projectPath = Path.GetDirectoryName(siteMapFile.ProjectElement.Project.Filename);

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the conceptual topic file(s)";
                dlg.Filter = "HTML Files (*.htm, *.html)|*.htm;*.html|All files (*.*)|*.*";
                dlg.DefaultExt = "html";
                dlg.InitialDirectory = (t != null && !String.IsNullOrEmpty(t.SourceFile)) ?
                    Path.GetDirectoryName(t.SourceFile) : projectPath;
                dlg.Multiselect = true;

                // If selected, add the new file(s).  Filenames that are already in the collection are ignored.
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
            TocEntryCollection parent, newTopics = new TocEntryCollection(null);
            TocEntry selectedTopic = ucSiteMapEditor.CurrentTopic;
            string projectPath = Path.GetDirectoryName(siteMapFile.ProjectElement.Project.Filename);
            int idx;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select a folder to add all of its content";
                dlg.SelectedPath = (selectedTopic != null && !String.IsNullOrEmpty(selectedTopic.SourceFile)) ?
                    Path.GetDirectoryName(selectedTopic.SourceFile) : projectPath;

                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        MouseCursor.Current = MouseCursors.WaitCursor;

                        newTopics.AddTopicsFromFolder(dlg.SelectedPath, dlg.SelectedPath,
                            siteMapFile.ProjectElement.Project);

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
                            parent = ucSiteMapEditor.Topics;
                            idx = 0;
                        }
                        else
                        {
                            parent = selectedTopic.Parent;
                            idx = parent.IndexOf(selectedTopic) + 1;
                        }

                        foreach(TocEntry t in newTopics)
                            parent.Insert(idx++, t);
                    }
                    else
                    {
                        // Insert as children
                        parent = selectedTopic.Children;

                        foreach(TocEntry t in newTopics)
                            parent.Add(t);

                        selectedTopic.IsExpanded = true;
                    }
            }
        }
        #endregion
    }
}
