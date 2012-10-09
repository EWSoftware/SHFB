//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EntityType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/09/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the entity type enumeration.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/04/2011  EFW  Created the code
//=============================================================================

namespace SandcastleBuilder.WPF
{
    /// <summary>
    /// This defines the entity types that can be inserted by the Entity References control
    /// </summary>
    internal enum EntityType
    {
        /// <summary>Token entity</summary>
        Token,
        /// <summary>Image entity</summary>
        Image,
        /// <summary>Table of contents entity</summary>
        TocEntry,
        /// <summary>Code snippet entity</summary>
        CodeSnippet,
        /// <summary>Code member entity</summary>
        CodeEntity,
        /// <summary>A top level file entity (not insertable)</summary>
        File
    }
}
