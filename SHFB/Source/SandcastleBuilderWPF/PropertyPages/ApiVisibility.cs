//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ApiVisibility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/29/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumeration for API visibility values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/29/2017  EFW  Moved the enumeration into its own file
//===============================================================================================================

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used to indicate the visibility of a member in the <see cref="ApiNodeInfo" />
    /// </summary>
    public enum ApiVisibility
    {
        /// <summary>The member is public</summary>
        Public,
        /// <summary>The member is protected</summary>
        Protected,
        /// <summary>The member is internal (Friend)</summary>
        Internal,
        /// <summary>The member is private</summary>
        Private
    }
}
