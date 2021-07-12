//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : WindowPlacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains the window placement structure used to store information about the main form's position
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/03/2007  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This structure is used to get and set the main form's position
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        /// <summary>The length of the structure</summary>
        public int length;
        /// <summary>Window position control flags</summary>
        public int flags;
        /// <summary>The current show state of the window</summary>
        public int showCmd;
        /// <summary>Window upper left when minimized</summary>
        public int ptMinPosition_x;
        /// <summary>Window upper left when minimized</summary>
        public int ptMinPosition_y;
        /// <summary>Window upper left when maximized</summary>
        public int ptMaxPosition_x;
        /// <summary>Window upper left when maximized</summary>
        public int ptMaxPosition_y;
        /// <summary>Window coordinates when in the restored position</summary>
        public int rcNormalPosition_left;
        /// <summary>Window coordinates when in the restored position</summary>
        public int rcNormalPosition_top;
        /// <summary>Window coordinates when in the restored position</summary>
        public int rcNormalPosition_right;
        /// <summary>Window coordinates when in the restored position</summary>
        public int rcNormalPosition_bottom;
    }
}
