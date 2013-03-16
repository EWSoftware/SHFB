namespace SandcastleBuilder.Components.UI
{
    partial class ESentResolveReferenceLinksConfigDlg
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
            this.btnSelectContentIdCacheFolder = new System.Windows.Forms.Button();
            this.txtContentIdCachePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lnkCodePlexSHFB = new System.Windows.Forms.LinkLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.udcFrameworkTargetsLocalCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSelectFrameworkTargetsCacheFolder = new System.Windows.Forms.Button();
            this.txtFrameworkTargetsCachePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkEnableLocalCache = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.udcProjectTargetsLocalCacheSize = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.btnSelectProjectTargetsCacheFolder = new System.Windows.Forms.Button();
            this.txtProjectTargetsCachePath = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnPurge = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcContentIdLocalCacheSize)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcFrameworkTargetsLocalCacheSize)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcProjectTargetsLocalCacheSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.udcContentIdLocalCacheSize);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnSelectContentIdCacheFolder);
            this.groupBox1.Controls.Add(this.txtContentIdCachePath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(671, 90);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "&MSDN Content ID Cache";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(213, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(170, 23);
            this.label5.TabIndex = 5;
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
            this.udcContentIdLocalCacheSize.Location = new System.Drawing.Point(151, 57);
            this.udcContentIdLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcContentIdLocalCacheSize.Name = "udcContentIdLocalCacheSize";
            this.udcContentIdLocalCacheSize.Size = new System.Drawing.Size(56, 22);
            this.udcContentIdLocalCacheSize.TabIndex = 4;
            this.udcContentIdLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcContentIdLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(18, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Local Cache Size";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSelectContentIdCacheFolder
            // 
            this.btnSelectContentIdCacheFolder.Location = new System.Drawing.Point(620, 28);
            this.btnSelectContentIdCacheFolder.Name = "btnSelectContentIdCacheFolder";
            this.btnSelectContentIdCacheFolder.Size = new System.Drawing.Size(32, 25);
            this.btnSelectContentIdCacheFolder.TabIndex = 2;
            this.btnSelectContentIdCacheFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectContentIdCacheFolder, "Select the folder in which to place the MSDN content ID cache");
            this.btnSelectContentIdCacheFolder.UseVisualStyleBackColor = true;
            this.btnSelectContentIdCacheFolder.Click += new System.EventHandler(this.btnSelectCacheFolder_Click);
            // 
            // txtContentIdCachePath
            // 
            this.txtContentIdCachePath.Location = new System.Drawing.Point(151, 29);
            this.txtContentIdCachePath.MaxLength = 256;
            this.txtContentIdCachePath.Name = "txtContentIdCachePath";
            this.txtContentIdCachePath.Size = new System.Drawing.Size(467, 22);
            this.txtContentIdCachePath.TabIndex = 1;
            this.txtContentIdCachePath.Text = "{@LocalDataFolder}Cache\\ESentMsdnContentIdCache";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(21, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Cache Location";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lnkCodePlexSHFB
            // 
            this.lnkCodePlexSHFB.Location = new System.Drawing.Point(282, 305);
            this.lnkCodePlexSHFB.Name = "lnkCodePlexSHFB";
            this.lnkCodePlexSHFB.Size = new System.Drawing.Size(218, 23);
            this.lnkCodePlexSHFB.TabIndex = 5;
            this.lnkCodePlexSHFB.TabStop = true;
            this.lnkCodePlexSHFB.Text = "Sandcastle Help File Builder";
            this.lnkCodePlexSHFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkCodePlexSHFB, "http://SHFB.CodePlex.com");
            this.lnkCodePlexSHFB.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCodePlexSHFB_LinkClicked);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(595, 300);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Cancel without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 300);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.btnOK.TabIndex = 3;
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
            this.groupBox2.Controls.Add(this.btnSelectFrameworkTargetsCacheFolder);
            this.groupBox2.Controls.Add(this.txtFrameworkTargetsCachePath);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 108);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(671, 90);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "&Framework Reflection Targets Cache";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(213, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(170, 23);
            this.label6.TabIndex = 5;
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
            this.udcFrameworkTargetsLocalCacheSize.Location = new System.Drawing.Point(151, 57);
            this.udcFrameworkTargetsLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcFrameworkTargetsLocalCacheSize.Name = "udcFrameworkTargetsLocalCacheSize";
            this.udcFrameworkTargetsLocalCacheSize.Size = new System.Drawing.Size(56, 22);
            this.udcFrameworkTargetsLocalCacheSize.TabIndex = 4;
            this.udcFrameworkTargetsLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcFrameworkTargetsLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(18, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "Local Cache Size";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSelectFrameworkTargetsCacheFolder
            // 
            this.btnSelectFrameworkTargetsCacheFolder.Location = new System.Drawing.Point(620, 28);
            this.btnSelectFrameworkTargetsCacheFolder.Name = "btnSelectFrameworkTargetsCacheFolder";
            this.btnSelectFrameworkTargetsCacheFolder.Size = new System.Drawing.Size(32, 25);
            this.btnSelectFrameworkTargetsCacheFolder.TabIndex = 2;
            this.btnSelectFrameworkTargetsCacheFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectFrameworkTargetsCacheFolder, "Select the folder in which to place the Framework targets cache");
            this.btnSelectFrameworkTargetsCacheFolder.UseVisualStyleBackColor = true;
            this.btnSelectFrameworkTargetsCacheFolder.Click += new System.EventHandler(this.btnSelectCacheFolder_Click);
            // 
            // txtFrameworkTargetsCachePath
            // 
            this.txtFrameworkTargetsCachePath.Location = new System.Drawing.Point(151, 29);
            this.txtFrameworkTargetsCachePath.MaxLength = 256;
            this.txtFrameworkTargetsCachePath.Name = "txtFrameworkTargetsCachePath";
            this.txtFrameworkTargetsCachePath.Size = new System.Drawing.Size(467, 22);
            this.txtFrameworkTargetsCachePath.TabIndex = 1;
            this.txtFrameworkTargetsCachePath.Text = "{@LocalDataFolder}Cache\\ESentFrameworkTargetCache";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(21, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 23);
            this.label4.TabIndex = 0;
            this.label4.Text = "Cache Location";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkEnableLocalCache);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.udcProjectTargetsLocalCacheSize);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.btnSelectProjectTargetsCacheFolder);
            this.groupBox3.Controls.Add(this.txtProjectTargetsCachePath);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Location = new System.Drawing.Point(12, 204);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(671, 90);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            // 
            // chkEnableLocalCache
            // 
            this.chkEnableLocalCache.AutoSize = true;
            this.chkEnableLocalCache.Location = new System.Drawing.Point(6, -1);
            this.chkEnableLocalCache.Name = "chkEnableLocalCache";
            this.chkEnableLocalCache.Size = new System.Drawing.Size(374, 21);
            this.chkEnableLocalCache.TabIndex = 0;
            this.chkEnableLocalCache.Text = "&Enable caching of current project reflection target data";
            this.chkEnableLocalCache.UseVisualStyleBackColor = true;
            this.chkEnableLocalCache.CheckedChanged += new System.EventHandler(this.chkEnableLocalCache_CheckedChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(213, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(170, 23);
            this.label7.TabIndex = 6;
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
            this.udcProjectTargetsLocalCacheSize.Location = new System.Drawing.Point(151, 57);
            this.udcProjectTargetsLocalCacheSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udcProjectTargetsLocalCacheSize.Name = "udcProjectTargetsLocalCacheSize";
            this.udcProjectTargetsLocalCacheSize.Size = new System.Drawing.Size(56, 22);
            this.udcProjectTargetsLocalCacheSize.TabIndex = 5;
            this.udcProjectTargetsLocalCacheSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcProjectTargetsLocalCacheSize.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(18, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(127, 23);
            this.label8.TabIndex = 4;
            this.label8.Text = "Local Cache Size";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSelectProjectTargetsCacheFolder
            // 
            this.btnSelectProjectTargetsCacheFolder.Enabled = false;
            this.btnSelectProjectTargetsCacheFolder.Location = new System.Drawing.Point(620, 28);
            this.btnSelectProjectTargetsCacheFolder.Name = "btnSelectProjectTargetsCacheFolder";
            this.btnSelectProjectTargetsCacheFolder.Size = new System.Drawing.Size(32, 25);
            this.btnSelectProjectTargetsCacheFolder.TabIndex = 3;
            this.btnSelectProjectTargetsCacheFolder.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectProjectTargetsCacheFolder, "Select the folder in which to place the current project\'s targets cache");
            this.btnSelectProjectTargetsCacheFolder.UseVisualStyleBackColor = true;
            this.btnSelectProjectTargetsCacheFolder.Click += new System.EventHandler(this.btnSelectCacheFolder_Click);
            // 
            // txtProjectTargetsCachePath
            // 
            this.txtProjectTargetsCachePath.Enabled = false;
            this.txtProjectTargetsCachePath.Location = new System.Drawing.Point(151, 29);
            this.txtProjectTargetsCachePath.MaxLength = 256;
            this.txtProjectTargetsCachePath.Name = "txtProjectTargetsCachePath";
            this.txtProjectTargetsCachePath.Size = new System.Drawing.Size(467, 22);
            this.txtProjectTargetsCachePath.TabIndex = 2;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(21, 29);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(124, 23);
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
            this.btnPurge.Location = new System.Drawing.Point(106, 300);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(88, 32);
            this.btnPurge.TabIndex = 4;
            this.btnPurge.Text = "Purge";
            this.toolTip1.SetToolTip(this.btnPurge, "Purge the content ID and target caches");
            this.btnPurge.UseVisualStyleBackColor = true;
            this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
            // 
            // ESentResolveReferenceLinksConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(695, 344);
            this.Controls.Add(this.btnPurge);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lnkCodePlexSHFB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ESentResolveReferenceLinksConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure ESent Resolve Reference Links Component";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcContentIdLocalCacheSize)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcFrameworkTargetsLocalCacheSize)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udcProjectTargetsLocalCacheSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSelectContentIdCacheFolder;
        private System.Windows.Forms.TextBox txtContentIdCachePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udcContentIdLocalCacheSize;
        private System.Windows.Forms.LinkLabel lnkCodePlexSHFB;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown udcFrameworkTargetsLocalCacheSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelectFrameworkTargetsCacheFolder;
        private System.Windows.Forms.TextBox txtFrameworkTargetsCachePath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkEnableLocalCache;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown udcProjectTargetsLocalCacheSize;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnSelectProjectTargetsCacheFolder;
        private System.Windows.Forms.TextBox txtProjectTargetsCachePath;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnPurge;
    }
}