namespace SandcastleBuilder.Package.PropertyPages
{
    partial class MissingTagPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MissingTagPropertiesPageControl));
            this.chkAutoDocumentConstructors = new System.Windows.Forms.CheckBox();
            this.chkAutoDocumentDisposeMethods = new System.Windows.Forms.CheckBox();
            this.chkShowMissingIncludeTargets = new System.Windows.Forms.CheckBox();
            this.chkShowMissingNamespaces = new System.Windows.Forms.CheckBox();
            this.chkShowMissingParams = new System.Windows.Forms.CheckBox();
            this.chkShowMissingRemarks = new System.Windows.Forms.CheckBox();
            this.chkShowMissingReturns = new System.Windows.Forms.CheckBox();
            this.chkShowMissingSummaries = new System.Windows.Forms.CheckBox();
            this.chkShowMissingTypeParams = new System.Windows.Forms.CheckBox();
            this.chkShowMissingValues = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            this.SuspendLayout();
            // 
            // chkAutoDocumentConstructors
            // 
            this.chkAutoDocumentConstructors.AutoSize = true;
            this.chkAutoDocumentConstructors.Checked = true;
            this.chkAutoDocumentConstructors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoDocumentConstructors.Location = new System.Drawing.Point(60, 54);
            this.chkAutoDocumentConstructors.Name = "chkAutoDocumentConstructors";
            this.chkAutoDocumentConstructors.Size = new System.Drawing.Size(113, 24);
            this.chkAutoDocumentConstructors.TabIndex = 1;
            this.chkAutoDocumentConstructors.Tag = "MissingTags";
            this.chkAutoDocumentConstructors.Text = "&Constructors";
            this.chkAutoDocumentConstructors.UseVisualStyleBackColor = true;
            // 
            // chkAutoDocumentDisposeMethods
            // 
            this.chkAutoDocumentDisposeMethods.AutoSize = true;
            this.chkAutoDocumentDisposeMethods.Checked = true;
            this.chkAutoDocumentDisposeMethods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoDocumentDisposeMethods.Location = new System.Drawing.Point(297, 54);
            this.chkAutoDocumentDisposeMethods.Name = "chkAutoDocumentDisposeMethods";
            this.chkAutoDocumentDisposeMethods.Size = new System.Drawing.Size(150, 24);
            this.chkAutoDocumentDisposeMethods.TabIndex = 2;
            this.chkAutoDocumentDisposeMethods.Text = "Dispose methods ";
            this.chkAutoDocumentDisposeMethods.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingIncludeTargets
            // 
            this.chkShowMissingIncludeTargets.AutoSize = true;
            this.epNotes.SetError(this.chkShowMissingIncludeTargets, "This option only has effect with C# generated XML comments files");
            this.epNotes.SetIconPadding(this.chkShowMissingIncludeTargets, 5);
            this.chkShowMissingIncludeTargets.Location = new System.Drawing.Point(297, 254);
            this.chkShowMissingIncludeTargets.Name = "chkShowMissingIncludeTargets";
            this.chkShowMissingIncludeTargets.Size = new System.Drawing.Size(207, 24);
            this.chkShowMissingIncludeTargets.TabIndex = 11;
            this.chkShowMissingIncludeTargets.Text = "<include> element targets";
            this.chkShowMissingIncludeTargets.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingNamespaces
            // 
            this.chkShowMissingNamespaces.AutoSize = true;
            this.chkShowMissingNamespaces.Checked = true;
            this.chkShowMissingNamespaces.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMissingNamespaces.Location = new System.Drawing.Point(60, 164);
            this.chkShowMissingNamespaces.Name = "chkShowMissingNamespaces";
            this.chkShowMissingNamespaces.Size = new System.Drawing.Size(214, 24);
            this.chkShowMissingNamespaces.TabIndex = 4;
            this.chkShowMissingNamespaces.Text = "&Namespace documentation";
            this.chkShowMissingNamespaces.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingParams
            // 
            this.chkShowMissingParams.AutoSize = true;
            this.chkShowMissingParams.Checked = true;
            this.chkShowMissingParams.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMissingParams.Location = new System.Drawing.Point(60, 224);
            this.chkShowMissingParams.Name = "chkShowMissingParams";
            this.chkShowMissingParams.Size = new System.Drawing.Size(158, 24);
            this.chkShowMissingParams.TabIndex = 6;
            this.chkShowMissingParams.Text = "<param> elements";
            this.chkShowMissingParams.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingRemarks
            // 
            this.chkShowMissingRemarks.AutoSize = true;
            this.chkShowMissingRemarks.Location = new System.Drawing.Point(297, 224);
            this.chkShowMissingRemarks.Name = "chkShowMissingRemarks";
            this.chkShowMissingRemarks.Size = new System.Drawing.Size(167, 24);
            this.chkShowMissingRemarks.TabIndex = 10;
            this.chkShowMissingRemarks.Text = "<remarks> elements";
            this.chkShowMissingRemarks.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingReturns
            // 
            this.chkShowMissingReturns.AutoSize = true;
            this.chkShowMissingReturns.Checked = true;
            this.chkShowMissingReturns.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMissingReturns.Location = new System.Drawing.Point(297, 164);
            this.chkShowMissingReturns.Name = "chkShowMissingReturns";
            this.chkShowMissingReturns.Size = new System.Drawing.Size(160, 24);
            this.chkShowMissingReturns.TabIndex = 8;
            this.chkShowMissingReturns.Text = "<returns> elements";
            this.chkShowMissingReturns.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingSummaries
            // 
            this.chkShowMissingSummaries.AutoSize = true;
            this.chkShowMissingSummaries.Checked = true;
            this.chkShowMissingSummaries.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMissingSummaries.Location = new System.Drawing.Point(60, 194);
            this.chkShowMissingSummaries.Name = "chkShowMissingSummaries";
            this.chkShowMissingSummaries.Size = new System.Drawing.Size(175, 24);
            this.chkShowMissingSummaries.TabIndex = 5;
            this.chkShowMissingSummaries.Text = "<summary> elements";
            this.chkShowMissingSummaries.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingTypeParams
            // 
            this.chkShowMissingTypeParams.AutoSize = true;
            this.chkShowMissingTypeParams.Checked = true;
            this.chkShowMissingTypeParams.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMissingTypeParams.Location = new System.Drawing.Point(60, 254);
            this.chkShowMissingTypeParams.Name = "chkShowMissingTypeParams";
            this.chkShowMissingTypeParams.Size = new System.Drawing.Size(187, 24);
            this.chkShowMissingTypeParams.TabIndex = 7;
            this.chkShowMissingTypeParams.Text = "<typeparam> elements";
            this.chkShowMissingTypeParams.UseVisualStyleBackColor = true;
            // 
            // chkShowMissingValues
            // 
            this.chkShowMissingValues.AutoSize = true;
            this.chkShowMissingValues.Location = new System.Drawing.Point(297, 194);
            this.chkShowMissingValues.Name = "chkShowMissingValues";
            this.chkShowMissingValues.Size = new System.Drawing.Size(150, 24);
            this.chkShowMissingValues.TabIndex = 9;
            this.chkShowMissingValues.Text = "<value> elements";
            this.chkShowMissingValues.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(20, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(517, 43);
            this.label1.TabIndex = 3;
            this.label1.Text = "Include a \"missing documentation\" warning for each of the following XML comment e" +
    "lements if they are not present on the appropriate class members:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(540, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Auto-document the following class members if they are missing XML comments:";
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // MissingTagPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkShowMissingValues);
            this.Controls.Add(this.chkShowMissingTypeParams);
            this.Controls.Add(this.chkShowMissingSummaries);
            this.Controls.Add(this.chkShowMissingReturns);
            this.Controls.Add(this.chkShowMissingRemarks);
            this.Controls.Add(this.chkShowMissingParams);
            this.Controls.Add(this.chkShowMissingNamespaces);
            this.Controls.Add(this.chkShowMissingIncludeTargets);
            this.Controls.Add(this.chkAutoDocumentDisposeMethods);
            this.Controls.Add(this.chkAutoDocumentConstructors);
            this.MinimumSize = new System.Drawing.Size(600, 295);
            this.Name = "MissingTagPropertiesPageControl";
            this.Size = new System.Drawing.Size(600, 295);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAutoDocumentConstructors;
        private System.Windows.Forms.CheckBox chkAutoDocumentDisposeMethods;
        private System.Windows.Forms.CheckBox chkShowMissingIncludeTargets;
        private System.Windows.Forms.CheckBox chkShowMissingNamespaces;
        private System.Windows.Forms.CheckBox chkShowMissingParams;
        private System.Windows.Forms.CheckBox chkShowMissingRemarks;
        private System.Windows.Forms.CheckBox chkShowMissingReturns;
        private System.Windows.Forms.CheckBox chkShowMissingSummaries;
        private System.Windows.Forms.CheckBox chkShowMissingTypeParams;
        private System.Windows.Forms.CheckBox chkShowMissingValues;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ErrorProvider epNotes;


    }
}
