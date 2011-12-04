namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class ContentLayoutWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentLayoutWindow));
            this.tvContent = new System.Windows.Forms.TreeView();
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.miPasteAsChild = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsTopics = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miDefaultTopic = new System.Windows.Forms.ToolStripMenuItem();
            this.miMarkAsMSHVRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.miApiContent = new System.Windows.Forms.ToolStripMenuItem();
            this.miCtxInsertApiAfter = new System.Windows.Forms.ToolStripMenuItem();
            this.miCtxInsertApiBefore = new System.Windows.Forms.ToolStripMenuItem();
            this.miCtxInsertApiAsChild = new System.Windows.Forms.ToolStripMenuItem();
            this.miCtxClearInsertionPoint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.miMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.miMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.miAddSibling = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsNewSiblingTopic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miStandardSibling = new System.Windows.Forms.ToolStripMenuItem();
            this.miCustomSibling = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.addExistingTopicFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addAllTopicsInFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.miAddEmptySibling = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbAddSiblingTopic = new System.Windows.Forms.ToolStripSplitButton();
            this.miAddChild = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsNewChildTopic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miStandardChild = new System.Windows.Forms.ToolStripMenuItem();
            this.miCustomChild = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.miAddEmptyChild = new System.Windows.Forms.ToolStripMenuItem();
            this.miAssociateTopic = new System.Windows.Forms.ToolStripMenuItem();
            this.miClearTopic = new System.Windows.Forms.ToolStripMenuItem();
            this.miRefreshAssociations = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.miDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.miCut = new System.Windows.Forms.ToolStripMenuItem();
            this.miPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.miCopyAsLink = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.miEditTopic = new System.Windows.Forms.ToolStripMenuItem();
            this.miSortTopics = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.miExpandAllTopics = new System.Windows.Forms.ToolStripMenuItem();
            this.miCollapseAllTopics = new System.Windows.Forms.ToolStripMenuItem();
            this.miExpandChildTopics = new System.Windows.Forms.ToolStripMenuItem();
            this.miCollapseChildTopics = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbAddChildTopic = new System.Windows.Forms.ToolStripSplitButton();
            this.pgProps = new SandcastleBuilder.Utils.Controls.CustomPropertyGrid();
            this.tsTopics = new System.Windows.Forms.ToolStrip();
            this.tsbDefaultTopic = new System.Windows.Forms.ToolStripButton();
            this.tsbApiInsertionPoint = new System.Windows.Forms.ToolStripSplitButton();
            this.miInsertApiAfter = new System.Windows.Forms.ToolStripMenuItem();
            this.miInsertApiBefore = new System.Windows.Forms.ToolStripMenuItem();
            this.miInsertApiAsChild = new System.Windows.Forms.ToolStripMenuItem();
            this.miClearApiInsertionPoint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbMoveUp = new System.Windows.Forms.ToolStripButton();
            this.tsbMoveDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbDeleteTopic = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCut = new System.Windows.Forms.ToolStripButton();
            this.tsbPaste = new System.Windows.Forms.ToolStripSplitButton();
            this.tsmiPasteAsSibling = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiPasteAsChild = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbEditTopic = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbHelp = new System.Windows.Forms.ToolStripButton();
            this.sbStatusBarText = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.txtFindId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cmsTopics.SuspendLayout();
            this.cmsNewSiblingTopic.SuspendLayout();
            this.cmsNewChildTopic.SuspendLayout();
            this.tsTopics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvContent
            // 
            this.tvContent.AllowDrop = true;
            this.tvContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvContent.HideSelection = false;
            this.tvContent.ImageIndex = 0;
            this.tvContent.ImageList = this.ilImages;
            this.tvContent.Location = new System.Drawing.Point(0, 0);
            this.tvContent.Name = "tvContent";
            this.tvContent.SelectedImageIndex = 0;
            this.tvContent.ShowNodeToolTips = true;
            this.tvContent.Size = new System.Drawing.Size(383, 300);
            this.sbStatusBarText.SetStatusBarText(this.tvContent, "Content: Drag an item and drop it in the topic");
            this.tvContent.TabIndex = 0;
            this.tvContent.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvContent_ItemDrag);
            this.tvContent.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvContent_AfterSelect);
            this.tvContent.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvContent_NodeMouseDoubleClick);
            this.tvContent.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvContent_DragDrop);
            this.tvContent.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvContent_DragOver);
            this.tvContent.DragOver += new System.Windows.Forms.DragEventHandler(this.tvContent_DragOver);
            this.tvContent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvContent_KeyDown);
            this.tvContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvContent_MouseDown);
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Magenta;
            this.ilImages.Images.SetKeyName(0, "NormalTopic.bmp");
            this.ilImages.Images.SetKeyName(1, "DefaultTopic.bmp");
            this.ilImages.Images.SetKeyName(2, "MoveUp.bmp");
            this.ilImages.Images.SetKeyName(3, "MoveDown.bmp");
            this.ilImages.Images.SetKeyName(4, "AddRootItem.bmp");
            this.ilImages.Images.SetKeyName(5, "AddChildItem.bmp");
            this.ilImages.Images.SetKeyName(6, "Cut.bmp");
            this.ilImages.Images.SetKeyName(7, "Copy.bmp");
            this.ilImages.Images.SetKeyName(8, "Paste.bmp");
            this.ilImages.Images.SetKeyName(9, "Delete.bmp");
            this.ilImages.Images.SetKeyName(10, "InsertApiAfter.bmp");
            this.ilImages.Images.SetKeyName(11, "InsertApiBefore.bmp");
            this.ilImages.Images.SetKeyName(12, "InsertApiAsChild.bmp");
            this.ilImages.Images.SetKeyName(13, "DefaultApiAfter.bmp");
            this.ilImages.Images.SetKeyName(14, "DefaultApiBefore.bmp");
            this.ilImages.Images.SetKeyName(15, "DefaultApiAsChild.bmp");
            this.ilImages.Images.SetKeyName(16, "RootContentContainer.bmp");
            // 
            // miPasteAsChild
            // 
            this.miPasteAsChild.Name = "miPasteAsChild";
            this.miPasteAsChild.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miPasteAsChild, "Paste the topic in the clipboard as a sibling of the selected item");
            this.miPasteAsChild.Text = "&Paste as Child";
            this.miPasteAsChild.Click += new System.EventHandler(this.tsbPaste_ButtonClick);
            // 
            // cmsTopics
            // 
            this.cmsTopics.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.cmsTopics.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miDefaultTopic,
            this.miMarkAsMSHVRoot,
            this.miApiContent,
            this.toolStripMenuItem1,
            this.miMoveUp,
            this.miMoveDown,
            this.toolStripMenuItem9,
            this.miAddSibling,
            this.miAddChild,
            this.miAssociateTopic,
            this.miClearTopic,
            this.miRefreshAssociations,
            this.toolStripMenuItem4,
            this.miDelete,
            this.toolStripMenuItem7,
            this.miCut,
            this.miPaste,
            this.miPasteAsChild,
            this.miCopyAsLink,
            this.toolStripSeparator10,
            this.miEditTopic,
            this.miSortTopics,
            this.toolStripMenuItem3,
            this.toolStripSeparator11,
            this.miHelp});
            this.cmsTopics.Name = "ctxTasks";
            this.cmsTopics.Size = new System.Drawing.Size(327, 534);
            // 
            // miDefaultTopic
            // 
            this.miDefaultTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.DefaultTopic;
            this.miDefaultTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miDefaultTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miDefaultTopic.Name = "miDefaultTopic";
            this.miDefaultTopic.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miDefaultTopic, "Mark the selected topic as the default topic");
            this.miDefaultTopic.Text = "Toggle De&fault Topic";
            this.miDefaultTopic.Click += new System.EventHandler(this.tsbDefaultTopic_Click);
            // 
            // miMarkAsMSHVRoot
            // 
            this.miMarkAsMSHVRoot.Image = global::SandcastleBuilder.Gui.Properties.Resources.RootContentContainer;
            this.miMarkAsMSHVRoot.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miMarkAsMSHVRoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miMarkAsMSHVRoot.Name = "miMarkAsMSHVRoot";
            this.miMarkAsMSHVRoot.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miMarkAsMSHVRoot, "Mark the selected topic as the root content container for MS Help Viewer output");
            this.miMarkAsMSHVRoot.Text = "Toggle MS Help &Viewer Root Container";
            this.miMarkAsMSHVRoot.Click += new System.EventHandler(this.miMarkAsMSHVRoot_Click);
            // 
            // miApiContent
            // 
            this.miApiContent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCtxInsertApiAfter,
            this.miCtxInsertApiBefore,
            this.miCtxInsertApiAsChild,
            this.miCtxClearInsertionPoint});
            this.miApiContent.Name = "miApiContent";
            this.miApiContent.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miApiContent, "Specify how the API content is inserted relative to the selected topic");
            this.miApiContent.Text = "API Co&ntent Insertion Point";
            // 
            // miCtxInsertApiAfter
            // 
            this.miCtxInsertApiAfter.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiAfter;
            this.miCtxInsertApiAfter.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miCtxInsertApiAfter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCtxInsertApiAfter.Name = "miCtxInsertApiAfter";
            this.miCtxInsertApiAfter.Size = new System.Drawing.Size(358, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCtxInsertApiAfter, "Insert the API content after the selected topic");
            this.miCtxInsertApiAfter.Text = "Insert API content &after selected topic";
            this.miCtxInsertApiAfter.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miCtxInsertApiBefore
            // 
            this.miCtxInsertApiBefore.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiBefore;
            this.miCtxInsertApiBefore.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miCtxInsertApiBefore.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCtxInsertApiBefore.Name = "miCtxInsertApiBefore";
            this.miCtxInsertApiBefore.Size = new System.Drawing.Size(358, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCtxInsertApiBefore, "Insert the API content before the selected topic");
            this.miCtxInsertApiBefore.Text = "Insert API content &before selected topic";
            this.miCtxInsertApiBefore.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miCtxInsertApiAsChild
            // 
            this.miCtxInsertApiAsChild.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiAsChild;
            this.miCtxInsertApiAsChild.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miCtxInsertApiAsChild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCtxInsertApiAsChild.Name = "miCtxInsertApiAsChild";
            this.miCtxInsertApiAsChild.Size = new System.Drawing.Size(358, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCtxInsertApiAsChild, "Insert the API content as a child of the selected topic");
            this.miCtxInsertApiAsChild.Text = "Insert API content as a &child of selected topic";
            this.miCtxInsertApiAsChild.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miCtxClearInsertionPoint
            // 
            this.miCtxClearInsertionPoint.Name = "miCtxClearInsertionPoint";
            this.miCtxClearInsertionPoint.Size = new System.Drawing.Size(358, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCtxClearInsertionPoint, "Clear the API insertion point");
            this.miCtxClearInsertionPoint.Text = "Clear the API &insertion point";
            this.miCtxClearInsertionPoint.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(323, 6);
            // 
            // miMoveUp
            // 
            this.miMoveUp.Image = global::SandcastleBuilder.Gui.Properties.Resources.MoveUp;
            this.miMoveUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miMoveUp.Name = "miMoveUp";
            this.miMoveUp.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miMoveUp, "Move the selected topic up within its group");
            this.miMoveUp.Text = "Move &Up";
            this.miMoveUp.Click += new System.EventHandler(this.tsbMoveItem_Click);
            // 
            // miMoveDown
            // 
            this.miMoveDown.Image = global::SandcastleBuilder.Gui.Properties.Resources.MoveDown;
            this.miMoveDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miMoveDown.Name = "miMoveDown";
            this.miMoveDown.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miMoveDown, "Move the selected topic down within its group");
            this.miMoveDown.Text = "Move Do&wn";
            this.miMoveDown.Click += new System.EventHandler(this.tsbMoveItem_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(323, 6);
            // 
            // miAddSibling
            // 
            this.miAddSibling.DropDown = this.cmsNewSiblingTopic;
            this.miAddSibling.Image = global::SandcastleBuilder.Gui.Properties.Resources.AddRootItem;
            this.miAddSibling.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miAddSibling.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAddSibling.Name = "miAddSibling";
            this.miAddSibling.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miAddSibling, "Add a new topic as a sibling of the selected item");
            this.miAddSibling.Text = "&Add Sibling Topic";
            // 
            // cmsNewSiblingTopic
            // 
            this.cmsNewSiblingTopic.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miStandardSibling,
            this.miCustomSibling,
            this.toolStripMenuItem2,
            this.addExistingTopicFileToolStripMenuItem,
            this.addAllTopicsInFolderToolStripMenuItem,
            this.toolStripSeparator6,
            this.miAddEmptySibling});
            this.cmsNewSiblingTopic.Name = "cmsNewTopic";
            this.cmsNewSiblingTopic.Size = new System.Drawing.Size(262, 136);
            // 
            // miStandardSibling
            // 
            this.miStandardSibling.Name = "miStandardSibling";
            this.miStandardSibling.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.miStandardSibling, "Select a standard template");
            this.miStandardSibling.Text = "Standard Templates";
            // 
            // miCustomSibling
            // 
            this.miCustomSibling.Name = "miCustomSibling";
            this.miCustomSibling.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.miCustomSibling, "Select a custom template");
            this.miCustomSibling.Text = "Custom Templates";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(258, 6);
            // 
            // addExistingTopicFileToolStripMenuItem
            // 
            this.addExistingTopicFileToolStripMenuItem.Name = "addExistingTopicFileToolStripMenuItem";
            this.addExistingTopicFileToolStripMenuItem.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.addExistingTopicFileToolStripMenuItem, "Add an existing topic file");
            this.addExistingTopicFileToolStripMenuItem.Text = "&Add Existing Topic File...";
            this.addExistingTopicFileToolStripMenuItem.Click += new System.EventHandler(this.AddExistingTopicFile_Click);
            // 
            // addAllTopicsInFolderToolStripMenuItem
            // 
            this.addAllTopicsInFolderToolStripMenuItem.Name = "addAllTopicsInFolderToolStripMenuItem";
            this.addAllTopicsInFolderToolStripMenuItem.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.addAllTopicsInFolderToolStripMenuItem, "Add all topics found in a folder and its subfolders");
            this.addAllTopicsInFolderToolStripMenuItem.Text = "Add All Topics in &Folder...";
            this.addAllTopicsInFolderToolStripMenuItem.Click += new System.EventHandler(this.AddAllTopicsInFolder_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(258, 6);
            // 
            // miAddEmptySibling
            // 
            this.miAddEmptySibling.Image = global::SandcastleBuilder.Gui.Properties.Resources.AddRootItem;
            this.miAddEmptySibling.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miAddEmptySibling.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAddEmptySibling.Name = "miAddEmptySibling";
            this.miAddEmptySibling.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.miAddEmptySibling, "Add a container not associated with a topic file");
            this.miAddEmptySibling.Text = "Add &Empty Container Node";
            this.miAddEmptySibling.Click += new System.EventHandler(this.tsbAddTopic_ButtonClick);
            // 
            // tsbAddSiblingTopic
            // 
            this.tsbAddSiblingTopic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddSiblingTopic.DropDown = this.cmsNewSiblingTopic;
            this.tsbAddSiblingTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.AddRootItem;
            this.tsbAddSiblingTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAddSiblingTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddSiblingTopic.Name = "tsbAddSiblingTopic";
            this.tsbAddSiblingTopic.Size = new System.Drawing.Size(32, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbAddSiblingTopic, "Add a topic as a sibling of the selected item");
            this.tsbAddSiblingTopic.ToolTipText = "Add topic as sibling of selected item";
            this.tsbAddSiblingTopic.ButtonClick += new System.EventHandler(this.tsbAddTopic_ButtonClick);
            // 
            // miAddChild
            // 
            this.miAddChild.DropDown = this.cmsNewChildTopic;
            this.miAddChild.Image = global::SandcastleBuilder.Gui.Properties.Resources.AddChildItem;
            this.miAddChild.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miAddChild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAddChild.Name = "miAddChild";
            this.miAddChild.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miAddChild, "Add a topic as a child of the selected topic");
            this.miAddChild.Text = "Add C&hild Topic";
            // 
            // cmsNewChildTopic
            // 
            this.cmsNewChildTopic.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miStandardChild,
            this.miCustomChild,
            this.toolStripSeparator7,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripSeparator8,
            this.miAddEmptyChild});
            this.cmsNewChildTopic.Name = "cmsNewTopic";
            this.cmsNewChildTopic.OwnerItem = this.tsbAddChildTopic;
            this.cmsNewChildTopic.Size = new System.Drawing.Size(262, 136);
            // 
            // miStandardChild
            // 
            this.miStandardChild.Name = "miStandardChild";
            this.miStandardChild.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.miStandardChild, "Select a standard template");
            this.miStandardChild.Text = "Standard Templates";
            // 
            // miCustomChild
            // 
            this.miCustomChild.Name = "miCustomChild";
            this.miCustomChild.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.miCustomChild, "Select a custom template");
            this.miCustomChild.Text = "Custom Templates";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(258, 6);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.toolStripMenuItem5, "Add an existing topic file");
            this.toolStripMenuItem5.Text = "&Add Existing Topic File...";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.AddExistingTopicFile_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.toolStripMenuItem6, "Add all topics found in a folder and its subfolders");
            this.toolStripMenuItem6.Text = "Add All Topics in &Folder...";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.AddAllTopicsInFolder_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(258, 6);
            // 
            // miAddEmptyChild
            // 
            this.miAddEmptyChild.Image = global::SandcastleBuilder.Gui.Properties.Resources.AddChildItem;
            this.miAddEmptyChild.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miAddEmptyChild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAddEmptyChild.Name = "miAddEmptyChild";
            this.miAddEmptyChild.Size = new System.Drawing.Size(261, 24);
            this.sbStatusBarText.SetStatusBarText(this.miAddEmptyChild, "Add a container not associated with a topic file");
            this.miAddEmptyChild.Text = "Add &Empty Container Node";
            this.miAddEmptyChild.Click += new System.EventHandler(this.tsbAddTopic_ButtonClick);
            // 
            // miAssociateTopic
            // 
            this.miAssociateTopic.Name = "miAssociateTopic";
            this.miAssociateTopic.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miAssociateTopic, "Associate a topic file with the selected entry");
            this.miAssociateTopic.Text = "Ass&ociate Topic File...";
            this.miAssociateTopic.Click += new System.EventHandler(this.miAssociateTopic_Click);
            // 
            // miClearTopic
            // 
            this.miClearTopic.Name = "miClearTopic";
            this.miClearTopic.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miClearTopic, "Clear the topic assoicated with the selected entry");
            this.miClearTopic.Text = "Clear Topic Assoc&iation";
            this.miClearTopic.Click += new System.EventHandler(this.miClearTopic_Click);
            // 
            // miRefreshAssociations
            // 
            this.miRefreshAssociations.Name = "miRefreshAssociations";
            this.miRefreshAssociations.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miRefreshAssociations, "Refresh the file associations to reflect changes made to the project");
            this.miRefreshAssociations.Text = "&Refresh Associations";
            this.miRefreshAssociations.Click += new System.EventHandler(this.miRefreshAssociations_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(323, 6);
            // 
            // miDelete
            // 
            this.miDelete.Image = global::SandcastleBuilder.Gui.Properties.Resources.Delete;
            this.miDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miDelete.Name = "miDelete";
            this.miDelete.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miDelete, "Delete the selected topic");
            this.miDelete.Text = "&Delete";
            this.miDelete.Click += new System.EventHandler(this.tsbDeleteTopic_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(323, 6);
            // 
            // miCut
            // 
            this.miCut.Image = global::SandcastleBuilder.Gui.Properties.Resources.Cut;
            this.miCut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCut.Name = "miCut";
            this.miCut.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miCut, "Cut the selected topic to the clipboard");
            this.miCut.Text = "&Cut";
            this.miCut.Click += new System.EventHandler(this.tsbCutCopy_Click);
            // 
            // miPaste
            // 
            this.miPaste.Image = global::SandcastleBuilder.Gui.Properties.Resources.Paste;
            this.miPaste.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miPaste.Name = "miPaste";
            this.miPaste.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miPaste, "Paste the topic on the clipboard as a sibling of the selected topic");
            this.miPaste.Text = "Pa&ste as Sibling";
            this.miPaste.Click += new System.EventHandler(this.tsbPaste_ButtonClick);
            // 
            // miCopyAsLink
            // 
            this.miCopyAsLink.Name = "miCopyAsLink";
            this.miCopyAsLink.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miCopyAsLink, "Copy as a topic link");
            this.miCopyAsLink.Text = "Cop&y as Topic Link";
            this.miCopyAsLink.Click += new System.EventHandler(this.miCopyAsLink_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(323, 6);
            // 
            // miEditTopic
            // 
            this.miEditTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.PageEdit;
            this.miEditTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miEditTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miEditTopic.Name = "miEditTopic";
            this.miEditTopic.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miEditTopic, "Edit the selected topic");
            this.miEditTopic.Text = "&Edit Topic";
            this.miEditTopic.Click += new System.EventHandler(this.tsbEditTopic_Click);
            // 
            // miSortTopics
            // 
            this.miSortTopics.Name = "miSortTopics";
            this.miSortTopics.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miSortTopics, "Sort this topics and its siblings by their display title");
            this.miSortTopics.Text = "Sor&t Topics";
            this.miSortTopics.Click += new System.EventHandler(this.miSortTopics_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCollapseAllTopics,
            this.miExpandAllTopics,
            this.miCollapseChildTopics,
            this.miExpandChildTopics});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(326, 26);
            this.toolStripMenuItem3.Text = "E&xpand/Collapse";
            // 
            // miExpandAllTopics
            // 
            this.miExpandAllTopics.Name = "miExpandAllTopics";
            this.miExpandAllTopics.Size = new System.Drawing.Size(284, 22);
            this.sbStatusBarText.SetStatusBarText(this.miExpandAllTopics, "Expand all topics from the root down");
            this.miExpandAllTopics.Text = "&Expand all topics";
            this.miExpandAllTopics.Click += new System.EventHandler(this.ExpandCollapseTopics_Click);
            // 
            // miCollapseAllTopics
            // 
            this.miCollapseAllTopics.Name = "miCollapseAllTopics";
            this.miCollapseAllTopics.Size = new System.Drawing.Size(284, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCollapseAllTopics, "Collapse all topics from the root down");
            this.miCollapseAllTopics.Text = "&Collapse all topics";
            this.miCollapseAllTopics.Click += new System.EventHandler(this.ExpandCollapseTopics_Click);
            // 
            // miExpandChildTopics
            // 
            this.miExpandChildTopics.Name = "miExpandChildTopics";
            this.miExpandChildTopics.Size = new System.Drawing.Size(284, 22);
            this.sbStatusBarText.SetStatusBarText(this.miExpandChildTopics, "Expand the current topic and all child topics");
            this.miExpandChildTopics.Text = "Ex&pand topic and all child topics";
            this.miExpandChildTopics.Click += new System.EventHandler(this.ExpandCollapseTopics_Click);
            // 
            // miCollapseChildTopics
            // 
            this.miCollapseChildTopics.Name = "miCollapseChildTopics";
            this.miCollapseChildTopics.Size = new System.Drawing.Size(284, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCollapseChildTopics, "Collapse the current topic and all child topics");
            this.miCollapseChildTopics.Text = "C&ollapse topic and all child topics";
            this.miCollapseChildTopics.Click += new System.EventHandler(this.ExpandCollapseTopics_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(323, 6);
            // 
            // miHelp
            // 
            this.miHelp.Image = global::SandcastleBuilder.Gui.Properties.Resources.About;
            this.miHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(326, 26);
            this.sbStatusBarText.SetStatusBarText(this.miHelp, "View help for this form");
            this.miHelp.Text = "Help";
            this.miHelp.Click += new System.EventHandler(this.tsbHelp_Click);
            // 
            // tsbAddChildTopic
            // 
            this.tsbAddChildTopic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddChildTopic.DropDown = this.cmsNewChildTopic;
            this.tsbAddChildTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.AddChildItem;
            this.tsbAddChildTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAddChildTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddChildTopic.Name = "tsbAddChildTopic";
            this.tsbAddChildTopic.Size = new System.Drawing.Size(32, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbAddChildTopic, "Add a topic as a child of the selected item");
            this.tsbAddChildTopic.ToolTipText = "Add topic as child of selected item";
            this.tsbAddChildTopic.ButtonClick += new System.EventHandler(this.tsbAddTopic_ButtonClick);
            // 
            // pgProps
            // 
            this.pgProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgProps.Location = new System.Drawing.Point(0, 0);
            this.pgProps.Name = "pgProps";
            this.pgProps.PropertyNamePaneWidth = 150;
            this.pgProps.Size = new System.Drawing.Size(383, 295);
            this.sbStatusBarText.SetStatusBarText(this.pgProps, "Properties for the selected content item");
            this.pgProps.TabIndex = 0;
            this.pgProps.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgProps_PropertyValueChanged);
            // 
            // tsTopics
            // 
            this.tsTopics.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsTopics.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbDefaultTopic,
            this.tsbApiInsertionPoint,
            this.toolStripSeparator1,
            this.tsbMoveUp,
            this.tsbMoveDown,
            this.toolStripSeparator2,
            this.tsbAddSiblingTopic,
            this.tsbAddChildTopic,
            this.toolStripSeparator4,
            this.tsbDeleteTopic,
            this.toolStripSeparator3,
            this.tsbCut,
            this.tsbPaste,
            this.toolStripSeparator9,
            this.tsbEditTopic,
            this.toolStripSeparator5,
            this.tsbHelp});
            this.tsTopics.Location = new System.Drawing.Point(0, 0);
            this.tsTopics.Name = "tsTopics";
            this.tsTopics.Size = new System.Drawing.Size(385, 27);
            this.tsTopics.TabIndex = 3;
            // 
            // tsbDefaultTopic
            // 
            this.tsbDefaultTopic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDefaultTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.DefaultTopic;
            this.tsbDefaultTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbDefaultTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDefaultTopic.Name = "tsbDefaultTopic";
            this.tsbDefaultTopic.Size = new System.Drawing.Size(23, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbDefaultTopic, "Toggle the default topic");
            this.tsbDefaultTopic.ToolTipText = "Toggle the default topic";
            this.tsbDefaultTopic.Click += new System.EventHandler(this.tsbDefaultTopic_Click);
            // 
            // tsbApiInsertionPoint
            // 
            this.tsbApiInsertionPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbApiInsertionPoint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miInsertApiAfter,
            this.miInsertApiBefore,
            this.miInsertApiAsChild,
            this.miClearApiInsertionPoint});
            this.tsbApiInsertionPoint.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiAfter;
            this.tsbApiInsertionPoint.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbApiInsertionPoint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbApiInsertionPoint.Name = "tsbApiInsertionPoint";
            this.tsbApiInsertionPoint.Size = new System.Drawing.Size(32, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbApiInsertionPoint, "Set the API content insertion point");
            this.tsbApiInsertionPoint.ToolTipText = "Set API insertion point";
            this.tsbApiInsertionPoint.ButtonClick += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miInsertApiAfter
            // 
            this.miInsertApiAfter.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiAfter;
            this.miInsertApiAfter.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miInsertApiAfter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miInsertApiAfter.Name = "miInsertApiAfter";
            this.miInsertApiAfter.Size = new System.Drawing.Size(375, 24);
            this.sbStatusBarText.SetStatusBarText(this.miInsertApiAfter, "Insert the API content after the selected topic");
            this.miInsertApiAfter.Text = "Insert API content &after selected topic";
            this.miInsertApiAfter.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miInsertApiBefore
            // 
            this.miInsertApiBefore.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiBefore;
            this.miInsertApiBefore.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miInsertApiBefore.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miInsertApiBefore.Name = "miInsertApiBefore";
            this.miInsertApiBefore.Size = new System.Drawing.Size(375, 24);
            this.sbStatusBarText.SetStatusBarText(this.miInsertApiBefore, "Insert the API content before the selected topic");
            this.miInsertApiBefore.Text = "Insert API content &before selected topic";
            this.miInsertApiBefore.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miInsertApiAsChild
            // 
            this.miInsertApiAsChild.Image = global::SandcastleBuilder.Gui.Properties.Resources.InsertApiAsChild;
            this.miInsertApiAsChild.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miInsertApiAsChild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miInsertApiAsChild.Name = "miInsertApiAsChild";
            this.miInsertApiAsChild.Size = new System.Drawing.Size(375, 24);
            this.sbStatusBarText.SetStatusBarText(this.miInsertApiAsChild, "Insert the API content as a child of the selected topic");
            this.miInsertApiAsChild.Text = "Insert API content as a &child of selected topic";
            this.miInsertApiAsChild.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // miClearApiInsertionPoint
            // 
            this.miClearApiInsertionPoint.Name = "miClearApiInsertionPoint";
            this.miClearApiInsertionPoint.Size = new System.Drawing.Size(375, 24);
            this.sbStatusBarText.SetStatusBarText(this.miClearApiInsertionPoint, "Clear the API insertion point");
            this.miClearApiInsertionPoint.Text = "Clear the API &insertion point";
            this.miClearApiInsertionPoint.Click += new System.EventHandler(this.ApiInsertionPoint_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbMoveUp
            // 
            this.tsbMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMoveUp.Image = global::SandcastleBuilder.Gui.Properties.Resources.MoveUp;
            this.tsbMoveUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMoveUp.Name = "tsbMoveUp";
            this.tsbMoveUp.Size = new System.Drawing.Size(23, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbMoveUp, "Move the selected topic up within its group");
            this.tsbMoveUp.ToolTipText = "Move the selected topic up within its group";
            this.tsbMoveUp.Click += new System.EventHandler(this.tsbMoveItem_Click);
            // 
            // tsbMoveDown
            // 
            this.tsbMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMoveDown.Image = global::SandcastleBuilder.Gui.Properties.Resources.MoveDown;
            this.tsbMoveDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMoveDown.Name = "tsbMoveDown";
            this.tsbMoveDown.Size = new System.Drawing.Size(23, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbMoveDown, "Move the selected topic down within its group");
            this.tsbMoveDown.ToolTipText = "Move the selected topic down within its group";
            this.tsbMoveDown.Click += new System.EventHandler(this.tsbMoveItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbDeleteTopic
            // 
            this.tsbDeleteTopic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDeleteTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.Delete;
            this.tsbDeleteTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbDeleteTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDeleteTopic.Name = "tsbDeleteTopic";
            this.tsbDeleteTopic.Size = new System.Drawing.Size(23, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbDeleteTopic, "Delete the selected topic and all of its children");
            this.tsbDeleteTopic.ToolTipText = "Delete topic and all children";
            this.tsbDeleteTopic.Click += new System.EventHandler(this.tsbDeleteTopic_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbCut
            // 
            this.tsbCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCut.Image = global::SandcastleBuilder.Gui.Properties.Resources.Cut;
            this.tsbCut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCut.Name = "tsbCut";
            this.tsbCut.Size = new System.Drawing.Size(23, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbCut, "Cut the selected topic and its children to the clipboard");
            this.tsbCut.ToolTipText = "Cut topic and children to clipboard";
            this.tsbCut.Click += new System.EventHandler(this.tsbCutCopy_Click);
            // 
            // tsbPaste
            // 
            this.tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPaste.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiPasteAsSibling,
            this.tsmiPasteAsChild});
            this.tsbPaste.Image = global::SandcastleBuilder.Gui.Properties.Resources.Paste;
            this.tsbPaste.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPaste.Name = "tsbPaste";
            this.tsbPaste.Size = new System.Drawing.Size(32, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbPaste, "Paste the clipboard item as a sibling of the selected item");
            this.tsbPaste.ToolTipText = "Paste clipboard item as sibling of selected item";
            this.tsbPaste.ButtonClick += new System.EventHandler(this.tsbPaste_ButtonClick);
            // 
            // tsmiPasteAsSibling
            // 
            this.tsmiPasteAsSibling.Image = global::SandcastleBuilder.Gui.Properties.Resources.Paste;
            this.tsmiPasteAsSibling.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmiPasteAsSibling.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmiPasteAsSibling.Name = "tsmiPasteAsSibling";
            this.tsmiPasteAsSibling.Size = new System.Drawing.Size(179, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsmiPasteAsSibling, "Paste the clipboard item as a sibiling of the selected item");
            this.tsmiPasteAsSibling.Text = "Paste as sibling";
            this.tsmiPasteAsSibling.Click += new System.EventHandler(this.tsbPaste_ButtonClick);
            // 
            // tsmiPasteAsChild
            // 
            this.tsmiPasteAsChild.Name = "tsmiPasteAsChild";
            this.tsmiPasteAsChild.Size = new System.Drawing.Size(179, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsmiPasteAsChild, "Paste the clipboard item as a child of the selected item");
            this.tsmiPasteAsChild.Text = "Paste as child";
            this.tsmiPasteAsChild.Click += new System.EventHandler(this.tsbPaste_ButtonClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbEditTopic
            // 
            this.tsbEditTopic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbEditTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.PageEdit;
            this.tsbEditTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbEditTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbEditTopic.Name = "tsbEditTopic";
            this.tsbEditTopic.Size = new System.Drawing.Size(23, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbEditTopic, "Edit the selected topic");
            this.tsbEditTopic.ToolTipText = "Edit the selected topic";
            this.tsbEditTopic.Click += new System.EventHandler(this.tsbEditTopic_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbHelp
            // 
            this.tsbHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbHelp.Image = global::SandcastleBuilder.Gui.Properties.Resources.About;
            this.tsbHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbHelp.Name = "tsbHelp";
            this.tsbHelp.Size = new System.Drawing.Size(24, 24);
            this.sbStatusBarText.SetStatusBarText(this.tsbHelp, "View help for this editor");
            this.tsbHelp.ToolTipText = "View help for this editor";
            this.tsbHelp.Click += new System.EventHandler(this.tsbHelp_Click);
            // 
            // txtFindId
            // 
            this.txtFindId.AllowDrop = true;
            this.txtFindId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFindId.Location = new System.Drawing.Point(74, 28);
            this.txtFindId.MaxLength = 36;
            this.txtFindId.Name = "txtFindId";
            this.txtFindId.Size = new System.Drawing.Size(305, 22);
            this.sbStatusBarText.SetStatusBarText(this.txtFindId, "Find ID: Enter the ID of the item to find and hit Enter");
            this.txtFindId.TabIndex = 1;
            this.txtFindId.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtFindId_DragDrop);
            this.txtFindId.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtFindId_DragEnter);
            this.txtFindId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFindId_KeyDown);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(5, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find &ID";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 56);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvContent);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pgProps);
            this.splitContainer1.Size = new System.Drawing.Size(383, 603);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 2;
            // 
            // ContentLayoutWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(385, 660);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.tsTopics);
            this.Controls.Add(this.txtFindId);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "ContentLayoutWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ContentLayoutWindow_FormClosing);
            this.cmsTopics.ResumeLayout(false);
            this.cmsNewSiblingTopic.ResumeLayout(false);
            this.cmsNewChildTopic.ResumeLayout(false);
            this.tsTopics.ResumeLayout(false);
            this.tsTopics.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvContent;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbStatusBarText;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.ContextMenuStrip cmsTopics;
        private System.Windows.Forms.ToolStripMenuItem miCut;
        private System.Windows.Forms.ToolStripMenuItem miPasteAsChild;
        private System.Windows.Forms.ToolStripMenuItem miPaste;
        private System.Windows.Forms.ToolStripMenuItem miAddSibling;
        private System.Windows.Forms.ToolStripMenuItem miAddChild;
        private System.Windows.Forms.ToolStripMenuItem miDelete;
        private System.Windows.Forms.ToolStripMenuItem miMoveUp;
        private System.Windows.Forms.ToolStripMenuItem miMoveDown;
        private System.Windows.Forms.ToolStripMenuItem miDefaultTopic;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private SandcastleBuilder.Utils.Controls.CustomPropertyGrid pgProps;
        private System.Windows.Forms.ToolStripMenuItem miApiContent;
        private System.Windows.Forms.ToolStrip tsTopics;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbMoveUp;
        private System.Windows.Forms.ToolStripButton tsbMoveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSplitButton tsbAddSiblingTopic;
        private System.Windows.Forms.ToolStripSplitButton tsbAddChildTopic;
        private System.Windows.Forms.ToolStripButton tsbDeleteTopic;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbCut;
        private System.Windows.Forms.ToolStripSplitButton tsbPaste;
        private System.Windows.Forms.ToolStripMenuItem tsmiPasteAsChild;
        private System.Windows.Forms.ToolStripMenuItem tsmiPasteAsSibling;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ContextMenuStrip cmsNewSiblingTopic;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem addExistingTopicFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addAllTopicsInFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem miAddEmptySibling;
        private System.Windows.Forms.ContextMenuStrip cmsNewChildTopic;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem miAddEmptyChild;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton tsbEditTopic;
        private System.Windows.Forms.ToolStripMenuItem miStandardSibling;
        private System.Windows.Forms.ToolStripMenuItem miCustomSibling;
        private System.Windows.Forms.ToolStripMenuItem miStandardChild;
        private System.Windows.Forms.ToolStripMenuItem miCustomChild;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem miEditTopic;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFindId;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem miSortTopics;
        private System.Windows.Forms.ToolStripMenuItem miClearTopic;
        private System.Windows.Forms.ToolStripMenuItem miAssociateTopic;
        private System.Windows.Forms.ToolStripMenuItem miCopyAsLink;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton tsbHelp;
        private System.Windows.Forms.ToolStripMenuItem miRefreshAssociations;
        private System.Windows.Forms.ToolStripSplitButton tsbApiInsertionPoint;
        private System.Windows.Forms.ToolStripMenuItem miInsertApiBefore;
        private System.Windows.Forms.ToolStripMenuItem miInsertApiAfter;
        private System.Windows.Forms.ToolStripMenuItem miInsertApiAsChild;
        private System.Windows.Forms.ToolStripMenuItem miClearApiInsertionPoint;
        private System.Windows.Forms.ToolStripMenuItem miCtxInsertApiAfter;
        private System.Windows.Forms.ToolStripMenuItem miCtxInsertApiBefore;
        private System.Windows.Forms.ToolStripMenuItem miCtxInsertApiAsChild;
        private System.Windows.Forms.ToolStripMenuItem miCtxClearInsertionPoint;
        private System.Windows.Forms.ToolStripMenuItem miMarkAsMSHVRoot;
        private System.Windows.Forms.ToolStripButton tsbDefaultTopic;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem miExpandAllTopics;
        private System.Windows.Forms.ToolStripMenuItem miCollapseAllTopics;
        private System.Windows.Forms.ToolStripMenuItem miExpandChildTopics;
        private System.Windows.Forms.ToolStripMenuItem miCollapseChildTopics;
    }
}
