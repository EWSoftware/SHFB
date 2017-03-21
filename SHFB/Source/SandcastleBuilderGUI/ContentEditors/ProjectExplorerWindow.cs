//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ProjectExplorerWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/12/2017
// Note    : Copyright 2008-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to manage the project items and files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/26/2008  EFW  Created the code
// 12/04/2009  EFW  Added support for resource item files
// 04/08/2012  EFW  Added support for XAML configuration files
// 09/08/2012  EFW  Updated to support Windows Store App projects
// 10/22/2012  EFW  Updated to support .winmd documentation sources
// 05/03/2015  EFW  Removed support for topic transformation files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Build.Evaluation;

using WeifenLuo.WinFormsUI.Docking;

using Sandcastle.Core;

using SandcastleBuilder.Gui.MSBuild;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;
using SandcastleBuilder.Utils.MSBuild;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to manage the project items and files
    /// </summary>
    public partial class ProjectExplorerWindow : BaseContentEditor
    {
        #region Constants
        //=====================================================================

        /// <summary>
        /// This folder is located under the <see cref="Environment.SpecialFolder">LocalApplicationData</see>
        /// folder and contains user-defined item templates that can be added to a project.
        /// </summary>
        public const string ItemTemplates = Constants.ProgramDataFolder + "\\" + "Item Templates";

        /// <summary>
        /// This folder is located under the <see cref="Environment.SpecialFolder">LocalApplicationData</see>
        /// folder and contains user-defined conceptual content topic templates that can be added to a project.
        /// </summary>
        public const string ConceptualTemplates = Constants.ProgramDataFolder + "\\" + "Conceptual Templates";

        #endregion

        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject;
        private FileTree fileTree;
        private DocumentationSourceCollection docSources;
        private ReferenceItemCollection projectReferences;
        private BindingList<FileItem> fileItems;

        // This is used to search for a few common binary characters in the range \x00 to \x1F which should be
        // sufficient.  Note that \x1A (Ctrl+Z) is excluded as that's a common End of File marker.
        private static Regex reBinary = new Regex(@"[\x00-\x08\x0C\x0E-\x19\x1B-\x1F]");
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get a reference to the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get { return currentProject; }
            set
            {
                currentProject = value;
                this.LoadProject();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectExplorerWindow()
        {
            ToolStripMenuItem subMenu;
            InitializeComponent();

            this.AddItemTemplates();
            this.AddConceptualTemplates();
            this.AddCustomTemplates();

            subMenu = new ToolStripMenuItem("Documentation Sources");
            subMenu.DropDown = cmsDocSource;
            MainForm.Host.ProjectExplorerMenu.DropDownItems.Add(subMenu);

            subMenu = new ToolStripMenuItem("References");
            subMenu.DropDown = cmsReference;
            MainForm.Host.ProjectExplorerMenu.DropDownItems.Add(subMenu);

            subMenu = new ToolStripMenuItem("Files");
            subMenu.DropDown = cmsFile;
            MainForm.Host.ProjectExplorerMenu.DropDownItems.Add(subMenu);

            this.LoadProject();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override bool IsDirty
        {
            get { return currentProject != null && currentProject.IsDirty; }
            set { /* Handled by the property page and main form */ }
        }

        /// <inheritdoc />
        public override bool CanSaveContent
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool Save()
        {
            return this.Save(currentProject.Filename);
        }

        /// <inheritdoc />
        public override bool SaveAs()
        {
            bool result = false;

            if(!MainForm.Host.ProjectProperties.Apply())
                return false;

            using(SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Help Project As";
                dlg.Filter = "Sandcastle Help File Builder Project Files (*.shfbproj)|*.shfbproj|" +
                    "All files (*.*)|*.*";
                dlg.DefaultExt = "shfbproj";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();

                if(dlg.ShowDialog() == DialogResult.OK)
                    result = this.Save(dlg.FileName);
            }

            return result;
        }
        #endregion

        #region Public helper methods
        //=====================================================================

        /// <summary>
        /// This is called to refresh the project explorer view after another pane has added files to the project
        /// </summary>
        public void RefreshProject()
        {
            this.LoadProject();
        }

        /// <summary>
        /// This is called to ask the user if they want to save their project
        /// </summary>
        /// <returns>Returns true if saved successfully or no save is wanted.  Returns false on error or if
        /// canceled.</returns>
        public bool AskToSaveProject()
        {
            DialogResult dr;
            bool result = true;

            if(MainForm.Host.ProjectProperties.IsDirty)
            {
                dr = MessageBox.Show("Do you want to save your changes to this project?  Click YES to save " +
                  "changes, NO to discard them, or CANCEL to stay here and make more changes.", Constants.AppName,
                  MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if(dr == DialogResult.Cancel)
                    result = false;
                else
                    if(dr == DialogResult.Yes)
                        result = this.Save();
            }

            return result;
        }

        /// <summary>
        /// Save the project file
        /// </summary>
        /// <param name="filename">The filename to which the project is saved</param>
        /// <returns>True if successful, false if not</returns>
        public bool Save(string filename)
        {
            if(!MainForm.Host.ProjectProperties.Apply())
                return false;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Saving the project sets the given filename as the project's filename
                currentProject.SaveProject(filename);

                MainForm.UpdateMruList(currentProject.Filename);
                MainForm.Host.UpdateFilenameInfo();

                pgProps.Refresh();
                tvProjectFiles.Nodes[0].Text = Path.GetFileNameWithoutExtension(filename);
                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Create an editor for the specified file
        /// </summary>
        /// <param name="fullName">The full path to the file</param>
        /// <param name="fileItem">The project file item or null if the method should find it for itself</param>
        /// <returns>The editor window created for the file</returns>
        public DockContent CreateFileEditor(string fullName, FileItem fileItem)
        {
            DockContent editor = null;
            string ext = Path.GetExtension(fullName).ToLowerInvariant();

            if(fileItem == null)
            {
                if(this.CurrentProject != null)
                    fileItem = this.CurrentProject.FindFile(fullName);

                if(fileItem == null)
                    return null;
            }

            if(!File.Exists(fullName))
                return null;

            // Try for a built in editor
            switch(SandcastleProject.DefaultBuildAction(fullName))
            {
                case BuildAction.None:
                case BuildAction.Content:
                    switch(ext)
                    {
                        case ".aml":
                        case ".asp":
                        case ".aspx":
                        case ".ascx":
                        case ".cmp":
                        case ".config":
                        case ".css":
                        case ".htm":
                        case ".html":
                        case ".js":
                        case ".log":
                        case ".txt":
                        case ".xml":
                            editor = new TopicEditorWindow(fullName);
                            break;

                        default:
                            break;
                    }
                    break;

                case BuildAction.CodeSnippets:
                case BuildAction.XamlConfiguration:
                    editor = new TopicEditorWindow(fullName);
                    break;

                case BuildAction.ContentLayout:
                    editor = new ContentLayoutWindow(fileItem);
                    break;

                case BuildAction.ResourceItems:
                    // The content of the resource items could be bad (ill-formed XML) so edit as text if it
                    // cannot be loaded using the default editor.
                    try
                    {
                        editor = new ResourceItemEditorWindow(fileItem);
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        MessageBox.Show("Unable to load file using the resource item editor: " + ex.Message +
                            "\r\n\r\nThe file will be opened using the standard text editor.",
                            Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        editor = new TopicEditorWindow(fullName);
                    }
                    break;

                case BuildAction.SiteMap:
                    editor = new SiteMapEditorWindow(fileItem);
                    break;

                case BuildAction.Tokens:
                    // The content of the tokens could be bad (ill-formed XML) so edit as text if it cannot be
                    // loaded using the default editor.
                    try
                    {
                        editor = new TokenEditorWindow(fileItem);
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        MessageBox.Show("Unable to load file using the token editor: " + ex.Message +
                            "\r\n\r\nThe file will be opened using the standard text editor.",
                            Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        editor = new TopicEditorWindow(fullName);
                    }
                    break;

                default:    // No association, the caller may try to launch an external editor
                    break;
            }

            // If we couldn't create one and it looks like a text file, use the topic editor
            if(editor == null)
                try
                {
                    if(!reBinary.IsMatch(File.ReadAllText(fullName)))
                        editor = new TopicEditorWindow(fullName);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }

            return editor;
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to notify the project of changes to the documentation source collection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void docSources_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(currentProject != null)
            {
                docSources.SaveToProject();
                MainForm.Host.UpdateFilenameInfo();
            }
        }

        /// <summary>
        /// This is used to notify the project of changes to the references and file item collections
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void referencesAndFiles_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(currentProject != null)
                MainForm.Host.UpdateFilenameInfo();
        }

        /// <summary>
        /// Add item templates to the New Item menu
        /// </summary>
        private void AddItemTemplates()
        {
            EventHandler onClick = new EventHandler(templateFile_OnClick);
            ToolStripMenuItem miTemplate;
            string name;
            int idx = 0;

            foreach(string file in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(
              Assembly.GetExecutingAssembly().Location), "ItemTemplates"), "*.*"))
            {
                name = Path.GetFileNameWithoutExtension(file);

                miTemplate = new ToolStripMenuItem(name, null, onClick);
                miTemplate.Tag = file;
                sbStatusBarText.SetStatusBarText(miTemplate, "Add new '" + name + "' item");
                miNewItem.DropDownItems.Insert(idx++, miTemplate);
            }
        }

        /// <summary>
        /// Add conceptual topic templates to the New Item menu
        /// </summary>
        private void AddConceptualTemplates()
        {
            EventHandler onClick = new EventHandler(templateFile_OnClick);
            ToolStripMenuItem miTemplate;
            string name;

            foreach(string file in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(
              Assembly.GetExecutingAssembly().Location), "ConceptualTemplates"), "*.aml"))
            {
                name = Path.GetFileNameWithoutExtension(file);

                miTemplate = new ToolStripMenuItem(name, null, onClick);
                miTemplate.Tag = file;
                sbStatusBarText.SetStatusBarText(miTemplate, "Add new '" + name + "' item");
                miConceptualTemplates.DropDownItems.Add(miTemplate);
            }
        }

        /// <summary>
        /// Add user-defined templates to the New Item menu
        /// </summary>
        private void AddCustomTemplates()
        {
            EventHandler onClick = new EventHandler(templateFile_OnClick);
            ToolStripMenuItem miTemplate;
            string name;

            name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ItemTemplates);

            if(Directory.Exists(name))
            {
                foreach(string file in Directory.EnumerateFiles(name, "*.*"))
                {
                    name = Path.GetFileNameWithoutExtension(file);

                    miTemplate = new ToolStripMenuItem(name, null, onClick);
                    miTemplate.Tag = file;
                    sbStatusBarText.SetStatusBarText(miTemplate, "Add new '" + name + "' item");
                    miCustomTemplates.DropDownItems.Add(miTemplate);
                }

                if(miCustomTemplates.DropDownItems.Count != 0)
                    miCustomTemplates.DropDownItems.Add(new ToolStripSeparator());
            }

            name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ConceptualTemplates);

            if(Directory.Exists(name))
            {
                foreach(string file in Directory.EnumerateFiles(name, "*.aml"))
                {
                    name = Path.GetFileNameWithoutExtension(file);

                    miTemplate = new ToolStripMenuItem(name, null, onClick);
                    miTemplate.Tag = file;
                    sbStatusBarText.SetStatusBarText(miTemplate, "Add new '" + name + "' item");
                    miCustomTemplates.DropDownItems.Add(miTemplate);
                }
            }

            if(miCustomTemplates.DropDownItems.Count == 0)
                miCustomTemplates.Enabled = false;
        }

        /// <summary>
        /// Load the project content
        /// </summary>
        private void LoadProject()
        {
            TreeNode root;

            try
            {
                tvProjectFiles.SuspendLayout();
                pgProps.SelectedObject = null;
                tvProjectFiles.Nodes.Clear();

                fileTree = new FileTree(tvProjectFiles);

                if(currentProject == null)
                    return;

                root = new TreeNode(Path.GetFileNameWithoutExtension(currentProject.Filename));
                root.Tag = new NodeData(BuildAction.Project, null);
                root.Name = "*Project";
                root.ImageIndex = root.SelectedImageIndex = (int)NodeIcon.ProjectNode;

                tvProjectFiles.Nodes.Add(root);

                // Load the documentation sources
                this.LoadDocSources(true);

                // Load the references
                this.LoadReferences(true);

                // Load the folders and files
                fileItems = new BindingList<FileItem>(currentProject.FileItems.ToList());
                fileItems.ListChanged += referencesAndFiles_ListChanged;

                fileTree.LoadTree(fileItems);
                root.Expand();
            }
            finally
            {
                tvProjectFiles.ResumeLayout();

                MainForm.Host.UpdateFilenameInfo();
            }
        }

        /// <summary>
        /// Load the documentation sources
        /// </summary>
        /// <param name="createRoot">True to create the root documentation source node or false to find it and
        /// reload it.</param>
        /// <returns>The root reference item node</returns>
        private TreeNode LoadDocSources(bool createRoot)
        {
            TreeNode source, root = null;
            TreeNode[] matches;

            if(createRoot)
            {
                root = new TreeNode("Documentation Sources");
                root.Tag = new NodeData(BuildAction.DocumentationSource, null);
                root.Name = "*DocSources";
                root.ImageIndex = root.SelectedImageIndex = (int)NodeIcon.DocSourceFolder;

                tvProjectFiles.Nodes[0].Nodes.Add(root);

                if(docSources != null)
                    docSources.ListChanged -= docSources_ListChanged;

                // Cast it back to the collection so that we can use it for editing
                docSources = (DocumentationSourceCollection)currentProject.DocumentationSources;
                docSources.ListChanged += docSources_ListChanged;
            }
            else
            {
                matches = tvProjectFiles.Nodes[0].Nodes.Find("*DocSources", false);

                if(matches.Length == 1)
                    root = matches[0];
            }

            if(root != null)
            {
                root.Nodes.Clear();

                foreach(DocumentationSource ds in docSources.OrderBy(ds => ds.SourceDescription))
                {
                    source = new TreeNode(ds.SourceDescription);
                    source.Name = ds.SourceFile;
                    source.Tag = new NodeData(BuildAction.DocumentationSource, ds);
                    source.ImageIndex = source.SelectedImageIndex = (int)NodeIcon.DocSource;
                    root.Nodes.Add(source);
                }

                root.Expand();
            }

            return root;
        }

        /// <summary>
        /// Load the reference items
        /// </summary>
        /// <param name="createRoot">True to create the root references node or false to find it and reload it</param>
        /// <returns>The root reference item node</returns>
        private TreeNode LoadReferences(bool createRoot)
        {
            TreeNode source, root = null;
            TreeNode[] matches;

            if(createRoot)
            {
                root = new TreeNode("References");
                root.Tag = new NodeData(BuildAction.ReferenceItem, null);
                root.Name = "*References";
                root.ImageIndex = root.SelectedImageIndex = (int)NodeIcon.ReferenceFolder;

                tvProjectFiles.Nodes[0].Nodes.Add(root);

                projectReferences = new ReferenceItemCollection(currentProject);
                projectReferences.ListChanged += referencesAndFiles_ListChanged;
            }
            else
            {
                matches = tvProjectFiles.Nodes[0].Nodes.Find("*References", false);

                if(matches.Length == 1)
                    root = matches[0];
            }

            if(root != null && projectReferences != null)
            {
                root.Nodes.Clear();

                foreach(ReferenceItem refItem in projectReferences.OrderBy(r => r.Reference))
                {
                    source = new TreeNode(refItem.Reference);
                    source.Name = refItem.Reference;
                    source.Tag = new NodeData(BuildAction.ReferenceItem, refItem);
                    source.ImageIndex = source.SelectedImageIndex = (int)NodeIcon.ReferenceItem;
                    root.Nodes.Add(source);
                }

                root.Expand();
            }

            return root;
        }

        /// <summary>
        /// Edit the selected node's file if possible
        /// </summary>
        /// <param name="node">The node to edit</param>
        private void EditNodeFile(TreeNode node)
        {
            NodeData nodeData = (NodeData)node.Tag;
            DockContent editor;
            FileItem fileItem;
            string fullName, ext;

            if(nodeData.BuildAction >= BuildAction.Folder)
                return;

            fileItem = (FileItem)nodeData.Item;
            fullName = fileItem.IncludePath;
            ext = Path.GetExtension(fullName);

            // If the document is already open, just activate it
            foreach(IDockContent content in this.DockPanel.Contents)
                if(String.Compare(content.DockHandler.ToolTipText, fullName, true, CultureInfo.CurrentCulture) == 0)
                {
                    content.DockHandler.Activate();
                    return;
                }

            if(!File.Exists(fullName))
            {
                MessageBox.Show("File does not exist: " + fullName, Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Give preference to the external editors if any are defined
            foreach(ContentFileEditor fileEditor in ContentFileEditorCollection.GlobalEditors)
                if(fileEditor.IsEditorFor(ext))
                    if(ContentFileEditorCollection.GlobalEditors.LaunchEditorFor(fullName, currentProject.Filename))
                        return;

            // Try for a built-in editor
            editor = this.CreateFileEditor(fullName, fileItem);

            if(editor != null)
                editor.Show(this.DockPanel);
            else
                if(!ContentFileEditorCollection.GlobalEditors.LaunchEditorFor(fullName, currentProject.Filename))
                    MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                        "Unable to launch '{0}' for editing.  Reason: {1}", fullName,
                        ContentFileEditorCollection.GlobalEditors.LastError.Message),
                        Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// This is used to determine whether or not the Open with Text Editor context menu option is shown
        /// </summary>
        /// <param name="fullName">The full path to the file</param>
        /// <returns>True if it should be shown, or false if not (not supported or it's the default editor</returns>
        private static bool ShowOpenWithTextEditor(string fullName)
        {
            string ext = Path.GetExtension(fullName).ToLowerInvariant();

            switch(SandcastleProject.DefaultBuildAction(fullName))
            {
                case BuildAction.None:
                case BuildAction.Content:
                    switch(ext)
                    {
                        case ".aml":
                        case ".asp":
                        case ".aspx":
                        case ".ascx":
                        case ".cmp":
                        case ".config":
                        case ".css":
                        case ".htm":
                        case ".html":
                        case ".js":
                        case ".log":
                        case ".txt":
                        case ".xml":
                            return false;

                        default:
                            return true;
                    }

                case BuildAction.CodeSnippets:
                case BuildAction.Image:
                case BuildAction.XamlConfiguration:
                    return false;

                default:
                    return true;
            }
        }
        #endregion

        #region Property grid event handlers
        //=====================================================================

        /// <summary>
        /// Refresh the node text when a property changes
        /// </summary>
        /// <param name="s">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pgProps_PropertyValueChanged(object s,
          PropertyValueChangedEventArgs e)
        {
            TreeNode selectedNode = tvProjectFiles.SelectedNode;
            NodeData nodeData = (NodeData)selectedNode.Tag;

            switch(nodeData.BuildAction)
            {
                case BuildAction.DocumentationSource:
                    selectedNode.Text = ((DocumentationSource)nodeData.Item).SourceDescription;
                    break;

                case BuildAction.ReferenceItem:
                    selectedNode.Text = ((ReferenceItem)nodeData.Item).Reference;
                    break;

                default:
                    selectedNode.Text = ((FileItem)nodeData.Item).Name;

                    if(nodeData.BuildAction == BuildAction.Folder)
                        fileTree.RefreshPathsInChildren(selectedNode);
                    break;
            }
        }
        #endregion

        #region Tree view event handlers
        //=====================================================================

        /// <summary>
        /// Prevent the root node from being collapsed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if(e.Node.Parent == null)
                e.Cancel = true;
        }

        /// <summary>
        /// After a selection is made, set it as the current object in the property grid
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            pgProps.SelectedObject = ((NodeData)e.Node.Tag).Item;
        }

        /// <summary>
        /// This is used to select the clicked node and display the context
        /// menu when a right click occurs.  This ensures that the correct
        /// node is affected by the selected operation.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_MouseDown(object sender, MouseEventArgs e)
        {
            ContextMenuStrip cms;
            Point pt;
            TreeNode targetNode;
            NodeData nodeData;

            if(e.Button == MouseButtons.Right && currentProject != null)
            {
                pt = new Point(e.X, e.Y);

                if(pt == Point.Empty)
                    targetNode = tvProjectFiles.SelectedNode;
                else
                    targetNode = tvProjectFiles.GetNodeAt(pt);

                if(targetNode != null)
                    tvProjectFiles.SelectedNode = targetNode;

                if(targetNode == null)
                    cms = cmsFile;
                else
                {
                    nodeData = (NodeData)targetNode.Tag;

                    switch(nodeData.BuildAction)
                    {
                        case BuildAction.DocumentationSource:
                            cms = cmsDocSource;
                            break;

                        case BuildAction.ReferenceItem:
                            cms = cmsReference;
                            break;

                        default:
                            cms = cmsFile;
                            break;
                    }
                }

                cms.Show(tvProjectFiles, pt);
            }
        }

        /// <summary>
        /// Edit the node's file if it is double-clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
                this.EditNodeFile(e.Node);
        }

        /// <summary>
        /// Handle shortcut keys in the tree view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_KeyDown(object sender, KeyEventArgs e)
        {
            NodeData nodeData;

            if(tvProjectFiles.SelectedNode != null)
                switch(e.KeyCode)
                {
                    case Keys.Apps:
                        tvProjectFiles_MouseDown(sender, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
                        e.Handled = e.SuppressKeyPress = true;
                        break;

                    case Keys.Delete:
                        nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;

                        switch(nodeData.BuildAction)
                        {
                            case BuildAction.Project:
                                break;

                            case BuildAction.DocumentationSource:
                                if(nodeData.Item != null)
                                    miRemoveDocSource_Click(sender, e);
                                break;

                            case BuildAction.ReferenceItem:
                                if(nodeData.Item != null)
                                    miRemoveReference_Click(sender, e);
                                break;

                            default:
                                if(((FileItem)nodeData.Item).HasMetadata(BuildItemMetadata.LinkPath))
                                    miExcludeFromProject_Click(sender, e);
                                else
                                    miDelete_Click(sender, e);
                                break;
                        }

                        e.Handled = e.SuppressKeyPress = true;
                        break;

                    case Keys.Enter:
                        this.EditNodeFile(tvProjectFiles.SelectedNode);
                        e.Handled = e.SuppressKeyPress = true;
                        break;

                    case Keys.F2:
                        tvProjectFiles.SelectedNode.BeginEdit();
                        break;

                    default:
                        break;
                }
        }

        /// <summary>
        /// Disable label edit on doc source and reference nodes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            NodeData nodeData = (NodeData)e.Node.Tag;

            if(nodeData.BuildAction > BuildAction.Project)
            {
                e.CancelEdit = true;
                return;
            }
        }

        /// <summary>
        /// Rename the node after the label edit finishes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            NodeData nodeData = (NodeData)e.Node.Tag;
            string newName;
            int pos;

            if(e.Label == null)
                return;

            try
            {
                if(nodeData.BuildAction == BuildAction.Project)
                {
                    newName = Path.Combine(Path.GetDirectoryName(currentProject.Filename), e.Label + ".shfbproj");
                    File.Move(currentProject.Filename, newName);
                    currentProject.SaveProject(newName);
                    MainForm.Host.UpdateFilenameInfo();
                }
                else
                {
                    ((FileItem)nodeData.Item).Name = e.Label;

                    // The node name needs to preserve the path
                    newName = e.Node.FullPath;
                    pos = newName.IndexOf('\\');

                    if(pos != -1)
                        newName = newName.Substring(pos + 1);

                    newName = newName.Substring(0, newName.Length - e.Node.Text.Length);

                    e.Node.Name = newName + e.Label;

                    if(nodeData.BuildAction == BuildAction.Folder)
                        e.Node.Name += @"\";
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Unable to rename item: " + ex.Message, Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                e.CancelEdit = true;
            }
            finally
            {
                if(nodeData.BuildAction == BuildAction.Folder)
                    fileTree.RefreshPathsInChildren(e.Node);

                currentProject.MSBuildProject.ReevaluateIfNecessary();

                pgProps.Refresh();
            }
        }
        #endregion

        #region Documentation source context menu event handlers
        //=====================================================================

        /// <summary>
        /// Update the command states when the menu opens
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmsDocSource_Opening(object sender, CancelEventArgs e)
        {
            NodeData nodeData = null;
            
            if(tvProjectFiles.SelectedNode != null)
                nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;

            miRemoveDocSource.Visible = (nodeData != null &&
                nodeData.BuildAction == BuildAction.DocumentationSource && nodeData.Item != null);
        }

        /// <summary>
        /// Add one or more new documentation sources
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAddDocSource_Click(object sender, EventArgs e)
        {
            string ext, otherFile;

            if(docSources == null)
                return;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the documentation source(s)";
                dlg.Filter = "Assemblies, Comments Files, and Projects (*.dll, *.exe, *.winmd, *.xml, " +
                    "*.sln, *.*proj)|*.dll;*.exe;*.winmd;*.xml;*.sln;*.*proj|" +
                    "Library Files (*.dll, *.winmd)|*.dll;*.winmd|" +
                    "Executable Files (*.exe)|*.exe|" +
                    "XML Comments Files (*.xml)|*.xml|" +
                    "Visual Studio Solution Files (*.sln)|*.sln|" +
                    "Visual Studio Project Files (*.*proj)|*.*proj|" +
                    "All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "dll";
                dlg.Multiselect = true;

                // If selected, add the new file(s)
                if(dlg.ShowDialog() == DialogResult.OK)
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        HashSet<string> projectList = new HashSet<string>();

                        foreach(string file in dlg.FileNames)
                            if(!file.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                                projectList.Add(file);
                            else
                                foreach(string project in SelectProjectsDlg.SelectSolutionOrProjects(file))
                                    projectList.Add(project);

                        foreach(string file in projectList)
                        {
                            docSources.Add(file, null, null, false);

                            ext = Path.GetExtension(file).ToLowerInvariant();

                            // If there's a match for a comments file or an assembly, add it too
                            if(ext == ".xml")
                            {
                                otherFile = Path.ChangeExtension(file, ".dll");

                                if(File.Exists(otherFile))
                                    docSources.Add(otherFile, null, null, false);
                                else
                                {
                                    otherFile = Path.ChangeExtension(file, ".exe");

                                    if(File.Exists(otherFile))
                                        docSources.Add(otherFile, null, null, false);
                                    else
                                    {
                                        otherFile = Path.ChangeExtension(file, ".winmd");

                                        if(File.Exists(otherFile))
                                            docSources.Add(otherFile, null, null, false);
                                    }
                                }
                            }
                            else
                                if(ext == ".dll" || ext == ".exe" || ext == ".winmd")
                                {
                                    otherFile = Path.ChangeExtension(file, ".xml");

                                    if(File.Exists(otherFile))
                                        docSources.Add(otherFile, null, null, false);
                                }
                        }

                        tvProjectFiles.SelectedNode = this.LoadDocSources(false);
                        tvProjectFiles.SelectedNode.Expand();
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
            }
        }

        /// <summary>
        /// Remove a documentation source from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miRemoveDocSource_Click(object sender, EventArgs e)
        {
            NodeData nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;
            DocumentationSource ds = (DocumentationSource)nodeData.Item;

            if(MessageBox.Show("Are you sure you want to remove '" + ds.SourceDescription + "' from the project?",
              Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                docSources.Remove(ds);
                tvProjectFiles.SelectedNode = this.LoadDocSources(false);
                tvProjectFiles_AfterSelect(tvProjectFiles, new TreeViewEventArgs(tvProjectFiles.SelectedNode));
            }
        }
        #endregion

        #region Reference context menu event handlers
        //=====================================================================

        /// <summary>
        /// Update the command states when the menu opens
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmsReference_Opening(object sender, CancelEventArgs e)
        {
            NodeData nodeData = null;
            
            if(tvProjectFiles.SelectedNode != null)
                nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;

            miRemoveReference.Visible = (nodeData != null &&
                nodeData.BuildAction == BuildAction.ReferenceItem &&
                nodeData.Item != null);
        }

        /// <summary>
        /// Add a project or file reference to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAddReference_Click(object sender, EventArgs e)
        {
            string extension;

            if(projectReferences == null)
                return;

            try
            {
                using(OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select the reference file(s)";
                    dlg.Filter = "Library, Executable, and Project Files (*.dll, *.exe, *.winmd, *.*proj)|" +
                        "*.dll;*.exe;*.winmd;*.*proj|" +
                        "Library Files (*.dll, *.winmd)|*.dll;*.winmd|" +
                        "Executable Files (*.exe)|*.exe|" +
                        "Visual Studio Project Files (*.*proj)|*.*proj|" +
                        "All Files (*.*)|*.*";
                    dlg.InitialDirectory = Directory.GetCurrentDirectory();
                    dlg.DefaultExt = "dll";
                    dlg.Multiselect = true;

                    // If selected, add the file(s)
                    if(dlg.ShowDialog() == DialogResult.OK)
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            foreach(string file in dlg.FileNames)
                            {
                                extension = Path.GetExtension(file).ToLowerInvariant();

                                if(extension == ".exe" || extension == ".dll" || extension == ".winmd")
                                    projectReferences.AddReference(Path.GetFileNameWithoutExtension(file), file);
                                else
                                    if(extension.EndsWith("proj", StringComparison.Ordinal))
                                        projectReferences.AddProjectReference(file);
                            }

                            tvProjectFiles.SelectedNode = this.LoadReferences(false);
                            tvProjectFiles.SelectedNode.Expand();
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Add a GAC reference to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAddGacReference_Click(object sender, EventArgs e)
        {
            if(projectReferences == null)
                return;

            try
            {
                using(SelectGacEntriesDlg dlg = new SelectGacEntriesDlg())
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            foreach(string gacEntry in dlg.SelectedEntries)
                                projectReferences.AddReference(gacEntry, null);

                            tvProjectFiles.SelectedNode = this.LoadReferences(false);
                            tvProjectFiles.SelectedNode.Expand();
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Remove the selected reference from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miRemoveReference_Click(object sender, EventArgs e)
        {
            if(projectReferences != null)
            {
                NodeData nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;
                ReferenceItem refItem = (ReferenceItem)nodeData.Item;

                if(MessageBox.Show("Are you sure you want to remove '" + refItem.Reference + "' from the project?",
                  Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    projectReferences.Remove(refItem);
                    tvProjectFiles.SelectedNode = this.LoadReferences(false);
                    tvProjectFiles_AfterSelect(tvProjectFiles, new TreeViewEventArgs(tvProjectFiles.SelectedNode));
                }
            }
        }
        #endregion

        #region File context menu event handlers
        //=====================================================================

        /// <summary>
        /// Update the command states when the menu opens
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmsFile_Opening(object sender, CancelEventArgs e)
        {
            NodeData nodeData = null;
            FileItem fileItem = null;

            if(tvProjectFiles.SelectedNode != null)
            {
                nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;

                if(nodeData.BuildAction <= BuildAction.Folder)
                    fileItem = (FileItem)nodeData.Item;
            }

            miOpen.Visible = miOpenSeparator.Visible = (nodeData != null &&
                nodeData.BuildAction < BuildAction.Folder);
            miOpenWithTextEditor.Visible = miOpenWithSeparator.Visible = (nodeData != null &&
                nodeData.BuildAction < BuildAction.Folder && fileItem != null &&
                ShowOpenWithTextEditor(fileItem.IncludePath));
            miDelete.Visible = (fileItem != null && !fileItem.HasMetadata(BuildItemMetadata.LinkPath));
            miExcludeFromProject.Enabled = miCut.Enabled = miCopy.Enabled = miRename.Enabled =
                (nodeData != null && nodeData.BuildAction <= BuildAction.Folder);
            miPaste.Enabled = (Clipboard.GetDataObject().GetDataPresent(DataFormats.FileDrop) &&
                Clipboard.GetDataObject().GetDataPresent("Preferred DropEffect") && nodeData != null &&
                nodeData.BuildAction <= BuildAction.Project);
        }

        /// <summary>
        /// Open the selected file for editing using the default editor
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miOpen_Click(object sender, EventArgs e)
        {
            this.EditNodeFile(tvProjectFiles.SelectedNode);
        }

        /// <summary>
        /// Open the file for editing using the text editor if possible
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miOpenWithTextEditor_Click(object sender, EventArgs e)
        {
            NodeData nodeData = (NodeData)tvProjectFiles.SelectedNode.Tag;
            DockContent editor;
            FileItem fileItem;
            string fullName;

            if(nodeData.BuildAction >= BuildAction.Folder)
                return;

            fileItem = (FileItem)nodeData.Item;
            fullName = fileItem.IncludePath;

            // If the document is already open, just activate it
            foreach(IDockContent content in this.DockPanel.Contents)
                if(String.Compare(content.DockHandler.ToolTipText, fullName, true, CultureInfo.CurrentCulture) == 0)
                {
                    content.DockHandler.Activate();
                    return;
                }

            if(!File.Exists(fullName))
            {
                MessageBox.Show("File does not exist: " + fullName, Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                if(!reBinary.IsMatch(File.ReadAllText(fullName)))
                {
                    editor = new TopicEditorWindow(fullName);
                    editor.Show(this.DockPanel);
                }
                else
                    MessageBox.Show("The selected file does not appear to be a text file.  Use the Open " +
                        "option instead to open the default editor", Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Unable to create text editor: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Rename the selected node
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miRename_Click(object sender, EventArgs e)
        {
            tvProjectFiles.SelectedNode.BeginEdit();
        }

        /// <summary>
        /// Exclude an item from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>This removes the item and any child items from the project
        /// but leaves them on disk.</remarks>
        private void miExcludeFromProject_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = tvProjectFiles.SelectedNode;

            try
            {
                if(MessageBox.Show("The item '" + selectedNode.Text + "' will be removed from the project.  " +
                  "Are you sure?", Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                  MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    fileTree.RemoveNode(selectedNode, false);

                    var nodeData = (NodeData)selectedNode.Tag;

                    fileItems.Remove((FileItem)nodeData.Item);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error removing item: " + ex.Message, Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                tvProjectFiles_AfterSelect(tvProjectFiles, new TreeViewEventArgs(tvProjectFiles.SelectedNode));
            }
        }

        /// <summary>
        /// Delete an item from the project and the file system
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>This removes the item and any child items from the project
        /// and the file system.</remarks>
        private void miDelete_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = tvProjectFiles.SelectedNode;

            try
            {
                if(MessageBox.Show("The item '" + selectedNode.Text + "' will be permanently deleted.  Are you sure?",
                  Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                  MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    fileTree.RemoveNode(selectedNode, true);

                    var nodeData = (NodeData)selectedNode.Tag;

                    fileItems.Remove((FileItem)nodeData.Item);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Error deleting item: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                tvProjectFiles_AfterSelect(tvProjectFiles,
                    new TreeViewEventArgs(tvProjectFiles.SelectedNode));
            }
        }

        /// <summary>
        /// Add a new folder to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miNewFolder_Click(object sender, EventArgs e)
        {
            TreeNode parent = tvProjectFiles.SelectedNode;
            TreeNode[] matches;
            FileItem folderItem;
            NodeData nodeData;
            string path, newFolder;
            int uniqueId = 0;

            if(parent != null)
            {
                nodeData = (NodeData)parent.Tag;

                // If it's a folder, add it as a child of the folder
                if(nodeData.BuildAction != BuildAction.Folder)
                    parent = parent.Parent;
            }

            if(parent == null || parent == tvProjectFiles.Nodes[0])
                path = Path.GetDirectoryName(currentProject.Filename);
            else
            {
                nodeData = (NodeData)parent.Tag;

                if(nodeData.BuildAction == BuildAction.DocumentationSource ||
                  nodeData.BuildAction == BuildAction.ReferenceItem)
                {
                    parent = null;
                    path = Path.GetDirectoryName(currentProject.Filename);
                }
                else
                    if(nodeData.BuildAction == BuildAction.Folder)
                        path = ((FileItem)nodeData.Item).IncludePath;
                    else
                        path = Path.GetDirectoryName(((FileItem)nodeData.Item).IncludePath);
            }

            do
            {
                uniqueId++;
                newFolder = Path.Combine(path, "NewFolder" + uniqueId.ToString(CultureInfo.InvariantCulture));

            } while(Directory.Exists(newFolder));

            folderItem = currentProject.AddFolderToProject(newFolder);

            fileItems.Add(folderItem);
            fileTree.LoadTree(new[] { folderItem });

            if(parent == null)
                matches = tvProjectFiles.Nodes[0].Nodes.Find(folderItem.IncludePath.PersistablePath, false);
            else
                matches = parent.Nodes.Find(folderItem.IncludePath.PersistablePath, false);

            if(matches.Length > 0)
            {
                tvProjectFiles.SelectedNode = matches[0];
                matches[0].BeginEdit();
            }
        }

        /// <summary>
        /// Add an existing folder to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAddExistingFolder_Click(object sender, EventArgs e)
        {
            List<FileItem> toAdd = new List<FileItem>();
            TreeNode parent = tvProjectFiles.SelectedNode;
            TreeNode[] matches;
            FileItem folderItem;
            NodeData nodeData;
            string path, newFolder;

            if(parent != null)
            {
                nodeData = (NodeData)parent.Tag;

                // If it's a folder, add it as a child of the folder
                if(nodeData.BuildAction != BuildAction.Folder)
                    parent = parent.Parent;
            }

            if(parent == null || parent == tvProjectFiles.Nodes[0])
                path = Path.GetDirectoryName(currentProject.Filename);
            else
            {
                nodeData = (NodeData)parent.Tag;

                if(nodeData.BuildAction == BuildAction.DocumentationSource ||
                  nodeData.BuildAction == BuildAction.ReferenceItem)
                {
                    parent = null;
                    path = Path.GetDirectoryName(currentProject.Filename);
                }
                else
                    if(nodeData.BuildAction == BuildAction.Folder)
                        path = ((FileItem)nodeData.Item).IncludePath;
                    else
                        path = Path.GetDirectoryName(((FileItem)nodeData.Item).IncludePath);
            }

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the folder to add";
                dlg.SelectedPath = path;

                // If selected, set the new folder
                if(dlg.ShowDialog() != DialogResult.OK)
                    return;

                newFolder = dlg.SelectedPath + @"\";
            }

            if(!newFolder.StartsWith(path, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The new folder must be under '" + path + "'",
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                folderItem = currentProject.AddFolderToProject(newFolder);
                toAdd.Add(folderItem);

                // Get all of the files and subfolders within it too
                this.AddSubFolders(newFolder, toAdd);

                foreach(var item in toAdd)
                    fileItems.Add(item);

                fileTree.LoadTree(toAdd);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if(parent == null)
                matches = tvProjectFiles.Nodes[0].Nodes.Find(
                    folderItem.IncludePath.PersistablePath, false);
            else
                matches = parent.Nodes.Find(folderItem.IncludePath.PersistablePath,
                    false);

            if(matches.Length > 0)
                tvProjectFiles.SelectedNode = matches[0];
        }

        /// <summary>
        /// Add all files and subfolders to the project
        /// </summary>
        /// <param name="rootFolder">The root folder</param>
        /// <param name="items">The file items added</param>
        private void AddSubFolders(string rootFolder, List<FileItem> items)
        {
            foreach(string file in Directory.EnumerateFiles(rootFolder, "*.*"))
                items.Add(currentProject.AddFileToProject(file, file));

            foreach(string folder in Directory.EnumerateDirectories(rootFolder))
            {
                items.Add(currentProject.AddFolderToProject(folder));
                this.AddSubFolders(folder, items);
            }
        }

        /// <summary>
        /// Add an existing item to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAddExistingItem_Click(object sender, EventArgs e)
        {
            List<FileItem> toAdd = new List<FileItem>();
            NodeData nodeData;
            TreeNode parent = tvProjectFiles.SelectedNode;
            FileItem fileItem;
            string path, newPath;

            if(parent == null || parent == tvProjectFiles.Nodes[0])
                path = Path.GetDirectoryName(currentProject.Filename);
            else
            {
                nodeData = (NodeData)parent.Tag;

                if(nodeData.BuildAction == BuildAction.DocumentationSource ||
                  nodeData.BuildAction == BuildAction.ReferenceItem)
                    path = Path.GetDirectoryName(currentProject.Filename);
                else
                    if(nodeData.BuildAction == BuildAction.Folder)
                        path = ((FileItem)nodeData.Item).IncludePath;
                    else
                        path = Path.GetDirectoryName(((FileItem)nodeData.Item).IncludePath);
            }

            try
            {
                using(OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select the file(s) to add";
                    dlg.Filter = "Project Files (*.aml, *.htm*, *.css, *.js, " +
                        "*.content, *.sitemap, *.snippets, *.tokens, *.items)|*.aml;" +
                        "*.htm*;*.css;*.js;*.content;*.sitemap;*.tokens;" +
                        "*.snippets;*.items|Content Files (*.aml, *.htm*)|*.aml;*.htm*|" +
                        "Content Layout Files (*.content, *.sitemap)|" +
                        "*.content;*.sitemap|Image Files (*.bmp, *.gif, " +
                        "*.jpg, *.jpe*, *.png)|*.bmp;*.gif;*.jpg;*.jpe*;" +
                        "*.png|All Files (*.*)|*.*";
                    dlg.InitialDirectory = path;
                    dlg.DefaultExt = "aml";
                    dlg.Multiselect = true;

                    // If selected, add the file(s)
                    if(dlg.ShowDialog() == DialogResult.OK)
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            foreach(string file in dlg.FileNames)
                            {
                                newPath = Path.Combine(path, Path.GetFileName(file));
                                fileItem = currentProject.AddFileToProject(file, newPath);

                                fileItems.Add(fileItem);
                                toAdd.Add(fileItem);
                            }

                            fileTree.LoadTree(toAdd);
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Import image file information from an existing MAML media content file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miImportMediaFile_Click(object sender, EventArgs e)
        {
            List<string> filesSeen = new List<string>();
            XPathDocument media;
            XPathNavigator navMedia, file, altText;
            FileItem fileItem;
            TreeNode parent = tvProjectFiles.SelectedNode;
            NodeData nodeData;
            string guid, id, path, destPath, newName;
            int uniqueId;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the MAML media content file";
                dlg.Filter = "MAML media content files (*.xml)|*.xml|All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "xml";

                // If selected, add the new images from the media file.  Images with an ID that is already in
                // the collection are ignored.
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    if(parent == null || parent == tvProjectFiles.Nodes[0])
                        destPath = Path.GetDirectoryName(currentProject.Filename);
                    else
                    {
                        nodeData = (NodeData)parent.Tag;

                        if(nodeData.BuildAction == BuildAction.DocumentationSource ||
                          nodeData.BuildAction == BuildAction.ReferenceItem)
                            destPath = Path.GetDirectoryName(currentProject.Filename);
                        else
                            if(nodeData.BuildAction == BuildAction.Folder)
                                destPath = ((FileItem)nodeData.Item).IncludePath;
                            else
                                destPath = Path.GetDirectoryName(
                                    ((FileItem)nodeData.Item).IncludePath);
                    }

                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        var images = currentProject.ImagesReferences;
                        media = new XPathDocument(dlg.FileName);
                        navMedia = media.CreateNavigator();

                        foreach(XPathNavigator item in navMedia.Select("//item"))
                        {
                            guid = null;
                            file = altText = null;
                            id = item.GetAttribute("id", String.Empty);
                            file = item.SelectSingleNode("image/@file");
                            altText = item.SelectSingleNode("image/altText");

                            if(!String.IsNullOrEmpty(id))
                                guid = id.Trim();

                            if(!String.IsNullOrEmpty(guid) && !images.Any(img => img.Id.Equals(guid,
                              StringComparison.OrdinalIgnoreCase)) && file != null &&
                              !String.IsNullOrEmpty(file.Value))
                            {
                                path = newName = file.Value;

                                // If relative, get the full path
                                if(!Path.IsPathRooted(path))
                                    path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(dlg.FileName), path));

                                // It's possible that two entries share the same file so we'll need to create a
                                // new copy as in SHFB, the settings are managed via the project explorer and
                                // each file is unique.
                                uniqueId = 1;

                                while(filesSeen.Contains(newName))
                                {
                                    newName = Path.Combine(Path.GetDirectoryName(newName),
                                        Path.GetFileNameWithoutExtension(newName) +
                                        uniqueId.ToString(CultureInfo.InvariantCulture) +
                                        Path.GetExtension(newName));
                                    uniqueId++;
                                }

                                filesSeen.Add(newName);

                                fileItem = currentProject.AddFileToProject(path,
                                    Path.Combine(destPath, Path.GetFileName(newName)));
                                fileItem.BuildAction = BuildAction.Image;
                                fileItem.ImageId = guid;

                                if(altText != null)
                                    fileItem.AlternateText = altText.Value;
                            }
                        }

                        // If existing items were found, we will have changed their build action to Image so
                        // reload the tree to ensure that the change is reflected there.
                        this.LoadProject();
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
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
            TreeNode parent = tvProjectFiles.SelectedNode;
            NodeData nodeData;
            FileItem fileItem;
            Guid guid = Guid.NewGuid();
            string path, newName, file = (string)miSelection.Tag;
            bool isConceptual;

            if(parent == null || parent == tvProjectFiles.Nodes[0])
                path = Path.GetDirectoryName(currentProject.Filename);
            else
            {
                nodeData = (NodeData)parent.Tag;

                if(nodeData.BuildAction == BuildAction.DocumentationSource ||
                  nodeData.BuildAction == BuildAction.ReferenceItem)
                    path = Path.GetDirectoryName(currentProject.Filename);
                else
                    if(nodeData.BuildAction == BuildAction.Folder)
                        path = ((FileItem)nodeData.Item).IncludePath;
                    else
                        path = Path.GetDirectoryName(((FileItem)nodeData.Item).IncludePath);
            }

            using(SaveFileDialog dlg = new SaveFileDialog())
            {
                if(file.EndsWith(".aml", StringComparison.OrdinalIgnoreCase))
                {
                    newName = guid.ToString() + ".aml";
                    isConceptual = true;
                }
                else
                {
                    newName = Path.GetFileName(file);
                    isConceptual = false;
                }

                dlg.Title = "Save New File As";
                dlg.Filter = "All files (*.*)|*.*";
                dlg.FileName = newName;
                dlg.InitialDirectory = path;
                dlg.DefaultExt = Path.GetExtension(file);

                string ext = Path.GetExtension(file);

                if(!String.IsNullOrWhiteSpace(ext))
                    dlg.Filter = String.Format(CultureInfo.InvariantCulture, "{0} files|*{1}|{2}",
                        miSelection.Text, ext, dlg.Filter);

                if(dlg.ShowDialog() == DialogResult.OK)
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        fileItem = currentProject.AddFileToProject(file, dlg.FileName);

                        fileItems.Add(fileItem);
                        fileTree.LoadTree(new[] { fileItem });

                        // If it's a conceptual content topic file, set the unique ID in it
                        if(isConceptual)
                            try
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(fileItem.FullPath);

                                XmlNode node = doc.SelectSingleNode("topic");

                                if(node == null)
                                    throw new InvalidOperationException(
                                        "Unable to locate root topic node");

                                if(node.Attributes["id"] == null)
                                    throw new InvalidOperationException(
                                        "Unable to locate 'id' attribute on root topic node");

                                node.Attributes["id"].Value = guid.ToString();
                                doc.Save(fileItem.FullPath);
                            }
                            catch(Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex);
                                MessageBox.Show("Unable to set topic ID.  Reason:" +
                                    ex.Message, Constants.AppName, MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
            }
        }
        #endregion

        #region Tree view drag and drop event handlers
        //=====================================================================

        /// <summary>
        /// This initiates drag and drop for the tree view nodes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DataObject data = new DataObject();
            TreeNode node = e.Item as TreeNode;

            if(node != null && node.Tag != null && e.Button == MouseButtons.Left)
            {
                // The tree supports dragging and dropping of nodes
                data.SetData(typeof(TreeNode), node);
                data.SetData(typeof(NodeData), node.Tag);

                this.DoDragDrop(data, DragDropEffects.All);
            }
        }

        /// <summary>
        /// This validates the drop target during the drag operation and show
        /// the appropriate cursor.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>It handles internal drag and drop items as well as files
        /// from Windows Explorer.</remarks>
        private void tvProjectFiles_DragOver(object sender, DragEventArgs e)
        {
            TreeNode targetNode, dropNode;
            NodeData nodeData;

            if(!e.Data.GetDataPresent(DataFormats.FileDrop) &&
              !e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;

            // As the mouse moves over nodes, provide feedback to the user
            // by highlighting the node that is the current drop target.
            targetNode = tvProjectFiles.GetNodeAt(tvProjectFiles.PointToClient(
                new Point(e.X, e.Y)));

            // Select the node currently under the cursor
            if(targetNode != null && tvProjectFiles.SelectedNode != targetNode)
                tvProjectFiles.SelectedNode = targetNode;
            else
                if(targetNode == null)
                    targetNode = tvProjectFiles.Nodes[0];

            // Check that the selected node is not the dropNode, that it is
            // not a child of the dropNode, or that the parent nodes match
            // and is therefore an invalid target.
            if(e.Data.GetDataPresent(typeof(TreeNode)))
            {
                dropNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

                // Don't allow drop from some other tree view (i.e. the
                // ones in the content layout editors).
                if(dropNode != null && dropNode.TreeView != tvProjectFiles)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                e.Effect = DragDropEffects.Move;

                while(targetNode != null)
                {
                    nodeData = (NodeData)targetNode.Tag;

                    if(targetNode == dropNode || (targetNode.Parent ==
                      dropNode.Parent && nodeData.BuildAction !=
                      BuildAction.Folder) || nodeData.BuildAction >
                      BuildAction.Project)
                    {
                        e.Effect = DragDropEffects.None;
                        break;
                    }

                    targetNode = targetNode.Parent;
                }
            }
        }

        /// <summary>
        /// This handles the drop operation for the tree view.  Dropped items
        /// can include other file nodes as well as files from Windows Explorer.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvProjectFiles_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode dropNode, targetNode;
            TreeNode[] matches;
            FileItem fileItem;
            NodeData nodeData;
            string path, newPath;

            // Handle files dropped from Windows Explorer
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                this.AddFilesFromWindowsExplorer(e);
                return;
            }

            if(!e.Data.GetDataPresent(typeof(TreeNode)))
                return;

            // Get the TreeNode being dragged
            dropNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // The target node should be selected from the DragOver event
            targetNode = tvProjectFiles.SelectedNode;

            if(targetNode == null || dropNode == targetNode)
                return;

            nodeData = (NodeData)targetNode.Tag;

            if(nodeData.BuildAction == BuildAction.Project)
                path = Path.GetDirectoryName(currentProject.Filename);
            else
                if(nodeData.BuildAction == BuildAction.Folder)
                    path = ((FileItem)nodeData.Item).IncludePath;
                else
                    path = Path.GetDirectoryName(((FileItem)nodeData.Item).IncludePath);

            nodeData = (NodeData)dropNode.Tag;
            fileItem = (FileItem)nodeData.Item;

            if(fileItem.BuildAction == BuildAction.Folder)
                newPath = Path.Combine(path, fileItem.Name);
            else
                newPath = Path.Combine(path, Path.GetFileName(fileItem.FullPath));

            path = fileItem.FullPath;

            if(path[path.Length - 1] == '\\')
                path = path.Substring(0, path.Length - 1);

            if(Path.GetDirectoryName(path) == Path.GetDirectoryName(newPath))
            {
                MessageBox.Show("The source and destination paths match and " +
                    "the item cannot be moved", Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if(fileItem.BuildAction != BuildAction.Folder)
                {
                    // If it's a link, copy the file to the project folder and remove the link metadata
                    if(fileItem.HasMetadata(BuildItemMetadata.LinkPath))
                    {
                        path = fileItem.LinkPath;
                        File.Copy(fileItem.IncludePath, path, true);
                        File.SetAttributes(path, FileAttributes.Normal);
                        fileItem.SetMetadata(BuildItemMetadata.LinkPath, null);
                    }

                    if(path != newPath)
                    {
                        if(File.Exists(newPath))
                            throw new ArgumentException("A file with that name already exists in the destination folder");

                        File.Move(path, newPath);
                        fileItem.IncludePath = new FilePath(newPath, fileItem.Project);
                    }
                }
                else
                {
                    // Rename the folder and all items starting with the folder name
                    if(Directory.Exists(newPath))
                        throw new ArgumentException("A folder with that " +
                            "name already exists in the destination folder");

                    Directory.Move(path, newPath);
                    path = fileItem.IncludePath;
                    newPath += "\\";
                    fileItem.IncludePath = new FilePath(newPath, fileItem.Project);

                    foreach(ProjectItem item in fileItem.Project.MSBuildProject.AllEvaluatedItems)
                        if(item.EvaluatedInclude.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                            item.UnevaluatedInclude = newPath + item.EvaluatedInclude.Substring(path.Length);
                }

                this.LoadProject();

                matches = tvProjectFiles.Nodes.Find(
                    fileItem.IncludePath.PersistablePath, true);

                if(matches.Length != 0)
                    tvProjectFiles.SelectedNode = matches[0];
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Unable to move item: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Add files dragged and dropped from Windows Explorer to the project
        /// </summary>
        /// <param name="e">The event arguments</param>
        private void AddFilesFromWindowsExplorer(DragEventArgs e)
        {
            TreeNode targetNode;
            NodeData nodeData;
            Array dropFiles;
            string path = null, ext, otherFile, newPath;

            dropFiles = (Array)e.Data.GetData(DataFormats.FileDrop);

            if(dropFiles == null || docSources == null)
                return;

            // The target node should be selected from the DragOver event.  If not, assume we are dropping files
            // in the root.
            targetNode = tvProjectFiles.SelectedNode;

            if(targetNode == null || targetNode == tvProjectFiles.Nodes[0])
            {
                path = Path.GetDirectoryName(currentProject.Filename);
                nodeData = new NodeData(BuildAction.None, null);
            }
            else
            {
                nodeData = (NodeData)targetNode.Tag;

                if(nodeData.BuildAction == BuildAction.Folder)
                    path = ((FileItem)nodeData.Item).IncludePath;
                else
                    if(nodeData.BuildAction != BuildAction.DocumentationSource &&
                      nodeData.BuildAction != BuildAction.ReferenceItem)
                        path = Path.GetDirectoryName(((FileItem)nodeData.Item).IncludePath);
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                foreach(string file in dropFiles)
                {
                    ext = Path.GetExtension(file).ToLowerInvariant();

                    // Documentation source?
                    if(nodeData.BuildAction == BuildAction.DocumentationSource)
                    {
                        if(ext != ".dll" && ext != ".exe" && ext != ".winmd" && ext != ".xml" && ext != ".sln" &&
                          !ext.EndsWith("proj", StringComparison.Ordinal))
                            continue;

                        if(ext == ".sln")
                        {
                            foreach(string project in SelectProjectsDlg.SelectSolutionOrProjects(file))
                                docSources.Add(project, null, null, false);

                            continue;
                        }

                        docSources.Add(file, null, null, false);

                        // If there's a match for a comments file or an assembly, add it too.
                        if(ext == ".xml")
                        {
                            otherFile = Path.ChangeExtension(file, ".dll");

                            if(File.Exists(otherFile))
                                docSources.Add(otherFile, null, null, false);
                            else
                            {
                                otherFile = Path.ChangeExtension(file, ".exe");

                                if(File.Exists(otherFile))
                                    docSources.Add(otherFile, null, null, false);
                                else
                                {
                                    otherFile = Path.ChangeExtension(file, ".winmd");

                                    if(File.Exists(otherFile))
                                        docSources.Add(otherFile, null, null, false);
                                }
                            }
                        }
                        else
                            if(ext == ".dll" || ext == ".exe" || ext == ".winmd")
                            {
                                otherFile = Path.ChangeExtension(file, ".xml");

                                if(File.Exists(otherFile))
                                    docSources.Add(otherFile, null, null, false);
                            }

                        continue;
                    }

                    // Reference item?
                    if(nodeData.BuildAction == BuildAction.ReferenceItem && projectReferences != null)
                    {
                        if(ext != ".dll" && ext != ".exe" && ext != ".winmd" &&
                          !ext.EndsWith("proj", StringComparison.Ordinal))
                            continue;

                        if(ext.EndsWith("proj", StringComparison.Ordinal))
                            projectReferences.AddProjectReference(file);
                        else
                            projectReferences.AddReference(Path.GetFileNameWithoutExtension(file), file);

                        continue;
                    }

                    // If dropped on a folder or file, ignore files that are obviously not content
                    if(ext == ".dll" || ext == ".exe" || ext == ".winmd" || ext == ".sln" ||
                      ext.EndsWith("proj", StringComparison.Ordinal))
                        continue;

                    newPath = Path.Combine(path, Path.GetFileName(file));
                    currentProject.AddFileToProject(file, newPath);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            // Rather than trying to synch the tree with the new nodes, we'll just reload the whole thing
            this.LoadProject();
        }
        #endregion

        #region Copy, copy, and paste event handlers
        //=====================================================================

        /// <summary>
        /// Cut or copy files to the clipboard
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCutCopy_Click(object sender, EventArgs e)
        {
            List<string> files = new List<string>();
            TreeNode node = tvProjectFiles.SelectedNode;

            if(node != null)
                this.GetFilesForClipboard(node, files);

            if(files.Count != 0)
            {
                string[] fileArray = new string[files.Count];
                files.CopyTo(fileArray, 0);

                DataObject data = new DataObject(DataFormats.FileDrop, fileArray);
                MemoryStream dropEffect = new MemoryStream(new byte[] { (byte)(sender == miCut ? 2 : 5), 0, 0, 0 });

                data.SetData("Preferred DropEffect", dropEffect);
                Clipboard.SetDataObject(data);
            }
        }

        /// <summary>
        /// This is used to get a collection of files to copy to the clipboard
        /// </summary>
        /// <param name="node">The tree node at which to start</param>
        /// <param name="files">The list used to contained the returned files</param>
        private void GetFilesForClipboard(TreeNode node, List<string> files)
        {
            NodeData nodeData = (NodeData)node.Tag;

            if(nodeData.BuildAction < BuildAction.Folder)
                files.Add(((FileItem)nodeData.Item).FullPath);
            else
                if(nodeData.BuildAction == BuildAction.Folder)
                {
                    files.Add(((FileItem)nodeData.Item).FullPath);

                    foreach(TreeNode childNode in node.Nodes)
                        this.GetFilesForClipboard(childNode, files);
                }
        }

        /// <summary>
        /// Paste files from the clipboard
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miPaste_Click(object sender, EventArgs e)
        {
            IDataObject data = Clipboard.GetDataObject();
            TreeNode pasteNode = tvProjectFiles.SelectedNode;
            TreeNode[] matches;
            NodeData nodeData;
            FileItem fileItem;
            string newPath, basePath, newSelection = null, rootFolder = null;
            int uniqueId = 0;

            if(pasteNode == null || !data.GetDataPresent(DataFormats.FileDrop) ||
              !data.GetDataPresent("Preferred DropEffect"))
                return;

            string[] files = (string[])data.GetData(DataFormats.FileDrop);
            MemoryStream stream = (MemoryStream)data.GetData("Preferred DropEffect", true);

            int dropEffect = stream.ReadByte();

            if(dropEffect != 2 && dropEffect != 5)
                return;

            nodeData = (NodeData)pasteNode.Tag;

            if(nodeData.BuildAction > BuildAction.Project)
                return;

            if(nodeData.BuildAction == BuildAction.Project)
            {
                basePath = Path.GetDirectoryName(currentProject.Filename);
                basePath += "\\";
            }
            else
            {
                if(nodeData.BuildAction == BuildAction.Folder)
                    basePath = ((FileItem)nodeData.Item).FullPath;
                else
                {
                    basePath = Path.GetDirectoryName(((FileItem)nodeData.Item).FullPath);
                    basePath += "\\";
                }
            }

            if(files[0][files[0].Length - 1] == '\\')
            {
                rootFolder = files[0];

                if(rootFolder == basePath)
                    return;

                rootFolder = rootFolder.Substring(0, rootFolder.Length - 1);
                rootFolder = rootFolder.Substring(0, rootFolder.LastIndexOf('\\'));
                rootFolder += "\\";
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                foreach(string file in files)
                {
                    if(rootFolder == null)
                        newPath = Path.Combine(basePath, Path.GetFileName(file));
                    else
                        newPath = Path.Combine(basePath, file.Substring(rootFolder.Length));

                    if((rootFolder != null || dropEffect == 2) &&
                      String.Compare(file, newPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MessageBox.Show("Files cannot be copied onto themselves", Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }

                    newSelection = newPath;

                    // Add new item to project
                    if(newPath[newPath.Length - 1] == '\\')
                        currentProject.AddFolderToProject(newPath);
                    else
                    {
                        while(File.Exists(newPath))
                        {
                            if(uniqueId == 0)
                                newPath = String.Format(CultureInfo.InvariantCulture, "{0}\\Copy of {1}",
                                    Path.GetDirectoryName(newPath), Path.GetFileName(file));
                            else
                                newPath = String.Format(CultureInfo.InvariantCulture, "{0}\\Copy ({1}) of {2}",
                                    Path.GetDirectoryName(newPath), uniqueId, Path.GetFileName(file));

                            uniqueId++;
                        }

                        currentProject.AddFileToProject(file, newPath);
                    }
                }

                // If cut, remove the old items now that everything has been added successfully
                if(dropEffect == 2)
                    foreach(string file in files)
                    {
                        fileItem = currentProject.FindFile(file);

                        if(fileItem != null)
                        {
                            fileItem.RemoveFromProjectFile();

                            if(fileItem.BuildAction == BuildAction.Folder)
                            {
                                if(Directory.Exists(fileItem.IncludePath))
                                    Directory.Delete(fileItem.IncludePath, true);
                            }
                            else
                                if(File.Exists(fileItem.IncludePath))
                                    File.Delete(fileItem.IncludePath);
                        }
                    }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if(newSelection != null)
            {
                this.LoadProject();
                fileItem = currentProject.FindFile(newSelection);

                if(fileItem != null)
                {
                    matches = tvProjectFiles.Nodes[0].Nodes.Find(
                        fileItem.IncludePath.PersistablePath, true);

                    if(matches.Length == 1)
                        tvProjectFiles.SelectedNode = matches[0];
                }
            }
        }
        #endregion
    }
}
