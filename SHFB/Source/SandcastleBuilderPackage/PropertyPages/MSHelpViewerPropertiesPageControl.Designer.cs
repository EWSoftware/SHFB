namespace SandcastleBuilder.Package.PropertyPages
{
    partial class MSHelpViewerPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MSHelpViewerPropertiesPageControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.txtCatalogProductId = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCatalogVersion = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cboMSHelpViewerSdkLinkType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtVendorName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtProductTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.udcTocOrder = new System.Windows.Forms.NumericUpDown();
            this.txtTopicVersion = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTocParentVersion = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtTocParentId = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtCatalogName = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnAddAttribute = new System.Windows.Forms.Button();
            this.btnDeleteAttribute = new System.Windows.Forms.Button();
            this.btnDefaultAttributes = new System.Windows.Forms.Button();
            this.dgvHelpAttributes = new System.Windows.Forms.DataGridView();
            this.label10 = new System.Windows.Forms.Label();
            this.tbcName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbcValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.udcTocOrder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHelpAttributes)).BeginInit();
            this.SuspendLayout();
            // 
            // txtCatalogProductId
            // 
            this.epNotes.SetError(this.txtCatalogProductId, "Typically left set to the default, \"VS\"");
            this.epNotes.SetIconPadding(this.txtCatalogProductId, 5);
            this.txtCatalogProductId.Location = new System.Drawing.Point(279, 12);
            this.txtCatalogProductId.Name = "txtCatalogProductId";
            this.txtCatalogProductId.Size = new System.Drawing.Size(209, 27);
            this.txtCatalogProductId.TabIndex = 1;
            this.txtCatalogProductId.Tag = "CatalogProductId";
            this.txtCatalogProductId.Text = "VS";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(24, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(249, 23);
            this.label9.TabIndex = 0;
            this.label9.Text = "Help Viewer 1.0 &catalog product ID";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCatalogVersion
            // 
            this.epNotes.SetError(this.txtCatalogVersion, "Typically left set to the default, \"100\"");
            this.epNotes.SetIconPadding(this.txtCatalogVersion, 5);
            this.txtCatalogVersion.Location = new System.Drawing.Point(279, 45);
            this.txtCatalogVersion.Name = "txtCatalogVersion";
            this.txtCatalogVersion.Size = new System.Drawing.Size(209, 27);
            this.txtCatalogVersion.TabIndex = 3;
            this.txtCatalogVersion.Tag = "CatalogVersion";
            this.txtCatalogVersion.Text = "100";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(47, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Help Viewer 1.0 catalog version";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboMSHelpViewerSdkLinkType
            // 
            this.cboMSHelpViewerSdkLinkType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMSHelpViewerSdkLinkType.FormattingEnabled = true;
            this.cboMSHelpViewerSdkLinkType.Location = new System.Drawing.Point(279, 360);
            this.cboMSHelpViewerSdkLinkType.MaxDropDownItems = 16;
            this.cboMSHelpViewerSdkLinkType.Name = "cboMSHelpViewerSdkLinkType";
            this.cboMSHelpViewerSdkLinkType.Size = new System.Drawing.Size(302, 28);
            this.cboMSHelpViewerSdkLinkType.TabIndex = 19;
            this.cboMSHelpViewerSdkLinkType.Tag = "MSHelpViewerSdkLinkType";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(54, 362);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(219, 23);
            this.label2.TabIndex = 18;
            this.label2.Text = "MS Help Viewer SDK link type";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVendorName
            // 
            this.epNotes.SetError(this.txtVendorName, "If not set, \"Vendor Name\" will be used");
            this.epNotes.SetIconPadding(this.txtVendorName, 5);
            this.txtVendorName.Location = new System.Drawing.Point(279, 121);
            this.txtVendorName.Name = "txtVendorName";
            this.txtVendorName.Size = new System.Drawing.Size(445, 27);
            this.txtVendorName.TabIndex = 7;
            this.txtVendorName.Tag = "VendorName";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(163, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "Vendor name";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProductTitle
            // 
            this.epNotes.SetError(this.txtProductTitle, "If not set, the Help Title property value will be used");
            this.epNotes.SetIconPadding(this.txtProductTitle, 5);
            this.txtProductTitle.Location = new System.Drawing.Point(279, 154);
            this.txtProductTitle.Name = "txtProductTitle";
            this.txtProductTitle.Size = new System.Drawing.Size(445, 27);
            this.txtProductTitle.TabIndex = 9;
            this.txtProductTitle.Tag = "ProductTitle";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(167, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 23);
            this.label4.TabIndex = 8;
            this.label4.Text = "Product title";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(98, 208);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(175, 23);
            this.label5.TabIndex = 10;
            this.label5.Text = "&Starting TOC sort order";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // udcTocOrder
            // 
            this.epNotes.SetError(this.udcTocOrder, "Use -1 to let the build engine determine the sort order");
            this.epNotes.SetIconPadding(this.udcTocOrder, 5);
            this.udcTocOrder.Location = new System.Drawing.Point(279, 207);
            this.udcTocOrder.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.udcTocOrder.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.udcTocOrder.Name = "udcTocOrder";
            this.udcTocOrder.Size = new System.Drawing.Size(106, 27);
            this.udcTocOrder.TabIndex = 11;
            this.udcTocOrder.Tag = "TocOrder";
            this.udcTocOrder.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcTocOrder.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // txtTopicVersion
            // 
            this.txtTopicVersion.Location = new System.Drawing.Point(279, 306);
            this.txtTopicVersion.Name = "txtTopicVersion";
            this.txtTopicVersion.Size = new System.Drawing.Size(209, 27);
            this.txtTopicVersion.TabIndex = 17;
            this.txtTopicVersion.Tag = "TopicVersion";
            this.txtTopicVersion.Text = "100";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(78, 308);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(195, 23);
            this.label6.TabIndex = 16;
            this.label6.Text = "Topic version for this file";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTocParentVersion
            // 
            this.txtTocParentVersion.Location = new System.Drawing.Point(279, 273);
            this.txtTocParentVersion.Name = "txtTocParentVersion";
            this.txtTocParentVersion.Size = new System.Drawing.Size(209, 27);
            this.txtTocParentVersion.TabIndex = 15;
            this.txtTocParentVersion.Tag = "TocParentVersion";
            this.txtTocParentVersion.Text = "100";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(82, 275);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(191, 23);
            this.label7.TabIndex = 14;
            this.label7.Text = "TOC parent topic version";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTocParentId
            // 
            this.txtTocParentId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtTocParentId, "Use -1 to place the root elements at the root of the main table of contents");
            this.epNotes.SetIconPadding(this.txtTocParentId, 5);
            this.txtTocParentId.Location = new System.Drawing.Point(279, 240);
            this.txtTocParentId.Name = "txtTocParentId";
            this.txtTocParentId.Size = new System.Drawing.Size(475, 27);
            this.txtTocParentId.TabIndex = 13;
            this.txtTocParentId.Tag = "TocParentId";
            this.txtTocParentId.Text = "-1";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(124, 242);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(149, 23);
            this.label8.TabIndex = 12;
            this.label8.Text = "TOC parent topic ID";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // txtCatalogName
            // 
            this.epNotes.SetError(this.txtCatalogName, "Only specify this if using a custom catalog name.\r\nLeave blank for the default Vi" +
        "sual Studio catalog name.");
            this.epNotes.SetIconPadding(this.txtCatalogName, 5);
            this.txtCatalogName.Location = new System.Drawing.Point(279, 83);
            this.txtCatalogName.Name = "txtCatalogName";
            this.txtCatalogName.Size = new System.Drawing.Size(209, 27);
            this.txtCatalogName.TabIndex = 5;
            this.txtCatalogName.Tag = "CatalogName";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(6, 85);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(267, 23);
            this.label11.TabIndex = 4;
            this.label11.Text = "Help Viewer 2.x content catalog name";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddAttribute
            // 
            this.btnAddAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddAttribute.Location = new System.Drawing.Point(279, 565);
            this.btnAddAttribute.Name = "btnAddAttribute";
            this.btnAddAttribute.Size = new System.Drawing.Size(88, 32);
            this.btnAddAttribute.TabIndex = 22;
            this.btnAddAttribute.Text = "&Add";
            this.btnAddAttribute.UseVisualStyleBackColor = true;
            this.btnAddAttribute.Click += new System.EventHandler(this.btnAddAttribute_Click);
            // 
            // btnDeleteAttribute
            // 
            this.btnDeleteAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteAttribute.Location = new System.Drawing.Point(373, 565);
            this.btnDeleteAttribute.Name = "btnDeleteAttribute";
            this.btnDeleteAttribute.Size = new System.Drawing.Size(88, 32);
            this.btnDeleteAttribute.TabIndex = 23;
            this.btnDeleteAttribute.Text = "&Delete";
            this.btnDeleteAttribute.UseVisualStyleBackColor = true;
            this.btnDeleteAttribute.Click += new System.EventHandler(this.btnDeleteAttribute_Click);
            // 
            // btnDefaultAttributes
            // 
            this.btnDefaultAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDefaultAttributes.Location = new System.Drawing.Point(467, 565);
            this.btnDefaultAttributes.Name = "btnDefaultAttributes";
            this.btnDefaultAttributes.Size = new System.Drawing.Size(88, 32);
            this.btnDefaultAttributes.TabIndex = 24;
            this.btnDefaultAttributes.Text = "Defa&ult";
            this.btnDefaultAttributes.UseVisualStyleBackColor = true;
            this.btnDefaultAttributes.Click += new System.EventHandler(this.btnDefaultAttributes_Click);
            // 
            // dgvHelpAttributes
            // 
            this.dgvHelpAttributes.AllowUserToAddRows = false;
            this.dgvHelpAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvHelpAttributes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvHelpAttributes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvHelpAttributes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHelpAttributes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.tbcName,
            this.tbcValue});
            this.dgvHelpAttributes.Location = new System.Drawing.Point(279, 410);
            this.dgvHelpAttributes.Name = "dgvHelpAttributes";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.dgvHelpAttributes.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvHelpAttributes.RowTemplate.Height = 28;
            this.dgvHelpAttributes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHelpAttributes.Size = new System.Drawing.Size(475, 149);
            this.dgvHelpAttributes.TabIndex = 21;
            this.dgvHelpAttributes.Tag = "HelpAttributes";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(110, 410);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(163, 23);
            this.label10.TabIndex = 20;
            this.label10.Text = "H&elp topic attributes";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbcName
            // 
            this.tbcName.DataPropertyName = "AttributeName";
            this.tbcName.HeaderText = "Name";
            this.tbcName.Name = "tbcName";
            this.tbcName.Width = 150;
            // 
            // tbcValue
            // 
            this.tbcValue.DataPropertyName = "AttributeValue";
            this.tbcValue.HeaderText = "Value";
            this.tbcValue.Name = "tbcValue";
            this.tbcValue.Width = 250;
            // 
            // MSHelpViewerPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.btnAddAttribute);
            this.Controls.Add(this.btnDeleteAttribute);
            this.Controls.Add(this.btnDefaultAttributes);
            this.Controls.Add(this.dgvHelpAttributes);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtCatalogName);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtTocParentId);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtTocParentVersion);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtTopicVersion);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.udcTocOrder);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtProductTitle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtVendorName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboMSHelpViewerSdkLinkType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtCatalogVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCatalogProductId);
            this.Controls.Add(this.label9);
            this.MinimumSize = new System.Drawing.Size(765, 600);
            this.Name = "MSHelpViewerPropertiesPageControl";
            this.Size = new System.Drawing.Size(795, 600);
            ((System.ComponentModel.ISupportInitialize)(this.udcTocOrder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHelpAttributes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCatalogProductId;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCatalogVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtVendorName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtProductTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown udcTocOrder;
        private System.Windows.Forms.TextBox txtTopicVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTocParentVersion;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtTocParentId;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.ComboBox cboMSHelpViewerSdkLinkType;
        private System.Windows.Forms.TextBox txtCatalogName;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnAddAttribute;
        private System.Windows.Forms.Button btnDeleteAttribute;
        private System.Windows.Forms.Button btnDefaultAttributes;
        private System.Windows.Forms.DataGridView dgvHelpAttributes;
        private System.Windows.Forms.DataGridViewTextBoxColumn tbcName;
        private System.Windows.Forms.DataGridViewTextBoxColumn tbcValue;
        private System.Windows.Forms.Label label10;


    }
}
