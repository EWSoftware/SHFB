namespace SandcastleBuilder.Utils.Design
{
    partial class ComponentConfigurationEditorDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentConfigurationEditorDlg));
            this.lbAvailableComponents = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.txtComponentDescription = new System.Windows.Forms.TextBox();
            this.txtComponentCopyright = new System.Windows.Forms.TextBox();
            this.txtComponentVersion = new System.Windows.Forms.TextBox();
            this.lbProjectComponents = new System.Windows.Forms.CheckedListBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.btnAddComponent = new System.Windows.Forms.Button();
            this.btnConfigure = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.gbAvailableComponents = new System.Windows.Forms.GroupBox();
            this.gbProjectAddIns = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gbAvailableComponents.SuspendLayout();
            this.gbProjectAddIns.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbAvailableComponents
            // 
            this.lbAvailableComponents.IntegralHeight = false;
            this.lbAvailableComponents.ItemHeight = 16;
            this.lbAvailableComponents.Location = new System.Drawing.Point(6, 21);
            this.lbAvailableComponents.Name = "lbAvailableComponents";
            this.lbAvailableComponents.Size = new System.Drawing.Size(290, 175);
            this.lbAvailableComponents.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbAvailableComponents, "Select the build component to add to the project");
            this.lbAvailableComponents.TabIndex = 0;
            this.lbAvailableComponents.SelectedIndexChanged += new System.EventHandler(this.lbAvailableComponents_SelectedIndexChanged);
            this.lbAvailableComponents.DoubleClick += new System.EventHandler(this.btnAddComponent_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(562, 427);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnClose, "Close: Close this form");
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtComponentDescription
            // 
            this.txtComponentDescription.Location = new System.Drawing.Point(6, 296);
            this.txtComponentDescription.Multiline = true;
            this.txtComponentDescription.Name = "txtComponentDescription";
            this.txtComponentDescription.ReadOnly = true;
            this.txtComponentDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComponentDescription.Size = new System.Drawing.Size(290, 105);
            this.statusBarTextProvider1.SetStatusBarText(this.txtComponentDescription, "A description of the build component");
            this.txtComponentDescription.TabIndex = 3;
            // 
            // txtComponentCopyright
            // 
            this.txtComponentCopyright.Location = new System.Drawing.Point(6, 230);
            this.txtComponentCopyright.Multiline = true;
            this.txtComponentCopyright.Name = "txtComponentCopyright";
            this.txtComponentCopyright.ReadOnly = true;
            this.txtComponentCopyright.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComponentCopyright.Size = new System.Drawing.Size(290, 60);
            this.statusBarTextProvider1.SetStatusBarText(this.txtComponentCopyright, "Build component copyright information");
            this.txtComponentCopyright.TabIndex = 2;
            // 
            // txtComponentVersion
            // 
            this.txtComponentVersion.Location = new System.Drawing.Point(6, 202);
            this.txtComponentVersion.Name = "txtComponentVersion";
            this.txtComponentVersion.ReadOnly = true;
            this.txtComponentVersion.Size = new System.Drawing.Size(290, 22);
            this.statusBarTextProvider1.SetStatusBarText(this.txtComponentVersion, "Build component version information");
            this.txtComponentVersion.TabIndex = 1;
            // 
            // lbProjectComponents
            // 
            this.lbProjectComponents.Font = new System.Drawing.Font("Tahoma", 8F);
            this.lbProjectComponents.IntegralHeight = false;
            this.lbProjectComponents.Location = new System.Drawing.Point(6, 21);
            this.lbProjectComponents.Name = "lbProjectComponents";
            this.lbProjectComponents.Size = new System.Drawing.Size(318, 340);
            this.lbProjectComponents.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbProjectComponents, "Select the build component to configure");
            this.lbProjectComponents.TabIndex = 0;
            this.lbProjectComponents.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbProjectComponents_ItemCheck);
            // 
            // btnDelete
            // 
            this.btnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDelete.ImageIndex = 1;
            this.btnDelete.ImageList = this.ilImages;
            this.btnDelete.Location = new System.Drawing.Point(224, 367);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnDelete.Size = new System.Drawing.Size(100, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnDelete, "Delete: Delete the selected build component from the project");
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnDelete, "Delete the selected build component from the project");
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Magenta;
            this.ilImages.Images.SetKeyName(0, "ComponentAdd.png");
            this.ilImages.Images.SetKeyName(1, "Delete.bmp");
            this.ilImages.Images.SetKeyName(2, "MoveUp.bmp");
            this.ilImages.Images.SetKeyName(3, "MoveDown.bmp");
            this.ilImages.Images.SetKeyName(4, "Properties.bmp");
            // 
            // btnAddComponent
            // 
            this.btnAddComponent.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddComponent.ImageIndex = 0;
            this.btnAddComponent.ImageList = this.ilImages;
            this.btnAddComponent.Location = new System.Drawing.Point(6, 367);
            this.btnAddComponent.Name = "btnAddComponent";
            this.btnAddComponent.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnAddComponent.Size = new System.Drawing.Size(100, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddComponent, "Add Build Component: Add the selected build component to the project");
            this.btnAddComponent.TabIndex = 1;
            this.btnAddComponent.Text = "&Add";
            this.toolTip1.SetToolTip(this.btnAddComponent, "Add the selected build component to the project");
            this.btnAddComponent.Click += new System.EventHandler(this.btnAddComponent_Click);
            // 
            // btnConfigure
            // 
            this.btnConfigure.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfigure.ImageIndex = 4;
            this.btnConfigure.ImageList = this.ilImages;
            this.btnConfigure.Location = new System.Drawing.Point(112, 367);
            this.btnConfigure.Name = "btnConfigure";
            this.btnConfigure.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.btnConfigure.Size = new System.Drawing.Size(106, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnConfigure, "Configure: Configure the selected build component");
            this.btnConfigure.TabIndex = 2;
            this.btnConfigure.Text = "&Configure";
            this.btnConfigure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnConfigure, "Edit the selected build component\'s configuration");
            this.btnConfigure.UseVisualStyleBackColor = true;
            this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(468, 427);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnHelp, "Help: View help for this form");
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "&Help";
            this.toolTip1.SetToolTip(this.btnHelp, "View help for this form");
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // gbAvailableComponents
            // 
            this.gbAvailableComponents.Controls.Add(this.txtComponentVersion);
            this.gbAvailableComponents.Controls.Add(this.txtComponentCopyright);
            this.gbAvailableComponents.Controls.Add(this.txtComponentDescription);
            this.gbAvailableComponents.Controls.Add(this.lbAvailableComponents);
            this.gbAvailableComponents.Location = new System.Drawing.Point(12, 12);
            this.gbAvailableComponents.Name = "gbAvailableComponents";
            this.gbAvailableComponents.Size = new System.Drawing.Size(302, 407);
            this.gbAvailableComponents.TabIndex = 0;
            this.gbAvailableComponents.TabStop = false;
            this.gbAvailableComponents.Text = "Available Build Components";
            // 
            // gbProjectAddIns
            // 
            this.gbProjectAddIns.Controls.Add(this.btnConfigure);
            this.gbProjectAddIns.Controls.Add(this.btnDelete);
            this.gbProjectAddIns.Controls.Add(this.btnAddComponent);
            this.gbProjectAddIns.Controls.Add(this.lbProjectComponents);
            this.gbProjectAddIns.Location = new System.Drawing.Point(320, 12);
            this.gbProjectAddIns.Name = "gbProjectAddIns";
            this.gbProjectAddIns.Size = new System.Drawing.Size(330, 407);
            this.gbProjectAddIns.TabIndex = 1;
            this.gbProjectAddIns.TabStop = false;
            this.gbProjectAddIns.Text = "Custom Configurations in This Project";
            // 
            // ComponentConfigurationEditorDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(662, 471);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.gbProjectAddIns);
            this.Controls.Add(this.gbAvailableComponents);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ComponentConfigurationEditorDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select and Configure Build Components";
            this.gbAvailableComponents.ResumeLayout(false);
            this.gbAvailableComponents.PerformLayout();
            this.gbProjectAddIns.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbAvailableComponents;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox gbAvailableComponents;
        private System.Windows.Forms.TextBox txtComponentVersion;
        private System.Windows.Forms.TextBox txtComponentCopyright;
        private System.Windows.Forms.TextBox txtComponentDescription;
        private System.Windows.Forms.GroupBox gbProjectAddIns;
        private System.Windows.Forms.CheckedListBox lbProjectComponents;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAddComponent;
        private System.Windows.Forms.Button btnConfigure;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnHelp;
    }
}