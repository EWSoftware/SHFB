//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : NamespaceGroup.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/02/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to track namespace groups and their children
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/07/2013  EFW  Created the code
// 06/02/2021  EFW  Merged code into SandcastleBuilder.Utils
//===============================================================================================================

using System.Collections.Generic;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This is used to keep track of the namespace groups and their children
    /// </summary>
    internal class NamespaceGroup
    {
        /// <summary>
        /// This is used to get or set the namespace name
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// This read-only property returns a list of the child namespaces if this is a group
        /// </summary>
        /// <remarks>If empty, this is a normal namespace entry</remarks>
        public List<string> Children { get; } = new List<string>();
    }
}
