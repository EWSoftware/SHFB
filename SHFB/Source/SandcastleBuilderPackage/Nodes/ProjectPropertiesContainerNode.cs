//=============================================================================
// System  : Sandcastle Help File Builder Package
// File    : ProjectPropertiesContainerNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents the project properties container
// node in a Sandcastle Help File Builder project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  04/08/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Project;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using WindowCommandIds = Microsoft.VisualStudio.VSConstants.VsUIHierarchyWindowCmdIds;

using Microsoft.VisualStudio;

using SandcastleBuilder.Package.Automation;
using SandcastleBuilder.Package.Properties;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This represents the project properties container node in a Sancastle
    /// Help File Builder project.
    /// </summary>
    /// <remarks>Unlike normal Visual Studio projects, we will not support
    /// child nodes here.  This is just a convenient and obvious way to direct
    /// the user to the main set of project properties.</remarks>
    [CLSCompliant(false), ComVisible(true)]
    public class ProjectPropertiesContainerNode : HierarchyNode
    {
        #region Private data members and constants
        //=====================================================================

        internal const string PropertiesNodeVirtualName = "SHFB_ProjectProperties";
		#endregion

		#region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set the sort priority for the node
        /// </summary>
        /// <value>It will be sorted just above the Documentation Sources node</value>
		public override int SortPriority
		{
			get { return DefaultSortOrderNode.ReferenceContainerNode - 2; }
		}

		/// <summary>
		/// This is used to return the menu command ID for the node
		/// </summary>
        /// <remarks>This uses the References context menu but only our command
        /// shows up.</remarks>
        public override int MenuCommandId
		{
            get { return Microsoft.VisualStudio.Project.VsMenus.IDM_VS_CTXT_REFERENCE; }
		}

        /// <summary>
        /// This is overridden to return the item type GUID
        /// </summary>
        /// <value>It returns the standard GUID for a virtual folder</value>
		public override Guid ItemTypeGuid
		{
			get { return VSConstants.GUID_ItemType_VirtualFolder; }
		}

        /// <summary>
        /// This is overridden to return the URL for the node
        /// </summary>
        /// <value>It returns the virtual node name</value>
		public override string Url
		{
			get { return base.VirtualNodeName; }
		}

        /// <summary>
        /// This is overridden to return the caption for the node
        /// </summary>
        /// <value>This returns the localized "Documentation Sources" caption</value>
		public override string Caption
		{
			get { return Resources.PropertiesNodeCaption; }
		}
		#endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        public ProjectPropertiesContainerNode(ProjectNode root) : base(root)
        {
            this.VirtualNodeName = PropertiesNodeVirtualName;
            this.ExcludeNodeFromScc = true;
        }
        #endregion

		#region Overridden methods
        //=====================================================================

		/// <summary>
		/// This is overridden to disable inline editing of the node's caption
		/// </summary>
		/// <returns>Returns null</returns>
		public override string GetEditLabel()
		{
			return null;
		}

        /// <summary>
        /// This is overridden to get the node icon handle
        /// </summary>
        /// <param name="open">A flag indicating whether the folder is open
        /// or closed</param>
        /// <returns>Returns the handle to the icon to use for the node</returns>
		public override object GetIconHandle(bool open)
		{
            return this.ProjectMgr.ImageHandler.GetIconHandle(this.ProjectMgr.ImageIndex +
                (int)ProjectImageIndex.ProjectProperties);
        }

		/// <summary>
		/// This is overridden to prevent the node from being dragged
		/// </summary>
		/// <returns>Always returns null</returns>
		protected internal override StringBuilder PrepareSelectedNodesForClipBoard()
		{
			return null;
		}

		/// <summary>
		/// This is overridden to prevent the node from being excluded from
        /// the project.
		/// </summary>
        /// <returns>Always returns <c>OleConstants.OLECMDERR_E_NOTSUPPORTED</c></returns>
		protected override int ExcludeFromProject()
		{
			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

        /// <summary>
        /// This is overridden to handle command status on the node. 
        /// </summary>
        /// <param name="cmdGroup">A unique identifier of the command group.
        /// The <c>pguidCmdGroup</c> parameter can be null to specify the
        /// standard group.</param>
        /// <param name="cmd">The command to query status for.</param>
        /// <param name="pCmdText">Pointer to an <c>OLECMDTEXT</c> structure in
        /// which to return the name and/or status information of a single
        /// command. Can be null to indicate that the caller does not require
        /// this information.</param>
        /// <param name="result">An out parameter specifying the
        /// <c>QueryStatusResult</c> of the command.</param>
        /// <returns>If the method succeeds, it returns <c>S_OK</c>. If it
        /// fails, it returns an error code.</returns>
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
        /// This is overridden to handle command execution for the node
        /// </summary>
        /// <param name="cmdGroup">Unique identifier of the command group</param>
        /// <param name="cmd">The command to be executed.</param>
        /// <param name="nCmdexecopt">Values describe how the object should
        /// execute the command.</param>
        /// <param name="pvaIn">Pointer to a <c>VARIANTARG</c> structure
        /// containing input arguments. Can be NULL</param>
        /// <param name="pvaOut"><c>VARIANTARG</c> structure to receive command
        /// output.  It can be null.</param>
        /// <returns>If the method succeeds, it returns <c>S_OK</c>. If it
        /// fails, it returns an error code.</returns>
        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn,
          IntPtr pvaOut)
		{
            // Open the Project Properties window when double-clicked or Properties is selected
            if((cmdGroup == VsMenus.guidStandardCommandSet97 && (VsCommands)cmd == VsCommands.PropSheetOrProperties) ||
              (cmdGroup == VsMenus.guidVSUISet && (WindowCommandIds)cmd == WindowCommandIds.UIHWCMDID_DoubleClick))
            {
                IntPtr ip = (IntPtr)(-1);
                IVsWindowFrame frame = null;

                ((IVsProject2)this.ProjectMgr).ReopenItem(VSConstants.VSITEMID_ROOT,
                    VSConstants.GUID_ProjectDesignerEditor, null, Guid.Empty, ip, out frame);

                return VSConstants.S_OK;
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
		}

        /// <summary>
        /// This is overridden to prevent deletion of items
        /// </summary>
        /// <param name="deleteOperation">The delete operation</param>
        /// <returns>Always returns false</returns>
		protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
		{
			return false;
		}

		/// <summary>
		/// This is overridden to defines whether this node can show the
        /// default icon.
		/// </summary>
		/// <returns>Returns true if the virtual node name is not null and
        /// is not empty or false if it is.</returns>
		protected override bool CanShowDefaultIcon()
		{
            return !String.IsNullOrEmpty(this.VirtualNodeName);
		}
		#endregion
    }
}
