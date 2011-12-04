//=============================================================================
// System  : Sandcastle Help File Builder Package
// File    : DocumentationSourcesContainerNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/20/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents the documentation sources
// container node in a Sandcastle Help File Builder project.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Project;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;

using Microsoft.VisualStudio;

using SandcastleBuilder.Package.Automation;
using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This represents the documentation sources container node in a
    /// Sandcastle Help File Builder project.
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public sealed class DocumentationSourcesContainerNode : HierarchyNode
    {
        #region Private data members and constants
        //=====================================================================

        internal const string DocSourcesNodeVirtualName = "DocSources";

        // If I'd though this through a little better, I'd have made these project elements rather than
        // stuffing them into a property.  I can't be bothered to rewrite everything to work that way
        // so we'll use an XDocument to manage the documentation sources.
        private XDocument documentationSources;
		#endregion

		#region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set the sort priority for the node
        /// </summary>
        /// <value>It will be sorted just above the References node</value>
		public override int SortPriority
		{
			get { return DefaultSortOrderNode.ReferenceContainerNode - 1; }
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
			get { return Resources.DocSourcesNodeCaption; }
		}
		#endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        public DocumentationSourcesContainerNode(ProjectNode root) : base(root)
        {
            this.VirtualNodeName = DocSourcesNodeVirtualName;
            this.ExcludeNodeFromScc = true;

            documentationSources = new XDocument();
            documentationSources.Add(new XElement("DocumentationSources"));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This updates the documentation sources property in the project
        /// </summary>
        public void StoreDocumentationSources()
        {
            // Check out the project file if necessary
            if(!this.ProjectMgr.QueryEditProjectFile(false))
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);

            var reader = documentationSources.CreateReader();
            reader.MoveToContent();
            this.ProjectMgr.SetProjectProperty("DocumentationSources", reader.ReadInnerXml());
        }

        /// <summary>
        /// Adds documentation sources to this container from an MSBuild project
        /// </summary>
        /// <param name="buildProject">The build project from which to load the information</param>
        public void LoadDocSourcesFromBuildProject(Project buildProject)
        {
            ProjectProperty prop;
            string docSources = null;

            prop = prop = this.ProjectMgr.BuildProject.GetProperty("DocumentationSources");

            if(prop != null)
                docSources = prop.UnevaluatedValue;

            if(String.IsNullOrEmpty(docSources))
                return;

            documentationSources = XDocument.Parse("<DocumentationSources>" + docSources + "</DocumentationSources>");

            foreach(var ds in documentationSources.Root.Descendants())
                this.AddChild(new DocumentationSourceNode(this.ProjectMgr, ds));
        }

        /// <summary>
        /// This is used to add documentation sources to the project
        /// </summary>
        /// <remarks>Documentation sources can be assemblies, XML comments
        /// files, Visual Studio solution files, or Visual Studio project
        /// files.</remarks>
        private void AddDocumentationSources()
        {
            string ext, otherFile;

            // Check out the project file if necessary
            if(!this.ProjectMgr.QueryEditProjectFile(false))
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the documentation source(s)";
                dlg.Filter = "Assemblies, Comments Files, and Projects" +
                    "(*.dll, *.exe, *.xml, *.sln, *.*proj)|" +
                    "*.dll;*.exe;*.xml;*.sln;*.*proj|" +
                    "Library Files (*.dll)|*.dll|" +
                    "Executable Files (*.exe)|*.exe|" +
                    "XML Comments Files (*.xml)|*.xml|" +
                    "Visual Studio Solution Files (*.sln)|*.sln|" +
                    "Visual Studio Project Files (*.*proj)|*.*proj|" +
                    "All Files (*.*)|*.*";
                dlg.InitialDirectory = this.ProjectMgr.ProjectFolder;
                dlg.DefaultExt = "dll";
                dlg.Multiselect = true;

                // If selected, add the new file(s)
                if(dlg.ShowDialog() == DialogResult.OK)
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        foreach(string file in dlg.FileNames)
                        {
                            this.AddDocumentationSource(file);
                            ext = Path.GetExtension(file).ToLower(CultureInfo.InvariantCulture);

                            // If there's a match for a comments file or an assembly, add it too
                            if(ext == ".xml")
                            {
                                otherFile = Path.ChangeExtension(file, ".dll");

                                if(File.Exists(otherFile))
                                    this.AddDocumentationSource(otherFile);
                                else
                                {
                                    otherFile = Path.ChangeExtension(file, ".exe");

                                    if(File.Exists(otherFile))
                                        this.AddDocumentationSource(otherFile);
                                }
                            }
                            else
                                if(ext == ".dll" || ext == ".exe")
                                {
                                    otherFile = Path.ChangeExtension(file, ".xml");

                                    if(File.Exists(otherFile))
                                        this.AddDocumentationSource(otherFile);
                                }
                        }
                    }
                    finally
                    {
                        this.StoreDocumentationSources();
                        Cursor.Current = Cursors.Default;
                    }
            }
        }

        /// <summary>
        /// Add a new documentation source node
        /// </summary>
        /// <param name="filename">The filename for the documentation source</param>
        internal void AddDocumentationSource(string filename)
        {
            // Default to using a relative path based on the project folder
            filename = FolderPath.AbsoluteToRelativePath(
                Path.GetDirectoryName(this.ProjectMgr.BuildProject.FullPath), filename);

            XElement docSource = new XElement("DocumentationSource", new XAttribute("sourceFile", filename));

            if(!documentationSources.Descendants("DocumentationSource").Any(
              d => d.Attribute("sourceFile").Value.Equals(filename, StringComparison.OrdinalIgnoreCase)))
            {
                documentationSources.Root.Add(docSource);
                this.AddChild(new DocumentationSourceNode(this.ProjectMgr, docSource));
            }
        }
        #endregion

		#region Overridden methods
        //=====================================================================

        /// <summary>
        /// This returns the automation object for the node
        /// </summary>
        /// <returns>The automation object for the node</returns>
        public override object GetAutomationObject()
		{
			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
				return null;

            return new OADocSourcesFolderItem(this.ProjectMgr.GetAutomationObject()
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
        /// <param name="open">A flag indicating whether the folder is open
        /// or closed</param>
        /// <returns>Returns the handle to the icon to use for the node</returns>
		public override object GetIconHandle(bool open)
		{
            return this.ProjectMgr.ImageHandler.GetIconHandle(this.ProjectMgr.ImageIndex + (open ?
                (int)ProjectImageIndex.DocumentationSourcesOpen :
                (int)ProjectImageIndex.DocumentationSourcesClosed));
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
                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                return VSConstants.S_OK;
            }

            if(cmdGroup == VsMenus.guidStandardCommandSet97 && (VsCommands)cmd == VsCommands.PropSheetOrProperties)
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
            if(cmdGroup == GuidList.guidSandcastleBuilderPackageCmdSet && (int)cmd == PkgCmdIDList.AddDocSource)
            {
                this.AddDocumentationSources();
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

        /// <summary>
        /// This is overridden to remove the documentation source from the project property
        /// </summary>
        /// <param name="node">The node being removed</param>
        public override void RemoveChild(HierarchyNode node)
        {
            DocumentationSourceNode docSource = node as DocumentationSourceNode;

            if(node != null)
            {
                docSource.DocumentationSource.Remove();
                this.StoreDocumentationSources();
            }

            base.RemoveChild(node);
        }
		#endregion
    }
}
