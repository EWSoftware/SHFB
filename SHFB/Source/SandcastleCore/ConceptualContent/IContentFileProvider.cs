//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IContentFileProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2015-2021, Eric Woodruff, All rights reserved
//
// This file contains an interface used to provide content files from a help file builder project or some other
// source.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/18/2015  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;

using Sandcastle.Core.Project;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This interface is implemented to provide content files from a help file builder project or some other
    /// source.
    /// </summary>
    public interface IContentFileProvider
    {
        /// <summary>
        /// This returns an enumerable list of content files of the given type contained in the project or
        /// some other source.
        /// </summary>
        /// <param name="buildAction">The build action of the items to retrieve</param>
        /// <returns>An enumerable list of content files of the given type if any are found in the project or
        /// some other source.</returns>
        IEnumerable<ContentFile> ContentFiles(BuildAction buildAction);
    }
}
