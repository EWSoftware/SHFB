namespace SandcastleBuilder.Gui
{
    partial class NewFromOtherFormatDlg
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
            this.txtProjectFile = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectProject = new System.Windows.Forms.Button();
            this.txtNewProjectFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSelectNewFolder = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.cboProjectFormat = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // txtProjectFile
            // 
            this.txtProjectFile.Location = new System.Drawing.Point(218, 42);
            this.txtProjectFile.Name = "txtProjectFile";
            this.txtProjectFile.Size = new System.Drawing.Size(452, 22);
            this.statusBarTextProvider1.SetStatusBarText(this.txtProjectFile, "Project to Convert: Specify the project file to convert");
            this.txtProjectFile.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(71, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Project to Convert";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSelectProject
            // 
            this.btnSelectProject.Location = new System.Drawing.Point(671, 41);
            this.btnSelectProject.Name = "btnSelectProject";
            this.btnSelectProject.Size = new System.Drawing.Size(32, 25);
            this.statusBarTextProvider1.SetStatusBarText(this.btnSelectProject, "Browse: Browse for the project to convert");
            this.btnSelectProject.TabIndex = 4;
            this.btnSelectProject.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectProject, "Select project to convert");
            this.btnSelectProject.UseVisualStyleBackColor = true;
            this.btnSelectProject.Click += new System.EventHandler(this.btnSelectProject_Click);
            // 
            // txtNewProjectFolder
            // 
            this.txtNewProjectFolder.Location = new System.Drawing.Point(218, 72);
            this.txtNewProjectFolder.Name = "txtNewProjectFolder";
            this.txtNewProjectFolder.Size = new System.Drawing.Size(452, 22);
            this.statusBarTextProvider1.SetStatusBarText(this.txtNewProjectFolder, "New Project Location: Specify the folder to use for the new project");
            this.txtNewProjectFolder.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(199, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "&Location of New Project File";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSelectNewFolder
            // 
            this.btnSelectNewFolder.Location = new System.Drawing.Point(671, 71);
            this.btnSelectNewFolder.Name = "btnSelectNewFolder";
            this.btnSelectNewFolder.Size = new System.Drawing.Size(32, 25);
            this.statusBarTextProvider1.SetStatusBarText(this.btnSelectNewFolder, "Browse: Browse for the new folder location");
            this.btnSelectNewFolder.TabIndex = 7;
            this.btnSelectNewFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectNewFolder, "Select folder for the converted project files");
            this.btnSelectNewFolder.UseVisualStyleBackColor = true;
            this.btnSelectNewFolder.Click += new System.EventHandler(this.btnSelectNewFolder_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(12, 114);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnConvert, "Convert: Convert the project to the new format");
            this.btnConvert.TabIndex = 8;
            this.btnConvert.Text = "&Convert";
            this.toolTip1.SetToolTip(this.btnConvert, "Convert the project");
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(634, 114);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnCancel, "Cancel: Close without converting the project");
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Close without converting");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cboProjectFormat
            // 
            this.cboProjectFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProjectFormat.FormattingEnabled = true;
            this.cboProjectFormat.Items.AddRange(new object[] {
            "SHFB 1.7.0.0 or earlier",
            "NDoc 1.x",
            "DocProject 1.x",
            "Stephan Smetsers SandcastleGUI 1.x",
            "Microsoft Sandcastle Example GUI"});
            this.cboProjectFormat.Location = new System.Drawing.Point(218, 12);
            this.cboProjectFormat.Name = "cboProjectFormat";
            this.cboProjectFormat.Size = new System.Drawing.Size(274, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.cboProjectFormat, "Project Format: Select the format of the project to convert");
            this.cboProjectFormat.TabIndex = 1;
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(71, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 23);
            this.label3.TabIndex = 0;
            this.label3.Text = "Project &Format";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(540, 114);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnHelp, "Help: View help for this form");
            this.btnHelp.TabIndex = 9;
            this.btnHelp.Text = "&Help";
            this.toolTip1.SetToolTip(this.btnHelp, "View help for this form");
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // NewFromOtherFormatDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(734, 158);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.cboProjectFormat);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.txtNewProjectFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectNewFolder);
            this.Controls.Add(this.txtProjectFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelectProject);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewFromOtherFormatDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create New Project From Other Format";
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtProjectFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectProject;
        private System.Windows.Forms.TextBox txtNewProjectFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelectNewFolder;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnCancel;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.ComboBox cboProjectFormat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnHelp;
    }
}