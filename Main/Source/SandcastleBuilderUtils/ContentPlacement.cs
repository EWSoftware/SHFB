//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ContentPlacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/06/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the placement of the
// additional content items in the table of contents if any are defined.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  09/06/2006  EFW  Added ContentPlacement
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the placement of the additional
    /// content items in the table of contents if any are defined.
    /// </summary>
    [Serializable]
    public enum ContentPlacement
    {
        /// <summary>Additional content appears above the namespaces</summary>
        AboveNamespaces,
        /// <summary>Additional content appears below the namespaces</summary>
        BelowNamespaces
    }
}
