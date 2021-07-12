//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IConfigurationEditorMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that defines the metadata for a configurable build component
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/23/2021  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This class defines the metadata for a build component configuration editor
    /// </summary>
    public interface IConfigurationEditorMetadata
    {
        /// <summary>
        /// This read-only property returns the ID of the configurable build component
        /// </summary>
        /// <value>This must match the ID of the build component for which this will provide a configuration editor</value>
        string Id { get; }
    }
}
