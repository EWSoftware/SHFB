//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ContentPlacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the placement of the additional content items in the
// table of contents if any are defined.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/06/2006  EFW  Added ContentPlacement
//===============================================================================================================

using System;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This public enumerated type defines the placement of the conceptual content items in the table of
    /// contents if any are defined.
    /// </summary>
    [Serializable]
    public enum ContentPlacement
    {
        /// <summary>Conceptual content appears above the namespaces</summary>
        AboveNamespaces,
        /// <summary>Conceptual content appears below the namespaces</summary>
        BelowNamespaces
    }
}
