namespace SandcastleBuilder.Package.PropertyPages
{
    partial class BuildEventEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BuildEventEditorForm));
            this.btnOK = new System.Windows.Forms.Button();
            this.txtBuildEvent = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnInsertMacro = new System.Windows.Forms.Button();
            this.dgvMacros = new System.Windows.Forms.DataGridView();
            this.tbcKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbcValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scSplitter = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMacros)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scSplitter)).BeginInit();
            this.scSplitter.Panel1.SuspendLayout();
            this.scSplitter.Panel2.SuspendLayout();
            this.scSplitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(808, 547);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 35);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // txtBuildEvent
            // 
            this.txtBuildEvent.AcceptsReturn = true;
            this.txtBuildEvent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBuildEvent.Location = new System.Drawing.Point(0, 0);
            this.txtBuildEvent.Multiline = true;
            this.txtBuildEvent.Name = "txtBuildEvent";
            this.txtBuildEvent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBuildEvent.Size = new System.Drawing.Size(978, 250);
            this.txtBuildEvent.TabIndex = 0;
            this.txtBuildEvent.WordWrap = false;
            this.txtBuildEvent.Enter += new System.EventHandler(this.txtBuildEvent_Enter);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(902, 547);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnInsertMacro
            // 
            this.btnInsertMacro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInsertMacro.Location = new System.Drawing.Point(12, 547);
            this.btnInsertMacro.Name = "btnInsertMacro";
            this.btnInsertMacro.Size = new System.Drawing.Size(88, 35);
            this.btnInsertMacro.TabIndex = 1;
            this.btnInsertMacro.Text = "&Insert";
            this.btnInsertMacro.UseVisualStyleBackColor = true;
            this.btnInsertMacro.Click += new System.EventHandler(this.btnInsertMacro_Click);
            // 
            // dgvMacros
            // 
            this.dgvMacros.AllowUserToAddRows = false;
            this.dgvMacros.AllowUserToDeleteRows = false;
            this.dgvMacros.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvMacros.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMacros.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.tbcKey,
            this.tbcValue});
            this.dgvMacros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMacros.Location = new System.Drawing.Point(0, 0);
            this.dgvMacros.MultiSelect = false;
            this.dgvMacros.Name = "dgvMacros";
            this.dgvMacros.ReadOnly = true;
            this.dgvMacros.RowHeadersWidth = 25;
            this.dgvMacros.RowTemplate.Height = 24;
            this.dgvMacros.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMacros.Size = new System.Drawing.Size(978, 269);
            this.dgvMacros.TabIndex = 0;
            this.dgvMacros.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMacros_CellDoubleClick);
            // 
            // tbcKey
            // 
            this.tbcKey.DataPropertyName = "Key";
            this.tbcKey.HeaderText = "Macro";
            this.tbcKey.Name = "tbcKey";
            this.tbcKey.ReadOnly = true;
            this.tbcKey.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.tbcKey.Width = 200;
            // 
            // tbcValue
            // 
            this.tbcValue.DataPropertyName = "Value";
            this.tbcValue.HeaderText = "Value";
            this.tbcValue.Name = "tbcValue";
            this.tbcValue.ReadOnly = true;
            this.tbcValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.tbcValue.Width = 500;
            // 
            // scSplitter
            // 
            this.scSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scSplitter.Location = new System.Drawing.Point(12, 12);
            this.scSplitter.Name = "scSplitter";
            this.scSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scSplitter.Panel1
            // 
            this.scSplitter.Panel1.Controls.Add(this.txtBuildEvent);
            // 
            // scSplitter.Panel2
            // 
            this.scSplitter.Panel2.Controls.Add(this.dgvMacros);
            this.scSplitter.Size = new System.Drawing.Size(978, 529);
            this.scSplitter.SplitterDistance = 250;
            this.scSplitter.SplitterWidth = 10;
            this.scSplitter.TabIndex = 0;
            // 
            // BuildEventEditorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1002, 594);
            this.Controls.Add(this.scSplitter);
            this.Controls.Add(this.btnInsertMacro);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "BuildEventEditorForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Build Event Editor";
            ((System.ComponentModel.ISupportInitialize)(this.dgvMacros)).EndInit();
            this.scSplitter.Panel1.ResumeLayout(false);
            this.scSplitter.Panel1.PerformLayout();
            this.scSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scSplitter)).EndInit();
            this.scSplitter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtBuildEvent;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnInsertMacro;
        private System.Windows.Forms.DataGridView dgvMacros;
        private System.Windows.Forms.SplitContainer scSplitter;
        private System.Windows.Forms.DataGridViewTextBoxColumn tbcKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn tbcValue;
    }
}