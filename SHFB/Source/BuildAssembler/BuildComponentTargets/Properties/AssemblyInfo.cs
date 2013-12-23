//===============================================================================================================
// System  : Sandcastle Tools - BuildAssembler Build Component Targets Library
// File    : AssemblyInfo.cs
// Updated : 12/22/2013
// Note    : Copyright 2013, Microsoft Corporation, All rights reserved
//
// BuildAssembler build component targets assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//===============================================================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

//
// General Information about an assembly is controlled through the following set of attributes. Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Tools")]
[assembly: AssemblyTitle("BuildAssembler - Build Component Targets Library")]
[assembly: AssemblyDescription("This contains a set of build component reference link target classes used to " +
    "generate reference links in API topics.  They are separate from the main assembly as they are " +
    "serializable.  By separating them, they can be versioned independently of the main assembly so that " +
    "caches containing instances of the classes do not have to be rebuilt with every Sandcastle tools release.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("Copyright \xA9 2006-2013, Microsoft Corporation, All Rights Reserved.\r\n" +
    "Portions Copyright \xA9 2006-2013, Eric Woodruff, All Rights Reserved.")]
[assembly: AssemblyTrademark("Microsoft Corporation, All Rights Reserved")]
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
// NOTE: See the description attribute for the reason this assembly is versioned independently of the others.
//
[assembly: AssemblyVersion("2.7.3.0")]
