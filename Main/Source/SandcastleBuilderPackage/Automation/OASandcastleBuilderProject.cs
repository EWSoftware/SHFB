//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : OASandcastleBuilderProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/23/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used for project automation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/18/2011  EFW  Created the code
//=============================================================================

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
            {
                properties = new OAProperties(project.NodeProperties);

                // TODO: Is there a better way to handle these?

                // TODO: Can we get rid of the TargetFramework, AssemblyName and RootNamespace
                // project properties this way too?

                // The IDE always requests these for some reason when closing a project but we
                // don't contain them so we'll just add entries for them that return null.
                // There may be others.  They are logged to the Output Window when encountered.
                properties.Properties["DesignTimeReferences"] =
                    properties.Properties["SDEProjectExtender.NETCFVersion"] =
                    properties.Properties["SDEProjectExtender.PlatformFamily"] =
                    properties.Properties["WebSiteType"] = null;
            }
        }
        #endregion
    }
}
