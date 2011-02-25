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
            this.pgProps = new SandcastleBuilder.Utils.Controls.CustomPropertyGrid();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.SuspendLayout();
            // 
            // pgProps
            // 
            this.pgProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgProps.Location = new System.Drawing.Point(0, 0);
            this.pgProps.Name = "pgProps";
            this.pgProps.PropertyNamePaneWidth = 330;
            this.pgProps.Size = new System.Drawing.Size(475, 480);
            this.statusBarTextProvider1.SetStatusBarText(this.pgProps, "Project properties");
            this.pgProps.TabIndex = 0;
            this.pgProps.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgProps_PropertyValueChanged);
            // 
            // ProjectPropertiesWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(475, 480);
            this.Controls.Add(this.pgProps);
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

        private SandcastleBuilder.Utils.Controls.CustomPropertyGrid pgProps;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
    }
}
