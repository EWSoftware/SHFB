namespace SandcastleBuilder.Utils.Design
{
    partial class PlugInConfigurationEditorDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlugInConfigurationEditorDlg));
            this.lbAvailablePlugIns = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.txtPlugInDescription = new System.Windows.Forms.TextBox();
            this.txtPlugInCopyright = new System.Windows.Forms.TextBox();
            this.txtPlugInVersion = new System.Windows.Forms.TextBox();
            this.lbProjectPlugIns = new System.Windows.Forms.CheckedListBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.btnAddPlugIn = new System.Windows.Forms.Button();
            this.btnConfigure = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.gbAvailablePlugIns = new System.Windows.Forms.GroupBox();
            this.gbProjectAddIns = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gbAvailablePlugIns.SuspendLayout();
            this.gbProjectAddIns.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbAvailablePlugIns
            // 
            this.lbAvailablePlugIns.IntegralHeight = false;
            this.lbAvailablePlugIns.ItemHeight = 16;
            this.lbAvailablePlugIns.Location = new System.Drawing.Point(6, 21);
            this.lbAvailablePlugIns.Name = "lbAvailablePlugIns";
            this.lbAvailablePlugIns.Size = new System.Drawing.Size(337, 273);
            this.lbAvailablePlugIns.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbAvailablePlugIns, "Select the plug-in to add to the project");
            this.lbAvailablePlugIns.TabIndex = 0;
            this.lbAvailablePlugIns.SelectedIndexChanged += new System.EventHandler(this.lbAvailablePlugIns_SelectedIndexChanged);
            this.lbAvailablePlugIns.DoubleClick += new System.EventHandler(this.btnAddPlugIn_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(656, 523);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnClose, "Close: Close this form");
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtPlugInDescription
            // 
            this.txtPlugInDescription.Location = new System.Drawing.Point(6, 394);
            this.txtPlugInDescription.Multiline = true;
            this.txtPlugInDescription.Name = "txtPlugInDescription";
            this.txtPlugInDescription.ReadOnly = true;
            this.txtPlugInDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlugInDescription.Size = new System.Drawing.Size(337, 105);
            this.statusBarTextProvider1.SetStatusBarText(this.txtPlugInDescription, "A description of the plug-in");
            this.txtPlugInDescription.TabIndex = 3;
            // 
            // txtPlugInCopyright
            // 
            this.txtPlugInCopyright.Location = new System.Drawing.Point(6, 328);
            this.txtPlugInCopyright.Multiline = true;
            this.txtPlugInCopyright.Name = "txtPlugInCopyright";
            this.txtPlugInCopyright.ReadOnly = true;
            this.txtPlugInCopyright.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlugInCopyright.Size = new System.Drawing.Size(337, 60);
            this.statusBarTextProvider1.SetStatusBarText(this.txtPlugInCopyright, "Plug-in copyright information");
            this.txtPlugInCopyright.TabIndex = 2;
            // 
            // txtPlugInVersion
            // 
            this.txtPlugInVersion.Location = new System.Drawing.Point(6, 300);
            this.txtPlugInVersion.Name = "txtPlugInVersion";
            this.txtPlugInVersion.ReadOnly = true;
            this.txtPlugInVersion.Size = new System.Drawing.Size(337, 22);
            this.statusBarTextProvider1.SetStatusBarText(this.txtPlugInVersion, "Plug-in version information");
            this.txtPlugInVersion.TabIndex = 1;
            // 
            // lbProjectPlugIns
            // 
            this.lbProjectPlugIns.Font = new System.Drawing.Font("Tahoma", 8F);
            this.lbProjectPlugIns.IntegralHeight = false;
            this.lbProjectPlugIns.Location = new System.Drawing.Point(6, 21);
            this.lbProjectPlugIns.Name = "lbProjectPlugIns";
            this.lbProjectPlugIns.Size = new System.Drawing.Size(365, 436);
            this.lbProjectPlugIns.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbProjectPlugIns, "Select the plug-in to configure");
            this.lbProjectPlugIns.TabIndex = 0;
            this.lbProjectPlugIns.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbProjectPlugIns_ItemCheck);
            // 
            // btnDelete
            // 
            this.btnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDelete.ImageIndex = 1;
            this.btnDelete.ImageList = this.ilImages;
            this.btnDelete.Location = new System.Drawing.Point(271, 463);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnDelete.Size = new System.Drawing.Size(100, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnDelete, "Delete: Delete the selected plug-in from the project");
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnDelete, "Delete the selected plug-in from the project");
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Magenta;
            this.ilImages.Images.SetKeyName(0, "PlugInAdd.png");
            this.ilImages.Images.SetKeyName(1, "Delete.bmp");
            this.ilImages.Images.SetKeyName(2, "MoveUp.bmp");
            this.ilImages.Images.SetKeyName(3, "MoveDown.bmp");
            this.ilImages.Images.SetKeyName(4, "Properties.bmp");
            // 
            // btnAddPlugIn
            // 
            this.btnAddPlugIn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddPlugIn.ImageIndex = 0;
            this.btnAddPlugIn.ImageList = this.ilImages;
            this.btnAddPlugIn.Location = new System.Drawing.Point(6, 463);
            this.btnAddPlugIn.Name = "btnAddPlugIn";
            this.btnAddPlugIn.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnAddPlugIn.Size = new System.Drawing.Size(100, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddPlugIn, "Add Plug-In: Add the selected plug-in to the project");
            this.btnAddPlugIn.TabIndex = 1;
            this.btnAddPlugIn.Text = "&Add";
            this.toolTip1.SetToolTip(this.btnAddPlugIn, "Add the selected plug-in to the project");
            this.btnAddPlugIn.Click += new System.EventHandler(this.btnAddPlugIn_Click);
            // 
            // btnConfigure
            // 
            this.btnConfigure.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfigure.ImageIndex = 4;
            this.btnConfigure.ImageList = this.ilImages;
            this.btnConfigure.Location = new System.Drawing.Point(135, 463);
            this.btnConfigure.Name = "btnConfigure";
            this.btnConfigure.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.btnConfigure.Size = new System.Drawing.Size(106, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnConfigure, "Configure: Configure the selected plug-in");
            this.btnConfigure.TabIndex = 2;
            this.btnConfigure.Text = "&Configure";
            this.btnConfigure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnConfigure, "Edit the selected plug-in\'s configuration");
            this.btnConfigure.UseVisualStyleBackColor = true;
            this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(562, 523);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnHelp, "Help: View help for this form");
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "&Help";
            this.toolTip1.SetToolTip(this.btnHelp, "View help for this form");
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // gbAvailablePlugIns
            // 
            this.gbAvailablePlugIns.Controls.Add(this.txtPlugInVersion);
            this.gbAvailablePlugIns.Controls.Add(this.txtPlugInCopyright);
            this.gbAvailablePlugIns.Controls.Add(this.txtPlugInDescription);
            this.gbAvailablePlugIns.Controls.Add(this.lbAvailablePlugIns);
            this.gbAvailablePlugIns.Location = new System.Drawing.Point(12, 12);
            this.gbAvailablePlugIns.Name = "gbAvailablePlugIns";
            this.gbAvailablePlugIns.Size = new System.Drawing.Size(349, 505);
            this.gbAvailablePlugIns.TabIndex = 0;
            this.gbAvailablePlugIns.TabStop = false;
            this.gbAvailablePlugIns.Text = "Available Plug-Ins";
            // 
            // gbProjectAddIns
            // 
            this.gbProjectAddIns.Controls.Add(this.btnConfigure);
            this.gbProjectAddIns.Controls.Add(this.btnDelete);
            this.gbProjectAddIns.Controls.Add(this.btnAddPlugIn);
            this.gbProjectAddIns.Controls.Add(this.lbProjectPlugIns);
            this.gbProjectAddIns.Location = new System.Drawing.Point(367, 12);
            this.gbProjectAddIns.Name = "gbProjectAddIns";
            this.gbProjectAddIns.Size = new System.Drawing.Size(377, 505);
            this.gbProjectAddIns.TabIndex = 1;
            this.gbProjectAddIns.TabStop = false;
            this.gbProjectAddIns.Text = "Plug-Ins in This Project";
            // 
            // PlugInConfigurationEditorDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(756, 567);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.gbProjectAddIns);
            this.Controls.Add(this.gbAvailablePlugIns);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlugInConfigurationEditorDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select and Configure Build Process Plug-Ins";
            this.gbAvailablePlugIns.ResumeLayout(false);
            this.gbAvailablePlugIns.PerformLayout();
            this.gbProjectAddIns.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbAvailablePlugIns;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox gbAvailablePlugIns;
        private System.Windows.Forms.TextBox txtPlugInVersion;
        private System.Windows.Forms.TextBox txtPlugInCopyright;
        private System.Windows.Forms.TextBox txtPlugInDescription;
        private System.Windows.Forms.GroupBox gbProjectAddIns;
        private System.Windows.Forms.CheckedListBox lbProjectPlugIns;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAddPlugIn;
        private System.Windows.Forms.Button btnConfigure;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnHelp;
    }
}