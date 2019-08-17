//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : BuildPropertiesNeededEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/17/2019
// Note    : Copyright 2017-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to obtain the current build property settings from the project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/16/2017  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used to request the current build component or plug-in settings from the current project
    /// </summary>
    public class BuildPropertiesNeededEventArgs : EventArgs
    {
        /// <summary>
        /// This is used to get or set whether or not a project is loaded
        /// </summary>
        /// <value>Return true if a project is loaded and the values below are valid, false if not</value>
        public bool ProjectLoaded { get; set; }

        /// <summary>
        /// This is used to get or set the current presentation style setting
        /// </summary>
        public string PresentationStyle { get; set; }

        /// <summary>
        /// This is used to get the current syntax filters setting
        /// </summary>
        public string SyntaxFilters { get; set; }
    }
}
