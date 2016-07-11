namespace SandcastleBuilder.Package.PropertyPages
{
    partial class BuildEventPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BuildEventPropertiesPageControl));
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            this.cboRunPostBuildEvent = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPreBuildEvent = new System.Windows.Forms.TextBox();
            this.txtPostBuildEvent = new System.Windows.Forms.TextBox();
            this.btnEditPreBuildEvent = new System.Windows.Forms.Button();
            this.btnEditPostBuildEvent = new System.Windows.Forms.Button();
            this.lblStandaloneGUI = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            this.SuspendLayout();
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // cboRunPostBuildEvent
            // 
            this.cboRunPostBuildEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRunPostBuildEvent.FormattingEnabled = true;
            this.cboRunPostBuildEvent.Location = new System.Drawing.Point(236, 339);
            this.cboRunPostBuildEvent.MaxDropDownItems = 16;
            this.cboRunPostBuildEvent.Name = "cboRunPostBuildEvent";
            this.cboRunPostBuildEvent.Size = new System.Drawing.Size(215, 33);
            this.cboRunPostBuildEvent.TabIndex = 6;
            this.cboRunPostBuildEvent.Tag = "RunPostBuildEvent";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(6, 343);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(224, 26);
            this.label4.TabIndex = 5;
            this.label4.Text = "Ru&n the post-build event";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(304, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "P&re-build event command line:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 190);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(304, 26);
            this.label2.TabIndex = 3;
            this.label2.Text = "P&ost-build event command line:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPreBuildEvent
            // 
            this.txtPreBuildEvent.AcceptsReturn = true;
            this.txtPreBuildEvent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPreBuildEvent.Location = new System.Drawing.Point(7, 37);
            this.txtPreBuildEvent.Multiline = true;
            this.txtPreBuildEvent.Name = "txtPreBuildEvent";
            this.txtPreBuildEvent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPreBuildEvent.Size = new System.Drawing.Size(775, 113);
            this.txtPreBuildEvent.TabIndex = 1;
            this.txtPreBuildEvent.Tag = "PreBuildEvent";
            this.txtPreBuildEvent.WordWrap = false;
            this.txtPreBuildEvent.Enter += new System.EventHandler(this.txtBuildEvent_Enter);
            // 
            // txtPostBuildEvent
            // 
            this.txtPostBuildEvent.AcceptsReturn = true;
            this.txtPostBuildEvent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPostBuildEvent.Location = new System.Drawing.Point(7, 219);
            this.txtPostBuildEvent.Multiline = true;
            this.txtPostBuildEvent.Name = "txtPostBuildEvent";
            this.txtPostBuildEvent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPostBuildEvent.Size = new System.Drawing.Size(775, 113);
            this.txtPostBuildEvent.TabIndex = 4;
            this.txtPostBuildEvent.Tag = "PostBuildEvent";
            this.txtPostBuildEvent.WordWrap = false;
            this.txtPostBuildEvent.Enter += new System.EventHandler(this.txtBuildEvent_Enter);
            // 
            // btnEditPreBuildEvent
            // 
            this.btnEditPreBuildEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditPreBuildEvent.Location = new System.Drawing.Point(554, 156);
            this.btnEditPreBuildEvent.Name = "btnEditPreBuildEvent";
            this.btnEditPreBuildEvent.Size = new System.Drawing.Size(228, 35);
            this.btnEditPreBuildEvent.TabIndex = 2;
            this.btnEditPreBuildEvent.Text = "Ed&it Pre-build Event";
            this.btnEditPreBuildEvent.UseVisualStyleBackColor = true;
            this.btnEditPreBuildEvent.Click += new System.EventHandler(this.btnEditBuildEvent_Click);
            // 
            // btnEditPostBuildEvent
            // 
            this.btnEditPostBuildEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditPostBuildEvent.Location = new System.Drawing.Point(554, 339);
            this.btnEditPostBuildEvent.Name = "btnEditPostBuildEvent";
            this.btnEditPostBuildEvent.Size = new System.Drawing.Size(228, 35);
            this.btnEditPostBuildEvent.TabIndex = 7;
            this.btnEditPostBuildEvent.Text = "Edit Post-b&uild Event";
            this.btnEditPostBuildEvent.UseVisualStyleBackColor = true;
            this.btnEditPostBuildEvent.Click += new System.EventHandler(this.btnEditBuildEvent_Click);
            // 
            // lblStandaloneGUI
            // 
            this.lblStandaloneGUI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStandaloneGUI.BackColor = System.Drawing.SystemColors.Info;
            this.lblStandaloneGUI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStandaloneGUI.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStandaloneGUI.ForeColor = System.Drawing.SystemColors.InfoText;
            this.lblStandaloneGUI.Location = new System.Drawing.Point(3, 381);
            this.lblStandaloneGUI.Name = "lblStandaloneGUI";
            this.lblStandaloneGUI.Size = new System.Drawing.Size(779, 68);
            this.lblStandaloneGUI.TabIndex = 8;
            this.lblStandaloneGUI.Text = "Pre-build and post-build events will only be executed when the project is built w" +
    "ith MSBuild or Visual Studio.";
            this.lblStandaloneGUI.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStandaloneGUI.Visible = false;
            // 
            // BuildEventPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.lblStandaloneGUI);
            this.Controls.Add(this.btnEditPostBuildEvent);
            this.Controls.Add(this.btnEditPreBuildEvent);
            this.Controls.Add(this.txtPostBuildEvent);
            this.Controls.Add(this.txtPreBuildEvent);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboRunPostBuildEvent);
            this.Controls.Add(this.label4);
            this.MinimumSize = new System.Drawing.Size(785, 453);
            this.Name = "BuildEventPropertiesPageControl";
            this.Size = new System.Drawing.Size(785, 453);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.ComboBox cboRunPostBuildEvent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPostBuildEvent;
        private System.Windows.Forms.TextBox txtPreBuildEvent;
        private System.Windows.Forms.Button btnEditPreBuildEvent;
        private System.Windows.Forms.Button btnEditPostBuildEvent;
        private System.Windows.Forms.Label lblStandaloneGUI;
    }
}
