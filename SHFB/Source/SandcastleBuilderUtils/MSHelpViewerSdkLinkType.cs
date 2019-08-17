//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : MSHelpViewerSdkLinkType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/16/2019
// Note    : Copyright 2006-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the reference link types for SDK links in MS Help Viewer
// help files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/28/2006  EFW  Created the code
// 06/20/2010  EFW  Created SDK link type enums for each help format
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the type of links used to reference other help topics referring to
    /// framework (SDK) help topics in MS Help Viewer help files.
    /// </summary>
    [Serializable]
    public enum MSHelpViewerSdkLinkType
    {
        /// <summary>No active links</summary>
        None,
        /// <summary>Id style links for use within an MS Help viewer help file</summary>
        Id,
        /// <summary>Links to framework topics online</summary>
        Msdn
    }
}
