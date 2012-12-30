//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ConceptualLinkType.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains an enumerated type that defines the conceptual content link types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/26/2012 - EFW - Moved the enum into its own file and renamed it from LinkType to ConceptualLinkType
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
        /// <summary>Index links (MS Help 2 only)</summary>
        Index,
        /// <summary>Id links (MS Help Viewer only)</summary>
        Id
    }
}
