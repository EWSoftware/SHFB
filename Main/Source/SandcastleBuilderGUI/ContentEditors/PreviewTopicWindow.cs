//=============================================================================
// System  : Sandcastle Help File Builder
// File    : PreviewTopicWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/15/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to preview a topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/27/2008  EFW  Created the code
// 1.8.0.0  07/26/2008  EFW  Reworked for use with the new project format
// 1.9.0.0  06/07/2010  EFW  Added support for multi-format build output
//=============================================================================

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.PlugIn;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to preview a topic
    /// </summary>
    public partial class PreviewTopicWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject, tempProject;
        private Thread buildThread;
        private BuildProcess buildProcess;
        private BuildStep lastBuildStep;
        private FileItem fileItem;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to determine whether or not the
        /// preview window can be closed.
        /// </summary>
        public override bool CanClose
        {
            get
            {
                FormClosingEventArgs e = new FormClosingEventArgs(CloseReason.UserClosing, false);

                this.PreviewTopicWindow_FormClosing(null, e);
                return !e.Cancel;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PreviewTopicWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Build the conceptual content and preview the specified file
        /// </summary>
        /// <param name="project">The current project</param>
        /// <param name="preview">The file to preview</param>
        public void PreviewTopic(SandcastleProject project, FileItem preview)
        {
            // If a build is already in progress, let it finish
            if(buildProcess == null)
            {
                currentProject = project;
                fileItem = preview;
                wbPreview.Navigate("about:blank");
                lblLoading.Text = "Building...";
                pbWait.Visible = lblLoading.Visible = true;
                timer.Start();
                this.BuildConceptualTopics();
            }
        }
        #endregion

        #region Build methods
        //=====================================================================

        /// <summary>
        /// This kicks off the build process in a background thread
        /// </summary>
        private void BuildConceptualTopics()
        {
            PlugInConfiguration pc;
            string tempPath;

            try
            {
                // Set up the project using information from the current project
                tempProject = new SandcastleProject(currentProject);

                // The temporary project resides in the same folder as the current project (by filename
                // only, it isn't saved) to maintain relative paths.  However, build output is stored
                // in a temporary folder and it keeps the intermediate files.
                tempProject.CleanIntermediates = false;
                tempPath = Path.GetTempFileName();

                File.Delete(tempPath);
                tempPath = Path.Combine(Path.GetDirectoryName(tempPath), "SHFBPartialBuild");

                if(!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                tempProject.OutputPath = tempPath;

                // Force website output so that we know where to find the output
                tempProject.HelpFileFormat = HelpFileFormat.Website;

                // Add the Additional Content Only plug-in or update the it if
                // already there to only do a preview build.
                if(tempProject.PlugInConfigurations.TryGetValue("Additional Content Only", out pc))
                {
                    pc.Enabled = true;
                    pc.Configuration = "<configuration previewBuild='true' />";
                }
                else
                    tempProject.PlugInConfigurations.Add("Additional Content Only", true,
                        "<configuration previewBuild='true' />");

                buildProcess = new BuildProcess(tempProject);
                buildProcess.BuildStepChanged += buildProcess_BuildStepChanged;

                buildThread = new Thread(new ThreadStart(buildProcess.Build));
                buildThread.Name = "Help file builder thread";
                buildThread.IsBackground = true;
                buildThread.Start();
            }
            catch(Exception ex)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(currentProject.Filename));

                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to build project to preview topic.  " + "Error: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender, BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down or it hasn't
                // completed yet.
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(buildProcess_BuildStepChanged),
                        new object[] { sender, e });
            }
            else
            {
                lblLoading.Text = e.BuildStep.ToString();

                if(e.HasCompleted)
                {
                    // Restore the current project's base path
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(currentProject.Filename));
                    lastBuildStep = e.BuildStep;
                }
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to ignore Ctrl+F4 which closes the window rather
        /// than hide it when docked as a document.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing && this.DockState == DockState.Document)
            {
                this.Hide();
                e.Cancel = true;
            }
            else
                base.OnFormClosing(e);
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Shut down the build process thread and clean up on exit
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void PreviewTopicWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(buildThread != null && buildThread.IsAlive)
            {
                if(MessageBox.Show("A build is currently taking place to " +
                  "preview a topic.  Do you want to abort it and close this " +
                  "form?", Constants.AppName, MessageBoxButtons.YesNo,
                  MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    timer.Stop();

                    if(buildThread != null)
                        buildThread.Abort();

                    while(buildThread != null && !buildThread.Join(1000))
                        Application.DoEvents();

                    System.Diagnostics.Debug.WriteLine("Thread stopped");
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    buildThread = null;
                    buildProcess = null;
                }
            }

            if(sender == this && tempProject != null)
            {
                try
                {
                    // Delete the temporary project's working files
                    if(!String.IsNullOrEmpty(tempProject.OutputPath) && Directory.Exists(tempProject.OutputPath))
                        Directory.Delete(tempProject.OutputPath, true);
                }
                catch
                {
                    // Eat the exception.  We'll ignore it if the temporary files cannot be deleted.
                }

                tempProject.Dispose();
                tempProject = null;

                GC.Collect(2);
                GC.WaitForPendingFinalizers();
                GC.Collect(2);
            }
        }

        /// <summary>
        /// When the build finishes, load the topic if it was built
        /// successfully or view the log if the build failed.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>We have to wait for the thread to finish before we can
        /// view the log file so we use a timer event to wait for it.</remarks>
        private void timer_Tick(object sender, EventArgs e)
        {
            TopicFile topicFile;
            string path = null;

            if(buildThread != null && !buildThread.IsAlive)
            {
                timer.Stop();

                // If successful, preview the built topic
                if(lastBuildStep == BuildStep.Completed)
                {
                    // If it's a MAML topic or an HTML file with an ID, use
                    // the ID for the name.
                    topicFile = new TopicFile(fileItem);

                    if(!String.IsNullOrEmpty(topicFile.Id))
                        path = String.Concat(buildProcess.WorkingFolder,
                            @"Output\Website\html\", topicFile.Id, ".htm");
                    else
                    {
                        // If not, try to find it by name
                        path = Directory.EnumerateFiles(buildProcess.WorkingFolder + @"Output\Website\",
                            Path.GetFileNameWithoutExtension(fileItem.Name) + ".htm?",
                            SearchOption.AllDirectories).FirstOrDefault();

                        if(path == null)
                            path = fileItem.FullPath;
                    }

                    if(File.Exists(path))
                        wbPreview.Navigate(path);
                    else
                        wbPreview.DocumentText = "Unable to locate built topic file: " + path +
                            "<br/><br/>Does the project contain a content layout file with a " +
                            "<b>BuildAction</b> of <b>ContentLayout</b>?";
                }
                else
                {
                    // Show the raw log output for now.  This will be going away once the new previewer is done.
                    using(StreamReader sr = new StreamReader(buildProcess.LogFilename))
                    {
                        wbPreview.DocumentText = "<pre>" + sr.ReadToEnd() + "</pre>";
                    }
                }

                pbWait.Visible = lblLoading.Visible = false;
                buildThread = null;
                buildProcess = null;
            }
        }
        #endregion
    }
}
