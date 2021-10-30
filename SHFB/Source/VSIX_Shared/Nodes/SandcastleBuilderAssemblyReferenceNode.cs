//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderAssemblyReferenceNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class that represents assembly reference nodes in the project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/23/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to represent an assembly reference in the project
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public sealed class SandcastleBuilderAssemblyReferenceNode : AssemblyReferenceNode
    {
        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        /// <param name="element">The project element</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public SandcastleBuilderAssemblyReferenceNode(ProjectNode root, ProjectElement element) :
          base(root, element)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        /// <param name="assemblyPath">The path to the assembly</param>
        public SandcastleBuilderAssemblyReferenceNode(ProjectNode root, string assemblyPath) :
          base(root, assemblyPath)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to return a GUID for the node
        /// </summary>
        public override Guid ItemTypeGuid => new Guid("F33579D8-E01E-4e6f-9C70-9A8027B329F0");

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

        /// <summary>
        /// This is overridden to tell the project that the references node should handle drop
        /// operations.
        /// </summary>
        /// <returns>The documentation sources parent node</returns>
        protected internal override HierarchyNode GetDragTargetHandlerNode()
        {
            return this.Parent;
        }
        #endregion
    }
}
