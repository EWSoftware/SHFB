//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : BuildProcess.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/30/2022
// Note    : Copyright 2015-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to build the reflection data
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/29/2015  EFW  Created the code
// 04/04/2021  EFW  Merged code from SegregateByNamespace into the build process and removed the separate tool
//===============================================================================================================

// Ignore Spelling: nologo clp

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.Reflection;
using Sandcastle.Core.PresentationStyle;

namespace ReflectionDataManager
{
    /// <summary>
    /// This class is used to build the reflection data
    /// </summary>
    /// <remarks>This generates the reflection data for the given data set and segregates it by namespace.  The
    /// resulting reflection data files are stored in the same folder as the reflection data set configuration
    /// file.</remarks>
    public sealed class BuildProcess : IDisposable
    {
        #region Private data members
        //=====================================================================

        private readonly ReflectionDataSet dataSet;
        private readonly string workingFolder, msBuildExePath;
        private bool errorDetected;

        private static readonly Regex reErrorCheck = new Regex(@"^\s*((Error|UnrecognizedOption|Unhandled Exception|" +
            @"Fatal Error|Unexpected error.*):|Process is terminated|Build FAILED|\w+\s*:\s*Error\s.*?:|" +
            @"\w.*?\(\d*,\d*\):\s*error\s.*?:)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly XPathExpression apiExpression = XPathExpression.Compile("/*/apis/api");
        private static readonly XPathExpression apiNamespaceExpression = XPathExpression.Compile("string(containers/namespace/@api)");
        private static readonly XPathExpression assemblyNameExpression = XPathExpression.Compile("string(containers/library/@assembly)");
        private static readonly XPathExpression namespaceIdExpression = XPathExpression.Compile("string(@id)");

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// This is used to get or set the progress report provider
        /// </summary>
        public IProgress<string> ProgressProvider { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataSet">The reflection data set to build</param>
        public BuildProcess(ReflectionDataSet dataSet)
        {
            this.dataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));

            workingFolder = Path.GetTempFileName();
            File.Delete(workingFolder);
            Directory.CreateDirectory(workingFolder);

            // Use the latest version of MSBuild available rather than a specific version
            string latestToolsVersion = ProjectCollection.GlobalProjectCollection.Toolsets.FirstOrDefault(
                t => t.ToolsVersion.Equals("Current", StringComparison.OrdinalIgnoreCase))?.ToolsVersion;

            if(latestToolsVersion == null)
            {
                latestToolsVersion = ProjectCollection.GlobalProjectCollection.Toolsets.Max(
                    t => Version.TryParse(t.ToolsVersion, out Version ver) ? ver : new Version()).ToString();
            }

            msBuildExePath = Path.Combine(ProjectCollection.GlobalProjectCollection.Toolsets.First(
                t => t.ToolsVersion == latestToolsVersion).ToolsPath, "MSBuild.exe");
        }
        #endregion

        #region IDisposable members
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the build process if not done explicitly
        /// with <see cref="Dispose"/>.
        /// </summary>
        ~BuildProcess()
        {
            this.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                if(workingFolder != null && Directory.Exists(workingFolder))
                    Directory.Delete(workingFolder, true);
            }
            catch(Exception ex)
            {
                // Ignore errors but log them when debugging
                Debug.WriteLine(ex);
            }

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Build execution methods
        //=====================================================================

        /// <summary>
        /// Build the reflection data for the given reflection data set
        /// </summary>
        public void Build()
        {
            string projectFile, configFile = Path.Combine(ComponentUtilities.ToolsFolder,
                @"Templates\BuildReflectionData.config");

            string content = File.ReadAllText(configFile);

            content = content.Replace("{@Platform}", dataSet.Platform);
            content = content.Replace("{@Version}", dataSet.Version.ToString());
            content = content.Replace("{@SHFBRoot}", ComponentUtilities.RootFolder);
            content = content.Replace("{@IgnoreIfUnresolved}", dataSet.IgnoreIfUnresolvedConfiguration());
            content = content.Replace("{@BindingRedirections}", dataSet.BindingRedirectionConfiguration());
            content = content.Replace("{@IgnoredNamespaces}", dataSet.IgnoredNamespacesConfiguration());

            if(!Path.GetDirectoryName(dataSet.Filename).StartsWith(Path.GetDirectoryName(
              Assembly.GetExecutingAssembly().Location), StringComparison.OrdinalIgnoreCase))
                content = content.Replace("{@ComponentLocations}", String.Format(CultureInfo.InvariantCulture,
                    "<location folder=\"{0}\" />\r\n", WebUtility.HtmlEncode(Path.GetDirectoryName(
                    dataSet.Filename))));

            File.WriteAllText(Path.Combine(workingFolder, "BuildReflectionData.config"), content);

            configFile = Path.Combine(ComponentUtilities.ToolsFolder, @"Templates\BuildReflectionData.proj");
            content = File.ReadAllText(configFile);

            content = content.Replace("{@SHFBRoot}", ComponentUtilities.RootFolder);
            content = content.Replace("{@HtmlEncWorkingFolder}", WebUtility.HtmlEncode(workingFolder));

            content = content.Replace("{@Assemblies}", String.Join("\r\n", dataSet.IncludedAssemblies.Select(
                a => String.Format(CultureInfo.InvariantCulture, "<Assembly Include=\"{0}\" />", a.Filename))));

            projectFile = Path.Combine(workingFolder, "BuildReflectionData.proj");
            File.WriteAllText(projectFile, content);

            this.Run(msBuildExePath, projectFile, "/nologo /clp:NoSummary /v:n");

            this.ProgressProvider.Report("Applying standard document model");

            var docModel = new StandardDocumentModel();

            docModel.ApplyDocumentModel(Path.Combine(workingFolder, "reflection.org"),
                Path.Combine(workingFolder, "reflection.xml"));

            this.ProgressProvider.Report("Segregating reflection data by namespace");
            this.SegregateByNamespace();

            string targetPath = Path.GetDirectoryName(dataSet.Filename);

            this.ProgressProvider?.Report(String.Format(CultureInfo.InvariantCulture,
                "Clearing reflection data files from {0}...", targetPath));

            foreach(string file in Directory.EnumerateFiles(targetPath, "*.xml"))
                File.Delete(file);

            this.ProgressProvider?.Report(String.Format(CultureInfo.InvariantCulture,
                "Copying new reflection data files to {0}...", targetPath));

            foreach(string file in Directory.EnumerateFiles(Path.Combine(workingFolder, "Segregated")))
                File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)));

            this.ProgressProvider?.Report("\r\nBuild completed successfully");
        }

        /// <summary>
        /// This is used to run a step in the build process
        /// </summary>
        /// <param name="processFilename">The process to execute.</param>
        /// <param name="targetFile">An optional target file that the process will operate on such as an MSBuild
        /// project file.</param>
        /// <param name="arguments">An optional set of arguments to pass to the process</param>
        private void Run(string processFilename, string targetFile, string arguments)
        {
            Process currentProcess = null;

            if(processFilename == null)
                throw new ArgumentNullException(nameof(processFilename));

            this.ProgressProvider?.Report(String.Format(CultureInfo.InvariantCulture, "[{0}{1}]", processFilename,
                !String.IsNullOrWhiteSpace(targetFile) ? " - " + targetFile : String.Empty));

            if(arguments == null)
                arguments = String.Empty;

            if(!String.IsNullOrWhiteSpace(targetFile))
                arguments += " " + targetFile;

            try
            {
                currentProcess = new Process();
                errorDetected = false;

                ProcessStartInfo psi = currentProcess.StartInfo;

                // Set CreateNoWindow to true to suppress the window rather than setting WindowStyle to hidden as
                // WindowStyle has no effect on command prompt windows and they always appear.
                psi.CreateNoWindow = true;
                psi.FileName = processFilename;
                psi.Arguments = arguments.Trim();
                psi.WorkingDirectory = workingFolder;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = psi.RedirectStandardError = true;

                currentProcess.Start();

                // Spawn two separate tasks so that we can capture both STDOUT and STDERR without the risk of a
                // deadlock.
                using(var stdOutReader = Task.Run(() => this.ReadOutputStream(currentProcess.StandardOutput)))
                using(var stdErrReader = Task.Run(() => this.ReadOutputStream(currentProcess.StandardError)))
                using(var processWaiter = Task.Run(() =>
                    {
                        bool hasExited;

                        do
                        {
                            hasExited = currentProcess.WaitForExit(1000);

                            // Some processes run for a while without reporting any progress.  This should allow
                            // for faster cancellation in those situations.
                            if(!hasExited && this.CancellationToken.IsCancellationRequested)
                            {
                                TerminateProcess(currentProcess);
                                throw new OperationCanceledException();
                            }

                        } while(!hasExited);
                    }))
                {
                    Task.WaitAll(processWaiter, stdOutReader, stdErrReader);
                }

                // Stop the build if an error was detected in the process output
                if(errorDetected)
                    throw new InvalidOperationException("Unexpected error detected.  See build log for details.");
            }
            catch(AggregateException ex)
            {
                TerminateProcess(currentProcess);

                var canceledEx = ex.InnerExceptions.FirstOrDefault(e => e is OperationCanceledException);

                if(canceledEx != null)
                    throw canceledEx;

                throw;
            }
            catch
            {
                TerminateProcess(currentProcess);
                throw;
            }
            finally
            {
                currentProcess?.Dispose();
            }
        }

        /// <summary>
        /// Terminate the current process
        /// </summary>
        private static void TerminateProcess(Process currentProcess)
        {
            if(currentProcess != null && !currentProcess.HasExited)
                try
                {
                    currentProcess.Kill();
                }
                catch
                {
                    // Ignore any errors
                }
        }

        /// <summary>
        /// This is used to capture output from the given process stream
        /// </summary>
        private void ReadOutputStream(StreamReader stream)
        {
            string line;

            do
            {
                line = stream.ReadLine();

                if(line != null)
                {
                    // The ReportProgress method uses String.Format so double any braces in the output
                    if(line.IndexOf('{') != -1)
                        line = line.Replace("{", "{{");

                    if(line.IndexOf('}') != -1)
                        line = line.Replace("}", "}}");

                    // Check for errors
                    if(reErrorCheck.IsMatch(line))
                        errorDetected = true;

                    this.ProgressProvider?.Report(line);
                }

            } while(line != null);
        }

        /// <summary>
        /// This is used to split the reflection data into separate files by namespace
        /// </summary>
        private void SegregateByNamespace()
        {
            string reflectionDataFile = Path.Combine(workingFolder, "reflection.xml"),
                outputPath = Path.Combine(workingFolder, "Segregated");

            using(var reader = XmlReader.Create(reflectionDataFile, new XmlReaderSettings { CloseInput = true }))
            {
                XPathDocument document = new XPathDocument(reader);

                Dictionary<string, object> dictionary3;
                XmlWriter writer;
                string current;
                char[] invalidChars = Path.GetInvalidFileNameChars();

                Dictionary<string, Dictionary<string, object>> dictionary = new Dictionary<string, Dictionary<string, object>>();
                Dictionary<string, XmlWriter> dictionary2 = new Dictionary<string, XmlWriter>();
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

                try
                {
                    Directory.CreateDirectory(outputPath);

                    XPathNodeIterator iterator = document.CreateNavigator().Select(apiExpression);

                    foreach(XPathNavigator navigator in iterator)
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();

                        current = (string)navigator.Evaluate(apiNamespaceExpression);

                        if(!String.IsNullOrEmpty(current))
                        {
                            String key = (string)navigator.Evaluate(assemblyNameExpression);

                            if(!dictionary.TryGetValue(current, out dictionary3))
                            {
                                dictionary3 = new Dictionary<string, object>();
                                dictionary.Add(current, dictionary3);
                            }

                            if(!dictionary3.ContainsKey(key))
                                dictionary3.Add(key, null);
                        }
                    }

                    foreach(string currentKey in dictionary.Keys)
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();

                        string filename = currentKey.Substring(2) + ".xml";

                        if(filename == ".xml")
                            filename = "default_namespace.xml";
                        else
                        {
                            if(filename.IndexOfAny(invalidChars) != -1)
                                foreach(char c in invalidChars)
                                    filename = filename.Replace(c, '_');
                        }

                        filename = Path.Combine(outputPath, filename);

                        writer = XmlWriter.Create(filename, settings);

                        dictionary2.Add(currentKey, writer);

                        writer.WriteStartElement("reflection");
                        writer.WriteStartElement("assemblies");

                        dictionary3 = dictionary[currentKey];

                        foreach(string assemblyName in dictionary3.Keys)
                        {
                            this.CancellationToken.ThrowIfCancellationRequested();

                            XPathNavigator navigator2 = document.CreateNavigator().SelectSingleNode(
                                "/*/assemblies/assembly[@name='" + assemblyName + "']");

                            if(navigator2 != null)
                                navigator2.WriteSubtree(writer);
                            else
                                this.ProgressProvider.Report(String.Format(CultureInfo.InvariantCulture,
                                    "Error: Input file does not contain node for '{0}' assembly", assemblyName));
                        }

                        writer.WriteEndElement();
                        writer.WriteStartElement("apis");
                    }

                    foreach(XPathNavigator navigator in iterator)
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();

                        current = (string)navigator.Evaluate(apiNamespaceExpression);

                        if(String.IsNullOrEmpty(current))
                            current = (string)navigator.Evaluate(namespaceIdExpression);

                        writer = dictionary2[current];
                        navigator.WriteSubtree(writer);
                    }

                    foreach(XmlWriter w in dictionary2.Values)
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();

                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndDocument();
                    }

                    this.ProgressProvider.Report(String.Format(CultureInfo.InvariantCulture,
                        "Info: Wrote information on {0} APIs to {1} files.", iterator.Count, dictionary2.Count));
                }
                finally
                {
                    foreach(XmlWriter w in dictionary2.Values)
                        w.Close();
                }
            }
        }
        #endregion
    }
}
