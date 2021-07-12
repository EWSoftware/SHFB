//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalReferenceLinksPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/30/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in designed to add additional reference link targets to the Reflection Index Data
// and Resolve Reference Links build components so that links can be created to other third party help in a
// help collection or additional online content.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/25/2008  EFW  Created the code
// 08/13/2008  EFW  Updated to support the new project format
// 06/07/2010  EFW  Added support for multi-format build output
// 01/01/2013  EFW  Updated for use with the new cached build components.  Added code to insert the reflection
//                  file names into the GenerateInheritedDocs tool configuration file.
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to add additional reference link targets to the <strong>Reflection Index
    /// Data</strong> and <strong>Resolve Reference Links</strong> build components so that links can be created
    /// to other third party help in a help collection or additional online content.
    /// </summary>
    [HelpFileBuilderPlugInExport("Additional Reference Links", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in is used to add additional reference link " +
        "targets to the Reflection Index Data and Resolve Reference Links build component so that links can be " +
        "created to other third party help in a help collection or additional online content.")]
    public sealed class AdditionalReferenceLinksPlugIn : IPlugIn, IProgress<BuildProgressEventArgs>
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;
        private BuildStep lastBuildStep;

        private List<ReferenceLinkSettings> otherLinks;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        new ExecutionPoint(BuildStep.GenerateNamespaceSummaries, ExecutionBehaviors.Before),
                        new ExecutionPoint(BuildStep.GenerateInheritedDocumentation, ExecutionBehaviors.Before),
                        new ExecutionPoint(BuildStep.CreateBuildAssemblerConfigs, ExecutionBehaviors.Before),
                        new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.After)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process.</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize
        /// itself.</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in configuration is not valid.</exception>
        public void Initialize(BuildProcess buildProcess, XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));
            otherLinks = new List<ReferenceLinkSettings>();

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            if(configuration.IsEmpty)
                throw new BuilderException("ARL0001", "The Additional Reference Links plug-in has not been " +
                    "configured yet");

            // Load the reference links settings
            foreach(var t in configuration.Descendants("target"))
                otherLinks.Add(ReferenceLinkSettings.FromXml(buildProcess.CurrentProject, t));

            if(otherLinks.Count == 0)
            {
                throw new BuilderException("ARL0002", "At least one target is required for the Additional " +
                    "Reference Links plug-in.");
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            string workingPath, configFilename;

            if(context == null)
                throw new ArgumentNullException(nameof(context));

            if(context.BuildStep == BuildStep.GenerateNamespaceSummaries)
            {
                // Merge the additional reference links information
                builder.ReportProgress("Performing partial builds on additional targets' projects");

                // Build each of the projects
                foreach(ReferenceLinkSettings vs in otherLinks)
                {
                    using(SandcastleProject project = new SandcastleProject(vs.HelpFileProject, true, true))
                    {
                        // We'll use a working folder below the current project's working folder
                        workingPath = Path.Combine(builder.WorkingFolder,
                            vs.HelpFileProject.GetHashCode().ToString("X", CultureInfo.InvariantCulture));

                        bool success = this.BuildProject(project, workingPath);

                        // Switch back to the original folder for the current project
                        Directory.SetCurrentDirectory(builder.ProjectFolder);

                        if(!success)
                        {
                            throw new BuilderException("ARL0003", "Unable to build additional target project: " +
                                project.Filename);
                        }
                    }

                    // Save the reflection file location as we need it later
                    vs.ReflectionFilename = Path.Combine(workingPath, "reflection.xml");
                }

                return;
            }

            if(context.BuildStep == BuildStep.GenerateInheritedDocumentation)
            {
                this.MergeInheritedDocConfig();
                return;
            }

            if(context.BuildStep == BuildStep.CreateBuildAssemblerConfigs)
            {
                builder.ReportProgress("Adding additional reference link namespaces...");

                var rn = builder.ReferencedNamespaces;

                HashSet<string> validNamespaces = new HashSet<string>(Directory.EnumerateFiles(
                    builder.FrameworkReflectionDataFolder, "*.xml", SearchOption.AllDirectories).Select(
                        f => Path.GetFileNameWithoutExtension(f)));

                foreach(ReferenceLinkSettings vs in otherLinks)
                    if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                        foreach(var n in BuildProcess.GetReferencedNamespaces(vs.ReflectionFilename, validNamespaces))
                            rn.Add(n);

                return;
            }

            // Merge the reflection file info into sancastle.config
            configFilename = Path.Combine(builder.WorkingFolder, "sandcastle.config");

            if(File.Exists(configFilename))
                this.MergeReflectionInfo(configFilename);
        }

        /// <summary>
        /// This is used to merge the reflection file names into the inherited documentation tool's configuration
        /// file.
        /// </summary>
        private void MergeInheritedDocConfig()
        {
            string configFilename = Path.Combine(builder.WorkingFolder, "GenerateInheritedDocs.config");

            builder.ReportProgress("Adding references to {0}...", configFilename);
            
            var configFile = XDocument.Load(configFilename);

            foreach(ReferenceLinkSettings vs in otherLinks)
                if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                    configFile.Root.Add(new XElement("reflectionInfo", new XAttribute("file", vs.ReflectionFilename)));

            configFile.Save(configFilename);
        }

        /// <summary>
        /// This is used to merge the reflection file info into the named configuration file.
        /// </summary>
        /// <param name="configFilename">The configuration filename</param>
        private void MergeReflectionInfo(string configFilename)
        {
            XElement target;
            HelpFileFormats helpFormat;

            builder.ReportProgress("\r\nAdding references to {0}...", configFilename);

            var configFile = XDocument.Load(configFilename);

            // Add them to the Reflection Index Data component.  There are multiple copies of this component
            // type but we only need the first one.  This only appears in the reference build's configuration
            // file.
            var component = configFile.XPathSelectElement("//component[@id='Copy From Index Component']/" +
                "index[@name='reflection']");

            // If not found, try for the cached version
            if(component == null)
            {
                component = configFile.XPathSelectElement("//component[starts-with(@id, " +
                    "'Reflection Index Data')]/index[@name='reflection']");
            }

            if(component == null)
            {
                throw new BuilderException("ARL0004", "Unable to locate Reflection Index Data component in " +
                    configFilename);
            }

            var lastChild = component.Descendants().Last();

            foreach(ReferenceLinkSettings vs in otherLinks)
                if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                {
                    target = new XElement("data",
                        new XAttribute("files", vs.ReflectionFilename),
                        new XAttribute("groupId", builder.SubstitutionTags.TransformText("Project_Ref_{@UniqueID}")));

                    // Keep the current project's stuff listed last so that it takes precedence
                    lastChild.AddBeforeSelf(target);
                }

            // Add them to the Resolve Reference Links component
            var matchingComponents = configFile.XPathSelectElements("//component[starts-with(@id, 'Resolve Reference Links')]").ToList();

            if(matchingComponents.Count == 0)
            {
                throw new BuilderException("ARL0005", "Unable to locate Resolve Reference Links component in " +
                    configFilename);
            }

            foreach(XElement match in matchingComponents)
            {
                lastChild = match.Descendants().Last();

                foreach(ReferenceLinkSettings vs in otherLinks)
                    if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                    {
                        target = new XElement("targets", new XAttribute("files", vs.ReflectionFilename));

                        // Look for a format attribute on the parent component which is typically the
                        // Multi-format Output Component.
                        var format = match.Parent.Attribute("format");

                        // If not found, we've probably got a single format presentation style so look to the
                        // project for the format.
                        if(format != null)
                            helpFormat = (HelpFileFormats)Enum.Parse(typeof(HelpFileFormats), format.Value, true);
                        else
                            helpFormat = builder.CurrentProject.HelpFileFormat;

                        string formatType;

                        switch(helpFormat)
                        {
                            case HelpFileFormats.HtmlHelp1:
                                formatType = vs.HtmlSdkLinkType.ToString();
                                break;

                            case HelpFileFormats.MSHelpViewer:
                                formatType = vs.MSHelpViewerSdkLinkType.ToString();
                                break;

                            default:    // Website, Open XML, and markdown formats
                                formatType = vs.WebsiteSdkLinkType.ToString();
                                break;
                        }

                        target.Add(new XAttribute("type", formatType));
                        target.Add(new XAttribute("groupId", builder.SubstitutionTags.TransformText("Project_{@UniqueID}")));

                        // Keep the current project's stuff listed last so that it takes precedence
                        lastChild.AddBeforeSelf(target);
                    }
            }

            configFile.Save(configFilename);
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object.
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is called to build a project
        /// </summary>
        /// <param name="project">The project to build</param>
        /// <param name="workingPath">The working path for the project</param>
        /// <returns>Returns true if successful, false if not</returns>
        private bool BuildProject(SandcastleProject project, string workingPath)
        {
            BuildProcess buildProcess;

            lastBuildStep = BuildStep.None;

            builder.ReportProgress("\r\nBuilding {0}", project.Filename);

            try
            {
                project.Configuration = builder.CurrentProject.Configuration;
                project.Platform = builder.CurrentProject.Platform;

                // For the plug-in, we'll override some project settings
                project.HtmlHelp1xCompilerPath = new FolderPath(builder.Help1CompilerFolder, true, project);
                project.WorkingPath = new FolderPath(workingPath, true, project);
                project.OutputPath = new FolderPath(Path.Combine(workingPath, @"..\PartialBuildLog\"), true, project);

                // If the current project has defined OutDir, pass it on to the sub-project.
                string outDir = builder.CurrentProject.MSBuildProject.GetProperty("OutDir")?.EvaluatedValue;

                if(!String.IsNullOrEmpty(outDir) && outDir != @".\")
                    project.MSBuildOutDir = outDir;

                // Run the partial build through the transformation step as we need the document model
                // applied and filenames added.
                buildProcess = new BuildProcess(project, PartialBuildType.TransformReflectionInfo)
                {
                    ProgressReportProvider = this,
                    CancellationToken = builder.CancellationToken
                };

                // Since this is a plug-in, we'll run it synchronously rather than as a background task
                buildProcess.Build();

                lastBuildStep = buildProcess.CurrentBuildStep;

                // Add the list of the comments files in the other project to this build
                if(lastBuildStep == BuildStep.Completed)
                    foreach(XmlCommentsFile comments in buildProcess.CommentsFiles)
                        builder.CommentsFiles.Insert(0, comments);
            }
            catch(Exception ex)
            {
                throw new BuilderException("ARL0006", String.Format(CultureInfo.InvariantCulture,
                    "Fatal error, unable to compile project '{0}': {1}", project.Filename, ex.ToString()));
            }

            return (lastBuildStep == BuildStep.Completed);
        }
        #endregion

        #region IProgress<BuildProgressEventArgs> Members
        //=====================================================================

        /// <summary>
        /// This is called by the build process to report build progress for the reference link projects
        /// </summary>
        /// <param name="value">The event arguments</param>
        /// <remarks>Since the build is synchronous in this plug-in, we need to implement the interface and
        /// report progress synchronously as well or the final few messages can get lost and it looks like the
        /// build failed.</remarks>
        public void Report(BuildProgressEventArgs value)
        {
            if(value != null && value.StepChanged)
                builder.ReportProgress(value.BuildStep.ToString());
        }
        #endregion
    }
}
