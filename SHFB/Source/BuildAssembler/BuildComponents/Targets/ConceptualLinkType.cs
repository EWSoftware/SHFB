//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ConceptualLinkType.cs
// Note    : Copyright 2010-2015 Microsoft Corporation
//
// This file contains an enumerated type that defines the conceptual content link types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/26/2012 - EFW - Moved the enum into its own file and renamed it from LinkType to ConceptualLinkType
// 06/05/2015 - EFW - Removed support for the Help 2 Index link type
//===============================================================================================================

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This public enumerated type defines the conceptual content link types
    /// </summary>
    [Serializable]    
    public enum ConceptualLinkType
    {
        /// <summary>No links</summary>
        None,
        /// <summary>Local links</summary>
        Local,
        /// <summary>Id links (MS Help Viewer only)</summary>
        Id
    }
}
