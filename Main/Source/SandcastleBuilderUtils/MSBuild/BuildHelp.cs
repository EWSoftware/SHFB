//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildHelp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/26/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to build help file output using the
// Sandcastle Help File Builder.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/27/2008  EFW  Created the code
// 1.8.0.1  12/19/2008  EFW  Updated to work with MSBuild 3.5 and Team Build
// 1.8.0.2  04/20/2009  EFW  Added DumpLogOnFailure property
// 1.8.0.3  07/06/2009  EFW  Added support for MS Help Viewer output files
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to build help file output using the Sandcastle Help
    /// File Builder.
    /// </summary>
    /// <remarks>All messages from this task are logged with a high priority since it will run for a long time
    /// and we need to see the progress messages to know it's doing something.  If set to Normal and ran from
    /// within Visual Studio, it won't show the progress messages when the logging options are set to Minimal.</remarks>
    public class BuildHelp : Task, ICancelableTask
    {
        #region Private data members
        //=====================================================================

        private static Regex reParseMessage = new Regex(@"^(\w{2,}):\s*(.*?)\s*" +
            @"\W(warning|error)\W\s*(\w+?\d*?):\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private SandcastleProject sandcastleProject;
        private BuildProcess buildProcess;
        private BuildStep lastBuildStep;

        private bool buildCancelled;
        #endregion

        #region Task input properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the project filename
        /// </summary>
        /// <remarks>Since <see cref="SandcastleProject" /> already wraps the
        /// MSBuild project, it seemed redundant to define each and every
        /// property on this task and map them to the project properties.  As
        /// such, this task will attempt to use the executing project to create
        /// the Sandcastle project instance.  If that fails or
        /// <see cref="AlwaysLoadProject" /> is true, this file will be
        /// loaded instead.  The downside is that property overrides on the
        /// command line will be ignored.</remarks>
        [Required]
        public string ProjectFile { get; set; }

        /// <summary>
        /// This is used to pass in the configuration to use for the build
        /// </summary>
        [Required]
        public string Configuration { get; set; }

        /// <summary>
        /// This is used to pass in the platform to use for the build
        /// </summary>
        [Required]
        public string Platform { get; set; }

        /// <summary>
        /// This is used to specify the output directory containing the build
        /// output for solution and project documentation sources when using
        /// Team Build.
        /// </summary>
        /// <value>This property is optional.  If not specified, the default
        /// output path in project file documentation sources will be used.</value>
        public string OutDir { get; set; }

        /// <summary>
        /// This is used to set or get the output logging verbosity flag
        /// </summary>
        /// <value>This property is optional.  If set to false (the default),
        /// only build steps are written to the task log.  If set to true, all
        /// output from the build process is written to the task log.</value>
        public bool Verbose { get; set; }

        /// <summary>
        /// This is used to set or get whether the log file is dumped to the
        /// task log if the help file project build fails.
        /// </summary>
        /// <value>This property is optional.  If set to false (the default),
        /// the log is not dumped if the build fails.  If set to true, all
        /// output from the build process is written to the task log if the
        /// build fails.</value>
        public bool DumpLogOnFailure { get; set; }

        /// <summary>
        /// This is used to specify whether or not to load the specified
        /// <see cref="ProjectFile" /> rather than use the executing project.
        /// </summary>
        /// <value>This property is optional.  If set to false, the default,
        /// the executing project is used as the Sandcastle project to build.
        /// If set to true, the specified <see cref="ProjectFile" /> is loaded.
        /// In such cases, command line property overrides are ignored.</value>
        public bool AlwaysLoadProject { get; set; }
        #endregion

        #region Task output properties
        //=====================================================================

        /// <summary>
        /// This is used to return a list of the HTML Help 1 (CHM) files that
        /// resulted from the build.
        /// </summary>
        [Output]
        public ITaskItem[] Help1Files
        {
            get
            {
                List<ITaskItem> files = new List<ITaskItem>();

                if(buildProcess != null && lastBuildStep == BuildStep.Completed)
                    foreach(string file in buildProcess.Help1Files)
                        files.Add(new TaskItem(file));

                return files.ToArray();
            }
        }

        /// <summary>
        /// This is used to return a list of the MS Help 2 (HxS) files that
        /// resulted from the build.
        /// </summary>
        [Output]
        public ITaskItem[] Help2Files
        {
            get
            {
                List<ITaskItem> files = new List<ITaskItem>();

                if(buildProcess != null && lastBuildStep == BuildStep.Completed)
                    foreach(string file in buildProcess.Help2Files)
                        files.Add(new TaskItem(file));

                return files.ToArray();
            }
        }

        /// <summary>
        /// This is used to return a list of the MS Help Viewer (MSHC) files
        /// that resulted from the build.
        /// </summary>
        [Output]
        public ITaskItem[] HelpViewerFiles
        {
            get
            {
                List<ITaskItem> files = new List<ITaskItem>();

                if(buildProcess != null && lastBuildStep == BuildStep.Completed)
                    foreach(string file in buildProcess.HelpViewerFiles)
                        files.Add(new TaskItem(file));

                return files.ToArray();
            }
        }

        /// <summary>
        /// This is used to return a list of the website files that resulted
        /// from the build.
        /// </summary>
        [Output]
        public ITaskItem[] WebsiteFiles
        {
            get
            {
                List<ITaskItem> files = new List<ITaskItem>();

                if(buildProcess != null && lastBuildStep == BuildStep.Completed)
                    foreach(string file in buildProcess.WebsiteFiles)
                        files.Add(new TaskItem(file));

                return files.ToArray();
            }
        }

        /// <summary>
        /// This is used to return a list of all files that resulted from the
        /// build (all help formats).
        /// </summary>
        [Output]
        public ITaskItem[] AllHelpFiles
        {
            get
            {
                return this.Help1Files.Concat(
                    this.Help2Files.Concat(
                    this.HelpViewerFiles.Concat(
                    this.WebsiteFiles))).ToArray();
            }
        }
        #endregion

        #region Execute method
        //=====================================================================

        /// <summary>
        /// This is used to execute the task and perform the build
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            Project msBuildProject = null;
            ProjectInstance projectInstance = null;
            bool removeProjectWhenDisposed = false;
            string line;

            // If cancelled already, just return
            if(buildCancelled)
                return false;

            try
            {
                if(!this.AlwaysLoadProject)
                {
                    // Use the current project if possible.  This is preferable as we can make use
                    // of command line property overrides and any other user-defined properties.
                    this.ProjectFile = Path.GetFullPath(this.ProjectFile);

                    // This collection is new to the MSBuild 4.0 API.  If you load a project, it appears
                    // in the collection.  However, if you run MSBuild.exe, it doesn't put the project in
                    // this collection.  As such, we must still resort to reflection to get the executing
                    // project.  I'm leaving this here in case that ever changes as this is preferable to
                    // using Reflection to get at the current project.
                    var matchingProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(
                        this.ProjectFile);

                    if(matchingProjects.Count != 0)
                    {
                        if(matchingProjects.Count != 1)
                            Log.LogWarning(null, "BHT0004", "BHT0004", "SHFB", 0, 0, 0, 0, "Multiple matching " +
                                "projects were found.  Only the first one found will be built.");

                        msBuildProject = matchingProjects.First();
                    }
                    else
                        projectInstance = this.GetCurrentProjectInstance();
                }
            }
            catch(Exception ex)
            {
                // Ignore exceptions but issue a warning and fall back to using
                // the passed project filename instead.
                Log.LogWarning(null, "BHT0001", "BHT0001", "SHFB", 0, 0, 0, 0, "Unable to get executing " +
                    "project: {0}.  The specified project will be loaded but command line property " +
                    "overrides will be ignored.", ex.Message);
            }

            try
            {
                if(msBuildProject == null)
                {
                    removeProjectWhenDisposed = true;

                    if(projectInstance != null)
                    {
                        msBuildProject = new Project(projectInstance.ToProjectRootElement());

                        // ToProjectRootElement() will not add properties in the global collection to the
                        // project.  One problem with this is that command line overrides get missed.  As such,
                        // we'll add them back to the project as long as they are not reserved names and are not
                        // there already.
                        foreach(var p in projectInstance.GlobalProperties)
                            if(!SandcastleProject.restrictedProps.Contains(p.Key) &&
                              !msBuildProject.AllEvaluatedProperties.Any(ep => ep.Name == p.Key))
                                msBuildProject.SetProperty(p.Key, p.Value);

                        msBuildProject.FullPath = this.ProjectFile;
                    }
                    else
                    {
                        if(!File.Exists(this.ProjectFile))
                            throw new BuilderException("BHT0003", "The specified project file does not exist: " +
                                this.ProjectFile);

                        Log.LogWarning(null, "BHT0001", "BHT0001", "SHFB", 0, 0, 0, 0, "Unable to get " +
                            "executing project:  Unable to obtain matching project from the global " +
                            "collection.  The specified project will be loaded but command line property " +
                            "overrides will be ignored.");

                        // Create the project and set the configuration and platform options
                        msBuildProject = new Project(this.ProjectFile);
                    }

                    msBuildProject.SetGlobalProperty(ProjectElement.Configuration, this.Configuration);
                    msBuildProject.SetGlobalProperty(ProjectElement.Platform, this.Platform);

                    // Override the OutDir property if defined for Team Build.  Ignore ".\" as that's our default.
                    if(!String.IsNullOrEmpty(this.OutDir) && this.OutDir != @".\")
                        msBuildProject.SetGlobalProperty(ProjectElement.OutDir, this.OutDir);

                    msBuildProject.ReevaluateIfNecessary();
                }

                // Associate the MSBuild project with a SHFB project instance and build it
                using(sandcastleProject = new SandcastleProject(msBuildProject))
                {
                    buildProcess = new BuildProcess(sandcastleProject);
                    buildProcess.BuildStepChanged += buildProcess_BuildStepChanged;
                    buildProcess.BuildProgress += buildProcess_BuildProgress;

                    // Since this is an MSBuild task, we'll run it directly rather than in a background thread
                    Log.LogMessage(MessageImportance.High, "Building {0}", msBuildProject.FullPath);
                    buildProcess.Build();
                }
            }
            catch(Exception ex)
            {
                Log.LogError(null, "BHT0002", "BHT0002", "SHFB", 0, 0, 0, 0,
                    "Unable to build project '{0}': {1}", msBuildProject.FullPath, ex);
            }
            finally
            {
                // If we loaded it, we must unload it.  If not, it is cached and may cause problems later.
                if(removeProjectWhenDisposed)
                {
                    ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject);
                    ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject.Xml);
                }
            }

            if(this.DumpLogOnFailure && lastBuildStep == BuildStep.Failed)
                using(StreamReader sr = new StreamReader(buildProcess.LogFilename))
                {
                    Log.LogMessage(MessageImportance.High, "Log Content:");

                    do
                    {
                        line = sr.ReadLine();

                        // Don't output the XML elements, just the text
                        if(line != null && (line.Trim().Length == 0 || line.Trim()[0] != '<'))
                            Log.LogMessage(MessageImportance.High, line);

                    } while(line != null);
                }

            return (lastBuildStep == BuildStep.Completed);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to obtain project instance for the project that is
        /// currently being built.
        /// </summary>
        /// <returns>The project instance for the current project if possible
        /// or null if it could not be obtained.</returns>
        /// <remarks>When you run MSBuild.exe, it does not store the projects
        /// in the global project collection.  We could build the project
        /// without it but we lose the ability to use command line overrides
        /// and changes to user-defined properties.  As such we need to resort
        /// to reflection to get the current project information.  This is
        /// easier than in past MSBuild versions though.</remarks>
        private ProjectInstance GetCurrentProjectInstance()
        {
            FieldInfo fieldInfo;
            PropertyInfo propInfo;
            IEnumerable configCache;
            ProjectInstance project, lastMatchingProject = null;

            // From the BuildManager...
            fieldInfo = typeof(BuildManager).GetField("configCache", BindingFlags.NonPublic | BindingFlags.Instance);

            if(fieldInfo == null)
                return null;

            // ... get the build request configuration cache ...
            configCache = (IEnumerable)fieldInfo.GetValue(BuildManager.DefaultBuildManager);

            if(configCache == null)
                return null;

            // ... then find our project if it is there.
            foreach(Object config in configCache)
            {
                propInfo = config.GetType().GetProperty("Project", BindingFlags.Public | BindingFlags.Instance);

                if(propInfo != null)
                {
                    project = (ProjectInstance)propInfo.GetValue(config, null);

                    // If found, return the XML that defines all of the project settings.  There may be more
                    // than one instance.  Use the last one as that seems to be the one to which any command
                    // line overrides have been applied.
                    if(project != null && project.FullPath == this.ProjectFile)
                        lastMatchingProject = project;
                }
            }

            return lastMatchingProject;
        }

        /// <summary>
        /// This is called by the build process thread to update the
        /// application with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender, BuildProgressEventArgs e)
        {
            string outputPath;

            if(!this.Verbose)
                Log.LogMessage(MessageImportance.High, e.BuildStep.ToString());

            if(e.HasCompleted)
            {
                // If successful, report the location of the help file/website
                if(e.BuildStep == BuildStep.Completed)
                {
                    outputPath = buildProcess.OutputFolder + sandcastleProject.HtmlHelpName;

                    switch(sandcastleProject.HelpFileFormat)
                    {
                        case HelpFileFormat.HtmlHelp1:
                            outputPath += ".chm";
                            break;

                        case HelpFileFormat.MSHelp2:
                            outputPath += ".hxs";
                            break;

                        case HelpFileFormat.MSHelpViewer:
                            outputPath += ".mshc";
                            break;

                        default:
                            break;
                    }

                    // Report single file or multi-format output location
                    if(File.Exists(outputPath))
                        Log.LogMessage(MessageImportance.High, "The help file is located at: {0}", outputPath);
                    else
                        Log.LogMessage(MessageImportance.High, "The help output is located at: {0}", buildProcess.OutputFolder);
                }

                if(File.Exists(buildProcess.LogFilename))
                    Log.LogMessage(MessageImportance.High, "Build details can be found in {0}", buildProcess.LogFilename);
            }

            lastBuildStep = e.BuildStep;
        }

        /// <summary>
        /// This is called by the build process thread to update the
        /// task with information about progress.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildProgress(object sender, BuildProgressEventArgs e)
        {
            Match m = reParseMessage.Match(e.Message);

            // Always log errors and warnings
            if(m.Success)
            {
                if(String.Compare(m.Groups[3].Value, "warning", StringComparison.OrdinalIgnoreCase) == 0)
                    Log.LogWarning(null, m.Groups[4].Value, m.Groups[4].Value,
                        m.Groups[1].Value, 0, 0, 0, 0, m.Groups[5].Value.Trim());
                else
                    if(String.Compare(m.Groups[3].Value, "error", StringComparison.OrdinalIgnoreCase) == 0)
                        Log.LogError(null, m.Groups[4].Value, m.Groups[4].Value,
                            m.Groups[1].Value, 0, 0, 0, 0, m.Groups[5].Value.Trim());
                    else
                        Log.LogMessage(MessageImportance.High, e.Message);
            }
            else
                if(this.Verbose)
                    Log.LogMessage(MessageImportance.High, e.Message);
        }
        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will be cancelled as soo as the next message
        /// arrives from the build process.</remarks>
        public void Cancel()
        {
            buildCancelled = true;

            if(buildProcess != null)
                buildProcess.BuildCanceled = true;
        }
        #endregion
    }
}
