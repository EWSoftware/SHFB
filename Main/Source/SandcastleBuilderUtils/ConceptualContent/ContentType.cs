//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ContentType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/22/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the conceptual content
// data types.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/24/2008  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This defines the content types that can be edited
    /// </summary>
    [Serializable]
    public enum ContentType
    {
        /// <summary>Code snippets</summary>
        CodeSnippets,
        /// <summary>Edit image references</summary>
        Images,
        /// <summary>Edit tokens</summary>
        Tokens,
        /// <summary>Edit topics</summary>
        Topics
    }
}
