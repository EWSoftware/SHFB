namespace Microsoft.Ddue.Tools.UI
{
    partial class SyntaxComponentConfigDlg
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.chkRenderReferenceLinks = new System.Windows.Forms.CheckBox();
            this.lblConfiguration = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtConfiguration = new System.Windows.Forms.TextBox();
            this.tvGenerators = new System.Windows.Forms.TreeView();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.chkAddNoExampleTabs = new System.Windows.Forms.CheckBox();
            this.chkIncludeOnSingleSnippets = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(754, 420);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(648, 420);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(244, 420);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 35);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "R&eset";
            this.toolTip1.SetToolTip(this.btnReset, "Reset to default configuration");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMoveUp.Location = new System.Drawing.Point(12, 420);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(100, 35);
            this.btnMoveUp.TabIndex = 7;
            this.btnMoveUp.Text = "&Up";
            this.toolTip1.SetToolTip(this.btnMoveUp, "Move the generator up in the order");
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMoveDown.Location = new System.Drawing.Point(118, 420);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(100, 35);
            this.btnMoveDown.TabIndex = 8;
            this.btnMoveDown.Text = "&Down";
            this.toolTip1.SetToolTip(this.btnMoveDown, "Move the generator down in the order");
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // chkRenderReferenceLinks
            // 
            this.chkRenderReferenceLinks.AutoSize = true;
            this.chkRenderReferenceLinks.Location = new System.Drawing.Point(12, 12);
            this.chkRenderReferenceLinks.Name = "chkRenderReferenceLinks";
            this.chkRenderReferenceLinks.Size = new System.Drawing.Size(635, 29);
            this.chkRenderReferenceLinks.TabIndex = 0;
            this.chkRenderReferenceLinks.Text = "&Render reference links to online content (not supported by MS Help Viewer)";
            this.chkRenderReferenceLinks.UseVisualStyleBackColor = true;
            // 
            // lblConfiguration
            // 
            this.lblConfiguration.Location = new System.Drawing.Point(239, 125);
            this.lblConfiguration.Name = "lblConfiguration";
            this.lblConfiguration.Size = new System.Drawing.Size(250, 26);
            this.lblConfiguration.TabIndex = 5;
            this.lblConfiguration.Text = "Edit &Configuration";
            this.lblConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 125);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 26);
            this.label1.TabIndex = 3;
            this.label1.Text = "&Syntax Generators";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtConfiguration
            // 
            this.txtConfiguration.AcceptsReturn = true;
            this.txtConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfiguration.Enabled = false;
            this.txtConfiguration.Location = new System.Drawing.Point(244, 154);
            this.txtConfiguration.Multiline = true;
            this.txtConfiguration.Name = "txtConfiguration";
            this.txtConfiguration.Size = new System.Drawing.Size(610, 260);
            this.txtConfiguration.TabIndex = 6;
            // 
            // tvGenerators
            // 
            this.tvGenerators.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvGenerators.FullRowSelect = true;
            this.tvGenerators.HideSelection = false;
            this.tvGenerators.Location = new System.Drawing.Point(12, 154);
            this.tvGenerators.Name = "tvGenerators";
            this.tvGenerators.ShowRootLines = false;
            this.tvGenerators.Size = new System.Drawing.Size(226, 260);
            this.tvGenerators.TabIndex = 4;
            this.tvGenerators.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvGenerators_BeforeSelect);
            this.tvGenerators.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvGenerators_AfterSelect);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // chkAddNoExampleTabs
            // 
            this.chkAddNoExampleTabs.AutoSize = true;
            this.chkAddNoExampleTabs.Checked = true;
            this.chkAddNoExampleTabs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAddNoExampleTabs.Location = new System.Drawing.Point(12, 47);
            this.chkAddNoExampleTabs.Name = "chkAddNoExampleTabs";
            this.chkAddNoExampleTabs.Size = new System.Drawing.Size(688, 29);
            this.chkAddNoExampleTabs.TabIndex = 1;
            this.chkAddNoExampleTabs.Text = "&Add \"No example\" tabs in presentation styles that support code snippet grouping";
            this.chkAddNoExampleTabs.UseVisualStyleBackColor = true;
            this.chkAddNoExampleTabs.CheckedChanged += new System.EventHandler(this.chkAddNoExampleTabs_CheckedChanged);
            // 
            // chkIncludeOnSingleSnippets
            // 
            this.chkIncludeOnSingleSnippets.AutoSize = true;
            this.chkIncludeOnSingleSnippets.Location = new System.Drawing.Point(38, 82);
            this.chkIncludeOnSingleSnippets.Name = "chkIncludeOnSingleSnippets";
            this.chkIncludeOnSingleSnippets.Size = new System.Drawing.Size(318, 29);
            this.chkIncludeOnSingleSnippets.TabIndex = 2;
            this.chkIncludeOnSingleSnippets.Text = "&Include on standalone snippets too";
            this.chkIncludeOnSingleSnippets.UseVisualStyleBackColor = true;
            // 
            // SyntaxComponentConfigDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(866, 467);
            this.Controls.Add(this.chkIncludeOnSingleSnippets);
            this.Controls.Add(this.chkAddNoExampleTabs);
            this.Controls.Add(this.btnMoveDown);
            this.Controls.Add(this.btnMoveUp);
            this.Controls.Add(this.lblConfiguration);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtConfiguration);
            this.Controls.Add(this.tvGenerators);
            this.Controls.Add(this.chkRenderReferenceLinks);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SyntaxComponentConfigDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Syntax Component Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkRenderReferenceLinks;
        private System.Windows.Forms.Label lblConfiguration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TextBox txtConfiguration;
        private System.Windows.Forms.TreeView tvGenerators;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.CheckBox chkIncludeOnSingleSnippets;
        private System.Windows.Forms.CheckBox chkAddNoExampleTabs;
    }
}