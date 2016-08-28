namespace SandcastleBuilder.Package.PropertyPages
{
    partial class TransformArgumentsPageControl
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
                if(componentCache != null)
                {
                    componentCache.ComponentContainerLoaded -= componentCache_ComponentContainerLoaded;
                    componentCache.ComponentContainerLoadFailed -= componentCache_ComponentContainerLoadFailed;
                    componentCache.ComponentContainerReset -= componentCache_ComponentContainerReset;
                }

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
            this.tvArguments = new System.Windows.Forms.TreeView();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.chkIsForConceptualBuild = new System.Windows.Forms.CheckBox();
            this.chkIsForReferenceBuild = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tvArguments
            // 
            this.tvArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvArguments.FullRowSelect = true;
            this.tvArguments.HideSelection = false;
            this.tvArguments.Location = new System.Drawing.Point(3, 3);
            this.tvArguments.Name = "tvArguments";
            this.tvArguments.ShowLines = false;
            this.tvArguments.ShowPlusMinus = false;
            this.tvArguments.ShowRootLines = false;
            this.tvArguments.Size = new System.Drawing.Size(260, 449);
            this.tvArguments.TabIndex = 0;
            this.tvArguments.Tag = "TransformComponentArguments";
            this.tvArguments.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvArguments_BeforeSelect);
            this.tvArguments.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvArguments_AfterSelect);
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(269, 3);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(528, 283);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.TabStop = false;
            // 
            // chkIsForConceptualBuild
            // 
            this.chkIsForConceptualBuild.AutoSize = true;
            this.chkIsForConceptualBuild.Enabled = false;
            this.chkIsForConceptualBuild.Location = new System.Drawing.Point(275, 292);
            this.chkIsForConceptualBuild.Name = "chkIsForConceptualBuild";
            this.chkIsForConceptualBuild.Size = new System.Drawing.Size(22, 21);
            this.chkIsForConceptualBuild.TabIndex = 2;
            this.chkIsForConceptualBuild.UseVisualStyleBackColor = true;
            // 
            // chkIsForReferenceBuild
            // 
            this.chkIsForReferenceBuild.AutoSize = true;
            this.chkIsForReferenceBuild.Enabled = false;
            this.chkIsForReferenceBuild.Location = new System.Drawing.Point(542, 292);
            this.chkIsForReferenceBuild.Name = "chkIsForReferenceBuild";
            this.chkIsForReferenceBuild.Size = new System.Drawing.Size(22, 21);
            this.chkIsForReferenceBuild.TabIndex = 4;
            this.chkIsForReferenceBuild.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(303, 289);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(207, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Used in conceptual build";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(570, 289);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(193, 25);
            this.label2.TabIndex = 5;
            this.label2.Text = "Used in reference build";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(270, 326);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 25);
            this.label3.TabIndex = 6;
            this.label3.Text = "Value";
            // 
            // txtValue
            // 
            this.txtValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtValue.Location = new System.Drawing.Point(269, 355);
            this.txtValue.Multiline = true;
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(523, 97);
            this.txtValue.TabIndex = 7;
            this.txtValue.Tag = "";
            this.txtValue.TextChanged += new System.EventHandler(this.txtValue_TextChanged);
            // 
            // TransformArgumentsPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkIsForReferenceBuild);
            this.Controls.Add(this.chkIsForConceptualBuild);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.tvArguments);
            this.MinimumSize = new System.Drawing.Size(800, 455);
            this.Name = "TransformArgumentsPageControl";
            this.Size = new System.Drawing.Size(800, 455);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvArguments;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.CheckBox chkIsForConceptualBuild;
        private System.Windows.Forms.CheckBox chkIsForReferenceBuild;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtValue;



    }
}
