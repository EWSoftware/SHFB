namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class ResourceItemEditorWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceItemEditorWindow));
            this.editor = new SandcastleBuilder.Gui.ContentEditors.ContentEditorControl();
            this.cmsDropImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miMediaLink = new System.Windows.Forms.ToolStripMenuItem();
            this.miMediaLinkInline = new System.Windows.Forms.ToolStripMenuItem();
            this.miExternalLinkMedia = new System.Windows.Forms.ToolStripMenuItem();
            this.sbStatusBarText = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.tvResourceItems = new System.Windows.Forms.TreeView();
            this.txtId = new System.Windows.Forms.TextBox();
            this.btnRevert = new System.Windows.Forms.Button();
            this.txtFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.cmsDropImage.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.IsReadOnly = false;
            this.editor.Location = new System.Drawing.Point(6, 66);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(470, 259);
            this.sbStatusBarText.SetStatusBarText(this.editor, "Item Content: Edit the resource item\'s content");
            this.editor.TabIndex = 4;
            // 
            // cmsDropImage
            // 
            this.cmsDropImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miMediaLink,
            this.miMediaLinkInline,
            this.miExternalLinkMedia});
            this.cmsDropImage.Name = "cmsDropImage";
            this.cmsDropImage.ShowImageMargin = false;
            this.cmsDropImage.Size = new System.Drawing.Size(218, 76);
            // 
            // miMediaLink
            // 
            this.miMediaLink.Name = "miMediaLink";
            this.miMediaLink.Size = new System.Drawing.Size(217, 24);
            this.miMediaLink.Text = "&Insert <mediaLink>";
            // 
            // miMediaLinkInline
            // 
            this.miMediaLinkInline.Name = "miMediaLinkInline";
            this.miMediaLinkInline.Size = new System.Drawing.Size(217, 24);
            this.miMediaLinkInline.Text = "I&nsert <mediaLinkInline>";
            // 
            // miExternalLinkMedia
            // 
            this.miExternalLinkMedia.Name = "miExternalLinkMedia";
            this.miExternalLinkMedia.Size = new System.Drawing.Size(217, 24);
            this.miExternalLinkMedia.Text = "In&sert <externalLink>";
            // 
            // tvResourceItems
            // 
            this.tvResourceItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvResourceItems.FullRowSelect = true;
            this.tvResourceItems.HideSelection = false;
            this.tvResourceItems.Location = new System.Drawing.Point(0, 0);
            this.tvResourceItems.Name = "tvResourceItems";
            this.tvResourceItems.ShowLines = false;
            this.tvResourceItems.ShowRootLines = false;
            this.tvResourceItems.Size = new System.Drawing.Size(153, 328);
            this.sbStatusBarText.SetStatusBarText(this.tvResourceItems, "Resource Items: Select a resource item to edit");
            this.tvResourceItems.TabIndex = 0;
            this.tvResourceItems.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvResourceItems_AfterSelect);
            this.tvResourceItems.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvResourceItems_BeforeSelect);
            // 
            // txtId
            // 
            this.txtId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtId.Location = new System.Drawing.Point(94, 10);
            this.txtId.Name = "txtId";
            this.txtId.ReadOnly = true;
            this.txtId.Size = new System.Drawing.Size(264, 22);
            this.sbStatusBarText.SetStatusBarText(this.txtId, "Item ID: The item\'s ID");
            this.txtId.TabIndex = 1;
            // 
            // btnRevert
            // 
            this.btnRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRevert.Location = new System.Drawing.Point(12, 346);
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.Size = new System.Drawing.Size(88, 32);
            this.sbStatusBarText.SetStatusBarText(this.btnRevert, "Revert: Revert the item value to its default value");
            this.btnRevert.TabIndex = 1;
            this.btnRevert.Text = "&Revert";
            this.toolTip1.SetToolTip(this.btnRevert, "Revert item value to its default");
            this.btnRevert.UseVisualStyleBackColor = true;
            this.btnRevert.Click += new System.EventHandler(this.btnRevert_Click);
            // 
            // txtFilename
            // 
            this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilename.Location = new System.Drawing.Point(94, 38);
            this.txtFilename.Name = "txtFilename";
            this.txtFilename.ReadOnly = true;
            this.txtFilename.Size = new System.Drawing.Size(382, 22);
            this.sbStatusBarText.SetStatusBarText(this.txtFilename, "Source File: The source file containing the resource item");
            this.txtFilename.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(32, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Item ID";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvResourceItems);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.txtFilename);
            this.splitContainer1.Panel2.Controls.Add(this.editor);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.txtId);
            this.splitContainer1.Size = new System.Drawing.Size(636, 328);
            this.splitContainer1.SplitterDistance = 153;
            this.splitContainer1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Source File";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ResourceItemEditorWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(660, 390);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnRevert);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ResourceItemEditorWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ResourceItemEditorWindow_FormClosing);
            this.cmsDropImage.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SandcastleBuilder.Gui.ContentEditors.ContentEditorControl editor;
        private System.Windows.Forms.ContextMenuStrip cmsDropImage;
        private System.Windows.Forms.ToolStripMenuItem miMediaLinkInline;
        private System.Windows.Forms.ToolStripMenuItem miMediaLink;
        private System.Windows.Forms.ToolStripMenuItem miExternalLinkMedia;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbStatusBarText;
        private System.Windows.Forms.TreeView tvResourceItems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFilename;
    }
}