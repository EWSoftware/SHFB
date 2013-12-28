//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : AssemblyInfoShared.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2013
// Note    : Copyright 2006-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Sandcastle Help File Builder common assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
#region Prior history
// 1.0.0.0  08/02/2006  EFW  Created the code
// 1.1.0.0  08/22/2006  EFW  Added support for building MS Help 2 files
// 1.2.0.0  09/02/2006  EFW  Various additions and updates
// 1.3.0.0  09/08/2006  EFW  Various additions and updates
// 1.3.2.0  10/10/2006  EFW  Various additions and updates
// 1.3.4.0  12/24/2006  EFW  Various additions and updates
// 1.5.0.0  06/19/2007  EFW  Various additions and updates
// 1.6.0.0  06/19/2007  EFW  Various additions and updates
// 1.8.0.0  06/20/2008  EFW  Implemented new MSBuild project format
// 1.9.0.0  06/06/2010  EFW  Added support for generating MS Help Viewer files
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 1.9.4.0  04/15/2012  EFW  Updated for use with Sandcastle 2.7.0.0
// 1.9.5.0  09/30/2012  EFW  Updated for use with Sandcastle 2.7.1.0
#endregion
// -------  12/22/2013  EFW  Updated the version numbering scheme to use a date-based value
//===============================================================================================================

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

//
// General Information about an assembly is controlled through the following set of attributes. Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Help File Builder")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eric Woodruff")]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyTrademark("Eric Woodruff, All Rights Reserved")]
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
// numbers as the Sandcastle Help File Builder to indicate with which version their components are compatible.
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

    public const string Copyright = "Copyright \xA9 2006-2013, Eric Woodruff, All Rights Reserved";
}
