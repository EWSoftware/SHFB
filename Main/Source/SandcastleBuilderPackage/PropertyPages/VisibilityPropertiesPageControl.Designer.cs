namespace SandcastleBuilder.Package.PropertyPages
{
    partial class VisibilityPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisibilityPropertiesPageControl));
            this.label1 = new System.Windows.Forms.Label();
            this.chkAttributes = new System.Windows.Forms.CheckBox();
            this.chkExplicitInterfaceImplementations = new System.Windows.Forms.CheckBox();
            this.chkInheritedFrameworkInternalMembers = new System.Windows.Forms.CheckBox();
            this.chkInheritedFrameworkMembers = new System.Windows.Forms.CheckBox();
            this.chkInheritedFrameworkPrivateMembers = new System.Windows.Forms.CheckBox();
            this.chkInheritedMembers = new System.Windows.Forms.CheckBox();
            this.chkInternals = new System.Windows.Forms.CheckBox();
            this.chkPrivateFields = new System.Windows.Forms.CheckBox();
            this.chkPrivates = new System.Windows.Forms.CheckBox();
            this.chkProtected = new System.Windows.Forms.CheckBox();
            this.chkSealedProtected = new System.Windows.Forms.CheckBox();
            this.chkProtectedInternalAsProtected = new System.Windows.Forms.CheckBox();
            this.lblAPIFilterState = new System.Windows.Forms.Label();
            this.btnEditAPIFilter = new System.Windows.Forms.Button();
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(388, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Include the following API elements in the documentation:";
            // 
            // chkAttributes
            // 
            this.chkAttributes.AutoSize = true;
            this.chkAttributes.Location = new System.Drawing.Point(44, 48);
            this.chkAttributes.Name = "chkAttributes";
            this.chkAttributes.Size = new System.Drawing.Size(285, 24);
            this.chkAttributes.TabIndex = 1;
            this.chkAttributes.Tag = "VisibleItems";
            this.chkAttributes.Text = "Attr&ibutes on types and their members";
            this.chkAttributes.UseVisualStyleBackColor = true;
            // 
            // chkExplicitInterfaceImplementations
            // 
            this.chkExplicitInterfaceImplementations.AutoSize = true;
            this.chkExplicitInterfaceImplementations.Location = new System.Drawing.Point(44, 78);
            this.chkExplicitInterfaceImplementations.Name = "chkExplicitInterfaceImplementations";
            this.chkExplicitInterfaceImplementations.Size = new System.Drawing.Size(257, 24);
            this.chkExplicitInterfaceImplementations.TabIndex = 2;
            this.chkExplicitInterfaceImplementations.Text = "Explicit interface implementations";
            this.chkExplicitInterfaceImplementations.UseVisualStyleBackColor = true;
            // 
            // chkInheritedFrameworkInternalMembers
            // 
            this.chkInheritedFrameworkInternalMembers.AutoSize = true;
            this.chkInheritedFrameworkInternalMembers.Location = new System.Drawing.Point(44, 168);
            this.chkInheritedFrameworkInternalMembers.Name = "chkInheritedFrameworkInternalMembers";
            this.chkInheritedFrameworkInternalMembers.Size = new System.Drawing.Size(321, 24);
            this.chkInheritedFrameworkInternalMembers.TabIndex = 5;
            this.chkInheritedFrameworkInternalMembers.Text = "Inherited .NET Framework internal members";
            this.chkInheritedFrameworkInternalMembers.UseVisualStyleBackColor = true;
            this.chkInheritedFrameworkInternalMembers.CheckedChanged += new System.EventHandler(this.chkInheritedFrameworkInternalMembers_CheckedChanged);
            // 
            // chkInheritedFrameworkMembers
            // 
            this.chkInheritedFrameworkMembers.AutoSize = true;
            this.chkInheritedFrameworkMembers.Checked = true;
            this.chkInheritedFrameworkMembers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInheritedFrameworkMembers.Location = new System.Drawing.Point(44, 138);
            this.chkInheritedFrameworkMembers.Name = "chkInheritedFrameworkMembers";
            this.chkInheritedFrameworkMembers.Size = new System.Drawing.Size(267, 24);
            this.chkInheritedFrameworkMembers.TabIndex = 4;
            this.chkInheritedFrameworkMembers.Text = "Inherited .NET Framework members";
            this.chkInheritedFrameworkMembers.UseVisualStyleBackColor = true;
            this.chkInheritedFrameworkMembers.CheckedChanged += new System.EventHandler(this.chkInheritedFrameworkMembers_CheckedChanged);
            // 
            // chkInheritedFrameworkPrivateMembers
            // 
            this.chkInheritedFrameworkPrivateMembers.AutoSize = true;
            this.chkInheritedFrameworkPrivateMembers.Location = new System.Drawing.Point(44, 198);
            this.chkInheritedFrameworkPrivateMembers.Name = "chkInheritedFrameworkPrivateMembers";
            this.chkInheritedFrameworkPrivateMembers.Size = new System.Drawing.Size(317, 24);
            this.chkInheritedFrameworkPrivateMembers.TabIndex = 6;
            this.chkInheritedFrameworkPrivateMembers.Text = "Inherited .NET Framework private members";
            this.chkInheritedFrameworkPrivateMembers.UseVisualStyleBackColor = true;
            this.chkInheritedFrameworkPrivateMembers.CheckedChanged += new System.EventHandler(this.chkInheritedFrameworkPrivateMembers_CheckedChanged);
            // 
            // chkInheritedMembers
            // 
            this.chkInheritedMembers.AutoSize = true;
            this.chkInheritedMembers.Checked = true;
            this.chkInheritedMembers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInheritedMembers.Location = new System.Drawing.Point(44, 108);
            this.chkInheritedMembers.Name = "chkInheritedMembers";
            this.chkInheritedMembers.Size = new System.Drawing.Size(226, 24);
            this.chkInheritedMembers.TabIndex = 3;
            this.chkInheritedMembers.Text = "Inherited base class members";
            this.chkInheritedMembers.UseVisualStyleBackColor = true;
            this.chkInheritedMembers.CheckedChanged += new System.EventHandler(this.chkInheritedMembers_CheckedChanged);
            // 
            // chkInternals
            // 
            this.chkInternals.AutoSize = true;
            this.chkInternals.Location = new System.Drawing.Point(44, 228);
            this.chkInternals.Name = "chkInternals";
            this.chkInternals.Size = new System.Drawing.Size(147, 24);
            this.chkInternals.TabIndex = 7;
            this.chkInternals.Text = "Internal members";
            this.chkInternals.UseVisualStyleBackColor = true;
            this.chkInternals.CheckedChanged += new System.EventHandler(this.chkInternals_CheckedChanged);
            // 
            // chkPrivateFields
            // 
            this.chkPrivateFields.AutoSize = true;
            this.chkPrivateFields.Location = new System.Drawing.Point(44, 258);
            this.chkPrivateFields.Name = "chkPrivateFields";
            this.chkPrivateFields.Size = new System.Drawing.Size(116, 24);
            this.chkPrivateFields.TabIndex = 8;
            this.chkPrivateFields.Text = "Private fields";
            this.chkPrivateFields.UseVisualStyleBackColor = true;
            this.chkPrivateFields.CheckedChanged += new System.EventHandler(this.chkPrivateFields_CheckedChanged);
            // 
            // chkPrivates
            // 
            this.chkPrivates.AutoSize = true;
            this.chkPrivates.Location = new System.Drawing.Point(44, 288);
            this.chkPrivates.Name = "chkPrivates";
            this.chkPrivates.Size = new System.Drawing.Size(142, 24);
            this.chkPrivates.TabIndex = 9;
            this.chkPrivates.Text = "Private members";
            this.chkPrivates.UseVisualStyleBackColor = true;
            this.chkPrivates.CheckedChanged += new System.EventHandler(this.chkPrivates_CheckedChanged);
            // 
            // chkProtected
            // 
            this.chkProtected.AutoSize = true;
            this.chkProtected.Checked = true;
            this.chkProtected.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProtected.Location = new System.Drawing.Point(44, 318);
            this.chkProtected.Name = "chkProtected";
            this.chkProtected.Size = new System.Drawing.Size(161, 24);
            this.chkProtected.TabIndex = 10;
            this.chkProtected.Text = "Protected members";
            this.chkProtected.UseVisualStyleBackColor = true;
            this.chkProtected.CheckedChanged += new System.EventHandler(this.chkProtected_CheckedChanged);
            // 
            // chkSealedProtected
            // 
            this.chkSealedProtected.AutoSize = true;
            this.chkSealedProtected.Checked = true;
            this.chkSealedProtected.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSealedProtected.Location = new System.Drawing.Point(44, 348);
            this.chkSealedProtected.Name = "chkSealedProtected";
            this.chkSealedProtected.Size = new System.Drawing.Size(275, 24);
            this.chkSealedProtected.TabIndex = 11;
            this.chkSealedProtected.Text = "Protected members of sealed classes";
            this.chkSealedProtected.UseVisualStyleBackColor = true;
            this.chkSealedProtected.CheckedChanged += new System.EventHandler(this.chkSealedProtected_CheckedChanged);
            // 
            // chkProtectedInternalAsProtected
            // 
            this.chkProtectedInternalAsProtected.AutoSize = true;
            this.epNotes.SetError(this.chkProtectedInternalAsProtected, "This option is ignored if the Protected Members option is turned off");
            this.epNotes.SetIconPadding(this.chkProtectedInternalAsProtected, 5);
            this.chkProtectedInternalAsProtected.Location = new System.Drawing.Point(13, 398);
            this.chkProtectedInternalAsProtected.Name = "chkProtectedInternalAsProtected";
            this.chkProtectedInternalAsProtected.Size = new System.Drawing.Size(452, 24);
            this.chkProtectedInternalAsProtected.TabIndex = 12;
            this.chkProtectedInternalAsProtected.Text = "Doc&ument \"protected internal\" members as \"protected\" instead";
            this.chkProtectedInternalAsProtected.UseVisualStyleBackColor = true;
            // 
            // lblAPIFilterState
            // 
            this.lblAPIFilterState.AutoSize = true;
            this.lblAPIFilterState.Location = new System.Drawing.Point(13, 451);
            this.lblAPIFilterState.Name = "lblAPIFilterState";
            this.lblAPIFilterState.Size = new System.Drawing.Size(232, 20);
            this.lblAPIFilterState.TabIndex = 13;
            this.lblAPIFilterState.Tag = "ApiFilter";
            this.lblAPIFilterState.Text = "An API filter has not been defined";
            // 
            // btnEditAPIFilter
            // 
            this.btnEditAPIFilter.Location = new System.Drawing.Point(262, 445);
            this.btnEditAPIFilter.Name = "btnEditAPIFilter";
            this.btnEditAPIFilter.Size = new System.Drawing.Size(135, 32);
            this.btnEditAPIFilter.TabIndex = 14;
            this.btnEditAPIFilter.Text = "Edit API &Filter";
            this.btnEditAPIFilter.UseVisualStyleBackColor = true;
            this.btnEditAPIFilter.Click += new System.EventHandler(this.btnEditAPIFilter_Click);
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // VisibilityPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.btnEditAPIFilter);
            this.Controls.Add(this.lblAPIFilterState);
            this.Controls.Add(this.chkProtectedInternalAsProtected);
            this.Controls.Add(this.chkSealedProtected);
            this.Controls.Add(this.chkProtected);
            this.Controls.Add(this.chkPrivates);
            this.Controls.Add(this.chkPrivateFields);
            this.Controls.Add(this.chkInternals);
            this.Controls.Add(this.chkInheritedMembers);
            this.Controls.Add(this.chkInheritedFrameworkPrivateMembers);
            this.Controls.Add(this.chkInheritedFrameworkMembers);
            this.Controls.Add(this.chkInheritedFrameworkInternalMembers);
            this.Controls.Add(this.chkExplicitInterfaceImplementations);
            this.Controls.Add(this.chkAttributes);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(500, 495);
            this.Name = "VisibilityPropertiesPageControl";
            this.Size = new System.Drawing.Size(500, 495);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkAttributes;
        private System.Windows.Forms.CheckBox chkExplicitInterfaceImplementations;
        private System.Windows.Forms.CheckBox chkInheritedFrameworkInternalMembers;
        private System.Windows.Forms.CheckBox chkInheritedFrameworkMembers;
        private System.Windows.Forms.CheckBox chkInheritedFrameworkPrivateMembers;
        private System.Windows.Forms.CheckBox chkInheritedMembers;
        private System.Windows.Forms.CheckBox chkInternals;
        private System.Windows.Forms.CheckBox chkPrivateFields;
        private System.Windows.Forms.CheckBox chkPrivates;
        private System.Windows.Forms.CheckBox chkProtected;
        private System.Windows.Forms.CheckBox chkSealedProtected;
        private System.Windows.Forms.CheckBox chkProtectedInternalAsProtected;
        private System.Windows.Forms.Label lblAPIFilterState;
        private System.Windows.Forms.Button btnEditAPIFilter;
        private System.Windows.Forms.ErrorProvider epNotes;


    }
}
