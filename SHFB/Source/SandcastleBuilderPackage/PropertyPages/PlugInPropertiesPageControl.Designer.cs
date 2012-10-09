namespace SandcastleBuilder.Package.PropertyPages
{
    partial class PlugInPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlugInPropertiesPageControl));
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.gbProjectAddIns = new System.Windows.Forms.GroupBox();
            this.btnConfigure = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAddPlugIn = new System.Windows.Forms.Button();
            this.lbProjectPlugIns = new System.Windows.Forms.CheckedListBox();
            this.gbAvailablePlugIns = new System.Windows.Forms.GroupBox();
            this.txtPlugInVersion = new System.Windows.Forms.TextBox();
            this.txtPlugInCopyright = new System.Windows.Forms.TextBox();
            this.txtPlugInDescription = new System.Windows.Forms.TextBox();
            this.lbAvailablePlugIns = new System.Windows.Forms.ListBox();
            this.gbProjectAddIns.SuspendLayout();
            this.gbAvailablePlugIns.SuspendLayout();
            this.SuspendLayout();
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
            // gbProjectAddIns
            // 
            this.gbProjectAddIns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbProjectAddIns.Controls.Add(this.btnConfigure);
            this.gbProjectAddIns.Controls.Add(this.btnDelete);
            this.gbProjectAddIns.Controls.Add(this.btnAddPlugIn);
            this.gbProjectAddIns.Controls.Add(this.lbProjectPlugIns);
            this.gbProjectAddIns.Location = new System.Drawing.Point(355, 3);
            this.gbProjectAddIns.Name = "gbProjectAddIns";
            this.gbProjectAddIns.Size = new System.Drawing.Size(346, 542);
            this.gbProjectAddIns.TabIndex = 1;
            this.gbProjectAddIns.TabStop = false;
            this.gbProjectAddIns.Text = "&Plug-Ins in This Project";
            // 
            // btnConfigure
            // 
            this.btnConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnConfigure.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfigure.ImageIndex = 4;
            this.btnConfigure.ImageList = this.ilImages;
            this.btnConfigure.Location = new System.Drawing.Point(120, 502);
            this.btnConfigure.Name = "btnConfigure";
            this.btnConfigure.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.btnConfigure.Size = new System.Drawing.Size(106, 32);
            this.btnConfigure.TabIndex = 2;
            this.btnConfigure.Text = "&Configure";
            this.btnConfigure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConfigure.UseVisualStyleBackColor = true;
            this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDelete.ImageIndex = 1;
            this.btnDelete.ImageList = this.ilImages;
            this.btnDelete.Location = new System.Drawing.Point(240, 502);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnDelete.Size = new System.Drawing.Size(100, 32);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAddPlugIn
            // 
            this.btnAddPlugIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddPlugIn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddPlugIn.ImageIndex = 0;
            this.btnAddPlugIn.ImageList = this.ilImages;
            this.btnAddPlugIn.Location = new System.Drawing.Point(6, 502);
            this.btnAddPlugIn.Name = "btnAddPlugIn";
            this.btnAddPlugIn.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnAddPlugIn.Size = new System.Drawing.Size(100, 32);
            this.btnAddPlugIn.TabIndex = 1;
            this.btnAddPlugIn.Text = "&Add";
            this.btnAddPlugIn.Click += new System.EventHandler(this.btnAddPlugIn_Click);
            // 
            // lbProjectPlugIns
            // 
            this.lbProjectPlugIns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbProjectPlugIns.IntegralHeight = false;
            this.lbProjectPlugIns.Location = new System.Drawing.Point(6, 21);
            this.lbProjectPlugIns.Name = "lbProjectPlugIns";
            this.lbProjectPlugIns.Size = new System.Drawing.Size(334, 475);
            this.lbProjectPlugIns.Sorted = true;
            this.lbProjectPlugIns.TabIndex = 0;
            this.lbProjectPlugIns.Tag = "PlugInConfigurations";
            this.lbProjectPlugIns.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbProjectPlugIns_ItemCheck);
            // 
            // gbAvailablePlugIns
            // 
            this.gbAvailablePlugIns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbAvailablePlugIns.Controls.Add(this.txtPlugInVersion);
            this.gbAvailablePlugIns.Controls.Add(this.txtPlugInCopyright);
            this.gbAvailablePlugIns.Controls.Add(this.txtPlugInDescription);
            this.gbAvailablePlugIns.Controls.Add(this.lbAvailablePlugIns);
            this.gbAvailablePlugIns.Location = new System.Drawing.Point(3, 3);
            this.gbAvailablePlugIns.Name = "gbAvailablePlugIns";
            this.gbAvailablePlugIns.Size = new System.Drawing.Size(346, 542);
            this.gbAvailablePlugIns.TabIndex = 0;
            this.gbAvailablePlugIns.TabStop = false;
            this.gbAvailablePlugIns.Text = "A&vailable Plug-Ins";
            // 
            // txtPlugInVersion
            // 
            this.txtPlugInVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPlugInVersion.Location = new System.Drawing.Point(6, 285);
            this.txtPlugInVersion.Name = "txtPlugInVersion";
            this.txtPlugInVersion.ReadOnly = true;
            this.txtPlugInVersion.Size = new System.Drawing.Size(334, 27);
            this.txtPlugInVersion.TabIndex = 1;
            // 
            // txtPlugInCopyright
            // 
            this.txtPlugInCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPlugInCopyright.Location = new System.Drawing.Point(6, 318);
            this.txtPlugInCopyright.Multiline = true;
            this.txtPlugInCopyright.Name = "txtPlugInCopyright";
            this.txtPlugInCopyright.ReadOnly = true;
            this.txtPlugInCopyright.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlugInCopyright.Size = new System.Drawing.Size(334, 55);
            this.txtPlugInCopyright.TabIndex = 2;
            // 
            // txtPlugInDescription
            // 
            this.txtPlugInDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPlugInDescription.Location = new System.Drawing.Point(6, 379);
            this.txtPlugInDescription.Multiline = true;
            this.txtPlugInDescription.Name = "txtPlugInDescription";
            this.txtPlugInDescription.ReadOnly = true;
            this.txtPlugInDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlugInDescription.Size = new System.Drawing.Size(334, 155);
            this.txtPlugInDescription.TabIndex = 3;
            // 
            // lbAvailablePlugIns
            // 
            this.lbAvailablePlugIns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbAvailablePlugIns.IntegralHeight = false;
            this.lbAvailablePlugIns.ItemHeight = 20;
            this.lbAvailablePlugIns.Location = new System.Drawing.Point(6, 21);
            this.lbAvailablePlugIns.Name = "lbAvailablePlugIns";
            this.lbAvailablePlugIns.Size = new System.Drawing.Size(334, 258);
            this.lbAvailablePlugIns.Sorted = true;
            this.lbAvailablePlugIns.TabIndex = 0;
            this.lbAvailablePlugIns.SelectedIndexChanged += new System.EventHandler(this.lbAvailablePlugIns_SelectedIndexChanged);
            this.lbAvailablePlugIns.DoubleClick += new System.EventHandler(this.btnAddPlugIn_Click);
            // 
            // PlugInPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.gbProjectAddIns);
            this.Controls.Add(this.gbAvailablePlugIns);
            this.MinimumSize = new System.Drawing.Size(705, 550);
            this.Name = "PlugInPropertiesPageControl";
            this.Size = new System.Drawing.Size(705, 550);
            this.gbProjectAddIns.ResumeLayout(false);
            this.gbAvailablePlugIns.ResumeLayout(false);
            this.gbAvailablePlugIns.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.GroupBox gbProjectAddIns;
        private System.Windows.Forms.Button btnConfigure;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAddPlugIn;
        private System.Windows.Forms.CheckedListBox lbProjectPlugIns;
        private System.Windows.Forms.GroupBox gbAvailablePlugIns;
        private System.Windows.Forms.TextBox txtPlugInVersion;
        private System.Windows.Forms.TextBox txtPlugInCopyright;
        private System.Windows.Forms.TextBox txtPlugInDescription;
        private System.Windows.Forms.ListBox lbAvailablePlugIns;



    }
}
