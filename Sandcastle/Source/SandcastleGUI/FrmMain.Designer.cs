namespace SandcastleGui
{
	partial class FrmMain
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDelete3 = new System.Windows.Forms.Button();
            this.btnAddFolder3 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.btnDelete2 = new System.Windows.Forms.Button();
            this.btnAddFolder2 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lstDependent = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lstComments = new System.Windows.Forms.ListBox();
            this.btnDelete1 = new System.Windows.Forms.Button();
            this.btnAddFolder1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAdd1 = new System.Windows.Forms.Button();
            this.lstDll = new System.Windows.Forms.ListBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.btnBuild = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ckbWeb = new System.Windows.Forms.CheckBox();
            this.cmbLanguages = new System.Windows.Forms.ComboBox();
            this.ckbHxs = new System.Windows.Forms.CheckBox();
            this.ckbChm = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbTopicStyle = new System.Windows.Forms.ComboBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mniOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mniSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mniExit = new System.Windows.Forms.ToolStripMenuItem();
            this.fileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.saveDialog = new System.Windows.Forms.SaveFileDialog();
            this.openDialog = new System.Windows.Forms.OpenFileDialog();
            this.label9 = new System.Windows.Forms.Label();
            this.lnkProjectSite = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDelete3);
            this.groupBox1.Controls.Add(this.btnAddFolder3);
            this.groupBox1.Controls.Add(this.button12);
            this.groupBox1.Controls.Add(this.btnDelete2);
            this.groupBox1.Controls.Add(this.btnAddFolder2);
            this.groupBox1.Controls.Add(this.button8);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lstDependent);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lstComments);
            this.groupBox1.Controls.Add(this.btnDelete1);
            this.groupBox1.Controls.Add(this.btnAddFolder1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnAdd1);
            this.groupBox1.Controls.Add(this.lstDll);
            this.groupBox1.Location = new System.Drawing.Point(12, 31);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(486, 323);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Assembly and Comments files";
            // 
            // btnDelete3
            // 
            this.btnDelete3.Location = new System.Drawing.Point(403, 290);
            this.btnDelete3.Name = "btnDelete3";
            this.btnDelete3.Size = new System.Drawing.Size(75, 25);
            this.btnDelete3.TabIndex = 14;
            this.btnDelete3.Tag = "3";
            this.btnDelete3.Text = "Delete";
            this.btnDelete3.UseVisualStyleBackColor = true;
            this.btnDelete3.Click += new System.EventHandler(this.btnDelete1_Click);
            // 
            // btnAddFolder3
            // 
            this.btnAddFolder3.Location = new System.Drawing.Point(322, 290);
            this.btnAddFolder3.Name = "btnAddFolder3";
            this.btnAddFolder3.Size = new System.Drawing.Size(75, 25);
            this.btnAddFolder3.TabIndex = 13;
            this.btnAddFolder3.Tag = "3";
            this.btnAddFolder3.Text = "Add Folder";
            this.btnAddFolder3.UseVisualStyleBackColor = true;
            this.btnAddFolder3.Click += new System.EventHandler(this.btnAddFolder1_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(241, 290);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 25);
            this.button12.TabIndex = 12;
            this.button12.Tag = "3";
            this.button12.Text = "Add";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.btnAdd1_Click);
            // 
            // btnDelete2
            // 
            this.btnDelete2.Location = new System.Drawing.Point(403, 191);
            this.btnDelete2.Name = "btnDelete2";
            this.btnDelete2.Size = new System.Drawing.Size(75, 25);
            this.btnDelete2.TabIndex = 9;
            this.btnDelete2.Tag = "2";
            this.btnDelete2.Text = "Delete";
            this.btnDelete2.UseVisualStyleBackColor = true;
            this.btnDelete2.Click += new System.EventHandler(this.btnDelete1_Click);
            // 
            // btnAddFolder2
            // 
            this.btnAddFolder2.Location = new System.Drawing.Point(322, 191);
            this.btnAddFolder2.Name = "btnAddFolder2";
            this.btnAddFolder2.Size = new System.Drawing.Size(75, 25);
            this.btnAddFolder2.TabIndex = 8;
            this.btnAddFolder2.Tag = "2";
            this.btnAddFolder2.Text = "Add Folder";
            this.btnAddFolder2.UseVisualStyleBackColor = true;
            this.btnAddFolder2.Click += new System.EventHandler(this.btnAddFolder1_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(241, 191);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 25);
            this.button8.TabIndex = 7;
            this.button8.Tag = "2";
            this.button8.Text = "Add";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.btnAdd1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 217);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(153, 17);
            this.label3.TabIndex = 10;
            this.label3.Text = "Dependent Assemblies";
            // 
            // lstDependent
            // 
            this.lstDependent.FormattingEnabled = true;
            this.lstDependent.ItemHeight = 16;
            this.lstDependent.Location = new System.Drawing.Point(6, 235);
            this.lstDependent.Name = "lstDependent";
            this.lstDependent.Size = new System.Drawing.Size(472, 52);
            this.lstDependent.TabIndex = 11;
            this.lstDependent.Tag = "3";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Comments";
            // 
            // lstComments
            // 
            this.lstComments.FormattingEnabled = true;
            this.lstComments.ItemHeight = 16;
            this.lstComments.Location = new System.Drawing.Point(6, 136);
            this.lstComments.Name = "lstComments";
            this.lstComments.Size = new System.Drawing.Size(472, 52);
            this.lstComments.TabIndex = 6;
            this.lstComments.Tag = "2";
            // 
            // btnDelete1
            // 
            this.btnDelete1.Location = new System.Drawing.Point(403, 90);
            this.btnDelete1.Name = "btnDelete1";
            this.btnDelete1.Size = new System.Drawing.Size(75, 25);
            this.btnDelete1.TabIndex = 4;
            this.btnDelete1.Tag = "1";
            this.btnDelete1.Text = "Delete";
            this.btnDelete1.UseVisualStyleBackColor = true;
            this.btnDelete1.Click += new System.EventHandler(this.btnDelete1_Click);
            // 
            // btnAddFolder1
            // 
            this.btnAddFolder1.Location = new System.Drawing.Point(322, 90);
            this.btnAddFolder1.Name = "btnAddFolder1";
            this.btnAddFolder1.Size = new System.Drawing.Size(75, 25);
            this.btnAddFolder1.TabIndex = 3;
            this.btnAddFolder1.Tag = "1";
            this.btnAddFolder1.Text = "Add Folder";
            this.btnAddFolder1.UseVisualStyleBackColor = true;
            this.btnAddFolder1.Click += new System.EventHandler(this.btnAddFolder1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Assemblies";
            // 
            // btnAdd1
            // 
            this.btnAdd1.Location = new System.Drawing.Point(241, 90);
            this.btnAdd1.Name = "btnAdd1";
            this.btnAdd1.Size = new System.Drawing.Size(75, 25);
            this.btnAdd1.TabIndex = 2;
            this.btnAdd1.Tag = "1";
            this.btnAdd1.Text = "Add";
            this.btnAdd1.UseVisualStyleBackColor = true;
            this.btnAdd1.Click += new System.EventHandler(this.btnAdd1_Click);
            // 
            // lstDll
            // 
            this.lstDll.FormattingEnabled = true;
            this.lstDll.ItemHeight = 16;
            this.lstDll.Location = new System.Drawing.Point(6, 35);
            this.lstDll.Name = "lstDll";
            this.lstDll.Size = new System.Drawing.Size(472, 52);
            this.lstDll.TabIndex = 1;
            this.lstDll.Tag = "1";
            // 
            // rtbLog
            // 
            this.rtbLog.Location = new System.Drawing.Point(12, 385);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(768, 150);
            this.rtbLog.TabIndex = 6;
            this.rtbLog.Text = "";
            // 
            // btnBuild
            // 
            this.btnBuild.Location = new System.Drawing.Point(359, 541);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(75, 25);
            this.btnBuild.TabIndex = 7;
            this.btnBuild.Text = "Build";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ckbWeb);
            this.groupBox2.Controls.Add(this.cmbLanguages);
            this.groupBox2.Controls.Add(this.ckbHxs);
            this.groupBox2.Controls.Add(this.ckbChm);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cmbTopicStyle);
            this.groupBox2.Controls.Add(this.txtName);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(504, 31);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(276, 200);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // ckbWeb
            // 
            this.ckbWeb.AutoSize = true;
            this.ckbWeb.Location = new System.Drawing.Point(191, 170);
            this.ckbWeb.Name = "ckbWeb";
            this.ckbWeb.Size = new System.Drawing.Size(59, 21);
            this.ckbWeb.TabIndex = 9;
            this.ckbWeb.Text = "Web";
            this.ckbWeb.UseVisualStyleBackColor = true;
            // 
            // cmbLanguages
            // 
            this.cmbLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguages.FormattingEnabled = true;
            this.cmbLanguages.Items.AddRange(new object[] {
            "1033",
            "2052"});
            this.cmbLanguages.Location = new System.Drawing.Point(19, 119);
            this.cmbLanguages.Name = "cmbLanguages";
            this.cmbLanguages.Size = new System.Drawing.Size(234, 24);
            this.cmbLanguages.TabIndex = 5;
            // 
            // ckbHxs
            // 
            this.ckbHxs.AutoSize = true;
            this.ckbHxs.Location = new System.Drawing.Point(113, 170);
            this.ckbHxs.Name = "ckbHxs";
            this.ckbHxs.Size = new System.Drawing.Size(53, 21);
            this.ckbHxs.TabIndex = 8;
            this.ckbHxs.Text = "Hxs";
            this.ckbHxs.UseVisualStyleBackColor = true;
            // 
            // ckbChm
            // 
            this.ckbChm.AutoSize = true;
            this.ckbChm.Checked = true;
            this.ckbChm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbChm.Location = new System.Drawing.Point(19, 170);
            this.ckbChm.Name = "ckbChm";
            this.ckbChm.Size = new System.Drawing.Size(58, 21);
            this.ckbChm.TabIndex = 7;
            this.ckbChm.Text = "Chm";
            this.ckbChm.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 17);
            this.label5.TabIndex = 6;
            this.label5.Text = "Default Targets:";
            // 
            // cmbTopicStyle
            // 
            this.cmbTopicStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTopicStyle.FormattingEnabled = true;
            this.cmbTopicStyle.Items.AddRange(new object[] {
            "vs2005",
            "vs2010",
            "hana",
            "prototype"});
            this.cmbTopicStyle.Location = new System.Drawing.Point(150, 69);
            this.cmbTopicStyle.Name = "cmbTopicStyle";
            this.cmbTopicStyle.Size = new System.Drawing.Size(103, 24);
            this.cmbTopicStyle.TabIndex = 3;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(19, 35);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(235, 22);
            this.txtName.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 17);
            this.label6.TabIndex = 4;
            this.label6.Text = "Language ID:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 72);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 17);
            this.label7.TabIndex = 2;
            this.label7.Text = "Presenation Style: ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 17);
            this.label8.TabIndex = 0;
            this.label8.Text = "Name: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 368);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "Log";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(792, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniNew,
            this.mniOpen,
            this.mniSave,
            this.mniExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // mniNew
            // 
            this.mniNew.Name = "mniNew";
            this.mniNew.Size = new System.Drawing.Size(158, 24);
            this.mniNew.Text = "New Project";
            this.mniNew.Click += new System.EventHandler(this.mniNew_Click);
            // 
            // mniOpen
            // 
            this.mniOpen.Name = "mniOpen";
            this.mniOpen.Size = new System.Drawing.Size(158, 24);
            this.mniOpen.Text = "Open";
            this.mniOpen.Click += new System.EventHandler(this.mniOpen_Click);
            // 
            // mniSave
            // 
            this.mniSave.Name = "mniSave";
            this.mniSave.Size = new System.Drawing.Size(158, 24);
            this.mniSave.Text = "Save";
            this.mniSave.Click += new System.EventHandler(this.mniSave_Click);
            // 
            // mniExit
            // 
            this.mniExit.Name = "mniExit";
            this.mniExit.Size = new System.Drawing.Size(158, 24);
            this.mniExit.Text = "Exit";
            this.mniExit.Click += new System.EventHandler(this.mniExit_Click);
            // 
            // folderDialog
            // 
            this.folderDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderDialog.ShowNewFolderButton = false;
            // 
            // saveDialog
            // 
            this.saveDialog.Filter = "Project file (*.scproj)|*.scproj";
            // 
            // openDialog
            // 
            this.openDialog.Filter = "Project file (*.scproj)|*.scproj";
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.SystemColors.Info;
            this.label9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label9.Location = new System.Drawing.Point(504, 234);
            this.label9.Name = "label9";
            this.label9.Padding = new System.Windows.Forms.Padding(3);
            this.label9.Size = new System.Drawing.Size(276, 148);
            this.label9.TabIndex = 3;
            this.label9.Text = resources.GetString("label9.Text");
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.BackColor = System.Drawing.SystemColors.Info;
            this.lnkProjectSite.Location = new System.Drawing.Point(509, 358);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(266, 23);
            this.lnkProjectSite.TabIndex = 4;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "http://SHFB.CodePlex.com";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectSite_LinkClicked);
            // 
            // FrmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(792, 573);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sandcastle Documentation Builder (Example GUI)";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAdd1;
        private System.Windows.Forms.Button btnAddFolder1;
        private System.Windows.Forms.Button btnAddFolder2;
        private System.Windows.Forms.Button btnAddFolder3;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Button btnDelete1;
        private System.Windows.Forms.Button btnDelete2;
        private System.Windows.Forms.Button btnDelete3;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckBox ckbChm;
        private System.Windows.Forms.CheckBox ckbHxs;
        private System.Windows.Forms.CheckBox ckbWeb;
        private System.Windows.Forms.ComboBox cmbLanguages;
        private System.Windows.Forms.ComboBox cmbTopicStyle;
        private System.Windows.Forms.OpenFileDialog fileDialog;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderDialog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListBox lstComments;
        private System.Windows.Forms.ListBox lstDependent;
        private System.Windows.Forms.ListBox lstDll;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mniExit;
        private System.Windows.Forms.ToolStripMenuItem mniNew;
        private System.Windows.Forms.ToolStripMenuItem mniOpen;
        private System.Windows.Forms.ToolStripMenuItem mniSave;
        private System.Windows.Forms.OpenFileDialog openDialog;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.SaveFileDialog saveDialog;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
    }
}
