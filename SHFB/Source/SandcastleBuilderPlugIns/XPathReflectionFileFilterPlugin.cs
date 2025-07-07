//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : XPathReflectionFileFilterPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Eyal Post
// Updated : 06/20/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a plug-in that is used to filter out unwanted information from the reflection information
// file using XPath queries.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/31/2008  EFW  Created the code
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

// Ignore Spelling: Eyal

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to filter out unwanted information from the reflection information file using
    /// XPath queries.
    /// </summary>
    [HelpFileBuilderPlugInExport("XPath Reflection File Filter", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright + "\r\nBased on code submitted by " +
      "Eyal Post", Description = "This plug-in is used to remove unwanted items from the reflection information " +
      "file using XPath queries.")]
    public sealed class XPathReflectionFileFilterPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private IBuildProcess builder;
        private List<string> expressions;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints { get; } =
        [
            // This one has a slightly higher priority as it removes stuff that the other plug-ins
            // don't need to see.
            new ExecutionPoint(BuildStep.ApplyDocumentModel, ExecutionBehaviors.Before, 1100)
        ];

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(IBuildProcess buildProcess, XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            expressions = [];

            foreach(var expr in configuration.Descendants("expression"))
                expressions.Add(expr.Value);

            if(expressions.Count == 0)
            {
                throw new BuilderException("XRF0001", "No queries have been defined for the XPath Reflection " +
                    "File Filter plug-in");
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument refInfo = new();

            refInfo.Load(builder.ReflectionInfoFilename);

            foreach(string expression in expressions)
            {
                builder.ReportProgress("Removing items matching '{0}'", expression);

                XmlNodeList nodes = refInfo.SelectNodes(expression);

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
