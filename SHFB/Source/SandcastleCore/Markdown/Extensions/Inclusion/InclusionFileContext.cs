//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : InclusionFileContext.cs
// Author  : Jason Curl (jcurl@arcor.de)
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// Manages context information about included files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/05/2025  JMC  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// A context that can be disposed when pushing an included file.
/// </summary>
public readonly struct InclusionFileContext : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InclusionFileContext"/> struct.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public InclusionFileContext(string filePath)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    /// <value>The file path.</value>
    public string FilePath { get; }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting managed, or unmanaged
    /// resources.
    /// </summary>
    public void Dispose() => InclusionFiles.Pop();
}