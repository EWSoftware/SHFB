//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : XPathReflectionFileFilterPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Eyal Post
// Updated : 08/19/2014
// Note    : Copyright 2008-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that is used to filter out unwanted information from the reflection information
// file using XPath queries.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.8.0.1  10/31/2008  EFW  Created the code
// -------  12/17/2013  EFW  Updated to use MEF for the plug-ins
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
    /// This plug-in class is used to filter out unwanted information from the reflection information file using
    /// XPath queries.
    /// </summary>
    [HelpFileBuilderPlugInExport("XPath Reflection File Filter", IsConfigurable = true, RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright + "\r\nBased on code submitted by " +
      "Eyal Post",
      Description = "This plug in is used to remove unwanted items from the reflection information file using " +
      "XPath queries.")]
    public sealed class XPathReflectionFileFilterPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private List<string> expressions;
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
                        // This one has a slightly higher priority as it removes stuff that the other plug-ins
                        // don't need to see.
                        new ExecutionPoint(BuildStep.TransformReflectionInfo, ExecutionBehaviors.Before, 1100)
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
            using(XPathReflectionFileFilterConfigDlg dlg = new XPathReflectionFileFilterConfigDlg(currentConfig))
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
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            expressions = new List<string>();

            foreach(XPathNavigator nav in configuration.Select("configuration/expressions/expression"))
                expressions.Add(nav.InnerXml);

            if(expressions.Count == 0)
                throw new BuilderException("XRF0001", "No queries have been defined for the XPath Reflection " +
                    "File Filter plug-in");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument refInfo = new XmlDocument();
            XmlNodeList nodes;

            refInfo.Load(builder.ReflectionInfoFilename);

            foreach(string expression in expressions)
            {
                builder.ReportProgress("Removing items matching '{0}'", expression);

                nodes = refInfo.SelectNodes(expression);

                foreach(XmlNode node in nodes)
                    node.ParentNode.RemoveChild(node);

                builder.ReportProgress("    Removed {0} items", nodes.Count);
            }

            refInfo.Save(builder.ReflectionInfoFilename);
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
