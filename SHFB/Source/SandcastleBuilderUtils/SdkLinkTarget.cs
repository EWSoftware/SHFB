//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : SdkLinkTarget.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/16/2019
// Note    : Copyright 2006-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the link target for SDK links
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/25/2007  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the locations in which a browser window can be opened for the
    /// external website links.
    /// </summary>
    [Serializable]
    public enum SdkLinkTarget
    {
        /// <summary>The URL is loaded into a new unnamed window.</summary>
        Blank,
        /// <summary>The current document is replaced with the specified URL.</summary>
        Self,
        /// <summary>The URL is loaded into the current frame's parent. If the frame has no parent, this value
        /// acts like the value <c>Self</c>.</summary>
        Parent,
        /// <summary>The URL replaces any frame sets that may be loaded. If there are no frame sets defined, this
        /// value acts like the value <c>Self</c>.</summary>
        Top
    }
}
