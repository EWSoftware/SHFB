//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ITableOfContents.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains an interface used to interact with project files that can generate table of contents
// entries.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/12/2008  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This interface is used to interact with project files that can generate table of contents entries
    /// </summary>
    public interface ITableOfContents
    {
        /// <summary>
        /// This is used to get the content layout file metadata
        /// </summary>
        ContentFile ContentLayoutFile { get; }

        /// <summary>
        /// Generate the table of contents for the conceptual topics
        /// </summary>
        /// <param name="toc">The table of contents collection</param>
        /// <param name="includeInvisibleItems">True to include items marked invisible (useful for previewing)
        /// or false to exclude them.</param>
        void GenerateTableOfContents(TocEntryCollection toc, bool includeInvisibleItems);
    }
}
