//=============================================================================
// System  : Sandcastle Help File Builder
// File    : EntityReferenceWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/05/2009
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to look up code entity references, code
// snippets, tokens, and images, and allows them to be dragged and dropped into
// a topic editor window.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/12/2008  EFW  Created the code
// 1.8.0.0  08/18/2008  EFW  Reworked for use with the new project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.InheritedDocumentation;
using SandcastleBuilder.Utils.MSBuild;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to look up code entity references, code snippets,
    /// tokens, and images, and allows them to be dragged and dropped into
    /// a topic editor window.
    /// </summary>
    public partial class EntityReferenceWindow : BaseContentEditor
    {
        #region Entity types
        //=====================================================================

        /// <summary>
        /// This defines the entity types
        /// </summary>
        private enum EntityType
        {
            Tokens,
            Images,
            CodeSnippets,
            CodeEntities
        }
        #endregion

        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject;
        private ImageReferenceCollection images;
        private FileItemCollection tokenFiles, codeSnippetFiles;
        private string[] codeEntities;
        private Thread indexThread;
        private static object clipboard;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get { return currentProject; }
            set
            {
                currentProject = value;

                images = null;
                tokenFiles = codeSnippetFiles = null;
                codeEntities = null;

                tvEntities.Nodes.Clear();
                txtFindName.Enabled = tvEntities.Enabled = false;

                if(indexThread != null)
                {
                    indexThread.Abort();

                    while(indexThread != null && !indexThread.Join(1000))
                        Application.DoEvents();

                    indexThread = null;
                }

                if(currentProject != null && this.Visible)
                    this.cboContentType_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="statusLabel">The status text label control</param>
        public EntityReferenceWindow(ToolStripStatusLabel statusLabel)
        {
            InitializeComponent();
            sbStatusBarText.InstanceStatusBar = statusLabel;
            cboContentType.SelectedIndex = 0;
        }
        #endregion

        #region Comment indexing thread methods
        //=====================================================================

        private delegate void IndexingCompleted(IndexedCommentsCache cache);
        private delegate void IndexingFailed(string message);

        /// <summary>
        /// This is the thread method that indexes the comments files
        /// </summary>
        /// <remarks>Rather than a partial build, we'll just index the
        /// comments files.</remarks>
        private void IndexComments()
        {
            HashSet<string> projectDictionary = new HashSet<string>();
            Collection<string> frameworkLocations = new Collection<string>();
            Dictionary<string, string> cacheName = new Dictionary<string,string>();
            IndexedCommentsCache cache = new IndexedCommentsCache(100);
            MSBuildProject projRef;
            string path, lastSolution = null;

            try
            {
                BuildProcess.GetFrameworkCommentsFiles(frameworkLocations,
                    cacheName, currentProject.Language,
                    currentProject.FrameworkVersion);
                    
                // Index the framework comments
                foreach(string location in frameworkLocations)
                {
                    path = Environment.ExpandEnvironmentVariables(location);
                    cache.IndexCommentsFiles(path, null, true, null);
                }

                // Index the comments file documentation sources
                foreach(string file in currentProject.DocumentationSources.CommentsFiles)
                    cache.IndexCommentsFiles(Path.GetDirectoryName(file),
                        Path.GetFileName(file), false, null);

                // Also, index the comments files in project documentation sources
                foreach(DocumentationSource ds in currentProject.DocumentationSources)
                    foreach(var sourceProject in DocumentationSource.Projects(ds.SourceFile, ds.IncludeSubFolders,
                      !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : currentProject.Configuration,
                      !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : currentProject.Platform))
                    {
                        // NOTE: This code should be similar to the code in BuildProcess.ValidateDocumentationSources!

                        // Solutions are followed by the projects that they contain
                        if(sourceProject.ProjectFileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                        {
                            lastSolution = sourceProject.ProjectFileName;
                            continue;
                        }

                        // Ignore projects that we've already seen
                        if(projectDictionary.Add(sourceProject.ProjectFileName))
                        {
                            projRef = new MSBuildProject(sourceProject.ProjectFileName);

                            // Use the project file configuration and platform properties if they are set.  If not,
                            // use the documentation source values.  If they are not set, use the SHFB project settings.
                            projRef.SetConfiguration(
                                !String.IsNullOrEmpty(sourceProject.Configuration) ? sourceProject.Configuration :
                                    !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : currentProject.Configuration,
                                !String.IsNullOrEmpty(sourceProject.Platform) ? sourceProject.Platform :
                                    !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : currentProject.Platform,
                                currentProject.MSBuildOutDir);

                            // Add Visual Studio solution macros if necessary
                            if(lastSolution != null)
                                projRef.SetSolutionMacros(lastSolution);

                            if(!String.IsNullOrEmpty(projRef.XmlCommentsFile))
                                cache.IndexCommentsFiles(Path.GetDirectoryName(projRef.XmlCommentsFile),
                                    Path.GetFileName(projRef.XmlCommentsFile), false, null);
                        }
                    }

                this.Invoke(new IndexingCompleted(this.Completed), new object[] { cache });
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                this.Invoke(new IndexingFailed(this.Failed), new object[] { ex.Message });
            }
        }

        /// <summary>
        /// This is called if indexing completes successfully
        /// </summary>
        private void Completed(IndexedCommentsCache cache)
        {
            indexThread = null;
            pbWait.Visible = lblLoading.Visible = false;
            tvEntities.Enabled = true;
            codeEntities = cache.GetKeys();

            if(cboContentType.SelectedIndex == (int)EntityType.CodeEntities)
            {
                txtFindName.Enabled = true;
                txtFindName.ReadOnly = false;
            }
        }

        /// <summary>
        /// This is called if indexing fails
        /// </summary>
        private void Failed(string message)
        {
            indexThread = null;
            pbWait.Visible = lblLoading.Visible = false;
            txtFindName.Text = "Indexing failed.  Reason: " + message;
            txtFindName.Enabled = txtFindName.ReadOnly = true;
        }
        #endregion

        #region Load image information
        //=====================================================================

        /// <summary>
        /// This loads the tree view with image file entries from the project
        /// </summary>
        private void LoadImageInfo()
        {
            TreeNode node;

            tvEntities.ImageList = ilImages;
            tvEntities.Nodes.Clear();

            if(images == null)
                images = new ImageReferenceCollection(currentProject);

            foreach(ImageReference ir in images)
                if(!String.IsNullOrEmpty(ir.Id))
                {
                    node = tvEntities.Nodes.Add(ir.DisplayTitle);
                    node.Name = ir.Id;
                    node.Tag = ir;
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)EntityType.Images;
                }

            txtFindName.Enabled = true;
            tvEntities.Enabled = true;
        }
        #endregion

        #region Load token info
        //=====================================================================

        /// <summary>
        /// This loads the tree view with token file entries from the project
        /// </summary>
        private void LoadTokenInfo()
        {
            TreeNode rootNode = null, node;
            TokenCollection tokens;

            tvEntities.ImageList = ilImages;
            tvEntities.Nodes.Clear();

            if(tokenFiles == null)
                tokenFiles = new FileItemCollection(currentProject,
                    BuildAction.Tokens);

            foreach(FileItem tokenFile in tokenFiles)
                try
                {
                    if(File.Exists(tokenFile.FullPath))
                    {
                        rootNode = tvEntities.Nodes.Add(Path.GetFileName(
                            tokenFile.FullPath));
                        rootNode.ImageIndex = rootNode.SelectedImageIndex =
                            (int)EntityType.CodeEntities;

                        tokens = new TokenCollection(tokenFile);
                        tokens.Load();

                        foreach(Token t in tokens)
                        {
                            node = rootNode.Nodes.Add(t.TokenName);
                            node.Name = t.TokenName;
                            node.Tag = t;
                            node.ImageIndex = node.SelectedImageIndex =
                                (int)EntityType.Tokens;
                        }
                    }

                    rootNode = null;
                }
                catch(Exception ex)
                {
                    if(rootNode == null)
                        tvEntities.Nodes.Add("Unable to load file '" +
                            tokenFile.FullPath + "'.  Reason: " + ex.Message);
                    else
                        rootNode.Nodes.Add("Unable to load file: " + ex.Message);
                }

            txtFindName.Enabled = true;
            tvEntities.Enabled = true;
            tvEntities.ExpandAll();
        }
        #endregion

        #region Load code snippet info
        //=====================================================================

        private void LoadCodeSnippetInfo()
        {
            TreeNode rootNode = null, node;
            XPathDocument snippets;
            XPathNavigator navSnippets;
            CodeReference cr;

            tvEntities.ImageList = ilImages;
            tvEntities.Nodes.Clear();

            if(codeSnippetFiles == null)
                codeSnippetFiles = new FileItemCollection(currentProject,
                    BuildAction.CodeSnippets);

            foreach(FileItem snippetFile in codeSnippetFiles)
                try
                {
                    if(File.Exists(snippetFile.FullPath))
                    {
                        rootNode = tvEntities.Nodes.Add(Path.GetFileName(
                            snippetFile.FullPath));
                        rootNode.ImageIndex = rootNode.SelectedImageIndex =
                            (int)EntityType.CodeEntities;

                        snippets = new XPathDocument(snippetFile.FullPath);
                        navSnippets = snippets.CreateNavigator();

                        foreach(XPathNavigator nav in navSnippets.Select(
                            "examples/item/@id"))
                        {
                            cr = new CodeReference(nav.Value);
                            node = rootNode.Nodes.Add(cr.Id);
                            node.Name = cr.Id;
                            node.Tag = cr;
                            node.ImageIndex = node.SelectedImageIndex =
                                (int)EntityType.CodeSnippets;
                        }
                    }

                    rootNode = null;
                }
                catch(Exception ex)
                {
                    if(rootNode == null)
                        tvEntities.Nodes.Add("Unable to load file '" +
                            snippetFile.FullPath + "'.  Reason: " + ex.Message);
                    else
                        rootNode.Nodes.Add("Unable to load file: " + ex.Message);
                }

            txtFindName.Enabled = true;
            tvEntities.Enabled = true;
            tvEntities.ExpandAll();
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This loads the content when the window is first made visible
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void EntityReferenceWindow_VisibleChanged(object sender,
          EventArgs e)
        {
            bool loadInfo = false;

            if(this.Visible && currentProject != null)
            {
                switch((EntityType)cboContentType.SelectedIndex)
                {
                    case EntityType.Tokens:
                        loadInfo = (tokenFiles == null);
                        break;

                    case EntityType.Images:
                        loadInfo = (images == null);
                        break;

                    case EntityType.CodeSnippets:
                        loadInfo = (codeSnippetFiles == null);
                        break;

                    default:
                        loadInfo = (codeEntities == null);
                        break;
                }

                if(loadInfo)
                    this.cboContentType_SelectedIndexChanged(sender, e);
            }
        }

        /// <summary>
        /// This kills the build thread if it is still running when closed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void EntityReferenceWindow_FormClosing(object sender,
          FormClosingEventArgs e)
        {
            if(indexThread == null)
                return;

            clipboard = null;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if(indexThread != null)
                {
                    indexThread.Abort();

                    while(indexThread != null && !indexThread.Join(1000))
                        Application.DoEvents();
                }

                System.Diagnostics.Debug.WriteLine("Thread stopped");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                indexThread = null;
            }
        }

        /// <summary>
        /// Find all code entities matching the entered text when Enter is hit
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFindName_KeyDown(object sender, KeyEventArgs e)
        {
            TreeNode node;
            TreeNode[] matches;

            if(e.KeyCode != Keys.Enter || txtFindName.Text.Length == 0)
                return;

            switch((EntityType)cboContentType.SelectedIndex)
            {
                case EntityType.Tokens:
                case EntityType.Images:
                case EntityType.CodeSnippets:
                    matches = tvEntities.Nodes.Find(txtFindName.Text.Trim(), true);

                    if(matches.Length > 0)
                    {
                        e.SuppressKeyPress = e.Handled = true;
                        tvEntities.SelectedNode = matches[0];
                        tvEntities.Focus();
                    }
                    else
                        tvEntities.SelectedNode = null;
                    return;

                default:    // Code entities searches are handled below
                    break;
            }

            tvEntities.Nodes.Clear();

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Regex reSearch = new Regex(txtFindName.Text, RegexOptions.IgnoreCase);
                List<string> found = new List<string>();

                foreach(string key in codeEntities)
                    if(reSearch.IsMatch(key))
                    {
                        found.Add(key);

                        // Limit it to 1000 matches
                        if(found.Count == 1000)
                            break;
                    }

                if(found.Count != 0)
                {
                    found.Sort(delegate(string x, string y)
                      {
                          return String.Compare(x, y,
                              StringComparison.CurrentCulture);
                      });

                    foreach(string member in found)
                    {
                        node = tvEntities.Nodes.Add(member);
                        node.Tag = new CodeEntityReference(member);
                    }

                    tvEntities.Enabled = true;

                    if(found.Count == 1000)
                        MessageBox.Show("Too many matches found.  Only the " +
                            "first 1000 are shown.", "Code Entity Window",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    tvEntities.Nodes.Add("No members found");
                    tvEntities.Enabled = false;
                }
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show("The search regular expression is not valid: " +
                    ex.Message, "Find Code Entity", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            e.SuppressKeyPress = e.Handled = true;
        }

        /// <summary>
        /// Prevent nodes from being collapsed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvEntities_BeforeCollapse(object sender,
          TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Allow an item to be dragged and dropped onto a topic window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvEntities_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DataObject data = new DataObject();
            TreeNode node = e.Item as TreeNode;

            if(e.Button == MouseButtons.Left && node != null && node.Tag != null)
            {
                data.SetData(node.Tag.GetType(), node.Tag);
                this.DoDragDrop(data, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// Refresh the currently displayed entity information
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            switch((EntityType)cboContentType.SelectedIndex)
            {
                case EntityType.Tokens:
                    tokenFiles = null;
                    break;

                case EntityType.Images:
                    images = null;
                    break;

                case EntityType.CodeSnippets:
                    codeSnippetFiles = null;
                    break;

                default:
                    codeEntities = null;
                    break;
            }

            tvEntities.Nodes.Clear();
            this.cboContentType_SelectedIndexChanged(sender, e);
        }

        /// <summary>
        /// Change the type of entities listed in the window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboContentType_SelectedIndexChanged(object sender,
          EventArgs e)
        {
            txtFindName.Enabled = pbWait.Visible = lblLoading.Visible = false;
            txtFindName.ReadOnly = false;
            tvEntities.Nodes.Clear();

            if(currentProject == null)
                return;

            currentProject.EnsureProjectIsCurrent(false);

            switch((EntityType)cboContentType.SelectedIndex)
            {
                case EntityType.Tokens:
                    this.LoadTokenInfo();
                    break;

                case EntityType.Images:
                    this.LoadImageInfo();
                    break;

                case EntityType.CodeSnippets:
                    this.LoadCodeSnippetInfo();
                    break;

                default:    // Code entities
                    tvEntities.ImageList = null;

                    if(codeEntities == null)
                    {
                        pbWait.Visible = lblLoading.Visible = true;
                        tvEntities.Enabled = false;

                        if(indexThread == null)
                        {
                            indexThread = new Thread(new ThreadStart(IndexComments));
                            indexThread.Name = "Comments indexing thread";
                            indexThread.IsBackground = true;
                            indexThread.Start();
                        }
                    }
                    else
                        txtFindName.Enabled = true;
                    break;
            }
        }

        /// <summary>
        /// Act like a double-click when Enter is hit on a node
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvEntities_KeyDown(object sender, KeyEventArgs e)
        {
            if(tvEntities.SelectedNode != null && e.KeyCode == Keys.Enter)
            {
                this.tvEntities_NodeMouseDoubleClick(sender,
                    new TreeNodeMouseClickEventArgs(tvEntities.SelectedNode,
                        MouseButtons.Left, 1, 0, 0));

                e.Handled = e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// If a node is double-clicked, paste it into the active editor window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvEntities_NodeMouseDoubleClick(object sender,
          TreeNodeMouseClickEventArgs e)
        {
            DataObject msg;
            string text;

            this.DockPanel.CheckFocusedContent();    // HACK
            TopicEditorWindow topic = this.DockPanel.ActiveDocument as TopicEditorWindow;

            if(e.Node != null && e.Node.Tag != null && topic != null)
            {
                msg = new DataObject();
                clipboard = null;

                switch((EntityType)cboContentType.SelectedIndex)
                {
                    case EntityType.Tokens:
                        text = ((Token)e.Node.Tag).ToToken();
                        break;

                    case EntityType.Images:
                        clipboard = e.Node.Tag;
                        text = ((ImageReference)clipboard).ToMediaLink();

                        // Add it in its native form too so that the user has
                        // a choice of styles when pasted into a topic editor.
                        msg.SetData(typeof(ClipboardDataHandler),
                            new ClipboardDataHandler(GetClipboardData));
                        break;

                    case EntityType.CodeSnippets:
                        text = ((CodeReference)e.Node.Tag).ToCodeReference();
                        break;

                    default:
                        clipboard = e.Node.Tag;
                        text = ((CodeEntityReference)clipboard).ToCodeEntityReference();

                        // Add it in its native form too so that the user has
                        // a choice of styles when pasted into a topic editor.
                        msg.SetData(typeof(ClipboardDataHandler),
                            new ClipboardDataHandler(GetClipboardData));
                        break;
                }

                msg.SetData(DataFormats.Text, text);
                Clipboard.SetDataObject(msg, false);
                topic.PasteFromClipboard();
            }
        }

        /// <summary>
        /// This is used to return the actual object to paste from the Windows
        /// clipboard
        /// </summary>
        /// <returns>The object to paste</returns>
        private static object GetClipboardData()
        {
            return clipboard;
        }
        #endregion
    }
}
