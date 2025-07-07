//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IPlugInMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2013-2025, Eric Woodruff, All rights reserved
//
// This file contains a plug-in metadata interface definition used to implement a Sandcastle Help File Builder
// build process plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2013  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.PlugIn
{
    /// <summary>
    /// This class defines the metadata for a Sandcastle Help File Builder build process plug-in
    /// </summary>
    public interface IPlugInMetadata
    {
        /// <summary>
        /// This read-only property returns the ID for the plug-in
        /// </summary>
        string Id { get; }

        /// <summary>
        /// This read-only property returns true if the plug-in is hidden in the project plug-in property page
        /// </summary>
        /// <value>If this returns true, the plug-in is hidden in the project plug-in property page and cannot
        /// be added to the project.  If false, the default, it can be added to projects.  This is useful for
        /// presentation style dependency plug-ins that have no configurable elements and thus do not need to be
        /// manually added to the project to override settings.</value>
        bool IsHidden { get; }

        /// <summary>
        /// This read-only property returns true if the plug-in should run in a partial build or false if it
        /// should not.
        /// </summary>
        /// <value>If this returns false, the plug-in will not be loaded when a partial build is performed.</value>
        bool RunsInPartialBuild { get; }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        string Description { get; }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        string Version { get; }

        /// <summary>
        /// This read-only property returns the copyright information for the plug-in
        /// </summary>
        string Copyright { get; }
    }
}
