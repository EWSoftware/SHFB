namespace SandcastleBuilder.PlugIns
{
    partial class CompletionNotificationConfigDlg
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
            this.txtFailureEMailAddress = new System.Windows.Forms.TextBox();
            this.txtSuccessEMailAddress = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtFromEMail = new System.Windows.Forms.TextBox();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.txtXSLTransform = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkAttachLogFileOnFailure = new System.Windows.Forms.CheckBox();
            this.chkAttachLogFileOnSuccess = new System.Windows.Forms.CheckBox();
            this.chkUseDefaultCredentials = new System.Windows.Forms.CheckBox();
            this.udcSmtpPort = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSmtpServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.pnlOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcSmtpPort)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(566, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 280);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkCodePlexSHFB
            // 
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(224, 285);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 3;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCodePlexSHFB_LinkClicked);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // txtFailureEMailAddress
            // 
            this.txtFailureEMailAddress.Location = new System.Drawing.Point(180, 143);
            this.txtFailureEMailAddress.MaxLength = 256;
            this.txtFailureEMailAddress.Name = "txtFailureEMailAddress";
            this.txtFailureEMailAddress.Size = new System.Drawing.Size(412, 22);
            this.txtFailureEMailAddress.TabIndex = 14;
            // 
            // txtSuccessEMailAddress
            // 
            this.txtSuccessEMailAddress.Location = new System.Drawing.Point(180, 115);
            this.txtSuccessEMailAddress.MaxLength = 256;
            this.txtSuccessEMailAddress.Name = "txtSuccessEMailAddress";
            this.txtSuccessEMailAddress.Size = new System.Drawing.Size(412, 22);
            this.txtSuccessEMailAddress.TabIndex = 12;
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(450, 59);
            this.txtPassword.MaxLength = 50;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(164, 22);
            this.txtPassword.TabIndex = 8;
            // 
            // txtUserName
            // 
            this.txtUserName.Enabled = false;
            this.txtUserName.Location = new System.Drawing.Point(180, 59);
            this.txtUserName.MaxLength = 50;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(164, 22);
            this.txtUserName.TabIndex = 6;
            // 
            // txtFromEMail
            // 
            this.txtFromEMail.Location = new System.Drawing.Point(180, 87);
            this.txtFromEMail.MaxLength = 256;
            this.txtFromEMail.Name = "txtFromEMail";
            this.txtFromEMail.Size = new System.Drawing.Size(412, 22);
            this.txtFromEMail.TabIndex = 10;
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.txtXSLTransform);
            this.pnlOptions.Controls.Add(this.label8);
            this.pnlOptions.Controls.Add(this.txtFromEMail);
            this.pnlOptions.Controls.Add(this.label7);
            this.pnlOptions.Controls.Add(this.txtFailureEMailAddress);
            this.pnlOptions.Controls.Add(this.label4);
            this.pnlOptions.Controls.Add(this.txtSuccessEMailAddress);
            this.pnlOptions.Controls.Add(this.label6);
            this.pnlOptions.Controls.Add(this.chkAttachLogFileOnFailure);
            this.pnlOptions.Controls.Add(this.chkAttachLogFileOnSuccess);
            this.pnlOptions.Controls.Add(this.chkUseDefaultCredentials);
            this.pnlOptions.Controls.Add(this.udcSmtpPort);
            this.pnlOptions.Controls.Add(this.label5);
            this.pnlOptions.Controls.Add(this.txtPassword);
            this.pnlOptions.Controls.Add(this.label3);
            this.pnlOptions.Controls.Add(this.txtUserName);
            this.pnlOptions.Controls.Add(this.label2);
            this.pnlOptions.Controls.Add(this.txtSmtpServer);
            this.pnlOptions.Controls.Add(this.label1);
            this.pnlOptions.Location = new System.Drawing.Point(12, 12);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(642, 260);
            this.pnlOptions.TabIndex = 0;
            // 
            // txtXSLTransform
            // 
            this.txtXSLTransform.Location = new System.Drawing.Point(180, 225);
            this.txtXSLTransform.MaxLength = 256;
            this.txtXSLTransform.Name = "txtXSLTransform";
            this.txtXSLTransform.Size = new System.Drawing.Size(412, 22);
            this.txtXSLTransform.TabIndex = 18;
            this.txtXSLTransform.Text = "{@SHFBFolder}Templates\\TransformBuildLog.xsl";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(8, 225);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(166, 23);
            this.label8.TabIndex = 17;
            this.label8.Text = "&Optional XSL Transform";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(166, 23);
            this.label7.TabIndex = 9;
            this.label7.Text = "&From E-Mail Address";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(166, 23);
            this.label4.TabIndex = 13;
            this.label4.Text = "Failure E-Mail &Address";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(8, 115);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(166, 23);
            this.label6.TabIndex = 11;
            this.label6.Text = "Success &E-Mail Address";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkAttachLogFileOnFailure
            // 
            this.chkAttachLogFileOnFailure.Checked = true;
            this.chkAttachLogFileOnFailure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAttachLogFileOnFailure.Location = new System.Drawing.Point(180, 198);
            this.chkAttachLogFileOnFailure.Name = "chkAttachLogFileOnFailure";
            this.chkAttachLogFileOnFailure.Size = new System.Drawing.Size(265, 21);
            this.chkAttachLogFileOnFailure.TabIndex = 16;
            this.chkAttachLogFileOnFailure.Text = "A&ttach build log on failed build";
            this.chkAttachLogFileOnFailure.UseVisualStyleBackColor = true;
            // 
            // chkAttachLogFileOnSuccess
            // 
            this.chkAttachLogFileOnSuccess.Location = new System.Drawing.Point(180, 171);
            this.chkAttachLogFileOnSuccess.Name = "chkAttachLogFileOnSuccess";
            this.chkAttachLogFileOnSuccess.Size = new System.Drawing.Size(265, 21);
            this.chkAttachLogFileOnSuccess.TabIndex = 15;
            this.chkAttachLogFileOnSuccess.Text = "Attach &build log on successful build";
            this.chkAttachLogFileOnSuccess.UseVisualStyleBackColor = true;
            // 
            // chkUseDefaultCredentials
            // 
            this.chkUseDefaultCredentials.Checked = true;
            this.chkUseDefaultCredentials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseDefaultCredentials.Location = new System.Drawing.Point(180, 35);
            this.chkUseDefaultCredentials.Name = "chkUseDefaultCredentials";
            this.chkUseDefaultCredentials.Size = new System.Drawing.Size(190, 21);
            this.chkUseDefaultCredentials.TabIndex = 4;
            this.chkUseDefaultCredentials.Text = "Use &Default Credentials";
            this.chkUseDefaultCredentials.UseVisualStyleBackColor = true;
            this.chkUseDefaultCredentials.CheckedChanged += new System.EventHandler(this.chkUseDefaultCredentials_CheckedChanged);
            // 
            // udcSmtpPort
            // 
            this.udcSmtpPort.Location = new System.Drawing.Point(572, 8);
            this.udcSmtpPort.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.udcSmtpPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udcSmtpPort.Name = "udcSmtpPort";
            this.udcSmtpPort.Size = new System.Drawing.Size(56, 22);
            this.udcSmtpPort.TabIndex = 3;
            this.udcSmtpPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcSmtpPort.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(509, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 23);
            this.label5.TabIndex = 2;
            this.label5.Text = "&Port #";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(363, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 23);
            this.label3.TabIndex = 7;
            this.label3.Text = "Pass&word";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(86, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "&User Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSmtpServer
            // 
            this.txtSmtpServer.Location = new System.Drawing.Point(180, 7);
            this.txtSmtpServer.MaxLength = 256;
            this.txtSmtpServer.Name = "txtSmtpServer";
            this.txtSmtpServer.Size = new System.Drawing.Size(318, 22);
            this.txtSmtpServer.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(67, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "SMTP &Server";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CompletionNotificationConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(666, 324);
            this.Controls.Add(this.pnlOptions);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompletionNotificationConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Completion Notification Plug-In";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcSmtpPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.TextBox txtFailureEMailAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSuccessEMailAddress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkAttachLogFileOnFailure;
        private System.Windows.Forms.CheckBox chkAttachLogFileOnSuccess;
        private System.Windows.Forms.CheckBox chkUseDefaultCredentials;
        private System.Windows.Forms.NumericUpDown udcSmtpPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSmtpServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFromEMail;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtXSLTransform;
        private System.Windows.Forms.Label label8;
    }
}