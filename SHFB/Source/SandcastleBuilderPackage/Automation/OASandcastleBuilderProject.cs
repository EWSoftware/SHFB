//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : OASandcastleBuilderProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/12/2016
// Note    : Copyright 2011-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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

        private OAProperties properties;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the project properties
        /// </summary>
        public override EnvDTE.Properties Properties
        {
            get { return properties; }
        }
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
