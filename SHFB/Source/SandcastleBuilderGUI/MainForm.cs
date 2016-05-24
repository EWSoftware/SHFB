//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : MainForm.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/19/2016
// Note    : Copyright 2006-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the main form for the application.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
// 10/18/2006  EFW  Moved file logging to the BuildProcess class.  Added a Verbose Logging user setting
// 06/23/2007  EFW  Added user preferences dialog box and help file viewer options
// 07/03/2007  EFW  Added support for content file editor definitions and a content site map file
// 11/02/2007  EFW  Reworked support for custom build components
// 11/16/2007  EFW  Added "Open After Build" option
// 01/22/2008  EFW  Added support for drag and drop from Explorer
// 04/24/2008  EFW  Added support for conceptual content
// 06/20/2008  EFW  Converted the project to use an MSBuild file.  Changed the UI to a more Visual Studio-like
//                  layout better suited to editing the project.
// 07/05/2010  EFW  Added support for MS Help Viewer
// 07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0
// 01/08/2012  EFW  Updated to use shared NewFromOtherFormatDlg
// 01/20/2012  EFW  Updated to use the new topic previewer window
// 10/05/2012  EFW  Added support for Help Viewer 2.0
// 02/15/2014  EFW  Added support for the Open XML output format
// 04/01/2015  EFW  Added support for the Markdown file format
// 05/03/2015  EFW  Removed support for the MS Help 2 file format and the project converters
//===============================================================================================================

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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Build.Exceptions;

using Sandcastle.Core;

using SandcastleBuilder.MicrosoftHelpViewer;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.Controls;
using SandcastleBuilder.Utils.Design;

using SandcastleBuilder.Gui.ContentEditors;
using SandcastleBuilder.Gui.Properties;

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
        private BuildProcess buildProcess;
        private CancellationTokenSource cancellationTokenSource;
        private Process webServer;
        private ProjectExplorerWindow projectExplorer;
        private ProjectPropertiesWindow projectProperties;
        private OutputWindow outputWindow;
        private EntityReferenceWindow entityReferencesWindow;
        private PreviewTopicWindow previewWindow;
        private FileSystemWatcher fsw;
        private HashSet<string> changedFiles;
        private bool checkingForChangedFiles;
        private string excludedOutputFolder, excludedWorkingFolder;

        private static MainForm mainForm;

        private static Regex reWarning = new Regex(@"(Warn|Warning( HXC\d+)?):|" +
            @"SHFB\s*:\s*(W|w)arning\s.*?:|.*?(\(\d*,\d*\))?:\s*(W|w)arning\s.*?:");
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
        /// Get a reference to the Project Properties pane
        /// </summary>
        public ProjectPropertiesWindow ProjectProperties
        {
            get { return projectProperties; }
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
                string userName = Environment.GetEnvironmentVariable("USERNAME");

                if(String.IsNullOrWhiteSpace(userName))
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
        /// <param name="projectToLoad">A default project file to load.  If not specified, the most recently used
        /// project is loaded if there is one.</param>
        public MainForm(string projectToLoad)
        {
            mainForm = this;
            InitializeComponent();

            // Skip the rest of the initialization in design mode
            if(this.DesignMode)
                return;

            // We are only going to monitor for file changes.  We won't handle moves, deletes, or renames.
            changedFiles = new HashSet<string>();
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
                    MessageBox.Show("Unable to find project: " + projectToLoad, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Set the status label to use for status bar text
            StatusBarTextProvider.ApplicationStatusBar = tsslStatusText;

            // Define the status label and progress bar too.  This allows easy access to those items from
            // anywhere within the application.
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
                if(outputWindow == null)
                    outputWindow = new OutputWindow();

                return outputWindow;
            }

            if(persistString == typeof(EntityReferenceWindow).FullName)
            {
                if(entityReferencesWindow == null)
                {
                    entityReferencesWindow = new EntityReferenceWindow();
                    entityReferencesWindow.CurrentProject = project;
                }

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
        /// <param name="mustExist">True if it must exist or false if it is a new, unnamed project</param>
        private void CreateProject(string projectName, bool mustExist)
        {
            List<string> values;

            project = new SandcastleProject(projectName, mustExist, false);

            projectExplorer.CurrentProject = projectProperties.CurrentProject = project;

            if(entityReferencesWindow != null)
                entityReferencesWindow.CurrentProject = project;

            this.UpdateFilenameInfo();

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

            miDocumentation.Visible = miCloseProject.Enabled = miClose.Enabled = miSave.Enabled =
                miSaveAs.Enabled = tsbSave.Enabled = tsbSaveAll.Enabled = miSaveAll.Enabled =
                miProjectExplorer.Visible = miExplorerSeparator.Visible = tsbBuildProject.Enabled =
                tsbViewHelpFile.Enabled = miPreviewTopic.Enabled = tsbPreviewTopic.Enabled = true;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            if(Settings.Default.PerUserProjectState && File.Exists(project.Filename + WindowStateSuffix))
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    dockPanel.SuspendLayout(true);

                    projectExplorer.DockPanel = null;
                    projectProperties.DockPanel = null;

                    if(outputWindow != null)
                        outputWindow.DockPanel = null;

                    if(entityReferencesWindow != null)
                        entityReferencesWindow.DockPanel = null;

                    if(previewWindow != null)
                    {
                        previewWindow.Dispose();
                        previewWindow = null;
                    }

                    dockPanel.LoadFromXml(project.Filename + WindowStateSuffix, DeserializeState);
                }
                catch(InvalidOperationException )
                {
                    // Ignore errors if the content settings don't load
                    miViewProjectExplorer_Click(this, EventArgs.Empty);
                    miViewProjectProperties_Click(this, EventArgs.Empty);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    dockPanel.ResumeLayout(true, true);
                }
            }
            else
            {
                miViewProjectExplorer_Click(this, EventArgs.Empty);
                miViewProjectProperties_Click(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This is used to save the project and/or document windows prior to doing a build
        /// </summary>
        /// <returns>True if successful, false if it fails or is cancelled.</returns>
        private bool SaveBeforeBuild()
        {
            Collection<BaseContentEditor> filesToSave = new Collection<BaseContentEditor>();
            BaseContentEditor editor;
            DialogResult result;
            bool success = true;

            if(!projectProperties.Apply())
                return false;

            if(Settings.Default.BeforeBuild == BeforeBuildAction.DoNotSave)
                return true;

            // Content layout windows need saving too.  We'll prompt to save these
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

                // Dispose of the preview window as it doesn't make much sense to save its state
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
                    // If hidden, unhide it so that DockPanel can write to it.  We'll hide it again afterwards.
                    bool isHidden = (File.Exists(projectExplorer.CurrentProject.Filename + WindowStateSuffix) &&
                      (File.GetAttributes(projectExplorer.CurrentProject.Filename +
                        WindowStateSuffix) & FileAttributes.Hidden) != 0);

                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        if(isHidden)
                            File.SetAttributes(projectExplorer.CurrentProject.Filename + WindowStateSuffix,
                                FileAttributes.Normal);

                        dockPanel.SaveAsXml(projectExplorer.CurrentProject.Filename + WindowStateSuffix);

                        if(isHidden)
                            File.SetAttributes(projectExplorer.CurrentProject.Filename + WindowStateSuffix,
                                FileAttributes.Hidden);
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
                {
                    SandcastleBuilder.Package.PropertyPages.ComponentCache.RemoveComponentCache(project.Filename);
                    project.Dispose();
                }

                project = projectExplorer.CurrentProject = projectProperties.CurrentProject = null;
                this.UpdateFilenameInfo();

                miDocumentation.Visible = miCloseProject.Enabled = miClose.Enabled = miCloseAll.Enabled =
                    miCloseAllButCurrent.Enabled = miSave.Enabled = miSaveAs.Enabled = miSaveAll.Enabled =
                    tsbSave.Enabled = tsbSaveAll.Enabled = miProjectExplorer.Visible =
                    miExplorerSeparator.Visible = tsbBuildProject.Enabled = tsbViewHelpFile.Enabled =
                    miPreviewTopic.Enabled = tsbPreviewTopic.Enabled = false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// This is used to update the filename information in the title bar
        /// </summary>
        internal void UpdateFilenameInfo()
        {
            if(project == null)
                this.Text = Constants.AppName;
            else
                this.Text = String.Format(CultureInfo.CurrentCulture, "{0}{1} - {2}",
                    Path.GetFileNameWithoutExtension(project.Filename),
                    (project.IsDirty || (projectProperties != null && projectProperties.IsDirty)) ? "*" :
                    String.Empty, Constants.AppName);
        }

        /// <summary>
        /// This is used to enable or disable the UI elements for the build process
        /// </summary>
        /// <param name="enabled">True to enable the UI, false to disable it</param>
        private void SetUIEnabledState(bool enabled)
        {
            // These are always enabled even when building
            HashSet<string> alwaysEnabledIds = new HashSet<string>(new[] { miExit.Name,
                miViewProjectExplorer.Name, miViewProjectProperties.Name, miViewOutput.Name, miHelp.Name,
                miFaq.Name, miAbout.Name, tsbProjectExplorer.Name, tsbProjectProperties.Name, tsbViewOutput.Name,
                tsbFaq.Name });

            foreach(ToolStripMenuItem item in mnuMain.Items)
                foreach(ToolStripItem subItem in item.DropDownItems)
                    if(!alwaysEnabledIds.Contains(subItem.Name))
                        subItem.Enabled = enabled;

            foreach(ToolStripItem item in tsbMain.Items)
                if(!alwaysEnabledIds.Contains(item.Name))
                    item.Enabled = enabled;

            projectExplorer.Enabled = enabled;
            projectProperties.SetEnabledState(enabled);

            if(outputWindow == null)
                miClearOutput_Click(miClearOutput, EventArgs.Empty);

            outputWindow.SetBuildState(!enabled);

            // The Cancel Build options are the inverse of the value
            miCancelBuild.Enabled = tsbCancelBuild.Enabled = !enabled;
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
                Debug.WriteLine(ex.ToString());
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

            // Set the current directory to My Documents so that people don't save stuff in the SHFB folder by
            // default.
            try
            {
                Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
            catch
            {
                // This doesn't always work if they use a mapped network drive for My Documents.  In that case,
                // just ignore the failure and stay put.
            }

            // If not starting out minimized, attempt to load the last used window size and position
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

            // If not restored or something when wrong and the project explorer doesn't have a dock panel, show
            // them by default.
            if(projectExplorer.DockPanel == null)
            {
                projectExplorer.Show(dockPanel);
                projectProperties.Show(dockPanel);
                projectExplorer.Focus();
            }
            
            try
            {
                // Check for the SHFBROOT environment variable.  It may not be present yet if a reboot hasn't
                // occurred after installation.  In such cases, set it to the proper folder for this process so
                // that projects can be loaded and built.
                if(Environment.GetEnvironmentVariable("SHFBROOT") == null)
                {
                    string shfbRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    MessageBox.Show("The SHFBROOT system environment variable was not found.  This variable " +
                        "is usually created during installation and may require a reboot.  It has been defined " +
                        "temporarily for this process as:\n\nSHFBROOT=" + shfbRootPath, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Environment.SetEnvironmentVariable("SHFBROOT", shfbRootPath);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to detect or set SHFBROOT environment variable.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create the MRU on first use
            if(mruList == null)
            {
                Settings.Default.MruList = new StringCollection();
                this.CloseProject();
            }
            else
            {
                // Get rid of projects that no longer exist or that are not compatible
                while(idx < mruList.Count)
                    if(!File.Exists(mruList[idx]) || mruList[idx].EndsWith(".shfb", StringComparison.OrdinalIgnoreCase))
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
                        Debug.Write(pex);

                        MessageBox.Show("The project file format is invalid: " + pex.Message, Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch(Exception ex)
                    {
                        Debug.Write(ex);
                        MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if(cancellationTokenSource != null)
            {
                if(cancellationTokenSource.IsCancellationRequested)
                    return;

                if(MessageBox.Show("A build is currently taking place.  Do you want to abort it and exit?",
                  Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                miCancelBuild_Click(sender, e);
                return;
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
                    // If hidden, unhide it so that DockPanel can write to it.  We'll hide it again afterwards.
                    bool isHidden = (File.Exists(projectExplorer.CurrentProject.Filename + WindowStateSuffix) &&
                      (File.GetAttributes(projectExplorer.CurrentProject.Filename +
                        WindowStateSuffix) & FileAttributes.Hidden) != 0);

                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        if(isHidden)
                            File.SetAttributes(projectExplorer.CurrentProject.Filename + WindowStateSuffix,
                                FileAttributes.Normal);

                        dockPanel.SaveAsXml(projectExplorer.CurrentProject.Filename + WindowStateSuffix);

                        if(isHidden)
                            File.SetAttributes(projectExplorer.CurrentProject.Filename + WindowStateSuffix,
                                FileAttributes.Hidden);
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
            string topic;

            if(sender == miHelp || sender == tsbAbout)
                topic = "bd1ddb51-1c4f-434f-bb1a-ce2135d3a909";
            else
                topic = "1aea789d-b226-4b39-b534-4c97c256fac8";

            Utility.ShowHelpTopic(topic);
        }
        #endregion

        #region Project and build event handlers
        //=====================================================================

        /// <summary>
        /// This is used to report build progress
        /// </summary>
        /// <param name="e">The event arguments</param>
        private void buildProcess_ReportProgress(BuildProgressEventArgs e)
        {
            if(!this.IsDisposed)
            {
                if(e.BuildStep < BuildStep.Completed)
                    StatusBarTextProvider.UpdateProgress((int)e.BuildStep);

                if(Settings.Default.VerboseLogging || e.BuildStep == BuildStep.Failed)
                    outputWindow.AppendText(e.Message);
                else
                {
                    if(e.StepChanged)
                        outputWindow.AppendText(e.BuildStep.ToString());

                    // If not doing verbose logging, show warnings.  Errors will kill the build so we don't have to
                    // deal with them here.
                    if(reWarning.IsMatch(e.Message))
                        outputWindow.AppendText(e.Message);
                }
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
                        Debug.Write(ex);
                        MessageBox.Show(ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
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
                            Debug.Write(pex);

                            MessageBox.Show("The project file format is invalid: " + pex.Message,
                                Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch(Exception ex)
                        {
                            Debug.Write(ex);
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
            ToolStripMenuItem miMruProject;
            int idx = 1;

            miRecentProjects.DropDownItems.Clear();

            if(mruList.Count == 0)
            {
                miMruProject = new ToolStripMenuItem("(Empty)");
                miMruProject.Enabled = false;
                miRecentProjects.DropDownItems.Add(miMruProject);
            }
            else
                foreach(string mru in mruList)
                {
                    miMruProject = new ToolStripMenuItem();
                    miMruProject.Text = String.Format(CultureInfo.CurrentCulture, "&{0} {1}", idx++, mru);
                    miMruProject.Click += new EventHandler(miProject_Click);
                    miRecentProjects.DropDownItems.Add(miMruProject);
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
                    Debug.Write(pex);

                    MessageBox.Show("The project file format is invalid: " + pex.Message, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch(Exception ex)
                {
                    Debug.Write(ex);
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
                Settings.Default.LastConfig = project.Configuration = (string)tcbConfig.SelectedItem;
        }

        /// <summary>
        /// Set the active platform for the build
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tcbPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(project != null)
                Settings.Default.LastPlatform = project.Platform = (string)tcbPlatform.SelectedItem;
        }

        /// <summary>
        /// Build the help file using the current project settings
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private async void miBuildProject_Click(object sender, EventArgs e)
        {
            if(project == null || !this.SaveBeforeBuild())
                return;

            miClearOutput_Click(miBuildProject, e);
            this.SetUIEnabledState(false);
            Application.DoEvents();

            StatusBarTextProvider.InitializeProgressBar(0, (int)BuildStep.Completed, "Building help file");

            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                buildProcess = new BuildProcess(project)
                {
                    ProgressReportProvider = new Progress<BuildProgressEventArgs>(buildProcess_ReportProgress),
                    CancellationToken = cancellationTokenSource.Token,
                };

                await Task.Run(() => buildProcess.Build(), cancellationTokenSource.Token);
            }
            finally
            {
                if(cancellationTokenSource != null)
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }

                StatusBarTextProvider.ResetProgressBar();
                this.SetUIEnabledState(true);
                outputWindow.LogFile = buildProcess.LogFilename;

                if(buildProcess.CurrentBuildStep == BuildStep.Completed && Settings.Default.OpenHelpAfterBuild)
                    miViewHelpFile.PerformClick();

                buildProcess = null;
            }
        }

        /// <summary>
        /// Cancel the current build process
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCancelBuild_Click(object sender, EventArgs e)
        {
            if(cancellationTokenSource != null)
            {
                StatusBarTextProvider.UpdateProgress("Cancelling build...");
                cancellationTokenSource.Cancel();
            }
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
        /// Clean the output and working folders by deleting all files from them
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCleanOutput_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(1024);
            string projectFolder, outputFolder = project.OutputPath;

            // Make sure we start out in the project's output folder in case the output folder is relative to it
            projectFolder = Path.GetDirectoryName(Path.GetFullPath(project.Filename));
            Directory.SetCurrentDirectory(projectFolder);

            // If the output path contains MSBuild variables, get the evaluated value from the project
            if(outputFolder.IndexOf("$(", StringComparison.Ordinal) != -1)
                outputFolder = project.MSBuildProject.GetProperty("OutputPath").EvaluatedValue;

            outputFolder = Path.GetFullPath(outputFolder);

            // If the folder doesn't exist, don't bother
            if(Directory.Exists(outputFolder) && MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
              "This will delete all files from the output folder '{0}' including any not created by the help " +
              "file builder.  Do you want to continue?", outputFolder), Constants.AppName,
              MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
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
                    Debug.WriteLine(ex.ToString());
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
        /// <param name="e">The event arguments</param>
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
            }
        }

        /// <summary>
        /// Enable or disable the viewing options based on the current project's help file format setting
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ctxViewHelpMenu_Opening(object sender, CancelEventArgs e)
        {
            miViewHtmlHelp1.Enabled = ((project.HelpFileFormat & HelpFileFormats.HtmlHelp1) != 0);
            miViewAspNetWebsite.Enabled = miViewHtmlWebsite.Enabled =
                ((project.HelpFileFormat & HelpFileFormats.Website) != 0);
            miViewOpenXml.Enabled = ((project.HelpFileFormat & HelpFileFormats.OpenXml) != 0);
            miOpenHelpAfterBuild.Checked = Settings.Default.OpenHelpAfterBuild;
        }

        /// <summary>
        /// View the help file produced by the last build.  Pick the first available format.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewHelpFile_Click(object sender, EventArgs e)
        {
            if(project == null)
                return;

            if((project.HelpFileFormat & HelpFileFormats.HtmlHelp1) != 0)
                miViewBuiltHelpFile_Click(miViewHtmlHelp1, e);
            else
                if((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0)
                    miViewMSHelpViewer_Click(sender, e);
                else
                    if((project.HelpFileFormat & HelpFileFormats.OpenXml) != 0)
                        miViewBuiltHelpFile_Click(miViewOpenXml, e);
                    else
                        if((project.HelpFileFormat & HelpFileFormats.Markdown) != 0)
                            miViewBuiltHelpFile_Click(miViewHelpFile, e);
                        else
                            miViewAspNetWebsite_Click(sender, e);
        }

        /// <summary>
        /// View the last build help output
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewBuiltHelpFile_Click(object sender, EventArgs e)
        {
            // Make sure we start out in the project's output folder in case the output folder is relative to it
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            string outputPath = project.OutputPath;

            // If the output path contains MSBuild variables, get the evaluated value from the project
            if(outputPath.IndexOf("$(", StringComparison.Ordinal) != -1)
                outputPath = project.MSBuildProject.GetProperty("OutputPath").EvaluatedValue;

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            if(sender == miViewHtmlHelp1)
                outputPath += project.HtmlHelpName + ".chm";
            else
                if(sender == miViewOpenXml)
                    outputPath += project.HtmlHelpName + ".docx";
                else
                    if(sender == miViewHelpFile)
                        outputPath += "_Sidebar.md";
                    else
                        outputPath += "Index.html";

            // If there are substitution tags present, have a go at resolving them
            if(outputPath.IndexOf("{@", StringComparison.Ordinal) != -1)
            {
                try
                {
                    var bp = new BuildProcess(project);
                    outputPath = bp.SubstitutionTags.TransformText(outputPath);
                }
                catch
                {
                    // Ignore errors
                    MessageBox.Show("The help filename appears to contain substitution tags but they could " +
                        "not be resolved to determine the actual file to open for viewing.  Building " +
                        "website output and viewing it can be used to work around this issue.",
                        Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            if(!File.Exists(outputPath))
            {
                MessageBox.Show("A copy of the help file does not appear to exist yet.  It may need to be built.",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(outputPath);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture, "Unable to open help file '{0}'\r\nReason: {1}",
                    outputPath, ex.Message), Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Launch the MS Help Viewer to view a Microsoft Help Viewer file.  This will optionally install it
        /// first.
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
        /// Launch the ASP.NET Development Web Server to view the website output (Index.aspx/Index.html)
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miViewAspNetWebsite_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi;
            FilePath webServerPath = new FilePath(null);
            string outputPath, path, vPath = null, defaultPage = "Index.aspx";

            // Make sure we start out in the project's output folder in case the output folder is relative to it
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            outputPath = project.OutputPath;
            vPath = String.Format(" /vpath:\"/SHFBOutput_{0}\"", this.Handle);

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            outputPath += defaultPage;

            if(!File.Exists(outputPath))
            {
                outputPath = Path.ChangeExtension(outputPath, ".html");
                defaultPage = "Index.html";
            }

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
                    path = Path.Combine(Environment.GetFolderPath(Environment.Is64BitProcess ?
                        Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles),
                        @"Common Files\Microsoft Shared\DevServer");

                    if(Directory.Exists(path))
                    {
                        webServerPath.Path = Directory.EnumerateFiles(path, "WebDev.WebServer40.exe",
                            SearchOption.AllDirectories).FirstOrDefault();

                        // Fall back to the .NET 2.0/3.5 version?
                        if(!File.Exists(webServerPath))
                        {
                            webServerPath.Path = Directory.EnumerateFiles(path, "WebDev.WebServer20.exe",
                                SearchOption.AllDirectories).FirstOrDefault();

                            if(!File.Exists(webServerPath))
                            {
                                // Try for IIS Express
                                webServerPath.Path = Path.Combine(Environment.GetFolderPath(Environment.Is64BitProcess ?
                                    Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles),
                                    @"IIS Express\IISExpress.exe");
                                vPath = String.Empty;
                            }
                        }
                    }

                    if(!File.Exists(webServerPath))
                    {
                        MessageBox.Show("Unable to locate ASP.NET Development Web Server or IIS Express.  " +
                            "View the HTML website instead.", Constants.AppName, MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    webServer = new Process();
                    psi = webServer.StartInfo;

                    psi.FileName = webServerPath;
                    psi.Arguments = String.Format(CultureInfo.InvariantCulture, "/port:{0} /path:\"{1}\"{2}",
                        Settings.Default.ASPNETDevServerPort, outputPath, vPath);
                    psi.WorkingDirectory = outputPath;
                    psi.UseShellExecute = false;

                    webServer.Start();

                    if(!String.IsNullOrWhiteSpace(vPath))
                        webServer.WaitForInputIdle(30000);
                    else
                        Thread.Sleep(500);
                }
                else
                    if(webServer.ProcessName.StartsWith("IISExpress", StringComparison.OrdinalIgnoreCase))
                        vPath = String.Empty;

                // This form's handle is used to keep the URL unique in case multiple copies of SHFB are running
                // so that each can view website output (WebDevServer only).
                if(!String.IsNullOrWhiteSpace(vPath))
                {
                    outputPath = String.Format(CultureInfo.InvariantCulture,
                        "http://localhost:{0}/SHFBOutput_{1}/{2}", Settings.Default.ASPNETDevServerPort,
                        this.Handle, defaultPage);
                }
                else
                    outputPath = String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/{1}",
                        Settings.Default.ASPNETDevServerPort, defaultPage);

                Process.Start(outputPath);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to open ASP.NET website '{0}'\r\nReason: {1}", outputPath, ex.Message),
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Toggle the "Open After Build" option
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miOpenHelpAfterBuild_Click(object sender, EventArgs e)
        {
            Settings.Default.OpenHelpAfterBuild = !Settings.Default.OpenHelpAfterBuild;
        }
        #endregion

        #region Window related event handlers
        //=====================================================================

        /// <summary>
        /// Update the command states when content is added
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dockPanel_ContentAdded(object sender, DockContentEventArgs e)
        {
            miClose.Enabled = miCloseAll.Enabled = miCloseAllButCurrent.Enabled = true;
        }

        /// <summary>
        /// Update the command states when content is removed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dockPanel_ContentRemoved(object sender, DockContentEventArgs e)
        {
            miClose.Enabled = miCloseAll.Enabled = miCloseAllButCurrent.Enabled = (dockPanel.Contents.Count > 0);
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
                entityReferencesWindow = new EntityReferenceWindow();
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
            FileItem fileItem = null;

            dockPanel.CheckFocusedContent();    // HACK
            editor = dockPanel.ActiveDocument as TopicEditorWindow;

            // If we are in a topic editor, show it by default when the previewer opens
            if(editor == null)
            {
                foreach(IDockContent document in dockPanel.Contents)
                {
                    editor = document as TopicEditorWindow;

                    if(editor != null)
                        break;
                }
            }

            if(editor != null)
                fileItem = project.FindFile(editor.Filename);

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if(previewWindow == null)
                {
                    previewWindow = new PreviewTopicWindow();
                    previewWindow.Show(dockPanel);
                }

                previewWindow.PreviewTopic(project, (fileItem == null) ? null : fileItem.FullPath);
                previewWindow.Activate();

                // When the state is restored and it's a document pane, it doesn't always become the active pane
                // unless this is called.
                previewWindow.Show(dockPanel, previewWindow.DockState);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
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
                // Ignore the output and working folders defined in the current build process or project.  This
                // greatly reduces the number of changed files tracked when a build is in progress.
                if(buildProcess != null)
                {
                    try
                    {
                        excludedOutputFolder = buildProcess.OutputFolder;
                        excludedWorkingFolder = buildProcess.WorkingFolder;
                    }
                    catch
                    {
                        // If things are timed just right, it may be possible for the build to complete and be
                        // disposed of before we access the properties.  However, the chances of that are slim
                        // so we'll ignore any errors.
                        excludedOutputFolder = excludedWorkingFolder = "??";
                    }
                }
                else
                {
                    excludedOutputFolder = project.OutputPath;

                    if(String.IsNullOrEmpty(excludedOutputFolder))
                        excludedOutputFolder = Path.Combine(Path.GetDirectoryName(project.Filename), "Help");
                    else
                        excludedOutputFolder = Path.GetFullPath(excludedOutputFolder);

                    if(project.WorkingPath.Path.Length == 0)
                        excludedWorkingFolder = Path.Combine(excludedOutputFolder, "Working");
                    else
                        excludedWorkingFolder = project.WorkingPath;
                }

                if(excludedOutputFolder.EndsWith("\\", StringComparison.Ordinal))
                    excludedOutputFolder = excludedOutputFolder.Substring(0, excludedOutputFolder.Length - 1);

                if(excludedWorkingFolder.EndsWith("\\", StringComparison.Ordinal))
                    excludedWorkingFolder = excludedWorkingFolder.Substring(0, excludedWorkingFolder.Length - 1);

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
                    Debug.Write(ex);
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
        /// Note changes to the file system excluding files in the project's output and working folders that we
        /// don't care about.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void fsw_OnChanged(object sender, FileSystemEventArgs e)
        {
            if(!e.FullPath.StartsWith(excludedOutputFolder, StringComparison.OrdinalIgnoreCase) &&
              !e.FullPath.StartsWith(excludedWorkingFolder, StringComparison.OrdinalIgnoreCase))
                changedFiles.Add(e.FullPath);
        }
        #endregion
    }
}
