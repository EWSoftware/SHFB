namespace SandcastleBuilder.PlugIns
{
    partial class DbcsFixConfigDlg
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
            this.lnkSteelBytes = new System.Windows.Forms.LinkLabel();
            this.btnSelectLocation = new System.Windows.Forms.Button();
            this.txtSBAppLocalePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(731, 167);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Exit without saving changes");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 167);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save changes to configuration");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lnkProjectSite
            // 
            this.lnkProjectSite.Location = new System.Drawing.Point(118, 171);
            this.lnkProjectSite.Name = "lnkProjectSite";
            this.lnkProjectSite.Size = new System.Drawing.Size(280, 26);
            this.lnkProjectSite.TabIndex = 5;
            this.lnkProjectSite.TabStop = true;
            this.lnkProjectSite.Text = "Sandcastle Help File Builder";
            this.lnkProjectSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkProjectSite, "https://GitHub.com/EWSoftware/SHFB");
            this.lnkProjectSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.project_LinkClicked);
            // 
            // lnkSteelBytes
            // 
            this.lnkSteelBytes.Location = new System.Drawing.Point(433, 171);
            this.lnkSteelBytes.Name = "lnkSteelBytes";
            this.lnkSteelBytes.Size = new System.Drawing.Size(292, 26);
            this.lnkSteelBytes.TabIndex = 6;
            this.lnkSteelBytes.TabStop = true;
            this.lnkSteelBytes.Text = "Steel Bytes SBAppLocale";
            this.lnkSteelBytes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lnkSteelBytes, "http://www.SteelBytes.com/?mid=45");
            this.lnkSteelBytes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.project_LinkClicked);
            // 
            // btnSelectLocation
            // 
            this.btnSelectLocation.Location = new System.Drawing.Point(671, 90);
            this.btnSelectLocation.Name = "btnSelectLocation";
            this.btnSelectLocation.Size = new System.Drawing.Size(35, 35);
            this.btnSelectLocation.TabIndex = 2;
            this.btnSelectLocation.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectLocation, "Select the location of the Steel Bytes AppLocale tool");
            this.btnSelectLocation.UseVisualStyleBackColor = true;
            this.btnSelectLocation.Click += new System.EventHandler(this.btnSelectLocation_Click);
            // 
            // txtSBAppLocalePath
            // 
            this.txtSBAppLocalePath.Location = new System.Drawing.Point(136, 92);
            this.txtSBAppLocalePath.MaxLength = 256;
            this.txtSBAppLocalePath.Name = "txtSBAppLocalePath";
            this.txtSBAppLocalePath.Size = new System.Drawing.Size(535, 31);
            this.txtSBAppLocalePath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(819, 66);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to SBAppLocale executable.  Leave this blank if you do not want to use it.  " +
    "In that case, only encoding changes applied by the Sandcastle HTML Extract tool " +
    "will be applied.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DbcsFixConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(843, 214);
            this.Controls.Add(this.btnSelectLocation);
            this.Controls.Add(this.lnkSteelBytes);
            this.Controls.Add(this.txtSBAppLocalePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lnkProjectSite);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DbcsFixConfigDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure DBCS Fix Plug-In";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.LinkLabel lnkProjectSite;
        private System.Windows.Forms.TextBox txtSBAppLocalePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel lnkSteelBytes;
        private System.Windows.Forms.Button btnSelectLocation;
    }
}