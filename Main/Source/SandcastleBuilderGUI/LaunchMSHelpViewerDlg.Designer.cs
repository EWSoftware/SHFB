namespace SandcastleBuilder.Gui
{
    partial class LaunchMSHelpViewerDlg
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
            if(disposing)
            {
                if(actionThread != null)
                    actionThread.Dispose();

                if(components != null)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LaunchMSHelpViewerDlg));
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.rbOpenCurrent = new System.Windows.Forms.RadioButton();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.rbRemove = new System.Windows.Forms.RadioButton();
            this.rbInstall = new System.Windows.Forms.RadioButton();
            this.lblAction = new System.Windows.Forms.Label();
            this.pbWait = new System.Windows.Forms.PictureBox();
            this.grpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).BeginInit();
            this.SuspendLayout();
            // 
            // txtInfo
            // 
            this.txtInfo.BackColor = System.Drawing.SystemColors.Window;
            this.txtInfo.Location = new System.Drawing.Point(12, 12);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInfo.Size = new System.Drawing.Size(662, 166);
            this.statusBarTextProvider1.SetStatusBarText(this.txtInfo, "Information about the help content\'s state");
            this.txtInfo.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(586, 314);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnCancel, "Cancel: Close without opening the help file");
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(12, 314);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnOK, "OK: Execute the selected action");
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // rbOpenCurrent
            // 
            this.rbOpenCurrent.AutoSize = true;
            this.rbOpenCurrent.Location = new System.Drawing.Point(40, 34);
            this.rbOpenCurrent.Name = "rbOpenCurrent";
            this.rbOpenCurrent.Size = new System.Drawing.Size(497, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.rbOpenCurrent, "Open the currently installed help content even if it is out of date");
            this.rbOpenCurrent.TabIndex = 0;
            this.rbOpenCurrent.TabStop = true;
            this.rbOpenCurrent.Text = "&Open currently installed help file content for viewing even if it is out of date" +
                "";
            this.rbOpenCurrent.UseVisualStyleBackColor = true;
            // 
            // grpOptions
            // 
            this.grpOptions.Controls.Add(this.rbRemove);
            this.grpOptions.Controls.Add(this.rbInstall);
            this.grpOptions.Controls.Add(this.rbOpenCurrent);
            this.grpOptions.Enabled = false;
            this.grpOptions.Location = new System.Drawing.Point(12, 184);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(662, 124);
            this.grpOptions.TabIndex = 0;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Options";
            // 
            // rbRemove
            // 
            this.rbRemove.AutoSize = true;
            this.rbRemove.Location = new System.Drawing.Point(40, 88);
            this.rbRemove.Name = "rbRemove";
            this.rbRemove.Size = new System.Drawing.Size(212, 21);
            this.rbRemove.TabIndex = 2;
            this.rbRemove.TabStop = true;
            this.rbRemove.Text = "&Remove the installed content";
            this.rbRemove.UseVisualStyleBackColor = true;
            // 
            // rbInstall
            // 
            this.rbInstall.AutoSize = true;
            this.rbInstall.Location = new System.Drawing.Point(40, 61);
            this.rbInstall.Name = "rbInstall";
            this.rbInstall.Size = new System.Drawing.Size(592, 21);
            this.rbInstall.TabIndex = 1;
            this.rbInstall.TabStop = true;
            this.rbInstall.Text = "&Install content from last build, replacing any existing installed copy, and open" +
                " it for viewing";
            this.rbInstall.UseVisualStyleBackColor = true;
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.BackColor = System.Drawing.SystemColors.Window;
            this.lblAction.Location = new System.Drawing.Point(292, 86);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(105, 17);
            this.lblAction.TabIndex = 10;
            this.lblAction.Text = "Taking action...";
            this.lblAction.Visible = false;
            // 
            // pbWait
            // 
            this.pbWait.BackColor = System.Drawing.SystemColors.Window;
            this.pbWait.Image = ((System.Drawing.Image)(resources.GetObject("pbWait.Image")));
            this.pbWait.Location = new System.Drawing.Point(254, 78);
            this.pbWait.Name = "pbWait";
            this.pbWait.Size = new System.Drawing.Size(32, 32);
            this.pbWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbWait.TabIndex = 11;
            this.pbWait.TabStop = false;
            this.pbWait.Visible = false;
            // 
            // LaunchMSHelpViewerDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(686, 358);
            this.Controls.Add(this.lblAction);
            this.Controls.Add(this.pbWait);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grpOptions);
            this.Controls.Add(this.txtInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LaunchMSHelpViewerDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Microsoft Help Viewer Content";
            this.Load += new System.EventHandler(this.LaunchMSHelpViewerDlg_Load);
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInfo;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.RadioButton rbRemove;
        private System.Windows.Forms.RadioButton rbInstall;
        private System.Windows.Forms.RadioButton rbOpenCurrent;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.PictureBox pbWait;
    }
}