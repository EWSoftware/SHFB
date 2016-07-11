namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class SelectFileTemplateDlg
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
            this.tvTemplates = new System.Windows.Forms.TreeView();
            this.btnAddTopic = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.txtNewFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblExtension = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tvTemplates
            // 
            this.tvTemplates.Location = new System.Drawing.Point(12, 12);
            this.tvTemplates.Name = "tvTemplates";
            this.tvTemplates.Size = new System.Drawing.Size(477, 477);
            this.statusBarTextProvider1.SetStatusBarText(this.tvTemplates, "Templates: Select a template");
            this.tvTemplates.TabIndex = 0;
            this.tvTemplates.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTemplates_AfterSelect);
            // 
            // btnAddTopic
            // 
            this.btnAddTopic.Location = new System.Drawing.Point(12, 540);
            this.btnAddTopic.Name = "btnAddTopic";
            this.btnAddTopic.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddTopic, "Add: Add the topic template");
            this.btnAddTopic.TabIndex = 4;
            this.btnAddTopic.Text = "&Add";
            this.btnAddTopic.UseVisualStyleBackColor = true;
            this.btnAddTopic.Click += new System.EventHandler(this.btnAddTopic_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(389, 540);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnCancel, "Cancel: Close without adding the template");
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtNewFilename
            // 
            this.txtNewFilename.Location = new System.Drawing.Point(117, 495);
            this.txtNewFilename.Name = "txtNewFilename";
            this.txtNewFilename.Size = new System.Drawing.Size(280, 31);
            this.statusBarTextProvider1.SetStatusBarText(this.txtNewFilename, "Filename: Enter the new topic\'s filename (no path or extension)");
            this.txtNewFilename.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 497);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 26);
            this.label1.TabIndex = 1;
            this.label1.Text = "&Filename";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblExtension
            // 
            this.lblExtension.Location = new System.Drawing.Point(403, 497);
            this.lblExtension.Name = "lblExtension";
            this.lblExtension.Size = new System.Drawing.Size(86, 26);
            this.lblExtension.TabIndex = 3;
            this.lblExtension.Text = ".xxx";
            this.lblExtension.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SelectFileTemplateDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(501, 587);
            this.Controls.Add(this.lblExtension);
            this.Controls.Add(this.txtNewFilename);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAddTopic);
            this.Controls.Add(this.tvTemplates);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectFileTemplateDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select a Topic File Template";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvTemplates;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.Button btnAddTopic;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNewFilename;
        private System.Windows.Forms.Label lblExtension;
    }
}