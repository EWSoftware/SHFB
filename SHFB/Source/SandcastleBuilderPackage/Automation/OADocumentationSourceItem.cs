//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : OADocumentationSourceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used for documentation source automation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/30/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;

using SandcastleBuilder.Package.Nodes;

namespace SandcastleBuilder.Package.Automation
{
    /// <summary>
    /// Represents the automation object equivalent to a <see cref="DocumentationSourceNode"/> object
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public class OADocumentationSourceItem : OAProjectItem<DocumentationSourceNode>
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project item</param>
        /// <param name="node">The documentation source node</param>
        public OADocumentationSourceItem(OAProject project, DocumentationSourceNode node) : base(project, node)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        public override string Name
        {
            get => base.Name;
            set => throw new InvalidOperationException();
        }

        /// <summary>
        /// Not implemented. If called throws invalid operation exception.
        /// </summary>	
        public override void Delete()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Not implemented. If called throws invalid operation exception.
        /// </summary>
        /// <param name="viewKind">A view kind indicating the type of view to use.</param>
        /// <returns>Not applicable</returns>
        public override EnvDTE.Window Open(string viewKind)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the ProjectItems collection containing the ProjectItem object supporting this property.
        /// </summary>
        public override EnvDTE.ProjectItems Collection => UIThread.DoOnUIThread(() =>
        {
            // Get the parent node
            var parentNode = this.Node.Parent as DocumentationSourcesContainerNode;

            System.Diagnostics.Debug.Assert(parentNode != null, "Failed to get the parent node");

            // Get the ProjectItems object for the parent node
            if(parentNode != null)
                return ((OADocSourcesFolderItem)parentNode.GetAutomationObject()).ProjectItems;

            return null;
        });

        #endregion
    }
}
