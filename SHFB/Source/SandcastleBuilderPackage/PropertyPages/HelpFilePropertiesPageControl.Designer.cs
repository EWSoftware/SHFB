namespace SandcastleBuilder.Package.PropertyPages
{
    partial class HelpFilePropertiesPageControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpFilePropertiesPageControl));
            this.epNotes = new System.Windows.Forms.ErrorProvider(this.components);
            this.cboContentPlacement = new System.Windows.Forms.ComboBox();
            this.txtCopyrightText = new System.Windows.Forms.TextBox();
            this.txtHeaderText = new System.Windows.Forms.TextBox();
            this.txtFooterText = new System.Windows.Forms.TextBox();
            this.txtHtmlHelpName = new System.Windows.Forms.TextBox();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.txtRootNamespaceTitle = new System.Windows.Forms.TextBox();
            this.cboPresentationStyle = new System.Windows.Forms.ComboBox();
            this.chkNamespaceGrouping = new System.Windows.Forms.CheckBox();
            this.udcMaximumGroupParts = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCopyrightHref = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFeedbackEMailAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFeedbackEMailLinkText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtHelpTitle = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cboNamingMethod = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chkPreliminary = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkRootNamespaceContainer = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cboSdkLinkTarget = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.cblSyntaxFilters = new System.Windows.Forms.CheckedListBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtHelpFileVersion = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcMaximumGroupParts)).BeginInit();
            this.SuspendLayout();
            // 
            // epNotes
            // 
            this.epNotes.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epNotes.ContainerControl = this;
            this.epNotes.Icon = ((System.Drawing.Icon)(resources.GetObject("epNotes.Icon")));
            // 
            // cboContentPlacement
            // 
            this.cboContentPlacement.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.epNotes.SetError(this.cboContentPlacement, "This will be ignored if the TOC is split via a custom tag or a\nsite map/conceptua" +
        "l content topic setting");
            this.cboContentPlacement.FormattingEnabled = true;
            this.epNotes.SetIconPadding(this.cboContentPlacement, 5);
            this.cboContentPlacement.Location = new System.Drawing.Point(314, 240);
            this.cboContentPlacement.MaxDropDownItems = 16;
            this.cboContentPlacement.Name = "cboContentPlacement";
            this.cboContentPlacement.Size = new System.Drawing.Size(267, 33);
            this.cboContentPlacement.TabIndex = 13;
            this.cboContentPlacement.Tag = "ContentPlacement";
            // 
            // txtCopyrightText
            // 
            this.txtCopyrightText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtCopyrightText, "This value is treated as plain text");
            this.epNotes.SetIconPadding(this.txtCopyrightText, 5);
            this.txtCopyrightText.Location = new System.Drawing.Point(314, 535);
            this.txtCopyrightText.Name = "txtCopyrightText";
            this.txtCopyrightText.Size = new System.Drawing.Size(484, 31);
            this.txtCopyrightText.TabIndex = 28;
            this.txtCopyrightText.Tag = "CopyrightText";
            // 
            // txtHeaderText
            // 
            this.txtHeaderText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtHeaderText, "HTML elements are supported within this property\'s value");
            this.epNotes.SetIconPadding(this.txtHeaderText, 5);
            this.txtHeaderText.Location = new System.Drawing.Point(314, 424);
            this.txtHeaderText.Name = "txtHeaderText";
            this.txtHeaderText.Size = new System.Drawing.Size(484, 31);
            this.txtHeaderText.TabIndex = 22;
            this.txtHeaderText.Tag = "HeaderText";
            // 
            // txtFooterText
            // 
            this.txtFooterText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtFooterText, "HTML elements are supported within this property\'s value");
            this.epNotes.SetIconPadding(this.txtFooterText, 5);
            this.txtFooterText.Location = new System.Drawing.Point(314, 461);
            this.txtFooterText.Name = "txtFooterText";
            this.txtFooterText.Size = new System.Drawing.Size(484, 31);
            this.txtFooterText.TabIndex = 24;
            this.txtFooterText.Tag = "FooterText";
            // 
            // txtHtmlHelpName
            // 
            this.txtHtmlHelpName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtHtmlHelpName, "Do not include a path or extension.  For MS Help Viewer builds, avoid periods,\r\na" +
        "mpersands, and pound signs as they are not valid in the help file name.");
            this.epNotes.SetIconPadding(this.txtHtmlHelpName, 5);
            this.txtHtmlHelpName.Location = new System.Drawing.Point(314, 49);
            this.txtHtmlHelpName.Name = "txtHtmlHelpName";
            this.txtHtmlHelpName.Size = new System.Drawing.Size(484, 31);
            this.txtHtmlHelpName.TabIndex = 3;
            this.txtHtmlHelpName.Tag = "HtmlHelpName";
            this.txtHtmlHelpName.Text = "Documentation";
            // 
            // cboLanguage
            // 
            this.epNotes.SetError(this.cboLanguage, "Select a value from the dropdown or enter a language identifier such as en-US");
            this.cboLanguage.FormattingEnabled = true;
            this.epNotes.SetIconPadding(this.cboLanguage, 5);
            this.cboLanguage.Location = new System.Drawing.Point(314, 123);
            this.cboLanguage.MaxDropDownItems = 16;
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(267, 33);
            this.cboLanguage.TabIndex = 7;
            this.cboLanguage.Tag = "Language";
            this.cboLanguage.Validating += new System.ComponentModel.CancelEventHandler(this.cboLanguage_Validating);
            // 
            // txtRootNamespaceTitle
            // 
            this.txtRootNamespaceTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.epNotes.SetError(this.txtRootNamespaceTitle, "If not set, a default title of \"Namespaces\" is used");
            this.epNotes.SetIconPadding(this.txtRootNamespaceTitle, 5);
            this.txtRootNamespaceTitle.Location = new System.Drawing.Point(402, 280);
            this.txtRootNamespaceTitle.Name = "txtRootNamespaceTitle";
            this.txtRootNamespaceTitle.Size = new System.Drawing.Size(396, 31);
            this.txtRootNamespaceTitle.TabIndex = 16;
            this.txtRootNamespaceTitle.Tag = "RootNamespaceTitle";
            // 
            // cboPresentationStyle
            // 
            this.cboPresentationStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPresentationStyle.FormattingEnabled = true;
            this.epNotes.SetIconPadding(this.cboPresentationStyle, 5);
            this.cboPresentationStyle.Location = new System.Drawing.Point(314, 201);
            this.cboPresentationStyle.MaxDropDownItems = 16;
            this.cboPresentationStyle.Name = "cboPresentationStyle";
            this.cboPresentationStyle.Size = new System.Drawing.Size(346, 33);
            this.cboPresentationStyle.TabIndex = 11;
            this.cboPresentationStyle.Tag = "PresentationStyle";
            this.cboPresentationStyle.SelectedIndexChanged += new System.EventHandler(this.cboPresentationStyle_SelectedIndexChanged);
            // 
            // chkNamespaceGrouping
            // 
            this.chkNamespaceGrouping.AutoSize = true;
            this.epNotes.SetError(this.chkNamespaceGrouping, "The selected presentation style must support namespace\ngrouping for this to have " +
        "any effect.");
            this.chkNamespaceGrouping.Location = new System.Drawing.Point(314, 317);
            this.chkNamespaceGrouping.Name = "chkNamespaceGrouping";
            this.chkNamespaceGrouping.Size = new System.Drawing.Size(366, 29);
            this.chkNamespaceGrouping.TabIndex = 17;
            this.chkNamespaceGrouping.Tag = "NamespaceGrouping";
            this.chkNamespaceGrouping.Text = "Enable namespace grouping if supported";
            this.chkNamespaceGrouping.UseVisualStyleBackColor = true;
            // 
            // udcMaximumGroupParts
            // 
            this.epNotes.SetError(this.udcMaximumGroupParts, "A higher value results in more namespace groups");
            this.epNotes.SetIconPadding(this.udcMaximumGroupParts, 5);
            this.udcMaximumGroupParts.Location = new System.Drawing.Point(314, 352);
            this.udcMaximumGroupParts.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.udcMaximumGroupParts.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.udcMaximumGroupParts.Name = "udcMaximumGroupParts";
            this.udcMaximumGroupParts.Size = new System.Drawing.Size(106, 31);
            this.udcMaximumGroupParts.TabIndex = 19;
            this.udcMaximumGroupParts.Tag = "MaximumGroupParts";
            this.udcMaximumGroupParts.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.udcMaximumGroupParts.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 242);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(296, 26);
            this.label2.TabIndex = 12;
            this.label2.Text = "&Conceptual content placement";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 500);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(296, 26);
            this.label3.TabIndex = 25;
            this.label3.Text = "Copyright notice URL";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCopyrightHref
            // 
            this.txtCopyrightHref.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCopyrightHref.Location = new System.Drawing.Point(314, 498);
            this.txtCopyrightHref.Name = "txtCopyrightHref";
            this.txtCopyrightHref.Size = new System.Drawing.Size(484, 31);
            this.txtCopyrightHref.TabIndex = 26;
            this.txtCopyrightHref.Tag = "CopyrightHref";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 537);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(296, 26);
            this.label4.TabIndex = 27;
            this.label4.Text = "Copyright notice text";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtFeedbackEMailAddress
            // 
            this.txtFeedbackEMailAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFeedbackEMailAddress.Location = new System.Drawing.Point(314, 572);
            this.txtFeedbackEMailAddress.Name = "txtFeedbackEMailAddress";
            this.txtFeedbackEMailAddress.Size = new System.Drawing.Size(484, 31);
            this.txtFeedbackEMailAddress.TabIndex = 30;
            this.txtFeedbackEMailAddress.Tag = "FeedbackEMailAddress";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 574);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(296, 26);
            this.label5.TabIndex = 29;
            this.label5.Text = "Feedback e-mail address";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtFeedbackEMailLinkText
            // 
            this.txtFeedbackEMailLinkText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFeedbackEMailLinkText.Location = new System.Drawing.Point(314, 609);
            this.txtFeedbackEMailLinkText.Name = "txtFeedbackEMailLinkText";
            this.txtFeedbackEMailLinkText.Size = new System.Drawing.Size(484, 31);
            this.txtFeedbackEMailLinkText.TabIndex = 32;
            this.txtFeedbackEMailLinkText.Tag = "FeedbackEMailLinkText";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 611);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(296, 26);
            this.label6.TabIndex = 31;
            this.label6.Text = "Feedback e-mail link text";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 426);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(296, 26);
            this.label7.TabIndex = 21;
            this.label7.Text = "&Additional header content";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(12, 463);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(296, 26);
            this.label8.TabIndex = 23;
            this.label8.Text = "Additional footer content";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHelpTitle
            // 
            this.txtHelpTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHelpTitle.Location = new System.Drawing.Point(314, 12);
            this.txtHelpTitle.Name = "txtHelpTitle";
            this.txtHelpTitle.Size = new System.Drawing.Size(484, 31);
            this.txtHelpTitle.TabIndex = 1;
            this.txtHelpTitle.Tag = "HelpTitle";
            this.txtHelpTitle.Text = "A Sandcastle Documented Class Library";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(12, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(296, 26);
            this.label9.TabIndex = 0;
            this.label9.Text = "Help t&itle";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(12, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(296, 26);
            this.label10.TabIndex = 2;
            this.label10.Text = "Help file name";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 125);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(296, 26);
            this.label1.TabIndex = 6;
            this.label1.Text = "Help file language";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboNamingMethod
            // 
            this.cboNamingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNamingMethod.FormattingEnabled = true;
            this.cboNamingMethod.Location = new System.Drawing.Point(314, 162);
            this.cboNamingMethod.MaxDropDownItems = 16;
            this.cboNamingMethod.Name = "cboNamingMethod";
            this.cboNamingMethod.Size = new System.Drawing.Size(267, 33);
            this.cboNamingMethod.TabIndex = 9;
            this.cboNamingMethod.Tag = "NamingMethod";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(12, 164);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(296, 26);
            this.label11.TabIndex = 8;
            this.label11.Text = "Topic file naming method";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkPreliminary
            // 
            this.chkPreliminary.AutoSize = true;
            this.chkPreliminary.Location = new System.Drawing.Point(314, 389);
            this.chkPreliminary.Name = "chkPreliminary";
            this.chkPreliminary.Size = new System.Drawing.Size(396, 29);
            this.chkPreliminary.TabIndex = 20;
            this.chkPreliminary.Tag = "Preliminary";
            this.chkPreliminary.Text = "Include \"preliminary documentation\" warning";
            this.chkPreliminary.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(12, 203);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(296, 26);
            this.label12.TabIndex = 10;
            this.label12.Text = "Presentation style";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRootNamespaceContainer
            // 
            this.chkRootNamespaceContainer.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkRootNamespaceContainer.Location = new System.Drawing.Point(39, 283);
            this.chkRootNamespaceContainer.Name = "chkRootNamespaceContainer";
            this.chkRootNamespaceContainer.Size = new System.Drawing.Size(296, 26);
            this.chkRootNamespaceContainer.TabIndex = 14;
            this.chkRootNamespaceContainer.Tag = "RootNamespaceContainer";
            this.chkRootNamespaceContainer.Text = "Include root namespace container";
            this.chkRootNamespaceContainer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkRootNamespaceContainer.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(341, 282);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(55, 26);
            this.label13.TabIndex = 15;
            this.label13.Text = "Title";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboSdkLinkTarget
            // 
            this.cboSdkLinkTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSdkLinkTarget.FormattingEnabled = true;
            this.cboSdkLinkTarget.Location = new System.Drawing.Point(314, 646);
            this.cboSdkLinkTarget.MaxDropDownItems = 16;
            this.cboSdkLinkTarget.Name = "cboSdkLinkTarget";
            this.cboSdkLinkTarget.Size = new System.Drawing.Size(165, 33);
            this.cboSdkLinkTarget.TabIndex = 34;
            this.cboSdkLinkTarget.Tag = "SdkLinkTarget";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(12, 648);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(296, 26);
            this.label14.TabIndex = 33;
            this.label14.Text = "SDK link target";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(12, 685);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(296, 26);
            this.label15.TabIndex = 35;
            this.label15.Text = "Syntax &filters";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cblSyntaxFilters
            // 
            this.cblSyntaxFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cblSyntaxFilters.FormattingEnabled = true;
            this.cblSyntaxFilters.Location = new System.Drawing.Point(314, 685);
            this.cblSyntaxFilters.MultiColumn = true;
            this.cblSyntaxFilters.Name = "cblSyntaxFilters";
            this.cblSyntaxFilters.Size = new System.Drawing.Size(484, 108);
            this.cblSyntaxFilters.TabIndex = 36;
            this.cblSyntaxFilters.Tag = "SyntaxFilters";
            this.cblSyntaxFilters.SelectedIndexChanged += new System.EventHandler(this.cblSyntaxFilters_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(12, 353);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(296, 26);
            this.label16.TabIndex = 18;
            this.label16.Text = "Maximum group parts";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHelpFileVersion
            // 
            this.txtHelpFileVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHelpFileVersion.Location = new System.Drawing.Point(314, 86);
            this.txtHelpFileVersion.Name = "txtHelpFileVersion";
            this.txtHelpFileVersion.Size = new System.Drawing.Size(484, 31);
            this.txtHelpFileVersion.TabIndex = 5;
            this.txtHelpFileVersion.Tag = "HelpFileVersion";
            this.txtHelpFileVersion.Text = "1.0.0.0";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(12, 88);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(296, 26);
            this.label17.TabIndex = 4;
            this.label17.Text = "Help file version";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HelpFilePropertiesPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.txtHelpFileVersion);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.udcMaximumGroupParts);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.chkNamespaceGrouping);
            this.Controls.Add(this.cblSyntaxFilters);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.cboSdkLinkTarget);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtRootNamespaceTitle);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.chkRootNamespaceContainer);
            this.Controls.Add(this.cboPresentationStyle);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.chkPreliminary);
            this.Controls.Add(this.cboNamingMethod);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.cboLanguage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtHtmlHelpName);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtHelpTitle);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtFooterText);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtHeaderText);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtFeedbackEMailLinkText);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtFeedbackEMailAddress);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtCopyrightText);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtCopyrightHref);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboContentPlacement);
            this.Controls.Add(this.label2);
            this.MinimumSize = new System.Drawing.Size(825, 800);
            this.Name = "HelpFilePropertiesPageControl";
            this.Size = new System.Drawing.Size(825, 800);
            ((System.ComponentModel.ISupportInitialize)(this.epNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udcMaximumGroupParts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider epNotes;
        private System.Windows.Forms.ComboBox cboContentPlacement;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFooterText;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtHeaderText;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFeedbackEMailLinkText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtFeedbackEMailAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCopyrightText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCopyrightHref;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtHtmlHelpName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtHelpTitle;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkPreliminary;
        private System.Windows.Forms.ComboBox cboNamingMethod;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRootNamespaceTitle;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox chkRootNamespaceContainer;
        private System.Windows.Forms.ComboBox cboPresentationStyle;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cboSdkLinkTarget;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckedListBox cblSyntaxFilters;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox chkNamespaceGrouping;
        private System.Windows.Forms.NumericUpDown udcMaximumGroupParts;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtHelpFileVersion;
        private System.Windows.Forms.Label label17;

    }
}
