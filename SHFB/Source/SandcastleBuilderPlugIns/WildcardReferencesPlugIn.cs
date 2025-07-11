//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : WildcardReferencesPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
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

using Sandcastle.Core;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;

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

        private IBuildProcess builder;
        private List<WildcardReferenceSettings> referencePaths;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints { get; } =
        [
            new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before)
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

            builder = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            if(configuration.IsEmpty)
                throw new BuilderException("WRP0001", "The Wildcard References plug-in has not been configured yet");

            // Load the reference path settings
            referencePaths = [];

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
            Dictionary<string, string> assemblies = [];

            builder.ReportProgress("Adding wildcard references");

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
            {
                foreach(string fullPath in Directory.EnumerateFiles(r.ReferencePath.ToString().CorrectFilePathSeparators(),
                  r.Wildcard, r.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    string filename = Path.GetFileNameWithoutExtension(fullPath);

                    // "mscorlib" is ignored as it causes MRefBuilder to reset its target platform information.
                    // For something like Silverlight, that probably isn't what we want it to do so we'll ignore
                    // it.  It'll use what is passed in the platform configuration file option.
                    if(!assemblies.ContainsKey(filename) && !filename.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
                        assemblies.Add(filename, fullPath);
                }
            }

            XDocument project = XDocument.Load(projectFile);
            XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
            XElement itemGroup = null;

            // Remove references that are already there
            foreach(var r in project.Descendants(msbuild + "Reference"))
            {
                itemGroup ??= r.Parent;

                string include = r.Attribute("Include").Value;

                if(assemblies.TryGetValue(include, out string path))
                {
                    builder.ReportProgress("    Skipping {0} ({1}) as it appears to already be present", include, path);
                    assemblies.Remove(include);
                }
            }

            // Add an item group if there are no reference assemblies
            if(itemGroup == null)
            {
                itemGroup = new XElement(msbuild + "ItemGroup");
                project.Root.Add(itemGroup);
            }

            // Add the remaining references
            foreach(var r in assemblies)
            {
                itemGroup.Add(new XElement(msbuild + "Reference",
                    new XAttribute("Include", r.Key),
                    new XElement(msbuild + "HintPath", r.Value)));

                builder.ReportProgress("    Added reference {0} ({1})", r.Key, r.Value);
            }

            project.Save(projectFile);
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
