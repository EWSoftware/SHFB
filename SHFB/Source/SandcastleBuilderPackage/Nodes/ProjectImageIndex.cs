//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ProjectImageIndex.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type that defines additional node image index values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/22/2011  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This enumerated type defines the image index values for the Sandcastle
    /// Help File Builder project.
    /// </summary>
    public enum ProjectImageIndex
    {
        /// <summary>The project node image</summary>
        ProjectNode,
        /// <summary>The open documentation sources node image</summary>
        DocumentationSourcesOpen,
        /// <summary>The closed documentation sources node image</summary>
        DocumentationSourcesClosed,
        /// <summary>The documentation source node image</summary>
        DocumentationSource,
        /// <summary>A content layout/site map file</summary>
        ContentSiteMap,
        /// <summary>A resource items file</summary>
        ResourceItems,
        /// <summary>A code snippets file</summary>
        CodeSnippets,
        /// <summary>A token file</summary>
        Tokens,
        /// <summary>A conceptual content topic file</summary>
        ConceptualContent,
        /// <summary>Project properties folder</summary>
        ProjectProperties
    }
}
