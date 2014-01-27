//===============================================================================================================
// System  : Microsoft Managed Package Framework for Projects (MPFProj)
// File    : AssemblyInfo.cs
// Author  : Microsoft Corporation
// Updated : 01/16/2014
// Note    : Copyright 2009-2014, Microsoft Corporation, All rights reserved
// Compiler: Microsoft Visual C#
//
// Managed Package Framework for Projects assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://MPFProj10.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  03/20/2011  EFW  Merged project into SHFB project to create a package for Visual Studio integration
// 1.1.0.0  03/06/2013  EFW  Merged changes from the MPF 2012 project into this one so that this version can run
//                           under both VS 2010 and VS 2012.
//===============================================================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

// General assembly information
[assembly: AssemblyProduct("Microsoft Managed Package Framework for Projects")]
[assembly: AssemblyTitle("Microsoft Managed Package Framework for Projects")]
[assembly: AssemblyDescription("The base managed package framework used to create packages for Visual Studio " +
    "Integration")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("Copyright \xA9 2008-2014, Microsoft Corporation, All Rights Reserved")]
[assembly: AssemblyTrademark("Microsoft Corporation, All Rights Reserved")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(false)]

[assembly: ComVisible(false)]

[assembly: Guid("084954ec-af04-4ea3-b166-b1fced604dc8")]

[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]

// Expose the internal members to the types in the SandcastleBuilder.Package assembly
//
// sn.exe -p SandcastleTools.snk PublicKey.key
// sn.exe -tp PublicKey.key
//
// Cut and paste the public key below.
//
[assembly: InternalsVisibleTo("SandcastleBuilder.Package, PublicKey=" +
    "00240000048000009400000006020000002400005253413100040000010001002f5b57e5c28270" +
    "f49518ff41c8842759d9262f1cb9f50adf4d89a9fbcbffd17201be3da944edb7e8cf3bdc19b5fa" +
    "0de57f4f3231b1fa861124c6bbea5bc1216b5e361bfaa0c0fde01c9302d06146543d7e2477a740" +
    "e5279dd8fa7d0e5afe6acb0a2e552e770b63847cf68b47f49dff68375a38791e4cabe0639516df" +
    "e2ce348d")]
