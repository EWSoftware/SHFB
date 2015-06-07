//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : SubstitutionTagAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/24/2015
// Note    : Copyright 2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This attribute is used to mark substitution tag replacement methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubstitutionTagAttribute : Attribute
    {
    }
}
