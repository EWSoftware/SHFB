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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewTopicWindow));
            this.ehTopicPreviewerHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // ehTopicPreviewerHost
            // 
            this.ehTopicPreviewerHost.BackColor = System.Drawing.SystemColors.Window;
            this.ehTopicPreviewerHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehTopicPreviewerHost.Location = new System.Drawing.Point(0, 0);
            this.ehTopicPreviewerHost.Name = "ehTopicPreviewerHost";
            this.ehTopicPreviewerHost.Size = new System.Drawing.Size(882, 555);
            this.ehTopicPreviewerHost.TabIndex = 1;
            this.ehTopicPreviewerHost.Child = null;
            // 
            // PreviewTopicWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(882, 555);
            this.Controls.Add(this.ehTopicPreviewerHost);
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PreviewTopicWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ShowInTaskbar = false;
            this.TabText = "Topic Previewer";
            this.Text = "Topic Previewer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost ehTopicPreviewerHost;

    }
}
