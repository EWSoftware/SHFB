//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ReferenceLinkType.cs
// Note    : Copyright 2010-2019 Microsoft Corporation
//
// This file contains an enumerated type that defines the reference content link types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/26/2012 - EFW - Moved the enum into its own file and renamed it from LinkType2 to ReferenceLinkType
// 06/05/2015 - EFW - Removed support for the Help 2 Index and LocalOrIndex link types
//===============================================================================================================

using System;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This public enumerated type defines the reference content link types
    /// </summary>
    [Serializable]
    public enum ReferenceLinkType
    {
        /// <summary>No links</summary>
        None,
        /// <summary>Self-referencing link (link to topic within its own page)</summary>
        Self,
        /// <summary>Local links</summary>
        Local,
        /// <summary>Links to online content (Framework types only)</summary>
        Msdn,
        /// <summary>Id links (MS Help Viewer only)</summary>
        Id
    }
}
