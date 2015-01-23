//===============================================================================================================
// System  : Sandcastle Tools - BuildAssembler Build Component Targets Library
// File    : AssemblyInfo.cs
// Updated : 01/16/2014
// Note    : Copyright 2013-2014, Microsoft Corporation, All rights reserved
//
// BuildAssembler build component targets assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//===============================================================================================================

using System;
using System.Reflection;
using System.Resources;

// General assembly information
[assembly: AssemblyTitle("BuildAssembler - Build Component Targets Library")]
[assembly: AssemblyDescription("This contains a set of build component reference link target classes used to " +
    "generate reference links in API topics.  They are separate from the main assembly as they are " +
    "serializable.  By separating them, they can be versioned independently of the main assembly so that " +
    "caches containing instances of the classes do not have to be rebuilt with every Sandcastle tools release.")]

[assembly: CLSCompliant(true)]

// NOTE: See the description attribute for the reason this assembly is versioned independently of the others.
//       If this ever changes, we can switch to using the current date-based versioning scheme.
[assembly: AssemblyVersion("2.7.3.0")]
