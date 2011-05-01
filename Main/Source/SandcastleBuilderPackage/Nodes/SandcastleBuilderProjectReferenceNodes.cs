//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderProjectReferenceNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/04/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents project reference nodes in the
// project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/23/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to represent a project reference in the project
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public sealed class SandcastleBuilderProjectReferenceNode : ProjectReferenceNode
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        /// <param name="element">The project element</param>
        /// <overloads>There are two overloads for the constructor</overloads>
		public SandcastleBuilderProjectReferenceNode(ProjectNode root, ProjectElement element) :
          base(root, element)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        /// <param name="referencedProjectName">The referenced project name</param>
        /// <param name="projectPath">The path to the project</param>
        /// <param name="projectReference">The project reference</param>
		public SandcastleBuilderProjectReferenceNode(ProjectNode root, string referencedProjectName,
          string projectPath, string projectReference) :
            base(root, referencedProjectName, projectPath, projectReference)
        {
        }

        /// <summary>
        /// This is overridden to return a GUID for the node
        /// </summary>
        public override Guid ItemTypeGuid
        {
            get { return new Guid("0126ADCD-65D0-4026-8E2C-9DBCDBECA95C"); }
        }

        /// <inheritdoc />
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText,
          ref QueryStatusResult result)
        {
            if(cmdGroup == GuidList.guidSandcastleBuilderPackageCmdSet && cmd == PkgCmdIDList.AddDocSource)
            {
                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                return VSConstants.S_OK;
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
    }
}
