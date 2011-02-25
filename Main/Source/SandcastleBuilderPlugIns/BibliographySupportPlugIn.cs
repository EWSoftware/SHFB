//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BibliographySupportPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/07/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that is used to add bibliography support to the
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
// 1.8.0.1  11/07/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to add bibliography support to the topics
    /// </summary>
    public class BibliographySupportPlugIn : SandcastleBuilder.Utils.PlugIn.IPlugIn
    {
        #region Private data members
        private ExecutionPointCollection executionPoints;
        private BuildProcess builder;

        private string bibliographyFile;
        #endregion

        #region IPlugIn implementation
        //=====================================================================
        // IPlugIn implementation

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Bibliography Support"; }
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
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(
                    asm.Location);

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
                AssemblyCopyrightAttribute copyright =
                    (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
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
                return "This plug in is used to add bibliography support " +
                    "to the help file topics.";
            }
        }

        /// <summary>
        /// This plug-in runs in partial builds
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
                {
                    executionPoints = new ExecutionPointCollection();

                    executionPoints.Add(new ExecutionPoint(
                        BuildStep.MergeCustomConfigs,
                        ExecutionBehaviors.After));
                }

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
            using(BibliographySupportConfigDlg dlg =
              new BibliographySupportConfigDlg(currentConfig))
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
        public void Initialize(BuildProcess buildProcess,
          XPathNavigator configuration)
        {
            XPathNavigator root, node;

            builder = buildProcess;

            builder.ReportProgress("{0} Version {1}\r\n{2}\r\n",
                this.Name, this.Version, this.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("BIB0001", "The Bibliography " +
                    "support plug-in has not been configured yet");

            node = root.SelectSingleNode("bibliography");
            if(node != null)
                bibliographyFile = node.GetAttribute("path", String.Empty).Trim();

            if(String.IsNullOrEmpty(bibliographyFile))
                throw new BuilderException("BIB0002", "A path to the " +
                    "bibliography file is required");

            // If relative, the path is relative to the project folder
            bibliographyFile = FilePath.RelativeToAbsolutePath(
                builder.ProjectFolder, builder.TransformText(bibliographyFile));

            if(!File.Exists(bibliographyFile))
                throw new BuilderException("BIB0003", "Unable to locate " +
                    "bibliography file at " + bibliographyFile);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            string configFilename;

            // Merge the reflection file info into conceptual.config
            configFilename = builder.WorkingFolder + "conceptual.config";

            if(File.Exists(configFilename))
                this.AddBibliographyParameter(configFilename);

            // Merge the reflection file info into sancastle.config
            configFilename = builder.WorkingFolder + "sandcastle.config";

            if(File.Exists(configFilename))
                this.AddBibliographyParameter(configFilename);
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// Add the bibliography file parameter to the TransformComponent
        /// configuration.
        /// </summary>
        /// <param name="configFilename">The BuildAssembler configuration file
        /// to modify</param>
        private void AddBibliographyParameter(string configFilename)
        {
            XmlDocument configFile;
            XmlAttribute attr;
            XmlNode transform, argument;

            builder.ReportProgress("\r\nAdding bibliography parameter to {0}...",
                configFilename);
            configFile = new XmlDocument();
            configFile.Load(configFilename);


            // To configure Sandcastle, find the main XSLT component
            // (Microsoft.Ddue.Tools.TransformComponent) in the configuration
            // file and add a new argument to it:
            // <argument key='bibliographyData' value='../Data/bibliography.xml' />
            // Update sandcastle.config and conceptual.config if it exists.

            transform = configFile.SelectSingleNode(
                "configuration/dduetools/builder/components/component[" +
                "@type='Microsoft.Ddue.Tools.TransformComponent']/transform");

            if(transform == null)
                throw new BuilderException("BIB0004", "Unable to locate " +
                    "TransformComponent configuration in " + configFilename);

            argument = configFile.CreateElement("argument");

            attr = configFile.CreateAttribute("key");
            attr.Value = "bibliographyData";
            argument.Attributes.Append(attr);

            attr = configFile.CreateAttribute("value");
            attr.Value = bibliographyFile;
            argument.Attributes.Append(attr);

            transform.AppendChild(argument);

            configFile.Save(configFilename);
        }
        #endregion


        #region IDisposable implementation
        //=====================================================================
        // IDisposable implementation

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~BibliographySupportPlugIn()
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
