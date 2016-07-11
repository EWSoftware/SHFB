namespace SandcastleBuilder.Gui.ContentEditors
{
    partial class SpellCheckWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpellCheckWindow));
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.btnIgnoreOnce = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnAddWord = new System.Windows.Forms.Button();
            this.lbSuggestions = new System.Windows.Forms.ListBox();
            this.btnIgnoreAll = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblIssue = new System.Windows.Forms.Label();
            this.lblMisspelledWord = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnIgnoreOnce
            // 
            this.btnIgnoreOnce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIgnoreOnce.Location = new System.Drawing.Point(308, 179);
            this.btnIgnoreOnce.Name = "btnIgnoreOnce";
            this.btnIgnoreOnce.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnIgnoreOnce, "Ignore: Ignore just this occurrence of the misspelled word");
            this.btnIgnoreOnce.TabIndex = 6;
            this.btnIgnoreOnce.Text = "&Ignore Once";
            this.toolTip1.SetToolTip(this.btnIgnoreOnce, "Ignore just this occurrence of the misspelled word");
            this.btnIgnoreOnce.UseVisualStyleBackColor = true;
            this.btnIgnoreOnce.Click += new System.EventHandler(this.btnIgnoreOnce_Click);
            // 
            // btnReplace
            // 
            this.btnReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplace.Location = new System.Drawing.Point(308, 81);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnReplace, "Replace: Replace current misspelling with the selected word");
            this.btnReplace.TabIndex = 4;
            this.btnReplace.Text = "R&eplace";
            this.toolTip1.SetToolTip(this.btnReplace, "Replace current misspelling with the selected word");
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplaceAll.Location = new System.Drawing.Point(308, 122);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnReplaceAll, "Replace All: Replace all occurrences of the misspelling with the selected word");
            this.btnReplaceAll.TabIndex = 5;
            this.btnReplaceAll.Text = "Replace &All";
            this.toolTip1.SetToolTip(this.btnReplaceAll, "Replace all occurrences with the selected word");
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // btnAddWord
            // 
            this.btnAddWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddWord.Location = new System.Drawing.Point(308, 282);
            this.btnAddWord.Name = "btnAddWord";
            this.btnAddWord.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnAddWord, "Add: Add the word to the user dictionary");
            this.btnAddWord.TabIndex = 8;
            this.btnAddWord.Text = "Add &Word";
            this.toolTip1.SetToolTip(this.btnAddWord, "Add the word to the user dictionary");
            this.btnAddWord.UseVisualStyleBackColor = true;
            this.btnAddWord.Click += new System.EventHandler(this.btnAddWord_Click);
            // 
            // lbSuggestions
            // 
            this.lbSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSuggestions.FormattingEnabled = true;
            this.lbSuggestions.ItemHeight = 25;
            this.lbSuggestions.Location = new System.Drawing.Point(12, 81);
            this.lbSuggestions.Name = "lbSuggestions";
            this.lbSuggestions.Size = new System.Drawing.Size(280, 304);
            this.statusBarTextProvider1.SetStatusBarText(this.lbSuggestions, "Replace With: Select the word to replace the misspelled word");
            this.lbSuggestions.TabIndex = 3;
            this.lbSuggestions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbSuggestions_MouseDoubleClick);
            // 
            // btnIgnoreAll
            // 
            this.btnIgnoreAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIgnoreAll.Location = new System.Drawing.Point(308, 220);
            this.btnIgnoreAll.Name = "btnIgnoreAll";
            this.btnIgnoreAll.Size = new System.Drawing.Size(100, 35);
            this.statusBarTextProvider1.SetStatusBarText(this.btnIgnoreAll, "Ignore All: Ignore all occurrences of this misspelled word");
            this.btnIgnoreAll.TabIndex = 7;
            this.btnIgnoreAll.Text = "Ig&nore All";
            this.toolTip1.SetToolTip(this.btnIgnoreAll, "Ignore all occurrences of this misspelled word");
            this.btnIgnoreAll.UseVisualStyleBackColor = true;
            this.btnIgnoreAll.Click += new System.EventHandler(this.btnIgnoreAll_Click);
            // 
            // lblIssue
            // 
            this.lblIssue.Location = new System.Drawing.Point(12, 9);
            this.lblIssue.Name = "lblIssue";
            this.lblIssue.Size = new System.Drawing.Size(167, 26);
            this.lblIssue.TabIndex = 0;
            this.lblIssue.Text = "Misspelled Word";
            this.lblIssue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMisspelledWord
            // 
            this.lblMisspelledWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMisspelledWord.AutoEllipsis = true;
            this.lblMisspelledWord.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblMisspelledWord.Location = new System.Drawing.Point(185, 9);
            this.lblMisspelledWord.Name = "lblMisspelledWord";
            this.lblMisspelledWord.Size = new System.Drawing.Size(223, 26);
            this.lblMisspelledWord.TabIndex = 1;
            this.lblMisspelledWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(11, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 26);
            this.label3.TabIndex = 2;
            this.label3.Text = "&Suggestions";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SpellCheckWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(420, 400);
            this.Controls.Add(this.btnIgnoreAll);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbSuggestions);
            this.Controls.Add(this.lblMisspelledWord);
            this.Controls.Add(this.lblIssue);
            this.Controls.Add(this.btnAddWord);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnIgnoreOnce);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
            | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
            | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
            | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 300);
            this.Name = "SpellCheckWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Float;
            this.ShowInTaskbar = false;
            this.TabText = "Spell Check";
            this.Text = "Spell Check";
            this.VisibleChanged += new System.EventHandler(this.SpellCheckWindow_VisibleChanged);
            this.ParentChanged += new System.EventHandler(this.SpellCheckWindow_ParentChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.Button btnIgnoreOnce;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnAddWord;
        private System.Windows.Forms.Label lblIssue;
        private System.Windows.Forms.Label lblMisspelledWord;
        private System.Windows.Forms.ListBox lbSuggestions;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnIgnoreAll;
    }
}
