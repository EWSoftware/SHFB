//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ApplyChangesEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/11/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used for the property page ApplyChanges event
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/11/2017  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used to contain the event arguments for the property page <c>ApplyChanges</c> event
    /// </summary>
    public class ApplyChangesEventArgs : EventArgs
    {
        /// <summary>
        /// This is used to get or set whether or not changes were successfully applied
        /// </summary>
        /// <value>True if applied, false if not</value>
        public bool ChangesApplied { get; set; }
    }
}
