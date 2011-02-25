namespace SandcastleBuilder.PlugIns
{
    partial class DeploymentConfigUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkUseDefaultCredentials = new System.Windows.Forms.CheckBox();
            this.txtTargetLocation = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkUseProxyServer = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtProxyUserName = new System.Windows.Forms.TextBox();
            this.txtProxyServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtProxyPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkUseProxyDefCreds = new System.Windows.Forms.CheckBox();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(16, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 23);
            this.label7.TabIndex = 0;
            this.label7.Text = "&Target Location";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtUserName);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtPassword);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.chkUseDefaultCredentials);
            this.groupBox2.Location = new System.Drawing.Point(5, 31);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(587, 78);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "User Credentials";
            // 
            // txtUserName
            // 
            this.txtUserName.Enabled = false;
            this.txtUserName.Location = new System.Drawing.Point(128, 44);
            this.txtUserName.MaxLength = 50;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(164, 22);
            this.txtUserName.TabIndex = 2;
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
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(398, 44);
            this.txtPassword.MaxLength = 50;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(164, 22);
            this.txtPassword.TabIndex = 4;
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
            // txtTargetLocation
            // 
            this.txtTargetLocation.Location = new System.Drawing.Point(133, 3);
            this.txtTargetLocation.MaxLength = 256;
            this.txtTargetLocation.Name = "txtTargetLocation";
            this.txtTargetLocation.Size = new System.Drawing.Size(412, 22);
            this.txtTargetLocation.TabIndex = 1;
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
            this.groupBox3.Location = new System.Drawing.Point(5, 115);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(587, 138);
            this.groupBox3.TabIndex = 3;
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
            // txtProxyUserName
            // 
            this.txtProxyUserName.Enabled = false;
            this.txtProxyUserName.Location = new System.Drawing.Point(128, 100);
            this.txtProxyUserName.MaxLength = 50;
            this.txtProxyUserName.Name = "txtProxyUserName";
            this.txtProxyUserName.Size = new System.Drawing.Size(164, 22);
            this.txtProxyUserName.TabIndex = 5;
            // 
            // txtProxyServer
            // 
            this.txtProxyServer.Enabled = false;
            this.txtProxyServer.Location = new System.Drawing.Point(128, 48);
            this.txtProxyServer.MaxLength = 256;
            this.txtProxyServer.Name = "txtProxyServer";
            this.txtProxyServer.Size = new System.Drawing.Size(412, 22);
            this.txtProxyServer.TabIndex = 2;
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
            // txtProxyPassword
            // 
            this.txtProxyPassword.Enabled = false;
            this.txtProxyPassword.Location = new System.Drawing.Point(398, 100);
            this.txtProxyPassword.MaxLength = 50;
            this.txtProxyPassword.Name = "txtProxyPassword";
            this.txtProxyPassword.Size = new System.Drawing.Size(164, 22);
            this.txtProxyPassword.TabIndex = 7;
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
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // DeploymentConfigUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txtTargetLocation);
            this.Controls.Add(this.groupBox3);
            this.Name = "DeploymentConfigUserControl";
            this.Size = new System.Drawing.Size(596, 257);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkUseDefaultCredentials;
        private System.Windows.Forms.TextBox txtTargetLocation;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkUseProxyServer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtProxyUserName;
        private System.Windows.Forms.TextBox txtProxyServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProxyPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkUseProxyDefCreds;
        private System.Windows.Forms.ErrorProvider epErrors;
    }
}
