namespace SandcastleBuilder.Components.UI
{
    partial class SqlResolveReferenceLinksConfigDlg
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.udcContentIdLocalCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSetConnectionString = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.lnkProjectSite = new System.Windows.Forms.LinkLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.udcFrameworkTargetsLocalCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkEnableLocalCache = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.udcProjectTargetsLocalCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnPurge = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcContentIdLocalCacheSize)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcFrameworkTargetsLocalCacheSize)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcProjectTargetsLocalCacheSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.udcContentIdLocalCacheSize);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 171);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(854, 77);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "&MSDN Content ID Cache";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(375, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(272, 26);
            this.label5.TabIndex = 2;
            this.label5.Text = "(0 to disable local cache)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // udcContentIdLocalCacheSize
            // 
            this.udcContentIdLocalCacheSize.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udcContentIdLocalCacheSize.Location = new System.Drawing.Point(277, 31);
            this.udcContentIdLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcContentIdLocalCacheSize.Name = "udcContentIdLocalCacheSize";
            this.udcContentIdLocalCacheSize.Size = new System.Drawing.Size(92, 31);
            this.udcContentIdLocalCacheSize.TabIndex = 1;
            this.udcContentIdLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcContentIdLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(79, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(192, 26);
            this.label2.TabIndex = 0;
            this.label2.Text = "Local Cache Size";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSetConnectionString
            // 
            this.btnSetConnectionString.Location = new System.Drawing.Point(6, 112);
            this.btnSetConnectionString.Name = "btnSetConnectionString";
            this.btnSetConnectionString.Size = new System.Drawing.Size(100, 35);
            this.btnSetConnectionString.TabIndex = 1;
            this.btnSetConnectionString.Text = "&Setup";
            this.toolTip1.SetToolTip(this.btnSetConnectionString, "Set the connection string and configure the database");
            this.btnSetConnectionString.UseVisualStyleBackColor = true;
            this.btnSetConnectionString.Click += new System.EventHandler(this.btnSetConnectionString_Click);
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(6, 30);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.ReadOnly = true;
            this.txtConnectionString.Size = new System.Drawing.Size(842, 76);
            this.txtConnectionString.TabIndex = 0;
            this.txtConnectionString.Enter += new System.EventHandler(this.txtConnectionString_Enter);
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(295, 424);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(288, 26);
            this.lnkProjectSite.TabIndex = 6;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectSite_LinkClicked);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(766, 420);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Cancel without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 420);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save settings");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.udcFrameworkTargetsLocalCacheSize);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 254);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(854, 77);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "&Framework Reflection Targets Cache";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(375, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(272, 26);
            this.label6.TabIndex = 2;
            this.label6.Text = "(0 to disable local cache)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // udcFrameworkTargetsLocalCacheSize
            // 
            this.udcFrameworkTargetsLocalCacheSize.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udcFrameworkTargetsLocalCacheSize.Location = new System.Drawing.Point(277, 34);
            this.udcFrameworkTargetsLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcFrameworkTargetsLocalCacheSize.Name = "udcFrameworkTargetsLocalCacheSize";
            this.udcFrameworkTargetsLocalCacheSize.Size = new System.Drawing.Size(92, 31);
            this.udcFrameworkTargetsLocalCacheSize.TabIndex = 1;
            this.udcFrameworkTargetsLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcFrameworkTargetsLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(79, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 26);
            this.label3.TabIndex = 0;
            this.label3.Text = "Local Cache Size";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkEnableLocalCache);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.udcProjectTargetsLocalCacheSize);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Location = new System.Drawing.Point(12, 337);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(854, 77);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            // 
            // chkEnableLocalCache
            // 
            this.chkEnableLocalCache.AutoSize = true;
            this.chkEnableLocalCache.Location = new System.Drawing.Point(6, -1);
            this.chkEnableLocalCache.Name = "chkEnableLocalCache";
            this.chkEnableLocalCache.Size = new System.Drawing.Size(467, 29);
            this.chkEnableLocalCache.TabIndex = 0;
            this.chkEnableLocalCache.Text = "&Enable caching of current project reflection target data";
            this.chkEnableLocalCache.UseVisualStyleBackColor = true;
            this.chkEnableLocalCache.CheckedChanged += new System.EventHandler(this.chkEnableLocalCache_CheckedChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(375, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(272, 26);
            this.label7.TabIndex = 3;
            this.label7.Text = "(0 to disable local cache)";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // udcProjectTargetsLocalCacheSize
            // 
            this.udcProjectTargetsLocalCacheSize.Enabled = false;
            this.udcProjectTargetsLocalCacheSize.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udcProjectTargetsLocalCacheSize.Location = new System.Drawing.Point(277, 37);
            this.udcProjectTargetsLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcProjectTargetsLocalCacheSize.Name = "udcProjectTargetsLocalCacheSize";
            this.udcProjectTargetsLocalCacheSize.Size = new System.Drawing.Size(92, 31);
            this.udcProjectTargetsLocalCacheSize.TabIndex = 2;
            this.udcProjectTargetsLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcProjectTargetsLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(79, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(192, 26);
            this.label8.TabIndex = 1;
            this.label8.Text = "Local Cache Size";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // btnPurge
            // 
            this.btnPurge.Location = new System.Drawing.Point(118, 420);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(100, 35);
            this.btnPurge.TabIndex = 5;
            this.btnPurge.Text = "Purge";
            this.toolTip1.SetToolTip(this.btnPurge, "Purge the content ID and target caches");
            this.btnPurge.UseVisualStyleBackColor = true;
            this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtConnectionString);
            this.groupBox4.Controls.Add(this.btnSetConnectionString);
            this.groupBox4.Location = new System.Drawing.Point(12, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(854, 153);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "&Connection String";
            // 
            // SqlResolveReferenceLinksConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(878, 467);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.btnPurge);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SqlResolveReferenceLinksConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure SQL Resolve Reference Links Component";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.udcContentIdLocalCacheSize)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.udcFrameworkTargetsLocalCacheSize)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcProjectTargetsLocalCacheSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSetConnectionString;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udcContentIdLocalCacheSize;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown udcFrameworkTargetsLocalCacheSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkEnableLocalCache;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown udcProjectTargetsLocalCacheSize;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnPurge;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}