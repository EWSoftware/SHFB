//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : SubstitutionTagAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains an attribute class used to mark substitution tag replacement methods
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/10/2015  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.MSBuild.Design
{
    /// <summary>
    /// This attribute is used to mark substitution tag replacement methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubstitutionTagAttribute : Attribute
    {
    }
}
