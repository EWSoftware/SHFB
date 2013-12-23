//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AssemblyInfo.cs
// Updated : 12/17/2013
// Note    : Copyright 2006-2013, Microsoft Corporation, All rights reserved
//
// Sandcastle core class library assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2013  EFW  Renamed BuildAssemblerLibrary to SandcastleCore and moved various core classes from the
//                  other projects into it.
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
[assembly: AssemblyProduct("Sandcastle Tools")]
[assembly: AssemblyTitle("Sandcastle Tools Core Class Library")]
[assembly: AssemblyDescription("This contains a set of core base classes and Managed Extensibility Framework " +
    "(MEF) classes common to the other projects in the tool set.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eric Woodruff")]
[assembly: AssemblyCopyright("Copyright \xA9 2006-2013, Microsoft Corporation, All Rights Reserved.\r\n" +
    "Portions Copyright \xA9 2006-2013, Eric Woodruff, All Rights Reserved.")]
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
