//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProjectFileConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2010-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to hold configuration settings for Visual Studio project file documentation
// sources.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/06/2009  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This holds configuration settings for Visual Studio project file
    /// documentation sources
    /// </summary>
    public class ProjectFileConfiguration
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the project filename
        /// </summary>
        public string ProjectFileName { get; set; }

        /// <summary>
        /// This is used to get or set the configuration used for the build
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// This is used to get or set the platform used for the build
        /// </summary>
        public string Platform { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The project filename</param>
        public ProjectFileConfiguration(string name)
        {
            this.ProjectFileName = name;
        }
        #endregion
    }
}
