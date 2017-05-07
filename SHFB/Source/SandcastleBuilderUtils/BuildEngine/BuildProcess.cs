//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/24/2017
// Note    : Copyright 2006-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the thread class that handles all aspects of the build process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
//===============================================================================================================
#region Older history
// 1.0.0.0  08/04/2006  EFW  Created the code
// 1.3.0.0  09/09/2006  EFW  Added support for website output
// 1.3.1.0  09/29/2006  EFW  Added support for the ShowMissing* properties
// 1.3.1.0  10/02/2006  EFW  Added support for the September CTP and the Document* properties
// 1.3.2.0  11/10/2006  EFW  Added support for DocumentAssembly.CommentsOnly
// 1.3.4.0  12/24/2006  EFW  Various additions and updates
// 1.4.0.0  03/07/2007  EFW  Added support for the March 2007 CTP
// 1.5.0.0  06/19/2007  EFW  Various additions and updates for the June CTP
// 1.5.0.2  07/03/2007  EFW  Added support for a content site map file and reworked support for language
//                           resources.  Also added support for namespace ripping.
// 1.5.1.0  07/20/2007  EFW  Added support for the API filter property
// 1.5.2.0  09/10/2007  EFW  Exposed some members for use by plug-ins and added support for calling the
//                           plug-ins.
// 1.6.0.0  10/04/2007  EFW  Added support for the September 2007 CTP
// 1.6.0.1  10/29/2007  EFW  Added support for the October 2007 CTP
// 1.6.0.4  01/16/2008  EFW  Added support for the January 2008 release
// 1.6.0.5  02/04/2008  EFW  Added support for the new Extract HTML Info tool and the <inheritdoc /> tag
// 1.6.0.6  03/09/2008  EFW  Wrapped the log and build steps in XML tags
// 1.6.0.7  04/17/2008  EFW  Added support for wildcards in assembly names.  Added support for conceptual
//                           content.
// 1.8.0.0  07/26/2008  EFW  Modified to support the new project format
// 1.8.0.1  12/14/2008  EFW  Updated to use .NET 3.5 and MSBuild 3.5
// 1.8.0.3  07/04/2009  EFW  Added support for the July 2009 release and building MS Help Viewer files
// 1.9.0.0  05/22/2010  EFW  Added support for the June 2010 release.  Reworked solution file handling to
//                           honor solution-level per-project configuration and platform settings.  Added
//                           support for multi-format build output. Moved GenerateIntermediateTableOfContents
//                           so that it occurs right after MergeTablesOfContents.
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 1.9.2.0  01/16/2011  EFW  Updated to support selection of Silverlight Framework versions
// 1.9.3.2  08/20/2011  EFW  Updated to support selection of .NET Portable Framework versions
// 1.9.4.0  03/25/2012  EFW  Merged changes for VS2010 style from Don Fehr
// 1.9.5.0  09/10/2012  EFW  Updated to use the new framework definition file for the .NET Framework versions
// 1.9.6.0  10/25/2012  EFW  Updated to use the new presentation style definition files
// 1.9.7.0  01/02/2013  EFW  Added method to get referenced namespaces
// 1.9.8.0  06/21/2013  EFW  Added support for format-specific help content files.  Removed the
//                           ModifyHelpTopicFilenames build step.
// -------  12/04/2013  EFW  Removed the ApplyVisibilityProperties build step.  Plug-ins can apply visibility
//                           settings if needed by calling the ApplyVisibilityProperties() method.  Added
//                           support for namespace grouping based on changes submitted by Stazzz.
#endregion
//          12/17/2013  EFW  Removed the SandcastlePath property and all references to it
//          12/29/2013  EFW  Added support for the ReferenceOutputAssembly project reference metadata item
//          01/09/2014  EFW  Removed copying of branding files.  They are part of the presentation style now.
//          01/11/2014  EFW  Updated where shared content and stop word lists are copied from.  These files are
//                           part of each presentation style now.
//          02/15/2014  EFW  Added support for the Open XML output format
//          05/14/2014  EFW  Added support for presentation style plug-in dependencies
//          12/14/2014  EFW  Updated to use framework-specific reflection data folders
//          03/30/2015  EFW  Added support for the Markdown output format
//          05/03/2015  EFW  Removed support for the MS Help 2 file format
//          12/21/2015  EFW  Merged conceptual and reference topic build steps
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;
using Sandcastle.Core.Reflection;
using Sandcastle.Core.PresentationStyle;

using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.MSBuild;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class is used to handle all aspects of the build process in a separate thread
    /// </summary>
    public partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject project;      // The project to build
        private string originalProjectName;

        // The composition container for build components and the syntax generator list
        private CompositionContainer componentContainer;
        private List<ISyntaxGeneratorMetadata> syntaxGenerators;
        private Dictionary<string, BuildComponentFactory> buildComponents;

        // Framework, assembly, and reference information
        private ReflectionDataSetDictionary reflectionDataDictionary;
        private ReflectionDataSet frameworkReflectionData;
        private Collection<string> assembliesList;
        private Dictionary<string, Tuple<string, string, List<KeyValuePair<string, string>>>> referenceDictionary;
        private HashSet<string> referencedNamespaces;

        // Conceptual content settings
        private ConceptualContentSettings conceptualContent;
        private int apiTocOrder;
        private string apiTocParentId, rootContentContainerId;

        // The log file stream
        private StreamWriter swLog;

        // Build output file lists
        private Collection<string> help1Files, helpViewerFiles, websiteFiles, openXmlFiles, markdownFiles;

        // Build progress tracking
        private DateTime buildStart, stepStart;
        private bool buildCancelling;

        // Various paths and other strings
        private string templateFolder, projectFolder, outputFolder, workingFolder, hhcFolder, languageFolder,
            defaultTopic, reflectionFile, msBuildExePath;

        private Collection<string> helpFormatOutputFolders;

        private CultureInfo language;   // The project language

        private PresentationStyleSettings presentationStyle;    // The presentation style settings

        // The current help file format being generated
        private HelpFileFormats currentFormat;

        // Substitution tag replacement handler and task runner
        private SubstitutionTagReplacement substitutionTags;
        private TaskRunner taskRunner;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the progress report provider
        /// </summary>
        /// <remarks>If not set, no progress will be reported by the build</remarks>
        public IProgress<BuildProgressEventArgs> ProgressReportProvider { get; set; }

        /// <summary>
        /// This is used to get or set the cancellation token for the build if running as a task
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// This read-only property is used to get the current build step
        /// </summary>
        public BuildStep CurrentBuildStep { get; private set; }

        /// <summary>
        /// This read-only property returns the build start time
        /// </summary>
        public DateTime BuildStart
        {
            get { return buildStart; }
        }

        /// <summary>
        /// This returns the path to MSBuild.exe
        /// </summary>
        public string MSBuildExePath
        {
            get { return msBuildExePath; }
        }

        /// <summary>
        /// This returns the location of the help file builder template folder
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
        /// This returns the output folder where the log file and help file can be found after the build process
        /// has finished.
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
        /// This returns the name of the HTML Help 1 compiler folder determined by the build process
        /// </summary>
        public string Help1CompilerFolder
        {
            get { return hhcFolder; }
        }

        /// <summary>
        /// This returns the name of the folder that contains the reflection data for the selected framework
        /// platform and version (.NETFramework 4.5, .NETCore 4.5, Silverlight 5.0, etc.).
        /// </summary>
        public string FrameworkReflectionDataFolder
        {
            get
            {
                return Path.GetDirectoryName(frameworkReflectionData.Filename);
            }
        }

        /// <summary>
        /// This read-only property returns the language used for resource items, etc.
        /// </summary>
        public CultureInfo Language
        {
            get { return language; }
        }

        /// <summary>
        /// This read-only property returns the resource item file language folder name
        /// </summary>
        public string LanguageFolder
        {
            get { return languageFolder; }
        }

        /// <summary>
        /// This returns the presentation instance being used by the build process
        /// </summary>
        public PresentationStyleSettings PresentationStyle
        {
            get { return presentationStyle; }
        }

        /// <summary>
        /// This returns the name of the main Sandcastle presentation style folder determined by the build
        /// process.
        /// </summary>
        public string PresentationStyleFolder
        {
            get { return FolderPath.TerminatePath(presentationStyle.ResolvePath(presentationStyle.BasePath)); }
        }

        /// <summary>
        /// This returns the name of the presentation style resource items folder determined by the build
        /// process.
        /// </summary>
        public string PresentationStyleResourceItemsFolder
        {
            get
            {
                return FolderPath.TerminatePath(Path.Combine(presentationStyle.ResolvePath(
                    presentationStyle.ResourceItemsPath), languageFolder));
            }
        }

        /// <summary>
        /// This read-only property returns a collection of the output folders specific to each help file format
        /// produced by the build.
        /// </summary>
        public Collection<string> HelpFormatOutputFolders
        {
            get { return helpFormatOutputFolders; }
        }

        /// <summary>
        /// This returns the name of the log file used for saving the build progress messages
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
        /// This read-only property returns the framework reflection data dictionary used by the build
        /// </summary>
        public ReflectionDataSetDictionary ReflectionDataSetDictionary
        {
            get { return reflectionDataDictionary; }
        }

        /// <summary>
        /// This read-only property returns the framework reflection data settings used by the build
        /// </summary>
        public ReflectionDataSet FrameworkReflectionData
        {
            get { return frameworkReflectionData; }
        }

        /// <summary>
        /// This returns the current project being used for the build
        /// </summary>
        /// <remarks>Although there is nothing stopping it, project options should not be modified during a
        /// build.</remarks>
        public SandcastleProject CurrentProject
        {
            get { return project; }
        }

        /// <summary>
        /// This returns the current help file format being generated
        /// </summary>
        /// <remarks>The <b>GenerateHelpFormatTableOfContents</b>, <b>GenerateHelpFileIndex</b>,
        /// <b>GenerateHelpProject</b>, and <b>CompilingHelpFile</b> steps will run once for each help file
        /// format selected.  This property allows a plug-in to determine which files it may need to work with
        /// during those steps or to skip processing if it is not relevant.</remarks>
        public HelpFileFormats CurrentFormat
        {
            get { return currentFormat; }
        }

        /// <summary>
        /// This read-only property is used to get the partial build type
        /// </summary>
        /// <remarks>Partial builds occur when editing the namespace summaries, editing the API filter, and as
        /// part of some plug-ins that do not require all build options.  In a partial build, build steps after
        /// the point indicated by this property are not executed and the build stops.</remarks>
        public PartialBuildType PartialBuildType { get; private set; }

        /// <summary>
        /// This is used to get the conceptual content settings in effect for the build
        /// </summary>
        public ConceptualContentSettings ConceptualContent
        {
            get
            {
                // Create on first use.  Plug-ins may want to add stuff earlier than we need it.
                if(conceptualContent == null)
                    conceptualContent = new ConceptualContentSettings(project);

                return conceptualContent;
            }
        }

        /// <summary>
        /// This returns a list of the HTML Help 1 (CHM) files that were built
        /// </summary>
        /// <remarks>If the HTML Help 1 format was not built, this returns an empty collection</remarks>
        public Collection<string> Help1Files
        {
            get { return help1Files; }
        }

        /// <summary>
        /// This returns a list of the MS Help Viewer (MSHC) files that were built
        /// </summary>
        /// <remarks>If the MS Help Viewer format was not built, this returns an empty collection</remarks>
        public Collection<string> HelpViewerFiles
        {
            get { return helpViewerFiles; }
        }

        /// <summary>
        /// This returns a list of the website files that were built
        /// </summary>
        /// <remarks>If the website format was not built, this returns an empty collection</remarks>
        public Collection<string> WebsiteFiles
        {
            get { return websiteFiles; }
        }

        /// <summary>
        /// This returns a list of the Open XML files that were built
        /// </summary>
        /// <remarks>If the Open XML format was not built, this returns an empty collection</remarks>
        public Collection<string> OpenXmlFiles
        {
            get { return openXmlFiles; }
        }

        /// <summary>
        /// This returns a list of the Markdown files that were built
        /// </summary>
        /// <remarks>If the Markdown format was not built, this returns an empty collection</remarks>
        public Collection<string> MarkdownFiles
        {
            get { return markdownFiles; }
        }

        /// <summary>
        /// This returns the task runner instance
        /// </summary>
        public TaskRunner TaskRunner
        {
            get { return taskRunner; }
        }

        /// <summary>
        /// This returns the substitution tag replacement handler instance
        /// </summary>
        public SubstitutionTagReplacement SubstitutionTags
        {
            get
            {
                // The tag handler is created in the build process.  However, some code uses it for simple
                // transformations that don't rely on items created during the build.  In those cases, return
                // an instance that will work for simple substitutions such as project property values.
                if(substitutionTags == null)
                    substitutionTags = new SubstitutionTagReplacement(this);

                return substitutionTags;
            }
        }

        /// <summary>
        /// This controls whether or not the API filter is suppressed
        /// </summary>
        /// <value>By default, it is not suppressed and the API filter will be applied.  The API Filter designer
        /// uses this to suppress the filter so that all members are obtained.</value>
        public bool SuppressApiFilter { get; set; }

        /// <summary>
        /// This is used to get or set the table of contents parent for the API content
        /// </summary>
        /// <remarks>If not set, <see cref="RootContentContainerId" /> is used if it is set.  If it is not,
        /// <see cref="SandcastleProject.TocParentId" /> is used.  If this property is set, the value should be
        /// the ID of a topic in the project's conceptual content.  The topic must appear in a content layout
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
        /// This is used to get or set the sort order for API content so that it appears within its parent in the
        /// correct position.
        /// </summary>
        /// <remarks>The default is -1 to let the build engine determine the best value to use based on the other
        /// project properties.</remarks>
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
        /// This is used to get or set the topic ID to use for the root content container node
        /// </summary>
        /// <remarks>If not set, all content will appear at the root level in the
        /// <see cref="SandcastleProject.TocParentId" />.  If set, the value should be the ID of a topic in the
        /// project's conceptual content.  The topic must appear in a content layout file and must have its
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

        /// <summary>
        /// This returns the filename of the default topic as determined by the build engine
        /// </summary>
        /// <remarks>The path is relative to the root of the output folder (i.e. html/DefaultTopic.htm)</remarks>
        public string DefaultTopicFile
        {
            get { return defaultTopic ?? String.Empty; }
        }

        /// <summary>
        /// This returns the <see cref="SandcastleProject.HelpTitle"/> project property value with all
        /// substitution tags it contains, if any, resolved to actual values.
        /// </summary>
        public string ResolvedHelpTitle
        {
            get { return substitutionTags.TransformText(project.HelpTitle); }
        }

        /// <summary>
        /// This returns the <see cref="SandcastleProject.HtmlHelpName"/> project property value with all
        /// substitution tags it contains, if any, resolved to actual values.
        /// </summary>
        public string ResolvedHtmlHelpName
        {
            get { return substitutionTags.TransformText(project.HtmlHelpName); }
        }

        /// <summary>
        /// This read-only property returns a hash set used to contain a list of namespaces referenced by the
        /// project reflection data files, project XML comments files, and base framework XML comments files.
        /// </summary>
        /// <value>These namespaces are used to limit what the Resolve Reference Links component has to index</value>
        public HashSet<string> ReferencedNamespaces
        {
            get
            {
                if(referencedNamespaces == null)
                    referencedNamespaces = new HashSet<string>();

                return referencedNamespaces;
            }
        }

        /// <summary>
        /// This read-only property returns the MEF component container
        /// </summary>
        internal CompositionContainer ComponentContainer
        {
            get { return componentContainer; }
        }

        /// <summary>
        /// This read-only property returns the syntax generator metadata
        /// </summary>
        internal IEnumerable<ISyntaxGeneratorMetadata> SyntaxGenerators
        {
            get { return syntaxGenerators; }
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
            project = buildProject;

            // Save a copy of the project filename.  If using a temporary project, it won't match the passed
            // project's name.
            originalProjectName = buildProject.Filename;

            apiTocOrder = -1;
            apiTocParentId = rootContentContainerId = String.Empty;

            help1Files = new Collection<string>();
            helpViewerFiles = new Collection<string>();
            websiteFiles = new Collection<string>();
            openXmlFiles = new Collection<string>();
            markdownFiles = new Collection<string>();
            helpFormatOutputFolders = new Collection<string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProject">The project to build</param>
        /// <param name="partialBuildType">The partial build type to perform</param>
        public BuildProcess(SandcastleProject buildProject, PartialBuildType partialBuildType) : this(buildProject)
        {
            this.PartialBuildType = partialBuildType;
        }
        #endregion

        #region Main build method
        //=====================================================================

        /// <summary>
        /// Call this method to perform the build on the project.
        /// </summary>
        public void Build()
        {
            Project msBuildProject = null;
            ProjectItem projectItem;
            string resolvedPath, helpFile, languageFile, scriptFile, hintPath, message = null;
            SandcastleProject originalProject = null;

            System.Diagnostics.Debug.WriteLine("Build process starting\r\n");

            try
            {
                taskRunner = new TaskRunner(this);

                // If the project isn't using final values suitable for the build, create a copy of the
                // project that is using final values.
                if(!project.UsingFinalValues)
                {
                    originalProject = project;
                    project = new SandcastleProject(originalProject.MSBuildProject);
                }

                Assembly asm = Assembly.GetExecutingAssembly();

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                this.ReportProgress(BuildStep.Initializing, "[{0}, version {1}]", fvi.ProductName, fvi.ProductVersion);

                buildStart = stepStart = DateTime.Now;

                // The version of MSBuild to use is based on the tools version set in the project
                msBuildExePath = Path.Combine(ProjectCollection.GlobalProjectCollection.Toolsets.First(
                    t => t.ToolsVersion == project.MSBuildProject.ToolsVersion).ToolsPath, "MSBuild.exe");

                // Get the location of the template files
                templateFolder = ComponentUtilities.ToolsFolder + @"Templates\";

                // Make sure we start out in the project's output folder in case the output folder is relative
                // to it.
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

                // Create the log file.  The log may be in a folder other than the output so make sure it exists
                // too.
                if(!Directory.Exists(Path.GetDirectoryName(this.LogFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(this.LogFilename));

                swLog = new StreamWriter(this.LogFilename);

                swLog.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<shfbBuild product=\"{0}\" " +
                    "version=\"{1}\" projectFile=\"{2}\" started=\"{3}\">\r\n<buildStep step=\"{4}\">",
                    fvi.ProductName, fvi.ProductVersion, originalProjectName, DateTime.Now,
                    BuildStep.Initializing);

                if(project.WorkingPath.Path.Length == 0)
                    workingFolder = outputFolder + @"Working\";
                else
                    workingFolder = project.WorkingPath;

                if((project.HelpFileFormat & HelpFileFormats.Website) != 0)
                    BuildProcess.VerifySafePath("OutputPath", outputFolder, projectFolder);

                // The output folder and the working folder cannot be the same
                if(workingFolder == outputFolder)
                    throw new BuilderException("BE0030", "The OutputPath and WorkingPath properties cannot be " +
                        "set to the same path");

                // Make sure we can find the tools
                this.FindTools();

                // Check for the SHFBROOT environment variable.  It may not be present yet if a reboot hasn't
                // occurred after installation.  In such cases, set it to the proper folder for this process so
                // that projects can be loaded and built.
                if(Environment.GetEnvironmentVariable("SHFBROOT") == null)
                {
                    // We won't issue a warning since it may not be defined in some build environments such as
                    // on a build server.  In such cases, it is passed in as a command line option to MSBuild.
                    // Storing it in the environment here lets the SHFB build projects work as expected.
                    this.ReportProgress("The SHFBROOT system environment variable was not found.  This " +
                        "variable is usually created during installation and may require a reboot.  It has " +
                        "been defined temporarily for this process as: SHFBROOT={0}",
                        ComponentUtilities.ToolsFolder);

                    Environment.SetEnvironmentVariable("SHFBROOT", ComponentUtilities.ToolsFolder);
                }

                this.ReportProgress("Locating components in the following folder(s):");

                if(!String.IsNullOrEmpty(project.ComponentPath))
                    this.ReportProgress("    {0}", project.ComponentPath);

                this.ReportProgress("    {0}", Path.GetDirectoryName(project.Filename));

                this.ReportProgress("    {0}", ComponentUtilities.ComponentsFolder);
                this.ReportProgress("    {0}", ComponentUtilities.ToolsFolder);

                // Get the framework reflection data settings to use for the build
                reflectionDataDictionary = new ReflectionDataSetDictionary(new[] { project.ComponentPath,
                    Path.GetDirectoryName(project.Filename) });
                frameworkReflectionData = reflectionDataDictionary.CoreFrameworkByTitle(project.FrameworkVersion, true);

                if(frameworkReflectionData == null)
                    throw new BuilderException("BE0071", String.Format(CultureInfo.CurrentCulture,
                        "Unable to locate information for the project framework version '{0}' or a suitable " +
                        "redirected version on this system.  See error number help topic for details.",
                        project.FrameworkVersion));

                this.ReportProgress("Using framework reflection data for '{0}' located in '{1}'",
                    this.FrameworkReflectionData.Title, this.FrameworkReflectionDataFolder);

                if(!Directory.EnumerateFiles(this.FrameworkReflectionDataFolder, "*.xml").Any())
                    throw new BuilderException("BE0032", "Reflection data files for the selected framework " +
                        "do not exist yet (" + frameworkReflectionData.Title + ").  See help file for " +
                        "details about this error number.");

                // Warn if a different framework is being used for the build
                if(frameworkReflectionData.Title != project.FrameworkVersion)
                    this.ReportWarning("BE0072", "Project framework version '{0}' not found.  It has been " +
                        "redirected and will use '{1}' instead.", project.FrameworkVersion,
                        frameworkReflectionData.Title);

                // Get the composition container used to find build components in the rest of the build process
                componentContainer = ComponentUtilities.CreateComponentContainer(new[] { project.ComponentPath,
                    Path.GetDirectoryName(project.Filename) }, this.CancellationToken);

                syntaxGenerators = componentContainer.GetExports<ISyntaxGeneratorFactory,
                    ISyntaxGeneratorMetadata>().Select(sf => sf.Metadata).ToList();
                buildComponents = componentContainer.GetExports<BuildComponentFactory,
                    IBuildComponentMetadata>().GroupBy(c => c.Metadata.Id).Select(g => g.First()).ToDictionary(
                        key => key.Metadata.Id, value => value.Value);

                // Figure out which presentation style to use
                var style = componentContainer.GetExports<PresentationStyleSettings,
                    IPresentationStyleMetadata>().FirstOrDefault(s => s.Metadata.Id.Equals(
                        project.PresentationStyle, StringComparison.OrdinalIgnoreCase));

                if(style == null)
                    throw new BuilderException("BE0001", "The PresentationStyle property value of '" +
                        project.PresentationStyle + "' is not recognized as a valid presentation style definition");

                presentationStyle = style.Value;

                this.ReportProgress("Using presentation style '{0}' located in '{1}'", style.Metadata.Id,
                    Path.Combine(presentationStyle.Location, presentationStyle.BasePath ?? String.Empty));

                var psErrors = presentationStyle.CheckForErrors();

                if(psErrors.Any())
                    throw new BuilderException("BE0004", String.Format(CultureInfo.CurrentCulture,
                        "The selected presentation style ({0}) is not valid.  Reason(s):\r\n{1}",
                        style.Metadata.Id, String.Join("\r\n", psErrors)));

                // If the presentation style does not support one or more of the selected help file formats,
                // stop now.
                if((project.HelpFileFormat & ~presentationStyle.SupportedFormats) != 0)
                    throw new BuilderException("BE0074", String.Format(CultureInfo.CurrentCulture,
                        "The selected presentation style ({0}) does not support one or more of the selected " +
                        "help file formats.  Supported formats: {1}", style.Metadata.Id,
                        presentationStyle.SupportedFormats));

                // Create the substitution tag replacement handler now as we have everything it needs
                substitutionTags = new SubstitutionTagReplacement(this);

                // Load the plug-ins if necessary
                if(project.PlugInConfigurations.Count != 0 || presentationStyle.PlugInDependencies.Count != 0)
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

                    // For MS Help Viewer, the HTML Help Name cannot contain periods, ampersands, or pound signs
                    if((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0 &&
                      this.ResolvedHtmlHelpName.IndexOfAny(new[] { '.', '#', '&' }) != -1)
                        throw new BuilderException("BE0075", "For MS Help Viewer builds, the HtmlHelpName property " +
                            "cannot contain periods, ampersands, or pound signs as they are not valid in the " +
                            "help file name.");

                    // If the help file is open, it will fail to build so try to get rid of it now before we
                    // get too far into it.
                    helpFile = outputFolder + this.ResolvedHtmlHelpName + ".chm";

                    if((project.HelpFileFormat & HelpFileFormats.HtmlHelp1) != 0 && File.Exists(helpFile))
                        File.Delete(helpFile);

                    helpFile = Path.ChangeExtension(helpFile, ".mshc");

                    if((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0 && File.Exists(helpFile))
                        File.Delete(helpFile);

                    if((project.HelpFileFormat & HelpFileFormats.Website) != 0)
                    {
                        helpFile = outputFolder + "Index.aspx";

                        if(File.Exists(helpFile))
                            File.Delete(helpFile);

                        helpFile = Path.ChangeExtension(helpFile, ".html");

                        if(File.Exists(helpFile))
                            File.Delete(helpFile);
                    }

                    helpFile = outputFolder + this.ResolvedHtmlHelpName + ".docx";

                    if((project.HelpFileFormat & HelpFileFormats.OpenXml) != 0 && File.Exists(helpFile))
                        File.Delete(helpFile);
                }
                catch(IOException ex)
                {
                    throw new BuilderException("BE0025", "Unable to remove prior build output: " + ex.Message);
                }
                catch
                {
                    throw;
                }

                if((project.HelpFileFormat & (HelpFileFormats.Website | HelpFileFormats.Markdown)) != 0)
                {
                    this.ReportProgress("-------------------------------");
                    this.ReportProgress("Clearing any prior web/markdown output...");

                    // Purge all files and folders from the output path except for the working folder and the
                    // build log.  Read-only and/or hidden files and folders are ignored as they are assumed to
                    // be under source control.
                    foreach(string file in Directory.EnumerateFiles(outputFolder))
                        if(!file.EndsWith(Path.GetFileName(this.LogFilename), StringComparison.Ordinal))
                            if((File.GetAttributes(file) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                                File.Delete(file);
                            else
                                this.ReportProgress("    Ignoring read-only/hidden file {0}", file);

                    foreach(string folder in Directory.EnumerateDirectories(outputFolder))
                        try
                        {
                            // Ignore the working folder in case it wasn't removed above
                            if(!folder.Equals(workingFolder.Substring(0, workingFolder.Length - 1), StringComparison.OrdinalIgnoreCase))
                            {
                                // Some source control providers have a mix of read-only/hidden files within a
                                // folder that isn't read-only/hidden (i.e. Subversion).  In such cases, leave
                                // the folder alone.
                                if(Directory.EnumerateFileSystemEntries(folder, "*", SearchOption.AllDirectories).Any(
                                  f => (File.GetAttributes(f) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0))
                                {
                                    this.ReportProgress("    Did not delete folder '{0}' as it contains " +
                                        "read-only or hidden folders/files", folder);
                                }
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

                Directory.CreateDirectory(workingFolder);

                // Validate the documentation source information, gather assembly and reference info, and copy
                // XML comments files to the working folder.
                this.ValidateDocumentationSources();

                // If the framework reflection data is still set to the .NETStandard placeholder, there were no
                // project documentation sources.  As such, switch to the most recent .NET Framework.
                if(frameworkReflectionData.Platform == PlatformType.DotNetStandard)
                {
                    frameworkReflectionData = reflectionDataDictionary.CoreFrameworkMostRecent(PlatformType.DotNetFramework);
                    project.FrameworkVersion = frameworkReflectionData.Title;
                }

                // Transform the shared builder content files
                language = project.Language;
                languageFile = Path.Combine(presentationStyle.ResolvePath(presentationStyle.ToolResourceItemsPath),
                    language.Name + ".xml");

                this.ReportProgress(BuildStep.GenerateSharedContent, "Generating shared content files ({0}, {1})...",
                    language.Name, language.DisplayName);

                if(!File.Exists(languageFile))
                {
                    languageFile = Path.Combine(presentationStyle.ResolvePath(presentationStyle.ToolResourceItemsPath),
                        "en-US.xml");

                    // Warn the user about the default being used
                    this.ReportWarning("BE0002", "Help file builder content for the '{0}, {1}' language could " +
                        "not be found.  Using 'en-US, English (US)' defaults.", language.Name, language.DisplayName);
                }

                // See if the user has translated the Sandcastle resources.  If not found, default to US English.
                languageFolder = Path.Combine(presentationStyle.ResolvePath(presentationStyle.ResourceItemsPath),
                    language.Name);

                if(Directory.Exists(languageFolder))
                    languageFolder = language.Name + @"\";
                else
                {
                    // Warn the user about the default being used.  The language will still be used for the help
                    // file though.
                    if(language.Name != "en-US")
                        this.ReportWarning("BE0003", "Sandcastle shared content for the '{0}, {1}' language " +
                            "could not be found.  Using 'en-US, English (US)' defaults.", language.Name,
                            language.DisplayName);

                    languageFolder = String.Empty;
                }

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    substitutionTags.TransformTemplate(Path.GetFileName(languageFile), Path.GetDirectoryName(languageFile),
                        workingFolder);
                    File.Move(workingFolder + Path.GetFileName(languageFile), workingFolder + "SHFBContent.xml");

                    // Copy the stop word list
                    languageFile = Path.Combine(ComponentUtilities.ToolsFolder, @"PresentationStyles\Shared\" +
                        @"StopWordList\" + Path.GetFileNameWithoutExtension(languageFile) +".txt");
                    File.Copy(languageFile, workingFolder + "StopWordList.txt");
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
                    substitutionTags.TransformTemplate("MRefBuilder.config", templateFolder, workingFolder);
                    scriptFile = substitutionTags.TransformTemplate("GenerateRefInfo.proj", templateFolder, workingFolder);

                    try
                    {
                        msBuildProject = new Project(scriptFile);

                        // Add the references
                        foreach(var r in referenceDictionary.Values)
                        {
                            projectItem = msBuildProject.AddItem(r.Item1, r.Item2, r.Item3)[0];

                            // Make sure hint paths are correct by adding the project folder to any relative
                            // paths.  Skip any containing MSBuild variable references.
                            if(projectItem.HasMetadata(BuildItemMetadata.HintPath))
                            {
                                hintPath = projectItem.GetMetadataValue(BuildItemMetadata.HintPath);

                                if(!Path.IsPathRooted(hintPath) && hintPath.IndexOf("$(",
                                  StringComparison.Ordinal) == -1)
                                {
                                    hintPath = FilePath.GetFullPath(Path.Combine(projectFolder, hintPath));

                                    // If the full path length would exceed the system maximums, make it relative
                                    // to keep it under the maximum lengths.
                                    if(hintPath.Length > 259 || Path.GetDirectoryName(hintPath).Length > 247)
                                        hintPath = FolderPath.AbsoluteToRelativePath(workingFolder, hintPath);

                                    projectItem.SetMetadataValue(BuildItemMetadata.HintPath, hintPath);
                                }
                            }
                        }

                        // Add the assemblies to document
                        foreach(string assemblyName in assembliesList)
                            msBuildProject.AddItem("Assembly", assemblyName);

                        msBuildProject.Save(scriptFile);
                    }
                    finally
                    {
                        // If we loaded it, we must unload it.  If not, it is cached and may cause problems later.
                        if(msBuildProject != null)
                        {
                            ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject);
                            ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject.Xml);
                        }
                    }

                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    // Silverlight build targets are only available for 32-bit builds regardless of the framework
                    // version and require the 32-bit version of MSBuild in order to load the target file correctly.
                    if(project.FrameworkVersion.StartsWith("Silverlight", StringComparison.OrdinalIgnoreCase))
                        taskRunner.Run32BitProject("GenerateRefInfo.proj", false);
                    else
                        taskRunner.RunProject("GenerateRefInfo.proj", false);

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // If this was a partial build used to obtain API information, stop now
                if(this.PartialBuildType == PartialBuildType.GenerateReflectionInfo)
                {
                    commentsFiles.Save();
                    goto AllDone;       // Yeah, I know it's evil but it's quick
                }

                // Transform the reflection output based on the document model and create the topic manifest
                this.ReportProgress(BuildStep.TransformReflectionInfo, "Transforming reflection output...");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    scriptFile = substitutionTags.TransformTemplate("TransformManifest.proj", templateFolder, workingFolder);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    taskRunner.RunProject("TransformManifest.proj", false);

                    // Change the reflection file extension before running the ExecutionBehaviors.After plug-ins
                    // so that the plug-ins (if any) get the correct filename.
                    reflectionFile = Path.ChangeExtension(reflectionFile, ".xml");

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }
                else
                    reflectionFile = Path.ChangeExtension(reflectionFile, ".xml");

                // If this was a partial build used to obtain information for namespace and namespace group
                // comments, stop now.
                if(this.PartialBuildType == PartialBuildType.TransformReflectionInfo)
                {
                    commentsFiles.Save();
                    goto AllDone;       // Yeah, I know it's evil but it's quick
                }

                // Load the transformed reflection information file
                reflectionFile = workingFolder + "reflection.xml";

                // If there is nothing to document, stop the build
                if(!ComponentUtilities.XmlStreamAxis(reflectionFile, "api").Any())
                    throw new BuilderException("BE0033", "No APIs found to document.  See error topic in " +
                        "help file for details.");

                // Generate namespace summary information
                this.GenerateNamespaceSummaries();

                // For any reference assemblies that have a hint path, add any matching XML comments file to
                // the comments file collection for base class comments.  We add these after generating namespace
                // summaries as these aren't relevant to that step and we don't want to modify them.  We also
                // want the project documentation source XML files to override comments in these if there's a
                // conflict so we add them ahead of all other comments files.  We still need to copy the files as
                // the rest of the build process expects them to be in the working folder.
                foreach(var r in referenceDictionary.Values.Where(r => r.Item3.Any(v => v.Key == "HintPath")))
                {
                    string comments = Path.ChangeExtension(r.Item3.First(kv => kv.Key == "HintPath").Value, ".xml");
                    string workingPath = workingFolder + Path.GetFileName(comments);
                    int idx = 0;

                    if(File.Exists(comments) && !commentsFiles.Any(c => c.SourcePath == workingPath))
                    {
                        File.Copy(comments, workingPath, true);
                        File.SetAttributes(workingPath, FileAttributes.Normal);

                        commentsFiles.Insert(idx++, new XmlCommentsFile(workingPath));
                    }
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
                        substitutionTags.TransformTemplate("GenerateInheritedDocs.config", templateFolder, workingFolder);
                        scriptFile = substitutionTags.TransformTemplate("GenerateInheritedDocs.proj", templateFolder,
                            workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        taskRunner.RunProject("GenerateInheritedDocs.proj", true);
                        
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // This should always be last so that it overrides comments in the project XML comments files
                    commentsFiles.Add(new XmlCommentsFile(workingFolder + "_InheritedDocs_.xml"));
                }

                commentsFiles.Save();

                this.EnsureOutputFoldersExist(null);

                // Copy conceptual content files if there are topics or tokens.  Tokens can be replaced in
                // XML comments files so we check for them too.
                if(this.ConceptualContent.ContentLayoutFiles.Count != 0 || this.ConceptualContent.TokenFiles.Count != 0)
                {
                    this.ReportProgress(BuildStep.CopyConceptualContent, "Copying conceptual content...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ConceptualContent.CopyContentFiles(this);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.ReportProgress(BuildStep.CreateConceptualTopicConfigs,
                        "Creating conceptual topic configuration files...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ConceptualContent.CreateConfigurationFiles(this);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }
                else    // Create an empty xmlComp folder required by the build configuration
                    Directory.CreateDirectory(Path.Combine(workingFolder, "xmlComp"));

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
                    scriptFile = substitutionTags.TransformTemplate("GenerateIntermediateTOC.proj", templateFolder,
                        workingFolder);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    taskRunner.RunProject("GenerateIntermediateTOC.proj", false);

                    // Determine the API content placement
                    this.DetermineApiContentPlacement();

                    // If there is conceptual content, generate the conceptual intermediate TOC
                    if(toc != null)
                    {
                        this.ReportProgress("Generating conceptual content intermediate TOC file...");

                        toc.SaveToIntermediateTocFile((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0 ?
                            this.RootContentContainerId : null, project.TocOrder, workingFolder + "_ConceptualTOC_.xml");
                    }

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Create the Sandcastle configuration file
                this.ReportProgress(BuildStep.CreateBuildAssemblerConfigs, "Creating Sandcastle configuration files...");

                // Add referenced namespaces to the hash set.  These are used to ensure just the needed set of
                // reflection target files are loaded by BuildAssembler and nothing more to save some time and
                // memory.
                var rn = this.ReferencedNamespaces;

                // These are all of the valid namespaces we are interested in.  This prevents the methods below
                // from returning nested types as potential namespaces since they can't tell the difference.
                HashSet<string> validNamespaces = new HashSet<string>(Directory.EnumerateFiles(
                    this.FrameworkReflectionDataFolder, "*.xml", SearchOption.AllDirectories).Select(
                        f => Path.GetFileNameWithoutExtension(f)));

                // Get namespaces referenced in the XML comments of the documentation sources
                foreach(var n in commentsFiles.GetReferencedNamespaces(validNamespaces))
                    rn.Add(n);

                // Get namespaces referenced in the reflection data (plug-ins are responsible for adding
                // additional namespaces if they add other reflection data files).
                foreach(string n in GetReferencedNamespaces(reflectionFile, validNamespaces))
                    rn.Add(n);

                // Get namespaces from the Framework comments files of the referenced namespaces.  This adds
                // references for stuff like designer and support classes not directly referenced anywhere else.
                foreach(string n in frameworkReflectionData.GetReferencedNamespaces(language, rn, validNamespaces).ToList())
                    rn.Add(n);

                // If F# syntax is being generated, add some of the F# namespaces as the syntax sections generate
                // references to types that may not be there in non-F# projects.
                if(ComponentUtilities.SyntaxFiltersFrom(syntaxGenerators, project.SyntaxFilters).Any(
                  f => f.Id == "F#"))
                {
                    rn.Add("Microsoft.FSharp.Core");
                    rn.Add("Microsoft.FSharp.Control");
                }

                // If there are no referenced namespaces, add System as a default to prevent the build components
                // from loading the entire set.
                if(rn.Count == 0)
                    rn.Add("System");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    this.ExecutePlugIns(ExecutionBehaviors.Before);

                    this.ReportProgress("    sandcastle.config");

                    // The configuration varies based on the style.  We'll use a common name (sandcastle.config).
                    resolvedPath = presentationStyle.ResolvePath(presentationStyle.BuildAssemblerConfiguration);
                    substitutionTags.TransformTemplate(Path.GetFileName(resolvedPath), Path.GetDirectoryName(resolvedPath),
                        workingFolder);

                    if(!Path.GetFileName(resolvedPath).Equals("sandcastle.config", StringComparison.OrdinalIgnoreCase))
                        File.Move(workingFolder + Path.GetFileName(resolvedPath), workingFolder + "sandcastle.config");

                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Merge the build component custom configurations
                this.MergeComponentConfigurations();

                commentsFiles = null;

                // Build the help topics
                this.ReportProgress(BuildStep.BuildTopics, "Building help topics...");

                if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                {
                    scriptFile = substitutionTags.TransformTemplate("BuildTopics.proj", templateFolder,
                        workingFolder);

                    this.ExecutePlugIns(ExecutionBehaviors.Before);
                                        
                    taskRunner.RunProject("BuildTopics.proj", false);
                    
                    this.ExecutePlugIns(ExecutionBehaviors.After);
                }

                // Combine the conceptual and API intermediate TOC files into one
                this.CombineIntermediateTocFiles();

                // The last part differs based on the help file format
                if((project.HelpFileFormat & (HelpFileFormats.HtmlHelp1 | HelpFileFormats.Website)) != 0)
                {
                    this.ReportProgress(BuildStep.ExtractingHtmlInfo,
                        "Extracting HTML info for HTML Help 1 and/or website...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = substitutionTags.TransformTemplate("ExtractHtmlInfo.proj", templateFolder,
                            workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        taskRunner.RunProject("ExtractHtmlInfo.proj", true);

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                // Copy the standard help file content.  This is done just before compiling the help so that
                // template files from the presentation style can take advantage of tag substitution.  By this
                // point, we should have everything we could possibly need.
                this.CopyStandardHelpContent();

                if((project.HelpFileFormat & HelpFileFormats.HtmlHelp1) != 0)
                {
                    // Generate the table of contents and set the default topic
                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents,
                        "Generating HTML Help 1 table of contents file...");

                    currentFormat = HelpFileFormats.HtmlHelp1;

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
                        substitutionTags.TransformTemplate("Help1x.hhp", templateFolder, workingFolder);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Build the HTML Help 1 help file
                    this.ReportProgress(BuildStep.CompilingHelpFile, "Compiling HTML Help 1 file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = substitutionTags.TransformTemplate("Build1xHelpFile.proj", templateFolder,
                            workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        taskRunner.RunProject("Build1xHelpFile.proj", true);
                        
                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0)
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

                    currentFormat = HelpFileFormats.MSHelpViewer;

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
                        substitutionTags.TransformTemplate("HelpContentSetup.msha", templateFolder, workingFolder);

                        // Rename the content setup file to use the help filename to keep them related and
                        // so that multiple output files can be sent to the same output folder.
                        File.Move(workingFolder + "HelpContentSetup.msha", workingFolder + this.ResolvedHtmlHelpName + ".msha");

                        // Generate the example install and remove scripts
                        substitutionTags.TransformTemplate("InstallMSHC.bat", templateFolder, workingFolder);
                        File.Move(workingFolder + "InstallMSHC.bat", workingFolder + "Install_" +
                            this.ResolvedHtmlHelpName + ".bat");

                        substitutionTags.TransformTemplate("RemoveMSHC.bat", templateFolder, workingFolder);
                        File.Move(workingFolder + "RemoveMSHC.bat", workingFolder + "Remove_" +
                            this.ResolvedHtmlHelpName + ".bat");

                        // Copy the launcher utility
                        File.Copy(ComponentUtilities.ToolsFolder + "HelpLibraryManagerLauncher.exe",
                            workingFolder + "HelpLibraryManagerLauncher.exe");
                        File.SetAttributes(workingFolder + "HelpLibraryManagerLauncher.exe", FileAttributes.Normal);

                        scriptFile = substitutionTags.TransformTemplate("BuildHelpViewerFile.proj", templateFolder,
                            workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        taskRunner.RunProject("BuildHelpViewerFile.proj", true);

                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormats.Website) != 0)
                {
                    // Generate the table of contents and set the default topic
                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents,
                        "Generating website table of contents file...");

                    currentFormat = HelpFileFormats.Website;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        // It got created in the ExtractingHtmlInfo step above
                        // so there is actually nothing to do here.

                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.GenerateWebsite();
                }

                if((project.HelpFileFormat & HelpFileFormats.OpenXml) != 0)
                {
                    // The following build steps are executed to allow plug-ins to handle any necessary processing
                    // but nothing actually happens here:
                    //
                    //      BuildStep.GenerateHelpFormatTableOfContents
                    //      BuildStep.GenerateHelpProject
                    //
                    // For the Open XML format, there is no project file to compile and the TOC layout is
                    // generated when the document is opened.  All of the necessary TOC info is stored in the
                    // intermediate TOC file generated prior to building the topics.  The process used to merge
                    // the topics into a single document uses it to define the order in which the topics are
                    // combined.

                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents, "Executing informational " +
                        "Generate Table of Contents build step for plug-ins (not used for Open XML)");

                    currentFormat = HelpFileFormats.OpenXml;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.ReportProgress(BuildStep.GenerateHelpProject, "Executing informational Generate Help " +
                        "Project build step for plug-ins (not used for Open XML)");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Build the Open XML document
                    this.ReportProgress(BuildStep.CompilingHelpFile, "Generating Open XML document file...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = substitutionTags.TransformTemplate("BuildOpenXmlFile.proj", templateFolder,
                            workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        taskRunner.RunProject("BuildOpenXmlFile.proj", true);

                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
                }

                if((project.HelpFileFormat & HelpFileFormats.Markdown) != 0)
                {
                    // The following build steps are executed to allow plug-ins to handle any necessary processing
                    // but nothing actually happens here:
                    //
                    //      BuildStep.GenerateHelpFormatTableOfContents
                    //      BuildStep.GenerateHelpProject
                    //
                    // For the Markdown format, there is no project file to compile and the TOC layout is
                    // generated by the build task.  All of the necessary TOC info is stored in the intermediate
                    // TOC file generated prior to building the topics.  The build task uses it to find the
                    // topics to finalize and generate the sidebar TOC file.
                    this.ReportProgress(BuildStep.GenerateHelpFormatTableOfContents, "Executing informational " +
                        "Generate Table of Contents build step for plug-ins (not used for Markdown)");

                    currentFormat = HelpFileFormats.Markdown;

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    this.ReportProgress(BuildStep.GenerateHelpProject, "Executing informational Generate Help " +
                        "Project build step for plug-ins (not used for Markdown)");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        this.ExecutePlugIns(ExecutionBehaviors.Before);
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }

                    // Generate the markdown content
                    this.ReportProgress(BuildStep.CompilingHelpFile, "Generating markdown content...");

                    if(!this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                    {
                        scriptFile = substitutionTags.TransformTemplate("GenerateMarkdownContent.proj",
                            templateFolder, workingFolder);

                        this.ExecutePlugIns(ExecutionBehaviors.Before);

                        taskRunner.RunProject("GenerateMarkdownContent.proj", true);

                        this.GatherBuildOutputFilenames();
                        this.ExecutePlugIns(ExecutionBehaviors.After);
                    }
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
                TimeSpan runtime = DateTime.Now - buildStart;

                this.ReportProgress(BuildStep.Completed, "\r\nBuild completed successfully at {0}.  " +
                    "Total time: {1:00}:{2:00}:{3:00.0000}\r\n", DateTime.Now, Math.Floor(runtime.TotalSeconds / 3600),
                    Math.Floor((runtime.TotalSeconds % 3600) / 60), (runtime.TotalSeconds % 60));

                System.Diagnostics.Debug.WriteLine("Build process finished successfully\r\n");
            }
            catch(OperationCanceledException )
            {
                buildCancelling = true;

                this.ReportError(BuildStep.Canceled, "BE0064", "BUILD CANCELLED BY USER");

                System.Diagnostics.Debug.WriteLine("Build process aborted\r\n");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                var agEx = ex as AggregateException;

                if(agEx != null)
                    foreach(var inEx in agEx.InnerExceptions)
                    {
                        if(message != null)
                            message += "\r\n\r\n";

                        message += inEx.Message + "\r\n" + inEx.StackTrace;
                    }

                var bex = ex as BuilderException;

                do
                {
                    if(message != null)
                        message += "\r\n\r\n";

                    message += ex.Message + "\r\n" + ex.StackTrace;
                    ex = ex.InnerException;

                } while(ex != null);

                // NOTE: Message may contain format markers so pass it as a format argument
                if(bex != null)
                    this.ReportError(BuildStep.Failed, bex.ErrorCode, "{0}", message);
                else
                    this.ReportError(BuildStep.Failed, "BE0065", "BUILD FAILED: {0}", message);

                System.Diagnostics.Debug.WriteLine("Build process failed\r\n");
            }
            finally
            {
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

                    if(componentContainer != null)
                        componentContainer.Dispose();
                }
                catch(Exception ex)
                {
                    // Not much we can do at this point...
                    this.ReportProgress(ex.ToString());
                }
                finally
                {
                    if(swLog != null)
                    {
                        swLog.WriteLine("</buildStep>\r\n</shfbBuild>");
                        swLog.Close();
                        swLog = null;
                    }

                    // If we created a copy of the project, dispose of it and return to the original
                    if(originalProject != null)
                    {
                        project.Dispose();
                        project = originalProject;
                    }

                    if(this.CurrentBuildStep == BuildStep.Completed && !project.KeepLogFile)
                        File.Delete(this.LogFilename);
                }
            }
        }
        #endregion

        #region Progress reporting methods
        //=====================================================================

        /// <summary>
        /// This is used to report progress during the build process within the current step
        /// </summary>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        /// <overloads>This method has two overloads.</overloads>
        public void ReportProgress(string message, params object[] args)
        {
            this.ReportProgress(this.CurrentBuildStep, message, args);
        }

        /// <summary>
        /// This is used to report an error that will abort the build
        /// </summary>
        /// <param name="step">The current build step</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        /// <remarks>This just reports the error.  The caller must abort the build</remarks>
        private void ReportError(BuildStep step, string errorCode, string message, params object[] args)
        {
            string errorMessage = String.Format(CultureInfo.CurrentCulture, message, args);

            this.ReportProgress(step, "\r\nSHFB: Error {0}: {1}\r\n", errorCode, errorMessage);
        }

        /// <summary>
        /// This is used to report a warning that may need attention
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        public void ReportWarning(string warningCode, string message, params object[] args)
        {
            string warningMessage = String.Format(CultureInfo.CurrentCulture, message, args);

            this.ReportProgress(this.CurrentBuildStep, "SHFB: Warning {0}: {1}", warningCode, warningMessage);
        }

        /// <summary>
        /// This is used to report progress during the build process and possibly update the current step
        /// </summary>
        /// <param name="step">The current build step</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        protected void ReportProgress(BuildStep step, string message, params object[] args)
        {
            BuildProgressEventArgs pa;
            TimeSpan runtime;

            if(this.CancellationToken != CancellationToken.None && !buildCancelling)
                this.CancellationToken.ThrowIfCancellationRequested();

            bool stepChanged = (this.CurrentBuildStep != step);

            if(stepChanged)
            {
                // Don't bother reporting elapsed time for the initialization steps
                if(step > BuildStep.GenerateSharedContent)
                {
                    runtime = DateTime.Now - stepStart;

                    pa = new BuildProgressEventArgs(this.CurrentBuildStep, false,
                        String.Format(CultureInfo.CurrentCulture, "    Last step completed in " +
                        "{0:00}:{1:00}:{2:00.0000}", Math.Floor(runtime.TotalSeconds / 3600),
                        Math.Floor((runtime.TotalSeconds % 3600) / 60), (runtime.TotalSeconds % 60)));

                    if(swLog != null)
                        swLog.WriteLine(pa.Message);

                    if(this.ProgressReportProvider != null)
                        this.ProgressReportProvider.Report(pa);
                }

                if(this.ProgressReportProvider != null)
                    this.ProgressReportProvider.Report(new BuildProgressEventArgs(this.CurrentBuildStep, false,
                        "-------------------------------"));

                stepStart = DateTime.Now;
                this.CurrentBuildStep = step;

                if(swLog != null)
                    swLog.WriteLine("</buildStep>\r\n<buildStep step=\"{0}\">", step);
            }

            pa = new BuildProgressEventArgs(this.CurrentBuildStep, stepChanged,
                String.Format(CultureInfo.CurrentCulture, message, args));

            // Save the message to the log file
            if(swLog != null)
                swLog.WriteLine(HttpUtility.HtmlEncode(pa.Message));

            if(this.ProgressReportProvider != null)
                this.ProgressReportProvider.Report(pa);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Make sure the path isn't one the user would regret having nuked without warning
        /// </summary>
        /// <param name="propertyName">The name of the path property</param>
        /// <param name="propertyValue">It's current value</param>
        /// <param name="projectPath">The path to the current project</param>
        /// <remarks>Since most people don't read the help file and also ignore the warning in the property grid
        /// description pane, we'll take some steps to idiot-proof the dangerous path properties.  I'm starting
        /// to lose count of the number of people that point WorkingPath at the root of their C:\ drive and
        /// wonder why all their files disappear.
        /// 
        /// <p/>Paths checked for include root references to hard drives and network shares, most common
        /// well-known folders, and the project's root folder.</remarks>
        /// <exception cref="BuilderException">This is thrown if the path is one of the ones that probably should
        /// not be used.</exception>
        public static void VerifySafePath(string propertyName, string propertyValue, string projectPath)
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
                // While the path can be under the project path, it shouldn't match the project path or be its
                // parent.
                if(FolderPath.TerminatePath(projectPath).StartsWith(FolderPath.TerminatePath(tempPath),
                  StringComparison.OrdinalIgnoreCase))
                    isBadPath = true;

                if(tempPath[0] == '\\' && tempPath[1] == '\\')
                {
                    // UNC path.  Make sure it has more than just a share after the server name.
                    tempPath = tempPath.Substring(2);

                    pos = tempPath.IndexOf('\\');

                    if(pos != -1)
                        pos = tempPath.IndexOf('\\', pos + 1);

                    // This isn't perfect as the actual root of the share may be several folders down.  You
                    // can't have it all.
                    if(pos == -1)
                        isBadPath = true;
                }
                else
                {
                    // Fixed drive.  Make sure it isn't one of the well-known folders.  Some of these contain
                    // the same folders but we'll err on the side of caution and check them anyway.
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.System));

                    // Don't allow the folder, a subfolder, or parent
                    foreach(string path in specialPaths)
                        if(!String.IsNullOrEmpty(path) &&
                          (path.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase) ||
                          tempPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                        {
                            isBadPath = true;
                            break;
                        }

                    // We'll allow subfolders under Desktop, My Documents, Personal, Application Data, and Local
                    // Application Data folders as that's a common occurrence.  Again, not perfect but...
                    specialPaths.Clear();
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                    specialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

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
                throw new BuilderException("BE0034", String.Format(CultureInfo.CurrentCulture,
                    "The '{0}' property resolved to '{1}' which is a reserved folder name that cannot be used " +
                    "for build output or as the working files folder.  See the error or property topic in the " +
                    "help file for more details.", propertyName, propertyValue));
        }

        /// <summary>
        /// This is used to gather a list of files produced by the build
        /// </summary>
        private void GatherBuildOutputFilenames()
        {
            string[] patterns = new string[4];

            switch(currentFormat)
            {
                case HelpFileFormats.HtmlHelp1:
                    patterns[0] = this.ResolvedHtmlHelpName + "*.chm";
                    break;

                case HelpFileFormats.MSHelpViewer:
                    patterns[0] = this.ResolvedHtmlHelpName + "*.msh?";
                    patterns[1] = "Install_" + this.ResolvedHtmlHelpName + "*.bat";
                    patterns[2] = "Remove_" + this.ResolvedHtmlHelpName + "*.bat";
                    patterns[3] = "HelpLibraryManagerLauncher.exe";
                    break;

                case HelpFileFormats.OpenXml:
                    patterns[0] = this.ResolvedHtmlHelpName + "*.docx";
                    break;

                default:    // Website and markdown
                    patterns[0] = "*.*";
                    break;
            }

            foreach(string filePattern in patterns)
            {
                if(filePattern == null)
                    continue;

                foreach(string file in Directory.EnumerateFiles(outputFolder, filePattern, SearchOption.AllDirectories))
                {
                    if(file.StartsWith(workingFolder, StringComparison.OrdinalIgnoreCase) ||
                      file == project.LogFileLocation)
                        continue;

                    switch(currentFormat)
                    {
                        case HelpFileFormats.HtmlHelp1:
                            help1Files.Add(file);
                            break;

                        case HelpFileFormats.MSHelpViewer:
                            helpViewerFiles.Add(file);
                            break;

                        case HelpFileFormats.OpenXml:
                            openXmlFiles.Add(file);
                            break;

                        case HelpFileFormats.Markdown:
                            markdownFiles.Add(file);
                            break;

                        default:    // Website
                            // Open XML and Markdown are distinct and cannot be combined with web output so
                            // there's no need to exclude them here.
                            if(!help1Files.Contains(file) && !helpViewerFiles.Contains(file))
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
        /// <exception cref="BuilderException">This is thrown if any of the tools cannot be found</exception>
        protected void FindTools()
        {
            this.ReportProgress("Finding tools...");
            this.ExecutePlugIns(ExecutionBehaviors.Before);

            this.ReportProgress("The Sandcastle tools are located in '{0}'", ComponentUtilities.ToolsFolder);

            // Find the help compilers by looking on all fixed drives but only if the related format is used
            if((project.HelpFileFormat & HelpFileFormats.HtmlHelp1) != 0)
            {
                hhcFolder = project.HtmlHelp1xCompilerPath;

                if(hhcFolder.Length == 0)
                {
                    this.ReportProgress("Searching for HTML Help 1 compiler...");
                    hhcFolder = BuildProcess.FindOnFixedDrives(@"\HTML Help Workshop");
                }

                if(hhcFolder.Length == 0 || !Directory.Exists(hhcFolder))
                    throw new BuilderException("BE0037", "Could not find the path to the HTML Help 1 " +
                        "compiler. See the error number topic in the help file for details.\r\n");

                if(hhcFolder[hhcFolder.Length - 1] != '\\')
                    hhcFolder += @"\";

                this.ReportProgress("Found HTML Help 1 compiler in '{0}'", hhcFolder);
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// Find a folder by searching the Program Files folders on all fixed drives.
        /// </summary>
        /// <param name="path">The path for which to search</param>
        /// <returns>The path if found or an empty string if not found</returns>
        protected internal static string FindOnFixedDrives(string path)
        {
            // Check for a 64-bit process.  The tools will be in the x86 folder.  If running as a 32-bit process,
            // the folder will contain "(x86)" already if on a 64-bit OS.
            StringBuilder sb = new StringBuilder(Environment.GetFolderPath(Environment.Is64BitProcess ?
                Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles));

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
        /// This is used to find the named executable in one of the Visual Studio SDK installation folders.
        /// </summary>
        /// <param name="exeName">The name of the executable to find</param>
        /// <returns>The path if found or an empty string if not found</returns>
        /// <remarks>The search looks in all "*Visual*SDK*" folders under the Program Files special folder on all
        /// fixed drives.</remarks>
        protected internal static string FindSdkExecutable(string exeName)
        {
            // Check for a 64-bit process.  The tools will be in the x86 folder.  If running as a 32-bit process,
            // the folder will contain "(x86)" already if on a 64-bit OS.
            StringBuilder sb = new StringBuilder(Environment.GetFolderPath(Environment.Is64BitProcess ?
                Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles));
            string folder;

            foreach(DriveInfo di in DriveInfo.GetDrives())
                if(di.DriveType == DriveType.Fixed)
                {
                    sb[0] = di.Name[0];
                    folder = sb.ToString();

                    if(!Directory.Exists(folder))
                        continue;

                    foreach(string dir in Directory.EnumerateDirectories(folder, "*Visual*SDK*"))
                    {
                        // If more than one, sort them and take the last one as it should be the most recent.
                        var file = Directory.EnumerateFiles(dir, exeName, SearchOption.AllDirectories).OrderBy(
                            f => f).LastOrDefault();

                        if(file != null)
                            return Path.GetDirectoryName(file);
                    }
                }

            return String.Empty;
        }
        #endregion

        #region Validate documentation sources
        //=====================================================================

        /// <summary>
        /// Validate the documentation source information and copy the files to the working folder
        /// </summary>
        /// <exception cref="BuilderException">This is thrown if any of the information is invalid</exception>
        private void ValidateDocumentationSources()
        {
            List<string> commentsList = new List<string>();
            Dictionary<string, MSBuildProject> projectDictionary = new Dictionary<string, MSBuildProject>();
            HashSet<Tuple<string, string>> targetFrameworksSeen = new HashSet<Tuple<string, string>>();
            PackageReferenceResolver packageReferenceResolver = new PackageReferenceResolver(this);

            MSBuildProject projRef;
            XPathDocument testComments;
            XPathNavigator navComments;
            int fileCount;
            string workingPath, lastSolution;

            this.ReportProgress(BuildStep.ValidatingDocumentationSources,
                "Validating and copying documentation source information");

            // If the current project is part of a solution, use it for the solution macros
            lastSolution = project.MSBuildProject.GetPropertyValue("SolutionPath");

            if(lastSolution != null && lastSolution.Equals("*Undefined*", StringComparison.OrdinalIgnoreCase))
                lastSolution = null;

            assembliesList = new Collection<string>();
            referenceDictionary = new Dictionary<string, Tuple<string, string, List<KeyValuePair<string, string>>>>();
            commentsFiles = new XmlCommentsFileCollection();

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            // It's possible a plug-in might want to add or remove assemblies so we'll run them before checking
            // to see if the project has any.
            this.ExecutePlugIns(ExecutionBehaviors.Before);

            if(project.DocumentationSources.Count() == 0)
                throw new BuilderException("BE0039", "The project does not have any documentation sources defined");

            // Clone the project's references.  These will be added to a build project later on so we'll note the
            // necessary information needed to create the reference in the future project.
            foreach(string refType in (new string[] { "Reference", "COMReference" }))
                foreach(ProjectItem reference in project.MSBuildProject.GetItems(refType))
                    referenceDictionary.Add(reference.EvaluatedInclude, Tuple.Create(reference.ItemType,
                        reference.EvaluatedInclude, reference.Metadata.Select(m =>
                            new KeyValuePair<string, string>(m.Name, m.EvaluatedValue)).ToList()));

            // Convert project references to regular references that point to the output assembly.  Project
            // references get built and we may not have enough info for that to happen successfully.  As such,
            // we'll assume the project has already been built and that its target exists.
            foreach(ProjectItem reference in project.MSBuildProject.GetItems("ProjectReference"))
            {
                // Ignore references used only for MSBuild dependency determination
                var refOutput = reference.GetMetadata(BuildItemMetadata.ReferenceOutputAssembly);

                if(refOutput != null && refOutput.EvaluatedValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    this.ReportProgress("Ignoring reference to '{0}' which is only used for MSBuild dependency " +
                        "determination", reference.EvaluatedInclude);
                    continue;
                }

                using(projRef = new MSBuildProject(reference.EvaluatedInclude))
                {
                    // .NET 4.5 supports a property that tells MSBuild to put the project output into a
                    // project-specific folder in OutDir.
                    var projectSpecificFolder = project.MSBuildProject.AllEvaluatedProperties.FirstOrDefault(
                        p => p.Name == "GenerateProjectSpecificOutputFolder");

                    bool usesProjectSpecificOutput = (projectSpecificFolder != null &&
                      !String.IsNullOrWhiteSpace(projectSpecificFolder.EvaluatedValue) &&
                      Convert.ToBoolean(projectSpecificFolder.EvaluatedValue, CultureInfo.InvariantCulture));

                    projRef.SetConfiguration(project.Configuration, project.Platform, project.MSBuildOutDir,
                        usesProjectSpecificOutput);

                    referenceDictionary.Add(projRef.AssemblyName, Tuple.Create("Reference",
                        Path.GetFileNameWithoutExtension(projRef.AssemblyName),
                        (new [] { new KeyValuePair<string, string>("HintPath", projRef.AssemblyName) }).ToList()));
                }
            }

            try
            {
                // For each source, make three passes: one for projects, one for assemblies and one for comments
                // files.  Projects and comments files are optional but when all done, at least one assembly must
                // have been found.
                foreach(DocumentationSource ds in project.DocumentationSources)
                {
                    fileCount = 0;

                    this.ReportProgress("Source: {0}", ds.SourceFile);

                    foreach(var sourceProject in ds.Projects(
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

                            // .NET 4.5 supports a property that tells MSBuild to put the project output into a
                            // project-specific folder in OutDir.
                            var projectSpecificFolder = project.MSBuildProject.AllEvaluatedProperties.FirstOrDefault(
                                p => p.Name == "GenerateProjectSpecificOutputFolder");

                            bool usesProjectSpecificOutput = (projectSpecificFolder != null &&
                              !String.IsNullOrWhiteSpace(projectSpecificFolder.EvaluatedValue) &&
                              Convert.ToBoolean(projectSpecificFolder.EvaluatedValue, CultureInfo.InvariantCulture));

                            projRef = new MSBuildProject(sourceProject.ProjectFileName);

                            // Use the project file configuration and platform properties if they are set.  If not,
                            // use the documentation source values.  If they are not set, use the SHFB project settings.
                            projRef.SetConfiguration(
                                !String.IsNullOrEmpty(sourceProject.Configuration) ? sourceProject.Configuration :
                                    !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : project.Configuration,
                                !String.IsNullOrEmpty(sourceProject.Platform) ? sourceProject.Platform :
                                    !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : project.Platform,
                                project.MSBuildOutDir, usesProjectSpecificOutput);

                            // Add Visual Studio solution macros if necessary
                            if(!String.IsNullOrWhiteSpace(lastSolution))
                                projRef.SetSolutionMacros(lastSolution);

                            projectDictionary.Add(sourceProject.ProjectFileName, projRef);
                        }
                        else
                            this.ReportProgress("    Ignoring duplicate project file '{0}'", sourceProject.ProjectFileName);

                        fileCount++;
                    }

                    foreach(string asmName in ds.Assemblies)
                    {
                        if(!assembliesList.Contains(asmName))
                        {
                            // Assemblies are parsed in place by MRefBuilder so we don't have to do anything with
                            // them here.
                            this.ReportProgress("    Found assembly '{0}'", asmName);
                            assembliesList.Add(asmName);
                        }
                        else
                            this.ReportProgress("    Ignoring duplicate assembly file '{0}'", asmName);

                        fileCount++;
                    }

                    foreach(string commentsName in ds.CommentsFiles)
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
                        this.ReportWarning("BE0006", "Unable to locate any documentation sources for '{0}' " +
                            "(Configuration: {1} Platform: {2})", ds.SourceFile,
                            !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : project.Configuration,
                            !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : project.Platform);
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

                            if(!assembliesList.Contains(workingPath))
                            {
                                // Assemblies are parsed in place by MRefBuilder so we don't have to do anything
                                // with them here.
                                this.ReportProgress("    Found assembly '{0}'", workingPath);
                                assembliesList.Add(workingPath);
                            }
                            else
                                this.ReportProgress("    Ignoring duplicate assembly file '{0}'", workingPath);
                        }
                        else
                            throw new BuilderException("BE0067", String.Format(CultureInfo.CurrentCulture,
                                "Unable to obtain assembly name from project file '{0}' using Configuration " +
                                "'{1}', Platform '{2}'", msbProject.ProjectFile.FullPath,
                                msbProject.ProjectFile.AllEvaluatedProperties.Last(
                                    p => p.Name == BuildItemMetadata.Configuration).EvaluatedValue,
                                msbProject.ProjectFile.AllEvaluatedProperties.Last(
                                    p => p.Name == BuildItemMetadata.Platform).EvaluatedValue));

                        workingPath = msbProject.XmlCommentsFile;

                        if(!String.IsNullOrEmpty(workingPath))
                        {
                            if(!File.Exists(workingPath))
                                throw new BuilderException("BE0041",
                                    "Project XML comments file does not exist: " + workingPath);

                            if(!commentsList.Contains(workingPath))
                            {
                                // These are handled below
                                commentsList.Add(workingPath);
                            }
                            else
                                this.ReportProgress("    Ignoring duplicate comments file '{0}'", workingPath);
                        }

                        // Note the platforms seen and the highest framework version used
                        targetFrameworksSeen.Add(Tuple.Create(msbProject.TargetFrameworkIdentifier,
                            msbProject.TargetFrameworkVersion));

                        // Clone the project's reference information
                        msbProject.CloneReferenceInfo(packageReferenceResolver, referenceDictionary);
                    }

                    // If we saw multiple framework types in the projects, stop now.  Due to the different
                    // assemblies used, we cannot mix the project types within the same SHFB project.  They will
                    // need to be documented separately and can be merged using the Version Builder plug-in if
                    // needed.
                    if(!PlatformType.PlatformsAreCompatible(targetFrameworksSeen.Select(t => t.Item1)))
                        throw new BuilderException("BE0070", "Differing framework types were detected in the " +
                            "documentation sources (i.e. .NET, Silverlight, Portable).  Due to the different " +
                            "sets of assemblies used, the different frameworks cannot be mixed within the same " +
                            "documentation project.  See the error number topic in the help file for details.");

                    // Find the best matching set of framework reflection data
                    var projectFramework = reflectionDataDictionary.BestMatchFor(targetFrameworksSeen);

                    if(frameworkReflectionData != projectFramework)
                    {
                        // If redirected and no suitable version was found, we can't go any further
                        if(projectFramework == null)
                            throw new BuilderException("BE0073", String.Format(CultureInfo.CurrentCulture,
                                "A project with a different or higher framework version was found but that " +
                                "version or a suitable redirected version was not found on this " +
                                "system.  The build cannot continue.  Project framework versions: {0}",
                                String.Join(", ", targetFrameworksSeen.Select(f => f.Item1 + " " + f.Item2))));

                        if(frameworkReflectionData.Platform != PlatformType.DotNetStandard)
                            this.ReportWarning("BE0007", "A project with a different or higher framework version " +
                                "was found.  Changing project FrameworkVersion property from '{0}' to '{1}' for " +
                                "the build.", project.FrameworkVersion, projectFramework.Title);

                        project.FrameworkVersion = projectFramework.Title;
                        frameworkReflectionData = projectFramework;
                    }
                }
            }
            finally
            {
                // Dispose of any MSBuild projects that we loaded
                foreach(var p in projectDictionary.Values)
                    p.Dispose();
            }

            if(assembliesList.Count == 0)
                throw new BuilderException("BE0042", "You must specify at least one documentation source in " +
                    "the form of an assembly or a Visual Studio solution/project file");

            // Log the references found, if any
            if(referenceDictionary.Count != 0)
            {
                this.ReportProgress("\r\nReferences to include (excluding framework assemblies):");

                string[] keys = new string[referenceDictionary.Keys.Count];
                referenceDictionary.Keys.CopyTo(keys, 0);
                Array.Sort(keys);

                // Filter out references related to the framework.  MRefBuilder will resolve these
                // automatically.
                foreach(string key in keys)
                    if(frameworkReflectionData.ContainsAssembly(key))
                        referenceDictionary.Remove(key);
                    else
                        this.ReportProgress("    {0}", key);

                if(referenceDictionary.Count == 0)
                    this.ReportProgress("    None");
            }

            if(commentsList.Count != 0)
                this.ReportProgress("\r\nCopying XML comments files");

            // XML comments files are copied to the working folder in case they need to be fixed up
            foreach(string commentsName in commentsList)
            {
                workingPath = workingFolder + Path.GetFileName(commentsName);

                // Warn if there is a duplicate and copy the comments file to a unique name to preserve its
                // content.
                if(File.Exists(workingPath))
                {
                    workingPath = workingFolder + Guid.NewGuid().ToString("B");

                    this.ReportWarning("BE0063", "'{0}' matches a previously copied comments filename.  The " +
                        "duplicate will be copied to a unique name to preserve the comments it contains.",
                        commentsName);
                }

                try
                {
                    // Not all XML files found may be comments files.  Ignore those that are not.
                    testComments = new XPathDocument(commentsName);
                    navComments = testComments.CreateNavigator();

                    if(navComments.SelectSingleNode("doc/members") == null)
                    {
                        this.ReportWarning("BE0005", "File '{0}' does not contain a 'doc/members' node and " +
                            "will not be used as an XML comments file.", commentsName);
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    this.ReportWarning("BE0061", "File '{0}' could not be loaded and will not be used as an " +
                        "XML comments file.  Error: {1}", commentsName, ex.Message);
                    continue;
                }

                File.Copy(commentsName, workingPath, true);
                File.SetAttributes(workingPath, FileAttributes.Normal);

                // Add the file to the XML comments file collection
                commentsFiles.Add(new XmlCommentsFile(workingPath));

                this.ReportProgress("    {0} -> {1}", commentsName, workingPath);
            }

            if(commentsFiles.Count == 0)
                this.ReportWarning("BE0062", "No documentation source XML comments files found.  The help " +
                    "file will not contain any member comments.");

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }
        #endregion
    }
}
