//=============================================================================
// System  : Sandcastle Help File Builder
// File    : MainForm.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/26/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the main form for the application.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/02/2006  EFW  Created the code
// 1.3.1.2  10/18/2006  EFW  Moved file logging to the BuildProcess class.
//                           Added a Verbose Logging user setting.
// 1.5.0.0  06/23/2007  EFW  Added user preferences dialog box and help file
//                           viewer options.
// 1.5.0.2  07/03/2007  EFW  Added support for content file editor definitions
//                           and a content site map file.
// 1.6.0.2  11/02/2007  EFW  Reworked support for custom build components
// 1.6.0.3  11/16/2007  EFW  Added "Open After Build" option
// 1.6.0.4  01/22/2008  EFW  Added support for drag and drop from Explorer
// 1.6.0.7  04/24/2008  EFW  Added support for conceptual content
// 1.8.0.0  06/20/2008  EFW  Converted the project to use an MSBuild file.
//                           Changed the UI to a more Visual Studio-like
//                           layout better suited to editing the project.
// 1.9.0.0  07/05/2010  EFW  Added support for MS Help Viewer
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Build.Exceptions;

using SandcastleBuilder.Gui.ContentEditors;
using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.MicrosoftHelpViewer;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.Controls;
using SandcastleBuilder.Utils.Design;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This is the main form for the application
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject project;
        private Thread buildThread;
        private BuildProcess buildProcess;
        private Process webServer;
        private ProjectExplorerWindow projectExplorer;
        private ProjectPropertiesWindow projectProperties;
        private OutputWindow outputWindow;
        private EntityReferenceWindow entityReferencesWindow;
        private PreviewTopicWindow previewWindow;
        private FileSystemWatcher fsw;
        private List<string> changedFiles;
        private bool checkingForChangedFiles;

        private static MainForm mainForm;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Return a reference to the main form
        /// </summary>
        /// <remarks>This is used to let the various panes communicate between
        /// each other.  There are probably several different ways to do this
        /// that are better but this is quick and dirty and it works.</remarks>
        public static MainForm Host
        {
            get { return mainForm; }
        }

        /// <summary>
        /// Get a reference to the Project Explorer pane
        /// </summary>
        public ProjectExplorerWindow ProjectExplorer
        {
            get { return projectExplorer; }
        }

        /// <summary>
        /// Get a reference to the Project Explorer menu item
        /// </summary>
        public ToolStripMenuItem ProjectExplorerMenu
        {
            get { return miProjectExplorer; }
        }

        /// <summary>
        /// Get a reference to the status strip label used for status bar text
        /// </summary>
        public ToolStripStatusLabel StatusBarTextLabel
        {
            get { return tsslStatusText; }
        }

        /// <summary>
        /// This returns a suffix to use as the project's window state
        /// settings file suffix.
        /// </summary>
        /// <value>Settings files are saved per user.</value>
        private static string WindowStateSuffix
        {
            get
            {
                string userName = Environment.ExpandEnvironmentVariables("%USERNAME%");

                if(String.IsNullOrEmpty(userName) || userName[0] == '%')
                    userName = "DefaultUser";

                return "_" + userName;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectToLoad">A default project file to load.  If
        /// not specified, the most recently used project is loaded if there
        /// is one.</param>
        public MainForm(string projectToLoad)
        {
            mainForm = this;
            InitializeComponent();

            // Skip the rest of the initialization in design mode
            if(this.DesignMode)
                return;

            // We are only going to monitor for file changes.  We won't handle moves, deletes, or renames.
            changedFiles = new List<string>();
            fsw = new FileSystemWatcher();
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.IncludeSubdirectories = true;
            fsw.Changed += fsw_OnChanged;

            if(Settings.Default.ContentFileEditors == null)
                Settings.Default.ContentFileEditors = new ContentFileEditorCollection();

            // Add the editors to the global list
            ContentFileEditorCollection.GlobalEditors.AddRange(Settings.Default.ContentFileEditors);

            if(projectToLoad != null)
            {
                if(Settings.Default.MruList == null)
                    Settings.Default.MruList = new StringCollection();

                if(File.Exists(projectToLoad))
                    MainForm.UpdateMruList(projectToLoad);
                else
                    MessageBox.Show("Unable to find project: " + projectToLoad,
                        Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }

            // Set the status label to use for status bar text
            StatusBarTextProvider.ApplicationStatusBar = tsslStatusText;

            // Define the status label and progress bar too.  This allows
            // easy access to those items from anywhere within the
            // application.
            StatusBarTextProvider.StatusLabel = tsslProgressNote;
            StatusBarTextProvider.ProgressBar = tspbProgressBar;
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// Deserialize the state of a content pane.
        /// </summary>
        /// <param name="persistString">The name of the item being deserialized</param>
        /// <returns>The dock content if deserialized or null if not.</returns>
        private IDockContent DeserializeState(string persistString)
        {
            if(persistString == typeof(ProjectExplorerWindow).FullName)
                return projectExplorer;

            if(persistString == typeof(ProjectPropertiesWindow).FullName)
                return projectProperties;

            if(persistString == typeof(OutputWindow).FullName)
            {
                outputWindow = new OutputWindow();
                return outputWindow;
            }

            if(persistString == typeof(EntityReferenceWindow).FullName)
            {
                entityReferencesWindow = new EntityReferenceWindow(tsslStatusText);
                entityReferencesWindow.CurrentProject = project;
                return entityReferencesWindow;
            }

            // Ignore the preview window
            if(persistString == typeof(PreviewTopicWindow).FullName)
                return null;

            string[] parsedStrings = persistString.Split(new char[] { ',' });

            if(projectExplorer == null || parsedStrings.Length != 2 || parsedStrings[1].Length == 0)
                return null;

            // Try for a built-in editor
            return projectExplorer.CreateFileEditor(parsedStrings[1], null);
        }

        /// <summary>
        /// Create a new project instance and connect it to the UI
        /// </summary>
        /// <param name="projectName">The project filename</param>
        /// <param name="mustExist">True if it must exist or false if it is
        /// a new, unnamed project</param>
        private void CreateProject(string projectName, bool mustExist)
        {
            List<string> values;

            project = new SandcastleProject(projectName, mustExist);
            project.DocumentationSourcesChanged += new EventHandler(project_Modified);
            project.DirtyChanged += new EventHandler(project_Modified);

            projectExplorer.CurrentProject = projectProperties.CurrentProject = project;

            if(entityReferencesWindow != null)
                entityReferencesWindow.CurrentProject = project;

            this.project_Modified(this, EventArgs.Empty);

            // Get the configuration and platform values
            values = project.MSBuildProject.ConditionedProperties["Configuration"];

            tcbConfig.Items.Clear();

            foreach(string value in values)
                tcbConfig.Items.Add(value);

            if(tcbConfig.Items.Count == 0)
            {
                tcbConfig.Items.Add("Debug");
                tcbConfig.Items.Add("Release");
            }

            values = project.MSBuildProject.ConditionedProperties["Platform"];

            tcbPlatform.Items.Clear();

            foreach(string value in values)
                tcbPlatform.Items.Add(value);

            if(tcbPlatform.Items.Count == 0)
                tcbPlatform.Items.Add("AnyCPU");

            if(tcbConfig.Items.Contains(Settings.Default.LastConfig))
                tcbConfig.SelectedItem = Settings.Default.LastConfig;
            else
                tcbConfig.SelectedIndex = 0;

            if(tcbPlatform.Items.Contains(Settings.Default.LastPlatform))
                tcbPlatform.SelectedItem = Settings.Default.LastPlatform;
            else
                tcbPlatform.SelectedIndex = 0;

            miDocumentation.Visible = miCloseProject.Enabled =
                miClose.Enabled = miSave.Enabled = miSaveAs.Enabled =
                tsbSave.Enabled = tsbSaveAll.Enabled = miSaveAll.Enabled =
                miProjectExplorer.Visible = miExplorerSeparator.Visible =
                tsbBuildProject.Enabled = tsbViewHelpFile.Enabled = true;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            if(Settings.Default.PerUserProjectState && File.Exists(project.Filename + WindowStateSuffix))
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    dockPanel.SuspendLayout(true);
                    projectExplorer.DockPanel = projectProperties.DockPanel = null;

                    if(outputWindow != null)
                        outputWindow.DockPanel = null;

                    if(entityReferencesWindow != null)
                        entityReferencesWindow.DockPanel = null;

                    if(previewWindow != null)
                        previewWindow.DockPanel = null;

                    dockPanel.LoadFromXml(project.Filename + WindowStateSuffix, DeserializeState);
                    dockPanel.ResumeLayout(true, true);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            else
            {
                miViewProjectExplorer_Click(this, EventArgs.Empty);
                miViewProjectProperties_Click(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This is used to save the project and/or document windows prior to
        /// doing a build.
        /// </summary>
        /// <returns>True if successful, false if it fails or is cancelled.</returns>
        private bool SaveBeforeBuild()
        {
            Collection<BaseContentEditor> filesToSave = new Collection<BaseContentEditor>();
            BaseContentEditor editor;
            DialogResult result;
            bool success = true;

            if(Settings.Default.BeforeBuild == BeforeBuildAction.DoNotSave)
                return true;

            // Content layout windows need saving too.  We'll prompt to save
            // these.
            foreach(IDockContent document in dockPanel.Contents)
            {
                editor = document as BaseContentEditor;

                if(editor != null && editor.CanSaveContent)
                {
                    switch(Settings.Default.BeforeBuild)
                    {
                        case BeforeBuildAction.SaveAllChanges:
                            success = editor.Save();
                            break;

                        case BeforeBuildAction.SaveOpenDocuments:
                            if(editor.IsContentDocument)
                                success = editor.Save();
                            break;

                        default:
                            if(editor.IsDirty)
                                filesToSave.Add(editor);
                            break;
                    }
                }

                if(!success)
                    break;
            }

            if(success && filesToSave.Count != 0)
                using(PromptToSaveDlg dlg = new PromptToSaveDlg(filesToSave))
                {
                    result = dlg.ShowDialog();

                    switch(result)
                    {
                        case DialogResult.Yes:  // Save all
                            foreach(BaseContentEditor file in filesToSave)
                            {
                                success = file.Save();

                                if(!success)
                                    break;
                            }
                            break;

                        case DialogResult.No:   // Don't save
                            break;

                        default:
                            success = false;
                            break;
                    }
                }

            return success;
        }

        /// <summary>
        /// Close the current project
        /// </summary>
        /// <returns>True if the project was closed, false if not</returns>
        private bool CloseProject()
        {
            BaseContentEditor content;
            IDockContent[] contents;

            if(projectExplorer.AskToSaveProject())
            {
                miClearOutput_Click(this, EventArgs.Empty);
                this.KillWebServer();

                // Dispose of the preview window to get rid of its temporary project and build files.
                // It also doesn't make much sense to save its state.
                if(previewWindow != null)
                {
                    previewWindow.Dispose();
                    previewWindow = null;
                }

                if(outputWindow != null)
                {
                    outputWindow.ResetLogViewer();
                    outputWindow.LogFile = null;
                }

                if(entityReferencesWindow != null)
                    entityReferencesWindow.CurrentProject = null;

                if(projectExplorer.CurrentProject != null && Settings.Default.PerUserProjectState)
                {
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        dockPanel.SaveAsXml(projectExplorer.CurrentProject.Filename + WindowStateSuffix);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }

                // Close all content editors
                contents = new IDockContent[dockPanel.Contents.Count];
                dockPanel.Contents.CopyTo(contents, 0);

                foreach(IDockContent dockContent in contents)
                {
                    content = dockContent as BaseContentEditor;

                    // If one can't close, don't close the project
                    if(content != null && !content.CanClose)
                    {
                        projectExplorer.Activate();
                        return false;
                    }

                    if(dockContent.DockHandler.HideOnClose)
                        dockContent.DockHandler.Hide();
                    else
                        dockContent.DockHandler.Close();
                }

                if(project != null)
                    project.Dispose();

                project = projectExplorer.CurrentProject = projectProperties.CurrentProject = null;
                this.UpdateFilenameInfo();

                miDocumentation.Visible = miCloseProject.Enabled =
                    miClose.Enabled = miCloseAll.Enabled =
                    miCloseAllButCurrent.Enabled = miSave.Enabled =
                    miSaveAs.Enabled = miSaveAll.Enabled = tsbSave.Enabled =
                    tsbSaveAll.Enabled = miProjectExplorer.Visible =
                    miExplorerSeparator.Visible = tsbBuildProject.Enabled =
                    tsbViewHelpFile.Enabled = miPreviewTopic.Enabled =
                    tsbPreviewTopic.Enabled = false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// This is used to update the filename information in the title bar
        /// </summary>
        private void UpdateFilenameInfo()
        {
            if(project == null)
                this.Text = Constants.AppName;
            else
                this.Text = String.Format(CultureInfo.CurrentCulture,
                    "{0}{1} - {2}",
                    Path.GetFileNameWithoutExtension(project.Filename),
                    (project.IsDirty) ? "*" : String.Empty, Constants.AppName);
        }

        /// <summary>
        /// This is used to enable or disable the UI elements for the
        /// build process
        /// </summary>
        /// <param name="enabled">True to enable the UI, false to disable
        /// it</param>
        private void SetUIEnabledState(bool enabled)
        {
            foreach(ToolStripMenuItem item in mnuMain.Items)
                foreach(ToolStripItem subItem in item.DropDownItems)
                    subItem.Enabled = enabled;

            foreach(ToolStripItem item in tsbMain.Items)
                item.Enabled = enabled;

            projectExplorer.Enabled = projectProperties.Enabled = enabled;

            if(outputWindow == null)
                miClearOutput_Click(miClearOutput, EventArgs.Empty);

            outputWindow.SetBuildState(!enabled);

            // The Cancel Build options are the inverse of the value
            miCancelBuild.Enabled = tsbCancelBuild.Enabled = !enabled;

            // These are always enabled even when building
            miHelp.Enabled = miFaq.Enabled = miAbout.Enabled = miExit.Enabled =
                tsbFaq.Enabled = tsbAbout.Enabled = true;
        }

        /// <summary>
        /// Update the MRU list with the specified project filename by
        /// adding it to the list or making it the most recently used.
        /// </summary>
        internal static void UpdateMruList(string projectFile)
        {
            StringCollection mruList = Settings.Default.MruList;

            projectFile = Path.GetFullPath(projectFile);

            if(mruList.Contains(projectFile))
                mruList.Remove(projectFile);

            mruList.Insert(0, projectFile);

            while(mruList.Count > 10)
                mruList.RemoveAt(9);
        }

        /// <summary>
        ///  Kill the web server if it is running
        /// </summary>
        private void KillWebServer()
        {
            try
            {
                if(webServer != null && !webServer.HasExited)
                    webServer.Kill();
            }
            catch(Exception ex)
            {
                // Ignore errors trying to kill the web server
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if(webServer != null)
                {
                    webServer.Dispose();
                    webServer = null;
                }
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Load the last used project on startup
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            StringCollection mruList = Settings.Default.MruList;
            string state;
            int idx = 0;

            projectExplorer = new ProjectExplorerWindow();
            projectProperties = new ProjectPropertiesWindow();

            // Set the current directory to My Documents so that people don't
            // save stuff in the SHFB folder by default.
            try
            {
                Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
            catch
            {
                // This doesn't always work if they use a mapped network drive
                // for My Documents.  In that case, just ignore the failure and
                // stay put.
            }

            // If not starting out minimized, attempt to load the last used
            // window size and position.
            if(this.WindowState != FormWindowState.Minimized)
            {
                WINDOWPLACEMENT wp = (WINDOWPLACEMENT)Settings.Default.WindowPlacement;

                if(wp.length == Marshal.SizeOf(typeof(WINDOWPLACEMENT)))
                {
                    wp.flags = 0;

                    if(wp.showCmd == UnsafeNativeMethods.SW_SHOWMINIMIZED)
                        wp.showCmd = UnsafeNativeMethods.SW_SHOWNORMAL;

                    UnsafeNativeMethods.SetWindowPlacement(this.Handle, ref wp);
                }
            }

            // Restore default content state if present and shift isn't held down
            state = Settings.Default.ContentEditorDockState;

            if(!String.IsNullOrEmpty(state) && (Control.ModifierKeys & Keys.Shift) == 0)
            {
                byte[] stateBytes = Encoding.UTF8.GetBytes(state.ToCharArray());

                using(MemoryStream ms = new MemoryStream(stateBytes))
                {
                    dockPanel.LoadFromXml(ms, DeserializeState);
                }
            }

            // If not restored or something when wrong and the project explorer
            // doesn't have a dock panel, show them by default.
            if(projectExplorer.DockPanel == null)
            {
                projectExplorer.Show(dockPanel);
                projectProperties.Show(dockPanel);
                projectExplorer.Focus();
            }
            
            try
            {
                // Check for the SHFBROOT environment variable.  It may not be
                // present yet if a reboot hasn't occurred after installation.
                // In such cases, set it to the proper folder for this process
                // so that projects can be loaded and built.
                if(Environment.GetEnvironmentVariable("SHFBROOT") == null)
                {
                    string shfbRootPath = Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly().Location);

                    MessageBox.Show("The SHFBROOT system environment variable was not " +
                        "found.  This variable is usually created during installation " +
                        "and may require a reboot.  It has been defined temporarily " +
                        "for this process as:\n\nSHFBROOT=" + shfbRootPath,
                        Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Environment.SetEnvironmentVariable("SHFBROOT", shfbRootPath);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to detect or set SHFBROOT environment " +
                    "variable.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create the MRU on first use
            if(mruList == null)
            {
                Settings.Default.MruList = new StringCollection();
                this.CloseProject();
            }
            else
            {
                // Get rid of projects that no longer exist or that are not
                // compatible
                while(idx < mruList.Count)
                    if(!File.Exists(mruList[idx]) || mruList[idx].EndsWith(
                      ".shfb", StringComparison.OrdinalIgnoreCase))
                        mruList.RemoveAt(idx);
                    else
                        idx++;

                if(mruList.Count == 0)
                    this.CloseProject();
                else
                    try
                    {
                        this.CreateProject(mruList[0], true);
                    }
                    catch(InvalidProjectFileException pex)
                    {
                        System.Diagnostics.Debug.Write(pex);

                        if(pex.Message.IndexOf("<project>", StringComparison.Ordinal) != -1)
                        {
                            MessageBox.Show("The project file format is invalid.  " +
                                "If this project was created with an earlier " +
                                "version of the Sandcastle Help File Builder, " +
                                "use the File | New Project from Other Format " +
                                "option to convert it to the latest project file " +
                                "format.", Constants.AppName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                            MessageBox.Show(pex.Message, Constants.AppName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                        MessageBox.Show(ex.Message, Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
            }
        }

        /// <summary>
        /// Save changes on exit if necessary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            BaseContentEditor content;
            string state;

            if(buildThread != null && buildThread.IsAlive)
            {
                if(MessageBox.Show("A build is currently taking place.  Do you want to abort it and exit?",
                  Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                miCancelBuild_Click(sender, e);
            }

            if(!projectExplorer.AskToSaveProject())
                e.Cancel = true;
            else
                foreach(IDockContent dockContent in dockPanel.Contents)
                {
                    content = dockContent as BaseContentEditor;

                    if(content != null && !content.CanClose)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

            if(!e.Cancel && previewWindow != null)
                e.Cancel = !previewWindow.CanClose;

            if(!e.Cancel)
            {
                // Save the current window size and position if possible
                WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));

                UnsafeNativeMethods.GetWindowPlacement(this.Handle, out wp);
                Settings.Default.WindowPlacement = wp;

                // Save the content state.  If open, hide some of the windows
                // so that they default to hidden when re-opened.
                if(outputWindow != null && outputWindow.Visible)
                    outputWindow.Hide();

                if(entityReferencesWindow != null && entityReferencesWindow.Visible)
                    entityReferencesWindow.Hide();

                // Dispose of the preview window to get rid of its temporary project and build files
                if(previewWindow != null)
                {
                    previewWindow.Dispose();
                    previewWindow = null;
                }

                // Save the default window state
                using(MemoryStream ms = new MemoryStream())
                {
                    dockPanel.SaveAsXml(ms, Encoding.UTF8);
                    state = Encoding.UTF8.GetString(ms.ToArray());
                    Settings.Default.ContentEditorDockState = state;
                }

                Settings.Default.Save();
                this.KillWebServer();

                // Save per user project state if wanted
                if(projectExplorer.CurrentProject != null && Settings.Default.PerUserProjectState)
                {
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        dockPanel.SaveAsXml(projectExplorer.CurrentProject.Filename + WindowStateSuffix);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Show information about the application and contact info
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAbout_Click(object sender, EventArgs e)
        {
            using(AboutDlg dlg = new AboutDlg())
            {
                dlg.ShowDialog();
            }
        }

        /// <summary>
        /// View the help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miHelp_Click(object sender, EventArgs e)
        {
            string topic, path = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            try
            {
#if DEBUG
                path += @"\..\..\..\Doc\Help\SandcastleBuilder.chm";
#else
                path += @"\SandcastleBuilder.chm";
#endif

                if(sender == miHelp || sender == tsbAbout)
                    topic = "html/bd1ddb51-1c4f-434f-bb1a-ce2135d3a909.htm";
                else
                    topic = "html/1aea789d-b226-4b39-b534-4c97c256fac8.htm";

                Form form = new Form();
                form.CreateControl();
                Help.ShowHelp(form, path, HelpNavigator.Topic, topic);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to open help file '{0}'.  Reason: {1}",
                    path, ex.Message), Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        #region Project and build event handlers
        //=====================================================================

        /// <summary>
        /// This updates the state of the form when the project is modified.
        /// It also updates the Sandcastle path in the component configuration
        /// form.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void project_Modified(object sender, EventArgs e)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new EventHandler(project_Modified), new object[] { sender, e });
                return;
            }

            string scPath = BuildComponentManager.SandcastlePath, prjPath = project.SandcastlePath;

            if((String.IsNullOrEmpty(scPath) && prjPath.Length != 0) ||
              (!String.IsNullOrEmpty(scPath) && prjPath.Length != 0 && scPath != prjPath))
                BuildComponentManager.SandcastlePath = prjPath;

            this.UpdateFilenameInfo();

            projectProperties.RefreshProperties();
        }

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender,
          BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down or it hasn't
                // completed yet.
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildStepChanged),
                        new object[] { sender, e });
            }
            else
            {
                if(!Settings.Default.VerboseLogging)
                    outputWindow.AppendText(e.BuildStep.ToString());

                if(e.HasCompleted)
                {
                    StatusBarTextProvider.ResetProgressBar();
                    this.SetUIEnabledState(true);
                    outputWindow.LogFile = buildProcess.LogFilename;

                    buildThread = null;
                    buildProcess = null;

                    if(e.BuildStep == BuildStep.Completed &&
                      Settings.Default.OpenHelpAfterBuild)
                        miViewHelpFile.PerformClick();
                }
            }
        }

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with information about its progress.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildProgress(object sender,
          BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildProgress),
                        new object[] { sender, e });
            }
            else
            {
                if(e.BuildStep < BuildStep.Completed)
                    StatusBarTextProvider.UpdateProgress((int)e.BuildStep);

                if(Settings.Default.VerboseLogging ||
                  e.BuildStep == BuildStep.Failed)
                    outputWindow.AppendText(e.Message);
            }
        }


        /// <summary>
        /// Start a new help project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miNewProject_Click(object sender, EventArgs e)
        {
            if(this.CloseProject())
                using(SaveFileDialog dlg = new SaveFileDialog())
                {
                    try
                    {
                        dlg.Title = "Save New Help Project As";
                        dlg.Filter = "Sandcastle Help File Builder Project " +
                            "Files (*.shfbproj)|*.shfbproj|All files (*.*)|*.*";
                        dlg.DefaultExt = "shfbproj";
                        dlg.InitialDirectory = Directory.GetCurrentDirectory();

                        if(dlg.ShowDialog() == DialogResult.OK)
                        {
                            this.Cursor = Cursors.WaitCursor;

                            if(File.Exists(dlg.FileName))
                                File.Delete(dlg.FileName);

                            this.CreateProject(dlg.FileName, false);

                            projectExplorer.Save(dlg.FileName);
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                        MessageBox.Show(ex.Message, Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
        }

        /// <summary>
        /// Create a new project from a project file that is in a different
        /// format (i.e. SHFB 1.7.0.0 or earlier or NDoc 1.x)
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miNewFromOtherFormat_Click(object sender, EventArgs e)
        {
            if(this.CloseProject())
                using(NewFromOtherFormatDlg dlg = new NewFromOtherFormatDlg())
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            this.Cursor = Cursors.WaitCursor;
                            this.CreateProject(dlg.NewProjectFilename, true);
                            this.UpdateFilenameInfo();
                            MainForm.UpdateMruList(project.Filename);
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Debug.Write(ex);
                            MessageBox.Show(ex.Message, Constants.AppName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            this.Cursor = Cursors.Default;
                            projectProperties.RefreshProperties();
                        }
                    }
                }
        }

        /// <summary>
        /// Open an existing help project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miOpenProject_Click(object sender, EventArgs e)
        {
            if(this.CloseProject())
                using(OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Load Help Project";
                    dlg.Filter = "Sandcastle Help File Builder Project " +
                        "Files (*.shfbproj)|*.shfbproj|All files (*.*)|*.*";
                    dlg.DefaultExt = "shfbproj";
                    dlg.InitialDirectory = Directory.GetCurrentDirectory();

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            this.Cursor = Cursors.WaitCursor;
                            this.CreateProject(dlg.FileName, true);
                            MainForm.UpdateMruList(project.Filename);
                        }
                        catch(InvalidProjectFileException pex)
                        {
                            System.Diagnostics.Debug.Write(pex);

                            if(dlg.FileName.EndsWith(".shfb", StringComparison.OrdinalIgnoreCase) ||
                              pex.Message.IndexOf("<project>", StringComparison.Ordinal) != -1)
                            {
                                MessageBox.Show("The project file format is invalid.  " +
                                    "If this project was created with an earlier " +
                                    "version of the Sandcastle Help File Builder, " +
                                    "use the File | New Project from Other Format " +
                                    "option to convert it to the latest project file " +
                                    "format.", Constants.AppName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                                MessageBox.Show(pex.Message, Constants.AppName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Debug.Write(ex);
                            MessageBox.Show(ex.Message, Constants.AppName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            this.Cursor = Cursors.Default;
                            projectProperties.RefreshProperties();
                        }
                    }
                }
        }

        /// <summary>
        /// Close the current project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCloseProject_Click(object sender, EventArgs e)
        {
            this.CloseProject();
        }

        /// <summary>
        /// Build the MRU list when the Recent Projects menu opens
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miRecentProjects_DropDownOpening(object sender, EventArgs e)
        {
            StringCollection mruList = Settings.Default.MruList;
            ToolStripMenuItem miProject;
            int idx = 1;

            miRecentProjects.DropDownItems.Clear();

            if(mruList.Count == 0)
            {
                miProject = new ToolStripMenuItem("(Empty)");
                miProject.Enabled = false;
                miRecentProjects.DropDownItems.Add(miProject);
            }
            else
                foreach(string project in mruList)
                {
                    miProject = new ToolStripMenuItem();
                    miProject.Text = String.Format(CultureInfo.CurrentCulture,
                        "&{0} {1}", idx++, project);
                    miProject.Click += new EventHandler(miProject_Click);
                    miRecentProjects.DropDownItems.Add(miProject);
                }
        }

        /// <summary>
        /// This is used to load a project from the MRU List
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void miProject_Click(object sender, EventArgs e)
        {
            StringCollection mruList = Settings.Default.MruList;

            if(this.CloseProject())
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    this.CreateProject(mruList[miRecentProjects.DropDownItems.IndexOf(
                        (ToolStripMenuItem)sender)], true);

                    MainForm.UpdateMruList(project.Filename);
                }
                catch(InvalidProjectFileException pex)
                {
                    System.Diagnostics.Debug.Write(pex);

                    if(pex.Message.IndexOf("<project>", StringComparison.Ordinal) != -1)
                    {
                        MessageBox.Show("The project file format is invalid.  " +
                            "If this project was created with an earlier " +
                            "version of the Sandcastle Help File Builder, " +
                            "use the File | New Project from Other Format " +
                            "option to convert it to the latest project file " +
                            "format.", Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                        MessageBox.Show(pex.Message, Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                    MessageBox.Show(ex.Message, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    projectProperties.RefreshProperties();
                }
            }
        }

        /// <summary>
        /// Set the active configuration for the build
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tcbConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(project != null)
                Settings.Default.LastConfig = project.Configuration =
                    (string)tcbConfig.SelectedItem;
        }

        /// <summary>
        /// Set the active platform for the build
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tcbPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(project != null)
                Settings.Default.LastPlatform = project.Platform =
                    (string)tcbPlatform.SelectedItem;
        }

        /// <summary>
        /// Build the help file using the current project settings
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miBuildProject_Click(object sender, EventArgs e)
        {
            if(project == null || !this.SaveBeforeBuild())
                return;

            miClearOutput_Click(miBuildProject, e);
            this.SetUIEnabledState(false);
            Application.DoEvents();

            buildProcess = new BuildProcess(project);
            buildProcess.BuildStepChanged +=
                new EventHandler<BuildProgressEventArgs>(
                    buildProcess_BuildStepChanged);
            buildProcess.BuildProgress +=
                new EventHandler<BuildProgressEventArgs>(
                    buildProcess_BuildProgress);

            StatusBarTextProvider.InitializeProgressBar(0,
                (int)BuildStep.Completed, "Building help file");

            buildThread = new Thread(new ThreadStart(
                buildProcess.Build));
            buildThread.Name = "Help file builder thread";
            buildThread.IsBackground = true;
            buildThread.Start();
        }

        /// <summary>
        /// Cancel the current build process
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCancelBuild_Click(object sender, EventArgs e)
        {
            if(buildThread != null && buildThread.IsAlive)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    StatusBarTextProvider.UpdateProgress("Cancelling build...");
                    buildThread.Abort();

                    while(buildThread != null && !buildThread.Join(1000))
                        Application.DoEvents();

                    StatusBarTextProvider.ResetProgressBar();
                    System.Diagnostics.Debug.WriteLine("Thread stopped");
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    buildThread = null;
                    buildProcess = null;
                }
            }

            this.SetUIEnabledState(true);
        }
        #endregion

        #region Project output event handlers
        //=====================================================================

        /// <summary>
        /// Clear the last build information from the output window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miClearOutput_Click(object sender, EventArgs e)
        {
            if(outputWindow != null)
            {
                if(sender == miClearOutput || sender == miBuildProject)
                    outputWindow.Activate();

                outputWindow.ResetLogViewer();
            }
            else
                if(sender == miClearOutput || sender == miBuildProject)
                {
                    outputWindow = new OutputWindow();
                    outputWindow.Show(dockPanel);
                }
        }

        /// <summary>
        /// Clean the output and working folders by deleting all files from
        /// them.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCleanOutput_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(1024);
            string projectFolder, outputFolder;

            // Make sure we start out in the project's output folder in case the output folder
            // is relative to it.
            projectFolder = Path.GetDirectoryName(Path.GetFullPath(project.Filename));
            Directory.SetCurrentDirectory(projectFolder);

            outputFolder = Path.GetFullPath(project.OutputPath);

            // If the folder doesn't exist, don't bother
            if(Directory.Exists(outputFolder) &&
              MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                  "This will delete all files from the output folder '{0}' " +
                  "including any not created by the help file builder.  " +
                  "Do you want to continue?", outputFolder), Constants.AppName,
                  MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                  MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    miClearOutput_Click(sender, e);

                    if(webServer != null)
                    {
                        this.KillWebServer();

                        // Give it a second to stop
                        Thread.Sleep(1000);
                    }

                    if(project.WorkingPath.Path.Length != 0 && Directory.Exists(project.WorkingPath))
                    {
                        BuildProcess.VerifySafePath("WorkingPath", project.WorkingPath, projectFolder);
                        Directory.Delete(project.WorkingPath, true);
                    }

                    BuildProcess.VerifySafePath("OutputPath", outputFolder, projectFolder);

                    // Read-only and/or hidden files and folders are ignored as they are assumed to be under
                    // source control.
                    foreach(string file in Directory.EnumerateFiles(outputFolder))
                        if((File.GetAttributes(file) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                            File.Delete(file);
                        else
                            sb.AppendFormat("Did not delete read-only or hidden file '{0}'\r\n", file);

                    foreach(string folder in Directory.EnumerateDirectories(outputFolder))
                        try
                        {
                            // Some source control providers have a mix of read-only/hidden files within a folder
                            // that isn't read-only/hidden (i.e. Subversion).  In such cases, leave the folder alone.
                            if(Directory.EnumerateFileSystemEntries(folder, "*", SearchOption.AllDirectories).Any(f =>
                              (File.GetAttributes(f) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0))
                                sb.AppendFormat("Did not delete folder '{0}' as it contains read-only or hidden " +
                                    "folders/files\r\n", folder);
                            else
                                if((File.GetAttributes(folder) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                                    Directory.Delete(folder, true);
                                else
                                    sb.AppendFormat("Did not delete folder '{0}' as it is read-only or hidden\r\n", folder);
                        }
                        catch(IOException ioEx)
                        {
                            sb.AppendFormat("Did not delete folder '{0}': {1}\r\n", folder, ioEx.Message);
                        }
                        catch(UnauthorizedAccessException uaEx)
                        {
                            sb.AppendFormat("Did not delete folder '{0}': {1}\r\n", folder, uaEx.Message);
                        }

                    // Delete the log file too
                    if(File.Exists(project.LogFileLocation))
                        File.Delete(project.LogFileLocation);

                    if(sb.Length != 0)
                        MessageBox.Show(sb.ToString(), Constants.AppName, MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Unable to clean output folder.  Reason: " + ex.Message, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Modify user preferences that are unrelated to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event argumenst</param>
        private void miUserPreferences_Click(object sender, EventArgs e)
        {
            using(UserPreferencesDlg dlg = new UserPreferencesDlg())
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    foreach(IDockContent content in dockPanel.Contents)
                        if(content is OutputWindow)
                            ((OutputWindow)content).UpdateSettings();
                        else
                            if(content is TopicEditorWindow)
                                ((TopicEditorWindow)content).UpdateFont();
                            else
                                if(content is TokenEditorWindow)
                                    ((TokenEditorWindow)content).UpdateFont();
            }
        }

        /// <summary>
        /// Enable or disable the viewing options based on the current
        /// project's help file format setting
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event argumenst</param>
        private void ctxViewHelpMenu_Opening(object sender, CancelEventArgs e)
        {
            miViewHtmlHelp1.Enabled = ((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0);
            miViewMSHelp2.Enabled = ((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0);
            miViewMSHelpViewer.Enabled = ((project.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0);
            miViewAspNetWebsite.Enabled = miViewHtmlWebsite.Enabled =
                ((project.HelpFileFormat & HelpFileFormat.Website) != 0);
            miOpenHelpAfterBuild.Checked = Settings.Default.OpenHelpAfterBuild;
        }

        /// <summary>
        /// View the help file produced by the last build.  Pick the first
        /// available format.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewHelpFile_Click(object sender, EventArgs e)
        {
            if(project == null)
                return;

            if((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0)
                miViewBuiltHelpFile_Click(miViewHtmlHelp1, e);
            else
                if((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0)
                    miViewBuiltHelpFile_Click(miViewMSHelp2, e);
                else
                    if((project.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0)
                        miViewMSHelpViewer_Click(sender, e);
                    else
                        miViewAspNetWebsite_Click(sender, e);
        }

        /// <summary>
        /// View the last build HTML Help 1 file, MS Help 2 file, or website
        /// Index.aspx/Index.html page.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewBuiltHelpFile_Click(object sender, EventArgs e)
        {
            // Make sure we start out in the project's output folder in case the output folder
            // is relative to it.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            string outputPath = project.OutputPath, help2Viewer = Settings.Default.HTMLHelp2ViewerPath;

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            if(sender == miViewHtmlHelp1)
                outputPath += project.HtmlHelpName + ".chm";
            else
                if(sender == miViewMSHelp2)
                {
                    outputPath += project.HtmlHelpName + ".hxs";

                    if(help2Viewer.Length == 0 || !File.Exists(help2Viewer))
                    {
                        MessageBox.Show("MS Help 2 files must be registered in a collection to be viewed " +
                            "or you can use a standalone viewer.  Use Project | User Preferences to define a " +
                            "standalone viewer.  See Links to Resources in the help file if you need one.",
                            Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                else
                    outputPath += "Index.html";

            if(!File.Exists(outputPath))
            {
                MessageBox.Show("A copy of the help file does not appear to exist yet.  It may need to be built.",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if(outputPath.EndsWith(".hxs", StringComparison.OrdinalIgnoreCase))
                    System.Diagnostics.Process.Start(help2Viewer, outputPath);
                else
                    System.Diagnostics.Process.Start(outputPath);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture, "Unable to open help file '{0}'\r\nReason: {1}",
                    outputPath, ex.Message), Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Launch the MS Help Viewer to view a Microsoft Help Viewer file.  This
        /// will optionally install it first.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewMSHelpViewer_Click(object sender, EventArgs e)
        {
            using(LaunchMSHelpViewerDlg dlg = new LaunchMSHelpViewerDlg(project, Settings.Default.MSHelpViewerPath))
            {
                dlg.ShowDialog();
            }
        }

        /// <summary>
        /// Launch the ASP.NET Development Web Server to view the website
        /// output (Index.aspx).
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewAspNetWebsite_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi;
            FilePath webServerPath = new FilePath(null);
            string path;

            // Make sure we start out in the project's output folder
            // in case the output folder is relative to it.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            string outputPath = project.OutputPath;

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            outputPath += "Index.aspx";

            if(!File.Exists(outputPath))
            {
                MessageBox.Show("A copy of the website does not appear to exist yet.  It may need to be built.",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // See if the web server needs to be started
                if(webServer == null || webServer.HasExited)
                {
                    if(webServer != null)
                        webServer.Dispose();

                    outputPath = Path.GetDirectoryName(outputPath);

                    // Visual Studio conveniently provides a development web server that doesn't require IIS
                    webServerPath.Path = String.Format(CultureInfo.InvariantCulture, "%SystemRoot%" +
                        @"\Microsoft.NET\Framework\v{0}\WebDev.WebServer.exe",
                        FrameworkVersionTypeConverter.LatestFrameworkMatching(".NET 2"));

                    // Visual Studio 2008 and later put it in a different location
                    if(!File.Exists(webServerPath))
                    {
                        path = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Common Files\" +
                            @"Microsoft Shared\DevServer");

                        if(Directory.Exists(path))
                            webServerPath.Path = Directory.EnumerateFiles(path, "WebDev.WebServer.exe",
                                SearchOption.AllDirectories).FirstOrDefault();

                        if(!File.Exists(webServerPath))
                        {
                            path = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\" +
                                @"Common Files\Microsoft Shared\DevServer");

                            if(Directory.Exists(path))
                                webServerPath.Path = Directory.EnumerateFiles(path, "WebDev.WebServer.exe",
                                    SearchOption.AllDirectories).FirstOrDefault();
                        }
                    }

                    if(!File.Exists(webServerPath))
                    {
                        MessageBox.Show("Unable to locate ASP.NET Development Web Server.  View the HTML " +
                            "website instead.", Constants.AppName, MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    webServer = new Process();
                    psi = webServer.StartInfo;

                    psi.FileName = webServerPath;
                    psi.Arguments = String.Format(CultureInfo.InvariantCulture,
                        "/port:{0} /path:\"{1}\" /vpath:\"/SHFBOutput_{2}\"",
                        Settings.Default.ASPNETDevServerPort, outputPath, this.Handle);
                    psi.WorkingDirectory = outputPath;
                    psi.UseShellExecute = false;

                    webServer.Start();
                    webServer.WaitForInputIdle(30000);
                }

                // This form's handle is used to keep the URL unique in case
                // multiple copies of SHFB are running so that each can view
                // website output.
                outputPath = String.Format(CultureInfo.InvariantCulture,
                    "http://localhost:{0}/SHFBOutput_{1}/Index.aspx",
                    Settings.Default.ASPNETDevServerPort, this.Handle);

                System.Diagnostics.Process.Start(outputPath);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to open ASP.NET website '{0}'\r\nReason: {1}",
                    outputPath, ex.Message), Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Launch the Help Library Manager for interactive use based on the
        /// current project's settings.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miLaunchHlm_Click(object sender, EventArgs e)
        {
            try
            {
                HelpLibraryManager hlm = new HelpLibraryManager();

                hlm.LaunchInteractive(String.Format(CultureInfo.InvariantCulture,
                    "/product \"{0}\" /version \"{1}\" /locale {2} /brandingPackage Dev10.mshc",
                    project.CatalogProductId, project.CatalogVersion, project.Language.Name));
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to launch Help Library Manager.  Reason:\r\n{0}",
                    ex.Message), Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Toggle the "Open After Build" option
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miOpenHelpAfterBuild_Click(object sender, EventArgs e)
        {
            Settings.Default.OpenHelpAfterBuild =
                !Settings.Default.OpenHelpAfterBuild;
        }
        #endregion

        #region Window related event handlers
        //=====================================================================

        /// <summary>
        /// Update the command states when content is added
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dockPanel_ContentAdded(object sender,
          DockContentEventArgs e)
        {
            bool canPreview = false;

            miClose.Enabled = miCloseAll.Enabled =
                miCloseAllButCurrent.Enabled = true;

            if(e.Content is TopicEditorWindow)
                canPreview = true;
            else
                foreach(IDockContent document in dockPanel.Contents)
                    if(document is TopicEditorWindow)
                        canPreview = true;

            miPreviewTopic.Enabled = tsbPreviewTopic.Enabled = canPreview;
        }

        /// <summary>
        /// Update the command states when content is removed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dockPanel_ContentRemoved(object sender,
          DockContentEventArgs e)
        {
            bool canPreview = false;

            miClose.Enabled = miCloseAll.Enabled = miCloseAllButCurrent.Enabled =
                (dockPanel.Contents.Count > 0);

            foreach(IDockContent document in dockPanel.Contents)
                if(document is TopicEditorWindow)
                    canPreview = true;

            miPreviewTopic.Enabled = tsbPreviewTopic.Enabled = canPreview;
        }

        /// <summary>
        /// Close the active document window/panel
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miClose_Click(object sender, EventArgs e)
        {
            dockPanel.CheckFocusedContent();    // HACK
            BaseContentEditor content = dockPanel.ActiveContent as BaseContentEditor;

            if(content == null)
                content = projectExplorer;

            if(content != null && content.CanClose)
                if(content.DockHandler.HideOnClose)
                    content.DockHandler.Hide();
                else
                    content.DockHandler.Close();
        }

        /// <summary>
        /// Close all open content editors
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCloseAll_Click(object sender, EventArgs e)
        {
            BaseContentEditor content;
            IDockContent[] contents = new IDockContent[dockPanel.Contents.Count];
            dockPanel.Contents.CopyTo(contents, 0);

            foreach(IDockContent dockContent in contents)
            {
                // Close all but the current one?
                if(sender == miCloseAllButCurrent && dockContent.DockHandler.IsActivated)
                    continue;

                if(dockContent is ProjectExplorerWindow ||
                  dockContent is ProjectPropertiesWindow)
                    continue;

                content = dockContent as BaseContentEditor;

                if(content != null && !content.CanClose)
                    break;

                if(dockContent.DockHandler.HideOnClose)
                    dockContent.DockHandler.Hide();
                else
                    dockContent.DockHandler.Close();
            }
        }

        /// <summary>
        /// Save all changes to the current item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miSave_Click(object sender, EventArgs e)
        {
            dockPanel.CheckFocusedContent();    // HACK
            BaseContentEditor content = dockPanel.ActiveContent as BaseContentEditor;

            // Save the project by default
            if(content == null || !content.CanSaveContent)
                content = projectExplorer;

            if(content != null)
                content.Save();
        }

        /// <summary>
        /// Save all changes to the current item under a new name
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miSaveAs_Click(object sender, EventArgs e)
        {
            dockPanel.CheckFocusedContent();    // HACK
            BaseContentEditor content = dockPanel.ActiveContent as BaseContentEditor;

            if(content == null)
                content = projectExplorer;

            if(content != null && content.CanSaveContent)
                content.SaveAs();
        }

        /// <summary>
        /// Save all content editors and the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miSaveAll_Click(object sender, EventArgs e)
        {
            BaseContentEditor content;

            if(project.IsDirty)
                if(!projectExplorer.Save())
                    return;

            foreach(IDockContent dockContent in dockPanel.Contents)
            {
                content = dockContent as BaseContentEditor;

                if(content == null || content.CanSaveContent)
                    if(!content.Save())
                        break;
            }
        }

        /// <summary>
        /// View the project explorer window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewProjectExplorer_Click(object sender, EventArgs e)
        {
            projectExplorer.Activate();
        }

        /// <summary>
        /// View the project properties window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewProjectProperties_Click(object sender, EventArgs e)
        {
            projectProperties.Activate();
        }

        /// <summary>
        /// View the output from the last build
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewOutput_Click(object sender, EventArgs e)
        {
            string lastBuildLog = null;

            if(outputWindow == null)
            {
                outputWindow = new OutputWindow();
                outputWindow.Show(dockPanel);
            }

            if(project != null)
            {
                // Make sure we start out in the project's output folder
                // in case the output folder is relative to it.
                Directory.SetCurrentDirectory(Path.GetDirectoryName(
                    Path.GetFullPath(project.Filename)));

                lastBuildLog = project.LogFileLocation;
            }

            if(sender == miViewLog)
                outputWindow.ViewLogFile(lastBuildLog);
            else
            {
                // Set the log file so that the user can switch to it
                if(!File.Exists(lastBuildLog))
                    outputWindow.LogFile = null;
                else
                    outputWindow.LogFile = lastBuildLog;

                outputWindow.ViewBuildOutput();
            }
        }

        /// <summary>
        /// Open or show the entity references window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miEntityReferences_Click(object sender, EventArgs e)
        {
            if(entityReferencesWindow == null)
            {
                entityReferencesWindow = new EntityReferenceWindow(tsslStatusText);
                entityReferencesWindow.CurrentProject = project;
                entityReferencesWindow.Show(dockPanel);
            }
            else
            {
                if(entityReferencesWindow.CurrentProject != project)
                    entityReferencesWindow.CurrentProject = project;

                entityReferencesWindow.Activate();
            }
        }

        /// <summary>
        /// Open or show the preview topic window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miPreviewTopic_Click(object sender, EventArgs e)
        {
            TopicEditorWindow editor;
            FileItem fileItem;

            dockPanel.CheckFocusedContent();    // HACK
            editor = dockPanel.ActiveDocument as TopicEditorWindow;

            if(editor == null)
            {
                foreach(IDockContent document in dockPanel.Contents)
                {
                    editor = document as TopicEditorWindow;

                    if(editor != null)
                        break;
                }

                if(editor == null)
                    return;
            }

            if(!this.SaveBeforeBuild())
                return;

            if(previewWindow == null)
            {
                previewWindow = new PreviewTopicWindow();
                previewWindow.Show(dockPanel);
            }

            fileItem = project.FindFile(editor.Filename);

            if(fileItem != null)
            {
                // Save the editor to ensure it is current on disk.  This has
                // to happen regardless of the BeforeBuild preference or the
                // content won't be current.
                if(!editor.Save())
                    return;

                previewWindow.PreviewTopic(project, fileItem);
                previewWindow.Activate();

                // When the state is restored and it's a document pane, it
                // doesn't always become the active pane unless this is called.
                previewWindow.Show(dockPanel, previewWindow.DockState);
            }
        }
        #endregion

        #region File system watcher event handlers
        //=====================================================================

        /// <summary>
        /// On deactivation, start watching for changes in the project folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if(project != null && fsw != null && !checkingForChangedFiles)
            {
                fsw.Path = Path.GetDirectoryName(project.Filename);
                fsw.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// On activation ask the user as needed about any changes noticed in
        /// the project folder.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void MainForm_Activated(object sender, EventArgs e)
        {
            DockContent editor;
            string projectFilename;

            if(fsw != null && fsw.EnableRaisingEvents)
            {
                fsw.EnableRaisingEvents = false;

                try
                {
                    checkingForChangedFiles = true;

                    // Handle the project file first if it changed.  If so, we'll ignore the other changes and
                    // just reload the project.  Doing so will close all other open editors.
                    if(changedFiles.Contains(project.Filename))
                    {
                        if(MessageBox.Show("The project file has been changed outside of the Sandcastle Help " +
                            "File Builder.  Would you like to reload it?", Constants.AppName, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            projectFilename = project.Filename;
                            changedFiles.Remove(projectFilename);

                            // If the project isn't closed, we'll fall through and handle any other files
                            if(this.CloseProject())
                            {
                                this.CreateProject(projectFilename, true);
                                return;
                            }
                        }
                    }

                    foreach(string filename in changedFiles)
                        foreach(IDockContent content in dockPanel.Contents.ToArray())
                            if(content.DockHandler.ToolTipText != null &&
                                content.DockHandler.ToolTipText.Equals(filename, StringComparison.OrdinalIgnoreCase))
                            {
                                content.DockHandler.Activate();

                                if(MessageBox.Show(filename + " was modified outside of the source editor.  " +
                                    "Would you like to reload it?", Constants.AppName, MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                                {
                                    ((BaseContentEditor)content).IsDirty = false;

                                    content.DockHandler.Close();
                                    editor = projectExplorer.CreateFileEditor(filename, null);

                                    if(editor != null)
                                        editor.Show(dockPanel);
                                }
                            }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                    MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    changedFiles.Clear();
                    checkingForChangedFiles = false;
                }
            }
        }

        /// <summary>
        /// Note changes to the file system
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void fsw_OnChanged(object sender, FileSystemEventArgs e)
        {
            if(!changedFiles.Contains(e.FullPath))
                changedFiles.Add(e.FullPath);
        }
        #endregion
    }
}
