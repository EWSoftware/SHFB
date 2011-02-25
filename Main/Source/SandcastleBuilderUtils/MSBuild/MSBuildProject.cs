//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : MSBuildProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/10/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
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
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Microsoft.Build.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This is a simple wrapper around an MSBuild project that is used to
    /// extract information from it during a help file build.
    /// </summary>
    public class MSBuildProject
    {
        #region Private data members
        //=====================================================================

        private Project msBuildProject;
        private BuildPropertyGroup properties;
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
                string assemblyName, outputType, outputPath = null;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                // Give precedence to OutDir if defined
                if(properties["OutDir"] != null)
                    outputPath = properties["OutDir"].FinalValue;

                if(String.IsNullOrEmpty(outputPath) && properties["OutputPath"] != null)
                    outputPath = properties["OutputPath"].FinalValue;

                if(!String.IsNullOrEmpty(outputPath))
                {
                    assemblyName = properties["AssemblyName"].FinalValue;

                    if(properties["OutputType"] != null)
                        outputType = properties["OutputType"].FinalValue;
                    else
                        outputType = null;
                }
                else
                    assemblyName = outputType = null;

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
                                msBuildProject.FullFileName), outputPath), assemblyName);
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
                string docFile = null, outputPath = null;

                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties["DocumentationFile"] != null)
                {
                    docFile = properties["DocumentationFile"].FinalValue;

                    if(!String.IsNullOrEmpty(docFile))
                    {
                        // If rooted, take the path as it is
                        if(!Path.IsPathRooted(docFile))
                        {
                            // Give precedence to OutDir if defined
                            if(properties["OutDir"] != null)
                                outputPath = properties["OutDir"].FinalValue;

                            if(!String.IsNullOrEmpty(outputPath))
                            {
                                if(Path.IsPathRooted(outputPath))
                                    docFile = Path.Combine(outputPath, Path.GetFileName(docFile));
                                else
                                    docFile = Path.Combine(Path.Combine(Path.GetDirectoryName(
                                        msBuildProject.FullFileName), outputPath), Path.GetFileName(docFile));

                                // Fall back to the original location if not found
                                if(!File.Exists(docFile))
                                    docFile = Path.Combine(Path.GetDirectoryName(msBuildProject.FullFileName), docFile);
                            }
                            else
                                docFile = Path.Combine(Path.GetDirectoryName(msBuildProject.FullFileName), docFile);
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
        /// This is used to get the target framework version
        /// </summary>
        public string TargetFrameworkVersion
        {
            get
            {
                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties["TargetFrameworkVersion"] != null)
                    return properties["TargetFrameworkVersion"].FinalValue;

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
                if(properties == null)
                    throw new InvalidOperationException("Configuration has not been set");

                if(properties["ProjectGuid"] == null)
                    return Guid.Empty.ToString();

                return properties["ProjectGuid"].FinalValue;
            }
        }
        #endregion

        #region Constructor, methods, etc.
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectFile">The MSBuild project to load</param>
        public MSBuildProject(string projectFile)
        {
            msBuildProject = new Project(Engine.GlobalEngine);

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
                msBuildProject.Load(projectFile);
            }
            catch(InvalidProjectFileException ex)
            {
                // Some MSBuild 4.0 projects cannot be loaded yet.  Their
                // targets must be added as individual documentation sources
                // and reference items.
                if(reInvalidAttribute.IsMatch(ex.Message))
                    throw new BuilderException("BE0068", "Incompatible Visual Studio project " +
                        "file format.  See error code help topic for more information.\r\nThis " +
                        "project may be for a newer version of MSBuild and cannot be loaded.  " +
                        "Error message:", ex);

                throw;
            }
        }

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
            if(platform.Equals("Any CPU", StringComparison.OrdinalIgnoreCase))
            {
                List<string> values = new List<string>(
                    msBuildProject.GetConditionedPropertyValues(ProjectElement.Platform));

                if(values.IndexOf(platform) == -1 &&
                  values.IndexOf(SandcastleProject.DefaultPlatform) != -1)
                    platform = SandcastleProject.DefaultPlatform;
            }

            msBuildProject.GlobalProperties.SetProperty(ProjectElement.Configuration, configuration);
            msBuildProject.GlobalProperties.SetProperty(ProjectElement.Platform, platform);

            if(!String.IsNullOrEmpty(outDir))
                msBuildProject.GlobalProperties.SetProperty(ProjectElement.OutDir, outDir);

            properties = msBuildProject.EvaluatedProperties;
        }

        /// <summary>
        /// This is used to set the Visual Studio solution macros based on the
        /// specified project name.
        /// </summary>
        /// <param name="solutionName">The solution name to use</param>
        public void SetSolutionMacros(string solutionName)
        {
            msBuildProject.GlobalProperties.SetProperty(ProjectElement.SolutionPath, solutionName);
            msBuildProject.GlobalProperties.SetProperty(ProjectElement.SolutionDir, FolderPath.TerminatePath(
                Path.GetDirectoryName(solutionName)));
            msBuildProject.GlobalProperties.SetProperty(ProjectElement.SolutionFileName,
                Path.GetFileName(solutionName));
            msBuildProject.GlobalProperties.SetProperty(ProjectElement.SolutionName,
                Path.GetFileNameWithoutExtension(solutionName));
            msBuildProject.GlobalProperties.SetProperty(ProjectElement.SolutionExt, Path.GetExtension(solutionName));

            properties = msBuildProject.EvaluatedProperties;
        }

        /// <summary>
        /// Clone the project's references and add them to the dictionary
        /// </summary>
        /// <param name="references">The dictionary used to contain the
        /// cloned references</param>
        public void CloneReferences(Dictionary<string, BuildItem> references)
        {
            BuildItem refItem;
            string rootPath, path;

            rootPath = Path.GetDirectoryName(msBuildProject.FullFileName);

            foreach(string refType in (new string[] { "Reference", "COMReference", "ProjectReference" }))
                foreach(BuildItem reference in msBuildProject.GetEvaluatedItemsByName(refType))
                {
                    // Ignore nested project references.  We'll assume that
                    // they exist in the folder with the target and they'll
                    // be found automatically.  Imported references are also
                    // ignored since cloning them doesn't turn off the imported
                    // flag and we can't modify them.  Those will have to be
                    // added to the SHFB project as reference items if needed.
                    if(reference.Name == "ProjectReference" || reference.IsImported)
                        continue;

                    refItem = reference.Clone();

                    // Convert relative paths to absolute paths
                    if(refItem.Name == "Reference" && refItem.HasMetadata("HintPath"))
                    {
                        path = refItem.GetMetadata("HintPath");

                        if(!Path.IsPathRooted(path))
                            refItem.SetMetadata("HintPath", Path.Combine(rootPath, path));
                    }

                    if(!references.ContainsKey(refItem.Include))
                        references.Add(refItem.Include, refItem);
                }
        }
        #endregion
    }
}
