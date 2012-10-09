namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class ProjectExplorerWindow
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectExplorerWindow));
            this.sbStatusBarText = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.tvProjectFiles = new System.Windows.Forms.TreeView();
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.pgProps = new SandcastleBuilder.Utils.Controls.CustomPropertyGrid();
            this.miAddDocSource = new System.Windows.Forms.ToolStripMenuItem();
            this.miRemoveDocSource = new System.Windows.Forms.ToolStripMenuItem();
            this.miAddReference = new System.Windows.Forms.ToolStripMenuItem();
            this.miRemoveReference = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miExcludeFromProject = new System.Windows.Forms.ToolStripMenuItem();
            this.miDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.miAddGacReference = new System.Windows.Forms.ToolStripMenuItem();
            this.miPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.miCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.miCut = new System.Windows.Forms.ToolStripMenuItem();
            this.miRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.miNewItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.miConceptualTemplates = new System.Windows.Forms.ToolStripMenuItem();
            this.miCustomTemplates = new System.Windows.Forms.ToolStripMenuItem();
            this.miNewFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.miAddExistingFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.miAddExistingItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miImportMediaFile = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cmsDocSource = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsReference = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsFile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.miOpenSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.cmsDocSource.SuspendLayout();
            this.cmsReference.SuspendLayout();
            this.cmsFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvProjectFiles
            // 
            this.tvProjectFiles.AllowDrop = true;
            this.tvProjectFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvProjectFiles.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvProjectFiles.HideSelection = false;
            this.tvProjectFiles.ImageIndex = 0;
            this.tvProjectFiles.ImageList = this.ilImages;
            this.tvProjectFiles.LabelEdit = true;
            this.tvProjectFiles.Location = new System.Drawing.Point(0, 0);
            this.tvProjectFiles.Name = "tvProjectFiles";
            this.tvProjectFiles.SelectedImageIndex = 0;
            this.tvProjectFiles.ShowRootLines = false;
            this.tvProjectFiles.Size = new System.Drawing.Size(371, 363);
            this.sbStatusBarText.SetStatusBarText(this.tvProjectFiles, "Project items");
            this.tvProjectFiles.TabIndex = 0;
            this.tvProjectFiles.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvProjectFiles_NodeMouseDoubleClick);
            this.tvProjectFiles.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvProjectFiles_AfterLabelEdit);
            this.tvProjectFiles.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvProjectFiles_BeforeCollapse);
            this.tvProjectFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvProjectFiles_DragDrop);
            this.tvProjectFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvProjectFiles_AfterSelect);
            this.tvProjectFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvProjectFiles_MouseDown);
            this.tvProjectFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvProjectFiles_DragOver);
            this.tvProjectFiles.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvProjectFiles_BeforeLabelEdit);
            this.tvProjectFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvProjectFiles_KeyDown);
            this.tvProjectFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvProjectFiles_ItemDrag);
            this.tvProjectFiles.DragOver += new System.Windows.Forms.DragEventHandler(this.tvProjectFiles_DragOver);
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Magenta;
            this.ilImages.Images.SetKeyName(0, "Document.bmp");
            this.ilImages.Images.SetKeyName(1, "SandcastleBuilder.ico");
            this.ilImages.Images.SetKeyName(2, "DocSourcesFolder.bmp");
            this.ilImages.Images.SetKeyName(3, "ReferencesFolder.bmp");
            this.ilImages.Images.SetKeyName(4, "Folder.bmp");
            this.ilImages.Images.SetKeyName(5, "DocSource.png");
            this.ilImages.Images.SetKeyName(6, "ReferenceItem.bmp");
            this.ilImages.Images.SetKeyName(7, "ImageFile.bmp");
            this.ilImages.Images.SetKeyName(8, "SnippetsFile.bmp");
            this.ilImages.Images.SetKeyName(9, "SiteMap.png");
            this.ilImages.Images.SetKeyName(10, "TokenFile.bmp");
            this.ilImages.Images.SetKeyName(11, "Xsl.bmp");
            this.ilImages.Images.SetKeyName(12, "Xml.bmp");
            this.ilImages.Images.SetKeyName(13, "Html.bmp");
            this.ilImages.Images.SetKeyName(14, "ResourceItemFile.ico");
            // 
            // pgProps
            // 
            this.pgProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgProps.Location = new System.Drawing.Point(0, 0);
            this.pgProps.Name = "pgProps";
            this.pgProps.Size = new System.Drawing.Size(371, 192);
            this.sbStatusBarText.SetStatusBarText(this.pgProps, "Item properties");
            this.pgProps.TabIndex = 0;
            this.pgProps.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgProps_PropertyValueChanged);
            // 
            // miAddDocSource
            // 
            this.miAddDocSource.Name = "miAddDocSource";
            this.miAddDocSource.Size = new System.Drawing.Size(283, 22);
            this.sbStatusBarText.SetStatusBarText(this.miAddDocSource, "Add a new documentation source");
            this.miAddDocSource.Text = "&Add Documentation Source...";
            this.miAddDocSource.Click += new System.EventHandler(this.miAddDocSource_Click);
            // 
            // miRemoveDocSource
            // 
            this.miRemoveDocSource.Image = global::SandcastleBuilder.Gui.Properties.Resources.Delete;
            this.miRemoveDocSource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miRemoveDocSource.Name = "miRemoveDocSource";
            this.miRemoveDocSource.Size = new System.Drawing.Size(283, 22);
            this.sbStatusBarText.SetStatusBarText(this.miRemoveDocSource, "Remove the selected documentation source");
            this.miRemoveDocSource.Text = "&Remove";
            this.miRemoveDocSource.Click += new System.EventHandler(this.miRemoveDocSource_Click);
            // 
            // miAddReference
            // 
            this.miAddReference.Name = "miAddReference";
            this.miAddReference.Size = new System.Drawing.Size(278, 22);
            this.sbStatusBarText.SetStatusBarText(this.miAddReference, "Add a new reference (dependency)");
            this.miAddReference.Text = "&Add File/Project Reference...";
            this.miAddReference.Click += new System.EventHandler(this.miAddReference_Click);
            // 
            // miRemoveReference
            // 
            this.miRemoveReference.Image = global::SandcastleBuilder.Gui.Properties.Resources.Delete;
            this.miRemoveReference.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miRemoveReference.Name = "miRemoveReference";
            this.miRemoveReference.Size = new System.Drawing.Size(278, 22);
            this.sbStatusBarText.SetStatusBarText(this.miRemoveReference, "Remove the selected reference");
            this.miRemoveReference.Text = "&Remove";
            this.miRemoveReference.Click += new System.EventHandler(this.miRemoveReference_Click);
            // 
            // miOpen
            // 
            this.miOpen.Name = "miOpen";
            this.miOpen.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miOpen, "Open the file for editing");
            this.miOpen.Text = "&Open";
            this.miOpen.Click += new System.EventHandler(this.miOpen_Click);
            // 
            // miExcludeFromProject
            // 
            this.miExcludeFromProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.Delete;
            this.miExcludeFromProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miExcludeFromProject.Name = "miExcludeFromProject";
            this.miExcludeFromProject.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miExcludeFromProject, "Exclude the selected item from the project");
            this.miExcludeFromProject.Text = "&Exclude from Project";
            this.miExcludeFromProject.Click += new System.EventHandler(this.miExcludeFromProject_Click);
            // 
            // miDelete
            // 
            this.miDelete.Image = global::SandcastleBuilder.Gui.Properties.Resources.Delete;
            this.miDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miDelete.Name = "miDelete";
            this.miDelete.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miDelete, "Delete the selected item");
            this.miDelete.Text = "&Delete";
            this.miDelete.Click += new System.EventHandler(this.miDelete_Click);
            // 
            // miAddGacReference
            // 
            this.miAddGacReference.Name = "miAddGacReference";
            this.miAddGacReference.Size = new System.Drawing.Size(278, 22);
            this.sbStatusBarText.SetStatusBarText(this.miAddGacReference, "Add a reference from the Global Assembly Cache");
            this.miAddGacReference.Text = "Add &GAC Reference...";
            this.miAddGacReference.Click += new System.EventHandler(this.miAddGacReference_Click);
            // 
            // miPaste
            // 
            this.miPaste.Image = global::SandcastleBuilder.Gui.Properties.Resources.Paste;
            this.miPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miPaste.Name = "miPaste";
            this.miPaste.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miPaste, "Paste the item in the clipboard into the project");
            this.miPaste.Text = "P&aste";
            this.miPaste.Click += new System.EventHandler(this.miPaste_Click);
            // 
            // miCopy
            // 
            this.miCopy.Image = global::SandcastleBuilder.Gui.Properties.Resources.Copy;
            this.miCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCopy.Name = "miCopy";
            this.miCopy.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCopy, "Copy the selected item to the clipboard");
            this.miCopy.Text = "Co&py";
            this.miCopy.Click += new System.EventHandler(this.miCutCopy_Click);
            // 
            // miCut
            // 
            this.miCut.Image = global::SandcastleBuilder.Gui.Properties.Resources.Cut;
            this.miCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCut.Name = "miCut";
            this.miCut.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCut, "Cut the selected item to the clipboard");
            this.miCut.Text = "&Cut";
            this.miCut.Click += new System.EventHandler(this.miCutCopy_Click);
            // 
            // miRename
            // 
            this.miRename.Name = "miRename";
            this.miRename.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.miRename, "Rename the selected item");
            this.miRename.Text = "&Rename";
            this.miRename.Click += new System.EventHandler(this.miRename_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNewItem,
            this.miNewFolder,
            this.miAddExistingFolder,
            this.toolStripSeparator2,
            this.miAddExistingItem,
            this.miImportMediaFile});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(226, 22);
            this.sbStatusBarText.SetStatusBarText(this.toolStripMenuItem1, "Add new/existing item");
            this.toolStripMenuItem1.Text = "&Add";
            // 
            // miNewItem
            // 
            this.miNewItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator5,
            this.miConceptualTemplates,
            this.miCustomTemplates});
            this.miNewItem.Name = "miNewItem";
            this.miNewItem.Size = new System.Drawing.Size(311, 22);
            this.sbStatusBarText.SetStatusBarText(this.miNewItem, "Add a new project item from a template");
            this.miNewItem.Text = "New Item";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(215, 6);
            // 
            // miConceptualTemplates
            // 
            this.miConceptualTemplates.Name = "miConceptualTemplates";
            this.miConceptualTemplates.Size = new System.Drawing.Size(218, 22);
            this.sbStatusBarText.SetStatusBarText(this.miConceptualTemplates, "Add a conceptual content topic");
            this.miConceptualTemplates.Text = "Conceptual Content";
            // 
            // miCustomTemplates
            // 
            this.miCustomTemplates.Name = "miCustomTemplates";
            this.miCustomTemplates.Size = new System.Drawing.Size(218, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCustomTemplates, "Add a user-defined template item");
            this.miCustomTemplates.Text = "Custom Templates";
            // 
            // miNewFolder
            // 
            this.miNewFolder.Name = "miNewFolder";
            this.miNewFolder.Size = new System.Drawing.Size(311, 22);
            this.sbStatusBarText.SetStatusBarText(this.miNewFolder, "Add a new folder to the project");
            this.miNewFolder.Text = "New &Folder";
            this.miNewFolder.Click += new System.EventHandler(this.miNewFolder_Click);
            // 
            // miAddExistingFolder
            // 
            this.miAddExistingFolder.Name = "miAddExistingFolder";
            this.miAddExistingFolder.Size = new System.Drawing.Size(311, 22);
            this.sbStatusBarText.SetStatusBarText(this.miAddExistingFolder, "Add an existing folder to the project");
            this.miAddExistingFolder.Text = "&Existing Folder...";
            this.miAddExistingFolder.Click += new System.EventHandler(this.miAddExistingFolder_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(308, 6);
            // 
            // miAddExistingItem
            // 
            this.miAddExistingItem.Name = "miAddExistingItem";
            this.miAddExistingItem.Size = new System.Drawing.Size(311, 22);
            this.sbStatusBarText.SetStatusBarText(this.miAddExistingItem, "Add an existing item");
            this.miAddExistingItem.Text = "Existing &Item...";
            this.miAddExistingItem.Click += new System.EventHandler(this.miAddExistingItem_Click);
            // 
            // miImportMediaFile
            // 
            this.miImportMediaFile.Name = "miImportMediaFile";
            this.miImportMediaFile.Size = new System.Drawing.Size(311, 22);
            this.sbStatusBarText.SetStatusBarText(this.miImportMediaFile, "Import image file information from a media content file");
            this.miImportMediaFile.Text = "Import from Media Content File...";
            this.miImportMediaFile.Click += new System.EventHandler(this.miImportMediaFile_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvProjectFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pgProps);
            this.splitContainer1.Size = new System.Drawing.Size(371, 565);
            this.splitContainer1.SplitterDistance = 363;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 2;
            // 
            // cmsDocSource
            // 
            this.cmsDocSource.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAddDocSource,
            this.miRemoveDocSource});
            this.cmsDocSource.Name = "cmsDocSource";
            this.cmsDocSource.Size = new System.Drawing.Size(284, 48);
            this.cmsDocSource.Opening += new System.ComponentModel.CancelEventHandler(this.cmsDocSource_Opening);
            // 
            // cmsReference
            // 
            this.cmsReference.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAddReference,
            this.miAddGacReference,
            this.miRemoveReference});
            this.cmsReference.Name = "cmsReference";
            this.cmsReference.Size = new System.Drawing.Size(279, 70);
            this.cmsReference.Opening += new System.ComponentModel.CancelEventHandler(this.cmsReference_Opening);
            // 
            // cmsFile
            // 
            this.cmsFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripSeparator4,
            this.miOpen,
            this.miOpenSeparator,
            this.miCut,
            this.miCopy,
            this.miPaste,
            this.toolStripSeparator1,
            this.miExcludeFromProject,
            this.miDelete,
            this.toolStripSeparator3,
            this.miRename});
            this.cmsFile.Name = "cmsFile";
            this.cmsFile.Size = new System.Drawing.Size(227, 204);
            this.cmsFile.Opening += new System.ComponentModel.CancelEventHandler(this.cmsFile_Opening);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(223, 6);
            // 
            // miOpenSeparator
            // 
            this.miOpenSeparator.Name = "miOpenSeparator";
            this.miOpenSeparator.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(223, 6);
            // 
            // ProjectExplorerWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(371, 565);
            this.Controls.Add(this.splitContainer1);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "ProjectExplorerWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            this.ShowInTaskbar = false;
            this.TabText = "Project Explorer";
            this.Text = "Project Explorer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.cmsDocSource.ResumeLayout(false);
            this.cmsReference.ResumeLayout(false);
            this.cmsFile.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbStatusBarText;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvProjectFiles;
        private SandcastleBuilder.Utils.Controls.CustomPropertyGrid pgProps;
        private System.Windows.Forms.ContextMenuStrip cmsDocSource;
        private System.Windows.Forms.ToolStripMenuItem miAddDocSource;
        private System.Windows.Forms.ToolStripMenuItem miRemoveDocSource;
        private System.Windows.Forms.ContextMenuStrip cmsReference;
        private System.Windows.Forms.ToolStripMenuItem miAddReference;
        private System.Windows.Forms.ToolStripMenuItem miRemoveReference;
        private System.Windows.Forms.ContextMenuStrip cmsFile;
        private System.Windows.Forms.ToolStripMenuItem miOpen;
        private System.Windows.Forms.ToolStripMenuItem miExcludeFromProject;
        private System.Windows.Forms.ToolStripMenuItem miDelete;
        private System.Windows.Forms.ToolStripMenuItem miAddGacReference;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.ToolStripSeparator miOpenSeparator;
        private System.Windows.Forms.ToolStripMenuItem miCut;
        private System.Windows.Forms.ToolStripMenuItem miCopy;
        private System.Windows.Forms.ToolStripMenuItem miPaste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem miRename;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem miNewFolder;
        private System.Windows.Forms.ToolStripMenuItem miAddExistingFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem miAddExistingItem;
        private System.Windows.Forms.ToolStripMenuItem miImportMediaFile;
        private System.Windows.Forms.ToolStripMenuItem miNewItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem miConceptualTemplates;
        private System.Windows.Forms.ToolStripMenuItem miCustomTemplates;
    }
}
