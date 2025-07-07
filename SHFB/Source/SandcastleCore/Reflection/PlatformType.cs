//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PlatformType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This class holds a set of constants used to define the various .NET platform types
    /// </summary>
    public static class PlatformType
    {
        /// <summary>.NET 5.0 or later</summary>
        public const string DotNet= ".NET";
        /// <summary>.NET Core (Windows Store Apps) Framework</summary>
        public const string DotNetCore = ".NETCore";
        /// <summary>.NET Core Application</summary>
        public const string DotNetCoreApp = ".NETCoreApp";
        /// <summary>.NET Framework</summary>
        public const string DotNetFramework = ".NETFramework";
        /// <summary>.NET Micro Framework</summary>
        public const string DotNetMicroFramework = ".NETMicroFramework";
        /// <summary>.NET Portable Library Framework</summary>
        public const string DotNetPortable = ".NETPortable";
        /// <summary>.NET Standard Framework</summary>
        public const string DotNetStandard = ".NETStandard";
        /// <summary>Silverlight Framework</summary>
        public const string Silverlight = "Silverlight";
        /// <summary>Windows Phone Framework</summary>
        public const string WindowsPhone = "WindowsPhone";
        /// <summary>Windows Phone Store Apps Framework</summary>
        public const string WindowsPhoneApp = "WindowsPhoneApp";

        /// <summary>
        /// This read-only property returns an enumerable list of the valid platform types
        /// </summary>
        /// <remarks>.NETCoreApp is not returned.  It will be redirected to one of the other types.</remarks>
        public static IEnumerable<string> PlatformTypes => [ DotNet, DotNetFramework, DotNetCore,
            DotNetStandard, DotNetMicroFramework, DotNetPortable, Silverlight, WindowsPhone, WindowsPhoneApp ];

        /// <summary>
        /// This can be used to determine if the given set of platform types are compatible with each other for
        /// documentation purposes.
        /// </summary>
        /// <param name="platforms">An enumerable list of platform types</param>
        /// <returns>True if they are compatible, false if not</returns>
        /// <remarks>In general, platforms that have all of their types in mscorlib or netstandard are compatible
        /// but you can't mix both.  All platforms that redirect their types to System.Runtime and other
        /// assemblies are also typically compatible.  Mixing the sets or any combination of other frameworks
        /// is not compatible.</remarks>
        public static bool PlatformsAreCompatible(IEnumerable<(string platform, Version version)> platforms)
        {
            // All .NETFramework of any version or all .NETStandard 2.x is okay
            if(platforms.All(p => p.platform == DotNetFramework) ||
               platforms.All(p => p.platform == DotNetStandard && p.version.Major == 2))
            {
                return true;
            }

            // All .NET, .NETCore, .NETCoreApp of any version or .NET Standard 1.x is okay
            if(platforms.All(p => p.platform == DotNet || p.platform == DotNetCore ||
              p.platform == DotNetCoreApp || (p.platform == DotNetStandard && p.version.Major < 2)))
            {
                return true;
            }

            // .NETStandard 1.x and .NETStandard 2.x is not okay
            if(platforms.All(p => p.platform == DotNetStandard) &&
               platforms.Select(p => p.version.Major).Distinct().Count() != 1)
            {
                return false;
            }

            // All of a single platform any version is okay.  A mix of anything else is not.
            return platforms.Select(p => p.platform).Distinct().Count() == 1;
        }
    }
}
