//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildHelp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/01/2015
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to build help file output using the Sandcastle Help File Builder
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//     Date     Who  Comments
// ==============================================================================================================
// 06/27/2008  EFW  Created the code
// 12/19/2008  EFW  Updated to work with MSBuild 3.5 and Team Build
// 04/20/2009  EFW  Added DumpLogOnFailure property
// 07/06/2009  EFW  Added support for MS Help Viewer output files
// 07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 12/21/2013  EFW  Removed support for SHFBCOMPONENT root as the ComponentPath project property handles its
//                  functionality now.
// 02/15/2014  EFW  Added support for the Open XML output format
// 03/30/2015  EFW  Added support for the Markdown output format
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to build help file output using the Sandcastle Help File Builder.
    /// </summary>
    /// <remarks>All messages from this task are logged with a high priority since it will run for a long time
    /// and we need to see the progress messages to know it's doing something.  If set to Normal and ran from
    /// within Visual Studio, it won't show the progress messages when the logging options are set to Minimal.</remarks>
    public class BuildHelp : Task, ICancelableTask, IProgress<BuildProgressEventArgs>
    {
        #region Private data members
        //=====================================================================

        private static Regex reParseMessage = new Regex(@"^(\w{2,}):\s*(.*?)\s*" +
            @"\W(warning|error)\W\s*(\w+?\d*?):\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex reWarning = new Regex(@"(Warn|Warning( HXC\d+)?):|" +
            @"SHFB\s*:\s*(W|w)arning\s.*?:|.*?(\(\d*,\d*\))?:\s*(W|w)arning\s.*?:");

        private SandcastleProject sandcastleProject;
        private BuildProcess buildProcess;
        private BuildStep lastBuildStep;

        private CancellationTokenSource cts;
        private bool buildCancelled;
        #endregion

        #region Task input properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the project filename
        /// </summary>
        /// <remarks>Since <see cref="SandcastleProject" /> already wraps the MSBuild project, it seemed
        /// redundant to define each and every property on this task and map them to the project properties.  As
        /// such, this task will attempt to use the executing project to create the Sandcastle project instance.
        /// If that fails or <see cref="AlwaysLoadProject" /> is true, this file will be loaded instead.  The
        /// downside is that property overrides on the command line will be ignored.</remarks>
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
        /// This is used to specify the output directory containing the build output for solution and project
        /// documentation sources when using Team Build.
        /// </summary>
        /// <value>This property is optional.  If not specified, the default output path in project file
        /// documentation sources will be used.</value>
        public string OutDir { get; set; }

        /// <summary>
        /// This is used to set or get the output logging verbosity flag
        /// </summary>
        /// <value>This property is optional.  If set to false (the default), only build steps are written to the
        /// task log.  If set to true, all output from the build process is written to the task log.</value>
        public bool Verbose { get; set; }

        /// <summary>
        /// This is used to set or get whether the log file is dumped to the task log if the help file project
        /// build fails.
        /// </summary>
        /// <value>This property is optional.  If set to false (the default), the log is not dumped if the build
        /// fails.  If set to true, all output from the build process is written to the task log if the build
        /// fails.</value>
        public bool DumpLogOnFailure { get; set; }

        /// <summary>
        /// This is used to specify whether or not to load the specified <see cref="ProjectFile" /> rather than
        /// use the executing project.
        /// </summary>
        /// <value>This property is optional.  If set to false, the default, the executing project is used as the
        /// Sandcastle project to build.  If set to true, the specified <see cref="ProjectFile" /> is loaded.
        /// In such cases, command line property overrides are ignored.</value>
        public bool AlwaysLoadProject { get; set; }

    /// <summary>
    /// <para>Optional String parameter.</para>
    /// <para>A semicolon-delimited list of property name/value pairs that override properties read from the <see cref="ProjectFile" />.</para>
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Use this to provide dynamic properties, that are created during build. When building inside Visual Studio,
    ///     only static properties are available.</para>
    ///   <para>
    ///     This could for example be used if there are custom msbuild targets that initialize properties with version information.</para>
    /// </remarks>
    /// <example>Properties="Version=$(SemVersion);Optimize=$(Optimize)"</example>
    public string Properties { get; set; }

      #endregion

        #region Task output properties
        //=====================================================================

        /// <summary>
        /// This is used to return a list of the HTML Help 1 (chm) files that resulted from the build
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
        /// This is used to return a list of the MS Help Viewer (mshc) files that resulted from the build
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
        /// This is used to return a list of the website files that resulted from the build
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
        /// This is used to return a list of the Open XML (docx) files that resulted from the build
        /// </summary>
        [Output]
        public ITaskItem[] OpenXmlFiles
        {
            get
            {
                List<ITaskItem> files = new List<ITaskItem>();

                if(buildProcess != null && lastBuildStep == BuildStep.Completed)
                    foreach(string file in buildProcess.OpenXmlFiles)
                        files.Add(new TaskItem(file));

                return files.ToArray();
            }
        }

        /// <summary>
        /// This is used to return a list of the Markdown (md) files that resulted from the build
        /// </summary>
        [Output]
        public ITaskItem[] MarkdownFiles
        {
            get
            {
                List<ITaskItem> files = new List<ITaskItem>();

                if(buildProcess != null && lastBuildStep == BuildStep.Completed)
                    foreach(string file in buildProcess.MarkdownFiles)
                        files.Add(new TaskItem(file));

                return files.ToArray();
            }
        }

        /// <summary>
        /// This is used to return a list of all files that resulted from the build (all help formats)
        /// </summary>
        [Output]
        public ITaskItem[] AllHelpFiles
        {
            get
            {
                return this.Help1Files.Concat(
                    this.HelpViewerFiles.Concat(
                    this.WebsiteFiles.Concat(
                    this.OpenXmlFiles.Concat(
                    this.MarkdownFiles)))).ToArray();
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

            // If canceled already, just return
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

                    msBuildProject.SetGlobalProperty(BuildItemMetadata.Configuration, this.Configuration);
                    msBuildProject.SetGlobalProperty(BuildItemMetadata.Platform, this.Platform);

                    // Override the OutDir property if defined for Team Build.  Ignore ".\" as that's our default.
                    if(!String.IsNullOrEmpty(this.OutDir) && this.OutDir != @".\")
                        msBuildProject.SetGlobalProperty(BuildItemMetadata.OutDir, this.OutDir);

                    msBuildProject.ReevaluateIfNecessary();
                }

              // initialize properties that where provided in Properties
              if (!string.IsNullOrWhiteSpace(Properties))
              {
                foreach (string propertyKeyValue in this.Properties.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                {
                  int length = propertyKeyValue.IndexOf('=');
                  if (length != -1)
                  {
                    string propertyKey = propertyKeyValue.Substring(0, length).Trim();
                    string propertyValue = propertyKeyValue.Substring(length + 1).Trim();

                    if (!string.IsNullOrWhiteSpace(propertyKey))
                    {
                      Log.LogMessage(MessageImportance.Low, "Setting property {0}={1}", propertyKey, propertyValue);
                      msBuildProject.SetGlobalProperty(propertyKey, propertyValue);
                    }
                  }
                }

                msBuildProject.ReevaluateIfNecessary();
              }

              cts = new CancellationTokenSource();

                // Associate the MSBuild project with a SHFB project instance and build it
                using(sandcastleProject = new SandcastleProject(msBuildProject))
                {
                    buildProcess = new BuildProcess(sandcastleProject)
                    {
                        ProgressReportProvider = this,
                        CancellationToken = cts.Token
                    };

                    // Since this is an MSBuild task, we'll run it directly rather than in a background thread
                    Log.LogMessage(MessageImportance.High, "Building {0}", msBuildProject.FullPath);
                    buildProcess.Build();

                    lastBuildStep = buildProcess.CurrentBuildStep;
                }
            }
            catch(Exception ex)
            {
                Log.LogError(null, "BHT0002", "BHT0002", "SHFB", 0, 0, 0, 0,
                    "Unable to build project '{0}': {1}", msBuildProject.FullPath, ex);
            }
            finally
            {
                if(cts != null)
                {
                    cts.Dispose();
                    cts = null;
                }

                // If we loaded it, we must unload it.  If not, it is cached and may cause problems later.
                if(removeProjectWhenDisposed && msBuildProject != null)
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
        /// This is used to obtain project instance for the project that is currently being built
        /// </summary>
        /// <returns>The project instance for the current project if possible or null if it could not be
        /// obtained.</returns>
        /// <remarks>When you run MSBuild.exe, it does not store the projects in the global project collection.
        /// We could build the project without it but we lose the ability to use command line overrides and
        /// changes to user-defined properties.  As such we need to resort to reflection to get the current
        /// project information.  This is easier than in past MSBuild versions though.</remarks>
        private ProjectInstance GetCurrentProjectInstance()
        {
            FieldInfo fieldInfo;
            PropertyInfo propInfo;
            IEnumerable configCache;
            ProjectInstance project, lastMatchingProject = null;
            string projectInstanceFieldName = "projectInstance", configCacheFieldName = "configCache";

            // See if we can get the project instance from the build engine for this task.  This is preferred
            // as it will work when building projects synchronously or in parallel.
            Type taskHostType = Type.GetType("Microsoft.Build.BackEnd.TaskHost,Microsoft.Build");
            Type targetBuilderType = Type.GetType("Microsoft.Build.BackEnd.TargetBuilder,Microsoft.Build");

            if(taskHostType != null && targetBuilderType != null)
            {
                // From the this task's build engine ...
                fieldInfo = taskHostType.GetField("targetBuilderCallback", BindingFlags.NonPublic |
                    BindingFlags.Instance);

                if(fieldInfo == null)
                {
                    // MSBuild 14.0 adds an underscore to the field names
                    fieldInfo = taskHostType.GetField("_targetBuilderCallback", BindingFlags.NonPublic |
                        BindingFlags.Instance);

                    if(fieldInfo != null)
                    {
                        projectInstanceFieldName = "_" + projectInstanceFieldName;
                        configCacheFieldName = "_" + configCacheFieldName;
                    }
                }

                if(fieldInfo != null)
                {
                    // ... get the target builder ...
                    object targetBuilder = fieldInfo.GetValue(this.BuildEngine);

                    if(targetBuilder != null)
                    {
                        fieldInfo = targetBuilderType.GetField(projectInstanceFieldName, BindingFlags.NonPublic |
                            BindingFlags.Instance);

                        if(fieldInfo != null)
                        {
                            // ... then get the project instance XML from it.
                            project = (ProjectInstance)fieldInfo.GetValue(targetBuilder);

                            if(project != null)
                                return project;
                        }
                    }
                }
            }

            // If not, from the BuildManager ...
            fieldInfo = typeof(BuildManager).GetField(configCacheFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

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
        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will be cancelled as soon as the next message arrives from the build process</remarks>
        public void Cancel()
        {
            buildCancelled = true;

            if(cts != null)
                cts.Cancel();
        }
        #endregion

        #region IProgress<BuildProgressEventArgs> Members
        //=====================================================================

        /// <summary>
        /// This is called by the build process to report build progress
        /// </summary>
        /// <param name="value">The event arguments</param>
        /// <remarks>Since the build is synchronous in this task, we need to implement the interface and report
        /// progress synchronously as well or the final few messages can get lost and it looks like the build
        /// failed.</remarks>
        public void Report(BuildProgressEventArgs value)
        {
            Match m = reParseMessage.Match(value.Message);
            string outputPath;

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
                        Log.LogMessage(MessageImportance.High, value.Message);
            }
            else
                if(this.Verbose)
                    Log.LogMessage(MessageImportance.High, value.Message);
                else
                {
                    // If not doing verbose logging, show warnings and let MSBuild filter them out if not
                    // wanted.  Errors will kill the build so we don't have to deal with them here.
                    if(reWarning.IsMatch(value.Message))
                        Log.LogWarning(value.Message);
                }

            if(value.StepChanged)
            {
                if(!this.Verbose)
                    Log.LogMessage(MessageImportance.High, value.BuildStep.ToString());

                if(value.HasCompleted)
                {
                    // If successful, report the location of the help file/website
                    if(value.BuildStep == BuildStep.Completed)
                    {
                        outputPath = buildProcess.OutputFolder + buildProcess.ResolvedHtmlHelpName;

                        switch(sandcastleProject.HelpFileFormat)
                        {
                            case HelpFileFormats.HtmlHelp1:
                                outputPath += ".chm";
                                break;

                            case HelpFileFormats.MSHelpViewer:
                                outputPath += ".mshc";
                                break;

                            case HelpFileFormats.OpenXml:
                                outputPath += ".docx";
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
            }
        }
        #endregion
    }
}
