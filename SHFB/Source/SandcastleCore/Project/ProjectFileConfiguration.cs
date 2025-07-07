//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ProjectFileConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2025
// Note    : Copyright 2010-2025, Eric Woodruff, All rights reserved
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

using System.Xml.Linq;

namespace Sandcastle.Core.Project
{
    /// <summary>
    /// This holds configuration settings for Visual Studio project file documentation sources
    /// </summary>
    public class ProjectFileConfiguration
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// The project GUID from a .sln solution file
        /// </summary>
        public string ProjectGuid { get; set; }

        /// <summary>
        /// The project element from a .slnx solution file
        /// </summary>
        public XElement ProjectElement { get; set; }

        /// <summary>
        /// This is used to get or set the project filename
        /// </summary>
        public string ProjectFileName { get; set; }

        /// <summary>
        /// This is used to get or set the configuration used for the solution
        /// </summary>
        public string SolutionConfiguration { get; set; }

        /// <summary>
        /// This is used to get or set the platform used for the solution
        /// </summary>
        public string SolutionPlatform { get; set; }

        /// <summary>
        /// This is used to get or set the configuration used for the build
        /// </summary>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// This is used to get or set the platform used for the build
        /// </summary>
        public string BuildPlatform { get; set; }

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
