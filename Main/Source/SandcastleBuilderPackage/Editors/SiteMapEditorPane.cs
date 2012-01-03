//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SiteMapFileEditorPane.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/03/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to host the site map file editor control
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/27/2011  EFW  Created the code
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
    /// This is used to host the site map file editor control
    /// </summary>
    public class SiteMapEditorPane : SimpleEditorPane<SiteMapEditorFactory, SiteMapEditorControl>
    {
        #region Private data members
        //=====================================================================

        private FileItem siteMapFile;
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
        public SiteMapEditorPane()
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

            editor.ContentModified += ucSiteMapEditor_ContentModified;
            editor.AssociateTopic += ucSiteMapEditor_AssociateTopic;
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
        /// <returns>The topic that was just added</returns>
        private TocEntry AddTopicFile(string filename, bool addAsChild)
        {
            TocEntry newTopic, currentTopic = base.UIControl.CurrentTopic;
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
            return ".sitemap";
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
                siteMapFile = project.FindFile(fileName);

            if(siteMapFile == null)
                throw new InvalidOperationException("Unable to locate file in project: " + fileName);

            base.UIControl.LoadSiteMapFile(siteMapFile);
        }

        /// <inheritdoc />
        protected override void SaveFile(string fileName)
        {
            Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

            base.UIControl.CommitChanges();

            if(base.IsDirty || !fileName.Equals(siteMapFile.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                siteMapFile.Include = new FilePath(fileName, siteMapFile.ProjectElement.Project);
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
        private void ucSiteMapEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
        {
            base.OnContentChanged();
        }

        /// <summary>
        /// Associate a project file with the current topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucSiteMapEditor_AssociateTopic(object sender, RoutedEventArgs e)
        {
            FileNode thisNode = this.FileNode;
            TocEntry t = base.UIControl.CurrentTopic;
            string newPath, projectPath = Path.GetDirectoryName(
                siteMapFile.ProjectElement.Project.Filename);

            if(t != null)
                using(WinFormsOpenFileDialog dlg = new WinFormsOpenFileDialog())
                {
                    dlg.Title = "Select the additional content topic file";
                    dlg.Filter = "Additional Content Topics (*.htm, *.html)|*.htm;*.html|" +
                        "All files (*.*)|*.*";
                    dlg.DefaultExt = "html";
                    dlg.InitialDirectory = projectPath;
                    dlg.CheckFileExists = true;

                    if(dlg.ShowDialog() == WinFormsDialogResult.OK)
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
            TocEntry t = base.UIControl.CurrentTopic;

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
                t = base.UIControl.CurrentTopic;

            if(t.SourceFile.Path.Length != 0)
            {
                string fullName = t.SourceFile;

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
                if(base.UIControl.CurrentTopic != null)
                    topicNode = thisNode.ProjectMgr.FindChild(base.UIControl.CurrentTopic.SourceFile) as FileNode;

                project = (IVsProject3)thisNode.ProjectMgr;
                strBrowseLocations = Path.GetDirectoryName(thisNode.ProjectMgr.BaseURI.Uri.LocalPath);
                projectGuid = thisNode.ProjectMgr.ProjectGuid;
                addItemDialog = this.GetService(typeof(IVsAddProjectItemDlg)) as IVsAddProjectItemDlg;

                uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddNewItems |
                    __VSADDITEMFLAGS.VSADDITEM_SuggestTemplateName |
                    __VSADDITEMFLAGS.VSADDITEM_AllowHiddenTreeView);

                int hr = addItemDialog.AddProjectItemDlg((topicNode ?? thisNode).ID, ref projectGuid, project,
                    uiFlags, "Other Content", "Html Page", ref strBrowseLocations, ref strFilter,
                    out iDontShowAgain);

                // If successful, get the current project item (the one just added) and add it as a topic
                // to the collection.
                if(hr == VSConstants.S_OK)
                {
                    var node = thisNode.ProjectMgr.GetSelectedNodes().FirstOrDefault();

                    if(node != null && node.Url.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
                      node.Url.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
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
            TocEntry t = base.UIControl.CurrentTopic;
            string projectPath = Path.GetDirectoryName(siteMapFile.ProjectElement.Project.Filename);

            using(WinFormsOpenFileDialog dlg = new WinFormsOpenFileDialog())
            {
                dlg.Title = "Select the conceptual topic file(s)";
                dlg.Filter = "HTML Files (*.htm, *.html)|*.htm;*.html|All files (*.*)|*.*";
                dlg.DefaultExt = "html";
                dlg.InitialDirectory = (t != null && !String.IsNullOrEmpty(t.SourceFile)) ?
                    Path.GetDirectoryName(t.SourceFile) : projectPath;
                dlg.Multiselect = true;

                // If selected, add the new file(s).  Filenames that are already in the collection are ignored.
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
            TocEntryCollection parent, newTopics = new TocEntryCollection(null);
            TocEntry selectedTopic = base.UIControl.CurrentTopic;
            string projectPath = Path.GetDirectoryName(siteMapFile.ProjectElement.Project.Filename);
            int idx;

            using(WinFormsFolderBrowserDialog dlg = new WinFormsFolderBrowserDialog())
            {
                dlg.Description = "Select a folder to add all of its content";
                dlg.SelectedPath = (selectedTopic != null && !String.IsNullOrEmpty(selectedTopic.SourceFile)) ?
                    Path.GetDirectoryName(selectedTopic.SourceFile) : projectPath;

                if(dlg.ShowDialog() == WinFormsDialogResult.OK)
                {
                    Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

                    newTopics.AddTopicsFromFolder(dlg.SelectedPath, dlg.SelectedPath,
                        siteMapFile.ProjectElement.Project);

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
