//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/26/2014
// Note    : Copyright 2012-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// XML Comments Examples assembly attributes.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  12/05/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguageAttribute("en")]

// General assembly info
[assembly: AssemblyProduct("Sandcastle Tools - XML Comments Examples")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTitle("XMLCommentsExamples")]
[assembly: AssemblyDescription("This is used to demonstrate the various XML comments elements and serves " +
    "no other useful purpose.")]
[assembly: AssemblyCompany("Eric Woodruff")]
[assembly: AssemblyCopyright("Copyright \xA9 2012-2014, Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyTrademark("Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// Version information for an assembly consists of the following four values based on the current date:
//
//      Year of release     4 digit year
//      Month of release    1 or 2 digit month
//      Day of release      1 or 2 digit day
//      Revision            Typically zero unless multiple releases are made on the same day.  In such cases,
//                          increment the revision by one with each same day release.
//
[assembly: AssemblyVersion("2014.1.26.0")]
