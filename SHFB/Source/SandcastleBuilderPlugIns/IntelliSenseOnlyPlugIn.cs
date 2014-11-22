//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : IntelliSenseOnlyPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/17/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that can be used to build an IntelliSense XML comments file without a related
// help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// =====================================================================================================
// 11/17/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class can be used to build an IntelliSense XML comments file without a related help file
    /// </summary>
    [HelpFileBuilderPlugInExport("IntelliSense Only", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in can be used to build IntelliSense XML " +
      "comments files without a related help file.  This results in a faster build for support projects that " +
      "do not need a separate help file but do need IntelliSense XML comments files.")]
    public sealed class IntelliSenseOnlyPlugIn : IPlugIn
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
                        // This one has a lower priority since we want to update the manifest and reference
                        // build configuration file last.
                        new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.After, 100),
                        new ExecutionPoint(BuildStep.BuildConceptualTopics, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CombiningIntermediateTocFiles, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.ExtractingHtmlInfo, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CopyStandardHelpContent, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateHelpFormatTableOfContents, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateHelpFileIndex, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateHelpProject, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CompilingHelpFile, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateFullTextIndex, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CopyingWebsiteFiles, ExecutionBehaviors.InsteadOf)
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
            MessageBox.Show("This plug-in has no configurable settings", "IntelliSense Only Plug-In",
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

            builder.ReportProgress("{0} Version {1}\r\n{2}\r\n    This build will only generate IntelliSense " +
                "comments files.", metadata.Id, metadata.Version, metadata.Copyright);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument config;
            XPathNavigator navConfig, item, deleteItem;
            bool moreItems = true;
            string id;

            if(context.BuildStep == BuildStep.MergeCustomConfigs)
            {
                builder.ReportProgress("Removing non-member topics from the build manifest...");

                config = new XmlDocument();
                config.Load(builder.WorkingFolder + "manifest.xml");
                navConfig = config.CreateNavigator();

                item = navConfig.SelectSingleNode("topics/topic");

                while(item != null && moreItems)
                {
                    id = item.GetAttribute("id", String.Empty);

                    if(id.Length < 2 || id[1] != ':' || id[0] == 'R' || id[0] == 'G')
                    {
                        deleteItem = item.Clone();
                        moreItems = item.MoveToNext();
                        deleteItem.DeleteSelf();
                    }
                    else
                        moreItems = item.MoveToNext();
                }

                config.Save(builder.WorkingFolder + "manifest.xml");

                builder.ReportProgress("Removing irrelevant build components from the configuration file...");
                config = new XmlDocument();
                config.Load(builder.WorkingFolder + "sandcastle.config");
                navConfig = config.CreateNavigator();

                // The IntalliSense build component must be there
                item = navConfig.SelectSingleNode("//component[@id='IntelliSense Component']");

                if(item == null)
                    throw new BuilderException("ISO0001", "The IntelliSense Only plug-in requires that the " +
                        "IntelliSense Component be added to the project and configured.");

                // Remove Syntax Component
                item = navConfig.SelectSingleNode("//component[@id='Syntax Component']");

                if(item != null)
                    item.DeleteSelf();

                // Remove Code Block Component
                item = navConfig.SelectSingleNode("//component[@id='Code Block Component']");

                if(item != null)
                    item.DeleteSelf();

                // Remove the XSL Transform Component and everything after it
                item = navConfig.SelectSingleNode("//component[@id='XSL Transform Component']");
                moreItems = true;

                while(item != null && moreItems)
                {
                    deleteItem = item.Clone();
                    moreItems = item.MoveToNext();
                    deleteItem.DeleteSelf();
                }

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
