namespace SandcastleBuilder.Package.PropertyPages
{
    partial class BuildPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BuildPropertiesPageControl));
            this.label1 = new System.Windows.Forms.Label();
            this.chkCleanIntermediates = new System.Windows.Forms.CheckBox();
            this.chkCppCommentsFixup = new System.Windows.Forms.CheckBox();
            this.chkDisableCodeBlockComponent = new System.Windows.Forms.CheckBox();
            this.chkKeepLogFile = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboFrameworkVersion = new System.Windows.Forms.ComboBox();
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            this.chkIndentHtml = new System.Windows.Forms.CheckBox();
            this.txtBuildLogFile = new SandcastleBuilder.Utils.Controls.FilePathUserControl();
            this.label3 = new System.Windows.Forms.Label();
            this.cblHelpFileFormat = new System.Windows.Forms.CheckedListBox();
            this.cboBuildAssemblerVerbosity = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(279, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "B&uild Log Filename";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkCleanIntermediates
            // 
            this.chkCleanIntermediates.AutoSize = true;
            this.chkCleanIntermediates.Checked = true;
            this.chkCleanIntermediates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCleanIntermediates.Location = new System.Drawing.Point(292, 115);
            this.chkCleanIntermediates.Name = "chkCleanIntermediates";
            this.chkCleanIntermediates.Size = new System.Drawing.Size(342, 24);
            this.chkCleanIntermediates.TabIndex = 6;
            this.chkCleanIntermediates.Tag = "CleanIntermediates";
            this.chkCleanIntermediates.Text = "&Clean intermediate files after a successful build";
            this.chkCleanIntermediates.UseVisualStyleBackColor = true;
            // 
            // chkCppCommentsFixup
            // 
            this.chkCppCommentsFixup.AutoSize = true;
            this.chkCppCommentsFixup.Location = new System.Drawing.Point(292, 205);
            this.chkCppCommentsFixup.Name = "chkCppCommentsFixup";
            this.chkCppCommentsFixup.Size = new System.Drawing.Size(419, 24);
            this.chkCppCommentsFixup.TabIndex = 9;
            this.chkCppCommentsFixup.Tag = "CppCommentsFixup";
            this.chkCppCommentsFixup.Text = "Fix up method signature issues in C++ XML comments files";
            this.chkCppCommentsFixup.UseVisualStyleBackColor = true;
            // 
            // chkDisableCodeBlockComponent
            // 
            this.chkDisableCodeBlockComponent.AutoSize = true;
            this.epNotes.SetError(this.chkDisableCodeBlockComponent, "If checked, code colorization and the other related features will be disabled");
            this.chkDisableCodeBlockComponent.Location = new System.Drawing.Point(292, 175);
            this.chkDisableCodeBlockComponent.Name = "chkDisableCodeBlockComponent";
            this.chkDisableCodeBlockComponent.Size = new System.Drawing.Size(315, 24);
            this.chkDisableCodeBlockComponent.TabIndex = 8;
            this.chkDisableCodeBlockComponent.Tag = "DisableCodeBlockComponent";
            this.chkDisableCodeBlockComponent.Text = "Disable the custom code block component";
            this.chkDisableCodeBlockComponent.UseVisualStyleBackColor = true;
            // 
            // chkKeepLogFile
            // 
            this.chkKeepLogFile.AutoSize = true;
            this.chkKeepLogFile.Checked = true;
            this.chkKeepLogFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkKeepLogFile.Location = new System.Drawing.Point(292, 145);
            this.chkKeepLogFile.Name = "chkKeepLogFile";
            this.chkKeepLogFile.Size = new System.Drawing.Size(295, 24);
            this.chkKeepLogFile.TabIndex = 7;
            this.chkKeepLogFile.Tag = "KeepLogFile";
            this.chkKeepLogFile.Text = "Keep the log file after a successful build";
            this.chkKeepLogFile.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(279, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "F&ramework Version";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboFrameworkVersion
            // 
            this.cboFrameworkVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFrameworkVersion.FormattingEnabled = true;
            this.cboFrameworkVersion.Location = new System.Drawing.Point(431, 12);
            this.cboFrameworkVersion.MaxDropDownItems = 16;
            this.cboFrameworkVersion.Name = "cboFrameworkVersion";
            this.cboFrameworkVersion.Size = new System.Drawing.Size(280, 28);
            this.cboFrameworkVersion.TabIndex = 3;
            this.cboFrameworkVersion.Tag = "FrameworkVersion";
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // chkIndentHtml
            // 
            this.chkIndentHtml.AutoSize = true;
            this.epNotes.SetError(this.chkIndentHtml, "Build component debugging aid.  Leave this unchecked for normal builds\nto produce" +
        " more compact HTML");
            this.chkIndentHtml.Location = new System.Drawing.Point(292, 235);
            this.chkIndentHtml.Name = "chkIndentHtml";
            this.chkIndentHtml.Size = new System.Drawing.Size(180, 24);
            this.chkIndentHtml.TabIndex = 10;
            this.chkIndentHtml.Tag = "IndentHtml";
            this.chkIndentHtml.Text = "Indent rendered HTML";
            this.chkIndentHtml.UseVisualStyleBackColor = true;
            // 
            // txtBuildLogFile
            // 
            this.txtBuildLogFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtBuildLogFile, "If not specified, a file called LastBuild.log is created in the folder\nidentified" +
        " by the Output Path property.");
            this.txtBuildLogFile.Filter = "Log files (*.log)|*.log|All Files (*.*)|*.*";
            this.epNotes.SetIconPadding(this.txtBuildLogFile, 5);
            this.txtBuildLogFile.Location = new System.Drawing.Point(431, 46);
            this.txtBuildLogFile.Name = "txtBuildLogFile";
            this.txtBuildLogFile.Size = new System.Drawing.Size(330, 59);
            this.txtBuildLogFile.TabIndex = 5;
            this.txtBuildLogFile.Tag = "BuildLogFile";
            this.txtBuildLogFile.Title = "Select the log file location";
            this.txtBuildLogFile.UseFileOpenDialog = false;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 23);
            this.label3.TabIndex = 0;
            this.label3.Text = "&Build these help file formats:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cblHelpFileFormat
            // 
            this.cblHelpFileFormat.FormattingEnabled = true;
            this.cblHelpFileFormat.Location = new System.Drawing.Point(7, 41);
            this.cblHelpFileFormat.Name = "cblHelpFileFormat";
            this.cblHelpFileFormat.Size = new System.Drawing.Size(221, 246);
            this.cblHelpFileFormat.TabIndex = 1;
            this.cblHelpFileFormat.Tag = "HelpFileFormat";
            // 
            // cboBuildAssemblerVerbosity
            // 
            this.cboBuildAssemblerVerbosity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuildAssemblerVerbosity.FormattingEnabled = true;
            this.cboBuildAssemblerVerbosity.Location = new System.Drawing.Point(431, 273);
            this.cboBuildAssemblerVerbosity.MaxDropDownItems = 16;
            this.cboBuildAssemblerVerbosity.Name = "cboBuildAssemblerVerbosity";
            this.cboBuildAssemblerVerbosity.Size = new System.Drawing.Size(214, 28);
            this.cboBuildAssemblerVerbosity.TabIndex = 12;
            this.cboBuildAssemblerVerbosity.Tag = "BuildAssemblerVerbosity";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(233, 275);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(192, 23);
            this.label4.TabIndex = 11;
            this.label4.Text = "BuildAssembler &Verbosity";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BuildPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.cboBuildAssemblerVerbosity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtBuildLogFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cblHelpFileFormat);
            this.Controls.Add(this.chkIndentHtml);
            this.Controls.Add(this.cboFrameworkVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkKeepLogFile);
            this.Controls.Add(this.chkDisableCodeBlockComponent);
            this.Controls.Add(this.chkCppCommentsFixup);
            this.Controls.Add(this.chkCleanIntermediates);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(745, 285);
            this.Name = "BuildPropertiesPageControl";
            this.Size = new System.Drawing.Size(785, 310);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkCleanIntermediates;
        private System.Windows.Forms.CheckBox chkCppCommentsFixup;
        private System.Windows.Forms.CheckBox chkDisableCodeBlockComponent;
        private System.Windows.Forms.CheckBox chkKeepLogFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboFrameworkVersion;
        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.CheckBox chkIndentHtml;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox cblHelpFileFormat;
        private Utils.Controls.FilePathUserControl txtBuildLogFile;
        private System.Windows.Forms.ComboBox cboBuildAssemblerVerbosity;
        private System.Windows.Forms.Label label4;
    }
}
