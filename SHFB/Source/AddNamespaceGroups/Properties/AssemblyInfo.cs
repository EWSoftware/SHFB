//===============================================================================================================
// System  : Sandcastle Tools - Add Namespace Groups Utility
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/07/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Add namespace groups utility assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/07/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Reflection;

//
// General Information about an assembly is controlled through the following set of attributes.  Change these
// attribute values to modify the information associated with an assembly.
//
[assembly: AssemblyTitle("Add Namespace Groups Utility")]
[assembly: AssemblyDescription("This is used to add namespace groups to a reflection data file which can be " +
    "used to combine namespaces with a common root into groups in the table of contents of the generated " +
    "help files.")]

[assembly: CLSCompliant(true)]

// See AssemblyInfoShared.cs for the shared attributes common to all projects
// in the solution.
