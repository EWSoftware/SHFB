namespace SandcastleBuilder.Package.PropertyPages
{
    partial class MSHelp2PropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MSHelp2PropertiesPageControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cboMSHelp2SdkLinkType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkIncludeStopWordList = new System.Windows.Forms.CheckBox();
            this.cboCollectionTocStyle = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtPlugInNamespaces = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtHelpFileVersion = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnAddAttribute = new System.Windows.Forms.Button();
            this.btnDeleteAttribute = new System.Windows.Forms.Button();
            this.btnDefaultAttributes = new System.Windows.Forms.Button();
            this.dgvHelpAttributes = new System.Windows.Forms.DataGridView();
            this.tbcName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbcValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHelpAttributes)).BeginInit();
            this.SuspendLayout();
            // 
            // cboMSHelp2SdkLinkType
            // 
            this.cboMSHelp2SdkLinkType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMSHelp2SdkLinkType.FormattingEnabled = true;
            this.cboMSHelp2SdkLinkType.Location = new System.Drawing.Point(195, 14);
            this.cboMSHelp2SdkLinkType.MaxDropDownItems = 16;
            this.cboMSHelp2SdkLinkType.Name = "cboMSHelp2SdkLinkType";
            this.cboMSHelp2SdkLinkType.Size = new System.Drawing.Size(302, 28);
            this.cboMSHelp2SdkLinkType.TabIndex = 1;
            this.cboMSHelp2SdkLinkType.Tag = "MSHelp2SdkLinkType";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(181, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "MS Help &2 SDK link type";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkIncludeStopWordList
            // 
            this.chkIncludeStopWordList.AutoSize = true;
            this.chkIncludeStopWordList.Checked = true;
            this.chkIncludeStopWordList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeStopWordList.Location = new System.Drawing.Point(195, 148);
            this.chkIncludeStopWordList.Name = "chkIncludeStopWordList";
            this.chkIncludeStopWordList.Size = new System.Drawing.Size(513, 24);
            this.chkIncludeStopWordList.TabIndex = 8;
            this.chkIncludeStopWordList.Tag = "IncludeStopWordList";
            this.chkIncludeStopWordList.Text = "Include the stop word list to omit common words from the full text index";
            this.chkIncludeStopWordList.UseVisualStyleBackColor = true;
            // 
            // cboCollectionTocStyle
            // 
            this.cboCollectionTocStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.epNotes.SetError(this.cboCollectionTocStyle, "This determines the top level style when plugged into a Help 2 collection");
            this.cboCollectionTocStyle.FormattingEnabled = true;
            this.epNotes.SetIconPadding(this.cboCollectionTocStyle, 5);
            this.cboCollectionTocStyle.Location = new System.Drawing.Point(195, 48);
            this.cboCollectionTocStyle.MaxDropDownItems = 16;
            this.cboCollectionTocStyle.Name = "cboCollectionTocStyle";
            this.cboCollectionTocStyle.Size = new System.Drawing.Size(392, 28);
            this.cboCollectionTocStyle.TabIndex = 3;
            this.cboCollectionTocStyle.Tag = "CollectionTocStyle";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(35, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Collection TOC style";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // txtPlugInNamespaces
            // 
            this.txtPlugInNamespaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtPlugInNamespaces, "Specify a comma-separated list of namespaces that the collection is plugged\ninto " +
        "when deployed with H2Reg.exe");
            this.epNotes.SetIconPadding(this.txtPlugInNamespaces, 5);
            this.txtPlugInNamespaces.Location = new System.Drawing.Point(195, 82);
            this.txtPlugInNamespaces.Name = "txtPlugInNamespaces";
            this.txtPlugInNamespaces.Size = new System.Drawing.Size(511, 27);
            this.txtPlugInNamespaces.TabIndex = 5;
            this.txtPlugInNamespaces.Tag = "PlugInNamespaces";
            this.txtPlugInNamespaces.Text = "ms.vsipcc+, ms.vsexpresscc+";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(39, 84);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(150, 23);
            this.label9.TabIndex = 4;
            this.label9.Text = "Plug-in namespaces";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHelpFileVersion
            // 
            this.txtHelpFileVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHelpFileVersion.Location = new System.Drawing.Point(195, 115);
            this.txtHelpFileVersion.Name = "txtHelpFileVersion";
            this.txtHelpFileVersion.Size = new System.Drawing.Size(511, 27);
            this.txtHelpFileVersion.TabIndex = 7;
            this.txtHelpFileVersion.Tag = "HelpFileVersion";
            this.txtHelpFileVersion.Text = "1.0.0.0";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(55, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "Help file version";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(26, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(163, 23);
            this.label4.TabIndex = 9;
            this.label4.Text = "H&elp topic attributes";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddAttribute
            // 
            this.btnAddAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddAttribute.Location = new System.Drawing.Point(195, 380);
            this.btnAddAttribute.Name = "btnAddAttribute";
            this.btnAddAttribute.Size = new System.Drawing.Size(88, 32);
            this.btnAddAttribute.TabIndex = 11;
            this.btnAddAttribute.Text = "&Add";
            this.btnAddAttribute.UseVisualStyleBackColor = true;
            this.btnAddAttribute.Click += new System.EventHandler(this.btnAddAttribute_Click);
            // 
            // btnDeleteAttribute
            // 
            this.btnDeleteAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteAttribute.Location = new System.Drawing.Point(289, 380);
            this.btnDeleteAttribute.Name = "btnDeleteAttribute";
            this.btnDeleteAttribute.Size = new System.Drawing.Size(88, 32);
            this.btnDeleteAttribute.TabIndex = 12;
            this.btnDeleteAttribute.Text = "&Delete";
            this.btnDeleteAttribute.UseVisualStyleBackColor = true;
            this.btnDeleteAttribute.Click += new System.EventHandler(this.btnDeleteAttribute_Click);
            // 
            // btnDefaultAttributes
            // 
            this.btnDefaultAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDefaultAttributes.Location = new System.Drawing.Point(383, 380);
            this.btnDefaultAttributes.Name = "btnDefaultAttributes";
            this.btnDefaultAttributes.Size = new System.Drawing.Size(88, 32);
            this.btnDefaultAttributes.TabIndex = 13;
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
            this.dgvHelpAttributes.Location = new System.Drawing.Point(195, 178);
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
            this.dgvHelpAttributes.Size = new System.Drawing.Size(527, 196);
            this.dgvHelpAttributes.TabIndex = 10;
            this.dgvHelpAttributes.Tag = "HelpAttributes";
            // 
            // tbcName
            // 
            this.tbcName.DataPropertyName = "AttributeName";
            this.tbcName.HeaderText = "Name";
            this.tbcName.Name = "tbcName";
            this.tbcName.Width = 200;
            // 
            // tbcValue
            // 
            this.tbcValue.DataPropertyName = "AttributeValue";
            this.tbcValue.HeaderText = "Value";
            this.tbcValue.Name = "tbcValue";
            this.tbcValue.Width = 250;
            // 
            // MSHelp2PropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.btnAddAttribute);
            this.Controls.Add(this.btnDeleteAttribute);
            this.Controls.Add(this.btnDefaultAttributes);
            this.Controls.Add(this.dgvHelpAttributes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtHelpFileVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPlugInNamespaces);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.cboCollectionTocStyle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboMSHelp2SdkLinkType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkIncludeStopWordList);
            this.MinimumSize = new System.Drawing.Size(740, 420);
            this.Name = "MSHelp2PropertiesPageControl";
            this.Size = new System.Drawing.Size(740, 420);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHelpAttributes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboMSHelp2SdkLinkType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkIncludeStopWordList;
        private System.Windows.Forms.ComboBox cboCollectionTocStyle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtHelpFileVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPlugInNamespaces;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnAddAttribute;
        private System.Windows.Forms.Button btnDeleteAttribute;
        private System.Windows.Forms.Button btnDefaultAttributes;
        private System.Windows.Forms.DataGridView dgvHelpAttributes;
        private System.Windows.Forms.DataGridViewTextBoxColumn tbcName;
        private System.Windows.Forms.DataGridViewTextBoxColumn tbcValue;


    }
}
