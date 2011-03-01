//=============================================================================
// System  : Sandcastle Help File Builder
// File    : OutputWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2010
// Note    : Copyright 2008-2009, Eric Woodruff, All rights reserved
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
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Xml;
using System.Xml.Xsl;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to contain and view the build output
    /// </summary>
    public partial class OutputWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private string logFile;
        private bool logLoaded;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the log filename from the last build
        /// </summary>
        public string LogFile
        {
            get { return logFile; }
            set
            {
                if(value != null)
                    value = Path.GetFullPath(value);

                if(logFile != value)
                {
                    tslLogFile.ToolTipText = logFile = value;

                    if(value == null || value.Length < 51)
                        tslLogFile.Text = value;
                    else
                    {
                        // Try to cut it off at a whole folder name
                        int pos = value.Substring(0,
                            value.Length - 50).LastIndexOf('\\');

                        if(pos == -1)
                            pos = value.Length - 50;
                        else
                            pos++;

                        tslLogFile.Text = "..." + value.Substring(pos);
                    }

                    tcbViewOutput.SelectedIndex = 0;
                    wbLogContent.DocumentText = String.Empty;
                    logLoaded = false;
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

            tsbFilter.Checked = Settings.Default.FilterBuildLog;
            tcbViewOutput.SelectedIndex = 0;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to transform the log file from XML to HTML for viewing
        /// </summary>
        /// <param name="logFile">The log file to transform</param>
        /// <param name="filter">True to filter for warnings and errors only or
        /// false to include all content</param>
        /// <returns>The HTML representing the transformed log file</returns>
        public static string TransformLogFile(string logFile, bool filter)
        {
            XslCompiledTransform xslTransform;
            XsltArgumentList argList;
            XsltSettings settings;
            XmlReaderSettings readerSettings;
            XmlWriterSettings writerSettings;
            XmlReader reader = null;
            XmlWriter writer = null;
            StringReader sr = null;
            StringBuilder sb = null;
            string html = null;

            try
            {
                // Read in the log text we'll prefix it it with the error
                // message if the transform fails.
                using(StreamReader srdr = new StreamReader(logFile))
                {
                    html = srdr.ReadToEnd();
                }

                // Transform the log into something more readable
                readerSettings = new XmlReaderSettings();
                readerSettings.CloseInput = true;

                xslTransform = new XslCompiledTransform();
                settings = new XsltSettings(true, true);

                xslTransform.Load(XmlReader.Create(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location) +
                    @"\Templates\TransformBuildLog.xsl", readerSettings),
                    settings, new XmlUrlResolver());

                argList = new XsltArgumentList();
                argList.AddParam("filterOn", String.Empty, filter ? "true" : "false");

                sr = new StringReader(html);
                reader = XmlReader.Create(sr, readerSettings);
                writerSettings = xslTransform.OutputSettings.Clone();
                writerSettings.CloseOutput = true;
                writerSettings.Indent = false;

                sb = new StringBuilder(10240);
                writer = XmlWriter.Create(sb, writerSettings);
                xslTransform.Transform(reader, argList, writer);

                writer.Flush();
                return sb.ToString();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // Just use the raw data prefixed with the error message
                return String.Format(CultureInfo.CurrentCulture,
                    "<pre><b>An error occurred trying to transform the log " +
                    "file '{0}'</b>:\r\n{1}\r\n\r\n<b>Log Content:</b>\r\n" +
                    "{2}</pre>", logFile, ex.Message,
                    HttpUtility.HtmlEncode(html));
            }
            finally
            {
                if(reader != null)
                    reader.Close();

                if(writer != null)
                    writer.Close();

                if(sr != null)
                    sr.Close();
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Select the output to view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tcbViewOutput_SelectedIndexChanged(object sender,
          EventArgs e)
        {
            if(tcbViewOutput.SelectedIndex == 0)
            {
                wbLogContent.Visible = tsbFilter.Visible = tsSeparator.Visible =
                    tsbPrint.Visible = tslLogFile.Visible = false;
                txtBuildOutput.Visible = true;
                return;
            }

            txtBuildOutput.Visible = false;
            wbLogContent.Visible = tsbFilter.Visible = tsSeparator.Visible =
                tsbPrint.Visible = tslLogFile.Visible = true;
            Application.DoEvents();

            if(!String.IsNullOrEmpty(logFile) && File.Exists(logFile))
            {
                if(!logLoaded)
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        wbLogContent.DocumentText = TransformLogFile(logFile,
                            Settings.Default.FilterBuildLog);
                        logLoaded = true;
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
            }
            else
                wbLogContent.DocumentText = "<pre>There is no log file " +
                    "to view.  Please build the project first.<br/>You may " +
                    "also need to set the <b>KeepLogFile</b> property to " +
                    "true</pre>";
        }

        /// <summary>
        /// Toggle the warnings and errors filter on and off
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbFilter_Click(object sender, EventArgs e)
        {
            tsbFilter.Checked = Settings.Default.FilterBuildLog = !Settings.Default.FilterBuildLog;
            wbLogContent.DocumentText = String.Empty;
            logLoaded = false;
            tcbViewOutput_SelectedIndexChanged(sender, e);
        }

        /// <summary>
        /// Print the information in the browser control
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbPrint_Click(object sender, EventArgs e)
        {
            try
            {
                wbLogContent.Print();
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "An error occurred trying to print the log file: {0}",
                    ex.Message), Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is overridden to ignore Ctrl+F4 which closes the window rather
        /// than hide it when docked as a document.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing &&
              this.DockState == DockState.Document)
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
            wbLogContent.DocumentText = String.Empty;
            logLoaded = false;
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
            tcbViewOutput.Enabled = tsbFilter.Enabled = tsbPrint.Enabled = !isBuilding;
        }
        #endregion
    }
}
