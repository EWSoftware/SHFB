//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : BuildLogViewerControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/07/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to view the build log content.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/05/2012  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This control is used to view the help file builder log file content
    /// </summary>
    public partial class BuildLogViewerControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private string logFilename;
        private bool delayedLoad;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the log filename
        /// </summary>
        public string LogFilename
        {
            get { return logFilename; }
            set
            {
                logFilename = null;

                // Revert to plain text if reset
                rbPlain.IsChecked = true;

                logFilename = value;

                if(this.IsLoaded && this.IsVisible)
                    this.LoadLogFile();
                else
                    delayedLoad = true;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildLogViewerControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load a log file
        /// </summary>
        private void LoadLogFile()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                wbContent.NavigateToString(" ");    // Must use a space.  It doesn't like String.Empty

                if(!String.IsNullOrEmpty(logFilename) && File.Exists(logFilename))
                    wbContent.NavigateToString(TransformLogFile(logFilename, rbFilter.IsChecked.Value,
                        rbHighlight.IsChecked.Value));
                else
                    wbContent.NavigateToString(String.Format("<div style='font-family: Arial; font-size: 9pt'>" +
                        "Log File: {0}<br /><br />There is no log file to view.  Please build the project " +
                        "first.  You may also need to enable the <b>Keep log file after successful build " +
                        "(KeepLogFile)</b> project property.</div>", (logFilename == null) ? "(Build cleaned)" :
                        logFilename));
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                wbContent.NavigateToString(String.Format("<div style='font-family: Arial; font-size: 9pt'>" +
                    "Log File: {0}<br /><br />Unable to load log file: {1}</div>", logFilename, ex.Message));
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// This is used to transform the log file from XML to HTML for viewing
        /// </summary>
        /// <param name="logFile">The log file to transform</param>
        /// <param name="filtered">True to only show warnings and errors, false to show all messages</param>
        /// <param name="highlight">True to highlight warnings and errors in the full log text or false to
        /// just show the plain text.</param>
        /// <returns>The HTML representing the transformed log file</returns>
        private static string TransformLogFile(string logFile, bool filtered, bool highlight)
        {
            StringBuilder sb = new StringBuilder(10240);
            string html = null;

            try
            {
                // Read in the log text.  We'll prefix it it with the error message if the transform fails.
                using(StreamReader sr = new StreamReader(logFile))
                {
                    html = sr.ReadToEnd();
                }

                // Transform the log into something more readable
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.CloseInput = true;

                XslCompiledTransform xslTransform = new XslCompiledTransform();
                XsltSettings settings = new XsltSettings(true, true);

                xslTransform.Load(XmlReader.Create(Path.Combine(Environment.ExpandEnvironmentVariables("%SHFBROOT%"),
                    @"Templates\TransformBuildLog.xsl"), readerSettings), settings, new XmlUrlResolver());

                XsltArgumentList argList = new XsltArgumentList();
                argList.AddParam("filterOn", String.Empty, filtered ? "true" : "false");
                argList.AddParam("highlightOn", String.Empty, highlight ? "true" : "false");

                using(StringReader sr = new StringReader(html))
                {
                    using(XmlReader reader = XmlReader.Create(sr, readerSettings))
                    {
                        XmlWriterSettings writerSettings = xslTransform.OutputSettings.Clone();
                        writerSettings.CloseOutput = true;
                        writerSettings.Indent = false;

                        using(XmlWriter writer = XmlWriter.Create(sb, writerSettings))
                        {
                            xslTransform.Transform(reader, argList, writer);
                            writer.Flush();
                        }
                    }
                }

                return sb.ToString();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // Just use the raw data prefixed with the error message
                return String.Format(CultureInfo.CurrentCulture, "<pre><b>An error occurred trying to transform the log " +
                    "file '{0}'</b>:\r\n{1}\r\n\r\n<b>Log Content:</b>\r\n{2}</pre>", logFile, ex.Message,
                    WebUtility.HtmlEncode(html));
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This loads the log file when the control is first made visible
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucBuildLogViewerControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.IsVisible && (delayedLoad || (!String.IsNullOrEmpty(logFilename) && wbContent.Document == null)))
            {
                wbContent.NavigateToString(" ");
                delayedLoad = true;
            }
        }

        /// <summary>
        /// Reload the build log when the filter type changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            if(this.IsLoaded)
                this.LoadLogFile();
        }

        /// <summary>
        /// This loads the content once the control is actually visible.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>If you attempt to load the content before the control is actually visible, it will not
        /// get replaced by a blank page.  By delaying it, we get the content we want.</remarks>
        private void wbContent_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if(delayedLoad)
            {
                this.LoadLogFile();
                delayedLoad = false;
            }
        }
        #endregion
    }
}
