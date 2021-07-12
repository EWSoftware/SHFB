//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlugInConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/13/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to hold a plug-in's configuration and enabled state
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/25/2007  EFW  Created the code
// 07/01/2008  EFW  Reworked to support MSBuild project format
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class is used to contain a plug-in's configuration and enabled state
    /// </summary>
    public class PlugInConfiguration
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the plug-in's enabled state
        /// </summary>
        /// <value>If set to false, the plug-in will not be used in the build</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// This is used to get or set the plug-in's configuration information
        /// </summary>
        /// <value>This should be an XML fragment.  The root node should be named <c>configuration</c></value>
        public string Configuration { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="enabled">The enabled state</param>
        /// <param name="configuration">The configuration</param>
        internal PlugInConfiguration(bool enabled, string configuration)
        {
            this.Enabled = enabled;
            this.Configuration = String.IsNullOrWhiteSpace(configuration) ? "<configuration />" : configuration;
        }
        #endregion
    }
}
