//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlatformType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/10/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to define platform type constants
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.5.0  09/10/2012  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Utils.Frameworks
{
    /// <summary>
    /// This class holds a set of constants used to define the various .NET platform types
    /// </summary>
    public static class PlatformType
    {
        /// <summary>.NET Framework</summary>
        public const string DotNetFramework = ".NETFramework";
        /// <summary>.NET Portable Library Framework</summary>
        public const string DotNetPortable = ".NETPortable";
        /// <summary>Silverlight Framework</summary>
        public const string Silverlight = "Silverlight";
        /// <summary>.NET Core (Windows Store Apps) Framework</summary>
        public const string DotNetCore = ".NETCore";
    }
}
