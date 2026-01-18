//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ISubstitutionTags.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/18/2026
// Note    : Copyright 2025-2026, Eric Woodruff, All rights reserved
//
// This file contains the interface used to interact with the substitution tags handler in the build process
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/20/2025  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.BuildEngine;

/// <summary>
/// This defines the interface used to interact with the substitution tags handler in the build process
/// </summary>
public interface ISubstitutionTags
{
    /// <summary>
    /// Transform the specified template text by replacing the substitution tags with the corresponding
    /// project property values.
    /// </summary>
    /// <param name="templateText">The template text to transform</param>
    /// <param name="args">An optional list of arguments to format into the  template before transforming it</param>
    /// <returns>The transformed text</returns>
    string TransformText(string templateText, params object[] args);

    /// <summary>
    /// Transform the specified template file by inserting the necessary values into the substitution tags
    /// and saving it to the destination folder.
    /// </summary>
    /// <param name="templateFile">The template file to transform</param>
    /// <param name="sourceFolder">The folder where the template is located</param>
    /// <param name="destFolder">The folder in which to save the transformed file</param>
    /// <returns>The path to the transformed file</returns>
    string TransformTemplate(string templateFile, string sourceFolder, string destFolder);
}
