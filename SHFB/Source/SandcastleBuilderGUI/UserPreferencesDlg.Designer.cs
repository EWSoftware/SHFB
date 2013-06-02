namespace SandcastleBuilder.Gui
{
    partial class UserPreferencesDlg
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserPreferencesDlg));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.chkVerboseLogging = new System.Windows.Forms.CheckBox();
            this.txtHTMLHelp2ViewerPath = new System.Windows.Forms.TextBox();
            this.btnSelectHxSViewer = new System.Windows.Forms.Button();
            this.udcASPNetDevServerPort = new System.Windows.Forms.NumericUpDown();
            this.tabPreferences = new System.Windows.Forms.TabControl();
            this.pgGeneral = new System.Windows.Forms.TabPage();
            this.chkPerUserProjectState = new System.Windows.Forms.CheckBox();
            this.txtMSHelpViewerPath = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnSelectMSHCViewer = new System.Windows.Forms.Button();
            this.chkEnterMatching = new System.Windows.Forms.CheckBox();
            this.chkShowLineNumbers = new System.Windows.Forms.CheckBox();
            this.chkOpenHelp = new System.Windows.Forms.CheckBox();
            this.cboBeforeBuildAction = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnEditorFont = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lblEditorExample = new System.Windows.Forms.Label();
            this.btnBuildFont = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnBuildForeground = new System.Windows.Forms.Button();
            this.btnBuildBackground = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblBuildExample = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pgSpellChecking = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnDefaultAttributes = new System.Windows.Forms.Button();
            this.btnRemoveAttribute = new System.Windows.Forms.Button();
            this.lbSpellCheckedAttributes = new System.Windows.Forms.ListBox();
            this.btnAddAttribute = new System.Windows.Forms.Button();
            this.txtAttributeName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDefaultIgnored = new System.Windows.Forms.Button();
            this.btnRemoveIgnored = new System.Windows.Forms.Button();
            this.lbIgnoredXmlElements = new System.Windows.Forms.ListBox();
            this.btnAddIgnored = new System.Windows.Forms.Button();
            this.txtIgnoredElement = new System.Windows.Forms.TextBox();
            this.dividerLabel2 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.dividerLabel1 = new SandcastleBuilder.Utils.Controls.DividerLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.cboDefaultLanguage = new System.Windows.Forms.ComboBox();
            this.chkTreatUnderscoresAsSeparators = new System.Windows.Forms.CheckBox();
            this.chkIgnoreXmlInText = new System.Windows.Forms.CheckBox();
            this.chkIgnoreFilenamesAndEMail = new System.Windows.Forms.CheckBox();
            this.chkIgnoreAllUppercase = new System.Windows.Forms.CheckBox();
            this.chkIgnoreWordsWithDigits = new System.Windows.Forms.CheckBox();
            this.pgContentEditors = new System.Windows.Forms.TabPage();
            this.btnDelete = new System.Windows.Forms.Button();
            this.ilButton = new System.Windows.Forms.ImageList(this.components);
            this.btnAddFile = new System.Windows.Forms.Button();
            this.pgProps = new SandcastleBuilder.Utils.Controls.CustomPropertyGrid();
            this.lbContentEditors = new SandcastleBuilder.Utils.Controls.RefreshableItemListBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.lbUserDictionary = new System.Windows.Forms.ListBox();
            this.btnRemoveWord = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udcASPNetDevServerPort)).BeginInit();
            this.tabPreferences.SuspendLayout();
            this.pgGeneral.SuspendLayout();
            this.pgSpellChecking.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.pgContentEditors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(620, 522);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnCancel, "Cancel: Close without saving preferences");
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Close without saving");
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 522);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnOK, "OK: Save preferences");
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Save preferences");
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // chkVerboseLogging
            // 
            this.chkVerboseLogging.Location = new System.Drawing.Point(262, 204);
            this.chkVerboseLogging.Name = "chkVerboseLogging";
            this.chkVerboseLogging.Size = new System.Drawing.Size(292, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkVerboseLogging, "Verbose Logging: Check this box to display all output messages.  Uncheck to displ" +
        "ay summary messages only.");
            this.chkVerboseLogging.TabIndex = 11;
            this.chkVerboseLogging.Text = "&Build output verbose logging enabled";
            this.chkVerboseLogging.UseVisualStyleBackColor = true;
            // 
            // txtHTMLHelp2ViewerPath
            // 
            this.txtHTMLHelp2ViewerPath.Location = new System.Drawing.Point(262, 18);
            this.txtHTMLHelp2ViewerPath.Name = "txtHTMLHelp2ViewerPath";
            this.txtHTMLHelp2ViewerPath.Size = new System.Drawing.Size(358, 23);
            this.statusBarTextProvider1.SetStatusBarText(this.txtHTMLHelp2ViewerPath, "MS Help 2 (.HxS) Viewer: Enter the path and filename of the application used to v" +
        "iew MS Help 2 files");
            this.txtHTMLHelp2ViewerPath.TabIndex = 1;
            // 
            // btnSelectHxSViewer
            // 
            this.btnSelectHxSViewer.Location = new System.Drawing.Point(621, 17);
            this.btnSelectHxSViewer.Name = "btnSelectHxSViewer";
            this.btnSelectHxSViewer.Size = new System.Drawing.Size(32, 25);
            this.statusBarTextProvider1.SetStatusBarText(this.btnSelectHxSViewer, "Select Viewer: Browser for the MS Help 2 viewer application");
            this.btnSelectHxSViewer.TabIndex = 2;
            this.btnSelectHxSViewer.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectHxSViewer, "Select MS Help 2 viewer application");
            this.btnSelectHxSViewer.UseVisualStyleBackColor = true;
            this.btnSelectHxSViewer.Click += new System.EventHandler(this.btnSelectViewer_Click);
            // 
            // udcASPNetDevServerPort
            // 
            this.udcASPNetDevServerPort.Location = new System.Drawing.Point(262, 74);
            this.udcASPNetDevServerPort.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.udcASPNetDevServerPort.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udcASPNetDevServerPort.Name = "udcASPNetDevServerPort";
            this.udcASPNetDevServerPort.Size = new System.Drawing.Size(70, 23);
            this.statusBarTextProvider1.SetStatusBarText(this.udcASPNetDevServerPort, "Server Port: Select the port to use when launching the ASP.NET Development Web Se" +
        "rver");
            this.udcASPNetDevServerPort.TabIndex = 7;
            this.udcASPNetDevServerPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcASPNetDevServerPort.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // tabPreferences
            // 
            this.tabPreferences.Controls.Add(this.pgGeneral);
            this.tabPreferences.Controls.Add(this.pgSpellChecking);
            this.tabPreferences.Controls.Add(this.pgContentEditors);
            this.tabPreferences.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.tabPreferences.Location = new System.Drawing.Point(12, 12);
            this.tabPreferences.Name = "tabPreferences";
            this.tabPreferences.SelectedIndex = 0;
            this.tabPreferences.Size = new System.Drawing.Size(696, 504);
            this.statusBarTextProvider1.SetStatusBarText(this.tabPreferences, "User preferences");
            this.tabPreferences.TabIndex = 0;
            // 
            // pgGeneral
            // 
            this.pgGeneral.Controls.Add(this.chkPerUserProjectState);
            this.pgGeneral.Controls.Add(this.txtMSHelpViewerPath);
            this.pgGeneral.Controls.Add(this.label8);
            this.pgGeneral.Controls.Add(this.btnSelectMSHCViewer);
            this.pgGeneral.Controls.Add(this.chkEnterMatching);
            this.pgGeneral.Controls.Add(this.chkShowLineNumbers);
            this.pgGeneral.Controls.Add(this.chkOpenHelp);
            this.pgGeneral.Controls.Add(this.cboBeforeBuildAction);
            this.pgGeneral.Controls.Add(this.label7);
            this.pgGeneral.Controls.Add(this.btnEditorFont);
            this.pgGeneral.Controls.Add(this.label3);
            this.pgGeneral.Controls.Add(this.lblEditorExample);
            this.pgGeneral.Controls.Add(this.btnBuildFont);
            this.pgGeneral.Controls.Add(this.label4);
            this.pgGeneral.Controls.Add(this.btnBuildForeground);
            this.pgGeneral.Controls.Add(this.btnBuildBackground);
            this.pgGeneral.Controls.Add(this.label5);
            this.pgGeneral.Controls.Add(this.lblBuildExample);
            this.pgGeneral.Controls.Add(this.label6);
            this.pgGeneral.Controls.Add(this.txtHTMLHelp2ViewerPath);
            this.pgGeneral.Controls.Add(this.udcASPNetDevServerPort);
            this.pgGeneral.Controls.Add(this.chkVerboseLogging);
            this.pgGeneral.Controls.Add(this.label2);
            this.pgGeneral.Controls.Add(this.label1);
            this.pgGeneral.Controls.Add(this.btnSelectHxSViewer);
            this.pgGeneral.Location = new System.Drawing.Point(4, 25);
            this.pgGeneral.Name = "pgGeneral";
            this.pgGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.pgGeneral.Size = new System.Drawing.Size(688, 475);
            this.statusBarTextProvider1.SetStatusBarText(this.pgGeneral, "General user preferences");
            this.pgGeneral.TabIndex = 0;
            this.pgGeneral.Text = "General Preferences";
            this.pgGeneral.UseVisualStyleBackColor = true;
            // 
            // chkPerUserProjectState
            // 
            this.chkPerUserProjectState.Location = new System.Drawing.Point(262, 128);
            this.chkPerUserProjectState.Name = "chkPerUserProjectState";
            this.chkPerUserProjectState.Size = new System.Drawing.Size(320, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkPerUserProjectState, "Save Project State: Check this box to save the window layout for each project per" +
        " user");
            this.chkPerUserProjectState.TabIndex = 8;
            this.chkPerUserProjectState.Text = "&Save window state per project for each user";
            this.chkPerUserProjectState.UseVisualStyleBackColor = true;
            // 
            // txtMSHelpViewerPath
            // 
            this.txtMSHelpViewerPath.Location = new System.Drawing.Point(262, 46);
            this.txtMSHelpViewerPath.Name = "txtMSHelpViewerPath";
            this.txtMSHelpViewerPath.Size = new System.Drawing.Size(358, 23);
            this.statusBarTextProvider1.SetStatusBarText(this.txtMSHelpViewerPath, "MS Help Viewer (.mshc) Viewer: Enter the path and filename of the application use" +
        "d to view MS Help Viewer files");
            this.txtMSHelpViewerPath.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(6, 46);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(250, 23);
            this.label8.TabIndex = 3;
            this.label8.Text = "MS Help Viewer (.mshc) Viewer Path";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSelectMSHCViewer
            // 
            this.btnSelectMSHCViewer.Location = new System.Drawing.Point(621, 45);
            this.btnSelectMSHCViewer.Name = "btnSelectMSHCViewer";
            this.btnSelectMSHCViewer.Size = new System.Drawing.Size(32, 25);
            this.statusBarTextProvider1.SetStatusBarText(this.btnSelectMSHCViewer, "Select Viewer: Browser for the MS Help Viewer viewer application");
            this.btnSelectMSHCViewer.TabIndex = 5;
            this.btnSelectMSHCViewer.Text = "...";
            this.toolTip1.SetToolTip(this.btnSelectMSHCViewer, "Select MS Help Viewer viewer application");
            this.btnSelectMSHCViewer.UseVisualStyleBackColor = true;
            this.btnSelectMSHCViewer.Click += new System.EventHandler(this.btnSelectViewer_Click);
            // 
            // chkEnterMatching
            // 
            this.chkEnterMatching.AutoSize = true;
            this.chkEnterMatching.Location = new System.Drawing.Point(92, 425);
            this.chkEnterMatching.Name = "chkEnterMatching";
            this.chkEnterMatching.Size = new System.Drawing.Size(349, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkEnterMatching, "Enter Matching: When checked, matching brackets, parentheses, and quotes will be " +
        "entered automatically in the text editor");
            this.chkEnterMatching.TabIndex = 24;
            this.chkEnterMatching.Text = "E&nter matching brackets, parentheses, and quotes";
            this.chkEnterMatching.UseVisualStyleBackColor = true;
            // 
            // chkShowLineNumbers
            // 
            this.chkShowLineNumbers.AutoSize = true;
            this.chkShowLineNumbers.Location = new System.Drawing.Point(92, 399);
            this.chkShowLineNumbers.Name = "chkShowLineNumbers";
            this.chkShowLineNumbers.Size = new System.Drawing.Size(230, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkShowLineNumbers, "Show Line Numbers: Check this box to show line numbers in the text editor");
            this.chkShowLineNumbers.TabIndex = 20;
            this.chkShowLineNumbers.Text = "Sho&w line numbers in text editor";
            this.chkShowLineNumbers.UseVisualStyleBackColor = true;
            // 
            // chkOpenHelp
            // 
            this.chkOpenHelp.Location = new System.Drawing.Point(262, 234);
            this.chkOpenHelp.Name = "chkOpenHelp";
            this.chkOpenHelp.Size = new System.Drawing.Size(292, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkOpenHelp, "Open Help: Check this to open the help file after a successful build");
            this.chkOpenHelp.TabIndex = 12;
            this.chkOpenHelp.Text = "&Open help file after successful build";
            this.chkOpenHelp.UseVisualStyleBackColor = true;
            // 
            // cboBeforeBuildAction
            // 
            this.cboBeforeBuildAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBeforeBuildAction.FormattingEnabled = true;
            this.cboBeforeBuildAction.Items.AddRange(new object[] {
            "Save all changes",
            "Save changes to open documents only",
            "Prompt to save all changes",
            "Don\'t save any changes"});
            this.cboBeforeBuildAction.Location = new System.Drawing.Point(262, 174);
            this.cboBeforeBuildAction.Name = "cboBeforeBuildAction";
            this.cboBeforeBuildAction.Size = new System.Drawing.Size(292, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.cboBeforeBuildAction, "Before Build: Select the action to take before performing a build");
            this.cboBeforeBuildAction.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(135, 174);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(121, 23);
            this.label7.TabIndex = 9;
            this.label7.Text = "B&efore Building";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnEditorFont
            // 
            this.btnEditorFont.Location = new System.Drawing.Point(487, 396);
            this.btnEditorFont.Name = "btnEditorFont";
            this.btnEditorFont.Size = new System.Drawing.Size(32, 25);
            this.btnEditorFont.TabIndex = 22;
            this.btnEditorFont.Text = "...";
            this.btnEditorFont.UseVisualStyleBackColor = true;
            this.btnEditorFont.Click += new System.EventHandler(this.btnFont_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(352, 397);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 23);
            this.label3.TabIndex = 21;
            this.label3.Text = "&Text Editor Font";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEditorExample
            // 
            this.lblEditorExample.BackColor = System.Drawing.SystemColors.Window;
            this.lblEditorExample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblEditorExample.Font = new System.Drawing.Font("Courier New", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEditorExample.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblEditorExample.Location = new System.Drawing.Point(525, 371);
            this.lblEditorExample.Name = "lblEditorExample";
            this.lblEditorExample.Size = new System.Drawing.Size(128, 75);
            this.lblEditorExample.TabIndex = 23;
            this.lblEditorExample.Text = "Example Text";
            this.lblEditorExample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBuildFont
            // 
            this.btnBuildFont.Location = new System.Drawing.Point(487, 306);
            this.btnBuildFont.Name = "btnBuildFont";
            this.btnBuildFont.Size = new System.Drawing.Size(32, 25);
            this.btnBuildFont.TabIndex = 18;
            this.btnBuildFont.Text = "...";
            this.btnBuildFont.UseVisualStyleBackColor = true;
            this.btnBuildFont.Click += new System.EventHandler(this.btnFont_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(435, 307);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 23);
            this.label4.TabIndex = 17;
            this.label4.Text = "Font";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnBuildForeground
            // 
            this.btnBuildForeground.Location = new System.Drawing.Point(397, 306);
            this.btnBuildForeground.Name = "btnBuildForeground";
            this.btnBuildForeground.Size = new System.Drawing.Size(32, 25);
            this.btnBuildForeground.TabIndex = 16;
            this.btnBuildForeground.Text = "...";
            this.btnBuildForeground.UseVisualStyleBackColor = true;
            this.btnBuildForeground.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // btnBuildBackground
            // 
            this.btnBuildBackground.Location = new System.Drawing.Point(262, 306);
            this.btnBuildBackground.Name = "btnBuildBackground";
            this.btnBuildBackground.Size = new System.Drawing.Size(32, 25);
            this.btnBuildBackground.TabIndex = 14;
            this.btnBuildBackground.Text = "...";
            this.btnBuildBackground.UseVisualStyleBackColor = true;
            this.btnBuildBackground.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(300, 307);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 23);
            this.label5.TabIndex = 15;
            this.label5.Text = "Foreground";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBuildExample
            // 
            this.lblBuildExample.BackColor = System.Drawing.SystemColors.Window;
            this.lblBuildExample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBuildExample.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBuildExample.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblBuildExample.Location = new System.Drawing.Point(525, 281);
            this.lblBuildExample.Name = "lblBuildExample";
            this.lblBuildExample.Size = new System.Drawing.Size(128, 75);
            this.lblBuildExample.TabIndex = 19;
            this.lblBuildExample.Text = "Example Text";
            this.lblBuildExample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(82, 307);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(174, 23);
            this.label6.TabIndex = 13;
            this.label6.Text = "B&uild Output Background";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(46, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(210, 47);
            this.label2.TabIndex = 6;
            this.label2.Text = "&ASP.NET Development Web Server Port";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(43, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "MS &Help 2 (.HxS) Viewer Path";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pgSpellChecking
            // 
            this.pgSpellChecking.Controls.Add(this.label10);
            this.pgSpellChecking.Controls.Add(this.btnRemoveWord);
            this.pgSpellChecking.Controls.Add(this.lbUserDictionary);
            this.pgSpellChecking.Controls.Add(this.groupBox2);
            this.pgSpellChecking.Controls.Add(this.groupBox1);
            this.pgSpellChecking.Controls.Add(this.dividerLabel2);
            this.pgSpellChecking.Controls.Add(this.dividerLabel1);
            this.pgSpellChecking.Controls.Add(this.label9);
            this.pgSpellChecking.Controls.Add(this.cboDefaultLanguage);
            this.pgSpellChecking.Controls.Add(this.chkTreatUnderscoresAsSeparators);
            this.pgSpellChecking.Controls.Add(this.chkIgnoreXmlInText);
            this.pgSpellChecking.Controls.Add(this.chkIgnoreFilenamesAndEMail);
            this.pgSpellChecking.Controls.Add(this.chkIgnoreAllUppercase);
            this.pgSpellChecking.Controls.Add(this.chkIgnoreWordsWithDigits);
            this.pgSpellChecking.Location = new System.Drawing.Point(4, 25);
            this.pgSpellChecking.Name = "pgSpellChecking";
            this.pgSpellChecking.Padding = new System.Windows.Forms.Padding(3);
            this.pgSpellChecking.Size = new System.Drawing.Size(688, 475);
            this.statusBarTextProvider1.SetStatusBarText(this.pgSpellChecking, "Spell checking options");
            this.pgSpellChecking.TabIndex = 2;
            this.pgSpellChecking.Text = "Spell Checking";
            this.pgSpellChecking.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnDefaultAttributes);
            this.groupBox2.Controls.Add(this.btnRemoveAttribute);
            this.groupBox2.Controls.Add(this.lbSpellCheckedAttributes);
            this.groupBox2.Controls.Add(this.btnAddAttribute);
            this.groupBox2.Controls.Add(this.txtAttributeName);
            this.groupBox2.Location = new System.Drawing.Point(357, 260);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(310, 209);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Spell Checked &Attributes";
            // 
            // btnDefaultAttributes
            // 
            this.btnDefaultAttributes.Location = new System.Drawing.Point(87, 179);
            this.btnDefaultAttributes.Name = "btnDefaultAttributes";
            this.btnDefaultAttributes.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnDefaultAttributes, "Default: Reset the list to the default spell checked attributes");
            this.btnDefaultAttributes.TabIndex = 4;
            this.btnDefaultAttributes.Text = "Default";
            this.toolTip1.SetToolTip(this.btnDefaultAttributes, "Reset to default list of spell checked attributes");
            this.btnDefaultAttributes.UseVisualStyleBackColor = true;
            this.btnDefaultAttributes.Click += new System.EventHandler(this.btnDefaultAttributes_Click);
            // 
            // btnRemoveAttribute
            // 
            this.btnRemoveAttribute.Location = new System.Drawing.Point(6, 179);
            this.btnRemoveAttribute.Name = "btnRemoveAttribute";
            this.btnRemoveAttribute.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnRemoveAttribute, "Remove Attribute: Remove the selected XML attribute from the list");
            this.btnRemoveAttribute.TabIndex = 3;
            this.btnRemoveAttribute.Text = "Remove";
            this.toolTip1.SetToolTip(this.btnRemoveAttribute, "Remove the selected spell checked attribute");
            this.btnRemoveAttribute.UseVisualStyleBackColor = true;
            this.btnRemoveAttribute.Click += new System.EventHandler(this.btnRemoveAttribute_Click);
            // 
            // lbSpellCheckedAttributes
            // 
            this.lbSpellCheckedAttributes.FormattingEnabled = true;
            this.lbSpellCheckedAttributes.IntegralHeight = false;
            this.lbSpellCheckedAttributes.ItemHeight = 16;
            this.lbSpellCheckedAttributes.Location = new System.Drawing.Point(6, 49);
            this.lbSpellCheckedAttributes.Name = "lbSpellCheckedAttributes";
            this.lbSpellCheckedAttributes.Size = new System.Drawing.Size(298, 124);
            this.lbSpellCheckedAttributes.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbSpellCheckedAttributes, "Spell Checked Attributes: The values of these attributes will be spell checked wh" +
        "en an XML file is spell checked");
            this.lbSpellCheckedAttributes.TabIndex = 2;
            // 
            // btnAddAttribute
            // 
            this.btnAddAttribute.Location = new System.Drawing.Point(229, 20);
            this.btnAddAttribute.Name = "btnAddAttribute";
            this.btnAddAttribute.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddAttribute, "Add Spell Checked Attribute: Click this to add the XML attribute name to the list" +
        " of spell checked attributes");
            this.btnAddAttribute.TabIndex = 1;
            this.btnAddAttribute.Text = "Add";
            this.toolTip1.SetToolTip(this.btnAddAttribute, "Add new spell checked attribute");
            this.btnAddAttribute.UseVisualStyleBackColor = true;
            this.btnAddAttribute.Click += new System.EventHandler(this.btnAddAttribute_Click);
            // 
            // txtAttributeName
            // 
            this.txtAttributeName.Location = new System.Drawing.Point(6, 21);
            this.txtAttributeName.Name = "txtAttributeName";
            this.txtAttributeName.Size = new System.Drawing.Size(219, 23);
            this.statusBarTextProvider1.SetStatusBarText(this.txtAttributeName, "Attribute Name: Enter the name of an XML attribute that should be spell checked");
            this.txtAttributeName.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDefaultIgnored);
            this.groupBox1.Controls.Add(this.btnRemoveIgnored);
            this.groupBox1.Controls.Add(this.lbIgnoredXmlElements);
            this.groupBox1.Controls.Add(this.btnAddIgnored);
            this.groupBox1.Controls.Add(this.txtIgnoredElement);
            this.groupBox1.Location = new System.Drawing.Point(29, 260);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(310, 209);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ignored XML &Elements";
            // 
            // btnDefaultIgnored
            // 
            this.btnDefaultIgnored.Location = new System.Drawing.Point(87, 179);
            this.btnDefaultIgnored.Name = "btnDefaultIgnored";
            this.btnDefaultIgnored.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnDefaultIgnored, "Default: Reset the list to the default ignored XML elements");
            this.btnDefaultIgnored.TabIndex = 4;
            this.btnDefaultIgnored.Text = "Default";
            this.toolTip1.SetToolTip(this.btnDefaultIgnored, "Reset to default list of ignored XML elements");
            this.btnDefaultIgnored.UseVisualStyleBackColor = true;
            this.btnDefaultIgnored.Click += new System.EventHandler(this.btnDefaultIgnored_Click);
            // 
            // btnRemoveIgnored
            // 
            this.btnRemoveIgnored.Location = new System.Drawing.Point(6, 179);
            this.btnRemoveIgnored.Name = "btnRemoveIgnored";
            this.btnRemoveIgnored.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnRemoveIgnored, "Remove Element: Remove the selected XML element from the list");
            this.btnRemoveIgnored.TabIndex = 3;
            this.btnRemoveIgnored.Text = "Remove";
            this.toolTip1.SetToolTip(this.btnRemoveIgnored, "Remove selected ignored XML element");
            this.btnRemoveIgnored.UseVisualStyleBackColor = true;
            this.btnRemoveIgnored.Click += new System.EventHandler(this.btnRemoveIgnored_Click);
            // 
            // lbIgnoredXmlElements
            // 
            this.lbIgnoredXmlElements.FormattingEnabled = true;
            this.lbIgnoredXmlElements.IntegralHeight = false;
            this.lbIgnoredXmlElements.ItemHeight = 16;
            this.lbIgnoredXmlElements.Location = new System.Drawing.Point(6, 49);
            this.lbIgnoredXmlElements.Name = "lbIgnoredXmlElements";
            this.lbIgnoredXmlElements.Size = new System.Drawing.Size(298, 124);
            this.lbIgnoredXmlElements.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbIgnoredXmlElements, "Ignored XML Elements: The content of these elements will be ignored when an XML f" +
        "ile is spell checked");
            this.lbIgnoredXmlElements.TabIndex = 2;
            // 
            // btnAddIgnored
            // 
            this.btnAddIgnored.Location = new System.Drawing.Point(229, 20);
            this.btnAddIgnored.Name = "btnAddIgnored";
            this.btnAddIgnored.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddIgnored, "Add Ignored XML Element: Click this to add the XML element name to the list of ig" +
        "nored elements");
            this.btnAddIgnored.TabIndex = 1;
            this.btnAddIgnored.Text = "Add";
            this.toolTip1.SetToolTip(this.btnAddIgnored, "Add new ignored XML element");
            this.btnAddIgnored.UseVisualStyleBackColor = true;
            this.btnAddIgnored.Click += new System.EventHandler(this.btnAddIgnored_Click);
            // 
            // txtIgnoredElement
            // 
            this.txtIgnoredElement.Location = new System.Drawing.Point(6, 21);
            this.txtIgnoredElement.Name = "txtIgnoredElement";
            this.txtIgnoredElement.Size = new System.Drawing.Size(219, 23);
            this.statusBarTextProvider1.SetStatusBarText(this.txtIgnoredElement, "Ignored XML Element: Enter the name of an XML element whose content should be ign" +
        "ored when spell checking");
            this.txtIgnoredElement.TabIndex = 0;
            // 
            // dividerLabel2
            // 
            this.dividerLabel2.Location = new System.Drawing.Point(3, 11);
            this.dividerLabel2.Name = "dividerLabel2";
            this.dividerLabel2.Size = new System.Drawing.Size(676, 23);
            this.dividerLabel2.TabIndex = 0;
            this.dividerLabel2.Text = "General Options";
            // 
            // dividerLabel1
            // 
            this.dividerLabel1.Location = new System.Drawing.Point(6, 230);
            this.dividerLabel1.Name = "dividerLabel1";
            this.dividerLabel1.Size = new System.Drawing.Size(676, 23);
            this.dividerLabel1.TabIndex = 11;
            this.dividerLabel1.Text = "XML File Options";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(9, 44);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(446, 23);
            this.label9.TabIndex = 1;
            this.label9.Text = "&Default language if dictionary for project language is not available";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDefaultLanguage
            // 
            this.cboDefaultLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultLanguage.FormattingEnabled = true;
            this.cboDefaultLanguage.Location = new System.Drawing.Point(461, 44);
            this.cboDefaultLanguage.Name = "cboDefaultLanguage";
            this.cboDefaultLanguage.Size = new System.Drawing.Size(79, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.cboDefaultLanguage, "Default Language: Select the default language to use when a dictionary for the pr" +
        "oject language is not available");
            this.cboDefaultLanguage.TabIndex = 2;
            this.cboDefaultLanguage.SelectedIndexChanged += new System.EventHandler(this.cboDefaultLanguage_SelectedIndexChanged);
            // 
            // chkTreatUnderscoresAsSeparators
            // 
            this.chkTreatUnderscoresAsSeparators.AutoSize = true;
            this.chkTreatUnderscoresAsSeparators.Location = new System.Drawing.Point(35, 191);
            this.chkTreatUnderscoresAsSeparators.Name = "chkTreatUnderscoresAsSeparators";
            this.chkTreatUnderscoresAsSeparators.Size = new System.Drawing.Size(238, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkTreatUnderscoresAsSeparators, "Treat Underscores as Separators: Check this option to treat underscores as word s" +
        "eparators");
            this.chkTreatUnderscoresAsSeparators.TabIndex = 7;
            this.chkTreatUnderscoresAsSeparators.Text = "Treat underscores as separators";
            this.chkTreatUnderscoresAsSeparators.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreXmlInText
            // 
            this.chkIgnoreXmlInText.AutoSize = true;
            this.chkIgnoreXmlInText.Location = new System.Drawing.Point(35, 164);
            this.chkIgnoreXmlInText.Name = "chkIgnoreXmlInText";
            this.chkIgnoreXmlInText.Size = new System.Drawing.Size(418, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkIgnoreXmlInText, "Ignore XML in Text: Check this option to ignore words that look like XML elements" +
        " in spell checked text");
            this.chkIgnoreXmlInText.TabIndex = 6;
            this.chkIgnoreXmlInText.Text = "Ignore words that look like XML elements in spell checked text";
            this.chkIgnoreXmlInText.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreFilenamesAndEMail
            // 
            this.chkIgnoreFilenamesAndEMail.AutoSize = true;
            this.chkIgnoreFilenamesAndEMail.Location = new System.Drawing.Point(35, 137);
            this.chkIgnoreFilenamesAndEMail.Name = "chkIgnoreFilenamesAndEMail";
            this.chkIgnoreFilenamesAndEMail.Size = new System.Drawing.Size(398, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkIgnoreFilenamesAndEMail, "Ignore Filenames and E-Mail: Check this option to ignore words that look like fil" +
        "enames and e-mail addresses");
            this.chkIgnoreFilenamesAndEMail.TabIndex = 5;
            this.chkIgnoreFilenamesAndEMail.Text = "Ignore words that look like filenames and e-mail addresses";
            this.chkIgnoreFilenamesAndEMail.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreAllUppercase
            // 
            this.chkIgnoreAllUppercase.AutoSize = true;
            this.chkIgnoreAllUppercase.Location = new System.Drawing.Point(35, 110);
            this.chkIgnoreAllUppercase.Name = "chkIgnoreAllUppercase";
            this.chkIgnoreAllUppercase.Size = new System.Drawing.Size(215, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkIgnoreAllUppercase, "Ignore All Uppercase: Check this option to ignore words in all uppercase");
            this.chkIgnoreAllUppercase.TabIndex = 4;
            this.chkIgnoreAllUppercase.Text = "Ignore words in all uppercase";
            this.chkIgnoreAllUppercase.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreWordsWithDigits
            // 
            this.chkIgnoreWordsWithDigits.AutoSize = true;
            this.chkIgnoreWordsWithDigits.Location = new System.Drawing.Point(35, 83);
            this.chkIgnoreWordsWithDigits.Name = "chkIgnoreWordsWithDigits";
            this.chkIgnoreWordsWithDigits.Size = new System.Drawing.Size(176, 21);
            this.statusBarTextProvider1.SetStatusBarText(this.chkIgnoreWordsWithDigits, "Ignore Words With Digits: Check this option to ignore words that contain digits");
            this.chkIgnoreWordsWithDigits.TabIndex = 3;
            this.chkIgnoreWordsWithDigits.Text = "&Ignore words with digits";
            this.chkIgnoreWordsWithDigits.UseVisualStyleBackColor = true;
            // 
            // pgContentEditors
            // 
            this.pgContentEditors.Controls.Add(this.btnDelete);
            this.pgContentEditors.Controls.Add(this.btnAddFile);
            this.pgContentEditors.Controls.Add(this.pgProps);
            this.pgContentEditors.Controls.Add(this.lbContentEditors);
            this.pgContentEditors.Location = new System.Drawing.Point(4, 25);
            this.pgContentEditors.Name = "pgContentEditors";
            this.pgContentEditors.Padding = new System.Windows.Forms.Padding(3);
            this.pgContentEditors.Size = new System.Drawing.Size(688, 475);
            this.statusBarTextProvider1.SetStatusBarText(this.pgContentEditors, "Content file editors");
            this.pgContentEditors.TabIndex = 1;
            this.pgContentEditors.Text = "Content File Editors";
            this.pgContentEditors.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.ImageIndex = 1;
            this.btnDelete.ImageList = this.ilButton;
            this.btnDelete.Location = new System.Drawing.Point(650, 89);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(32, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnDelete, "Delete: Delete the selected definition");
            this.btnDelete.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnDelete, "Delete the selected definition");
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // ilButton
            // 
            this.ilButton.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilButton.ImageStream")));
            this.ilButton.TransparentColor = System.Drawing.Color.Magenta;
            this.ilButton.Images.SetKeyName(0, "AddItem.bmp");
            this.ilButton.Images.SetKeyName(1, "Delete.bmp");
            // 
            // btnAddFile
            // 
            this.btnAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFile.ImageIndex = 0;
            this.btnAddFile.ImageList = this.ilButton;
            this.btnAddFile.Location = new System.Drawing.Point(650, 6);
            this.btnAddFile.Name = "btnAddFile";
            this.btnAddFile.Size = new System.Drawing.Size(32, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddFile, "Add: Add a new content file editor definition");
            this.btnAddFile.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnAddFile, "Add a new content file editor definition");
            this.btnAddFile.UseVisualStyleBackColor = true;
            this.btnAddFile.Click += new System.EventHandler(this.btnAddFile_Click);
            // 
            // pgProps
            // 
            this.pgProps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgProps.Location = new System.Drawing.Point(6, 213);
            this.pgProps.Name = "pgProps";
            this.pgProps.PropertyNamePaneWidth = 150;
            this.pgProps.Size = new System.Drawing.Size(676, 256);
            this.statusBarTextProvider1.SetStatusBarText(this.pgProps, "Edit the properties of the selected content editor");
            this.pgProps.TabIndex = 3;
            this.pgProps.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgProps_PropertyValueChanged);
            // 
            // lbContentEditors
            // 
            this.lbContentEditors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbContentEditors.FormattingEnabled = true;
            this.lbContentEditors.IntegralHeight = false;
            this.lbContentEditors.ItemHeight = 16;
            this.lbContentEditors.Location = new System.Drawing.Point(6, 6);
            this.lbContentEditors.Name = "lbContentEditors";
            this.lbContentEditors.Size = new System.Drawing.Size(638, 201);
            this.statusBarTextProvider1.SetStatusBarText(this.lbContentEditors, "Select a content editor item");
            this.lbContentEditors.TabIndex = 0;
            this.lbContentEditors.SelectedIndexChanged += new System.EventHandler(this.lbContentEditors_SelectedIndexChanged);
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // lbUserDictionary
            // 
            this.lbUserDictionary.FormattingEnabled = true;
            this.lbUserDictionary.IntegralHeight = false;
            this.lbUserDictionary.ItemHeight = 16;
            this.lbUserDictionary.Location = new System.Drawing.Point(461, 99);
            this.lbUserDictionary.Name = "lbUserDictionary";
            this.lbUserDictionary.Size = new System.Drawing.Size(216, 108);
            this.lbUserDictionary.Sorted = true;
            this.statusBarTextProvider1.SetStatusBarText(this.lbUserDictionary, "User Dictionary: Ignored words in the user dictionary file");
            this.lbUserDictionary.TabIndex = 9;
            // 
            // btnRemoveWord
            // 
            this.btnRemoveWord.Location = new System.Drawing.Point(461, 213);
            this.btnRemoveWord.Name = "btnRemoveWord";
            this.btnRemoveWord.Size = new System.Drawing.Size(75, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.btnRemoveWord, "Remove Word: Remove the selected word from the user dictionary");
            this.btnRemoveWord.TabIndex = 10;
            this.btnRemoveWord.Text = "Remove";
            this.toolTip1.SetToolTip(this.btnRemoveWord, "Remove selected word from user dictionary");
            this.btnRemoveWord.UseVisualStyleBackColor = true;
            this.btnRemoveWord.Click += new System.EventHandler(this.btnRemoveWord_Click);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(461, 73);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(206, 23);
            this.label10.TabIndex = 8;
            this.label10.Text = "User Dictionary for Language";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UserPreferencesDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(720, 566);
            this.Controls.Add(this.tabPreferences);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserPreferencesDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User Preferences";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserPreferencesDlg_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.udcASPNetDevServerPort)).EndInit();
            this.tabPreferences.ResumeLayout(false);
            this.pgGeneral.ResumeLayout(false);
            this.pgGeneral.PerformLayout();
            this.pgSpellChecking.ResumeLayout(false);
            this.pgSpellChecking.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.pgContentEditors.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkVerboseLogging;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHTMLHelp2ViewerPath;
        private System.Windows.Forms.Button btnSelectHxSViewer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udcASPNetDevServerPort;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.TabControl tabPreferences;
        private System.Windows.Forms.TabPage pgGeneral;
        private System.Windows.Forms.TabPage pgContentEditors;
        private SandcastleBuilder.Utils.Controls.CustomPropertyGrid pgProps;
        private SandcastleBuilder.Utils.Controls.RefreshableItemListBox lbContentEditors;
        private System.Windows.Forms.ImageList ilButton;
        private System.Windows.Forms.Button btnAddFile;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnEditorFont;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblEditorExample;
        private System.Windows.Forms.Button btnBuildFont;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnBuildForeground;
        private System.Windows.Forms.Button btnBuildBackground;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblBuildExample;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkOpenHelp;
        private System.Windows.Forms.ComboBox cboBeforeBuildAction;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkShowLineNumbers;
        private System.Windows.Forms.CheckBox chkEnterMatching;
        private System.Windows.Forms.TextBox txtMSHelpViewerPath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnSelectMSHCViewer;
        private System.Windows.Forms.CheckBox chkPerUserProjectState;
        private System.Windows.Forms.TabPage pgSpellChecking;
        private Utils.Controls.DividerLabel dividerLabel2;
        private Utils.Controls.DividerLabel dividerLabel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cboDefaultLanguage;
        private System.Windows.Forms.CheckBox chkTreatUnderscoresAsSeparators;
        private System.Windows.Forms.CheckBox chkIgnoreXmlInText;
        private System.Windows.Forms.CheckBox chkIgnoreFilenamesAndEMail;
        private System.Windows.Forms.CheckBox chkIgnoreAllUppercase;
        private System.Windows.Forms.CheckBox chkIgnoreWordsWithDigits;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnRemoveAttribute;
        private System.Windows.Forms.ListBox lbSpellCheckedAttributes;
        private System.Windows.Forms.Button btnAddAttribute;
        private System.Windows.Forms.TextBox txtAttributeName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRemoveIgnored;
        private System.Windows.Forms.ListBox lbIgnoredXmlElements;
        private System.Windows.Forms.Button btnAddIgnored;
        private System.Windows.Forms.TextBox txtIgnoredElement;
        private System.Windows.Forms.Button btnDefaultAttributes;
        private System.Windows.Forms.Button btnDefaultIgnored;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnRemoveWord;
        private System.Windows.Forms.ListBox lbUserDictionary;
    }
}