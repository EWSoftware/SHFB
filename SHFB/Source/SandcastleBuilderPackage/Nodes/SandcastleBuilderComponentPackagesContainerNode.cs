//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderComponentPackagesContainerNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/22/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the class that represents the component packages container node.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/21/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.WPF.UI;

using MSBuild = Microsoft.Build.Evaluation;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to represent the component packages container node used to display any component NuGet
    /// package references in the help file builder project.
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public class SandcastleBuilderComponentPackagesContainerNode : HierarchyNode
    {
        #region Constants
        //=====================================================================

        /// <summary>
        /// This node name
        /// </summary>
        public const string ComponentPackagesNodeVirtualName = "Component Packages";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        public SandcastleBuilderComponentPackagesContainerNode(ProjectNode root) : base(root)
        {
            this.VirtualNodeName = ComponentPackagesNodeVirtualName;
            this.ExcludeNodeFromScc = true;
        }
        #endregion

        #region Overridden properties
        //=====================================================================

        /// <inheritdoc />
        public override int SortPriority => DefaultSortOrderNode.ReferenceContainerNode + 1;

        /// <inheritdoc />
        public override int MenuCommandId => VsMenus.IDM_VS_CTXT_REFERENCEROOT;

        /// <inheritdoc />
        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_VirtualFolder;

        /// <inheritdoc />
        public override string Url => this.VirtualNodeName;

        /// <inheritdoc />
        public override string Caption => this.VirtualNodeName;

        /// <inheritdoc />
        internal override object Object => this;

        #endregion

        #region Overridden methods
        //=====================================================================

        /// <summary>
        /// Disable inline editing of the node caption
        /// </summary>
        /// <returns>Always returns null</returns>
        public override string GetEditLabel()
        {
            return null;
        }

        /// <inheritdoc />
        public override object GetIconHandle(bool open)
        {
            return this.ProjectMgr.ImageHandler.GetIconHandle((int)ProjectNode.ImageName.NuGet);
        }

        /// <summary>
        /// This node type cannot be dragged.
        /// </summary>
        /// <returns>Always returns null</returns>
        protected internal override StringBuilder PrepareSelectedNodesForClipBoard()
        {
            return null;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        protected override int ExcludeFromProject()
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <inheritdoc />
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if(cmdGroup == VsMenus.guidStandardCommandSet97 || cmdGroup == VsMenus.guidStandardCommandSet2K)
                return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);

            return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
        }

        /// <inheritdoc />
        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Handle adding package references ourselves as Visual Studio doesn't currently support them in
            // third-party project systems.
            if(cmdGroup == GuidList.guidNuGetPackageManagerCmdSet && cmd == PkgCmdIDList.ManageNuGetPackages)
            {
                var dlg = new NuGetPackageManagerDlg(this.ProjectMgr.BuildProject);
                dlg.ShowModalDialog();
                return VSConstants.S_OK;
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <inheritdoc />
        protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            return false;
        }

        /// <summary>
        /// Defines whether this node is valid node for painting the references icon
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowDefaultIcon()
        {
            if(!String.IsNullOrEmpty(this.VirtualNodeName))
                return true;

            return false;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Adds package references to this container from an MSBuild project
        /// </summary>
        public void LoadComponentPackagesFromBuildProject()
        {
            foreach(MSBuild.ProjectItem item in this.ProjectMgr.BuildProject.GetItems("PackageReference"))
            {
                ProjectElement element = new ProjectElement(this.ProjectMgr, item, false);

                var node = new SandcastleBuilderPackageReferenceNode(this.ProjectMgr, element);

                bool found = false;

                for(HierarchyNode n = this.FirstChild; n != null && !found; n = n.NextSibling)
                    if(String.Compare(n.Caption, node.Caption, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        found = true;
                        break;
                    }

                if(!found)
                    this.AddChild(node);
            }
        }
        #endregion
    }
}
