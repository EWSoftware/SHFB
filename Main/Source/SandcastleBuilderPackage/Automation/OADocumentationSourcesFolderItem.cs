//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : OADocumentationSourcesFolderItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/30/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used for documentation source folder automation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/30/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;

using SandcastleBuilder.Package.Nodes;

namespace SandcastleBuilder.Package.Automation
{
    /// <summary>
    /// This is the automation object for the documentation sources folder node
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public class OADocSourcesFolderItem : OAProjectItem<DocumentationSourcesContainerNode>
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project node</param>
        /// <param name="node">The documentation sources container node</param>
        public OADocSourcesFolderItem(OAProject project, DocumentationSourcesContainerNode node) :
          base(project, node)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Returns the project items collection of all the documentation
        /// sources defined for this project.
        /// </summary>
        public override EnvDTE.ProjectItems ProjectItems
        {
            get
            {
                List<EnvDTE.ProjectItem> list = new List<EnvDTE.ProjectItem>();

                // Get a list of the project items in the node
                for(HierarchyNode child = this.Node.FirstChild; child != null; child = child.NextSibling)
                    if(child is DocumentationSourceNode)
                        list.Add(new OADocumentationSourceItem(this.Project,
                            child as DocumentationSourceNode));

                return new OANavigableProjectItems(this.Project, list, this.Node);
            }
        }
        #endregion
    }
}
