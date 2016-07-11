namespace SandcastleBuilder.Components.UI
{
    partial class ESentReflectionIndexConfigDlg
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
            this.lnkProjectSite = new System.Windows.Forms.LinkLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSelectFrameworkIndexCacheFolder = new System.Windows.Forms.Button();
            this.txtFrameworkIndexCachePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.udcLocalCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkEnableLocalCache = new System.Windows.Forms.CheckBox();
            this.btnSelectProjectIndexCacheFolder = new System.Windows.Forms.Button();
            this.txtProjectIndexCachePath = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnPurge = new System.Windows.Forms.Button();
            this.udcInMemoryCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcLocalCacheSize)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcInMemoryCacheSize)).BeginInit();
            this.SuspendLayout();
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(310, 284);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(290, 26);
            this.lnkProjectSite.TabIndex = 9;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectSite_LinkClicked);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(798, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Cancel without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 280);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save settings");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSelectFrameworkIndexCacheFolder);
            this.groupBox2.Controls.Add(this.txtFrameworkIndexCachePath);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(886, 78);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "&Framework Reflection Index Cache";
            // 
            // btnSelectFrameworkIndexCacheFolder
            // 
            this.btnSelectFrameworkIndexCacheFolder.Location = new System.Drawing.Point(705, 28);
            this.btnSelectFrameworkIndexCacheFolder.Name = "btnSelectFrameworkIndexCacheFolder";
            this.btnSelectFrameworkIndexCacheFolder.Size = new System.Drawing.Size(35, 35);
            this.btnSelectFrameworkIndexCacheFolder.TabIndex = 2;
            this.btnSelectFrameworkIndexCacheFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectFrameworkIndexCacheFolder, "Select the folder in which to place the Framework targets cache");
            this.btnSelectFrameworkIndexCacheFolder.UseVisualStyleBackColor = true;
            this.btnSelectFrameworkIndexCacheFolder.Click += new System.EventHandler(this.btnSelectCacheFolder_Click);
            // 
            // txtFrameworkIndexCachePath
            // 
            this.txtFrameworkIndexCachePath.Location = new System.Drawing.Point(236, 30);
            this.txtFrameworkIndexCachePath.MaxLength = 256;
            this.txtFrameworkIndexCachePath.Name = "txtFrameworkIndexCachePath";
            this.txtFrameworkIndexCachePath.Size = new System.Drawing.Size(467, 31);
            this.txtFrameworkIndexCachePath.TabIndex = 1;
            this.txtFrameworkIndexCachePath.Text = "{@LocalDataFolder}Cache\\ReflectionIndexCache";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(59, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(171, 26);
            this.label4.TabIndex = 0;
            this.label4.Text = "Cache Location";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(663, 193);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(235, 26);
            this.label6.TabIndex = 4;
            this.label6.Text = "(0 to disable local cache)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // udcLocalCacheSize
            // 
            this.udcLocalCacheSize.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udcLocalCacheSize.Location = new System.Drawing.Point(577, 192);
            this.udcLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcLocalCacheSize.Name = "udcLocalCacheSize";
            this.udcLocalCacheSize.Size = new System.Drawing.Size(80, 31);
            this.udcLocalCacheSize.TabIndex = 3;
            this.udcLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(347, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(224, 26);
            this.label3.TabIndex = 2;
            this.label3.Text = "Local Cache Size";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkEnableLocalCache);
            this.groupBox3.Controls.Add(this.btnSelectProjectIndexCacheFolder);
            this.groupBox3.Controls.Add(this.txtProjectIndexCachePath);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Location = new System.Drawing.Point(12, 97);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(886, 78);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            // 
            // chkEnableLocalCache
            // 
            this.chkEnableLocalCache.AutoSize = true;
            this.chkEnableLocalCache.Location = new System.Drawing.Point(6, -1);
            this.chkEnableLocalCache.Name = "chkEnableLocalCache";
            this.chkEnableLocalCache.Size = new System.Drawing.Size(462, 29);
            this.chkEnableLocalCache.TabIndex = 0;
            this.chkEnableLocalCache.Text = "&Enable caching of current project reflection index data";
            this.chkEnableLocalCache.UseVisualStyleBackColor = true;
            this.chkEnableLocalCache.CheckedChanged += new System.EventHandler(this.chkEnableLocalCache_CheckedChanged);
            // 
            // btnSelectProjectIndexCacheFolder
            // 
            this.btnSelectProjectIndexCacheFolder.Enabled = false;
            this.btnSelectProjectIndexCacheFolder.Location = new System.Drawing.Point(705, 33);
            this.btnSelectProjectIndexCacheFolder.Name = "btnSelectProjectIndexCacheFolder";
            this.btnSelectProjectIndexCacheFolder.Size = new System.Drawing.Size(35, 35);
            this.btnSelectProjectIndexCacheFolder.TabIndex = 3;
            this.btnSelectProjectIndexCacheFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectProjectIndexCacheFolder, "Select the folder in which to place the current project\'s targets cache");
            this.btnSelectProjectIndexCacheFolder.UseVisualStyleBackColor = true;
            this.btnSelectProjectIndexCacheFolder.Click += new System.EventHandler(this.btnSelectCacheFolder_Click);
            // 
            // txtProjectIndexCachePath
            // 
            this.txtProjectIndexCachePath.Enabled = false;
            this.txtProjectIndexCachePath.Location = new System.Drawing.Point(236, 35);
            this.txtProjectIndexCachePath.MaxLength = 256;
            this.txtProjectIndexCachePath.Name = "txtProjectIndexCachePath";
            this.txtProjectIndexCachePath.Size = new System.Drawing.Size(467, 31);
            this.txtProjectIndexCachePath.TabIndex = 2;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(59, 37);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(171, 26);
            this.label9.TabIndex = 1;
            this.label9.Text = "Cache Location";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // btnPurge
            // 
            this.btnPurge.Location = new System.Drawing.Point(118, 280);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(100, 35);
            this.btnPurge.TabIndex = 8;
            this.btnPurge.Text = "Purge";
            this.toolTip1.SetToolTip(this.btnPurge, "Purge the reflection index caches");
            this.btnPurge.UseVisualStyleBackColor = true;
            this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
            // 
            // udcInMemoryCacheSize
            // 
            this.udcInMemoryCacheSize.Location = new System.Drawing.Point(577, 229);
            this.udcInMemoryCacheSize.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udcInMemoryCacheSize.Name = "udcInMemoryCacheSize";
            this.udcInMemoryCacheSize.Size = new System.Drawing.Size(80, 31);
            this.udcInMemoryCacheSize.TabIndex = 6;
            this.udcInMemoryCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcInMemoryCacheSize.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 230);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(568, 26);
            this.label2.TabIndex = 5;
            this.label2.Text = "Maximum number of uncached files kept in memory during build";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ESentReflectionIndexConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(910, 327);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnPurge);
            this.Controls.Add(this.udcLocalCacheSize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.udcInMemoryCacheSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ESentReflectionIndexConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure ESENT Copy From Index Component (Reflection Index)";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcLocalCacheSize)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcInMemoryCacheSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown udcLocalCacheSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelectFrameworkIndexCacheFolder;
        private System.Windows.Forms.TextBox txtFrameworkIndexCachePath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkEnableLocalCache;
        private System.Windows.Forms.Button btnSelectProjectIndexCacheFolder;
        private System.Windows.Forms.TextBox txtProjectIndexCachePath;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnPurge;
        private System.Windows.Forms.NumericUpDown udcInMemoryCacheSize;
        private System.Windows.Forms.Label label2;
    }
}