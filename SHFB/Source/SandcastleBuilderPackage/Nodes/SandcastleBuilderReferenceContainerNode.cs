//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderReferenceContainerNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/21/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class that represents a reference container node.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.WPF.UI;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to represent the reference container node.
    /// </summary>
    /// <remarks>This handles creation of reference nodes by returning modified
    /// versions of the base reference types that contain an item type GUID to
    /// prevent the shell throwing exceptions because the base types do not
    /// define GUIDs for them.</remarks>
    [CLSCompliant(false), ComVisible(true)]
    public sealed class SandcastleBuilderReferenceContainerNode : ReferenceContainerNode
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root">The root project node</param>
        public SandcastleBuilderReferenceContainerNode(ProjectNode root) : base(root)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn,
          IntPtr pvaOut)
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

        #endregion

        #region Helper functions to add references
        //=====================================================================

        /// <summary>
        /// Creates a project reference node given an existing project element.
        /// </summary>
        protected override ProjectReferenceNode CreateProjectReferenceNode(ProjectElement element)
        {
            return new SandcastleBuilderProjectReferenceNode(this.ProjectMgr, element);
        }

        /// <summary>
        /// Create a Project to Project reference given a VSCOMPONENTSELECTORDATA structure
        /// </summary>
        protected override ProjectReferenceNode CreateProjectReferenceNode(
          VSCOMPONENTSELECTORDATA selectorData)
        {
            return new SandcastleBuilderProjectReferenceNode(this.ProjectMgr,
                selectorData.bstrTitle, selectorData.bstrFile, selectorData.bstrProjRef);
        }

        /// <summary>
        /// Creates an assembly reference node from a project element.
        /// </summary>
        /// <returns>An assembly reference node</returns>
        protected override AssemblyReferenceNode CreateAssemblyReferenceNode(ProjectElement element)
        {
            SandcastleBuilderAssemblyReferenceNode node = null;

            try
            {
                node = new SandcastleBuilderAssemblyReferenceNode(this.ProjectMgr, element);
            }
            catch(ArgumentNullException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileNotFoundException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(BadImageFormatException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileLoadException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(System.Security.SecurityException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }

            return node;
        }

        /// <summary>
        /// Creates an assembly reference node from a file path.
        /// </summary>
        /// <returns>An assembly reference node</returns>
        protected override AssemblyReferenceNode CreateAssemblyReferenceNode(string fileName)
        {
            SandcastleBuilderAssemblyReferenceNode node = null;

            try
            {
                node = new SandcastleBuilderAssemblyReferenceNode(this.ProjectMgr, fileName);
            }
            catch(ArgumentNullException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileNotFoundException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(BadImageFormatException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileLoadException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(System.Security.SecurityException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }

            return node;
        }

        /// <summary>
        /// Creates a COM reference node from the project element.
        /// </summary>
        /// <returns>A COM reference node</returns>
        protected override ComReferenceNode CreateComReferenceNode(ProjectElement reference)
        {
            return new SandcastleBuilderComReferenceNode(this.ProjectMgr, reference);
        }

        /// <summary>
        /// Creates a com reference node from a selector data.
        /// </summary>
        /// <returns>A COM reference node</returns>
        protected override ComReferenceNode CreateComReferenceNode(VSCOMPONENTSELECTORDATA selectorData,
          string wrapperTool = null)
        {
            return new SandcastleBuilderComReferenceNode(this.ProjectMgr, selectorData);
        }
        #endregion
    }
}
