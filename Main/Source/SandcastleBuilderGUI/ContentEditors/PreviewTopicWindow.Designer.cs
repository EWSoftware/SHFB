namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class PreviewTopicWindow
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
            if(disposing)
            {
                if(tempProject != null)
                    tempProject.Dispose();

                if(components != null)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewTopicWindow));
            this.wbPreview = new System.Windows.Forms.WebBrowser();
            this.lblLoading = new System.Windows.Forms.Label();
            this.pbWait = new System.Windows.Forms.PictureBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).BeginInit();
            this.SuspendLayout();
            // 
            // wbPreview
            // 
            this.wbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbPreview.Location = new System.Drawing.Point(0, 0);
            this.wbPreview.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbPreview.Name = "wbPreview";
            this.wbPreview.Size = new System.Drawing.Size(292, 260);
            this.wbPreview.TabIndex = 0;
            // 
            // lblLoading
            // 
            this.lblLoading.AutoSize = true;
            this.lblLoading.BackColor = System.Drawing.SystemColors.Window;
            this.lblLoading.Location = new System.Drawing.Point(43, 14);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(70, 17);
            this.lblLoading.TabIndex = 10;
            this.lblLoading.Text = "Building...";
            // 
            // pbWait
            // 
            this.pbWait.BackColor = System.Drawing.SystemColors.Window;
            this.pbWait.Image = global::SandcastleBuilder.Gui.Properties.Resources.SpinningWheel;
            this.pbWait.Location = new System.Drawing.Point(5, 5);
            this.pbWait.Name = "pbWait";
            this.pbWait.Size = new System.Drawing.Size(32, 32);
            this.pbWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbWait.TabIndex = 11;
            this.pbWait.TabStop = false;
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // PreviewTopicWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(292, 260);
            this.Controls.Add(this.lblLoading);
            this.Controls.Add(this.pbWait);
            this.Controls.Add(this.wbPreview);
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PreviewTopicWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.TabText = "Preview Topic";
            this.Text = "Preview Topic";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreviewTopicWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser wbPreview;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.PictureBox pbWait;
        private System.Windows.Forms.Timer timer;
    }
}
