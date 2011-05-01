//=============================================================================
// System  : Sandcastle Help File Builder Project Launcher
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Sandcastle Help File Builder project launcher attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/02/2006  EFW  Created the code
//=============================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Help File Builder")]
[assembly: AssemblyTitle("Sandcastle Help File Builder Project Launcher")]
[assembly: AssemblyDescription("This utility is used to open Sandcastle Help File Builder project files " +
    "using either the standalone GUI or Visual Studio.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eric Woodruff")]
[assembly: AssemblyCopyright("Copyright \xA9 2011, Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyTrademark("Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:

// NOTE: While the VSPackage is under development, this version number will control the version
//       number in the installer.  The build engine will be locked at version 1.9.3.0 so that
//       plug-ins and other components will continue to work with the latest official production
//       release as well as the development release containing the VSPackage.
[assembly: AssemblyVersion("1.9.3.1")]
