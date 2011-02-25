//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProjectFileConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/06/2010
// Note    : Copyright 2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to hold configuration settings for Visual
// Studio project file documentation sources.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.4  01/06/2009  EFW  Created the code
//=============================================================================

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
