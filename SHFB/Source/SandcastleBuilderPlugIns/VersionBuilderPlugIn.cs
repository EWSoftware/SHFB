//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : VersionBuilderPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/05/2014
// Note    : Copyright 2007-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to generate version information for assemblies in the current project
// and others related to the same product that can be merged into the current project's help file topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.3  12/01/2007  EFW  Created the code
// 1.8.0.0  08/13/2008  EFW  Updated to support the new project format
// 1.9.0.0  06/27/2010  EFW  Added support for /rip option
// -------  12/17/2013  EFW  Updated to use MEF for the plug-ins
//          12/28/2013  EFW  Updated to run VersionBuilder tool as an MSBuild task in GenerateRefInfo.proj
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to generate version information for assemblies in the current project and
    /// others related to the same product that can be merged into the current project's help file topics.
    /// </summary>
    [HelpFileBuilderPlugInExport("Version Builder", IsConfigurable = true, RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to generate version information for the current project and others " +
        "related to the same product and merge that information into a single help file for all of them.")]
    public sealed class VersionBuilderPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;
        private BuildStep lastBuildStep;

        // Plug-in configuration options
        private VersionSettings currentVersion;
        private VersionSettingsCollection allVersions;
        private List<string> uniqueLabels;
        private bool ripOldApis;

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
                        new ExecutionPoint(BuildStep.GenerateSharedContent, ExecutionBehaviors.After),
                        new ExecutionPoint(BuildStep.TransformReflectionInfo, ExecutionBehaviors.Before)
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
        /// <remarks>The configuration data will be stored in the help file builder project</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            using(VersionBuilderConfigDlg dlg = new VersionBuilderConfigDlg(project, currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in configuration is not valid</exception>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            XPathNavigator root, node;
            string ripOld;

            builder = buildProcess;
            allVersions = new VersionSettingsCollection();
            uniqueLabels = new List<string>();

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("VBP0001", "The Version Builder plug-in has not been configured yet");

            // Add an element for the current project.  This one won't have a project to build.
            currentVersion = new VersionSettings();
            allVersions.Add(currentVersion);

            node = root.SelectSingleNode("currentProject");
            if(node != null)
            {
                currentVersion.FrameworkLabel = node.GetAttribute("label", String.Empty).Trim();
                currentVersion.Version = node.GetAttribute("version", String.Empty).Trim();
                currentVersion.RipOldApis = false;

                ripOld = node.GetAttribute("ripOldApis", String.Empty);

                // This wasn't in older versions
                if(!String.IsNullOrEmpty(ripOld))
                    ripOldApis = Convert.ToBoolean(ripOld, CultureInfo.InvariantCulture);
            }

            allVersions.FromXml(builder.CurrentProject, root);

            // An empty label messes up the HTML so use a single space if it's blank
            if(String.IsNullOrEmpty(currentVersion.FrameworkLabel))
                currentVersion.FrameworkLabel = " ";

            if(node == null)
                throw new BuilderException("VBP0002", "A version value is required for the Version Builder plug-in");

            if(allVersions.Count == 1)
                builder.ReportProgress("No other version information was supplied.  Only version information " +
                    "for the documented assemblies will be included.");

            foreach(VersionSettings vs in allVersions)
                if(!uniqueLabels.Contains(vs.FrameworkLabel))
                    uniqueLabels.Add(vs.FrameworkLabel);

            uniqueLabels.Sort();
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            string workingPath;
            bool success;

            // Update shared content version items
            if(context.BuildStep == BuildStep.GenerateSharedContent)
            {
                this.UpdateVersionItems();
                return;
            }

            // Set the current version's reflection info filename and sort the collection so that the versions
            // are in ascending order.
            currentVersion.ReflectionFilename = builder.ReflectionInfoFilename;
            allVersions.Sort();

            // Merge the version information
            builder.ReportProgress("\r\nPerforming partial builds on prior version projects");

            // Build each of the projects
            foreach(VersionSettings vs in allVersions)
            {
                // Not needed for current project
                if(vs.HelpFileProject == null)
                    continue;

                using(SandcastleProject tempProject = new SandcastleProject(vs.HelpFileProject, true))
                {
                    // Set the configuration and platform here so that they are evaluated property in project
                    // properties when the project is loaded below.
                    tempProject.Configuration = builder.CurrentProject.Configuration;
                    tempProject.Platform = builder.CurrentProject.Platform;

                    // This looks odd but is necessary.  If we are in Visual Studio, the above constructor may
                    // return an instance that uses an underlying MSBuild project loaded in Visual Studio.
                    // Since the BuildProject() method modifies the project, those changes are propagated to the
                    // Visual Studio copy which we do not want to happen.  As such, we use this constructor to
                    // clone the MSBuild project XML thus avoiding modifications to the original project.
                    using(SandcastleProject project = new SandcastleProject(tempProject))
                    {
                        // We'll use a working folder below the current project's working folder
                        workingPath = builder.WorkingFolder + vs.HelpFileProject.GetHashCode().ToString("X",
                            CultureInfo.InvariantCulture) + "\\";

                        success = this.BuildProject(project, workingPath);

                        // Switch back to the original folder for the current project
                        Directory.SetCurrentDirectory(builder.ProjectFolder);

                        if(!success)
                            throw new BuilderException("VBP0003", "Unable to build prior version project: " +
                                project.Filename);
                    }
                }

                // Save the reflection file location as we need it later
                vs.ReflectionFilename = workingPath + "reflection.org";
            }

            // Create the Version Builder configuration and add the parameters to the transform project
            this.CreateVersionBuilderConfigurationFile();
            this.ModifyTransformManifestProject();
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
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
        /// Update the version information items in the shared builder content file
        /// </summary>
        /// <remarks>Remove the standard version information items from the shared content file as the version
        /// builder information will take its place in the topics.  New items are added for each version of the
        /// project defined in the configuration settings.</remarks>
        private void UpdateVersionItems()
        {
            XmlAttribute attr;
            XmlDocument sharedContent;
            XmlNode root, node;
            string sharedContentFilename, hashValue;
            List<string> uniqueVersions = new List<string>();

            builder.ReportProgress("Removing standard version information items from shared content file");

            sharedContentFilename = builder.WorkingFolder + "SHFBContent.xml";
            sharedContent = new XmlDocument();
            sharedContent.Load(sharedContentFilename);

            root = sharedContent.SelectSingleNode("content");

            node = root.SelectSingleNode("item[@id='locationInformation']");

            if(node != null)
                root.RemoveChild(node);

            node = root.SelectSingleNode("item[@id='assemblyNameAndModule']");

            if(node != null)
                root.RemoveChild(node);

            builder.ReportProgress("Adding version information items from plug-in settings");

            // Add items for each framework label
            foreach(string label in uniqueLabels)
            {
                // We need to use a hash value as this ends up as an XML attribute name in the reflection
                // data file and this ensures it only contains valid characters.
                hashValue = label.GetHashCode().ToString("X", CultureInfo.InvariantCulture);

                // Label item
                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");
                attr.Value = "SHFB_VBPI_Lbl_" + hashValue;
                node.Attributes.Append(attr);

                // Empty strings mess up the HTML so use a single space if blank
                node.InnerText = String.IsNullOrEmpty(label) ? " " : label;
                root.AppendChild(node);

                // Framework menu labels
                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");
                attr.Value = "memberFrameworksSHFB_VBPI_Lbl_" + hashValue;
                node.Attributes.Append(attr);
                node.InnerText = String.IsNullOrEmpty(label) ? " " : label;
                root.AppendChild(node);

                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");
                attr.Value = "IncludeSHFB_VBPI_Lbl_" + hashValue + "Members";
                node.Attributes.Append(attr);
                node.InnerText = String.IsNullOrEmpty(label) ? " " : label;
                root.AppendChild(node);
            }

            // Write out a label for each framework and version
            foreach(VersionSettings vs in allVersions)
            {
                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");

                // We need to use a hash value as this ends up as an XML attribute name in the reflection
                // data file and this ensures it only contains valid characters.
                attr.Value = "SHFB_VBPI_" + (vs.FrameworkLabel + " " + vs.Version).GetHashCode().ToString("X",
                    CultureInfo.InvariantCulture);

                node.Attributes.Append(attr);
                node.InnerText = vs.Version;
                root.AppendChild(node);
            }

            sharedContent.Save(sharedContentFilename);
        }

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

                buildProcess = new BuildProcess(project, PartialBuildType.GenerateReflectionInfo);

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
                throw new BuilderException("VBP0004", String.Format(CultureInfo.InvariantCulture,
                    "Fatal error, unable to compile project '{0}': {1}", project.Filename, ex.ToString()));
            }

            return (lastBuildStep == BuildStep.Completed);
        }

        /// <summary>
        /// This is called by the build process thread to update the application with the current build step
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender, BuildProgressEventArgs e)
        {
            builder.ReportProgress(e.BuildStep.ToString());
            lastBuildStep = e.BuildStep;
        }

        /// <summary>
        /// This creates the Version Builder configuration file
        /// </summary>
        private void CreateVersionBuilderConfigurationFile()
        {
            StringBuilder config = new StringBuilder(4096);

            builder.ReportProgress("Creating Version Builder configuration file");

            config.Append("<versions>\r\n");

            // Write out a <versions> element for each unique label that contains info for each related version.
            // We also copy the reflection files to unique names as we will create a new reflection.org file that
            // contains everything.
            foreach(string label in uniqueLabels)
            {
                config.AppendFormat("  <versions name=\"SHFB_VBPI_Lbl_{0:X}\">\r\n", label.GetHashCode());

                // Add info for each related version
                foreach(VersionSettings vs in allVersions)
                    if(vs.FrameworkLabel == label)
                    {
                        config.AppendFormat("    <version name=\"SHFB_VBPI_{0}\" file=\"{1:X}.ver\" ripOldApis=\"{2}\" />\r\n",
                            (vs.FrameworkLabel + " " + vs.Version).GetHashCode().ToString("X",
                            CultureInfo.InvariantCulture), vs.GetHashCode(), vs.RipOldApis);

                        File.Copy(vs.ReflectionFilename, Path.Combine(builder.WorkingFolder,
                            String.Format(CultureInfo.InvariantCulture, "{0:X}.ver", vs.GetHashCode())), true);
                    }

                config.Append("  </versions>\r\n");
            }

            config.Append("</versions>\r\n");

            // Save the file
            using(StreamWriter sw = new StreamWriter(builder.WorkingFolder + "VersionBuilder.config"))
            {
                sw.Write(config.ToString());
            }
        }

        /// <summary>
        /// This is used to modify the GenerateRefInfo.proj file for use with VersionBuilder
        /// </summary>
        private void ModifyTransformManifestProject()
        {
            XmlNamespaceManager nsm;
            XmlDocument project;
            XmlNode property;
            string projectFile;

            projectFile = builder.WorkingFolder + "TransformManifest.proj";

            // If the project doesn't exist we have nothing to do.  However, it could be that some other plug-in
            // has bypassed it so only issue a warning.
            if(!File.Exists(projectFile))
            {
                builder.ReportWarning("VBP0005", "The transform manifest project '{0}' could not be found.  " +
                    "The Version Builder plug-in did not run successfully.", projectFile);
                return;
            }

            builder.ReportProgress("Adding Version Builder parameters to TransformManifest.proj");

            project = new XmlDocument();
            project.Load(projectFile);
            nsm = new XmlNamespaceManager(project.NameTable);
            nsm.AddNamespace("MSBuild", project.DocumentElement.NamespaceURI);

            property = project.SelectSingleNode("//MSBuild:VersionBuilderConfigurationFile", nsm);

            if(property == null)
                throw new BuilderException("VBP0006", "Unable to locate Version Builder property: " +
                    "VersionBuilderConfigurationFile");

            property.InnerText = Path.Combine(builder.WorkingFolder, "VersionBuilder.config");

            property = project.SelectSingleNode("//MSBuild:RipOldApis", nsm);

            if(property == null)
                throw new BuilderException("VBP0006", "Unable to locate Version Builder property: RipOldApis");

            property.InnerText = ripOldApis.ToString().ToLowerInvariant();

            project.Save(projectFile);
        }
        #endregion
    }
}
