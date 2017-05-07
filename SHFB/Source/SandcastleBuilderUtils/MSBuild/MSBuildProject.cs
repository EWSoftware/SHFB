//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : MSBuildProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/07/2017
// Note    : Copyright 2008-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an MSBuild project wrapper used by the Sandcastle Help File builder during the build
// process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/11/2008  EFW  Created the code
// 07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 08/20/2011  EFW  Updated to support Portable .NET Framework
// 09/08/2012  EFW  Updated to support Windows Store App projects
// 10/22/2012  EFW  Updated to support the .winmd output type
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;

using Sandcastle.Core.Reflection;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This is a simple wrapper around an MSBuild project that is used to extract information from it during a
    /// help file build.
    /// </summary>
    public class MSBuildProject : IDisposable
    {
        #region Global property name constants
        //=====================================================================

        /// <summary>Solution path (directory and filename) global property</summary>
        public const string SolutionPath = "SolutionPath";
        /// <summary>Solution directory global property</summary>
        public const string SolutionDir = "SolutionDir";
        /// <summary>Solution filename (no path) global property</summary>
        public const string SolutionFileName = "SolutionFileName";
        /// <summary>Solution name (no path or extension) global property</summary>
        public const string SolutionName = "SolutionName";
        /// <summary>Solution extension global property</summary>
        public const string SolutionExt = "SolutionExt";

        #endregion

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
                if(properties.TryGetValue(BuildItemMetadata.OutDir, out prop))
                {
                    outputPath = prop.EvaluatedValue;
                  
                    if(outputPath == @".\")
                        outputPath = null;
                    else
                    {
                        // As of .NET 4.5, the GenerateProjectSpecificOutputFolder property can be used to make
                        // MSBuild set project-specific output folders.  The problem is that it also tries to
                        // apply that to the SHFB project which masks the output folder of the actual project for
                        // which we are trying to get the output folder.  Make an assumption here that if the
                        // folder doesn't exist, we need to look for a project-specific output folder.
                        if(Path.IsPathRooted(outputPath) && !Directory.Exists(outputPath))
                        {
                            outputPath = outputPath.Substring(0, outputPath.Length - 1);

                            if(outputPath.LastIndexOf('\\') != -1)
                            {
                                outputPath = outputPath.Substring(0, outputPath.LastIndexOf('\\'));

                                // The ProjectName property can override the actual project name
                                if(properties.TryGetValue(BuildItemMetadata.ProjectName, out prop))
                                    outputPath = Path.Combine(outputPath, prop.EvaluatedValue);
                                else
                                    outputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(
                                        msBuildProject.FullPath));

                                // If still not there, give up and go for the default
                                if(!Directory.Exists(outputPath))
                                    outputPath = null;
                            }
                        }
                    }
                }

                if(String.IsNullOrEmpty(outputPath) && properties.TryGetValue("OutputPath", out prop))
                    outputPath = prop.EvaluatedValue;

                if(!String.IsNullOrEmpty(outputPath))
                {
                    // Give precedence to TargetName as it may differ in C++ projects
                    if(properties.TryGetValue("TargetName", out prop))
                        assemblyName = prop.EvaluatedValue;

                    if(String.IsNullOrEmpty(assemblyName) && properties.TryGetValue("AssemblyName", out prop))
                        assemblyName = prop.EvaluatedValue;

                    if(properties.TryGetValue("OutputType", out prop))
                        outputType = prop.EvaluatedValue;
                }

                if(!String.IsNullOrEmpty(assemblyName))
                {
                    // Had an odd case where the name contained a line feed so trim it just in case
                    assemblyName = assemblyName.Trim();

                    // The values are case insensitive
                    if(String.Compare(outputType, "Library", StringComparison.OrdinalIgnoreCase) == 0)
                        assemblyName += ".dll";
                    else
                        if(String.Compare(outputType, "Exe", StringComparison.OrdinalIgnoreCase) == 0 ||
                          String.Compare(outputType, "WinExe", StringComparison.OrdinalIgnoreCase) == 0 ||
                          String.Compare(outputType, "AppContainerExe", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            assemblyName += ".exe";
                        }
                        else
                            if(String.Compare(outputType, "winmdobj", StringComparison.OrdinalIgnoreCase) == 0)
                                assemblyName += ".winmd";
                            else
                                assemblyName = null;

                    if(assemblyName != null)
                        if(Path.IsPathRooted(outputPath))
                            assemblyName = Path.Combine(outputPath, assemblyName);
                        else
                            assemblyName = Path.Combine(Path.Combine(Path.GetDirectoryName(
                                msBuildProject.FullPath), outputPath), assemblyName);

                    // .NETCoreApp projects don't seem to return the correct output type
                    if(!File.Exists(assemblyName) && this.TargetFrameworkIdentifier == PlatformType.DotNetCoreApp)
                        assemblyName = Path.ChangeExtension(assemblyName, "dll");

                    // If the TargetFrameworks property is used, the assembly is most likely in a subfolder
                    // under the output folder based on one of the target frameworks specified.
                    if(!File.Exists(assemblyName) && properties.TryGetValue("TargetFrameworks", out prop))
                    {
                        outputPath = Path.GetDirectoryName(assemblyName);

                        foreach(string subfolder in prop.EvaluatedValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            if(Directory.EnumerateFiles(Path.Combine(outputPath, subfolder),
                              Path.GetFileName(assemblyName)).Any())
                            {
                                assemblyName = Path.Combine(outputPath, subfolder, Path.GetFileName(assemblyName));
                                break;
                            }
                    }
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
                string docFile = null, outputPath = null, origDocFile;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties.TryGetValue("DocumentationFile", out prop))
                {
                    docFile = prop.EvaluatedValue;

                    if(!String.IsNullOrEmpty(docFile))
                    {
                        // Had an odd case where the name contained a line feed so trim it just in case
                        docFile = docFile.Trim();

                        // If rooted, take the path as it is
                        if(!Path.IsPathRooted(docFile))
                        {
                            // Give precedence to OutDir if defined.  Ignore ".\" as that's our default.
                            if(properties.TryGetValue(BuildItemMetadata.OutDir, out prop))
                            {
                                outputPath = prop.EvaluatedValue;

                                if(outputPath == @".\")
                                    outputPath = null;
                                else
                                {
                                    // As of .NET 4.5, the GenerateProjectSpecificOutputFolder property can be
                                    // used to make MSBuild set project-specific output folders.  The problem is
                                    // that it also tries to apply that to the SHFB project which masks the
                                    // output folder of the actual project for which we are trying to get the
                                    // output folder.  Make an assumption here that if the folder doesn't exist,
                                    // we need to look for a project-specific output folder.
                                    if(Path.IsPathRooted(outputPath) && !Directory.Exists(outputPath))
                                    {
                                        outputPath = outputPath.Substring(0, outputPath.Length - 1);

                                        if(outputPath.LastIndexOf('\\') != -1)
                                        {
                                            outputPath = outputPath.Substring(0, outputPath.LastIndexOf('\\'));

                                            // The ProjectName property can override the actual project name
                                            if(properties.TryGetValue(BuildItemMetadata.ProjectName, out prop))
                                                outputPath = Path.Combine(outputPath, prop.EvaluatedValue);
                                            else
                                                outputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(
                                                    msBuildProject.FullPath));

                                            // If still not there, give up and go for the default
                                            if(!Directory.Exists(outputPath))
                                                outputPath = null;
                                        }
                                    }
                                }
                            }

                            if(!String.IsNullOrEmpty(outputPath))
                            {
                                origDocFile = docFile;

                                if(Path.IsPathRooted(outputPath))
                                    docFile = Path.Combine(outputPath, Path.GetFileName(docFile));
                                else
                                    docFile = Path.Combine(Path.Combine(Path.GetDirectoryName(
                                        msBuildProject.FullPath), outputPath), Path.GetFileName(docFile));

                                // Fall back to the original location if not found
                                if(!File.Exists(docFile))
                                    docFile = Path.Combine(Path.GetDirectoryName(msBuildProject.FullPath), origDocFile);
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
        /// This is used to get the target framework identifier
        /// </summary>
        public string TargetFrameworkIdentifier
        {
            get
            {
                ProjectProperty prop;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties.TryGetValue("TargetFrameworkIdentifier", out prop))
                    return prop.EvaluatedValue;

                // If not found, assume it is a normal .NET Framework project
                return PlatformType.DotNetFramework;
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
                string versionValue = null;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                switch(this.TargetFrameworkIdentifier)
                {
                    case PlatformType.Silverlight:
                        if(properties.TryGetValue("SilverlightVersion", out prop))
                            versionValue = prop.EvaluatedValue;
                        break;

                    default:
                        if(properties.TryGetValue("TargetFrameworkVersion", out prop))
                            versionValue = prop.EvaluatedValue;
                        break;
                }

                if(versionValue == null)
                {
                    // If not found but TargetFrameworks is specified, just assume some version of .NETFramework
                    if(properties.TryGetValue("TargetFrameworks", out prop))
                        return "4.5.2";

                    throw new InvalidOperationException("Unable to determine target framework version for project");
                }

                if(versionValue[0] == 'v')
                    versionValue = versionValue.Substring(1);

                return versionValue;
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
            if(!Path.IsPathRooted(projectFile))
                projectFile = Path.GetFullPath(projectFile);

            if(!File.Exists(projectFile))
                throw new BuilderException("BE0051", "The specified project file does not exist: " + projectFile);

            if(Path.GetExtension(projectFile).ToUpperInvariant() == ".VCPROJ")
                throw new BuilderException("BE0068", "Incompatible Visual Studio project file format.  See " +
                    "the error code help topic for more information.\r\nC++ project files prior to Visual " +
                    "Studio 2010 are not currently supported.");

            try
            {
                // If the project is already loaded, we'll use it as-is
                msBuildProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectFile).FirstOrDefault();

                if(msBuildProject == null)
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
                if(reInvalidAttribute.IsMatch(ex.Message) || ex.Message.StartsWith("The default XML namespace " +
                  "of the project", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BuilderException("BE0068", "Incompatible Visual Studio project file format.  " +
                        "See the error code help topic for more information.\r\nThis project may be for a " +
                        "newer version of MSBuild and cannot be loaded.  Error message:", ex);
                }

                throw;
            }
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// MSBuild project if not done explicitly with <see cref="Dispose()"/>.
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
        /// <param name="usesProjectSpecificOutput">True if the build is using project-specific output folders, 
        /// false if not.</param>
        /// <remarks>If the platform is set to any variation of "Any CPU" and it isn't found in the project, it
        /// will be converted to "AnyCPU" (no space).  This works around an issue with Team Build that includes
        /// the space even though it should not be present.</remarks>
        public void SetConfiguration(string configuration, string platform, string outDir,
          bool usesProjectSpecificOutput)
        {
            // If we didn't load the project, we won't modify its settings.
            // Typically, they already match in that case.
            if(removeProjectWhenDisposed)
            {
                if(platform.Equals("Any CPU", StringComparison.OrdinalIgnoreCase))
                {
                    List<string> values = new List<string>(
                        msBuildProject.ConditionedProperties[BuildItemMetadata.Platform]);

                    if(values.IndexOf(platform) == -1 &&
                      values.IndexOf(SandcastleProject.DefaultPlatform) != -1)
                        platform = SandcastleProject.DefaultPlatform;
                }

                msBuildProject.SetGlobalProperty(BuildItemMetadata.Configuration, configuration);
                msBuildProject.SetGlobalProperty(BuildItemMetadata.Platform, platform);

                if(!String.IsNullOrEmpty(outDir))
                {
                    // .NET 4.5 supports a property that tells MSBuild to put the project output into a
                    // project-specific folder in OutDir.
                    if(usesProjectSpecificOutput)
                    {
                        // The output directory will contain the SHFB project name so we need to remove it first
                        if(outDir[outDir.Length - 1] == '\\')
                            outDir = outDir.Substring(0, outDir.Length - 1);

                        outDir = Path.Combine(Path.GetDirectoryName(outDir),
                            Path.GetFileNameWithoutExtension(msBuildProject.FullPath));
                    }

                    msBuildProject.SetGlobalProperty(BuildItemMetadata.OutDir, outDir);
                }

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
                msBuildProject.SetGlobalProperty(SolutionPath, solutionName);
                msBuildProject.SetGlobalProperty(SolutionDir, FolderPath.TerminatePath(Path.GetDirectoryName(solutionName)));
                msBuildProject.SetGlobalProperty(SolutionFileName, Path.GetFileName(solutionName));
                msBuildProject.SetGlobalProperty(SolutionName, Path.GetFileNameWithoutExtension(solutionName));
                msBuildProject.SetGlobalProperty(SolutionExt, Path.GetExtension(solutionName));

                msBuildProject.ReevaluateIfNecessary();

                properties = msBuildProject.AllEvaluatedProperties.GroupBy(p => p.Name).Select(
                    g => g.Last()).ToDictionary(p => p.Name);
            }
        }

        /// <summary>
        /// Clone the project's reference information and add it to the given dictionary
        /// </summary>
        /// <param name="resolver">The package reference resolver to use</param>
        /// <param name="references">The dictionary used to contain the cloned reference information</param>
        internal void CloneReferenceInfo(PackageReferenceResolver resolver, Dictionary<string,
          Tuple<string, string, List<KeyValuePair<string, string>>>> references)
        {
            string rootPath, path;

            rootPath = Path.GetDirectoryName(msBuildProject.FullPath);

            // Nested project references are ignored.  We'll assume that they exist in the folder with the target
            // and they'll be found automatically.  
            foreach(string refType in (new string[] { "Reference", "COMReference" }))
                foreach(ProjectItem reference in msBuildProject.GetItems(refType))
                    if(!references.ContainsKey(reference.EvaluatedInclude))
                    {
                        var metadata = reference.Metadata.Select(m => new KeyValuePair<string, string>(m.Name,
                            m.EvaluatedValue)).ToList();
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

            // Resolve any package references by converting them to regular references
            if(resolver.LoadPackageReferenceInfo(msBuildProject))
                foreach(string pr in resolver.ReferenceAssemblies)
                {
                    string refName = Path.GetFileNameWithoutExtension(pr);

                    if(!references.ContainsKey(refName) && File.Exists(pr))
                        references.Add(refName, Tuple.Create("Reference", refName,
                            new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("HintPath", pr),
                                new KeyValuePair<string, string>("FromPackageReference", "true")
                            }));
                }
        }
        #endregion
    }
}
