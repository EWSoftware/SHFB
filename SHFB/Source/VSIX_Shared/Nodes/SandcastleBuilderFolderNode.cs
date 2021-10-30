//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderFolderNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/11/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class that represents a folder node in a Sandcastle Help File Builder Visual Studio
// project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/11/2021  EFW  Created the code
// 04/08/2012  EFW  Added support for XAML configuration files
//===============================================================================================================

using System;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.WPF.UI;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This class represents a folder node in a Sandcastle Help File Builder Visual Studio project
    /// </summary>
    public class SandcastleBuilderFolderNode : FolderNode
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">Root node of the hierarchy</param>
        /// <param name="relativePath">Relative path from root i.e.: "NewFolder1\\NewFolder2\\NewFolder3</param>
        /// <param name="element">Associated project element</param>
        public SandcastleBuilderFolderNode(ProjectNode root, string relativePath, ProjectElement element) :
          base(root, relativePath, element)
        {
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
    }
}
