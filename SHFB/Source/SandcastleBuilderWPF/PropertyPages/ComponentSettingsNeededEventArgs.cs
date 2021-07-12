//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ComponentSettingsNeededEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/07/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
//
// This file contains the class used to obtain the current build component settings from the project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/07/2017  EFW  Created the code
//===============================================================================================================

using System;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used to request the current build component or plug-in settings from the current project
    /// </summary>
    public class ComponentSettingsNeededEventArgs : EventArgs
    {
        /// <summary>
        /// This is used to get or set whether or not a project is loaded
        /// </summary>
        /// <value>Return true if a project is loaded and the values below are valid, false if not</value>
        public bool ProjectLoaded { get; set; }

        /// <summary>
        /// This is used to get or set the current build components in the project
        /// </summary>
        public ComponentConfigurationDictionary Components { get; set; }

        /// <summary>
        /// This is used to get or set the current plug-ins in the project
        /// </summary>
        public PlugInConfigurationDictionary PlugIns { get; set; }
    }
}
