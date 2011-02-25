//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AjaxDocPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/12/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to generate XML comments and
// reflection file information for Atlas client script libraries using AjaxDoc.
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
// 1.6.0.1  10/19/2007  EFW  Added execution behavior for ValidateAssemblies
// 1.6.0.6  03/10/2008  EFW  Added support for the API filter
// 1.8.0.0  08/12/2008  EFW  Modified to support the new project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to generate XML comments and reflection
    /// file information for Atlas client script libraries using AjaxDoc.
    /// </summary>
    public class AjaxDocPlugIn : IPlugIn
    {
        #region Private data members
        private ExecutionPointCollection executionPoints;

        private BuildProcess builder;

        private static Regex reExtractFiles = new Regex("\"resultsLabel\"\\>" +
            ".*?\\<.*?a href=\"(?<CommentsFile>.*?)\".*?\\<.*?a " +
            "href=\"(?<ReflectionFile>.*?)\"", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        // Plug-in configuration options
        private bool regenerateFiles;
        private string ajaxDocUrl, projectName;
        private UserCredentials userCreds;
        private ProxyCredentials proxyCreds;

        // Thread variables
        private int navCount;
        private string commentsFile, reflectionFile, errorText;
        #endregion

        #region IPlugIn implementation
        //=====================================================================
        // IPlugIn implementation

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "AjaxDoc Builder"; }
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

                return copyright.Copyright + "\r\nAjaxDoc is Copyright \xA9 " +
                    "2006-2007 Bertrand Le Roy, All Rights Reserved";
            }
        }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        public string Description
        {
            get
            {
                return "This plug-in is used to generate XML comments and " +
                    "reflection information for Atlas client script " +
                    "libraries using AjaxDoc that can then be used by " +
                    "the Sandcastle Help File Builder to produce a help file.";
            }
        }

        /// <summary>
        /// This plug-in runs in partial builds
        /// </summary>
        public bool RunsInPartialBuild
        {
            get { return true; }
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
                        BuildStep.ValidatingDocumentationSources,
                        ExecutionBehaviors.InsteadOf));
                    executionPoints.Add(new ExecutionPoint(
                        BuildStep.GenerateSharedContent,
                        ExecutionBehaviors.After));
                    executionPoints.Add(new ExecutionPoint(
                        BuildStep.GenerateReflectionInfo,
                        ExecutionBehaviors.InsteadOf));
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
            using(AjaxDocConfigDlg dlg = new AjaxDocConfigDlg(currentConfig))
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

            builder = buildProcess;
            ajaxDocUrl = projectName = String.Empty;
            userCreds = new UserCredentials();
            proxyCreds = new ProxyCredentials();

            builder.ReportProgress("{0} Version {1}\r\n{2}",
                this.Name, this.Version, this.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("ADP0001", "The AjaxDoc plug-in " +
                    "has not been configured yet");

            node = root.SelectSingleNode("ajaxDoc");
            if(node != null)
            {
                ajaxDocUrl = node.GetAttribute("url", String.Empty).Trim();
                projectName = node.GetAttribute("project", String.Empty).Trim();
                regenerateFiles = Convert.ToBoolean(node.GetAttribute(
                    "regenerate", String.Empty), CultureInfo.InvariantCulture);
            }

            userCreds = UserCredentials.FromXPathNavigator(root);
            proxyCreds = ProxyCredentials.FromXPathNavigator(root);

            if(ajaxDocUrl.Length == 0 || projectName.Length == 0 ||
              (!userCreds.UseDefaultCredentials &&
              (userCreds.UserName.Length == 0 ||
              userCreds.Password.Length == 0)) ||
              (proxyCreds.UseProxyServer &&
              (proxyCreds.ProxyServer == null ||
              (!proxyCreds.Credentials.UseDefaultCredentials &&
              (proxyCreds.Credentials.UserName.Length == 0 ||
              proxyCreds.Credentials.Password.Length == 0)))))
                throw new BuilderException("ADP0002", "The AjaxDoc plug-in " +
                    "has an invalid configuration");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(Utils.PlugIn.ExecutionContext context)
        {
            Encoding enc = Encoding.Default;
            Thread browserThread;
            XmlDocument sharedContent;
            XPathNavigator navContent, item;
            XmlCommentsFile comments;
            string sharedContentFilename, workingPath, content;

            // Copy any XML comments files from the project to the working
            // folder.  Solutions, projects, and assemblies are ignored as
            // they won't be used with this build type.
            if(context.BuildStep == BuildStep.ValidatingDocumentationSources)
            {
                builder.ExecuteBeforeStepPlugIns();

                foreach(DocumentationSource ds in
                  builder.CurrentProject.DocumentationSources)
                    foreach(string commentsName in
                      DocumentationSource.CommentsFiles(ds.SourceFile,
                      ds.IncludeSubFolders))
                    {
                        workingPath = builder.WorkingFolder +
                            Path.GetFileName(commentsName);

                        // Warn if there is a duplicate and copy the comments
                        // file to a unique name to preserve its content.
                        if(File.Exists(workingPath))
                        {
                            workingPath = builder.WorkingFolder +
                                Guid.NewGuid().ToString("B");

                            builder.ReportWarning("BE0063", "'{0}' matches " +
                                "a previously copied comments filename.  The " +
                                "duplicate will be copied to a unique name " +
                                "to preserve the comments it contains.",
                                commentsName);
                        }

                        File.Copy(commentsName, workingPath, true);
                        File.SetAttributes(workingPath, FileAttributes.Normal);

                        // Add the file to the XML comments file collection
                        comments = new XmlCommentsFile(workingPath);

                        // Fixup comments for CPP comments files?
                        if(builder.CurrentProject.CppCommentsFixup)
                            comments.FixupComments();

                        builder.CommentsFiles.Add(comments);
                        builder.ReportProgress("    {0} -> {1}", commentsName,
                            workingPath);
                    }

                builder.ExecuteAfterStepPlugIns();
                return;
            }

            // Remove the version information items from the shared content
            // file as the AjaxDoc reflection file doesn't contain version
            // information.
            if(context.BuildStep == BuildStep.GenerateSharedContent)
            {
                builder.ReportProgress("Removing version information items " +
                    "from shared content file");

                sharedContentFilename = builder.WorkingFolder +
                    "SharedBuilderContent.xml";

                sharedContent = new XmlDocument();
                sharedContent.Load(sharedContentFilename);
                navContent = sharedContent.CreateNavigator();

                item = navContent.SelectSingleNode("content/item[@id='" +
                    "locationInformation']");

                if(item != null)
                    item.DeleteSelf();

                item = navContent.SelectSingleNode("content/item[@id='" +
                    "assemblyNameAndModule']");

                if(item != null)
                    item.DeleteSelf();

                sharedContent.Save(sharedContentFilename);
                return;
            }

            builder.ReportProgress("Using project '{0}'", projectName);

            if(regenerateFiles)
            {
                // Regenerate the files first.  This is done by starting a
                // thread to invoke the AjaxDoc application via a web browser
                // control.  This is necessary as the brower control needs to
                // run in a thread with a single-threaded apartment state.
                // We can't just request the page as AjaxDoc has to post back
                // to itself in order to store the generated information.
                builder.ReportProgress("Generating XML comments and " +
                    "reflection information via AjaxDoc");

                browserThread = new Thread(RunBrowser);
                browserThread.SetApartmentState(ApartmentState.STA);

                navCount = 0;
                browserThread.Start();

                if(!browserThread.Join(11000))
                    browserThread.Abort();

                if(!String.IsNullOrEmpty(errorText))
                    throw new BuilderException("ADP0003", "AjaxDoc encountered " +
                        "a scripting error: " + errorText);

                if(commentsFile == null || reflectionFile == null)
                    throw new BuilderException("ADP0004", "Unable to produce " +
                        "comments file and/or reflection file");

                builder.ReportProgress("Generated comments file '{0}' " +
                    "and reflection file '{1}'", commentsFile, reflectionFile);
            }
            else
            {
                // Use the existing files
                commentsFile = "Output/" + projectName + ".xml";
                reflectionFile = "Output/" + projectName + ".org";

                builder.ReportProgress("Using existing XML comments file " +
                    "'{0}' and reflection information file '{1}'",
                    commentsFile, reflectionFile);
            }

            // Allow Before step plug-ins to run
            builder.ExecuteBeforeStepPlugIns();

            // Download the files
            using(WebClient webClient = new WebClient())
            {
                webClient.UseDefaultCredentials = userCreds.UseDefaultCredentials;
                if(!userCreds.UseDefaultCredentials)
                    webClient.Credentials = new NetworkCredential(
                        userCreds.UserName, userCreds.Password);

                webClient.CachePolicy = new RequestCachePolicy(
                    RequestCacheLevel.NoCacheNoStore);

                if(proxyCreds.UseProxyServer)
                {
                    webClient.Proxy = new WebProxy(proxyCreds.ProxyServer, true);

                    if(!proxyCreds.Credentials.UseDefaultCredentials)
                        webClient.Proxy.Credentials = new NetworkCredential(
                            proxyCreds.Credentials.UserName,
                            proxyCreds.Credentials.Password);
                }

                // Since there are only two, download them synchronously
                workingPath = builder.WorkingFolder + projectName + ".xml";
                builder.ReportProgress("Downloading {0}", commentsFile);
                webClient.DownloadFile(ajaxDocUrl + commentsFile, workingPath);
                builder.CommentsFiles.Add(new XmlCommentsFile(workingPath));

                builder.ReportProgress("Downloading {0}", reflectionFile);
                webClient.DownloadFile(ajaxDocUrl + reflectionFile,
                    builder.ReflectionInfoFilename);

                builder.ReportProgress("Downloads completed successfully");
            }

            // AjaxDoc 1.1 prefixes all member names with "J#" which causes
            // BuildAssembler's ResolveReferenceLinksComponent2 component in
            // the Sept 2007 CTP to crash.  As such, we'll strip it out.  I
            // can't see a need for it anyway.
            content = BuildProcess.ReadWithEncoding(workingPath, ref enc);
            content = content.Replace(":J#", ":");

            using(StreamWriter sw = new StreamWriter(workingPath, false, enc))
            {
                sw.Write(content);
            }

            content = BuildProcess.ReadWithEncoding(
                builder.ReflectionInfoFilename, ref enc);
            content = content.Replace(":J#", ":");

            using(StreamWriter sw = new StreamWriter(
              builder.ReflectionInfoFilename, false, enc))
            {
                sw.Write(content);
            }

            // Don't apply the filter in a partial build
            if(!builder.IsPartialBuild && builder.BuildApiFilter.Count != 0)
            {
                builder.ReportProgress("Applying API filter manually");
                builder.ApplyManualApiFilter(builder.BuildApiFilter,
                    builder.ReflectionInfoFilename);
            }

            // Allow After step plug-ins to run
            builder.ExecuteAfterStepPlugIns();
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================
        // IDisposable implementation

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~AjaxDocPlugIn()
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

        #region Web browser thread methods
        //=====================================================================
        // Web browser thread methods

        /// <summary>
        /// This is the entry point for the thread that runs the browser to
        /// invoke AjaxDoc.
        /// </summary>
        private void RunBrowser()
        {
            int counter = 0;

            using(WebBrowser wb = new WebBrowser())
            {
                wb.ScriptErrorsSuppressed = true;
                wb.Navigated += new WebBrowserNavigatedEventHandler(wb_Navigated);
                wb.DocumentCompleted += new
                    WebBrowserDocumentCompletedEventHandler(wb_DocumentCompleted);
                wb.Navigate(String.Format(CultureInfo.InvariantCulture,
                    "{0}Default.aspx?project={1}", ajaxDocUrl, projectName));

                // See notes below for the Navigated event handler
                navCount = 0;
                commentsFile = reflectionFile = null;

                while(navCount < 2 && counter < 1000)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                    counter++;
                }

                if(String.IsNullOrEmpty(errorText))
                {
                    Match m = reExtractFiles.Match(wb.DocumentText);

                    if(m.Success)
                    {
                        commentsFile = m.Groups["CommentsFile"].Value;
                        reflectionFile = m.Groups["ReflectionFile"].Value;
                    }

                    // We could download the files via the web browser object
                    // but that's a bit of a hack so we'll let the main thread
                    // download them via a WebClient object.  Also, that lets
                    // the plug-in bypass this step if not needed.
                }
            }
        }

        /// <summary>
        /// Hook up the error handler on completion to handle script errors
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void wb_DocumentCompleted(object sender,
          WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error +=
                new HtmlElementErrorEventHandler(Window_Error);
        }

        /// <summary>
        /// If a script error occurs, stop processing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            navCount = 2;
            errorText = e.Description;
        }

        /// <summary>
        /// This notes when the page completes navigating
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>If ran successfully, the page navigates twice.  The first
        /// time is the initial page load.  It immediately submits itself to
        /// post the results.  On the second navigation, we can assume it was
        /// successful and the page contains the "success" result message.
        /// </remarks>
        private void wb_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            navCount++;
        }
        #endregion
    }
}
