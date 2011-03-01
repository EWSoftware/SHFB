//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalReferenceLinksPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/23/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to add additional reference link
// targets to the Reflection Index Data and Resolve Reference Links build
// components so that links can be created to other third party help in a
// Help 2 collection.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/25/2008  EFW  Created the code
// 1.8.0.0  08/13/2008  EFW  Updated to support the new project format
// 1.9.0.0  06/07/2010  EFW  Added support for multi-format build output
//=============================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to add additional reference link
    /// targets to the <b>Reflection Index Data</b> and <b>Resolve Reference
    /// Links</b> build components so that links can be created to other third
    /// party help in a Help 2 collection.
    /// </summary>
    public class AdditionalReferenceLinksPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private ExecutionPointCollection executionPoints;

        private BuildProcess builder;
        private BuildStep lastBuildStep;

        // Plug-in configuration options
        private ReferenceLinkSettingsCollection otherLinks;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Additional Reference Links"; }
        }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        public Version Version
        {
            get
            {
                // Use the assembly version
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

                return new Version(fvi.ProductVersion);
            }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the
        /// plug-in.
        /// </summary>
        public string Copyright
        {
            get
            {
                // Use the assembly copyright
                Assembly asm = Assembly.GetExecutingAssembly();
                AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                    asm, typeof(AssemblyCopyrightAttribute));

                return copyright.Copyright;
            }
        }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        public string Description
        {
            get
            {
                return "This plug-in is used to add additional reference " +
                    "link targets to the Reflection Index Data and Resolve " +
                    "Reference Links build component so that links can be " +
                    "created to other third party help in a Help 2 " +
                    "collection or additional online MSDN content.";
            }
        }

        /// <summary>
        /// This plug-in does not run in partial builds
        /// </summary>
        public bool RunsInPartialBuild
        {
            get { return false; }
        }

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public ExecutionPointCollection ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new ExecutionPointCollection
                    {
                        new ExecutionPoint(BuildStep.ApplyVisibilityProperties, ExecutionBehaviors.After),
                        new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.After),
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project,
          string currentConfig)
        {
            using(AdditionalReferenceLinksConfigDlg dlg = new AdditionalReferenceLinksConfigDlg(project, currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in
        /// configuration is not valid.</exception>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            XPathNavigator root;

            builder = buildProcess;
            otherLinks = new ReferenceLinkSettingsCollection();

            builder.ReportProgress("{0} Version {1}\r\n{2}", this.Name, this.Version, this.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("ARL0001", "The Additional Reference " +
                    "Links plug-in has not been configured yet");

            // Load the reference links settings
            otherLinks.FromXml(buildProcess.CurrentProject, root);

            if(otherLinks.Count == 0)
                throw new BuilderException("ARL0002", "At least one target is " +
                    "required for the Additional Reference Links plug-in.");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(Utils.PlugIn.ExecutionContext context)
        {
            StringBuilder sb = new StringBuilder(1024);
            string workingPath, configFilename;
            bool success;

            if(context.BuildStep == BuildStep.ApplyVisibilityProperties)
            {
                // Merge the additional reference links information
                builder.ReportProgress("Performing partial builds on additional targets' projects");

                // Build each of the projects
                foreach(ReferenceLinkSettings vs in otherLinks)
                {
                    using(SandcastleProject project = new SandcastleProject(vs.HelpFileProject, true))
                    {
                        // We'll use a working folder below the current project's working folder
                        workingPath = builder.WorkingFolder + vs.HelpFileProject.GetHashCode().ToString("X",
                            CultureInfo.InvariantCulture) + "\\";

                        success = this.BuildProject(project, workingPath);

                        // Switch back to the original folder for the current project
                        Directory.SetCurrentDirectory(builder.ProjectFolder);

                        if(!success)
                            throw new BuilderException("ARL0003", "Unable to build " +
                                "additional target project: " + project.Filename);
                    }

                    // Save the reflection file location as we need it later
                    vs.ReflectionFilename = workingPath + "reflection.xml";

                    sb.AppendFormat("\"{0}ProductionTools\\XslTransform\" " +
                        "/xsl:\"{0}ProductionTransforms\\ApplyVSDocModel.xsl\"," +
                        "\"{0}ProductionTransforms\\AddGuidFilenames.xsl\" " +
                        "\"{1}reflection.org\" /out:\"{1}reflection.xml\" " +
                        "/arg:IncludeAllMembersTopic=true " +
                        "/arg:IncludeInheritedOverloadTopics=true\r\n",
                        builder.SandcastleFolder, workingPath);
                }

                // Run the Transform step on the reflection.org files.  This
                // adds some stuff the build components need.
                sb.Insert(0, "@ECHO OFF\r\n");
                workingPath = builder.WorkingFolder + "TransformReferences.bat";

                using(StreamWriter sw = new StreamWriter(workingPath))
                {
                    sw.Write(sb.ToString());
                }

                builder.ReportProgress("\r\nTransforming reference reflection files...");
                builder.RunProcess(workingPath, null);
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
        /// This is used to merge the reflection file info into the named
        /// configuration file.
        /// </summary>
        /// <param name="configFilename">The configuration filename</param>
        /// <param name="isConceptual">True if it is the conceptual
        /// configuration file or false if it is the reference configuration
        /// file.</param>
        private void MergeReflectionInfo(string configFilename, bool isConceptual)
        {
            XmlDocument configFile;
            XmlAttribute attr;
            XmlNodeList matchingComponents;
            XmlNode component, target;
            HelpFileFormat helpFormat;

            builder.ReportProgress("\r\nAdding references to {0}...", configFilename);
            configFile = new XmlDocument();
            configFile.Load(configFilename);

            if(!isConceptual)
            {
                // Add them to the Reflection Index Data component.  There are
                // multiple copies of this component type but we only need the
                // first one.  This only appears in the reference build's
                // configuration file.
                component = configFile.SelectSingleNode("configuration/dduetools/builder/components/component[@type=" +
                    "'Microsoft.Ddue.Tools.CopyFromIndexComponent']/index");

                // If not found, try for the cached version
                if(component == null)
                    component = configFile.SelectSingleNode("configuration/dduetools/builder/components/component[" +
                        "@id='Cached Reflection Index Data']/index");

                if(component == null)
                    throw new BuilderException("ARL0004", "Unable to locate " +
                        "Reflection Index Data component in " + configFilename);

                foreach(ReferenceLinkSettings vs in otherLinks)
                    if(!String.IsNullOrEmpty(vs.ReflectionFilename))
                    {
                        target = configFile.CreateElement("data");

                        attr = configFile.CreateAttribute("files");
                        attr.Value = vs.ReflectionFilename;
                        target.Attributes.Append(attr);

                        // Keep the current project's stuff listed last so that
                        // it takes precedence.
                        component.InsertAfter(target, component.ChildNodes[0]);
                    }
            }

            // Add them to the Resolve Reference Links component
            matchingComponents = configFile.SelectNodes(
                "//component[@type='Microsoft.Ddue.Tools.ResolveReferenceLinksComponent2']");

            // If not found, try for the cached version
            if(matchingComponents.Count == 0)
                matchingComponents = configFile.SelectNodes("//component[@id='Cached MSDN URL References']");

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

                        helpFormat = (HelpFileFormat)Enum.Parse(typeof(HelpFileFormat),
                            match.ParentNode.Attributes["format"].Value, true);

                        switch(helpFormat)
                        {
                            case HelpFileFormat.HtmlHelp1:
                                attr.Value = vs.HtmlSdkLinkType.ToString();
                                break;

                            case HelpFileFormat.MSHelp2:
                                attr.Value = vs.MSHelp2SdkLinkType.ToString();
                                break;

                            case HelpFileFormat.MSHelpViewer:
                                attr.Value = vs.MSHelpViewerSdkLinkType.ToString();
                                break;

                            default:
                                attr.Value = vs.WebsiteSdkLinkType.ToString();
                                break;
                        }

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
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~AdditionalReferenceLinksPlugIn()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the plug-in object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own
        /// disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed
        /// and unmanaged resources or false to just dispose of the
        /// unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose of in this one
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
                project.SandcastlePath = new FolderPath(builder.SandcastleFolder, true, project);
                project.HtmlHelp1xCompilerPath = new FolderPath(builder.Help1CompilerFolder, true, project);
                project.HtmlHelp2xCompilerPath = new FolderPath(builder.Help2CompilerFolder, true, project);
                project.WorkingPath = new FolderPath(workingPath, true, project);
                project.OutputPath = new FolderPath(workingPath + @"..\PartialBuildLog\", true, project);

                buildProcess = new BuildProcess(project, true);

                buildProcess.BuildStepChanged += buildProcess_BuildStepChanged;

                // Since this is a plug-in, we'll run it directly rather
                // than in a background thread.
                buildProcess.Build();

                // Add the list of the comments files in the other project to
                // this build.
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
        /// This is called by the build process thread to update the
        /// application with the current build step.
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
