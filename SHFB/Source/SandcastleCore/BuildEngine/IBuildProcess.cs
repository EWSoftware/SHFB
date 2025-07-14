//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IBuildProcess.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/12/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the interface used to interact with a help file builder build process
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

// Ignore Spelling: nologo clp

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;

using Sandcastle.Core.InheritedDocumentation;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.PresentationStyle;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.BuildEngine
{
    /// <summary>
    /// This defines the interface used to interact with a help file builder build process
    /// </summary>
    public interface IBuildProcess
    {
        /// <summary>
        /// This controls whether or not the API filter is suppressed
        /// </summary>
        /// <value>By default, it is not suppressed and the API filter will be applied.  The API Filter designer
        /// uses this to suppress the filter so that all members are obtained.</value>
        bool SuppressApiFilter { get; set; }

        /// <summary>
        /// This is used to get or set the progress report provider
        /// </summary>
        /// <remarks>If not set, no progress will be reported by the build</remarks>
        IProgress<BuildProgressEventArgs> ProgressReportProvider { get; set; }

        /// <summary>
        /// This is used to get or set the cancellation token for the build if running as a task
        /// </summary>
        CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// This read-only property is used to get the current build step
        /// </summary>
        BuildStep CurrentBuildStep { get; }

        /// <summary>
        /// This read-only property is used to get the current help file format being generated
        /// </summary>
        /// <remarks>The <strong>GenerateHelpProject</strong>, and <strong>CompilingHelpFile</strong>
        /// steps will run once for each help file format selected.  This property allows a plug-in to determine
        /// which files it may need to work with during those steps or to skip processing if it is not relevant.</remarks>
        HelpFileFormats CurrentFormat { get; }

        /// <summary>
        /// This read-only property returns the language used for resource items, etc.
        /// </summary>
        CultureInfo Language { get; }

        /// <summary>
        /// This read-only property returns the project folder name
        /// </summary>
        string ProjectFolder { get; }

        /// <summary>
        /// This read-only property is used to get the name of the working files folder
        /// </summary>
        string WorkingFolder { get; }

        /// <summary>
        /// This read-only property is used to get the name of the folder that contains the reflection data for
        /// the selected framework platform and version (.NETFramework 4., .NETCore 4.5, Silverlight 5.0, etc.).
        /// </summary>
        string FrameworkReflectionDataFolder { get; }

        /// <summary>
        /// This read-only property is used to get the name of the HTML Help 1 compiler folder determined by the
        /// build process.
        /// </summary>
        string Help1CompilerFolder { get; }

        /// <summary>
        /// This read-only property provides access to the generate inherited documentation tool's reflection
        /// data files during the <see cref="BuildStep.GenerateInheritedDocumentation" /> build step.
        /// </summary>
        /// <remarks>This can be used by plug-ins to add additional reflection data files to use for inherited
        /// documentation.</remarks>
        ReflectionFiles InheritedDocsReflectionFiles { get; }

        /// <summary>
        /// This read-only property is used to get the Help 1 folder for the title and keyword HTML extract tool
        /// during the <see cref="BuildStep.ExtractingHtmlInfo" /> build step.
        /// </summary>
        /// <remarks>This can be used by plug-ins to find the help 1 content</remarks>
        string HtmlExtractHelp1Folder { get; }

        /// <summary>
        /// This property is used to get or set the localized folder for the title and keyword HTML extract tool
        /// during the <see cref="BuildStep.ExtractingHtmlInfo" /> build step.
        /// </summary>
        /// <remarks>This can be used by plug-ins to adjust how the tool runs</remarks>
        string HtmlExtractLocalizedFolder { get; set; }

        /// <summary>
        /// This read-only property is used to get the project filename without the folder
        /// </summary>
        string ProjectFilename { get; }

        /// <summary>
        /// This read-only property is used to get the name of the log file used for saving the build progress
        /// messages.
        /// </summary>
        string LogFilename { get; }

        /// <summary>
        /// This read-only property is used to get the output folder where the log file and help file can be
        /// found after the build process has finished.
        /// </summary>
        string OutputFolder { get; }

        /// <summary>
        /// This read-only property returns a collection of the output folders specific to each help file format
        /// produced by the build.
        /// </summary>
        Collection<string> HelpFormatOutputFolders { get; }

        /// <summary>
        /// This read-only property is used to get the name of the reflection information file
        /// </summary>
        string ReflectionInfoFilename { get; }

        /// <summary>
        /// This read-only property is used to get the name of the BuildAssembler topic manifest file
        /// </summary>
        string BuildAssemblerManifestFile { get; }

        /// <summary>
        /// This read-only property is used to get the name of the BuildAssembler configuration file
        /// </summary>
        string BuildAssemblerConfigurationFile { get; }

        /// <summary>
        /// This read-only property is used to get the XML comments files collection
        /// </summary>
        XmlCommentsFileCollection CommentsFiles { get; }

        /// <summary>
        /// This read-only property is used to get the presentation instance being used by the build process
        /// </summary>
        PresentationStyleSettings PresentationStyle { get; }

        /// <summary>
        /// This read-only property is used to get the current project being used for the build
        /// </summary>
        /// <remarks>Although there is nothing stopping it, project options should not be modified during a
        /// build.</remarks>
        ISandcastleProject CurrentProject { get; }

        /// <summary>
        /// This read-only property is used to get a hash set used to contain a list of namespaces referenced by
        /// the project reflection data files, project XML comments files, and base framework XML comments files.
        /// </summary>
        /// <value>These namespaces are used to limit what the Resolve Reference Links component has to index</value>
        HashSet<string> ReferencedNamespaces { get; }

        /// <summary>
        /// This read-only property is used to get the filename of the default topic as determined by the build
        /// engine.
        /// </summary>
        /// <remarks>The path is relative to the root of the output folder (i.e. html/DefaultTopic.htm)</remarks>
        string DefaultTopicFile { get; }

        /// <summary>
        /// This read-only property is used to get the <see cref="ISandcastleProject.HelpTitle"/> project property
        /// value with all substitution tags it contains, if any, resolved to actual values.
        /// </summary>
        string ResolvedHelpTitle { get; }

        /// <summary>
        /// This read-only property is used to get the substitution tag handler
        /// </summary>
        ISubstitutionTags SubstitutionTags { get; }

        /// <summary>
        /// This read-only property is used to get a list of the HTML Help 1 (CHM) files that were built
        /// </summary>
        /// <remarks>If the HTML Help 1 format was not built, this returns an empty collection</remarks>
        Collection<string> Help1Files { get; }

        /// <summary>
        /// This read-only property is used to get a list of the MS Help Viewer (MSHC) files that were built
        /// </summary>
        /// <remarks>If the MS Help Viewer format was not built, this returns an empty collection</remarks>
        Collection<string> HelpViewerFiles { get; }

        /// <summary>
        /// This read-only property is used to get a list of the website files that were built
        /// </summary>
        /// <remarks>If the website format was not built, this returns an empty collection</remarks>
        Collection<string> WebsiteFiles { get; }

        /// <summary>
        /// This read-only property is used to get a list of the Open XML files that were built
        /// </summary>
        /// <remarks>If the Open XML format was not built, this returns an empty collection</remarks>
        Collection<string> OpenXmlFiles { get; }

        /// <summary>
        /// This read-only property is used to get a list of the Markdown files that were built
        /// </summary>
        /// <remarks>If the Markdown format was not built, this returns an empty collection</remarks>
        Collection<string> MarkdownFiles { get; }

        /// <summary>
        /// Call this method to perform the build on the project.
        /// </summary>
        void Build();

        /// <summary>
        /// This is used to report progress during the build process within the current step
        /// </summary>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        /// <overloads>This method has two overloads.</overloads>
        void ReportProgress(string message, params object[] args);

        /// <summary>
        /// This is used to report an error that will abort the build
        /// </summary>
        /// <param name="step">The current build step</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        /// <remarks>This just reports the error.  The caller must abort the build</remarks>
        void ReportError(BuildStep step, string errorCode, string message, params object[] args);

        /// <summary>
        /// This is used to report a warning that may need attention
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">A list of arguments to format into the message text</param>
        void ReportWarning(string warningCode, string message, params object[] args);

        /// <summary>
        /// This can be used by plug-ins using the <see cref="ExecutionBehaviors.InsteadOf" /> execution behavior
        /// to execute plug-ins that want to run before the plug-in executes its main processing.
        /// </summary>
        /// <remarks>This will only run once per step.  Any subsequent calls by other plug-ins will be ignored.</remarks>
        void ExecuteBeforeStepPlugIns();

        /// <summary>
        /// This can be used by plug-ins using the <see cref="ExecutionBehaviors.InsteadOf" /> execution behavior
        /// to execute plug-ins that want to run after the plug-in has executed its main processing.
        /// </summary>
        /// <remarks>This will only run once per step.  Any subsequent calls by other plug-ins will be ignored.</remarks>
        void ExecuteAfterStepPlugIns();


        /// <summary>
        /// Load a help file builder project from the given filename unrelated to the current build process
        /// </summary>
        /// <param name="filename">The project to load</param>
        /// <param name="mustExist">Specify true if the file must exist or false if a new project should be
        /// created if the file does not exist.</param>
        /// <param name="useFinalValues">True to use final evaluated property values, or false to use the
        /// unevaluated property values.  For builds, this should always be true.  If loading the project for
        /// editing, it should always be false.</param>
        /// <returns>A reference to the loaded help file builder project</returns>
        /// <exception cref="ArgumentException">This is thrown if a filename is not specified or if it does not
        /// exist and <c>mustExist</c> is true.</exception>
        ISandcastleProject Load(string filename, bool mustExist, bool useFinalValues);

        /// <summary>
        /// Run the specified MSBuild project using MSBuild.exe or dotnet.exe
        /// </summary>
        /// <param name="projectFile">The project file to run</param>
        /// <param name="minimalOutput">True for minimal output, false for normal output</param>
        /// <remarks>A standard set of command line options will be used (<c>/nologo /clp:NoSummary</c>) along
        /// with the given verbosity level.  The 64-bit version of MSBuild will be used if available on 64-bit
        /// systems.</remarks>
        void RunProject(string projectFile, bool minimalOutput);

        /// <summary>
        /// Run the specified MSBuild project using the 32-bit version of MSBuild
        /// </summary>
        /// <param name="projectFile">The project file to run</param>
        /// <param name="minimalOutput">True for minimal output, false for normal output</param>
        /// <remarks>A standard set of command line options will be used (<c>/nologo /clp:NoSummary</c>) along
        /// with the given verbosity level.  Silverlight build targets are only available for 32-bit builds
        /// regardless of the framework version and require the 32-bit version of MSBuild in order to load the
        /// targets files correctly.</remarks>
        void Run32BitProject(string projectFile, bool minimalOutput);

        /// <summary>
        /// Run the specified process with the given arguments
        /// </summary>
        /// <param name="processFilename">The process to execute.</param>
        /// <param name="targetFile">An optional target file that the process will operate on such as an MSBuild
        /// project file.</param>
        /// <param name="arguments">An optional set of arguments to pass to the process</param>
        void Run(string processFilename, string targetFile, string arguments);
    }
}
