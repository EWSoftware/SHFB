//=============================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Sandcastle Help File Builder Generate Inherited Documentation attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Help File Builder Generate Inherited Documentation")]
[assembly: AssemblyTitle("Sandcastle Help File Builder Generate Inherited Documentation")]
[assembly: AssemblyDescription("A command line tool that scans XML comments " +
    "files for <inheritdoc /> tags and produces a new XML comments file " +
    "containing the inherited documentation for use by Sandcastle.")]

[assembly: CLSCompliant(true)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
