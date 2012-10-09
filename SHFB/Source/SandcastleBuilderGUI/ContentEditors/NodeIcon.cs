//=============================================================================
// System  : Sandcastle Help File Builder
// File    : NodeIcon.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/04/2009
// Note    : Copyright 2008-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the node icon index
// values for the project explorer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/29/2008  EFW  Created the code
// 1.8.0.3  12/04/2009  EFW  Added support for resource item files
//=============================================================================

using System;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This defines the <see cref="ProjectExplorerWindow" /> tree view node
    /// icon index values.
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
        /// <summary>Conceptual content layout/sitemap file</summary>
        ContentLayout,
        /// <summary>Conceptual content token file</summary>
        TokenFile,
        /// <summary>Additional content topic transformation file</summary>
        TopicTransform,
        /// <summary>XML file</summary>
        XmlFile,
        /// <summary>General content file (MAML, HTML, stylesheet, etc.)</summary>
        Content,
        /// <summary>Resource item file</summary>
        ResourceItemFile
    }
}
