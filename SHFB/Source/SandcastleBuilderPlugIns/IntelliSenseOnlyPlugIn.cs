//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : IntelliSenseOnlyPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/17/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in that can be used to build an IntelliSense XML comments file without a related
// help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/17/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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
                        new ExecutionPoint(BuildStep.CombiningIntermediateTocFiles, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.ExtractingHtmlInfo, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CopyStandardHelpContent, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateHelpProject, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CompilingHelpFile, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.GenerateFullTextIndex, ExecutionBehaviors.InsteadOf),
                        new ExecutionPoint(BuildStep.CopyingWebsiteFiles, ExecutionBehaviors.InsteadOf)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XElement configuration)
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
            XPathNavigator deleteItem;

            if(context == null)
                throw new ArgumentNullException(nameof(context));

            if(context.BuildStep == BuildStep.MergeCustomConfigs)
            {
                builder.ReportProgress("Removing non-member topics from the build manifest...");

                XmlDocument config = new XmlDocument();
                string configFile = Path.Combine(builder.WorkingFolder, "manifest.xml");

                config.Load(configFile);

                XPathNavigator navConfig = config.CreateNavigator();
                XPathNavigator item = navConfig.SelectSingleNode("topics/topic");

                bool moreItems = true;

                while(item != null && moreItems)
                {
                    string id = item.GetAttribute("id", String.Empty);
                    string topicType = item.GetAttribute("type", String.Empty);

                    if(topicType != "API" || id.Length < 2 || id[1] != ':' || id[0] == 'G' || id[0] == 'N' ||
                      id[0] == 'R')
                    {
                        deleteItem = item.Clone();
                        moreItems = item.MoveToNext();
                        deleteItem.DeleteSelf();
                    }
                    else
                        moreItems = item.MoveToNext();
                }

                config.Save(configFile);

                builder.ReportProgress("Removing irrelevant build components from the configuration file...");

                configFile = Path.Combine(builder.WorkingFolder, "sandcastle.config");
                config = new XmlDocument();
                config.Load(configFile);
                navConfig = config.CreateNavigator();

                // Delete the MAML configuration component set if present
                item = navConfig.SelectSingleNode("//component[@id='Switch Component']/case[@value='MAML']");

                if(item != null)
                    item.DeleteSelf();

                // The IntelliSense build component must be there
                item = navConfig.SelectSingleNode("//component[@id='IntelliSense Component']");

                if(item == null)
                    throw new BuilderException("ISO0001", "The IntelliSense Only plug-in requires that the " +
                        "IntelliSense Component be added to the project and configured.");

                // Remove the Syntax Component
                item = navConfig.SelectSingleNode("//component[@id='Syntax Component']");

                if(item != null)
                    item.DeleteSelf();

                // Remove the Code Block Component
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

                config.Save(configFile);
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
