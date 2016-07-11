namespace SandcastleBuilder.Gui
{
    partial class AboutDlg
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
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnSysInfo = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lvComponents = new System.Windows.Forms.ListView();
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lnkEWoodruffUrl = new System.Windows.Forms.LinkLabel();
            this.lnkProjectUrl = new System.Windows.Forms.LinkLabel();
            this.sbMessage = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.lblCopyright = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lnkHelp
            // 
            this.lnkHelp.Location = new System.Drawing.Point(537, 388);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(237, 26);
            this.sbMessage.SetStatusBarText(this.lnkHelp, "Send e-mail to the help desk");
            this.lnkHelp.TabIndex = 7;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Eric@EWoodruff.us";
            this.toolTip1.SetToolTip(this.lnkHelp, "Send e-mail requesting help");
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_LinkClicked);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Name";
            this.ColumnHeader1.Width = 400;
            // 
            // btnSysInfo
            // 
            this.btnSysInfo.Location = new System.Drawing.Point(630, 497);
            this.btnSysInfo.Name = "btnSysInfo";
            this.btnSysInfo.Size = new System.Drawing.Size(150, 35);
            this.sbMessage.SetStatusBarText(this.btnSysInfo, "System Info: View system information for the PC on which the application is runni" +
        "ng");
            this.btnSysInfo.TabIndex = 10;
            this.btnSysInfo.Text = "System Info...";
            this.toolTip1.SetToolTip(this.btnSysInfo, "Display system information");
            this.btnSysInfo.Click += new System.EventHandler(this.btnSysInfo_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(786, 497);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(150, 35);
            this.sbMessage.SetStatusBarText(this.btnOK, "OK: Close this dialog box");
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Close this dialog box");
            // 
            // lvComponents
            // 
            this.lvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2});
            this.lvComponents.FullRowSelect = true;
            this.lvComponents.GridLines = true;
            this.lvComponents.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvComponents.Location = new System.Drawing.Point(336, 168);
            this.lvComponents.MultiSelect = false;
            this.lvComponents.Name = "lvComponents";
            this.lvComponents.Size = new System.Drawing.Size(600, 180);
            this.sbMessage.SetStatusBarText(this.lvComponents, "Component name and version information");
            this.lvComponents.TabIndex = 4;
            this.lvComponents.UseCompatibleStateImageBehavior = false;
            this.lvComponents.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Version";
            this.ColumnHeader2.Width = 150;
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(336, 139);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(266, 26);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "Product Components";
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(336, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(600, 26);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "<Application Name>";
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(336, 80);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(600, 59);
            this.lblDescription.TabIndex = 2;
            this.lblDescription.Text = "<Description>";
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(336, 44);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(600, 26);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "<Version>";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(336, 388);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(199, 26);
            this.label2.TabIndex = 6;
            this.label2.Text = "For help send e-mail to";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Tan;
            this.pictureBox1.Image = global::SandcastleBuilder.Gui.Properties.Resources.Sandcastle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(312, 520);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // lnkEWoodruffUrl
            // 
            this.lnkEWoodruffUrl.Location = new System.Drawing.Point(336, 420);
            this.lnkEWoodruffUrl.Name = "lnkEWoodruffUrl";
            this.lnkEWoodruffUrl.Size = new System.Drawing.Size(438, 26);
            this.sbMessage.SetStatusBarText(this.lnkEWoodruffUrl, "Open a browser to view www.EWoodruff.us");
            this.lnkEWoodruffUrl.TabIndex = 8;
            this.lnkEWoodruffUrl.TabStop = true;
            this.lnkEWoodruffUrl.Text = "http://www.EWoodruff.us";
            this.toolTip1.SetToolTip(this.lnkEWoodruffUrl, "View www.EWoodruff.us");
            this.lnkEWoodruffUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_LinkClicked);
            // 
            // lnkProjectUrl
            // 
            this.lnkProjectUrl.Location = new System.Drawing.Point(336, 456);
            this.lnkProjectUrl.Name = "lnkProjectUrl";
            this.lnkProjectUrl.Size = new System.Drawing.Size(438, 26);
            this.sbMessage.SetStatusBarText(this.lnkProjectUrl, "View the project website");
            this.lnkProjectUrl.TabIndex = 9;
            this.lnkProjectUrl.TabStop = true;
            this.lnkProjectUrl.Text = "https://GitHub.com/EWSoftware/SHFB";
            this.toolTip1.SetToolTip(this.lnkProjectUrl, "View project website");
            this.lnkProjectUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_LinkClicked);
            // 
            // lblCopyright
            // 
            this.lblCopyright.Location = new System.Drawing.Point(336, 351);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(600, 26);
            this.lblCopyright.TabIndex = 5;
            this.lblCopyright.Text = "<Copyright>";
            // 
            // AboutDlg
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(948, 544);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.lnkProjectUrl);
            this.Controls.Add(this.lnkEWoodruffUrl);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnSysInfo);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lvComponents);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About";
            this.Load += new System.EventHandler(this.AboutDlg_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ColumnHeader ColumnHeader1;
        private System.Windows.Forms.ColumnHeader ColumnHeader2;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button btnSysInfo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListView lvComponents;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbMessage;
        private System.Windows.Forms.LinkLabel lnkEWoodruffUrl;
        private System.Windows.Forms.LinkLabel lnkProjectUrl;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblVersion;
    }
}
