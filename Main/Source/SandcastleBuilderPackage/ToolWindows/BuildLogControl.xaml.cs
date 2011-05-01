//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildLogControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/23/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to view the build log content.  This will
// eventually be replaced by something with more features such as search,
// item lookup, etc.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  04/22/2011  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Xsl;

namespace SandcastleBuilder.Package.ToolWindows
{
    /// <summary>
    /// This is used to view the build log content.
    /// </summary>
    /// <remarks>This is a temporary viewer for the log.  It will eventually be
    /// replaced by something with more features such as search, item lookup, etc.</remarks>
    public partial class BuildLogControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private string logFilename;
        private bool delayedLoad, isFiltered;
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
                logFilename = value;
                this.LoadLogFile();
            }
        }

        /// <summary>
        /// This is used to get whether or not the log is filtered to only show warnings and errors
        /// </summary>
        public bool IsFiltered
        {
            get { return isFiltered; }
            set
            {
                if(isFiltered != value)
                {
                    isFiltered = value;
                    
                    if(!String.IsNullOrEmpty(logFilename))
                        this.LoadLogFile();
                }
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildLogControl()
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
            if(!this.IsLoaded)
            {
                delayedLoad = true;
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                wbContent.NavigateToString(" ");    // Must use a space.  It doesn't like String.Empty

                if(!String.IsNullOrEmpty(logFilename) && File.Exists(logFilename))
                {
                    using(StreamReader sr = new StreamReader(logFilename, isFiltered))
                    {
                        wbContent.NavigateToString(TransformLogFile(logFilename, isFiltered));
                    }
                }
                else
                    wbContent.NavigateToString("<pre>There is no log file to view.  Please build the project " +
                        "first.<br/>You may also need to enable the <b>Keep log file after successful build</b> " +
                        "property.</pre>");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                wbContent.NavigateToString("Unable to load log file: " + ex.Message);
            }
            finally
            {
                this.Cursor = null;
            }
        }

        /// <summary>
        /// This is used to transform the log file from XML to HTML for viewing
        /// </summary>
        /// <param name="logFile">The log file to transform</param>
        /// <param name="filtered">True to only show warnings and errors, false to show all messages</param>
        /// <returns>The HTML representing the transformed log file</returns>
        private static string TransformLogFile(string logFile, bool filtered)
        {
            StringBuilder sb = new StringBuilder(10240);
            string html = null;

            try
            {
                // Read in the log text.  We'll prefix it it with the error message if the transform fails.
                using(StreamReader srdr = new StreamReader(logFile))
                {
                    html = srdr.ReadToEnd();
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
        /// This loads blank content to force a load of the actual file when the control is actually visible
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>If the log content is loaded before the control is loaded,
        /// it disappears on first use.</remarks>
        private void BuildLog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(wbContent.Document == null || delayedLoad)
                wbContent.NavigateToString(" ");
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
