//=============================================================================
// System  : Sandcastle Help File Builder MRefBuilder Components
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Sandcastle Help File Builder MRefBuilder components assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  11/14/2008  EFW  Created the code
//=============================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyProduct("Sandcastle Help File Builder MRefBuilder Components")]
[assembly: AssemblyTitle("Sandcastle Help File Builder MRefBuilder Components")]
[assembly: AssemblyDescription("A set of components used to extend the features " +
    "of Sandcastle's MRefBuilder tool.")]

// The base Sandcastle component types are not CLS compliant
[assembly: CLSCompliant(false)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
