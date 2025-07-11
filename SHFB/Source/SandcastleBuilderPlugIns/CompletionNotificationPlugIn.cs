//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : CompletionNotificationPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a plug-in designed to run after the build completes to send notification of the completion
// status via e-mail.  The log file can be sent as an attachment.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2007  EFW  Created the code
// 03/09/2008  EFW  Added support for log file XSL transform
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to run after the build completes to send notification of the completion
    /// status via e-mail.  The log file can be sent as an attachment.
    /// </summary>
    [HelpFileBuilderPlugInExport("Completion Notification", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in is used to send notification of the " +
        "build completion status via e-mail.  The log file can be sent as an attachment.")]
    public sealed class CompletionNotificationPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private IBuildProcess builder;
        private bool attachLogOnSuccess, attachLogOnFailure;
        private string smtpServer, fromEMailAddress, successEMailAddress, failureEMailAddress, xslTransformFile;
        private UserCredentials credentials;
        private int smtpPort;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints { get; } =
        [
            new ExecutionPoint(BuildStep.Completed, ExecutionBehaviors.After),
            new ExecutionPoint(BuildStep.Canceled, ExecutionBehaviors.After),
            new ExecutionPoint(BuildStep.Failed, ExecutionBehaviors.After)
        ];

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in configuration is not valid</exception>
        public void Initialize(IBuildProcess buildProcess, XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess;
            attachLogOnSuccess = false;
            attachLogOnFailure = true;
            smtpServer = successEMailAddress = failureEMailAddress = xslTransformFile = String.Empty;
            credentials = new UserCredentials();
            smtpPort = 25;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            if(configuration.IsEmpty)
            {
                throw new BuilderException("CNP0001", "The Completion Notification plug-in has not been " +
                    "configured yet");
            }

            var node = configuration.Element("smtpServer");

            if(node != null)
            {
                smtpServer = node.Attribute("host").Value;
                smtpPort = (int)node.Attribute("port");
            }

            credentials = UserCredentials.FromXml(configuration);

            node = configuration.Element("fromEMail");

            if(node != null)
                fromEMailAddress = node.Attribute("address").Value;

            node = configuration.Element("successEMail");

            if(node != null)
            {
                successEMailAddress = node.Attribute("address").Value;
                attachLogOnSuccess = (bool)node.Attribute("attachLog");
            }

            node = configuration.Element("failureEMail");

            if(node != null)
            {
                failureEMailAddress = node.Attribute("address").Value;
                attachLogOnFailure = (bool)node.Attribute("attachLog");
            }

            node = configuration.Element("xslTransform");

            if(node != null)
            {
                xslTransformFile = builder.SubstitutionTags.TransformText(
                    node.Attribute("filename").Value.CorrectFilePathSeparators());
            }

            if((!credentials.UseDefaultCredentials && (credentials.UserName.Length == 0 ||
              credentials.Password.Length == 0)) || failureEMailAddress.Length == 0)
            {
                throw new BuilderException("CNP0002", "The Completion Notification plug-in has an invalid configuration");
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        /// <remarks>Since this runs after completion of the build and the log file is closed, any progress
        /// messages reported here will not appear in it, just in the output window on the main form.</remarks>
        public void Execute(ExecutionContext context)
        {
            string logFilename = null, messageTo = "(Not set)";

            if(context == null)
                throw new ArgumentNullException(nameof(context));

            // There is nothing to do on completion if there is no success e-mail address
            if(context.BuildStep == BuildStep.Completed && successEMailAddress.Length == 0)
            {
                context.Executed = false;
                return;
            }

            try
            {
                logFilename = builder.LogFilename;

                // Run the log file through an XSL transform first?
                if(!String.IsNullOrEmpty(xslTransformFile) && File.Exists(logFilename))
                    logFilename = this.TransformLogFile();

                using var msg = new MailMessage
                {
                    IsBodyHtml = false,
                    Subject = String.Format(CultureInfo.InvariantCulture, "Build {0}: {1}", context.BuildStep,
                    builder.ProjectFilename)
                };
                
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

                messageTo = msg.To[0].Address;

                msg.Body = String.Format(CultureInfo.InvariantCulture,
                    "Build {0}: {1}{2}\r\nBuild output is located at {3}\r\n", context.BuildStep,
                    builder.ProjectFolder, builder.ProjectFilename, builder.OutputFolder);

                if(context.BuildStep != BuildStep.Completed || builder.CurrentProject.KeepLogFile)
                    msg.Body += "Build details can be found in the log file " + builder.LogFilename + "\r\n";

                using SmtpClient smtp = new();

                if(smtpServer.Length != 0)
                {
                    smtp.Host = smtpServer;
                    smtp.Port = smtpPort;
                }

                if(!credentials.UseDefaultCredentials)
                    smtp.Credentials = new NetworkCredential(credentials.UserName, credentials.Password);

                smtp.Send(msg);

                builder.ReportProgress("The build notification e-mail was sent successfully to {0}",
                    msg.To[0].Address);
            }
            catch(FormatException)
            {
                builder.ReportProgress("Failed to send build notification e-mail!  The e-mail addresses " +
                    "'{0}' appears to be invalid.", messageTo);
            }
            catch(SmtpFailedRecipientException recipEx)
            {
                builder.ReportProgress("Failed to send build notification e-mail!  A problem occurred trying " +
                    "to send the e-mail to the recipient '{0}': {1}", recipEx.FailedRecipient, recipEx.Message);
            }
            catch(SmtpException smtpEx)
            {
                System.Diagnostics.Debug.WriteLine(smtpEx.ToString());

                builder.ReportProgress("Failed to send build notification e-mail!  A problem occurred trying " +
                    "to connect to the e-mail server.  Details:\r\n{0}\r\n", smtpEx.ToString());
            }
            finally
            {
                // Delete the transformed log file if it exists
                if(!String.IsNullOrEmpty(logFilename) && logFilename.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    File.Delete(logFilename);
            }
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Helper method
        //=====================================================================

        /// <summary>
        /// This is used to run the log file through an XSL transform so that it is more readable
        /// </summary>
        /// <returns>The transformed log filename</returns>
        private string TransformLogFile()
        {
            XslCompiledTransform xslTransform;
            XsltSettings settings;
            XmlReaderSettings readerSettings;
            XmlWriterSettings writerSettings;
            StringBuilder sb = new(10240);
            string html = null, logFile = Path.ChangeExtension(builder.LogFilename, ".html");

            try
            {
                // Read in the log text.  We'll prefix it with the error message if the transform fails
                html = File.ReadAllText(builder.LogFilename);

                // Transform the log into something more readable
                readerSettings = new XmlReaderSettings { CloseInput = true };

                xslTransform = new XslCompiledTransform();
                settings = new XsltSettings(true, true);

                // Don't use a simplified using here.  We want to ensure the writer below gets disposed of so
                // that the string builder contains all of the content.
                using(var transformReader = XmlReader.Create(xslTransformFile, readerSettings))
                {
                    xslTransform.Load(transformReader, settings, new XmlUrlResolver());

                    using var sr = new StringReader(html);
                    using var reader = XmlReader.Create(sr, readerSettings);
                    
                    writerSettings = xslTransform.OutputSettings.Clone();
                    writerSettings.CloseOutput = true;
                    writerSettings.Indent = false;

                    sb = new StringBuilder(10240);

                    using var writer = XmlWriter.Create(sb, writerSettings);
                    xslTransform.Transform(reader, writer);
                }

                html = sb.ToString();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // Just use the raw data prefixed with the error message
                html = String.Format(CultureInfo.CurrentCulture, "<pre><b>An error occurred trying to " +
                    "transform the log file '{0}'</b>:\r\n{1}\r\n\r\n<b>Log Content:</b>\r\n{2}</pre>",
                    builder.LogFilename, ex.Message, WebUtility.HtmlEncode(html));
            }

            using StreamWriter sw = new(logFile, false, Encoding.UTF8);
            sw.Write(html);

            return logFile;
        }
        #endregion
    }
}
