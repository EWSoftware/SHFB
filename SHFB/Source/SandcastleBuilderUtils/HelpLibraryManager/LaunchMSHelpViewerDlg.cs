//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : LaunchMSHelpViewDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/29/2016
// Note    : Copyright 2010-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This form is used to determine the state of the current MS Help Viewer content and offer options to install,
// launch, or remove it.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/05/2010  EFW  Created the code
// 04/02/2011  EFW  Made it project independent so that it could be used in the VSPackage too
// 03/24/2012  EFW  Merged changes from Don Fehr
// 10/05/2012  EFW  Added support for Help Viewer 2.0
// 12/14/2013  EFW  Added support for Help Viewer 2.1
// 03/24/2015  EFW  Added support for Help Viewer 2.2 and added Open Content Manager option
// 08/29/2016  EFW  Added support for Help Viewer 2.3
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This form is used determine the state of the current MS Help Viewer content and offer options to
    /// install it, launch it, remove it, or open the content manager.
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
            Remove,
            /// <summary>Open the content manager</summary>
            OpenContentManager
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
            OpeningContent,
            /// <summary>Opening the content manager</summary>
            OpeningContentManager
        }
        #endregion

        #region Private data members
        //=====================================================================

        private SandcastleProject project;
        private string helpFilePath, setupFile, msHelpViewer, catalogName;
        private BackgroundWorker actionThread;
        private Thread runningThread;
        private Version viewerVersion;

        private static int lastVersionSelected = 3;
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

            // If an external viewer is not defined, we'll launch it via the standard help viewer or the
            // ms-xhelp protocol.
            if(String.IsNullOrWhiteSpace(msHelpViewer) || !File.Exists(msHelpViewer))
                msHelpViewer = null;

            actionThread = new BackgroundWorker { WorkerReportsProgress = true };
            actionThread.DoWork += actionThread_DoWork;
            actionThread.ProgressChanged += actionThread_ProgressChanged;
            actionThread.RunWorkerCompleted += actionThread_RunWorkerCompleted;

            cboHelpViewerVersion.SelectedIndex = lastVersionSelected;
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
                HelpLibraryManager hlm = new HelpLibraryManager(viewerVersion);

                // Remove old content.  We'll remove it if installing to be sure that the latest copy is
                // installed.
                if(action == ThreadAction.Install || action == ThreadAction.Remove)
                {
                    if(action == ThreadAction.Install)
                        actionThread.ReportProgress(0, (int)ThreadState.RemovingOldContent);
                    else
                        actionThread.ReportProgress(0, (int)ThreadState.RemovingContent);

                    if(viewerVersion.Major == 1)
                        arguments = String.Format(CultureInfo.InvariantCulture,
                            "/product \"{0}\" /version \"{1}\" /locale {2} /uninstall /silent /vendor " +
                            "\"{3}\" /productName \"{4}\" /mediaBookList \"{5}\"",
                            project.CatalogProductId, project.CatalogVersion, project.Language.Name,
                            !String.IsNullOrEmpty(project.VendorName) ? project.VendorName : "Vendor Name",
                            !String.IsNullOrEmpty(project.ProductTitle) ? project.ProductTitle : project.HelpTitle,
                            project.HelpTitle);
                    else
                        arguments = String.Format(CultureInfo.InvariantCulture,
                            "/catalogName \"{0}\" /locale {1} /wait 0 /operation uninstall /vendor \"{2}\" " +
                            "/productName \"{3}\" /bookList \"{4}\" ", catalogName, project.Language.Name,
                            !String.IsNullOrEmpty(project.VendorName) ? project.VendorName : "Vendor Name",
                            !String.IsNullOrEmpty(project.ProductTitle) ? project.ProductTitle : project.HelpTitle,
                            project.HelpTitle);

                    // If there are substitution tags present, have a go at resolving them
                    if(arguments.IndexOf("{@", StringComparison.Ordinal) != -1)
                    {
                        try
                        {
                            var bp = new BuildProcess(project);
                            arguments = bp.SubstitutionTags.TransformText(arguments);
                        }
                        catch(Exception ex)
                        {
                            throw new InvalidOperationException("Unable to transform substitution tags: " +
                                ex.Message, ex);
                        }
                    }

                    // This doesn't have to run as an administrator
                    errorCode = hlm.RunAsNormalUser(arguments, ProcessWindowStyle.Minimized);

                    // Ignore it if not found and we are installing
                    if(errorCode != HelpLibraryManagerException.Success &&
                      (errorCode != HelpLibraryManagerException.NoBooksToInstall || action == ThreadAction.Remove))
                        throw new HelpLibraryManagerException(viewerVersion, errorCode);
                }

                if(action == ThreadAction.Install)
                {
                    // Install the new content
                    actionThread.ReportProgress(0, (int)ThreadState.InstallingContent);

                    // Copy the MSHA file to the required name
                    contentSetupFile = Path.Combine(Path.GetDirectoryName(setupFile), "HelpContentSetup.msha");
                    File.Copy(setupFile, contentSetupFile, true);

                    if(viewerVersion.Major == 1)
                        arguments = String.Format(CultureInfo.InvariantCulture, "/product \"{0}\" " +
                            "/version \"{1}\" /locale {2} /sourceMedia \"{3}\"", project.CatalogProductId,
                            project.CatalogVersion, project.Language.Name, contentSetupFile);
                    else
                        arguments = String.Format(CultureInfo.InvariantCulture, "/catalogName \"{0}\" " +
                            "/locale {1} /wait 0 /operation install /sourceUri \"{2}\"", catalogName,
                            project.Language.Name, contentSetupFile);

                    // Always interactive and must run as administrator.  We can't run silently as we don't have
                    // a signed cabinet file.
                    errorCode = hlm.RunAsAdministrator(arguments, ProcessWindowStyle.Normal);

                    if(errorCode != HelpLibraryManagerException.Success)
                        throw new HelpLibraryManagerException(viewerVersion, errorCode);

                    // Open it if installed successfully
                    action = ThreadAction.OpenCurrent;
                }

                if(action == ThreadAction.OpenCurrent)
                {
                    arguments = null;

                    if(msHelpViewer == null)
                    {
                        msHelpViewer = hlm.HelpViewerPath;

                        if(msHelpViewer == null)
                            msHelpViewer = "ms-xhelp:///?method=page&id=-1";
                        else
                            if(viewerVersion.Major == 2)
                                arguments = "/catalogname \"" + catalogName + "\"";
                    }

                    actionThread.ReportProgress(0, (int)ThreadState.OpeningContent);
                    System.Diagnostics.Process.Start(msHelpViewer, arguments);
                }

                if(action == ThreadAction.OpenContentManager)
                {
                    actionThread.ReportProgress(0, (int)ThreadState.OpeningContentManager);

                    // Can't do anything if the Help Library Manager is not installed
                    if(hlm.HelpLibraryManagerPath == null)
                        throw new HelpLibraryManagerException(viewerVersion,
                            HelpLibraryManagerException.HelpLibraryManagerNotFound);

                    if(viewerVersion.Major == 1)
                        hlm.LaunchInteractive(String.Format(CultureInfo.InvariantCulture,
                            "/product \"{0}\" /version \"{1}\" /locale {2}", project.CatalogProductId,
                            project.CatalogVersion, project.Language.Name));
                    else
                        hlm.LaunchInteractive(String.Format(CultureInfo.InvariantCulture,
                            "/catalogName \"{0}\" /locale {1} /manage", catalogName, project.Language.Name));
                }
            }
            catch(ThreadAbortException)
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
                cboHelpViewerVersion.Enabled = grpOptions.Enabled = btnOK.Enabled = true;

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
                            rbLaunchContentManager.Checked = true;
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
        /// This is used to determine the state of the help content and set the form options when a help viewer
        /// version is selected.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboHelpViewerVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtInfo.Text = null;

            grpOptions.Enabled = rbInstall.Enabled = true;
            lastVersionSelected = cboHelpViewerVersion.SelectedIndex;

            // If there are substitution tags present, have a go at resolving them
            if(helpFilePath.IndexOf("{@", StringComparison.Ordinal) != -1)
            {
                try
                {
                    var bp = new BuildProcess(project);
                    helpFilePath = bp.SubstitutionTags.TransformText(helpFilePath);
                    setupFile = Path.ChangeExtension(helpFilePath, ".msha");
                }
                catch
                {
                    // Ignore errors
                    txtInfo.AppendText("The help filename appears to contain substitution tags but they could " +
                        "not be resolved to determine the actual file to use for installation.  Building " +
                        "website output and viewing it can be used to work around this issue.\r\n\r\n");
                    rbInstall.Enabled = false;
                }
            }

            if(rbInstall.Enabled && (!File.Exists(helpFilePath) || !File.Exists(setupFile)))
            {
                txtInfo.AppendText("A copy of the help file does not appear to exist yet.  It may need to be built.\r\n\r\n");
                rbInstall.Enabled = false;
            }

            try
            {
                viewerVersion = new Version((string)cboHelpViewerVersion.SelectedItem);

                HelpLibraryManager hlm = new HelpLibraryManager(viewerVersion);

                // Can't do anything if the Help Library Manager is not installed
                if(hlm.HelpLibraryManagerPath == null)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.HelpLibraryManagerNotFound);

                // Can't do anything if the Help Library Manager is already running
                if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(hlm.HelpLibraryManagerPath)).Length > 0)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.HelpLibraryManagerAlreadyRunning);

                // Can't do anything if the local store is not initialized
                if(!hlm.LocalStoreInitialized)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.LocalStoreNotInitialized);

                if(hlm.HelpContentFileInstalled(helpFilePath))
                    rbOpenCurrent.Enabled = rbRemove.Enabled = true;
                else
                {
                    txtInfo.AppendText("The help file does not appear to be installed yet.\r\n");
                    rbOpenCurrent.Enabled = rbRemove.Enabled = false;
                }
            }
            catch(Exception ex)
            {
                txtInfo.AppendText("Problem: " + ex.Message + "\r\n");
                rbOpenCurrent.Enabled = rbRemove.Enabled = false;
            }

            if(rbOpenCurrent.Enabled)
                rbOpenCurrent.Checked = true;
            else
                if(rbInstall.Enabled)
                    rbInstall.Checked = true;
                else
                    rbLaunchContentManager.Checked = true;

            if(!rbOpenCurrent.Enabled && !rbInstall.Enabled && !rbRemove.Enabled)
                txtInfo.AppendText("\r\nNo action can be taken with the help content.");

            // Determine the catalog name here as it's used in a lot of places and varies by version if not
            // defined in the project.
            catalogName = !String.IsNullOrWhiteSpace(project.CatalogName) ? project.CatalogName :
                HelpLibraryManager.DefaultCatalogName(viewerVersion);

            // If it looks like a default value, warn the user if it doesn't match.  It may need to be cleared.
            if(!String.IsNullOrWhiteSpace(project.CatalogName) && project.CatalogName.StartsWith("VisualStudio",
              StringComparison.Ordinal) && project.CatalogName != HelpLibraryManager.DefaultCatalogName(viewerVersion))
                txtInfo.AppendText("\r\n\r\nWARNING:  The project's catalog name property is set to '" +
                    project.CatalogName + "' which does not match the default catalog name for the selected " +
                    "version of the help viewer.  If necessary, clear the catalog name property value.");
        }

        /// <summary>
        /// Execute the selected action
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            ThreadAction action;

            txtInfo.Text = null;

            try
            {
                HelpLibraryManager hlm = new HelpLibraryManager(viewerVersion);

                // Can't do anything if the Help Library Manager is already running
                if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(hlm.HelpLibraryManagerPath)).Length > 0)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.HelpLibraryManagerAlreadyRunning);
            }
            catch(Exception ex)
            {
                txtInfo.AppendText("Problem: " + ex.Message + "\r\n");
                return;
            }

            cboHelpViewerVersion.Enabled = grpOptions.Enabled = btnOK.Enabled = false;

            if(rbOpenCurrent.Checked)
                action = ThreadAction.OpenCurrent;
            else
                if(rbInstall.Checked)
                    action = ThreadAction.Install;
                else
                    if(rbRemove.Checked)
                        action = ThreadAction.Remove;
                    else
                        action = ThreadAction.OpenContentManager;

            lblAction.Text = "Please wait...";
            pbWait.Visible = lblAction.Visible = true;
            actionThread.RunWorkerAsync(action);
        }
        #endregion
    }
}
