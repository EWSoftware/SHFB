namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class TokenEditorWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TokenEditorWindow));
            this.editor = new SandcastleBuilder.Gui.ContentEditors.ContentEditorControl();
            this.cmsDropImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miMediaLink = new System.Windows.Forms.ToolStripMenuItem();
            this.miMediaLinkInline = new System.Windows.Forms.ToolStripMenuItem();
            this.miExternalLinkMedia = new System.Windows.Forms.ToolStripMenuItem();
            this.sbStatusBarText = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.tvTokens = new System.Windows.Forms.TreeView();
            this.txtTokenID = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
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
            this.editor.Location = new System.Drawing.Point(3, 49);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(417, 279);
            this.sbStatusBarText.SetStatusBarText(this.editor, "Token Content: Edit the token content");
            this.editor.TabIndex = 3;
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
            // tvTokens
            // 
            this.tvTokens.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTokens.FullRowSelect = true;
            this.tvTokens.HideSelection = false;
            this.tvTokens.Location = new System.Drawing.Point(0, 0);
            this.tvTokens.Name = "tvTokens";
            this.tvTokens.ShowLines = false;
            this.tvTokens.ShowRootLines = false;
            this.tvTokens.Size = new System.Drawing.Size(212, 328);
            this.sbStatusBarText.SetStatusBarText(this.tvTokens, "Tokens: Select a token to edit");
            this.tvTokens.TabIndex = 0;
            this.tvTokens.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTokens_AfterSelect);
            this.tvTokens.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvTokens_BeforeSelect);
            // 
            // txtTokenID
            // 
            this.txtTokenID.Location = new System.Drawing.Point(82, 1);
            this.txtTokenID.Name = "txtTokenID";
            this.txtTokenID.Size = new System.Drawing.Size(241, 22);
            this.sbStatusBarText.SetStatusBarText(this.txtTokenID, "Token ID: Enter the token ID");
            this.txtTokenID.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(12, 346);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(88, 32);
            this.sbStatusBarText.SetStatusBarText(this.btnAdd, "Add: Add new token");
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "&Add";
            this.toolTip1.SetToolTip(this.btnAdd, "Add a new token");
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(136, 346);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(88, 32);
            this.sbStatusBarText.SetStatusBarText(this.btnDelete, "Delete: Delete the selected token");
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "&Delete";
            this.toolTip1.SetToolTip(this.btnDelete, "Delete selected token");
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Token ID";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Token &Content";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.splitContainer1.Panel1.Controls.Add(this.tvTokens);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.editor);
            this.splitContainer1.Panel2.Controls.Add(this.txtTokenID);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(636, 328);
            this.splitContainer1.SplitterDistance = 212;
            this.splitContainer1.TabIndex = 0;
            // 
            // TokenEditorWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(660, 390);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDelete);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TokenEditorWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TokenEditorWindow_FormClosing);
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
        private System.Windows.Forms.TreeView tvTokens;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTokenID;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}