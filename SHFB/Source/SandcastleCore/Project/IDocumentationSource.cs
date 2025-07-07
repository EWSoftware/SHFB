//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IDocumentationSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the interface used to interact with a help file builder project documentation source
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/19/2025  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;

namespace Sandcastle.Core.Project
{
    /// <summary>
    /// This defines the interface used to interact with a help file builder project documentation source
    /// </summary>
    public interface IDocumentationSource
    {
        /// <summary>
        /// This is used to get or set the project configuration to use when the source path refers to a Visual
        /// Studio solution or project.
        /// </summary>
        /// <value>If not set, the configuration value from the owning help file project will be used.  This will
        /// be ignored for assembly and XML comments file entries.</value>
        string Configuration { get; set; }

        /// <summary>
        /// This is used to get or set the project platform to use when the source path refers to a Visual Studio
        /// solution or project.
        /// </summary>
        /// <value>If not set, the platform value from the owning help file project will be used.  This will be
        /// ignored for assembly and XML comments file entries.</value>
        string Platform { get; set; }

        /// <summary>
        /// This is used to get or set the project target framework to use when the source path refers to a
        /// Visual Studio solution or project.
        /// </summary>
        /// <value>This only applies if the project uses multi-targeting.  If not set, the first target framework
        /// will be used.  This will be ignored for assembly and XML comments file entries.</value>
        string TargetFramework { get; set; }

        /// <summary>
        /// This is used to set or get the documentation source file path
        /// </summary>
        /// <value>Wildcards are supported.  If used, all files matching the wildcard will be included as long as
        /// their extension is one of the following: .exe, .dll, .winmd, .*proj, .sln.</value>
        FilePath SourceFile { get; set; }

        /// <summary>
        /// This is used to get or set whether subfolders are included when searching for files if the
        /// <see cref="SourceFile" /> value contains wildcards.
        /// </summary>
        /// <value>If set to true and the source file value contains wildcards, subfolders will be included.  If
        /// set to false, the default, or the source file value does not contain wildcards, only the top-level
        /// folder is included in the search.</value>
        bool IncludeSubFolders { get; set; }

        /// <summary>
        /// This returns an enumerable list of MSBuild project file configurations based on the current settings
        /// and the given configuration and platform.
        /// </summary>
        /// <param name="configurationName">The configuration to use</param>
        /// <param name="platformName">The platform to use</param>
        /// <returns>An enumerable list of project configurations matching the <see cref="SourceFile"/> path.
        /// Sub-folders are only included if <see cref="IncludeSubFolders"/> is set to true.  Any solution files
        /// (.sln) found are returned last, each followed by the projects extracted from them.</returns>
        IEnumerable<ProjectFileConfiguration> Projects(string configurationName, string platformName);

        /// <summary>
        /// This read-only property returns an enumerable list of XML comments files based on the current settings
        /// </summary>
        /// <returns>An enumerable list of XML comments files matching the <see cref="SourceFile"/> path.
        /// Sub-folders are only included if <see cref="IncludeSubFolders"/> is set to true.</returns>
        IEnumerable<string> CommentsFiles { get; }
    }
}
