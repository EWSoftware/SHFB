//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ExecutionBehaviors.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/13/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type that defines the execution behavior of a build process plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2007  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This public enumerated type defines the execution behavior of a build process plug-in
    /// </summary>
    [Flags, Serializable]
    public enum ExecutionBehaviors
    {
        /// <summary>Execute before the help file builder's normal processing.</summary>
        Before         = 0x0001,
        /// <summary>Execute after the help file builder's normal processing.</summary>
        After          = 0x0002,
        /// <summary>Execute both before and after the help file builder's normal processing.</summary>
        BeforeAndAfter = 0x0003,
        /// <summary>Execute instead of the help file builder's normal processing.  If this flag is set,
        /// <c>Before</c> and <c>After</c> are ignored.</summary>
        InsteadOf      = 0x0004
    }
}
