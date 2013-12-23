//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/22/2013
// Note    : Copyright 2006-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Sandcastle Help File Builder utility assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
// 12/22/2013  EFW  Updated the version numbering scheme to use a date-based value
//===============================================================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

//
// General Information about an assembly is controlled through the following set of attributes.  Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Help File Builder")]
[assembly: AssemblyTitle("Sandcastle Help File Builder Utilities")]
[assembly: AssemblyDescription("This assembly contains the project, build engine, and Managed Extensibility " +
    "Framework (MEF) classes used by all of the other Sandcastle Help File Builder applications.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eric Woodruff")]
[assembly: AssemblyCopyright("Copyright \xA9 2006-2013, Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyTrademark("Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]
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
// NOTE: This assembly is versioned independently of the others that use a shared version number.  This allows
// third party components that reference it to work with future versions of the core library unless a breaking
// change is made that requires a new version number in this assembly.
//
[assembly: AssemblyVersion("2013.12.21.0")]
