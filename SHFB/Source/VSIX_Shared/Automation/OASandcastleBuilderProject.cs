//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : OASandcastleBuilderProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used for project automation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB
// This notice, the author's name, and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/18/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;

namespace SandcastleBuilder.Package.Automation
{
    /// <summary>
    /// This is the automation object for the project
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public class OASandcastleBuilderProject : OAProject
    {
        #region Private data members
        //=====================================================================

        private readonly OAProperties properties;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the project properties
        /// </summary>
        public override EnvDTE.Properties Properties => properties;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project node</param>
        public OASandcastleBuilderProject(ProjectNode project) : base(project)
        {
            if(project != null)
                properties = new OAProperties(project.NodeProperties);
        }
        #endregion
    }
}
