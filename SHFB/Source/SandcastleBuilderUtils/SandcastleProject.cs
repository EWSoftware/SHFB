//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : SandcastleProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/24/2014
// Note    : Copyright 2006-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the project class.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
#region Older History
// 1.0.0.0  08/03/2006  EFW  Created the code
// 1.1.0.0  08/28/2006  EFW  Added various new options for the August CTP
// 1.2.0.0  09/04/2006  EFW  Added new properties to support namespace summaries and project summary comments
// 1.3.1.0  09/26/2006  EFW  Added the ShowMissing* properties
// 1.3.1.0  10/02/2006  EFW  Added support for the September CTP and the the Document* properties
// 1.3.2.0  11/03/2006  EFW  Added the NamingMethod property
// 1.3.3.1  11/24/2006  EFW  Added the SyntaxFilters property, support for build component configurations, and
//                           other stuff.
// 1.3.4.0  12/24/2006  EFW  Added WorkingPath project property.  Reworked the load and save code to use
//                           reflection for most properties to simplify the code and support setting them via the
//                           command line from the console mode builder.  Converted folder properties to
//                           FolderPath objects.
// 1.4.0.0  02/02/2007  EFW  Converted PresentationStyle to a string with a type converter listing the
//                           presentation folders.  Added FooterText property.
// 1.4.0.2  05/11/2007  EFW  Missing namespace messages are now optional
// 1.5.0.2  07/03/2007  EFW  Added support to additional content site map
// 1.5.1.0  07/20/2007  EFW  Added ApiFilter and KeepLogFile project properties
// 1.5.1.0  08/24/2007  EFW  Added support for the inherited private/internal framework member flags
// 1.5.2.0  09/10/2007  EFW  Added support for plug-in configurations
// 1.6.0.0  09/30/2007  EFW  Added support for transforming *.topic files
// 1.6.0.1  10/14/2007  EFW  Added support for ShowFeedbackControl and SdkLinkTarget
// 1.6.0.2  11/01/2007  EFW  Reworked to support better handling of components
// 1.6.0.5  02/20/2008  EFW  Added the FeedbackEMailLinkText property
// 1.6.0.7  03/21/2008  EFW  Added Help 2 and ShowMissingTypeParam properties.  Removed the PurgeDuplicateTopics
//                           option.  Added the BuildLogFile property.  Started laying the foundations for
//                           conceptual content support.  ContentSiteMap and TopicFileTransform were made
//                           sub-properties of the additional content collection to connect them with it and to
//                           prevent associating them with conceptual content.
// 1.8.0.0  06/20/2008  EFW  Converted the project to use an MSBuild file.
// 1.8.0.1  12/14/2008  EFW  Updated for use with .NET 3.5 and MSBuild 3.5.  Added support for user-defined
//                           project properties.  Added support for ShowMissingIncludeTargets.
// 1.8.0.3  07/05/2009  EFW  Added support for MS Help Viewer format
// 1.8.0.3  11/10/2009  EFW  Changed SyntaxFilters property to a string to support custom syntax filter build
//                           components.
// 1.8.0.3  11/19/2009  EFW  Added support for AutoDocumentDisposeMethods
// 1.8.0.3  12/06/2009  EFW  Removed support for ShowFeedbackControl
// 1.9.0.0  06/19/2010  EFW  Added properties to support MS Help Viewer.  Removed ProjectLinkType property.
//                           Replaced SdkLinkType with help format specific SDK link type properties.
#endregion
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0
// 1.9.2.0  01/16/2011  EFW  Updated to support selection of Silverlight Framework versions
// 1.9.3.2  08/20/2011  EFW  Updated to support selection of .NET Portable Framework versions
// 1.9.4.0  04/08/2012  EFW  Merged changes for VS2010 style from Don Fehr.  Added BuildAssemblerVerbosity
//                           property.  Added Support for XAML configuration files.
// 1.9.5.0  09/10/2012  EFW  Updated to use the new framework definition file for the .NET Framework versions.
//                           Added the CatalogName property for Help Viewer 2.0 support.
// 1.9.6.0  10/13/2012  EFW  Removed the BrandingPackageName and SelfBranded properties.  Added support for
//                           transform component arguments.
// 1.9.9.0  11/30/2013  EFW  Merged changes from Stazzz to support namespace grouping
// -------  12/17/2013  EFW  Removed the SandcastlePath property and all references to it
//          12/20/2013  EFW  Added support for the ComponentPath project property
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.Frameworks;
using Sandcastle.Core.PresentationStyle;

using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class represents all of the properties that make up a Sandcastle Help File Builder project
    /// </summary>
    [DefaultProperty("HtmlHelpName")]
    public class SandcastleProject : IBasePathProvider, IDisposable
    {
        #region Constants
        //=====================================================================

        // NOTE: When this changes, update the version in Resources\ProjectTemplate.txt and in the
        //       SancastleBuilderPackage project templates.
        /// <summary>
        /// The schema version used in the saved project files
        /// </summary>
        public static readonly Version SchemaVersion = new Version(1, 9, 9, 0);

        /// <summary>The default configuration</summary>
        public const string DefaultConfiguration = "Debug";
        /// <summary>The default platform</summary>
        public const string DefaultPlatform = "AnyCPU";

        // Restricted property names that cannot be used for user-defined property names
        internal static List<string> restrictedProps = new List<string>() {
            "AssemblyName", "Configuration", "CustomAfterSHFBTargets", "CustomBeforeSHFBTargets",
            "DumpLogOnFailure", "Name", "Platform", "PostBuildEvent", "PreBuildEvent", "ProjectGuid",
            "RootNamespace", "RunPostBuildEvent", "SccAuxPath", "SccLocalPath", "SccProjectName", "SccProvider",
            "SchemaVersion", "SHFBSchemaVersion", "TransformComponentArguments", "Verbose" };

        // Bad characters for the vendor name property
        private static Regex reBadVendorNameChars = new Regex(@"[:\\/\.,#&]");
        #endregion

        #region Private data members
        //=====================================================================

        // These are used to decode hex values in the copyright text
        private static Regex reDecode = new Regex(@"\\x[0-9a-f]{2,4}", RegexOptions.IgnoreCase);

        private MatchEvaluator characterMatchEval, buildVarMatchEval;

        // MS Build and property items
        private Project msBuildProject;
        private Dictionary<string, ProjectProperty> projectPropertyCache;  // MSBuild property cache
        private bool usingFinalValues, removeProjectWhenDisposed;

        // Local property info cache
        private static Dictionary<string, PropertyInfo> propertyCache = InitializePropertyCache();
        private static PropertyDescriptorCollection pdcCache;
        private bool loadingProperties, isDirty;

        // The list of documentation sources to use when building the help file
        private DocumentationSourceCollection docSources;

        // List of namespace summary items
        private NamespaceSummaryItemCollection namespaceSummaries;

        // List of reference dependencies for the documentation sources
        private ReferenceItemCollection references;

        // Build component configurations
        private ComponentConfigurationDictionary componentConfigs;

        // Build process plug-in configurations
        private PlugInConfigurationDictionary plugInConfigs;

        // Path and build-related properties
        private FolderPath hhcPath, hxcompPath, workingPath, componentPath;
        private FilePath buildLogFile;
        private string outputPath, frameworkVersion;
        private bool cleanIntermediates, keepLogFile, cppCommentsFixup, disableCodeBlockComponent,
            namespaceGrouping;
        private int maximumGroupParts;
        private HelpFileFormats helpFileFormat;
        private BuildAssemblerVerbosity buildAssemblerVerbosity;

        // Help file properties
        private ContentPlacement contentPlacement;
        private bool binaryTOC, includeFavorites, preliminary, rootNSContainer, includeStopWordList, indentHtml;
        private string helpTitle, htmlHelpName, copyrightHref, copyrightText, feedbackEMailAddress,
            feedbackEMailLinkText, headerText, footerText, projectSummary, rootNSTitle, presentationStyle,
            plugInNamespaces, helpFileVersion, syntaxFilters, vendorName, productTitle, topicVersion,
            tocParentId, tocParentVersion, catalogProductId, catalogVersion, catalogName;
        private CultureInfo language;
        private HtmlSdkLinkType htmlSdkLinkType, websiteSdkLinkType;
        private MSHelp2SdkLinkType help2SdkLinkType;
        private MSHelpViewerSdkLinkType helpViewerSdkLinkType;
        private SdkLinkTarget sdkLinkTarget;
        private NamingMethod namingMethod;
        private CollectionTocStyle collectionTocStyle;
        private int tocOrder;

        // Help 2 additional attributes
        private MSHelpAttrCollection helpAttributes;

        // Show Missing Tags options
        private MissingTags missingTags;

        // Visibility options
        private VisibleItems visibleItems;
        private ApiFilterCollection apiFilter;
        #endregion

        #region Non-browsable Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the MSBuild project property cache
        /// </summary>
        [Browsable(false), XmlIgnore]
        private Dictionary<string, ProjectProperty> ProjectPropertyCache
        {
            get
            {
                // There can be duplicate versions of the properties so pick the last one as it will contain
                // the value to use.
                if(projectPropertyCache == null)
                    projectPropertyCache = msBuildProject.AllEvaluatedProperties.GroupBy(p => p.Name).Select(
                        g => g.Last()).ToDictionary(p => p.Name);

                return projectPropertyCache;
            }
        }

        /// <summary>
        /// This read-only property is used to get the underlying MSBuild
        /// project.
        /// </summary>
        [Browsable(false), XmlIgnore]
        public Project MSBuildProject
        {
            get { return msBuildProject; }
        }

        /// <summary>
        /// This read-only property is used to get whether or not the project
        /// is using final values for the project properties.
        /// </summary>
        /// <value>If true, final values (i.e. evaluated values used at build
        /// time) are being returned by the properties in this instance.</value>
        [Browsable(false), XmlIgnore]
        public bool UsingFinalValues
        {
            get { return usingFinalValues; }
        }

        /// <summary>
        /// This read-only property is used to get the filename for the project
        /// </summary>
        [Browsable(false), XmlIgnore]
        public string Filename
        {
            get { return msBuildProject.FullPath; }
        }

        /// <summary>
        /// This is used to get or set the configuration to use when building
        /// the project.
        /// </summary>
        /// <value>This value is used for project documentation sources and
        /// project references so that the correct items are used from them.</value>
        [Browsable(false), XmlIgnore]
        public string Configuration
        {
            get
            {
                string config = null;

                if(msBuildProject != null)
                {
                    if(!msBuildProject.GlobalProperties.TryGetValue(ProjectElement.Configuration, out config))
                        config = null;
                }

                if(String.IsNullOrEmpty(config))
                    config = DefaultConfiguration;

                return config;
            }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = DefaultConfiguration;

                msBuildProject.SetGlobalProperty(ProjectElement.Configuration, value);
                msBuildProject.ReevaluateIfNecessary();
            }
        }

        /// <summary>
        /// This is used to get or set the platform to use when building the
        /// project.
        /// </summary>
        /// <value>This value is used for project documentation sources and
        /// project references so that the correct items are used from them.</value>
        [Browsable(false), XmlIgnore]
        public string Platform
        {
            get
            {
                string platform = null;

                if(msBuildProject != null)
                {
                    if(!msBuildProject.GlobalProperties.TryGetValue(ProjectElement.Platform, out platform))
                        platform = null;
                }

                if(String.IsNullOrEmpty(platform))
                    platform = DefaultPlatform;

                return platform;
            }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = DefaultPlatform;

                msBuildProject.SetGlobalProperty(ProjectElement.Platform, value);
                msBuildProject.ReevaluateIfNecessary();
            }
        }

        /// <summary>
        /// This is used to get or set the MSBuild <c>OutDir</c> property value
        /// that is defined when using Team Build.
        /// </summary>
        /// <value>This value is used for project documentation sources and
        /// project references so that the correct items are used from them.</value>
        [Browsable(false), XmlIgnore]
        public string MSBuildOutDir
        {
            get
            {
                string outDir = null;

                if(msBuildProject != null)
                {
                    // Ignore ".\" as that's our default.
                    if(msBuildProject.GlobalProperties.TryGetValue(ProjectElement.OutDir, out outDir))
                        if(outDir == @".\")
                            outDir = null;
                }

                return outDir;
            }
            set
            {
                if(value == null)
                    value = String.Empty;

                msBuildProject.SetGlobalProperty(ProjectElement.OutDir, value);
                msBuildProject.ReevaluateIfNecessary();
            }
        }

        /// <summary>
        /// This is used to get the dirty state of the project
        /// </summary>
        [Browsable(false)]
        public bool IsDirty
        {
            get { return isDirty || msBuildProject.Xml.HasUnsavedChanges; }
        }

        /// <summary>
        /// This is used to get a collection of reference dependencies (files,
        /// GAC, COM, or project) for MRefBuilder if needed.
        /// </summary>
        [Browsable(false)]
        public ReferenceItemCollection References
        {
            get { return references; }
        }

        /// <summary>
        /// Returns the list of documentation sources to use in building the
        /// help file.
        /// </summary>
        [Browsable(false)]
        public DocumentationSourceCollection DocumentationSources
        {
            get { return docSources; }
        }

        /// <summary>
        /// This read-only property is used to get the build log file
        /// location.
        /// </summary>
        /// <value>If <see cref="BuildLogFile"/> is set, it returns its
        /// value.  If not set, it returns the full path created by using
        /// the <see cref="OutputPath"/> property value and a filename of
        /// <b>LastBuild.log</b>.</value>
        [Browsable(false)]
        public string LogFileLocation
        {
            get
            {
                string path;

                if(buildLogFile.Path.Length != 0)
                    return buildLogFile.ToString();

                if(Path.IsPathRooted(outputPath))
                    path = outputPath;
                else
                    path = Path.Combine(Path.GetDirectoryName(msBuildProject.FullPath), outputPath);

                return Path.GetFullPath(path + "LastBuild.log");
            }
        }

        /// <summary>
        /// This is used to get the copyright notice that appears in the footer
        /// of each page with any hex value place holders replaced with their
        /// actual character.
        /// </summary>
        [Browsable(false)]
        public string DecodedCopyrightText
        {
            get
            {
                return reDecode.Replace(copyrightText, characterMatchEval);
            }
        }

        /// <summary>
        /// This read-only helper property returns the flags to use when
        /// looking for missing tags.
        /// </summary>
        [Browsable(false)]
        public MissingTags MissingTags
        {
            get { return missingTags; }
            private set
            {
                this.SetProjectProperty("MissingTags", value);
                missingTags = value;
            }
        }

        /// <summary>
        /// This read-only helper property returns the flags used to indicate
        /// which optional items to document.
        /// </summary>
        [Browsable(false)]
        public VisibleItems VisibleItems
        {
            get { return visibleItems; }
            private set
            {
                this.SetProjectProperty("VisibleItems", value);
                visibleItems = value;
            }
        }

        /// <summary>
        /// This returns a collection of all build items in the project that
        /// represent folders and files.
        /// </summary>
        /// <remarks>This collection is generated each time the property is
        /// used.  As such, cache a copy if you need to use it repeatedly.</remarks>
        [Browsable(false)]
        public Collection<FileItem> FileItems
        {
            get
            {
                Collection<FileItem> fileItems = new Collection<FileItem>();
                List<string> buildActions = new List<string>(Enum.GetNames(typeof(BuildAction)));

                foreach(ProjectItem item in msBuildProject.AllEvaluatedItems)
                    if(buildActions.IndexOf(item.ItemType) != -1)
                        fileItems.Add(new FileItem(new ProjectElement(this, item)));

                return fileItems;
            }
        }

        /// <summary>
        /// This returns an enumerable list of transform component arguments
        /// </summary>
        /// <remarks>These are passed as arguments to the XSL transformations used by the <b>BuildAssembler</b>
        /// <c>TransformComponent</c>.</remarks>
        /// <returns>An enumerable list of transform component arguments</returns>
        [Browsable(false)]
        public IEnumerable<TransformComponentArgument> TransformComponentArguments
        {
            get
            {
                var argsProp = msBuildProject.GetProperty("TransformComponentArguments");

                if(argsProp != null && !String.IsNullOrEmpty(argsProp.UnevaluatedValue))
                {
                    var xr = new XmlTextReader("<Args>" + argsProp.UnevaluatedValue + "</Args>",
                        XmlNodeType.Element, new XmlParserContext(null, null, null, XmlSpace.Preserve));
                    xr.Namespaces = false;
                    xr.MoveToContent();

                    foreach(var arg in XElement.Load(xr, LoadOptions.PreserveWhitespace).Descendants("Argument"))
                        yield return new TransformComponentArgument(arg);
                }
            }
        }
        #endregion

        #region Summary related properties
        //=====================================================================

        /// <summary>
        /// Returns the list of namespace summaries
        /// </summary>
        [Category("Summaries"), Description("Namespaces to document and their related summary comments")]
        public NamespaceSummaryItemCollection NamespaceSummaries
        {
            get { return namespaceSummaries; }
        }

        /// <summary>
        /// This is used to get or set the project summary comments
        /// </summary>
        /// <remarks>These notes will appear in the root namespaces page if
        /// entered.</remarks>
        [Category("Summaries"), Description("Project summary comments"), EscapeValue]
        public string ProjectSummary
        {
            get { return projectSummary; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;

                this.SetProjectProperty("ProjectSummary", value);
                projectSummary = value;
            }
        }
        #endregion

        #region Path-related properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to a folder containing additional, project-specific build
        /// components.
        /// </summary>
        /// <value>If left blank, the current project's folder is searched instead.</value>
        [Category("Paths"), Description("This is used to get or set the path to a folder containing " +
          "additional, project-specific build components."), DefaultValue(null),
          Editor(typeof(FolderPathObjectEditor), typeof(UITypeEditor)),
          FolderDialog("Select an additional build component location", Environment.SpecialFolder.MyDocuments)]
        public FolderPath ComponentPath
        {
            get { return componentPath; }
            set
            {
                if(value == null)
                    value = new FolderPath(this);

                this.SetProjectProperty("ComponentPath", value);
                componentPath = value;
                componentPath.PersistablePathChanging += PathProperty_Changing;
                componentPath.PersistablePathChanged += PathProperty_Changed;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the HTML Help 1 compiler
        /// (HHC.EXE).
        /// </summary>
        /// <value>You only need to set this if the builder cannot determine
        /// the path for itself.</value>
        [Category("Paths"), Description("The path to the HTML Help 1 compiler (HHC.EXE).  This only needs to " +
          "be set if the builder cannot determine the path for itself."), DefaultValue(null),
          Editor(typeof(FolderPathObjectEditor), typeof(UITypeEditor)),
          FolderDialog("Select the HTML Help 1 compiler installation location", Environment.SpecialFolder.ProgramFiles)]
        public FolderPath HtmlHelp1xCompilerPath
        {
            get { return hhcPath; }
            set
            {
                if(value == null)
                    value = new FolderPath(this);

                this.SetProjectProperty("HtmlHelp1xCompilerPath", value);
                hhcPath = value;
                hhcPath.PersistablePathChanging += PathProperty_Changing;
                hhcPath.PersistablePathChanged += PathProperty_Changed;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the MS Help 2 compiler
        /// (HXCOMP.EXE).
        /// </summary>
        /// <value>You only need to set this if the builder cannot determine
        /// the path for itself.</value>
        [Category("Paths"), Description("The path to the MS Help 2 compiler (HXCOMP.EXE).  This only needs to " +
          "be set if the builder cannot determine the path for itself."), DefaultValue(null),
          Editor(typeof(FolderPathObjectEditor), typeof(UITypeEditor)),
          FolderDialog("Select the MS Help 2 compiler installation location", Environment.SpecialFolder.ProgramFiles)]
        public FolderPath HtmlHelp2xCompilerPath
        {
            get { return hxcompPath; }
            set
            {
                if(value == null)
                    value = new FolderPath(this);

                this.SetProjectProperty("HtmlHelp2xCompilerPath", value);
                hxcompPath = value;
                hxcompPath.PersistablePathChanging += PathProperty_Changing;
                hxcompPath.PersistablePathChanged += PathProperty_Changed;
            }
        }

        /// <summary>
        /// This is used to get or set the path to which the help files
        /// will be generated.
        /// </summary>
        /// <remarks>The default is to create it in a folder called
        /// <b>Help</b> in the same folder as the project file.
        /// <p/><b>Warning:</b> If building a web site, the output folder's
        /// prior content will be erased without warning prior to copying
        /// the new web site content to it!</remarks>
        [Category("Paths"), Description("The path to which the help files will be generated.  The default is " +
          "to save it to the .\\Help folder relative to the project file's folder.  WARNING: When building a " +
          "web site, the prior content of the output folder will be erased without warning before copying the " +
          "new content to it!"), DefaultValue(@".\Help\"),
          Editor(typeof(FolderPathStringEditor), typeof(UITypeEditor)),
          FolderDialog("Select the output location for the help file", Environment.SpecialFolder.Personal)]
        public string OutputPath
        {
            get { return outputPath; }
            set
            {
                if(value == null)
                    value = @".\Help\";
                else
                    if(value.Trim().Length == 0)
                        value = @".\";
                    else
                        value = FolderPath.TerminatePath(value);

                this.SetProjectProperty("OutputPath", value);
                outputPath = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the working folder used
        /// during the build process to store the intermediate files.
        /// </summary>
        /// <value>This can be used to perform the build in a different
        /// location with a shorter path if you encounter errors due to long
        /// file path names.  If not specified, it defaults to a folder
        /// called <b>.\Working</b> under the folder specified by the
        /// <see cref="OutputPath"/> property.
        /// <p/><b>Warning:</b> All files and folders in the path specified
        /// in this property will be erased without warning when the build
        /// starts.</value>
        [Category("Paths"), Description("An alternate location to use for " +
          "the intermediate build files.  If not set, it defaults to " +
          ".\\Working under the OutputPath folder.  WARNING: All files and " +
          "folders in this path will be erased without warning when the " +
          "build starts!"), DefaultValue(null),
          Editor(typeof(FolderPathObjectEditor), typeof(UITypeEditor)),
          FolderDialog("Select the working files location", Environment.SpecialFolder.Personal)]
        public FolderPath WorkingPath
        {
            get { return workingPath; }
            set
            {
                if(value == null)
                    value = new FolderPath(this);

                this.SetProjectProperty("WorkingPath", value);
                workingPath = value;
                workingPath.PersistablePathChanging += PathProperty_Changing;
                workingPath.PersistablePathChanged += PathProperty_Changed;
            }
        }
        #endregion

        #region Build-related properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the build assembler tool verbosity level
        /// </summary>
        /// <value>The default is <c>AllMessages</c> to report all messages.</value>
        /// <remarks>Setting this property to <c>OnlyWarningsAndErrors</c> or <c>OnlyErrors</c> can
        /// significantly reduce the size of the build log for large projects.</remarks>
        [Category("Build"), Description("This sets the verbosity level of the BuildAssembler tool.  Setting it " +
          "to OnlyWarningsAndErrors or OnlyErrors can significantly reduce the size of the build log for " +
          "large projects."), DefaultValue(BuildAssemblerVerbosity.OnlyWarningsAndErrors)]
        public BuildAssemblerVerbosity BuildAssemblerVerbosity
        {
            get { return buildAssemblerVerbosity; }
            set
            {
                this.SetProjectProperty("BuildAssemblerVerbosity", value);
                buildAssemblerVerbosity = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether intermediate files are deleted after a successful build
        /// </summary>
        /// <value>The default value is true.</value>
        [Category("Build"), Description("If set to true, intermediate files are deleted after a successful build"),
          DefaultValue(true)]
        public bool CleanIntermediates
        {
            get { return cleanIntermediates; }
            set
            {
                this.SetProjectProperty("CleanIntermediates", value);
                cleanIntermediates = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the log file is retained after a successful build
        /// </summary>
        /// <value>The default value is true.</value>
        [Category("Build"), Description("If set to true, the log file is retained after a successful build.  " +
          "If false, it is deleted."), DefaultValue(true)]
        public bool KeepLogFile
        {
            get { return keepLogFile; }
            set
            {
                this.SetProjectProperty("KeepLogFile", value);
                keepLogFile = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path and filename of the build log file
        /// </summary>
        /// <value>If not specified, a default name of <b>LastBuild.log</b> is used and the file is saved in the
        /// path identified in the <see cref="OutputPath" /> property.</value>
        [Category("Build"), Description("The build log filename.  If not specified, a file called " +
          "LastBuild.log is created in the folder identified by the OutputPath property."), DefaultValue(null),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          FileDialog("Select the log file location", "Log files (*.log)|*.log|All Files (*.*)|*.*",
          FileDialogType.FileSave)]
        public FilePath BuildLogFile
        {
            get { return buildLogFile; }
            set
            {
                if(value == null)
                    value = new FilePath(this);

                this.SetProjectProperty("BuildLogFile", value);
                buildLogFile = value;
                buildLogFile.PersistablePathChanging += PathProperty_Changing;
                buildLogFile.PersistablePathChanged += PathProperty_Changed;
            }
        }

        /// <summary>
        /// This is used to get or set the help file format generated by the build process
        /// </summary>
        /// <value>The default is to produce an HTML Help 1 format file built using HHC.exe.</value>
        /// <remarks>If building a web site, the output folder will be cleared before the new content is copied
        /// to it.</remarks>
        [Category("Build"), Description("Specify the type of help produced (HTML Help 1 built with HHC.EXE, " +
          "MS Help 2 built with HXCOMP.EXE, MS Help Viewer which is a compressed container file, and/or a web " +
          "site.  WARNING: When building a web site, the prior content of the output folder will be erased " +
          "without warning before copying the new content to it!"), DefaultValue(HelpFileFormats.HtmlHelp1),
          Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
        public HelpFileFormats HelpFileFormat
        {
            get { return helpFileFormat; }
            set
            {
                this.SetProjectProperty("HelpFileFormat", value);
                helpFileFormat = value;
            }
        }

        /// <summary>
        /// This is used to disable the custom Code Block Component so that <c>&lt;code&gt;</c> elements are
        /// rendered in their standard format by the Sandcastle XSL transformations.
        /// </summary>
        /// <value>The default is false so that the Code Block Component is used by default.</value>
        [Category("Build"), Description("Set this to true to disable the custom Code Block Component and " +
          "render all <code> elements in their default format using the Sandcastle XSL transformations"),
          DefaultValue(false)]
        public bool DisableCodeBlockComponent
        {
            get { return disableCodeBlockComponent; }
            set
            {
                this.SetProjectProperty("DisableCodeBlockComponent", value);
                disableCodeBlockComponent = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether to fix-up the XML comments files to work around an issue with
        /// those generated by the C++ compiler.
        /// </summary>
        /// <value>The default value is false.</value>
        /// <remarks>The C++ compiler generates method signatures that differ from the other .NET compilers for
        /// methods that take generics as parameters.  These methods fail to get documented as they do not match
        /// the output of <b>MRefBuilder</b>.  The C# and VB.NET compilers generate names that do match it and
        /// this option is not needed for comments files generated by them.  Set this to true if the project
        /// contains C++ compiler generated XML comments files and your project contains methods that take
        /// generic types for parameters.</remarks>
        [Category("Build"), Description("Set this to true to work around a C++ compiler generated XML " +
          "comments file issue."), DefaultValue(false)]
        public bool CppCommentsFixup
        {
            get { return cppCommentsFixup; }
            set
            {
                this.SetProjectProperty("CppCommentsFixup", value);
                cppCommentsFixup = value;
            }
        }

        /// <summary>
        /// This is used to get or set the .NET Framework version used to resolve references to system types
        /// (basic .NET Framework, Silverlight, Portable, etc.).
        /// </summary>
        /// <remarks>If set to null, it will default to the most recent version of the basic .NET Framework
        /// installed.</remarks>
        [Category("Build"), Description("The .NET Framework version used to resolve references to system types."),
          EscapeValue]
        public string FrameworkVersion
        {
            get { return frameworkVersion; }
            set
            {
                // Let bad values through.  The property pages or the build engine will catch bad values if
                // necessary.
                if(value == null)
                    value = FrameworkDictionary.DefaultFrameworkTitle;

                this.SetProjectProperty("FrameworkVersion", value);
                frameworkVersion = value;
            }
        }

        /// <summary>
        /// This is used to get a dictionary of build component configurations.
        /// </summary>
        /// <remarks>This allows you to configure the settings for third party build components if they
        /// support it.</remarks>
        [Category("Build"), Description("Configuration options for third party build components such as the " +
          "Code Block Colorizer")]
        public ComponentConfigurationDictionary ComponentConfigurations
        {
            get { return componentConfigs; }
        }

        /// <summary>
        /// This is used to get a dictionary of build process plug-in configurations
        /// </summary>
        /// <remarks>This allows you to select and configure the settings for third party build process
        /// plug-ins.</remarks>
        [Category("Build"), Description("Configuration options for third party build process plug-ins such " +
          "as the AjaxDoc plug-in")]
        public PlugInConfigurationDictionary PlugInConfigurations
        {
            get { return plugInConfigs; }
        }

        /// <summary>
        /// This is used to get or set whether or not the HTML rendered by <b>BuildAssembler</b> is indented.
        /// </summary>
        /// <value>This is mainly a debugging aid.  Leave it set to false, the default to produce more compact
        /// HTML.</value>
        [Category("Build"), Description("Debugging aid.  If set to true, the HTML rendered by BuildAssembler " +
          "is indented to make it more readable.  Leave it set to false to produce more compact HTML."),
          DefaultValue(false)]
        public bool IndentHtml
        {
            get { return indentHtml; }
            set
            {
                this.SetProjectProperty("IndentHtml", value);
                indentHtml = value;
            }
        }
        #endregion

        #region Help file properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the placement of any additional and
        /// conceptual content items in the table of contents.
        /// </summary>
        /// <value>The default is to place additional and conceptual content
        /// items above the namespaces.</value>
        [Category("Help File"),
          Description("Specify whether the additional and conceptual content items appear above or below " +
          "the namespaces in the table of contents.  This will be ignored if the TOC is split via a " +
          "custom tag or a site map/conceptual content topic setting."),
          DefaultValue(ContentPlacement.AboveNamespaces)]
        public ContentPlacement ContentPlacement
        {
            get { return contentPlacement; }
            set
            {
                this.SetProjectProperty("ContentPlacement", value);
                contentPlacement = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not all pages should be
        /// marked with a "preliminary documentation" warning in the page
        /// header.
        /// </summary>
        [Category("Help File"), Description("If true, all pages will contain " +
            "a 'preliminary documentation' warning in the page header"),
            DefaultValue(false)]
        public bool Preliminary
        {
            get { return preliminary; }
            set
            {
                this.SetProjectProperty("Preliminary", value);
                preliminary = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not a root namespace entry
        /// is added to the table of contents to act as a container for the
        /// namespaces from the documented assemblies.
        /// </summary>
        /// <value>If true, a root <b>Namespaces</b> table of contents entry
        /// will be created as the container of the namespaces in the
        /// documented assemblies.  If false, the default, the namespaces are
        /// listed in the table of contents as root entries.</value>
        [Category("Help File"), Description("If true, a root \"Namespaces\" " +
          "table of contents entry will be created as the container of the " +
          "namespaces in the documented assemblies.  If false, the default, " +
          "the namespaces are listed in the table of contents as root entries."),
          DefaultValue(false)]
        public bool RootNamespaceContainer
        {
            get { return rootNSContainer; }
            set
            {
                this.SetProjectProperty("RootNamespaceContainer", value);
                rootNSContainer = value;
            }
        }

        /// <summary>
        /// This is used to get or set an alternate title for the root
        /// namespaces page and the root table of contents container that
        /// appears when <see cref="RootNamespaceContainer"/> is set to true.
        /// </summary>
        /// <value>If left blank (the default), the localized version of the
        /// text "Namespaces" will be used.</value>
        [Category("Help File"), Description("An alternate title for the root " +
          "namespaces page and the root table of contents container."),
          DefaultValue(""), EscapeValue]
        public string RootNamespaceTitle
        {
            get { return rootNSTitle; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("RootNamespaceTitle", value);
                rootNSTitle = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether namespace grouping is enabled.  The presentation style must have
        /// support for namespace grouping in order for the feature to work.
        /// </summary>
        /// <value>If <c>true</c>, namespace grouping is enabled. Otherwise, namespace grouping is not enabled.</value>
        /// <remarks>Namespace groups are determined automatically and may be documented as well.</remarks>
        [Category("Help file"), Description("If true, and if the presentation style supports it, this enables " +
          "the namespace grouping feature.  The namespace groups behave like normal namespaces but they may " +
          "not contain types or anything else.  Each group is determined automatically."), DefaultValue(false)]
        public bool NamespaceGrouping
        {
            get { return namespaceGrouping; }
            set
            {
                this.SetProjectProperty("NamespaceGrouping", value);
                namespaceGrouping = value;
            }
        }

        /// <summary>
        /// This is used to get or set the maximum number of namespace parts to consider when namespace grouping
        /// is enabled.
        /// </summary>
        /// <value>The minimum and default is 2.  A higher value results in more namespace groups.</value>
        /// <remarks>Namespace groups are determined automatically and may be documented as well.</remarks>
        [Category("Help file"), Description("This is used to get or set the maximum number of namespace parts " +
          "to consider when namespace grouping is enabled."), DefaultValue(2)]
        public int MaximumGroupParts
        {
            get { return maximumGroupParts; }
            set
            {
                if(value < 2)
                    value = 2;

                this.SetProjectProperty("MaximumGroupParts", value);
                maximumGroupParts = value;
            }
        }

        /// <summary>
        /// This is used to get or set the help file's title
        /// </summary>
        [Category("Help File"), Description("The title for the help file"),
          DefaultValue("A Sandcastle Documented Class Library"), EscapeValue]
        public string HelpTitle
        {
            get { return helpTitle; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "A Sandcastle Documented Class Library";
                else
                    value = value.Trim();

                this.SetProjectProperty("HelpTitle", value);
                helpTitle = value;
            }
        }

        /// <summary>
        /// This is used to get or set the name of the compiled help file.
        /// Do not include a path or the extension.  For MS Help 2 builds,
        /// this is also used as the collection namespace name (avoid spaces).
        /// </summary>
        [Category("Help File"), Description("The name of the compiled help " +
          "file.  Do not include a path or the extension.  For MS Help 2 " +
          "builds, this is also used as the collection namespace name " +
          "(avoid spaces)."), DefaultValue("Documentation"), EscapeValue]
        public string HtmlHelpName
        {
            get { return htmlHelpName; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "Documentation";
                else
                    value = value.Trim();

                this.SetProjectProperty("HtmlHelpName", value);
                htmlHelpName = value;
            }
        }

        /// <summary>
        /// This is used to get or set the language option for the help file and to determine which set of
        /// presentation resource files to use.
        /// </summary>
        /// <value>If a matching set of presentation resources cannot be found for the specified language, the
        /// US English set will be used.</value>
        /// <remarks>The MS Help Viewer 1.0 Catalog ID is composed of the <see cref="CatalogProductId"/>, the
        /// <see cref="CatalogVersion"/>, and the <c>Language</c> code. For example, the English Visual Studio 10
        /// catalog is <c>VS_100_EN-US</c>.</remarks>
        [Category("Help File"), Description("The help file language.  For MS Help Viewer 1.0, this is the " +
          "Locale portion of the Catalog ID."), DefaultValue(typeof(CultureInfo), "en-US"),
          TypeConverter(typeof(LanguageResourceConverter))]
        public CultureInfo Language
        {
            get { return language; }
            set
            {
                if(value == null || value == CultureInfo.InvariantCulture)
                    value = new CultureInfo("en-US");

                this.SetProjectProperty("Language", value);
                language = value;
            }
        }

        /// <summary>
        /// This is used to get or set the URL to use as the link for the
        /// copyright notice.
        /// </summary>
        /// <value>If not set, the see cref="CopyrighText"/> (if any) is not
        /// turned into a clickable link.</value>
        [Category("Help File"), Description("The URL reference for the copyright notice"),
            DefaultValue(""), EscapeValue]
        public string CopyrightHref
        {
            get { return copyrightHref; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("CopyrightHref", value);
                copyrightHref = value;
            }
        }

        /// <summary>
        /// This is used to get or set the copyright notice that appears in
        /// the footer of each page.
        /// </summary>
        /// <remarks>If not set, no copyright note will appear.  If a
        /// <see cref="CopyrightHref" /> is specified without copyright text,
        /// the URL appears instead.</remarks>
        [Category("Help File"), Description("The copyright notice for the page footer"),
            DefaultValue(""), EscapeValue]
        public string CopyrightText
        {
            get { return copyrightText; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("CopyrightText", value);
                copyrightText = value;
            }
        }

        /// <summary>
        /// This is used to get or set the feedback e-mail address that
        /// appears in the footer of each page.
        /// </summary>
        /// <remarks>If not set, no feedback link will appear.  If
        /// <see cref="FeedbackEMailLinkText"/> is set, that text will appear
        /// as the text for the link.  If not set, the e-mail address is used
        /// as the link text.</remarks>
        [Category("Help File"), Description("The feedback e-mail address that " +
          "will appear in the footer of each page"), DefaultValue(""),
          EscapeValue]
        public string FeedbackEMailAddress
        {
            get { return feedbackEMailAddress; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("FeedbackEMailAddress", value);
                feedbackEMailAddress = value;
            }
        }

        /// <summary>
        /// This is used to get or set the feedback e-mail link text that
        /// appears in the feedback e-mail link in the footer of each page.
        /// </summary>
        /// <remarks>If set, this text will appear as the link text for the
        /// <see cref="FeedbackEMailAddress"/> link.  If not set, the e-mail
        /// address is used for the link text.</remarks>
        [Category("Help File"), Description("The text to display in place " +
          "of the e-mail address in the feedback e-mail link."),
          DefaultValue(""), EscapeValue]
        public string FeedbackEMailLinkText
        {
            get { return feedbackEMailLinkText; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("FeedbackEMailLinkText", value);
                feedbackEMailLinkText = value;
            }
        }

        /// <summary>
        /// This is used to get or set additional text that should appear
        /// in the header of every page.
        /// </summary>
        [Category("Help File"), Description("Additional text for the header " +
          "in every page"), DefaultValue(""), EscapeValue]
        public string HeaderText
        {
            get { return headerText; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("HeaderText", value);
                headerText = value;
            }
        }

        /// <summary>
        /// This is used to get or set additional text that should appear
        /// in the footer of every page.
        /// </summary>
        [Category("Help File"), Description("Additional text for the footer " +
          "in every page"), DefaultValue(""), EscapeValue]
        public string FooterText
        {
            get { return footerText; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("FooterText", value);
                footerText = value;
            }
        }

        /// <summary>
        /// This is used to get or set the target window for MSDN SDK links.
        /// </summary>
        /// <value>The default is <b>Blank</b> to open the MSDN topics in a
        /// new window.  This option only has an effect on the
        /// <see cref="HtmlSdkLinkType"/>, <see cref="MSHelp2SdkLinkType"/>,
        /// <see cref="MSHelpViewerSdkLinkType"/>, and <see cref="WebsiteSdkLinkType"/>
        /// properties if they are set to <b>MSDN</b>.</value>
        [Category("Help File"), Description("Specify where MSDN link targets " +
          "will be opened in the browser"), DefaultValue(SdkLinkTarget.Blank)]
        public SdkLinkTarget SdkLinkTarget
        {
            get { return sdkLinkTarget; }
            set
            {
                this.SetProjectProperty("SdkLinkTarget", value);
                sdkLinkTarget = value;
            }
        }

        /// <summary>
        /// This is used to get or set the presentation style for the help topic pages.
        /// </summary>
        /// <value>The default is to use the VS2013 style.</value>
        [Category("Help File"), Description("Select which presentation style to use for the generated " +
          "help topic pages"), EscapeValue, RefreshProperties(RefreshProperties.Repaint)]
        public string PresentationStyle
        {
            get { return presentationStyle; }
            set
            {
                // Let bad values through.  The property pages or the build engine will catch bad values if
                // necessary.
                if(String.IsNullOrWhiteSpace(value))
                    value = Constants.DefaultPresentationStyle;

                this.SetProjectProperty("PresentationStyle", value);
                presentationStyle = value;
            }
        }

        /// <summary>
        /// This is used to get or set the naming method used to generate the help topic filenames
        /// </summary>
        /// <value>The default is to use GUID values as the filenames</value>
        [Category("Help File"), Description("Specify the naming method to use for the help topic filenames"),
          DefaultValue(NamingMethod.Guid)]
        public NamingMethod NamingMethod
        {
            get { return namingMethod; }
            set
            {
                this.SetProjectProperty("NamingMethod", value);
                namingMethod = value;
            }
        }

        /// <summary>
        /// This is used to get or set the language filters which determines which languages appear in the
        /// <b>Syntax</b> section of the help topics.
        /// </summary>
        /// <value>The default is <b>Standard</b> (C#, VB.NET, and C++).</value>
        [Category("Help File"), Description("Select which languages will appear in the Syntax section of each " +
          "help topic.  Select values from the dropdown or enter a comma-separated list of values."),
          DefaultValue("Standard")]
        public string SyntaxFilters
        {
            get { return syntaxFilters; }
            set
            {
                this.SetProjectProperty("SyntaxFilters", value);
                syntaxFilters = value;
            }
        }
        #endregion

        #region HTML Help 1 properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to the Sandcastle components
        /// </summary>
        /// <remarks>This can significantly reduce the amount of time
        /// required to load a very large help document.</remarks>
        [Category("HTML Help 1"), Description("Create a binary table of " +
            "contents file.  This can significantly reduce the amount " +
            "of time required to load a very large help document."),
          DefaultValue(true)]
        public bool BinaryTOC
        {
            get { return binaryTOC; }
            set
            {
                this.SetProjectProperty("BinaryTOC", value);
                binaryTOC = value;
            }
        }

        /// <summary>
        /// This is used to get or set the type of links used to reference
        /// other help topics referring to framework (SDK) help topics in
        /// HTML Help 1 help files.
        /// </summary>
        /// <value>The default is to produce links to online MSDN content.</value>
        [Category("HTML Help 1"), Description("Specify which type of links to " +
          "create when referencing other help topics related to framework " +
          "(SDK) topics.  None = No links, MSDN = Online links to MSDN help topics."),
          DefaultValue(HtmlSdkLinkType.Msdn)]
        public HtmlSdkLinkType HtmlSdkLinkType
        {
            get { return htmlSdkLinkType; }
            set
            {
                this.SetProjectProperty("HtmlSdkLinkType", value);
                htmlSdkLinkType = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not a Favorites tab will
        /// appear in the help file.
        /// </summary>
        [Category("HTML Help 1"), Description("Set to true to include a " +
            "Favorites tab in the compiled help file"), DefaultValue(false)]
        public bool IncludeFavorites
        {
            get { return includeFavorites; }
            set
            {
                this.SetProjectProperty("IncludeFavorites", value);
                includeFavorites = value;
            }
        }
        #endregion

        #region MS Help 2 properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the collection table of contents style
        /// used when plugged into an MS Help 2 collection.
        /// </summary>
        /// <remarks>The default is <b>Hierarchical</b>.</remarks>
        [Category("MS Help 2"), Description("This defines the collection " +
          "table of contents style used when plugged into an MS Help 2 " +
          "collection"), DefaultValue(CollectionTocStyle.Hierarchical)]
        public CollectionTocStyle CollectionTocStyle
        {
            get { return collectionTocStyle; }
            set
            {
                this.SetProjectProperty("CollectionTocStyle", value);
                collectionTocStyle = value;
            }
        }

        /// <summary>
        /// This is used to get or set the type of links used to reference
        /// other help topics referring to framework (SDK) help topics in
        /// MS Help 2 help files.
        /// </summary>
        /// <value>The default is to produce links to online MSDN content.</value>
        [Category("MS Help 2"), Description("Specify which type of links to " +
          "create when referencing other help topics related to framework " +
          "(SDK) topics.  None = No links, Index = Local Index links, MSDN = Online links to MSDN help topics."),
          DefaultValue(MSHelp2SdkLinkType.Msdn)]
        public MSHelp2SdkLinkType MSHelp2SdkLinkType
        {
            get { return help2SdkLinkType; }
            set
            {
                this.SetProjectProperty("MSHelp2SdkLinkType", value);
                help2SdkLinkType = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not to include the stop word
        /// list used to identify words to omit from the Help 2 full text
        /// search index.
        /// </summary>
        [Category("MS Help 2"), Description("Indicate whether or not to " +
          "include the stop word list used to identify words to omit from " +
          "the Help 2 full text search index"), DefaultValue(true)]
        public bool IncludeStopWordList
        {
            get { return includeStopWordList; }
            set
            {
                this.SetProjectProperty("IncludeStopWordList", value);
                includeStopWordList = value;
            }
        }

        /// <summary>
        /// This is used to get or set a comma-separated list of namespaces
        /// that the collection will be plugged into when deployed using
        /// <b>H2Reg.exe</b>.
        /// </summary>
        [Category("MS Help 2"), Description("Specify a comma-separated " +
          "list of namespaces that the collection will be plugged into " +
          "when deployed using H2Reg.exe."),
          DefaultValue("ms.vsipcc+, ms.vsexpresscc+"), EscapeValue]
        public string PlugInNamespaces
        {
            get { return plugInNamespaces; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "ms.vsipcc+, ms.vsexpresscc+";
                else
                    value = value.Trim();

                this.SetProjectProperty("PlugInNamespaces", value);
                plugInNamespaces = value;
            }
        }

        /// <summary>
        /// This is used to get or set the version number applied to the
        /// help file.
        /// </summary>
        /// <remarks>The default is 1.0.0.0</remarks>
        [Category("MS Help 2"), Description("Specify the version number " +
          "that should be applied to the help file (#.#.#.#)"),
          DefaultValue("1.0.0.0"), EscapeValue]
        public string HelpFileVersion
        {
            get { return helpFileVersion; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "1.0.0.0";
                else
                    value = value.Trim();

                this.SetProjectProperty("HelpFileVersion", value);
                helpFileVersion = value;
            }
        }

        /// <summary>
        /// This is used to get a collection of additional Help 2 attributes
        /// that will be added to each generated help topic.
        /// </summary>
        /// <remarks>The attributes are added by a custom build component in
        /// the BuildAssembler step.</remarks>
        [Category("MS Help 2"), Description("Additional help attributes " +
          "to add to each generated help topic")]
        public MSHelpAttrCollection HelpAttributes
        {
            get { return helpAttributes; }
        }
        #endregion

        #region MS Help Viewer properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the type of links used to reference other help topics referring to
        /// framework (SDK) help topics in MS Help Viewer help files.
        /// </summary>
        /// <value>The default is to produce links to online MSDN content.</value>
        [Category("MS Help Viewer"), Description("Specify which type of links to create when referencing other " +
          "help topics related to framework (SDK) topics.  None = No links, Id = Local Id links, " +
          "MSDN = Online links to MSDN help topics."), DefaultValue(MSHelpViewerSdkLinkType.Msdn)]
        public MSHelpViewerSdkLinkType MSHelpViewerSdkLinkType
        {
            get { return helpViewerSdkLinkType; }
            set
            {
                this.SetProjectProperty("MSHelpViewerSdkLinkType", value);
                helpViewerSdkLinkType = value;
            }
        }

        /// <summary>
        /// This is used to get or set the Product ID portion of the MS Help Viewer 1.0 Catalog ID.
        /// </summary>
        /// <remarks>If not specified, the default is "VS".
        /// <para>The MS Help Viewer Catalog 1.0 ID is composed of the <c>CatalogProductId</c> the
        /// <see cref="CatalogVersion"/>, and the <see cref="Language"/> code. For example, the English Visual
        /// Studio 10 catalog is <c>VS_100_EN-US</c>.</para>
        /// <note type="note">For styles other than VS2010, you should used the default value.</note>
        /// </remarks>
        [Category("MS Help Viewer"), Description("Specify the Product ID portion of the MS Help Viewer 1.0 " +
          "Catalog ID.  If not set, the default is \"VS\" (for \"Visual Studio\")."), DefaultValue("VS"),
          EscapeValue]
        public string CatalogProductId
        {
            get { return catalogProductId; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "VS";
                else
                    value = Uri.EscapeDataString(Uri.UnescapeDataString(value.Trim()));

                this.SetProjectProperty("CatalogProductId", value);
                catalogProductId = value;
            }
        }

        /// <summary>
        /// This is used to get or set the Version portion of the MS Help Viewer 1.0 Catalog ID.
        /// </summary>
        /// <remarks>If not specified, the default is "100".
        /// <para>The MS Help Viewer 1.0 Catalog ID is composed of the <see cref="CatalogProductId"/>, the
        /// <c>CatalogVersion</c>, and the <see cref="Language"/> code. For example, the English Visual Studio 10
        /// catalog is <c>VS_100_EN-US</c>.</para>
        /// <note type="note">For styles other than VS2010, you should used the default value.</note>
        /// </remarks>
        [Category("MS Help Viewer"), Description("Specify the Version portion of the MS Help Viewer 1.0 " +
          "Catalog ID.  If not set, the default is \"100\" (meaning \"10.0\")."), DefaultValue("100"),
          EscapeValue]
        public string CatalogVersion
        {
            get { return catalogVersion; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "100";
                else
                    value = value.Trim();

                this.SetProjectProperty("CatalogVersion", value);
                catalogVersion = value;
            }
        }

        /// <summary>
        /// This is used to get or set the MS Help Viewer 2.x content catalog name.
        /// </summary>
        /// <remarks>If not specified, the default is "VisualStudio11" for Visual Studio 2012.  If using Visual
        /// Studio 2013, change it to "VisualStudio12".</remarks>
        [Category("MS Help Viewer"), Description("Specify the MS Help Viewer 2.x content catalog name.  For " +
          "Visual Studio 2012 use \"VisualStudio11\".  For Visual Studio 2013 use \"VisualStudio12\"."),
          DefaultValue("VisualStudio11"), EscapeValue]
        public string CatalogName
        {
            get { return catalogName; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "VisualStudio11";
                else
                    value = Uri.EscapeDataString(Uri.UnescapeDataString(value.Trim()));

                this.SetProjectProperty("CatalogName", value);
                catalogName = value;
            }
        }

        /// <summary>
        /// This is used to get or set the vendor name for the help viewer file
        /// </summary>
        /// <remarks>The default if not specified will be "Vendor Name".  The value must not contain the ':',
        /// '\', '/', '.', ',', '#', or '&amp;' characters.</remarks>
        [Category("MS Help Viewer"), Description("Specify the vendor name for the help file.  If not set, " +
          "'Vendor Name' will be used at build time."), DefaultValue(""), EscapeValue]
        public string VendorName
        {
            get { return vendorName; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                {
                    // Replace any problem characters in their normal and/or encoded forms
                    value = reBadVendorNameChars.Replace(Uri.UnescapeDataString(value.Trim()), String.Empty);
                }

                this.SetProjectProperty("VendorName", value);
                vendorName = value;
            }
        }

        /// <summary>
        /// This is used to get or set the product title for the help viewer file
        /// </summary>
        /// <remarks>The default if not specified will be the value of the <see cref="HelpTitle" />
        /// property.</remarks>
        [Category("MS Help Viewer"), Description("Specify the product title for the help file.  If not set, " +
          "the value of the HelpTitle property will be used at build time."), DefaultValue(""), EscapeValue]
        public string ProductTitle
        {
            get { return productTitle; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = String.Empty;
                else
                    value = value.Trim();

                this.SetProjectProperty("ProductTitle", value);
                productTitle = value;
            }
        }

        /// <summary>
        /// This is used to get or set the topic version for each topic in the help file
        /// </summary>
        /// <remarks>The default is "100" (meaning 10.0).</remarks>
        [Category("MS Help Viewer"), Description("Specify the topic version for each topic in the help file.  " +
          "If not set, the default is '100' (meaning 10.0)"), DefaultValue("100"), EscapeValue]
        public string TopicVersion
        {
            get { return topicVersion; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "100";
                else
                    value = value.Trim();

                this.SetProjectProperty("TopicVersion", value);
                topicVersion = value;
            }
        }

        /// <summary>
        /// This is used to get or set the table of contents parent for each root
        /// topic in the help file.
        /// </summary>
        /// <remarks>The default is "-1" to show the root topics in the root of the main table of
        /// content.</remarks>
        [Category("MS Help Viewer"), Description("Specify the table of content parent topic ID.  " +
          "Use -1 to place root elements in the root of the main table of content."), DefaultValue("-1"),
          EscapeValue]
        public string TocParentId
        {
            get { return tocParentId; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "-1";
                else
                    value = value.Trim();

                this.SetProjectProperty("TocParentId", value);
                tocParentId = value;
            }
        }

        /// <summary>
        /// This is used to get or set the topic version of the <see cref="TocParentId" /> topic
        /// </summary>
        /// <remarks>The default is "100".</remarks>
        [Category("MS Help Viewer"), Description("Specify the topic version of the TOC parent topic.  " +
          "If not set, the default is '100' (meaning 10.0)"), DefaultValue("100"), EscapeValue]
        public string TocParentVersion
        {
            get { return tocParentVersion; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = "100";
                else
                    value = value.Trim();

                this.SetProjectProperty("TocParentVersion", value);
                tocParentVersion = value;
            }
        }

        /// <summary>
        /// This is used to get or set the sort order for conceptual content so that it appears within its
        /// parent in the correct position.
        /// </summary>
        /// <remarks>The default is -1 to let the build engine determine the best value to use based on the
        /// other project properties.</remarks>
        [Category("MS Help Viewer"), Description("Specify the sort order to use when adding conceptual " +
          "topics to the table of contents.  Leave this set to -1 to let the build engine determine " +
          "the sort order based on the other project settings."), DefaultValue(-1)]
        public int TocOrder
        {
            get { return tocOrder; }
            set
            {
                if(value < -1)
                    value = -1;

                this.SetProjectProperty("TocOrder", value);
                tocOrder = value;
            }
        }
        #endregion

        #region Website properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the type of links used to reference
        /// other help topics referring to framework (SDK) help topics in
        /// HTML Help 1 help files.
        /// </summary>
        /// <value>The default is to produce links to online MSDN content.</value>
        [Category("Website"), Description("Specify which type of links to " +
          "create when referencing other help topics related to framework " +
          "(SDK) topics.  None = No links, MSDN = Online links to MSDN help topics."),
          DefaultValue(HtmlSdkLinkType.Msdn)]
        public HtmlSdkLinkType WebsiteSdkLinkType
        {
            get { return websiteSdkLinkType; }
            set
            {
                this.SetProjectProperty("WebsiteSdkLinkType", value);
                websiteSdkLinkType = value;
            }
        }
        #endregion

        #region Show Missing Tags properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set whether or not missing namespace
        /// comments are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that the &lt;summary&gt; tag is missing.  A message
        /// is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to namespace help topics if the namespace " +
          "comments are missing.  If set to false, it is not."),
          DefaultValue(true), XmlIgnore]
        public bool ShowMissingNamespaces
        {
            get { return ((missingTags & MissingTags.Namespace) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.Namespace;
                else
                    this.MissingTags &= ~MissingTags.Namespace;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;summary&gt;
        /// tags are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that the &lt;summary&gt; tag is missing.  A message
        /// is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if the <summary> tag is " +
          "missing.  If set to false, it is not."), DefaultValue(true),
          XmlIgnore]
        public bool ShowMissingSummaries
        {
            get { return ((missingTags & MissingTags.Summary) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.Summary;
                else
                    this.MissingTags &= ~MissingTags.Summary;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;param&gt;
        /// tags are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that a &lt;param&gt; tag is missing.  A message
        /// is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if a <param> tag is " +
          "missing.  If set to false, it is not."), DefaultValue(true),
          XmlIgnore]
        public bool ShowMissingParams
        {
            get { return ((missingTags & MissingTags.Parameter) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.Parameter;
                else
                    this.MissingTags &= ~MissingTags.Parameter;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;typeparam&gt;
        /// tags on generic types and methods are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that a &lt;typeparam&gt; tag is missing from a generic
        /// type or method.  A message is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if a <typeparam> tag is " +
          "missing on a generic type or method.  If set to false, it is not."),
          DefaultValue(true), XmlIgnore]
        public bool ShowMissingTypeParams
        {
            get { return ((missingTags & MissingTags.TypeParameter) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.TypeParameter;
                else
                    this.MissingTags &= ~MissingTags.TypeParameter;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;returns&gt;
        /// tags are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that the &lt;returns&gt; tag is missing.  A message
        /// is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if the <returns> tag is " +
          "missing.  If set to false, it is not."), DefaultValue(true),
          XmlIgnore]
        public bool ShowMissingReturns
        {
            get { return ((missingTags & MissingTags.Returns) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.Returns;
                else
                    this.MissingTags &= ~MissingTags.Returns;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;value&gt;
        /// tags are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that the &lt;value&gt; tag is missing.  A message
        /// is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if the <value> tag is " +
          "missing.  If set to false, it is not."), DefaultValue(false),
          XmlIgnore]
        public bool ShowMissingValues
        {
            get { return ((missingTags & MissingTags.Value) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.Value;
                else
                    this.MissingTags &= ~MissingTags.Value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;remarks&gt;
        /// tags are indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that the &lt;remarks&gt; tag is missing.  A message
        /// is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if the <remarks> tag is " +
          "missing.  If set to false, it is not."), DefaultValue(false),
          XmlIgnore]
        public bool ShowMissingRemarks
        {
            get { return ((missingTags & MissingTags.Remarks) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.Remarks;
                else
                    this.MissingTags &= ~MissingTags.Remarks;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not constructors are
        /// automatically documented if they are missing the &lt;summary&gt;
        /// tag and for classes with compiler generated constructors.
        /// </summary>
        /// <value>Set this to true to automatically add default text for the
        /// &lt;summary&gt; tag on constructors that are missing it and for
        /// classes with a compiler generated constructor.  If set to false
        /// and <see cref="ShowMissingSummaries"/> is true, a "missing summary"
        /// warning will appear instead.  A message is also written to the log
        /// file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "default message is added to constructors that are missing their " +
          "<summary> tag.  If set to false, it is not."), DefaultValue(true),
          XmlIgnore]
        public bool AutoDocumentConstructors
        {
            get { return ((missingTags & MissingTags.AutoDocumentCtors) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.AutoDocumentCtors;
                else
                    this.MissingTags &= ~MissingTags.AutoDocumentCtors;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not dispose methods are
        /// automatically documented if they are missing the &lt;summary&gt;
        /// tag and for classes with compiler generated dispose methods.
        /// </summary>
        /// <value>Set this to true to automatically add default text for the
        /// &lt;summary&gt; tag on dispose methods that are missing it and for
        /// classes with compiler generated dispose methods.  If set to false
        /// and <see cref="ShowMissingSummaries"/> is true, a "missing summary"
        /// warning will appear instead.  A message is also written to the log
        /// file.  If a <c>Dispose(Boolean)</c> method is present, its parameter
        /// will also be auto-documented if necessary.  If set to false and
        /// <see cref="ShowMissingParams" /> is true, a "missing parameter"
        /// message will appear instead.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "default message is added to dispose methods that are missing their " +
          "<summary> tag.  If set to false, it is not."), DefaultValue(true),
          XmlIgnore]
        public bool AutoDocumentDisposeMethods
        {
            get { return ((missingTags & MissingTags.AutoDocumentDispose) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.AutoDocumentDispose;
                else
                    this.MissingTags &= ~MissingTags.AutoDocumentDispose;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not missing &lt;include&gt;
        /// tag target documentation is indicated in the help file.
        /// </summary>
        /// <value>Set this to true to add a message to the help topic
        /// to indicate that the &lt;include&gt; tag's target documentation is
        /// missing.  A message is also written to the log file.</value>
        [Category("Show Missing Tags"), Description("If set to true, a " +
          "message is added to the help topic if the target documentation " +
          "for an <include> tag was not found.  If set to false, it is not.  " +
          "This option only has effect with C# generated XML comments files."),
          DefaultValue(false), XmlIgnore]
        public bool ShowMissingIncludeTargets
        {
            get { return ((missingTags & MissingTags.IncludeTargets) != 0); }
            set
            {
                if(value)
                    this.MissingTags |= MissingTags.IncludeTargets;
                else
                    this.MissingTags &= ~MissingTags.IncludeTargets;
            }
        }
        #endregion

        #region Visibility properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set whether or not attributes on types and members are documented in the
        /// syntax portion of the help file.
        /// </summary>
        /// <value>Set to true to document attributes or false to hide them</value>
        [Category("Visibility"), Description("If set to true, attributes on types and members are documented " +
          "in the syntax portion of the help pages.  If set to false, they are not."), DefaultValue(false),
          XmlIgnore]
        public bool DocumentAttributes
        {
            get { return ((visibleItems & VisibleItems.Attributes) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.Attributes;
                else
                    this.VisibleItems &= ~VisibleItems.Attributes;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not explicit interface implementations are documented
        /// </summary>
        /// <value>Set to true to document explicit interface implementations or false to hide them</value>
        [Category("Visibility"), Description("If set to true, explicit interface implementations are " +
          "documented.  If set to false, they are not."), DefaultValue(false), XmlIgnore]
        public bool DocumentExplicitInterfaceImplementations
        {
            get { return ((visibleItems & VisibleItems.ExplicitInterfaceImplementations) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.ExplicitInterfaceImplementations;
                else
                    this.VisibleItems &= ~VisibleItems.ExplicitInterfaceImplementations;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited members are documented
        /// </summary>
        /// <value>Set to true to document inherited members or false to hide them</value>
        [Category("Visibility"), Description("If set to true, inherited members are documented.  If set to " +
          "false, they are not."), DefaultValue(true), RefreshProperties(RefreshProperties.All), XmlIgnore]
        public bool DocumentInheritedMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedMembers) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.InheritedMembers;
                else
                    this.VisibleItems &= ~(VisibleItems.InheritedMembers |
                        VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkInternalMembers |
                        VisibleItems.InheritedFrameworkPrivateMembers);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited framework members are documented
        /// </summary>
        /// <value>Set to true to document inherited framework members or false to hide them.  For this to work,
        /// <see cref="DocumentInheritedMembers"/> must also be enabled.</value>
        [Category("Visibility"), Description("If set to true, inherited framework members are documented.  If " +
          "set to false, they are not.  NOTE: To work, DocumentInheritedMembers must also be set to True."),
          DefaultValue(true), XmlIgnore, RefreshProperties(RefreshProperties.All)]
        public bool DocumentInheritedFrameworkMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedFrameworkMembers) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= (VisibleItems.InheritedMembers |
                        VisibleItems.InheritedFrameworkMembers);
                else
                    this.VisibleItems &= ~(VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkInternalMembers |
                        VisibleItems.InheritedFrameworkPrivateMembers);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited internal framework members are documented
        /// </summary>
        /// <value>Set to true to document inherited internal framework members or false to hide them.  For this
        /// to work, <see cref="DocumentInheritedFrameworkMembers"/> and <see cref="DocumentInternals"/> must
        /// also be enabled.</value>
        [Category("Visibility"), Description("If set to true, inherited internal framework members are " +
          "documented.  If set to false, they are not.  NOTE: To work, DocumentInheritedFrameworkMembers and " +
          "DocumentInternals also be set to True."), DefaultValue(false), XmlIgnore,
          RefreshProperties(RefreshProperties.All)]
        public bool DocumentInheritedFrameworkInternalMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedFrameworkInternalMembers) != 0); }
            set
            {
                if(value)
                {
                    this.VisibleItems |= (VisibleItems.InheritedMembers |
                        VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkInternalMembers);
                    this.DocumentInternals = true;
                }
                else
                    this.VisibleItems &= ~VisibleItems.InheritedFrameworkInternalMembers;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited private framework members are documented
        /// </summary>
        /// <value>Set to true to document inherited private framework members or false to hide them.  For this
        /// to work, <see cref="DocumentInheritedFrameworkMembers"/> and <see cref="DocumentPrivates"/> must also
        /// be enabled.</value>
        [Category("Visibility"), Description("If set to true, inherited private framework members are " +
          "documented.  If set to false, they are not.  NOTE: To work, DocumentInheritedFrameworkMembers and " +
          "DocumentPrivates also be set to True."), DefaultValue(false), XmlIgnore,
          RefreshProperties(RefreshProperties.All)]
        public bool DocumentInheritedFrameworkPrivateMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedFrameworkPrivateMembers) != 0); }
            set
            {
                if(value)
                {
                    this.VisibleItems |= (VisibleItems.InheritedMembers |
                        VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkPrivateMembers);
                    this.DocumentPrivates = true;
                }
                else
                    this.VisibleItems &= ~VisibleItems.InheritedFrameworkPrivateMembers;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not internal members are documented in the help file
        /// </summary>
        /// <value>Set to true to document internal members or false to hide them</value>
        [Category("Visibility"), Description("If set to true, internal members are documented in the help " +
          "file.  If set to false, they are not."), DefaultValue(false), XmlIgnore,
          RefreshProperties(RefreshProperties.All)]
        public bool DocumentInternals
        {
            get { return ((visibleItems & VisibleItems.Internals) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.Internals;
                else
                {
                    this.VisibleItems &= ~VisibleItems.Internals;
                    this.DocumentInheritedFrameworkInternalMembers = false;
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not private members are documented in the help file
        /// </summary>
        /// <value>Set to true to document private members or false to hide them</value>
        [Category("Visibility"), Description("If set to true, private members are documented in the help " +
          "file.  If set to false, they are not."), DefaultValue(false), XmlIgnore,
          RefreshProperties(RefreshProperties.All)]
        public bool DocumentPrivates
        {
            get { return ((visibleItems & VisibleItems.Privates) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.Privates;
                else
                {
                    this.VisibleItems &= ~(VisibleItems.Privates | VisibleItems.PrivateFields);
                    this.DocumentInheritedFrameworkPrivateMembers = false;
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not private fields are documented in the help file
        /// </summary>
        /// <value>Set to true to document private fields or false to hide them.  For this to work,
        /// <see cref="DocumentPrivates"/> must also be enabled.</value>
        /// <remarks>Private fields are most often used to back properties and do not have documentation.  With
        /// this set to false, they are omitted from the help file to reduce unnecessary clutter.</remarks>
        [Category("Visibility"), Description("If set to true, private fields are documented in the help file.  " +
          "If set to false, they are not.  NOTE: To work, DocumentPrivates must also be set to True."),
          DefaultValue(false), XmlIgnore]
        public bool DocumentPrivateFields
        {
            get { return ((visibleItems & VisibleItems.PrivateFields) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= (VisibleItems.Privates | VisibleItems.PrivateFields);
                else
                    this.VisibleItems &= ~VisibleItems.PrivateFields;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not protected members are documented in the help file
        /// </summary>
        /// <value>Set to true to document protected members or false to hide them</value>
        [Category("Visibility"), Description("If set to true, protected members are documented in the help " +
          "file.  If set to false, they are not."), DefaultValue(true), XmlIgnore,
          RefreshProperties(RefreshProperties.All)]
        public bool DocumentProtected
        {
            get { return ((visibleItems & VisibleItems.Protected) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.Protected;
                else
                    this.VisibleItems &= ~(VisibleItems.Protected | VisibleItems.SealedProtected);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not protected members of sealed classes are documented in the
        /// help file.
        /// </summary>
        /// <value>Set to true to document protected members of sealed classes or false to hide them. For this to
        /// work, <see cref="DocumentProtected"/> must also be enabled.</value>
        [Category("Visibility"), Description("If set to true, protected members of sealed classes are " +
          "documented in the help file.  If set to false, they are not.  NOTE: To work, DocumentProtected " +
          "must also be enabled."), RefreshProperties(RefreshProperties.All), DefaultValue(false), XmlIgnore]
        public bool DocumentSealedProtected
        {
            get { return ((visibleItems & VisibleItems.SealedProtected) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= (VisibleItems.SealedProtected | VisibleItems.Protected);
                else
                    this.VisibleItems &= ~VisibleItems.SealedProtected;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not "protected internal" members are documented as "protected"
        /// only in the help file.
        /// </summary>
        /// <value>Set to true to document "protected internal" members as "protected" only or false to document
        /// them normally.  This option is ignored if <see cref="DocumentProtected"/> is false.</value>
        [Category("Visibility"), Description("If set to true, \"protected internal\" members are documented as " +
          "\"protected\" only.  If set to false, they documented normally.  NOTE: This option is ignored if " +
          "DocumentProtected is false"), DefaultValue(true), XmlIgnore]
        public bool DocumentProtectedInternalAsProtected
        {
            get { return ((visibleItems & VisibleItems.ProtectedInternalAsProtected) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.ProtectedInternalAsProtected;
                else
                    this.VisibleItems &= ~VisibleItems.ProtectedInternalAsProtected;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not no-PIA (Primary Interop Assembly) embedded interop types
        /// are documented in the help file.
        /// </summary>
        /// <value>Set to true to document no-PIA COM types or false to hide them</value>
        [Category("Visibility"), Description("If set to true, no-PIA (Primary Interop Assembly) embedded " +
          "interop types are documented in the help file.  If set to false, they are not."), DefaultValue(true),
          XmlIgnore]
        public bool DocumentNoPIATypes
        {
            get { return ((visibleItems & VisibleItems.NoPIATypes) != 0); }
            set
            {
                if(value)
                    this.VisibleItems |= VisibleItems.NoPIATypes;
                else
                    this.VisibleItems &= ~VisibleItems.NoPIATypes;
            }
        }

        /// <summary>
        /// This is used to get the API filter collection.
        /// </summary>
        [Category("Visibility"), Description("The API filter used to remove " +
          "unwanted elements from the help file.")]
        public ApiFilterCollection ApiFilter
        {
            get { return apiFilter; }
        }
        #endregion

        #region IBasePathProvider Members
        //=====================================================================

        /// <inheritdoc />
        [Browsable(false), XmlIgnore]
        public string BasePath
        {
            get
            {
                return Path.GetDirectoryName(MSBuildProject.FullPath);
            }
        }

        /// <summary>
        /// This method resolves any MSBuild environment variables in the
        /// path objects.
        /// </summary>
        /// <param name="path">The path to use</param>
        /// <returns>A copy of the path after performing any custom resolutions</returns>
        public string ResolvePath(string path)
        {
            return FilePath.reMSBuildVar.Replace(path, buildVarMatchEval);
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when a property is about to be changed to see
        /// if the project file can be edited.
        /// </summary>
        /// <remarks>If the project file cannot be edited, the handler should
        /// cancel the event so that the property is not changed.</remarks>
        public event EventHandler<CancelEventArgs> QueryEditProjectFile;

        /// <summary>
        /// This raises the <see cref="QueryEditProjectFile"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected internal void OnQueryEditProjectFile(CancelEventArgs e)
        {
            var handler = QueryEditProjectFile;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when a property is changed
        /// </summary>
        public event EventHandler<ProjectPropertyChangedEventArgs> ProjectPropertyChanged;

        /// <summary>
        /// This raises the <see cref="ProjectPropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        /// <remarks>This will also mark the project as dirty</remarks>
        protected void OnProjectPropertyChanged(ProjectPropertyChangedEventArgs e)
        {
            var handler = ProjectPropertyChanged;

            if(handler != null)
                handler(this, e);

            this.MarkAsDirty();
        }

        /// <summary>
        /// This event is raised when the dirty property changes
        /// </summary>
        public event EventHandler DirtyChanged;

        /// <summary>
        /// This raises the <see cref="DirtyChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected void OnDirtyChanged(EventArgs e)
        {
            var handler = DirtyChanged;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when the assembly list is modified
        /// </summary>
        public event EventHandler DocumentationSourcesChanged;

        /// <summary>
        /// This raises the <see cref="DocumentationSourcesChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected void OnDocumentationSourcesChanged(EventArgs e)
        {
            var handler = DocumentationSourcesChanged;

            if(handler != null)
                handler(this, e);
        }
        #endregion

        #region Designer methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the <see cref="NamespaceSummaries"/> property should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a collection and we don't want to lose all
        /// items accidentally.</remarks>
        private bool ShouldSerializeNamespaceSummaries()
        {
            return (this.NamespaceSummaries.Count != 0);
        }

        /// <summary>
        /// This is used to see if the <see cref="ComponentConfigurations"/> property should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a dictionary and we don't want to lose all
        /// items accidentally.</remarks>
        private bool ShouldSerializeComponentConfigurations()
        {
            return (this.ComponentConfigurations.Count != 0);
        }

        /// <summary>
        /// This is used to see if the <see cref="PlugInConfigurations"/> property should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a dictionary and we don't want to lose all
        /// items accidentally.</remarks>
        private bool ShouldSerializePlugInConfigurations()
        {
            return (this.PlugInConfigurations.Count != 0);
        }

        /// <summary>
        /// This is used to see if the <see cref="ApiFilter"/> property should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a collection and we don't want to lose all
        /// items accidentally.</remarks>
        private bool ShouldSerializeApiFilter()
        {
            return (this.ApiFilter.Count != 0);
        }

        /// <summary>
        /// This is used to see if the <see cref="HelpAttributes"/> property should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a collection and we don't want to lose all
        /// items accidentally.</remarks>
        private bool ShouldSerializeHelpAttributes()
        {
            return (this.HelpAttributes.Count != 0);
        }
        #endregion

        #region Private class methods
        //=====================================================================

        /// <summary>
        /// This is used to initialize the local property info and property
        /// descriptor caches.
        /// </summary>
        private static Dictionary<string, PropertyInfo> InitializePropertyCache()
        {
            PropertyInfo[] propertyInfo;

            propertyCache = new Dictionary<string, PropertyInfo>();
            pdcCache = TypeDescriptor.GetProperties(typeof(SandcastleProject));

            propertyInfo = typeof(SandcastleProject).GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach(PropertyInfo property in propertyInfo)
                propertyCache.Add(property.Name, property);

            return propertyCache;
        }

        /// <summary>
        /// This is handled to mark the project as dirty when the list of
        /// documentation sources changes.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event parameters</param>
        private void docSources_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(!loadingProperties)
            {
                this.MarkAsDirty();
                this.OnDocumentationSourcesChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// This is handled to mark the project as dirty when the various
        /// collection properties are modified.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event parameters</param>
        private void ItemList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(!loadingProperties)
                this.MarkAsDirty();
        }

        /// <summary>
        /// This is used to ensure that the project is editable before a
        /// sub-property on a project path property object is changed.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void PathProperty_Changing(object sender, EventArgs e)
        {
            CancelEventArgs ce = new CancelEventArgs();
            this.OnQueryEditProjectFile(ce);

            if(ce.Cancel)
                throw new OperationCanceledException("Project cannot be edited");
        }

        /// <summary>
        /// This is used to ensure that path properties are written to the project when one of their
        /// sub-properties is edited.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void PathProperty_Changed(object sender, EventArgs e)
        {
            FilePath changedPath = sender as FilePath;
            string propName = null;

            if(changedPath == buildLogFile)
                propName = "BuildLogFile";
            else if(changedPath == hhcPath)
                propName = "HtmlHelp1xCompilerPath";
            else if(changedPath == hxcompPath)
                propName = "HtmlHelp2xCompilerPath";
            else if(changedPath == workingPath)
                propName = "WorkingPath";
            else if(changedPath == componentPath)
                propName = "ComponentPath";
            else
                throw new ArgumentException("Unknown path property changed", "sender");

            this.SetProjectProperty(propName, sender);
        }

        /// <summary>
        /// Replace a \xNN value in the copyright text with its actual character
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnCharacterMatch(Match match)
        {
            // Ignore it if it is escaped
            if(match.Index != 0 && copyrightText[match.Index - 1] == '\\')
                return match.Value;

            int value = Convert.ToInt32(match.Value.Substring(2), 16);
            char ch = (char)value;

            return new String(ch, 1);
        }

        /// <summary>
        /// Resolve references to MSBuild variables in a path value
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnBuildVarMatch(Match match)
        {
            ProjectProperty prop;

            if(!this.ProjectPropertyCache.TryGetValue(match.Groups[1].Value, out prop))
                return String.Empty;

            return prop.EvaluatedValue;
        }

        /// <summary>
        /// This is used to load the properties from the project file
        /// </summary>
        private void LoadProperties()
        {
            ProjectProperty property;
            Version schemaVersion;
            string helpFormats;
            Dictionary<string, string> translateFormat = new Dictionary<string, string> {
                { "HTMLHELP1X", "HtmlHelp1" },
                { "HTMLHELP2X", "MSHelp2" },
                { "HELP1XANDHELP2X", "HtmlHelp1, MSHelp2" },
                { "HELP1XANDWEBSITE", "HtmlHelp1, Website" },
                { "HELP2XANDWEBSITE", "MSHelp2, Website" },
                { "HELP1XAND2XANDWEBSITE", "HtmlHelp1, MSHelp2, Website" } };

            try
            {
                // Ensure that we use the correct build engine for the project.  On VS 2013,
                // msBuildProject.ToolsVersion is "12.0" but the Xml.ToolsVersion is "4.0" which is okay.
                if(msBuildProject.ToolsVersion != "4.0" && msBuildProject.Xml.ToolsVersion != "4.0")
                    msBuildProject.Xml.ToolsVersion = "4.0";

                loadingProperties = true;

                if(!this.ProjectPropertyCache.TryGetValue("SHFBSchemaVersion", out property))
                    throw new BuilderException("PRJ0001", "Invalid or missing SHFBSchemaVersion");

                if(String.IsNullOrEmpty(property.EvaluatedValue))
                    throw new BuilderException("PRJ0001", "Invalid or missing SHFBSchemaVersion");

                schemaVersion = new Version(property.EvaluatedValue);

                if(schemaVersion > SandcastleProject.SchemaVersion)
                    throw new BuilderException("PRJ0002", "The selected file is for a more recent version of " +
                        "the help file builder.  Please upgrade your copy to load the file.");

                // Note that many properties don't use the final value as they don't contain variables that
                // need replacing.
                foreach(ProjectProperty prop in this.ProjectPropertyCache.Values)
                    switch(prop.Name.ToUpperInvariant())
                    {
                        case "CONFIGURATION":   // These are ignored
                        case "PLATFORM":
                        case "TRANSFORMCOMPONENTARGUMENTS":
                            break;

                        case "APIFILTER":
                            apiFilter.FromXml(prop.UnevaluatedValue);
                            break;

                        case "COMPONENTCONFIGURATIONS":
                            componentConfigs.FromXml(prop.UnevaluatedValue);
                            break;

                        case "DOCUMENTATIONSOURCES":
                            // The paths in the elements may contain variable references so use final values if
                            // requested.
                            if(!usingFinalValues)
                                docSources.FromXml(prop.UnevaluatedValue);
                            else
                                docSources.FromXml(prop.EvaluatedValue);
                            break;

                        case "HELPATTRIBUTES":
                            helpAttributes.FromXml(prop.UnevaluatedValue);
                            break;

                        case "NAMESPACESUMMARIES":
                            namespaceSummaries.FromXml(prop.UnevaluatedValue);
                            break;

                        case "PLUGINCONFIGURATIONS":
                            plugInConfigs.FromXml(prop.UnevaluatedValue);
                            break;

                        case "HELPFILEFORMAT":
                            // The enum value names changed in v1.8.0.3
                            if(schemaVersion.Major == 1 && schemaVersion.Minor == 8 &&
                              schemaVersion.Build == 0 && schemaVersion.Revision < 3)
                            {
                                helpFormats = prop.UnevaluatedValue.ToUpper(CultureInfo.InvariantCulture);

                                foreach(string key in translateFormat.Keys)
                                    helpFormats = helpFormats.Replace(key, translateFormat[key]);

                                this.SetLocalProperty(prop.Name, helpFormats);

                                msBuildProject.SetProperty("HelpFileFormat", this.HelpFileFormat.ToString());
                            }
                            else
                                this.SetLocalProperty(prop.Name, prop.UnevaluatedValue);
                            break;

                        case "FRAMEWORKVERSION":
                            // The values changed in v1.9.2.0 to include the framework type.  They changed in
                            // v1.9.5.0 to use the titles from the Sandcastle framework definition file.
                            if(schemaVersion.Major == 1 && (schemaVersion.Minor < 9 ||
                              (schemaVersion.Minor == 9 && schemaVersion.Build < 5)))
                            {
                                this.SetLocalProperty(prop.Name, Utility.ConvertFromOldValue(prop.UnevaluatedValue));
                                msBuildProject.SetProperty("FrameworkVersion", this.FrameworkVersion);
                            }
                            else
                                this.SetLocalProperty(prop.Name, prop.UnevaluatedValue);
                            break;

                        default:
                            // These may or may not contain variable references
                            // so use the final value if requested.
                            if(!usingFinalValues)
                                this.SetLocalProperty(prop.Name, prop.UnevaluatedValue);
                            else
                                this.SetLocalProperty(prop.Name, prop.EvaluatedValue);
                            break;
                    }

                // Note: Project data stored in item groups are loaded on demand when first used (i.e.
                // references, additional and conceptual content, etc.)
            }
            catch(Exception ex)
            {
                throw new BuilderException("PRJ0003", String.Format(CultureInfo.CurrentCulture,
                    "Error reading project from '{0}':\r\n{1}", msBuildProject.FullPath, ex.Message), ex);
            }
            finally
            {
                loadingProperties = false;
                this.OnDocumentationSourcesChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// This is used to set the named property to the specified value using Reflection
        /// </summary>
        /// <param name="name">The name of the property to set</param>
        /// <param name="value">The value to which it is set</param>
        /// <returns>The parsed object value to which the property was set.</returns>
        /// <remarks>Property name matching is case insensitive as are the values.  This is used to allow setting
        /// of simple project properties (non-collection) from the MSBuild project file.  Unknown properties are
        /// ignored.</remarks>
        /// <exception cref="ArgumentNullException">This is thrown if the name parameter is null or an empty
        /// string.</exception>
        /// <exception cref="BuilderException">This is thrown if an error occurs while trying to set the named
        /// property.</exception>
        private void SetLocalProperty(string name, string value)
        {
            TypeConverter tc;
            EscapeValueAttribute escAttr;
            PropertyInfo property;
            FilePath filePath;
            object parsedValue;

            if(String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // Ignore unknown properties
            if(!propertyCache.TryGetValue(name, out property))
            {
                property = null;

                // Could be mismatched by case, so try again the long way
                foreach(string key in propertyCache.Keys)
                    if(String.Compare(key, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        name = key;
                        property = propertyCache[name];
                        break;
                    }

                if(property == null)
                    return;
            }

            if(!property.CanWrite || property.IsDefined(typeof(XmlIgnoreAttribute), true))
                throw new BuilderException("PRJ0004", String.Format(CultureInfo.CurrentCulture,
                    "An attempt was made to set a read-only or ignored property: {0}   Value: {1}",
                    name, value));

            // If escaped, unescape it
            escAttr = pdcCache[name].Attributes[typeof(EscapeValueAttribute)] as EscapeValueAttribute;

            if(escAttr != null)
                value = EscapeValueAttribute.Unescape(value);

            try
            {
                if(property.PropertyType.IsEnum)
                    parsedValue = Enum.Parse(property.PropertyType, value, true);
                else
                    if(property.PropertyType == typeof(Version))
                        parsedValue = new Version(value);
                    else
                    {
                        if(property.PropertyType == typeof(FilePath))
                            parsedValue = new FilePath(value, this);
                        else
                            if(property.PropertyType == typeof(FolderPath))
                                parsedValue = new FolderPath(value, this);
                            else
                            {
                                tc = TypeDescriptor.GetConverter(property.PropertyType);
                                parsedValue = tc.ConvertFromString(value);
                            }

                        // If it's a file or folder path, set the IsFixedPath property based on whether or not
                        // it is rooted.
                        filePath = parsedValue as FilePath;

                        if(filePath != null && Path.IsPathRooted(value))
                            filePath.IsFixedPath = true;
                    }
            }
            catch(Exception ex)
            {
                // Ignore exceptions for the Language property.  A few people have had an environment variable
                // with that name that gets picked up as a default and the value isn't typically valid for a
                // culture name.
                if(!name.Equals("Language", StringComparison.OrdinalIgnoreCase))
                    throw new BuilderException("PRJ0005", "Unable to parse value '" + value +
                        "' for property '" + name + "'", ex);

                parsedValue = null;
            }

            property.SetValue(this, parsedValue, null);
        }

        /// <summary>
        /// Set the value of an MSBuild project property that matches a
        /// property on this class by name.
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        /// <exception cref="ArgumentException">This is thrown if the property
        /// name is null, an empty string, or is not a recognized property
        /// name.</exception>
        private void SetProjectProperty(string propertyName, object propertyValue)
        {
            PropertyInfo localProp;
            DefaultValueAttribute defValue;
            EscapeValueAttribute escAttr;
            ProjectProperty projProp;
            FilePath filePath;
            string oldValue, newValue;

            // Skip it if loading properties at construction
            if(loadingProperties)
                return;

            if(String.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName",
                    "Property name cannot be null or empty");

            if(!propertyCache.TryGetValue(propertyName, out localProp))
                throw new ArgumentException("Unknown local property name: " +
                    propertyName, "propertyName");

            // Currently there are no configuration-specific project properties.
            // If that changes, the current configuration should be set before
            // calling this.  It may also be handled elsewhere and the project
            // cache will be set to null so that this refreshes it when needed.
            // See the MPF ProjectNode class for an example.

            if(!this.ProjectPropertyCache.TryGetValue(propertyName, out projProp))
                oldValue = null;
            else
                oldValue = projProp.UnevaluatedValue;

            if(propertyValue == null)
            {
                // If the property is already null, do nothing
                if(oldValue == null)
                    return;

                // Otherwise, set it to empty
                newValue = String.Empty;
            }
            else
            {
                // FilePath objects should use the PersistablePath property
                // which isn't expanded.
                filePath = propertyValue as FilePath;

                if(filePath != null)
                    newValue = filePath.PersistablePath;
                else
                    newValue = propertyValue.ToString();
            }

            // If oldValue is null, set it to the default value for
            // the property.  That way it won't get created if it
            // doesn't need to be.
            if(oldValue == null)
            {
                defValue = pdcCache[propertyName].Attributes[
                    typeof(DefaultValueAttribute)] as DefaultValueAttribute;

                if(defValue != null && defValue.Value != null)
                    oldValue = defValue.Value.ToString();
            }

            // Only do the work if this is different to what we had
            if(String.Compare(oldValue, newValue, StringComparison.Ordinal) != 0)
            {
                // See if the project can be edited.  If not, abort the change
                // by throwing an exception.
                CancelEventArgs ce = new CancelEventArgs();
                this.OnQueryEditProjectFile(ce);

                if(ce.Cancel)
                    throw new OperationCanceledException("Project cannot be edited");

                // Escape the value if necessary
                escAttr = pdcCache[propertyName].Attributes[
                    typeof(EscapeValueAttribute)] as EscapeValueAttribute;

                if(escAttr != null)
                    msBuildProject.SetProperty(propertyName, EscapeValueAttribute.Escape(newValue));
                else
                    msBuildProject.SetProperty(propertyName, newValue);

                // The cache needs to be refreshed
                projectPropertyCache = null;

                // Notify everyone of the property change
                this.OnProjectPropertyChanged(new ProjectPropertyChangedEventArgs(
                    propertyName, oldValue, newValue));
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are five overloads for the constructor</overloads>
        protected SandcastleProject()
        {
            characterMatchEval = new MatchEvaluator(this.OnCharacterMatch);
            buildVarMatchEval = new MatchEvaluator(this.OnBuildVarMatch);

            docSources = new DocumentationSourceCollection(this);
            docSources.ListChanged += docSources_ListChanged;

            namespaceSummaries = new NamespaceSummaryItemCollection(this);
            namespaceSummaries.ListChanged += ItemList_ListChanged;

            references = new ReferenceItemCollection(this);
            references.ListChanged += ItemList_ListChanged;

            componentConfigs = new ComponentConfigurationDictionary(this);
            plugInConfigs = new PlugInConfigurationDictionary(this);

            apiFilter = new ApiFilterCollection(this);
            apiFilter.ListChanged += ItemList_ListChanged;

            helpAttributes = new MSHelpAttrCollection(this);
            helpAttributes.ListChanged += ItemList_ListChanged;

            try
            {
                loadingProperties = removeProjectWhenDisposed = true;

                contentPlacement = ContentPlacement.AboveNamespaces;
                cleanIntermediates = keepLogFile = binaryTOC = includeStopWordList = true;

                this.BuildLogFile = null;

                missingTags = MissingTags.Summary | MissingTags.Parameter | MissingTags.TypeParameter |
                    MissingTags.Returns | MissingTags.AutoDocumentCtors | MissingTags.Namespace |
                    MissingTags.AutoDocumentDispose;

                visibleItems = VisibleItems.InheritedFrameworkMembers | VisibleItems.InheritedMembers |
                    VisibleItems.Protected | VisibleItems.ProtectedInternalAsProtected;

                buildAssemblerVerbosity = BuildAssemblerVerbosity.OnlyWarningsAndErrors;
                helpFileFormat = HelpFileFormats.HtmlHelp1;
                htmlSdkLinkType = websiteSdkLinkType = HtmlSdkLinkType.Msdn;
                help2SdkLinkType = MSHelp2SdkLinkType.Msdn;
                helpViewerSdkLinkType = MSHelpViewerSdkLinkType.Msdn;
                sdkLinkTarget = SdkLinkTarget.Blank;
                presentationStyle = Constants.DefaultPresentationStyle;
                namingMethod = NamingMethod.Guid;
                syntaxFilters = ComponentUtilities.DefaultSyntaxFilter;
                collectionTocStyle = CollectionTocStyle.Hierarchical;
                helpFileVersion = "1.0.0.0";
                tocOrder = -1;
                maximumGroupParts = 2;

                this.OutputPath = null;
                this.HtmlHelp1xCompilerPath = this.HtmlHelp2xCompilerPath = this.WorkingPath =
                    this.ComponentPath = null;

                this.HelpTitle = this.HtmlHelpName = this.CopyrightHref = this.CopyrightText =
                    this.FeedbackEMailAddress = this.FeedbackEMailLinkText = this.HeaderText = this.FooterText =
                    this.ProjectSummary = this.RootNamespaceTitle = this.PlugInNamespaces = this.TopicVersion =
                    this.TocParentId = this.TocParentVersion = this.CatalogProductId = this.CatalogVersion =
                    this.CatalogName = null;
                this.FrameworkVersion = null;

                language = new CultureInfo("en-US");
            }
            finally
            {
                loadingProperties = false;
            }
        }

        /// <summary>
        /// Load a Sandcastle Builder project from the given filename.
        /// </summary>
        /// <param name="filename">The filename to load</param>
        /// <param name="mustExist">Specify true if the file must exist or false if a new project should be
        /// created if the file does not exist.</param>
        /// <exception cref="ArgumentException">This is thrown if a filename is not specified or if it does not
        /// exist and <c>mustExist</c> is true.</exception>
        public SandcastleProject(string filename, bool mustExist) : this()
        {
            string template;

            if(String.IsNullOrEmpty(filename))
                throw new ArgumentException("A filename must be specified", "filename");

            filename = Path.GetFullPath(filename);

            if(!File.Exists(filename))
            {
                if(mustExist)
                    throw new ArgumentException("The specific file must exist", "filename");

                // Create new project from template file
                template = Properties.Resources.ProjectTemplate;
                template = template.Replace("$guid1$", Guid.NewGuid().ToString("B"));
                template = template.Replace("$safeprojectname$", "Documentation");

                using(StringReader sr = new StringReader(template))
                {
                    using(XmlReader xr = XmlReader.Create(sr))
                    {
                        msBuildProject = new Project(xr);
                    }
                }

                msBuildProject.FullPath = filename;
            }
            else
            {
                // If already loaded into the global project collection, use the existing instance
                var loadedProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(filename).ToList();

                if(loadedProjects.Count != 0)
                {
                    msBuildProject = loadedProjects[0];
                    removeProjectWhenDisposed = false;
                }
                else
                    msBuildProject = new Project(filename);
            }

            this.LoadProperties();
        }

        /// <summary>
        /// This is used to create a Sandcastle Builder project from an
        /// existing MSBuild project instance.
        /// </summary>
        /// <param name="existingProject">The existing project instance</param>
        /// <remarks>It is assumed that the project has been loaded, the
        /// property values are current, and that the configuration and platform
        /// have been set in the MSBuild project global properties in order to
        /// get the correct final values.</remarks>
        public SandcastleProject(Project existingProject) : this()
        {
            // Do not remove the project from the MSBuild project collection when this is disposed of since we
            // didn't create it.
            removeProjectWhenDisposed = false;

            msBuildProject = existingProject;
            usingFinalValues = true;
            this.LoadProperties();
        }

        /// <summary>
        /// This is used to clone an existing project in order to build it without affecting the existing
        /// project's properties.
        /// </summary>
        /// <param name="cloneProject">The project to clone</param>
        /// <remarks>This is used to perform partial builds where we may want to use alternate property values.</remarks>
        public SandcastleProject(SandcastleProject cloneProject) : this()
        {
            string newName = Guid.NewGuid().ToString();

            cloneProject.EnsureProjectIsCurrent(false);
            usingFinalValues = true;

            using(StringReader sr = new StringReader(cloneProject.msBuildProject.Xml.RawXml))
            {
                using(XmlReader xr = XmlReader.Create(sr))
                {
                    msBuildProject = new Project(xr);
                }
            }

            // Use the same folder so that relative paths have the same base location.  Use a different filename
            // to prevent the cloned instance from being unloaded by the build engine.
            msBuildProject.FullPath = Path.Combine(Path.GetDirectoryName(cloneProject.Filename),
                newName + ".shfbproj");

            this.Configuration = cloneProject.Configuration;
            this.Platform = cloneProject.Platform;

            if(!String.IsNullOrEmpty(cloneProject.MSBuildOutDir))
                this.MSBuildOutDir = cloneProject.MSBuildOutDir;

            this.LoadProperties();
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the Sandcastle project if not done
        /// explicitly with <see cref="Dispose()"/>.
        /// </summary>
        ~SandcastleProject()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the Sandcastle project object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // If we loaded the MSBuild project, we must unload it.  If not, it is cached and will cause problems
            // if loaded a second time.
            if(removeProjectWhenDisposed && msBuildProject != null && !String.IsNullOrEmpty(this.Filename))
            {
                ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject);
                ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject.Xml);
            }
        }
        #endregion

        #region Methods, etc.
        //=====================================================================

        /// <summary>
        /// This is used to determine the default build action for a file based
        /// on its extension.
        /// </summary>
        /// <param name="filename">The filename to use</param>
        /// <returns>The build action based on the extension</returns>
        public static BuildAction DefaultBuildAction(string filename)
        {
            string ext = Path.GetExtension(filename).ToLowerInvariant();

            switch(ext)
            {
                case ".asp":        // Content/config
                case ".aspx":
                case ".ascx":
                case ".config":
                case ".css":
                case ".htm":
                case ".html":
                case ".js":
                case ".topic":
                case ".txt":
                case ".zip":
                    return BuildAction.Content;

                case ".bmp":        // Images for conceptual content.  The default used to be Content but
                case ".gif":        // the additional content model has been deprecated so this is now the
                case ".jpg":        // more appropriate choice.
                case ".jpeg":
                case ".png":
                    return BuildAction.Image;

                case ".content":
                    return BuildAction.ContentLayout;

                case ".items":
                    return BuildAction.ResourceItems;

                case ".sitemap":
                    return BuildAction.SiteMap;

                case ".snippets":
                    return BuildAction.CodeSnippets;

                case ".tokens":
                    return BuildAction.Tokens;

                case ".xamlcfg":
                    return BuildAction.XamlConfiguration;

                case ".xsl":
                case ".xslt":
                    return BuildAction.TopicTransform;

                default:    // Anything else defaults to None
                    break;
            }

            return BuildAction.None;
        }

        /// <summary>
        /// This is used to mark the project as dirty and in need of being
        /// saved.
        /// </summary>
        /// <event cref="DirtyChanged">This event is raised to let interested
        /// parties know that the project's dirty state has been changed.</event>
        public void MarkAsDirty()
        {
            // We cannot mark the MSBuild project as dirty as the value sticks and never gets cleared.
            // As such, we track the dirty state for ourselves.
            isDirty = true;
            this.OnDirtyChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Get a collection containing all user-defined properties
        /// </summary>
        /// <returns>A collection containing all properties determined not to
        /// be help file builder project properties, MSBuild build engine
        /// related properties, or environment variables.</returns>
        public Collection<ProjectProperty> GetUserDefinedProperties()
        {
            Collection<ProjectProperty> userProps = new Collection<ProjectProperty>();

            if(msBuildProject != null && propertyCache != null)
                foreach(ProjectProperty prop in msBuildProject.AllEvaluatedProperties)
                    if(!prop.IsEnvironmentProperty && !prop.IsGlobalProperty && !prop.IsImported &&
                      !prop.IsReservedProperty && !propertyCache.ContainsKey(prop.Name) &&
                      restrictedProps.IndexOf(prop.Name) == -1)
                        userProps.Add(prop);

            return userProps;
        }

        /// <summary>
        /// This is used to determine whether or not the given name can be
        /// used for a user-defined project property.
        /// </summary>
        /// <param name="name">The name to check</param>
        /// <returns>True if it can be used, false if it cannot be used</returns>
        public bool IsValidUserDefinedPropertyName(string name)
        {
            ProjectProperty prop;

            if(msBuildProject == null || propertyCache.ContainsKey(name) || restrictedProps.IndexOf(name) != -1)
                return false;

            if(this.ProjectPropertyCache.TryGetValue(name, out prop))
                return (!prop.IsImported && !prop.IsReservedProperty);

            return true;
        }

        /// <summary>
        /// Add a new folder build item to the project
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <returns>The new <see cref="FileItem"/>.</returns>
        /// <remarks>If the folder does not exist in the project, it is added
        /// and created if not already there.  If the folder is already part of
        /// the project, the existing item is returned.</remarks>
        /// <exception cref="ArgumentException">This is thrown if the path
        /// matches the project root path or is not below it.</exception>
        public FileItem AddFolderToProject(string folder)
        {
            FolderPath folderPath;
            FileItem newFileItem = null;
            string folderAction = BuildAction.Folder.ToString(),
                rootPath = Path.GetDirectoryName(msBuildProject.FullPath);

            if(folder.Length != 0 && folder[folder.Length - 1] == '\\')
                folder = folder.Substring(0, folder.Length - 1);

            if(!Path.IsPathRooted(folder))
                folder = Path.GetFullPath(Path.Combine(rootPath, folder));

            if(String.Compare(folder, 0, rootPath, 0, rootPath.Length,
              StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException("The folder must be below the project's root path", "folder");

            if(folder.Length == rootPath.Length)
                throw new ArgumentException("The folder cannot match the project's root path", "folder");

            folderPath = new FolderPath(folder, this);

            // Note that Visual Studio doesn't add the trailing backslash so look for a match with and without it.
            // Folders don't always have a relative path in the item when first added.  As such, check both the
            // relative and full paths for a match.
            foreach(ProjectItem item in msBuildProject.GetItems(folderAction))
                if(item.EvaluatedInclude == folderPath.PersistablePath || item.EvaluatedInclude + @"\" == folderPath.PersistablePath ||
                  item.EvaluatedInclude == folderPath.Path || item.EvaluatedInclude + @"\" == folderPath.Path)
                {
                    newFileItem = new FileItem(new ProjectElement(this, item));
                    break;
                }

            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if(newFileItem == null)
                newFileItem = new FileItem(new ProjectElement(this, folderAction, folder));

            return newFileItem;
        }

        /// <summary>
        /// Add a new file build item to the project
        /// </summary>
        /// <param name="sourceFile">The source filename</param>
        /// <param name="destFile">The optional destination path.  If empty,
        /// null, or it does not start with the project folder, the file is
        /// copied to the root folder of the project.</param>
        /// <returns>The new <see cref="FileItem" />.</returns>
        /// <remarks>If the file does not exist in the project, it is copied to
        /// the destination path or project folder if not already there.  The
        /// default build action is determined based on the filename's
        /// extension.  If the file is already part of the project, the
        /// existing item is returned.</remarks>
        public FileItem AddFileToProject(string sourceFile, string destFile)
        {
            BuildAction buildAction;
            FilePath filePath;
            FileItem newFileItem = null;
            string[] folders;
            string itemPath, rootPath = Path.GetDirectoryName(msBuildProject.FullPath);

            if(String.IsNullOrEmpty(destFile) || !destFile.StartsWith(rootPath,
              StringComparison.OrdinalIgnoreCase))
                destFile = Path.Combine(rootPath, Path.GetFileName(sourceFile));

            filePath = new FilePath(destFile, this);
            buildAction = DefaultBuildAction(destFile);

            foreach(ProjectItem item in msBuildProject.AllEvaluatedItems)
            {
                if(item.HasMetadata(ProjectElement.LinkPath))
                    itemPath = item.GetMetadataValue(ProjectElement.LinkPath);
                else
                    itemPath = item.EvaluatedInclude;

                // Files don't always have a relative path in the item when first added.  As such, check both
                // the relative and full paths for a match.
                if(itemPath == filePath.PersistablePath || itemPath == filePath.Path)
                {
                    newFileItem = new FileItem(new ProjectElement(this, item))
                    {
                        BuildAction = buildAction,
                    };
                    break;
                }
            }

            if(!File.Exists(destFile))
            {
                if(!Directory.Exists(Path.GetDirectoryName(destFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                // Make sure folder items exists for all parts of the path
                folders = Path.GetDirectoryName(filePath.PersistablePath).Split('\\');
                itemPath = String.Empty;

                foreach(string path in folders)
                    if(path.Length != 0)
                    {
                        itemPath += path + "\\";
                        this.AddFolderToProject(itemPath);
                    }

                if(File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, destFile);
                    File.SetAttributes(destFile, FileAttributes.Normal);
                }
            }

            if(newFileItem == null)
            {
                newFileItem = new FileItem(new ProjectElement(this, buildAction.ToString(), destFile));

                // For images, assign the build action again so that it sets the default alternate text and ID
                if(buildAction == BuildAction.Image)
                    newFileItem.BuildAction = buildAction;
            }

            return newFileItem;
        }

        /// <summary>
        /// This is used to locate a file by name in the project
        /// </summary>
        /// <param name="fileToFind">The fully qualified file path to find</param>
        /// <returns>The file item if found or null if not found</returns>
        public FileItem FindFile(string fileToFind)
        {
            FilePath filePath;
            FileItem fileItem = null;
            string itemPath, rootPath = Path.GetDirectoryName(msBuildProject.FullPath);

            if(String.IsNullOrEmpty(fileToFind) ||
              !fileToFind.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                return null;

            filePath = new FilePath(fileToFind, this);

            foreach(ProjectItem item in msBuildProject.AllEvaluatedItems)
            {
                if(item.HasMetadata(ProjectElement.LinkPath))
                    itemPath = item.GetMetadataValue(ProjectElement.LinkPath);
                else
                    itemPath = item.EvaluatedInclude;

                // Files don't always have a relative path in the item when first added.  As such, check both
                // the relative and full paths for a match.
                if(itemPath == filePath.PersistablePath || itemPath == filePath.Path)
                {
                    fileItem = new FileItem(new ProjectElement(this, item));
                    break;
                }
            }

            return fileItem;
        }

        /// <summary>
        /// This marks the project as dirty along with the project collections
        /// for the purpose of upgrading the project.
        /// </summary>
        /// <remarks>Most properties are converted when loaded.  This ensures that the all collections
        /// are saved and the tools version and SHFB schema version are also updated when the project
        /// is next saved.</remarks>
        public void UpgradeProjectProperties()
        {
            apiFilter.IsDirty = componentConfigs.IsDirty = docSources.IsDirty = helpAttributes.IsDirty =
                namespaceSummaries.IsDirty = plugInConfigs.IsDirty = true;
            this.MarkAsDirty();
            this.EnsureProjectIsCurrent(true);
        }

        /// <summary>
        /// This is used to ensure that all local collection project properties
        /// have been stored in the MSBuild project file and that path-type
        /// properties are current based on the current project location.
        /// </summary>
        /// <param name="forceUpdate">True to force an update of all affected
        /// properties or false to only update those that need it.</param>
        /// <remarks>This only affects the property-based collection properties
        /// and path-type properties.  Simple types and item group element
        /// properties are stored when modified.  This will also ensure that
        /// the <c>SHFBSchemaVersion</c> is set to the current version too.</remarks>
        public void EnsureProjectIsCurrent(bool forceUpdate)
        {
            ProjectProperty property;
            Version schemaVersion;

            if(apiFilter.IsDirty)
            {
                this.SetProjectProperty("ApiFilter", apiFilter.ToXml());
                apiFilter.IsDirty = false;
            }

            if(componentConfigs.IsDirty)
            {
                this.SetProjectProperty("ComponentConfigurations", componentConfigs.ToXml());
                componentConfigs.IsDirty = false;
            }

            if(docSources.IsDirty || forceUpdate)
            {
                this.SetProjectProperty("DocumentationSources", docSources.ToXml());
                docSources.IsDirty = false;
            }

            if(helpAttributes.IsDirty)
            {
                this.SetProjectProperty("HelpAttributes", helpAttributes.ToXml());
                helpAttributes.IsDirty = false;
            }

            if(namespaceSummaries.IsDirty)
            {
                this.SetProjectProperty("NamespaceSummaries", namespaceSummaries.ToXml());
                namespaceSummaries.IsDirty = false;
            }

            if(plugInConfigs.IsDirty)
            {
                this.SetProjectProperty("PlugInConfigurations", plugInConfigs.ToXml());
                plugInConfigs.IsDirty = false;
            }

            if(forceUpdate)
            {
                this.SetProjectProperty("BuildLogFile", buildLogFile);
                this.SetProjectProperty("HtmlHelp1xCompilerPath", hhcPath);
                this.SetProjectProperty("HtmlHelp2xCompilerPath", hxcompPath);
                this.SetProjectProperty("WorkingPath", workingPath);
                this.SetProjectProperty("ComponentPath", componentPath);
            }

            // Update the schema version if necessary but only if the project is dirty
            if(this.IsDirty)
            {
                property = msBuildProject.AllEvaluatedProperties.FirstOrDefault(
                    p => p.Name == "SHFBSchemaVersion");

                if(property != null && !String.IsNullOrEmpty(property.EvaluatedValue))
                {
                    schemaVersion = new Version(property.EvaluatedValue);

                    if(schemaVersion != SandcastleProject.SchemaVersion)
                        msBuildProject.SetProperty("SHFBSchemaVersion",
                            SandcastleProject.SchemaVersion.ToString(4));
                }
            }
        }

        /// <summary>
        /// This is used to save the project file
        /// </summary>
        /// <param name="filename">The filename for the project</param>
        public void SaveProject(string filename)
        {
            bool forceUpdate;

            try
            {
                filename = Path.GetFullPath(filename);
                forceUpdate = (filename != msBuildProject.FullPath);

                msBuildProject.FullPath = filename;
                this.EnsureProjectIsCurrent(forceUpdate);

                msBuildProject.Save(filename);
                isDirty = false;
                this.OnDirtyChanged(EventArgs.Empty);
            }
            catch(Exception ex)
            {
                throw new BuilderException("PRJ0006", String.Format(CultureInfo.CurrentCulture,
                    "Error saving project to '{0}':\r\n{1}", filename, ex.Message), ex);
            }
        }

        /// <summary>
        /// This returns true if the project contains items using the
        /// given build action.
        /// </summary>
        /// <param name="buildAction">The build action for which to check</param>
        /// <returns>True if at least one item has the given build action or
        /// false if there are no items with the given build action.</returns>
        public bool HasItems(BuildAction buildAction)
        {
            return (msBuildProject.GetItems(buildAction.ToString()).Count != 0);
        }

        /// <summary>
        /// This refreshes the project instance property values by reloading them
        /// from the underlying MSBuild project.
        /// </summary>
        public void RefreshProjectProperties()
        {
            this.EnsureProjectIsCurrent(false);

            try
            {
                loadingProperties = true;
                projectPropertyCache = null;

                apiFilter.Clear();
                componentConfigs.Clear();
                docSources.Clear();
                helpAttributes.Clear();
                namespaceSummaries.Clear();
                plugInConfigs.Clear();

                apiFilter.IsDirty = componentConfigs.IsDirty = docSources.IsDirty = helpAttributes.IsDirty =
                    namespaceSummaries.IsDirty = plugInConfigs.IsDirty = false;
            }
            finally
            {
                loadingProperties = false;
            }

            this.LoadProperties();
        }
        #endregion
    }
}
