namespace SandcastleBuilder.Package.PropertyPages
{
    partial class HelpFilePropertiesPageControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.host = new System.Windows.Forms.Integration.ElementHost();
            this.ucHelpFilePropertiesPageContent = new SandcastleBuilder.WPF.PropertyPages.HelpFilePropertiesPageContent();
            this.SuspendLayout();
            // 
            // host
            // 
            this.host.Dock = System.Windows.Forms.DockStyle.Fill;
            this.host.Location = new System.Drawing.Point(0, 0);
            this.host.Name = "host";
            this.host.Size = new System.Drawing.Size(400, 400);
            this.host.TabIndex = 0;
            this.host.Child = this.ucHelpFilePropertiesPageContent;
            // 
            // HelpFilePropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.host);
            this.Name = "HelpFilePropertiesPageControl";
            this.Size = new System.Drawing.Size(400, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost host;
        private WPF.PropertyPages.HelpFilePropertiesPageContent ucHelpFilePropertiesPageContent;
    }
}
