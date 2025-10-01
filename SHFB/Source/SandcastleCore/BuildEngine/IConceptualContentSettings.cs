//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IConceptualContentSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/30/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the interface used to interact with conceptual content settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/30/2025  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;

using Sandcastle.Core.ConceptualContent;

namespace Sandcastle.Core.BuildEngine;

/// <summary>
/// This defines the interface used to interact with conceptual content settings
/// </summary>
public interface IConceptualContentSettings
{
    /// <summary>
    /// This read-only property gets the conceptual content image files
    /// </summary>
    IList<ImageReference> ImageFiles { get; }

    /// <summary>
    /// This read-only property gets the conceptual content code snippet files
    /// </summary>
    IList<ContentFile> CodeSnippetFiles { get; }

    /// <summary>
    /// This read-only property gets the conceptual content token files
    /// </summary>
    IList<ContentFile> TokenFiles { get; }

    /// <summary>
    /// This read-only property gets the conceptual content layout files
    /// </summary>
    IList<ContentFile> ContentLayoutFiles { get; }

    /// <summary>
    /// This read-only property gets a collection of the conceptual content topics
    /// </summary>
    IList<TopicCollection> Topics { get; }
}
