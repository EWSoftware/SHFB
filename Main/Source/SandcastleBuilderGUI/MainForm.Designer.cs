namespace SandcastleBuilder.Gui
{
    partial class MainForm
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
                if(project != null)
                    project.Dispose();

                if(fsw != null)
                {
                    fsw.Dispose();
                    fsw = null;
                }

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.sbStatusBarText = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.miProject = new System.Windows.Forms.ToolStripMenuItem();
            this.miNewProject = new System.Windows.Forms.ToolStripMenuItem();
            this.miNewFromOtherFormat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.miOpenProject = new System.Windows.Forms.ToolStripMenuItem();
            this.miCloseProject = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.miClose = new System.Windows.Forms.ToolStripMenuItem();
            this.miCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miCloseAllButCurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.miProjectExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.miExplorerSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.miSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.miRecentProjects = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.miUserPreferences = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miFaq = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.miAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.miDocumentation = new System.Windows.Forms.ToolStripMenuItem();
            this.miBuildProject = new System.Windows.Forms.ToolStripMenuItem();
            this.miCancelBuild = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.miViewHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxViewHelpMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miViewHelpFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.miViewHtmlHelp1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.miViewMSHelp2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.miViewMSHelpViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.miLaunchHlm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.miViewAspNetWebsite = new System.Windows.Forms.ToolStripMenuItem();
            this.miViewHtmlWebsite = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.miOpenHelpAfterBuild = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbViewHelpFile = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.miCleanOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.tcbConfig = new System.Windows.Forms.ToolStripComboBox();
            this.tcbPlatform = new System.Windows.Forms.ToolStripComboBox();
            this.miWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.miViewProjectExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.miViewProjectProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.miEntityReferences = new System.Windows.Forms.ToolStripMenuItem();
            this.miPreviewTopic = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.miViewOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.miViewLog = new System.Windows.Forms.ToolStripMenuItem();
            this.miClearOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbNewProject = new System.Windows.Forms.ToolStripButton();
            this.tsbOpenProject = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbSaveAll = new System.Windows.Forms.ToolStripButton();
            this.tsbProjectExplorer = new System.Windows.Forms.ToolStripButton();
            this.tsbProjectProperties = new System.Windows.Forms.ToolStripButton();
            this.tsbCodeEntitySearch = new System.Windows.Forms.ToolStripButton();
            this.tsbPreviewTopic = new System.Windows.Forms.ToolStripButton();
            this.tsbBuildProject = new System.Windows.Forms.ToolStripButton();
            this.tsbCancelBuild = new System.Windows.Forms.ToolStripButton();
            this.tsbViewOutput = new System.Windows.Forms.ToolStripButton();
            this.tsbFaq = new System.Windows.Forms.ToolStripButton();
            this.tsbAbout = new System.Windows.Forms.ToolStripButton();
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.tsbMain = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslProgressNote = new System.Windows.Forms.ToolStripStatusLabel();
            this.tspbProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.ctxViewHelpMenu.SuspendLayout();
            this.mnuMain.SuspendLayout();
            this.tsbMain.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // miProject
            // 
            this.miProject.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNewProject,
            this.miNewFromOtherFormat,
            this.toolStripSeparator2,
            this.miOpenProject,
            this.miCloseProject,
            this.toolStripSeparator7,
            this.miClose,
            this.miCloseAll,
            this.miCloseAllButCurrent,
            this.toolStripSeparator14,
            this.miProjectExplorer,
            this.miExplorerSeparator,
            this.miSave,
            this.miSaveAs,
            this.miSaveAll,
            this.toolStripSeparator6,
            this.miRecentProjects,
            this.toolStripMenuItem2,
            this.miUserPreferences,
            this.toolStripSeparator1,
            this.miExit});
            this.miProject.Name = "miProject";
            this.miProject.Size = new System.Drawing.Size(42, 21);
            this.sbStatusBarText.SetStatusBarText(this.miProject, "File related options");
            this.miProject.Text = "&File";
            // 
            // miNewProject
            // 
            this.miNewProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.NewProject;
            this.miNewProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miNewProject.Name = "miNewProject";
            this.miNewProject.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.miNewProject.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miNewProject, "Start a brand new project");
            this.miNewProject.Text = "&New Project";
            this.miNewProject.Click += new System.EventHandler(this.miNewProject_Click);
            // 
            // miNewFromOtherFormat
            // 
            this.miNewFromOtherFormat.Name = "miNewFromOtherFormat";
            this.miNewFromOtherFormat.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miNewFromOtherFormat, "Create a new Sandcastle Help File Builder project from an existing project that i" +
        "s in a different format");
            this.miNewFromOtherFormat.Text = "Ne&w Project from Other Format...";
            this.miNewFromOtherFormat.Click += new System.EventHandler(this.miNewFromOtherFormat_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(297, 6);
            // 
            // miOpenProject
            // 
            this.miOpenProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.OpenProject;
            this.miOpenProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miOpenProject.Name = "miOpenProject";
            this.miOpenProject.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.miOpenProject.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miOpenProject, "Open an existing project");
            this.miOpenProject.Text = "&Open Project...";
            this.miOpenProject.Click += new System.EventHandler(this.miOpenProject_Click);
            // 
            // miCloseProject
            // 
            this.miCloseProject.Name = "miCloseProject";
            this.miCloseProject.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCloseProject, "Close the project");
            this.miCloseProject.Text = "Clos&e Project";
            this.miCloseProject.Click += new System.EventHandler(this.miCloseProject_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(297, 6);
            // 
            // miClose
            // 
            this.miClose.Name = "miClose";
            this.miClose.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miClose, "Close the current window");
            this.miClose.Text = "&Close";
            this.miClose.Click += new System.EventHandler(this.miClose_Click);
            // 
            // miCloseAll
            // 
            this.miCloseAll.Name = "miCloseAll";
            this.miCloseAll.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCloseAll, "Close all open content editors");
            this.miCloseAll.Text = "C&lose All Content Editors";
            this.miCloseAll.Click += new System.EventHandler(this.miCloseAll_Click);
            // 
            // miCloseAllButCurrent
            // 
            this.miCloseAllButCurrent.Name = "miCloseAllButCurrent";
            this.miCloseAllButCurrent.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCloseAllButCurrent, "Close all but the current content editor");
            this.miCloseAllButCurrent.Text = "Close All &But Current Content Editor";
            this.miCloseAllButCurrent.Click += new System.EventHandler(this.miCloseAll_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(297, 6);
            // 
            // miProjectExplorer
            // 
            this.miProjectExplorer.Name = "miProjectExplorer";
            this.miProjectExplorer.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miProjectExplorer, "Project Explorer options");
            this.miProjectExplorer.Text = "&Project Explorer";
            // 
            // miExplorerSeparator
            // 
            this.miExplorerSeparator.Name = "miExplorerSeparator";
            this.miExplorerSeparator.Size = new System.Drawing.Size(297, 6);
            // 
            // miSave
            // 
            this.miSave.Image = global::SandcastleBuilder.Gui.Properties.Resources.SaveProject;
            this.miSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miSave.Name = "miSave";
            this.miSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.miSave.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miSave, "Save all changes to the current item");
            this.miSave.Text = "&Save";
            this.miSave.Click += new System.EventHandler(this.miSave_Click);
            // 
            // miSaveAs
            // 
            this.miSaveAs.Name = "miSaveAs";
            this.miSaveAs.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miSaveAs, "Save the current item under a new name");
            this.miSaveAs.Text = "Save &As...";
            this.miSaveAs.Click += new System.EventHandler(this.miSaveAs_Click);
            // 
            // miSaveAll
            // 
            this.miSaveAll.Image = global::SandcastleBuilder.Gui.Properties.Resources.SaveAll;
            this.miSaveAll.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miSaveAll.Name = "miSaveAll";
            this.miSaveAll.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.miSaveAll.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miSaveAll, "Save all open items and the project");
            this.miSaveAll.Text = "Sa&ve All";
            this.miSaveAll.Click += new System.EventHandler(this.miSaveAll_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(297, 6);
            // 
            // miRecentProjects
            // 
            this.miRecentProjects.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.miRecentProjects.Name = "miRecentProjects";
            this.miRecentProjects.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miRecentProjects, "Select a recent project to load");
            this.miRecentProjects.Text = "&Recent Projects";
            this.miRecentProjects.DropDownOpening += new System.EventHandler(this.miRecentProjects_DropDownOpening);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(125, 22);
            this.toolStripMenuItem1.Text = "(Empty)";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(297, 6);
            // 
            // miUserPreferences
            // 
            this.miUserPreferences.Name = "miUserPreferences";
            this.miUserPreferences.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miUserPreferences, "Modify user preferences");
            this.miUserPreferences.Text = "&User Preferences...";
            this.miUserPreferences.Click += new System.EventHandler(this.miUserPreferences_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(297, 6);
            // 
            // miExit
            // 
            this.miExit.Name = "miExit";
            this.miExit.Size = new System.Drawing.Size(300, 22);
            this.sbStatusBarText.SetStatusBarText(this.miExit, "Exit this application");
            this.miExit.Text = "E&xit";
            this.miExit.Click += new System.EventHandler(this.miExit_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHelp,
            this.miFaq,
            this.toolStripSeparator8,
            this.miAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(49, 21);
            this.sbStatusBarText.SetStatusBarText(this.helpToolStripMenuItem, "Help related options");
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // miHelp
            // 
            this.miHelp.Name = "miHelp";
            this.miHelp.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.miHelp.Size = new System.Drawing.Size(306, 22);
            this.sbStatusBarText.SetStatusBarText(this.miHelp, "View the help file");
            this.miHelp.Text = "&Help";
            this.miHelp.Click += new System.EventHandler(this.miHelp_Click);
            // 
            // miFaq
            // 
            this.miFaq.Image = global::SandcastleBuilder.Gui.Properties.Resources.FAQ;
            this.miFaq.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miFaq.Name = "miFaq";
            this.miFaq.Size = new System.Drawing.Size(306, 22);
            this.sbStatusBarText.SetStatusBarText(this.miFaq, "View frequently asked questions");
            this.miFaq.Text = "&Frequently Asked Questions";
            this.miFaq.Click += new System.EventHandler(this.miHelp_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(303, 6);
            // 
            // miAbout
            // 
            this.miAbout.Image = global::SandcastleBuilder.Gui.Properties.Resources.About;
            this.miAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAbout.Name = "miAbout";
            this.miAbout.Size = new System.Drawing.Size(306, 22);
            this.sbStatusBarText.SetStatusBarText(this.miAbout, "View application, system, and contact information");
            this.miAbout.Text = "&About Sandcastle Help File Builder...";
            this.miAbout.Click += new System.EventHandler(this.miAbout_Click);
            // 
            // miDocumentation
            // 
            this.miDocumentation.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miBuildProject,
            this.miCancelBuild,
            this.toolStripSeparator5,
            this.miViewHelp,
            this.toolStripSeparator9,
            this.miCleanOutput});
            this.miDocumentation.Name = "miDocumentation";
            this.miDocumentation.Size = new System.Drawing.Size(115, 21);
            this.sbStatusBarText.SetStatusBarText(this.miDocumentation, "Documentation related options");
            this.miDocumentation.Text = "&Documentation";
            // 
            // miBuildProject
            // 
            this.miBuildProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.BuildProject;
            this.miBuildProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miBuildProject.Name = "miBuildProject";
            this.miBuildProject.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.B)));
            this.miBuildProject.Size = new System.Drawing.Size(237, 22);
            this.sbStatusBarText.SetStatusBarText(this.miBuildProject, "Build the current project to produce a help file");
            this.miBuildProject.Text = "&Build Project";
            this.miBuildProject.Click += new System.EventHandler(this.miBuildProject_Click);
            // 
            // miCancelBuild
            // 
            this.miCancelBuild.Enabled = false;
            this.miCancelBuild.Image = global::SandcastleBuilder.Gui.Properties.Resources.CancelBuild;
            this.miCancelBuild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miCancelBuild.Name = "miCancelBuild";
            this.miCancelBuild.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.miCancelBuild.Size = new System.Drawing.Size(237, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCancelBuild, "Cancel the current build process");
            this.miCancelBuild.Text = "&Cancel Build";
            this.miCancelBuild.Click += new System.EventHandler(this.miCancelBuild_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(234, 6);
            // 
            // miViewHelp
            // 
            this.miViewHelp.DropDown = this.ctxViewHelpMenu;
            this.miViewHelp.Name = "miViewHelp";
            this.miViewHelp.Size = new System.Drawing.Size(237, 22);
            this.sbStatusBarText.SetStatusBarText(this.miViewHelp, "View the help file produced by the last build");
            this.miViewHelp.Text = "&View Help File";
            // 
            // ctxViewHelpMenu
            // 
            this.ctxViewHelpMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miViewHelpFile,
            this.toolStripSeparator10,
            this.miViewHtmlHelp1,
            this.toolStripSeparator16,
            this.miViewMSHelp2,
            this.toolStripSeparator17,
            this.miViewMSHelpViewer,
            this.miLaunchHlm,
            this.toolStripSeparator18,
            this.miViewAspNetWebsite,
            this.miViewHtmlWebsite,
            this.toolStripSeparator11,
            this.miOpenHelpAfterBuild});
            this.ctxViewHelpMenu.Name = "ctxViewHelpMenu";
            this.ctxViewHelpMenu.OwnerItem = this.miViewHelp;
            this.ctxViewHelpMenu.Size = new System.Drawing.Size(315, 248);
            this.ctxViewHelpMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ctxViewHelpMenu_Opening);
            // 
            // miViewHelpFile
            // 
            this.miViewHelpFile.Image = global::SandcastleBuilder.Gui.Properties.Resources.ViewHelpFile;
            this.miViewHelpFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miViewHelpFile.Name = "miViewHelpFile";
            this.miViewHelpFile.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.V)));
            this.miViewHelpFile.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miViewHelpFile, "View help file using first available format");
            this.miViewHelpFile.Text = "&View Help File";
            this.miViewHelpFile.Click += new System.EventHandler(this.miViewHelpFile_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(311, 6);
            // 
            // miViewHtmlHelp1
            // 
            this.miViewHtmlHelp1.Name = "miViewHtmlHelp1";
            this.miViewHtmlHelp1.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miViewHtmlHelp1, "View HTML Help 1 (.chm) help file");
            this.miViewHtmlHelp1.Text = "View &HTML Help 1 (.chm) File";
            this.miViewHtmlHelp1.Click += new System.EventHandler(this.miViewBuiltHelpFile_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(311, 6);
            // 
            // miViewMSHelp2
            // 
            this.miViewMSHelp2.Name = "miViewMSHelp2";
            this.miViewMSHelp2.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miViewMSHelp2, "View MS Help 2 (.HxS) help file");
            this.miViewMSHelp2.Text = "View &MS Help 2 (.HxS) File";
            this.miViewMSHelp2.Click += new System.EventHandler(this.miViewBuiltHelpFile_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(311, 6);
            // 
            // miViewMSHelpViewer
            // 
            this.miViewMSHelpViewer.Name = "miViewMSHelpViewer";
            this.miViewMSHelpViewer.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miViewMSHelpViewer, "View MS Help Viewer (.mshc) help file");
            this.miViewMSHelpViewer.Text = "View M&S Help Viewer (.mshc) File";
            this.miViewMSHelpViewer.Click += new System.EventHandler(this.miViewMSHelpViewer_Click);
            // 
            // miLaunchHlm
            // 
            this.miLaunchHlm.Name = "miLaunchHlm";
            this.miLaunchHlm.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miLaunchHlm, "Launch the Help Library Manager for interactive use");
            this.miLaunchHlm.Text = "Launch Help Library Manager";
            this.miLaunchHlm.Click += new System.EventHandler(this.miLaunchHlm_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(311, 6);
            // 
            // miViewAspNetWebsite
            // 
            this.miViewAspNetWebsite.Name = "miViewAspNetWebsite";
            this.miViewAspNetWebsite.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miViewAspNetWebsite, "View the ASP.NET website using the ASP.NET Development Web Server");
            this.miViewAspNetWebsite.Text = "View Website (&ASP.NET)";
            this.miViewAspNetWebsite.Click += new System.EventHandler(this.miViewAspNetWebsite_Click);
            // 
            // miViewHtmlWebsite
            // 
            this.miViewHtmlWebsite.Name = "miViewHtmlWebsite";
            this.miViewHtmlWebsite.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miViewHtmlWebsite, "View the website using the basic HTML index page");
            this.miViewHtmlWebsite.Text = "View &Website (HTML)";
            this.miViewHtmlWebsite.Click += new System.EventHandler(this.miViewBuiltHelpFile_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(311, 6);
            // 
            // miOpenHelpAfterBuild
            // 
            this.miOpenHelpAfterBuild.Name = "miOpenHelpAfterBuild";
            this.miOpenHelpAfterBuild.Size = new System.Drawing.Size(314, 24);
            this.sbStatusBarText.SetStatusBarText(this.miOpenHelpAfterBuild, "Check this option to automatically open the help file after a successful build");
            this.miOpenHelpAfterBuild.Text = "&Open help file after successful build";
            this.miOpenHelpAfterBuild.Click += new System.EventHandler(this.miOpenHelpAfterBuild_Click);
            // 
            // tsbViewHelpFile
            // 
            this.tsbViewHelpFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbViewHelpFile.DropDown = this.ctxViewHelpMenu;
            this.tsbViewHelpFile.Image = global::SandcastleBuilder.Gui.Properties.Resources.ViewHelpFile;
            this.tsbViewHelpFile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbViewHelpFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbViewHelpFile.Name = "tsbViewHelpFile";
            this.tsbViewHelpFile.Size = new System.Drawing.Size(32, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbViewHelpFile, "View the help file produced by the last build");
            this.tsbViewHelpFile.ToolTipText = "View help file from last build";
            this.tsbViewHelpFile.ButtonClick += new System.EventHandler(this.miViewHelpFile_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(234, 6);
            // 
            // miCleanOutput
            // 
            this.miCleanOutput.Name = "miCleanOutput";
            this.miCleanOutput.Size = new System.Drawing.Size(237, 22);
            this.sbStatusBarText.SetStatusBarText(this.miCleanOutput, "Clean the output folder by deleting all files in it");
            this.miCleanOutput.Text = "Clea&n Output Folder";
            this.miCleanOutput.Click += new System.EventHandler(this.miCleanOutput_Click);
            // 
            // tcbConfig
            // 
            this.tcbConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tcbConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcbConfig.Name = "tcbConfig";
            this.tcbConfig.Size = new System.Drawing.Size(121, 32);
            this.sbStatusBarText.SetStatusBarText(this.tcbConfig, "Select the configuration for the build");
            this.tcbConfig.SelectedIndexChanged += new System.EventHandler(this.tcbConfig_SelectedIndexChanged);
            // 
            // tcbPlatform
            // 
            this.tcbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tcbPlatform.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcbPlatform.Name = "tcbPlatform";
            this.tcbPlatform.Size = new System.Drawing.Size(121, 32);
            this.sbStatusBarText.SetStatusBarText(this.tcbPlatform, "Select the platform for the build");
            this.tcbPlatform.SelectedIndexChanged += new System.EventHandler(this.tcbPlatform_SelectedIndexChanged);
            // 
            // miWindow
            // 
            this.miWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miViewProjectExplorer,
            this.miViewProjectProperties,
            this.miEntityReferences,
            this.miPreviewTopic,
            this.toolStripMenuItem3,
            this.miViewOutput,
            this.miViewLog,
            this.miClearOutput,
            this.toolStripMenuItem4});
            this.miWindow.Name = "miWindow";
            this.miWindow.Size = new System.Drawing.Size(69, 21);
            this.sbStatusBarText.SetStatusBarText(this.miWindow, "Window list");
            this.miWindow.Text = "&Window";
            // 
            // miViewProjectExplorer
            // 
            this.miViewProjectExplorer.Image = global::SandcastleBuilder.Gui.Properties.Resources.Explorer;
            this.miViewProjectExplorer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miViewProjectExplorer.Name = "miViewProjectExplorer";
            this.miViewProjectExplorer.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.miViewProjectExplorer.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miViewProjectExplorer, "View the project explorer");
            this.miViewProjectExplorer.Text = "&Project Explorer";
            this.miViewProjectExplorer.Click += new System.EventHandler(this.miViewProjectExplorer_Click);
            // 
            // miViewProjectProperties
            // 
            this.miViewProjectProperties.Image = global::SandcastleBuilder.Gui.Properties.Resources.Properties;
            this.miViewProjectProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miViewProjectProperties.Name = "miViewProjectProperties";
            this.miViewProjectProperties.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.miViewProjectProperties.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miViewProjectProperties, "View project properties");
            this.miViewProjectProperties.Text = "Project P&roperties";
            this.miViewProjectProperties.Click += new System.EventHandler(this.miViewProjectProperties_Click);
            // 
            // miEntityReferences
            // 
            this.miEntityReferences.Image = global::SandcastleBuilder.Gui.Properties.Resources.CodeFile;
            this.miEntityReferences.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miEntityReferences.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miEntityReferences.Name = "miEntityReferences";
            this.miEntityReferences.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.miEntityReferences.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miEntityReferences, "Open the entity references window");
            this.miEntityReferences.Text = "&Entity References";
            this.miEntityReferences.Click += new System.EventHandler(this.miEntityReferences_Click);
            // 
            // miPreviewTopic
            // 
            this.miPreviewTopic.Name = "miPreviewTopic";
            this.miPreviewTopic.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.miPreviewTopic.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miPreviewTopic, "Preview the topic being edited");
            this.miPreviewTopic.Text = "Preview &Topic";
            this.miPreviewTopic.Click += new System.EventHandler(this.miPreviewTopic_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(288, 6);
            // 
            // miViewOutput
            // 
            this.miViewOutput.Image = global::SandcastleBuilder.Gui.Properties.Resources.ViewLog;
            this.miViewOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miViewOutput.Name = "miViewOutput";
            this.miViewOutput.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.W)));
            this.miViewOutput.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miViewOutput, "View the last build\'s output");
            this.miViewOutput.Text = "Build &Output";
            this.miViewOutput.Click += new System.EventHandler(this.miViewOutput_Click);
            // 
            // miViewLog
            // 
            this.miViewLog.Name = "miViewLog";
            this.miViewLog.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miViewLog, "View the last build\'s log file");
            this.miViewLog.Text = "Build &Log Content";
            this.miViewLog.Click += new System.EventHandler(this.miViewOutput_Click);
            // 
            // miClearOutput
            // 
            this.miClearOutput.Name = "miClearOutput";
            this.miClearOutput.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.X)));
            this.miClearOutput.Size = new System.Drawing.Size(291, 22);
            this.sbStatusBarText.SetStatusBarText(this.miClearOutput, "Clear the last build information from the output window");
            this.miClearOutput.Text = "&Clear Output Window";
            this.miClearOutput.Click += new System.EventHandler(this.miClearOutput_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(288, 6);
            // 
            // tsbNewProject
            // 
            this.tsbNewProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNewProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.NewProject;
            this.tsbNewProject.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbNewProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewProject.Name = "tsbNewProject";
            this.tsbNewProject.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbNewProject, "Start a brand new project");
            this.tsbNewProject.ToolTipText = "Start a brand new project";
            this.tsbNewProject.Click += new System.EventHandler(this.miNewProject_Click);
            // 
            // tsbOpenProject
            // 
            this.tsbOpenProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpenProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.OpenProject;
            this.tsbOpenProject.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbOpenProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenProject.Name = "tsbOpenProject";
            this.tsbOpenProject.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbOpenProject, "Open an existing project");
            this.tsbOpenProject.ToolTipText = "Open an existing project";
            this.tsbOpenProject.Click += new System.EventHandler(this.miOpenProject_Click);
            // 
            // tsbSave
            // 
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSave.Image = global::SandcastleBuilder.Gui.Properties.Resources.SaveProject;
            this.tsbSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbSave, "Save all changes to the current item");
            this.tsbSave.ToolTipText = "Save the current item";
            this.tsbSave.Click += new System.EventHandler(this.miSave_Click);
            // 
            // tsbSaveAll
            // 
            this.tsbSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSaveAll.Image = global::SandcastleBuilder.Gui.Properties.Resources.SaveAll;
            this.tsbSaveAll.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSaveAll.Name = "tsbSaveAll";
            this.tsbSaveAll.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbSaveAll, "Save all items and the project");
            this.tsbSaveAll.ToolTipText = "Save all items and the project";
            this.tsbSaveAll.Click += new System.EventHandler(this.miSaveAll_Click);
            // 
            // tsbProjectExplorer
            // 
            this.tsbProjectExplorer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbProjectExplorer.Image = global::SandcastleBuilder.Gui.Properties.Resources.Explorer;
            this.tsbProjectExplorer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbProjectExplorer.Name = "tsbProjectExplorer";
            this.tsbProjectExplorer.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbProjectExplorer, "View the project explorer");
            this.tsbProjectExplorer.ToolTipText = "View project explorer";
            this.tsbProjectExplorer.Click += new System.EventHandler(this.miViewProjectExplorer_Click);
            // 
            // tsbProjectProperties
            // 
            this.tsbProjectProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbProjectProperties.Image = global::SandcastleBuilder.Gui.Properties.Resources.Properties;
            this.tsbProjectProperties.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbProjectProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbProjectProperties.Name = "tsbProjectProperties";
            this.tsbProjectProperties.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbProjectProperties, "View the project properties");
            this.tsbProjectProperties.ToolTipText = "View project properties";
            this.tsbProjectProperties.Click += new System.EventHandler(this.miViewProjectProperties_Click);
            // 
            // tsbCodeEntitySearch
            // 
            this.tsbCodeEntitySearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCodeEntitySearch.Image = global::SandcastleBuilder.Gui.Properties.Resources.CodeFile;
            this.tsbCodeEntitySearch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbCodeEntitySearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCodeEntitySearch.Name = "tsbCodeEntitySearch";
            this.tsbCodeEntitySearch.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbCodeEntitySearch, "Insert token, image, and code entity/snippet references");
            this.tsbCodeEntitySearch.ToolTipText = "Token, image, and code entity/snippet references";
            this.tsbCodeEntitySearch.Click += new System.EventHandler(this.miEntityReferences_Click);
            // 
            // tsbPreviewTopic
            // 
            this.tsbPreviewTopic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPreviewTopic.Image = global::SandcastleBuilder.Gui.Properties.Resources.TopicPreview;
            this.tsbPreviewTopic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbPreviewTopic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPreviewTopic.Name = "tsbPreviewTopic";
            this.tsbPreviewTopic.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbPreviewTopic, "Preview the topic being edited");
            this.tsbPreviewTopic.ToolTipText = "Preview topic";
            this.tsbPreviewTopic.Click += new System.EventHandler(this.miPreviewTopic_Click);
            // 
            // tsbBuildProject
            // 
            this.tsbBuildProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBuildProject.Image = global::SandcastleBuilder.Gui.Properties.Resources.BuildProject;
            this.tsbBuildProject.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbBuildProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBuildProject.Name = "tsbBuildProject";
            this.tsbBuildProject.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbBuildProject, "Build the current project to produce a help file");
            this.tsbBuildProject.ToolTipText = "Build the help file";
            this.tsbBuildProject.Click += new System.EventHandler(this.miBuildProject_Click);
            // 
            // tsbCancelBuild
            // 
            this.tsbCancelBuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCancelBuild.Enabled = false;
            this.tsbCancelBuild.Image = global::SandcastleBuilder.Gui.Properties.Resources.CancelBuild;
            this.tsbCancelBuild.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbCancelBuild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCancelBuild.Name = "tsbCancelBuild";
            this.tsbCancelBuild.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbCancelBuild, "Cancel the current build process");
            this.tsbCancelBuild.ToolTipText = "Cancel the current build process";
            this.tsbCancelBuild.Click += new System.EventHandler(this.miCancelBuild_Click);
            // 
            // tsbViewOutput
            // 
            this.tsbViewOutput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbViewOutput.Image = global::SandcastleBuilder.Gui.Properties.Resources.ViewLog;
            this.tsbViewOutput.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbViewOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbViewOutput.Name = "tsbViewOutput";
            this.tsbViewOutput.Size = new System.Drawing.Size(23, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbViewOutput, "View output from last build");
            this.tsbViewOutput.ToolTipText = "View build output";
            this.tsbViewOutput.Click += new System.EventHandler(this.miViewOutput_Click);
            // 
            // tsbFaq
            // 
            this.tsbFaq.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFaq.Image = global::SandcastleBuilder.Gui.Properties.Resources.FAQ;
            this.tsbFaq.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbFaq.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFaq.Name = "tsbFaq";
            this.tsbFaq.Size = new System.Drawing.Size(24, 29);
            this.sbStatusBarText.SetStatusBarText(this.tsbFaq, "View frequently asked questions");
            this.tsbFaq.ToolTipText = "View frequently asked questions";
            this.tsbFaq.Click += new System.EventHandler(this.miHelp_Click);
            // 
            // tsbAbout
            // 
            this.tsbAbout.AutoSize = false;
            this.tsbAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAbout.Image = global::SandcastleBuilder.Gui.Properties.Resources.About;
            this.tsbAbout.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAbout.Name = "tsbAbout";
            this.tsbAbout.Size = new System.Drawing.Size(23, 22);
            this.sbStatusBarText.SetStatusBarText(this.tsbAbout, "View application, system, and contact information");
            this.tsbAbout.ToolTipText = "View application, system, and contact information";
            this.tsbAbout.Click += new System.EventHandler(this.miAbout_Click);
            // 
            // mnuMain
            // 
            this.mnuMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miProject,
            this.miDocumentation,
            this.miWindow,
            this.helpToolStripMenuItem});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.MdiWindowListItem = this.miWindow;
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(1016, 25);
            this.mnuMain.TabIndex = 0;
            // 
            // tsbMain
            // 
            this.tsbMain.AutoSize = false;
            this.tsbMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.tsbMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsbMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewProject,
            this.tsbOpenProject,
            this.toolStripSeparator13,
            this.tsbSave,
            this.tsbSaveAll,
            this.toolStripSeparator15,
            this.tsbProjectExplorer,
            this.tsbProjectProperties,
            this.tsbCodeEntitySearch,
            this.tsbPreviewTopic,
            this.toolStripSeparator3,
            this.tcbConfig,
            this.tcbPlatform,
            this.toolStripSeparator12,
            this.tsbBuildProject,
            this.tsbCancelBuild,
            this.tsbViewOutput,
            this.tsbViewHelpFile,
            this.toolStripSeparator4,
            this.tsbFaq,
            this.tsbAbout});
            this.tsbMain.Location = new System.Drawing.Point(0, 25);
            this.tsbMain.Name = "tsbMain";
            this.tsbMain.Size = new System.Drawing.Size(1016, 32);
            this.tsbMain.TabIndex = 1;
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 32);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(6, 32);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 32);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 32);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 32);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslStatusText,
            this.tsslProgressNote,
            this.tspbProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 713);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1016, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslStatusText
            // 
            this.tsslStatusText.Margin = new System.Windows.Forms.Padding(0);
            this.tsslStatusText.Name = "tsslStatusText";
            this.tsslStatusText.Size = new System.Drawing.Size(895, 22);
            this.tsslStatusText.Spring = true;
            this.tsslStatusText.Text = "Ready";
            this.tsslStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsslProgressNote
            // 
            this.tsslProgressNote.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.tsslProgressNote.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.tsslProgressNote.Margin = new System.Windows.Forms.Padding(0);
            this.tsslProgressNote.Name = "tsslProgressNote";
            this.tsslProgressNote.Size = new System.Drawing.Size(4, 22);
            this.tsslProgressNote.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tspbProgressBar
            // 
            this.tspbProgressBar.Name = "tspbProgressBar";
            this.tspbProgressBar.Size = new System.Drawing.Size(100, 16);
            this.tspbProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // dockPanel
            // 
            this.dockPanel.ActiveAutoHideContent = null;
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.Location = new System.Drawing.Point(0, 57);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.Size = new System.Drawing.Size(1016, 656);
            this.dockPanel.TabIndex = 7;
            this.dockPanel.ContentAdded += new System.EventHandler<WeifenLuo.WinFormsUI.Docking.DockContentEventArgs>(this.dockPanel_ContentAdded);
            this.dockPanel.ContentRemoved += new System.EventHandler<WeifenLuo.WinFormsUI.Docking.DockContentEventArgs>(this.dockPanel_ContentRemoved);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(1016, 735);
            this.Controls.Add(this.dockPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tsbMain);
            this.Controls.Add(this.mnuMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.mnuMain;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.Text = "Sandcastle Help File Builder";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ctxViewHelpMenu.ResumeLayout(false);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.tsbMain.ResumeLayout(false);
            this.tsbMain.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbStatusBarText;
        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem miProject;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStrip tsbMain;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton tsbAbout;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatusText;
        private System.Windows.Forms.ToolStripStatusLabel tsslProgressNote;
        private System.Windows.Forms.ToolStripProgressBar tspbProgressBar;
        private System.Windows.Forms.ToolStripMenuItem miNewProject;
        private System.Windows.Forms.ToolStripMenuItem miOpenProject;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem miSave;
        private System.Windows.Forms.ToolStripMenuItem miSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem miDocumentation;
        private System.Windows.Forms.ToolStripMenuItem miBuildProject;
        private System.Windows.Forms.ToolStripMenuItem miViewHelp;
        private System.Windows.Forms.ToolStripButton tsbNewProject;
        private System.Windows.Forms.ToolStripButton tsbOpenProject;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbBuildProject;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem miAbout;
        private System.Windows.Forms.ToolStripButton tsbCancelBuild;
        private System.Windows.Forms.ToolStripMenuItem miCancelBuild;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton tsbViewOutput;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem miRecentProjects;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator miExplorerSeparator;
        private System.Windows.Forms.ToolStripMenuItem miNewFromOtherFormat;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem miCleanOutput;
        private System.Windows.Forms.ToolStripButton tsbFaq;
        private System.Windows.Forms.ToolStripMenuItem miFaq;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem miUserPreferences;
        private System.Windows.Forms.ToolStripSplitButton tsbViewHelpFile;
        private System.Windows.Forms.ContextMenuStrip ctxViewHelpMenu;
        private System.Windows.Forms.ToolStripMenuItem miViewHelpFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem miViewHtmlHelp1;
        private System.Windows.Forms.ToolStripMenuItem miViewMSHelp2;
        private System.Windows.Forms.ToolStripMenuItem miViewAspNetWebsite;
        private System.Windows.Forms.ToolStripMenuItem miViewHtmlWebsite;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem miOpenHelpAfterBuild;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripComboBox tcbConfig;
        private System.Windows.Forms.ToolStripComboBox tcbPlatform;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private System.Windows.Forms.ToolStripMenuItem miWindow;
        private System.Windows.Forms.ToolStripMenuItem miViewProjectExplorer;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem miViewOutput;
        private System.Windows.Forms.ToolStripMenuItem miViewLog;
        private System.Windows.Forms.ToolStripMenuItem miClearOutput;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem miCloseProject;
        private System.Windows.Forms.ToolStripMenuItem miViewProjectProperties;
        private System.Windows.Forms.ToolStripButton tsbProjectProperties;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem miProjectExplorer;
        private System.Windows.Forms.ToolStripMenuItem miEntityReferences;
        private System.Windows.Forms.ToolStripMenuItem miPreviewTopic;
        private System.Windows.Forms.ToolStripMenuItem miSaveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem miClose;
        private System.Windows.Forms.ToolStripMenuItem miCloseAllButCurrent;
        private System.Windows.Forms.ToolStripMenuItem miCloseAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripButton tsbSaveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripButton tsbCodeEntitySearch;
        private System.Windows.Forms.ToolStripButton tsbProjectExplorer;
        private System.Windows.Forms.ToolStripButton tsbPreviewTopic;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripMenuItem miViewMSHelpViewer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private System.Windows.Forms.ToolStripMenuItem miLaunchHlm;
    }
}

