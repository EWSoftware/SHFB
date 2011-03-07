//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FrameworkCommentsFileLocation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/05/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to represent a framework XML comments file
// location.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/05/2011  EFW  Created the code
//=============================================================================

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This is used to represent a framework XML comments file location
    /// </summary>
    public class FrameworkCommentsFileLocation
    {
        /// <summary>The folder location</summary>
        public string Folder { get; set; }

        /// <summary>Cache filename for the cached build components</summary>
        public string CacheFilename { get; set; }

        /// <summary>
        /// Specify true if the XML comments can have localized versions in
        /// a language sub-folder.
        /// </summary>
        /// <value>The default is false to not check for language-specific
        /// versions of the XML comments files.</value>
        public bool CanHaveLocalizedVersion { get; set; }

        /// <summary>
        /// Specify true to recurse this folder for additional comments files
        /// or false to only look in the given folder.
        /// </summary>
        /// <value>The default is false to no recurse sub-folders</value>
        public bool Recurse { get; set; }
    }
}
