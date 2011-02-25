//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : CompletionNotificationPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/12/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to run after the build completes to
// send notification of the completion status via e-mail.  The log file can
// be sent as an attachment.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/09/2007  EFW  Created the code
// 1.6.0.6  03/09/2008  EFW  Added support for log file XSL transform
//=============================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to run after the build completes to
    /// send notification of the completion status via e-mail.  The log file
    /// can be sent as an attachment.
    /// </summary>
    public class CompletionNotificationPlugIn : IPlugIn
    {
        #region Private data members
        private ExecutionPointCollection executionPoints;

        private BuildProcess builder;
        private bool attachLogOnSuccess, attachLogOnFailure;
        private string smtpServer, fromEMailAddress, successEMailAddress,
            failureEMailAddress, xslTransformFile;
        private UserCredentials credentials;
        private int smtpPort;
        #endregion

        #region IPlugIn implementation
        //=====================================================================
        // IPlugIn implementation

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Completion Notification"; }
        }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        public Version Version
        {
            get
            {
                // Use the assembly version
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(
                    asm.Location);

                return new Version(fvi.ProductVersion);
            }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the
        /// plug-in.
        /// </summary>
        public string Copyright
        {
            get
            {
                // Use the assembly copyright
                Assembly asm = Assembly.GetExecutingAssembly();
                AssemblyCopyrightAttribute copyright =
                    (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                        asm, typeof(AssemblyCopyrightAttribute));

                return copyright.Copyright;
            }
        }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        public string Description
        {
            get
            {
                return "This plug-in is used to send notification of the " +
                    "build completion status via e-mail.  The log file can " +
                    "be sent as an attachment.";
            }
        }

        /// <summary>
        /// This plug-in does not run in partial builds
        /// </summary>
        public bool RunsInPartialBuild
        {
            get { return false; }
        }

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public ExecutionPointCollection ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                {
                    executionPoints = new ExecutionPointCollection();

                    executionPoints.Add(new ExecutionPoint(
                        BuildStep.Completed, ExecutionBehaviors.After));
                    executionPoints.Add(new ExecutionPoint(
                        BuildStep.Canceled, ExecutionBehaviors.After));
                    executionPoints.Add(new ExecutionPoint(
                        BuildStep.Failed, ExecutionBehaviors.After));
                }

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project,
          string currentConfig)
        {
            using(CompletionNotificationConfigDlg dlg =
              new CompletionNotificationConfigDlg(currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in
        /// configuration is not valid.</exception>
        public void Initialize(BuildProcess buildProcess,
          XPathNavigator configuration)
        {
            XPathNavigator root, node;
            string value;

            builder = buildProcess;
            attachLogOnSuccess = false;
            attachLogOnFailure = true;
            smtpServer = successEMailAddress = failureEMailAddress =
                xslTransformFile = String.Empty;
            credentials = new UserCredentials();
            smtpPort = 25;

            builder.ReportProgress("{0} Version {1}\r\n{2}",
                this.Name, this.Version, this.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("CNP0001", "The Completion " +
                    "Notification plug-in has not been configured yet");

            node = root.SelectSingleNode("smtpServer");
            if(node != null)
            {
                smtpServer = node.GetAttribute("host", String.Empty).Trim();
                value = node.GetAttribute("port", String.Empty);

                if(!Int32.TryParse(value, out smtpPort))
                    smtpPort = 25;
            }

            credentials = UserCredentials.FromXPathNavigator(root);

            node = root.SelectSingleNode("fromEMail");
            if(node != null)
                fromEMailAddress = node.GetAttribute("address",
                    String.Empty).Trim();

            node = root.SelectSingleNode("successEMail");
            if(node != null)
            {
                successEMailAddress = node.GetAttribute("address",
                    String.Empty).Trim();
                attachLogOnSuccess = Convert.ToBoolean(node.GetAttribute(
                    "attachLog", String.Empty), CultureInfo.InvariantCulture);
            }

            node = root.SelectSingleNode("failureEMail");
            if(node != null)
            {
                failureEMailAddress = node.GetAttribute("address",
                    String.Empty).Trim();
                attachLogOnFailure = Convert.ToBoolean(node.GetAttribute(
                    "attachLog", String.Empty), CultureInfo.InvariantCulture);
            }

            node = root.SelectSingleNode("xslTransform");
            if(node != null)
                xslTransformFile = builder.TransformText(
                    node.GetAttribute("filename", String.Empty).Trim());

            if((!credentials.UseDefaultCredentials &&
              (credentials.UserName.Length == 0 ||
              credentials.Password.Length == 0)) ||
              failureEMailAddress.Length == 0)
                throw new BuilderException("CNP0002", "The Completion " +
                    "Notification plug-in has an invalid configuration");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        /// <remarks>Since this runs after completion of the build and the
        /// log file is closed, any progress messages reported here will not
        /// appear in it, just in the output window on the main form.</remarks>
        public void Execute(ExecutionContext context)
        {
            MailMessage msg = null;
            string logFilename = null;

            // There is nothing to do on completion if there is no success
            // e-mail address.
            if(context.BuildStep == BuildStep.Completed &&
              successEMailAddress.Length == 0)
            {
                context.Executed = false;
                return;
            }

            try
            {
                logFilename = builder.LogFilename;

                // Run the log file through an XSL transform first?
                if(!String.IsNullOrEmpty(xslTransformFile) &&
                  File.Exists(logFilename))
                    logFilename = this.TransformLogFile();

                msg = new MailMessage();

                msg.IsBodyHtml = false;
                msg.Subject = String.Format(CultureInfo.InvariantCulture,
                    "Build {0}: {1}", context.BuildStep,
                    builder.ProjectFilename);

                if(fromEMailAddress.Length != 0)
                    msg.From = new MailAddress(fromEMailAddress);
                else
                    msg.From = new MailAddress("noreply@noreply.com");

                if(context.BuildStep == BuildStep.Completed)
                {
                    msg.To.Add(successEMailAddress);

                    if(attachLogOnSuccess && File.Exists(logFilename))
                        msg.Attachments.Add(new Attachment(logFilename));
                }
                else
                {
                    msg.To.Add(failureEMailAddress);

                    if(attachLogOnFailure && File.Exists(logFilename))
                        msg.Attachments.Add(new Attachment(logFilename));
                }

                msg.Body = String.Format(CultureInfo.InvariantCulture,
                    "Build {0}: {1}{2}\r\nBuild output is located at {3}\r\n",
                    context.BuildStep, builder.ProjectFolder,
                    builder.ProjectFilename, builder.OutputFolder);

                if(context.BuildStep != BuildStep.Completed ||
                  builder.CurrentProject.KeepLogFile)
                    msg.Body += "Build details can be found in the log file " +
                        builder.LogFilename + "\r\n";

                SmtpClient smtp = new SmtpClient();

                if(smtpServer.Length != 0)
                {
                    smtp.Host = smtpServer;
                    smtp.Port = smtpPort;
                }

                if(!credentials.UseDefaultCredentials)
                    smtp.Credentials = new NetworkCredential(
                        credentials.UserName, credentials.Password);

                smtp.Send(msg);

                builder.ReportProgress("The build notification e-mail was " +
                    "sent successfully to {0}", msg.To[0].Address);
            }
            catch(FormatException)
            {
                builder.ReportProgress("Failed to send build notification " +
                    "e-mail!  The e-mail addresses '{0}' appears to be " +
                    "invalid.", msg.To[0]);
            }
            catch(SmtpFailedRecipientException recipEx)
            {
                builder.ReportProgress("Failed to send build notification " +
                    "e-mail!  A problem occurred trying to send the e-mail " +
                    "to the recipient '{0}': {1}", recipEx.FailedRecipient,
                    recipEx.Message);
            }
            catch(SmtpException smtpEx)
            {
                System.Diagnostics.Debug.WriteLine(smtpEx.ToString());

                builder.ReportProgress("Failed to send build notification " +
                    "e-mail!  A problem occurred trying to connect to the " +
                    "e-mail server.  Details:\r\n{0}\r\n", smtpEx.ToString());
            }
            finally
            {
                if(msg != null)
                    msg.Dispose();

                // Delete the transformed log file if it exists
                if(!String.IsNullOrEmpty(logFilename) &&
                  logFilename.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    File.Delete(logFilename);
            }
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================
        // IDisposable implementation

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~CompletionNotificationPlugIn()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the plug-in object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own
        /// disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed
        /// and unmanaged resources or false to just dispose of the
        /// unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose of in this one
        }
        #endregion

        #region Helper method
        //=====================================================================

        /// <summary>
        /// This is used to run the log file through an XSL transform so that
        /// it is more readable.
        /// </summary>
        /// <returns>The transformed log filename</returns>
        private string TransformLogFile()
        {
            XslCompiledTransform xslTransform;
            XsltSettings settings;
            XmlReaderSettings readerSettings;
            XmlWriterSettings writerSettings;
            XmlReader reader = null;
            XmlWriter writer = null;
            StringReader sr = null;
            StringBuilder sb = null;
            string html = null, logFile = Path.ChangeExtension(
                builder.LogFilename, ".html");

            try
            {
                // Read in the log text we'll prefix it it with the error
                // message if the transform fails.
                using(StreamReader srdr = new StreamReader(builder.LogFilename))
                {
                    html = srdr.ReadToEnd();
                }

                // Transform the log into something more readable
                readerSettings = new XmlReaderSettings();
                readerSettings.ProhibitDtd = false;
                readerSettings.CloseInput = true;

                xslTransform = new XslCompiledTransform();
                settings = new XsltSettings(true, true);

                xslTransform.Load(XmlReader.Create(xslTransformFile,
                    readerSettings), settings, new XmlUrlResolver());

                sr = new StringReader(html);
                reader = XmlReader.Create(sr, readerSettings);
                writerSettings = xslTransform.OutputSettings.Clone();
                writerSettings.CloseOutput = true;
                writerSettings.Indent = false;

                sb = new StringBuilder(10240);
                writer = XmlWriter.Create(sb, writerSettings);
                xslTransform.Transform(reader, writer);

                writer.Flush();
                html = sb.ToString();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // Just use the raw data prefixed with the error message
                html = String.Format(CultureInfo.CurrentCulture,
                    "<pre><b>An error occurred trying to transform the log " +
                    "file '{0}'</b>:\r\n{1}\r\n\r\n<b>Log Content:</b>\r\n" +
                    "{2}</pre>", builder.LogFilename, ex.Message,
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

            using(StreamWriter sw = new StreamWriter(logFile, false,
              Encoding.UTF8))
            {
                sw.Write(html);
            }

            return logFile;
        }
        #endregion
    }
}
