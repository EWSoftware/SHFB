//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ISandcastleProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/27/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the interface used to interact with a Sandcastle Help File Builder project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/19/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.ConceptualContent;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.PresentationStyle.Transformation;

namespace Sandcastle.Core.Project
{
    /// <summary>
    /// This defines the interface used to interact with a Sandcastle Help File Builder project
    /// </summary>
    public interface ISandcastleProject : IBasePathProvider, IContentFileProvider, IDisposable
    {
        #region Miscellaneous properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get whether or not the project is using final values for the
        /// project properties.
        /// </summary>
        /// <value>If true, final values (i.e. evaluated values used at build time) are being returned by the
        /// properties in this instance.</value>
        bool UsingFinalValues { get; }

        /// <summary>
        /// This read-only property is used to get the filename for the project
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// This is used to get or set the configuration to use when building the project
        /// </summary>
        /// <value>This value is used for project documentation sources and project references so that the
        /// correct items are used from them.</value>
        string Configuration { get; set; }

        /// <summary>
        /// This is used to get or set the platform to use when building the project
        /// </summary>
        /// <value>This value is used for project documentation sources and project references so that the
        /// correct items are used from them.</value>
        string Platform { get; set; }

        /// <summary>
        /// This is used to get or set the MSBuild <c>OutDir</c> property value that is defined when using Team
        /// Build.  This value comes from the global properties rather than the project properties.
        /// </summary>
        /// <value>This value is used for project documentation sources and project references so that the
        /// correct items are used from them.</value>
        string MSBuildOutDir { get; set; }

        /// <summary>
        /// This read-only property is used to get the project-specific <c>OutDir</c> property value if defined
        /// </summary>
        string ProjectOutDir { get; }

        /// <summary>
        /// This read-only property is used to get the dirty state of the project
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// This read-only property is used to get the build log file location
        /// </summary>
        /// <value>If <see cref="BuildLogFile"/> is set, it returns its value.  If not set, it returns the full
        /// path created by using the <see cref="OutputPath"/> property value and a filename of
        /// <strong>LastBuild.log</strong>.</value>
        string LogFileLocation { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of documentation sources to use in building the
        /// help file.
        /// </summary>
        IEnumerable<IDocumentationSource> DocumentationSources { get; }

        /// <summary>
        /// This read-only property is used to get an enumerable list of all build items in the project that
        /// represent folders and files.
        /// </summary>
        IEnumerable<IFileItem> FileItems { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of image references contained in the project
        /// </summary>
        /// <returns>An enumerable list of image references if any are found in the project</returns>
        /// <remarks>Only images with IDs are returned</remarks>
        IEnumerable<ImageReference> ImagesReferences { get; }

        /// <summary>
        /// This returns an enumerable list of transform component arguments
        /// </summary>
        /// <remarks>These are passed as arguments to the XSL transformations used by the <b>BuildAssembler</b>
        /// <c>TransformComponent</c>.</remarks>
        /// <returns>An enumerable list of transform component arguments</returns>
        IEnumerable<TransformationArgument> TransformComponentArguments { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of all user-defined property names and values
        /// </summary>
        /// <returns>An enumerable list of all properties and their values determined not to be help file builder
        /// project properties, MSBuild build engine related properties, or environment variables.</returns>
        IEnumerable<(string Name, string Value)> UserDefinedProperties { get; }

        #endregion

        #region Project and namespace summary properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the project summary comments
        /// </summary>
        /// <remarks>These notes will appear in the root namespaces page if entered</remarks>
        string ProjectSummary { get; set; }

        /// <summary>
        /// This read-only property returns the list of namespace summaries
        /// </summary>
        NamespaceSummaryItemCollection NamespaceSummaries { get; }

        #endregion

        #region Path properties
        //=====================================================================

        /// <summary>
        /// This property is used to get or set the path to a folder containing additional, project-specific
        /// build components.
        /// </summary>
        /// <value>If left blank, the current project's folder is searched instead</value>
        FolderPath ComponentPath { get; set; }

        /// <summary>
        /// This property is used to get or set the base path used to locate source code for the documented
        /// assemblies.
        /// </summary>
        /// <value>If left blank, source context information will be omitted from the reflection data</value>
        FolderPath SourceCodeBasePath { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to issue a warning if a source code context could not be
        /// determined for a type.
        /// </summary>
        /// <value>This is false by default and missing source context issues will be reported as informational
        /// messages.  If set to true, they are reported as warnings that MSBuild will also report.</value>
        bool WarnOnMissingSourceContext { get; set; }

        /// <summary>
        /// This property is used to get or set the path to the HTML Help 1 compiler (HHC.EXE)
        /// </summary>
        /// <value>You only need to set this if the builder cannot determine the path for itself</value>
        FolderPath HtmlHelp1xCompilerPath { get; set; }

        /// <summary>
        /// This property is used to get or set the path to which the help files will be generated
        /// </summary>
        /// <remarks><para>The default is to create it in a folder called <strong>.\Help</strong> in the same
        /// folder as the project file.</para>
        /// 
        /// <para><strong>Warning:</strong> If building a web site, the output folder's prior content will be
        /// erased without warning prior to copying the new web site content to it!</para></remarks>
        string OutputPath { get; set; }

        /// <summary>
        /// This is used to get or set the path to the working folder used during the build process to store the
        /// intermediate files.
        /// </summary>
        /// <value><para>This can be used to perform the build in a different location with a shorter path if you
        /// encounter errors due to long file path names.  If not specified, it defaults to a folder called
        /// <strong>.\Working</strong> under the folder specified by the <see cref="OutputPath"/> property.</para>
        /// 
        /// <para><strong>Warning:</strong> All files and folders in the path specified in this property will be
        /// erased without warning when the build starts.</para></value>
        FolderPath WorkingPath { get; set; }

        /// <summary>
        /// This read-only property returns an enumerable list of folders to search for additional build
        /// components, plug-ins, presentation styles, and syntax generators.
        /// </summary>
        IEnumerable<string> ComponentSearchPaths { get; }

        #endregion

        #region Build properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the build assembler tool verbosity level
        /// </summary>
        /// <value>The default is <c>AllMessages</c> to report all messages</value>
        /// <remarks>Setting this property to <c>OnlyWarningsAndErrors</c> or <c>OnlyErrors</c> can
        /// significantly reduce the size of the build log for large projects.</remarks>
        BuildAssemblerVerbosity BuildAssemblerVerbosity { get; }

        /// <summary>
        /// This property is used to get or set whether intermediate files are deleted after a successful build
        /// </summary>
        /// <value>The default value is true</value>
        bool CleanIntermediates { get; set; }

        /// <summary>
        /// This property is used to get or set whether or not the log file is retained after a successful build
        /// </summary>
        /// <value>The default value is true</value>
        bool KeepLogFile { get; set; }

        /// <summary>
        /// This read-only property is used to get the path and filename of the build log file
        /// </summary>
        /// <value>If not specified, a default name of <strong>LastBuild.log</strong> is used and the file is
        /// saved in the path identified in the <see cref="OutputPath" /> property.</value>
        FilePath BuildLogFile { get; }

        /// <summary>
        /// This read-only property is used to get the help file format generated by the build process
        /// </summary>
        /// <value>The default is to produce an HTML Help 1 format file</value>
        /// <remarks>If building a web site, the output folder will be cleared before the new content is copied
        /// to it.</remarks>
        HelpFileFormats HelpFileFormat { get; }

        /// <summary>
        /// This read-only property is used to get whether or not to disable the custom Code Block Component so
        /// that <c>&lt;code&gt;</c> elements are rendered in their standard format by the Sandcastle XSL
        /// transformations.
        /// </summary>
        /// <value>The default is false so that the Code Block Component is used by default</value>
        bool DisableCodeBlockComponent { get; }

        /// <summary>
        /// This is used to get or set the .NET Framework version used to resolve references to system types
        /// (basic .NET Framework, Silverlight, Portable, etc.).
        /// </summary>
        /// <remarks>If set to null, it will default to the most recent version of the basic .NET Framework
        /// installed.  The build engine will adjust this at build time if necessary based on the framework
        /// types and versions found in the documentation sources.</remarks>
        string FrameworkVersion { get; set; }

        /// <summary>
        /// This read-only property is used to get whether or not the HTML rendered by <strong>BuildAssembler</strong>
        /// is indented.
        /// </summary>
        /// <value>This is mainly a debugging aid.  Leave it set to false, the default, to produce more compact
        /// HTML.</value>
        bool IndentHtml { get; }

        /// <summary>
        /// This read-only property is used to get the build assembler Save Component writer task cache capacity
        /// </summary>
        /// <value>The default is 100 to limit the cache to 100 entries</value>
        /// <remarks>Decrease the value to conserve memory, increase it to help with build speed at the expense
        /// of memory used.  Set it to zero to allow an unbounded cache for the writer task (best speed at the
        /// expense of memory used).</remarks>
        int SaveComponentCacheCapacity { get; }

        /// <summary>
        /// This read-only property is used to get a dictionary of build component configurations
        /// </summary>
        /// <remarks>This allows you to configure the settings for third party build components if they
        /// support it.</remarks>
        ComponentConfigurationDictionary ComponentConfigurations { get; }

        /// <summary>
        /// This read-only property is used to get a dictionary of build process plug-in configurations
        /// </summary>
        /// <remarks>This allows you to select and configure the settings for third party build process plug-ins</remarks>
        PlugInConfigurationDictionary PlugInConfigurations { get; }

        #endregion

        #region Help file properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the placement of any additional and conceptual content items
        /// in the table of contents.
        /// </summary>
        /// <value>The default is to place additional and conceptual content items above the namespaces</value>
        ContentPlacement ContentPlacement { get; }

        /// <summary>
        /// This read-only property is used to get whether or not all pages should be marked with a "preliminary
        /// documentation" warning in the page header.
        /// </summary>
        bool Preliminary { get; }

        /// <summary>
        /// This read-only property is used to get whether or not a root namespace entry is added to the table of
        /// contents to act as a container for the namespaces from the documented assemblies.
        /// </summary>
        /// <value>If true, a root <strong>Namespaces</strong> table of contents entry will be created as the
        /// container of the namespaces in the documented assemblies.  If false, the default, the namespaces are
        /// listed in the table of contents as root entries.</value>
        bool RootNamespaceContainer { get; }

        /// <summary>
        /// This read-only property is used to get an alternate title for the root namespaces page and the root
        /// table of contents container that appears when <see cref="RootNamespaceContainer"/> is set to true.
        /// </summary>
        /// <value>If left blank (the default), the localized version of the text "Namespaces" will be used</value>
        string RootNamespaceTitle { get; }

        /// <summary>
        /// This read-only property is used to get whether namespace grouping is enabled.  The presentation style
        /// must have support for namespace grouping in order for the feature to work.
        /// </summary>
        /// <value>If <c>true</c>, namespace grouping is enabled. Otherwise, namespace grouping is not enabled</value>
        /// <remarks>Namespace groups are determined automatically and may be documented as well</remarks>
        bool NamespaceGrouping { get; }

        /// <summary>
        /// This read-only property is used to get the maximum number of namespace parts to consider when
        /// namespace grouping is enabled.
        /// </summary>
        /// <value>The minimum and default is 2.  A higher value results in more namespace groups</value>
        /// <remarks>Namespace groups are determined automatically and may be documented as well</remarks>
        int MaximumGroupParts { get; }

        /// <summary>
        /// This read-only property is used to get or set the help file's title
        /// </summary>
        string HelpTitle { get; }

        /// <summary>
        /// This read-only property is used to get the name of the compiled help file
        /// </summary>
        /// <remarks>Do not include a path or the extension.  For MS Help Viewer builds, avoid periods,
        /// ampersands, and pound signs as they are not valid in the help file name.</remarks>
        string HtmlHelpName { get; }

        /// <summary>
        /// This read-only property is used to get the version number applied to the help file
        /// </summary>
        /// <remarks>The default is 1.0.0.0</remarks>
        string HelpFileVersion { get; }

        /// <summary>
        /// This read-only property is used to get the language option for the help file and to determine which
        /// set of presentation resource files to use.
        /// </summary>
        /// <value>If a matching set of presentation resources cannot be found for the specified language, the
        /// US English set will be used.</value>
        /// <remarks>The MS Help Viewer 1.0 Catalog ID is composed of the <see cref="CatalogProductId"/>, the
        /// <see cref="CatalogVersion"/>, and the <c>Language</c> code. For example, the English Visual Studio 10
        /// catalog is <c>VS_100_EN-US</c>.</remarks>
        CultureInfo Language { get; }

        /// <summary>
        /// This read-only property is used to get the URL to use as the link for the copyright notice
        /// </summary>
        /// <value>If not set, the <see cref="CopyrightText"/> (if any) is not turned into a clickable link</value>
        string CopyrightHref { get; }

        /// <summary>
        /// This read-only property is used to get the copyright notice that appears in the footer of each page
        /// </summary>
        /// <remarks>If not set, no copyright note will appear.  If a <see cref="CopyrightHref" /> is specified
        /// without copyright text, the URL appears instead.</remarks>
        string CopyrightText { get; }

        /// <summary>
        /// This read-only property is used to get the copyright notice that appears in the footer of each page
        /// with any hex value place holders replaced with their actual character.
        /// </summary>
        string DecodedCopyrightText { get; }

        /// <summary>
        /// This read-only property is used to get the feedback e-mail address that appears in the footer of each
        /// page.
        /// </summary>
        /// <remarks>If not set, no feedback link will appear.  If <see cref="FeedbackEMailLinkText"/> is set,
        /// that text will appear as the text for the link.  If not set, the e-mail address is used as the link
        /// text.</remarks>
        string FeedbackEMailAddress { get; }

        /// <summary>
        /// This read-only property is used to get the feedback e-mail link text that appears in the feedback
        /// e-mail link in the footer of each page.
        /// </summary>
        /// <remarks>If set, this text will appear as the link text for the <see cref="FeedbackEMailAddress"/>
        /// link.  If not set, the e-mail address is used for the link text.</remarks>
        string FeedbackEMailLinkText { get; }

        /// <summary>
        /// This read-only property is used to get additional text that should appear in the header of every page
        /// </summary>
        string HeaderText { get; }

        /// <summary>
        /// This read-only property is used to get additional text that should appear in the footer of every page
        /// </summary>
        string FooterText { get; }

        /// <summary>
        /// This read-only property is used to get the target window for external SDK links
        /// </summary>
        /// <value>The default is <c>Blank</c> to open the topics in a new window.  This option only has an
        /// effect on the <see cref="HtmlSdkLinkType"/>, <see cref="MSHelpViewerSdkLinkType"/>, and
        /// <see cref="WebsiteSdkLinkType"/> properties if they are set to <c>Msdn</c>.</value>
        SdkLinkTarget SdkLinkTarget { get; }

        /// <summary>
        /// This read-only property is used to get the presentation style for the help topic pages
        /// </summary>
        /// <value>The default is defined by <see cref="Constants.DefaultPresentationStyle" qualifyHint="true" /></value>
        string PresentationStyle { get; }

        /// <summary>
        /// This read-only property is used to get the naming method used to generate the help topic filenames
        /// </summary>
        /// <value>The default is to use GUID values as the filenames</value>
        NamingMethod NamingMethod { get; }

        /// <summary>
        /// This read-only property is used to get the language filters which determines which languages appear
        /// in the <strong>Syntax</strong> section of the help topics.
        /// </summary>
        /// <value>The default is <strong>Standard</strong> (C#, VB.NET, and C++)</value>
        string SyntaxFilters { get; }

        #endregion

        #region HTML Help 1 properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get whether or not to create a binary table of contents in Help 1
        /// files.
        /// </summary>
        /// <remarks>This can significantly reduce the amount of time required to load a very large help file</remarks>
        bool BinaryTOC { get; }

        /// <summary>
        /// This read-only property is used to get the type of links used to reference other help topics
        /// referring to framework (SDK) help topics in HTML Help 1 help files.
        /// </summary>
        /// <value>The default is to produce links to online content</value>
        HtmlSdkLinkType HtmlSdkLinkType { get; }

        /// <summary>
        /// This read-only property is used to get whether or not a Favorites tab will appear in the help file
        /// </summary>
        bool IncludeFavorites { get; }

        #endregion

        #region MS Help Viewer properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the type of links used to reference other help topics
        /// referring to framework (SDK) help topics in MS Help Viewer help files.
        /// </summary>
        /// <value>The default is to produce links to online content</value>
        MSHelpViewerSdkLinkType MSHelpViewerSdkLinkType { get; }

        /// <summary>
        /// This read-only property is used to get the Product ID portion of the MS Help Viewer 1.0 Catalog ID
        /// </summary>
        /// <value>If not specified, the default is "VS".</value>
        /// <remarks><para>The MS Help Viewer Catalog 1.0 ID is composed of the <c>CatalogProductId</c> the
        /// <see cref="CatalogVersion"/>, and the <see cref="Language"/> code. For example, the English Visual
        /// Studio 10 catalog is <c>VS_100_EN-US</c>.</para>
        /// 
        /// <note type="note">You should typically use the default value</note>
        /// </remarks>
        string CatalogProductId { get; }

        /// <summary>
        /// This read-only property is used to get the Version portion of the MS Help Viewer 1.0 Catalog ID
        /// </summary>
        /// <value>If not specified, the default is "100"</value>
        /// <remarks><para>The MS Help Viewer 1.0 Catalog ID is composed of the <see cref="CatalogProductId"/>,
        /// the <c>CatalogVersion</c>, and the <see cref="Language"/> code. For example, the English Visual
        /// Studio 10 catalog is <c>VS_100_EN-US</c>.</para>
        /// 
        /// <note type="note">You should typically used the default value</note>
        /// </remarks>
        string CatalogVersion { get; }

        /// <summary>
        /// This read-only property is used to get a non-standard MS Help Viewer 2.x content catalog name
        /// </summary>
        /// <value>If not specified, the default will be set based on the Visual Studio version catalog related
        /// to the Help Viewer (VisualStudio12 for Visual Studio 2013 for example).</value>
        string CatalogName { get; }

        /// <summary>
        /// This read-only property is used to get the vendor name for the help viewer file
        /// </summary>
        /// <value>The default if not specified will be "Vendor Name".  The value must not contain the ':',
        /// '\', '/', '.', ',', '#', or '&amp;' characters.</value>
        string VendorName { get; }

        /// <summary>
        /// This read-only property is used to get the product title for the help viewer file
        /// </summary>
        /// <value>The default if not specified will be the value of the <see cref="HelpTitle" /> property</value>
        string ProductTitle { get; }

        /// <summary>
        /// This read-only property is used to get the topic version for each topic in the help file
        /// </summary>
        /// <value>The default is "100" (meaning 10.0)</value>
        string TopicVersion { get; }

        /// <summary>
        /// This read-only property is used to get the table of contents parent for each root topic in the help
        /// file.
        /// </summary>
        /// <value>The default is "-1" to show the root topics in the root of the main table of content</value>
        string TocParentId { get; }

        /// <summary>
        /// This read-only property is used to get the topic version of the <see cref="TocParentId" /> topic
        /// </summary>
        /// <value>The default is "100" meaning "10.0"</value>
        string TocParentVersion { get; }

        /// <summary>
        /// This read-only property is used to get the sort order for conceptual content so that it appears
        /// within its parent in the correct position.
        /// </summary>
        /// <value>The default is -1 to let the build engine determine the best value to use based on the
        /// other project properties.</value>
        int TocOrder { get; }

        /// <summary>
        /// This is used to get or set the display version shown below entries in the search results pane in the
        /// help viewer application.
        /// </summary>
        /// <value>If not set, a display version will not be shown for topics in the search results pane</value>
        /// <remarks>If set, this typically refers to the SDK name and version or the module in which the member
        /// resides to help differentiate it from other entries with the same title in the search results.</remarks>
        string SearchResultsDisplayVersion { get; set; }

        #endregion

        #region Website properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the type of links used to reference other help topics
        /// referring to framework (SDK) help topics in HTML Help 1 help files.
        /// </summary>
        /// <value>The default is to produce links to online content</value>
        HtmlSdkLinkType WebsiteSdkLinkType { get; }

        /// <summary>
        /// This read-only property is used to get the ad content to place in each page in the website help file
        /// format.
        /// </summary>
        string WebsiteAdContent { get; }

        #endregion

        #region Markdown properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get whether or not to append ".md" extensions to topic URLs
        /// </summary>
        /// <value>The default is to false to leave them off.  This is suitable for GitHib wiki content which
        /// does not add the filename extensions.  Adding them causes the wiki to link to the raw file content
        /// rather than the rendered topic.  If your site uses them or if you are rendering content to store in
        /// source control where they are used, set this property to true.</value>
        bool AppendMarkdownFileExtensionsToUrls { get; }

        #endregion

        #region Show Missing Tags properties
        //=====================================================================

        /// <summary>
        /// This read-only helper property returns the flags to use when looking for missing tags
        /// </summary>
        MissingTags MissingTags { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing namespace comments are indicated in the
        /// help file.
        /// </summary>
        bool ShowMissingNamespaces { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;summary&gt; tags are indicated in
        /// the help file.
        /// </summary>
        bool ShowMissingSummaries { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;param&gt; tags are indicated in the
        /// help file
        /// </summary>
        bool ShowMissingParams { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;typeparam&gt; tags on generic types
        /// and methods are indicated in the help file.
        /// </summary>
        bool ShowMissingTypeParams { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;returns&gt; tags are indicated in
        /// the help file.
        /// </summary>
        bool ShowMissingReturns { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;value&gt; tags are indicated in the
        /// help file.
        /// </summary>
        bool ShowMissingValues { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;remarks&gt; tags are indicated in
        /// the help file.
        /// </summary>
        bool ShowMissingRemarks { get; }

        /// <summary>
        /// This read-only property is used to get whether or not constructors are automatically documented if
        /// they are missing the &lt;summary&gt; tag and for classes with compiler generated constructors.
        /// </summary>
        bool AutoDocumentConstructors { get; }

        /// <summary>
        /// This read-only property is used to get whether or not dispose methods are automatically documented if
        /// they are missing the &lt;summary&gt; tag and for classes with compiler generated dispose methods.
        /// </summary>
        bool AutoDocumentDisposeMethods { get; }

        /// <summary>
        /// This read-only property is used to get whether or not missing &lt;include&gt; tag target
        /// documentation is indicated in the help file.
        /// </summary>
        bool ShowMissingIncludeTargets { get; }

        #endregion

        #region Visibility properties
        //=====================================================================

        /// <summary>
        /// This read-only helper property returns the flags used to indicate which optional items to document
        /// </summary>
        VisibleItems VisibleItems { get; }

        /// <summary>
        /// This read-only property is used to get whether or not attributes on types and members are documented
        /// in the syntax portion of the help file.
        /// </summary>
        bool DocumentAttributes { get; }

        /// <summary>
        /// This read-only property is used to get whether or not explicit interface implementations are
        /// documented.
        /// </summary>
        bool DocumentExplicitInterfaceImplementations { get; }

        /// <summary>
        /// This read-only property is used to get whether or not inherited members are documented
        /// </summary>
        bool DocumentInheritedMembers { get; }

        /// <summary>
        /// This read-only property is used to get whether or not inherited framework members are documented
        /// </summary>
        bool DocumentInheritedFrameworkMembers { get; }

        /// <summary>
        /// This read-only property is used to get whether or not inherited internal framework members are
        /// documented.
        /// </summary>
        bool DocumentInheritedFrameworkInternalMembers { get; }

        /// <summary>
        /// This read-only property is used to get whether or not inherited private framework members are
        /// documented.
        /// </summary>
        bool DocumentInheritedFrameworkPrivateMembers { get; }

        /// <summary>
        /// This read-only property is used to get whether or not internal members are documented in the help
        /// file.
        /// </summary>
        bool DocumentInternals { get; }

        /// <summary>
        /// This read-only property is used to get whether or not private members are documented in the help file
        /// </summary>
        bool DocumentPrivates { get; }

        /// <summary>
        /// This read-only property is used to get or set whether or not private fields are documented in the
        /// help file.
        /// </summary>
        bool DocumentPrivateFields { get; }

        /// <summary>
        /// This read-only property is used to get whether or not protected members are documented in the help
        /// file.
        /// </summary>
        bool DocumentProtected { get; }

        /// <summary>
        /// This read-only property is used to get whether or not protected members of sealed classes are
        /// documented in the help file.
        /// </summary>
        bool DocumentSealedProtected { get; }

        /// <summary>
        /// This read-only property is used to get whether or not "protected internal" members are documented as
        /// "protected" only in the help file.
        /// </summary>
        bool DocumentProtectedInternalAsProtected { get; }

        /// <summary>
        /// This read-only property is used to get whether or not no-PIA (Primary Interop Assembly) embedded
        /// interop types are documented in the help file.
        /// </summary>
        bool DocumentNoPIATypes { get; }

        /// <summary>
        /// This read-only property is used to get whether or not public compiler generated types and members are
        /// documented in the help file.
        /// </summary>
        bool DocumentPublicCompilerGenerated { get; }

        /// <summary>
        /// This read-only property is used to get whether or not members marked with an
        /// <see cref="EditorBrowsableAttribute"/> set to <c>Never</c> are documented.
        /// </summary>
        bool DocumentEditorBrowsableNever { get; }

        /// <summary>
        /// This read-only property is used to get whether or not members marked with a
        /// <see cref="BrowsableAttribute"/> set to <c>False</c> are documented.
        /// </summary>
        bool DocumentNonBrowsable { get; }

        /// <summary>
        /// This read-only property is used to get whether or not internal members of base types in other
        /// assemblies and private members in base types are documented.
        /// </summary>
        bool DocumentInternalAndPrivateIfExternal { get; }

        /// <summary>
        /// This read-only property is used to get whether or not extension methods are included in member list
        /// topics.
        /// </summary>
        /// <value>Note that the property is the inverse of the underlying <see cref="VisibleItems.OmitExtensionMethods"/>
        /// value to maintain backward compatibility with prior releases.</value>
        bool IncludeExtensionMethodsInMemberLists { get; }

        /// <summary>
        /// This read-only property is used to get whether or not extension methods that extend <see cref="Object"/>
        /// are included in member list topics.
        /// </summary>
        /// <remarks>This has no effect if <see cref="IncludeExtensionMethodsInMemberLists"/> is false.</remarks>
        /// <value>Note that the property is the inverse of the underlying <see cref="VisibleItems.OmitObjectExtensionMethods"/>
        /// value to maintain backward compatibility with prior releases.</value>
        bool IncludeObjectExtensionMethodsInMemberLists { get; }

        /// <summary>
        /// This read-only property is used to get the API filter
        /// </summary>
        ApiFilterCollection ApiFilter { get; }

        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add a new folder build item to the project
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <returns>The new <see cref="IFileItem"/>.</returns>
        /// <remarks>If the folder does not exist in the project, it is added and created if not already there.
        /// If the folder is already part of the project, the existing item is returned.</remarks>
        /// <exception cref="ArgumentException">This is thrown if the path matches the project root path or is
        /// not below it.</exception>
        IFileItem AddFolderToProject(string folder);

        /// <summary>
        /// Add a new file build item to the project
        /// </summary>
        /// <param name="sourceFile">The source filename</param>
        /// <param name="destinationFile">The optional destination path.  If empty, null, or it does not start with the
        /// project folder, the file is copied to the root folder of the project.</param>
        /// <returns>The new <see cref="IFileItem" /></returns>
        /// <remarks>If the file does not exist in the project, it is copied to the destination path or project
        /// folder if not already there.  The default build action is determined based on the filename's
        /// extension.  If the file is already part of the project, the existing item is returned.</remarks>
        IFileItem AddFileToProject(string sourceFile, string destinationFile);

        /// <summary>
        /// This is used to locate a file by name in the project
        /// </summary>
        /// <param name="fileToFind">The fully qualified file path to find</param>
        /// <returns>The file item if found or null if not found</returns>
        IFileItem FindFile(string fileToFind);

        /// <summary>
        /// This returns true if the project contains items using the given build action
        /// </summary>
        /// <param name="buildAction">The build action for which to check</param>
        /// <returns>True if at least one item has the given build action or false if there are no items with
        /// the given build action.</returns>
        bool HasItems(BuildAction buildAction);

        /// <summary>
        /// This is used to determine whether or not the given name can be used for a user-defined project
        /// property.
        /// </summary>
        /// <param name="name">The name to check</param>
        /// <returns>True if it can be used, false if it cannot be used</returns>
        bool IsValidUserDefinedPropertyName(string name);

        /// <summary>
        /// This is used to create copy of the project
        /// </summary>
        /// <returns>A clone of the project</returns>
        ISandcastleProject Clone();

        /// <summary>
        /// Create a new build process for the project
        /// </summary>
        /// <param name="partialBuildType">The partial build type to use</param>
        /// <returns>A build process for the project</returns>
        IBuildProcess CreateBuildProcess(PartialBuildType partialBuildType);

        #endregion
    }
}
