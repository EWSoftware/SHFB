//===============================================================================================================
// System  : Sandcastle Tools
// File    : AssemblyInfoShared.cs
// Updated : 12/27/2013
// Note    : Copyright 2006-2013, Microsoft Corporation, All rights reserved
//
// Sandcastle tools common assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//===============================================================================================================

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

//
// General Information about an assembly is controlled through the following set of attributes.  Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Tools")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyTrademark("Microsoft Corporation, All Rights Reserved")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

//
// Version information for an assembly consists of the following four values:
//
//      Year of release
//      Month of release
//      Day of release
//      Revision (typically zero unless multiple releases are made on the same day)
//
// This versioning scheme allows build component and plug-in developers to use the same major, minor, and build
// numbers as the Sandcastle tools to indicate with which version their components are compatible.
//
[assembly: AssemblyVersion(AssemblyInfo.Version)]

// See AssemblyInfo.cs for project-specific assembly attributes

/// <summary>
/// This defines constants that can be used by plug-ins and components in their metadata
/// </summary>
internal static partial class AssemblyInfo
{
    /// <summary>
    /// Common assembly version
    /// </summary>
    public const string Version = "2013.12.27.0";

    public const string Copyright = "Copyright \xA9 2006-2013, Microsoft Corporation, All Rights Reserved.\r\n" +
        "Portions Copyright \xA9 2006-2013, Eric Woodruff, All Rights Reserved.";
}
