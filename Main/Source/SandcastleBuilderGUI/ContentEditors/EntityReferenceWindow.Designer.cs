namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class EntityReferenceWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityReferenceWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.txtFindName = new System.Windows.Forms.TextBox();
            this.sbStatusBarText = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.tvEntities = new System.Windows.Forms.TreeView();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.cboContentType = new System.Windows.Forms.ToolStripComboBox();
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.lblLoading = new System.Windows.Forms.Label();
            this.pbWait = new System.Windows.Forms.PictureBox();
            this.tsOptions = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).BeginInit();
            this.tsOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(2, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Fin&d";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtFindName
            // 
            this.txtFindName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFindName.Enabled = false;
            this.txtFindName.Location = new System.Drawing.Point(48, 28);
            this.txtFindName.MaxLength = 256;
            this.txtFindName.Name = "txtFindName";
            this.txtFindName.Size = new System.Drawing.Size(312, 22);
            this.sbStatusBarText.SetStatusBarText(this.txtFindName, "Find: Enter the ID or, for code entities, a regular expression for which to searc" +
                    "h");
            this.txtFindName.TabIndex = 1;
            this.txtFindName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFindName_KeyDown);
            // 
            // tvEntities
            // 
            this.tvEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tvEntities.Enabled = false;
            this.tvEntities.FullRowSelect = true;
            this.tvEntities.Location = new System.Drawing.Point(1, 56);
            this.tvEntities.Name = "tvEntities";
            this.tvEntities.ShowLines = false;
            this.tvEntities.ShowNodeToolTips = true;
            this.tvEntities.ShowPlusMinus = false;
            this.tvEntities.ShowRootLines = false;
            this.tvEntities.Size = new System.Drawing.Size(360, 203);
            this.sbStatusBarText.SetStatusBarText(this.tvEntities, "Entities: Drag an item and drop it in the topic");
            this.tvEntities.TabIndex = 2;
            this.tvEntities.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvEntities_NodeMouseDoubleClick);
            this.tvEntities.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvEntities_BeforeCollapse);
            this.tvEntities.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvEntities_KeyDown);
            this.tvEntities.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvEntities_ItemDrag);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRefresh.Image = global::SandcastleBuilder.Gui.Properties.Resources.Refresh;
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Size = new System.Drawing.Size(23, 22);
            this.sbStatusBarText.SetStatusBarText(this.tsbRefresh, "Refresh the content list using the current project settings");
            this.tsbRefresh.ToolTipText = "Refresh the content lists";
            this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
            // 
            // cboContentType
            // 
            this.cboContentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboContentType.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboContentType.Items.AddRange(new object[] {
            "Tokens",
            "Images",
            "Code Snippets",
            "Code Entities"});
            this.cboContentType.Name = "cboContentType";
            this.cboContentType.Size = new System.Drawing.Size(121, 25);
            this.sbStatusBarText.SetStatusBarText(this.cboContentType, "Select the type of entity reference");
            this.cboContentType.SelectedIndexChanged += new System.EventHandler(this.cboContentType_SelectedIndexChanged);
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Magenta;
            this.ilImages.Images.SetKeyName(0, "TokenFile.bmp");
            this.ilImages.Images.SetKeyName(1, "ImageFile.bmp");
            this.ilImages.Images.SetKeyName(2, "SnippetsFile.bmp");
            this.ilImages.Images.SetKeyName(3, "NormalTopic.bmp");
            // 
            // lblLoading
            // 
            this.lblLoading.AutoSize = true;
            this.lblLoading.BackColor = System.Drawing.SystemColors.Window;
            this.lblLoading.Location = new System.Drawing.Point(50, 75);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(140, 17);
            this.lblLoading.TabIndex = 3;
            this.lblLoading.Text = "Indexing comments...";
            this.lblLoading.Visible = false;
            // 
            // pbWait
            // 
            this.pbWait.BackColor = System.Drawing.SystemColors.Window;
            this.pbWait.Image = ((System.Drawing.Image)(resources.GetObject("pbWait.Image")));
            this.pbWait.Location = new System.Drawing.Point(12, 67);
            this.pbWait.Name = "pbWait";
            this.pbWait.Size = new System.Drawing.Size(32, 32);
            this.pbWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbWait.TabIndex = 9;
            this.pbWait.TabStop = false;
            this.pbWait.Visible = false;
            // 
            // tsOptions
            // 
            this.tsOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbRefresh,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.cboContentType});
            this.tsOptions.Location = new System.Drawing.Point(0, 0);
            this.tsOptions.Name = "tsOptions";
            this.tsOptions.Size = new System.Drawing.Size(362, 25);
            this.tsOptions.TabIndex = 4;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(40, 22);
            this.toolStripLabel1.Text = "&Type";
            // 
            // EntityReferenceWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(362, 260);
            this.Controls.Add(this.tsOptions);
            this.Controls.Add(this.lblLoading);
            this.Controls.Add(this.pbWait);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFindName);
            this.Controls.Add(this.tvEntities);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EntityReferenceWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            this.TabText = "Entity References";
            this.Text = "Entity References";
            this.VisibleChanged += new System.EventHandler(this.EntityReferenceWindow_VisibleChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EntityReferenceWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).EndInit();
            this.tsOptions.ResumeLayout(false);
            this.tsOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFindName;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbStatusBarText;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.PictureBox pbWait;
        private System.Windows.Forms.TreeView tvEntities;
        private System.Windows.Forms.ToolStrip tsOptions;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox cboContentType;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    }
}