//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalContentOnlyPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/22/2015
// Note    : Copyright 2007-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that can be used to build a help file consisting of nothing but additional
// content items.  It is also useful for proofreading your additional content without having to build all the
// API topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/13/2007  EFW  Created the code
// 05/27/2008  EFW  Modified to support use in conceptual preview
// 08/05/2008  EFW  Modified to support the new project format
// 06/07/2010  EFW  Added support for multi-format build output
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
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
    /// This plug-in class can be used to build a help file consisting of nothing but additional content items.
    /// It is also useful for proofreading your additional content without having to build all the API topics.
    /// </summary>
    [HelpFileBuilderPlugInExport("Additional Content Only", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in can be used to build a help file " +
      "consisting of nothing but conceptual content and/or additional content items.  It is also useful for " +
      "proofreading your conceptual and/or additional content without having to build all the API topics.")]
    public sealed class AdditionalContentOnlyPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;
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
                        new ExecutionPoint(BuildStep.ValidatingDocumentationSources, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateApiFilter, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateNamespaceSummaries, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.TransformReflectionInfo, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateInheritedDocumentation, ExecutionBehaviors.InsteadOf),
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
        /// <remarks>The configuration data will be stored in the help file builder project</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            MessageBox.Show("This plug-in has no configurable settings", "Additional Content Only Plug-In",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}\r\n    This build will only include additional " +
                "content items.", metadata.Id, metadata.Version, metadata.Copyright);

            if(!builder.CurrentProject.HasItems(BuildAction.ContentLayout) &&
              !builder.CurrentProject.HasItems(BuildAction.SiteMap) &&
              !builder.CurrentProject.HasItems(BuildAction.Content))
                throw new BuilderException("ACP0001", "The Additional Content Only plug-in requires a " +
                    "conceptual content layout file, a site map file, or content items in the project.");
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

                File.Copy(builder.ReflectionInfoFilename, Path.ChangeExtension(builder.ReflectionInfoFilename,
                    ".xml"), true);

                // Allow After step plug-ins to run
                builder.ExecuteAfterStepPlugIns();
            }
            else
                if(context.BuildStep == BuildStep.MergeCustomConfigs &&
                  builder.CurrentProject.HasItems(BuildAction.ContentLayout))
                {
                    // Remove the reflection.xml file from the conceptual configuration since it isn't valid
                    config = new XmlDocument();
                    config.Load(builder.WorkingFolder + "sandcastle.config");
                    navConfig = config.CreateNavigator();

                    XPathNodeIterator allTargets = navConfig.Select("//targets[@files='reflection.xml']");

                    foreach(XPathNavigator target in allTargets)
                        deleteTargets.Add(target);

                    foreach(var t in deleteTargets)
                        t.DeleteSelf();

                    config.Save(builder.WorkingFolder + "sandcastle.config");
                }

            // Ignore all other the steps
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
    }
}
