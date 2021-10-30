//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : CompletionAction.cs
// Author  : Eric Woodruff
// Updated : 04/21/2021
//
// This file contains a class used to define an optional action to take when the guided installation has
// completed.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/12/2011  EFW  Created the code
//===============================================================================================================

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
