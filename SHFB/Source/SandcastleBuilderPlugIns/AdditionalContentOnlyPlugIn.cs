//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalContentOnlyPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/30/2010
// Note    : Copyright 2007-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that can be used to build a help file
// consisting of nothing but additional content items.  It is also useful for
// proofreading your additional content without having to build all the API
// topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/13/2007  EFW  Created the code
// 1.6.0.7  05/27/2008  EFW  Modified to support use in conceptual preview
// 1.8.0.0  08/05/2008  EFW  Modified to support the new project format
// 1.9.0.0  06/07/2010  EFW  Added support for multi-format build output
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// This plug-in class can be used to build a help file consisting of
    /// nothing but additional content items.  It is also useful for
    /// proofreading your additional content without having to build all the
    /// API topics.
    /// </summary>
    public class AdditionalContentOnlyPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private ExecutionPointCollection executionPoints;

        private BuildProcess builder;
        private bool isPreviewBuild;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Additional Content Only"; }
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
                return "This plug-in can be used to build a help file " +
                    "consisting of nothing but conceptual content and/or " +
                    "additional content items.  It is also useful for " +
                    "proofreading your conceptual and/or additional content " +
                    "without having to build all the API topics.";
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
                        new ExecutionPoint(BuildStep.ValidatingDocumentationSources, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateApiFilter, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateNamespaceSummaries, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.ApplyVisibilityProperties, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateInheritedDocumentation, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.TransformReflectionInfo, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.ModifyHelpTopicFilenames, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.After),
                        new ExecutionPoint(BuildStep.BuildReferenceTopics, ExecutionBehaviors.InsteadOf),
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
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            MessageBox.Show("This plug-in has no configurable settings", "Additional Content Only Plug-In",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

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
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            builder.ReportProgress("{0} Version {1}\r\n{2}\r\n    This build will only include " +
                "additional content items.", this.Name, this.Version, this.Copyright);

            if(!builder.CurrentProject.HasItems(BuildAction.ContentLayout) &&
              !builder.CurrentProject.HasItems(BuildAction.SiteMap) &&
              !builder.CurrentProject.HasItems(BuildAction.Content))
                throw new BuilderException("ACP0001", "The Additional Content " +
                    "Only plug-in requires a conceptual content layout file, " +
                    "a site map file, or content items in the project.");

            // If doing a preview for conceptual content, suppress all the
            // steps that actually build the help file too.
            configuration.MoveToChild(XPathNodeType.All);

            if(configuration.GetAttribute("previewBuild", String.Empty) == "true")
            {
                isPreviewBuild = true;

                this.ExecutionPoints.AddRange(new [] {
                    new ExecutionPoint(BuildStep.GenerateIntermediateTableOfContents, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.CombiningIntermediateTocFiles, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.ExtractingHtmlInfo, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.GenerateHelpFormatTableOfContents, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.GenerateHelpFileIndex,  ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.GenerateHelpProject, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.CompilingHelpFile, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.GenerateFullTextIndex, ExecutionBehaviors.InsteadOf),
                    new ExecutionPoint(BuildStep.CopyingWebsiteFiles, ExecutionBehaviors.InsteadOf) });
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument config;
            XPathNavigator navConfig;
            List<XPathNavigator> deleteTargets = new List<XPathNavigator>();

            // Create a dummy reflection.org and reflection.xml file
            if(context.BuildStep == BuildStep.GenerateReflectionInfo)
            {
                // Allow Before step plug-ins to run
                builder.ExecuteBeforeStepPlugIns();

                using(StreamWriter sw = new StreamWriter(builder.ReflectionInfoFilename, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine("<reflection>");
                    sw.WriteLine("  <assemblies/>");
                    sw.WriteLine("  <apis>");
                    sw.WriteLine("    <api id=\"N:\"/>");
                    sw.WriteLine("  </apis>");
                    sw.WriteLine("</reflection>");
                }

                File.Copy(builder.ReflectionInfoFilename,
                    Path.ChangeExtension(builder.ReflectionInfoFilename, ".xml"), true);

                // Allow After step plug-ins to run
                builder.ExecuteAfterStepPlugIns();
            }
            else
                if(context.BuildStep == BuildStep.MergeCustomConfigs &&
                  builder.CurrentProject.HasItems(BuildAction.ContentLayout))
                {
                    // Remove the reflection.xml file from the conceptual
                    // configuration file since it isn't valid.
                    config = new XmlDocument();
                    config.Load(builder.WorkingFolder + "conceptual.config");
                    navConfig = config.CreateNavigator();

                    XPathNodeIterator allTargets = navConfig.Select("//targets[@files='reflection.xml']");

                    foreach(XPathNavigator target in allTargets)
                        deleteTargets.Add(target);

                    // Get rid of the framework targets too in preview build.
                    // This can knock about 20 seconds off the build time.
                    if(isPreviewBuild)
                    {
                        allTargets = navConfig.Select("//targets[contains(@base, 'Data\\Reflection')]");

                        foreach(XPathNavigator target in allTargets)
                            deleteTargets.Add(target);
                    }

                    foreach(var t in deleteTargets)
                        t.DeleteSelf();

                    config.Save(builder.WorkingFolder + "conceptual.config");
                }

            // Ignore all other the steps
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~AdditionalContentOnlyPlugIn()
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
    }
}
