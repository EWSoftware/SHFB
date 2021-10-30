//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BibliographySupportPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in that is used to add bibliography support to the topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/07/2008  EFW  Created the code
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
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
    /// This plug-in class is used to add bibliography support to the topics
    /// </summary>
    [HelpFileBuilderPlugInExport("Bibliography Support", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in is used to add bibliography support to " +
        "the help file topics.")]
    public sealed class BibliographySupportPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private string bibliographyFile;

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
                        new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.After)
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
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            // If the file format is Open XML, this plug-in is not supported and will not run
            if((builder.CurrentProject.HelpFileFormat & HelpFileFormats.OpenXml) != 0)
            {
                builder.ReportWarning("BIP0005", "The bibliography plug-in is not supported in the Open XML " +
                    "file format and will not run.");
                executionPoints = new List<ExecutionPoint>();
                return;
            }

            if(configuration.IsEmpty)
                throw new BuilderException("BIP0001", "The Bibliography support plug-in has not been configured yet");

            var node = configuration.Element("bibliography");

            if(node != null)
                bibliographyFile = node.Attribute("path").Value;

            if(String.IsNullOrWhiteSpace(bibliographyFile))
                throw new BuilderException("BIP0002", "A path to the bibliography file is required");

            // If relative, the path is relative to the project folder
            bibliographyFile = FilePath.RelativeToAbsolutePath(builder.ProjectFolder,
                builder.SubstitutionTags.TransformText(bibliographyFile));

            if(!File.Exists(bibliographyFile))
                throw new BuilderException("BIP0003", "Unable to locate bibliography file at " + bibliographyFile);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            string configFilename = Path.Combine(builder.WorkingFolder, "sandcastle.config");

            if(!File.Exists(configFilename))
                return;

            builder.ReportProgress("\r\nAdding bibliography parameter to {0}...", configFilename);

            var configFile = XDocument.Load(configFilename);

            // Find the XSL Transform Components in the configuration file and add a new argument to them:
            // <argument key="bibliographyData" value="C:\Path\To\bibliography.xml" />
            var components = configFile.XPathSelectElements("//component[@id='XSL Transform Component']/transform").ToList();

            if(components.Count == 0)
            {
                throw new BuilderException("BIP0004", "Unable to locate XSL Transform Component configuration in " +
                    configFilename);
            }

            foreach(var transform in components)
            {
                transform.Add(new XElement("argument",
                    new XAttribute("key", "bibliographyData"),
                    new XAttribute("value", bibliographyFile)));
            }

            configFile.Save(configFilename);
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
