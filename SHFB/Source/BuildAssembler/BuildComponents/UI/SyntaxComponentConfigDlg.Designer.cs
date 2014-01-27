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
            this.chkRenderReferenceLinks = new System.Windows.Forms.CheckBox();
            this.lblConfiguration = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtConfiguration = new System.Windows.Forms.TextBox();
            this.tvGenerators = new System.Windows.Forms.TreeView();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(687, 304);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(593, 304);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(12, 304);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(88, 32);
            this.btnReset.TabIndex = 5;
            this.btnReset.Text = "Re&set";
            this.toolTip1.SetToolTip(this.btnReset, "Reset to default configuration");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // chkRenderReferenceLinks
            // 
            this.chkRenderReferenceLinks.AutoSize = true;
            this.chkRenderReferenceLinks.Location = new System.Drawing.Point(12, 12);
            this.chkRenderReferenceLinks.Name = "chkRenderReferenceLinks";
            this.chkRenderReferenceLinks.Size = new System.Drawing.Size(507, 21);
            this.chkRenderReferenceLinks.TabIndex = 0;
            this.chkRenderReferenceLinks.Text = "&Render reference links to online content (not supported by MS Help Viewer)";
            this.chkRenderReferenceLinks.UseVisualStyleBackColor = true;
            // 
            // lblConfiguration
            // 
            this.lblConfiguration.Location = new System.Drawing.Point(240, 53);
            this.lblConfiguration.Name = "lblConfiguration";
            this.lblConfiguration.Size = new System.Drawing.Size(132, 23);
            this.lblConfiguration.TabIndex = 3;
            this.lblConfiguration.Text = "&Edit Configuration";
            this.lblConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(222, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Configurable Syntax Generators";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtConfiguration
            // 
            this.txtConfiguration.AcceptsReturn = true;
            this.txtConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfiguration.Enabled = false;
            this.txtConfiguration.Location = new System.Drawing.Point(240, 79);
            this.txtConfiguration.Multiline = true;
            this.txtConfiguration.Name = "txtConfiguration";
            this.txtConfiguration.Size = new System.Drawing.Size(535, 219);
            this.txtConfiguration.TabIndex = 4;
            // 
            // tvGenerators
            // 
            this.tvGenerators.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvGenerators.FullRowSelect = true;
            this.tvGenerators.HideSelection = false;
            this.tvGenerators.Location = new System.Drawing.Point(12, 79);
            this.tvGenerators.Name = "tvGenerators";
            this.tvGenerators.ShowRootLines = false;
            this.tvGenerators.Size = new System.Drawing.Size(222, 219);
            this.tvGenerators.TabIndex = 2;
            this.tvGenerators.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvGenerators_BeforeSelect);
            this.tvGenerators.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvGenerators_AfterSelect);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // SyntaxComponentConfigDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(787, 348);
            this.Controls.Add(this.lblConfiguration);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtConfiguration);
            this.Controls.Add(this.tvGenerators);
            this.Controls.Add(this.chkRenderReferenceLinks);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
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
    }
}