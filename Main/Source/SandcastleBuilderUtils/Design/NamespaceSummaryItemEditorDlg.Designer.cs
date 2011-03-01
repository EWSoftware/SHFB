namespace SandcastleBuilder.Utils.Design
{
    partial class NamespaceSummaryItemEditorDlg
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
                if(tempProject != null)
                    tempProject.Dispose();

                if(components != null)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NamespaceSummaryItemEditorDlg));
            this.lbNamespaces = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSummary = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnApplyFilter = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnNone = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.lbAppearsIn = new System.Windows.Forms.ListBox();
            this.cboAssembly = new System.Windows.Forms.ComboBox();
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.pbWait = new System.Windows.Forms.PictureBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // lbNamespaces
            // 
            this.lbNamespaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lbNamespaces.Font = new System.Drawing.Font("Tahoma", 8F);
            this.lbNamespaces.HorizontalScrollbar = true;
            this.lbNamespaces.Location = new System.Drawing.Point(12, 102);
            this.lbNamespaces.Name = "lbNamespaces";
            this.lbNamespaces.Size = new System.Drawing.Size(592, 194);
            this.statusBarTextProvider1.SetStatusBarText(this.lbNamespaces, "Namespaces: Check namespaces to include them in the help file.  Uncheck them to e" +
                    "xclude them.");
            this.lbNamespaces.TabIndex = 2;
            this.lbNamespaces.SelectedIndexChanged += new System.EventHandler(this.lbNamespaces_SelectedIndexChanged);
            this.lbNamespaces.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbNamespaces_ItemCheck);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(592, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "&Checked namespaces will appear in the help file.  Unchecked namespaces will not." +
                "";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(12, 313);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(326, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "&Edit the summary for the selected namespace.";
            // 
            // txtSummary
            // 
            this.txtSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSummary.Location = new System.Drawing.Point(12, 339);
            this.txtSummary.Multiline = true;
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSummary.Size = new System.Drawing.Size(858, 156);
            this.statusBarTextProvider1.SetStatusBarText(this.txtSummary, "Summary: Edit the summary information for the namespace");
            this.txtSummary.TabIndex = 7;
            this.txtSummary.Leave += new System.EventHandler(this.txtSummary_Leave);
            this.txtSummary.Enter += new System.EventHandler(this.txtSummary_Enter);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(782, 501);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnClose, "Close: Close this form");
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.toolTip1.SetToolTip(this.btnClose, "Close this form");
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(200, 501);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnDelete, "Delete: Delete an old namespace that is no longer present");
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "&Delete";
            this.toolTip1.SetToolTip(this.btnDelete, "Delete old namespace");
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApplyFilter.Location = new System.Drawing.Point(764, 19);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnApplyFilter, "Apply: Apply the filter to the namespace list");
            this.btnApplyFilter.TabIndex = 4;
            this.btnApplyFilter.Text = "&Apply";
            this.toolTip1.SetToolTip(this.btnApplyFilter, "Apply filter");
            this.btnApplyFilter.UseVisualStyleBackColor = true;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(688, 501);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnHelp, "Help: View help for this form");
            this.btnHelp.TabIndex = 11;
            this.btnHelp.Text = "&Help";
            this.toolTip1.SetToolTip(this.btnHelp, "View help for this form");
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnNone
            // 
            this.btnNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNone.Location = new System.Drawing.Point(106, 501);
            this.btnNone.Name = "btnNone";
            this.btnNone.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnNone, "None: Uncheck all namespaces");
            this.btnNone.TabIndex = 9;
            this.btnNone.Text = "N&one";
            this.toolTip1.SetToolTip(this.btnNone, "Uncheck all namespaces");
            this.btnNone.UseVisualStyleBackColor = true;
            this.btnNone.Click += new System.EventHandler(this.btnNone_Click);
            // 
            // btnAll
            // 
            this.btnAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAll.Location = new System.Drawing.Point(12, 501);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAll, "All: Check all namespaces");
            this.btnAll.TabIndex = 8;
            this.btnAll.Text = "A&ll";
            this.toolTip1.SetToolTip(this.btnAll, "Check all namespaces");
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // lbAppearsIn
            // 
            this.lbAppearsIn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbAppearsIn.FormattingEnabled = true;
            this.lbAppearsIn.HorizontalScrollbar = true;
            this.lbAppearsIn.ItemHeight = 16;
            this.lbAppearsIn.Location = new System.Drawing.Point(610, 102);
            this.lbAppearsIn.Name = "lbAppearsIn";
            this.lbAppearsIn.Size = new System.Drawing.Size(260, 196);
            this.statusBarTextProvider1.SetStatusBarText(this.lbAppearsIn, "Appears In: The selected namespace appears in the listed assemblies");
            this.lbAppearsIn.TabIndex = 5;
            // 
            // cboAssembly
            // 
            this.cboAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAssembly.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAssembly.FormattingEnabled = true;
            this.cboAssembly.Location = new System.Drawing.Point(94, 24);
            this.cboAssembly.Name = "cboAssembly";
            this.cboAssembly.Size = new System.Drawing.Size(351, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.cboAssembly, "Assembly: Select the assembly by which to filter");
            this.cboAssembly.TabIndex = 1;
            // 
            // txtSearchText
            // 
            this.txtSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchText.Location = new System.Drawing.Point(544, 24);
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.Size = new System.Drawing.Size(200, 22);
            this.statusBarTextProvider1.SetStatusBarText(this.txtSearchText, "Name Like: Enter a string or regular expression for which to search");
            this.txtSearchText.TabIndex = 3;
            // 
            // pbWait
            // 
            this.pbWait.BackColor = System.Drawing.SystemColors.Window;
            this.pbWait.Image = global::SandcastleBuilder.Utils.Properties.Resources.SpinningWheel;
            this.pbWait.Location = new System.Drawing.Point(200, 189);
            this.pbWait.Name = "pbWait";
            this.pbWait.Size = new System.Drawing.Size(32, 32);
            this.pbWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbWait.TabIndex = 6;
            this.pbWait.TabStop = false;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.BackColor = System.Drawing.SystemColors.Window;
            this.lblProgress.Location = new System.Drawing.Point(238, 196);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(155, 17);
            this.lblProgress.TabIndex = 3;
            this.lblProgress.Text = "Loading namespaces...";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(610, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(243, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "Selected namespace appears &in:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnApplyFilter);
            this.groupBox1.Controls.Add(this.txtSearchText);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cboAssembly);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(858, 61);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter Namespaces";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(451, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 23);
            this.label5.TabIndex = 2;
            this.label5.Text = "&Name Like";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 23);
            this.label4.TabIndex = 0;
            this.label4.Text = "A&ssembly";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // NamespaceSummaryItemEditorDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(882, 545);
            this.Controls.Add(this.btnAll);
            this.Controls.Add(this.btnNone);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbAppearsIn);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.pbWait);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtSummary);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbNamespaces);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(1600, 1600);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(890, 585);
            this.Name = "NamespaceSummaryItemEditorDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Namespace Summaries";
            this.Load += new System.EventHandler(this.NamespacesDlg_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NamespacesDlg_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox lbNamespaces;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSummary;
        private System.Windows.Forms.Button btnClose;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.PictureBox pbWait;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ListBox lbAppearsIn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnApplyFilter;
        private System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboAssembly;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.Button btnNone;
    }
}
