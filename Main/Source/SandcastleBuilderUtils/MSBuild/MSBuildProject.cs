//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : MSBuildProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/27/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an MSBuild project wrapper used by the Sandcastle Help
// File builder during the build process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/11/2008  EFW  Created the code
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This is a simple wrapper around an MSBuild project that is used to
    /// extract information from it during a help file build.
    /// </summary>
    public class MSBuildProject : IDisposable
    {
        #region Private data members
        //=====================================================================

        private Project msBuildProject;
        private Dictionary<string, ProjectProperty> properties;
        private bool removeProjectWhenDisposed;

        private static Regex reInvalidAttribute = new Regex(
            "The attribute \"(.*?)\" in element \\<(.*?)\\> is unrecognized", RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the underlying MSBuild project file reference
        /// </summary>
        public Project ProjectFile
        {
            get { return msBuildProject; }
        }

        /// <summary>
        /// This is used to get the assembly name
        /// </summary>
        public string AssemblyName
        {
            get
            {
                ProjectProperty prop;
                string assemblyName = null, outputType = null, outputPath = null;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                // Give precedence to OutDir if defined.  Ignore ".\" as that's our default.
                if(properties.TryGetValue(ProjectElement.OutDir, out prop))
                {
                    outputPath = prop.EvaluatedValue;

                    if(outputPath == @".\")
                        outputPath = null;
                }

                if(String.IsNullOrEmpty(outputPath) && properties.TryGetValue("OutputPath", out prop))
                    outputPath = prop.EvaluatedValue;

                if(!String.IsNullOrEmpty(outputPath))
                {
                    if(properties.TryGetValue("AssemblyName", out prop))
                        assemblyName = prop.EvaluatedValue;

                    if(properties.TryGetValue("OutputType", out prop))
                        outputType = prop.EvaluatedValue;
                }

                if(!String.IsNullOrEmpty(assemblyName))
                {
                    // The values are case insensitive
                    if(String.Compare(outputType, "Library", StringComparison.OrdinalIgnoreCase) == 0)
                        assemblyName += ".dll";
                    else
                        if(String.Compare(outputType, "Exe", StringComparison.OrdinalIgnoreCase) == 0 ||
                          String.Compare(outputType, "WinExe", StringComparison.OrdinalIgnoreCase) == 0)
                            assemblyName += ".exe";
                        else
                            assemblyName = null;

                    if(assemblyName != null)
                        if(Path.IsPathRooted(outputPath))
                            assemblyName = Path.Combine(outputPath, assemblyName);
                        else
                            assemblyName = Path.Combine(Path.Combine(Path.GetDirectoryName(
                                msBuildProject.FullPath), outputPath), assemblyName);
                }

                return assemblyName;
            }
        }

        /// <summary>
        /// This is used to get the XML comments file name
        /// </summary>
        public string XmlCommentsFile
        {
            get
            {
                ProjectProperty prop;
                string docFile = null, outputPath = null;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties.TryGetValue("DocumentationFile", out prop))
                {
                    docFile = prop.EvaluatedValue;

                    if(!String.IsNullOrEmpty(docFile))
                    {
                        // If rooted, take the path as it is
                        if(!Path.IsPathRooted(docFile))
                        {
                            // Give precedence to OutDir if defined.  Ignore ".\" as that's our default.
                            if(properties.TryGetValue(ProjectElement.OutDir, out prop))
                            {
                                outputPath = prop.EvaluatedValue;

                                if(outputPath == @".\")
                                    outputPath = null;
                            }

                            if(!String.IsNullOrEmpty(outputPath))
                            {
                                if(Path.IsPathRooted(outputPath))
                                    docFile = Path.Combine(outputPath, Path.GetFileName(docFile));
                                else
                                    docFile = Path.Combine(Path.Combine(Path.GetDirectoryName(
                                        msBuildProject.FullPath), outputPath), Path.GetFileName(docFile));

                                // Fall back to the original location if not found
                                if(!File.Exists(docFile))
                                    docFile = Path.Combine(Path.GetDirectoryName(msBuildProject.FullPath), docFile);
                            }
                            else
                                docFile = Path.Combine(Path.GetDirectoryName(msBuildProject.FullPath), docFile);
                        }
                    }
                }
                else
                {
                    // If not defined, assume it's in the same place as the assembly with the same name
                    // but a ".xml" extension.  This can happen when using Team Build for some reason.
                    docFile = this.AssemblyName;

                    if(!String.IsNullOrEmpty(docFile))
                    {
                        docFile = Path.ChangeExtension(docFile, ".xml");

                        if(!File.Exists(docFile))
                            docFile = null;
                    }
                }

                return docFile;
            }
        }

        /// <summary>
        /// This is used to get the target framework type and version
        /// </summary>
        public string TargetFrameworkVersion
        {
            get
            {
                ProjectProperty prop;
                string versionValue;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                // Is it a Silverlight version?
                if(properties.TryGetValue("SilverlightVersion", out prop))
                {
                    versionValue = prop.EvaluatedValue;

                    if(versionValue[0] == 'v')
                        versionValue = versionValue.Substring(1);

                    return "Silverlight " + versionValue;
                }

                // Try for a regular .NET version
                if(properties.TryGetValue("TargetFrameworkVersion", out prop))
                {
                    versionValue = prop.EvaluatedValue;

                    if(versionValue[0] == 'v')
                        versionValue = versionValue.Substring(1);

                    return ".NET " + versionValue;
                }

                return null;
            }
        }

        /// <summary>
        /// This is used to get the project GUID
        /// </summary>
        public string ProjectGuid
        {
            get
            {
                ProjectProperty prop;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties.TryGetValue("ProjectGuid", out prop))
                    return prop.EvaluatedValue;

                return Guid.Empty.ToString();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectFile">The MSBuild project to load</param>
        public MSBuildProject(string projectFile)
        {
            if(!File.Exists(projectFile))
                throw new BuilderException("BE0051", "The specified project " +
                    "file does not exist: " + projectFile);

            if(Path.GetExtension(projectFile).ToUpperInvariant() == ".VCPROJ")
                throw new BuilderException("BE0068", "Incompatible Visual " +
                    "Studio project file format.  See error code help topic " +
                    "for more information.\r\nC++ project files prior to Visual " +
                    "Studio 2010 are not currently supported.");

            try
            {
                // If the project is already loaded, we'll use it as-is
                var matchingProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(
                    projectFile).FirstOrDefault();

                if(matchingProject == null)
                {
                    // Not loaded, so we must load it and dispose of it later
                    removeProjectWhenDisposed = true;
                    msBuildProject = new Project(projectFile);
                }
            }
            catch(InvalidProjectFileException ex)
            {
                // Future MSBuild projects may not be loadable.  Their targets must be added as individual
                // documentation sources and reference items.
                if(reInvalidAttribute.IsMatch(ex.Message))
                    throw new BuilderException("BE0068", "Incompatible Visual Studio project " +
                        "file format.  See error code help topic for more information.\r\nThis " +
                        "project may be for a newer version of MSBuild and cannot be loaded.  " +
                        "Error message:", ex);

                throw;
            }
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// MSBuild project if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~MSBuildProject()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the MSBuild project object.
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
            // If we loaded the MSBuild project, we must unload it.  If not, it
            // is cached and will cause problems if loaded a second time.
            if(removeProjectWhenDisposed && msBuildProject != null)
            {
                ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject);
                ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject.Xml);
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to set the active configuration and platform used when
        /// evaluating the properties.
        /// </summary>
        /// <param name="configuration">The active configuration</param>
        /// <param name="platform">The active platform</param>
        /// <param name="outDir">The output directory</param>
        /// <remarks>If the platform is set to any variation of "Any CPU" and
        /// it isn't found in the project, it will be converted to "AnyCPU"
        /// (no space).  This works around an issue with Team Build that
        /// includes the space even though it should not be present.</remarks>
        public void SetConfiguration(string configuration, string platform, string outDir)
        {
            // If we didn't load the project, we won't modify its settings.
            // Typically, they already match in that case.
            if(removeProjectWhenDisposed)
            {
                if(platform.Equals("Any CPU", StringComparison.OrdinalIgnoreCase))
                {
                    List<string> values = new List<string>(
                        msBuildProject.ConditionedProperties[ProjectElement.Platform]);

                    if(values.IndexOf(platform) == -1 &&
                      values.IndexOf(SandcastleProject.DefaultPlatform) != -1)
                        platform = SandcastleProject.DefaultPlatform;
                }

                msBuildProject.SetGlobalProperty(ProjectElement.Configuration, configuration);
                msBuildProject.SetGlobalProperty(ProjectElement.Platform, platform);

                if(!String.IsNullOrEmpty(outDir))
                    msBuildProject.SetGlobalProperty(ProjectElement.OutDir, outDir);

                msBuildProject.ReevaluateIfNecessary();
            }

            // There can be duplicate versions of the properties so pick the last one
            // as it will contain the value to use.
            properties = msBuildProject.AllEvaluatedProperties.GroupBy(p => p.Name).Select(
                g => g.Last()).ToDictionary(p => p.Name);
        }

        /// <summary>
        /// This is used to set the Visual Studio solution macros based on the
        /// specified project name.
        /// </summary>
        /// <param name="solutionName">The solution name to use</param>
        public void SetSolutionMacros(string solutionName)
        {
            // If we didn't load the project, we won't modify its settings.
            // Typically, they already match in that case.
            if(removeProjectWhenDisposed)
            {
                msBuildProject.SetGlobalProperty(ProjectElement.SolutionPath, solutionName);
                msBuildProject.SetGlobalProperty(ProjectElement.SolutionDir, FolderPath.TerminatePath(
                    Path.GetDirectoryName(solutionName)));
                msBuildProject.SetGlobalProperty(ProjectElement.SolutionFileName,
                    Path.GetFileName(solutionName));
                msBuildProject.SetGlobalProperty(ProjectElement.SolutionName,
                    Path.GetFileNameWithoutExtension(solutionName));
                msBuildProject.SetGlobalProperty(ProjectElement.SolutionExt, Path.GetExtension(solutionName));

                msBuildProject.ReevaluateIfNecessary();

                properties = msBuildProject.AllEvaluatedProperties.GroupBy(p => p.Name).Select(
                    g => g.Last()).ToDictionary(p => p.Name);
            }
        }

        /// <summary>
        /// Clone the project's reference information and add it to the
        /// given dictionary.
        /// </summary>
        /// <param name="references">The dictionary used to contain the
        /// cloned reference information</param>
        internal void CloneReferenceInfo(Dictionary<string, Tuple<string, string,
          List<KeyValuePair<string, string>>>> references)
        {
            string rootPath, path;

            rootPath = Path.GetDirectoryName(msBuildProject.FullPath);

            // Nested project references are ignored.  We'll assume that they exist in the
            // folder with the target and they'll be found automatically.  
            foreach(string refType in (new string[] { "Reference", "COMReference" }))
                foreach(ProjectItem reference in msBuildProject.GetItems(refType))
                    if(!references.ContainsKey(reference.EvaluatedInclude))
                    {
                        var metadata = reference.Metadata.Select(m => new KeyValuePair<string, string>(
                            m.Name, m.EvaluatedValue)).ToList();
                        var hintPath = metadata.FirstOrDefault(m => m.Key == "HintPath");

                        // Convert relative paths to absolute paths
                        if(hintPath.Key != null)
                        {
                            path = reference.GetMetadataValue("HintPath");

                            if(!Path.IsPathRooted(path))
                            {
                                metadata.Remove(hintPath);
                                metadata.Add(new KeyValuePair<string,string>("HintPath",
                                    Path.Combine(rootPath, path)));
                            }
                        }

                        references.Add(reference.EvaluatedInclude, Tuple.Create(reference.ItemType,
                            reference.EvaluatedInclude, metadata));
                    }
        }
        #endregion
    }
}
