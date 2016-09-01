namespace SandcastleBuilder.Utils.MSBuild
{
    partial class SelectProjectsDlg
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
            this.lblSolutionName = new System.Windows.Forms.Label();
            this.rbAddSolution = new System.Windows.Forms.RadioButton();
            this.rbAddProjects = new System.Windows.Forms.RadioButton();
            this.cblProjects = new System.Windows.Forms.CheckedListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblSolutionName
            // 
            this.lblSolutionName.AutoEllipsis = true;
            this.lblSolutionName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSolutionName.Location = new System.Drawing.Point(12, 9);
            this.lblSolutionName.Name = "lblSolutionName";
            this.lblSolutionName.Size = new System.Drawing.Size(854, 26);
            this.lblSolutionName.TabIndex = 0;
            this.lblSolutionName.Text = "--";
            this.lblSolutionName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rbAddSolution
            // 
            this.rbAddSolution.AutoSize = true;
            this.rbAddSolution.Checked = true;
            this.rbAddSolution.Location = new System.Drawing.Point(12, 49);
            this.rbAddSolution.Name = "rbAddSolution";
            this.rbAddSolution.Size = new System.Drawing.Size(793, 29);
            this.rbAddSolution.TabIndex = 1;
            this.rbAddSolution.TabStop = true;
            this.rbAddSolution.Text = "Add the &solution as the documentation source (includes all projects below plus a" +
    "ny added later)";
            this.rbAddSolution.UseVisualStyleBackColor = true;
            // 
            // rbAddProjects
            // 
            this.rbAddProjects.AutoSize = true;
            this.rbAddProjects.Location = new System.Drawing.Point(12, 84);
            this.rbAddProjects.Name = "rbAddProjects";
            this.rbAddProjects.Size = new System.Drawing.Size(712, 29);
            this.rbAddProjects.TabIndex = 2;
            this.rbAddProjects.TabStop = true;
            this.rbAddProjects.Text = "Add only the following selected &projects from the solution as documentation sour" +
    "ces";
            this.rbAddProjects.UseVisualStyleBackColor = true;
            // 
            // cblProjects
            // 
            this.cblProjects.FormattingEnabled = true;
            this.cblProjects.IntegralHeight = false;
            this.cblProjects.Location = new System.Drawing.Point(12, 130);
            this.cblProjects.Name = "cblProjects";
            this.cblProjects.Size = new System.Drawing.Size(854, 316);
            this.cblProjects.TabIndex = 3;
            this.cblProjects.SelectedIndexChanged += new System.EventHandler(this.cblProjects_SelectedIndexChanged);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 452);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(766, 452);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SelectProjectsDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(878, 499);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cblProjects);
            this.Controls.Add(this.rbAddProjects);
            this.Controls.Add(this.rbAddSolution);
            this.Controls.Add(this.lblSolutionName);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectProjectsDlg";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Solution or Projects as Documentation Source(s)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSolutionName;
        private System.Windows.Forms.RadioButton rbAddSolution;
        private System.Windows.Forms.RadioButton rbAddProjects;
        private System.Windows.Forms.CheckedListBox cblProjects;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}