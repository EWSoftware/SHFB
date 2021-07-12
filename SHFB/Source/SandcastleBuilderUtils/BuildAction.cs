//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildAction.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/13/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the build action (item name) for build items in a project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/24/2008  EFW  Created the code
// 12/04/2009  EFW  Added support for resource item files
// 04/08/2012  EFW  Added support for XAML configuration files
// 05/03/2015  EFW  Removed support for topic transformation
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This defines the build action (item name) for build items in a project
    /// </summary>
    [Serializable]
    public enum BuildAction
    {
        /// <summary>No action, the item is ignored</summary>
        None,
        /// <summary>Conceptual content image file</summary>
        Image,
        /// <summary>Conceptual content code snippets file</summary>
        CodeSnippets,
        /// <summary>Conceptual content token file</summary>
        Tokens,
        /// <summary>Conceptual content layout file</summary>
        ContentLayout,
        /// <summary>Additional content site map file</summary>
        SiteMap,
        /// <summary>General content file (HTML, style sheet, images not  related to conceptual content, etc.)</summary>
        Content,
        /// <summary>Resource items file</summary>
        ResourceItems,
        /// <summary>XAML configuration file (for BuildAssembler)</summary>
        XamlConfiguration,

        // Items below this point are specific to the project explorer and are not actual build actions

        /// <summary>A folder (project explorer designation only, not a build
        /// action)</summary>
        Folder,
        /// <summary>The project (project explorer designation only, not a
        /// build action)</summary>
        Project,
        /// <summary>A documentation source (project explorer designation only,
        /// not a build action)</summary>
        DocumentationSource,
        /// <summary>A reference item (project explorer designation only, not a
        /// build action)</summary>
        ReferenceItem
    }
}
