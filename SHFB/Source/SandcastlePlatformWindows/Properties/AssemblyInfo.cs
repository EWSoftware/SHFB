//===============================================================================================================
// System  : Sandcastle Tools - Windows platform specific code
// File    : AssemblyInfo.cs
// Updated : 04/05/2021
// Note    : Copyright 2021, Microsoft Corporation, All rights reserved
//
// Sandcastle Windows platform specific assembly
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2013  EFW  Renamed BuildAssemblerLibrary to SandcastleCore and moved various core classes from the
//                  other projects into it.
//===============================================================================================================

using System;
using System.Reflection;

// General assembly information
[assembly: AssemblyTitle("Sandcastle Windows Platform Library")]
[assembly: AssemblyDescription("This contains a set of utility classes used by the Sandcastle Tools that are " +
    "specific to the Windows platform.")]

[assembly: CLSCompliant(true)]
