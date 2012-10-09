namespace SandcastleBuilder.ProjectLauncher
{
    partial class ProjectLauncherForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectLauncherForm));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.rbSHFB = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.rbVisualStudio = new System.Windows.Forms.RadioButton();
            this.chkAlwaysUseSelection = new System.Windows.Forms.CheckBox();
            this.btnLaunch = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblNotInstalled = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SandcastleBuilder.ProjectLauncher.Properties.Resources.SandcastleLogo;
            this.pictureBox1.Location = new System.Drawing.Point(42, 69);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(36, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Enabled = false;
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(42, 141);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(36, 42);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // rbSHFB
            // 
            this.rbSHFB.AutoSize = true;
            this.rbSHFB.Location = new System.Drawing.Point(99, 80);
            this.rbSHFB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rbSHFB.Name = "rbSHFB";
            this.rbSHFB.Size = new System.Drawing.Size(234, 22);
            this.rbSHFB.TabIndex = 1;
            this.rbSHFB.TabStop = true;
            this.rbSHFB.Text = "&Sandcastle Help File Builder GUI";
            this.rbSHFB.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(426, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Use the following application to open help file builder project files:";
            // 
            // rbVisualStudio
            // 
            this.rbVisualStudio.AutoSize = true;
            this.rbVisualStudio.Location = new System.Drawing.Point(99, 151);
            this.rbVisualStudio.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rbVisualStudio.Name = "rbVisualStudio";
            this.rbVisualStudio.Size = new System.Drawing.Size(352, 22);
            this.rbVisualStudio.TabIndex = 2;
            this.rbVisualStudio.TabStop = true;
            this.rbVisualStudio.Text = "Latest version of &Visual Studio with SHFB Package";
            this.rbVisualStudio.UseVisualStyleBackColor = true;
            // 
            // chkAlwaysUseSelection
            // 
            this.chkAlwaysUseSelection.AutoSize = true;
            this.chkAlwaysUseSelection.Location = new System.Drawing.Point(42, 216);
            this.chkAlwaysUseSelection.Name = "chkAlwaysUseSelection";
            this.chkAlwaysUseSelection.Size = new System.Drawing.Size(387, 22);
            this.chkAlwaysUseSelection.TabIndex = 4;
            this.chkAlwaysUseSelection.Text = "Do not ask again.  &Always use the selected application.";
            this.chkAlwaysUseSelection.UseVisualStyleBackColor = true;
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(12, 310);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(88, 32);
            this.btnLaunch.TabIndex = 6;
            this.btnLaunch.Text = "&Launch";
            this.btnLaunch.UseVisualStyleBackColor = true;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(383, 310);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "&Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(60, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(396, 66);
            this.label2.TabIndex = 5;
            this.label2.Text = "If checked, you can run this from the help file builder\'s Start menu folder to ch" +
    "ange the default.";
            // 
            // lblNotInstalled
            // 
            this.lblNotInstalled.AutoSize = true;
            this.lblNotInstalled.Location = new System.Drawing.Point(118, 177);
            this.lblNotInstalled.Name = "lblNotInstalled";
            this.lblNotInstalled.Size = new System.Drawing.Size(222, 18);
            this.lblNotInstalled.TabIndex = 3;
            this.lblNotInstalled.Text = "(Extension package not installed)";
            this.lblNotInstalled.Visible = false;
            // 
            // ProjectLauncherForm
            // 
            this.AcceptButton = this.btnLaunch;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(483, 354);
            this.Controls.Add(this.lblNotInstalled);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnLaunch);
            this.Controls.Add(this.chkAlwaysUseSelection);
            this.Controls.Add(this.rbVisualStudio);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbSHFB);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProjectLauncherForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sandcastle Help File Builder Project Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProjectLauncherForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.RadioButton rbSHFB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbVisualStudio;
        private System.Windows.Forms.CheckBox chkAlwaysUseSelection;
        private System.Windows.Forms.Button btnLaunch;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblNotInstalled;
    }
}

