namespace SandcastleBuilder.Package.PropertyPages
{
    partial class SummaryPropertiesPageControl
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
            this.txtProjectSummary = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dividerLabel1 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.dividerLabel2 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.lblNamespaceSummaryState = new System.Windows.Forms.Label();
            this.btnEditNamespaces = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtProjectSummary
            // 
            this.txtProjectSummary.AcceptsReturn = true;
            this.txtProjectSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectSummary.Location = new System.Drawing.Point(3, 88);
            this.txtProjectSummary.Multiline = true;
            this.txtProjectSummary.Name = "txtProjectSummary";
            this.txtProjectSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtProjectSummary.Size = new System.Drawing.Size(557, 169);
            this.txtProjectSummary.TabIndex = 2;
            this.txtProjectSummary.Tag = "ProjectSummary";
            this.txtProjectSummary.Enter += new System.EventHandler(this.txtProjectSummary_Enter);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(3, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(557, 42);
            this.label2.TabIndex = 1;
            this.label2.Text = "&These comments will appear in the root namespaces page.  HTML markup can be used" +
    " to provide formatting, links for e-mail or to other websites, etc.";
            // 
            // dividerLabel1
            // 
            this.dividerLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dividerLabel1.Location = new System.Drawing.Point(3, 11);
            this.dividerLabel1.Name = "dividerLabel1";
            this.dividerLabel1.Size = new System.Drawing.Size(557, 23);
            this.dividerLabel1.TabIndex = 0;
            this.dividerLabel1.Text = "Project S&ummary";
            // 
            // dividerLabel2
            // 
            this.dividerLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dividerLabel2.Location = new System.Drawing.Point(3, 277);
            this.dividerLabel2.Name = "dividerLabel2";
            this.dividerLabel2.Size = new System.Drawing.Size(557, 23);
            this.dividerLabel2.TabIndex = 3;
            this.dividerLabel2.Text = "Namespace Summaries";
            // 
            // lblNamespaceSummaryState
            // 
            this.lblNamespaceSummaryState.AutoSize = true;
            this.lblNamespaceSummaryState.Location = new System.Drawing.Point(18, 313);
            this.lblNamespaceSummaryState.Name = "lblNamespaceSummaryState";
            this.lblNamespaceSummaryState.Size = new System.Drawing.Size(276, 20);
            this.lblNamespaceSummaryState.TabIndex = 4;
            this.lblNamespaceSummaryState.Tag = "NamespaceSummaries";
            this.lblNamespaceSummaryState.Text = "No summaries are defined in the project";
            // 
            // btnEditNamespaces
            // 
            this.btnEditNamespaces.Location = new System.Drawing.Point(343, 307);
            this.btnEditNamespaces.Name = "btnEditNamespaces";
            this.btnEditNamespaces.Size = new System.Drawing.Size(217, 32);
            this.btnEditNamespaces.TabIndex = 5;
            this.btnEditNamespaces.Text = "Edit &Namespace Summaries";
            this.btnEditNamespaces.UseVisualStyleBackColor = true;
            this.btnEditNamespaces.Click += new System.EventHandler(this.btnEditNamespaces_Click);
            // 
            // SummaryPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.btnEditNamespaces);
            this.Controls.Add(this.lblNamespaceSummaryState);
            this.Controls.Add(this.dividerLabel2);
            this.Controls.Add(this.dividerLabel1);
            this.Controls.Add(this.txtProjectSummary);
            this.Controls.Add(this.label2);
            this.MinimumSize = new System.Drawing.Size(580, 355);
            this.Name = "SummaryPropertiesPageControl";
            this.Size = new System.Drawing.Size(580, 355);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtProjectSummary;
        private System.Windows.Forms.Label label2;
        private Utils.Controls.DividerLabel dividerLabel1;
        private Utils.Controls.DividerLabel dividerLabel2;
        private System.Windows.Forms.Label lblNamespaceSummaryState;
        private System.Windows.Forms.Button btnEditNamespaces;


    }
}
