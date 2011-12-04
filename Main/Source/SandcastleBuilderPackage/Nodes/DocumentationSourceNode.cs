//=============================================================================
// System  : Sandcastle Help File Builder Package
// File    : DocumentationSourceNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/19/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents a documentation source in a
// Sandcastle Help File Builder project.
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
// 1.9.3.3  11/19/2011  EFW  Added support for drag and drop from Explorer
//=============================================================================

using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Package.Automation;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This represents a documentation source in a Sandcastle Help File
    /// Builder project.
    /// </summary>
    public sealed class DocumentationSourceNode : HierarchyNode
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the <see cref="XElement"/> that represents
        /// the documentation source.
        /// </summary>
        public XElement DocumentationSource { get; private set; }

        /// <summary>
        /// This is overridden to return the menu command ID for the node
        /// </summary>
        /// <remarks>This uses the same menu command ID as a Reference node</remarks>
        public override int MenuCommandId
        {
            get { return VsMenus.IDM_VS_CTXT_REFERENCE; }
        }

        /// <summary>
        /// Get the URL for the documentation source
        /// </summary>
        public override string Url
        {
            get { return this.DocumentationSource.Attribute("sourceFile").Value; }
        }

        /// <summary>
        /// Get the caption for the documentation source
        /// </summary>
        public override string Caption
        {
            get { return Path.GetFileName(this.Url); }
        }

        /// <summary>
        /// Get the item type guid for the item
        /// </summary>
        public override Guid ItemTypeGuid
        {
            get { return new Guid("9873897F-9B4E-433b-BEB9-C2678A729FFC"); }
        }
        #endregion

		#region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        /// <param name="docSource">The <see cref="XElement"/> representing the documentation source</param>
        public DocumentationSourceNode(ProjectNode root, XElement docSource) : base(root)
		{
            this.DocumentationSource = docSource;
			this.ExcludeNodeFromScc = true;
        }
        #endregion

        #region Overridden methods
        //=====================================================================

        /// <summary>
        /// Creates an object derived from <see cref="NodeProperties"/> that
        /// will be used to expose properties specific for this object to the
        /// property browser.
        /// </summary>
        /// <returns>A new <see cref="SandcastleBuilderFileNodeProperties"/>
        /// object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            return new DocumentationSourceNodeProperties(this);
        }

        /// <summary>
        /// This returns the automation object for the node
        /// </summary>
        /// <returns>The automation object for the node</returns>
        public override object GetAutomationObject()
		{
			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
				return null;

            return new OADocumentationSourceItem(this.ProjectMgr.GetAutomationObject()
                as OASandcastleBuilderProject, this);
        }

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
        /// <param name="open">Not used by this node</param>
        /// <returns>Returns the handle to the icon to use for the node</returns>
        public override object GetIconHandle(bool open)
		{
			return this.ProjectMgr.ImageHandler.GetIconHandle(this.ProjectMgr.ImageIndex +
                (int)ProjectImageIndex.DocumentationSource);
		}

		/// <summary>
		/// This method is called by the interface method <c>GetMkDocument</c>
        /// to specify the item moniker.
		/// </summary>
		/// <returns>The moniker for this item</returns>
		public override string GetMkDocument()
		{
			return this.Url;
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
        /// This is overridden to prevent deletion of items but allow removal
        /// </summary>
        /// <param name="deleteOperation">The delete operation</param>
        /// <returns>Returns true to allow requests for removal and false to
        /// prevent requests for deletion.</returns>
        protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            return (deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_RemoveFromProject);
        }

        /// <summary>
        /// This is overridden to tell the project that the documentation sources node should handle drop
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
