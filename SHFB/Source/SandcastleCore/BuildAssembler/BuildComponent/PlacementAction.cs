//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PlacementAction.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/23/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an enumeration that defines the build component configuration file placement action values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/23/2013  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This enumeration defines the build component configuration file placement action values
    /// </summary>
    [Serializable]
    public enum PlacementAction
    {
        /// <summary>The component is not used in this configuration</summary>
        None,
        /// <summary>Insert the component at the start of the configuration file</summary>
        Start,
        /// <summary>Insert the component at the end of the configuration file</summary>
        End,
        /// <summary>Place the component before the one indicated</summary>
        Before,
        /// <summary>Place the component after the one indicated</summary>
        After,
        /// <summary>Replace the indicated component configuration with this one</summary>
        Replace
    }
}
