//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : IBasePath.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains an interface used to define the properties used to obtain the base path for a FilePath
// object.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/23/2008  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This interface defines the properties used to obtain a base path for a <see cref="FilePath" /> object
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
