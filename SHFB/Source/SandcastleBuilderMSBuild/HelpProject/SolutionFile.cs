//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : SolutionFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/12/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to parse project information out of solution files (.sln, .slnf, or.slnx)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/19/2025  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: http proj

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.Project;

namespace SandcastleBuilder.MSBuild.HelpProject;

/// <summary>
/// This class is used to parse project information out of solution files (.sln, .slnf, or .slnx)
/// </summary>
/// <remarks>Microsoft did release a NuGet package for parsing solution files but it doesn't currently
/// support .NET Standard so we handle it with this class instead which returns equivalent information.</remarks>
public class SolutionFile
{
    #region Private data members
    //=====================================================================

    private static readonly HashSet<string> projectFileExtension = new(
        [".csproj", ".fsproj", ".vbproj", ".vcproj", ".vcxproj", ".wapproj"], StringComparer.OrdinalIgnoreCase);

    private readonly string slnContent;
    private readonly XDocument slnxContent;
    private readonly List<string> projectFilter = [];

    // Regular expression used to parse solution files
    private static readonly Regex reExtractProjectGuids = new(
        "^Project\\(\"\\{(.*?)\\}\"\\) = \".*?\", \"(?!http)" +
        "(?<Path>.*?proj)\", \"\\{(?<GUID>.*?)\\}\"", RegexOptions.Multiline);

    private static readonly JsonDocumentOptions documentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// The solution filename
    /// </summary>
    public string SolutionFilename { get; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="solutionFilename">The solution filename</param>
    public SolutionFile(string solutionFilename)
    {
        this.SolutionFilename = solutionFilename;

        try
        {
            if(Path.GetExtension(solutionFilename).Equals(".sln", StringComparison.OrdinalIgnoreCase))
                slnContent = File.ReadAllText(solutionFilename);
            else
            {
                if(Path.GetExtension(solutionFilename).Equals(".slnx", StringComparison.OrdinalIgnoreCase))
                    slnxContent = XDocument.Load(solutionFilename);
                else
                {
                    // A solution filter is just a JSON file containing a path to the solution file and the
                    // projects it should build.  We'll use the solution file from it with the filtered
                    // list of projects.  The solution file will be used to determine whether the project
                    // will build given specific platform and configuration settings since that information
                    // is not in the filter file.
                    var filter = JsonDocument.Parse(File.ReadAllText(solutionFilename), documentOptions);

                    var solutionFilter = JsonSerializer.Deserialize<SolutionFilter>(
                        filter.RootElement.GetProperty("solution"), serializerOptions);

                    solutionFilter.FullyQualifyPaths(solutionFilename);

                    projectFilter.AddRange(solutionFilter.Projects);

                    var sf = new SolutionFile(solutionFilter.SolutionPath);
                    
                    slnContent = sf.slnContent;
                    slnxContent = sf.slnxContent;
                }
            }
        }
        catch(Exception ex)
        {
            // Ignore errors loading the solution.  We'll treat it like an empty solution.
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
    #endregion

    #region Helper methods
    //=====================================================================

    /// <summary>
    /// Determine if a project is likely to contain assemblies that can be documented
    /// </summary>
    /// <param name="projectFilename">The project filename to check</param>
    /// <returns>True if the project is likely to contain projects that can be documented, false if not</returns>
    public static bool IsSupportedProjectType(string projectFilename)
    {
        return projectFileExtension.Contains(Path.GetExtension(projectFilename));
    }

    /// <summary>
    /// Get an enumerable list of the project files in the solution regardless of the configuration and
    /// platform.
    /// </summary>
    /// <returns>An enumerable list of the projects in the solution</returns>
    public IEnumerable<ProjectFileConfiguration> EnumerateProjectFiles()
    {
        string filename, folder = Path.GetDirectoryName(this.SolutionFilename);

        if(slnContent != null)
        {
            MatchCollection projects = reExtractProjectGuids.Matches(slnContent);

            foreach(Match solutionMatch in projects)
            {
                filename = Path.Combine(folder, solutionMatch.Groups["Path"].Value.CorrectFilePathSeparators());

                if((projectFilter.Count == 0 || projectFilter.Any(
                  f => f.Equals(filename, StringComparison.OrdinalIgnoreCase))) &&
                  projectFileExtension.Contains(Path.GetExtension(filename)))
                {
                    yield return new ProjectFileConfiguration(filename) { ProjectGuid = solutionMatch.Groups["GUID"].Value };
                }
            }
        }
        else
        {
            if(slnxContent != null)
            {
                foreach(var p in slnxContent.Root.Descendants("Project"))
                {
                    filename = p.Attribute("Path")?.Value.CorrectFilePathSeparators();

                    if(!String.IsNullOrWhiteSpace(filename))
                    {
                        filename = Path.Combine(folder, filename);

                        if((projectFilter.Count == 0 || projectFilter.Any(
                          f => f.Equals(filename, StringComparison.OrdinalIgnoreCase))) &&
                          projectFileExtension.Contains(Path.GetExtension(filename)))
                        {
                            yield return new ProjectFileConfiguration(filename) { ProjectElement = p };
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check to see if the given project will build for the given configuration and platform
    /// </summary>
    /// <param name="project">The project file information</param>
    /// <param name="configuration">The build configuration to check</param>
    /// <param name="platform">The platform to check</param>
    /// <returns>True if it will be built, false if not.  On return, the actual build configuration and
    /// build platform used for the project will be set in the project instance.  They can differ from the
    /// solution configuration and/or platform.</returns>
    public bool WillBuild(ProjectFileConfiguration project, string configuration, string platform)
    {
        if(project == null || configuration == null || platform == null ||
         (projectFilter.Count != 0 && !projectFilter.Any(f => f.Equals(project.ProjectFileName,
         StringComparison.OrdinalIgnoreCase))))
        {
            return false;
        }

        if(slnContent != null)
        {
            // See if the project is included in the build and get the configuration and platform
            var reIsInBuild = new Regex($@"\{{{project.ProjectGuid}\}}\.{configuration}\|{platform}\." +
                @"Build\.0\s*=\s*(?<Configuration>.*?)\|(?<Platform>.*)", RegexOptions.IgnoreCase);
            var buildMatch = reIsInBuild.Match(slnContent);

            project.SolutionConfiguration = project.BuildConfiguration = configuration;
            project.SolutionPlatform = project.BuildPlatform = platform;

            // If the platform is "AnyCPU" and it didn't match, try "Any CPU" (with a space)
            if(!buildMatch.Success && platform.Equals("AnyCPU", StringComparison.OrdinalIgnoreCase))
            {
                reIsInBuild = new Regex($@"\{{{project.ProjectGuid}\}}\.{configuration}\|Any CPU\." +
                    @"Build\.0\s*=\s*(?<Configuration>.*?)\|(?<Platform>.*)", RegexOptions.IgnoreCase);

                buildMatch = reIsInBuild.Match(slnContent);
            }

            if(buildMatch.Success)
            {
                project.BuildConfiguration = buildMatch.Groups["Configuration"].Value.Trim();
                project.BuildPlatform = buildMatch.Groups["Platform"].Value.Trim();
                return true;
            }

            return false;
        }

        if(project.ProjectElement == null)
            return false;

        project.SolutionConfiguration = project.BuildConfiguration = configuration;
        project.SolutionPlatform = project.BuildPlatform = platform;

        // See if there is a specific build configuration for the project
        foreach(var buildType in project.ProjectElement.Descendants("BuildType"))
        {
            string[] parts = (buildType.Attribute("Solution")?.Value ?? "*|*").Split('|');

            if(parts.Length == 2 &&
              (parts[0] == "*" || parts[0].Equals(configuration, StringComparison.OrdinalIgnoreCase)) &&
              (parts[1] == "*" || parts[1].Equals(platform, StringComparison.OrdinalIgnoreCase)))
            {
                project.BuildConfiguration = buildType.Attribute("Project")?.Value ?? configuration;
                break;
            }
        }

        // See if there is a specific build platform for the project
        foreach(var buildType in project.ProjectElement.Descendants("Platform"))
        {
            string[] parts = (buildType.Attribute("Solution")?.Value ?? "*|*").Split('|');

            if(parts.Length == 2 &&
              (parts[0] == "*" || parts[0].Equals(configuration, StringComparison.OrdinalIgnoreCase)) &&
              (parts[1] == "*" || parts[1].Equals(platform, StringComparison.OrdinalIgnoreCase)))
            {
                project.BuildPlatform = buildType.Attribute("Project")?.Value ?? platform;
                break;
            }
        }

        // See if the project is included in the build for the configuration/platform.  If there are no
        // build elements, it's assumed that it is.
        foreach(var build in project.ProjectElement.Descendants("Build"))
        {
            string[] parts = (build.Attribute("Solution")?.Value ?? "*|*").Split('|');

            if(parts.Length == 2 &&
              (parts[0] == "*" || parts[0].Equals(configuration, StringComparison.OrdinalIgnoreCase)) &&
              (parts[1] == "*" || parts[1].Equals(platform, StringComparison.OrdinalIgnoreCase)) &&
              (build.Attribute("Project")?.Value ?? String.Empty).Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
    #endregion
}
