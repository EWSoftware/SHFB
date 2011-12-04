//=============================================================================
// System  : Sandcastle Help File Builder
// File    : SiteMapEditorWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/19/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
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
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;

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

        private TocEntryCollection topics;
        private TreeNode defaultNode, splitTocNode, firstNode;
        private TocEntry firstSelection;
        private static object cutClipboard, copyClipboard;

        //=====================================================================
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileItem">The project file item to edit</param>
        public SiteMapEditorWindow(FileItem fileItem)
        {
            EventHandler onClick = new EventHandler(templateFile_OnClick);
            ToolStripMenuItem miTemplate;
            string name;

            InitializeComponent();

            sbStatusBarText.InstanceStatusBar = MainForm.Host.StatusBarTextLabel;

            // Look for custom templates in the local application data folder
            name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Constants.ItemTemplates);

            if(Directory.Exists(name))
                foreach(string file in Directory.EnumerateFiles(name, "*.htm?"))
                {
                    name = Path.GetFileNameWithoutExtension(file);

                    miTemplate = new ToolStripMenuItem(name, null, onClick);
                    miTemplate.Tag = file;
                    sbStatusBarText.SetStatusBarText(miTemplate, "Add new '" + name + "' topic");
                    miCustomSibling.DropDownItems.Add(miTemplate);

                    miTemplate = new ToolStripMenuItem(name, null, onClick);
                    miTemplate.Tag = file;
                    sbStatusBarText.SetStatusBarText(miTemplate, "Add new '" + name + "' topic");
                    miCustomChild.DropDownItems.Add(miTemplate);
                }

            miCustomSibling.Enabled = miCustomChild.Enabled = (miCustomSibling.DropDownItems.Count != 0);

            topics = new TocEntryCollection(fileItem);
            topics.Load();
            topics.ListChanged += new ListChangedEventHandler(topics_ListChanged);

            this.Text = Path.GetFileName(fileItem.FullPath);
            this.ToolTipText = fileItem.FullPath;
            this.LoadTopics(null);
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
            string projectPath = Path.GetDirectoryName(
                topics.FileItem.ProjectElement.Project.Filename);

            if(!filename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The file must reside in the project folder " +
                    "or a folder below it", Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            topics.FileItem.Include = new FilePath(filename,
                topics.FileItem.ProjectElement.Project);
            this.Text = Path.GetFileName(filename);
            this.ToolTipText = filename;
            this.IsDirty = true;
            return this.Save();
        }

        /// <summary>
        /// Load the tree view with the topics and set the form up to edit them
        /// </summary>
        /// <param name="selectedEntry">If not null, the node containing the
        /// specified entry is set as the selected node.  If null, the first
        /// node is selected.</param>
        private void LoadTopics(TocEntry selectedEntry)
        {
            TreeNode node;

            try
            {
                tvContent.SuspendLayout();
                tvContent.Nodes.Clear();

                defaultNode = splitTocNode = firstNode = null;
                firstSelection = selectedEntry;

                if(topics.Count != 0)
                {
                    foreach(TocEntry t in topics)
                    {
                        node = tvContent.Nodes.Add(t.Title);
                        node.Name = t.SourceFile;
                        node.Tag = t;

                        if(t.IsDefaultTopic)
                            defaultNode = node;

                        // This is only valid at the root level
                        if(t.ApiParentMode != ApiParentMode.None)
                            splitTocNode = node;

                        if(t == firstSelection)
                            firstNode = node;

                        if(t.Children.Count != 0)
                            this.AddChildren(t.Children, node);
                    }

                    if(defaultNode != null)
                    {
                        defaultNode.ToolTipText = "Default topic";
                        defaultNode.ImageIndex = defaultNode.SelectedImageIndex = 1;
                    }

                    if(splitTocNode != null)
                    {
                        splitTocNode.ToolTipText = "Split Table of Contents";
                        splitTocNode.ImageIndex = splitTocNode.SelectedImageIndex = 10;
                    }

                    if(defaultNode != null && defaultNode == splitTocNode)
                    {
                        defaultNode.ToolTipText = "Default topic/Split TOC";
                        defaultNode.ImageIndex = defaultNode.SelectedImageIndex = 11;
                    }

                    tvContent.ExpandAll();

                    if(firstNode != null)
                    {
                        tvContent.SelectedNode = firstNode;
                        firstNode.EnsureVisible();
                    }
                    else
                    {
                        tvContent.SelectedNode = tvContent.Nodes[0];
                        tvContent.SelectedNode.EnsureVisible();
                    }
                }
                else
                    this.UpdateControlStatus();
            }
            finally
            {
                tvContent.ResumeLayout();
            }
        }

        /// <summary>
        /// Add child nodes to the tree view recursively
        /// </summary>
        /// <param name="children">The collection of entries to add</param>
        /// <param name="root">The root to which they are added</param>
        private void AddChildren(TocEntryCollection children, TreeNode root)
        {
            TreeNode node;

            foreach(TocEntry t in children)
            {
                node = root.Nodes.Add(t.Title);
                node.Name = t.SourceFile;
                node.Tag = t;

                if(t.IsDefaultTopic)
                    defaultNode = node;

                if(t == firstSelection)
                    firstNode = node;

                if(t.Children.Count != 0)
                    this.AddChildren(t.Children, node);
            }
        }

        /// <summary>
        /// This is used to update the state of the form controls
        /// </summary>
        private void UpdateControlStatus()
        {
            TreeNode current = tvContent.SelectedNode;

            tsbPaste.Enabled = tsmiPasteAsSibling.Enabled = tsmiPasteAsChild.Enabled = miPaste.Enabled =
                miPasteAsChild.Enabled = (cutClipboard != null);

            if(tvContent.Nodes.Count == 0)
            {
                miDefaultTopic.Enabled = miSplitToc.Enabled = miMoveUp.Enabled = miMoveDown.Enabled =
                    miAddChild.Enabled = miDelete.Enabled = miCut.Enabled = miCopyAsLink.Enabled =
                    miSortTopics.Enabled = tsbDefaultTopic.Enabled = tsbSplitTOC.Enabled =
                    tsbAddChildTopic.Enabled = tsbDeleteTopic.Enabled = tsbMoveUp.Enabled =
                    tsbMoveDown.Enabled = tsbCut.Enabled = tsbEditTopic.Enabled = miEditTopic.Enabled =
                    pgProps.Enabled = false;

                pgProps.SelectedObject = null;
            }
            else
            {
                miDefaultTopic.Enabled = miAddChild.Enabled = miDelete.Enabled = miCut.Enabled = miCopyAsLink.Enabled =
                    miSortTopics.Enabled = tsbDefaultTopic.Enabled = tsbAddChildTopic.Enabled =
                    tsbDeleteTopic.Enabled = tsbCut.Enabled = pgProps.Enabled = true;

                tsbEditTopic.Enabled = miEditTopic.Enabled = (((TocEntry)current.Tag).SourceFile.Path.Length != 0);
                tsbSplitTOC.Enabled = miSplitToc.Enabled = (current.Parent == null);
                tsbMoveDown.Enabled = miMoveDown.Enabled = (current.NextNode != null);
                tsbMoveUp.Enabled = miMoveUp.Enabled = (current.PrevNode != null);

                if(pgProps.SelectedObject != current.Tag)
                    pgProps.SelectedObject = current.Tag;
            }
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

                if(this.IsDirty)
                {
                    topics.Save();
                    this.Text = Path.GetFileName(this.ToolTipText);
                    this.IsDirty = false;
                }

                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to save file.  Reason: " + ex.Message,
                    "Site Map Editor", MessageBoxButtons.OK,
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
                dlg.Title = "Save Site Map File As";
                dlg.Filter = "Site map files (*.sitemap)|*.sitemap|" +
                    "All Files (*.*)|*.*";
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
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void topics_ListChanged(object sender, ListChangedEventArgs e)
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
        private void SiteMapEditorWindow_FormClosing(object sender,
          FormClosingEventArgs e)
        {
            cutClipboard = copyClipboard = null;
            e.Cancel = !this.CanClose;
        }

        /// <summary>
        /// Update the node text when a property changes if it affects the
        /// node text.
        /// </summary>
        /// <param name="s">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pgProps_PropertyValueChanged(object s,
          PropertyValueChangedEventArgs e)
        {
            TreeNode selectedNode = tvContent.SelectedNode;
            string label = e.ChangedItem.Label;
            TocEntry t;

            if(label == "Title" || label == "SourceFile")
            {
                t = (TocEntry)tvContent.SelectedNode.Tag;
                selectedNode.Name = t.SourceFile;
                selectedNode.Text = t.Title;
                UpdateControlStatus();
            }

            this.topics_ListChanged(this, new ListChangedEventArgs(
                ListChangedType.ItemChanged, -1));
        }
        #endregion

        #region Topic toolstrip event handlers
        //=====================================================================

        /// <summary>
        /// Set the selected node as the default topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The sender of the event</param>
        private void tsbDefaultTopic_Click(object sender, EventArgs e)
        {
            TreeNode newDefault = tvContent.SelectedNode;

            if(defaultNode != null && defaultNode == splitTocNode)
            {
                defaultNode.ToolTipText = "Split Table of Contents";
                defaultNode.ImageIndex = defaultNode.SelectedImageIndex = 10;
            }
            else
                if(defaultNode != null)
                {
                    defaultNode.ToolTipText = null;
                    defaultNode.ImageIndex = defaultNode.SelectedImageIndex = 0;
                }

            if(defaultNode != newDefault)
            {
                if(newDefault == splitTocNode)
                {
                    newDefault.ToolTipText = "Default topic/Split TOC";
                    newDefault.ImageIndex = newDefault.SelectedImageIndex = 11;
                }
                else
                {
                    newDefault.ToolTipText = "Default topic";
                    newDefault.ImageIndex = newDefault.SelectedImageIndex = 1;
                }

                defaultNode = newDefault;

                if(topics.DefaultTopic != null)
                    topics.DefaultTopic.IsDefaultTopic = false;

                ((TocEntry)defaultNode.Tag).IsDefaultTopic = true;
            }
            else
                if(defaultNode != null)
                {
                    defaultNode = null;     // Turn it off altogether

                    if(topics.DefaultTopic != null)
                        topics.DefaultTopic.IsDefaultTopic = false;
                }

            this.topics_ListChanged(this, new ListChangedEventArgs(
                ListChangedType.ItemChanged, -1));
        }

        /// <summary>
        /// Set the selected node as the split TOC location
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The sender of the event</param>
        private void tsbSplitTOC_Click(object sender, EventArgs e)
        {
            TreeNode newSplit = tvContent.SelectedNode;

            if(splitTocNode != null && defaultNode == splitTocNode)
            {
                splitTocNode.ToolTipText = "Default topic";
                splitTocNode.ImageIndex = splitTocNode.SelectedImageIndex = 1;
            }
            else
                if(splitTocNode != null)
                {
                    splitTocNode.ToolTipText = null;
                    splitTocNode.ImageIndex = splitTocNode.SelectedImageIndex = 0;
                }

            if(splitTocNode != newSplit)
            {
                if(newSplit == defaultNode)
                {
                    newSplit.ToolTipText = "Default topic/Split TOC";
                    newSplit.ImageIndex = newSplit.SelectedImageIndex = 11;
                }
                else
                {
                    newSplit.ToolTipText = "Split Table of Contents";
                    newSplit.ImageIndex = newSplit.SelectedImageIndex = 10;
                }

                splitTocNode = newSplit;

                if(topics.ApiContentInsertionPoint != null)
                    topics.ApiContentInsertionPoint.ApiParentMode = ApiParentMode.None;

                ((TocEntry)splitTocNode.Tag).ApiParentMode = ApiParentMode.InsertAfter;
            }
            else
                if(splitTocNode != null)
                {
                    splitTocNode = null;    // Turn it off altogether

                    if(topics.ApiContentInsertionPoint != null)
                        topics.ApiContentInsertionPoint.ApiParentMode = ApiParentMode.None;
                }

            this.topics_ListChanged(this, new ListChangedEventArgs(ListChangedType.ItemChanged, -1));
        }

        /// <summary>
        /// Move the selected node up or down within the group
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The sender of the event</param>
        private void tsbMoveItem_Click(object sender, EventArgs e)
        {
            TreeNode moveNode, insertNode;
            TreeNodeCollection tnc;
            TocEntryCollection parent;
            TocEntry topic;
            int index;

            moveNode = tvContent.SelectedNode;
            topic = (TocEntry)moveNode.Tag;
            parent = topic.Parent;
            index = parent.IndexOf(topic);

            if(moveNode.Parent == null)
                tnc = tvContent.Nodes;
            else
                tnc = moveNode.Parent.Nodes;

            if(sender == tsbMoveUp || sender == miMoveUp)
            {
                insertNode = moveNode.PrevNode;
                tnc.Remove(moveNode);
                tnc.Insert(tnc.IndexOf(insertNode), moveNode);

                parent.RemoveAt(index);
                parent.Insert(index - 1, topic);
            }
            else
            {
                insertNode = moveNode.NextNode;
                tnc.Remove(moveNode);
                tnc.Insert(tnc.IndexOf(insertNode) + 1, moveNode);

                parent.RemoveAt(index);
                parent.Insert(index + 1, topic);
            }

            tvContent.SelectedNode = moveNode;
        }

        /// <summary>
        /// Add an existing topic file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void AddExistingTopicFile_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem miSelection = (ToolStripMenuItem)sender;
            TreeNode selectedNode = tvContent.SelectedNode;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the conceptual topic file(s)";
                dlg.Filter = "HTML Files (*.htm, *.html)|*.htm;*.html|" +
                    "All files (*.*)|*.*";
                dlg.DefaultExt = "html";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.Multiselect = true;

                // If selected, add the new file(s).  Filenames that are
                // already in the collection are ignored.
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach(string filename in dlg.FileNames)
                    {
                        this.AddTopicFile(filename, miSelection.Owner == cmsNewChildTopic);

                        if(selectedNode != null)
                            tvContent.SelectedNode = selectedNode;
                    }

                    MainForm.Host.ProjectExplorer.RefreshProject();
                }
            }
        }

        /// <summary>
        /// Add a new item based on the selected template file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void templateFile_OnClick(object sender, EventArgs e)
        {
            ToolStripItem miSelection = (ToolStripItem)sender;
            string file = (string)miSelection.Tag;
            Guid guid = Guid.NewGuid();

            using(SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save New Topic As";
                dlg.Filter = "HTML Files (*.htm, *.html)|*.htm;*.html|" +
                    "All files (*.*)|*.*";
                dlg.FileName = guid.ToString() + ".html";
                dlg.DefaultExt = "html";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();

                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(file, dlg.FileName);
                    File.SetAttributes(dlg.FileName, FileAttributes.Normal);

                    this.AddTopicFile(dlg.FileName,
                        sender == tsbAddChildTopic ||
                        miSelection.OwnerItem == miCustomChild);

                    MainForm.Host.ProjectExplorer.RefreshProject();
                    tsbEditTopic_Click(sender, e);
                }
            }
        }

        /// <summary>
        /// Add a new topic file
        /// </summary>
        /// <param name="filename">The filename of the topic to add</param>
        /// <param name="addAsChild">True to add as a child of the selected
        /// node or false to add it as a sibling.</param>
        private void AddTopicFile(string filename, bool addAsChild)
        {
            TocEntry topic, parentTopic;
            TreeNode tnNew, tnParent;
            string newPath = filename, projectPath = Path.GetDirectoryName(
                topics.FileItem.ProjectElement.Project.Filename);

            // The file must reside under the project path
            if(!Path.GetDirectoryName(filename).StartsWith(projectPath,
              StringComparison.OrdinalIgnoreCase))
                newPath = Path.Combine(projectPath, Path.GetFileName(filename));

            // Add the file to the project
            topics.FileItem.ProjectElement.Project.AddFileToProject(filename,
                newPath);
            topic = new TocEntry(topics.FileItem.ProjectElement.Project);
            topic.SourceFile = new FilePath(newPath, topic.BasePathProvider);
            topic.Title = Path.GetFileNameWithoutExtension(newPath);

            tnNew = new TreeNode(topic.Title);
            tnNew.Name = topic.SourceFile;
            tnNew.Tag = topic;

            if(addAsChild)
            {
                tvContent.SelectedNode.Nodes.Add(tnNew);
                parentTopic = (TocEntry)tvContent.SelectedNode.Tag;
                parentTopic.Children.Add(topic);
            }
            else
                if(tvContent.SelectedNode == null)
                {
                    tvContent.Nodes.Add(tnNew);
                    topics.Add(topic);
                }
                else
                {
                    if(tvContent.SelectedNode.Parent == null)
                        tvContent.Nodes.Insert(tvContent.Nodes.IndexOf(
                            tvContent.SelectedNode) + 1, tnNew);
                    else
                    {
                        tnParent = tvContent.SelectedNode.Parent;
                        tnParent.Nodes.Insert(tnParent.Nodes.IndexOf(
                            tvContent.SelectedNode) + 1, tnNew);
                    }

                    parentTopic = (TocEntry)tvContent.SelectedNode.Tag;
                    parentTopic.Parent.Insert(
                        parentTopic.Parent.IndexOf(parentTopic) + 1,
                        topic);
                }

            tvContent.SelectedNode = tnNew;
        }

        /// <summary>
        /// Add an empty container node that is not associated with any topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbAddTopic_ButtonClick(object sender, EventArgs e)
        {
            ToolStripItem tiAdd = (ToolStripItem)sender;
            TocEntry topic, parentTopic;
            TreeNode tnNew, tnParent;

            topic = new TocEntry(topics.FileItem.ProjectElement.Project);
            topic.Title = "Table of Contents Container";
            tnNew = new TreeNode(topic.Title);
            tnNew.Name = topic.SourceFile;
            tnNew.Tag = topic;

            if(tiAdd == tsbAddChildTopic || tiAdd.Owner == cmsNewChildTopic)
            {
                tvContent.SelectedNode.Nodes.Add(tnNew);
                parentTopic = (TocEntry)tvContent.SelectedNode.Tag;
                parentTopic.Children.Add(topic);
            }
            else
                if(tvContent.SelectedNode == null)
                {
                    tvContent.Nodes.Add(tnNew);
                    topics.Add(topic);
                }
                else
                {
                    if(tvContent.SelectedNode.Parent == null)
                        tvContent.Nodes.Insert(tvContent.Nodes.IndexOf(
                            tvContent.SelectedNode) + 1, tnNew);
                    else
                    {
                        tnParent = tvContent.SelectedNode.Parent;
                        tnParent.Nodes.Insert(tnParent.Nodes.IndexOf(
                            tvContent.SelectedNode) + 1, tnNew);
                    }

                    parentTopic = (TocEntry)tvContent.SelectedNode.Tag;
                    parentTopic.Parent.Insert(
                        parentTopic.Parent.IndexOf(parentTopic) + 1,
                        topic);
                }

            tvContent.SelectedNode = tnNew;
        }

        /// <summary>
        /// Add all topic files found in the selected folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void AddAllTopicsInFolder_Click(object sender, EventArgs e)
        {
            ToolStripItem tiAdd = (ToolStripItem)sender;
            TocEntryCollection parent, newTopics = new TocEntryCollection(null);
            TocEntry selectedTopic;
            int idx;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select a folder to add all of its content";
                dlg.SelectedPath = Directory.GetCurrentDirectory();

                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        newTopics.AddTopicsFromFolder(dlg.SelectedPath,
                            dlg.SelectedPath, topics.FileItem.ProjectElement.Project);
                        MainForm.Host.ProjectExplorer.RefreshProject();
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }

                if(newTopics.Count != 0)
                {
                    if(tiAdd.Owner != cmsNewChildTopic)
                    {
                        // Insert as siblings
                        if(tvContent.SelectedNode == null)
                        {
                            parent = topics;
                            idx = 0;
                        }
                        else
                        {
                            selectedTopic = (TocEntry)tvContent.SelectedNode.Tag;
                            parent = selectedTopic.Parent;
                            idx = parent.IndexOf(selectedTopic) + 1;
                        }

                        foreach(TocEntry t in newTopics)
                        {
                            parent.Insert(idx, t);
                            idx++;
                        }
                    }
                    else    // Insert as children
                    {
                        parent = ((TocEntry)tvContent.SelectedNode.Tag).Children;

                        foreach(TocEntry t in newTopics)
                            parent.Add(t);
                    }

                    // Take the easy way out and reload the tree
                    this.LoadTopics(newTopics[0]);
                }
            }
        }

        /// <summary>
        /// Delete a content item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbDeleteTopic_Click(object sender, EventArgs e)
        {
            TreeNode tn = tvContent.SelectedNode;
            TocEntry topic = (TocEntry)tn.Tag;

            if(MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
              "Are you sure you want to delete the content item '{0}' and " +
              "all of its sub-items?", tn.Text), Constants.AppName,
              MessageBoxButtons.YesNo, MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                topic.Parent.Remove(topic);
                tn.Remove();
            }

            this.UpdateControlStatus();
        }

        /// <summary>
        /// Copy as a link to the Windows clipboard for pasting into a topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCopyAsLink_Click(object sender, EventArgs e)
        {
            DataObject msg;
            TreeNode selectedNode = tvContent.SelectedNode;

            if(selectedNode != null)
            {
                // The topic is too complex to serialize to the Windows
                // clipboard so we'll just serialize a static delegate that
                // returns this object instead.
                copyClipboard = selectedNode.Tag;

                msg = new DataObject();
                msg.SetData(typeof(ClipboardDataHandler),
                    new ClipboardDataHandler(GetClipboardData));
                Clipboard.SetDataObject(msg, false);
            }
        }

        /// <summary>
        /// This is used to return the actual object to paste from the Windows
        /// clipboard
        /// </summary>
        /// <returns>The object to paste</returns>
        private static object GetClipboardData()
        {
            return copyClipboard;
        }

        /// <summary>
        /// Cut or copy the selected node to the internal clipboard
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks><b>NOTE:</b> Copying within the file is currently not
        /// enabled.  Copying would require cloning the topic, its children,
        /// and all related properties.  Copying a topic may not be of any use
        /// anyway since the IDs have to be unique.</remarks>
        private void tsbCutCopy_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = tvContent.SelectedNode;

            if(selectedNode != null)
            {
                cutClipboard = selectedNode.Tag;

                if(sender == tsbCut || sender == miCut)
                {
                    TocEntry t = (TocEntry)cutClipboard;
                    t.Parent.Remove(t);
                    selectedNode.Remove();
                }

                this.UpdateControlStatus();
            }
        }

        /// <summary>
        /// Paste the node from the internal clipboard.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbPaste_ButtonClick(object sender, EventArgs e)
        {
            TreeNode target = tvContent.SelectedNode;
            TocEntry targetTopic, newTopic = null;

            if(cutClipboard != null)
                newTopic = cutClipboard as TocEntry;

            // Don't allow pasting multiple copies of the same item
            cutClipboard = null;

            if(newTopic != null)
            {
                if(target == null)
                    topics.Add(newTopic);
                else
                {
                    targetTopic = (TocEntry)target.Tag;

                    if(sender == miPasteAsChild || sender == tsmiPasteAsChild)
                        targetTopic.Children.Add(newTopic);
                    else
                        targetTopic.Parent.Insert(targetTopic.Parent.IndexOf(
                            targetTopic) + 1, newTopic);
                }

                // Trying to synch up the new nodes with new tree nodes would
                // be difficult so we'll take the easy way out and reload
                // the tree.
                this.LoadTopics(newTopic);
            }
        }

        /// <summary>
        /// Edit the selected topic file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbEditTopic_Click(object sender, EventArgs e)
        {
            TocEntry t = (TocEntry)tvContent.SelectedNode.Tag;

            if(t.SourceFile.Path.Length != 0)
            {
                string fullName = t.SourceFile;

                // If the document is already open, just activate it
                foreach(IDockContent content in this.DockPanel.Documents)
                    if(String.Compare(content.DockHandler.ToolTipText, fullName,
                      true, CultureInfo.CurrentCulture) == 0)
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
                    MessageBox.Show("File does not exist: " + fullName,
                        Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("No file is associated with this entry",
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }

        /// <summary>
        /// Sort the topics by display title
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miSortTopics_Click(object sender, EventArgs e)
        {
            TocEntry t = tvContent.SelectedNode.Tag as TocEntry;

            if(t != null)
            {
                t.Parent.Sort();
                this.LoadTopics(t);
            }
        }

        /// <summary>
        /// View help for this editor
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbHelp_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            try
            {
#if DEBUG
                path += @"\..\..\..\Doc\Help\SandcastleBuilder.chm";
#else
                path += @"\SandcastleBuilder.chm";
#endif

                Form form = new Form();
                form.CreateControl();
                Help.ShowHelp(form, path, HelpNavigator.Topic,
                    "html/3dd5fe3b-1bc3-42e5-8900-56165e3f9aed.htm");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to open help file '{0}'.  Reason: {1}",
                    path, ex.Message), Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Handle the expand/collapse context menu options
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ExpandCollapseTopics_Click(object sender, EventArgs e)
        {
            if(sender == miExpandAllTopics)
                tvContent.ExpandAll();
            else
                if(sender == miCollapseAllTopics)
                    tvContent.CollapseAll();
                else
                    if(sender == miExpandChildTopics && tvContent.SelectedNode != null)
                        tvContent.SelectedNode.ExpandAll();
                    else
                        if(tvContent.SelectedNode != null)
                            tvContent.SelectedNode.Collapse();

            if(tvContent.SelectedNode != null)
                tvContent.SelectedNode.EnsureVisible();
        }
        #endregion

        #region TreeView drag and drop
        /// <summary>
        /// This initiates drag and drop for the tree view nodes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DataObject data = new DataObject();
            TreeNode node = e.Item as TreeNode;

            if(node != null && e.Button == MouseButtons.Left)
            {
                // The tree supports dragging and dropping of nodes
                data.SetData(typeof(TreeNode), node);
                data.SetData(typeof(TocEntry), node.Tag);

                this.DoDragDrop(data, DragDropEffects.All);
            }
        }

        /// <summary>
        /// This validates the drop target during the drag operation
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_DragOver(object sender, DragEventArgs e)
        {
            TreeNode targetNode, dropNode;

            if(!e.Data.GetDataPresent(typeof(TreeNode)))
                return;

            // As the mouse moves over nodes, provide feedback to the user
            // by highlighting the node that is the current drop target.
            targetNode = tvContent.GetNodeAt(tvContent.PointToClient(
                new Point(e.X, e.Y)));

            // Select the node currently under the cursor
            if(targetNode != null && tvContent.SelectedNode != targetNode)
                tvContent.SelectedNode = targetNode;
            else
                if(targetNode == null)
                    targetNode = tvContent.SelectedNode;

            // Check that the selected node is not the dropNode and also
            // that it is not a child of the dropNode and therefore an
            // invalid target
            dropNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Don't allow drop from some other tree view (i.e. the
            // ones in the other content layout editors or project
            // explorer).
            if(dropNode != null && dropNode.TreeView != tvContent)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            while(targetNode != null)
            {
                if(targetNode == dropNode)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                targetNode = targetNode.Parent;
            }

            // Currently selected node is a suitable target
            e.Effect = DragDropEffects.Move;
        }

        /// <summary>
        /// This handles the drop operation for the tree view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode dropNode, targetNode;
            TocEntry movedEntry, targetEntry;
            int offset;

            if(!e.Data.GetDataPresent(typeof(TreeNode)))
                return;

            // Get the TreeNode being dragged
            dropNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // The target node should be selected from the DragOver event
            targetNode = tvContent.SelectedNode;

            if(targetNode == null || dropNode == targetNode)
                return;

            // Remove the entry from its current location
            movedEntry = (TocEntry)dropNode.Tag;
            targetEntry = (TocEntry)targetNode.Tag;

            // If the drop node is the next sibling of the target node,
            // insert it in the target's location (swap them).
            offset = (targetNode.NextNode == dropNode) ? 0 : 1;

            movedEntry.Parent.Remove(movedEntry);
            dropNode.Remove();

            // If Shift is not held down, make it a sibling of the drop node.
            // If Shift is help down, make it a child of the drop node.
            if((e.KeyState & 4) == 0)
            {
                targetEntry.Parent.Insert(targetEntry.Parent.IndexOf(
                    targetEntry) + offset, movedEntry);

                if(targetNode.Parent == null)
                    tvContent.Nodes.Insert(tvContent.Nodes.IndexOf(
                        targetNode) + offset, dropNode);
                else
                    targetNode.Parent.Nodes.Insert(targetNode.Parent.Nodes.IndexOf(
                        targetNode) + offset, dropNode);
            }
            else
            {
                targetEntry.Children.Add(movedEntry);
                targetNode.Nodes.Add(dropNode);
            }

            // Ensure that the moved node is visible to the user and select it
            dropNode.EnsureVisible();
            tvContent.SelectedNode = dropNode;
        }
        #endregion

        #region Other tree view event handlers
        //=====================================================================

        /// <summary>
        /// Update the state of the controls based on the current selection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateControlStatus();
        }

        /// <summary>
        /// This is used to select the clicked node and display the context
        /// menu when a right click occurs.  This ensures that the correct
        /// node is affected by the selected operation.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_MouseDown(object sender, MouseEventArgs e)
        {
            Point pt;
            TreeNode targetNode;

            if(e.Button == MouseButtons.Right)
            {
                pt = new Point(e.X, e.Y);

                if(pt == Point.Empty)
                    targetNode = tvContent.SelectedNode;
                else
                    targetNode = tvContent.GetNodeAt(pt);

                if(targetNode != null)
                    tvContent.SelectedNode = targetNode;

                cmsTopics.Show(tvContent, pt);
            }
        }

        /// <summary>
        /// Edit the node if it is double-clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_NodeMouseDoubleClick(object sender,
          TreeNodeMouseClickEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
                tsbEditTopic_Click(sender, e);
        }

        /// <summary>
        /// Handle shortcut keys in the tree view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Apps)
            {
                tvContent_MouseDown(sender, new MouseEventArgs(
                    MouseButtons.Right, 1, 0, 0, 0));
                e.Handled = e.SuppressKeyPress = true;
                return;
            }

            if(tvContent.SelectedNode != null)
                switch(e.KeyCode)
                {
                    case Keys.Delete:
                        if(!e.Shift)
                        {
                            tsbDeleteTopic_Click(sender, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        else
                        {
                            tsbCutCopy_Click(tsbCut, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.Insert:
                        if(e.Shift)
                        {
                            if(!e.Alt)
                                tsbPaste_ButtonClick(tsbPaste, e);
                            else
                                tsbPaste_ButtonClick(miPasteAsChild, e);

                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.U:
                        if(e.Control && tvContent.SelectedNode.PrevNode != null)
                        {
                            tsbMoveItem_Click(tsbMoveUp, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.D:
                        if(e.Control && tvContent.SelectedNode.NextNode != null)
                        {
                            tsbMoveItem_Click(tsbMoveDown, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.X:
                        if(e.Control)
                        {
                            tsbCutCopy_Click(tsbCut, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.C:
                        if(e.Control)
                        {
                            miCopyAsLink_Click(miCopyAsLink, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.V:
                        if(e.Control)
                        {
                            if(!e.Alt)
                                tsbPaste_ButtonClick(tsbPaste, e);
                            else
                                tsbPaste_ButtonClick(miPasteAsChild, e);

                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.E:
                        if(e.Control)
                        {
                            tsbEditTopic_Click(sender, e);
                            e.Handled = e.SuppressKeyPress = true;
                        }
                        break;

                    default:
                        break;
                }
        }
        #endregion
    }
}
