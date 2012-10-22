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
            this.btnSelectLogo = new System.Windows.Forms.Button();
            this.txtLogoFile = new System.Windows.Forms.TextBox();
            this.txtAltText = new System.Windows.Forms.TextBox();
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
            ((System.ComponentModel.ISupportInitialize)(this.udcHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(626, 402);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 402);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 18;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkCodePlexSHFB
            // 
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(270, 407);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 19;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCodePlexSHFB_LinkClicked);
            // 
            // btnSelectLogo
            // 
            this.btnSelectLogo.Location = new System.Drawing.Point(666, 33);
            this.btnSelectLogo.Name = "btnSelectLogo";
            this.btnSelectLogo.Size = new System.Drawing.Size(32, 25);
            this.btnSelectLogo.TabIndex = 2;
            this.btnSelectLogo.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectLogo, "Select the logo file to use");
            this.btnSelectLogo.UseVisualStyleBackColor = true;
            this.btnSelectLogo.Click += new System.EventHandler(this.SelectImage_Click);
            // 
            // txtLogoFile
            // 
            this.txtLogoFile.Location = new System.Drawing.Point(29, 35);
            this.txtLogoFile.MaxLength = 256;
            this.txtLogoFile.Name = "txtLogoFile";
            this.txtLogoFile.Size = new System.Drawing.Size(636, 22);
            this.txtLogoFile.TabIndex = 1;
            this.txtLogoFile.Leave += new System.EventHandler(this.txtLogoFile_Leave);
            // 
            // txtAltText
            // 
            this.txtAltText.Location = new System.Drawing.Point(132, 71);
            this.txtAltText.Name = "txtAltText";
            this.txtAltText.Size = new System.Drawing.Size(307, 22);
            this.txtAltText.TabIndex = 4;
            // 
            // cboAlignment
            // 
            this.cboAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlignment.FormattingEnabled = true;
            this.cboAlignment.Items.AddRange(new object[] {
            "Left",
            "Right",
            "Center"});
            this.cboAlignment.Location = new System.Drawing.Point(339, 127);
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
            this.cboPlacement.Location = new System.Drawing.Point(132, 127);
            this.cboPlacement.Name = "cboPlacement";
            this.cboPlacement.Size = new System.Drawing.Size(100, 24);
            this.cboPlacement.TabIndex = 14;
            this.cboPlacement.SelectedIndexChanged += new System.EventHandler(this.cboPlacement_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(246, 127);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(87, 23);
            this.label11.TabIndex = 15;
            this.label11.Text = "Alig&nment";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(37, 127);
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
            this.chkProportional.Location = new System.Drawing.Point(588, 99);
            this.chkProportional.Name = "chkProportional";
            this.chkProportional.Size = new System.Drawing.Size(118, 24);
            this.chkProportional.TabIndex = 12;
            this.chkProportional.Text = "&Proportional";
            this.chkProportional.UseVisualStyleBackColor = true;
            this.chkProportional.CheckedChanged += new System.EventHandler(this.WidthHeight_ValueChanged);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(360, 99);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(222, 23);
            this.label9.TabIndex = 11;
            this.label9.Text = "(zero to use actual height/width)";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(21, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 23);
            this.label6.TabIndex = 3;
            this.label6.Text = "&Alternate Text";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlImage
            // 
            this.pnlImage.BackColor = System.Drawing.SystemColors.Window;
            this.pnlImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlImage.Location = new System.Drawing.Point(12, 160);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(702, 236);
            this.pnlImage.TabIndex = 17;
            this.pnlImage.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlImage_Paint);
            // 
            // udcHeight
            // 
            this.udcHeight.Location = new System.Drawing.Point(280, 99);
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
            this.udcWidth.Location = new System.Drawing.Point(132, 99);
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
            this.label8.Location = new System.Drawing.Point(212, 98);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 23);
            this.label8.TabIndex = 9;
            this.label8.Text = "&Height";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(24, 98);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(102, 23);
            this.label7.TabIndex = 7;
            this.label7.Text = "Display &Width";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblActualSize
            // 
            this.lblActualSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblActualSize.Location = new System.Drawing.Point(582, 70);
            this.lblActualSize.Name = "lblActualSize";
            this.lblActualSize.Size = new System.Drawing.Size(124, 23);
            this.lblActualSize.TabIndex = 6;
            this.lblActualSize.Text = "WxH";
            this.lblActualSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(446, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 23);
            this.label5.TabIndex = 5;
            this.label5.Text = "Actual Size (WxH)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(29, 9);
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
            this.ClientSize = new System.Drawing.Size(726, 446);
            this.Controls.Add(this.cboAlignment);
            this.Controls.Add(this.cboPlacement);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chkProportional);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnSelectLogo);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtLogoFile);
            this.Controls.Add(this.txtAltText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pnlImage);
            this.Controls.Add(this.lblActualSize);
            this.Controls.Add(this.udcHeight);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.udcWidth);
            this.Controls.Add(this.label8);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostTransformConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Post Transform Component";
            ((System.ComponentModel.ISupportInitialize)(this.udcHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
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
    }
}