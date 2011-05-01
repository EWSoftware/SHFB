//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderReferenceContainerNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/04/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents a reference container node.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

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
        /// Creates an assembly refernce node from a project element.
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
