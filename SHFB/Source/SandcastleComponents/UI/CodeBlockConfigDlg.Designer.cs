namespace SandcastleBuilder.Components.UI
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
            this.lnkProjectSite = new System.Windows.Forms.LinkLabel();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnSelectSyntax = new System.Windows.Forms.Button();
            this.btnSelectXsltStylesheet = new System.Windows.Forms.Button();
            this.btnSelectScript = new System.Windows.Forms.Button();
            this.btnSelectCssStylesheet = new System.Windows.Forms.Button();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtSyntaxFile = new System.Windows.Forms.TextBox();
            this.txtXsltStylesheetFile = new System.Windows.Forms.TextBox();
            this.txtCssStylesheet = new System.Windows.Forms.TextBox();
            this.txtScriptFile = new System.Windows.Forms.TextBox();
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
            this.chkDefaultTitle = new System.Windows.Forms.CheckBox();
            this.chkKeepSeeTags = new System.Windows.Forms.CheckBox();
            this.chkAllowMissingSource = new System.Windows.Forms.CheckBox();
            this.chkRemoveRegionMarkers = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcTabSize)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(870, 517);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 27;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 517);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 25;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(354, 521);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(274, 26);
            this.lnkProjectSite.TabIndex = 26;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectSite_LinkClicked);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(841, 36);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(35, 35);
            this.btnSelectFolder.TabIndex = 2;
            this.btnSelectFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectFolder, "Select base source folder");
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnSelectSyntax
            // 
            this.btnSelectSyntax.Location = new System.Drawing.Point(841, 99);
            this.btnSelectSyntax.Name = "btnSelectSyntax";
            this.btnSelectSyntax.Size = new System.Drawing.Size(35, 35);
            this.btnSelectSyntax.TabIndex = 5;
            this.btnSelectSyntax.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectSyntax, "Select the language syntax filename");
            this.btnSelectSyntax.UseVisualStyleBackColor = true;
            this.btnSelectSyntax.Click += new System.EventHandler(this.SelectFile_Click);
            // 
            // btnSelectXsltStylesheet
            // 
            this.btnSelectXsltStylesheet.Location = new System.Drawing.Point(841, 162);
            this.btnSelectXsltStylesheet.Name = "btnSelectXsltStylesheet";
            this.btnSelectXsltStylesheet.Size = new System.Drawing.Size(35, 35);
            this.btnSelectXsltStylesheet.TabIndex = 8;
            this.btnSelectXsltStylesheet.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectXsltStylesheet, "Select the XSLT transformation file");
            this.btnSelectXsltStylesheet.UseVisualStyleBackColor = true;
            this.btnSelectXsltStylesheet.Click += new System.EventHandler(this.SelectFile_Click);
            // 
            // btnSelectScript
            // 
            this.btnSelectScript.Location = new System.Drawing.Point(841, 288);
            this.btnSelectScript.Name = "btnSelectScript";
            this.btnSelectScript.Size = new System.Drawing.Size(35, 35);
            this.btnSelectScript.TabIndex = 14;
            this.btnSelectScript.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectScript, "Select the colorizer JavaScript file");
            this.btnSelectScript.UseVisualStyleBackColor = true;
            // 
            // btnSelectCssStylesheet
            // 
            this.btnSelectCssStylesheet.Location = new System.Drawing.Point(841, 225);
            this.btnSelectCssStylesheet.Name = "btnSelectCssStylesheet";
            this.btnSelectCssStylesheet.Size = new System.Drawing.Size(35, 35);
            this.btnSelectCssStylesheet.TabIndex = 11;
            this.btnSelectCssStylesheet.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectCssStylesheet, "Select the colorizer stylesheet filename");
            this.btnSelectCssStylesheet.UseVisualStyleBackColor = true;
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // txtSyntaxFile
            // 
            this.epErrors.SetIconPadding(this.txtSyntaxFile, 35);
            this.txtSyntaxFile.Location = new System.Drawing.Point(12, 101);
            this.txtSyntaxFile.MaxLength = 256;
            this.txtSyntaxFile.Name = "txtSyntaxFile";
            this.txtSyntaxFile.Size = new System.Drawing.Size(823, 31);
            this.txtSyntaxFile.TabIndex = 4;
            // 
            // txtXsltStylesheetFile
            // 
            this.epErrors.SetIconPadding(this.txtXsltStylesheetFile, 35);
            this.txtXsltStylesheetFile.Location = new System.Drawing.Point(12, 164);
            this.txtXsltStylesheetFile.MaxLength = 256;
            this.txtXsltStylesheetFile.Name = "txtXsltStylesheetFile";
            this.txtXsltStylesheetFile.Size = new System.Drawing.Size(823, 31);
            this.txtXsltStylesheetFile.TabIndex = 7;
            // 
            // txtCssStylesheet
            // 
            this.epErrors.SetIconPadding(this.txtCssStylesheet, 35);
            this.txtCssStylesheet.Location = new System.Drawing.Point(12, 227);
            this.txtCssStylesheet.MaxLength = 256;
            this.txtCssStylesheet.Name = "txtCssStylesheet";
            this.txtCssStylesheet.Size = new System.Drawing.Size(823, 31);
            this.txtCssStylesheet.TabIndex = 10;
            // 
            // txtScriptFile
            // 
            this.epErrors.SetIconPadding(this.txtScriptFile, 35);
            this.txtScriptFile.Location = new System.Drawing.Point(12, 290);
            this.txtScriptFile.MaxLength = 256;
            this.txtScriptFile.Name = "txtScriptFile";
            this.txtScriptFile.Size = new System.Drawing.Size(823, 31);
            this.txtScriptFile.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(823, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Base path for relative paths in <code> \'source\' attributes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBasePath
            // 
            this.txtBasePath.Location = new System.Drawing.Point(12, 38);
            this.txtBasePath.MaxLength = 256;
            this.txtBasePath.Name = "txtBasePath";
            this.txtBasePath.Size = new System.Drawing.Size(823, 31);
            this.txtBasePath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(823, 26);
            this.label2.TabIndex = 3;
            this.label2.Text = "&Language syntax configuration file";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 135);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(823, 26);
            this.label3.TabIndex = 6;
            this.label3.Text = "&XSLT style transformation file";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 342);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(174, 26);
            this.label4.TabIndex = 15;
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
            this.cboLanguage.Location = new System.Drawing.Point(192, 340);
            this.cboLanguage.MaxDropDownItems = 16;
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(162, 33);
            this.cboLanguage.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(359, 342);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(156, 26);
            this.label5.TabIndex = 17;
            this.label5.Text = "Default &Tab Size";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // udcTabSize
            // 
            this.udcTabSize.Location = new System.Drawing.Point(521, 341);
            this.udcTabSize.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.udcTabSize.Name = "udcTabSize";
            this.udcTabSize.Size = new System.Drawing.Size(70, 31);
            this.udcTabSize.TabIndex = 18;
            this.udcTabSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkNumberLines
            // 
            this.chkNumberLines.AutoSize = true;
            this.chkNumberLines.Location = new System.Drawing.Point(12, 397);
            this.chkNumberLines.Name = "chkNumberLines";
            this.chkNumberLines.Size = new System.Drawing.Size(214, 29);
            this.chkNumberLines.TabIndex = 19;
            this.chkNumberLines.Text = "Enable line &numbering";
            this.chkNumberLines.UseVisualStyleBackColor = true;
            // 
            // chkOutlining
            // 
            this.chkOutlining.AutoSize = true;
            this.chkOutlining.Location = new System.Drawing.Point(12, 432);
            this.chkOutlining.Name = "chkOutlining";
            this.chkOutlining.Size = new System.Drawing.Size(468, 29);
            this.chkOutlining.TabIndex = 20;
            this.chkOutlining.Text = "Enable &collapsible #region and #if/#else/#endif blocks";
            this.chkOutlining.UseVisualStyleBackColor = true;
            // 
            // chkDefaultTitle
            // 
            this.chkDefaultTitle.AutoSize = true;
            this.chkDefaultTitle.Checked = true;
            this.chkDefaultTitle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDefaultTitle.Location = new System.Drawing.Point(521, 397);
            this.chkDefaultTitle.Name = "chkDefaultTitle";
            this.chkDefaultTitle.Size = new System.Drawing.Size(424, 29);
            this.chkDefaultTitle.TabIndex = 22;
            this.chkDefaultTitle.Text = "&Use language name as title if one is not specified";
            this.chkDefaultTitle.UseVisualStyleBackColor = true;
            // 
            // chkKeepSeeTags
            // 
            this.chkKeepSeeTags.AutoSize = true;
            this.chkKeepSeeTags.Location = new System.Drawing.Point(12, 467);
            this.chkKeepSeeTags.Name = "chkKeepSeeTags";
            this.chkKeepSeeTags.Size = new System.Drawing.Size(488, 29);
            this.chkKeepSeeTags.TabIndex = 21;
            this.chkKeepSeeTags.Text = "&Keep <see> XML comment tags that occur with the code";
            this.chkKeepSeeTags.UseVisualStyleBackColor = true;
            // 
            // chkAllowMissingSource
            // 
            this.chkAllowMissingSource.AutoSize = true;
            this.chkAllowMissingSource.Location = new System.Drawing.Point(521, 432);
            this.chkAllowMissingSource.Name = "chkAllowMissingSource";
            this.chkAllowMissingSource.Size = new System.Drawing.Size(351, 29);
            this.chkAllowMissingSource.TabIndex = 23;
            this.chkAllowMissingSource.Text = "Allo&w missing source code files/regions";
            this.chkAllowMissingSource.UseVisualStyleBackColor = true;
            // 
            // chkRemoveRegionMarkers
            // 
            this.chkRemoveRegionMarkers.AutoSize = true;
            this.chkRemoveRegionMarkers.Location = new System.Drawing.Point(521, 467);
            this.chkRemoveRegionMarkers.Name = "chkRemoveRegionMarkers";
            this.chkRemoveRegionMarkers.Size = new System.Drawing.Size(284, 29);
            this.chkRemoveRegionMarkers.TabIndex = 24;
            this.chkRemoveRegionMarkers.Text = "Remove nested region &markers";
            this.chkRemoveRegionMarkers.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 198);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(823, 26);
            this.label6.TabIndex = 9;
            this.label6.Text = "Colorized code &style sheet file";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 261);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(823, 26);
            this.label7.TabIndex = 12;
            this.label7.Text = "Colorized code &JavaScript file";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CodeBlockConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(982, 564);
            this.Controls.Add(this.txtCssStylesheet);
            this.Controls.Add(this.btnSelectScript);
            this.Controls.Add(this.btnSelectCssStylesheet);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtScriptFile);
            this.Controls.Add(this.chkRemoveRegionMarkers);
            this.Controls.Add(this.chkAllowMissingSource);
            this.Controls.Add(this.chkKeepSeeTags);
            this.Controls.Add(this.chkDefaultTitle);
            this.Controls.Add(this.btnSelectXsltStylesheet);
            this.Controls.Add(this.btnSelectSyntax);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.chkOutlining);
            this.Controls.Add(this.chkNumberLines);
            this.Controls.Add(this.udcTabSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboLanguage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtXsltStylesheetFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSyntaxFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBasePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
        private System.Windows.Forms.TextBox txtXsltStylesheetFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSyntaxFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBasePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udcTabSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.CheckBox chkOutlining;
        private System.Windows.Forms.CheckBox chkNumberLines;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnSelectXsltStylesheet;
        private System.Windows.Forms.Button btnSelectSyntax;
        private System.Windows.Forms.CheckBox chkDefaultTitle;
        private System.Windows.Forms.CheckBox chkKeepSeeTags;
        private System.Windows.Forms.CheckBox chkAllowMissingSource;
        private System.Windows.Forms.CheckBox chkRemoveRegionMarkers;
        private System.Windows.Forms.TextBox txtCssStylesheet;
        private System.Windows.Forms.Button btnSelectScript;
        private System.Windows.Forms.Button btnSelectCssStylesheet;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtScriptFile;
    }
}