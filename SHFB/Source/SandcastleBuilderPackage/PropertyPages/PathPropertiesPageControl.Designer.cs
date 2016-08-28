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
            this.txtComponentPath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.txtSourceCodeBasePath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.epWarning = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtHtmlHelp1xCompilerPath = new SandcastleBuilder.Utils.Controls.FolderPathUserControl();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dividerLabel3 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.chkWarnOnMissingSourceContext = new System.Windows.Forms.CheckBox();
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
            this.dividerLabel1.Size = new System.Drawing.Size(779, 26);
            this.dividerLabel1.TabIndex = 0;
            this.dividerLabel1.Text = "Tool Paths";
            // 
            // dividerLabel2
            // 
            this.dividerLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dividerLabel2.Location = new System.Drawing.Point(3, 470);
            this.dividerLabel2.Name = "dividerLabel2";
            this.dividerLabel2.Size = new System.Drawing.Size(779, 26);
            this.dividerLabel2.TabIndex = 10;
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
            this.txtOutputPath.DefaultFolder = System.Environment.SpecialFolder.MyDocuments;
            this.epWarning.SetError(this.txtOutputPath, "WARNING: When building a web site or markdown output, the prior content of\nthe ou" +
        "tput folder will be erased without warning before copying the new\ncontent to it!" +
        "");
            this.epNotes.SetError(this.txtOutputPath, "The default is a .\\Help folder relative to the project folder");
            this.epNotes.SetIconPadding(this.txtOutputPath, 5);
            this.epWarning.SetIconPadding(this.txtOutputPath, 25);
            this.txtOutputPath.Location = new System.Drawing.Point(341, 506);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.PersistablePath = "Help\\";
            this.txtOutputPath.ShowFixedPathOption = false;
            this.txtOutputPath.ShowNewFolderButton = true;
            this.txtOutputPath.Size = new System.Drawing.Size(389, 35);
            this.txtOutputPath.TabIndex = 12;
            this.txtOutputPath.Tag = "OutputPath";
            this.txtOutputPath.Title = "Select the output location for the help file";
            this.txtOutputPath.PersistablePathChanged += new System.EventHandler(this.txtOutputPath_PersistablePathChanged);
            // 
            // txtWorkingPath
            // 
            this.txtWorkingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWorkingPath.DefaultFolder = System.Environment.SpecialFolder.MyDocuments;
            this.epWarning.SetError(this.txtWorkingPath, "WARNING: All files and folders in this path will be erased without warning\nwhen t" +
        "he build starts!");
            this.epNotes.SetError(this.txtWorkingPath, "The default is a .\\Working folder under the Output Path folder");
            this.epNotes.SetIconPadding(this.txtWorkingPath, 5);
            this.epWarning.SetIconPadding(this.txtWorkingPath, 25);
            this.txtWorkingPath.Location = new System.Drawing.Point(341, 553);
            this.txtWorkingPath.Name = "txtWorkingPath";
            this.txtWorkingPath.ShowNewFolderButton = true;
            this.txtWorkingPath.Size = new System.Drawing.Size(389, 67);
            this.txtWorkingPath.TabIndex = 14;
            this.txtWorkingPath.Tag = "WorkingPath";
            this.txtWorkingPath.Title = "Select the working files location";
            // 
            // txtComponentPath
            // 
            this.txtComponentPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtComponentPath.DefaultFolder = System.Environment.SpecialFolder.MyDocuments;
            this.epNotes.SetError(this.txtComponentPath, "Use this to find project-specific build components.\nIf blank, the project folder " +
        "is searched.");
            this.epNotes.SetIconPadding(this.txtComponentPath, 5);
            this.txtComponentPath.Location = new System.Drawing.Point(341, 165);
            this.txtComponentPath.Name = "txtComponentPath";
            this.txtComponentPath.Size = new System.Drawing.Size(389, 67);
            this.txtComponentPath.TabIndex = 5;
            this.txtComponentPath.Tag = "ComponentPath";
            this.txtComponentPath.Title = "Select the folder containing project-specific build components";
            // 
            // txtSourceCodeBasePath
            // 
            this.txtSourceCodeBasePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceCodeBasePath.DefaultFolder = System.Environment.SpecialFolder.MyDocuments;
            this.epNotes.SetIconPadding(this.txtSourceCodeBasePath, 5);
            this.txtSourceCodeBasePath.Location = new System.Drawing.Point(220, 349);
            this.txtSourceCodeBasePath.Name = "txtSourceCodeBasePath";
            this.txtSourceCodeBasePath.Size = new System.Drawing.Size(510, 67);
            this.txtSourceCodeBasePath.TabIndex = 8;
            this.txtSourceCodeBasePath.Tag = "SourceCodeBasePath";
            this.txtSourceCodeBasePath.Title = "Select the base folder containing source code for the documented assemblies";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(41, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(294, 26);
            this.label1.TabIndex = 2;
            this.label1.Text = "HTML Help 1 compiler path";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 506);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(327, 23);
            this.label3.TabIndex = 11;
            this.label3.Text = "H&elp content output path";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(95, 553);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(240, 26);
            this.label4.TabIndex = 13;
            this.label4.Text = "Working files path";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(771, 26);
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
            // txtHtmlHelp1xCompilerPath
            // 
            this.txtHtmlHelp1xCompilerPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHtmlHelp1xCompilerPath.DefaultFolder = System.Environment.SpecialFolder.ProgramFiles;
            this.txtHtmlHelp1xCompilerPath.Location = new System.Drawing.Point(341, 81);
            this.txtHtmlHelp1xCompilerPath.Name = "txtHtmlHelp1xCompilerPath";
            this.txtHtmlHelp1xCompilerPath.Size = new System.Drawing.Size(389, 67);
            this.txtHtmlHelp1xCompilerPath.TabIndex = 3;
            this.txtHtmlHelp1xCompilerPath.Tag = "HtmlHelp1xCompilerPath";
            this.txtHtmlHelp1xCompilerPath.Title = "Select the HTML Help 1 compiler installation location";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(13, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(322, 26);
            this.label6.TabIndex = 4;
            this.label6.Text = "Project-specific components path";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(8, 266);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(771, 75);
            this.label2.TabIndex = 7;
            this.label2.Text = resources.GetString("label2.Text");
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dividerLabel3
            // 
            this.dividerLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dividerLabel3.Location = new System.Drawing.Point(3, 233);
            this.dividerLabel3.Name = "dividerLabel3";
            this.dividerLabel3.Size = new System.Drawing.Size(779, 26);
            this.dividerLabel3.TabIndex = 6;
            this.dividerLabel3.Text = "Source Code Base Path";
            // 
            // chkWarnOnMissingSourceContext
            // 
            this.chkWarnOnMissingSourceContext.AutoSize = true;
            this.chkWarnOnMissingSourceContext.Location = new System.Drawing.Point(220, 430);
            this.chkWarnOnMissingSourceContext.Name = "chkWarnOnMissingSourceContext";
            this.chkWarnOnMissingSourceContext.Size = new System.Drawing.Size(423, 29);
            this.chkWarnOnMissingSourceContext.TabIndex = 9;
            this.chkWarnOnMissingSourceContext.Tag = "WarnOnMissingSourceContext";
            this.chkWarnOnMissingSourceContext.Text = "Report missing type source contexts as warnings";
            this.chkWarnOnMissingSourceContext.UseVisualStyleBackColor = true;
            // 
            // PathPropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.chkWarnOnMissingSourceContext);
            this.Controls.Add(this.txtSourceCodeBasePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dividerLabel3);
            this.Controls.Add(this.txtComponentPath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtWorkingPath);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.txtHtmlHelp1xCompilerPath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dividerLabel2);
            this.Controls.Add(this.dividerLabel1);
            this.MinimumSize = new System.Drawing.Size(785, 625);
            this.Name = "PathPropertiesPageControl";
            this.Size = new System.Drawing.Size(785, 625);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Utils.Controls.DividerLabel dividerLabel1;
        private Utils.Controls.DividerLabel dividerLabel2;
        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ErrorProvider epWarning;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private Utils.Controls.FolderPathUserControl txtWorkingPath;
        private Utils.Controls.FolderPathUserControl txtOutputPath;
        private Utils.Controls.FolderPathUserControl txtHtmlHelp1xCompilerPath;
        private Utils.Controls.FolderPathUserControl txtComponentPath;
        private System.Windows.Forms.Label label6;
        private Utils.Controls.FolderPathUserControl txtSourceCodeBasePath;
        private System.Windows.Forms.Label label2;
        private Utils.Controls.DividerLabel dividerLabel3;
        private System.Windows.Forms.CheckBox chkWarnOnMissingSourceContext;
    }
}
