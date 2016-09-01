//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : BuildProcess.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/29/2016
// Note    : Copyright 2015-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
//===============================================================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.Reflection;

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

        private ReflectionDataSet dataSet;
        private string workingFolder, msBuildExePath;
        private bool errorDetected;

        private static Regex reErrorCheck = new Regex(@"^\s*((Error|UnrecognizedOption|Unhandled Exception|" +
            @"Fatal Error|Unexpected error.*):|Process is terminated|Build FAILED|\w+\s*:\s*Error\s.*?:|" +
            @"\w.*?\(\d*,\d*\):\s*error\s.*?:)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

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
            if(dataSet == null)
                throw new ArgumentNullException("dataSet");

            this.dataSet = dataSet;

            workingFolder = Path.GetTempFileName();
            File.Delete(workingFolder);
            Directory.CreateDirectory(workingFolder);

            // Use the latest version of MSBuild available rather than a specific version
            string latestToolsVersion = ProjectCollection.GlobalProjectCollection.Toolsets.Max(
                t => new Version(t.ToolsVersion)).ToString();

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
                System.Diagnostics.Debug.WriteLine(ex);
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
            content = content.Replace("{@SHFBFolder}", ComponentUtilities.ToolsFolder);
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

            content = content.Replace("{@SHFBFolder}", ComponentUtilities.ToolsFolder);
            content = content.Replace("{@HtmlEncWorkingFolder}", WebUtility.HtmlEncode(workingFolder));

            content = content.Replace("{@Assemblies}", String.Join("\r\n", dataSet.IncludedAssemblies.Select(
                a => String.Format(CultureInfo.InvariantCulture, "<Assembly Include=\"{0}\" />", a.Filename))));

            projectFile = Path.Combine(workingFolder, "BuildReflectionData.proj");
            File.WriteAllText(projectFile, content);

            this.Run(msBuildExePath, projectFile, "/nologo /clp:NoSummary /v:n");

            string targetPath = Path.GetDirectoryName(dataSet.Filename);

            if(this.ProgressProvider != null)
                this.ProgressProvider.Report(String.Format(CultureInfo.InvariantCulture,
                    "Clearing reflection data files from {0}...", targetPath));

            foreach(string file in Directory.EnumerateFiles(targetPath, "*.xml"))
                File.Delete(file);

            if(this.ProgressProvider != null)
                this.ProgressProvider.Report(String.Format(CultureInfo.InvariantCulture,
                    "Copying new reflection data files to {0}...", targetPath));

            foreach(string file in Directory.EnumerateFiles(workingFolder + @"\Segregated"))
                File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)));

            if(this.ProgressProvider != null)
                this.ProgressProvider.Report("\r\nBuild completed successfully");
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
                throw new ArgumentNullException("processFilename");

            if(this.ProgressProvider != null)
                this.ProgressProvider.Report(String.Format(CultureInfo.InvariantCulture, "[{0}{1}]", processFilename,
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
                            if(!hasExited && this.CancellationToken != null &&
                              this.CancellationToken.IsCancellationRequested)
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
                if(currentProcess != null)
                    currentProcess.Dispose();
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

                    if(this.ProgressProvider != null)
                        this.ProgressProvider.Report(line);
                }

            } while(line != null);
        }
        #endregion
    }
}
