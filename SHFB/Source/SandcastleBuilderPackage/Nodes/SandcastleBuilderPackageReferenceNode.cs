//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderPackageReferenceNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/22/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the class that represents a package reference node.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/22/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This represents a package reference node in a help file builder project
    /// </summary>
    /// <remarks>These are for display only to show additional build component included in the project via NuGet</remarks>
    [CLSCompliant(false), ComVisible(true)]
    public class SandcastleBuilderPackageReferenceNode : ReferenceNode
    {
        #region Properties

        /// <inheritdoc />
        public override string Url { get; }

        /// <inheritdoc />
        public override string Caption { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public SandcastleBuilderPackageReferenceNode(ProjectNode root, ProjectElement element) : base(root, element)
        {
            this.Url = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
            this.Caption = $"{this.Url} ({this.ItemNode.GetMetadata("Version")})";
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <inheritdoc />
        protected override void BindReferenceData()
        {
        }

        /// <inheritdoc />
        protected override NodeProperties CreatePropertiesObject()
        {
            return null;
        }

        /// <inheritdoc />
        protected internal override bool IsAlreadyAdded(out ReferenceNode existingReference)
        {
            existingReference = null;
            return false;
        }

        /// <inheritdoc />
        public override object GetIconHandle(bool open)
        {
            return this.ProjectMgr.ImageHandler.GetIconHandle((int)ProjectNode.ImageName.NuGet);
        }

        /// <inheritdoc />
        protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            return false;
        }

        /// <inheritdoc />
        protected override bool CanShowDefaultIcon()
        {
            return true;
        }
        #endregion
    }
}
