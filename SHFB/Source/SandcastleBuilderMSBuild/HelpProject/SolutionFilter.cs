//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : SolutionFilter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/12/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to load solution filter files (.slnf)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/12/2025  EFW  Created the code
//===============================================================================================================

// This class is used but is only created during JSON deserialization
#pragma warning disable CA1812

using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using Sandcastle.Core;

namespace SandcastleBuilder.MSBuild.HelpProject;

/// <summary>
/// This class is used to load solution filter files (.slnf)
/// </summary>
internal sealed class SolutionFilter
{
    /// <summary>
    /// This read-only property is used to get the solution file path
    /// </summary>
    [JsonPropertyName("path")]
    public string SolutionPath { get; set; }

    /// <summary>
    /// This read-only property is used to get a list of the projects in the solution filter file
    /// </summary>
    public List<string> Projects { get; set; }

    /// <summary>
    /// This is used to fully qualify the solution and project file paths
    /// </summary>
    /// <param name="filterFilename">The solution filter filename</param>
    public void FullyQualifyPaths(string filterFilename)
    {
        this.SolutionPath = this.SolutionPath.CorrectFilePathSeparators();

        // The solution file may be relative to the filter file path
        if(!Path.IsPathRooted(this.SolutionPath))
            this.SolutionPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filterFilename), this.SolutionPath));

        // Project file paths are always relative to the solution file path
        string solutionPath = Path.GetDirectoryName(this.SolutionPath);

        for(int idx = 0; idx < this.Projects.Count; idx++)
            this.Projects[idx] = Path.Combine(solutionPath, this.Projects[idx].CorrectFilePathSeparators());
    }
}
