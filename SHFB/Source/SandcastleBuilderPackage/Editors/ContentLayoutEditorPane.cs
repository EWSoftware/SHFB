//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ContentLayoutFileEditorPane.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/25/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to host the content layout file editor
// control.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Created the code
//=============================================================================

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;
using WinFormsFolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using WinFormsOpenFileDialog = System.Windows.Forms.OpenFileDialog;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF;
using SandcastleBuilder.WPF.Commands;
using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This is used to host the content layout file editor control
    /// </summary>
    public class ContentLayoutEditorPane : SimpleEditorPane<ContentLayoutEditorFactory, ContentLayoutEditorControl>
    {
        #region Private data members
        //=====================================================================

        private FileItem contentLayoutFile;
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
                base.UIControl.CommitChanges();
                return base.UIControl.Topics;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentLayoutEditorPane()
        {
            var editor = base.UIControl;

            // Hook up the command bindings and event handlers
            editor.CommandBindings.Add(new CommandBinding(EditorCommands.Edit, cmdEdit_Executed,
                cmdEdit_CanExecute));
            editor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddFromTemplate,
                cmdAddFromTemplate_Executed));
            editor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddExistingFile,
                cmdAddExistingFile_Executed));
            editor.CommandBindings.Add(new CommandBinding(ProjectCommands.AddAllFromFolder,
                cmdAddAllFromFolder_Executed));

            editor.ContentModified += ucContentLayoutEditor_ContentModified;
            editor.AssociateTopic += ucContentLayoutEditor_AssociateTopic;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add a new topic file to the editor
        /// </summary>
        /// <param name="filename">The filename of the topic to add</param>
        /// <param name="addAsChild">True to add as a child of the selected
        /// topic or false to add it as a sibling.</param>
        /// <returns>The topic that was just added or null if unsuccessful</returns>
        private Topic AddTopicFile(string filename, bool addAsChild)
        {
            Topic newTopic = null, currentTopic = base.UIControl.CurrentTopic;
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
                    base.UIControl.Topics.Add(newTopic);
                else
                    currentTopic.Parent.Insert(currentTopic.Parent.IndexOf(currentTopic) + 1, newTopic);

            newTopic.IsSelected = true;

            return newTopic;
        }
        #endregion

        #region Abstract method implementations
        //=====================================================================

        /// <inheritdoc />
        protected override string GetFileExtension()
        {
            return ".content";
        }

        /// <inheritdoc />
        protected override Guid GetCommandSetGuid()
        {
            return Guid.Empty;
        }

        /// <inheritdoc />
        protected override void LoadFile(string fileName)
        {
            var project = SandcastleBuilderPackage.CurrentSandcastleProject;

            if(project != null)
                contentLayoutFile = project.FindFile(fileName);

            if(contentLayoutFile == null)
                throw new InvalidOperationException("Unable to locate file in project: " + fileName);

            base.UIControl.LoadContentLayoutFile(contentLayoutFile);
        }

        /// <inheritdoc />
        protected override void SaveFile(string fileName)
        {
            Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

            base.UIControl.CommitChanges();

            if(base.IsDirty || !fileName.Equals(contentLayoutFile.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                contentLayoutFile.Include = new FilePath(fileName, contentLayoutFile.ProjectElement.Project);
                base.UIControl.Topics.Save();
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucContentLayoutEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
        {
            base.OnContentChanged();
        }

        /// <summary>
        /// Associate a project file with the current topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucContentLayoutEditor_AssociateTopic(object sender, RoutedEventArgs e)
        {
            FileNode thisNode = this.FileNode;
            FileItem newItem;
            Topic t = base.UIControl.CurrentTopic;
            string newPath, projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);

            if(t != null)
                using(WinFormsOpenFileDialog dlg = new WinFormsOpenFileDialog())
                {
                    dlg.Title = "Select the conceptual topic file";
                    dlg.Filter = "Conceptual Topics (*.aml)|*.aml|All files (*.*)|*.*";
                    dlg.DefaultExt = "aml";
                    dlg.InitialDirectory = projectPath;
                    dlg.CheckFileExists = true;

                    if(dlg.ShowDialog() == WinFormsDialogResult.OK)
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

                        if(thisNode != null)
                            thisNode.ProjectMgr.RefreshProject();
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
            Topic t = base.UIControl.CurrentTopic;

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
                t = base.UIControl.CurrentTopic;

            if(t.TopicFile != null)
            {
                string fullName = t.TopicFile.FullPath;

                if(File.Exists(fullName))
                    VsShellUtilities.OpenDocument(this, fullName);
                else
                    Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "File does not exist: " + fullName);
            }
            else
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "No file is associated with this topic");
        }

        /// <summary>
        /// Add a new topic from a template file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddFromTemplate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileNode thisNode = this.FileNode, topicNode = null;
            IVsAddProjectItemDlg addItemDialog;
            string strFilter = String.Empty, strBrowseLocations;
            int iDontShowAgain;
            uint uiFlags;
            IVsProject3 project;
            Guid projectGuid;

            if(thisNode != null)
            {
                // If we have a topic, use it's parent as the parent for the new topic.  If not, we'll use the
                // parent of the content layout file.
                if(base.UIControl.CurrentTopic != null && base.UIControl.CurrentTopic.TopicFile != null)
                    topicNode = thisNode.ProjectMgr.FindChild(base.UIControl.CurrentTopic.TopicFile.FullPath) as FileNode;

                project = (IVsProject3)thisNode.ProjectMgr;
                strBrowseLocations = Path.GetDirectoryName(thisNode.ProjectMgr.BaseURI.Uri.LocalPath);
                projectGuid = thisNode.ProjectMgr.ProjectGuid;
                addItemDialog = this.GetService(typeof(IVsAddProjectItemDlg)) as IVsAddProjectItemDlg;

                uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddNewItems |
                    __VSADDITEMFLAGS.VSADDITEM_SuggestTemplateName |
                    __VSADDITEMFLAGS.VSADDITEM_AllowHiddenTreeView);

                int hr = addItemDialog.AddProjectItemDlg((topicNode ?? thisNode).ID, ref projectGuid, project,
                    uiFlags, "Conceptual Content|Topics", "Conceptual", ref strBrowseLocations, ref strFilter,
                    out iDontShowAgain);

                // If successful, get the current project item (the one just added) and add it as a topic
                // to the collection.
                if(hr == VSConstants.S_OK)
                {
                    var node = thisNode.ProjectMgr.GetSelectedNodes().FirstOrDefault();

                    if(node != null && node.Url.EndsWith(".aml", StringComparison.OrdinalIgnoreCase))
                        this.AddTopicFile(node.Url, e.Parameter != null);
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
            FileNode thisNode = this.FileNode;
            Topic t = base.UIControl.CurrentTopic;
            string projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);

            using(WinFormsOpenFileDialog dlg = new WinFormsOpenFileDialog())
            {
                dlg.Title = "Select the conceptual topic file(s)";
                dlg.Filter = "Conceptual Topics (*.aml)|*.aml|All files (*.*)|*.*";
                dlg.DefaultExt = "aml";
                dlg.InitialDirectory = (t != null && t.TopicFile != null) ?
                    Path.GetDirectoryName(t.TopicFile.FullPath) : projectPath;
                dlg.Multiselect = true;

                // If selected, add the new file(s).  Filenames that are
                // already in the collection are ignored.
                if(dlg.ShowDialog() == WinFormsDialogResult.OK)
                {
                    foreach(string filename in dlg.FileNames)
                    {
                        this.AddTopicFile(filename, e.Parameter != null);

                        if(t != null)
                            t.IsSelected = true;
                    }

                    if(thisNode != null)
                        thisNode.ProjectMgr.RefreshProject();
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
            FileNode thisNode = this.FileNode;
            TopicCollection parent, newTopics = new TopicCollection(null);
            Topic selectedTopic = base.UIControl.CurrentTopic;
            string projectPath = Path.GetDirectoryName(contentLayoutFile.ProjectElement.Project.Filename);
            int idx;

            using(WinFormsFolderBrowserDialog dlg = new WinFormsFolderBrowserDialog())
            {
                dlg.Description = "Select a folder to add all of its content";
                dlg.SelectedPath = (selectedTopic != null && selectedTopic.TopicFile != null) ?
                    Path.GetDirectoryName(selectedTopic.TopicFile.FullPath) : projectPath;

                if(dlg.ShowDialog() == WinFormsDialogResult.OK)
                {
                    Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

                    newTopics.AddTopicsFromFolder(dlg.SelectedPath, dlg.SelectedPath,
                        contentLayoutFile.ProjectElement.Project);

                    if(thisNode != null)
                        thisNode.ProjectMgr.RefreshProject();
                }

                if(newTopics.Count != 0)
                    if(e.Parameter == null || selectedTopic == null)
                    {
                        // Insert as siblings
                        if(selectedTopic == null)
                        {
                            parent = base.UIControl.Topics;
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
