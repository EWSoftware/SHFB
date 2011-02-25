//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : CollectionTocStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/21/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the collection table of
// contents style for MS Help 2 collections.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  03/21/2008  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the collection table of contents
    /// style for MS Help 2 collections.
    /// </summary>
    [Serializable]
    public enum CollectionTocStyle
    {
        /// <summary>The collection content will be grouped under a root
        /// node.</summary>
        Hierarchical,
        /// <summary>The collection content will be listed at the root
        /// level.</summary>
        Flat
    }
}
