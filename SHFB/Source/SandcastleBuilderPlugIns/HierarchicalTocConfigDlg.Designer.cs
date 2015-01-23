namespace SandcastleBuilder.PlugIns
{
    partial class HierarchicalTocConfigDlg
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.udcMinParts = new System.Windows.Forms.NumericUpDown();
            this.chkInsertBelow = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udcMinParts)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(510, 147);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 147);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(196, 152);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(218, 23);
            this.lnkProjectSite.TabIndex = 6;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.project_LinkClicked);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(55, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(425, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Create containers for common root namespaces with more than";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(534, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "part(s)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // udcMinParts
            // 
            this.udcMinParts.Location = new System.Drawing.Point(486, 84);
            this.udcMinParts.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.udcMinParts.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udcMinParts.Name = "udcMinParts";
            this.udcMinParts.Size = new System.Drawing.Size(42, 22);
            this.udcMinParts.TabIndex = 2;
            this.udcMinParts.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcMinParts.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // chkInsertBelow
            // 
            this.chkInsertBelow.Location = new System.Drawing.Point(106, 112);
            this.chkInsertBelow.Name = "chkInsertBelow";
            this.chkInsertBelow.Size = new System.Drawing.Size(484, 21);
            this.chkInsertBelow.TabIndex = 4;
            this.chkInsertBelow.Text = "Insert nested namespaces below the parent namespace\'s content";
            this.chkInsertBelow.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Info;
            this.panel1.Controls.Add(this.label3);
            this.panel1.ForeColor = System.Drawing.SystemColors.InfoText;
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(621, 60);
            this.panel1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(13, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(594, 43);
            this.label3.TabIndex = 0;
            this.label3.Text = "This plug-in has been deprecated.  Use the NamespaceGrouping and MaximumGroupPart" +
    "s project options instead.";
            // 
            // HierarchicalTocConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(645, 191);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkInsertBelow);
            this.Controls.Add(this.udcMinParts);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HierarchicalTocConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Hierarchical TOC Plug-In";
            ((System.ComponentModel.ISupportInitialize)(this.udcMinParts)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udcMinParts;
        private System.Windows.Forms.CheckBox chkInsertBelow;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
    }
}