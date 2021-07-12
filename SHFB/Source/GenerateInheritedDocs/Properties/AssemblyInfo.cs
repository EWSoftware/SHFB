//===============================================================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/09/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// Sandcastle Help File Builder Generate Inherited Documentation attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/27/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following set of attributes. Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyTitle("Sandcastle Help File Builder Generate Inherited Documentation")]
[assembly: AssemblyDescription("A command line tool that scans XML comments files for <inheritdoc /> tags and " +
    "produces a new XML comments file containing the inherited documentation for use by Sandcastle.")]

[assembly: CLSCompliant(true)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects in the solution.
