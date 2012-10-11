//=============================================================================
// System  : Sandcastle Tools - Segregate Reflection Data by Assembly Utility
// File    : AssemblyInfo.cs
// Updated : 03/10/2012
// Note    : Copyright 2006-2012, Microsoft Corporation, All rights reserved
//
// Segregate reflection data by assembly utility assembly attributes.
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
[assembly: AssemblyTitle("Segregate Reflection Data by Assembly Utility")]
[assembly: AssemblyDescription("This is used to parse a single reflection information file produced by " +
    "MRefBuilder and split it into individual reflection information files by assembly such that each one " +
    "only contains related types from the individual assembly.")]

[assembly: CLSCompliant(true)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
