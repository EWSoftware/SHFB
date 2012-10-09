//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildComponentConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/01/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to hold a build component's configuration
// and enabled state.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.2  11/01/2007  EFW  Created the code
// 1.8.0.0  07/01/2008  EFW  Reworked to support MSBuild project format
//=============================================================================

using System;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This collection class is used to hold the additional content items
    /// for a project.
    /// </summary>
    public class BuildComponentConfiguration : PropertyBasedCollectionItem
    {
        #region Private data members
        //=====================================================================

        private bool enabled;
        private string config;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the build component's enabled state
        /// </summary>
        /// <value>If set to false, the component will not be used in the
        /// build.</value>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                this.CheckProjectIsEditable();
                enabled = value;
            }
        }

        /// <summary>
        /// This is used to get or set the component's configuration
        /// information.
        /// </summary>
        /// <value>This should be an XML fragment.  The root node should be
        /// named <b>configuration</b>.</value>
        public string Configuration
        {
            get { return config; }
            set
            {
                this.CheckProjectIsEditable();

                if(String.IsNullOrEmpty(value))
                    config = "<configuration />";
                else
                    config = value;
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
        internal BuildComponentConfiguration(bool isEnabled,
          string configuration, SandcastleProject project) : base(project)
        {
            enabled = isEnabled;

            if(String.IsNullOrEmpty(configuration))
                config = "<configuration />";
            else
                config = configuration;
        }
        #endregion
    }
}
