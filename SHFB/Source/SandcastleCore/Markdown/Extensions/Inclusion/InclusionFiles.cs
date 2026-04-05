//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : InclusionFiles.cs
// Author  : Jason Curl (jcurl@arcor.de)
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// Manages global information about included files.
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// Class InclusionFiles.
/// </summary>
/// <remarks>
/// Manages the stack of files being included and detects circular references. It also ensures that all file paths are
/// absolute and normalized to avoid issues with different relative paths referring to the same file. The first file is
/// pushed using <see cref="PushFile(String)"/> and all subsequent files are pushed using
/// <see cref="PushDependency(String)"/>. When done with a file, call <see cref="Pop"/> to remove it from the stack and
/// allow it to be included again if needed.
/// </remarks>
public static class InclusionFiles
{
    private static readonly HashSet<string> files = new(StringComparer.InvariantCultureIgnoreCase);
    private static readonly Stack<string> stack = new();

    /// <summary>
    /// Gets a list of warnings when parsing this node.
    /// </summary>
    /// <value>The list of warnings.</value>
    public static IList<ParseMessages> Warnings { get; } = [];

    /// <summary>
    /// Initialises the stack with the first file to be included. This should be called once for the main file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>The fully qualified path name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="fileName"/> is not a valid path.</exception>
    public static string PushFile(string fileName)
    {
        if(fileName is null) throw new ArgumentNullException(nameof(fileName));

        string fullPath;
        if(Path.IsPathRooted(fileName))
        {
            fullPath = Path.GetFullPath(fileName);
        }
        else
        {
            fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, fileName));
        }

        files.Clear();
        stack.Clear();
        files.Add(fullPath);
        stack.Push(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Pushes a file dependency to the stack dependency.
    /// </summary>
    /// <param name="fileName">Name of the file that is included from the previous file pushed.</param>
    /// <returns>A context that can be disposed, which then pops the context.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fileName"/> is not a valid relative path.
    /// <para>- or -</para>
    /// Circular reference detected.
    /// <para>- or -</para>
    /// File not found.
    /// </exception>
    public static InclusionFileContext PushDependency(string fileName)
    {
        if(Path.IsPathRooted(fileName))
        {
            throw new ArgumentException($"File name '{fileName}' is not a valid relative path", nameof(fileName));
        }

        string prevPath = stack.Peek();
        string newPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(prevPath), fileName));

        if(files.Contains(newPath))
        {
            string[] files = stack.ToArray();
            StringBuilder circularReferences = new();
            for (int s = files.Length - 1; s > 0; s--) {
                if(s != files.Length) circularReferences.Append("<-- ");
                circularReferences.AppendLine(files[s]);
            }
            Warnings.Add(new("BE0077",
                $"The conceptual topic file '{files[0]}' has circular references includes. " + 
                $"The generated topic file may be incomplete.\n{circularReferences}"));

            throw new ArgumentException("Circular reference detected", nameof(fileName));
        }

        if(!File.Exists(newPath))
        {
            string[] files = stack.ToArray();
            Warnings.Add(new("BE0076",
                $"The conceptual topic file '{files[0]}' has missing includes. " +
                $"The generated topic file may be incomplete. Missing file {newPath} " +
                $"from file {prevPath}"));
            throw new ArgumentException($"File not found: '{newPath}'", nameof(fileName));
        }

        stack.Push(newPath);
        files.Add(newPath);
        return new(newPath);
    }

    /// <summary>
    /// Pops the current file from the stack.
    /// </summary>
    /// <exception cref="InvalidOperationException">No file to pop,</exception>
    /// <remarks>
    /// Normally not needed to call this directly, as the context returned from <see cref="PushDependency(String)"/>
    /// will pop the file when disposed.
    /// </remarks>
    public static void Pop()
    {
        if(stack.Count <= 1)
        {
            throw new InvalidOperationException("No file to pop");
        }

        string popped = stack.Pop();
        files.Remove(popped);
    }
}
