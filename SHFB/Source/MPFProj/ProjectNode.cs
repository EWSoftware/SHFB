/********************************************************************************************

Copyright (c) Microsoft Corporation 
All rights reserved. 

Microsoft Public License: 

This license governs use of the accompanying software. If you use the software, you 
accept this license. If you do not accept the license, do not use the software. 

1. Definitions 
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the 
same meaning here as under U.S. copyright law. 
A "contribution" is the original software, or any additions or changes to the software. 
A "contributor" is any person that distributes its contribution under this license. 
"Licensed patents" are a contributor's patent claims that read directly on its contribution. 

2. Grant of Rights 
(A) Copyright Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free copyright license to reproduce its contribution, prepare derivative works of 
its contribution, and distribute its contribution or any derivative works that you create. 
(B) Patent Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free license under its licensed patents to make, have made, use, sell, offer for 
sale, import, and/or otherwise dispose of its contribution in the software or derivative 
works of the contribution in the software. 

3. Conditions and Limitations 
(A) No Trademark License- This license does not grant you rights to use any contributors' 
name, logo, or trademarks. 
(B) If you bring a patent claim against any contributor over patents that you claim are 
infringed by the software, your patent license from such contributor to the software ends 
automatically. 
(C) If you distribute any portion of the software, you must retain all copyright, patent, 
trademark, and attribution notices that are present in the software. 
(D) If you distribute any portion of the software in source code form, you may do so only 
under this license by including a complete copy of this license with your distribution. 
If you distribute any portion of the software in compiled or object code form, you may only 
do so under a license that complies with this license. 
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give 
no express warranties, guarantees or conditions. You may have additional consumer rights 
under your local laws which this license cannot change. To the extent permitted under your 
local laws, the contributors exclude the implied warranties of merchantability, fitness for 
a particular purpose and non-infringement.

********************************************************************************************/

//===============================================================================================================
// File    : ProjectNode.cs
// Updated : 02/15/2012
// Modifier: Eric Woodruff  (Eric@EWoodruff.us)
//
// This file has been modified to support "Add as Link" and "Show All Files" in the project.  Search for "!EFW"
// to find the changes.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/18/2008  EFW  Added support for linked project files
// 06/20/2008  EFW  Added support for "Show All Files"
// 03/20/2011  EFW  Updated to use MPFProj for VS2010
// 04/17/2011  EFW  Made UpgradeProject() virtual
// 03/20/2012  EFW  Fixed odd bug related to adding new items to deeply nested collapsed nodes.
//===============================================================================================================

// Ignore Spelling: se vsopts winexe zombied langproj retargeted

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Xml;

using EnvDTE;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;
using MSBuild = Microsoft.Build.Evaluation;
using MSBuildConstruction = Microsoft.Build.Construction;
using MSBuildExecution = Microsoft.Build.Execution;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Manages the persistent state of the project (References, options, files, etc.) and deals with user interaction via a GUI in the form a hierarchy.
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(true)]
    public abstract partial class ProjectNode : HierarchyNode,
        IVsGetCfgProvider,
        IVsProject3,
        IVsAggregatableProject,
        IVsProjectFlavorCfgProvider,
        IPersistFileFormat,
        IVsProjectBuildSystem,
        IVsBuildPropertyStorage,
        IVsComponentUser,
        IVsDependencyProvider,
        IVsSccProject2,
        IBuildDependencyUpdate,
        IProjectEventsListener,
        IProjectEventsProvider,
        IReferenceContainerProvider,
        IVsProjectSpecialFiles,
        IVsProjectUpgrade,
        IVsDesignTimeAssemblyResolution,
        IVsSetTargetFrameworkWorkerCallback
    {
        #region nested types

        public enum ImageName
        {
            OfflineWebApp = 0,
            WebReferencesFolder = 1,
            OpenReferenceFolder = 2,
            ReferenceFolder = 3,
            Reference = 4,
            SDLWebReference = 5,
            DISCOWebReference = 6,
            Folder = 7,
            OpenFolder = 8,
            ExcludedFolder = 9,
            OpenExcludedFolder = 10,
            ExcludedFile = 11,
            DependentFile = 12,
            MissingFile = 13,
            WindowsForm = 14,
            WindowsUserControl = 15,
            WindowsComponent = 16,
            XMLSchema = 17,
            XMLFile = 18,
            WebForm = 19,
            WebService = 20,
            WebUserControl = 21,
            WebCustomUserControl = 22,
            ASPPage = 23,
            GlobalApplicationClass = 24,
            WebConfig = 25,
            HTMLPage = 26,
            StyleSheet = 27,
            ScriptFile = 28,
            TextFile = 29,
            SettingsFile = 30,
            Resources = 31,
            Bitmap = 32,
            Icon = 33,
            Image = 34,
            ImageMap = 35,
            XWorld = 36,
            Audio = 37,
            Video = 38,
            CAB = 39,
            JAR = 40,
            DataEnvironment = 41,
            PreviewFile = 42,
            DanglingReference = 43,
            XSLTFile = 44,
            Cursor = 45,
            AppDesignerFolder = 46,
            Data = 47,
            Application = 48,
            DataSet = 49,
            PFX = 50,
            SNK = 51,
            NuGet = 52,

            ImageLast = 52
        }

        /// <summary>
        /// Flags for specifying which events to stop triggering.
        /// </summary>
        [Flags]
        internal enum EventTriggering
        {
            TriggerAll = 0,
            DoNotTriggerHierarchyEvents = 1,
            DoNotTriggerTrackerEvents = 2
        }

        #endregion

        #region constants
        /// <summary>
        /// The user file extension.
        /// </summary>
        internal const string PerUserFileExtension = ".user";
     
        private Guid GUID_MruPage = new Guid("{19B97F03-9594-4c1c-BE28-25FF030113B3}");

        /// <summary>
        /// The VS command that allows projects to open Windows Explorer to the project directory.
        /// </summary>
        private const VsCommands2K ExploreFolderInWindowsCommand = (VsCommands2K)1635;
        
        #endregion

        #region fields

        private static readonly FrameworkName DefaultTargetFrameworkMoniker = new FrameworkName(".NETFramework", new Version(4, 8));

        private static Guid addComponentLastActiveTab = VSConstants.GUID_SolutionPage;

        private static uint addComponentDialogSizeX = 0;

        private static uint addComponentDialogSizeY = 0;

        /// <summary>
        /// List of output groups names and their associated target
        /// </summary>
        private static readonly KeyValuePair<string, string>[] outputGroupNames =
        {                                      // Name                    Target (MSBuild)
            new KeyValuePair<string, string>("Built",                 "BuiltProjectOutputGroup"),
            new KeyValuePair<string, string>("ContentFiles",          "ContentFilesProjectOutputGroup"),
            new KeyValuePair<string, string>("LocalizedResourceDlls", "SatelliteDllsProjectOutputGroup"),
            new KeyValuePair<string, string>("Documentation",         "DocumentationProjectOutputGroup"),
            new KeyValuePair<string, string>("Symbols",               "DebugSymbolsProjectOutputGroup"),
            new KeyValuePair<string, string>("SourceFiles",           "SourceFilesProjectOutputGroup"),
            new KeyValuePair<string, string>("XmlSerializer",         "SGenFilesOutputGroup"),
        };

        /// <summary>A project will only try to build if it can obtain a lock on this object</summary>
        private static readonly object BuildLock = new object();

        /// <summary>Maps integer ids to project item instances</summary>
        private readonly EventSinkCollection itemIdMap = new EventSinkCollection();

        /// <summary>A service provider call back object provided by the IDE hosting the project manager</summary>
        private ServiceProvider site;

        public static ServiceProvider ServiceProvider { get; set; }

        private TrackDocumentsHelper tracker;

        /// <summary>
        /// A cached copy of project options.
        /// </summary>
        private ProjectOptions options;

        /// <summary>
        /// This property returns the time of the last change made to this project.
        /// It is not the time of the last change on the project file, but actually of
        /// the in memory project settings.  In other words, it is the last time that 
        /// SetProjectDirty was called.
        /// </summary>
        private DateTime lastModifiedTime;

        /// <summary>
        /// MSBuild engine we are going to use 
        /// </summary>
        private ProjectCollection buildEngine;

        private Build.Utilities.Logger buildLogger;

        private bool useProvidedLogger;

        private MSBuild.Project buildProject;

        private ProjectInstance currentConfig;

        private DesignTimeAssemblyResolution designTimeAssemblyResolution;

        private ConfigProvider configProvider;

        private TaskProvider taskProvider;

        private string filename;

        private Url baseUri;

        private bool isDirty;

        private bool isNewProject;

        private bool projectOpened;

        private bool buildIsPrepared;

        private ImageHandler imageHandler;

        private string errorString;

        private string warningString;

        private Guid projectIdGuid;

        private bool isClosed;

        private EventTriggering eventTriggeringFlag = EventTriggering.TriggerAll;

        private bool invokeMSBuildWhenResumed;

        private uint suspendMSBuildCounter;

        private bool canFileNodesHaveChilds;

        private bool isProjectEventsListener = true;

        /// <summary>
        /// The build dependency list passed to IVsDependencyProvider::EnumDependencies 
        /// </summary>
        private readonly List<IVsBuildDependency> buildDependencyList = new List<IVsBuildDependency>();

        /// <summary>
        /// Defines if Project System supports Project Designer
        /// </summary>
        private bool supportsProjectDesigner;

        private bool showProjectInSolutionPage = true;

        private bool buildInProcess;

        /// <summary>
        /// Field for determining whether source control should be disabled.
        /// </summary>
        private bool disableScc;

        private string sccProjectName;

        private string sccLocalPath;

        private string sccAuxPath;

        private string sccProvider;

        /// <summary>
        /// Flag for controlling how many times we register with the SCC manager.
        /// </summary>
        private bool isRegisteredWithScc;

        /// <summary>
        /// Flag for controlling query edit should communicate with the SCC manager.
        /// </summary>
        private bool disableQueryEdit;

        /// <summary>
        /// Control if command with potential destructive behavior such as delete should
        /// be enabled for nodes of this project.
        /// </summary>
        private bool canProjectDeleteItems;

        /// <summary>
        /// Token processor used by the project sample.
        /// </summary>
        private TokenProcessor tokenProcessor;

        /// <summary>
        /// Member to store output base relative path. Used by OutputBaseRelativePath property
        /// </summary>
        private string outputBaseRelativePath = "bin";

        private IProjectEvents projectEventsProvider;

        /// <summary>
        /// Used for flavoring to hold the XML fragments
        /// </summary>
        private XmlDocument xmlFragments;

        /// <summary>
        /// Used to map types to CATID. This provide a generic way for us to do this
        /// and make it simpler for a project to provide it's CATIDs for the different type of objects
        /// for which it wants to support extensibility. This also enables us to have multiple
        /// type mapping to the same CATID if we choose to.
        /// </summary>
        private readonly Dictionary<Type, Guid> catidMapping = new Dictionary<Type, Guid>();

        /// <summary>
        /// The internal package implementation.
        /// </summary>
        private ProjectPackage package;

        // Has the object been disposed.
        private bool isDisposed;

        #endregion

        #region abstract properties
        /// <summary>
        /// This Guid must match the Guid you registered under
        /// HKLM\Software\Microsoft\VisualStudio\%version%\Projects.
        /// Among other things, the Project framework uses this 
        /// guid to find your project and item templates.
        /// </summary>
        public abstract Guid ProjectGuid
        {
            get;
        }

        /// <summary>
        /// Returns a caption for VSHPROPID_TypeName.
        /// </summary>
        /// <returns></returns>
        public abstract string ProjectType
        {
            get;
        }
        #endregion

        #region virtual properties
        /// <summary>
        /// This is the project instance guid that is persisted in the project file
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public virtual Guid ProjectIDGuid
        {
            get => this.projectIdGuid;
            set
            {
                if(this.projectIdGuid != value)
                {
                    this.projectIdGuid = value;
                    if(this.buildProject != null)
                    {
                        this.SetProjectProperty("ProjectGuid", this.projectIdGuid.ToString("B"));
                    }
                }
            }
        }
        #endregion

        #region properties

        #region overridden properties
        public override int MenuCommandId => VsMenus.IDM_VS_CTXT_PROJNODE;

        public override string Url => this.GetMkDocument();

        public override string Caption
        {
            get
            {
                // Default to file name
                string caption = this.buildProject.FullPath;

                if (String.IsNullOrEmpty(caption))
                {
                    if (this.buildProject.GetProperty(ProjectFileConstants.Name) != null)
                    {
                        caption = this.buildProject.GetProperty(ProjectFileConstants.Name).EvaluatedValue;
                        if (caption == null || caption.Length == 0)
                        {
                            caption = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
                        }
                    }
                }
                else
                {
                    caption = Path.GetFileNameWithoutExtension(caption);
                }

                return caption;
            }
        }

        public override Guid ItemTypeGuid => this.ProjectGuid;

        public override int ImageIndex => (int)ProjectNode.ImageName.Application;


        #endregion

        #region virtual properties

        public virtual string ErrorString
        {
            get
            {
                if (this.errorString == null)
                {
                    this.errorString = SR.GetString(SR.Error, CultureInfo.CurrentUICulture);
                }

                return this.errorString;
            }
        }

        public virtual string WarningString
        {
            get
            {
                if (this.warningString == null)
                {
                    this.warningString = SR.GetString(SR.Warning, CultureInfo.CurrentUICulture);
                }

                return this.warningString;
            }
        }

        /// <summary>
        /// The target name that will be used for evaluating the project file (i.e., pseudo-builds).
        /// This target is used to trigger a build with when the project system changes. 
        /// Example: The language projects are triggering a build with the Compile target whenever 
        /// the project system changes.
        /// </summary>
        protected internal virtual string ReEvaluateProjectFileTargetName => null;

        /// <summary>
        /// This is the object that will be returned by EnvDTE.Project.Object for this project
        /// </summary>
        protected internal virtual object ProjectObject => null;

        /// <summary>
        /// Override this property to specify when the project file is dirty.
        /// </summary>
        protected virtual bool IsProjectFileDirty
        {
            get
            {
                string document = this.GetMkDocument();

                if (String.IsNullOrEmpty(document))
                {
                    return this.isDirty;
                }

                return (this.isDirty || !File.Exists(document));
            }
        }

        /// <summary>
        /// True if the project uses the Project Designer Editor instead of the property page frame to edit project properties.
        /// </summary>
        protected virtual bool SupportsProjectDesigner
        {
            get => this.supportsProjectDesigner;
            set => this.supportsProjectDesigner = value;

        }

        protected virtual Guid ProjectDesignerEditor => VSConstants.GUID_ProjectDesignerEditor;

        /// <summary>
        /// Defines the flag that supports the VSHPROPID.ShowProjInSolutionPage
        /// </summary>
        protected virtual bool ShowProjectInSolutionPage
        {
            get => this.showProjectInSolutionPage;
            set => this.showProjectInSolutionPage = value;
        }

        #endregion

        /// <summary>
        /// Gets or sets the ability of a project file node to have child nodes (sub items).
        /// Example would be C#/VB forms having .resx and designer files.
        /// </summary>
        protected internal bool CanFileNodesHaveChilds
        {
            get => canFileNodesHaveChilds;
            set => canFileNodesHaveChilds = value;
        }

        /// <summary>
        /// Get and set the Token processor.
        /// </summary>
        public TokenProcessor FileTemplateProcessor
        {
            get
            {
                if(tokenProcessor == null)
                    tokenProcessor = new TokenProcessor();
                return tokenProcessor;
            }
            set => tokenProcessor = value;
        }

        /// <summary>
        /// Gets a service provider object provided by the IDE hosting the project
        /// </summary>
        public IServiceProvider Site => this.site;

        /// <summary>
        /// Gets an ImageHandler for the project node.
        /// </summary>
        public ImageHandler ImageHandler
        {
            get
            {
                if (null == imageHandler)
                {
                    imageHandler = new ImageHandler(typeof(ProjectNode).Assembly.GetManifestResourceStream("Microsoft.VisualStudio.Project.Resources.imagelis.bmp"));
                }
                return imageHandler;
            }
        }

        /// <summary>
        /// This property returns the time of the last change made to this project.
        /// It is not the time of the last change on the project file, but actually of
        /// the in memory project settings.  In other words, it is the last time that 
        /// SetProjectDirty was called.
        /// </summary>
        public DateTime LastModifiedTime => this.lastModifiedTime;

        /// <summary>
        /// Determines whether this project is a new project.
        /// </summary>
        public bool IsNewProject => this.isNewProject;

        /// <summary>
        /// Gets the path to the folder containing the project.
        /// </summary>
        public string ProjectFolder => Path.GetDirectoryName(this.filename);

        /// <summary>
        /// Gets or sets the project filename.
        /// </summary>
        public string ProjectFile
        {
            get => Path.GetFileName(this.filename);
            set => this.SetEditLabel(value);
        }

        /// <summary>
        /// Gets the Base Uniform Resource Identifier (URI).
        /// </summary>
        public Url BaseURI
        {
            get
            {
                if (baseUri == null && this.buildProject != null)
                {
                    string path = Path.GetDirectoryName(this.buildProject.FullPath);

                    // Uri/Url behave differently when you have trailing slash and when you don't
                    if (!path.EndsWith("\\", StringComparison.Ordinal) && !path.EndsWith("/", StringComparison.Ordinal))
                        path += "\\";
                    baseUri = new Url(path);
                }

                Debug.Assert(baseUri != null, "Base URL should not be null. Did you call BaseURI before loading the project?");
                return baseUri;
            }
        }

        /// <summary>
        /// Gets whether or not the project is closed.
        /// </summary>
        public bool IsClosed => this.isClosed;

        /// <summary>
        /// Gets whether or not the project is being built.
        /// </summary>
        public bool BuildInProgress => buildInProcess;

        /// <summary>
        /// Gets or set the relative path to the folder containing the project output. 
        /// </summary>
        public virtual string OutputBaseRelativePath
        {
            get => this.outputBaseRelativePath;
            set
            {
                if(Path.IsPathRooted(value))
                {
                    throw new ArgumentException("Path must not be rooted.");
                }

                this.outputBaseRelativePath = value;
            }
        }

        public FrameworkName TargetFrameworkMoniker
        {
            get
            {
                if (this.options == null)
                {
                    GetProjectOptions();
                }
                if (this.options != null)
                {
                    return this.options.TargetFrameworkMoniker ?? DefaultTargetFrameworkMoniker;
                }
                else
                {
                    return DefaultTargetFrameworkMoniker;
                }
            }

            set
            {
                if (this.options == null)
                {
                    GetProjectOptions();
                }

                if (value == null)
                {
                    value = DefaultTargetFrameworkMoniker;
                }

                if (this.options.TargetFrameworkMoniker != value)
                {
                    this.OnTargetFrameworkMonikerChanged(this.options, this.options.TargetFrameworkMoniker, value);
                }
            }
        }

        /// <summary>
        /// Version of this node as an IVsHierarchy that can be safely passed to native code from a background thread. 
        /// </summary>
        public IVsHierarchy InteropSafeIVsHierarchy
        {
            get;
            protected set;
        }

        /// <summary>
        /// Version of this node as an IVsUIHierarchy that can be safely passed to native code from a background thread. 
        /// </summary>
        public IVsUIHierarchy InteropSafeIVsUIHierarchy
        {
            get;
            protected set;
        }

        /// <summary>
        /// Version of this node as an IVsProject3 that can be safely passed to native code from a background thread. 
        /// </summary>
        public IVsProject3 InteropSafeIVsProject3
        {
            get;
            protected set;
        }

        /// <summary>
        /// Version of this node as an IVsSccProject2 that can be safely passed to native code from a background thread. 
        /// </summary>
        public IVsSccProject2 InteropSafeIVsSccProject2
        {
            get;
            protected set;
        }

        /// <summary>
        /// Version of this node as an IVsUIHierWinClipboardHelperEvents that can be safely passed to native code from a background thread. 
        /// </summary>
        public IVsUIHierWinClipboardHelperEvents InteropSafeIVsUIHierWinClipboardHelperEvents
        {
            get;
            protected set;
        }

        public IVsComponentUser InteropSafeIVsComponentUser
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the flag whether query edit should communicate with the SCC manager.
        /// </summary>
        protected bool DisableQueryEdit
        {
            get => this.disableQueryEdit;
            set => this.disableQueryEdit = value;
        }

        /// <summary>
        /// Gets a collection of integer ids that maps to project item instances
        /// </summary>
        internal EventSinkCollection ItemIdMap => this.itemIdMap;

        /// <summary>
        /// Get the helper object that track document changes.
        /// </summary>
        internal TrackDocumentsHelper Tracker => this.tracker;

        /// <summary>
        /// Gets or sets the build logger.
        /// </summary>
        protected Build.Utilities.Logger BuildLogger
        {
            get => this.buildLogger;
            set
            {
                this.buildLogger = value;
                this.useProvidedLogger = true;
            }
        }

        /// <summary>
        /// Gets the task provider.
        /// </summary>
        protected TaskProvider TaskProvider => this.taskProvider;

        /// <summary>
        /// Gets the project file name.
        /// </summary>
        protected string FileName => this.filename;

        protected bool IsIdeInCommandLineMode
        {
            get
            {
                bool cmdline = false;

                if(this.site.GetService(typeof(SVsShell)) is IVsShell shell)
                {
                    Marshal.ThrowExceptionForHR(shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out object obj));
                    cmdline = (bool)obj;
                }
                return cmdline;
            }
        }

        /// <summary>
        /// Gets the configuration provider.
        /// </summary>
        protected ConfigProvider ConfigProvider
        {
            get
            {
                if (this.configProvider == null)
                {
                    this.configProvider = CreateConfigProvider();
                }

                return this.configProvider;
            }
        }

        /// <summary>
        /// Gets or sets whether or not source code control is disabled for this project.
        /// </summary>
        protected bool IsSccDisabled
        {
            get => this.disableScc;
            set => this.disableScc = value;
        }

        /// <summary>
        /// Gets or set whether items can be deleted for this project.
        /// Enabling this feature can have the potential destructive behavior such as deleting files from disk.
        /// </summary>
        protected internal bool CanProjectDeleteItems
        {
            get => canProjectDeleteItems;
            set => canProjectDeleteItems = value;
        }

        /// <summary>
        /// Determines whether the project was fully opened. This is set when the OnAfterOpenProject has triggered.
        /// </summary>
        protected internal bool HasProjectOpened => this.projectOpened;

        /// <summary>
        /// Gets or sets event triggering flags.
        /// </summary>
        internal EventTriggering EventTriggeringFlag
        {
            get => this.eventTriggeringFlag;
            set => this.eventTriggeringFlag = value;
        }

        /// <summary>
        /// Defines the build project that has loaded the project file.
        /// </summary>
        protected internal MSBuild.Project BuildProject
        {
            get => this.buildProject;
            set => SetBuildProject(value);
        }

        /// <summary>
        /// Gets the current config.
        /// </summary>
        /// <value>The current config.</value>
        protected internal ProjectInstance CurrentConfig => this.currentConfig;

        /// <summary>
        /// Defines the build engine that is used to build the project file.
        /// </summary>
        internal ProjectCollection BuildEngine
        {
            get => this.buildEngine;
            set => this.buildEngine = value;
        }

        /// <summary>
        /// The internal package implementation.
        /// </summary>
        internal ProjectPackage Package
        {
            get => this.package;
            set => this.package = value;
        }

        // !EFW
        /// <summary>
        /// This returns a flag indicating whether or not all files are showing
        /// </summary>
        public bool ShowingAllFiles { get; private set; }

        #endregion

        #region ctor

        protected ProjectNode()
        {
            this.Initialize();
        }
        #endregion

        #region overridden methods
        protected override NodeProperties CreatePropertiesObject()
        {
            return new ProjectNodeProperties(this);
        }

        /// <summary>
        /// Sets the properties for the project node.
        /// </summary>
        /// <param name="propid">Identifier of the hierarchy property. For a list of property ID values, <see cref="__VSHPROPID"/> </param>
        /// <param name="value">The value to set. </param>
        /// <returns>A success or failure value.</returns>
        public override int SetProperty(int propid, object value)
        {
            __VSHPROPID id = (__VSHPROPID)propid;

            switch (id)
            {
                case __VSHPROPID.VSHPROPID_ShowProjInSolutionPage:
                    this.ShowProjectInSolutionPage = (bool)value;
                    return VSConstants.S_OK;
            }

            return base.SetProperty(propid, value);
        }

        /// <summary>
        /// Renames the project node.
        /// </summary>
        /// <param name="label">The new name</param>
        /// <returns>A success or failure value.</returns>
        public override int SetEditLabel(string label)
        {
            // Validate the filename. 
            if (String.IsNullOrEmpty(label))
            {
                throw new InvalidOperationException(SR.GetString(SR.ErrorInvalidFileName, CultureInfo.CurrentUICulture));
            }
            else if (this.ProjectFolder.Length + label.Length + 1 > NativeMethods.MAX_PATH)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.PathTooLong, CultureInfo.CurrentUICulture), label));
            }
            else if (Utilities.IsFileNameInvalid(label))
            {
                throw new InvalidOperationException(SR.GetString(SR.ErrorInvalidFileName, CultureInfo.CurrentUICulture));
            }

            string fileName = Path.GetFileNameWithoutExtension(label);

            // if there is no filename or it starts with a leading dot issue an error message and quit.
            if (String.IsNullOrEmpty(fileName) || fileName[0] == '.')
            {
                throw new InvalidOperationException(SR.GetString(SR.FileNameCannotContainALeadingPeriod, CultureInfo.CurrentUICulture));
            }

            // Nothing to do if the name is the same
            string oldFileName = Path.GetFileNameWithoutExtension(this.Url);
            if (String.Compare(oldFileName, label, StringComparison.Ordinal) == 0)
            {
                return VSConstants.S_FALSE;
            }

            // Now check whether the original file is still there. It could have been renamed.
            if (!File.Exists(this.Url))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.FileOrFolderCannotBeFound, CultureInfo.CurrentUICulture), this.ProjectFile));
            }

            // Get the full file name and then rename the project file.
            string newFile = Path.Combine(this.ProjectFolder, label);
            string extension = Path.GetExtension(this.Url);

            // Make sure it has the correct extension
            if (String.Compare(Path.GetExtension(newFile), extension, StringComparison.OrdinalIgnoreCase) != 0)
            {
                newFile += extension;
            }

            this.RenameProjectFile(newFile);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the automation object for the project node.
        /// </summary>
        /// <returns>An instance of an EnvDTE.Project implementation object representing the automation object for the project.</returns>
        public override object GetAutomationObject()
        {
            return new Automation.OAProject(this);
        }

        /// <summary>
        /// Closes the project node.
        /// </summary>
        /// <returns>A success or failure value.</returns>
        public override int Close()
        {
            int hr = VSConstants.S_OK;
            try
            {
                // Walk the tree and close all nodes.
                // This has to be done before the project closes, since we want still state available for the ProjectMgr on the nodes 
                // when nodes are closing.
                try
                {
                    CloseAllNodes(this);
                }
                finally
                {
                    this.Dispose(true);
                }
            }
            catch (COMException e)
            {
                hr = e.ErrorCode;
            }
            finally
            {
                ErrorHandler.ThrowOnFailure(base.Close());
            }

            this.isClosed = true;

            return hr;
        }

        /// <summary>
        /// Sets the service provider from which to access the services. 
        /// </summary>
        /// <param name="site">An instance to an Microsoft.VisualStudio.OLE.Interop object</param>
        /// <returns>A success or failure value.</returns>
        public override int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider site)
        {
            CCITracing.TraceCall();
            this.site = new ServiceProvider(site);
            ServiceProvider = this.site;

            if (taskProvider != null)
            {
                taskProvider.Dispose();
            }
            taskProvider = new TaskProvider(this.site);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the properties of the project node. 
        /// </summary>
        /// <param name="propId">The __VSHPROPID of the property.</param>
        /// <returns>A property dependent value. See: <see cref="__VSHPROPID"/> for details.</returns>
        public override object GetProperty(int propId)
        {
            switch ((__VSHPROPID)propId)
            {
                case __VSHPROPID.VSHPROPID_ConfigurationProvider:
                    return this.ConfigProvider;

                case __VSHPROPID.VSHPROPID_ProjectName:
                    return this.Caption;

                case __VSHPROPID.VSHPROPID_ProjectDir:
                    return this.ProjectFolder;

                case __VSHPROPID.VSHPROPID_TypeName:
                    return this.ProjectType;

                case __VSHPROPID.VSHPROPID_ShowProjInSolutionPage:
                    return this.ShowProjectInSolutionPage;

                case __VSHPROPID.VSHPROPID_ExpandByDefault:
                    return true;

                // Use the same icon as if the folder was closed
                case __VSHPROPID.VSHPROPID_OpenFolderIconIndex:
                    return GetProperty((int)__VSHPROPID.VSHPROPID_IconIndex);
            }

            switch ((__VSHPROPID2)propId)
            {
                case __VSHPROPID2.VSHPROPID_SupportsProjectDesigner:
                    return this.SupportsProjectDesigner;

                case __VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
                    return Utilities.CreateSemicolonDelimitedListOfStringFromGuids(this.GetConfigurationIndependentPropertyPages());

                case __VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList:
                    return Utilities.CreateSemicolonDelimitedListOfStringFromGuids(this.GetConfigurationDependentPropertyPages());

                case __VSHPROPID2.VSHPROPID_PriorityPropertyPagesCLSIDList:
                    return Utilities.CreateSemicolonDelimitedListOfStringFromGuids(this.GetPriorityProjectDesignerPages());

                case __VSHPROPID2.VSHPROPID_Container:
                    return true;
                default:
                    break;
            }

            return base.GetProperty(propId);
        }

        /// <summary>
        /// Gets the GUID value of the node. 
        /// </summary>
        /// <param name="propid">A __VSHPROPID or __VSHPROPID2 value of the guid property</param>
        /// <param name="guid">The guid to return for the property.</param>
        /// <returns>A success or failure value.</returns>
        public override int GetGuidProperty(int propid, out Guid guid)
        {
            if((__VSHPROPID)propid == __VSHPROPID.VSHPROPID_ProjectIDGuid)
            {
                guid = this.ProjectIDGuid;
            }
            else if (propid == (int)__VSHPROPID.VSHPROPID_CmdUIGuid)
            {
                guid = this.ProjectGuid;
            }
            else if ((__VSHPROPID2)propid == __VSHPROPID2.VSHPROPID_ProjectDesignerEditor && this.SupportsProjectDesigner)
            {
                guid = this.ProjectDesignerEditor;
            }
            else
            {
                base.GetGuidProperty(propid, out guid);
            }

            if (guid.CompareTo(Guid.Empty) == 0)
            {
                return VSConstants.DISP_E_MEMBERNOTFOUND;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Sets Guid properties for the project node.
        /// </summary>
        /// <param name="propid">A __VSHPROPID or __VSHPROPID2 value of the guid property</param>
        /// <param name="guid">The guid value to set.</param>
        /// <returns>A success or failure value.</returns>
        public override int SetGuidProperty(int propid, ref Guid guid)
        {
            switch ((__VSHPROPID)propid)
            {
                case __VSHPROPID.VSHPROPID_ProjectIDGuid:
                    this.ProjectIDGuid = guid;
                    return VSConstants.S_OK;
            }
            CCITracing.TraceCall(String.Format(CultureInfo.CurrentCulture, "Property {0} not found", propid));
            return VSConstants.DISP_E_MEMBERNOTFOUND;
        }

        /// <summary>
        /// Removes items from the hierarchy. 
        /// </summary>
        /// <devdoc>Project overwrites this.</devdoc>
        public override void Remove(bool removeFromStorage)
        {
            // the project will not be deleted from disk, just removed      
            if (removeFromStorage)
            {
                return;
            }

            // Remove the entire project from the solution
            IVsSolution solution = this.Site.GetService(typeof(SVsSolution)) as IVsSolution;
            uint iOption = 1; // SLNSAVEOPT_PromptSave
            ErrorHandler.ThrowOnFailure(solution.CloseSolutionElement(iOption, this.InteropSafeIVsHierarchy, 0));
        }

        /// <summary>
        /// Gets the moniker for the project node. That is the full path of the project file.
        /// </summary>
        /// <returns>The moniker for the project file.</returns>
        public override string GetMkDocument()
        {
            Debug.Assert(!String.IsNullOrEmpty(this.filename));
            Debug.Assert(this.BaseURI != null && !String.IsNullOrEmpty(this.BaseURI.AbsoluteUrl));
            return Path.Combine(this.BaseURI.AbsoluteUrl, this.filename);
        }

        /// <summary>
        /// Disposes the project node object.
        /// </summary>
        /// <param name="disposing">Flag determining whether it was deterministic or non deterministic clean up.</param>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            try
            {
                try
                {
                    this.UnRegisterProject();
                }
                finally
                {
                    try
                    {
                        this.RegisterClipboardNotifications(false);
                    }
                    finally
                    {
                        try
                        {
                            if (this.projectEventsProvider != null)
                            {
                                this.projectEventsProvider.AfterProjectFileOpened -= this.OnAfterProjectOpen;
                            }
                            if (this.taskProvider != null)
                            {
                                taskProvider.Tasks.Clear();
                                this.taskProvider.Dispose();
                                this.taskProvider = null;
                            }

                            if (this.buildLogger != null)
                            {
                                this.buildLogger.Shutdown();
                                buildLogger = null;
                            }

                            if (this.site != null)
                            {
                                this.site.Dispose();
                            }
                        }
                        finally
                        {
                            this.buildEngine = null;
                        }
                    }
                }

                if (this.buildProject != null)
                {
                    this.buildProject.ProjectCollection.UnloadProject(this.buildProject);
                    this.buildProject.ProjectCollection.UnloadProject(this.buildProject.Xml);
                    this.buildProject = null;
                }

                if (null != imageHandler)
                {
                    imageHandler.Close();
                    imageHandler = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Handles command status on the project node. If a command cannot be handled then the base should be called.
        /// </summary>
        /// <param name="cmdGroup">A unique identifier of the command group. The pguidCmdGroup parameter can be NULL to specify the standard group.</param>
        /// <param name="cmd">The command to query status for.</param>
        /// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return the name and/or status information of a single command. Can be NULL to indicate that the caller does not require this information.</param>
        /// <param name="result">An out parameter specifying the QueryStatusResult of the command.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    case VsCommands.BuildSln:   // !EFW - Don't allow if building already
                        result |= QueryStatusResult.SUPPORTED;

                        if(!this.buildInProcess)
                            result |= QueryStatusResult.ENABLED;
                        else
                            result |= QueryStatusResult.INVISIBLE;

                        return VSConstants.S_OK;

                    case VsCommands.Copy:
                    case VsCommands.Paste:
                    case VsCommands.Cut:
                    case VsCommands.Rename:
                    case VsCommands.Exit:
                    case VsCommands.ProjectSettings:
                    case VsCommands.UnloadProject:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;

                    case VsCommands.ViewForm:
                        if (this.HasDesigner)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;

                    case VsCommands.CancelBuild:
                        result |= QueryStatusResult.SUPPORTED;
                        if (this.buildInProcess)
                            result |= QueryStatusResult.ENABLED;
                        else
                            result |= QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;

                    case VsCommands.NewFolder:
                    case VsCommands.AddNewItem:
                    case VsCommands.AddExistingItem:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;

                    case VsCommands.SetStartupProject:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.ADDREFERENCE:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;

                    case VsCommands2K.EXCLUDEFROMPROJECT:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;

                    case ExploreFolderInWindowsCommand:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else
            {
                return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        /// <summary>
        /// Handles command execution.
        /// </summary>
        /// <param name="cmdGroup">Unique identifier of the command group</param>
        /// <param name="cmd">The command to be executed.</param>
        /// <param name="nCmdexecopt">Values describe how the object should execute the command.</param>
        /// <param name="pvaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL</param>
        /// <param name="pvaOut">VARIANTARG structure to receive command output. Can be NULL.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {

                    case VsCommands.UnloadProject:
                        return this.UnloadProject();
                    case VsCommands.CleanSel:
                    case VsCommands.CleanCtx:
                        return this.CleanProject();
                }
            }
            else if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.ADDREFERENCE:
                        return this.AddProjectReference();

                    case VsCommands2K.ADDWEBREFERENCE:
                    case VsCommands2K.ADDWEBREFERENCECTX:
                        return this.AddWebReference();

                    case ExploreFolderInWindowsCommand:
                        string explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                        System.Diagnostics.Process.Start(explorerPath, this.ProjectFolder);
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Get the boolean value for the deletion of a project item
        /// </summary>
        /// <param name="deleteOperation">A flag that specifies the type of delete operation (delete from storage or remove from project)</param>
        /// <returns>true if item can be deleted from project</returns>
        protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            if (deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_RemoveFromProject)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a specific Document manager to handle opening and closing of the Project(Application) Designer if project designer is supported.
        /// </summary>
        /// <returns>Document manager object</returns>
        protected internal override DocumentManager GetDocumentManager()
        {
            if (this.SupportsProjectDesigner)
            {
                return new ProjectDesignerDocumentManager(this);
            }
            return null;
        }

        #endregion

        #region virtual methods

        /// <summary>
        /// Executes a wizard.
        /// </summary>
        /// <param name="parentNode">The node to which the wizard should add item(s).</param>
        /// <param name="itemName">The name of the file that the user typed in.</param>
        /// <param name="wizardToRun">The name of the wizard to run.</param>
        /// <param name="dlgOwner">The owner of the dialog box.</param>
        /// <returns>A VSADDRESULT enum value describing success or failure.</returns>
        public virtual VSADDRESULT RunWizard(HierarchyNode parentNode, string itemName, string wizardToRun, IntPtr dlgOwner)
        {
            Debug.Assert(!String.IsNullOrEmpty(itemName), "The Add item dialog was passing in a null or empty item to be added to the hierarchy.");
            Debug.Assert(!String.IsNullOrEmpty(this.ProjectFolder), "The Project Folder is not specified for this project.");

            if (parentNode == null)
            {
                throw new ArgumentNullException(nameof(parentNode));
            }

            if (String.IsNullOrEmpty(itemName))
            {
                throw new ArgumentNullException(nameof(itemName));
            }

            // We just validate for length, since we assume other validation has been performed by the dlgOwner.
            if (this.ProjectFolder.Length + itemName.Length + 1 > NativeMethods.MAX_PATH)
            {
                string errorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.PathTooLong, CultureInfo.CurrentUICulture), itemName);
                if (!Utilities.IsInAutomationFunction(this.Site))
                {
                    string title = null;
                    OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                    OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
                    OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                    VsShellUtilities.ShowMessageBox(this.Site, title, errorMessage, icon, buttons, defaultButton);
                    return VSADDRESULT.ADDRESULT_Failure;
                }
                else
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }


            // Build up the ContextParams safearray
            //  [0] = Wizard type guid  (bstr)
            //  [1] = Project name  (bstr)
            //  [2] = ProjectItems collection (bstr)
            //  [3] = Local Directory (bstr)
            //  [4] = Filename the user typed (bstr)
            //  [5] = Product install Directory (bstr)
            //  [6] = Run silent (bool)

            object[] contextParams = new object[7];
            contextParams[0] = EnvDTE.Constants.vsWizardAddItem;
            contextParams[1] = this.Caption;
            object automationObject = parentNode.GetAutomationObject();
            
            if(automationObject is EnvDTE.Project project)
            {
                contextParams[2] = project.ProjectItems;
            }
            else
            {
                // This would normally be a folder unless it is an item with subitems
                EnvDTE.ProjectItem item = (EnvDTE.ProjectItem)automationObject;
                contextParams[2] = item.ProjectItems;
            }

            contextParams[3] = this.ProjectFolder;

            contextParams[4] = itemName;

            IVsShell shell = (IVsShell)this.GetService(typeof(IVsShell));
            ErrorHandler.ThrowOnFailure(shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out object objInstallationDir));
            string installDir = (string)objInstallationDir;

            // append a '\' to the install directory to mimic what the shell does (though it doesn't add one to destination directory)
            if (!installDir.EndsWith("\\", StringComparison.Ordinal))
            {
                installDir += "\\";
            }

            contextParams[5] = installDir;

            contextParams[6] = true;

            IVsExtensibility3 ivsExtensibility = this.GetService(typeof(IVsExtensibility)) as IVsExtensibility3;
            Debug.Assert(ivsExtensibility != null, "Failed to get IVsExtensibility3 service");
            if (ivsExtensibility == null)
            {
                return VSADDRESULT.ADDRESULT_Failure;
            }

            // Determine if we have the trust to run this wizard.
            IVsDetermineWizardTrust wizardTrust = this.GetService(typeof(SVsDetermineWizardTrust)) as IVsDetermineWizardTrust;
            if (wizardTrust != null)
            {
                Guid guidProjectAdding = Guid.Empty;
                ErrorHandler.ThrowOnFailure(wizardTrust.OnWizardInitiated(wizardToRun, ref guidProjectAdding));
            }

            int wizResultAsInt;
            try
            {
                Array contextParamsAsArray = contextParams;

                int result = ivsExtensibility.RunWizardFile(wizardToRun, dlgOwner, ref contextParamsAsArray, out wizResultAsInt);

                if(!ErrorHandler.Succeeded(result) && result != VSConstants.OLE_E_PROMPTSAVECANCELLED)
                {
                    ErrorHandler.ThrowOnFailure(result);
                }
            }
            finally
            {
                if (wizardTrust != null)
                {
                    ErrorHandler.ThrowOnFailure(wizardTrust.OnWizardCompleted());
                }
            }

            EnvDTE.wizardResult wizardResult = (EnvDTE.wizardResult)wizResultAsInt;

            switch (wizardResult)
            {
                default:
                    return VSADDRESULT.ADDRESULT_Cancel;
                case wizardResult.wizardResultSuccess:
                    return VSADDRESULT.ADDRESULT_Success;
                case wizardResult.wizardResultFailure:
                    return VSADDRESULT.ADDRESULT_Failure;
            }
        }

        /// <summary>
        /// Override this method if you want to modify the behavior of the Add Reference dialog
        /// By example you could change which pages are visible and which is visible by default.
        /// </summary>
        /// <returns></returns>
        public virtual int AddProjectReference()
        {
            CCITracing.TraceCall();

            IVsComponentSelectorDlg4 componentDialog;
            string strBrowseLocations = Path.GetDirectoryName(this.BaseURI.Uri.LocalPath);
            var tabInitList = new List<VSCOMPONENTSELECTORTABINIT>()
            {
                new VSCOMPONENTSELECTORTABINIT {
                    guidTab = VSConstants.GUID_COMPlusPage,
                    varTabInitInfo = GetComponentPickerDirectories(),
                },
                new VSCOMPONENTSELECTORTABINIT {
                    guidTab = VSConstants.GUID_COMClassicPage,
                },
                new VSCOMPONENTSELECTORTABINIT {
                    // Tell the Add Reference dialog to call hierarchies GetProperty with the following
                    // propID to enable filtering out ourself from the Project to Project reference
                    varTabInitInfo = (int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage,
                    guidTab = VSConstants.GUID_SolutionPage,
                },
                // Add the Browse for file page            
                new VSCOMPONENTSELECTORTABINIT {
                    varTabInitInfo = 0,
                    guidTab = VSConstants.GUID_BrowseFilePage,
                },
                new VSCOMPONENTSELECTORTABINIT {
                    guidTab = GUID_MruPage,
                },
            };
            tabInitList.ForEach(tab => tab.dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT)));

            componentDialog = this.GetService(typeof(IVsComponentSelectorDlg)) as IVsComponentSelectorDlg4;
            try
            {
                // call the container to open the add reference dialog.
                if (componentDialog != null)
                {
                    // Let the project know not to show itself in the Add Project Reference Dialog page
                    this.ShowProjectInSolutionPage = false;
                    // call the container to open the add reference dialog.
                    string browseFilters = "Component Files (*.exe;*.dll)\0*.exe;*.dll\0";
                    ErrorHandler.ThrowOnFailure(componentDialog.ComponentSelectorDlg5(
                        (System.UInt32)(__VSCOMPSELFLAGS.VSCOMSEL_MultiSelectMode | __VSCOMPSELFLAGS.VSCOMSEL_IgnoreMachineName),
                        this.InteropSafeIVsComponentUser,
                        0,
                        null,
                        SR.GetString(SR.AddReferenceDialogTitle, CultureInfo.CurrentUICulture),   // Title
                        "VS.AddReference",                          // Help topic
                        addComponentDialogSizeX,
                        addComponentDialogSizeY,
                        (uint)tabInitList.Count,
                        tabInitList.ToArray(),
                        ref addComponentLastActiveTab,
                        browseFilters,
                        ref strBrowseLocations,
                        this.TargetFrameworkMoniker.FullName));
                }
            }
            catch (COMException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
                return e.ErrorCode;
            }
            finally
            {
                // Let the project know it can show itself in the Add Project Reference Dialog page
                this.ShowProjectInSolutionPage = true;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the Compiler associated to the project 
        /// </summary>
        /// <returns>Null</returns>
        public virtual ICodeCompiler GetCompiler()
        {

            return null;
        }

        /// <summary>
        /// Override this method if you have your own project specific
        /// subclass of ProjectOptions
        /// </summary>
        /// <returns>This method returns a new instance of the ProjectOptions base class.</returns>
        public virtual ProjectOptions CreateProjectOptions()
        {
            return new ProjectOptions();
        }

        /// <summary>
        /// Loads a project file. Called from the factory CreateProject to load the project.
        /// </summary>
        /// <param name="fileName">File name of the project that will be created. </param>
        /// <param name="location">Location where the project will be created.</param>
        /// <param name="name">If applicable, the name of the template to use when cloning a new project.</param>
        /// <param name="flags">Set of flag values taken from the VSCREATEPROJFLAGS enumeration.</param>
        /// <param name="iidProject">Identifier of the interface that the caller wants returned. </param>
        /// <param name="canceled">An out parameter specifying if the project creation was canceled</param>
        public virtual void Load(string fileName, string location, string name, uint flags, ref Guid iidProject, out int canceled)
        {
            try
            {
                this.disableQueryEdit = true;

                // set up internal members and icons
                canceled = 0;

                this.ProjectMgr = this;
                this.isNewProject = false;

                if ((flags & (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE) == (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE)
                {
                    // we need to generate a new guid for the project
                    this.projectIdGuid = Guid.NewGuid();
                }
                else
                {
                    this.SetProjectGuidFromProjectFile();
                }

                // This is almost a No op if the engine has already been instantiated in the factory.
                this.buildEngine = Utilities.InitializeMsBuildEngine(this.buildEngine, this.Site);

                // based on the passed in flags, this either reloads/loads a project, or tries to create a new one
                // now we create a new project... we do that by loading the template and then saving under a new name
                // we also need to copy all the associated files with it.					
                if ((flags & (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE) == (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE)
                {
                    Debug.Assert(!String.IsNullOrEmpty(fileName) && File.Exists(fileName), "Invalid filename passed to load the project. A valid filename is expected");

                    this.isNewProject = true;

                    // This should be a very fast operation if the build project is already initialized by the Factory.
                    SetBuildProject(Utilities.ReinitializeMsBuildProject(this.buildEngine, fileName, this.buildProject));

                    // Compute the file name
                    // We try to solve two problems here. When input comes from a wizard in case of zipped based projects
                    // the parameters are different.
                    // In that case the filename has the new filename in a temporary path.

                    // First get the extension from the template.
                    // Then get the filename from the name.
                    // Then create the new full path of the project.
                    string extension = Path.GetExtension(fileName);

                    string tempName = String.Empty;

                    // We have to be sure that we are not going to loose data here. If the project name is a.b.c then for a project that was based on a zipped template(the wizard calls us) GetFileNameWithoutExtension will suppress "c".
                    // We are going to check if the parameter "name" is extension based and the extension is the same as the one from the "filename" parameter.
                    string tempExtension = Path.GetExtension(name);
                    if (!String.IsNullOrEmpty(tempExtension))
                    {
                        bool isSameExtension = (String.Compare(tempExtension, extension, StringComparison.OrdinalIgnoreCase) == 0);

                        if (isSameExtension)
                        {
                            tempName = Path.GetFileNameWithoutExtension(name);
                        }
                        // If the tempExtension is not the same as the extension that the project name comes from then assume that the project name is a dotted name.
                        else
                        {
                            tempName = Path.GetFileName(name);
                        }
                    }
                    else
                    {
                        tempName = Path.GetFileName(name);
                    }

                    Debug.Assert(!String.IsNullOrEmpty(tempName), "Could not compute project name");
                    string tempProjectFileName = tempName + extension;
                    this.filename = Path.Combine(location, tempProjectFileName);

                    // Initialize the common project properties.
                    this.InitializeProjectProperties();

                    ErrorHandler.ThrowOnFailure(this.Save(this.filename, 1, 0));

                    // now we do have the project file saved. we need to create embedded files.
                    foreach (MSBuild.ProjectItem item in this.BuildProject.Items)
                    {
                        // Ignore the item if it is a reference or folder
                        if (this.FilterItemTypeToBeAddedToHierarchy(item.ItemType))
                        {
                            continue;
                        }

                        // MSBuilds tasks/targets can create items (such as object files),
                        // such items are not part of the project per se, and should not be displayed
                        // so ignore those items.
                        if (!this.IsItemTypeFileType(item.ItemType))
                        {
                            continue;
                        }

                        string strRelFilePath = item.EvaluatedInclude;
                        string basePath = Path.GetDirectoryName(fileName);
                        string strPathToFile;
                        string newFileName;
                        // taking the base name from the project template + the relative pathname,
                        // and you get the filename
                        strPathToFile = Path.Combine(basePath, strRelFilePath);
                        // the new path should be the base directory of the new project (location) + the rel path of the file
                        newFileName = Path.Combine(location, strRelFilePath);
                        // now the copy file
                        AddFileFromTemplate(strPathToFile, newFileName);
                    }
                }
                else
                {
                    this.filename = fileName;
                }

                // now reload to fix up references
                this.Reload();
            }
            finally
            {
                this.disableQueryEdit = false;
            }
        }

        /// <summary>
        /// Called to add a file to the project from a template.
        /// Override to do it yourself if you want to customize the file
        /// </summary>
        /// <param name="source">Full path of template file</param>
        /// <param name="target">Full path of file once added to the project</param>
        public virtual void AddFileFromTemplate(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (String.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target));
            }

            try
            {
                string directory = Path.GetDirectoryName(target);
                if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                FileInfo fiOrg = new FileInfo(source);
                FileInfo fiNew = fiOrg.CopyTo(target, true);

                fiNew.Attributes = FileAttributes.Normal; // remove any read only attributes.
            }
            catch (IOException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch (ArgumentException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch (NotSupportedException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
        }

        /// <summary>
        /// Called when the project opens an editor window for the given file
        /// </summary>
        public virtual void OnOpenItem(string fullPathToSourceFile)
        {
        }

        /// <summary>
        /// This add methods adds the "key" item to the hierarchy, potentially adding other subitems in the process
        /// This method may recurse if the parent is an other subitem
        /// 
        /// </summary>
        /// <param name="subitems">List of subitems not yet added to the hierarchy</param>
        /// <param name="key">Key to retrieve the target item from the subitems list</param>
        /// <returns>Newly added node</returns>
        /// <remarks>If the parent node was found we add the dependent item to it otherwise we add the item ignoring the "DependentUpon" metadata</remarks>
        protected virtual HierarchyNode AddDependentFileNode(IDictionary<String, MSBuild.ProjectItem> subitems, string key)
        {
            if (subitems == null)
            {
                throw new ArgumentNullException(nameof(subitems));
            }

            MSBuild.ProjectItem item = subitems[key];
            subitems.Remove(key);

            HierarchyNode newNode;
            HierarchyNode parent = null;

            string dependentOf = item.GetMetadataValue(ProjectFileConstants.DependentUpon);
            Debug.Assert(String.Compare(dependentOf, key, StringComparison.OrdinalIgnoreCase) != 0, "File dependent upon itself is not valid. Ignoring the DependentUpon metadata");
            if (subitems.ContainsKey(dependentOf))
            {
                // The parent item is an other subitem, so recurse into this method to add the parent first
                parent = AddDependentFileNode(subitems, dependentOf);
            }
            else
            {
                // See if the parent node already exist in the hierarchy
                string path = Path.Combine(this.ProjectFolder, dependentOf);
                
                ErrorHandler.ThrowOnFailure(this.ParseCanonicalName(path, out uint parentItemID));
                
                if (parentItemID != (uint)VSConstants.VSITEMID.Nil)
                    parent = this.NodeFromItemId(parentItemID);

                Debug.Assert(parent != null, "File dependent upon a non existing item or circular dependency. Ignoring the DependentUpon metadata");
            }

            // If the parent node was found we add the dependent item to it otherwise we add the item ignoring the "DependentUpon" metadata
            if (parent != null)
                newNode = this.AddDependentFileNodeToNode(item, parent);
            else
                newNode = this.AddIndependentFileNode(item);

            return newNode;
        }

        /// <summary>
        /// This is called from the main thread before the background build starts.
        /// cleanBuild is not part of the vsopts, but passed down as the call path is differently
        /// PrepareBuild mainly creates directories and cleans house if cleanBuild is true
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cleanBuild"></param>
        public virtual void PrepareBuild(string config, bool cleanBuild)
        {
            if (this.buildIsPrepared && !cleanBuild) return;

            ProjectOptions options = this.GetProjectOptions(config);
            string outputPath = Path.GetDirectoryName(options.OutputAssembly);

            if (cleanBuild && this.currentConfig.Targets.ContainsKey(MsBuildTarget.Clean))
            {
                this.InvokeMsBuild(MsBuildTarget.Clean);
            }

            PackageUtilities.EnsureOutputPath(outputPath);
            if (!String.IsNullOrEmpty(options.XmlDocFileName))
            {
                PackageUtilities.EnsureOutputPath(Path.GetDirectoryName(options.XmlDocFileName));
            }

            this.buildIsPrepared = true;
        }

        /// <summary>
        /// Do the build by invoking MSBuild
        /// </summary>
        public virtual MSBuildResult Build(uint vsopts, string config, IVsOutputWindowPane output, string target)
        {
            lock (ProjectNode.BuildLock)
            {
                bool engineLogOnlyCritical = this.BuildPrelude(output);

                MSBuildResult result = MSBuildResult.Failed;

                try
                {
                    this.SetBuildConfigurationProperties(config);

                    result = this.InvokeMsBuild(target);
                }
                finally
                {
                    // Unless someone specifically request to use an output window pane, we should not output to it
                    if (null != output)
                    {
                        this.SetOutputLogger(null);
                        BuildEngine.OnlyLogCriticalEvents = engineLogOnlyCritical;
                    }
                }

                return result;
            }
        }


        /// <summary>
        /// Do the build by invoking MSBuild
        /// </summary>
        internal virtual void BuildAsync(uint vsopts, string config, IVsOutputWindowPane output, string target, Action<MSBuildResult, string> uiThreadCallback)
        {
            this.BuildPrelude(output);
            this.SetBuildConfigurationProperties(config);
            this.DoMSBuildSubmission(BuildKind.Async, target, uiThreadCallback);
        }

        /// <summary>
        /// Return the value of a project property
        /// </summary>
        /// <param name="propertyName">Name of the property to get</param>
        /// <param name="resetCache">True to avoid using the cache</param>
        /// <returns>null if property does not exist, otherwise value of the property</returns>
        public virtual string GetProjectProperty(string propertyName, bool resetCache)
        {
            ProjectPropertyInstance property = GetMsBuildProperty(propertyName, resetCache);
            
            if (property == null)
                return null;

            return property.EvaluatedValue;
        }

        /// <summary>
        /// Set value of project property
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        public virtual void SetProjectProperty(string propertyName, string propertyValue)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Cannot set a null project property");

            string oldValue = null;
            ProjectPropertyInstance oldProp = GetMsBuildProperty(propertyName, true);

            if (oldProp != null)
                oldValue = oldProp.EvaluatedValue;
            
            if (propertyValue == null)
            {
                // if property already null, do nothing
                if (oldValue == null)
                    return;
                // otherwise, set it to empty
                propertyValue = String.Empty;
            }

            // Only do the work if this is different to what we had before
            if (String.Compare(oldValue, propertyValue, StringComparison.Ordinal) != 0)
            {
                // Check out the project file.
                if (!this.ProjectMgr.QueryEditProjectFile(false))
                {
                    throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                }

                this.buildProject.SetProperty(propertyName, propertyValue);
                RaiseProjectPropertyChanged(propertyName, oldValue, propertyValue);

                // property cache will need to be updated
                this.currentConfig = null;
                this.SetProjectFileDirty(true);
            }
            return;
        }

        public virtual ProjectOptions GetProjectOptions(string config = null)
        {
            if (String.IsNullOrEmpty(config))
            {
                EnvDTE.Project automationObject = this.GetAutomationObject() as EnvDTE.Project;
                try
                {
                    config = Utilities.GetActiveConfigurationName(automationObject);
                }
                catch (InvalidOperationException)
                {
                    // Can't figure out the active configuration.  Perhaps during solution load, or in a unit test.
                }
                catch (COMException)
                {
                    // We may be in solution load and don't have an active config yet.
                }
            }

            // Even though the options are the same, when this config was originally set, it may have been before 
            // the project system was ready to set up its configuration, so go ahead and call through to SetConfiguration 
            // anyway -- it should do effectively nothing if the config is the same and all the initialization has 
            // already occurred. 
            if (this.options != null && String.Equals(this.options.Config, config, StringComparison.OrdinalIgnoreCase))
            {
                if (config != null)
                {
                    // Fancy dance with the options required because SetConfiguration nulls out this.options 
                    // even if the configuration itself has not changed; whereas we're only calling SetConfiguration 
                    // for purposes of initializing some other fields here; since we know the config is the same, it
                    // should be perfectly safe to keep the same options as before.  
                    ProjectOptions currentOptions = this.options;
                    this.SetConfiguration(config);
                    this.options = currentOptions;
                }

                return this.options;
            }

            ProjectOptions options = CreateProjectOptions();
            options.Config = config;

            string targetFrameworkMoniker = GetProjectProperty("TargetFrameworkMoniker", false);

            if (!String.IsNullOrEmpty(targetFrameworkMoniker))
            {
                try
                {
                    options.TargetFrameworkMoniker = new FrameworkName(targetFrameworkMoniker);
                }
                catch (ArgumentException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
            }

            if (config == null)
            {
                this.options = options;
                return options;
            }

            options.GenerateExecutable = true;

            this.SetConfiguration(config);

            string outputPath = this.GetOutputPath(this.currentConfig);
            if (!String.IsNullOrEmpty(outputPath))
            {
                // Make absolute relative to project folder location
                outputPath = Path.Combine(this.ProjectFolder, outputPath);
            }

            // Set some default values
            options.OutputAssembly = outputPath + this.Caption + ".exe";
            options.ModuleKind = ModuleKindFlags.ConsoleApplication;

            options.RootNamespace = GetProjectProperty(ProjectFileConstants.RootNamespace, false);
            options.OutputAssembly = outputPath + this.GetAssemblyName(config);

            string outputtype = GetProjectProperty(ProjectFileConstants.OutputType, false);
            if (!String.IsNullOrEmpty(outputtype))
            {
                outputtype = outputtype.ToLower(CultureInfo.InvariantCulture);
            }

            if (outputtype == "library")
            {
                options.ModuleKind = ModuleKindFlags.DynamicallyLinkedLibrary;
                options.GenerateExecutable = false; // DLL's have no entry point.
            }
            else if (outputtype == "winexe")
                options.ModuleKind = ModuleKindFlags.WindowsApplication;
            else
                options.ModuleKind = ModuleKindFlags.ConsoleApplication;

            options.Win32Icon = GetProjectProperty("ApplicationIcon", false);
            options.MainClass = GetProjectProperty("StartupObject", false);

            //    other settings from CSharp we may want to adopt at some point...
            //    AssemblyKeyContainerName = ""  //This is the key file used to sign the interop assembly generated when importing a com object via add reference
            //    AssemblyOriginatorKeyFile = ""
            //    DelaySign = "false"
            //    DefaultClientScript = "JScript"
            //    DefaultHTMLPageLayout = "Grid"
            //    DefaultTargetSchema = "IE50"
            //    PreBuildEvent = ""
            //    PostBuildEvent = ""
            //    RunPostBuildEvent = "OnBuildSuccess"

            // transfer all config build options...
            if (GetBoolAttr(this.currentConfig, "AllowUnsafeBlocks"))
            {
                options.AllowUnsafeCode = true;
            }

            if (GetProjectProperty("BaseAddress", false) != null)
            {
                try
                {
                    options.BaseAddress = Int64.Parse(GetProjectProperty("BaseAddress", false), CultureInfo.InvariantCulture);
                }
                catch (ArgumentNullException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (ArgumentException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (FormatException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (OverflowException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
            }

            if (GetBoolAttr(this.currentConfig, "CheckForOverflowUnderflow"))
            {
                options.CheckedArithmetic = true;
            }

            if (GetProjectProperty("DefineConstants", false) != null)
            {
                options.DefinedPreprocessorSymbols = new StringCollection();
                foreach (string s in GetProjectProperty("DefineConstants", false).Replace(" \t\r\n", "").Split(';'))
                {
                    options.DefinedPreprocessorSymbols.Add(s);
                }
            }

            string docFile = GetProjectProperty("DocumentationFile", false);
            if (!String.IsNullOrEmpty(docFile))
            {
                options.XmlDocFileName = Path.Combine(this.ProjectFolder, docFile);
            }

            if (GetBoolAttr(this.currentConfig, "DebugSymbols"))
            {
                options.IncludeDebugInformation = true;
            }

            if (GetProjectProperty("FileAlignment", false) != null)
            {
                try
                {
                    options.FileAlignment = Int32.Parse(GetProjectProperty("FileAlignment", false), CultureInfo.InvariantCulture);
                }
                catch (ArgumentNullException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (ArgumentException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (FormatException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (OverflowException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
            }

            if (GetBoolAttr(this.currentConfig, "IncrementalBuild"))
            {
                options.IncrementalCompile = true;
            }

            if (GetBoolAttr(this.currentConfig, "Optimize"))
            {
                options.Optimize = true;
            }

            if (GetBoolAttr(this.currentConfig, "RegisterForComInterop"))
            {
            }

            if (GetBoolAttr(this.currentConfig, "RemoveIntegerChecks"))
            {
            }

            if (GetBoolAttr(this.currentConfig, "TreatWarningsAsErrors"))
            {
                options.TreatWarningsAsErrors = true;
            }

            if (GetProjectProperty("WarningLevel", false) != null)
            {
                try
                {
                    options.WarningLevel = Int32.Parse(GetProjectProperty("WarningLevel", false), CultureInfo.InvariantCulture);
                }
                catch (ArgumentNullException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (ArgumentException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (FormatException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
                catch (OverflowException e)
                {
                    Trace.WriteLine("Exception : " + e.Message);
                }
            }

            this.options = options; // do this AFTER setting configuration so it doesn't clear it.
            return options;
        }

        public virtual void OnTargetFrameworkMonikerChanged(ProjectOptions options, FrameworkName currentTargetFramework, FrameworkName newTargetFramework)
        {
            if (currentTargetFramework == null)
            {
                throw new ArgumentNullException(nameof(currentTargetFramework));
            }
            if (newTargetFramework == null)
            {
                throw new ArgumentNullException(nameof(newTargetFramework));
            }

            if(!(this.site.GetService(typeof(SVsTrackProjectRetargeting)) is IVsTrackProjectRetargeting retargetingService))
            {
                // Probably in a unit test.
                ////throw new InvalidOperationException("Unable to acquire the SVsTrackProjectRetargeting service.");
                Marshal.ThrowExceptionForHR(UpdateTargetFramework(this.InteropSafeIVsHierarchy, currentTargetFramework.FullName, newTargetFramework.FullName));
            }
            else
            {
                Marshal.ThrowExceptionForHR(retargetingService.OnSetTargetFramework(this.InteropSafeIVsHierarchy, currentTargetFramework.FullName, newTargetFramework.FullName, this, true));
            }
        }

        public virtual bool GetBoolAttr(string config, string name)
        {
            this.SetConfiguration(config);
            return this.GetBoolAttr(this.currentConfig, name);
        }

        /// <summary>
        /// Get the assembly name for a give configuration
        /// </summary>
        /// <param name="config">the matching configuration in the MSBuild file</param>
        /// <returns>assembly name</returns>
        public virtual string GetAssemblyName(string config)
        {
            this.SetConfiguration(config);
            return GetAssemblyName(this.currentConfig);
        }

        /// <summary>
        /// Determines whether a file is a code file.
        /// </summary>
        /// <param name="fileName">Name of the file to be evaluated</param>
        /// <returns>false by default for any fileName</returns>
        public virtual bool IsCodeFile(string fileName)
        {
            return false;
        }

        /// <summary>
        /// Determines whether the given file is a resource file (.resx file).
        /// </summary>
        /// <param name="fileName">Name of the file to be evaluated.</param>
        /// <returns>true if the file is a .resx file, otherwise false.</returns>
        public virtual bool IsEmbeddedResource(string fileName)
        {
            if (String.Compare(Path.GetExtension(fileName), ".ResX", StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Create a file node based on an MSBuild item.
        /// </summary>
        /// <param name="item">MSBuild item</param>
        /// <returns>FileNode added</returns>
        public virtual FileNode CreateFileNode(ProjectElement item)
        {
            return new FileNode(this, item);
        }

        /// <summary>
        /// Create a file node based on a string.
        /// </summary>
        /// <param name="file">filename of the new file node</param>
        /// <returns>File node added</returns>
        public virtual FileNode CreateFileNode(string file)
        {
            ProjectElement item = this.AddFileToMsBuild(file);
            return this.CreateFileNode(item);
        }

        /// <summary>
        /// Create dependent file node based on an MSBuild item
        /// </summary>
        /// <param name="item">MSBuild item</param>
        /// <returns>dependent file node</returns>
        public virtual DependentFileNode CreateDependentFileNode(ProjectElement item)
        {
            return new DependentFileNode(this, item);
        }

        /// <summary>
        /// Create a dependent file node based on a string.
        /// </summary>
        /// <param name="file">filename of the new dependent file node</param>
        /// <returns>Dependent node added</returns>
        public virtual DependentFileNode CreateDependentFileNode(string file)
        {
            ProjectElement item = this.AddFileToMsBuild(file);
            return this.CreateDependentFileNode(item);
        }

        /// <summary>
        /// Walks the sub-paths of a project relative path and checks if the folder nodes hierarchy is already there, if not creates it.
        /// </summary>
        /// <param name="path">Path of the folder, can be relative to project or absolute</param>
        public virtual HierarchyNode CreateFolderNodes(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (Path.IsPathRooted(path))
            {
                // Ensure we are using a relative path
                if (String.Compare(this.ProjectFolder, 0, path, 0, this.ProjectFolder.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    throw new ArgumentException("The path is not relative", nameof(path));

                path = path.Substring(this.ProjectFolder.Length);
            }

            string[] parts;
            HierarchyNode curParent;

            parts = path.Split(Path.DirectorySeparatorChar);
            path = String.Empty;
            curParent = this;

            // now we have an array of subparts....
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    path += parts[i] + "\\";
                    curParent = VerifySubFolderExists(path, curParent);
                }
            }
            return curParent;
        }

        /// <summary>
        /// Defines if Node has Designer. By default we do not support designers for nodes
        /// </summary>
        /// <param name="itemPath">Path to item to query for designer support</param>
        /// <returns>true if node has designer</returns>
        public virtual bool NodeHasDesigner(string itemPath)
        {
            return false;
        }

        /// <summary>
        /// List of GUIDs of the config independent property pages. It is called by the GetProperty for VSHPROPID_PropertyPagesCLSIDList property.
        /// </summary>
        /// <returns></returns>
        protected virtual Guid[] GetConfigurationIndependentPropertyPages()
        {
            return new Guid[] { Guid.Empty };
        }

        /// <summary>
        /// Returns a list of GUIDs of the configuration dependent property pages. It is called by the GetProperty for VSHPROPID_CfgPropertyPagesCLSIDList property.
        /// </summary>
        /// <returns></returns>
        protected virtual Guid[] GetConfigurationDependentPropertyPages()
        {
            return Array.Empty<Guid>();
        }

        /// <summary>
        /// An ordered list of GUIDs of the preferred property pages. See <c>__VSHPROPID.VSHPROPID_PriorityPropertyPagesCLSIDList"</c>
        /// </summary>
        /// <returns>An array of GUIDs.</returns>
        protected virtual Guid[] GetPriorityProjectDesignerPages()
        {
            return new Guid[] { Guid.Empty };
        }

        /// <summary>
        /// Takes a path and verifies that we have a node with that name.
        /// It is meant to be a helper method for CreateFolderNodes().
        /// For some scenario it may be useful to override.
        /// </summary>
        /// <param name="path">full path to the subfolder we want to verify.</param>
        /// <param name="parent">the parent node where to add the subfolder if it does not exist.</param>
        /// <returns>the folder node corresponding to the path.</returns>
        protected virtual FolderNode VerifySubFolderExists(string path, HierarchyNode parent)
        {
            FolderNode folderNode = null;
            Url url = new Url(this.BaseURI, path);
            string strFullPath = url.AbsoluteUrl;

            // Folders end in our storage with a backslash, so add one...
            this.ParseCanonicalName(strFullPath, out uint uiItemId);
            
            if (uiItemId != (uint)VSConstants.VSITEMID.Nil)
            {
                Debug.Assert(this.NodeFromItemId(uiItemId) is FolderNode, "Not a FolderNode");
                folderNode = (FolderNode)this.NodeFromItemId(uiItemId);
            }

            if (folderNode == null && path != null && parent != null)
            {
                // folder does not exist yet...
                // We could be in the process of loading so see if MSBuild knows about it
                ProjectElement item = null;
                foreach (MSBuild.ProjectItem folder in buildProject.GetItems(ProjectFileConstants.Folder))
                {
                    if (String.Compare(folder.EvaluatedInclude.TrimEnd('\\'), path.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        item = new ProjectElement(this, folder, false);
                        break;
                    }
                }
                // If MSBuild did not know about it, create a new one
                if (item == null)
                    item = this.AddFolderToMsBuild(path);
                folderNode = this.CreateFolderNode(path, item);
                parent.AddChild(folderNode);
            }

            return folderNode;
        }

        /// <summary>
        /// To support virtual folders, override this method to return your own folder nodes
        /// </summary>
        /// <param name="path">Path to store for this folder</param>
        /// <param name="element">Element corresponding to the folder</param>
        /// <returns>A FolderNode that can then be added to the hierarchy</returns>
        protected internal virtual FolderNode CreateFolderNode(string path, ProjectElement element)
        {
            FolderNode folderNode = new FolderNode(this, path, element);
            return folderNode;
        }

        /// <summary>
        /// Gets the list of selected HierarchyNode objects
        /// </summary>
        /// <returns>A list of HierarchyNode objects</returns>
        protected internal virtual IList<HierarchyNode> GetSelectedNodes()
        {
            // Retrieve shell interface in order to get current selection
            if(!(this.GetService(typeof(IVsMonitorSelection)) is IVsMonitorSelection monitorSelection))
            {
                throw new InvalidOperationException();
            }

            List<HierarchyNode> selectedNodes = new List<HierarchyNode>();
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainer = IntPtr.Zero;
            try
            {
                // Get the current project hierarchy, project item, and selection container for the current selection
                // If the selection spans multiple hierarchies, hierarchyPtr is Zero
                ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out uint itemid,
                    out IVsMultiItemSelect multiItemSelect, out selectionContainer));

                // We only care if there are one ore more nodes selected in the tree
                if (itemid != VSConstants.VSITEMID_NIL && hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;

                    if (itemid != VSConstants.VSITEMID_SELECTION)
                    {
                        // This is a single selection. Compare hierarchy with our hierarchy and get node from item ID
                        if (Utilities.IsSameComObject(this.InteropSafeIVsHierarchy, hierarchy))
                        {
                            HierarchyNode node = this.NodeFromItemId(itemid);
                            if (node != null)
                            {
                                selectedNodes.Add(node);
                            }
                        }
                        else
                        {
                            NestedProjectNode node = this.GetNestedProjectForHierarchy(hierarchy);
                            if (node != null)
                            {
                                selectedNodes.Add(node);
                            }
                        }
                    }
                    else if (multiItemSelect != null)
                    {
                        // This is a multiple item selection.

                        //Get number of items selected and also determine if the items are located in more than one hierarchy
                        ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out uint numberOfSelectedItems, out int isSingleHierarchyInt));
                        bool isSingleHierarchy = (isSingleHierarchyInt != 0);

                        // Now loop all selected items and add to the list only those that are selected within this hierarchy
                        if (!isSingleHierarchy || (isSingleHierarchy && Utilities.IsSameComObject(this.InteropSafeIVsHierarchy, hierarchy)))
                        {
                            Debug.Assert(numberOfSelectedItems > 0, "Bad number of selected items");
                            VSITEMSELECTION[] vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
                            uint flags = (isSingleHierarchy) ? (uint)__VSGSIFLAGS.GSI_fOmitHierPtrs : 0;
                            ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(flags, numberOfSelectedItems, vsItemSelections));
                            foreach (VSITEMSELECTION vsItemSelection in vsItemSelections)
                            {
                                if (isSingleHierarchy || Utilities.IsSameComObject(this.InteropSafeIVsHierarchy, vsItemSelection.pHier))
                                {
                                    HierarchyNode node = this.NodeFromItemId(vsItemSelection.itemid);
                                    if (node != null)
                                    {
                                        selectedNodes.Add(node);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
                if (selectionContainer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainer);
                }
            }

            return selectedNodes;
        }

        /// <summary>
        /// Recursively walks the hierarchy nodes and redraws the state icons
        /// </summary>
        protected internal override void UpdateSccStateIcons()
        {
            if (this.FirstChild == null)
            {
                return;
            }

            for (HierarchyNode n = this.FirstChild; n != null; n = n.NextSibling)
            {
                n.UpdateSccStateIcons();
            }
        }

        // !EFW
        /// <summary>
        /// Handles the Shows All Files command.
        /// </summary>
        /// <returns>Returns <c>S_OK</c> if successful or an error code if not</returns>
        protected internal virtual int ShowAllFiles()
        {
            return this.ToggleShowAllFiles();
        }

        /// <summary>
        /// Handles the Add web reference command.
        /// </summary>
        /// <returns></returns>
        protected internal virtual int AddWebReference()
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Unloads the project.
        /// </summary>
        /// <returns></returns>
        protected internal virtual int UnloadProject()
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Handles the clean project command.
        /// </summary>
        /// <returns></returns>
        protected virtual int CleanProject()
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Reload project from project file
        /// </summary>
        protected virtual void Reload()
        {
            Debug.Assert(this.buildEngine != null, "There is no build engine defined for this project");

            try
            {
                this.disableQueryEdit = true;

                this.isClosed = false;
                this.eventTriggeringFlag = ProjectNode.EventTriggering.DoNotTriggerHierarchyEvents | ProjectNode.EventTriggering.DoNotTriggerTrackerEvents;

                SetBuildProject(Utilities.ReinitializeMsBuildProject(this.buildEngine, this.filename, this.buildProject));

                // Load the guid
                this.SetProjectGuidFromProjectFile();

                this.ProcessReferences();

                this.ProcessFiles();

                this.ProcessFolders();

                this.LoadNonBuildInformation();

                this.InitSccInfo();

                this.RegisterSccProject();
            }
            finally
            {
                this.SetProjectFileDirty(false);
                this.eventTriggeringFlag = ProjectNode.EventTriggering.TriggerAll;
                this.disableQueryEdit = false;
            }
        }

        /// <summary>
        /// Renames the project file
        /// </summary>
        /// <param name="newFile">The full path of the new project file.</param>
        protected virtual void RenameProjectFile(string newFile)
        {
            IVsUIShell shell = this.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;
            Debug.Assert(shell != null, "Could not get the UI shell from the project");
            if (shell == null)
            {
                throw new InvalidOperationException();
            }

            // Do some name validation
            if (Utilities.ContainsInvalidFileNameChars(newFile))
            {
                throw new InvalidOperationException(SR.GetString(SR.ErrorInvalidProjectName, CultureInfo.CurrentUICulture));
            }

            // Figure out what the new full name is
            string oldFile = this.Url;

            IVsSolution vsSolution = (IVsSolution)this.GetService(typeof(SVsSolution));

            if (ErrorHandler.Succeeded(vsSolution.QueryRenameProject(this.InteropSafeIVsProject3, oldFile, newFile, 0, out int canContinue))
                && canContinue != 0)
            {
                bool isFileSame = NativeMethods.IsSamePath(oldFile, newFile);

                // If file already exist and is not the same file with different casing
                if (!isFileSame && File.Exists(newFile))
                {
                    // Prompt the user for replace
                    string message = SR.GetString(SR.FileAlreadyExists, newFile);

                    if (!Utilities.IsInAutomationFunction(this.Site))
                    {
                        if (!VsShellUtilities.PromptYesNo(message, null, OLEMSGICON.OLEMSGICON_WARNING, shell))
                        {
                            throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(message);
                    }

                    // Delete the destination file after making sure it is not read only
                    File.SetAttributes(newFile, FileAttributes.Normal);
                    File.Delete(newFile);
                }

                SuspendFileChanges fileChanges = new SuspendFileChanges(this.Site, this.filename);
                fileChanges.Suspend();
                try
                {
                    // Actual file rename
                    this.SaveMSBuildProjectFileAs(newFile);

                    this.SetProjectFileDirty(false);

                    if (!isFileSame)
                    {
                        // Now that the new file name has been created delete the old one.
                        // TODO: Handle source control issues.
                        File.SetAttributes(oldFile, FileAttributes.Normal);
                        File.Delete(oldFile);
                    }

                    this.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_Caption, 0);

                    // Update solution
                    ErrorHandler.ThrowOnFailure(vsSolution.OnAfterRenameProject(this.InteropSafeIVsProject3, oldFile, newFile, 0));

                    ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(0));
                }
                finally
                {
                    fileChanges.Resume();
                }
            }
            else
            {
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
            }
        }

        /// <summary>
        /// Called by the project to know if the item is a file (that is part of the project)
        /// or an intermediate file used by the MSBuild tasks/targets
        /// Override this method if your project has more types or different ones
        /// </summary>
        /// <param name="type">Type name</param>
        /// <returns>True = items of this type should be included in the project</returns>
        protected virtual bool IsItemTypeFileType(string type)
        {
            if (String.Compare(type, BuildAction.Compile.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, BuildAction.Content.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, BuildAction.EmbeddedResource.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, BuildAction.None.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            // we don't know about this type, so ignore it.
            return false;
        }

        /// <summary>
        /// Filter items that should not be processed as file items. Example: Folders and References.
        /// </summary>
        protected virtual bool FilterItemTypeToBeAddedToHierarchy(string itemType)
        {
            return (String.Compare(itemType, ProjectFileConstants.Reference, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(itemType, ProjectFileConstants.ProjectReference, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(itemType, ProjectFileConstants.COMReference, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(itemType, ProjectFileConstants.Folder, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(itemType, ProjectFileConstants.WebReference, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(itemType, ProjectFileConstants.WebReferenceFolder, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Associate window output pane to the build logger
        /// </summary>
        /// <param name="output"></param>
        protected virtual void SetOutputLogger(IVsOutputWindowPane output)
        {
            // Create our logger, if it was not specified
            if (!this.useProvidedLogger || this.buildLogger == null)
            {
                // Because we may be aggregated, we need to make sure to get the outer IVsHierarchy
                IntPtr unknown = IntPtr.Zero;
                IVsHierarchy hierarchy = null;
                try
                {
                    unknown = Marshal.GetIUnknownForObject(this);
                    hierarchy = Marshal.GetTypedObjectForIUnknown(unknown, typeof(IVsHierarchy)) as IVsHierarchy;
                }
                finally
                {
                    if (unknown != IntPtr.Zero)
                        Marshal.Release(unknown);
                }
                // Create the logger
                this.BuildLogger = new IDEBuildLogger(output, this.TaskProvider, hierarchy);

                // To retrieve the verbosity level, the build logger depends on the registry root 
                // (otherwise it will used an hard-coded default)
                if(this.GetService(typeof(SLocalRegistry)) is ILocalRegistry2 registry)
                {
                    ErrorHandler.ThrowOnFailure(registry.GetLocalRegistryRoot(out string registryRoot));

                    if(!String.IsNullOrEmpty(registryRoot) && (this.BuildLogger is IDEBuildLogger logger))
                    {
                        logger.BuildVerbosityRegistryRoot = registryRoot;
                        logger.ErrorString = this.ErrorString;
                        logger.WarningString = this.WarningString;
                    }
                }
            }
            else
            {
                ((IDEBuildLogger)this.BuildLogger).OutputWindowPane = output;
            }
        }

        /// <summary>
        /// Set configuration properties for a specific configuration
        /// </summary>
        /// <param name="config">configuration name</param>
        protected virtual void SetBuildConfigurationProperties(string config)
        {
            ProjectOptions options = null;

            if (!String.IsNullOrEmpty(config))
            {
                options = this.GetProjectOptions(config);
            }

            if (options != null && this.buildProject != null)
            {
                // Make sure the project configuration is set properly
                this.SetConfiguration(config);
            }
        }

        /// <summary>
        /// This execute an MSBuild target for a design-time build.
        /// </summary>
        /// <param name="target">Name of the MSBuild target to execute</param>
        /// <returns>Result from executing the target (success/failure)</returns>
        /// <remarks>
        /// If you depend on the items/properties generated by the target
        /// you should be aware that any call to BuildTarget on any project
        /// will reset the list of generated items/properties
        /// </remarks>
        protected virtual MSBuildResult InvokeMsBuild(string target)
        {
            MSBuildResult result = MSBuildResult.Failed;
            const bool designTime = true;
            bool requiresUIThread = UIThread.Instance.IsUIThread; // we don't run tasks that require calling the STA thread, so unless we're ON it, we don't need it.

            BuildSubmission submission = null;

            if (this.buildProject != null)
            {
                if (!TryBeginBuild(designTime, requiresUIThread))
                {
                    throw new InvalidOperationException("A build is already in progress.");
                }

                try
                {
                    // Do the actual Build
                    string[] targetsToBuild = new string[target != null ? 1 : 0];
                    if (target != null)
                    {
                        targetsToBuild[0] = target;
                    }

                    currentConfig = BuildProject.CreateProjectInstance();

                    BuildRequestData requestData = new BuildRequestData(currentConfig, targetsToBuild, this.BuildProject.ProjectCollection.HostServices, BuildRequestDataFlags.ReplaceExistingProjectInstance);
                    submission = BuildManager.DefaultBuildManager.PendBuildRequest(requestData);
                    if (this.Site.GetService(typeof(SVsBuildManagerAccessor)) is IVsBuildManagerAccessor accessor)
                    {
                        ErrorHandler.ThrowOnFailure(accessor.RegisterLogger(submission.SubmissionId, this.buildLogger));
                    }

                    BuildResult buildResult = submission.Execute();

                    result = (buildResult.OverallResult == BuildResultCode.Success) ? MSBuildResult.Successful : MSBuildResult.Failed;
                }
                finally
                {
                    EndBuild(submission, designTime, requiresUIThread);
                }
            }

            return result;
        }

        /// <summary>
        /// Start MSBuild build submission
        /// </summary>
        /// If buildKind is ASync, this method starts the submission and returns. uiThreadCallback will be called on UI thread once submissions completes.
        /// if buildKind is Sync, this method executes the submission and runs uiThreadCallback
        /// <param name="buildKind">Is it a Sync or ASync build</param>
        /// <param name="target">target to build</param>
        /// <param name="uiThreadCallback">callback to be run UI thread </param>
        /// <returns>A Build submission instance.</returns>
        protected virtual BuildSubmission DoMSBuildSubmission(BuildKind buildKind, string target, Action<MSBuildResult, string> uiThreadCallback)
        {
            const bool designTime = false;
            bool requiresUIThread = buildKind == BuildKind.Sync && UIThread.Instance.IsUIThread; // we don't run tasks that require calling the STA thread, so unless we're ON it, we don't need it.

            IVsBuildManagerAccessor accessor = (IVsBuildManagerAccessor)this.Site.GetService(typeof(SVsBuildManagerAccessor));
            if (accessor == null)
            {
                throw new InvalidOperationException();
            }

            if (!TryBeginBuild(designTime, requiresUIThread))
            {
                uiThreadCallback?.Invoke(MSBuildResult.Failed, target);
                return null;
            }

            string[] targetsToBuild = new string[target != null ? 1 : 0];

            if (target != null)
            {
                targetsToBuild[0] = target;
            }

            ProjectInstance projectInstance = BuildProject.CreateProjectInstance();

            projectInstance.SetProperty(GlobalProperty.VisualStudioStyleErrors.ToString(), "true");
            projectInstance.SetProperty("UTFOutput", "true");
            projectInstance.SetProperty(GlobalProperty.BuildingInsideVisualStudio.ToString(), "true");

            this.BuildProject.ProjectCollection.HostServices.SetNodeAffinity(projectInstance.FullPath, NodeAffinity.InProc);
            BuildRequestData requestData = new BuildRequestData(projectInstance, targetsToBuild, this.BuildProject.ProjectCollection.HostServices, BuildRequestDataFlags.ReplaceExistingProjectInstance);
            BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(requestData);
            try
            {
                if (useProvidedLogger && buildLogger != null)
                {
                    ErrorHandler.ThrowOnFailure(accessor.RegisterLogger(submission.SubmissionId, buildLogger));
                }

                if (buildKind == BuildKind.Async)
                {
                    submission.ExecuteAsync(sub =>
                    {
                        UIThread.Instance.Run(() =>
                        {
                            this.FlushBuildLoggerContent();
                            EndBuild(sub, designTime, requiresUIThread);
                            uiThreadCallback((sub.BuildResult.OverallResult == BuildResultCode.Success) ? MSBuildResult.Successful : MSBuildResult.Failed, target);
                        });
                    }, null);
                }
                else
                {
                    submission.Execute();
                    EndBuild(submission, designTime, requiresUIThread);
                    MSBuildResult msbuildResult = (submission.BuildResult.OverallResult == BuildResultCode.Success) ? MSBuildResult.Successful : MSBuildResult.Failed;
                    uiThreadCallback?.Invoke(msbuildResult, target);
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
                EndBuild(submission, designTime, requiresUIThread);
                uiThreadCallback?.Invoke(MSBuildResult.Failed, target);

                throw;
            }

            return submission;
        }

        /// <summary>
        /// Initialize common project properties with default value if they are empty
        /// </summary>
        /// <remarks>The following common project properties are defaulted to projectName (if empty):
        ///    AssemblyName, Name and RootNamespace.
        /// If the project filename is not set then no properties are set</remarks>
        protected virtual void InitializeProjectProperties()
        {
            // Get projectName from project filename. Return if not set
            string projectName = Path.GetFileNameWithoutExtension(this.filename);
            if (String.IsNullOrEmpty(projectName))
            {
                return;
            }

            if (String.IsNullOrEmpty(GetProjectProperty(ProjectFileConstants.AssemblyName)))
            {
                SetProjectProperty(ProjectFileConstants.AssemblyName, projectName);
            }
            if (String.IsNullOrEmpty(GetProjectProperty(ProjectFileConstants.Name)))
            {
                SetProjectProperty(ProjectFileConstants.Name, projectName);
            }
            if (String.IsNullOrEmpty(GetProjectProperty(ProjectFileConstants.RootNamespace)))
            {
                SetProjectProperty(ProjectFileConstants.RootNamespace, projectName);
            }
        }

        /// <summary>
        /// Factory method for configuration provider
        /// </summary>
        /// <returns>Configuration provider created</returns>
        protected virtual ConfigProvider CreateConfigProvider()
        {
            return new ConfigProvider(this);
        }

        /// <summary>
        /// Factory method for reference container node
        /// </summary>
        /// <returns>ReferenceContainerNode created</returns>
        protected virtual ReferenceContainerNode CreateReferenceContainerNode()
        {
            return new ReferenceContainerNode(this);
        }

        /// <summary>
        /// Saves the project file on a new name.
        /// </summary>
        /// <param name="newFileName">The new name of the project file.</param>
        /// <returns>Success value or an error code.</returns>
        protected virtual int SaveAs(string newFileName)
        {
            Debug.Assert(!String.IsNullOrEmpty(newFileName), "Cannot save project file for an empty or null file name");
            if (String.IsNullOrEmpty(newFileName))
            {
                throw new ArgumentNullException(nameof(newFileName));
            }

            newFileName = newFileName.Trim();

            string errorMessage = String.Empty;

            if (newFileName.Length > NativeMethods.MAX_PATH)
            {
                errorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.PathTooLong, CultureInfo.CurrentUICulture), newFileName);
            }
            else
            {
                string fileName = String.Empty;

                try
                {
                    fileName = Path.GetFileNameWithoutExtension(newFileName);
                }
                // We want to be consistent in the error message and exception we throw. fileName could be for example #�&%"�&"%  and that would trigger an ArgumentException on Path.IsRooted.
                catch (ArgumentException)
                {
                    errorMessage = SR.GetString(SR.ErrorInvalidFileName, CultureInfo.CurrentUICulture);
                }

                if (errorMessage.Length == 0)
                {
                    // If there is no filename or it starts with a leading dot issue an error message and quit.
                    // For some reason the save as dialog box allows to save files like "......ext"
                    if (String.IsNullOrEmpty(fileName) || fileName[0] == '.')
                    {
                        errorMessage = SR.GetString(SR.FileNameCannotContainALeadingPeriod, CultureInfo.CurrentUICulture);
                    }
                    else if (Utilities.ContainsInvalidFileNameChars(newFileName))
                    {
                        errorMessage = SR.GetString(SR.ErrorInvalidFileName, CultureInfo.CurrentUICulture);
                    }
                    else
                    {
                        string url = Path.GetDirectoryName(newFileName);
                        string oldUrl = Path.GetDirectoryName(this.Url);

                        if (!NativeMethods.IsSamePath(oldUrl, url))
                        {
                            errorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.SaveOfProjectFileOutsideCurrentDirectory, CultureInfo.CurrentUICulture), this.ProjectFolder);
                        }
                    }
                }
            }
            if (errorMessage.Length > 0)
            {
                // If it is not called from an automation method show a dialog box.
                if (!Utilities.IsInAutomationFunction(this.Site))
                {
                    string title = null;
                    OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                    OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
                    OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                    VsShellUtilities.ShowMessageBox(this.Site, title, errorMessage, icon, buttons, defaultButton);
                    return VSConstants.OLE_E_PROMPTSAVECANCELLED;
                }

                throw new InvalidOperationException(errorMessage);
            }

            string oldName = this.filename;

            IVsSolution solution = this.Site.GetService(typeof(IVsSolution)) as IVsSolution;
            Debug.Assert(solution != null, "Could not retrieve the solution form the service provider");
            if (solution == null)
            {
                throw new InvalidOperationException();
            }

            ErrorHandler.ThrowOnFailure(solution.QueryRenameProject(this.InteropSafeIVsProject3, this.filename, newFileName, 0, out int canRenameContinue));

            if (canRenameContinue == 0)
            {
                return VSConstants.OLE_E_PROMPTSAVECANCELLED;
            }

            SuspendFileChanges fileChanges = new SuspendFileChanges(this.Site, oldName);
            fileChanges.Suspend();
            try
            {
                // Save the project file and project file related properties.
                this.SaveMSBuildProjectFileAs(newFileName);

                this.SetProjectFileDirty(false);


                // TODO: If source control is enabled check out the project file.

                //Redraw.
                this.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_Caption, 0);

                ErrorHandler.ThrowOnFailure(solution.OnAfterRenameProject(this.InteropSafeIVsProject3, oldName, this.filename, 0));

                IVsUIShell shell = this.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;
                Debug.Assert(shell != null, "Could not get the UI shell from the project");
                if (shell == null)
                {
                    throw new InvalidOperationException();
                }
                ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(0));
            }
            finally
            {
                fileChanges.Resume();
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Saves project file related information to the new file name. It also calls MSBuild API to save the project file.
        /// It is called by the SaveAs method and the SetEditLabel before the project file rename related events are triggered. 
        /// An implementer can override this method to provide specialized semantics on how the project file is renamed in the MSBuild file.
        /// </summary>
        /// <param name="newFileName">The new full path of the project file</param>
        protected virtual void SaveMSBuildProjectFileAs(string newFileName)
        {
            Debug.Assert(!String.IsNullOrEmpty(newFileName), "Cannot save project file for an empty or null file name");

            this.buildProject.FullPath = newFileName;

            this.filename = newFileName;

            string newFileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFileName);

            // Refresh solution explorer
            this.SetProjectProperty(ProjectFileConstants.Name, newFileNameWithoutExtension);

            // Saves the project file on disk.
            this.buildProject.Save(newFileName);

        }

        // !EFW - Added "internal" to allow calling for Include in Project
        /// <summary>
        /// Adds a file to the MSBuild project.
        /// </summary>
        /// <param name="file">The file to be added.</param>
        /// <returns>A ProjectElement describing the newly added file.</returns>
        protected internal virtual ProjectElement AddFileToMsBuild(string file)
        {
            ProjectElement newItem;

            // !EFW - Just make it relative to the project file to support linked files
            string itemPath = PackageUtilities.MakeRelative(this.FileName, file);

            if (this.IsCodeFile(itemPath))
            {
                newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.Compile);
                newItem.SetMetadata(ProjectFileConstants.SubType, ProjectFileAttributeValue.Code);
            }
            else if (this.IsEmbeddedResource(itemPath))
            {
                newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.EmbeddedResource);
            }
            else
            {
                newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.Content);
                newItem.SetMetadata(ProjectFileConstants.SubType, ProjectFileConstants.Content);
            }

            return newItem;
        }

        // !EFW - Added "internal" to allow calling for Include in Project
        /// <summary>
        /// Adds a folder to the MSBuild project.
        /// </summary>
        /// <param name="folder">The folder to be added.</param>
        /// <returns>A project element describing the newly added folder.</returns>
        protected internal virtual ProjectElement AddFolderToMsBuild(string folder)
        {
            ProjectElement newItem;

            string itemPath = PackageUtilities.MakeRelativeIfRooted(folder, this.BaseURI);
            Debug.Assert(!Path.IsPathRooted(itemPath), "Cannot add item with full path.");

            newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.Folder);

            return newItem;
        }

        /// <summary>
        /// Determines whether an item can be overwritten in the hierarchy.
        /// </summary>
        /// <param name="originalFileName">The original filename.</param>
        /// <param name="computedNewFileName">The computed new file name, that will be copied to the project directory or into the folder .</param>
        /// <returns>S_OK for success, or an error message</returns>
        protected virtual int CanOverwriteExistingItem(string originalFileName, string computedNewFileName)
        {
            if (String.IsNullOrEmpty(originalFileName) || String.IsNullOrEmpty(computedNewFileName))
            {
                return VSConstants.E_INVALIDARG;
            }

            string title = String.Empty;
            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
            OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
            OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;

            // If the document is open then return error message.
            bool isOpen = VsShellUtilities.IsDocumentOpen(this.Site, computedNewFileName, Guid.Empty, out _, out _, out _);

            string message;
            if(isOpen)
            {
                message = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.CannotAddFileThatIsOpenInEditor, CultureInfo.CurrentUICulture), Path.GetFileName(computedNewFileName));
                VsShellUtilities.ShowMessageBox(this.Site, title, message, icon, buttons, defaultButton);
                return VSConstants.E_ABORT;
            }


            // File already exists in project... message box
            message = SR.GetString(SR.FileAlreadyInProject, CultureInfo.CurrentUICulture);
            icon = OLEMSGICON.OLEMSGICON_QUERY;
            buttons = OLEMSGBUTTON.OLEMSGBUTTON_YESNO;
            int msgboxResult = VsShellUtilities.ShowMessageBox(this.Site, title, message, icon, buttons, defaultButton);
            if (msgboxResult != NativeMethods.IDYES)
            {
                return (int)OleConstants.OLECMDERR_E_CANCELED;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Handle overwriting of an existing item in the hierarchy.
        /// </summary>
        /// <param name="existingNode">The node that exists.</param>
        protected virtual void OverwriteExistingItem(HierarchyNode existingNode)
        {

        }

        /// <summary>
        /// Adds a new file node to the hierarchy.
        /// </summary>
        /// <param name="parentNode">The parent of the new fileNode</param>
        /// <param name="fileName">The file name</param>
        protected virtual void AddNewFileNodeToHierarchy(HierarchyNode parentNode, string fileName)
        {
            if (parentNode == null)
            {
                throw new ArgumentNullException(nameof(parentNode));
            }

            HierarchyNode child;

            // In the case of subitem, we want to create dependent file node
            // and set the DependentUpon property
            if (this.canFileNodesHaveChilds && (parentNode is FileNode || parentNode is DependentFileNode))
            {
                child = this.CreateDependentFileNode(fileName);
                child.ItemNode.SetMetadata(ProjectFileConstants.DependentUpon, parentNode.ItemNode.GetMetadata(ProjectFileConstants.Include));

                // Make sure to set the HasNameRelation flag on the dependent node if it is related to the parent by name
                if (!child.HasParentNodeNameRelation && String.Compare(child.GetRelationalName(), parentNode.GetRelationalName(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    child.HasParentNodeNameRelation = true;
                }
            }
            else
            {
                //Create and add new file node to the project
                child = this.CreateFileNode(fileName);
            }

            parentNode.AddChild(child);

            // TODO : Revisit the VSADDFILEFLAGS here. Can it be a nested project?
            this.tracker.OnItemAdded(fileName, VSADDFILEFLAGS.VSADDFILEFLAGS_NoFlags);
        }

        // !EFW
        /// <summary>
        /// Adds a new linked file node to the hierarchy.
        /// </summary>
        /// <param name="parentNode">The parent of the new fileNode</param>
        /// <param name="linkedFileName">The linked filename</param>
        /// <param name="fileName">The file name in the project hierarchy</param>
        protected virtual void AddNewLinkedFileNodeToHierarchy(
          HierarchyNode parentNode, string linkedFileName, string fileName)
        {
            // NOTE: The general process should match that of
            // AddNewFileNodeToHierarchy with the exception of the filename
            // used and the addition of the Link metadata.
            HierarchyNode child;

            // In the case of subitem, we want to create dependent file node
            // and set the DependentUpon property
            if(this.canFileNodesHaveChilds && (parentNode is FileNode || parentNode is DependentFileNode))
            {
                child = this.CreateDependentFileNode(linkedFileName);
                child.ItemNode.SetMetadata(ProjectFileConstants.Link,
                    PackageUtilities.MakeRelative(base.ProjectMgr.FileName, fileName));
                child.ItemNode.SetMetadata(ProjectFileConstants.DependentUpon,
                    parentNode.ItemNode.GetMetadata(ProjectFileConstants.Include));

                // Make sure to set the HasNameRelation flag on the dependent
                // node if it is related to the parent by name
                if(!child.HasParentNodeNameRelation && String.Compare(child.GetRelationalName(),
                  parentNode.GetRelationalName(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    child.HasParentNodeNameRelation = true;
                }
            }
            else
            {
                //Create and add new file node to the project
                child = this.CreateFileNode(linkedFileName);
                child.ItemNode.SetMetadata(ProjectFileConstants.Link,
                    PackageUtilities.MakeRelative(base.ProjectMgr.FileName, fileName));
            }

            parentNode.AddChild(child);

            // TODO : Revisit the VSADDFILEFLAGS here. Can it be a nested project?
            this.tracker.OnItemAdded(fileName, VSADDFILEFLAGS.VSADDFILEFLAGS_NoFlags);
        }

        /// <summary>
        /// Defines whether the current mode of the project is in a suppress command mode.
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool IsCurrentStateASuppressCommandsMode()
        {
            if (VsShellUtilities.IsSolutionBuilding(this.Site))
            {
                return true;
            }

            DBGMODE dbgMode = VsShellUtilities.GetDebugMode(this.Site) & ~DBGMODE.DBGMODE_EncMask;
            if (dbgMode == DBGMODE.DBGMODE_Run || dbgMode == DBGMODE.DBGMODE_Break)
            {
                return true;
            }

            return false;

        }


        /// <summary>
        /// This is the list of output groups that the configuration object should
        /// provide.
        /// The first string is the name of the group.
        /// The second string is the target name (MSBuild) for that group.
        /// 
        /// To add/remove OutputGroups, simply override this method and edit the list.
        /// 
        /// To get nice display names and description for your groups, override:
        ///        - GetOutputGroupDisplayName
        ///        - GetOutputGroupDescription
        /// </summary>
        /// <returns>List of output group name and corresponding MSBuild target</returns>
        protected internal virtual IList<KeyValuePair<string, string>> GetOutputGroupNames()
        {
            return new List<KeyValuePair<string, string>>(outputGroupNames);
        }

        /// <summary>
        /// Get the display name of the given output group.
        /// </summary>
        /// <param name="canonicalName">Canonical name of the output group</param>
        /// <returns>Display name</returns>
        protected internal virtual string GetOutputGroupDisplayName(string canonicalName)
        {
            string result = SR.GetString(String.Format(CultureInfo.InvariantCulture, "Output{0}", canonicalName), CultureInfo.CurrentUICulture);
            if (String.IsNullOrEmpty(result))
                result = canonicalName;
            return result;
        }

        /// <summary>
        /// Get the description of the given output group.
        /// </summary>
        /// <param name="canonicalName">Canonical name of the output group</param>
        /// <returns>Description</returns>
        protected internal virtual string GetOutputGroupDescription(string canonicalName)
        {
            string result = SR.GetString(String.Format(CultureInfo.InvariantCulture, "Output{0}Description", canonicalName), CultureInfo.CurrentUICulture);
            if (String.IsNullOrEmpty(result))
                result = canonicalName;
            return result;
        }

        /// <summary>
        /// Set the configuration in MSBuild.
        /// This does not get persisted and is used to evaluate MSBuild conditions
        /// which are based on the $(Configuration) property.
        /// </summary>
        protected internal virtual void SetCurrentConfiguration()
        {
            if (this.BuildInProgress)
            {
                // we are building so this should already be the current configuration
                return;
            }

            // Can't ask for the active config until the project is opened, so do nothing in that scenario
            if (!this.projectOpened)
                return;

            EnvDTE.Project automationObject = this.GetAutomationObject() as EnvDTE.Project;

            this.SetConfiguration(Utilities.GetActiveConfigurationName(automationObject));
        }

        /// <summary>
        /// Set the configuration property in MSBuild.
        /// This does not get persisted and is used to evaluate MSBuild conditions
        /// which are based on the $(Configuration) property.
        /// </summary>
        /// <param name="config">Configuration name</param>
        protected internal virtual void SetConfiguration(string config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Can't ask for the active config until the project is opened, so do nothing in that scenario
            if (!projectOpened || String.IsNullOrWhiteSpace(config))
                return;

            // We cannot change properties during the build so if the config
            // we want to set is the current, we do nothing otherwise we fail.
            if (this.BuildInProgress)
            {
                EnvDTE.Project automationObject = this.GetAutomationObject() as EnvDTE.Project;
                string currentConfigName = Utilities.GetActiveConfigurationName(automationObject);
                bool configsAreEqual = String.Compare(currentConfigName, config, StringComparison.OrdinalIgnoreCase) == 0;

                if (configsAreEqual)
                {
                    return;
                }
                throw new InvalidOperationException();
            }

            bool propertiesChanged = this.buildProject.SetGlobalProperty(ProjectFileConstants.Configuration, config);
            if (this.currentConfig == null || propertiesChanged)
            {
                this.currentConfig = this.buildProject.CreateProjectInstance();
            }

            if (propertiesChanged || this.designTimeAssemblyResolution == null)
            {
                if (this.designTimeAssemblyResolution == null)
                {
                    this.designTimeAssemblyResolution = new DesignTimeAssemblyResolution();
                }

                this.designTimeAssemblyResolution.Initialize(this);
            }

            this.options = null;
        }

        /// <summary>
        /// Loads reference items from the project file into the hierarchy.
        /// </summary>
        protected internal virtual void ProcessReferences()
        {
            IReferenceContainer container = GetReferenceContainer();
            if (null == container)
            {
                // Process References
                ReferenceContainerNode referencesFolder = CreateReferenceContainerNode();

                if (null == referencesFolder)
                {
                    // This project type does not support references or there is a problem
                    // creating the reference container node.
                    // In both cases there is no point to try to process references, so exit.
                    return;
                }

                this.AddChild(referencesFolder);
                container = referencesFolder;
            }

            // Load the references.
            container.LoadReferencesFromBuildProject(buildProject);
        }

        /// <summary>
        /// Loads folders from the project file into the hierarchy.
        /// </summary>
        protected internal virtual void ProcessFolders()
        {
            // Process Folders (useful to persist empty folder)
            foreach (MSBuild.ProjectItem folder in this.buildProject.GetItems(ProjectFileConstants.Folder))
            {
                string strPath = folder.EvaluatedInclude;

                // We do not need any special logic for assuring that a folder is only added once to the UI hierarchy.
                // The below method will only add once the folder to the UI hierarchy
                this.CreateFolderNodes(strPath);
            }
        }

        /// <summary>
        /// Loads file items from the project file into the hierarchy.
        /// </summary>
        protected internal virtual void ProcessFiles()
        {
            List<String> subitemsKeys = new List<String>();
            Dictionary<String, MSBuild.ProjectItem> subitems = new Dictionary<String, MSBuild.ProjectItem>();

            // Define a set for our build items. The value does not really matter here.
            Dictionary<String, MSBuild.ProjectItem> items = new Dictionary<String, MSBuild.ProjectItem>();

            // !EFW Converted to list.  If a new folder is found and added to the project, it alters
            // the underlying collection.

            // Process Files
            foreach (MSBuild.ProjectItem item in this.buildProject.Items.ToList())
            {
                // Ignore the item if it is a reference or folder
                if (this.FilterItemTypeToBeAddedToHierarchy(item.ItemType))
                    continue;

                // MSBuilds tasks/targets can create items (such as object files),
                // such items are not part of the project per se, and should not be displayed
                // so ignore those items.
                if (!this.IsItemTypeFileType(item.ItemType))
                    continue;

                // If the item is already contained do nothing.
                // TODO: possibly report in the error list that the item is already contained in the project file similar to Language projects.
                if (items.ContainsKey(item.EvaluatedInclude.ToUpperInvariant()))
                    continue;

                // Make sure that we do not want to add the item, dependent, or independent twice to the UI hierarchy
                items.Add(item.EvaluatedInclude.ToUpperInvariant(), item);

                string dependentOf = item.GetMetadataValue(ProjectFileConstants.DependentUpon);

                if (!this.CanFileNodesHaveChilds || String.IsNullOrEmpty(dependentOf))
                {
                    AddIndependentFileNode(item);
                }
                else
                {
                    // We will process dependent items later.
                    // Note that we use 2 lists as we want to remove elements from
                    // the collection as we loop through it
                    subitemsKeys.Add(item.EvaluatedInclude);
                    subitems.Add(item.EvaluatedInclude, item);
                }
            }

            // Now process the dependent items.
            if (this.CanFileNodesHaveChilds)
            {
                ProcessDependentFileNodes(subitemsKeys, subitems);
            }

        }

        /// <summary>
        /// Processes dependent file nodes from list of subitems. Multi level supported, but not circular dependencies.
        /// </summary>
        /// <param name="subitemsKeys">List of sub item keys </param>
        /// <param name="subitems"></param>
        protected internal virtual void ProcessDependentFileNodes(IList<String> subitemsKeys, Dictionary<String, MSBuild.ProjectItem> subitems)
        {
            if (subitemsKeys == null || subitems == null)
            {
                return;
            }

            foreach (string key in subitemsKeys)
            {
                // A previous pass could have removed the key so make sure it still needs to be added
                if (!subitems.ContainsKey(key))
                    continue;

                AddDependentFileNode(subitems, key);
            }
        }

        /// <summary>
        /// For flavored projects which implement IPersistXMLFragment, load the information now
        /// </summary>
        protected internal virtual void LoadNonBuildInformation()
        {
            if(this.InteropSafeIVsHierarchy is IPersistXMLFragment outerHierarchy)
            {
                this.LoadXmlFragment(outerHierarchy, null);
            }
        }

        /// <summary>
        /// Used to sort nodes in the hierarchy.
        /// </summary>
        protected internal virtual int CompareNodes(HierarchyNode node1, HierarchyNode node2)
        {
            Debug.Assert(node1 != null && node2 != null);
            
            if (node1 == null)
            {
                throw new ArgumentNullException(nameof(node1));
            }

            if (node2 == null)
            {
                throw new ArgumentNullException(nameof(node2));
            }

            if (node1.SortPriority == node2.SortPriority)
            {
                return String.Compare(node2.Caption, node1.Caption, true, CultureInfo.CurrentCulture);
            }
            else
            {
                return node2.SortPriority - node1.SortPriority;
            }
        }

        /// <summary>
        /// Handles global properties related to configuration and platform changes invoked by a change in the active configuration.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event args</param>
        protected virtual void OnHandleConfigurationRelatedGlobalProperties(object sender, ActiveConfigurationChangedEventArgs eventArgs)
        {
            Debug.Assert(eventArgs != null, "Wrong hierarchy passed as event arg for the configuration change listener.");

            // If (eventArgs.Hierarchy == NULL) then we received this event because the solution configuration
            // was changed.
            // If it is not null we got the event because a project in the configuration manager has changed its active configuration.
            // We care only about our project in the default implementation.
            if (eventArgs == null || eventArgs.Hierarchy == null || !Utilities.IsSameComObject(eventArgs.Hierarchy, this.InteropSafeIVsHierarchy))
            {
                return;
            }

            if(!Utilities.TryGetActiveConfigurationAndPlatform(this.Site, this.InteropSafeIVsHierarchy, out string name, out string platform))
            {
                throw new InvalidOperationException();
            }

            this.buildProject.SetGlobalProperty(GlobalProperty.Configuration.ToString(), name);

            // If the platform is "Any CPU" then it should be set to AnyCPU, since that is the property value in MsBuild terms.
            if (String.Compare(platform, ConfigProvider.AnyCPUPlatform, StringComparison.Ordinal) == 0)
            {
                platform = ProjectFileValues.AnyCPU;
            }

            this.buildProject.SetGlobalProperty(GlobalProperty.Platform.ToString(), platform);
        }

        /// <summary>
        /// Flush any remaining content from build logger.
        /// This method is called as part of the callback method passed to the build submission during async build
        /// so that results can be printed when the build is finished.
        /// </summary>
        protected virtual void FlushBuildLoggerContent()
        {
        }
        #endregion

        #region non-virtual methods

        /// <summary>
        /// Suspends MSBuild
        /// </summary>
        public void SuspendMSBuild()
        {
            this.suspendMSBuildCounter++;
        }

        /// <summary>
        /// Resumes MSBuild.
        /// </summary>
        public void ResumeMSBuild(string config, IVsOutputWindowPane output, string target)
        {
            this.suspendMSBuildCounter--;

            if (this.suspendMSBuildCounter == 0 && this.invokeMSBuildWhenResumed)
            {
                try
                {
                    this.Build(config, output, target);
                }
                finally
                {
                    this.invokeMSBuildWhenResumed = false;
                }
            }
        }

        /// <summary>
        /// Resumes MSBuild.
        /// </summary>
        public void ResumeMSBuild(string config, string target)
        {
            this.ResumeMSBuild(config, null, target);
        }

        /// <summary>
        /// Resumes MSBuild.
        /// </summary>
        public void ResumeMSBuild(string target)
        {
            this.ResumeMSBuild(String.Empty, null, target);
        }

        /// <summary>
        /// Calls MSBuild if it is not suspended. If it is suspended then it will remember to call when MSBuild is resumed.
        /// </summary>
        public MSBuildResult CallMSBuild(string config, IVsOutputWindowPane output, string target)
        {
            if (this.suspendMSBuildCounter > 0)
            {
                // remember to invoke MSBuild
                this.invokeMSBuildWhenResumed = true;
                return MSBuildResult.Suspended;
            }
            else
            {
                return this.Build(config, output, target);
            }
        }

        /// <summary>
        /// Overloaded method. Calls MSBuild if it is not suspended. Does not log on the output window.  If it is
        /// suspended then it will remember to call when MSBuild is resumed.
        /// </summary>
        public MSBuildResult CallMSBuild(string config, string target)
        {
            return this.CallMSBuild(config, null, target);
        }

        /// <summary>
        /// Calls MSBuild if it is not suspended. Does not log and uses current configuration. If it is suspended
        /// then it will remember to call when MSBuild is resumed.
        /// </summary>
        public MSBuildResult CallMSBuild(string target)
        {
            return this.CallMSBuild(String.Empty, null, target);
        }

        /// <summary>
        /// Calls MSBuild if it is not suspended. Uses current configuration. If it is suspended then it will remember to call when MSBuild is resumed.
        /// </summary>
        public MSBuildResult CallMSBuild(string target, IVsOutputWindowPane output)
        {
            return this.CallMSBuild(String.Empty, output, target);
        }

        /// <summary>
        /// Overloaded method to invoke MSBuild
        /// </summary>
        public MSBuildResult Build(string config, IVsOutputWindowPane output, string target)
        {
            return this.Build(0, config, output, target);
        }

        /// <summary>
        /// Overloaded method to invoke MSBuild. Does not log build results to the output window pane.
        /// </summary>
        public MSBuildResult Build(string config, string target)
        {
            return this.Build(0, config, null, target);
        }

        /// <summary>
        /// Overloaded method. Invokes MSBuild using the default configuration and does without logging on the output window pane.
        /// </summary>
        public MSBuildResult Build(string target)
        {
            return this.Build(0, String.Empty, null, target);
        }

        /// <summary>
        /// Overloaded method. Invokes MSBuild using the default configuration.
        /// </summary>
        public MSBuildResult Build(string target, IVsOutputWindowPane output)
        {
            return this.Build(0, String.Empty, output, target);
        }

        /// <summary>
        /// Get the output path for a specific configuration name
        /// </summary>
        /// <param name="config">name of configuration</param>
        /// <returns>Output path</returns>
        public string GetOutputPath(string config)
        {
            this.SetConfiguration(config);
            return this.GetOutputPath(this.currentConfig);
        }

        /// <summary>
        /// Get value of Project property
        /// </summary>
        /// <param name="propertyName">Name of Property to retrieve</param>
        /// <returns>Evaluated value of property.</returns>
        public string GetProjectProperty(string propertyName)
        {
            return this.GetProjectProperty(propertyName, true);
        }

        /// <summary>
        /// Gets the unevaluated value of a project property.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        /// <returns>Unevaluated value of the property.</returns>
        public string GetProjectPropertyUnevaluated(string propertyName)
        {
            return this.buildProject.GetProperty(propertyName).UnevaluatedValue;
        }

        /// <summary>
        /// Set dirty state of project
        /// </summary>
        /// <param name="value">boolean value indicating dirty state</param>
        public void SetProjectFileDirty(bool value)
        {
            this.isDirty = value;
            if (this.isDirty)
            {
                this.lastModifiedTime = DateTime.Now;
                this.buildIsPrepared = false;
            }
        }

        /// <summary>
        /// Get output assembly for a specific configuration name
        /// </summary>
        /// <param name="config">Name of configuration</param>
        /// <returns>Name of output assembly</returns>
        public string GetOutputAssembly(string config)
        {
            ProjectOptions options = this.GetProjectOptions(config);

            return options.OutputAssembly;
        }

        /// <summary>
        /// Get Node from ItemID.
        /// </summary>
        /// <param name="itemId">ItemID for the requested node</param>
        /// <returns>Node if found</returns>
        public HierarchyNode NodeFromItemId(uint itemId)
        {
            if (VSConstants.VSITEMID_ROOT == itemId)
            {
                return this;
            }
            else if (VSConstants.VSITEMID_NIL == itemId)
            {
                return null;
            }
            else if (VSConstants.VSITEMID_SELECTION == itemId)
            {
                throw new NotImplementedException();
            }

            return (HierarchyNode)this.ItemIdMap[itemId];
        }

        /// <summary>
        /// This method return new project element, and add new MSBuild item to the project/build hierarchy
        /// </summary>
        /// <param name="file">file name</param>
        /// <param name="itemType">MSBuild item type</param>
        /// <returns>new project element</returns>
        public ProjectElement CreateMsBuildFileItem(string file, string itemType)
        {
            return new ProjectElement(this, file, itemType);
        }

        /// <summary>
        /// This method returns new project element based on existing MSBuild item. It does not modify/add project/build hierarchy at all.
        /// </summary>
        /// <param name="item">MSBuild item instance</param>
        /// <returns>wrapping project element</returns>
        public ProjectElement GetProjectElement(MSBuild.ProjectItem item)
        {
            return new ProjectElement(this, item, false);
        }

        /// <summary>
        /// Create FolderNode from Path
        /// </summary>
        /// <param name="path">Path to folder</param>
        /// <returns>FolderNode created that can be added to the hierarchy</returns>
        protected internal FolderNode CreateFolderNode(string path)
        {
            ProjectElement item = this.AddFolderToMsBuild(path);
            FolderNode folderNode = CreateFolderNode(path, item);
            return folderNode;
        }

        /// <summary>
        /// Verify if the file can be written to.
        /// Return false if the file is read only and/or not checked out
        /// and the user did not give permission to change it.
        /// Note that exact behavior can also be affected based on the SCC
        /// settings under Tools->Options.
        /// </summary>
        internal bool QueryEditProjectFile(bool suppressUI)
        {
            bool result = true;
            if (this.site == null)
            {
                // We're already zombied. Better return FALSE.
                result = false;
            }
            else if (this.disableQueryEdit)
            {
                return true;
            }
            else
            {
                if(this.GetService(typeof(SVsQueryEditQuerySave)) is IVsQueryEditQuerySave2 queryEditQuerySave)
                {   // Project path depends on server/client project
                    string path = this.filename;

                    tagVSQueryEditFlags qef = tagVSQueryEditFlags.QEF_AllowInMemoryEdits;
                    if(suppressUI)
                        qef |= tagVSQueryEditFlags.QEF_SilentMode;

                    // If we are debugging, we want to prevent our project from being reloaded. To 
                    // do this, we pass the QEF_NoReload flag
                    if(!Utilities.IsVisualStudioInDesignMode(this.Site))
                        qef |= tagVSQueryEditFlags.QEF_NoReload;

                    string[] files = new string[1];
                    files[0] = path;
                    uint[] flags = new uint[1];
                    VSQEQS_FILE_ATTRIBUTE_DATA[] attributes = new VSQEQS_FILE_ATTRIBUTE_DATA[1];

                    int hr = queryEditQuerySave.QueryEditFiles(
                                    (uint)qef,
                                    1, // 1 file
                                    files, // array of files
                                    flags, // no per file flags
                                    attributes, // no per file attributes
                                    out uint verdict,
                                    out _ /* ignore additional results */);

                    tagVSQueryEditResult qer = (tagVSQueryEditResult)verdict;
                    if(ErrorHandler.Failed(hr) || (qer != tagVSQueryEditResult.QER_EditOK))
                    {
                        if(!suppressUI && !Utilities.IsInAutomationFunction(this.Site))
                        {
                            string message = SR.GetString(SR.CancelQueryEdit, path);
                            string title = String.Empty;
                            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                            OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
                            OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                            VsShellUtilities.ShowMessageBox(this.Site, title, message, icon, buttons, defaultButton);
                        }
                        result = false;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks whether a hierarchy is a nested project.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        internal NestedProjectNode GetNestedProjectForHierarchy(IVsHierarchy hierarchy)
        {
            if(hierarchy is IVsProject3)
            {
                string mkDocument = String.Empty;
                //ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out mkDocument));

                if(!String.IsNullOrEmpty(mkDocument))
                {
                    HierarchyNode node = this.FindChild(mkDocument);

                    return node as NestedProjectNode;
                }
            }

            return null;
        }

        /// <summary>
        /// Given a node determines what is the directory that can accept files.
        /// If the node is a FoldeNode than it is the Url of the Folder.
        /// If the node is a ProjectNode it is the project folder.
        /// Otherwise (such as FileNode subitem) it delegate the resolution to the parent node.
        /// </summary>
        internal string GetBaseDirectoryForAddingFiles(HierarchyNode nodeToAddFile)
        {
            string baseDir = String.Empty;

            if (nodeToAddFile is FolderNode)
            {
                baseDir = nodeToAddFile.Url;
            }
            else if (nodeToAddFile is ProjectNode)
            {
                baseDir = this.ProjectFolder;
            }
            else if (nodeToAddFile != null)
            {
                baseDir = GetBaseDirectoryForAddingFiles(nodeToAddFile.Parent);
            }

            return baseDir;
        }

        /// <summary>
        /// For internal use only.
        /// This creates a copy of an existing configuration and add it to the project.
        /// Caller should change the condition on the PropertyGroup.
        /// If derived class want to accomplish this, they should call ConfigProvider.AddCfgsOfCfgName()
        /// It is expected that in the future MSBuild will have support for this so we don't have to
        /// do it manually.
        /// </summary>
        /// <param name="group">PropertyGroup to clone</param>
        /// <returns></returns>
        internal MSBuildConstruction.ProjectPropertyGroupElement ClonePropertyGroup(MSBuildConstruction.ProjectPropertyGroupElement group)
        {
            // Create a new (empty) PropertyGroup
            MSBuildConstruction.ProjectPropertyGroupElement newPropertyGroup = this.buildProject.Xml.AddPropertyGroup();

            // Now copy everything from the group we are trying to clone to the group we are creating
            if (!String.IsNullOrEmpty(group.Condition))
                newPropertyGroup.Condition = group.Condition;
            foreach (MSBuildConstruction.ProjectPropertyElement prop in group.Properties)
            {
                MSBuildConstruction.ProjectPropertyElement newProperty = newPropertyGroup.AddProperty(prop.Name, prop.Value);
                if (!String.IsNullOrEmpty(prop.Condition))
                    newProperty.Condition = prop.Condition;
            }

            return newPropertyGroup;
        }

        /// <summary>
        /// Get the project extensions
        /// </summary>
        /// <returns></returns>
        internal MSBuildConstruction.ProjectExtensionsElement GetProjectExtensions()
        {
            var extensionsElement = this.buildProject.Xml.ChildrenReversed.OfType<MSBuildConstruction.ProjectExtensionsElement>().FirstOrDefault();

            if (extensionsElement == null)
            {
                extensionsElement = this.buildProject.Xml.CreateProjectExtensionsElement();
                this.buildProject.Xml.AppendChild(extensionsElement);
            }

            return extensionsElement;
        }

        /// <summary>
        /// Set the xmlText as a project extension element with the id passed.
        /// </summary>
        /// <param name="id">The id of the project extension element.</param>
        /// <param name="xmlText">The value to set for a project extension.</param>
        internal void SetProjectExtensions(string id, string xmlText)
        {
            MSBuildConstruction.ProjectExtensionsElement element = this.GetProjectExtensions();

            // If it doesn't already have a value and we're asked to set it to
            // nothing, don't do anything. Same as old OM. Keeps project neat.
            if (element == null)
            {
                if (xmlText.Length == 0)
                {
                    return;
                }

                element = this.buildProject.Xml.CreateProjectExtensionsElement();
                this.buildProject.Xml.AppendChild(element);
            }

            element[id] = xmlText;
        }

        /// <summary>
        /// Register the project with the SCC manager.
        /// </summary>
        protected void RegisterSccProject()
        {
            if (this.IsSccDisabled || this.isRegisteredWithScc || String.IsNullOrEmpty(this.sccProjectName))
            {
                return;
            }

            if(this.Site.GetService(typeof(SVsSccManager)) is IVsSccManager2 sccManager)
            {
                ErrorHandler.ThrowOnFailure(sccManager.RegisterSccProject(this.InteropSafeIVsSccProject2, this.sccProjectName, this.sccAuxPath, this.sccLocalPath, this.sccProvider));

                this.isRegisteredWithScc = true;
            }
        }

        /// <summary>
        ///  Unregisters us from the SCC manager
        /// </summary>
        protected void UnRegisterProject()
        {
            if (this.IsSccDisabled || !this.isRegisteredWithScc)
            {
                return;
            }

            if(this.Site.GetService(typeof(SVsSccManager)) is IVsSccManager2 sccManager)
            {
                ErrorHandler.ThrowOnFailure(sccManager.UnregisterSccProject(this.InteropSafeIVsSccProject2));
                this.isRegisteredWithScc = false;
            }
        }

        /// <summary>
        /// Get the CATID corresponding to the specified type.
        /// </summary>
        /// <param name="type">Type of the object for which you want the CATID</param>
        /// <returns>CATID</returns>
        protected internal Guid GetCATIDForType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (catidMapping.ContainsKey(type))
                return catidMapping[type];
            // If you get here and you want your object to be extensible, then add a call to AddCATIDMapping() in your project constructor
            return Guid.Empty;
        }

        /// <summary>
        /// This is used to specify a CATID corresponding to a BrowseObject or an ExtObject.
        /// The CATID can be any GUID you choose. For types which are your owns, you could use
        /// their type GUID, while for other types (such as those provided in the MPF) you should
        /// provide a different GUID.
        /// </summary>
        /// <param name="type">Type of the extensible object</param>
        /// <param name="catid">GUID that extender can use to uniquely identify your object type</param>
        protected void AddCATIDMapping(Type type, Guid catid)
        {
            catidMapping.Add(type, catid);
        }

        /// <summary>
        /// Initialize an object with an XML fragment.
        /// </summary>
        /// <param name="persistXmlFragment">Object that support being initialized with an XML fragment</param>
        /// <param name="configName">Name of the configuration being initialized, null if it is the project</param>
        protected internal void LoadXmlFragment(IPersistXMLFragment persistXmlFragment, string configName)
        {
            if (persistXmlFragment == null)
            {
                throw new ArgumentNullException(nameof(persistXmlFragment));
            }

            if (xmlFragments == null)
            {
                // Retrieve the xml fragments from MSBuild
                xmlFragments = new XmlDocument();

                string fragments = GetProjectExtensions()[ProjectFileConstants.VisualStudio];
                fragments = String.Format(CultureInfo.InvariantCulture, "<root>{0}</root>", fragments);
                xmlFragments.LoadXml(fragments);
            }

            // We need to loop through all the flavors
            ErrorHandler.ThrowOnFailure(((IVsAggregatableProject)this).GetAggregateProjectTypeGuids(out string flavorsGuid));

            foreach (Guid flavor in Utilities.GuidsArrayFromSemicolonDelimitedStringOfGuids(flavorsGuid))
            {
                // Look for a matching fragment
                string flavorGuidString = flavor.ToString("B");
                string fragment = null;
                XmlNode node = null;
                foreach (XmlNode child in xmlFragments.FirstChild.ChildNodes)
                {
                    if (child.Attributes.Count > 0)
                    {
                        string guid = String.Empty;
                        string configuration = String.Empty;
                        if (child.Attributes[ProjectFileConstants.Guid] != null)
                            guid = child.Attributes[ProjectFileConstants.Guid].Value;
                        if (child.Attributes[ProjectFileConstants.Configuration] != null)
                            configuration = child.Attributes[ProjectFileConstants.Configuration].Value;

                        if (String.Compare(child.Name, ProjectFileConstants.FlavorProperties, StringComparison.OrdinalIgnoreCase) == 0
                                && String.Compare(guid, flavorGuidString, StringComparison.OrdinalIgnoreCase) == 0
                                && ((String.IsNullOrEmpty(configName) && String.IsNullOrEmpty(configuration))
                                    || (String.Compare(configuration, configName, StringComparison.OrdinalIgnoreCase) == 0)))
                        {
                            // we found the matching fragment
                            fragment = child.InnerXml;
                            node = child;
                            break;
                        }
                    }
                }

                Guid flavorGuid = flavor;
                if (String.IsNullOrEmpty(fragment))
                {
                    // the fragment was not found so initialize with default values
                    ErrorHandler.ThrowOnFailure(persistXmlFragment.InitNew(ref flavorGuid, (uint)_PersistStorageType.PST_PROJECT_FILE));
                    // While we don't yet support user files, our flavors might, so we will store that in the project file until then
                    // TODO: Refactor this code when we support user files
                    ErrorHandler.ThrowOnFailure(persistXmlFragment.InitNew(ref flavorGuid, (uint)_PersistStorageType.PST_USER_FILE));
                }
                else
                {
                    ErrorHandler.ThrowOnFailure(persistXmlFragment.Load(ref flavorGuid, (uint)_PersistStorageType.PST_PROJECT_FILE, fragment));
                    // While we don't yet support user files, our flavors might, so we will store that in the project file until then
                    // TODO: Refactor this code when we support user files
                    if (node.NextSibling != null && node.NextSibling.Attributes[ProjectFileConstants.User] != null)
                        ErrorHandler.ThrowOnFailure(persistXmlFragment.Load(ref flavorGuid, (uint)_PersistStorageType.PST_USER_FILE, node.NextSibling.InnerXml));
                }
            }
        }

        /// <summary>
        /// Retrieve all XML fragments that need to be saved from the flavors and store the information in MSBuild.
        /// </summary>
        protected void PersistXMLFragments()
        {
            if (this.IsFlavorDirty() != 0)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("ROOT");

                // We will need the list of configuration inside the loop, so get it before entering the loop
                uint[] count = new uint[1];
                IVsCfg[] configs = null;
                int hr = this.ConfigProvider.GetCfgs(0, null, count, null);
                if (ErrorHandler.Succeeded(hr) && count[0] > 0)
                {
                    configs = new IVsCfg[count[0]];
                    hr = this.ConfigProvider.GetCfgs((uint)configs.Length, configs, count, null);
                    if (ErrorHandler.Failed(hr))
                        count[0] = 0;
                }

                if (count[0] == 0)
                    configs = Array.Empty<IVsCfg>();

                // We need to loop through all the flavors
                ErrorHandler.ThrowOnFailure(((IVsAggregatableProject)this).GetAggregateProjectTypeGuids(out string flavorsGuid));

                foreach (Guid flavor in Utilities.GuidsArrayFromSemicolonDelimitedStringOfGuids(flavorsGuid))
                {
                    // First check the project
                    if(this.InteropSafeIVsHierarchy is IPersistXMLFragment outerHierarchy)
                    {
                        // Retrieve the XML fragment
                        string fragment = String.Empty;
                        Guid flavorGuid = flavor;
                        ErrorHandler.ThrowOnFailure((outerHierarchy).Save(ref flavorGuid, (uint)_PersistStorageType.PST_PROJECT_FILE, out fragment, 1));
                        if(!String.IsNullOrEmpty(fragment))
                        {
                            // Add the fragment to our XML
                            WrapXmlFragment(doc, root, flavor, null, fragment);
                        }
                        // While we don't yet support user files, our flavors might, so we will store that in the project file until then
                        // TODO: Refactor this code when we support user files
                        fragment = String.Empty;
                        ErrorHandler.ThrowOnFailure((outerHierarchy).Save(ref flavorGuid, (uint)_PersistStorageType.PST_USER_FILE, out fragment, 1));
                        if(!String.IsNullOrEmpty(fragment))
                        {
                            // Add the fragment to our XML
                            XmlElement node = WrapXmlFragment(doc, root, flavor, null, fragment);
                            node.Attributes.Append(doc.CreateAttribute(ProjectFileConstants.User));
                        }
                    }

                    // Then look at the configurations
                    foreach (IVsCfg config in configs)
                    {
                        // Get the fragment for this flavor/config pair
                        ErrorHandler.ThrowOnFailure(((ProjectConfig)config).GetXmlFragment(flavor, _PersistStorageType.PST_PROJECT_FILE, out string fragment));

                        if (!String.IsNullOrEmpty(fragment))
                        {
                            ErrorHandler.ThrowOnFailure(config.get_DisplayName(out string configName));
                            WrapXmlFragment(doc, root, flavor, configName, fragment);
                        }
                    }
                }

                if (root.ChildNodes != null && root.ChildNodes.Count > 0)
                {
                    // Save our XML (this is only the non-build information for each flavor) in MSBuild
                    SetProjectExtensions(ProjectFileConstants.VisualStudio, root.InnerXml.ToString());
                }
            }
        }

        #endregion

        #region IVsGetCfgProvider Members
        //=================================================================================

        public virtual int GetCfgProvider(out IVsCfgProvider p)
        {
            CCITracing.TraceCall();
            // Be sure to call the property here since that is doing a polymorphic ProjectConfig creation.
            p = this.ConfigProvider;
            return (p == null ? VSConstants.E_NOTIMPL : VSConstants.S_OK);
        }
        #endregion

        #region IPersist Members

        public int GetClassID(out Guid clsid)
        {
            clsid = this.ProjectGuid;
            return VSConstants.S_OK;
        }
        #endregion

        #region IPersistFileFormat Members

        int IPersistFileFormat.GetClassID(out Guid clsid)
        {
            clsid = this.ProjectGuid;
            return VSConstants.S_OK;
        }

        public virtual int GetCurFile(out string name, out uint formatIndex)
        {
            name = this.filename;
            formatIndex = 0;
            return VSConstants.S_OK;
        }

        public virtual int GetFormatList(out string formatlist)
        {
            formatlist = String.Empty;
            return VSConstants.S_OK;
        }

        public virtual int InitNew(uint formatIndex)
        {
            return VSConstants.S_OK;
        }

        public virtual int IsDirty(out int isDirty)
        {
            if(this.buildProject.Xml.HasUnsavedChanges || this.IsProjectFileDirty)
            {
                isDirty = 1;
                return VSConstants.S_OK;
            }

            isDirty = IsFlavorDirty();

            return VSConstants.S_OK;
        }

        protected int IsFlavorDirty()
        {
            int isDirty = 0;

            // See if one of our flavor consider us dirty
            if(this.InteropSafeIVsHierarchy is IPersistXMLFragment outerHierarchy)
            {
                // First check the project
                ErrorHandler.ThrowOnFailure(outerHierarchy.IsFragmentDirty((uint)_PersistStorageType.PST_PROJECT_FILE, out isDirty));
                // While we don't yet support user files, our flavors might, so we will store that in the project file until then
                // TODO: Refactor this code when we support user files
                if(isDirty == 0)
                    ErrorHandler.ThrowOnFailure(outerHierarchy.IsFragmentDirty((uint)_PersistStorageType.PST_USER_FILE, out isDirty));
            }
            if (isDirty == 0)
            {
                // Then look at the configurations
                uint[] count = new uint[1];
                int hr = this.ConfigProvider.GetCfgs(0, null, count, null);
                if (ErrorHandler.Succeeded(hr) && count[0] > 0)
                {
                    // We need to loop through the configurations
                    IVsCfg[] configs = new IVsCfg[count[0]];
                    hr = this.ConfigProvider.GetCfgs((uint)configs.Length, configs, count, null);
                    Debug.Assert(ErrorHandler.Succeeded(hr), "failed to retrieve configurations");
                    foreach (IVsCfg config in configs)
                    {
                        isDirty = ((ProjectConfig)config).IsFlavorDirty(_PersistStorageType.PST_PROJECT_FILE);
                        if (isDirty != 0)
                            break;
                    }
                }
            }
            return isDirty;
        }

        public virtual int Load(string fileName, uint mode, int readOnly)
        {
            this.filename = fileName;
            this.Reload();
            return VSConstants.S_OK;
        }

        public virtual int Save(string fileToBeSaved, int remember, uint formatIndex)
        {
            // The file name can be null. Then try to use the Url.
            string tempFileToBeSaved = fileToBeSaved;

            if (String.IsNullOrEmpty(tempFileToBeSaved) && !String.IsNullOrEmpty(this.Url))
            {
                tempFileToBeSaved = this.Url;
            }

            if (String.IsNullOrEmpty(tempFileToBeSaved))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), nameof(fileToBeSaved));
            }

            bool setProjectFileDirtyAfterSave = false;
            if (remember == 0)
            {
                setProjectFileDirtyAfterSave = this.IsProjectFileDirty;
            }

            // Update the project with the latest flavor data (if needed)
            PersistXMLFragments();

            int result = VSConstants.S_OK;
            bool saveAs = true;
            if (NativeMethods.IsSamePath(tempFileToBeSaved, this.filename))
            {
                saveAs = false;
            }
            if (!saveAs)
            {
                SuspendFileChanges fileChanges = new SuspendFileChanges(this.Site, this.filename);
                fileChanges.Suspend();
                try
                {
                    // Ensure the directory exist
                    string saveFolder = Path.GetDirectoryName(tempFileToBeSaved);
                    if (!Directory.Exists(saveFolder))
                        Directory.CreateDirectory(saveFolder);
                    // Save the project
                    this.buildProject.Save(tempFileToBeSaved);
                    this.SetProjectFileDirty(false);
                }
                finally
                {
                    fileChanges.Resume();
                }
            }
            else
            {
                result = this.SaveAs(tempFileToBeSaved);
                if (result != VSConstants.OLE_E_PROMPTSAVECANCELLED)
                {
                    ErrorHandler.ThrowOnFailure(result);
                }

            }

            if (setProjectFileDirtyAfterSave)
            {
                this.SetProjectFileDirty(true);
            }

            return result;
        }

        public virtual int SaveCompleted(string filename)
        {
            // TODO: turn file watcher back on.
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsProject3 Members

        /// <summary>
        /// Callback from the add item dialog. Deals with adding new and existing items
        /// </summary>
        public virtual int GetMkDocument(uint itemId, out string mkDoc)
        {
            mkDoc = null;
            if (itemId == VSConstants.VSITEMID_SELECTION)
            {
                return VSConstants.E_UNEXPECTED;
            }

            HierarchyNode n = this.NodeFromItemId(itemId);
            if (n == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            mkDoc = n.GetMkDocument();

            if (String.IsNullOrEmpty(mkDoc))
            {
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }


        public virtual int AddItem(uint itemIdLoc, VSADDITEMOPERATION op, string itemName, uint filesToOpen, string[] files, IntPtr dlgOwner, VSADDRESULT[] result)
        {
            Guid empty = Guid.Empty;

            return AddItemWithSpecific(itemIdLoc, op, itemName, filesToOpen, files, dlgOwner, 0, ref empty, null, ref empty, result);
        }

        /// <summary>
        /// Creates new items in a project, adds existing files to a project, or causes Add Item wizards to be run
        /// </summary>
        /// <param name="itemIdLoc"></param>
        /// <param name="op"></param>
        /// <param name="itemName"></param>
        /// <param name="filesToOpen"></param>
        /// <param name="files">Array of file names.
        /// If dwAddItemOperation is VSADDITEMOP_CLONEFILE the first item in the array is the name of the file to clone.
        /// If dwAddItemOperation is VSADDITEMOP_OPENDIRECTORY, the first item in the array is the directory to open.
        /// If dwAddItemOperation is VSADDITEMOP_RUNWIZARD, the first item is the name of the wizard to run,
        /// and the second item is the file name the user supplied (same as itemName).</param>
        /// <param name="dlgOwner"></param>
        /// <param name="editorFlags"></param>
        /// <param name="editorType"></param>
        /// <param name="physicalView"></param>
        /// <param name="logicalView"></param>
        /// <param name="result"></param>
        /// <returns>S_OK if it succeeds </returns>
        /// <remarks>The result array is initialized to failure.</remarks>
        public virtual int AddItemWithSpecific(uint itemIdLoc, VSADDITEMOPERATION op, string itemName, uint filesToOpen, string[] files, IntPtr dlgOwner, uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, VSADDRESULT[] result)
        {
            if (files == null || result == null || files.Length == 0 || result.Length == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            // Locate the node to be the container node for the file(s) being added
            // only project node or folder node and file nodes are valid container nodes
            // We need to locate the parent since the item wizard expects the parent to be passed.
            HierarchyNode n = this.NodeFromItemId(itemIdLoc);
            if (n == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            while ((!(n is ProjectNode)) && (!(n is FolderNode)) && (!this.CanFileNodesHaveChilds || !(n is FileNode)))
            {
                n = n.Parent;
            }
            Debug.Assert(n != null, "We should at this point have either a ProjectNode or FolderNode or a FileNode as a container for the new file nodes");

            // !EFW
            // Handle run wizard operations at this point
            if(op == VSADDITEMOPERATION.VSADDITEMOP_RUNWIZARD)
            {
                result[0] = this.RunWizard(n, itemName, files[0], dlgOwner);
                return VSConstants.S_OK;
            }

            string[] actualFiles = new string[files.Length];

            VSQUERYADDFILEFLAGS[] flags = this.GetQueryAddFileFlags(files);

            string baseDir = this.GetBaseDirectoryForAddingFiles(n);
            // If we did not get a directory for node that is the parent of the item then fail.
            if (String.IsNullOrEmpty(baseDir))
            {
                return VSConstants.E_FAIL;
            }

            // Pre-calculates some paths that we can use when calling CanAddItems
            List<string> filesToAdd = new List<string>();
            for (int index = 0; index < files.Length; index++)
            {
                string newFileName = String.Empty;

                string file = files[index];

                switch(op)
                {
                    case VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE:
                        {
                            // New item added. Need to copy template to new location and then add new location
                            string fileName = Path.GetFileName(itemName);
                            newFileName = Path.Combine(baseDir, fileName);
                        }
                        break;

                    case VSADDITEMOPERATION.VSADDITEMOP_OPENFILE:
                    case VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE:     // !EFW
                        {
                            string fileName = Path.GetFileName(file);
                            newFileName = Path.Combine(baseDir, fileName);
                        }
                        break;
                }
                filesToAdd.Add(newFileName);
            }

            // Ask tracker objects if we can add files
            if(!this.tracker.CanAddItems(filesToAdd.ToArray(), flags))
            {
                // We were not allowed to add the files
                return VSConstants.E_FAIL;
            }

            if(!this.ProjectMgr.QueryEditProjectFile(false))
            {
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
            }

            // Add the files to the hierarchy
            int actualFilesAddedIndex = 0;
            for(int index = 0; index < filesToAdd.Count; index++)
            {
                HierarchyNode child;
                bool overwrite = false;
                string newFileName = filesToAdd[index];

                string file = files[index];
                result[0] = VSADDRESULT.ADDRESULT_Failure;

                child = this.FindChild(newFileName);
                if (child != null)
                {
                    // If the file to be added is an existing file part of the hierarchy then continue.
                    if (NativeMethods.IsSamePath(file, newFileName))
                    {
                        // !EFW
                        if(child.IsNonMemberItem)
                            result[0] = child.IncludeInProject() == VSConstants.S_OK ?
                                VSADDRESULT.ADDRESULT_Success : VSADDRESULT.ADDRESULT_Cancel;
                        else
                            result[0] = VSADDRESULT.ADDRESULT_Cancel;

                        continue;
                    }

                    int canOverWriteExistingItem = this.CanOverwriteExistingItem(file, newFileName);

                    if (canOverWriteExistingItem == (int)OleConstants.OLECMDERR_E_CANCELED)
                    {
                        result[0] = VSADDRESULT.ADDRESULT_Cancel;
                        return canOverWriteExistingItem;
                    }
                    else if (canOverWriteExistingItem == VSConstants.S_OK)
                    {
                        overwrite = true;
                    }
                    else
                    {
                        return canOverWriteExistingItem;
                    }
                }

                // !EFW
                // If the file to be added is not in the same path copy it unless we are creating a link to it
                if(op != VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE &&
                  NativeMethods.IsSamePath(file, newFileName) == false)
                {
                    if (!overwrite && File.Exists(newFileName))
                    {
                        string message = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.FileAlreadyExists, CultureInfo.CurrentUICulture), newFileName);
                        string title = String.Empty;
                        OLEMSGICON icon = OLEMSGICON.OLEMSGICON_QUERY;
                        OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_YESNO;
                        OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                        int messageboxResult = VsShellUtilities.ShowMessageBox(this.Site, title, message, icon, buttons, defaultButton);
                        if (messageboxResult == NativeMethods.IDNO)
                        {
                            result[0] = VSADDRESULT.ADDRESULT_Cancel;
                            return (int)OleConstants.OLECMDERR_E_CANCELED;
                        }
                    }

                    // Copy the file to the correct location.
                    // We will suppress the file change events to be triggered to this item, since we are going to copy over the existing file and thus we will trigger a file change event.
                    // We do not want the file change event to occur in this case, similar that we do not want a file change event to occur when saving a file.
                    if(!(this.site.GetService(typeof(SVsFileChangeEx)) is IVsFileChangeEx fileChange))
                    {
                        throw new InvalidOperationException();
                    }

                    try
                    {
                        ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(VSConstants.VSCOOKIE_NIL, newFileName, 1));
                        if (op == VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE)
                        {
                            this.AddFileFromTemplate(file, newFileName);
                        }
                        else
                        {
                            PackageUtilities.CopyUrlToLocal(new Uri(file), newFileName);
                        }
                    }
                    finally
                    {
                        ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(VSConstants.VSCOOKIE_NIL, newFileName, 0));
                    }
                }

                if (overwrite)
                {
                    this.OverwriteExistingItem(child);
                }
                else
                {
                    // !EFW
                    // Add new file node/dependent file node
                    if(op != VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE)
                        this.AddNewFileNodeToHierarchy(n, newFileName);
                    else
                        this.AddNewLinkedFileNodeToHierarchy(n, file, newFileName);
                }

                result[0] = VSADDRESULT.ADDRESULT_Success;
                actualFiles[actualFilesAddedIndex++] = newFileName;
            }

            // Notify listeners that items were appended.
            if(actualFilesAddedIndex > 0)
            {
                // !EFW - Really odd bug.  In release builds, if the new item is added to a collapsed node and
                // is nested deeply enough, the OnItemsAppended call will fail somewhere and kill Visual Studio.
                // Expanding the parent nodes first appears to work around the issue.
                IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.ProjectMgr.Site,
                    SolutionExplorer);

                if(uiWindow != null)
                    ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this.ProjectMgr.InteropSafeIVsUIHierarchy,
                        n.ID, EXPANDFLAGS.EXPF_ExpandParentsToShowItem));

                n.OnItemsAppended(n);
            }

            //Open files if this was requested through the editorFlags
            bool openFiles = (editorFlags & (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen) != 0;
            if (openFiles && actualFiles.Length <= filesToOpen)
            {
                for (int i = 0; i < filesToOpen; i++)
                {
                    if (!String.IsNullOrEmpty(actualFiles[i]))
                    {
                        string name = actualFiles[i];
                        HierarchyNode child = this.FindChild(name);
                        Debug.Assert(child != null, "We should have been able to find the new element in the hierarchy");
                        if (child != null)
                        {
                            IVsWindowFrame frame;
                            if (editorType == Guid.Empty)
                            {
                                Guid view = Guid.Empty;
                                ErrorHandler.ThrowOnFailure(this.OpenItem(child.ID, ref view, IntPtr.Zero, out frame));
                            }
                            else
                            {
                                ErrorHandler.ThrowOnFailure(this.OpenItemWithSpecific(child.ID, editorFlags, ref editorType, physicalView, ref logicalView, IntPtr.Zero, out frame));
                            }

                            // Show the window frame in the UI and make it the active window
                            if (frame != null)
                            {
                                ErrorHandler.ThrowOnFailure(frame.Show());
                            }
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// for now used by add folder. Called on the ROOT, as only the project should need
        /// to implement this.
        /// for folders, called with parent folder, blank extension and blank suggested root
        /// </summary>
        public virtual int GenerateUniqueItemName(uint itemIdLoc, string ext, string suggestedRoot, out string itemName)
        {
            string rootName = String.Empty;
            string extToUse = String.Empty;

            //force new items to have a number
            int cb = 1;
            bool found = false;
            bool fFolderCase = false;
            HierarchyNode parent = this.NodeFromItemId(itemIdLoc);

            if (!String.IsNullOrEmpty(ext))
            {
                extToUse = ext.Trim();
            }

            if (!String.IsNullOrEmpty(suggestedRoot))
            {
                suggestedRoot = suggestedRoot.Trim();
            }

            if (suggestedRoot == null || suggestedRoot.Length == 0)
            {
                // folder case, we assume... 
                suggestedRoot = "NewFolder";
                fFolderCase = true;
            }

            while (!found)
            {
                rootName = suggestedRoot;
                if (cb > 0)
                    rootName += cb.ToString(CultureInfo.CurrentCulture);

                if (extToUse.Length > 0)
                {
                    rootName += extToUse;
                }

                cb++;
                found = true;
                for (HierarchyNode n = parent.FirstChild; n != null; n = n.NextSibling)
                {
                    if (rootName == n.GetEditLabel())
                    {
                        found = false;
                        break;
                    }

                    //if parent is a folder, we need the whole url
                    string parentFolder = parent.Url;
                    if (parent is ProjectNode)
                        parentFolder = Path.GetDirectoryName(parent.Url);

                    string checkFile = Path.Combine(parentFolder, rootName);

                    if (fFolderCase)
                    {
                        if (Directory.Exists(checkFile))
                        {
                            found = false;
                            break;
                        }
                    }
                    else
                    {
                        if (File.Exists(checkFile))
                        {
                            found = false;
                            break;
                        }
                    }
                }
            }

            itemName = rootName;
            return VSConstants.S_OK;
        }


        public virtual int GetItemContext(uint itemId, out Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            CCITracing.TraceCall();
            psp = null;
            HierarchyNode child = this.NodeFromItemId(itemId);
            if (child != null)
            {
                psp = child.OleServiceProvider as IOleServiceProvider;
            }
            return VSConstants.S_OK;
        }


        public virtual int IsDocumentInProject(string mkDoc, out int found, VSDOCUMENTPRIORITY[] pri, out uint itemId)
        {
            CCITracing.TraceCall();
            if (pri != null && pri.Length >= 1)
            {
                pri[0] = VSDOCUMENTPRIORITY.DP_Unsupported;
            }
            found = 0;
            itemId = 0;

            // If it is the project file just return.
            if (NativeMethods.IsSamePath(mkDoc, this.GetMkDocument()))
            {
                found = 1;
                itemId = VSConstants.VSITEMID_ROOT;
            }
            else
            {
                HierarchyNode child = this.FindChild(mkDoc);
                if (child != null)
                {
                    found = 1;
                    itemId = child.ID;
                }
            }

            if (found == 1)
            {
                if (pri != null && pri.Length >= 1)
                {
                    pri[0] = VSDOCUMENTPRIORITY.DP_Standard;
                }
            }

            return VSConstants.S_OK;

        }


        public virtual int OpenItem(uint itemId, ref Guid logicalView, IntPtr punkDocDataExisting, out IVsWindowFrame frame)
        {
            // Initialize output params
            frame = null;

            HierarchyNode n = this.NodeFromItemId(itemId);
            if (n == null)
            {
                throw new ArgumentException(SR.GetString(SR.ParameterMustBeAValidItemId, CultureInfo.CurrentUICulture), nameof(itemId));
            }

            // Delegate to the document manager object that knows how to open the item
            DocumentManager documentManager = n.GetDocumentManager();
            if (documentManager != null)
            {
                return documentManager.Open(ref logicalView, punkDocDataExisting, out frame, WindowFrameShowAction.DoNotShow);
            }

            // This node does not have an associated document manager and we must fail
            return VSConstants.E_FAIL;
        }


        public virtual int OpenItemWithSpecific(uint itemId, uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame)
        {
            // Initialize output params
            frame = null;

            HierarchyNode n = this.NodeFromItemId(itemId);
            if (n == null)
            {
                throw new ArgumentException(SR.GetString(SR.ParameterMustBeAValidItemId, CultureInfo.CurrentUICulture), nameof(itemId));
            }

            // Delegate to the document manager object that knows how to open the item
            DocumentManager documentManager = n.GetDocumentManager();
            if (documentManager != null)
            {
                return documentManager.OpenWithSpecific(editorFlags, ref editorType, physicalView, ref logicalView, docDataExisting, out frame, WindowFrameShowAction.DoNotShow);
            }

            // This node does not have an associated document manager and we must fail
            return VSConstants.E_FAIL;
        }


        public virtual int RemoveItem(uint reserved, uint itemId, out int result)
        {
            HierarchyNode n = this.NodeFromItemId(itemId);
            if (n == null)
            {
                throw new ArgumentException(SR.GetString(SR.ParameterMustBeAValidItemId, CultureInfo.CurrentUICulture), nameof(itemId));
            }
            n.Remove(true);
            result = 1;
            return VSConstants.S_OK;
        }


        public virtual int ReopenItem(uint itemId, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame)
        {
            // Initialize output params
            frame = null;

            HierarchyNode n = this.NodeFromItemId(itemId);
            if (n == null)
            {
                throw new ArgumentException(SR.GetString(SR.ParameterMustBeAValidItemId, CultureInfo.CurrentUICulture), nameof(itemId));
            }

            // Delegate to the document manager object that knows how to open the item
            DocumentManager documentManager = n.GetDocumentManager();
            if (documentManager != null)
            {
                return documentManager.OpenWithSpecific(0, ref editorType, physicalView, ref logicalView, docDataExisting, out frame, WindowFrameShowAction.DoNotShow);
            }

            // This node does not have an associated document manager and we must fail
            return VSConstants.E_FAIL;
        }


        /// <summary>
        /// Implements IVsProject3::TransferItem
        /// This function is called when an open miscellaneous file is being transferred
        /// to our project. The sequence is for the shell to call AddItemWithSpecific and
        /// then use TransferItem to transfer the open document to our project.
        /// </summary>
        /// <param name="oldMkDoc">Old document name</param>
        /// <param name="newMkDoc">New document name</param>
        /// <param name="frame">Optional frame if the document is open</param>
        /// <returns></returns>
        public virtual int TransferItem(string oldMkDoc, string newMkDoc, IVsWindowFrame frame)
        {
            // Fail if hierarchy already closed
            if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
            {
                return VSConstants.E_FAIL;
            }
            //Fail if the document names passed are null.
            if (oldMkDoc == null || newMkDoc == null)
                return VSConstants.E_INVALIDARG;

            int hr = VSConstants.S_OK;
            VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];
            uint cookie = 0;

            if(!(GetService(typeof(IVsRunningDocumentTable)) is IVsRunningDocumentTable pRdt))
                return VSConstants.E_ABORT;

            IVsHierarchy pHier;
            uint id;
            IntPtr docdataForCookiePtr = IntPtr.Zero;
            IntPtr docDataPtr = IntPtr.Zero;
            IntPtr hierPtr = IntPtr.Zero;

            // We get the document from the running doc table so that we can see if it is transient
            try
            {
                ErrorHandler.ThrowOnFailure(pRdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, oldMkDoc,
                    out pHier, out id, out docdataForCookiePtr, out cookie));
            }
            finally
            {
                if (docdataForCookiePtr != IntPtr.Zero)
                    Marshal.Release(docdataForCookiePtr);
            }

            //Get the document info
            try
            {
                ErrorHandler.ThrowOnFailure(pRdt.GetDocumentInfo(cookie, out uint grfFlags, out uint readLocks,
                    out uint editLocks, out string doc, out pHier, out id, out docDataPtr));
            }
            finally
            {
                if (docDataPtr != IntPtr.Zero)
                    Marshal.Release(docDataPtr);
            }

            // Now see if the document is in the project. If not, we fail
            try
            {
                ErrorHandler.ThrowOnFailure(IsDocumentInProject(newMkDoc, out int found, priority, out uint itemid));
                Debug.Assert(itemid != VSConstants.VSITEMID_NIL && itemid != VSConstants.VSITEMID_ROOT);
                hierPtr = Marshal.GetComInterfaceForObject(this, typeof(IVsUIHierarchy));
                // Now rename the document
                ErrorHandler.ThrowOnFailure(pRdt.RenameDocument(oldMkDoc, newMkDoc, hierPtr, itemid));
            }
            finally
            {
                if (hierPtr != IntPtr.Zero)
                    Marshal.Release(hierPtr);
            }

            //Change the caption if we are passed a window frame
            if (frame != null)
            {
                string caption = "%2";
                hr = frame.SetProperty((int)(__VSFPROPID.VSFPROPID_OwnerCaption), caption);
            }
            return hr;
        }

        #endregion

        #region IVsProjectBuidSystem Members
        public virtual int SetHostObject(string targetName, string taskName, object hostObject)
        {
            Debug.Assert(targetName != null && taskName != null && this.buildProject != null && this.buildProject.Targets != null);

            if (targetName == null || taskName == null || this.buildProject == null || this.buildProject.Targets == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            this.buildProject.ProjectCollection.HostServices.RegisterHostObject(this.buildProject.FullPath, targetName, taskName, (Microsoft.Build.Framework.ITaskHost)hostObject);

            return VSConstants.S_OK;
        }

        public int BuildTarget(string targetName, out bool success)
        {
            success = false;

            MSBuildResult result = this.Build(targetName);

            if (result == MSBuildResult.Successful)
            {
                success = true;
            }

            return VSConstants.S_OK;
        }

        public virtual int CancelBatchEdit()
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int EndBatchEdit()
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int StartBatchEdit()
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Used to determine the kind of build system, in VS 2005 there's only one defined kind: MSBuild
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public virtual int GetBuildSystemKind(out uint kind)
        {
            kind = (uint)_BuildSystemKindFlags2.BSK_MSBUILD_VS10;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsComponentUser methods

        /// <summary>
        /// Add Components to the Project.
        /// Used by the environment to add components specified by the user in the Component Selector dialog
        /// to the specified project
        /// </summary>
        /// <param name="dwAddCompOperation">The component operation to be performed.</param>
        /// <param name="cComponents">Number of components to be added</param>
        /// <param name="rgpcsdComponents">array of component selector data</param>
        /// <param name="hwndDialog">Handle to the component picker dialog</param>
        /// <param name="pResult">Result to be returned to the caller</param>
        public virtual int AddComponent(VSADDCOMPOPERATION dwAddCompOperation, uint cComponents, System.IntPtr[] rgpcsdComponents, System.IntPtr hwndDialog, VSADDCOMPRESULT[] pResult)
        {
            if (rgpcsdComponents == null || pResult == null)
            {
                return VSConstants.E_FAIL;
            }

            // Initialize the out parameter
            pResult[0] = VSADDCOMPRESULT.ADDCOMPRESULT_Success;

            IReferenceContainer references = GetReferenceContainer();
            if (null == references)
            {
                // This project does not support references or the reference container was not created.
                // In both cases this operation is not supported.
                return VSConstants.E_NOTIMPL;
            }
            for (int cCount = 0; cCount < cComponents; cCount++)
            {
                IntPtr ptr = rgpcsdComponents[cCount];
                VSCOMPONENTSELECTORDATA selectorData = (VSCOMPONENTSELECTORDATA)Marshal.PtrToStructure(ptr, typeof(VSCOMPONENTSELECTORDATA));
                if (null == references.AddReferenceFromSelectorData(selectorData))
                {
                    //Skip further processing since a reference has to be added
                    pResult[0] = VSADDCOMPRESULT.ADDCOMPRESULT_Failure;
                    return VSConstants.S_OK;
                }
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsDependencyProvider Members
        public int EnumDependencies(out IVsEnumDependencies enumDependencies)
        {
            enumDependencies = new EnumDependencies(this.buildDependencyList);
            return VSConstants.S_OK;
        }

        public int OpenDependency(string szDependencyCanonicalName, out IVsDependency dependency)
        {
            dependency = null;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsSccProject2 Members
        /// <summary>
        /// This method is called to determine which files should be placed under source control for a given VSITEMID within this hierarchy.
        /// </summary>
        /// <param name="itemid">Identifier for the VSITEMID being queried.</param>
        /// <param name="stringsOut">Pointer to an array of CALPOLESTR strings containing the file names for this item.</param>
        /// <param name="flagsOut">Pointer to a CADWORD array of flags stored in DWORDs indicating that some of the files have special behaviors.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
        public virtual int GetSccFiles(uint itemid, CALPOLESTR[] stringsOut, CADWORD[] flagsOut)
        {
            if (itemid == VSConstants.VSITEMID_SELECTION)
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), nameof(itemid));
            }

            HierarchyNode n = this.NodeFromItemId(itemid);
            if (n == null)
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), nameof(itemid));
            }

            List<string> files = new List<string>();
            List<tagVsSccFilesFlags> flags = new List<tagVsSccFilesFlags>();

            n.GetSccFiles(files, flags);

            if (stringsOut != null && stringsOut.Length > 0)
            {
                stringsOut[0] = Utilities.CreateCALPOLESTR(files);
            }

            if (flagsOut != null && flagsOut.Length > 0)
            {
                flagsOut[0] = Utilities.CreateCADWORD(flags);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This method is called to discover special (hidden files) associated with a given VSITEMID within this hierarchy. 
        /// </summary>
        /// <param name="itemid">Identifier for the VSITEMID being queried.</param>
        /// <param name="sccFile">One of the files associated with the node</param>
        /// <param name="stringsOut">Pointer to an array of CALPOLESTR strings containing the file names for this item.</param>
        /// <param name="flagsOut">Pointer to a CADWORD array of flags stored in DWORDs indicating that some of the files have special behaviors.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
        /// <remarks>This method is called to discover any special or hidden files associated with an item in the project hierarchy. It is called when GetSccFiles returns with the SFF_HasSpecialFiles flag set for any of the files associated with the node.</remarks>
        public virtual int GetSccSpecialFiles(uint itemid, string sccFile, CALPOLESTR[] stringsOut, CADWORD[] flagsOut)
        {
            if (itemid == VSConstants.VSITEMID_SELECTION)
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), nameof(itemid));
            }

            HierarchyNode n = this.NodeFromItemId(itemid);
            if (n == null)
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), nameof(itemid));
            }

            List<string> files = new List<string>();

            List<tagVsSccFilesFlags> flags = new List<tagVsSccFilesFlags>();

            n.GetSccSpecialFiles(sccFile, files, flags);

            if (stringsOut != null && stringsOut.Length > 0)
            {
                stringsOut[0] = Utilities.CreateCALPOLESTR(files);
            }

            if (flagsOut != null && flagsOut.Length > 0)
            {
                flagsOut[0] = Utilities.CreateCADWORD(flags);
            }

            return VSConstants.S_OK;

        }

        /// <summary>
        /// This method is called by the source control portion of the environment to inform the project of changes to the source control glyph on various nodes. 
        /// </summary>
        /// <param name="affectedNodes">Count of changed nodes.</param>
        /// <param name="itemidAffectedNodes">An array of VSITEMID identifiers of the changed nodes.</param>
        /// <param name="newGlyphs">An array of VsStateIcon glyphs representing the new state of the corresponding item in rgitemidAffectedNodes.</param>
        /// <param name="newSccStatus">An array of status flags from SccStatus corresponding to rgitemidAffectedNodes. </param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
        public virtual int SccGlyphChanged(int affectedNodes, uint[] itemidAffectedNodes, VsStateIcon[] newGlyphs, uint[] newSccStatus)
        {
            // if all the parameters are null and the count is 0, it means SCC wants us to updated everything
            if (affectedNodes == 0 && itemidAffectedNodes == null && newGlyphs == null && newSccStatus == null)
            {
                this.ReDraw(UIHierarchyElement.SccState);
                this.UpdateSccStateIcons();
            }
            else if (affectedNodes > 0 && itemidAffectedNodes != null && newGlyphs != null && newSccStatus != null)
            {
                for (int i = 0; i < affectedNodes; i++)
                {
                    HierarchyNode n = this.NodeFromItemId(itemidAffectedNodes[i]);
                    if (n == null)
                    {
                        throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), nameof(itemidAffectedNodes));
                    }

                    n.ReDraw(UIHierarchyElement.SccState);
                }
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This method is called by the source control portion of the environment when a project is initially added to source control, or to change some of the project's settings.
        /// </summary>
        /// <param name="sccProjectName">String, opaque to the project, that identifies the project location on the server. Persist this string in the project file. </param>
        /// <param name="sccLocalPath">String, opaque to the project, that identifies the path to the server. Persist this string in the project file.</param>
        /// <param name="sccAuxPath">String, opaque to the project, that identifies the local path to the project. Persist this string in the project file.</param>
        /// <param name="sccProvider">String, opaque to the project, that identifies the source control package. Persist this string in the project file.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int SetSccLocation(string sccProjectName, string sccAuxPath, string sccLocalPath, string sccProvider)
        {
            if (this.IsSccDisabled)
            {
                throw new NotImplementedException();
            }

            if (sccProjectName == null)
            {
                throw new ArgumentNullException(nameof(sccProjectName));
            }

            if (sccAuxPath == null)
            {
                throw new ArgumentNullException(nameof(sccAuxPath));
            }

            if (sccLocalPath == null)
            {
                throw new ArgumentNullException(nameof(sccLocalPath));
            }

            if (sccProvider == null)
            {
                throw new ArgumentNullException(nameof(sccProvider));
            }

            // Save our settings (returns true if something changed)
            if (!this.SetSccSettings(sccProjectName, sccLocalPath, sccAuxPath, sccProvider))
            {
                return VSConstants.S_OK;
            }

            bool unbinding = (sccProjectName.Length == 0 && sccProvider.Length == 0);

            if (unbinding || this.QueryEditProjectFile(false))
            {
                this.buildProject.SetProperty(ProjectFileConstants.SccProjectName, sccProjectName);
                this.buildProject.SetProperty(ProjectFileConstants.SccProvider, sccProvider);
                this.buildProject.SetProperty(ProjectFileConstants.SccAuxPath, sccAuxPath);
                this.buildProject.SetProperty(ProjectFileConstants.SccLocalPath, sccLocalPath);
            }

            this.isRegisteredWithScc = true;

            return VSConstants.S_OK;
        }
        #endregion

        #region IVsProjectSpecialFiles Members
        /// <summary>
        /// Allows you to query the project for special files and optionally create them.
        /// </summary>
        /// <param name="fileId">__PSFFILEID of the file</param>
        /// <param name="flags">__PSFFLAGS flags for the file</param>
        /// <param name="itemid">The item ID of the node in the hierarchy</param>
        /// <param name="fileName">The file name of the special file.</param>
        /// <returns></returns>
        public virtual int GetFile(int fileId, uint flags, out uint itemid, out string fileName)
        {
            itemid = VSConstants.VSITEMID_NIL;
            fileName = String.Empty;

            // We need to return S_OK, otherwise the property page tabs will not be shown.
            return VSConstants.E_NOTIMPL;
        }
        #endregion

        #region IAggregatedHierarchy Members

        /// <summary>
        /// Get the inner object of an aggregated hierarchy
        /// </summary>
        /// <returns>A HierarchyNode</returns>
        public virtual HierarchyNode GetInner()
        {
            return this;
        }

        #endregion

        #region IBuildDependencyUpdate Members

        public virtual IVsBuildDependency[] BuildDependencies => this.buildDependencyList.ToArray();

        public virtual void AddBuildDependency(IVsBuildDependency dependency)
        {
            if (this.isClosed || dependency == null)
            {
                return;
            }

            if (!this.buildDependencyList.Contains(dependency))
            {
                this.buildDependencyList.Add(dependency);
            }
        }

        public virtual void RemoveBuildDependency(IVsBuildDependency dependency)
        {
            if (this.isClosed || dependency == null)
            {
                return;
            }

            if (this.buildDependencyList.Contains(dependency))
            {
                this.buildDependencyList.Remove(dependency);
            }
        }

        #endregion

        #region IReferenceDataProvider Members
        /// <summary>
        /// Returns the reference container node.
        /// </summary>
        /// <returns></returns>
        public IReferenceContainer GetReferenceContainer()
        {
            return this.FindChild(ReferenceContainerNode.ReferencesNodeVirtualName) as IReferenceContainer;
        }

        #endregion

        #region IProjectEventsListener Members
        public bool IsProjectEventsListener
        {
            get => this.isProjectEventsListener;
            set => this.isProjectEventsListener = value;
        }
        #endregion

        #region IProjectEventsProvider Members

        /// <summary>
        /// Defines the provider for the project events
        /// </summary>
        IProjectEvents IProjectEventsProvider.ProjectEventsProvider
        {
            get => this.projectEventsProvider;
            set
            {
                if(null != this.projectEventsProvider)
                {
                    this.projectEventsProvider.AfterProjectFileOpened -= this.OnAfterProjectOpen;
                }

                this.projectEventsProvider = value;

                if(null != this.projectEventsProvider)
                {
                    this.projectEventsProvider.AfterProjectFileOpened += this.OnAfterProjectOpen;
                }
            }
        }

        #endregion

        #region IVsAggregatableProject Members

        /// <summary>
        /// Retrieve the list of project GUIDs that are aggregated together to make this project.
        /// </summary>
        /// <param name="projectTypeGuids">Semi colon separated list of GUIDs. Typically, the last GUID would be the GUID of the base project factory</param>
        /// <returns>HResult</returns>
        public int GetAggregateProjectTypeGuids(out string projectTypeGuids)
        {
            projectTypeGuids = this.GetProjectProperty(ProjectFileConstants.ProjectTypeGuids);
            // In case someone manually removed this from our project file, default to our project without flavors
            if (String.IsNullOrEmpty(projectTypeGuids))
                projectTypeGuids = this.ProjectGuid.ToString("B");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This is where the initialization occurs.
        /// </summary>
        public virtual int InitializeForOuter(string filename, string location, string name, uint flags, ref Guid iid, out IntPtr projectPointer, out int canceled)
        {
            projectPointer = IntPtr.Zero;

            // Initialize the interop-safe versions of this node's implementations of various VS interfaces,
            // which point to the outer object. The project node itself should never be passed to unmanaged 
            // code -- we should always use these properties instead. 
            this.InteropSafeIVsHierarchy = Utilities.GetOuterAs<IVsHierarchy>(this);
            this.InteropSafeIVsUIHierarchy = Utilities.GetOuterAs<IVsUIHierarchy>(this);
            this.InteropSafeIVsProject3 = Utilities.GetOuterAs<IVsProject3>(this);
            this.InteropSafeIVsSccProject2 = Utilities.GetOuterAs<IVsSccProject2>(this);
            this.InteropSafeIVsUIHierWinClipboardHelperEvents = Utilities.GetOuterAs<IVsUIHierWinClipboardHelperEvents>(this);
            this.InteropSafeIVsComponentUser = Utilities.GetOuterAs<IVsComponentUser>(this);

            // Initialize the project
            this.Load(filename, location, name, flags, ref iid, out canceled);

            if (canceled != 1)
            {
                // Set ourself as the project
                return Marshal.QueryInterface(Marshal.GetIUnknownForObject(this), ref iid, out projectPointer);
            }

            return VSConstants.OLE_E_PROMPTSAVECANCELLED;
        }

        /// <summary>
        /// This is called after the project is done initializing the different layer of the aggregations
        /// </summary>
        /// <returns>HResult</returns>
        public virtual int OnAggregationComplete()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Set the list of GUIDs that are aggregated together to create this project.
        /// </summary>
        /// <param name="projectTypeGuids">Semi-colon separated list of GUIDs, the last one is usually the project factory of the base project factory</param>
        /// <returns>HResult</returns>
        public int SetAggregateProjectTypeGuids(string projectTypeGuids)
        {
            this.SetProjectProperty(ProjectFileConstants.ProjectTypeGuids, projectTypeGuids);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// We are always the inner most part of the aggregation
        /// and as such we don't support setting an inner project
        /// </summary>
        public int SetInnerProject(object innerProject)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsProjectFlavorCfgProvider Members

        int IVsProjectFlavorCfgProvider.CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
        {
            // Our config object is also our IVsProjectFlavorCfg object
            ppFlavorCfg = pBaseProjectCfg as IVsProjectFlavorCfg;

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsBuildPropertyStorage Members

        /// <summary>
        /// Get the property of an item
        /// </summary>
        /// <param name="item">ItemID</param>
        /// <param name="attributeName">Name of the property</param>
        /// <param name="attributeValue">Value of the property (out parameter)</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.GetItemAttribute(uint item, string attributeName, out string attributeValue)
        {
            attributeValue = null;

            HierarchyNode node = NodeFromItemId(item);
            if (node == null)
                throw new ArgumentException("Invalid item id", nameof(item));

            attributeValue = node.ItemNode.GetMetadata(attributeName);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Get the value of the property in the project file
        /// </summary>
        /// <param name="propertyName">Name of the property to remove</param>
        /// <param name="configName">Configuration for which to remove the property</param>
        /// <param name="storage">Project or user file (_PersistStorageType)</param>
        /// <param name="propertyValue">Value of the property (out parameter)</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.GetPropertyValue(string propertyName, string configName, uint storage, out string propertyValue)
        {
            // TODO: when adding support for User files, we need to update this method
            if(String.IsNullOrEmpty(configName))
            {
                propertyValue = this.GetProjectProperty(propertyName);
            }
            else
            {
                ErrorHandler.ThrowOnFailure(this.ConfigProvider.GetCfgOfName(configName, String.Empty, out IVsCfg configurationInterface));
                ProjectConfig config = (ProjectConfig)configurationInterface;
                propertyValue = config.GetConfigurationProperty(propertyName, true);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Delete a property
        /// In our case this simply mean defining it as null
        /// </summary>
        /// <param name="propertyName">Name of the property to remove</param>
        /// <param name="configName">Configuration for which to remove the property</param>
        /// <param name="storage">Project or user file (_PersistStorageType)</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.RemoveProperty(string propertyName, string configName, uint storage)
        {
            return ((IVsBuildPropertyStorage)this).SetPropertyValue(propertyName, configName, storage, null);
        }

        /// <summary>
        /// Set a property on an item
        /// </summary>
        /// <param name="item">ItemID</param>
        /// <param name="attributeName">Name of the property</param>
        /// <param name="attributeValue">New value for the property</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.SetItemAttribute(uint item, string attributeName, string attributeValue)
        {
            HierarchyNode node = NodeFromItemId(item);

            if (node == null)
                throw new ArgumentException("Invalid item id", nameof(item));

            node.ItemNode.SetMetadata(attributeName, attributeValue);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Set a project property
        /// </summary>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="configName">Configuration for which to set the property</param>
        /// <param name="storage">Project file or user file (_PersistStorageType)</param>
        /// <param name="propertyValue">New value for that property</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.SetPropertyValue(string propertyName, string configName, uint storage, string propertyValue)
        {
            // TODO: when adding support for User files, we need to update this method
            if (String.IsNullOrEmpty(configName))
            {
                this.SetProjectProperty(propertyName, propertyValue);
            }
            else
            {
                ErrorHandler.ThrowOnFailure(this.ConfigProvider.GetCfgOfName(configName, String.Empty, out IVsCfg configurationInterface));
                ProjectConfig config = (ProjectConfig)configurationInterface;
                config.SetConfigurationProperty(propertyName, propertyValue);
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsDesignTimeAssemblyResolution methods

        public int GetTargetFramework(out string ppTargetFramework)
        {
            ppTargetFramework = this.ProjectMgr.TargetFrameworkMoniker.FullName;
            return VSConstants.S_OK;
        }

        public int ResolveAssemblyPathInTargetFx(string[] prgAssemblySpecs, uint cAssembliesToResolve, VsResolvedAssemblyPath[] prgResolvedAssemblyPaths, out uint pcResolvedAssemblyPaths)
        {
            if (prgAssemblySpecs == null || cAssembliesToResolve == 0 || prgResolvedAssemblyPaths == null)
            {
                throw new ArgumentException("One or more of the arguments are invalid.");
            }

            pcResolvedAssemblyPaths = 0;

            try
            {
                var results = designTimeAssemblyResolution.Resolve(prgAssemblySpecs.Take((int)cAssembliesToResolve));
                results.CopyTo(prgResolvedAssemblyPaths, 0);
                pcResolvedAssemblyPaths = (uint)results.Length;
            }
            catch (Exception ex)
            {
                return Marshal.GetHRForException(ex);
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region private helper methods

        /// <summary>
        /// Initialize projectNode
        /// </summary>
        private void Initialize()
        {
            this.ID = VSConstants.VSITEMID_ROOT;
            this.tracker = new TrackDocumentsHelper(this);
        }

        /// <summary>
        /// Add an item to the hierarchy based on the item path
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>Added node</returns>
        private HierarchyNode AddIndependentFileNode(MSBuild.ProjectItem item)
        {
            HierarchyNode currentParent = GetItemParentNode(item);
            return AddFileNodeToNode(item, currentParent);
        }

        /// <summary>
        /// Add a dependent file node to the hierarchy
        /// </summary>
        /// <param name="item">MSBuild item to add</param>
        /// <param name="parentNode">Parent Node</param>
        /// <returns>Added node</returns>
        private HierarchyNode AddDependentFileNodeToNode(MSBuild.ProjectItem item, HierarchyNode parentNode)
        {
            FileNode node = this.CreateDependentFileNode(new ProjectElement(this, item, false));
            parentNode.AddChild(node);

            // Make sure to set the HasNameRelation flag on the dependent node if it is related to the parent by name
            if (!node.HasParentNodeNameRelation && String.Compare(node.GetRelationalName(), parentNode.GetRelationalName(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                node.HasParentNodeNameRelation = true;
            }

            return node;
        }

        /// <summary>
        /// Add a file node to the hierarchy
        /// </summary>
        /// <param name="item">MSBuild item to add</param>
        /// <param name="parentNode">Parent Node</param>
        /// <returns>Added node</returns>
        private HierarchyNode AddFileNodeToNode(MSBuild.ProjectItem item, HierarchyNode parentNode)
        {
            FileNode node = this.CreateFileNode(new ProjectElement(this, item, false));
            parentNode.AddChild(node);
            return node;
        }

        /// <summary>
        /// Get the parent node of an MSBuild item
        /// </summary>
        /// <param name="item">MSBuild item</param>
        /// <returns>parent node</returns>
        private HierarchyNode GetItemParentNode(MSBuild.ProjectItem item)
        {
            HierarchyNode currentParent = this;
            string strPath;

            // !EFW
            // If it's a link, use the linked path for the node
            strPath = item.GetMetadataValue(ProjectFileConstants.Link);

            if(String.IsNullOrEmpty(strPath))
                strPath = item.EvaluatedInclude;

            strPath = Path.GetDirectoryName(strPath);
            if(strPath.Length > 0)
            {
                // Use the relative to verify the folders...
                currentParent = this.CreateFolderNodes(strPath);
            }
            return currentParent;
        }

        private MSBuildExecution.ProjectPropertyInstance GetMsBuildProperty(string propertyName, bool resetCache)
        {
            if (resetCache || this.currentConfig == null)
            {
                // Get properties from project file and cache it
                this.SetCurrentConfiguration();

                if(this.buildProject == null)
                    return null;

                this.currentConfig = this.buildProject.CreateProjectInstance();
            }

            if (this.currentConfig == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.FailedToRetrieveProperties, CultureInfo.CurrentUICulture), propertyName));

            // return property asked for
            return this.currentConfig.GetProperty(propertyName);
        }

        private string GetOutputPath(MSBuildExecution.ProjectInstance properties)
        {
            this.currentConfig = properties;
            string outputPath = GetProjectProperty("OutputPath");

            if (!String.IsNullOrEmpty(outputPath))
            {
                outputPath = outputPath.Replace('/', Path.DirectorySeparatorChar);
                if (outputPath[outputPath.Length - 1] != Path.DirectorySeparatorChar)
                    outputPath += Path.DirectorySeparatorChar;
            }

            return outputPath;
        }

        private bool GetBoolAttr(MSBuildExecution.ProjectInstance properties, string name)
        {
            this.currentConfig = properties;
            string s = GetProjectProperty(name);

            return (s != null && s.ToUpperInvariant().Trim() == "TRUE");
        }

        private string GetAssemblyName(MSBuildExecution.ProjectInstance properties)
        {
            this.currentConfig = properties;

            string name = GetProjectProperty(ProjectFileConstants.AssemblyName);

            if (name == null)
                name = this.Caption;

            string outputtype = GetProjectProperty(ProjectFileConstants.OutputType, false);

            if (outputtype == "library")
                name += ".dll";
            else
                name += ".exe";

            return name;
        }

        /// <summary>
        /// Updates our SCC project settings. 
        /// </summary>
        /// <param name="sccProjectName">String, opaque to the project, that identifies the project location on the server. Persist this string in the project file. </param>
        /// <param name="sccLocalPath">String, opaque to the project, that identifies the path to the server. Persist this string in the project file.</param>
        /// <param name="sccAuxPath">String, opaque to the project, that identifies the local path to the project. Persist this string in the project file.</param>
        /// <param name="sccProvider">String, opaque to the project, that identifies the source control package. Persist this string in the project file.</param>
        /// <returns>Returns true if something changed.</returns>
        private bool SetSccSettings(string sccProjectName, string sccLocalPath, string sccAuxPath, string sccProvider)
        {
            bool changed = false;
            Debug.Assert(sccProjectName != null && sccLocalPath != null && sccAuxPath != null && sccProvider != null);
            if (String.Compare(sccProjectName, this.sccProjectName, StringComparison.OrdinalIgnoreCase) != 0 ||
                String.Compare(sccLocalPath, this.sccLocalPath, StringComparison.OrdinalIgnoreCase) != 0 ||
                String.Compare(sccAuxPath, this.sccAuxPath, StringComparison.OrdinalIgnoreCase) != 0 ||
                String.Compare(sccProvider, this.sccProvider, StringComparison.OrdinalIgnoreCase) != 0)
            {
                changed = true;
                this.sccProjectName = sccProjectName;
                this.sccLocalPath = sccLocalPath;
                this.sccAuxPath = sccAuxPath;
                this.sccProvider = sccProvider;
            }


            return changed;
        }

        /// <summary>
        /// Sets the SCC info from the project file.
        /// </summary>
        private void InitSccInfo()
        {
            this.sccProjectName = this.GetProjectProperty(ProjectFileConstants.SccProjectName);
            this.sccLocalPath = this.GetProjectProperty(ProjectFileConstants.SccLocalPath);
            this.sccProvider = this.GetProjectProperty(ProjectFileConstants.SccProvider);
            this.sccAuxPath = this.GetProjectProperty(ProjectFileConstants.SccAuxPath);
        }

        private void OnAfterProjectOpen(object sender, AfterProjectFileOpenedEventArgs e)
        {
            this.projectOpened = true;
        }

        private static XmlElement WrapXmlFragment(XmlDocument document, XmlElement root, Guid flavor, string configuration, string fragment)
        {
            XmlElement node = document.CreateElement(ProjectFileConstants.FlavorProperties);
            XmlAttribute attribute = document.CreateAttribute(ProjectFileConstants.Guid);
            attribute.Value = flavor.ToString("B");
            node.Attributes.Append(attribute);
            if (!String.IsNullOrEmpty(configuration))
            {
                attribute = document.CreateAttribute(ProjectFileConstants.Configuration);
                attribute.Value = configuration;
                node.Attributes.Append(attribute);
            }
            node.InnerXml = fragment;
            root.AppendChild(node);
            return node;
        }

        /// <summary>
        /// Sets the project guid from the project file. If no guid is found a new one is created and assigned for the instance project guid.
        /// </summary>
        private void SetProjectGuidFromProjectFile()
        {
            string projectGuid = this.GetProjectProperty(ProjectFileConstants.ProjectGuid);
            if (String.IsNullOrEmpty(projectGuid))
            {
                this.projectIdGuid = Guid.NewGuid();
            }
            else
            {
                Guid guid = new Guid(projectGuid);
                if (guid != this.projectIdGuid)
                {
                    this.projectIdGuid = guid;
                }
            }
        }

        /// <summary>
        /// Helper for sharing common code between Build() and BuildAsync()
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private bool BuildPrelude(IVsOutputWindowPane output)
        {
            bool engineLogOnlyCritical = false;
            // If there is some output, then we can ask the build engine to log more than
            // just the critical events.
            if (null != output)
            {
                engineLogOnlyCritical = BuildEngine.OnlyLogCriticalEvents;
                BuildEngine.OnlyLogCriticalEvents = false;
            }

            this.SetOutputLogger(output);
            return engineLogOnlyCritical;
        }

        /// <summary>
        /// Recursively parses the tree and closes all nodes.
        /// </summary>
        /// <param name="node">The subtree to close.</param>
        private static void CloseAllNodes(HierarchyNode node)
        {
            for (HierarchyNode n = node.FirstChild; n != null; n = n.NextSibling)
            {
                if (n.FirstChild != null)
                {
                    CloseAllNodes(n);
                }

                n.Close();
            }
        }

        /// <summary>
        /// Set the build project with the new project instance value
        /// </summary>
        /// <param name="project">The new build project instance</param>
        private void SetBuildProject(MSBuild.Project project)
        {
            this.buildProject = project;
            if (this.buildProject != null)
            {
                SetupProjectGlobalPropertiesThatAllProjectSystemsMustSet();
            }
        }

        /// <summary>
        /// Setup the global properties for project instance.
        /// </summary>
        private void SetupProjectGlobalPropertiesThatAllProjectSystemsMustSet()
        {
            string solutionDirectory = null;
            string solutionFile = null;

            if(this.Site.GetService(typeof(SVsSolution)) is IVsSolution solution)
            {
                // We do not want to throw. If we cannot set the solution related constants we set them to empty string.
                solution.GetSolutionInfo(out solutionDirectory, out solutionFile, out _);
            }

            if (solutionDirectory == null)
            {
                solutionDirectory = String.Empty;
            }

            if (solutionFile == null)
            {
                solutionFile = String.Empty;
            }

            string solutionFileName = (solutionFile.Length == 0) ? String.Empty : Path.GetFileName(solutionFile);

            string solutionName = (solutionFile.Length == 0) ? String.Empty : Path.GetFileNameWithoutExtension(solutionFile);

            string solutionExtension = String.Empty;
            if (solutionFile.Length > 0 && Path.HasExtension(solutionFile))
            {
                solutionExtension = Path.GetExtension(solutionFile);
            }

            this.buildProject.SetGlobalProperty(GlobalProperty.SolutionDir.ToString(), solutionDirectory);
            this.buildProject.SetGlobalProperty(GlobalProperty.SolutionPath.ToString(), solutionFile);
            this.buildProject.SetGlobalProperty(GlobalProperty.SolutionFileName.ToString(), solutionFileName);
            this.buildProject.SetGlobalProperty(GlobalProperty.SolutionName.ToString(), solutionName);
            this.buildProject.SetGlobalProperty(GlobalProperty.SolutionExt.ToString(), solutionExtension);

            // Other misc properties
            this.buildProject.SetGlobalProperty(GlobalProperty.BuildingInsideVisualStudio.ToString(), "true");
            this.buildProject.SetGlobalProperty(GlobalProperty.Configuration.ToString(), ProjectConfig.Debug);
            this.buildProject.SetGlobalProperty(GlobalProperty.Platform.ToString(), ProjectConfig.AnyCPU);

            // DevEnvDir property
            object installDirAsObject = null;

            if(this.Site.GetService(typeof(SVsShell)) is IVsShell shell)
            {
                // We do not want to throw. If we cannot set the solution related constants we set them to empty string.
                shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirAsObject);
            }

            string installDir = ((string)installDirAsObject);

            if (String.IsNullOrEmpty(installDir))
            {
                installDir = String.Empty;
            }
            else
            {
                // Ensure that we have a trailing backslash as this is done for the langproj macros too.
                if (installDir[installDir.Length - 1] != Path.DirectorySeparatorChar)
                {
                    installDir += Path.DirectorySeparatorChar;
                }
            }

            this.buildProject.SetGlobalProperty(GlobalProperty.DevEnvDir.ToString(), installDir);
        }

        /// <summary>
        /// Attempts to lock in the privilege of running a build in Visual Studio.
        /// </summary>
        /// <param name="designTime"><c>false</c> if this build was called for by the Solution Build Manager; <c>true</c> otherwise.</param>
        /// <param name="requiresUIThread">
        /// Need to claim the UI thread for build under the following conditions:
        /// 1. The build must use a resource that uses the UI thread, such as
        /// - you set HostServices and you have a host object which requires (even indirectly) the UI thread (VB and C# compilers do this for instance.)
        /// or,
        /// 2. The build requires the in-process node AND waits on the UI thread for the build to complete, such as:
        /// - you use a ProjectInstance to build, or
        /// - you have specified a host object, whether or not it requires the UI thread, or
        /// - you set HostServices and you have specified a node affinity.
        /// - In addition to the above you also call submission.Execute(), or you call submission.ExecuteAsync() and then also submission.WaitHandle.Wait*().
        /// </param>
        /// <returns>A value indicating whether a build may proceed.</returns>
        /// <remarks>
        /// This method must be called on the UI thread.
        /// </remarks>
        private bool TryBeginBuild(bool designTime, bool requiresUIThread = false)
        {
            IVsBuildManagerAccessor accessor = null;

            if (this.Site != null)
            {
                accessor = this.Site.GetService(typeof(SVsBuildManagerAccessor)) as IVsBuildManagerAccessor;
            }

            bool releaseUIThread = false;

            try
            {
                // If the SVsBuildManagerAccessor service is absent, we're not running within Visual Studio.
                if (accessor != null)
                {
                    if (requiresUIThread)
                    {
                        int result = accessor.ClaimUIThreadForBuild();
                        if (result < 0)
                        {
                            // Not allowed to claim the UI thread right now. Try again later.
                            return false;
                        }

                        releaseUIThread = true; // assume we need to release this immediately until we get through the whole gauntlet.
                    }

                    if (designTime)
                    {
                        int result = accessor.BeginDesignTimeBuild();
                        if (result < 0)
                        {
                            // Not allowed to begin a design-time build at this time. Try again later.
                            return false;
                        }
                    }

                    // We obtained all the resources we need.  So don't release the UI thread until after the build is finished.
                    releaseUIThread = false;
                }
                else
                {
                    BuildParameters buildParameters = new BuildParameters(this.buildEngine ?? ProjectCollection.GlobalProjectCollection);
                    BuildManager.DefaultBuildManager.BeginBuild(buildParameters);
                }

                this.buildInProcess = true;
                return true;
            }
            finally
            {
                // If we were denied the privilege of starting a design-time build,
                // we need to release the UI thread.
                if (releaseUIThread)
                {
                    Debug.Assert(accessor != null, "We think we need to release the UI thread for an accessor we don't have!");
                    Marshal.ThrowExceptionForHR(accessor.ReleaseUIThreadForBuild());
                }
            }
        }

        /// <summary>
        /// Lets Visual Studio know that we're done with our design-time build so others can use the build manager.
        /// </summary>
        /// <param name="submission">The build submission that built, if any.</param>
        /// <param name="designTime">This must be the same value as the one passed to <see cref="TryBeginBuild"/>.</param>
        /// <param name="requiresUIThread">This must be the same value as the one passed to <see cref="TryBeginBuild"/>.</param>
        /// <remarks>
        /// This method must be called on the UI thread.
        /// </remarks>
        private void EndBuild(BuildSubmission submission, bool designTime, bool requiresUIThread = false)
        {
            IVsBuildManagerAccessor accessor = null;

            if (this.Site != null)
            {
                accessor = this.Site.GetService(typeof(SVsBuildManagerAccessor)) as IVsBuildManagerAccessor;
            }

            if (accessor != null)
            {
                // It's very important that we try executing all three end-build steps, even if errors occur partway through.
                try
                {
                    if (submission != null)
                    {
                        Marshal.ThrowExceptionForHR(accessor.UnregisterLoggers(submission.SubmissionId));
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorHandler.IsCriticalException(ex))
                    {
                        throw;
                    }

                    Trace.TraceError(ex.ToString());
                }

                try
                {
                    if (designTime)
                    {
                        Marshal.ThrowExceptionForHR(accessor.EndDesignTimeBuild());
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorHandler.IsCriticalException(ex))
                    {
                        throw;
                    }

                    Trace.TraceError(ex.ToString());
                }


                try
                {
                    if (requiresUIThread)
                    {
                        Marshal.ThrowExceptionForHR(accessor.ReleaseUIThreadForBuild());
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorHandler.IsCriticalException(ex))
                    {
                        throw;
                    }

                    Trace.TraceError(ex.ToString());
                }
            }
            else
            {
                BuildManager.DefaultBuildManager.EndBuild();
            }

            this.buildInProcess = false;
        }

        private string GetComponentPickerDirectories()
        {
            if(!(this.site.GetService(typeof(SCompEnumService)) is IVsComponentEnumeratorFactory4 enumFactory))
            {
                throw new InvalidOperationException("Missing the SCompEnumService service.");
            }

            Marshal.ThrowExceptionForHR(enumFactory.GetReferencePathsForTargetFramework(this.TargetFrameworkMoniker.FullName, out IEnumComponents enumerator));
            
            if (enumerator == null)
            {
                throw new ApplicationException("IVsComponentEnumeratorFactory4.GetReferencePathsForTargetFramework returned null.");
            }

            StringBuilder paths = new StringBuilder();
            VSCOMPONENTSELECTORDATA[] data = new VSCOMPONENTSELECTORDATA[1];
            
            while(enumerator.Next(1, data, out uint fetchedCount) == VSConstants.S_OK && fetchedCount == 1)
            {
                Debug.Assert(data[0].type == VSCOMPONENTTYPE.VSCOMPONENTTYPE_Path);
                paths.Append(data[0].bstrFile);
                paths.Append(";");
            }

            // Trim off the last semicolon.
            if (paths.Length > 0)
            {
                paths.Length -= 1;
            }

            return paths.ToString();
        }

        #endregion

        #region Upgrade and retargeting members
        //=====================================================================

        public int UpdateTargetFramework(IVsHierarchy pHier, string currentTargetFramework, string newTargetFramework)
        {
            FrameworkName moniker = new FrameworkName(newTargetFramework);
            SetProjectProperty("TargetFrameworkIdentifier", moniker.Identifier);
            SetProjectProperty("TargetFrameworkVersion", "v" + moniker.Version);
            SetProjectProperty("TargetFrameworkProfile", moniker.Profile);
            return VSConstants.S_OK;
        }

        // !EFW - This should be virtual so that we can override it and handle project upgrade tasks
        /// <summary>
        /// Handle project upgrades
        /// </summary>
        /// <param name="grfUpgradeFlags">The upgrade flags</param>
        /// <returns>S_OK if successful or no upgrade needed; otherwise, an error code.</returns>
        public virtual int UpgradeProject(uint grfUpgradeFlags)
        {
            int hr = VSConstants.S_OK;

            if (!PerformTargetFrameworkCheck())
            {
                // Just return OLE_E_PROMPTSAVECANCELLED here which will cause the shell
                // to leave the project in an unloaded state.
                hr = VSConstants.OLE_E_PROMPTSAVECANCELLED;
            }

            return hr;
        }

        private bool PerformTargetFrameworkCheck()
        {
            if (this.IsFrameworkOnMachine())
            {
                // Nothing to do since the framework is present.
                return true;
            }

            return ShowRetargetingDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// <c>true</c> if the project will be retargeted.  <c>false</c> to load project in unloaded state.
        /// </returns>
        private bool ShowRetargetingDialog()
        {
            if(!(this.site.GetService(typeof(SVsFrameworkRetargetingDlg)) is IVsFrameworkRetargetingDlg retargetDialog))
            {
                throw new InvalidOperationException("Missing SVsFrameworkRetargetingDlg service.");
            }

            // We can only display the retargeting dialog if the IDE is not in command-line mode.
            if (IsIdeInCommandLineMode)
            {
                string message = SR.GetString(SR.CannotLoadUnknownTargetFrameworkProject, this.FileName, this.TargetFrameworkMoniker);
                
                if(this.site.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow)
                {
                    Guid outputPaneGuid = VSConstants.GUID_BuildOutputWindowPane;
                    
                    if(outputWindow.GetPane(ref outputPaneGuid, out IVsOutputWindowPane outputPane) >= 0 && outputPane != null)
                    {
                        Marshal.ThrowExceptionForHR(outputPane.OutputStringThreadSafe(message));
                    }
                }

                throw new InvalidOperationException(message);
            }
            else
            {
                Marshal.ThrowExceptionForHR(retargetDialog.ShowFrameworkRetargetingDlg(
                    this.Package.ProductUserContext,
                    this.FileName,
                    this.TargetFrameworkMoniker.FullName,
                    (uint)__FRD_FLAGS.FRDF_DEFAULT,
                    out uint outcome,
                    out _));

                switch ((__FRD_OUTCOME)outcome)
                {
                    case __FRD_OUTCOME.FRDO_GOTO_DOWNLOAD_SITE:
                        Marshal.ThrowExceptionForHR(retargetDialog.NavigateToFrameworkDownloadUrl());
                        return false;
                    case __FRD_OUTCOME.FRDO_LEAVE_UNLOADED:
                        return false;
                    case __FRD_OUTCOME.FRDO_RETARGET_TO_40:
                        // If we are retargeting to 4.0, then set the flag to set the appropriate Target Framework.
                        // This will dirty the project file, so we check it out of source control now -- so that
                        // the user can associate getting the checkout prompt with the "No Framework" dialog.
                        if (QueryEditProjectFile(false /* bSuppressUI */))
                        {
                            if(this.site.GetService(typeof(SVsTrackProjectRetargeting)) is IVsTrackProjectRetargeting retargetingService)
                            {
                                // We surround our batch retargeting request with begin/end because in individual project load
                                // scenarios the solution load context hasn't done it for us.
                                Marshal.ThrowExceptionForHR(retargetingService.BeginRetargetingBatch());
                                Marshal.ThrowExceptionForHR(retargetingService.BatchRetargetProject(this.InteropSafeIVsHierarchy, DefaultTargetFrameworkMoniker.FullName, true));
                                Marshal.ThrowExceptionForHR(retargetingService.EndRetargetingBatch());
                            }
                            else
                            {
                                // Just setting the moniker to null will allow the default framework (.NETFX 4.0) to assert itself.
                                this.TargetFrameworkMoniker = null;
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        throw new ArgumentException("Unexpected outcome from retargeting dialog.");
                }
            }
        }

        private bool IsFrameworkOnMachine()
        {
            var multiTargeting = this.site.GetService(typeof(SVsFrameworkMultiTargeting)) as IVsFrameworkMultiTargeting;
            Marshal.ThrowExceptionForHR(multiTargeting.GetSupportedFrameworks(out Array frameworks));
            foreach (string fx in frameworks)
            {
                int hr = multiTargeting.CheckFrameworkCompatibility(this.TargetFrameworkMoniker.FullName, fx, out uint compat);
                if (hr < 0)
                {
                    break;
                }

                if ((__VSFRAMEWORKCOMPATIBILITY)compat == __VSFRAMEWORKCOMPATIBILITY.VSFRAMEWORKCOMPATIBILITY_COMPATIBLE)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        // !EFW
        #region Core Show All Files/Refresh support
        //=====================================================================

        // Core functionality implemented in the ProjectNode.cs:
        // - Added ShowingAllFiles property.
        // - Updated the ShowAllFiles() method to call ToggleShowAllFiles().
        // - Added RefreshProject() method.

        // Additional functionality not implemented in ProjectNode.cs (handle in derived class):
        // - Override ShowAllFiles() to persist toggle state if so desired.
        // - Override Reload() to restore the toggle state if so desired.
        // - Override IsItemTypeFileType() if necessary to handle custom file item types.

        /// <summary>
        /// Refresh the project by toggling the Show All Files state
        /// </summary>
        public void RefreshProject()
        {
            this.ToggleShowAllFiles();
            this.ToggleShowAllFiles();
        }

        /// <summary>
        /// Toggles the state of Show All Files
        /// </summary>
        /// <returns>S_OK if it is possible to toggle the state, OLECMDERR_E_NOTSUPPORTED if not</returns>
        protected internal int ToggleShowAllFiles()
        {
            if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;

            this.ShowingAllFiles = !this.ShowingAllFiles;

            // Set the wait cursor as this may take a while if there are a lot of files
            IVsUIShell shell = (IVsUIShell)((System.IServiceProvider)package).GetService(typeof(SVsUIShell));

            if(shell != null)
                shell.SetWaitCursor();

            if(!this.ShowingAllFiles)
            {
                // Find all non-member items and reverse the list so that we remove children first
                // followed by their parent nodes.
                foreach(var n in HierarchyNode.FindNodes(this, n => n.IsNonMemberItem).Reverse())
                {
                    n.OnItemDeleted();
                    n.Parent.RemoveChild(n);
                }
            }
            else
            {
                this.AddMissingProjectMembers();
                this.AddNonMemberItems();
            }

            return NativeMethods.S_OK;
        }

        /// <summary>
        /// This is used to get an enumerable list of folders to ignore when showing all files
        /// </summary>
        /// <returns>An enumerable list of folders to ignore.  This can cut down on the number of files to
        /// consider when checking the project to see if they are already in it.  By default, this returns
        /// an empty list.  Override this in derived project types to supply a list of folders.</returns>
        protected virtual IEnumerable<string> FoldersToIgnore()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Add non-member items to the hierarchy
        /// </summary>
        private void AddNonMemberItems()
        {
            string path;

            var ignoredFolders = this.FoldersToIgnore();

            // Get a list of the folders and files in the project folder excluding those that are hidden or not
            // wanted.  Hash sets are used so that we can do case-insensitive comparisons when excluding existing
            // project items from the lists.
            HashSet<string> folders = new HashSet<string>(Directory.EnumerateDirectories(
                this.ProjectFolder, "*", SearchOption.AllDirectories).Where(p =>
                {
                    if(ignoredFolders.Any(folder => p.StartsWith(folder, StringComparison.OrdinalIgnoreCase)))
                        return false;

                    DirectoryInfo di = new DirectoryInfo(p);

                    return !((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
                }).Select(p => PackageUtilities.MakeRelative(this.ProjectFolder, p)),
                StringComparer.OrdinalIgnoreCase);

            HashSet<string> files = new HashSet<string>(Directory.EnumerateFiles(
                this.ProjectFolder, "*", SearchOption.AllDirectories).Where(f =>
                {
                    if(ignoredFolders.Any(folder => f.StartsWith(folder, StringComparison.OrdinalIgnoreCase)))
                        return false;

                    FileInfo fi = new FileInfo(f);

                    return !((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
                }).Select(f => PackageUtilities.MakeRelative(this.ProjectFolder, f)),
                StringComparer.OrdinalIgnoreCase);

            // Remove the folders and files that are already in the project
            foreach(Microsoft.Build.Evaluation.ProjectItem item in this.BuildProject.Items)
                if(folders.Count != 0 && item.ItemType.Equals(ProjectFileConstants.Folder,
                  StringComparison.OrdinalIgnoreCase))
                {
                    path = item.EvaluatedInclude;

                    // It should be relative already but check it just in case
                    if(Path.IsPathRooted(path)) 
                        path = PackageUtilities.MakeRelative(this.ProjectFolder, path);

                    if(folders.Contains(path))
                        folders.Remove(path);
                }
                else
                    if(files.Count != 0 && this.IsItemTypeFileType(item.ItemType))
                    {
                        path = item.EvaluatedInclude;

                        // It should be relative already but check it just in case
                        if(Path.IsPathRooted(path)) 
                            path = PackageUtilities.MakeRelative(this.ProjectFolder, path);

                        if(files.Contains(path))
                            files.Remove(path);
                    }

            // Add the remaining items to the project
            this.AddNonMemberFolderItems(folders);
            this.AddNonMemberFileItems(files);
        }

        /// <summary>
        /// Add non-member folder items to the hierarchy
        /// </summary>
        /// <param name="foldersToAdd">An enumerable list of the folders to add</param>
        private void AddNonMemberFolderItems(IEnumerable<string> foldersToAdd)
        {
            IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.Site, SolutionExplorer);
            HierarchyNode parent, child, first;
            string[] pathParts;
            string childId;

            foreach(string relativePath in foldersToAdd)
            {
                parent = this;
                first = null;
                pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Add folder items for each part of the path
                foreach(string name in pathParts)
                {
                    childId = Path.Combine(parent.VirtualNodeName, name);
                    child = parent.FindChild(childId);

                    if(child == null)
                    {
                        // Note the first folder so that we can collapse it when done if necessary
                        if(first == null)
                        {
                            first = parent as FolderNode;

                            if(first != null && !first.IsNonMemberItem && first.IsExpanded)
                                first = null;
                        }

                        child = this.CreateFolderNode(childId, new ProjectElement(this, null, true));
                        child.IsNonMemberItem = true;
                        parent.AddChild(child);
                    }

                    parent = child;
                }

                if(first != null && uiWindow != null)
                {
                    first.IsExpanded = false;
                    first.SetProperty((int)__VSHPROPID.VSHPROPID_Expanded, false);
                    ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this, first.ID, EXPANDFLAGS.EXPF_CollapseFolder));
                }
            }
        }

        /// <summary>
        /// Add non-member file items to the hierarchy
        /// </summary>
        /// <param name="filesToAdd">An enumerable list of the files to add</param>
        private void AddNonMemberFileItems(IEnumerable<string> filesToAdd)
        {
            IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.Site, SolutionExplorer);
            HierarchyNode parent, child, first;
            ProjectElement element;
            string[] pathParts;
            string childId;

            foreach(string relativePath in filesToAdd)
            {
                // TODO: Probably should exclude the .user file too.  Any others?
                // Don't add the project file itself
                if(String.Equals(relativePath, this.ProjectFile, StringComparison.OrdinalIgnoreCase))
                    continue;

                parent = this;
                first = null;
                pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Find the parent folder item
                foreach(string name in pathParts)
                {
                    childId = Path.Combine(parent.VirtualNodeName, name);
                    child = parent.FindChild(childId);

                    if(child == null)
                    {
                        // Note the first folder so that we can collapse it when done if necessary
                        if(first == null)
                        {
                            first = parent as FolderNode;

                            if(first != null && !first.IsNonMemberItem && first.IsExpanded)
                                first = null;
                        }

                        element = new ProjectElement(this, null, true);
                        element.Rename(childId);
                        element.SetMetadata(ProjectFileConstants.Name, childId);
                        child = this.CreateFileNode(element);
                        child.IsNonMemberItem = true;
                        parent.AddChild(child);
                        break;
                    }

                    parent = child;
                }

                if(first != null)
                {
                    first.IsExpanded = false;
                    first.SetProperty((int)__VSHPROPID.VSHPROPID_Expanded, false);
                    ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this, first.ID, EXPANDFLAGS.EXPF_CollapseFolder));
                }
            }
        }

        /// <summary>
        /// This will scan the project looking for file items that don't exist in the hierarchy.
        /// If any are found, they are added as proper hierarchy members.
        /// </summary>
        /// <remarks>This is similar to <see cref="ProcessFiles"/> but it filters out items
        /// already in the hierarchy.</remarks>
        private void AddMissingProjectMembers()
        {
            List<String> subitemsKeys = new List<String>();
            Dictionary<String, MSBuild.ProjectItem> subitems = new Dictionary<String, MSBuild.ProjectItem>();
            HashSet<string> items = new HashSet<string>();
            string fullPath;

            foreach(MSBuild.ProjectItem item in this.buildProject.Items.ToList())
            {
                // Ignore the item if it is a reference or folder
                if (this.FilterItemTypeToBeAddedToHierarchy(item.ItemType))
                    continue;

                // MSBuilds tasks/targets can create items (such as object files), such items are not part
                // of the project per se, and should not be displayed so ignore those items.
                if(!this.IsItemTypeFileType(item.ItemType))
                    continue;

                fullPath = item.EvaluatedInclude;

                if(!Path.IsPathRooted(fullPath))
                    fullPath = Path.Combine(this.ProjectFolder, fullPath);

                // If the item is already contained do nothing
                if(items.Contains(item.EvaluatedInclude.ToUpperInvariant()) ||
                  base.FindChild(fullPath) != null)
                    continue;

                // Make sure that we do not want to add the item, dependent, or independent twice to
                // the UI hierarchy.
                items.Add(item.EvaluatedInclude.ToUpperInvariant());

                string dependentOf = item.GetMetadataValue(ProjectFileConstants.DependentUpon);

                if(!this.CanFileNodesHaveChilds || String.IsNullOrEmpty(dependentOf))
                    this.AddIndependentFileNode(item);
                else
                {
                    // We will process dependent items later.  Note that we use 2 lists as we want to
                    // remove elements from the collection as we loop through it.
                    subitemsKeys.Add(item.EvaluatedInclude);
                    subitems.Add(item.EvaluatedInclude, item);
                }
            }

            // Now process the dependent items
            if(this.CanFileNodesHaveChilds)
                this.ProcessDependentFileNodes(subitemsKeys, subitems);
        }
        #endregion
    }
}
