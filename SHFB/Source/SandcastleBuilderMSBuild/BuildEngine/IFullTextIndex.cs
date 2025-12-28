//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : IFullTextIndex.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains an interface used to create full-text index data files used to search for topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/27/2025  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.MSBuild.BuildEngine;

/// <summary>
/// This interface is used to create full-text index data file used to search for topics
/// </summary>
public interface IFullTextIndex
{
    /// <summary>
    /// Create a full-text index from web pages found in the specified file path
    /// </summary>
    /// <param name="filePath">The path containing the files to index</param>
    void CreateFullTextIndex(string filePath);

    /// <summary>
    /// Save the index information to the specified location.
    /// </summary>
    /// <param name="indexPath">The path to which the index files are saved.</param>
    void SaveIndex(string indexPath);
}
