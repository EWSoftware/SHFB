//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ReferenceLinkType.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains an enumerated type that defines the reference content link types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/26/2012 - EFW - Moved the enum into its own file and renamed it from LinkType2 to ReferenceLinkType
//===============================================================================================================

using System;

namespace Microsoft.Ddue.Tools.Targets
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
        /// <summary>Index links (MS Help 2 only)</summary>
        Index,
        /// <summary>Local or Index links (MS Help 2 only, the component will decide the type at runtime)</summary>
        LocalOrIndex,
        /// <summary>Online links to MSDN (Framework types only)</summary>
        Msdn,
        /// <summary>Id links (MS Help Viewer only)</summary>
        Id
    }
}
