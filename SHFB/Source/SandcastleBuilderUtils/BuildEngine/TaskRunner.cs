//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TaskRunner.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2015
// Note    : Copyright 2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to execute external tasks such as MSBuild
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/25/2015  EFW  Refactored the task execution code and moved it into its own class
//===============================================================================================================

// Ignore Spelling: hhc sbapplocale nologo clp

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class is used to execute external tasks such as MSBuild
    /// </summary>
    public class TaskRunner
    {
        #region Private data members
        //=====================================================================

        private BuildProcess currentBuild;
        private bool errorDetected;

        private static Regex reErrorCheck = new Regex(@"^\s*((Error|UnrecognizedOption|Unhandled Exception|" +
            @"Fatal Error|Unexpected error.*|HHC\d+: Error|(Fatal )?Error HXC\d+):|Process is terminated|" +
            @"Build FAILED|\w+\s*:\s*Error\s.*?:|\w.*?\(\d*,\d*\):\s*error\s.*?:)", RegexOptions.IgnoreCase |
            RegexOptions.Multiline);

        private static Regex reKillProcess = new Regex("hhc|sbapplocale", RegexOptions.IgnoreCase);

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentBuild">The current build for which to execute tasks</param>
        public TaskRunner(BuildProcess currentBuild)
        {
            this.currentBuild = currentBuild;
        }
        #endregion

        #region Task execution methods
        //=====================================================================

        /// <summary>
        /// Run the specified MSBuild project
        /// </summary>
        /// <param name="projectFile">The project file to run</param>
        /// <param name="minimalOutput">True for minimal output, false for normal output</param>
        /// <remarks>A standard set of command line options will be used (<c>/nologo /clp:NoSummary</c>) along
        /// with the given verbosity level.  The 64-bit version of MSBuild will be used if available on 64-bit
        /// systems.</remarks>
        public void RunProject(string projectFile, bool minimalOutput)
        {
            this.Run(currentBuild.MSBuildExePath, projectFile, "/nologo /clp:NoSummary /v:" +
                (minimalOutput ? "m" : "n"));
        }

        /// <summary>
        /// Run the specified MSBuild project using the 32-bit version of MSBuild
        /// </summary>
        /// <param name="projectFile">The project file to run</param>
        /// <param name="minimalOutput">True for minimal output, false for normal output</param>
        /// <remarks>A standard set of command line options will be used (<c>/nologo /clp:NoSummary</c>) along
        /// with the given verbosity level.  Silverlight build targets are only available for 32-bit builds
        /// regardless of the framework version and require the 32-bit version of MSBuild in order to load the
        /// targets files correctly.</remarks>
        public void Run32BitProject(string projectFile, bool minimalOutput)
        {
            // Check for both earlier versions of MSBuild which are in the framework folder as well as later
            // versions which are in Program Files.
            string msBuild = currentBuild.MSBuildExePath.Replace("Framework64", "Framework").Replace(
                "amd64\\", String.Empty);

            this.Run(msBuild, projectFile, "/nologo /clp:NoSummary /v:" + (minimalOutput ? "m" : "n"));
        }

        /// <summary>
        /// This is used to run a step in the build process
        /// </summary>
        /// <param name="processFilename">The process to execute.</param>
        /// <param name="targetFile">An optional target file that the process will operate on such as an MSBuild
        /// project file.</param>
        /// <param name="arguments">An optional set of arguments to pass to the process</param>
        public void Run(string processFilename, string targetFile, string arguments)
        {
            Process currentProcess = null;

            if(processFilename == null)
                throw new ArgumentNullException("process");

            currentBuild.ReportProgress("[{0}{1}]", processFilename,
                !String.IsNullOrWhiteSpace(targetFile) ? " - " + targetFile : String.Empty);

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
                psi.WorkingDirectory = currentBuild.WorkingFolder;
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
                            if(!hasExited && currentBuild.CancellationToken.IsCancellationRequested)
                            {
                                this.TerminateProcessAndChildren(currentProcess);

                                throw new OperationCanceledException();
                            }

                        } while(!hasExited);
                    }))
                {
                    Task.WaitAll(processWaiter, stdOutReader, stdErrReader);
                }

                // Stop the build if an error was detected in the process output
                if(errorDetected)
                    throw new BuilderException("BE0043", "Unexpected error detected in last build step.  See " +
                        "build log for details.");
            }
            catch(AggregateException ex)
            {
                this.TerminateProcessAndChildren(currentProcess);

                var canceledEx = ex.InnerExceptions.FirstOrDefault(e => e is OperationCanceledException);

                if(canceledEx != null)
                    throw canceledEx;

                throw;
            }
            catch
            {
                this.TerminateProcessAndChildren(currentProcess);
                throw;
            }
            finally
            {
                if(currentProcess != null)
                    currentProcess.Dispose();
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Terminate the current process and any known child processes if they're still alive
        /// </summary>
        private void TerminateProcessAndChildren(Process currentProcess)
        {
            DateTime procStart = DateTime.MinValue;

            if(currentProcess != null && !currentProcess.HasExited)
            {
                // Only kill potential matches started after the current process's start time.  It's not perfect
                // if you've got two or more SHFB builds running concurrently but it's the best we can do without
                // getting really complicated which I'm not prepared to do since this is an extremely low
                // occurrence issue.
                try
                {
                    procStart = currentProcess.StartTime;
                    currentProcess.Kill();
                }
                catch
                {
                    // If we couldn't get the start time, assume the build start time
                    if(procStart == DateTime.MinValue)
                        procStart = currentBuild.BuildStart;
                }

                foreach(Process p in Process.GetProcesses())
                    try
                    {
                        if(reKillProcess.IsMatch(p.ProcessName) && !p.HasExited && p.StartTime > procStart)
                        {
                            System.Diagnostics.Debug.WriteLine("Killing " + p.ProcessName);

                            p.Kill();
                        }
                    }
                    catch
                    {
                        // Ignore exceptions, the process had probably already exited
                    }
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

                    currentBuild.ReportProgress(line);
                }

            } while(line != null);
        }
        #endregion
    }
}
