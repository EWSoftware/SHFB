//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildHelp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/27/2009
// Note    : Copyright 2008-2009, Eric Woodruff, All rights reserved
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
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to build help file output using the Sandcastle Help
    /// File Builder.
    /// </summary>
    public class BuildHelp : Task
    {
        #region Private data members
        //=====================================================================

        private static Regex reParseMessage = new Regex(@"^(\w{2,}):\s*(.*?)\s*" +
            @"\W(warning|error)\W\s*(\w+?\d*?):\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private SandcastleProject sandcastleProject;
        private BuildProcess buildProcess;
        private BuildStep lastBuildStep;

        private string projectFile, configuration, platform, outDir;
        private bool verbose, alwaysLoadProject, isPriorMSBuildVersion, dumpLogOnFailure;
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
        public string ProjectFile
        {
            get { return projectFile; }
            set { projectFile = value; }
        }

        /// <summary>
        /// This is used to pass in the configuration to use for the build
        /// </summary>
        [Required]
        public string Configuration
        {
            get { return configuration; }
            set { configuration = value; }
        }

        /// <summary>
        /// This is used to pass in the platform to use for the build
        /// </summary>
        [Required]
        public string Platform
        {
            get { return platform; }
            set { platform = value; }
        }

        /// <summary>
        /// This is used to specify the output directory containing the build
        /// output for solution and project documentation sources when using
        /// Team Build.
        /// </summary>
        /// <value>This property is optional.  If not specified, the default
        /// output path in project file documentation sources will be used.</value>
        public string OutDir
        {
            get { return outDir; }
            set { outDir = value; }
        }

        /// <summary>
        /// This is used to set or get the output logging verbosity flag
        /// </summary>
        /// <value>This property is optional.  If set to false (the default),
        /// only build steps are written to the task log.  If set to true, all
        /// output from the build process is written to the task log.</value>
        public bool Verbose
        {
            get { return verbose; }
            set { verbose = value; }
        }

        /// <summary>
        /// This is used to set or get whether the log file is dumped to the
        /// task log if the help file project build fails.
        /// </summary>
        /// <value>This property is optional.  If set to false (the default),
        /// the log is not dumped if the build fails.  If set to true, all
        /// output from the build process is written to the task log if the
        /// build fails.</value>
        public bool DumpLogOnFailure
        {
            get { return dumpLogOnFailure; }
            set { dumpLogOnFailure = value; }
        }

        /// <summary>
        /// This is used to specify whether or not to load the specified
        /// <see cref="ProjectFile" /> rather than use the executing project.
        /// </summary>
        /// <value>This property is optional.  If set to false, the default,
        /// the executing project is used as the Sandcastle project to build.
        /// If set to true, the specified <see cref="ProjectFile" /> is loaded.
        /// In such cases, command line property overrides are ignored.</value>
        public bool AlwaysLoadProject
        {
            get { return alwaysLoadProject; }
            set { alwaysLoadProject = value; }
        }
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
            string line;

            try
            {
                if(!alwaysLoadProject)
                {
                    // Use the current project if possible.  This is preferable
                    // as we can make use of command line property overrides
                    // and any other user-defined properties.
                    msBuildProject = this.GetCurrentProject();

                    if(msBuildProject == null)
                    {
                        // We can't use a prior MSBuild version
                        if(isPriorMSBuildVersion)
                        {
                            Log.LogError(null, "BHT0004", "BHT0004", "SHFB",
                                0, 0, 0, 0, "An older MSBuild version is " +
                                "being used.  Unable to build help project.");
                            return false;
                        }

                        Log.LogWarning(null, "BHT0001", "BHT0001", "SHFB",
                            0, 0, 0, 0, "Unable to get executing project: " +
                            "Unable to obtain internal reference.  The " +
                            "specified project will be loaded but command " +
                            "line property overrides will be ignored.");
                    }
                }
            }
            catch(Exception ex)
            {
                // Ignore exceptions but issue a warning and fall back to using
                // the passed project filename instead.
                Log.LogWarning(null, "BHT0001", "BHT0001", "SHFB", 0, 0, 0, 0,
                    "Unable to get executing project: {0}.  The specified " +
                    "project will be loaded but command line property " +
                    "overrides will be ignored.", ex.Message);
            }

            if(msBuildProject == null)
            {
                // Create the project and set the configuration and platform
                // options.
                msBuildProject = new Project(Engine.GlobalEngine);
                msBuildProject.GlobalProperties.SetProperty(
                    ProjectElement.Configuration, configuration);
                msBuildProject.GlobalProperties.SetProperty(
                    ProjectElement.Platform, platform);

                // Override the OutDir property if defined for Team Build
                if(!String.IsNullOrEmpty(outDir))
                    msBuildProject.GlobalProperties.SetProperty(
                        ProjectElement.OutDir, outDir);

                if(!File.Exists(projectFile))
                    throw new BuilderException("BHT0003", "The specified " +
                        "project file does not exist: " + projectFile);

                msBuildProject.Load(projectFile);
            }

            // Load the MSBuild project and associate it with a SHFB
            // project instance.
            sandcastleProject = new SandcastleProject(msBuildProject, true);

            try
            {
                buildProcess = new BuildProcess(sandcastleProject);
                buildProcess.BuildStepChanged +=
                    new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildStepChanged);
                buildProcess.BuildProgress +=
                    new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildProgress);

                // Since this is an MSBuild task, we'll run it directly rather
                // than in a background thread.
                Log.LogMessage("Building {0}", msBuildProject.FullFileName);
                buildProcess.Build();
            }
            catch(Exception ex)
            {
                Log.LogError(null, "BHT0002", "BHT0002", "SHFB", 0, 0, 0, 0,
                  "Unable to build project '{0}': {1}",
                  msBuildProject.FullFileName, ex);
            }

            if(dumpLogOnFailure && lastBuildStep == BuildStep.Failed)
                using(StreamReader sr = new StreamReader(buildProcess.LogFilename))
                {
                    Log.LogMessage(MessageImportance.High, "Log Content:");

                    do
                    {
                        line = sr.ReadLine();

                        // Don't output the XML elements, just the text
                        if(line != null && (line.Trim().Length == 0 ||
                          line.Trim()[0] != '<'))
                            Log.LogMessage(MessageImportance.High, line);

                    } while(line != null);
                }

            return (lastBuildStep == BuildStep.Completed);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to obtain a reference to the project that is currently
        /// being built.
        /// </summary>
        /// <returns>The current project if possible or null if it could not
        /// be obtained.</returns>
        /// <remarks>The build engine provides no way to get a reference to
        /// the current project.  As such, we have to resort to reflection to
        /// get it.  This was much easier under .NET 2.0 as the project was a
        /// project of the BuildEngine object.  The .NET 3.5 build engine hides
        /// it way down in the object hierarchy.  We could build the project
        /// without it but we lose the ability to use command line overrides
        /// and changes to user-defined properties.</remarks>
        private Project GetCurrentProject()
        {
            Type type;
            FieldInfo fieldInfo;
            object obj;

            if(base.BuildEngine == null)
                return null;

            // From the EngineProxy ...
            type = base.BuildEngine.GetType();
            fieldInfo = type.GetField("parentModule",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if(fieldInfo == null)
            {
                // If null, it's probably running under MSBuild 2.0
                fieldInfo = type.GetField("project",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                // Unfortunately, we can't cast a copy of the object from
                // a prior MSBuild version to this version's Project type
                // so it will fail to build.
                if(fieldInfo != null)
                    isPriorMSBuildVersion = true;

                return null;
            }

            obj = fieldInfo.GetValue(base.BuildEngine);

            if(obj == null)
                return null;

            // ... we go to the TaskExecutionModule ...
            type = obj.GetType();
            fieldInfo = type.GetField("engineCallback",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if(fieldInfo == null)
                return null;

            obj = fieldInfo.GetValue(obj);

            if(obj == null)
                return null;

            // ... then into the EngineCallBack ...
            type = obj.GetType();
            fieldInfo = type.GetField("parentEngine",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if(fieldInfo == null)
                return null;

            obj = fieldInfo.GetValue(obj);

            if(obj == null)
                return null;

            // ... and finally we arrive at the Engine object ...
            type = obj.GetType();
            fieldInfo = type.GetField("cacheOfBuildingProjects",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if(fieldInfo == null)
                return null;

            obj = fieldInfo.GetValue(obj);

            if(obj == null)
                return null;

            // ... which has the cache of bulding projects ...
            type = obj.GetType();
            fieldInfo = type.GetField("projects",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if(fieldInfo == null)
                return null;

            obj = fieldInfo.GetValue(obj);

            if(obj == null)
                return null;

            // ... which contains the hash table we need containing the
            // actual project reference.
            foreach(DictionaryEntry entry in (Hashtable)obj)
                foreach(Project project in (ArrayList)entry.Value)
                    if(project.FullFileName == projectFile)
                        return project;

            return null;
        }

        /// <summary>
        /// This is called by the build process thread to update the
        /// application with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender,
          BuildProgressEventArgs e)
        {
            string outputPath;

            if(!verbose)
                Log.LogMessage(e.BuildStep.ToString());

            if(e.HasCompleted)
            {
                // If successful, report the location of the help file/website
                if(e.BuildStep == BuildStep.Completed)
                {
                    outputPath = buildProcess.OutputFolder +
                        sandcastleProject.HtmlHelpName;

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
                        Log.LogMessage("The help file is located at: {0}",
                            outputPath);
                    else
                        Log.LogMessage("The help output is located at: {0}",
                            buildProcess.OutputFolder);
                }

                if(File.Exists(buildProcess.LogFilename))
                    Log.LogMessage("Build details can be found in {0}",
                        buildProcess.LogFilename);
            }

            lastBuildStep = e.BuildStep;
        }

        /// <summary>
        /// This is called by the build process thread to update the
        /// task with information about progress.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildProgress(object sender,
          BuildProgressEventArgs e)
        {
            Match m = reParseMessage.Match(e.Message);

            // Always log errors and warnings
            if(m.Success)
            {
                if(String.Compare(m.Groups[3].Value, "warning",
                  StringComparison.OrdinalIgnoreCase) == 0)
                    Log.LogWarning(null, m.Groups[4].Value, m.Groups[4].Value,
                      m.Groups[1].Value, 0, 0, 0, 0,
                      m.Groups[5].Value.Trim());
                else
                    if(String.Compare(m.Groups[3].Value, "error",
                      StringComparison.OrdinalIgnoreCase) == 0)
                        Log.LogError(null, m.Groups[4].Value, m.Groups[4].Value,
                          m.Groups[1].Value, 0, 0, 0, 0,
                          m.Groups[5].Value.Trim());
                    else
                        Log.LogMessage(e.Message);
            }
            else
                if(verbose)
                    Log.LogMessage(e.Message);
        }
        #endregion
    }
}
