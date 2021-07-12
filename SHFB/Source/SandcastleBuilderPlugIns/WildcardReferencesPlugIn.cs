//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : WildcardReferencesPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in designed to modify the MRefBuilder project file by adding in reference
// assemblies matching wildcard search paths.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/17/2011  EFW  Created the code
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

// Ignore Spelling: mscorlib

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to modify the MRefBuilder project file by adding in reference assemblies
    /// matching wildcard search paths.
    /// </summary>
    [HelpFileBuilderPlugInExport("Wildcard Assembly References", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to modify the Generate Reflection Information build step by adding " +
        "assembly references found in one or more wildcard search paths.")]
    public sealed class WildcardReferencesPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;
        private List<WildcardReferenceSettings> referencePaths;

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
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before)
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

            builder = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            if(configuration.IsEmpty)
                throw new BuilderException("WRP0001", "The Wildcard References plug-in has not been configured yet");

            // Load the reference path settings
            referencePaths = new List<WildcardReferenceSettings>();

            foreach(var r in configuration.Descendants("reference"))
                referencePaths.Add(WildcardReferenceSettings.FromXml(buildProcess.CurrentProject, r));

            if(referencePaths.Count == 0)
            {
                throw new BuilderException("WRP0002", "At least one reference path is required for the " +
                    "Wildcard References plug-in.");
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            Project msBuildProject = null;
            ProjectItem projectItem;
            Dictionary<string, string> assemblies = new Dictionary<string, string>();

            // The project is named uniquely due to a cache used by the assembly resolution task that uses the
            // project name to name the cache.  If not unique, it can cause parallel builds to fail as it can't
            // access the same cache file in more than one build.
            string projectFile = Directory.EnumerateFiles(builder.WorkingFolder, "GenerateRefInfo*.proj").FirstOrDefault();

            // If the project doesn't exist we have nothing to do.  However, it could be that some other plug-in
            // has bypassed it so only issue a warning.
            if(projectFile == null || !File.Exists(projectFile))
            {
                builder.ReportWarning("WRP0003", "The reflection information generation project '{0}' could " +
                    "not be found.  The Wildcard References plug-in did not run.", projectFile);
                return;
            }

            // Find all unique references
            foreach(var r in referencePaths)
                foreach(string fullPath in Directory.EnumerateFiles(r.ReferencePath, r.Wildcard,
                  (r.Recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    string filename = Path.GetFileNameWithoutExtension(fullPath);

                    // "mscorlib" is ignored as it causes MRefBuilder to reset its target platform information.
                    // For something like Silverlight, that probably isn't what we want it to do so we'll ignore
                    // it.  It'll use what is passed in the platform configuration file option.
                    if(!assemblies.ContainsKey(filename) && !filename.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
                        assemblies.Add(filename, fullPath);
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
                    projectItem.SetMetadataValue(BuildItemMetadata.HintPath, r.Value);
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
