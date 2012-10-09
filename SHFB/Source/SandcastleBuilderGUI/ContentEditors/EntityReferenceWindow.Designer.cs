namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class EntityReferenceWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityReferenceWindow));
            this.ehEntityReferencesHost = new System.Windows.Forms.Integration.ElementHost();
            this.ucEntityReferences = new SandcastleBuilder.WPF.UserControls.EntityReferencesControl();
            this.SuspendLayout();
            // 
            // ehEntityReferencesHost
            // 
            this.ehEntityReferencesHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehEntityReferencesHost.Location = new System.Drawing.Point(0, 0);
            this.ehEntityReferencesHost.Name = "ehEntityReferencesHost";
            this.ehEntityReferencesHost.Size = new System.Drawing.Size(358, 384);
            this.ehEntityReferencesHost.TabIndex = 0;
            this.ehEntityReferencesHost.Child = this.ucEntityReferences;
            // 
            // EntityReferenceWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(358, 384);
            this.Controls.Add(this.ehEntityReferencesHost);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
            | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
            | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
            | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "EntityReferenceWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            this.ShowInTaskbar = false;
            this.TabText = "Entity References";
            this.Text = "Entity References";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost ehEntityReferencesHost;
        private WPF.UserControls.EntityReferencesControl ucEntityReferences;

    }
}