namespace SandcastleBuilder.Components
{
    partial class PostTransformConfigDlg
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
                if(bmImage != null)
                    bmImage.Dispose();

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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lnkCodePlexSHFB = new System.Windows.Forms.LinkLabel();
            this.btnSelectStylesheet = new System.Windows.Forms.Button();
            this.btnSelectScript = new System.Windows.Forms.Button();
            this.btnSelectLogo = new System.Windows.Forms.Button();
            this.btnSelectImage = new System.Windows.Forms.Button();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtStylesheet = new System.Windows.Forms.TextBox();
            this.txtScriptFile = new System.Windows.Forms.TextBox();
            this.txtLogoFile = new System.Windows.Forms.TextBox();
            this.txtAltText = new System.Windows.Forms.TextBox();
            this.txtCopyImage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabConfig = new System.Windows.Forms.TabControl();
            this.pgGeneral = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.pgLogo = new System.Windows.Forms.TabPage();
            this.cboAlignment = new System.Windows.Forms.ComboBox();
            this.cboPlacement = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.chkProportional = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pnlImage = new System.Windows.Forms.Panel();
            this.udcHeight = new System.Windows.Forms.NumericUpDown();
            this.udcWidth = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblActualSize = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.tabConfig.SuspendLayout();
            this.pgGeneral.SuspendLayout();
            this.pgLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(658, 421);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 421);
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
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(270, 426);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 2;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCodePlexSHFB_LinkClicked);
            // 
            // btnSelectStylesheet
            // 
            this.btnSelectStylesheet.Location = new System.Drawing.Point(666, 63);
            this.btnSelectStylesheet.Name = "btnSelectStylesheet";
            this.btnSelectStylesheet.Size = new System.Drawing.Size(32, 25);
            this.btnSelectStylesheet.TabIndex = 2;
            this.btnSelectStylesheet.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectStylesheet, "Select the colorizer stylesheet filename");
            this.btnSelectStylesheet.UseVisualStyleBackColor = true;
            this.btnSelectStylesheet.Click += new System.EventHandler(this.SelectFile_Click);
            // 
            // btnSelectScript
            // 
            this.btnSelectScript.Location = new System.Drawing.Point(666, 140);
            this.btnSelectScript.Name = "btnSelectScript";
            this.btnSelectScript.Size = new System.Drawing.Size(32, 25);
            this.btnSelectScript.TabIndex = 5;
            this.btnSelectScript.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectScript, "Select the colorizer JavaScript file");
            this.btnSelectScript.UseVisualStyleBackColor = true;
            this.btnSelectScript.Click += new System.EventHandler(this.SelectFile_Click);
            // 
            // btnSelectLogo
            // 
            this.btnSelectLogo.Location = new System.Drawing.Point(666, 30);
            this.btnSelectLogo.Name = "btnSelectLogo";
            this.btnSelectLogo.Size = new System.Drawing.Size(32, 25);
            this.btnSelectLogo.TabIndex = 2;
            this.btnSelectLogo.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectLogo, "Select the logo file to use");
            this.btnSelectLogo.UseVisualStyleBackColor = true;
            this.btnSelectLogo.Click += new System.EventHandler(this.SelectImage_Click);
            // 
            // btnSelectImage
            // 
            this.btnSelectImage.Location = new System.Drawing.Point(666, 214);
            this.btnSelectImage.Name = "btnSelectImage";
            this.btnSelectImage.Size = new System.Drawing.Size(32, 25);
            this.btnSelectImage.TabIndex = 8;
            this.btnSelectImage.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectImage, "Select the colorizer \"Copy\" image file");
            this.btnSelectImage.UseVisualStyleBackColor = true;
            this.btnSelectImage.Click += new System.EventHandler(this.SelectImage_Click);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // txtStylesheet
            // 
            this.epErrors.SetIconPadding(this.txtStylesheet, 35);
            this.txtStylesheet.Location = new System.Drawing.Point(29, 65);
            this.txtStylesheet.MaxLength = 256;
            this.txtStylesheet.Name = "txtStylesheet";
            this.txtStylesheet.Size = new System.Drawing.Size(636, 22);
            this.txtStylesheet.TabIndex = 1;
            // 
            // txtScriptFile
            // 
            this.epErrors.SetIconPadding(this.txtScriptFile, 35);
            this.txtScriptFile.Location = new System.Drawing.Point(29, 141);
            this.txtScriptFile.MaxLength = 256;
            this.txtScriptFile.Name = "txtScriptFile";
            this.txtScriptFile.Size = new System.Drawing.Size(636, 22);
            this.txtScriptFile.TabIndex = 4;
            // 
            // txtLogoFile
            // 
            this.epErrors.SetIconPadding(this.txtLogoFile, 35);
            this.txtLogoFile.Location = new System.Drawing.Point(29, 32);
            this.txtLogoFile.MaxLength = 256;
            this.txtLogoFile.Name = "txtLogoFile";
            this.txtLogoFile.Size = new System.Drawing.Size(636, 22);
            this.txtLogoFile.TabIndex = 1;
            this.txtLogoFile.Leave += new System.EventHandler(this.txtLogoFile_Leave);
            // 
            // txtAltText
            // 
            this.epErrors.SetIconPadding(this.txtAltText, 35);
            this.txtAltText.Location = new System.Drawing.Point(124, 60);
            this.txtAltText.Name = "txtAltText";
            this.txtAltText.Size = new System.Drawing.Size(307, 22);
            this.txtAltText.TabIndex = 4;
            // 
            // txtCopyImage
            // 
            this.epErrors.SetIconPadding(this.txtCopyImage, 35);
            this.txtCopyImage.Location = new System.Drawing.Point(29, 215);
            this.txtCopyImage.MaxLength = 256;
            this.txtCopyImage.Name = "txtCopyImage";
            this.txtCopyImage.Size = new System.Drawing.Size(636, 22);
            this.txtCopyImage.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(29, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(228, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "Code colorizer &stylesheet file";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(29, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(210, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "Code colorizer &JavaScript file";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.pgGeneral);
            this.tabConfig.Controls.Add(this.pgLogo);
            this.tabConfig.Location = new System.Drawing.Point(12, 12);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.SelectedIndex = 0;
            this.tabConfig.Size = new System.Drawing.Size(734, 400);
            this.tabConfig.TabIndex = 0;
            // 
            // pgGeneral
            // 
            this.pgGeneral.Controls.Add(this.btnSelectImage);
            this.pgGeneral.Controls.Add(this.label12);
            this.pgGeneral.Controls.Add(this.txtCopyImage);
            this.pgGeneral.Controls.Add(this.txtStylesheet);
            this.pgGeneral.Controls.Add(this.btnSelectScript);
            this.pgGeneral.Controls.Add(this.btnSelectStylesheet);
            this.pgGeneral.Controls.Add(this.label2);
            this.pgGeneral.Controls.Add(this.label3);
            this.pgGeneral.Controls.Add(this.txtScriptFile);
            this.pgGeneral.Location = new System.Drawing.Point(4, 25);
            this.pgGeneral.Name = "pgGeneral";
            this.pgGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.pgGeneral.Size = new System.Drawing.Size(726, 371);
            this.pgGeneral.TabIndex = 0;
            this.pgGeneral.Text = "General";
            this.pgGeneral.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(29, 189);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(350, 23);
            this.label12.TabIndex = 6;
            this.label12.Text = "Code colorizer \"Copy\" &image file";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pgLogo
            // 
            this.pgLogo.Controls.Add(this.cboAlignment);
            this.pgLogo.Controls.Add(this.cboPlacement);
            this.pgLogo.Controls.Add(this.label11);
            this.pgLogo.Controls.Add(this.label10);
            this.pgLogo.Controls.Add(this.chkProportional);
            this.pgLogo.Controls.Add(this.label9);
            this.pgLogo.Controls.Add(this.label6);
            this.pgLogo.Controls.Add(this.txtAltText);
            this.pgLogo.Controls.Add(this.pnlImage);
            this.pgLogo.Controls.Add(this.udcHeight);
            this.pgLogo.Controls.Add(this.udcWidth);
            this.pgLogo.Controls.Add(this.label8);
            this.pgLogo.Controls.Add(this.label7);
            this.pgLogo.Controls.Add(this.lblActualSize);
            this.pgLogo.Controls.Add(this.label5);
            this.pgLogo.Controls.Add(this.txtLogoFile);
            this.pgLogo.Controls.Add(this.btnSelectLogo);
            this.pgLogo.Controls.Add(this.label4);
            this.pgLogo.Location = new System.Drawing.Point(4, 25);
            this.pgLogo.Name = "pgLogo";
            this.pgLogo.Padding = new System.Windows.Forms.Padding(3);
            this.pgLogo.Size = new System.Drawing.Size(726, 371);
            this.pgLogo.TabIndex = 1;
            this.pgLogo.Text = "Logo File";
            this.pgLogo.UseVisualStyleBackColor = true;
            // 
            // cboAlignment
            // 
            this.cboAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlignment.FormattingEnabled = true;
            this.cboAlignment.Items.AddRange(new object[] {
            "Left",
            "Right",
            "Center"});
            this.cboAlignment.Location = new System.Drawing.Point(331, 116);
            this.cboAlignment.Name = "cboAlignment";
            this.cboAlignment.Size = new System.Drawing.Size(100, 24);
            this.cboAlignment.TabIndex = 16;
            // 
            // cboPlacement
            // 
            this.cboPlacement.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPlacement.FormattingEnabled = true;
            this.cboPlacement.Items.AddRange(new object[] {
            "Left",
            "Right",
            "Above"});
            this.cboPlacement.Location = new System.Drawing.Point(124, 116);
            this.cboPlacement.Name = "cboPlacement";
            this.cboPlacement.Size = new System.Drawing.Size(100, 24);
            this.cboPlacement.TabIndex = 14;
            this.cboPlacement.SelectedIndexChanged += new System.EventHandler(this.cboPlacement_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(238, 116);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(87, 23);
            this.label11.TabIndex = 15;
            this.label11.Text = "Alig&nment";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(29, 116);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 23);
            this.label10.TabIndex = 13;
            this.label10.Text = "Pla&cement";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkProportional
            // 
            this.chkProportional.Checked = true;
            this.chkProportional.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProportional.Location = new System.Drawing.Point(580, 88);
            this.chkProportional.Name = "chkProportional";
            this.chkProportional.Size = new System.Drawing.Size(118, 24);
            this.chkProportional.TabIndex = 12;
            this.chkProportional.Text = "&Proportional";
            this.chkProportional.UseVisualStyleBackColor = true;
            this.chkProportional.CheckedChanged += new System.EventHandler(this.WidthHeight_ValueChanged);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(352, 88);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(222, 23);
            this.label9.TabIndex = 11;
            this.label9.Text = "(zero to use actual height/width)";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(6, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(112, 23);
            this.label6.TabIndex = 3;
            this.label6.Text = "&Alternate Text";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlImage
            // 
            this.pnlImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlImage.Location = new System.Drawing.Point(21, 146);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(685, 219);
            this.pnlImage.TabIndex = 17;
            this.pnlImage.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlImage_Paint);
            // 
            // udcHeight
            // 
            this.udcHeight.Location = new System.Drawing.Point(272, 88);
            this.udcHeight.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.udcHeight.Name = "udcHeight";
            this.udcHeight.Size = new System.Drawing.Size(74, 22);
            this.udcHeight.TabIndex = 10;
            this.udcHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcHeight.ValueChanged += new System.EventHandler(this.WidthHeight_ValueChanged);
            // 
            // udcWidth
            // 
            this.udcWidth.Location = new System.Drawing.Point(124, 88);
            this.udcWidth.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.udcWidth.Name = "udcWidth";
            this.udcWidth.Size = new System.Drawing.Size(74, 22);
            this.udcWidth.TabIndex = 8;
            this.udcWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcWidth.ValueChanged += new System.EventHandler(this.WidthHeight_ValueChanged);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(204, 87);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 23);
            this.label8.TabIndex = 9;
            this.label8.Text = "&Height";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(10, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(108, 23);
            this.label7.TabIndex = 7;
            this.label7.Text = "Display &Width";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblActualSize
            // 
            this.lblActualSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblActualSize.Location = new System.Drawing.Point(574, 59);
            this.lblActualSize.Name = "lblActualSize";
            this.lblActualSize.Size = new System.Drawing.Size(124, 23);
            this.lblActualSize.TabIndex = 6;
            this.lblActualSize.Text = "WxH";
            this.lblActualSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(438, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 23);
            this.label5.TabIndex = 5;
            this.label5.Text = "Actual Size (WxH)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(29, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(294, 23);
            this.label4.TabIndex = 0;
            this.label4.Text = "&Logo filename (leave blank for no logo)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PostTransformConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(758, 465);
            this.Controls.Add(this.tabConfig);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostTransformConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Post Transform Component";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.tabConfig.ResumeLayout(false);
            this.pgGeneral.ResumeLayout(false);
            this.pgGeneral.PerformLayout();
            this.pgLogo.ResumeLayout(false);
            this.pgLogo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.TextBox txtScriptFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStylesheet;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
        private System.Windows.Forms.Button btnSelectScript;
        private System.Windows.Forms.Button btnSelectStylesheet;
        private System.Windows.Forms.TabControl tabConfig;
        private System.Windows.Forms.TabPage pgGeneral;
        private System.Windows.Forms.TabPage pgLogo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblActualSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSelectLogo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlImage;
        private System.Windows.Forms.NumericUpDown udcHeight;
        private System.Windows.Forms.NumericUpDown udcWidth;
        private System.Windows.Forms.TextBox txtLogoFile;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtAltText;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkProportional;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cboAlignment;
        private System.Windows.Forms.ComboBox cboPlacement;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtCopyImage;
    }
}