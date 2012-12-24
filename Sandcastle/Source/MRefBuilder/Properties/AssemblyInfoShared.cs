//===============================================================================================================
// System  : Sandcastle Tools
// File    : AssemblyInfoShared.cs
// Updated : 12/22/2012
// Note    : Copyright 2006-2012, Microsoft Corporation, All rights reserved
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
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Tools")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("Copyright \xA9 2006-2012, Microsoft Corporation, All Rights Reserved.\r\n" +
    "Portions Copyright \xA9 2006-2012, Eric Woodruff, All Rights Reserved.")]
[assembly: AssemblyTrademark("Microsoft Corporation, All Rights Reserved")]
[assembly: AssemblyCulture("")]
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

// NOTE: Revision number should always be zero.  This allows build component
// and plug-in developers to use the same major, minor, and build numbers
// as the Sandcastle tools to indicate with which version their components are
// compatible.
[assembly: AssemblyVersion("2.7.2.0")]

// See AssemblyInfo.cs for project-specific assembly attributes
