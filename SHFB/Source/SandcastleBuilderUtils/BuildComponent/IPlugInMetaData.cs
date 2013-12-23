//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : IPlugInMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/17/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in metadata interface definition used to implement a Sandcastle Help File Builder
// build process plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2013  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Utils.BuildComponent
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
        /// This read-only property returns true if the plug-in is configurable or false if it is not
        /// </summary>
        /// <value>If this returns true, the <see cref="IPlugIn.ConfigurePlugIn"/> method will be called to allow
        /// the user to configure the plug-in's settings when requested.</value>
        bool IsConfigurable { get; }

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

        /// <summary>
        /// This read-only property returns additional copyright information for the plug-in
        /// </summary>
        /// <value>This value is user-defined and is set on the plug-in class itself</value>
        string AdditionalCopyrightInfo { get; }
    }
}
