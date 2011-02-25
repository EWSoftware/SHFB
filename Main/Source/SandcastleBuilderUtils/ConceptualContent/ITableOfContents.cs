//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ITableOfContents.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/16/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an interface used to interact with project files that
// can generate table of contents entries.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  08/12/2008  EFW  Created the code
//=============================================================================

using System;

using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This interface is used to interact with project files that can generate
    /// table of contents entries.
    /// </summary>
    public interface ITableOfContents
    {
        /// <summary>
        /// Get the content layout <see cref="FileItem" />
        /// </summary>
        FileItem ContentLayoutFile { get; }

        /// <summary>
        /// Generate the table of contents for the conceptual topics
        /// </summary>
        /// <param name="toc">The table of contents collection</param>
        /// <param name="pathProvider">The base path provider</param>
        void GenerateTableOfContents(TocEntryCollection toc,
          IBasePathProvider pathProvider);
    }
}
