//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2010
// Note    : Copyright 2006-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Sandcastle Help File Builder components assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.3.0  08/02/2006  EFW  Created the code
//=============================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Help File Builder BuildAssembler Components")]
[assembly: AssemblyTitle("Sandcastle Help File Builder BuildAssembler Components")]
[assembly: AssemblyDescription("A set of build components used to extend the " +
    "features of Sandcastle's BuildAssembler tool.")]

// The base Sandcastle component types are not CLS compliant
[assembly: CLSCompliant(false)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
