namespace SandcastleBuilder.PlugIns
{
    partial class AjaxDocConfigDlg
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
            this.lnkCodePlexAjaxDoc = new System.Windows.Forms.LinkLabel();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtAjaxDocUrl = new System.Windows.Forms.TextBox();
            this.txtProxyUserName = new System.Windows.Forms.TextBox();
            this.txtProxyPassword = new System.Windows.Forms.TextBox();
            this.txtProxyServer = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkUseDefaultCredentials = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkRegenerateFiles = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkUseProxyServer = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkUseProxyDefCreds = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(511, 356);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 356);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkCodePlexSHFB
            // 
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(140, 361);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 5;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.codePlex_LinkClicked);
            // 
            // lnkCodePlexAjaxDoc
            // 
            this.lnkCodePlexAjaxDoc.Location = new System.Drawing.Point(366, 361);
            this.lnkCodePlexAjaxDoc.Name = "lnkCodePlexAjaxDoc";
            this.lnkCodePlexAjaxDoc.Size = new System.Drawing.Size(105, 23);
            this.lnkCodePlexAjaxDoc.TabIndex = 6;
            this.lnkCodePlexAjaxDoc.TabStop = true;
            this.lnkCodePlexAjaxDoc.Text = "AjaxDoc";
            this.lnkCodePlexAjaxDoc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexAjaxDoc, "http://AjaxDoc.CodePlex.com");
            this.lnkCodePlexAjaxDoc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.codePlex_LinkClicked);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // txtProjectName
            // 
            this.epErrors.SetIconPadding(this.txtProjectName, 35);
            this.txtProjectName.Location = new System.Drawing.Point(128, 52);
            this.txtProjectName.MaxLength = 256;
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.Size = new System.Drawing.Size(412, 22);
            this.txtProjectName.TabIndex = 3;
            this.txtProjectName.Text = "MicrosoftAjax";
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.epErrors.SetIconPadding(this.txtPassword, 35);
            this.txtPassword.Location = new System.Drawing.Point(398, 44);
            this.txtPassword.MaxLength = 50;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(164, 22);
            this.txtPassword.TabIndex = 4;
            // 
            // txtUserName
            // 
            this.txtUserName.Enabled = false;
            this.epErrors.SetIconPadding(this.txtUserName, 35);
            this.txtUserName.Location = new System.Drawing.Point(128, 44);
            this.txtUserName.MaxLength = 50;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(164, 22);
            this.txtUserName.TabIndex = 2;
            // 
            // txtAjaxDocUrl
            // 
            this.epErrors.SetIconPadding(this.txtAjaxDocUrl, 35);
            this.txtAjaxDocUrl.Location = new System.Drawing.Point(128, 24);
            this.txtAjaxDocUrl.MaxLength = 256;
            this.txtAjaxDocUrl.Name = "txtAjaxDocUrl";
            this.txtAjaxDocUrl.Size = new System.Drawing.Size(412, 22);
            this.txtAjaxDocUrl.TabIndex = 1;
            this.txtAjaxDocUrl.Text = "http://localhost/AjaxDoc/";
            // 
            // txtProxyUserName
            // 
            this.txtProxyUserName.Enabled = false;
            this.epErrors.SetIconPadding(this.txtProxyUserName, 35);
            this.txtProxyUserName.Location = new System.Drawing.Point(128, 100);
            this.txtProxyUserName.MaxLength = 50;
            this.txtProxyUserName.Name = "txtProxyUserName";
            this.txtProxyUserName.Size = new System.Drawing.Size(164, 22);
            this.txtProxyUserName.TabIndex = 5;
            // 
            // txtProxyPassword
            // 
            this.txtProxyPassword.Enabled = false;
            this.epErrors.SetIconPadding(this.txtProxyPassword, 35);
            this.txtProxyPassword.Location = new System.Drawing.Point(398, 100);
            this.txtProxyPassword.MaxLength = 50;
            this.txtProxyPassword.Name = "txtProxyPassword";
            this.txtProxyPassword.Size = new System.Drawing.Size(164, 22);
            this.txtProxyPassword.TabIndex = 7;
            // 
            // txtProxyServer
            // 
            this.txtProxyServer.Enabled = false;
            this.epErrors.SetIconPadding(this.txtProxyServer, 35);
            this.txtProxyServer.Location = new System.Drawing.Point(128, 48);
            this.txtProxyServer.MaxLength = 256;
            this.txtProxyServer.Name = "txtProxyServer";
            this.txtProxyServer.Size = new System.Drawing.Size(412, 22);
            this.txtProxyServer.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(11, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 23);
            this.label7.TabIndex = 0;
            this.label7.Text = "&AjaxDoc URL";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(14, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 23);
            this.label6.TabIndex = 2;
            this.label6.Text = "&Project Name";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkUseDefaultCredentials
            // 
            this.chkUseDefaultCredentials.Checked = true;
            this.chkUseDefaultCredentials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseDefaultCredentials.Location = new System.Drawing.Point(128, 20);
            this.chkUseDefaultCredentials.Name = "chkUseDefaultCredentials";
            this.chkUseDefaultCredentials.Size = new System.Drawing.Size(190, 21);
            this.chkUseDefaultCredentials.TabIndex = 0;
            this.chkUseDefaultCredentials.Text = "Use &Default Credentials";
            this.chkUseDefaultCredentials.UseVisualStyleBackColor = true;
            this.chkUseDefaultCredentials.CheckedChanged += new System.EventHandler(this.chkUseDefaultCredentials_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(311, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "Pass&word";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(34, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "&User Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkRegenerateFiles);
            this.groupBox1.Controls.Add(this.txtAjaxDocUrl);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtProjectName);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(587, 110);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AjaxDoc Project";
            // 
            // chkRegenerateFiles
            // 
            this.chkRegenerateFiles.Checked = true;
            this.chkRegenerateFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRegenerateFiles.Location = new System.Drawing.Point(128, 80);
            this.chkRegenerateFiles.Name = "chkRegenerateFiles";
            this.chkRegenerateFiles.Size = new System.Drawing.Size(384, 21);
            this.chkRegenerateFiles.TabIndex = 4;
            this.chkRegenerateFiles.Text = "&Regenerate the output files before downloading them";
            this.chkRegenerateFiles.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtUserName);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtPassword);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.chkUseDefaultCredentials);
            this.groupBox2.Location = new System.Drawing.Point(12, 128);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(587, 78);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "User Credentials";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkUseProxyServer);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.txtProxyUserName);
            this.groupBox3.Controls.Add(this.txtProxyServer);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.txtProxyPassword);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.chkUseProxyDefCreds);
            this.groupBox3.Location = new System.Drawing.Point(12, 212);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(587, 138);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Proxy Credentials";
            // 
            // chkUseProxyServer
            // 
            this.chkUseProxyServer.Location = new System.Drawing.Point(128, 21);
            this.chkUseProxyServer.Name = "chkUseProxyServer";
            this.chkUseProxyServer.Size = new System.Drawing.Size(153, 21);
            this.chkUseProxyServer.TabIndex = 0;
            this.chkUseProxyServer.Text = "User Pr&oxy Server";
            this.chkUseProxyServer.UseVisualStyleBackColor = true;
            this.chkUseProxyServer.CheckedChanged += new System.EventHandler(this.chkUseProxyServer_CheckedChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(14, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 23);
            this.label5.TabIndex = 1;
            this.label5.Text = "Pro&xy Server";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(311, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "Pa&ssword";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(34, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 23);
            this.label4.TabIndex = 4;
            this.label4.Text = "Us&er Name";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkUseProxyDefCreds
            // 
            this.chkUseProxyDefCreds.Checked = true;
            this.chkUseProxyDefCreds.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseProxyDefCreds.Enabled = false;
            this.chkUseProxyDefCreds.Location = new System.Drawing.Point(128, 76);
            this.chkUseProxyDefCreds.Name = "chkUseProxyDefCreds";
            this.chkUseProxyDefCreds.Size = new System.Drawing.Size(190, 21);
            this.chkUseProxyDefCreds.TabIndex = 3;
            this.chkUseProxyDefCreds.Text = "Use &Default &Credentials";
            this.chkUseProxyDefCreds.UseVisualStyleBackColor = true;
            this.chkUseProxyDefCreds.CheckedChanged += new System.EventHandler(this.chkUseProxyDefCreds_CheckedChanged);
            // 
            // AjaxDocConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(611, 400);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lnkCodePlexAjaxDoc);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AjaxDocConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure AjaxDoc Builder Plug-In";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkUseDefaultCredentials;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAjaxDocUrl;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.LinkLabel lnkCodePlexAjaxDoc;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkUseProxyServer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtProxyUserName;
        private System.Windows.Forms.TextBox txtProxyServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProxyPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkUseProxyDefCreds;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkRegenerateFiles;
    }
}