//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/15/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the thread class that handles all aspects of the build
// process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
#region Older history
// 1.0.0.0  08/04/2006  EFW  Created the code
// 1.3.0.0  09/09/2006  EFW  Added support for website output
// 1.3.1.0  09/29/2006  EFW  Added support for the ShowMissing* properties
// 1.3.1.0  10/02/2006  EFW  Added support for the September CTP and the
//                           the Document* properties.
// 1.3.2.0  11/10/2006  EFW  Added support for DocumentAssembly.CommentsOnly
// 1.3.4.0  12/24/2006  EFW  Various additions and updates
// 1.4.0.0  03/07/2007  EFW  Added support for the March 2007 CTP
// 1.5.0.0  06/19/2007  EFW  Various additions and updates for the June CTP
// 1.5.0.2  07/03/2007  EFW  Added support for a content site map file and
//                           reworked support for language resources.  Also
//                           added support for namespace ripping.
// 1.5.1.0  07/20/2007  EFW  Added support for the API filter property
// 1.5.2.0  09/10/2007  EFW  Exposed some members for use by plug-ins and
//                           added support for calling the plug-ins.
// 1.6.0.0  10/04/2007  EFW  Added support for the September 2007 CTP
// 1.6.0.1  10/29/2007  EFW  Added support for the October 2007 CTP
// 1.6.0.4  01/16/2008  EFW  Added support for the January 2008 release
// 1.6.0.5  02/04/2008  EFW  Added support for the new Extract HTML Info tool
//                           and the <inheritdoc /> tag.
// 1.6.0.6  03/09/2008  EFW  Wrapped the log and build steps in XML tags
// 1.6.0.7  04/17/2008  EFW  Added support for wildcards in assembly names.
//                           Added support for conceptual content.
#endregion
// 1.8.0.0  07/26/2008  EFW  Modified to support the new project format
// 1.8.0.1  12/14/2008  EFW  Updated to use .NET 3.5 and MSBuild 3.5
// 1.8.0.3  07/04/2009  EFW  Added support for the July 2009 release and
//                           building MS Help Viewer files.
// 1.9.0.0  05/22/2010  EFW  Added support for the June 2010 release.  Reworked
//                           solution file handling to honor solution-level
//                           per-project configuration and platform settings.
//                           Added support for multi-format build output. Moved
//                           GenerateIntermediateTableOfContents so that it
//                           occurs right after MergeTablesOfContents.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.Design;
using SandcastleBuilder.Utils.MSBuild;
using SandcastleBuilder.Utils.PlugIn;

using Microsoft.Build.BuildEngine;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class is used to handle all aspects of the build process in a
    /// separate thread.
    /// </summary>
    public partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject project;      // The project to build
        private string originalProjectName;

        // Assembly and reference information
        private Collection<string> assembliesList;
        private Dictionary<string, BuildItem> referenceDictionary;

        // Conceptual content settings
        private ConceptualContentSettings conceptualContent;
        private int apiTocOrder;
        private string apiTocParentId, rootContentContainerId;

        // The log file stream
        private StreamWriter swLog;

        // Partial build flag for obtaining API information
        private bool isPartialBuild, suppressApiFilter;

        // Build output file lists
        private Collection<string> help1Files, help2Files, helpViewerFiles,
            websiteFiles;

        // Progress event arguments
        private BuildProgressEventArgs progressArgs;
        private DateTime buildStart, stepStart;

        // Various paths and other strings
        private string shfbFolder, templateFolder, projectFolder, outputFolder,
            workingFolder, sandcastleFolder, hhcFolder, hxcompFolder,
            languageFolder, webFolder, presentationFolder, defaultTopic,
            namespacesTopic, reflectionFile, presentationParam, msBuildExePath;

        private Collection<string> helpFormatOutputFolders;

        // Process information for the tools and scripts
        private Process currentProcess;
        private Thread stdOutThread, stdErrThread;
        private bool errorDetected;

        private CultureInfo language;   // The project language

        // The current help file format being generated
        private HelpFileFormat currentFormat;

        // The current reflection information file used in various steps
        private XmlDocument reflectionInfo;
        private XmlNode apisNode;

        // Regular expressions used for error message checking
        private static Regex reErrorCheck = new Regex(
            @"^\s*((Error|UnrecognizedOption|Unhandled Exception|Fatal Error|" +
            @"Unexpected error.*|HHC\d+: Error|(Fatal )?Error HXC\d+):|" +
            @"Process is terminated|Build FAILED|\w+\s*:\s*Error\s.*?:|" +
            @"\w.*?\(\d*,\d*\):\s*error\s.*?:)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex reKillProcess = new Regex(
            "hhc|hxcomp|BuildAssembler|XslTransform|MRefBuilder|" +
            "GenerateInheritedDocs|SandcastleHtmlExtract",
            RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the path to MSBuild.exe
        /// </summary>
        public string MSBuildExePath
        {
            get { return msBuildExePath; }
        }

        /// <summary>
        /// This returns the location of the help file builder executables
        /// </summary>
        public string HelpFileBuilderFolder
        {
            get { return shfbFolder; }
        }

        /// <summary>
        /// This returns the location of the help file builder template
        /// folder.
        /// </summary>
        public string TemplateFolder
        {
            get { return templateFolder; }
        }

        /// <summary>
        /// This returns the project folder name
        /// </summary>
        public string ProjectFolder
        {
            get { return projectFolder; }
        }

        /// <summary>
        /// This returns the project filename without the folder
        /// </summary>
        public string ProjectFilename
        {
            get { return Path.GetFileName(originalProjectName); }
        }

        /// <summary>
        /// This returns the output folder where the log file and help file
        /// can be found after the build process has finished.
        /// </summary>
        public string OutputFolder
        {
            get { return outputFolder; }
        }

        /// <summary>
        /// This returns the name of the working files folder
        /// </summary>
        public string WorkingFolder
        {
            get { return workingFolder; }
        }

        /// <summary>
        /// This returns the name of the main Sandcastle folder determined
        /// by the build process.
        /// </summary>
        public string SandcastleFolder
        {
            get { return sandcastleFolder; }
        }

        /// <summary>
        /// This returns the name of the HTML Help 1 compiler folder determined
        /// by the build process.
        /// </summary>
        public string Help1CompilerFolder
        {
            get { return hhcFolder; }
        }

        /// <summary>
        /// This returns the name of the MS Help 2 compiler folder determined
        /// by the build process.
        /// </summary>
        public string Help2CompilerFolder
        {
            get { return hxcompFolder; }
        }

        /// <summary>
        /// This returns the name of the main Sandcastle presentation style
        /// folder determined by the build process.
        /// </summary>
        public string PresentationStyleFolder
        {
            get { return presentationFolder; }
        }

        /// <summary>
        /// This read-only property returns a collection of the output folders
        /// specific to each help file format produced by the build.
        /// </summary>
        public Collection<string> HelpFormatOutputFolders
        {
            get { return helpFormatOutputFolders; }
        }

        /// <summary>
        /// This returns the name of the log file used for saving the
        /// build progress messages.
        /// </summary>
        public string LogFilename
        {
            get { return project.LogFileLocation; }
        }

        /// <summary>
        /// This returns the name of the reflection information file
        /// </summary>
        public string ReflectionInfoFilename
        {
            get { return reflectionFile; }
        }

        /// <summary>
        /// This returns the current project being used for the build
        /// </summary>
        /// <remarks>Although there is nothing stopping it, project options
        /// should not be modified during a build.</remarks>
        public SandcastleProject CurrentProject
        {
            get { return project; }
        }

        /// <summary>
        /// This returns the current help file format being generated
        /// </summary>
        /// <remarks>The <b>GenerateHelpFormatTableOfContents</b>,
        /// <b>GenerateHelpFileIndex</b>, <b>GenerateHelpProject</b>,
        /// and <b>CompilingHelpFile</b> steps will run once for each help file
        /// format selected.  This property allows a plug-in to determine which
        /// files it may need to work with during those steps or to skip processing
        /// if it is not relevant.</remarks>
        public HelpFileFormat CurrentFormat
        {
            get { return currentFormat; }
        }

        /// <summary>
        /// This read-only property is used to get the partial build flag
        /// </summary>
        /// <remarks>Partial builds occur when editing the namespace summaries,
        /// editing the API filter, and as part of some plug-ins and may not
        /// require all build options.  In a partial build, build steps after
        /// <b>ApplyVisibilityProperties</b> are not executed.</remarks>
        public bool IsPartialBuild
        {
            get { return isPartialBuild; }
        }

        /// <summary>
        /// This is used to get the conceptual content settings in effect for
        /// the build.
        /// </summary>
        /// <remarks>This will not return a useable value until after the
        /// <b>CopyStandardContent</b> <see cref="BuildStep" />.</remarks>
        public ConceptualContentSettings ConceptualContent
        {
            get { return conceptualContent; }
        }

        /// <summary>
        /// This returns a list of the HTML Help 1 (CHM) files that were built
        /// </summary>
        /// <remarks>If the HTML Help 1 format was not built, this returns an
        /// empty collection.</remarks>
        public Collection<string> Help1Files
        {
            get { return help1Files; }
        }

        /// <summary>
        /// This returns a list of the MS Help 2 (HxS) files that were built
        /// </summary>
        /// <remarks>If the MS Help 2 format was not built, this returns an
        /// empty collection.</remarks>
        public Collection<string> Help2Files
        {
            get { return help2Files; }
        }

        /// <summary>
        /// This returns a list of the MS Help Viewer (MSHC) files that were built
        /// </summary>
        /// <remarks>If the MS Help Viewer format was not built, this returns
        /// an empty collection.</remarks>
        public Collection<string> HelpViewerFiles
        {
            get { return helpViewerFiles; }
        }

        /// <summary>
        /// This returns a list of the website files that were built
        /// </summary>
        /// <remarks>If the website format was not built, this returns an
        /// empty collection.</remarks>
        public Collection<string> WebsiteFiles
        {
            get { return websiteFiles; }
        }

        /// <summary>
        /// This controls whether or not the API filter is suppressed.
        /// </summary>
        /// <value>By default, it is not suppressed and the API filter will be
        /// applied.  The API Filter designer uses this to suppress the filter
        /// so that all members are obtained.</value>
        public bool SuppressApiFilter
        {
            get { return suppressApiFilter; }
            set { suppressApiFilter = value; }
        }

        /// <summary>
        /// This is used to get or set the table of contents parent for the API
        /// content.
        /// </summary>
        /// <remarks>If not set, <see cref="RootContentContainerId" /> is used if
        /// it is set.  If it is not, <see cref="SandcastleProject.TocParentId" /> is
        /// used.  If this property is set, the value should be the ID of a topic in
        /// the project's conceptual content.  The topic must appear in a content layout
        /// file and must have its <c>Visible</c> property set to True in the layout file.</remarks>
        public string ApiTocParentId
        {
            get { return apiTocParentId; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                apiTocParentId = value;
            }
        }

        /// <summary>
        /// This is used to get or set the sort order for API content so that it
        /// appears within its parent in the correct position.
        /// </summary>
        /// <remarks>The default is -1 to let the build engine determine the best
        /// value to use based on the other project properties.</remarks>
        public int ApiTocOrder
        {
            get { return apiTocOrder; }
            set
            {
                if(value < -1)
                    value = -1;

                apiTocOrder = value;
            }
        }

        /// <summary>
        /// This is used to get or set the topic ID to use for the root content
        /// container node.
        /// </summary>
        /// <remarks>If not set, all content will appear at the root level
        /// in the <see cref="SandcastleProject.TocParentId" />.  If set, the
        /// value should be the ID of a topic in the project's conceptual content.
        /// The topic must appear in a content layout file and must have its
        /// <c>Visible</c> property set to False in the layout file.</remarks>
        public string RootContentContainerId
        {
            get { return rootContentContainerId; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                rootContentContainerId = value;
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised to report a change in the build step
        /// </summary>
        public event EventHandler<BuildProgressEventArgs> BuildStepChanged;

        /// <summary>
        /// This raises the <see cref="BuildStepChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBuildStepChanged(BuildProgressEventArgs e)
        {
            var handler = BuildStepChanged;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised to report progress information throughout
        /// each build step.
        /// </summary>
        public event EventHandler<BuildProgressEventArgs> BuildProgress;

        /// <summary>
        /// This raises the <see cref="BuildProgress"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBuildProgress(BuildProgressEventArgs e)
        {
            var handler = BuildProgress;

            if(handler != null)
                handler(this, e);
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProject">The project to build</param>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public BuildProcess(SandcastleProject buildProject)
        {
            // If the project isn't using final values suitable for the build,
            // create a new project that is using final values.
            if(buildProject.UsingFinalValues)
                project = buildProject;
            else
                project = new SandcastleProject(buildProject, true);

            // Save a copy of the project filename.  If using a temporary
            // project, it won't match the passed project's name.
            originalProjectName = buildProject.Filename;

            apiTocOrder = -1;
            apiTocParentId = rootContentContainerId = String.Empty;

            progressArgs = new BuildProgressEventArgs();

            fieldMatchEval = new MatchEvaluator(OnFieldMatch);
            contentMatchEval = new MatchEvaluator(OnContentMatch);
            linkMatchEval = new MatchEvaluator(OnLinkMatch);
            codeBlockMatchEval = new MatchEvaluator(OnCodeBlockMatch);
            excludeElementEval = new MatchEvaluator(OnExcludeElement);

            help1Files = new Collection<string>();
            help2Files = new Collection<string>();
            helpViewerFiles = new Collection<string>();
            websiteFiles = new Collection<string>();
            helpFormatOutputFolders = new Collection<string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProject">The project to build</param>
        /// <param name="partialBuild">Pass true to perform a partial build</param>
        public BuildProcess(SandcastleProject buildProject, bool partialBuild) : this(buildProject)
        {
            isPartialBuild = partialBuild;
        }
        #endregion

        #region Main build method
        //=====================================================================

        /// <summary>
        /// Call this method to perform the build on the project.
        /// </summary>
        /// <event cref="BuildStepChanged">This event fires when the
        /// current build step changes.</event>
        /// <event cref="BuildProgress">This event fires to report progress
        /// information.</event>
        public void Build()
        {
            Project msBuildProject;
            BuildItem buildItem;
            Version v;
            string helpFile, languageFile, scriptFile, message = null;
            int waitCount;

            System.Diagnostics.Debug.WriteLine("Build process starting\r\n");

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                this.ReportProgress(BuildStep.Initializing,
                    "[{0}, version {1}]", fvi.ProductName, fvi.ProductVersion);

                buildStart = stepStart = DateTime.Now;

                msBuildExePath = Path.Combine(Engine.GlobalEngine.Toolsets[
                    project.MSBuildProject.ToolsVersion].ToolsPath, "MSBuild.exe");

                // Base folder for SHFB
                shfbFolder = Path.GetDirectoryName(asm.Location) + @"\";

                // Get the location of the template files
                templateFolder = shfbFolder + @"Templates\";

                // Get the location of the web files
                webFolder = shfbFolder + @"\Web\";

                // Make sure we start out in the project's output folder
                // in case the output folder is relative to it.
                projectFolder = Path.GetDirectoryName(originalProjectName);
                if(projectFolder.Length == 0)
                    projectFolder = Directory.GetCurrentDirectory();

                projectFolder += @"\";

                Directory.SetCurrentDirectory(projectFolder);

                this.ReportProgress("Creating output and working folders...");

                outputFolder = project.OutputPath;
                if(String.IsNullOrEmpty(outputFolder))
                    outputFolder = Directory.GetCurrentDirectory();
                else
                    outputFolder = Path.GetFullPath(outputFolder);

                if(!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                if(outputFolder[outputFolder.Length - 1] != '\\')
                    outputFolder += @"\";

                // Create the log file.  The log may be in a folder other than
                // the output so make sure it exists too.
                if(!Directory.Exists(Path.GetDirectoryName(this.LogFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(this.LogFilename));

                swLog = new StreamWriter(this.LogFilename);
                swLog.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "\r\n<shfbBuild product=\"{0}\" version=\"{1}\" " +
                    "projectFile=\"{2}\" started=\"{3}\">\r\n" +
                    "<buildStep step=\"{4}\">",
                    fvi.ProductName, fvi.ProductVersion, originalProjectName,
                    DateTime.Now, BuildStep.Initializing);

                if(project.WorkingPath.Path.Length == 0)
                    workingFolder = outputFolder + @"Working\";
                else
                    workingFolder = project.WorkingPath;

                if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                    BuildProcess.VerifySafePath("OutputPath", outputFolder, projectFolder);

                // The output folder and the working folder cannot be the same
                if(workingFolder == outputFolder)
                    throw new BuilderException("BE0030", "The OutputPath and " +
                        "WorkingPath properties cannot be set to the same path");

                // For MS Help 2, the HTML Help Name cannot contain spaces
                if((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0 &&
                  project.HtmlHelpName.IndexOf(' ') != -1)
                    throw new BuilderException("BE0031", "For MS Help 2 " +
                        "builds, the HtmlHelpName property cannot contain " +
                        "spaces as they are not valid in the collection name.");

                // Check for the SHFBROOT environment variable.  It may not be
                // present yet if a reboot hasn't occurred after installation.
                // In such cases, set it to the proper folder for this process
                // so that projects can be loaded and built.
                if(Environment.GetEnvironmentVariable("SHFBROOT") == null)
                {
                    // We won't issue a warning since it may not be defined
                    // in some build environments such as on a build server.
                    // In such cases, it is passed in as a command line
                    // option to MSBuild.  Storing it in the environment here
                    // lets the SHFB build projects work as expected.
                    this.ReportProgress("The SHFBROOT system environment variable was " +
                        "not found.  This variable is usually created during installation " +
                        "and may require a reboot.  It has been defined temporarily " +
                        "for this process as: SHFBROOT={0}", shfbFolder);

                    Environment.SetEnvironmentVariable("SHFBROOT", shfbFolder);
                }

                if(project.PlugInConfigurations.Count != 0)
                    this.LoadPlugIns();

                this.ExecutePlugIns(ExecutionBehaviors.After);

                try
                {
                    if(Directory.Exists(workingFolder))
                    {
                        // Clear any data from a prior run
                        this.ReportProgress(BuildStep.ClearWorkFolder, "Clearing working folder...");
                        BuildProcess.VerifySafePath("WorkingPath", workingFolder, projectFolder);

                        if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                        {
                            this.ExecutePlugIns(ExecutionBehaviors.Before);

                            try
                            {
                                Directory.Delete(workingFolder, true);
                            }
                            catch(IOException ioEx)
                            {
                                this.ReportProgress("    Not all prior output was removed from '{0}': {1}",
                                    workingFolder, ioEx.Message);
                            }
                            catch(UnauthorizedAccessException uaEx)
                            {
                                this.ReportProgress("    Not all prior output was removed from '{0}': {1}",
                                    workingFolder, uaEx.Message);
                            }

                            this.ExecutePlugIns(ExecutionBehaviors.After);
                        }
                    }

                    // If the help file is open, it will fail to build so try
                    // to get rid of it now before we get too far into it.
                    helpFile = outputFolder + project.HtmlHelpName + ".chm";

                    if((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0 && File.Exists(helpFile))
                        File.Delete(helpFile);

                    helpFile = Path.ChangeExtension(helpFile, ".hxs");

                    if((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0 && File.Exists(helpFile))
                        File.Delete(helpFile);

                    helpFile = Path.ChangeExtension(helpFile, ".mshc");

                    if((project.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0 && File.Exists(helpFile))
                        File.Delete(helpFile);

                    if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                    {
                        helpFile = outputFolder + "Index.aspx";

                        if(File.Exists(helpFile))
                            File.Delete(helpFile);

                        helpFile = Path.ChangeExtension(helpFile, ".html");

                        if(File.Exists(helpFile))
                            File.Delete(helpFile);
                    }
                }
                catch(IOException ex)
                {
                    throw new BuilderException("BE0025", "Unable to remove prior build output: " + ex.Message);
                }
                catch
                {
                    throw;
                }

                Directory.CreateDirectory(workingFolder);

                // Make sure we can find the tools
                this.FindTools();

                if(!Directory.Exists(sandcastleFolder + @"Data\Reflection"))
                    throw new BuilderException("BE0032", "Reflection data files do not exist yet");

                // Make sure the HelpFileVersion property is in the form of
                // a real version number.
                try
                {
                    if(project.HelpFileVersion.IndexOf('{') == -1)
                        v = new Version(project.HelpFileVersion);
                    else
                        v = new Version(this.TransformText(project.HelpFileVersion));

                    if(v.Build == -1 || v.Revision == -1)
                        throw new FormatException("The version number must specify all four parts.  " +
                            "Specify zero for unused parts.");
                }
                catch(Exception ex)
                {
                    throw new BuilderException("BE0066", "The HelpFileVersion property value '" +
                        project.HelpFileVersion + "' is not in the correct format (#.#.#.#)", ex);
                }

                this.GarbageCollect();

                // Validate the documentation source information, gather
                // assembly and reference info, and copy XML comments files
                // to the working folder.
                this.ValidateDocumentationSources();

                // Transform the shared builder content files
                language = project.Language;
                languageFile = "SharedBuilderContent_" + language.Name + ".xml";

                this.ReportProgress(BuildStep.GenerateSharedContent, "Generating shared content files ({0}, {1})...",
                    language.Name, language.DisplayName);

                // First we need to figure out which style is in effect.
                // Base it on whether the presentation style folder contains
                // "v2005", "hana", or "prototype".
                presentationParam = project.PresentationStyle.ToLower(CultureInfo.InvariantCulture);

                if(presentationParam.IndexOf("vs2005", StringComparison.Ordinal) != -1)
                    presentationParam = "vs2005";
                else
                    if(presentationParam.IndexOf("hana", StringComparison.Ordinal) != -1)
                        presentationParam = "hana";
                    else
                    {
                        if(presentationParam.IndexOf("prototype", StringComparison.Ordinal) == -1)
                            this.ReportWarning("BE0001", "Unable to determine presentation style from folder " +
                                "'{0}'.  Assuming Prototype style.", project.PresentationStyle);

                        presentationParam = "prototype";
                    }

                if(!File.Exists(templateFolder + @"..\SharedContent\" + languageFile))
                {
                    languageFile = "SharedBuilderContent_en-US.xml";

                    // Warn the user about the default being used
                    this.ReportWarning("BE0002", "Shared builder content " +
                        "for the '{0}, {1}' language could not be found.  " +
                        "Using 'en-US, English (US)' defaults.",
                        language.Name, language.DisplayName);
                }

                // See if the user has translated the Sandcastle resources.
                // If not found, default to US English.
                languageFolder = presentationFolder + @"\Content\" + language.Name;

                if(Directory.Exists(languageFolder))
                    languageFolder = language.Name + @"\";
                else
                {
                    // Warn the user about the default being used.  The
                    // language will still be used for the help file though.
                    if(language.Name != "en-US")
                        this.ReportWarning("BE0003", "Sandcastle shared " +
                            "content for the '{0}, {1}' language could not " +
                            "be found.  Using 'en-US, English (US)' " +
                            "defaults.", language.Name, language.DisplayName);

                    languageFolder = String.Empty;
                }

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    this.TransformTemplate(languageFile, templateFolder + @"..\SharedContent\", workingFolder);
                    File.Move(workingFolder + languageFile, workingFolder + "SharedBuilderContent.xml");

                    // Presentation-style specific shared content
                    languageFile = languageFile.Replace("Shared", presentationParam);

                    this.TransformTemplate(languageFile, templateFolder + @"..\SharedContent\", workingFolder);
                    File.Move(workingFolder + languageFile, workingFolder + "PresentationStyleBuilderContent.xml");

                    // Copy the stop word list
                    languageFile = Path.ChangeExtension(languageFile.Replace(presentationParam +
                        "BuilderContent", "StopWordList"), ".txt");
                    File.Copy(templateFolder + @"..\SharedContent\" + languageFile, workingFolder + "StopWordList.txt");
                    File.SetAttributes(workingFolder + "StopWordList.txt", FileAttributes.Normal);

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Generate the API filter used by MRefBuilder
                this.GenerateApiFilter();

                // Generate the reflection information
                this.ReportProgress(BuildStep.GenerateReflectionInfo, "Generating reflection information...");

                reflectionFile = workingFolder + "reflection.org";

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    this.TransformTemplate("MRefBuilder.config", templateFolder, workingFolder);
                    scriptFile = this.TransformTemplate("GenerateRefInfo.proj", templateFolder, workingFolder);

                    msBuildProject = new Project(Engine.GlobalEngine);
                    msBuildProject.Load(scriptFile);

                    // Add the references
                    foreach(BuildItem item in referenceDictionary.Values)
                    {
                        buildItem = msBuildProject.AddNewItem(item.Name, item.Include);
                        item.CopyCustomMetadataTo(buildItem);
                    }

                    // Add the assemblies to document
                    foreach(string assemblyName in assembliesList)
                        buildItem = msBuildProject.AddNewItem("Assembly", assemblyName);

                    msBuildProject.Save(scriptFile);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);
                    this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m GenerateRefInfo.proj");
                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                reflectionInfo = new XmlDocument();
                reflectionInfo.Load(reflectionFile);
                apisNode = reflectionInfo.SelectSingleNode("reflection/apis");

                // Generate namespace summary information
                this.GenerateNamespaceSummaries();

                // Remove unwanted items from the reflection information file
                this.ApplyVisibilityProperties();

                // If there is nothing to document, stop the build
                if(apisNode.ChildNodes.Count == 0)
                    throw new BuilderException("BE0033", "No APIs found to " +
                        "document.  See error topic in help file for details.");

                reflectionInfo = null;
                apisNode = null;

                // If this was a partial build used to obtain API information,
                // stop now.
                if(isPartialBuild)
                {
                    commentsFiles.Save();
                    goto AllDone;       // Yeah, I know it's evil but it's quick
                }

                // Expand <inheritdoc /> tags?
                if(commentsFiles.ContainsInheritedDocumentation)
                {
                    commentsFiles.Save();

                    // Transform the reflection output.
                    this.ReportProgress(BuildStep.GenerateInheritedDocumentation,
                        "Generating inherited documentation...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.TransformTemplate("GenerateInheritedDocs.config", templateFolder, workingFolder);
                        scriptFile = this.TransformTemplate("GenerateInheritedDocs.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m GenerateInheritedDocs.proj");
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // This should always be last so that it overrides comments in the project XML comments files
                    commentsFiles.Add(new XmlCommentsFile(workingFolder + "_InheritedDocs_.xml"));
                }

                this.GarbageCollect();

                // Transform the reflection output.
                this.ReportProgress(BuildStep.TransformReflectionInfo, "Transforming reflection output...");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    scriptFile = this.TransformTemplate("TransformManifest.proj", templateFolder, workingFolder);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);
                    this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m TransformManifest.proj");
                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Load the transformed file
                reflectionFile = reflectionFile.Replace(".org", ".xml");
                reflectionInfo = new XmlDocument();
                reflectionInfo.Load(reflectionFile);
                apisNode = reflectionInfo.SelectSingleNode("reflection/apis");

                // Alter the help topic filenames if necessary
                this.ModifyHelpTopicFilenames();

                // Backup the original reflection file for reference and save the changed file
                File.Copy(reflectionFile, Path.ChangeExtension(reflectionFile, ".bak"), true);

                reflectionInfo.Save(reflectionFile);
                commentsFiles.Save();

                // Copy the standard help file content
                this.CopyStandardHelpContent();

                // Copy conceptual content files if there are topics or tokens.  Tokens can be replaced in
                // XML comments files so we check for them too.
                conceptualContent = new ConceptualContentSettings(project);

                if(conceptualContent.ContentLayoutFiles.Count != 0 || conceptualContent.TokenFiles.Count != 0)
                {
                    this.ReportProgress(BuildStep.CopyConceptualContent, "Copying conceptual content...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        conceptualContent.CopyContentFiles(this);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.ReportProgress(BuildStep.CreateConceptualTopicConfigs,
                        "Creating conceptual topic configuration files...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        conceptualContent.CreateConfigurationFiles(this);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                // Copy the additional content
                this.CopyAdditionalContent();

                // Merge the conceptual and additional content TOC info
                this.MergeConceptualAndAdditionalContentTocInfo();

                // Generate the intermediate table of contents file.  This
                // must occur prior to running BuildAssembler as the MS Help
                // Viewer build component is dependent on the toc.xml file.
                this.ReportProgress(BuildStep.GenerateIntermediateTableOfContents,
                    "Generating intermediate table of contents file...");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    scriptFile = this.TransformTemplate("GenerateIntermediateTOC.proj", templateFolder, workingFolder);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m GenerateIntermediateTOC.proj");

                    // Determine the API content placement
                    this.DetermineApiContentPlacement();

                    // If there is conceptual content, generate the conceptual intermediate TOC
                    if(toc != null)
                    {
                        this.ReportProgress("Generating conceptual content intermediate TOC file...");

                        toc.SaveToIntermediateTocFile((project.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0 ?
                            this.RootContentContainerId : null, project.TocOrder, workingFolder + "_ConceptualTOC_.xml");
                    }

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // The June 2007 CTP removed the root namespace container from the TOC so we'll get
                // the default project page filename from the refelection information file.
                XmlNode defTopic = apisNode.SelectSingleNode("api[@id='R:Project']/file/@name");

                if(defTopic != null)
                {
                    namespacesTopic = defTopic.Value;

                    // Use it as the default topic if one wasn't specified explicitly in the additional content
                    if(defaultTopic == null)
                        defaultTopic = @"html\" + namespacesTopic + ".htm";
                }

                // Create the Sandcastle configuration file
                this.ReportProgress(BuildStep.CreateBuildAssemblerConfigs, "Creating Sandcastle configuration files...");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    this.ReportProgress("    sandcastle.config");

                    // The configuration varies based on the style.  However,
                    // we'll use a common name (sandcastle.config).
                    this.TransformTemplate(presentationParam + ".config", templateFolder, workingFolder);
                    File.Move(workingFolder + presentationParam + ".config", workingFolder + "sandcastle.config");

                    // The conceptual content configuration file is common to
                    // all styles.  It is only created if needed.
                    if(conceptualContent.ContentLayoutFiles.Count != 0)
                    {
                        this.ReportProgress("    conceptual.config");
                        this.TransformTemplate("conceptual.config", templateFolder, workingFolder);
                    }

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Merge the build component custom configurations
                this.MergeComponentConfigurations();

                reflectionInfo = null;
                commentsFiles = null;
                apisNode = null;

                this.GarbageCollect();

                // Build the conceptual help topics
                if(conceptualContent.ContentLayoutFiles.Count != 0)
                {
                    this.ReportProgress(BuildStep.BuildConceptualTopics, "Building conceptual help topics...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = this.TransformTemplate("BuildConceptualTopics.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m BuildConceptualTopics.proj");
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.GarbageCollect();
                }

                // Build the reference help topics
                this.ReportProgress(BuildStep.BuildReferenceTopics, "Building reference help topics...");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    scriptFile = this.TransformTemplate("BuildReferenceTopics.proj", templateFolder, workingFolder);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);
                    this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m BuildReferenceTopics.proj");
                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Combine the conceptual and API intermediate TOC files into one
                this.CombineIntermediateTocFiles();

                // The last part differs based on the help file format
                if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                {
                    this.ReportProgress("\r\nClearing any prior web output");

                    // Purge all files and folders from the output path except for the working folder
                    // and the build log.  This is done first as the CHM and HxS files end up in the
                    // same output folder.  However, the website output is built last so that unnecessary
                    // files are not compiled into the CHM and HxS files.  Read-only and/or hidden files
                    // and folders are ignored as they are assumed to be under source control.
                    foreach(string file in Directory.GetFiles(outputFolder))
                        if(!file.EndsWith(Path.GetFileName(this.LogFilename), StringComparison.Ordinal))
                            if((File.GetAttributes(file) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                                File.Delete(file);
                            else
                                this.ReportProgress("    Ignoring read-only/hidden file {0}", file);

                    foreach(string folder in Directory.GetDirectories(outputFolder))
                        try
                        {
                            // Ignore the working folder
                            if(!folder.EndsWith("Working", StringComparison.Ordinal))
                            {
                                // Some source control providers have a mix of read-only/hidden files within a folder
                                // that isn't read-only/hidden (i.e. Subversion).  In such cases, leave the folder alone.
                                if(Directory.GetFileSystemEntries(folder, "*").Any(f =>
                                  (File.GetAttributes(f) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0))
                                    this.ReportProgress("    Did not delete folder '{0}' as it contains " +
                                        "read-only or hidden folders/files", folder);
                                else
                                    if((File.GetAttributes(folder) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                                        Directory.Delete(folder, true);
                                    else
                                        this.ReportProgress("    Ignoring read-only/hidden folder {0}", folder);
                            }
                        }
                        catch(IOException ioEx)
                        {
                            this.ReportProgress("    Ignoring folder '{0}': {1}", folder, ioEx.Message);
                        }
                        catch(UnauthorizedAccessException uaEx)
                        {
                            this.ReportProgress("    Ignoring folder '{0}': {1}", folder, uaEx.Message);
                        }
                }

                if((project.HelpFileFormat & (HelpFileFormat.HtmlHelp1 | HelpFileFormat.Website)) != 0)
                {
                    this.ReportProgress(BuildStep.ExtractingHtmlInfo,
                        "Extracting HTML info for HTML Help 1 and/or website...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = this.TransformTemplate("ExtractHtmlInfo.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m ExtractHtmlInfo.proj");
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0)
                {
                    // Generate the table of contents and set the default topic
                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents,
                        "Generating HTML Help 1 table of contents file...");

                    currentFormat = HelpFileFormat.HtmlHelp1;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        // It got created in the ExtractingHtmlInfo step above
                        // so there is actually nothing to do here.

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Generate the help file index
                    this.ReportProgress(BuildStep.GenerateHelpFileIndex, "Generating HTML Help 1 index file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        // It got created in the ExtractingHtmlInfo step above
                        // so there is actually nothing to do here.

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Generate the help project file
                    this.ReportProgress(BuildStep.GenerateHelpProject, "Generating HTML Help 1 project file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.TransformTemplate("Help1x.hhp", templateFolder, workingFolder);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Build the HTML Help 1 help file
                    this.ReportProgress(BuildStep.CompilingHelpFile, "Compiling HTML Help 1 file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = this.TransformTemplate("Build1xHelpFile.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m Build1xHelpFile.proj");
                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0)
                {
                    // Generate the table of contents and set the default topic
                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents,
                        "Generating MS Help 2 table of contents file...");

                    currentFormat = HelpFileFormat.MSHelp2;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = this.TransformTemplate("Generate2xTOC.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m Generate2xTOC.proj");
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Generate the help project files
                    this.ReportProgress(BuildStep.GenerateHelpProject, "Generating MS Help 2 project files...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        string[] help2xFiles = Directory.GetFiles(templateFolder, "Help2x*.*");

                        foreach(string projectFile in help2xFiles)
                            this.TransformTemplate(Path.GetFileName(projectFile), templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Build the MS Help 2 help file
                    this.ReportProgress(BuildStep.CompilingHelpFile, "Compiling MS Help 2 file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = this.TransformTemplate("Build2xHelpFile.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m Build2xHelpFile.proj");

                        // Clean up the collection files
                        this.CleanUpCollectionFiles();

                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0)
                {
                    // The following build steps are executed to allow plug-ins to handle any necessary processing
                    // but nothing actually happens here:
                    //
                    //      BuildStep.GenerateHelpFormatTableOfContents
                    //      BuildStep.GenerateHelpProject
                    //
                    // For the MS Help Viewer format, there is no project file to compile and the TOC layout is
                    // generated when the help file is ultimately installed using metadata within each topic file.
                    // All of the necessary TOC info is stored in the intermediate TOC file generated prior to
                    // building the topics.  The BuildAssembler MSHCComponent inserts the TOC info into each topic
                    // as it is built.

                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents,
                        "Executing informational Generate Table of Contents " +
                        "build step for plug-ins (not used for MS Help Viewer)");

                    currentFormat = HelpFileFormat.MSHelpViewer;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.ReportProgress(BuildStep.GenerateHelpProject,
                        "Executing informational Generate Help Project " +
                        "build step for plug-ins (not used for MS Help Viewer)");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Build the MS Help Viewer help file
                    this.ReportProgress(BuildStep.CompilingHelpFile, "Generating MS Help Viewer file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.TransformTemplate("HelpContentSetup.msha", templateFolder, workingFolder);

                        // Rename the content setup file to use the help filename to keep them related and
                        // so that multiple output files can be sent to the same output folder.
                        File.Move(workingFolder + "HelpContentSetup.msha", workingFolder + project.HtmlHelpName + ".msha");

                        // Generate the example install and remove scripts
                        this.TransformTemplate("InstallMSHC.bat", templateFolder, workingFolder);
                        File.Move(workingFolder + "InstallMSHC.bat", workingFolder + "Install_" + project.HtmlHelpName + ".bat");

                        this.TransformTemplate("RemoveMSHC.bat", templateFolder, workingFolder);
                        File.Move(workingFolder + "RemoveMSHC.bat", workingFolder + "Remove_" + project.HtmlHelpName + ".bat");

                        // Copy the launcher utility
                        File.Copy(shfbFolder + "HelpLibraryManagerLauncher.exe", workingFolder + "HelpLibraryManagerLauncher.exe");
                        File.SetAttributes(workingFolder + "HelpLibraryManagerLauncher.exe", FileAttributes.Normal);

                        scriptFile = this.TransformTemplate("BuildHelpViewerFile.proj", templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.RunProcess(msBuildExePath, "/nologo /clp:NoSummary /v:m BuildHelpViewerFile.proj");

                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                {
                    // Generate the table of contents and set the default topic
                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents,
                        "Generating website table of contents file...");

                    currentFormat = HelpFileFormat.Website;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        // It got created in the ExtractingHtmlInfo step above
                        // so there is actually nothing to do here.

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.GenerateWebsite();
                }

                // All done
                if(project.CleanIntermediates)
                {
                    this.ReportProgress(BuildStep.CleanIntermediates, "Removing intermediate files...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        try
                        {
                            Directory.Delete(workingFolder, true);
                        }
                        catch(IOException ioEx)
                        {
                            this.ReportProgress("    Not all build output was removed from '{0}': {1}",
                                workingFolder, ioEx.Message);
                        }
                        catch(UnauthorizedAccessException uaEx)
                        {
                            this.ReportProgress("    Not all build output was removed from '{0}': {1}",
                                workingFolder, uaEx.Message);
                        }

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }
AllDone:
                progressArgs.HasCompleted = true;

                TimeSpan runtime = DateTime.Now - buildStart;

                this.ReportProgress(BuildStep.Completed,
                    "\r\nBuild completed successfully at {0:MM/dd/yyyy hh:mm tt}.  " +
                    "Total time: {1:00}:{2:00}:{3:00.0000}\r\n", DateTime.Now, Math.Floor(runtime.TotalSeconds / 3600),
                    Math.Floor((runtime.TotalSeconds % 3600) / 60), (runtime.TotalSeconds % 60));

                System.Diagnostics.Debug.WriteLine("Build process finished successfully\r\n");
            }
            catch(ThreadAbortException )
            {
                // Kill off the process, known child processes and the STDOUT
                // and STDERR threads too if necessary.
                if(currentProcess != null && !currentProcess.HasExited)
                {
                    currentProcess.Kill();

                    foreach(Process p in Process.GetProcesses())
                        if(reKillProcess.IsMatch(p.ProcessName))
                        {
                            System.Diagnostics.Debug.WriteLine("Killing " + p.ProcessName);
                            p.Kill();
                        }
                }

                if(stdOutThread != null && stdOutThread.IsAlive)
                {
                    stdOutThread.Abort();
                    waitCount = 0;

                    while(waitCount < 5 && !stdOutThread.Join(1000))
                        waitCount++;
                }

                if(stdErrThread != null && stdErrThread.IsAlive)
                {
                    stdErrThread.Abort();
                    waitCount = 0;

                    while(waitCount < 5 && !stdErrThread.Join(1000))
                        waitCount++;
                }

                progressArgs.HasCompleted = true;
                this.ReportError(BuildStep.Canceled, "BE0064", "BUILD CANCELLED BY USER");
                System.Diagnostics.Debug.WriteLine("Build process aborted\r\n");
            }
            catch(Exception ex)
            {
                BuilderException bex = ex as BuilderException;
                System.Diagnostics.Debug.WriteLine(ex);
                progressArgs.HasCompleted = true;

                do
                {
                    if(message != null)
                        message += "\r\n";

                    message += ex.Message;
                    ex = ex.InnerException;

                } while(ex != null);

                // NOTE: Message may contain format markers so pass it as a
                // format argument.
                if(bex != null)
                    this.ReportError(BuildStep.Failed, bex.ErrorCode, "{0}", message);
                else
                    this.ReportError(BuildStep.Failed, "BE0065", "BUILD FAILED: {0}", message);

                System.Diagnostics.Debug.WriteLine("Build process failed\r\n");
            }
            finally
            {
                if(currentProcess != null)
                    currentProcess.Dispose();

                try
                {
                    this.ExecutePlugIns(ExecutionBehaviors.Before);
                }
                catch(Exception ex)
                {
                    // Not much we can do at this point...
                    this.ReportProgress(ex.ToString());
                }

                try
                {
                    this.ExecutePlugIns(ExecutionBehaviors.After);

                    if(loadedPlugIns != null)
                        foreach(IPlugIn plugIn in loadedPlugIns.Values)
                            plugIn.Dispose();
                }
                catch(Exception ex)
                {
                    // Not much we can do at this point...
                    this.ReportProgress(ex.ToString());
                }
                finally
                {
                    this.GarbageCollect();

                    if(swLog != null)
                    {
                        swLog.WriteLine("</buildStep>\r\n</shfbBuild>");
                        swLog.Close();
                        swLog = null;
                    }

                    if(progressArgs.BuildStep == BuildStep.Completed && !project.KeepLogFile)
                        File.Delete(this.LogFilename);
                }
            }
        }
        #endregion

        #region Progress reporting methods
        //=====================================================================

        /// <summary>
        /// This is used to report progress during the build process
        /// within the current step.
        /// </summary>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message
        /// text</param>
        /// <overloads>This method has two overloads.</overloads>
        public void ReportProgress(string message, params object[] args)
        {
            this.ReportProgress(progressArgs.BuildStep, message, args);
        }

        /// <summary>
        /// This is used to report an error that will abort the build
        /// </summary>
        /// <param name="step">The current build step</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message
        /// text</param>
        /// <remarks>This just reports the error.  The caller must abort the
        /// build.</remarks>
        private void ReportError(BuildStep step, string errorCode,
          string message, params object[] args)
        {
            string errorMessage = String.Format(CultureInfo.CurrentCulture,
                message, args);

            this.ReportProgress(step, "\r\nSHFB: Error {0}: {1}\r\n",
                errorCode, errorMessage);
        }

        /// <summary>
        /// This is used to report a warning that may need attention
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message
        /// text</param>
        public void ReportWarning(string warningCode, string message,
          params object[] args)
        {
            string warningMessage = String.Format(CultureInfo.CurrentCulture,
                message, args);

            this.ReportProgress(progressArgs.BuildStep,
                "SHFB: Warning {0}: {1}", warningCode, warningMessage);
        }

        /// <summary>
        /// This is used to report progress during the build process
        /// and possibly update the current step.
        /// </summary>
        /// <param name="step">The current build step</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the
        /// message text</param>
        /// <event cref="BuildStepChanged">This event fires when the
        /// current build step changes.</event>
        /// <event cref="BuildProgress">This event fires to report progress
        /// information.</event>
        protected void ReportProgress(BuildStep step, string message,
          params object[] args)
        {
            TimeSpan runtime;

            bool stepChanged = (progressArgs.BuildStep != step);

            if(stepChanged)
            {
                // Don't bother for the initialization steps
                if(step > BuildStep.GenerateSharedContent)
                {
                    runtime = DateTime.Now - stepStart;
                    progressArgs.Message = String.Format(
                        CultureInfo.InvariantCulture, "    Last step " +
                        "completed in {0:00}:{1:00}:{2:00.0000}",
                        Math.Floor(runtime.TotalSeconds / 3600),
                        Math.Floor((runtime.TotalSeconds % 3600) / 60),
                        (runtime.TotalSeconds % 60));

                    if(swLog != null)
                        swLog.WriteLine(progressArgs.Message);

                    this.OnBuildProgress(progressArgs);
                }

                progressArgs.Message = "-------------------------------";
                this.OnBuildProgress(progressArgs);

                stepStart = DateTime.Now;
                progressArgs.BuildStep = step;

                if(swLog != null)
                    swLog.WriteLine("</buildStep>\r\n<buildStep step=\"{0}\">",
                        step);
            }

            progressArgs.Message = String.Format(CultureInfo.CurrentCulture,
                message, args);

            // Save the message to the log file
            if(swLog != null)
                swLog.WriteLine(HttpUtility.HtmlEncode(progressArgs.Message));

            // Report progress first and then the step change so that any
            // final information gets saved to the log file.
            this.OnBuildProgress(progressArgs);

            if(stepChanged)
                OnBuildStepChanged(progressArgs);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Force garbage collection to reduce memory usage.
        /// </summary>
        /// <remarks>The reflection information file and XML comments files
        /// can be quite large.  To reduce memory usage, we force a garbage
        /// collection to get rid of all the discarded objects.</remarks>
        private void GarbageCollect()
        {
#if DEBUG
            this.ReportProgress("\r\n  GC: Memory used before: {0:0,0}",
                GC.GetTotalMemory(false));
#endif
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
#if DEBUG
            this.ReportProgress("  GC: Memory used after: {0:0,0}\r\n",
                GC.GetTotalMemory(false));
#endif
        }

        /// <summary>
        /// Make sure the path isn't one the user would regret having nuked
        /// without warning.
        /// </summary>
        /// <param name="propertyName">The name of the path property</param>
        /// <param name="propertyValue">It's current value</param>
        /// <param name="projectPath">The path to the current project</param>
        /// <remarks>Since most people don't read the help file and also ignore
        /// the warning in the property grid description pane, we'll take some
        /// steps to idiot-proof the dangerous path properties.  I'm starting
        /// to lose count of the number of people that point WorkingPath at the
        /// root of their C:\ drive and wonder why all their files disappear.
        /// <p/>Paths checked for include root references to hard drives and
        /// network shares, most common well-known folders, and the project's
        /// root folder.</remarks>
        /// <exception cref="BuilderException">This is thrown if the path is
        /// one of the ones that probably should not be used.</exception>
        public static void VerifySafePath(string propertyName,
          string propertyValue, string projectPath)
        {
            List<string> specialPaths = new List<string>();
            string tempPath = Path.GetFullPath(propertyValue);
            bool isBadPath = false;
            int pos;

            if(tempPath.Length != 0 && tempPath[tempPath.Length - 1] == '\\')
                tempPath = tempPath.Substring(0, tempPath.Length - 1);

            if(tempPath.Length == 2 && tempPath[1] == ':')
                isBadPath = true;

            if(tempPath.Length > 2)
            {
                // While the path can be under the project path, it shouldn't
                // match the project path or be its parent.
                if(FolderPath.TerminatePath(projectPath).StartsWith(
                  FolderPath.TerminatePath(tempPath), StringComparison.OrdinalIgnoreCase))
                    isBadPath = true;

                if(tempPath[0] == '\\' && tempPath[1] == '\\')
                {
                    // UNC path.  Make sure it has more than just a share
                    // after the server name.
                    tempPath = tempPath.Substring(2);

                    pos = tempPath.IndexOf('\\');

                    if(pos != -1)
                        pos = tempPath.IndexOf('\\', pos + 1);

                    // This isn't perfect as the actual root of the share
                    // may be several folders down.  You can't have it all.
                    if(pos == -1)
                        isBadPath = true;
                }
                else
                {
                    // Fixed drive.  Make sure it isn't one of the well-known
                    // folders.  Some of these contain the same folders but
                    // we'll err on the side of caution and check them anyway.
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonApplicationData));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonProgramFiles));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.Desktop));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.DesktopDirectory));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.MyComputer));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.ProgramFiles));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.Programs));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.System));

                    // Don't allow the folder, a subfolder, or parent
                    foreach(string path in specialPaths)
                        if(!String.IsNullOrEmpty(path) &&
                          (path.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase) ||
                          tempPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                        {
                            isBadPath = true;
                            break;
                        }

                    // We'll allow subfolders under My Documents, Personal,
                    // Application Data, and Local Application Data folders as
                    // that's a common occurrence.  Again, not perfect but...
                    specialPaths.Clear();
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.Personal));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData));
                    specialPaths.Add(Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData));

                    foreach(string path in specialPaths)
                        if(!String.IsNullOrEmpty(path) &&
                          path.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase))
                        {
                            isBadPath = true;
                            break;
                        }
                }
            }

            if(isBadPath)
                throw new BuilderException("BE0034", String.Format(
                    CultureInfo.InvariantCulture, "The '{0}' property " +
                    "resolved to '{1}' which is a reserved folder name.  " +
                    "See error or property topic in help file for details.",
                    propertyName, propertyValue));
        }

        /// <summary>
        /// This is used to gather a list of files produced by the build
        /// </summary>
        private void GatherBuildOutputFilenames()
        {
            string[] files;
            string[] patterns = new string[4];

            switch(currentFormat)
            {
                case HelpFileFormat.HtmlHelp1:
                    patterns[0] = project.HtmlHelpName + "*.chm";
                    break;

                case HelpFileFormat.MSHelp2:
                    patterns[0] = project.HtmlHelpName + "*.Hx?";
                    patterns[1] = project.HtmlHelpName + "*.ini";
                    break;

                case HelpFileFormat.MSHelpViewer:
                    patterns[0] = project.HtmlHelpName + "*.msh?";
                    patterns[1] = project.HtmlHelpName + "Install_*.bat";
                    patterns[2] = project.HtmlHelpName + "Remove_*.bat";
                    patterns[3] = project.HtmlHelpName + "HelpLibraryManagerLauncher.exe";
                    break;

                default:    // Website
                    patterns[0] = "*.*";
                    break;
            }

            foreach(string filePattern in patterns)
            {
                if(filePattern == null)
                    continue;

                files = Directory.GetFiles(outputFolder, filePattern, SearchOption.AllDirectories);

                foreach(string file in files)
                {
                    if(file.StartsWith(workingFolder, StringComparison.OrdinalIgnoreCase) ||
                      file == project.LogFileLocation)
                        continue;

                    switch(currentFormat)
                    {
                        case HelpFileFormat.HtmlHelp1:
                            help1Files.Add(file);
                            break;

                        case HelpFileFormat.MSHelp2:
                            help2Files.Add(file);
                            break;

                        case HelpFileFormat.MSHelpViewer:
                            helpViewerFiles.Add(file);
                            break;

                        default:    // Website
                            if(!help1Files.Contains(file) && !help2Files.Contains(file) &&
                              !helpViewerFiles.Contains(file))
                                websiteFiles.Add(file);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Tool location methods
        //=====================================================================

        /// <summary>
        /// Find the Sandcastle tools and the HTML help compiler
        /// </summary>
        /// <exception cref="BuilderException">This is thrown if any of
        /// the tools cannot be found.</exception>
        protected void FindTools()
        {
            this.ReportProgress(BuildStep.FindingTools, "Finding tools...");
            this.ExecutePlugIns(ExecutionBehaviors.Before);

            sandcastleFolder = project.SandcastlePath;

            // Try to find it based on the DXROOT environment variable
            if(sandcastleFolder.Length == 0)
            {
                sandcastleFolder = Environment.GetEnvironmentVariable("DXROOT");
                if(String.IsNullOrEmpty(sandcastleFolder) || !sandcastleFolder.Contains(@"\Sandcastle"))
                    sandcastleFolder = String.Empty;
            }

            // Try to find Sandcastle based on the path if not specified
            // in the project or DXROOT.
            if(sandcastleFolder.Length == 0)
            {
                Match m = Regex.Match(Environment.GetEnvironmentVariable("PATH"),
                    @"[A-Z]:\\.[^;]+\\Sandcastle(?=\\Prod)", RegexOptions.IgnoreCase);

                // If not found in the path, search all fixed drives
                if(m.Success)
                    sandcastleFolder = m.Value;
                else
                {
                    this.ReportProgress(BuildStep.FindingTools, "Searching for Sandcastle tools...");
                    sandcastleFolder = BuildProcess.FindOnFixedDrives(@"\Sandcastle");

                    // If not found there, try the VS 2005 SDK folders
                    if(sandcastleFolder.Length == 0)
                    {
                        sandcastleFolder = BuildProcess.FindSdkExecutable("MRefBuilder.exe");

                        if(sandcastleFolder.Length != 0)
                            sandcastleFolder = sandcastleFolder.Substring(0, sandcastleFolder.LastIndexOf('\\'));
                    }
                }
            }
            else
                sandcastleFolder = Path.GetFullPath(sandcastleFolder);

            if(sandcastleFolder.Length != 0 && sandcastleFolder[sandcastleFolder.Length - 1] != '\\')
                sandcastleFolder += @"\";

            if(sandcastleFolder.Length == 0 || !Directory.Exists(sandcastleFolder) ||
              !File.Exists(sandcastleFolder + @"ProductionTools\MRefBuilder.exe"))
                throw new BuilderException("BE0035", "Could not find the path to the Microsoft Sandcastle " +
                    "documentation compiler tools.  See the error number topic in the help file for details.\r\n");

            this.ReportProgress("Found Sandcastle tools in '{0}'", sandcastleFolder);

            // Set the presentation folder too
            presentationFolder = String.Format(CultureInfo.InvariantCulture,
                @"{0}Presentation\{1}\", sandcastleFolder, project.PresentationStyle);

            // Make sure we've got a version we can use
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(
                sandcastleFolder + @"ProductionTools\MRefBuilder.exe");

            Version fileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart,
                fvi.FileBuildPart, fvi.FilePrivatePart);

            Version expectedVersion = new Version("2.6.10621.1");

            if(fileVersion < expectedVersion)
                throw new BuilderException("BE0036", String.Format(CultureInfo.InvariantCulture,
                    "Your version of the Microsoft Sandcastle documentation compiler tools is out of " +
                    "date (found '{0}' but expected '{1}').  See the error number topic in the help " +
                    "file for details.\r\n", fileVersion, expectedVersion));

            // If the version is greater, we can't use it as the build components are bound to
            // the older version and it will fail later on in the BuildAssembler step.
            if(fileVersion > expectedVersion)
            {
                // I tend to forget, so this is a clue to me that I need to update
                // the version number used above.
                if(System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                throw new BuilderException("BE0004", String.Format(CultureInfo.InvariantCulture,
                    "MRefBuilder has a version of '{0}' but version '{1}' was expected.  You need to " +
                    "update your copy of the help file builder.\r\nSee the error number topic in the help " +
                    "file for details.\r\n", fileVersion, expectedVersion));
            }

            // Find the help compilers by looking on all fixed drives.
            // We don't need them if the result is only a website.
            if((project.HelpFileFormat & ~HelpFileFormat.Website) != 0)
            {
                if((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0)
                {
                    hhcFolder = project.HtmlHelp1xCompilerPath;

                    if(hhcFolder.Length == 0)
                    {
                        this.ReportProgress(BuildStep.FindingTools, "Searching for HTML Help 1 compiler...");
                        hhcFolder = BuildProcess.FindOnFixedDrives(@"\HTML Help Workshop");
                    }

                    if(hhcFolder.Length == 0 || !Directory.Exists(hhcFolder))
                        throw new BuilderException("BE0037", "Could not find the path the the HTML Help 1 " +
                            "compiler. See the error number topic in the help file for details.\r\n");

                    if(hhcFolder[hhcFolder.Length - 1] != '\\')
                        hhcFolder += @"\";

                    this.ReportProgress("Found HTML Help 1 compiler in '{0}'",
                        hhcFolder);
                }

                if((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0)
                {
                    hxcompFolder = project.HtmlHelp2xCompilerPath;

                    if(hxcompFolder.Length == 0)
                    {
                        this.ReportProgress(BuildStep.FindingTools, "Searching for HTML Help 2 compiler...");

                        // Search the Visual Studio SDK folders first as it
                        // usually has a more recent version.
                        hxcompFolder = BuildProcess.FindSdkExecutable("hxcomp.exe");

                        // If not found there, try the default installation
                        // folders.
                        if(hxcompFolder.Length == 0)
                        {
                            hxcompFolder = BuildProcess.FindOnFixedDrives(
                                @"\Common Files\Microsoft Shared\Help 2.0 Compiler");

                            if(hxcompFolder.Length == 0)
                                hxcompFolder = BuildProcess.FindOnFixedDrives(@"\Microsoft Help 2.0 SDK");
                        }
                    }

                    if(hxcompFolder.Length == 0 ||
                      !Directory.Exists(hxcompFolder))
                        throw new BuilderException("BE0038", "Could not find the path to the MS Help 2 " +
                            "compiler.  See error topic in help file for details.\r\n");

                    if(hxcompFolder[hxcompFolder.Length - 1] != '\\')
                        hxcompFolder += @"\";

                    this.ReportProgress("Found MS Help 2 help compiler in '{0}'", hxcompFolder);
                }
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// Find a folder by searching the Program Files folders on all fixed
        /// drives.
        /// </summary>
        /// <param name="path">The path for which to search</param>
        /// <returns>The path if found or an empty string if not found</returns>
        protected internal static string FindOnFixedDrives(string path)
        {
            StringBuilder sb = new StringBuilder(Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFiles));

            // Check for 64-bit Windows.  The tools will be in the x86 folder.
            if(Directory.Exists(sb.ToString() + " (x86)"))
                sb.Append(" (x86)");

            sb.Append(path);

            foreach(DriveInfo di in DriveInfo.GetDrives())
                if(di.DriveType == DriveType.Fixed)
                {
                    sb[0] = di.Name[0];
                    if(Directory.Exists(sb.ToString()))
                        return sb.ToString();
                }

            return String.Empty;
        }

        /// <summary>
        /// This is used to find the named executable in one of the Visual
        /// Studio SDK installation folders.
        /// </summary>
        /// <param name="exeName">The name of the executable to find</param>
        /// <returns>The path if found or an empty string if not found</returns>
        /// <remarks>The search looks in all "Visual*" folders under the
        /// Program Files special folder on all fixed drives.</remarks>
        protected internal static string FindSdkExecutable(string exeName)
        {
            StringBuilder sb = new StringBuilder(Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFiles));
            string[] dirs, files;
            string folder;

            // Check for 64-bit Windows.  The tools will be in the x86 folder.
            if(Directory.Exists(sb.ToString() + " (x86)"))
                sb.Append(" (x86)");

            foreach(DriveInfo di in DriveInfo.GetDrives())
                if(di.DriveType == DriveType.Fixed)
                {
                    sb[0] = di.Name[0];
                    folder = sb.ToString();

                    if(!Directory.Exists(folder))
                        continue;

                    dirs = Directory.GetDirectories(folder, "Visual*");

                    foreach(string dir in dirs)
                    {
                        files = Directory.GetFiles(dir, exeName,
                            SearchOption.AllDirectories);

                        if(files.Length != 0)
                        {
                            // If more than one, sort them and take the
                            // last one as it should be the most recent.
                            if(files.Length > 1)
                                Array.Sort(files);

                            return Path.GetDirectoryName(files[files.Length - 1]);
                        }
                    }
                }

            return String.Empty;
        }
        #endregion

        #region Validate documentation sources
        //=====================================================================

        /// <summary>
        /// Validate the documentation source information and copy the files to
        /// the working folder.
        /// </summary>
        /// <exception cref="BuilderException">This is thrown if any of
        /// the information is invalid.</exception>
        private void ValidateDocumentationSources()
        {
            List<string> commentsList = new List<string>();
            Dictionary<string, MSBuildProject> projectDictionary = new Dictionary<string, MSBuildProject>();

            MSBuildProject projRef;
            BuildItem buildItem;
            XPathDocument testComments;
            XPathNavigator navComments;
            XmlCommentsFile comments;
            int fileCount;
            string workingPath, projFramework, hintPath, lastSolution = null,
                targetFramework = project.FrameworkVersion;

            this.ReportProgress(BuildStep.ValidatingDocumentationSources,
                "Validating and copying documentation source information");

            assembliesList = new Collection<string>();
            referenceDictionary = new Dictionary<string, BuildItem>();
            commentsFiles = new XmlCommentsFileCollection();

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            // It's possible a plug-in might want to add or remove assemblies
            // so we'll run them before checking to see if the project has any.
            this.ExecutePlugIns(ExecutionBehaviors.Before);

            if(project.DocumentationSources.Count == 0)
                throw new BuilderException("BE0039", "The project does " +
                    "not have any documentation sources defined");

            // Clone the project's references
            foreach(string refType in (new string[] { "Reference", "COMReference" }))
                foreach(BuildItem reference in project.MSBuildProject.GetEvaluatedItemsByName(refType))
                {
                    buildItem = reference.Clone();

                    // Make sure hint paths are correct by adding the project folder to any relative
                    // paths.  Skip any containing MSBuild variable references.
                    if(buildItem.HasMetadata(ProjectElement.HintPath))
                    {
                        hintPath = buildItem.GetMetadata(ProjectElement.HintPath);

                        if(!Path.IsPathRooted(hintPath) &&
                          hintPath.IndexOf("$(", StringComparison.Ordinal) == -1)
                            buildItem.SetMetadata(ProjectElement.HintPath, Path.Combine(projectFolder,
                                hintPath));
                    }

                    referenceDictionary.Add(reference.Include, buildItem);
                }

            // Convert project references to regular references that point to the output assembly.
            // Project references get built and we may not have enough info for that to happen
            // successfully.  As such, we'll assume the project has already been built and that its
            // target exists.
            foreach(BuildItem reference in project.MSBuildProject.GetEvaluatedItemsByName("ProjectReference"))
            {
                projRef = new MSBuildProject(reference.Include);
                projRef.SetConfiguration(project.Configuration, project.Platform, project.MSBuildOutDir);

                buildItem = projRef.ProjectFile.AddNewItem("Reference",
                    Path.GetFileNameWithoutExtension(projRef.AssemblyName));
                buildItem.SetMetadata("HintPath", projRef.AssemblyName);
                referenceDictionary.Add(buildItem.Include, buildItem);
            }

            // For each source, make three passes: one for projects, one for assemblies and one
            // for comments files.  Projects and comments files are optional but when all done,
            // at least one assembly must have been found.
            foreach(DocumentationSource ds in project.DocumentationSources)
            {
                fileCount = 0;

                this.ReportProgress("Source: {0}", ds.SourceFile);

                foreach(var sourceProject in DocumentationSource.Projects(ds.SourceFile, ds.IncludeSubFolders,
                  !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : project.Configuration,
                  !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : project.Platform))
                {
                    // NOTE: This code in EntityReferenceWindow.IndexComments should be similar to this!

                    // Solutions are followed by the projects that they contain
                    if(sourceProject.ProjectFileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                    {
                        lastSolution = sourceProject.ProjectFileName;
                        continue;
                    }

                    if(!projectDictionary.ContainsKey(sourceProject.ProjectFileName))
                    {
                        // These are handled below
                        this.ReportProgress("    Found project '{0}'", sourceProject.ProjectFileName);

                        projRef = new MSBuildProject(sourceProject.ProjectFileName);

                        // Use the project file configuration and platform properties if they are set.  If not,
                        // use the documentation source values.  If they are not set, use the SHFB project settings.
                        projRef.SetConfiguration(
                            !String.IsNullOrEmpty(sourceProject.Configuration) ? sourceProject.Configuration :
                                !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : project.Configuration,
                            !String.IsNullOrEmpty(sourceProject.Platform) ? sourceProject.Platform :
                                !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : project.Platform,
                            project.MSBuildOutDir);

                        // Add Visual Studio solution macros if necessary
                        if(lastSolution != null)
                            projRef.SetSolutionMacros(lastSolution);

                        projectDictionary.Add(sourceProject.ProjectFileName, projRef);
                    }
                    else
                        this.ReportProgress("    Ignoring duplicate project file '{0}'", sourceProject.ProjectFileName);

                    fileCount++;
                }

                foreach(string asmName in DocumentationSource.Assemblies(ds.SourceFile, ds.IncludeSubFolders))
                {
                    if(!assembliesList.Contains(asmName))
                    {
                        // Assemblies are parsed in place by MRefBuilder so we
                        // don't have to do anything with them here.
                        this.ReportProgress("    Found assembly '{0}'", asmName);
                        assembliesList.Add(asmName);
                    }
                    else
                        this.ReportProgress("    Ignoring duplicate assembly file '{0}'", asmName);

                    fileCount++;
                }

                foreach(string commentsName in DocumentationSource.CommentsFiles(
                  ds.SourceFile, ds.IncludeSubFolders))
                {
                    if(!commentsList.Contains(commentsName))
                    {
                        // These are handled below
                        commentsList.Add(commentsName);
                    }
                    else
                        this.ReportProgress("    Ignoring duplicate comments file '{0}'", commentsName);

                    fileCount++;
                }

                if(fileCount == 0)
                    this.ReportWarning("BE0006", "Unable to location any " +
                        "documentation sources for '{0}'", ds.SourceFile);
            }

            // Parse projects for assembly, comments, and reference info
            if(projectDictionary.Count != 0)
            {
                this.ReportProgress("\r\nParsing project files");

                foreach(MSBuildProject msbProject in projectDictionary.Values)
                {
                    workingPath = msbProject.AssemblyName;

                    if(!String.IsNullOrEmpty(workingPath))
                    {
                        if(!File.Exists(workingPath))
                            throw new BuilderException("BE0040", "Project assembly does not exist: " + workingPath);

                        this.ReportProgress("    Found assembly '{0}'", workingPath);
                        assembliesList.Add(workingPath);
                    }
                    else
                        throw new BuilderException("BE0067", String.Format(CultureInfo.InvariantCulture,
                            "Unable to obtain assembly name from project file '{0}' using Configuration " +
                            "'{1}', Platform '{2}'", msbProject.ProjectFile.FullFileName,
                            msbProject.ProjectFile.GetEvaluatedProperty(ProjectElement.Configuration),
                            msbProject.ProjectFile.GetEvaluatedProperty(ProjectElement.Platform)));

                    workingPath = msbProject.XmlCommentsFile;

                    if(!String.IsNullOrEmpty(workingPath))
                    {
                        if(!File.Exists(workingPath))
                            throw new BuilderException("BE0041",
                                "Project XML comments file does not exist: " + workingPath);

                        commentsList.Add(workingPath);
                    }

                    // Note the highest framework version used
                    projFramework = msbProject.TargetFrameworkVersion;

                    if(!String.IsNullOrEmpty(projFramework))
                    {
                        projFramework = FrameworkVersionTypeConverter.LatestMatching(projFramework.Substring(1));

                        if(String.Compare(targetFramework, projFramework, StringComparison.OrdinalIgnoreCase) < 0)
                            targetFramework = projFramework;
                    }

                    // Clone the project's references
                    msbProject.CloneReferences(referenceDictionary);
                }
            }

            if(project.FrameworkVersion != targetFramework)
            {
                this.ReportWarning("BE0007", "A project with a higher " +
                    "framework version was found.  Changing project " +
                    "FrameworkVersion property from '{0}' to '{1}' for " +
                    "the build", project.FrameworkVersion, targetFramework);

                project.FrameworkVersion = targetFramework;
            }

            if(assembliesList.Count == 0)
                throw new BuilderException("BE0042", "You must specify at " +
                    "least one documentation source in the form of an " +
                    "assembly or a Visual Studio solution/project file");

            // Log the references found, if any
            if(referenceDictionary.Count != 0)
            {
                this.ReportProgress("\r\nReferences to use:");

                string[] keys = new string[referenceDictionary.Keys.Count];
                referenceDictionary.Keys.CopyTo(keys, 0);
                Array.Sort(keys);

                foreach(string key in keys)
                    this.ReportProgress("    {0}", key);
            }

            if(commentsList.Count != 0)
                this.ReportProgress("\r\nCopying XML comments files");

            // XML comments files are copied to the working folder in case they
            // need to be fixed up.
            foreach(string commentsName in commentsList)
            {
                workingPath = workingFolder + Path.GetFileName(commentsName);

                // Warn if there is a duplicate and copy the comments file
                // to a unique name to preserve its content.
                if(File.Exists(workingPath))
                {
                    workingPath = workingFolder + Guid.NewGuid().ToString("B");

                    this.ReportWarning("BE0063", "'{0}' matches a " +
                        "previously copied comments filename.  The " +
                        "duplicate will be copied to a unique name to " +
                        "preserve the comments it contains.", commentsName);
                }

                try
                {
                    // Not all XML files found may be comments files.  Ignore
                    // those that are not.
                    testComments = new XPathDocument(commentsName);
                    navComments = testComments.CreateNavigator();

                    if(navComments.SelectSingleNode("doc/members") == null)
                    {
                        this.ReportWarning("BE0005", "File '{0}' does not " +
                            "contain a 'doc/members' node and will not be " +
                            "used as an XML comments file.",
                            commentsName);
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    this.ReportWarning("BE0061", "File '{0}' could not be " +
                        "loaded and will not be used as an XML comments file." +
                        "  Error: {1}", commentsName, ex.Message);
                    continue;
                }

                File.Copy(commentsName, workingPath, true);
                File.SetAttributes(workingPath, FileAttributes.Normal);

                // Add the file to the XML comments file collection
                comments = new XmlCommentsFile(workingPath);

                // Fixup comments for CPP comments files?
                if(project.CppCommentsFixup)
                    comments.FixupComments();

                commentsFiles.Add(comments);

                this.ReportProgress("    {0} -> {1}", commentsName,
                    workingPath);
            }

            if(commentsFiles.Count == 0)
                this.ReportWarning("BE0062", "No XML comments files found.  " +
                    "The help file will not contain any member comments.");

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }
        #endregion

        #region Run an external tool or process
        //=====================================================================

        /// <summary>
        /// This is used to run a step in the build process
        /// </summary>
        /// <param name="fileToRun">The file to execute.  This will be one of
        /// the template batch files with all the necessary values for the
        /// paths and options plugged into it.</param>
        /// <param name="args">The arguments to pass to the file if any.</param>
        public void RunProcess(string fileToRun, string args)
        {
            int waitCount;

            if(fileToRun == null)
                throw new ArgumentNullException("fileToRun");

            currentProcess = new Process();
            errorDetected = false;

            ProcessStartInfo psi = currentProcess.StartInfo;

            // Set CreateNoWindow to true to suppress the window rather
            // than setting  WindowStyle to hidden as WindowStyle has no
            // effect on command prompt windows and they always appear.
            psi.CreateNoWindow = true;
            psi.FileName = fileToRun;
            psi.Arguments = args;
            psi.WorkingDirectory = workingFolder;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = psi.RedirectStandardError = true;

            this.ReportProgress(String.Format(CultureInfo.CurrentCulture,
                "[{0}]", fileToRun));
            currentProcess.Start();

            // Spawn two threads so that we can capture both STDOUT
            // and STDERR without the risk of a deadlock.
            stdOutThread = new Thread(ReadStdOut);
            stdOutThread.Start();

            stdErrThread = new Thread(ReadStdErr);
            stdErrThread.Start();

            currentProcess.WaitForExit();
            waitCount = 0;

            // Give the output threads a chance to finish
            while(waitCount < 5 && stdOutThread.IsAlive &&
              !stdOutThread.Join(1000))
                waitCount++;

            waitCount = 0;
            while(waitCount < 5 && stdErrThread.IsAlive &&
              !stdErrThread.Join(1000))
                waitCount++;

            currentProcess.Dispose();
            currentProcess = null;
            stdOutThread = stdErrThread = null;

            // Stop if an error was detected
            if(errorDetected)
                throw new BuilderException("BE0043", "Unexpected error " +
                    "detected in last build step.  See output above for " +
                    "details.");
        }

        /// <summary>
        /// This is the thread procedure used to capture standard ouput text
        /// </summary>
        private void ReadStdOut()
        {
            string line;

            try
            {
                do
                {
                    line = currentProcess.StandardOutput.ReadLine();

                    if(line != null)
                        this.ReportToolOutput(line);

                } while(line != null);
            }
            catch(ThreadAbortException)
            {
                System.Diagnostics.Debug.WriteLine("ReadStdOut thread aborted\r\n");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debug.WriteLine("ReadStdOut thread failed\r\n");
            }

            System.Diagnostics.Debug.WriteLine("ReadStdOut thread stopped\r\n");
        }

        /// <summary>
        /// This is the thread procedure used to capture standard error text
        /// </summary>
        private void ReadStdErr()
        {
            string line;

            try
            {
                do
                {
                    line = currentProcess.StandardError.ReadLine();

                    if(line != null)
                        this.ReportToolOutput(line);

                } while(line != null);
            }
            catch(ThreadAbortException)
            {
                System.Diagnostics.Debug.WriteLine("ReadStdErr thread aborted\r\n");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debug.WriteLine("ReadStdErr thread failed\r\n");
            }

            System.Diagnostics.Debug.WriteLine("ReadStdErr thread stopped\r\n");
        }

        /// <summary>
        /// Report the output from the currently running tool
        /// </summary>
        /// <param name="line">The line to report</param>
        private void ReportToolOutput(string line)
        {
            // The ReportProgress method uses String.Format so double
            // any braces in the output.
            if(line.IndexOf('{') != -1)
                line = line.Replace("{", "{{");

            if(line.IndexOf('}') != -1)
                line = line.Replace("}", "}}");

            // Check for errors
            if(reErrorCheck.IsMatch(line))
                errorDetected = true;

            this.ReportProgress(line);
        }
        #endregion
    }
}
