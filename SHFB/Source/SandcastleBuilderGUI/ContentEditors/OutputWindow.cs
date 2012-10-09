//=============================================================================
// System  : Sandcastle Help File Builder
// File    : OutputWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/07/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to contain and view the build output
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/26/2008  EFW  Created the code
// 1.8.0.3  12/30/2009  EFW  Added option to filter for warnings and errors
// 1.9.3.4  01/07/2012  EFW  Replaced the web browser with a common user
//                           control shared by this and the standalone GUI.
//=============================================================================

using System;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.WPF.UserControls;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to contain and view the build output
    /// </summary>
    public partial class OutputWindow : BaseContentEditor
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the log filename from the last build
        /// </summary>
        public string LogFile
        {
            get { return ucBuildLogViewer.LogFilename; }
            set
            {
                if(value != null)
                    value = Path.GetFullPath(value);

                if(ucBuildLogViewer.LogFilename != value)
                {
                    tslLogFile.ToolTipText = ucBuildLogViewer.LogFilename = value;

                    if(value == null || value.Length < 51)
                        tslLogFile.Text = value;
                    else
                    {
                        // Try to cut it off at a whole folder name
                        int pos = value.Substring(0, value.Length - 50).LastIndexOf('\\');

                        if(pos == -1)
                            pos = value.Length - 50;
                        else
                            pos++;

                        tslLogFile.Text = "..." + value.Substring(pos);
                    }

                    tcbViewOutput.SelectedIndex = 0;
                }
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public OutputWindow()
        {
            InitializeComponent();

            txtBuildOutput.BackColor = Settings.Default.BuildOutputBackground;
            txtBuildOutput.ForeColor = Settings.Default.BuildOutputForeground;
            txtBuildOutput.Font = Settings.Default.BuildOutputFont;

            tcbViewOutput.SelectedIndex = 0;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Select the output to view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tcbViewOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tcbViewOutput.SelectedIndex == 0)
            {
                ehLogViewer.Visible = tslLogFile.Visible = false;
                txtBuildOutput.Visible = true;
            }
            else
            {
                txtBuildOutput.Visible = false;
                ehLogViewer.Visible = tslLogFile.Visible = true;
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is overridden to Reset the build log control when hidden to work around an odd crash
        /// related to the embedded browser control.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Odd error.  The web browser embedded in the user control fails randomly after being hidden and
            // reshown.  As such, just create a new instance of the control when hidden to work around it.
            if(!base.Visible)
                ehLogViewer.Child = ucBuildLogViewer = new BuildLogViewerControl();
        }

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

        /// <summary>
        /// Update the window settings based on the user preferences
        /// </summary>
        public void UpdateSettings()
        {
            txtBuildOutput.BackColor = Settings.Default.BuildOutputBackground;
            txtBuildOutput.ForeColor = Settings.Default.BuildOutputForeground;
            txtBuildOutput.Font = Settings.Default.BuildOutputFont;
        }

        /// <summary>
        /// Reset the log viewer ready for a new build
        /// </summary>
        public void ResetLogViewer()
        {
            txtBuildOutput.Text = null;
            ucBuildLogViewer.LogFilename = null;
            tcbViewOutput.SelectedIndex = 0;
        }

        /// <summary>
        /// View the build output
        /// </summary>
        public void ViewBuildOutput()
        {
            tcbViewOutput.SelectedIndex = 0;
            this.Activate();
        }

        /// <summary>
        /// View the specified log file
        /// </summary>
        /// <param name="logFilename">The log file to view</param>
        public void ViewLogFile(string logFilename)
        {
            this.LogFile = logFilename;
            tcbViewOutput.SelectedIndex = 1;
            this.Activate();
        }

        /// <summary>
        /// Append text to the build output
        /// </summary>
        /// <param name="text">The text to append</param>
        /// <remarks>A carriage return and line feed are added
        /// automatically.</remarks>
        public void AppendText(string text)
        {
            txtBuildOutput.AppendText(text + "\r\n");
        }

        /// <summary>
        /// Enable or disable based on the build state
        /// </summary>
        /// <param name="isBuilding">True if building, false if not</param>
        public void SetBuildState(bool isBuilding)
        {
            tcbViewOutput.Enabled = !isBuilding;
        }
        #endregion
    }
}
