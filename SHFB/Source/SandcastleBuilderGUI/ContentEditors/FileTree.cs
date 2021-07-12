//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : FileTree.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to manage the project's files in the Project Explorer tree view control
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/28/2008  EFW  Created the code
// 12/04/2009  EFW  Added support for resource item files
// 04/08/2012  EFW  Added support for XAML configuration files
// 05/03/2015  EFW  Removed support for topic transformation files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used to load the <see cref="ProjectExplorerWindow" /> tree view with the project file tree
    /// </summary>
    /// <remarks>It is also used to handle such tasks as renaming, removing, and adding nodes</remarks>
    public class FileTree
    {
        #region Private data members
        //=====================================================================

        private readonly TreeView treeView;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="treeControl">The <see cref="TreeView"/> control with which this instance is associated</param>
        public FileTree(TreeView treeControl)
        {
            treeView = treeControl;
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get an appropriate icon for the node based on the filename
        /// </summary>
        /// <param name="filename">The filename associated with the node</param>
        private static NodeIcon NodeIconFromFilename(string filename)
        {
            string ext = Path.GetExtension(filename).ToLowerInvariant();

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
                case ".txt":
                    return NodeIcon.Content;

                case ".xml":
                case ".xamlcfg":
                    return NodeIcon.XmlFile;

                case ".bmp":
                case ".gif":
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return NodeIcon.ImageFile;

                case ".content":
                case ".sitemap":
                    return NodeIcon.ContentLayout;

                case ".items":
                    return NodeIcon.ResourceItemFile;

                case ".snippets":
                    return NodeIcon.CodeSnippets;

                case ".tokens":
                    return NodeIcon.TokenFile;

                default:
                    break;
            }

            return NodeIcon.None;
        }

        /// <summary>
        /// Insert the node into the collection in the correct position based on its name
        /// </summary>
        /// <param name="nodes">The node collection</param>
        /// <param name="newNode">The new node</param>
        private static void AddNode(TreeNodeCollection nodes, TreeNode newNode)
        {
            NodeData compareData, newNodeData = (NodeData)newNode.Tag;
            int idx;

            for(idx = 0; idx < nodes.Count; idx++)
            {
                compareData = (NodeData)nodes[idx].Tag;

                if(compareData.BuildAction > BuildAction.Folder)
                    continue;

                if(compareData.BuildAction == BuildAction.Folder && newNodeData.BuildAction != BuildAction.Folder)
                    continue;

                if(compareData.BuildAction != BuildAction.Folder && newNodeData.BuildAction == BuildAction.Folder)
                    break;

                if(String.Compare(nodes[idx].Text, newNode.Text, StringComparison.OrdinalIgnoreCase) > 0)
                    break;
            }

            if(idx < nodes.Count)
                nodes.Insert(idx, newNode);
            else
                nodes.Add(newNode);
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Load the tree with the folder and file items
        /// </summary>
        /// <param name="files">The folder and file build items from the project</param>
        public void LoadTree(IList<FileItem> files)
        {
            SortedDictionary<string, FileItem> fileItems = new SortedDictionary<string,FileItem>();
            List<string> folderNames = new List<string>(), additionalFolders = new List<string>();
            SandcastleProject project = null;
            FileItem fileItem;
            TreeNode root, itemNode;
            TreeNode[] matches;
            string name;
            string[] parts;

            if(files == null)
                throw new ArgumentNullException(nameof(files));

            if(files.Count > 0)
                project = files[0].Project;

            try
            {
                treeView.SuspendLayout();

                // First, get a list of all folders and files in sorted order
                foreach(FileItem item in files)
                {
                    name = item.LinkPath.PersistablePath;

                    // Resolve MSBuild and environment variable references
                    if(name.IndexOf("$(", StringComparison.Ordinal) != -1 || name.IndexOf('%') != -1)
                        name = FilePath.AbsoluteToRelativePath(item.LinkPath.BasePath, item.LinkPath);

                    // Ignore duplicate items if any are found
                    if(!fileItems.ContainsKey(name))
                    {
                        fileItems.Add(name, item);

                        if(item.BuildAction != BuildAction.Folder)
                        {
                            name = Path.GetDirectoryName(name);

                            if(name.Length != 0)
                                name = FolderPath.TerminatePath(name);
                        }
                        else
                            name = FolderPath.TerminatePath(name);

                        if(folderNames.IndexOf(name) == -1)
                        {
                            folderNames.Add(name);

                            // Note all paths leading up to this item as well
                            parts = name.Split('\\');

                            name = String.Empty;

                            for(int idx = 0; idx < parts.Length - 2; idx++)
                            {
                                name += parts[idx] + @"\";

                                if(additionalFolders.IndexOf(name) == -1)
                                    additionalFolders.Add(name);
                            }
                        }
                    }
                }

                foreach(string folder in additionalFolders)
                    if(folderNames.IndexOf(folder) == -1)
                        folderNames.Add(folder);

                folderNames.Sort();

                // Create a tree node hierarchy for all folders
                foreach(string folder in folderNames)
                {
                    // Ignore the root folder
                    if(folder.Length == 0)
                        continue;

                    name = folder.Substring(0, folder.Length - 1);
                    root = treeView.Nodes[0];

                    // Ignore it if the folder is already present
                    if(treeView.Nodes.Find(folder, true).Length != 0)
                        continue;

                    if(name.IndexOf('\\') != -1)
                    {
                        matches = treeView.Nodes.Find(name.Substring(0, name.LastIndexOf('\\') + 1), true);

                        if(matches.Length == 1)
                        {
                            root = matches[0];
                            name = name.Substring(root.Name.Length);
                        }
                    }

                    fileItems.TryGetValue(folder, out fileItem);

                    // Add a folder node if one doesn't exist
                    if(fileItem == null)
                        fileItem = project.AddFolderToProject(folder);

                    itemNode = new TreeNode(name)
                    {
                        Name = folder,
                        Tag = new NodeData(BuildAction.Folder, fileItem)
                    };

                    itemNode.ImageIndex = itemNode.SelectedImageIndex = (int)NodeIcon.GeneralFolder;

                    AddNode(root.Nodes, itemNode);
                }

                // Add each file node in the appropriate folder node
                foreach(string key in fileItems.Keys)
                {
                    name = Path.GetDirectoryName(key);
                    fileItem = fileItems[key];

                    if(fileItem.BuildAction == BuildAction.Folder)
                        continue;

                    // Ignore it if the file is already present
                    if(treeView.Nodes.Find(key, true).Length != 0)
                        continue;

                    if(name.Length != 0)
                        name = FolderPath.TerminatePath(name);

                    matches = treeView.Nodes.Find(name, true);

                    if(matches.Length == 1)
                        root = matches[0];
                    else
                        root = treeView.Nodes[0];

                    itemNode = new TreeNode(Path.GetFileName(key))
                    {
                        Name = key,
                        Tag = new NodeData(fileItem.BuildAction, fileItem)
                    };

                    itemNode.ImageIndex = itemNode.SelectedImageIndex = (int)NodeIconFromFilename(key);

                    AddNode(root.Nodes, itemNode);
                }
            }
            finally
            {
                treeView.ResumeLayout();
            }
        }

        /// <summary>
        /// Refresh the path info in each child node due to a renamed parent folder node
        /// </summary>
        /// <param name="node">The node in which to refresh the children</param>
        public void RefreshPathsInChildren(TreeNode node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            foreach(TreeNode n in node.Nodes)
            {
                NodeData nodeData = (NodeData)n.Tag;
                ((FileItem)nodeData.Item).RefreshPaths();

                if(n.Nodes.Count != 0)
                    this.RefreshPathsInChildren(n);
            }
        }

        /// <summary>
        /// Remove the children of a folder node from the project
        /// </summary>
        /// <param name="node">The parent folder node</param>
        /// <param name="permanently">True to delete the items or false to just remove them from the project</param>
        public void RemoveNode(TreeNode node, bool permanently)
        {
            NodeData nodeData;
            FileItem fileItem;
            TreeNode[] children;
            string projectFolder;

            if(node == null)
                throw new ArgumentNullException(nameof(node));

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                treeView.SuspendLayout();

                // Remove any children
                if(node.Nodes.Count > 0)
                {
                    children = new TreeNode[node.Nodes.Count];
                    node.Nodes.CopyTo(children, 0);

                    foreach(TreeNode n in children)
                        this.RemoveNode(n, permanently);
                }

                // Remove the node itself
                nodeData = (NodeData)node.Tag;
                fileItem = (FileItem)nodeData.Item;
                projectFolder = Path.GetDirectoryName(fileItem.Project.Filename);
                fileItem.RemoveFromProjectFile();
                node.Remove();

                if(permanently)
                    if(fileItem.BuildAction == BuildAction.Folder)
                    {
                        // If in or below the folder to remove, get out of it
                        if(FolderPath.TerminatePath(Directory.GetCurrentDirectory()).StartsWith(
                          fileItem.IncludePath, StringComparison.OrdinalIgnoreCase))
                            Directory.SetCurrentDirectory(projectFolder);

                        if(Directory.Exists(fileItem.IncludePath))
                            Directory.Delete(fileItem.IncludePath, true);
                    }
                    else
                        if(File.Exists(fileItem.IncludePath))
                            File.Delete(fileItem.IncludePath);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                treeView.ResumeLayout();
            }
        }
        #endregion
    }
}
