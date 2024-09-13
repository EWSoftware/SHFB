//===============================================================================================================
// System  : Sandcastle Tools - MRefBuilder
// File    : AssemblyInfo.cs
// Updated : 04/09/2021
// Note    : Copyright 2006-2021, Microsoft Corporation, All rights reserved
//
// MRefBuilder assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//=============================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following set of attributes.  Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyTitle("MRefBuilder")]
[assembly: AssemblyDescription("This is used to parse assemblies and generate reflection information files " +
    "containing details about the APIs that can be used to create help files.")]

[assembly: CLSCompliant(false)]

#if NET8_0_OR_GREATER
// This tool is dependent on Windows APIs
[assembly:System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

// See AssemblyInfoShared.cs for the shared attributes common to all projects in the solution.
