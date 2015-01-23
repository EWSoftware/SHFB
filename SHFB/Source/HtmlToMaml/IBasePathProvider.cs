//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : IBasePath.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/23/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an interface used to define the properties used to
// obtain the base path for a FilePath object.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/23/2008  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This interface defines the properties used to obtain a base path for
    /// a <see cref="FilePath" /> object.
    /// </summary>
    public interface IBasePathProvider
    {
        /// <summary>
        /// This read-only property returns the base path
        /// </summary>
        string BasePath
        {
            get;
        }
    }
}
