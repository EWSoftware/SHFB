//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : WildcardReferencesPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/17/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to modify the MRefBuilder project
// file by adding in reference assemblies matching wildcard search paths.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.2.0  01/17/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.XPath;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to modify the MRefBuilder project
    /// file by adding in reference assemblies matching wildcard search paths.
    /// </summary>
    public class WildcardReferencesPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private ExecutionPointCollection executionPoints;

        private BuildProcess builder;

        // Plug-in configuration options
        private WildcardReferenceSettingsCollection referencePaths;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Wildcard Assembly References"; }
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
                return "This plug-in is used to modify the Generate Refelection Information build step by " +
                    "adding assembly references found in one or more wildcard search paths.";
            }
        }

        /// <summary>
        /// This plug-in runs in partial builds
        /// </summary>
        public bool RunsInPartialBuild
        {
            get { return true; }
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

                    executionPoints.Add(new ExecutionPoint(BuildStep.GenerateReflectionInfo,
                        ExecutionBehaviors.Before));
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
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            using(WildcardReferencesConfigDlg dlg = new WildcardReferencesConfigDlg(project, currentConfig))
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
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            builder.ReportProgress("{0} Version {1}\r\n{2}", this.Name, this.Version, this.Copyright);

            XElement root = XElement.Parse(configuration.OuterXml);

            if(root.IsEmpty)
                throw new BuilderException("WRP0001", "The Wildcard References plug-in has not been configured yet");

            // Load the reference links settings
            referencePaths = new WildcardReferenceSettingsCollection();
            referencePaths.FromXml(buildProcess.CurrentProject, root);

            if(referencePaths.Count == 0)
                throw new BuilderException("WRP0002", "At least one reference path is required for the " +
                    "Wildcard References plug-in.");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(Utils.PlugIn.ExecutionContext context)
        {
            Project msBuildProject = null;
            ProjectItem projectItem;
            SearchOption searchOpts;
            Dictionary<string, string> assemblies = new Dictionary<string, string>();

            string filename, projectFile = builder.WorkingFolder + "GenerateRefInfo.proj";

            // If the project doesn't exist we have nothing to do.  However, it could be that some other
            // plug-in has bypassed it so only issue a warning.
            if(!File.Exists(projectFile))
            {
                builder.ReportWarning("WRP0003", "The reflection information generation project '{0}' could " +
                    "not be found.  The Wildcard References plug-in did not run.", projectFile);
                return;
            }

            // Find all unique references
            foreach(var r in referencePaths)
            {
                searchOpts = (r.Recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                foreach(string fullPath in Directory.EnumerateFiles(r.ReferencePath, r.Wildcard, searchOpts))
                {
                    filename = Path.GetFileNameWithoutExtension(fullPath);

                    if(!assemblies.ContainsKey(filename))
                        assemblies.Add(filename, fullPath);
                }
            }

            builder.ReportProgress("Adding wildcard references");

            try
            {
                msBuildProject = new Project(projectFile);

                // Remove references that are already there
                foreach(ProjectItem r in msBuildProject.GetItems("Reference"))
                    if(assemblies.ContainsKey(r.EvaluatedInclude))
                    {
                        builder.ReportProgress("    Skipping {0} ({1}) as it appears to already be present",
                            r.EvaluatedInclude, assemblies[r.EvaluatedInclude]);
                        assemblies.Remove(r.EvaluatedInclude);
                    }

                // Add the remaining references
                foreach(var r in assemblies)
                {
                    projectItem = msBuildProject.AddItem("Reference", r.Key)[0];
                    projectItem.SetMetadataValue(ProjectElement.HintPath, r.Value);
                    builder.ReportProgress("    Added reference {0} ({1})", r.Key, r.Value);
                }

                msBuildProject.Save(projectFile);
            }
            finally
            {
                // If we loaded it, we must unload it.  If not, it is cached and may cause problems later.
                if(msBuildProject != null)
                {
                    ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject);
                    ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject.Xml);
                }
            }
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~WildcardReferencesPlugIn()
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
