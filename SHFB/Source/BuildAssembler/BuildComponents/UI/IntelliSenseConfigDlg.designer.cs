namespace Microsoft.Ddue.Tools.UI
{
    partial class IntelliSenseConfigDlg
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
            this.txtNamespacesFile = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkIncludeNamespaces = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.udcBoundedCapacity = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.udcBoundedCapacity)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(662, 195);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 195);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(230, 199);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(315, 26);
            this.lnkProjectSite.TabIndex = 10;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectSite_LinkClicked);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(649, 91);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(35, 35);
            this.btnSelectFolder.TabIndex = 5;
            this.btnSelectFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectFolder, "Select output folder");
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // txtNamespacesFile
            // 
            this.txtNamespacesFile.Location = new System.Drawing.Point(379, 12);
            this.txtNamespacesFile.MaxLength = 256;
            this.txtNamespacesFile.Name = "txtNamespacesFile";
            this.txtNamespacesFile.Size = new System.Drawing.Size(178, 31);
            this.txtNamespacesFile.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(605, 26);
            this.label1.TabIndex = 3;
            this.label1.Text = "&Folder in which to create the IntelliSense files";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(12, 93);
            this.txtFolder.MaxLength = 256;
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(636, 31);
            this.txtFolder.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(563, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(199, 26);
            this.label2.TabIndex = 2;
            this.label2.Text = "(no path or extension)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkIncludeNamespaces
            // 
            this.chkIncludeNamespaces.Location = new System.Drawing.Point(12, 14);
            this.chkIncludeNamespaces.Name = "chkIncludeNamespaces";
            this.chkIncludeNamespaces.Size = new System.Drawing.Size(361, 29);
            this.chkIncludeNamespaces.TabIndex = 0;
            this.chkIncludeNamespaces.Text = "&Export project/namespace comments to";
            this.chkIncludeNamespaces.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(319, 26);
            this.label3.TabIndex = 6;
            this.label3.Text = "&Maximum writer task cache capacity";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(414, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(222, 26);
            this.label4.TabIndex = 8;
            this.label4.Text = "(0 for no limit)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // udcBoundedCapacity
            // 
            this.udcBoundedCapacity.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udcBoundedCapacity.Location = new System.Drawing.Point(332, 139);
            this.udcBoundedCapacity.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.udcBoundedCapacity.Name = "udcBoundedCapacity";
            this.udcBoundedCapacity.Size = new System.Drawing.Size(76, 31);
            this.udcBoundedCapacity.TabIndex = 7;
            this.udcBoundedCapacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcBoundedCapacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // IntelliSenseConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(774, 242);
            this.Controls.Add(this.udcBoundedCapacity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.chkIncludeNamespaces);
            this.Controls.Add(this.txtNamespacesFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IntelliSenseConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure IntelliSense Component";
            ((System.ComponentModel.ISupportInitialize)(this.udcBoundedCapacity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtNamespacesFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.CheckBox chkIncludeNamespaces;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown udcBoundedCapacity;
    }
}