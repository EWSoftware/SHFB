//=============================================================================
// System  : Sandcastle Tools - Aggregate Reflection Data by Namespace Utility
// File    : AssemblyInfo.cs
// Updated : 03/10/2012
// Note    : Copyright 2006-2012, Microsoft Corporation, All rights reserved
//
// Aggregate reflection data by namespace utility assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://Sandcastle.CodePlex.com.   This notice and
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
[assembly: AssemblyTitle("Aggregate Reflection Data by Namespace Utility")]
[assembly: AssemblyDescription("This is used to parse multiple reflection information files produced by " +
    "MRefBuilder and combine them into a single reflection information file by namespace such that all " +
    "members from all namespaces in the individual files are merged into a single file.")]

[assembly: CLSCompliant(true)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
