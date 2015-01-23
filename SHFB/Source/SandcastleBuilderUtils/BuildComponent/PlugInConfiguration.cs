//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlugInConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/17/2013
// Note    : Copyright 2007-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to hold a plug-in's configuration and enabled state
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.5.2.0  09/25/2007  EFW  Created the code
// 1.8.0.0  07/01/2008  EFW  Reworked to support MSBuild project format
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This collection class is used to hold a plug-in's configuration and enabled state
    /// </summary>
    public class PlugInConfiguration : PropertyBasedCollectionItem
    {
        #region Private data members
        //=====================================================================

        private bool enabled;
        private string config;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the plug-in's enabled state
        /// </summary>
        /// <value>If set to false, the plug-in will not be used in the build</value>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                base.CheckProjectIsEditable();

                enabled = value;
            }
        }

        /// <summary>
        /// This is used to get or set the plug-in's configuration information
        /// </summary>
        /// <value>This should be an XML fragment.  The root node should be named <c>configuration</c>.</value>
        public string Configuration
        {
            get { return config; }
            set
            {
                base.CheckProjectIsEditable();

                config = String.IsNullOrWhiteSpace(value) ? "<configuration />" : value;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="isEnabled">The enabled state</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="project">The owning project</param>
        internal PlugInConfiguration(bool isEnabled, string configuration, SandcastleProject project) :
          base(project)
        {
            enabled = isEnabled;

            config = String.IsNullOrWhiteSpace(configuration) ? "<configuration />" : configuration;
        }
        #endregion
    }
}
