//=============================================================================
// System  : Sandcastle Guided Installation
// File    : CompletionAction.cs
// Author  : Eric Woodruff
// Updated : 03/05/2011
// Compiler: Microsoft Visual C#
//
// This file contains a class used to define an optional action to take when
// the guided installation has completed.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  02/12/2011  EFW  Created the code
//=============================================================================

using System;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This class is used to define an optional action to take when the
    /// guided installation has completed.
    /// </summary>
    public class CompletionAction
    {
        /// <summary>
        /// This is used to get or set a description of the action
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set the action to take
        /// </summary>
        public Action Action { get; set; }
    }
}
