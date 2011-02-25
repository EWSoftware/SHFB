namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class TopicEditorWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TopicEditorWindow));
            this.editor = new SandcastleBuilder.Gui.ContentEditors.ContentEditorControl();
            this.cmsDropImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miMediaLink = new System.Windows.Forms.ToolStripMenuItem();
            this.miMediaLinkInline = new System.Windows.Forms.ToolStripMenuItem();
            this.miExternalLinkMedia = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbLegacyBold = new System.Windows.Forms.ToolStripButton();
            this.tsbLegacyItalic = new System.Windows.Forms.ToolStripButton();
            this.tsbLegacyUnderline = new System.Windows.Forms.ToolStripButton();
            this.tsbCodeInline = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbListBullet = new System.Windows.Forms.ToolStripButton();
            this.tsbListNumber = new System.Windows.Forms.ToolStripButton();
            this.tsbTable = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLocalLink = new System.Windows.Forms.ToolStripButton();
            this.tsbExternalLink = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbInsertElement = new System.Windows.Forms.ToolStripSplitButton();
            this.miCode = new System.Windows.Forms.ToolStripMenuItem();
            this.miDefinitionTable = new System.Windows.Forms.ToolStripMenuItem();
            this.miSection = new System.Windows.Forms.ToolStripMenuItem();
            this.miQuote = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.miApplication = new System.Windows.Forms.ToolStripMenuItem();
            this.miCommand = new System.Windows.Forms.ToolStripMenuItem();
            this.miEnvironmentVariable = new System.Windows.Forms.ToolStripMenuItem();
            this.miHardware = new System.Windows.Forms.ToolStripMenuItem();
            this.miLiteral = new System.Windows.Forms.ToolStripMenuItem();
            this.miLocalUri = new System.Windows.Forms.ToolStripMenuItem();
            this.miMath = new System.Windows.Forms.ToolStripMenuItem();
            this.miNewTerm = new System.Windows.Forms.ToolStripMenuItem();
            this.miPara = new System.Windows.Forms.ToolStripMenuItem();
            this.miPhrase = new System.Windows.Forms.ToolStripMenuItem();
            this.miQuoteInline = new System.Windows.Forms.ToolStripMenuItem();
            this.miReplaceable = new System.Windows.Forms.ToolStripMenuItem();
            this.miSubscript = new System.Windows.Forms.ToolStripMenuItem();
            this.miSuperscript = new System.Windows.Forms.ToolStripMenuItem();
            this.miSystem = new System.Windows.Forms.ToolStripMenuItem();
            this.miUI = new System.Windows.Forms.ToolStripMenuItem();
            this.miUserInput = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbHtmlEncode = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCutText = new System.Windows.Forms.ToolStripButton();
            this.tsbCopyText = new System.Windows.Forms.ToolStripButton();
            this.tsbPasteText = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbUndo = new System.Windows.Forms.ToolStripButton();
            this.tsbRedo = new System.Windows.Forms.ToolStripButton();
            this.miAlert = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDropImage.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.IsReadOnly = false;
            this.editor.Location = new System.Drawing.Point(0, 25);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(530, 231);
            this.editor.TabIndex = 0;
            // 
            // cmsDropImage
            // 
            this.cmsDropImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miMediaLink,
            this.miMediaLinkInline,
            this.miExternalLinkMedia});
            this.cmsDropImage.Name = "cmsDropImage";
            this.cmsDropImage.ShowImageMargin = false;
            this.cmsDropImage.Size = new System.Drawing.Size(218, 76);
            // 
            // miMediaLink
            // 
            this.miMediaLink.Name = "miMediaLink";
            this.miMediaLink.Size = new System.Drawing.Size(217, 24);
            this.miMediaLink.Text = "&Insert <mediaLink>";
            this.miMediaLink.Click += new System.EventHandler(this.MediaLinkItem_Click);
            // 
            // miMediaLinkInline
            // 
            this.miMediaLinkInline.Name = "miMediaLinkInline";
            this.miMediaLinkInline.Size = new System.Drawing.Size(217, 24);
            this.miMediaLinkInline.Text = "I&nsert <mediaLinkInline>";
            this.miMediaLinkInline.Click += new System.EventHandler(this.MediaLinkItem_Click);
            // 
            // miExternalLinkMedia
            // 
            this.miExternalLinkMedia.Name = "miExternalLinkMedia";
            this.miExternalLinkMedia.Size = new System.Drawing.Size(217, 24);
            this.miExternalLinkMedia.Text = "In&sert <externalLink>";
            this.miExternalLinkMedia.Click += new System.EventHandler(this.MediaLinkItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbLegacyBold,
            this.tsbLegacyItalic,
            this.tsbLegacyUnderline,
            this.tsbCodeInline,
            this.toolStripSeparator3,
            this.tsbListBullet,
            this.tsbListNumber,
            this.tsbTable,
            this.toolStripSeparator1,
            this.tsbLocalLink,
            this.tsbExternalLink,
            this.toolStripSeparator2,
            this.tsbInsertElement,
            this.toolStripSeparator6,
            this.tsbHtmlEncode,
            this.toolStripSeparator4,
            this.tsbCutText,
            this.tsbCopyText,
            this.tsbPasteText,
            this.toolStripSeparator5,
            this.tsbUndo,
            this.tsbRedo});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(530, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbLegacyBold
            // 
            this.tsbLegacyBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLegacyBold.Image = global::SandcastleBuilder.Gui.Properties.Resources.Bold;
            this.tsbLegacyBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLegacyBold.Name = "tsbLegacyBold";
            this.tsbLegacyBold.Size = new System.Drawing.Size(23, 24);
            this.tsbLegacyBold.Text = "legacyBold";
            this.tsbLegacyBold.ToolTipText = "Insert <legacyBold> (Ctrl+B)";
            this.tsbLegacyBold.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // tsbLegacyItalic
            // 
            this.tsbLegacyItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLegacyItalic.Image = global::SandcastleBuilder.Gui.Properties.Resources.Italic;
            this.tsbLegacyItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLegacyItalic.Name = "tsbLegacyItalic";
            this.tsbLegacyItalic.Size = new System.Drawing.Size(23, 24);
            this.tsbLegacyItalic.Text = "legacyItalic";
            this.tsbLegacyItalic.ToolTipText = "Insert <legacyItalic> (Ctrl+I)";
            this.tsbLegacyItalic.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // tsbLegacyUnderline
            // 
            this.tsbLegacyUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLegacyUnderline.Image = ((System.Drawing.Image)(resources.GetObject("tsbLegacyUnderline.Image")));
            this.tsbLegacyUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLegacyUnderline.Name = "tsbLegacyUnderline";
            this.tsbLegacyUnderline.Size = new System.Drawing.Size(23, 24);
            this.tsbLegacyUnderline.Text = "legacyUnderline";
            this.tsbLegacyUnderline.ToolTipText = "Insert <legacyUnderline> (Ctrl+U)";
            this.tsbLegacyUnderline.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // tsbCodeInline
            // 
            this.tsbCodeInline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCodeInline.Image = global::SandcastleBuilder.Gui.Properties.Resources.SnippetsFile;
            this.tsbCodeInline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCodeInline.Name = "tsbCodeInline";
            this.tsbCodeInline.Size = new System.Drawing.Size(23, 24);
            this.tsbCodeInline.Text = "codeInline";
            this.tsbCodeInline.ToolTipText = "Insert <codeInline> (Ctrl+K)";
            this.tsbCodeInline.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbListBullet
            // 
            this.tsbListBullet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbListBullet.Image = global::SandcastleBuilder.Gui.Properties.Resources.List_Bullets;
            this.tsbListBullet.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbListBullet.Name = "tsbListBullet";
            this.tsbListBullet.Size = new System.Drawing.Size(23, 24);
            this.tsbListBullet.ToolTipText = "Insert <list class=\"bullet\">";
            this.tsbListBullet.Click += new System.EventHandler(this.insertList_Click);
            // 
            // tsbListNumber
            // 
            this.tsbListNumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbListNumber.Image = global::SandcastleBuilder.Gui.Properties.Resources.List_Numbered;
            this.tsbListNumber.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbListNumber.Name = "tsbListNumber";
            this.tsbListNumber.Size = new System.Drawing.Size(23, 24);
            this.tsbListNumber.ToolTipText = "Insert <list class=\"ordered\">";
            this.tsbListNumber.Click += new System.EventHandler(this.insertList_Click);
            // 
            // tsbTable
            // 
            this.tsbTable.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbTable.Image = global::SandcastleBuilder.Gui.Properties.Resources.Table;
            this.tsbTable.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbTable.Name = "tsbTable";
            this.tsbTable.Size = new System.Drawing.Size(23, 24);
            this.tsbTable.ToolTipText = "Insert <table>";
            this.tsbTable.Click += new System.EventHandler(this.tsbTable_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbLocalLink
            // 
            this.tsbLocalLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLocalLink.Image = global::SandcastleBuilder.Gui.Properties.Resources.LocalLink;
            this.tsbLocalLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLocalLink.Name = "tsbLocalLink";
            this.tsbLocalLink.Size = new System.Drawing.Size(23, 24);
            this.tsbLocalLink.ToolTipText = "Insert <link> for in-page address";
            this.tsbLocalLink.Click += new System.EventHandler(this.tsbLocalLink_Click);
            // 
            // tsbExternalLink
            // 
            this.tsbExternalLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbExternalLink.Image = global::SandcastleBuilder.Gui.Properties.Resources.ExternalLink;
            this.tsbExternalLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExternalLink.Name = "tsbExternalLink";
            this.tsbExternalLink.Size = new System.Drawing.Size(23, 24);
            this.tsbExternalLink.ToolTipText = "Insert <externalLink>";
            this.tsbExternalLink.Click += new System.EventHandler(this.tsbExternalLink_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbInsertElement
            // 
            this.tsbInsertElement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbInsertElement.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAlert,
            this.miCode,
            this.miDefinitionTable,
            this.miSection,
            this.miQuote,
            this.toolStripSeparator7,
            this.miApplication,
            this.miCommand,
            this.miEnvironmentVariable,
            this.miHardware,
            this.miLiteral,
            this.miLocalUri,
            this.miMath,
            this.miNewTerm,
            this.miPara,
            this.miPhrase,
            this.miQuoteInline,
            this.miReplaceable,
            this.miSubscript,
            this.miSuperscript,
            this.miSystem,
            this.miUI,
            this.miUserInput});
            this.tsbInsertElement.Name = "tsbInsertElement";
            this.tsbInsertElement.Size = new System.Drawing.Size(55, 24);
            this.tsbInsertElement.Text = "alert";
            this.tsbInsertElement.ToolTipText = "Insert <alert>";
            this.tsbInsertElement.ButtonClick += new System.EventHandler(this.tsbInsertElement_ButtonClick);
            // 
            // miCode
            // 
            this.miCode.Name = "miCode";
            this.miCode.Size = new System.Drawing.Size(216, 24);
            this.miCode.Text = "code";
            this.miCode.ToolTipText = "Insert <code>";
            this.miCode.Click += new System.EventHandler(this.miCode_Click);
            // 
            // miDefinitionTable
            // 
            this.miDefinitionTable.Name = "miDefinitionTable";
            this.miDefinitionTable.Size = new System.Drawing.Size(216, 24);
            this.miDefinitionTable.Text = "definitionTable";
            this.miDefinitionTable.ToolTipText = "Insert <definitionTable>";
            this.miDefinitionTable.Click += new System.EventHandler(this.miDefinitionTable_Click);
            // 
            // miSection
            // 
            this.miSection.Name = "miSection";
            this.miSection.Size = new System.Drawing.Size(216, 24);
            this.miSection.Text = "section";
            this.miSection.ToolTipText = "Insert <section>";
            this.miSection.Click += new System.EventHandler(this.miSection_Click);
            // 
            // miQuote
            // 
            this.miQuote.Name = "miQuote";
            this.miQuote.Size = new System.Drawing.Size(216, 24);
            this.miQuote.Text = "quote";
            this.miQuote.ToolTipText = "Insert <quote>";
            this.miQuote.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(213, 6);
            // 
            // miApplication
            // 
            this.miApplication.Name = "miApplication";
            this.miApplication.Size = new System.Drawing.Size(216, 24);
            this.miApplication.Text = "application";
            this.miApplication.ToolTipText = "Insert <application>";
            this.miApplication.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miCommand
            // 
            this.miCommand.Name = "miCommand";
            this.miCommand.Size = new System.Drawing.Size(216, 24);
            this.miCommand.Text = "command";
            this.miCommand.ToolTipText = "Insert <command>";
            this.miCommand.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miEnvironmentVariable
            // 
            this.miEnvironmentVariable.Name = "miEnvironmentVariable";
            this.miEnvironmentVariable.Size = new System.Drawing.Size(216, 24);
            this.miEnvironmentVariable.Text = "environmentVariable";
            this.miEnvironmentVariable.ToolTipText = "Insert <environmentVariable>";
            this.miEnvironmentVariable.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miHardware
            // 
            this.miHardware.Name = "miHardware";
            this.miHardware.Size = new System.Drawing.Size(216, 24);
            this.miHardware.Text = "hardware";
            this.miHardware.ToolTipText = "Insert <hardware>";
            this.miHardware.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miLiteral
            // 
            this.miLiteral.Name = "miLiteral";
            this.miLiteral.Size = new System.Drawing.Size(216, 24);
            this.miLiteral.Text = "literal";
            this.miLiteral.ToolTipText = "Insert <literal>";
            this.miLiteral.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miLocalUri
            // 
            this.miLocalUri.Name = "miLocalUri";
            this.miLocalUri.Size = new System.Drawing.Size(216, 24);
            this.miLocalUri.Text = "localUri";
            this.miLocalUri.ToolTipText = "Insert <localUri>";
            this.miLocalUri.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miMath
            // 
            this.miMath.Name = "miMath";
            this.miMath.Size = new System.Drawing.Size(216, 24);
            this.miMath.Text = "math";
            this.miMath.ToolTipText = "Insert <math>";
            this.miMath.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miNewTerm
            // 
            this.miNewTerm.Name = "miNewTerm";
            this.miNewTerm.Size = new System.Drawing.Size(216, 24);
            this.miNewTerm.Text = "newTerm";
            this.miNewTerm.ToolTipText = "Insert <newTerm>";
            this.miNewTerm.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miPara
            // 
            this.miPara.Name = "miPara";
            this.miPara.Size = new System.Drawing.Size(216, 24);
            this.miPara.Text = "para";
            this.miPara.ToolTipText = "Insert <para>";
            this.miPara.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miPhrase
            // 
            this.miPhrase.Name = "miPhrase";
            this.miPhrase.Size = new System.Drawing.Size(216, 24);
            this.miPhrase.Text = "phrase";
            this.miPhrase.ToolTipText = "Insert <phrase>";
            this.miPhrase.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miQuoteInline
            // 
            this.miQuoteInline.Name = "miQuoteInline";
            this.miQuoteInline.Size = new System.Drawing.Size(216, 24);
            this.miQuoteInline.Text = "quoteInline";
            this.miQuoteInline.ToolTipText = "Insert <quoteInline>";
            this.miQuoteInline.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miReplaceable
            // 
            this.miReplaceable.Name = "miReplaceable";
            this.miReplaceable.Size = new System.Drawing.Size(216, 24);
            this.miReplaceable.Text = "replaceable";
            this.miReplaceable.ToolTipText = "Insert <replaceable>";
            this.miReplaceable.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miSubscript
            // 
            this.miSubscript.Name = "miSubscript";
            this.miSubscript.Size = new System.Drawing.Size(216, 24);
            this.miSubscript.Text = "subscript";
            this.miSubscript.ToolTipText = "Insert <subscript>";
            this.miSubscript.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miSuperscript
            // 
            this.miSuperscript.Name = "miSuperscript";
            this.miSuperscript.Size = new System.Drawing.Size(216, 24);
            this.miSuperscript.Text = "superscript";
            this.miSuperscript.ToolTipText = "Insert <superscript>";
            this.miSuperscript.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miSystem
            // 
            this.miSystem.Name = "miSystem";
            this.miSystem.Size = new System.Drawing.Size(216, 24);
            this.miSystem.Text = "system";
            this.miSystem.ToolTipText = "Insert <system>";
            this.miSystem.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miUI
            // 
            this.miUI.Name = "miUI";
            this.miUI.Size = new System.Drawing.Size(216, 24);
            this.miUI.Text = "ui";
            this.miUI.ToolTipText = "Insert <ui>";
            this.miUI.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // miUserInput
            // 
            this.miUserInput.Name = "miUserInput";
            this.miUserInput.Size = new System.Drawing.Size(216, 24);
            this.miUserInput.Text = "userInput";
            this.miUserInput.ToolTipText = "Insert <userInput>";
            this.miUserInput.Click += new System.EventHandler(this.insertElement_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbHtmlEncode
            // 
            this.tsbHtmlEncode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbHtmlEncode.Image = global::SandcastleBuilder.Gui.Properties.Resources.HtmlEncode;
            this.tsbHtmlEncode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbHtmlEncode.Name = "tsbHtmlEncode";
            this.tsbHtmlEncode.Size = new System.Drawing.Size(23, 24);
            this.tsbHtmlEncode.ToolTipText = "HTML encode selected text";
            this.tsbHtmlEncode.Click += new System.EventHandler(this.tsbHtmlEncode_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbCutText
            // 
            this.tsbCutText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCutText.Image = global::SandcastleBuilder.Gui.Properties.Resources.Cut;
            this.tsbCutText.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCutText.Name = "tsbCutText";
            this.tsbCutText.Size = new System.Drawing.Size(23, 24);
            this.tsbCutText.ToolTipText = "Cut text (Ctrl+X)";
            this.tsbCutText.Click += new System.EventHandler(this.tsbCutText_Click);
            // 
            // tsbCopyText
            // 
            this.tsbCopyText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCopyText.Image = global::SandcastleBuilder.Gui.Properties.Resources.Copy;
            this.tsbCopyText.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCopyText.Name = "tsbCopyText";
            this.tsbCopyText.Size = new System.Drawing.Size(23, 24);
            this.tsbCopyText.ToolTipText = "Copy text (Ctrl+C)";
            this.tsbCopyText.Click += new System.EventHandler(this.tsbCopyText_Click);
            // 
            // tsbPasteText
            // 
            this.tsbPasteText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPasteText.Image = global::SandcastleBuilder.Gui.Properties.Resources.Paste;
            this.tsbPasteText.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPasteText.Name = "tsbPasteText";
            this.tsbPasteText.Size = new System.Drawing.Size(23, 24);
            this.tsbPasteText.ToolTipText = "Paste text (Ctrl+V)";
            this.tsbPasteText.Click += new System.EventHandler(this.tsbPasteText_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbUndo
            // 
            this.tsbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUndo.Image = global::SandcastleBuilder.Gui.Properties.Resources.Undo;
            this.tsbUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUndo.Name = "tsbUndo";
            this.tsbUndo.Size = new System.Drawing.Size(23, 24);
            this.tsbUndo.ToolTipText = "Undo (Ctrl+Z)";
            this.tsbUndo.Click += new System.EventHandler(this.tsbUndo_Click);
            // 
            // tsbRedo
            // 
            this.tsbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRedo.Image = global::SandcastleBuilder.Gui.Properties.Resources.Redo;
            this.tsbRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRedo.Name = "tsbRedo";
            this.tsbRedo.Size = new System.Drawing.Size(23, 24);
            this.tsbRedo.ToolTipText = "Redo (Ctrl+Y)";
            this.tsbRedo.Click += new System.EventHandler(this.tsbRedo_Click);
            // 
            // miAlert
            // 
            this.miAlert.Name = "miAlert";
            this.miAlert.Size = new System.Drawing.Size(216, 24);
            this.miAlert.Text = "alert";
            this.miAlert.ToolTipText = "Insert <alert>";
            this.miAlert.Click += new System.EventHandler(this.miAlert_Click);
            // 
            // TopicEditorWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(530, 256);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.editor);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TopicEditorWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ShowInTaskbar = false;
            this.cmsDropImage.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SandcastleBuilder.Gui.ContentEditors.ContentEditorControl editor;
        private System.Windows.Forms.ContextMenuStrip cmsDropImage;
        private System.Windows.Forms.ToolStripMenuItem miMediaLinkInline;
        private System.Windows.Forms.ToolStripMenuItem miMediaLink;
        private System.Windows.Forms.ToolStripMenuItem miExternalLinkMedia;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbLegacyBold;
        private System.Windows.Forms.ToolStripButton tsbLegacyItalic;
        private System.Windows.Forms.ToolStripButton tsbListNumber;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbTable;
        private System.Windows.Forms.ToolStripButton tsbLocalLink;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbListBullet;
        private System.Windows.Forms.ToolStripButton tsbExternalLink;
        private System.Windows.Forms.ToolStripSplitButton tsbInsertElement;
        private System.Windows.Forms.ToolStripMenuItem miSection;
        private System.Windows.Forms.ToolStripMenuItem miPara;
        private System.Windows.Forms.ToolStripMenuItem miCode;
        private System.Windows.Forms.ToolStripButton tsbLegacyUnderline;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tsbUndo;
        private System.Windows.Forms.ToolStripButton tsbRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton tsbCutText;
        private System.Windows.Forms.ToolStripButton tsbCopyText;
        private System.Windows.Forms.ToolStripButton tsbPasteText;
        private System.Windows.Forms.ToolStripButton tsbHtmlEncode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem miDefinitionTable;
        private System.Windows.Forms.ToolStripButton tsbCodeInline;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem miQuote;
        private System.Windows.Forms.ToolStripMenuItem miEnvironmentVariable;
        private System.Windows.Forms.ToolStripMenuItem miLocalUri;
        private System.Windows.Forms.ToolStripMenuItem miApplication;
        private System.Windows.Forms.ToolStripMenuItem miCommand;
        private System.Windows.Forms.ToolStripMenuItem miHardware;
        private System.Windows.Forms.ToolStripMenuItem miLiteral;
        private System.Windows.Forms.ToolStripMenuItem miMath;
        private System.Windows.Forms.ToolStripMenuItem miNewTerm;
        private System.Windows.Forms.ToolStripMenuItem miPhrase;
        private System.Windows.Forms.ToolStripMenuItem miQuoteInline;
        private System.Windows.Forms.ToolStripMenuItem miReplaceable;
        private System.Windows.Forms.ToolStripMenuItem miSubscript;
        private System.Windows.Forms.ToolStripMenuItem miSuperscript;
        private System.Windows.Forms.ToolStripMenuItem miSystem;
        private System.Windows.Forms.ToolStripMenuItem miUI;
        private System.Windows.Forms.ToolStripMenuItem miUserInput;
        private System.Windows.Forms.ToolStripMenuItem miAlert;
    }
}