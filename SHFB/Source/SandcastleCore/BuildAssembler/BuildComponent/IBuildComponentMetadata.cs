//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IBuildComponentMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/23/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that defines the metadata for a BuildAssembler build component
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/23/2013  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This class defines the metadata for a BuildAssembler build component
    /// </summary>
    public interface IBuildComponentMetadata
    {
        /// <summary>
        /// This read-only property returns the ID for the build component
        /// </summary>
        string Id { get; }

        /// <summary>
        /// This read-only property returns true if the build component is visible to configuration tools
        /// </summary>
        /// <remarks>Configuration tools can use this to exclude components that should not appear for the user
        /// to select.</remarks>
        bool IsVisible { get; }

        /// <summary>
        /// This read-only property returns a brief description of the build component
        /// </summary>
        string Description { get; }

        /// <summary>
        /// This read-only property returns the version of the build component
        /// </summary>
        string Version { get; }

        /// <summary>
        /// This read-only property returns the copyright information for the build component
        /// </summary>
        string Copyright { get; }
    }
}
