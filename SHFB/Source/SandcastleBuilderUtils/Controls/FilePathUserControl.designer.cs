namespace SandcastleBuilder.Utils.Controls
{
    partial class FilePathUserControl
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
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.chkFixedPath = new System.Windows.Forms.CheckBox();
            this.lblExpandedPath = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtFile
            // 
            this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFile.Location = new System.Drawing.Point(0, 1);
            this.txtFile.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(299, 22);
            this.txtFile.TabIndex = 3;
            this.txtFile.Leave += new System.EventHandler(this.txtFile_Leave);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFile.Location = new System.Drawing.Point(300, 0);
            this.btnSelectFile.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(32, 24);
            this.btnSelectFile.TabIndex = 4;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // chkFixedPath
            // 
            this.chkFixedPath.AutoSize = true;
            this.chkFixedPath.Location = new System.Drawing.Point(3, 26);
            this.chkFixedPath.Name = "chkFixedPath";
            this.chkFixedPath.Size = new System.Drawing.Size(95, 21);
            this.chkFixedPath.TabIndex = 5;
            this.chkFixedPath.Text = "Fixed path";
            this.chkFixedPath.UseVisualStyleBackColor = true;
            this.chkFixedPath.CheckedChanged += new System.EventHandler(this.chkFixedPath_CheckedChanged);
            // 
            // lblExpandedPath
            // 
            this.lblExpandedPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExpandedPath.AutoEllipsis = true;
            this.lblExpandedPath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblExpandedPath.Location = new System.Drawing.Point(104, 26);
            this.lblExpandedPath.Name = "lblExpandedPath";
            this.lblExpandedPath.Size = new System.Drawing.Size(228, 23);
            this.lblExpandedPath.TabIndex = 6;
            this.lblExpandedPath.Text = "(Not specified)";
            this.lblExpandedPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FilePathUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.lblExpandedPath);
            this.Controls.Add(this.chkFixedPath);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnSelectFile);
            this.Name = "FilePathUserControl";
            this.Size = new System.Drawing.Size(335, 50);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.CheckBox chkFixedPath;
        private System.Windows.Forms.Label lblExpandedPath;
    }
}
