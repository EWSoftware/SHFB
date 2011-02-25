namespace SandcastleBuilder.Utils.Design
{
    partial class ApiFilterEditorDlg
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
                if(italicFont != null)
                    italicFont.Dispose();

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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Documented APIs", 46, 46);
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Inherited APIs", 47, 47);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApiFilterEditorDlg));
            this.btnClose = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnReset = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnGoto = new System.Windows.Forms.Button();
            this.btnInclude = new System.Windows.Forms.Button();
            this.btnExclude = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.tvApiList = new System.Windows.Forms.TreeView();
            this.ilTreeImages = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.lblLoading = new System.Windows.Forms.Label();
            this.pbWait = new System.Windows.Forms.PictureBox();
            this.lvSearchResults = new System.Windows.Forms.ListView();
            this.colMember = new System.Windows.Forms.ColumnHeader();
            this.colFullName = new System.Windows.Forms.ColumnHeader();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pnlOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.chkCaseSensitive = new System.Windows.Forms.CheckBox();
            this.chkFullyQualified = new System.Windows.Forms.CheckBox();
            this.chkNamespaces = new System.Windows.Forms.CheckBox();
            this.chkClasses = new System.Windows.Forms.CheckBox();
            this.chkStructures = new System.Windows.Forms.CheckBox();
            this.chkInterfaces = new System.Windows.Forms.CheckBox();
            this.chkEnumerations = new System.Windows.Forms.CheckBox();
            this.chkDelegates = new System.Windows.Forms.CheckBox();
            this.chkConstructors = new System.Windows.Forms.CheckBox();
            this.chkMethods = new System.Windows.Forms.CheckBox();
            this.chkOperators = new System.Windows.Forms.CheckBox();
            this.chkProperties = new System.Windows.Forms.CheckBox();
            this.chkEvents = new System.Windows.Forms.CheckBox();
            this.chkFields = new System.Windows.Forms.CheckBox();
            this.chkPublic = new System.Windows.Forms.CheckBox();
            this.chkProtected = new System.Windows.Forms.CheckBox();
            this.chkInternal = new System.Windows.Forms.CheckBox();
            this.chkPrivate = new System.Windows.Forms.CheckBox();
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.epErrors = new System.Windows.Forms.ErrorProvider(this.components);
            this.statusBarTextProvider1 = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(842, 611);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnClose, "Close: Close this form");
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.toolTip1.SetToolTip(this.btnClose, "Close this form");
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(748, 611);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnReset, "Reset: Reset the API filter to its default state");
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "&Reset";
            this.toolTip1.SetToolTip(this.btnReset, "Reset the API filter");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnFind
            // 
            this.btnFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFind.Location = new System.Drawing.Point(500, 16);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnFind, "Find: Find all members matching the search expression");
            this.btnFind.TabIndex = 1;
            this.btnFind.Text = "&Find";
            this.toolTip1.SetToolTip(this.btnFind, "Find matching members");
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnGoto
            // 
            this.btnGoto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGoto.Enabled = false;
            this.btnGoto.Location = new System.Drawing.Point(3, 558);
            this.btnGoto.Name = "btnGoto";
            this.btnGoto.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnGoto, "Goto: Goto the selected member in the tree view");
            this.btnGoto.TabIndex = 3;
            this.btnGoto.Text = "&Goto";
            this.toolTip1.SetToolTip(this.btnGoto, "Goto the selected member");
            this.btnGoto.UseVisualStyleBackColor = true;
            this.btnGoto.Click += new System.EventHandler(this.btnGoto_Click);
            // 
            // btnInclude
            // 
            this.btnInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInclude.Enabled = false;
            this.btnInclude.Location = new System.Drawing.Point(97, 558);
            this.btnInclude.Name = "btnInclude";
            this.btnInclude.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnInclude, "Include: Include the selected members");
            this.btnInclude.TabIndex = 4;
            this.btnInclude.Text = "&Include";
            this.toolTip1.SetToolTip(this.btnInclude, "Include selected members");
            this.btnInclude.UseVisualStyleBackColor = true;
            this.btnInclude.Click += new System.EventHandler(this.btnIncludeExclude_Click);
            // 
            // btnExclude
            // 
            this.btnExclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExclude.Enabled = false;
            this.btnExclude.Location = new System.Drawing.Point(191, 558);
            this.btnExclude.Name = "btnExclude";
            this.btnExclude.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnExclude, "Exclude: Exclude the selected members");
            this.btnExclude.TabIndex = 5;
            this.btnExclude.Text = "&Exclude";
            this.toolTip1.SetToolTip(this.btnExclude, "Exclude the selected members");
            this.btnExclude.UseVisualStyleBackColor = true;
            this.btnExclude.Click += new System.EventHandler(this.btnIncludeExclude_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(654, 611);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(88, 32);
            this.statusBarTextProvider1.SetStatusBarText(this.btnHelp, "Help: View help for this form");
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "&Help";
            this.toolTip1.SetToolTip(this.btnHelp, "View help for this form");
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // tvApiList
            // 
            this.tvApiList.CheckBoxes = true;
            this.tvApiList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvApiList.HideSelection = false;
            this.tvApiList.ImageIndex = 0;
            this.tvApiList.ImageList = this.ilTreeImages;
            this.tvApiList.Location = new System.Drawing.Point(0, 0);
            this.tvApiList.Name = "tvApiList";
            treeNode1.Checked = true;
            treeNode1.ImageIndex = 46;
            treeNode1.Name = "docNode";
            treeNode1.SelectedImageIndex = 46;
            treeNode1.Text = "Documented APIs";
            treeNode1.ToolTipText = "Documented APIs in your assemblies";
            treeNode2.Checked = true;
            treeNode2.ImageIndex = 47;
            treeNode2.Name = "inheritedNode";
            treeNode2.SelectedImageIndex = 47;
            treeNode2.Text = "Inherited APIs";
            treeNode2.ToolTipText = "APIs inherited from classes in dependent assemblies and the .NET Framework";
            this.tvApiList.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.tvApiList.SelectedImageIndex = 0;
            this.tvApiList.ShowNodeToolTips = true;
            this.tvApiList.Size = new System.Drawing.Size(308, 593);
            this.statusBarTextProvider1.SetStatusBarText(this.tvApiList, "The API list to filter");
            this.tvApiList.TabIndex = 1;
            this.tvApiList.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvApiList_AfterCheck);
            this.tvApiList.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvApiList_BeforeExpand);
            this.tvApiList.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvApiList_BeforeCheck);
            // 
            // ilTreeImages
            // 
            this.ilTreeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilTreeImages.ImageStream")));
            this.ilTreeImages.TransparentColor = System.Drawing.Color.Magenta;
            this.ilTreeImages.Images.SetKeyName(0, "Api_Unknown.bmp");
            this.ilTreeImages.Images.SetKeyName(1, "Api_Namespace.bmp");
            this.ilTreeImages.Images.SetKeyName(2, "Api_Public_Class.bmp");
            this.ilTreeImages.Images.SetKeyName(3, "Api_Public_Structure.bmp");
            this.ilTreeImages.Images.SetKeyName(4, "Api_Public_Interface.bmp");
            this.ilTreeImages.Images.SetKeyName(5, "Api_Public_Enum.bmp");
            this.ilTreeImages.Images.SetKeyName(6, "Api_Public_Delegate.bmp");
            this.ilTreeImages.Images.SetKeyName(7, "Api_Public_Constructor.bmp");
            this.ilTreeImages.Images.SetKeyName(8, "Api_Public_Method.bmp");
            this.ilTreeImages.Images.SetKeyName(9, "Api_Public_Operator.bmp");
            this.ilTreeImages.Images.SetKeyName(10, "Api_Public_Property.bmp");
            this.ilTreeImages.Images.SetKeyName(11, "Api_Public_Event.bmp");
            this.ilTreeImages.Images.SetKeyName(12, "Api_Public_Field.bmp");
            this.ilTreeImages.Images.SetKeyName(13, "Api_Protected_Class.bmp");
            this.ilTreeImages.Images.SetKeyName(14, "Api_Protected_Structure.bmp");
            this.ilTreeImages.Images.SetKeyName(15, "Api_Protected_Interface.bmp");
            this.ilTreeImages.Images.SetKeyName(16, "Api_Protected_Enum.bmp");
            this.ilTreeImages.Images.SetKeyName(17, "Api_Protected_Delegate.bmp");
            this.ilTreeImages.Images.SetKeyName(18, "Api_Protected_Constructor.bmp");
            this.ilTreeImages.Images.SetKeyName(19, "Api_Protected_Method.bmp");
            this.ilTreeImages.Images.SetKeyName(20, "Api_Protected_Operator.bmp");
            this.ilTreeImages.Images.SetKeyName(21, "Api_Protected_Property.bmp");
            this.ilTreeImages.Images.SetKeyName(22, "Api_Protected_Event.bmp");
            this.ilTreeImages.Images.SetKeyName(23, "Api_Protected_Field.bmp");
            this.ilTreeImages.Images.SetKeyName(24, "Api_Internal_Class.bmp");
            this.ilTreeImages.Images.SetKeyName(25, "Api_Internal_Structure.bmp");
            this.ilTreeImages.Images.SetKeyName(26, "Api_Internal_Interface.bmp");
            this.ilTreeImages.Images.SetKeyName(27, "Api_Internal_Enum.bmp");
            this.ilTreeImages.Images.SetKeyName(28, "Api_Internal_Delegate.bmp");
            this.ilTreeImages.Images.SetKeyName(29, "Api_Internal_Constructor.bmp");
            this.ilTreeImages.Images.SetKeyName(30, "Api_Internal_Method.bmp");
            this.ilTreeImages.Images.SetKeyName(31, "Api_Internal_Operator.bmp");
            this.ilTreeImages.Images.SetKeyName(32, "Api_Internal_Property.bmp");
            this.ilTreeImages.Images.SetKeyName(33, "Api_Internal_Event.bmp");
            this.ilTreeImages.Images.SetKeyName(34, "Api_Internal_Field.bmp");
            this.ilTreeImages.Images.SetKeyName(35, "Api_Private_Class.bmp");
            this.ilTreeImages.Images.SetKeyName(36, "Api_Private_Structure.bmp");
            this.ilTreeImages.Images.SetKeyName(37, "Api_Private_Interface.bmp");
            this.ilTreeImages.Images.SetKeyName(38, "Api_Private_Enum.bmp");
            this.ilTreeImages.Images.SetKeyName(39, "Api_Private_Delegate.bmp");
            this.ilTreeImages.Images.SetKeyName(40, "Api_Private_Constructor.bmp");
            this.ilTreeImages.Images.SetKeyName(41, "Api_Private_Method.bmp");
            this.ilTreeImages.Images.SetKeyName(42, "Api_Private_Operator.bmp");
            this.ilTreeImages.Images.SetKeyName(43, "Api_Private_Property.bmp");
            this.ilTreeImages.Images.SetKeyName(44, "Api_Private_Event.bmp");
            this.ilTreeImages.Images.SetKeyName(45, "Api_Private_Field.bmp");
            this.ilTreeImages.Images.SetKeyName(46, "Api_Documented.bmp");
            this.ilTreeImages.Images.SetKeyName(47, "Api_Undocumented.bmp");
            this.ilTreeImages.Images.SetKeyName(48, "Api_Protected.bmp");
            this.ilTreeImages.Images.SetKeyName(49, "Api_Internal.bmp");
            this.ilTreeImages.Images.SetKeyName(50, "Api_Private.bmp");
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 12);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.lblLoading);
            this.splitContainer.Panel1.Controls.Add(this.pbWait);
            this.splitContainer.Panel1.Controls.Add(this.tvApiList);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer.Panel2.Controls.Add(this.btnExclude);
            this.splitContainer.Panel2.Controls.Add(this.btnInclude);
            this.splitContainer.Panel2.Controls.Add(this.btnGoto);
            this.splitContainer.Panel2.Controls.Add(this.lvSearchResults);
            this.splitContainer.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer.Size = new System.Drawing.Size(918, 593);
            this.splitContainer.SplitterDistance = 308;
            this.splitContainer.SplitterWidth = 10;
            this.splitContainer.TabIndex = 0;
            // 
            // lblLoading
            // 
            this.lblLoading.AutoSize = true;
            this.lblLoading.BackColor = System.Drawing.SystemColors.Window;
            this.lblLoading.Location = new System.Drawing.Point(50, 85);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(155, 17);
            this.lblLoading.TabIndex = 0;
            this.lblLoading.Text = "Loading namespaces...";
            // 
            // pbWait
            // 
            this.pbWait.BackColor = System.Drawing.SystemColors.Window;
            this.pbWait.Image = global::SandcastleBuilder.Utils.Properties.Resources.SpinningWheel;
            this.pbWait.Location = new System.Drawing.Point(12, 77);
            this.pbWait.Name = "pbWait";
            this.pbWait.Size = new System.Drawing.Size(32, 32);
            this.pbWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbWait.TabIndex = 7;
            this.pbWait.TabStop = false;
            // 
            // lvSearchResults
            // 
            this.lvSearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colMember,
            this.colFullName});
            this.lvSearchResults.FullRowSelect = true;
            this.lvSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSearchResults.HideSelection = false;
            this.lvSearchResults.Location = new System.Drawing.Point(3, 217);
            this.lvSearchResults.Name = "lvSearchResults";
            this.lvSearchResults.Size = new System.Drawing.Size(594, 335);
            this.lvSearchResults.SmallImageList = this.ilTreeImages;
            this.lvSearchResults.TabIndex = 1;
            this.lvSearchResults.UseCompatibleStateImageBehavior = false;
            this.lvSearchResults.View = System.Windows.Forms.View.Details;
            this.lvSearchResults.DoubleClick += new System.EventHandler(this.lvSearchResults_DoubleClick);
            // 
            // colMember
            // 
            this.colMember.Text = "Members Found";
            this.colMember.Width = 200;
            // 
            // colFullName
            // 
            this.colFullName.Text = "Full Name";
            this.colFullName.Width = 350;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.pnlOptions);
            this.groupBox1.Controls.Add(this.btnFind);
            this.groupBox1.Controls.Add(this.txtSearchText);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(594, 208);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "&Search Options";
            // 
            // pnlOptions
            // 
            this.pnlOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOptions.AutoScroll = true;
            this.pnlOptions.Controls.Add(this.chkCaseSensitive);
            this.pnlOptions.Controls.Add(this.chkFullyQualified);
            this.pnlOptions.Controls.Add(this.chkNamespaces);
            this.pnlOptions.Controls.Add(this.chkClasses);
            this.pnlOptions.Controls.Add(this.chkStructures);
            this.pnlOptions.Controls.Add(this.chkInterfaces);
            this.pnlOptions.Controls.Add(this.chkEnumerations);
            this.pnlOptions.Controls.Add(this.chkDelegates);
            this.pnlOptions.Controls.Add(this.chkConstructors);
            this.pnlOptions.Controls.Add(this.chkMethods);
            this.pnlOptions.Controls.Add(this.chkOperators);
            this.pnlOptions.Controls.Add(this.chkProperties);
            this.pnlOptions.Controls.Add(this.chkEvents);
            this.pnlOptions.Controls.Add(this.chkFields);
            this.pnlOptions.Controls.Add(this.chkPublic);
            this.pnlOptions.Controls.Add(this.chkProtected);
            this.pnlOptions.Controls.Add(this.chkInternal);
            this.pnlOptions.Controls.Add(this.chkPrivate);
            this.pnlOptions.Location = new System.Drawing.Point(6, 49);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(582, 151);
            this.pnlOptions.TabIndex = 2;
            // 
            // chkCaseSensitive
            // 
            this.chkCaseSensitive.Location = new System.Drawing.Point(3, 3);
            this.chkCaseSensitive.Name = "chkCaseSensitive";
            this.chkCaseSensitive.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkCaseSensitive, "Perform a case-sensitive search");
            this.chkCaseSensitive.TabIndex = 0;
            this.chkCaseSensitive.Text = "Case-sensitive";
            this.chkCaseSensitive.UseVisualStyleBackColor = true;
            // 
            // chkFullyQualified
            // 
            this.chkFullyQualified.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkFullyQualified.ImageIndex = 11;
            this.chkFullyQualified.Location = new System.Drawing.Point(144, 3);
            this.chkFullyQualified.Name = "chkFullyQualified";
            this.chkFullyQualified.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkFullyQualified, "Search the fully-qualified names rather than just the member names");
            this.chkFullyQualified.TabIndex = 1;
            this.chkFullyQualified.Text = "Fully-qualified";
            this.chkFullyQualified.UseVisualStyleBackColor = true;
            // 
            // chkNamespaces
            // 
            this.chkNamespaces.Checked = true;
            this.chkNamespaces.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNamespaces.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkNamespaces.ImageIndex = 1;
            this.chkNamespaces.ImageList = this.ilTreeImages;
            this.chkNamespaces.Location = new System.Drawing.Point(285, 3);
            this.chkNamespaces.Name = "chkNamespaces";
            this.chkNamespaces.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkNamespaces, "Include namespaces in the results");
            this.chkNamespaces.TabIndex = 2;
            this.chkNamespaces.Text = "Namespaces";
            this.chkNamespaces.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkNamespaces.UseVisualStyleBackColor = true;
            // 
            // chkClasses
            // 
            this.chkClasses.Checked = true;
            this.chkClasses.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClasses.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkClasses.ImageIndex = 2;
            this.chkClasses.ImageList = this.ilTreeImages;
            this.chkClasses.Location = new System.Drawing.Point(426, 3);
            this.chkClasses.Name = "chkClasses";
            this.chkClasses.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkClasses, "Include classes in the results");
            this.chkClasses.TabIndex = 3;
            this.chkClasses.Text = "Classes";
            this.chkClasses.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkClasses.UseVisualStyleBackColor = true;
            // 
            // chkStructures
            // 
            this.chkStructures.Checked = true;
            this.chkStructures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStructures.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkStructures.ImageIndex = 3;
            this.chkStructures.ImageList = this.ilTreeImages;
            this.chkStructures.Location = new System.Drawing.Point(3, 33);
            this.chkStructures.Name = "chkStructures";
            this.chkStructures.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkStructures, "Include structures in the results");
            this.chkStructures.TabIndex = 4;
            this.chkStructures.Text = "Structures";
            this.chkStructures.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkStructures.UseVisualStyleBackColor = true;
            // 
            // chkInterfaces
            // 
            this.chkInterfaces.Checked = true;
            this.chkInterfaces.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInterfaces.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkInterfaces.ImageIndex = 4;
            this.chkInterfaces.ImageList = this.ilTreeImages;
            this.chkInterfaces.Location = new System.Drawing.Point(144, 33);
            this.chkInterfaces.Name = "chkInterfaces";
            this.chkInterfaces.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkInterfaces, "Include interfaces in the results");
            this.chkInterfaces.TabIndex = 5;
            this.chkInterfaces.Text = "Interfaces";
            this.chkInterfaces.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkInterfaces.UseVisualStyleBackColor = true;
            // 
            // chkEnumerations
            // 
            this.chkEnumerations.Checked = true;
            this.chkEnumerations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnumerations.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkEnumerations.ImageIndex = 5;
            this.chkEnumerations.ImageList = this.ilTreeImages;
            this.chkEnumerations.Location = new System.Drawing.Point(285, 33);
            this.chkEnumerations.Name = "chkEnumerations";
            this.chkEnumerations.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkEnumerations, "Include enumerations in the results");
            this.chkEnumerations.TabIndex = 6;
            this.chkEnumerations.Text = "Enumerations";
            this.chkEnumerations.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkEnumerations.UseVisualStyleBackColor = true;
            // 
            // chkDelegates
            // 
            this.chkDelegates.Checked = true;
            this.chkDelegates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDelegates.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkDelegates.ImageIndex = 6;
            this.chkDelegates.ImageList = this.ilTreeImages;
            this.chkDelegates.Location = new System.Drawing.Point(426, 33);
            this.chkDelegates.Name = "chkDelegates";
            this.chkDelegates.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkDelegates, "Include delegates in the results");
            this.chkDelegates.TabIndex = 7;
            this.chkDelegates.Text = "Delegates";
            this.chkDelegates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkDelegates.UseVisualStyleBackColor = true;
            // 
            // chkConstructors
            // 
            this.chkConstructors.Checked = true;
            this.chkConstructors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkConstructors.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkConstructors.ImageIndex = 7;
            this.chkConstructors.ImageList = this.ilTreeImages;
            this.chkConstructors.Location = new System.Drawing.Point(3, 63);
            this.chkConstructors.Name = "chkConstructors";
            this.chkConstructors.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkConstructors, "Include constructors in the results");
            this.chkConstructors.TabIndex = 8;
            this.chkConstructors.Text = "Constructors";
            this.chkConstructors.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkConstructors.UseVisualStyleBackColor = true;
            // 
            // chkMethods
            // 
            this.chkMethods.Checked = true;
            this.chkMethods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMethods.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkMethods.ImageIndex = 8;
            this.chkMethods.ImageList = this.ilTreeImages;
            this.chkMethods.Location = new System.Drawing.Point(144, 63);
            this.chkMethods.Name = "chkMethods";
            this.chkMethods.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkMethods, "Include methods in the results");
            this.chkMethods.TabIndex = 9;
            this.chkMethods.Text = "Methods";
            this.chkMethods.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkMethods.UseVisualStyleBackColor = true;
            // 
            // chkOperators
            // 
            this.chkOperators.Checked = true;
            this.chkOperators.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOperators.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkOperators.ImageIndex = 9;
            this.chkOperators.ImageList = this.ilTreeImages;
            this.chkOperators.Location = new System.Drawing.Point(285, 63);
            this.chkOperators.Name = "chkOperators";
            this.chkOperators.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkOperators, "Include operators in the results");
            this.chkOperators.TabIndex = 13;
            this.chkOperators.Text = "Operators";
            this.chkOperators.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkOperators.UseVisualStyleBackColor = true;
            // 
            // chkProperties
            // 
            this.chkProperties.Checked = true;
            this.chkProperties.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProperties.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkProperties.ImageIndex = 10;
            this.chkProperties.ImageList = this.ilTreeImages;
            this.chkProperties.Location = new System.Drawing.Point(426, 63);
            this.chkProperties.Name = "chkProperties";
            this.chkProperties.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkProperties, "Include properties in the results");
            this.chkProperties.TabIndex = 10;
            this.chkProperties.Text = "Properties";
            this.chkProperties.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkProperties.UseVisualStyleBackColor = true;
            // 
            // chkEvents
            // 
            this.chkEvents.Checked = true;
            this.chkEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEvents.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkEvents.ImageIndex = 11;
            this.chkEvents.ImageList = this.ilTreeImages;
            this.chkEvents.Location = new System.Drawing.Point(3, 93);
            this.chkEvents.Name = "chkEvents";
            this.chkEvents.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkEvents, "Include events in the results");
            this.chkEvents.TabIndex = 11;
            this.chkEvents.Text = "Events";
            this.chkEvents.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkEvents.UseVisualStyleBackColor = true;
            // 
            // chkFields
            // 
            this.chkFields.Checked = true;
            this.chkFields.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFields.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkFields.ImageIndex = 12;
            this.chkFields.ImageList = this.ilTreeImages;
            this.chkFields.Location = new System.Drawing.Point(144, 93);
            this.chkFields.Name = "chkFields";
            this.chkFields.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkFields, "Include fields in the results");
            this.chkFields.TabIndex = 12;
            this.chkFields.Text = "Fields";
            this.chkFields.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkFields.UseVisualStyleBackColor = true;
            // 
            // chkPublic
            // 
            this.chkPublic.Checked = true;
            this.chkPublic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPublic.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkPublic.ImageList = this.ilTreeImages;
            this.chkPublic.Location = new System.Drawing.Point(285, 93);
            this.chkPublic.Name = "chkPublic";
            this.chkPublic.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkPublic, "Include public members in the results");
            this.chkPublic.TabIndex = 14;
            this.chkPublic.Text = "Public";
            this.chkPublic.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkPublic.UseVisualStyleBackColor = true;
            // 
            // chkProtected
            // 
            this.chkProtected.Checked = true;
            this.chkProtected.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProtected.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkProtected.ImageIndex = 48;
            this.chkProtected.ImageList = this.ilTreeImages;
            this.chkProtected.Location = new System.Drawing.Point(426, 93);
            this.chkProtected.Name = "chkProtected";
            this.chkProtected.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkProtected, "Include protected members in the results");
            this.chkProtected.TabIndex = 15;
            this.chkProtected.Text = "Protected";
            this.chkProtected.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkProtected.UseVisualStyleBackColor = true;
            // 
            // chkInternal
            // 
            this.chkInternal.Checked = true;
            this.chkInternal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInternal.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkInternal.ImageIndex = 49;
            this.chkInternal.ImageList = this.ilTreeImages;
            this.chkInternal.Location = new System.Drawing.Point(3, 123);
            this.chkInternal.Name = "chkInternal";
            this.chkInternal.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkInternal, "Include internal members in the results");
            this.chkInternal.TabIndex = 16;
            this.chkInternal.Text = "Internal";
            this.chkInternal.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkInternal.UseVisualStyleBackColor = true;
            // 
            // chkPrivate
            // 
            this.chkPrivate.Checked = true;
            this.chkPrivate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrivate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkPrivate.ImageIndex = 50;
            this.chkPrivate.ImageList = this.ilTreeImages;
            this.chkPrivate.Location = new System.Drawing.Point(144, 123);
            this.chkPrivate.Name = "chkPrivate";
            this.chkPrivate.Size = new System.Drawing.Size(135, 24);
            this.statusBarTextProvider1.SetStatusBarText(this.chkPrivate, "Include private members in the results");
            this.chkPrivate.TabIndex = 17;
            this.chkPrivate.Text = "Private";
            this.chkPrivate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkPrivate.UseVisualStyleBackColor = true;
            // 
            // txtSearchText
            // 
            this.txtSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchText.Location = new System.Drawing.Point(6, 21);
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.Size = new System.Drawing.Size(471, 22);
            this.statusBarTextProvider1.SetStatusBarText(this.txtSearchText, "Search Text: Enter a string or regular expression for which to search");
            this.txtSearchText.TabIndex = 0;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.AutoEllipsis = true;
            this.lblProgress.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblProgress.Location = new System.Drawing.Point(12, 616);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(636, 23);
            this.lblProgress.TabIndex = 1;
            this.lblProgress.Text = "--";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // epErrors
            // 
            this.epErrors.ContainerControl = this;
            // 
            // ApiFilterEditorDlg
            // 
            this.AcceptButton = this.btnFind;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(942, 660);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.btnClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(950, 600);
            this.Name = "ApiFilterEditorDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "API Filter";
            this.Load += new System.EventHandler(this.ApiFilterEditorDlg_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApiFilterEditorDlg_FormClosing);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbWait)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.pnlOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.epErrors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider statusBarTextProvider1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView tvApiList;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.PictureBox pbWait;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ImageList ilTreeImages;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.ErrorProvider epErrors;
        private System.Windows.Forms.ListView lvSearchResults;
        private System.Windows.Forms.ColumnHeader colMember;
        private System.Windows.Forms.FlowLayoutPanel pnlOptions;
        private System.Windows.Forms.CheckBox chkCaseSensitive;
        private System.Windows.Forms.CheckBox chkNamespaces;
        private System.Windows.Forms.CheckBox chkClasses;
        private System.Windows.Forms.CheckBox chkStructures;
        private System.Windows.Forms.CheckBox chkInterfaces;
        private System.Windows.Forms.CheckBox chkEnumerations;
        private System.Windows.Forms.CheckBox chkDelegates;
        private System.Windows.Forms.CheckBox chkConstructors;
        private System.Windows.Forms.CheckBox chkMethods;
        private System.Windows.Forms.CheckBox chkProperties;
        private System.Windows.Forms.CheckBox chkEvents;
        private System.Windows.Forms.CheckBox chkFields;
        private System.Windows.Forms.ColumnHeader colFullName;
        private System.Windows.Forms.CheckBox chkFullyQualified;
        private System.Windows.Forms.Button btnGoto;
        private System.Windows.Forms.Button btnInclude;
        private System.Windows.Forms.Button btnExclude;
        private System.Windows.Forms.CheckBox chkOperators;
        private System.Windows.Forms.CheckBox chkPublic;
        private System.Windows.Forms.CheckBox chkProtected;
        private System.Windows.Forms.CheckBox chkInternal;
        private System.Windows.Forms.CheckBox chkPrivate;
        private System.Windows.Forms.Button btnHelp;
    }
}