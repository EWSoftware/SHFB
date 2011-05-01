//=============================================================================
// System  : Sandcastle Help File Builder
// File    : LaunchMSHelpViewDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/02/2011
// Note    : Copyright 2010-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This form is used to determine the state of the current MS Help Viewer
// content and offer options to install, launch, or remove it.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.0.0  07/05/2010  EFW  Created the code
// 1.9.3.0  04/02/2011  EFW  Made it project independent so that it could be
//                           used in the VSPackage too.
//=============================================================================

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This form is used determine the state of the current MS Help Viewer
    /// content and offer options to install, launch, or remove it.
    /// </summary>
    public partial class LaunchMSHelpViewerDlg : Form
    {
        #region Thread actions
        //=====================================================================

        /// <summary>
        /// This is used to tell the thread what to do
        /// </summary>
        private enum ThreadAction
        {
            /// <summary>Open the current content</summary>
            OpenCurrent,
            /// <summary>Install content from the last build and open it</summary>
            Install,
            /// <summary>Remove the current content</summary>
            Remove
        }
        #endregion

        #region Thread state
        //=====================================================================

        /// <summary>
        /// This is used to indicate the current state of the thread
        /// </summary>
        private enum ThreadState
        {
            /// <summary>Removing old content prior to installing new content</summary>
            RemovingOldContent,
            /// <summary>Content is being removed</summary>
            RemovingContent,
            /// <summary>Content is being installed</summary>
            InstallingContent,
            /// <summary>Content is being opened</summary>
            OpeningContent
        }
        #endregion

        #region Private data members
        //=====================================================================

        private SandcastleProject project;
        private string helpFilePath, setupFile, msHelpViewer;
        private BackgroundWorker actionThread;
        private Thread runningThread;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentProject">The current project</param>
        /// <param name="helpViewerPath">The path to the MS Help Viewer</param>
        public LaunchMSHelpViewerDlg(SandcastleProject currentProject, string helpViewerPath)
        {
            InitializeComponent();

            project = currentProject;

            // Make sure we start out in the project's output folder in case the output folder
            // is relative to it.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));

            helpFilePath = project.OutputPath;
            msHelpViewer = helpViewerPath;

            if(String.IsNullOrEmpty(helpFilePath))
                helpFilePath = Directory.GetCurrentDirectory();
            else
                helpFilePath = Path.GetFullPath(helpFilePath);

            helpFilePath += project.HtmlHelpName + ".mshc";
            setupFile = Path.ChangeExtension(helpFilePath, ".msha");

            // If an external viewer is not defined, we'll launch it via the ms-xhelp protocol
            if(msHelpViewer.Length == 0 || !File.Exists(msHelpViewer))
                msHelpViewer = "ms-xhelp:///?method=page&id=-1&topicversion=100&product=vs&productversion=100&locale=" +
                    project.Language.Name;

            actionThread = new BackgroundWorker { WorkerReportsProgress = true };
            actionThread.DoWork += actionThread_DoWork;
            actionThread.ProgressChanged += actionThread_ProgressChanged;
            actionThread.RunWorkerCompleted += actionThread_RunWorkerCompleted;
        }
        #endregion

        #region Background worker thread helper methods and event handlers
        //=====================================================================

        /// <summary>
        /// This performs the requested task
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void actionThread_DoWork(object sender, DoWorkEventArgs e)
        {
            ThreadAction action = (ThreadAction)e.Argument;
            string arguments, contentSetupFile;
            int errorCode;

            runningThread = Thread.CurrentThread;

            try
            {
                HelpLibraryManager hlm = new HelpLibraryManager();

                // Remove old content.  We'll remove it if installing to be sure that
                // the latest copy is installed.
                if(action == ThreadAction.Install || action == ThreadAction.Remove)
                {
                    if(action == ThreadAction.Install)
                        actionThread.ReportProgress(0, (int)ThreadState.RemovingOldContent);
                    else
                        actionThread.ReportProgress(0, (int)ThreadState.RemovingContent);

                    arguments = String.Format(CultureInfo.InvariantCulture,
                        "/product \"{0}\" /version \"{1}\" /locale {2} /uninstall /silent /vendor " +
                        "\"{3}\" /mediaBookList \"{4}\" /productName \"{5}\"",
                        project.CatalogProductId, project.CatalogVersion, project.Language.Name,
                        project.VendorName, project.HelpTitle, project.ProductTitle);

                    // This doesn't have to run as an administrator
                    errorCode = hlm.RunAsNormalUser(arguments, ProcessWindowStyle.Minimized);

                    // Ignore it if not found and we are installing
                    if(errorCode != HelpLibraryManagerException.Success &&
                      (errorCode != HelpLibraryManagerException.NoBooksToInstall || action == ThreadAction.Remove))
                        throw new HelpLibraryManagerException(errorCode);
                }

                if(action == ThreadAction.Install)
                {
                    // Install the new content
                    actionThread.ReportProgress(0, (int)ThreadState.InstallingContent);

                    // Copy the MSHA file to the required name
                    contentSetupFile = Path.Combine(Path.GetDirectoryName(setupFile), "HelpContentSetup.msha");
                    File.Copy(setupFile, contentSetupFile, true);

                    arguments = String.Format(CultureInfo.InvariantCulture,
                        "/product \"{0}\" /version \"{1}\" /locale {2} /brandingPackage Dev10.mshc " +
                        "/sourceMedia \"{3}", project.CatalogProductId, project.CatalogVersion, project.Language.Name,
                        contentSetupFile);

                    // Always interactive and must run as administrator.  We can't run
                    // silently as we don't have a signed cabinet file.
                    errorCode = hlm.RunAsAdministrator(arguments, ProcessWindowStyle.Normal);

                    if(errorCode != HelpLibraryManagerException.Success)
                        throw new HelpLibraryManagerException(errorCode);

                    // Open it if installed successfully
                    action = ThreadAction.OpenCurrent;
                }

                if(action == ThreadAction.OpenCurrent)
                {
                    actionThread.ReportProgress(0, (int)ThreadState.OpeningContent);
                    System.Diagnostics.Process.Start(msHelpViewer);
                }
            }
            catch(ThreadAbortException )
            {
                // Ignore thread abort exceptions
            }
        }

        /// <summary>
        /// This updates the status as the thread runs
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void actionThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch((ThreadState)e.UserState)
            {
                case ThreadState.RemovingOldContent:
                    lblAction.Text = "Removing old help content...";
                    break;

                case ThreadState.InstallingContent:
                    lblAction.Text = "Installing help content...";
                    break;

                case ThreadState.RemovingContent:
                    lblAction.Text = "Removing help content...";
                    break;

                default:
                    lblAction.Text = "Opening help content...";
                    break;
            }
        }

        /// <summary>
        /// This enables the form when the thread finishes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void actionThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            runningThread = null;

            pbWait.Visible = lblAction.Visible = false;

            if(!this.IsDisposed)
            {
                grpOptions.Enabled = btnOK.Enabled = true;

                if(e.Error != null)
                    txtInfo.AppendText("ERROR: " + e.Error.Message);
                else
                {
                    if(rbRemove.Checked)
                    {
                        txtInfo.AppendText("The operation completed successfully");
                        rbRemove.Enabled = rbOpenCurrent.Enabled = false;

                        if(rbInstall.Enabled)
                            rbInstall.Checked = true;
                        else
                            btnOK.Enabled = false;
                    }
                    else
                        this.Close();   // Close if content was opened
                }
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(actionThread.IsBusy && runningThread != null)
                runningThread.Abort();

            this.Close();
        }

        /// <summary>
        /// This is used to determine the state of the help content and set
        /// the form options.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void LaunchMSHelpViewerDlg_Load(object sender, EventArgs e)
        {
            if(!File.Exists(helpFilePath) || !File.Exists(setupFile))
            {
                txtInfo.AppendText("A copy of the help file does not appear to exist yet.  It may need to be built.\r\n");
                rbInstall.Enabled = false;
            }

            try
            {
                HelpLibraryManager hlm = new HelpLibraryManager();

                // Can't do anything if the Help Library Manager is not installed
                if(hlm.HelpLibraryManagerPath == null)
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.HelpLibraryManagerNotFound);

                // Can't do anything if the Help Library Manager is already running
                if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(hlm.HelpLibraryManagerPath)).Length > 0)
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.HelpLibraryManagerAlreadyRunning);

                // Can't do anything if the local store is not initialized
                if(!hlm.LocalStoreInitialized)
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.LocalStoreNotInitialized);

                if(!hlm.HelpContentFileInstalled(helpFilePath))
                {
                    txtInfo.AppendText("The help file does not appear to be installed yet.\r\n");
                    rbOpenCurrent.Enabled = rbRemove.Enabled = false;
                }

                grpOptions.Enabled = true;
                btnOK.Enabled = (rbOpenCurrent.Enabled || rbInstall.Enabled || rbRemove.Enabled);
            }
            catch(Exception ex)
            {
                txtInfo.AppendText("Problem: " + ex.Message + "\r\n");
            }

            if(!btnOK.Enabled)
                txtInfo.AppendText("\r\nNo action can be taken.");
        }

        /// <summary>
        /// Execute the selected action
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            ThreadAction action;

            grpOptions.Enabled = btnOK.Enabled = false;

            if(rbOpenCurrent.Checked)
                action = ThreadAction.OpenCurrent;
            else
                if(rbInstall.Checked)
                    action = ThreadAction.Install;
                else
                    action = ThreadAction.Remove;

            lblAction.Text = "Please wait...";
            pbWait.Visible = lblAction.Visible = true;
            actionThread.RunWorkerAsync(action);
        }
        #endregion
    }
}
