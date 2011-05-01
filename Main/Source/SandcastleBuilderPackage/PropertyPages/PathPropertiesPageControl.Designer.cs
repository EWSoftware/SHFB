namespace SandcastleBuilder.Package.PropertyPages
{
    partial class PathPropertiesPageControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PathPropertiesPageControl));
            this.dividerLabel1 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.dividerLabel2 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtOutputPath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.txtWorkingPath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.epWarning = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtSandcastlePath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.txtHtmlHelp1xCompilerPath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.txtHtmlHelp2xCompilerPath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // dividerLabel1
            // 
            this.dividerLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dividerLabel1.Location = new System.Drawing.Point(3, 9);
            this.dividerLabel1.Name = "dividerLabel1";
            this.dividerLabel1.Size = new System.Drawing.Size(639, 24);
            this.dividerLabel1.TabIndex = 0;
            this.dividerLabel1.Text = "Tool Paths";
            // 
            // dividerLabel2
            // 
            this.dividerLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dividerLabel2.Location = new System.Drawing.Point(3, 283);
            this.dividerLabel2.Name = "dividerLabel2";
            this.dividerLabel2.Size = new System.Drawing.Size(639, 24);
            this.dividerLabel2.TabIndex = 8;
            this.dividerLabel2.Text = "Output Paths";
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputPath.DefaultFolder = System.Environment.SpecialFolder.Personal;
            this.epNotes.SetError(this.txtOutputPath, "The default is a .\\Help folder relative to the project folder");
            this.epWarning.SetError(this.txtOutputPath, "WARNING: When building a web site, the prior content of the output folder will be" +
        "\nerased without warning before copying the new content to it!");
            this.epWarning.SetIconPadding(this.txtOutputPath, 25);
            this.epNotes.SetIconPadding(this.txtOutputPath, 5);
            this.txtOutputPath.Location = new System.Drawing.Point(224, 310);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.PersistablePath = "Help\\";
            this.txtOutputPath.ShowFixedPathOption = false;
            this.txtOutputPath.ShowNewFolderButton = true;
            this.txtOutputPath.Size = new System.Drawing.Size(366, 31);
            this.txtOutputPath.TabIndex = 10;
            this.txtOutputPath.Tag = "OutputPath";
            this.txtOutputPath.Title = "Select the output location for the help file";
            this.txtOutputPath.PersistablePathChanged += new System.EventHandler(this.txtOutputPath_PersistablePathChanged);
            // 
            // txtWorkingPath
            // 
            this.txtWorkingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWorkingPath.DefaultFolder = System.Environment.SpecialFolder.Personal;
            this.epNotes.SetError(this.txtWorkingPath, "The default is a .\\Working folder under the Output Path folder");
            this.epWarning.SetError(this.txtWorkingPath, "WARNING: All files and folders in this path will be erased without warning\nwhen t" +
        "he build starts!");
            this.epWarning.SetIconPadding(this.txtWorkingPath, 25);
            this.epNotes.SetIconPadding(this.txtWorkingPath, 5);
            this.txtWorkingPath.Location = new System.Drawing.Point(224, 347);
            this.txtWorkingPath.Name = "txtWorkingPath";
            this.txtWorkingPath.ShowNewFolderButton = true;
            this.txtWorkingPath.Size = new System.Drawing.Size(366, 59);
            this.txtWorkingPath.TabIndex = 12;
            this.txtWorkingPath.Tag = "WorkingPath";
            this.txtWorkingPath.Title = "Select the working files location";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(48, 78);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(170, 23);
            this.label9.TabIndex = 2;
            this.label9.Text = "Sa&ndcastle tools folder";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "HTML Help 1 compiler folder";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(29, 208);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(189, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "MS Help 2 compiler folder";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(33, 310);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(185, 23);
            this.label3.TabIndex = 9;
            this.label3.Text = "H&elp content output path";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(73, 347);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(145, 23);
            this.label4.TabIndex = 11;
            this.label4.Text = "Working files path";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(634, 23);
            this.label5.TabIndex = 1;
            this.label5.Text = "If not specified, the help file builder will search for the tools in their well-k" +
    "nown locations.";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // epWarning
            // 
            this.epWarning.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epWarning.ContainerControl = this;
            this.epWarning.Icon = ((System.Drawing.Icon)(resources.GetObject("epWarning.Icon")));
            // 
            // txtSandcastlePath
            // 
            this.txtSandcastlePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSandcastlePath.DefaultFolder = System.Environment.SpecialFolder.ProgramFiles;
            this.txtSandcastlePath.Location = new System.Drawing.Point(224, 78);
            this.txtSandcastlePath.Name = "txtSandcastlePath";
            this.txtSandcastlePath.Size = new System.Drawing.Size(366, 59);
            this.txtSandcastlePath.TabIndex = 3;
            this.txtSandcastlePath.Tag = "SandcastlePath";
            this.txtSandcastlePath.Title = "Select the Sandcastle installation location";
            // 
            // txtHtmlHelp1xCompilerPath
            // 
            this.txtHtmlHelp1xCompilerPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHtmlHelp1xCompilerPath.DefaultFolder = System.Environment.SpecialFolder.ProgramFiles;
            this.txtHtmlHelp1xCompilerPath.Location = new System.Drawing.Point(224, 143);
            this.txtHtmlHelp1xCompilerPath.Name = "txtHtmlHelp1xCompilerPath";
            this.txtHtmlHelp1xCompilerPath.Size = new System.Drawing.Size(366, 59);
            this.txtHtmlHelp1xCompilerPath.TabIndex = 5;
            this.txtHtmlHelp1xCompilerPath.Tag = "HtmlHelp1xCompilerPath";
            this.txtHtmlHelp1xCompilerPath.Title = "Select the HTML Help 1 compiler installation location";
            // 
            // txtHtmlHelp2xCompilerPath
            // 
            this.txtHtmlHelp2xCompilerPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHtmlHelp2xCompilerPath.DefaultFolder = System.Environment.SpecialFolder.ProgramFiles;
            this.txtHtmlHelp2xCompilerPath.Location = new System.Drawing.Point(224, 208);
            this.txtHtmlHelp2xCompilerPath.Name = "txtHtmlHelp2xCompilerPath";
            this.txtHtmlHelp2xCompilerPath.Size = new System.Drawing.Size(366, 59);
            this.txtHtmlHelp2xCompilerPath.TabIndex = 7;
            this.txtHtmlHelp2xCompilerPath.Tag = "HtmlHelp2xCompilerPath";
            this.txtHtmlHelp2xCompilerPath.Title = "Select the MS Help 2 compiler installation location";
            // 
            // PathPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.txtWorkingPath);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.txtHtmlHelp2xCompilerPath);
            this.Controls.Add(this.txtHtmlHelp1xCompilerPath);
            this.Controls.Add(this.txtSandcastlePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.dividerLabel2);
            this.Controls.Add(this.dividerLabel1);
            this.MinimumSize = new System.Drawing.Size(645, 420);
            this.Name = "PathPropertiesPageControl";
            this.Size = new System.Drawing.Size(645, 420);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epWarning)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Utils.Controls.DividerLabel dividerLabel1;
        private Utils.Controls.DividerLabel dividerLabel2;
        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ErrorProvider epWarning;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private Utils.Controls.FolderPathUserControl txtWorkingPath;
        private Utils.Controls.FolderPathUserControl txtOutputPath;
        private Utils.Controls.FolderPathUserControl txtHtmlHelp2xCompilerPath;
        private Utils.Controls.FolderPathUserControl txtHtmlHelp1xCompilerPath;
        private Utils.Controls.FolderPathUserControl txtSandcastlePath;


    }
}
