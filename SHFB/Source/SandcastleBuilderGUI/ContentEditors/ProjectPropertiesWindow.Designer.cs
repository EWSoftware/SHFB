namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class ProjectPropertiesWindow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectPropertiesWindow));
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.tvPropertyPages = new System.Windows.Forms.TreeView();
            this.pnlPropertyPages = new SandcastleBuilder.Gui.ContentEditors.ScrollablePanel();
            this.SuspendLayout();
            // 
            // tvPropertyPages
            // 
            this.tvPropertyPages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvPropertyPages.FullRowSelect = true;
            this.tvPropertyPages.HideSelection = false;
            this.tvPropertyPages.Location = new System.Drawing.Point(12, 12);
            this.tvPropertyPages.Name = "tvPropertyPages";
            this.tvPropertyPages.ShowLines = false;
            this.tvPropertyPages.ShowPlusMinus = false;
            this.tvPropertyPages.ShowRootLines = false;
            this.tvPropertyPages.Size = new System.Drawing.Size(208, 531);
            this.statusBarTextProvider1.SetStatusBarText(this.tvPropertyPages, "Select a property category");
            this.tvPropertyPages.TabIndex = 0;
            this.tvPropertyPages.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvPropertyPages_BeforeSelect);
            this.tvPropertyPages.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvPropertyPages_AfterSelect);
            // 
            // pnlPropertyPages
            // 
            this.pnlPropertyPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPropertyPages.AutoScroll = true;
            this.pnlPropertyPages.Location = new System.Drawing.Point(226, 12);
            this.pnlPropertyPages.Name = "pnlPropertyPages";
            this.pnlPropertyPages.Size = new System.Drawing.Size(544, 531);
            this.statusBarTextProvider1.SetStatusBarText(this.pnlPropertyPages, "Category properties");
            this.pnlPropertyPages.TabIndex = 1;
            // 
            // ProjectPropertiesWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(782, 555);
            this.Controls.Add(this.pnlPropertyPages);
            this.Controls.Add(this.tvPropertyPages);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "ProjectPropertiesWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ShowInTaskbar = false;
            this.TabText = "Project Properties";
            this.Text = "Project Properties";
            this.ResumeLayout(false);

        }

        #endregion

        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.TreeView tvPropertyPages;
        private ScrollablePanel pnlPropertyPages;
    }
}
