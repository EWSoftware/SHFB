namespace SandcastleBuilder.Components
{
    partial class CodeBlockConfigDlg
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
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnSelectSyntax = new System.Windows.Forms.Button();
            this.btnSelectStyle = new System.Windows.Forms.Button();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtSyntaxFile = new System.Windows.Forms.TextBox();
            this.txtStyleFile = new System.Windows.Forms.TextBox();
            this.txtCopyImageUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBasePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.udcTabSize = new System.Windows.Forms.NumericUpDown();
            this.chkNumberLines = new System.Windows.Forms.CheckBox();
            this.chkOutlining = new System.Windows.Forms.CheckBox();
            this.chkLanguageFilter = new System.Windows.Forms.CheckBox();
            this.chkDefaultTitle = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkKeepSeeTags = new System.Windows.Forms.CheckBox();
            this.chkAllowMissingSource = new System.Windows.Forms.CheckBox();
            this.chkRemoveRegionMarkers = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcTabSize)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(668, 395);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 395);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 22;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkCodePlexSHFB
            // 
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(275, 400);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 23;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCodePlexSHFB_LinkClicked);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(724, 33);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(32, 25);
            this.btnSelectFolder.TabIndex = 2;
            this.btnSelectFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectFolder, "Select base source folder");
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnSelectSyntax
            // 
            this.btnSelectSyntax.Location = new System.Drawing.Point(724, 83);
            this.btnSelectSyntax.Name = "btnSelectSyntax";
            this.btnSelectSyntax.Size = new System.Drawing.Size(32, 25);
            this.btnSelectSyntax.TabIndex = 5;
            this.btnSelectSyntax.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectSyntax, "Select the language syntax filename");
            this.btnSelectSyntax.UseVisualStyleBackColor = true;
            this.btnSelectSyntax.Click += new System.EventHandler(this.SelectFile_Click);
            // 
            // btnSelectStyle
            // 
            this.btnSelectStyle.Location = new System.Drawing.Point(724, 135);
            this.btnSelectStyle.Name = "btnSelectStyle";
            this.btnSelectStyle.Size = new System.Drawing.Size(32, 25);
            this.btnSelectStyle.TabIndex = 8;
            this.btnSelectStyle.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectStyle, "Select the XSLT transformation file");
            this.btnSelectStyle.UseVisualStyleBackColor = true;
            this.btnSelectStyle.Click += new System.EventHandler(this.SelectFile_Click);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // txtSyntaxFile
            // 
            this.epErrors.SetIconPadding(this.txtSyntaxFile, 35);
            this.txtSyntaxFile.Location = new System.Drawing.Point(12, 86);
            this.txtSyntaxFile.MaxLength = 256;
            this.txtSyntaxFile.Name = "txtSyntaxFile";
            this.txtSyntaxFile.Size = new System.Drawing.Size(711, 22);
            this.txtSyntaxFile.TabIndex = 4;
            // 
            // txtStyleFile
            // 
            this.epErrors.SetIconPadding(this.txtStyleFile, 35);
            this.txtStyleFile.Location = new System.Drawing.Point(12, 137);
            this.txtStyleFile.MaxLength = 256;
            this.txtStyleFile.Name = "txtStyleFile";
            this.txtStyleFile.Size = new System.Drawing.Size(711, 22);
            this.txtStyleFile.TabIndex = 7;
            // 
            // txtCopyImageUrl
            // 
            this.epErrors.SetIconPadding(this.txtCopyImageUrl, 35);
            this.txtCopyImageUrl.Location = new System.Drawing.Point(12, 188);
            this.txtCopyImageUrl.MaxLength = 256;
            this.txtCopyImageUrl.Name = "txtCopyImageUrl";
            this.txtCopyImageUrl.Size = new System.Drawing.Size(711, 22);
            this.txtCopyImageUrl.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(376, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Base path for relative paths in <code> \'source\' attributes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBasePath
            // 
            this.txtBasePath.Location = new System.Drawing.Point(12, 35);
            this.txtBasePath.MaxLength = 256;
            this.txtBasePath.Name = "txtBasePath";
            this.txtBasePath.Size = new System.Drawing.Size(711, 22);
            this.txtBasePath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(244, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "&Language syntax configuration file";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(222, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "&XSLT style transformation file";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(6, 220);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 23);
            this.label4.TabIndex = 11;
            this.label4.Text = "&Default Language";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboLanguage
            // 
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Items.AddRange(new object[] {
            "None",
            "C#",
            "VB.NET",
            "C++",
            "C",
            "JavaScript",
            "JScript.NET",
            "J#",
            "VBScript",
            "XML",
            "XAML",
            "SQL",
            "PowerShell"});
            this.cboLanguage.Location = new System.Drawing.Point(143, 220);
            this.cboLanguage.MaxDropDownItems = 16;
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(121, 24);
            this.cboLanguage.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(270, 220);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 23);
            this.label5.TabIndex = 13;
            this.label5.Text = "Default &Tab Size";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // udcTabSize
            // 
            this.udcTabSize.Location = new System.Drawing.Point(410, 222);
            this.udcTabSize.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.udcTabSize.Name = "udcTabSize";
            this.udcTabSize.Size = new System.Drawing.Size(56, 22);
            this.udcTabSize.TabIndex = 14;
            this.udcTabSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkNumberLines
            // 
            this.chkNumberLines.Location = new System.Drawing.Point(12, 257);
            this.chkNumberLines.Name = "chkNumberLines";
            this.chkNumberLines.Size = new System.Drawing.Size(192, 21);
            this.chkNumberLines.TabIndex = 15;
            this.chkNumberLines.Text = "Enable line &numbering";
            this.chkNumberLines.UseVisualStyleBackColor = true;
            // 
            // chkOutlining
            // 
            this.chkOutlining.Location = new System.Drawing.Point(12, 284);
            this.chkOutlining.Name = "chkOutlining";
            this.chkOutlining.Size = new System.Drawing.Size(379, 21);
            this.chkOutlining.TabIndex = 16;
            this.chkOutlining.Text = "Enable &collapsible #region and #if/#else/#endif blocks";
            this.chkOutlining.UseVisualStyleBackColor = true;
            // 
            // chkLanguageFilter
            // 
            this.chkLanguageFilter.Checked = true;
            this.chkLanguageFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLanguageFilter.Location = new System.Drawing.Point(12, 338);
            this.chkLanguageFilter.Name = "chkLanguageFilter";
            this.chkLanguageFilter.Size = new System.Drawing.Size(311, 21);
            this.chkLanguageFilter.TabIndex = 18;
            this.chkLanguageFilter.Text = "Connect code blocks to the language &filter";
            this.chkLanguageFilter.UseVisualStyleBackColor = true;
            // 
            // chkDefaultTitle
            // 
            this.chkDefaultTitle.Checked = true;
            this.chkDefaultTitle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDefaultTitle.Location = new System.Drawing.Point(413, 257);
            this.chkDefaultTitle.Name = "chkDefaultTitle";
            this.chkDefaultTitle.Size = new System.Drawing.Size(343, 21);
            this.chkDefaultTitle.TabIndex = 19;
            this.chkDefaultTitle.Text = "&Use language name as title if one is not specified";
            this.chkDefaultTitle.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 162);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(338, 23);
            this.label6.TabIndex = 9;
            this.label6.Text = "\"Copy\" &image URL";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkKeepSeeTags
            // 
            this.chkKeepSeeTags.Location = new System.Drawing.Point(12, 311);
            this.chkKeepSeeTags.Name = "chkKeepSeeTags";
            this.chkKeepSeeTags.Size = new System.Drawing.Size(394, 21);
            this.chkKeepSeeTags.TabIndex = 17;
            this.chkKeepSeeTags.Text = "&Keep <see> XML comment tags that occur with the code";
            this.chkKeepSeeTags.UseVisualStyleBackColor = true;
            // 
            // chkAllowMissingSource
            // 
            this.chkAllowMissingSource.Location = new System.Drawing.Point(413, 284);
            this.chkAllowMissingSource.Name = "chkAllowMissingSource";
            this.chkAllowMissingSource.Size = new System.Drawing.Size(283, 21);
            this.chkAllowMissingSource.TabIndex = 20;
            this.chkAllowMissingSource.Text = "Allo&w missing source code files/regions";
            this.chkAllowMissingSource.UseVisualStyleBackColor = true;
            // 
            // chkRemoveRegionMarkers
            // 
            this.chkRemoveRegionMarkers.Location = new System.Drawing.Point(413, 311);
            this.chkRemoveRegionMarkers.Name = "chkRemoveRegionMarkers";
            this.chkRemoveRegionMarkers.Size = new System.Drawing.Size(250, 21);
            this.chkRemoveRegionMarkers.TabIndex = 21;
            this.chkRemoveRegionMarkers.Text = "Remove nested region &markers";
            this.chkRemoveRegionMarkers.UseVisualStyleBackColor = true;
            // 
            // CodeBlockConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(768, 439);
            this.Controls.Add(this.chkRemoveRegionMarkers);
            this.Controls.Add(this.chkAllowMissingSource);
            this.Controls.Add(this.chkKeepSeeTags);
            this.Controls.Add(this.txtCopyImageUrl);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkDefaultTitle);
            this.Controls.Add(this.chkLanguageFilter);
            this.Controls.Add(this.btnSelectStyle);
            this.Controls.Add(this.btnSelectSyntax);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.chkOutlining);
            this.Controls.Add(this.chkNumberLines);
            this.Controls.Add(this.udcTabSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboLanguage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtStyleFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSyntaxFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBasePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CodeBlockConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Code Block Component";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcTabSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.TextBox txtStyleFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSyntaxFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBasePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udcTabSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
        private System.Windows.Forms.CheckBox chkOutlining;
        private System.Windows.Forms.CheckBox chkNumberLines;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnSelectStyle;
        private System.Windows.Forms.Button btnSelectSyntax;
        private System.Windows.Forms.CheckBox chkLanguageFilter;
        private System.Windows.Forms.CheckBox chkDefaultTitle;
        private System.Windows.Forms.TextBox txtCopyImageUrl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkKeepSeeTags;
        private System.Windows.Forms.CheckBox chkAllowMissingSource;
        private System.Windows.Forms.CheckBox chkRemoveRegionMarkers;
    }
}