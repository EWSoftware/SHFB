//=============================================================================
// System  : Sandcastle Tools - DBCS Fix Utility
// File    : AssemblyInfo.cs
// Updated : 03/10/2012
// Note    : Copyright 2006-2012, Microsoft Corporation, All rights reserved
//
// DBCS fix utility assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//=============================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("DBCS Fix Utility")]
[assembly: AssemblyDescription("This is used to parse the HTML topics produced by Build Assembler and alter " +
    "them ready for compilation into a Help 1 CHM file that uses a language with a double-byte character " +
    "set (DBCS).  This is necessary to work around various issues in the Help 1 compiler due to its lack " +
    "of support for Unicode character sets.")]

[assembly: CLSCompliant(true)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
