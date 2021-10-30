//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EntityType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the entity type enumeration.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/04/2011  EFW  Created the code
//===============================================================================================================

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
