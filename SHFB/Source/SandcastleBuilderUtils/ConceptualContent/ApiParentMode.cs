﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ApiParentMode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2010-2021, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the API parent mode for a conceptual topic
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/01/2010  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This public enumerated type defines the API parent mode for a conceptual
    /// topic.
    /// </summary>
    [Serializable]
    public enum ApiParentMode
    {
        /// <summary>Not a parent to the API content</summary>
        None,
        /// <summary>Insert the API content before this element</summary>
        InsertBefore,
        /// <summary>Insert the API content after this element</summary>
        InsertAfter,
        /// <summary>Insert the API content as a child of this element</summary>
        InsertAsChild
    }
}
