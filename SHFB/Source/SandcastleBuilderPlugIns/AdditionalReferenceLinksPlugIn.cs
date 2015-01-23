//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalReferenceLinksPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/14/2014
// Note    : Copyright 2008-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to add additional reference link targets to the Reflection Index Data
// and Resolve Reference Links build components so that links can be created to other third party help in a
// Help 2 collection.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
//===============================================================================================================
// 1.6.0.5  02/25/2008  EFW  Created the code
// 1.8.0.0  08/13/2008  EFW  Updated to support the new project format
// 1.9.0.0  06/07/2010  EFW  Added support for multi-format build output
// 1.9.7.0  01/01/2013  EFW  Updated for use with the new cached build components.  Added code to insert the
//                           reflection file names into the GenerateInheritedDocs tool configuration file.
// -------  12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to add additional reference link targets to the <b>Reflection Index
    /// Data</b> and <b>Resolve Reference Links</b> build components so that links can be created to other third
    /// party help in Help 2/MS Help Viewer collections or additional online MSDN content.
    /// </summary>
    [HelpFileBuilderPlugInExport("Additional Reference Links", IsConfigurable = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to add additional reference link targets to the Reflection Index " +
        "Data and Resolve Reference Links build component so that links can be created to other third " +
        "party help in a Help 2 collection, MS Help Viewer collection, or additional online MSDN content.")]
    public sealed class AdditionalReferenceLinksPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;
        private BuildStep lastBuildStep;

        // Plug-in configuration options
        private ReferenceLinkSettingsCollection otherLinks;
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
        /// This method is used by the Sandcastle Help File Builder to let the plug-in perform its own
        /// configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            using(AdditionalReferenceLinksConfigDlg dlg = new AdditionalReferenceLinksConfigDlg(project, currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process.</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize
        /// itself.</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in configuration is not valid.</exception>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            XPathNavigator root;

            builder = buildProcess;
            otherLinks = new ReferenceLinkSettingsCollection();

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("ARL0001", "The Additional Reference Links plug-in has not been " +
                    "configured yet");

            // Load the reference links settings
            otherLinks.FromXml(buildProcess.CurrentProject, root);

            if(otherLinks.Count == 0)
                throw new BuilderException("ARL0002", "At least one target is required for the Additional " +
                    "Reference Links plug-in.");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            string workingPath, configFilename;
            bool success;

            if(context.BuildStep == BuildStep.GenerateNamespaceSummaries)
            {
                // Merge the additional reference links information
                builder.ReportProgress("Performing partial builds on additional targets' projects");

                // Build each of the projects
                foreach(ReferenceLinkSettings vs in otherLinks)
                {
                    using(SandcastleProject tempProject = new SandcastleProject(vs.HelpFileProject, true))
                    {
                        // Set the configuration and platform here so that they are evaluated properly in project
                        // properties when the project is loaded below.
                        tempProject.Configuration = builder.CurrentProject.Configuration;
                        tempProject.Platform = builder.CurrentProject.Platform;

                        // This looks odd but is necessary.  If we are in Visual Studio, the above constructor
                        // may return an instance that uses an underlying MSBuild project loaded in Visual
                        // Studio.  Since the BuildProject() method modifies the project, those changes are
                        // propagated to the Visual Studio copy which we do not want to happen.  As such, we use
                        // this constructor to clone the MSBuild project XML thus avoiding modifications to the
                        // original project.
                        using(SandcastleProject project = new SandcastleProject(tempProject))
                        {
                            // We'll use a working folder below the current project's working folder
                            workingPath = builder.WorkingFolder + vs.HelpFileProject.GetHashCode().ToString("X",
                                CultureInfo.InvariantCulture) + "\\";

                            success = this.BuildProject(project, workingPath);

                            // Switch back to the original folder for the current project
                            Directory.SetCurrentDirectory(builder.ProjectFolder);

                            if(!success)
                                throw new BuilderException("ARL0003", "Unable to build additional target " +
                                    "project: " + project.Filename);
                        }
                    }

                    // Save the reflection file location as we need it later
                    vs.ReflectionFilename = workingPath + "reflection.xml";
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
                        foreach(var n in builder.GetReferencedNamespaces(vs.ReflectionFilename, validNamespaces))
                            rn.Add(n);

                return;
            }

            // Merge the reflection file info into conceptual.config
            configFilename = builder.WorkingFolder + "conceptual.config";

            if(File.Exists(configFilename))
                this.MergeReflectionInfo(configFilename, true);

            // Merge the reflection file info into sancastle.config
            configFilename = builder.WorkingFolder + "sandcastle.config";

            if(File.Exists(configFilename))
                this.MergeReflectionInfo(configFilename, false);
        }

        /// <summary>
        /// This is used to merge the reflection file names into the inherited documentation tool's configuration
        /// file.
        /// </summary>
        private void MergeInheritedDocConfig()
        {
            XmlDocument configFile;
            XmlNode config, reflectionInfo;
            XmlAttribute attr;

            string configFilename = builder.WorkingFolder + "GenerateInheritedDocs.config";

            builder.ReportProgress("Adding references to {0}...", configFilename);
            configFile = new XmlDocument();
            configFile.Load(configFilename);

            config = configFile.SelectSingleNode("configuration");

            foreach(ReferenceLinkSettings vs in otherLinks)
                if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                {
                    reflectionInfo = configFile.CreateElement("reflectionInfo");

                    attr = configFile.CreateAttribute("file");
                    attr.Value = vs.ReflectionFilename;
                    reflectionInfo.Attributes.Append(attr);

                    config.AppendChild(reflectionInfo);
                }

            configFile.Save(configFilename);
        }

        /// <summary>
        /// This is used to merge the reflection file info into the named configuration file.
        /// </summary>
        /// <param name="configFilename">The configuration filename</param>
        /// <param name="isConceptual">True if it is the conceptual configuration file or false if it is the
        /// reference configuration file.</param>
        private void MergeReflectionInfo(string configFilename, bool isConceptual)
        {
            XmlDocument configFile;
            XmlAttribute attr;
            XmlNodeList matchingComponents;
            XmlNode component, target;
            HelpFileFormats helpFormat;

            builder.ReportProgress("\r\nAdding references to {0}...", configFilename);
            configFile = new XmlDocument();
            configFile.Load(configFilename);

            if(!isConceptual)
            {
                // Add them to the Reflection Index Data component.  There are multiple copies of this component
                // type but we only need the first one.  This only appears in the reference build's configuration
                // file.
                component = configFile.SelectSingleNode("configuration/dduetools/builder/components/component[" +
                    "@id='Copy From Index Component']/index");

                // If not found, try for the cached version
                if(component == null)
                    component = configFile.SelectSingleNode("configuration/dduetools/builder/components/" +
                        "component[starts-with(@id, 'Reflection Index Data')]/index");

                if(component == null)
                    throw new BuilderException("ARL0004", "Unable to locate Reflection Index Data component in " +
                        configFilename);

                foreach(ReferenceLinkSettings vs in otherLinks)
                    if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                    {
                        target = configFile.CreateElement("data");

                        attr = configFile.CreateAttribute("files");
                        attr.Value = vs.ReflectionFilename;
                        target.Attributes.Append(attr);

                        attr = configFile.CreateAttribute("groupId");
                        attr.Value = builder.TransformText("Project_Ref_{@UniqueID}");
                        target.Attributes.Append(attr);

                        // Keep the current project's stuff listed last so that it takes precedence
                        component.InsertAfter(target, component.ChildNodes[0]);
                    }
            }

            // Add them to the Resolve Reference Links component
            matchingComponents = configFile.SelectNodes("//component[starts-with(@id, 'Resolve Reference Links')]");

            if(matchingComponents.Count == 0)
                throw new BuilderException("ARL0005", "Unable to locate Resolve Reference Links component in " +
                    configFilename);

            foreach(XmlNode match in matchingComponents)
                foreach(ReferenceLinkSettings vs in otherLinks)
                    if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                    {
                        target = configFile.CreateElement("targets");

                        attr = configFile.CreateAttribute("files");
                        attr.Value = vs.ReflectionFilename;
                        target.Attributes.Append(attr);

                        attr = configFile.CreateAttribute("type");

                        // Look for a format attribute on the parent component which is typically the
                        // Multi-format Output Component.
                        var format = match.ParentNode.Attributes["format"];

                        // If not found, we've probably got a single format presentation style so look to the
                        // project for the format.
                        if(format != null)
                            helpFormat = (HelpFileFormats)Enum.Parse(typeof(HelpFileFormats), format.Value, true);
                        else
                            helpFormat = builder.CurrentProject.HelpFileFormat;

                        switch(helpFormat)
                        {
                            case HelpFileFormats.HtmlHelp1:
                                attr.Value = vs.HtmlSdkLinkType.ToString();
                                break;

                            case HelpFileFormats.MSHelp2:
                                attr.Value = vs.MSHelp2SdkLinkType.ToString();
                                break;

                            case HelpFileFormats.MSHelpViewer:
                                attr.Value = vs.MSHelpViewerSdkLinkType.ToString();
                                break;

                            default:    // Website and Open XML format
                                attr.Value = vs.WebsiteSdkLinkType.ToString();
                                break;
                        }

                        target.Attributes.Append(attr);

                        attr = configFile.CreateAttribute("groupId");
                        attr.Value = builder.TransformText("Project_{@UniqueID}");
                        target.Attributes.Append(attr);

                        // Keep the current project's stuff listed last so that it takes precedence
                        match.InsertAfter(target, match.ChildNodes[0]);
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
                // For the plug-in, we'll override some project settings
                project.HtmlHelp1xCompilerPath = new FolderPath(builder.Help1CompilerFolder, true, project);
                project.HtmlHelp2xCompilerPath = new FolderPath(builder.Help2CompilerFolder, true, project);
                project.WorkingPath = new FolderPath(workingPath, true, project);
                project.OutputPath = new FolderPath(workingPath + @"..\PartialBuildLog\", true, project);

                // If the current project has defined OutDir, pass it on to the sub-project.
                string outDir = builder.CurrentProject.MSBuildProject.GetProperty("OutDir").EvaluatedValue;

                if(!String.IsNullOrEmpty(outDir) && outDir != @".\")
                    project.MSBuildOutDir = outDir;

                // Run the partial build through the transformation step as we need the document model
                // applied and filenames added.
                buildProcess = new BuildProcess(project, PartialBuildType.TransformReflectionInfo);

                buildProcess.BuildStepChanged += buildProcess_BuildStepChanged;

                // Since this is a plug-in, we'll run it directly rather than in a background thread
                buildProcess.Build();

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

        /// <summary>
        /// This is called by the build process thread to update the application with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender, BuildProgressEventArgs e)
        {
            builder.ReportProgress(e.BuildStep.ToString());
            lastBuildStep = e.BuildStep;
        }
        #endregion
    }
}
