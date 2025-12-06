//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IFileItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/26/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the interface used to interact with a file item in a help file builder project
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

using Sandcastle.Core.ConceptualContent;

namespace Sandcastle.Core.Project;

/// <summary>
/// This defines the interface used to interact with a file item in a help file builder project
/// </summary>
public interface IFileItem
{
    /// <summary>
    /// This read-only property is used to get the containing project
    /// </summary>
    ISandcastleProject Project { get; }

    /// <summary>
    /// This is used to get or set the build action of the item
    /// </summary>
    BuildAction BuildAction { get; set; }

    /// <summary>
    /// This is used to get or set the filename (the <c>Include</c> path)
    /// </summary>
    FilePath IncludePath {get; set; }

    /// <summary>
    /// This is used to get or set the link path
    /// </summary>
    /// <value>If the item has no link path, this returns the <see cref="IncludePath" /> path</value>
    FilePath LinkPath { get; set; }

    /// <summary>
    /// This read-only property is used to get the full path to the item
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// This is used to get or set an ID for a conceptual content image
    /// </summary>
    /// <remarks>This is used to indicate that an image file is part of the conceptual content.  Image items
    /// without an ID are not valid and will be ignored.</remarks>
    string ImageId { get; set; }

    /// <summary>
    /// This is used to get or set alternate text for an image
    /// </summary>
    string AlternateText { get; set; }

    /// <summary>
    /// This is used to convert the file item to a <see cref="ContentFile"/> instance
    /// </summary>
    /// <returns>The file item as a <see cref="ContentFile"/></returns>
    ContentFile ToContentFile();

    /// <summary>
    /// Remove the item from the containing project
    /// </summary>
    void RemoveFromProjectFile();
}
