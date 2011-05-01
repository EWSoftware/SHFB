//=============================================================================
// System  : Microsoft Managed Package Framework for Projects (MPFProj)
// File    : AssemblyInfo.cs
// Author  : Microsoft Corporation
// Updated : 03/20/2011
// Note    : Copyright 2009-2011, Microsoft Corporation, All rights reserved
// Compiler: Microsoft Visual C#
//
// Managed Package Framework for Projects assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://MPFProj10.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  03/20/2011  EFW  Merged project into SHFB project to create a
//                           package for Visual Studio integration.
//=============================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyProduct("Microsoft Managed Package Framework for Projects")]
[assembly: AssemblyTitle("Microsoft Managed Package Framework for Projects")]
[assembly: AssemblyDescription("The base managed package framework used to create packages for " +
    "Visual Studio Integration")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("Copyright \xA9 2008-2011, Microsoft Corporation, All Rights Reserved")]
[assembly: AssemblyTrademark("Microsoft Corporation, All Rights Reserved")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

[assembly: Guid("084954ec-af04-4ea3-b166-b1fced604dc8")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Expose the internal members to the types in the SandcastleBuilder.Package assembly
[assembly: InternalsVisibleTo("SandcastleBuilder.Package, PublicKey=" +
    "002400000480000094000000060200000024000052534131000400000100010091ab9" +
    "bc23e07d4fb7404041ec4d81193cfa9d661e0e24bd2c03182e0e7fc75b265a092a3f8" +
    "53c672895e55b95611684ea090e787497b0d11b902b1eccd9bc9ea3c9a56740ecda8e" +
    "961c93c3960136eefcdf106855a4eb8fff2a97f66049cd0228854b24709c0c945b499" +
    "403d29a2801a39d4c4c30bab653ebc8bf604f5840c88")]
