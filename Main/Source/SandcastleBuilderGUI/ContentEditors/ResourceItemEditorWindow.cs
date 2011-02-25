//=============================================================================
// System  : Sandcastle Help File Builder
// File    : ResourceItemEditorWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/06/2009
// Note    : Copyright 2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the resource item files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  12/04/2009  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to edit a resource item file.
    /// </summary>
    public partial class ResourceItemEditorWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private FileItem itemsFile;
        private SortedDictionary<string, ResourceItem> allItems, sandcastleItems;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileItem">The project file item to edit</param>
        public ResourceItemEditorWindow(FileItem fileItem)
        {
            InitializeComponent();
            itemsFile = fileItem;

            allItems = new SortedDictionary<string, ResourceItem>();
            sandcastleItems = new SortedDictionary<string, ResourceItem>();

            sbStatusBarText.InstanceStatusBar = MainForm.Host.StatusBarTextLabel;

            editor.TextEditorProperties.Font = Settings.Default.TextEditorFont;
            editor.TextEditorProperties.ShowLineNumbers = Settings.Default.ShowLineNumbers;
            editor.SetHighlighting("XML");

            this.Text = Path.GetFileName(fileItem.FullPath);
            this.ToolTipText = fileItem.FullPath;
            this.LoadResourceItems();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Update the editor font used based on the selected user settings
        /// </summary>
        public void UpdateFont()
        {
            editor.TextEditorProperties.Font = Settings.Default.TextEditorFont;
            editor.TextEditorProperties.ShowLineNumbers = Settings.Default.ShowLineNumbers;
        }

        /// <summary>
        /// Save the topic to a new filename
        /// </summary>
        /// <param name="filename">The new filename</param>
        /// <returns>True if saved successfully, false if not</returns>
        /// <overloads>There are two overloads for this method</overloads>
        public bool Save(string filename)
        {
            string projectPath = Path.GetDirectoryName(
                itemsFile.ProjectElement.Project.Filename);

            if(!filename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The file must reside in the project folder " +
                    "or a folder below it", Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            itemsFile.Include = new FilePath(filename, itemsFile.ProjectElement.Project);
            this.Text = Path.GetFileName(filename);
            this.ToolTipText = filename;
            this.IsDirty = true;
            return this.Save();
        }

        /// <summary>
        /// Load the tree view with the items and set the form up to edit them
        /// </summary>
        private void LoadResourceItems()
        {
            SandcastleProject project = itemsFile.ProjectElement.Project;
            TreeNode node;
            string shfbStyleContent, shfbSharedContent, sharedFolder, presentationFolder;

            // Get the presentation style folders
            shfbStyleContent = Assembly.GetExecutingAssembly().Location;
            shfbStyleContent = shfbSharedContent = Path.Combine(shfbStyleContent.Substring(0,
                shfbStyleContent.LastIndexOf('\\')), "SharedContent");

            shfbStyleContent = Path.Combine(shfbStyleContent, project.PresentationStyle +
                "BuilderContent_");
            shfbSharedContent = Path.Combine(shfbSharedContent, "SharedBuilderContent_");

            if(!String.IsNullOrEmpty(project.SandcastlePath))
                presentationFolder = Path.Combine(project.SandcastlePath, "Presentation");
            else
                presentationFolder = Path.Combine(BuildComponentManager.SandcastlePath, "Presentation");

            sharedFolder = Path.Combine(presentationFolder, @"Shared\Content");
            presentationFolder = Path.Combine(presentationFolder,
                project.PresentationStyle + @"\Content");

            // Use the language-specific files if they are present
            if(Directory.Exists(Path.Combine(sharedFolder, project.Language.Name)))
                sharedFolder = Path.Combine(sharedFolder, project.Language.Name);

            if(Directory.Exists(Path.Combine(presentationFolder, project.Language.Name)))
                presentationFolder = Path.Combine(presentationFolder, project.Language.Name);

            if(File.Exists(Path.Combine(shfbStyleContent, project.Language.Name + ".xml")))
                shfbStyleContent = shfbStyleContent + project.Language.Name + ".xml";
            else
                shfbStyleContent = shfbStyleContent + "en-US.xml";

            if(File.Exists(Path.Combine(shfbSharedContent, project.Language.Name + ".xml")))
                shfbSharedContent = shfbSharedContent + project.Language.Name + ".xml";
            else
                shfbSharedContent = shfbSharedContent + "en-US.xml";

            // Load the sandcastle and SHFB content files in the order in the
            // configuration files:
            //      shared_content.xml
            //      reference_content.xml
            //      syntax_content.xml
            //      feedback_content.xml
            //      conceptual_content.xml
            //      SharedBuilderContent.xml
            //      PresentationStyleBuilderContent.xml
            foreach(string file in new string[] { "shared_content.xml",
              "reference_content.xml", "syntax_content.xml",
              "feedback_content.xml", "conceptual_content.xml" })
            {
                if(File.Exists(Path.Combine(presentationFolder, file)))
                    this.LoadItemFile(Path.Combine(presentationFolder, file), false);
                else
                    if(File.Exists(Path.Combine(sharedFolder, file)))
                        this.LoadItemFile(Path.Combine(sharedFolder, file), false);
            }

            if(File.Exists(shfbSharedContent))
                this.LoadItemFile(shfbSharedContent, false);

            if(File.Exists(shfbStyleContent))
                this.LoadItemFile(shfbStyleContent, false);

            // Load the user's file with their overrides
            this.LoadItemFile(itemsFile.FullPath, true);

            // Load everything into the tree view control
            foreach(ResourceItem r in allItems.Values)
            {
                node = tvResourceItems.Nodes.Add(r.Id);
                node.Tag = r;

                if(r.IsOverridden)
                    node.BackColor = Color.LightBlue;
            }

            if(tvResourceItems.Nodes.Count != 0)
                tvResourceItems.SelectedNode = tvResourceItems.Nodes[0];
            else
                txtId.Enabled = editor.Enabled = btnRevert.Enabled = false;
        }

        /// <summary>
        /// This is used to load a resource item file's content into the
        /// dictionaries used by the editor.
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="containsOverrides">True if this file contains overrides
        /// for the Sandcastle items</param>
        private void LoadItemFile(string filename, bool containsOverrides)
        {
            ResourceItem r;
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader xr = null;

            try
            {
                settings.CloseInput = true;

                xr = XmlReader.Create(filename, settings);
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element && xr.Name == "item")
                    {
                        r = new ResourceItem(filename, xr.GetAttribute("id"),
                            xr.ReadInnerXml(), containsOverrides);

                        allItems[r.Id] = r;

                        // Create a clone of the original for Sandcastle items
                        if(!containsOverrides)
                        {
                            r = new ResourceItem(filename, r.Id, r.Value, false);
                            sandcastleItems[r.Id] = r;
                        }
                    }

                    xr.Read();
                }
            }
            finally
            {
                if(xr != null)
                    xr.Close();
            }
        }

        /// <summary>
        /// Save the modified resource items to the project's resource item file
        /// </summary>
        private void SaveResourceItems()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(itemsFile.FullPath, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("content");
                writer.WriteAttributeString("xml", "space", null, "preserve");

                foreach(ResourceItem r in allItems.Values)
                    if(r.IsOverridden)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("id", r.Id);

                        // The value is written as raw text to preserve any XML
                        // within it.  The item value is also trimmed to remove
                        // unnecessary whitespace that might affect the layout.
                        writer.WriteRaw(r.Value.Trim());
                        writer.WriteEndElement();
                    }

                writer.WriteEndElement();   // </content>
                writer.WriteEndDocument();
            }
            finally
            {
                if(writer != null)
                    writer.Close();
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override bool CanClose
        {
            get
            {
                tvResourceItems_BeforeSelect(tvResourceItems, new TreeViewCancelEventArgs(
                    tvResourceItems.SelectedNode, false, TreeViewAction.Unknown));

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

                tvResourceItems_BeforeSelect(tvResourceItems, new TreeViewCancelEventArgs(
                    tvResourceItems.SelectedNode, false, TreeViewAction.Unknown));

                if(this.IsDirty)
                {
                    this.SaveResourceItems();
                    this.Text = Path.GetFileName(this.ToolTipText);
                    this.IsDirty = false;
                }

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Unable to save file.  Reason: " + ex.Message,
                    "Resource Item File Editor", MessageBoxButtons.OK,
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
                dlg.Title = "Save Resource Item File As";
                dlg.Filter = "Resource item files (*.items)|*.items|" +
                    "All Files (*.*)|*.*";
                dlg.DefaultExt = Path.GetExtension(this.ToolTipText);
                dlg.InitialDirectory = Path.GetDirectoryName(this.ToolTipText);

                if(dlg.ShowDialog() == DialogResult.OK)
                    return this.Save(dlg.FileName);
            }

            return false;
        }

        /// <summary>
        /// This is overriden to prompt to save changes if necessary
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !this.CanClose;
            base.OnClosing(e);
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to prompt for save when closing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ResourceItemEditorWindow_FormClosing(object sender,
          FormClosingEventArgs e)
        {
            e.Cancel = !this.CanClose;
        }

        /// <summary>
        /// Revert the selected item to its default value from the Sandcastle
        /// resource file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRevert_Click(object sender, EventArgs e)
        {
            TreeNode node = tvResourceItems.SelectedNode;
            ResourceItem defaultItem, r = (ResourceItem)node.Tag;

            if(MessageBox.Show("Do you want to revert the resource item '" +
              r.Id + "' to its default value?", Constants.AppName, MessageBoxButtons.YesNo,
              MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) ==
              DialogResult.Yes)
            {
                if(sandcastleItems.TryGetValue(r.Id, out defaultItem))
                {
                    r.SourceFile = defaultItem.SourceFile;
                    r.Value = defaultItem.Value;
                    r.IsOverridden = false;

                    node.BackColor = tvResourceItems.BackColor;

                    txtFilename.Text = r.SourceFile;
                    editor.Text = r.Value;
                    editor.Refresh();
                }

                if(!this.IsDirty)
                {
                    this.IsDirty = true;
                    this.Text += "*";
                }

                tvResourceItems.Focus();
            }
        }
        #endregion

        #region Tree view event handlers
        //=====================================================================

        /// <summary>
        /// This updates the resource item value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvResourceItems_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = tvResourceItems.SelectedNode;

            if(node == null)
                return;

            ResourceItem r = (ResourceItem)node.Tag;

            if(editor.Text != r.Value)
            {
                // Store new value in item
                r.SourceFile = itemsFile.FullPath;
                r.Value = editor.Text;
                r.IsOverridden = true;

                node.BackColor = Color.LightBlue;

                if(!this.IsDirty)
                {
                    this.IsDirty = true;
                    this.Text += "*";
                }
            }
        }

        /// <summary>
        /// This loads the selected item ID and text into the editor fields
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvResourceItems_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ResourceItem r = (ResourceItem)e.Node.Tag;

            txtId.Text = r.Id;
            txtFilename.Text = r.SourceFile;
            editor.Text = r.Value;
            editor.Refresh();

            btnRevert.Enabled = r.IsOverridden;
        }
        #endregion
    }
}
