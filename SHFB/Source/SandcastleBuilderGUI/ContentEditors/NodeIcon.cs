//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : NodeIcon.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/03/2015
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the node icon index values for the project explorer
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/29/2008  EFW  Created the code
// 12/04/2009  EFW  Added support for resource item files
//===============================================================================================================

using System;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This defines the <see cref="ProjectExplorerWindow" /> tree view node icon index values
    /// </summary>
    [Serializable]
    public enum NodeIcon
    {
        /// <summary>Unknown item</summary>
        None,
        /// <summary>The root project node</summary>
        ProjectNode,
        /// <summary>Documentation source folder</summary>
        DocSourceFolder,
        /// <summary>References folder</summary>
        ReferenceFolder,
        /// <summary>General folder</summary>
        GeneralFolder,
        /// <summary>Documentation source</summary>
        DocSource,
        /// <summary>Reference</summary>
        ReferenceItem,
        /// <summary>Image file</summary>
        ImageFile,
        /// <summary>Code snippets file</summary>
        CodeSnippets,
        /// <summary>Conceptual content layout/site map file</summary>
        ContentLayout,
        /// <summary>Conceptual content token file</summary>
        TokenFile,
        /// <summary>XML file</summary>
        XmlFile,
        /// <summary>General content file (MAML, HTML, style sheet, etc.)</summary>
        Content,
        /// <summary>Resource item file</summary>
        ResourceItemFile
    }
}
