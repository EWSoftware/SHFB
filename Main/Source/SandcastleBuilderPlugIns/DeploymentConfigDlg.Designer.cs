namespace SandcastleBuilder.PlugIns
{
    partial class DeploymentConfigDlg
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
            this.lnkCodePlexSHFB = new System.Windows.Forms.LinkLabel();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabConfig = new System.Windows.Forms.TabControl();
            this.pgHtmlHelp1 = new System.Windows.Forms.TabPage();
            this.ucHtmlHelp1 = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgMSHelp2 = new System.Windows.Forms.TabPage();
            this.ucMSHelp2 = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgMSHelpViewer = new System.Windows.Forms.TabPage();
            this.ucMSHelpViewer = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgWebsite = new System.Windows.Forms.TabPage();
            this.ucWebsite = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.chkDeleteAfterDeploy = new System.Windows.Forms.CheckBox();
            this.chkRenameMSHA = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.tabConfig.SuspendLayout();
            this.pgHtmlHelp1.SuspendLayout();
            this.pgMSHelp2.SuspendLayout();
            this.pgMSHelpViewer.SuspendLayout();
            this.pgWebsite.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(538, 379);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 379);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkCodePlexSHFB
            // 
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(210, 384);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 4;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.codePlex_LinkClicked);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.pgHtmlHelp1);
            this.tabConfig.Controls.Add(this.pgMSHelp2);
            this.tabConfig.Controls.Add(this.pgMSHelpViewer);
            this.tabConfig.Controls.Add(this.pgWebsite);
            this.tabConfig.Location = new System.Drawing.Point(12, 42);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.SelectedIndex = 0;
            this.tabConfig.Size = new System.Drawing.Size(614, 331);
            this.tabConfig.TabIndex = 1;
            // 
            // pgHtmlHelp1
            // 
            this.pgHtmlHelp1.Controls.Add(this.ucHtmlHelp1);
            this.pgHtmlHelp1.Location = new System.Drawing.Point(4, 25);
            this.pgHtmlHelp1.Name = "pgHtmlHelp1";
            this.pgHtmlHelp1.Padding = new System.Windows.Forms.Padding(3);
            this.pgHtmlHelp1.Size = new System.Drawing.Size(606, 302);
            this.pgHtmlHelp1.TabIndex = 0;
            this.pgHtmlHelp1.Text = "HTML Help 1";
            this.pgHtmlHelp1.UseVisualStyleBackColor = true;
            // 
            // ucHtmlHelp1
            // 
            this.ucHtmlHelp1.Location = new System.Drawing.Point(5, 12);
            this.ucHtmlHelp1.Name = "ucHtmlHelp1";
            this.ucHtmlHelp1.Size = new System.Drawing.Size(596, 257);
            this.ucHtmlHelp1.TabIndex = 0;
            // 
            // pgMSHelp2
            // 
            this.pgMSHelp2.Controls.Add(this.ucMSHelp2);
            this.pgMSHelp2.Location = new System.Drawing.Point(4, 25);
            this.pgMSHelp2.Name = "pgMSHelp2";
            this.pgMSHelp2.Padding = new System.Windows.Forms.Padding(3);
            this.pgMSHelp2.Size = new System.Drawing.Size(606, 302);
            this.pgMSHelp2.TabIndex = 1;
            this.pgMSHelp2.Text = "MS Help 2";
            this.pgMSHelp2.UseVisualStyleBackColor = true;
            // 
            // ucMSHelp2
            // 
            this.ucMSHelp2.Location = new System.Drawing.Point(5, 12);
            this.ucMSHelp2.Name = "ucMSHelp2";
            this.ucMSHelp2.Size = new System.Drawing.Size(596, 257);
            this.ucMSHelp2.TabIndex = 0;
            // 
            // pgMSHelpViewer
            // 
            this.pgMSHelpViewer.Controls.Add(this.chkRenameMSHA);
            this.pgMSHelpViewer.Controls.Add(this.ucMSHelpViewer);
            this.pgMSHelpViewer.Location = new System.Drawing.Point(4, 25);
            this.pgMSHelpViewer.Name = "pgMSHelpViewer";
            this.pgMSHelpViewer.Padding = new System.Windows.Forms.Padding(3);
            this.pgMSHelpViewer.Size = new System.Drawing.Size(606, 302);
            this.pgMSHelpViewer.TabIndex = 3;
            this.pgMSHelpViewer.Text = "MS Help Viewer";
            this.pgMSHelpViewer.UseVisualStyleBackColor = true;
            // 
            // ucMSHelpViewer
            // 
            this.ucMSHelpViewer.Location = new System.Drawing.Point(5, 12);
            this.ucMSHelpViewer.Name = "ucMSHelpViewer";
            this.ucMSHelpViewer.Size = new System.Drawing.Size(596, 257);
            this.ucMSHelpViewer.TabIndex = 0;
            // 
            // pgWebsite
            // 
            this.pgWebsite.Controls.Add(this.ucWebsite);
            this.pgWebsite.Location = new System.Drawing.Point(4, 25);
            this.pgWebsite.Name = "pgWebsite";
            this.pgWebsite.Padding = new System.Windows.Forms.Padding(3);
            this.pgWebsite.Size = new System.Drawing.Size(606, 302);
            this.pgWebsite.TabIndex = 2;
            this.pgWebsite.Text = "Website";
            this.pgWebsite.UseVisualStyleBackColor = true;
            // 
            // ucWebsite
            // 
            this.ucWebsite.Location = new System.Drawing.Point(5, 12);
            this.ucWebsite.Name = "ucWebsite";
            this.ucWebsite.Size = new System.Drawing.Size(596, 257);
            this.ucWebsite.TabIndex = 0;
            // 
            // chkDeleteAfterDeploy
            // 
            this.chkDeleteAfterDeploy.Location = new System.Drawing.Point(12, 12);
            this.chkDeleteAfterDeploy.Name = "chkDeleteAfterDeploy";
            this.chkDeleteAfterDeploy.Size = new System.Drawing.Size(291, 24);
            this.chkDeleteAfterDeploy.TabIndex = 0;
            this.chkDeleteAfterDeploy.Text = "Delete source files &after deploying them";
            this.chkDeleteAfterDeploy.UseVisualStyleBackColor = true;
            // 
            // chkRenameMSHA
            // 
            this.chkRenameMSHA.AutoSize = true;
            this.chkRenameMSHA.Location = new System.Drawing.Point(93, 275);
            this.chkRenameMSHA.Name = "chkRenameMSHA";
            this.chkRenameMSHA.Size = new System.Drawing.Size(420, 21);
            this.chkRenameMSHA.TabIndex = 1;
            this.chkRenameMSHA.Text = "Rename MSHA file to HelpContentSetup.msha when deployed";
            this.chkRenameMSHA.UseVisualStyleBackColor = true;
            // 
            // DeploymentConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(638, 423);
            this.Controls.Add(this.chkDeleteAfterDeploy);
            this.Controls.Add(this.tabConfig);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeploymentConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Output Deployment Plug-In";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.tabConfig.ResumeLayout(false);
            this.pgHtmlHelp1.ResumeLayout(false);
            this.pgMSHelp2.ResumeLayout(false);
            this.pgMSHelpViewer.ResumeLayout(false);
            this.pgMSHelpViewer.PerformLayout();
            this.pgWebsite.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
        private System.Windows.Forms.TabControl tabConfig;
        private System.Windows.Forms.TabPage pgHtmlHelp1;
        private System.Windows.Forms.TabPage pgMSHelp2;
        private System.Windows.Forms.TabPage pgWebsite;
        private System.Windows.Forms.CheckBox chkDeleteAfterDeploy;
        private System.Windows.Forms.TabPage pgMSHelpViewer;
        private DeploymentConfigUserControl ucHtmlHelp1;
        private DeploymentConfigUserControl ucMSHelp2;
        private DeploymentConfigUserControl ucMSHelpViewer;
        private DeploymentConfigUserControl ucWebsite;
        private System.Windows.Forms.CheckBox chkRenameMSHA;
    }
}
