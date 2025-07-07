//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IPlugInConfigurationEditorMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that defines the metadata for a configurable plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/26/2021  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.PlugIn
{
    /// <summary>
    /// This class defines the metadata for a plug-in configuration editor
    /// </summary>
    public interface IPlugInConfigurationEditorMetadata
    {
        /// <summary>
        /// This read-only property returns the ID of the configurable plug-in
        /// </summary>
        /// <value>This must match the ID of the plug-in for which this will provide a configuration editor</value>
        string Id { get; }
    }
}
