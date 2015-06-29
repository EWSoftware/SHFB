//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PlatformType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/28/2015
// Note    : Copyright 2012-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to define platform type constants
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/10/2012  EFW  Created the code
// 01/02/2014  EFW  Moved the frameworks code to Sandcastle.Core
// 06/21/2015  EFW  Moved to the Reflection namespace and reworked for use with the Reflection Data Manager
//===============================================================================================================

// TODO: Move to Sandcastle.Core project

using System.Collections.Generic;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This class holds a set of constants used to define the various .NET platform types
    /// </summary>
    public static class PlatformType
    {
        /// <summary>.NET Core (Windows Store Apps) Framework</summary>
        public const string DotNetCore = ".NETCore";
        /// <summary>.NET Framework</summary>
        public const string DotNetFramework = ".NETFramework";
        /// <summary>.NET Micro Framework</summary>
        public const string DotNetMicroFramework = ".NETMicroFramework";
        /// <summary>.NET Portable Library Framework</summary>
        public const string DotNetPortable = ".NETPortable";
        /// <summary>Silverlight Framework</summary>
        public const string Silverlight = "Silverlight";
        /// <summary>Windows Phone Framework</summary>
        public const string WindowsPhone = "WindowsPhone";
        /// <summary>Windows Phone Store Apps Framework</summary>
        public const string WindowsPhoneApp = "WindowsPhoneApp";

        /// <summary>
        /// This read-only property returns an enumerable list of the valid platform types
        /// </summary>
        public static IEnumerable<string> PlatformTypes
        {
            get
            {
                return new[] { DotNetFramework, DotNetCore, DotNetMicroFramework, DotNetPortable, Silverlight,
                    WindowsPhone, WindowsPhoneApp };
            }
        }
    }
}
