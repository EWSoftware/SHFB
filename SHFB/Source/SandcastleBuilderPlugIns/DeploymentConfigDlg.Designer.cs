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
            this.lnkProjectSite = new System.Windows.Forms.LinkLabel();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabConfig = new System.Windows.Forms.TabControl();
            this.pgHtmlHelp1 = new System.Windows.Forms.TabPage();
            this.ucHtmlHelp1 = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgMSHelpViewer = new System.Windows.Forms.TabPage();
            this.chkRenameMSHA = new System.Windows.Forms.CheckBox();
            this.ucMSHelpViewer = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgWebsite = new System.Windows.Forms.TabPage();
            this.ucWebsite = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgOpenXml = new System.Windows.Forms.TabPage();
            this.ucOpenXml = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.pgMarkdown = new System.Windows.Forms.TabPage();
            this.ucMarkdownContent = new SandcastleBuilder.PlugIns.DeploymentConfigUserControl();
            this.chkDeleteAfterDeploy = new System.Windows.Forms.CheckBox();
            this.chkVerboseLogging = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.tabConfig.SuspendLayout();
            this.pgHtmlHelp1.SuspendLayout();
            this.pgMSHelpViewer.SuspendLayout();
            this.pgWebsite.SuspendLayout();
            this.pgOpenXml.SuspendLayout();
            this.pgMarkdown.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(680, 508);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 508);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(247, 512);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(298, 26);
            this.lnkProjectSite.TabIndex = 4;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectSite_LinkClicked);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.pgHtmlHelp1);
            this.tabConfig.Controls.Add(this.pgMSHelpViewer);
            this.tabConfig.Controls.Add(this.pgWebsite);
            this.tabConfig.Controls.Add(this.pgOpenXml);
            this.tabConfig.Controls.Add(this.pgMarkdown);
            this.tabConfig.Location = new System.Drawing.Point(12, 47);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.SelectedIndex = 0;
            this.tabConfig.Size = new System.Drawing.Size(768, 455);
            this.tabConfig.TabIndex = 2;
            // 
            // pgHtmlHelp1
            // 
            this.pgHtmlHelp1.Controls.Add(this.ucHtmlHelp1);
            this.pgHtmlHelp1.Location = new System.Drawing.Point(4, 34);
            this.pgHtmlHelp1.Name = "pgHtmlHelp1";
            this.pgHtmlHelp1.Padding = new System.Windows.Forms.Padding(3);
            this.pgHtmlHelp1.Size = new System.Drawing.Size(760, 417);
            this.pgHtmlHelp1.TabIndex = 0;
            this.pgHtmlHelp1.Text = "HTML Help 1";
            this.pgHtmlHelp1.UseVisualStyleBackColor = true;
            // 
            // ucHtmlHelp1
            // 
            this.ucHtmlHelp1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ucHtmlHelp1.Location = new System.Drawing.Point(18, 7);
            this.ucHtmlHelp1.Name = "ucHtmlHelp1";
            this.ucHtmlHelp1.Size = new System.Drawing.Size(725, 360);
            this.ucHtmlHelp1.TabIndex = 0;
            // 
            // pgMSHelpViewer
            // 
            this.pgMSHelpViewer.Controls.Add(this.chkRenameMSHA);
            this.pgMSHelpViewer.Controls.Add(this.ucMSHelpViewer);
            this.pgMSHelpViewer.Location = new System.Drawing.Point(4, 34);
            this.pgMSHelpViewer.Name = "pgMSHelpViewer";
            this.pgMSHelpViewer.Padding = new System.Windows.Forms.Padding(3);
            this.pgMSHelpViewer.Size = new System.Drawing.Size(760, 417);
            this.pgMSHelpViewer.TabIndex = 3;
            this.pgMSHelpViewer.Text = "MS Help Viewer";
            this.pgMSHelpViewer.UseVisualStyleBackColor = true;
            // 
            // chkRenameMSHA
            // 
            this.chkRenameMSHA.AutoSize = true;
            this.chkRenameMSHA.Location = new System.Drawing.Point(25, 373);
            this.chkRenameMSHA.Name = "chkRenameMSHA";
            this.chkRenameMSHA.Size = new System.Drawing.Size(532, 29);
            this.chkRenameMSHA.TabIndex = 1;
            this.chkRenameMSHA.Text = "Rename MSHA file to HelpContentSetup.msha when deployed";
            this.chkRenameMSHA.UseVisualStyleBackColor = true;
            // 
            // ucMSHelpViewer
            // 
            this.ucMSHelpViewer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ucMSHelpViewer.Location = new System.Drawing.Point(18, 7);
            this.ucMSHelpViewer.Name = "ucMSHelpViewer";
            this.ucMSHelpViewer.Size = new System.Drawing.Size(725, 360);
            this.ucMSHelpViewer.TabIndex = 0;
            // 
            // pgWebsite
            // 
            this.pgWebsite.Controls.Add(this.ucWebsite);
            this.pgWebsite.Location = new System.Drawing.Point(4, 34);
            this.pgWebsite.Name = "pgWebsite";
            this.pgWebsite.Padding = new System.Windows.Forms.Padding(3);
            this.pgWebsite.Size = new System.Drawing.Size(760, 417);
            this.pgWebsite.TabIndex = 2;
            this.pgWebsite.Text = "Website";
            this.pgWebsite.UseVisualStyleBackColor = true;
            // 
            // ucWebsite
            // 
            this.ucWebsite.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ucWebsite.Location = new System.Drawing.Point(18, 7);
            this.ucWebsite.Name = "ucWebsite";
            this.ucWebsite.Size = new System.Drawing.Size(725, 360);
            this.ucWebsite.TabIndex = 0;
            // 
            // pgOpenXml
            // 
            this.pgOpenXml.Controls.Add(this.ucOpenXml);
            this.pgOpenXml.Location = new System.Drawing.Point(4, 34);
            this.pgOpenXml.Name = "pgOpenXml";
            this.pgOpenXml.Padding = new System.Windows.Forms.Padding(3);
            this.pgOpenXml.Size = new System.Drawing.Size(760, 417);
            this.pgOpenXml.TabIndex = 4;
            this.pgOpenXml.Text = "Open XML";
            this.pgOpenXml.UseVisualStyleBackColor = true;
            // 
            // ucOpenXml
            // 
            this.ucOpenXml.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ucOpenXml.Location = new System.Drawing.Point(18, 7);
            this.ucOpenXml.Name = "ucOpenXml";
            this.ucOpenXml.Size = new System.Drawing.Size(725, 360);
            this.ucOpenXml.TabIndex = 0;
            // 
            // pgMarkdown
            // 
            this.pgMarkdown.Controls.Add(this.ucMarkdownContent);
            this.pgMarkdown.Location = new System.Drawing.Point(4, 34);
            this.pgMarkdown.Name = "pgMarkdown";
            this.pgMarkdown.Padding = new System.Windows.Forms.Padding(3);
            this.pgMarkdown.Size = new System.Drawing.Size(760, 417);
            this.pgMarkdown.TabIndex = 5;
            this.pgMarkdown.Text = "Markdown Content";
            this.pgMarkdown.UseVisualStyleBackColor = true;
            // 
            // ucMarkdownContent
            // 
            this.ucMarkdownContent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ucMarkdownContent.Location = new System.Drawing.Point(18, 7);
            this.ucMarkdownContent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMarkdownContent.Name = "ucMarkdownContent";
            this.ucMarkdownContent.Size = new System.Drawing.Size(725, 360);
            this.ucMarkdownContent.TabIndex = 0;
            // 
            // chkDeleteAfterDeploy
            // 
            this.chkDeleteAfterDeploy.AutoSize = true;
            this.chkDeleteAfterDeploy.Location = new System.Drawing.Point(12, 12);
            this.chkDeleteAfterDeploy.Name = "chkDeleteAfterDeploy";
            this.chkDeleteAfterDeploy.Size = new System.Drawing.Size(353, 29);
            this.chkDeleteAfterDeploy.TabIndex = 0;
            this.chkDeleteAfterDeploy.Text = "Delete source files &after deploying them";
            this.chkDeleteAfterDeploy.UseVisualStyleBackColor = true;
            // 
            // chkVerboseLogging
            // 
            this.chkVerboseLogging.AutoSize = true;
            this.chkVerboseLogging.Location = new System.Drawing.Point(432, 12);
            this.chkVerboseLogging.Name = "chkVerboseLogging";
            this.chkVerboseLogging.Size = new System.Drawing.Size(315, 29);
            this.chkVerboseLogging.TabIndex = 1;
            this.chkVerboseLogging.Text = "Log the names of all files deployed";
            this.chkVerboseLogging.UseVisualStyleBackColor = true;
            // 
            // DeploymentConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(792, 555);
            this.Controls.Add(this.chkVerboseLogging);
            this.Controls.Add(this.chkDeleteAfterDeploy);
            this.Controls.Add(this.tabConfig);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.pgMSHelpViewer.ResumeLayout(false);
            this.pgMSHelpViewer.PerformLayout();
            this.pgWebsite.ResumeLayout(false);
            this.pgOpenXml.ResumeLayout(false);
            this.pgMarkdown.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.TabControl tabConfig;
        private System.Windows.Forms.TabPage pgHtmlHelp1;
        private System.Windows.Forms.TabPage pgWebsite;
        private System.Windows.Forms.CheckBox chkDeleteAfterDeploy;
        private System.Windows.Forms.TabPage pgMSHelpViewer;
        private DeploymentConfigUserControl ucHtmlHelp1;
        private DeploymentConfigUserControl ucMSHelpViewer;
        private DeploymentConfigUserControl ucWebsite;
        private System.Windows.Forms.CheckBox chkRenameMSHA;
        private System.Windows.Forms.TabPage pgOpenXml;
        private DeploymentConfigUserControl ucOpenXml;
        private System.Windows.Forms.TabPage pgMarkdown;
        private DeploymentConfigUserControl ucMarkdownContent;
        private System.Windows.Forms.CheckBox chkVerboseLogging;
    }
}
