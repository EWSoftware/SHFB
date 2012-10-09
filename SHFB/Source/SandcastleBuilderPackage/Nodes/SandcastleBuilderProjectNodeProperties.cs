//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderProjectNodeProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/23/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that exposes the properties for the
// SandcastleBuilderProjectNode object.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/22/2011  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to expose the properties for
    /// <see cref="SandcastleBuilderProjectNode" /> objects.
    /// </summary>
    /// <remarks>This class must be public and marked as ComVisible in order
    /// for the <c>DispatchWrapper</c> to work correctly.</remarks>
    [CLSCompliant(false), ComVisible(true), Guid("E412091C-17F0-4d50-A681-4A77BC3E3297")]
    public sealed class SandcastleBuilderProjectNodeProperties : ProjectNodeProperties
    {
        #region Properties
        //=====================================================================
// TODO: Still needed?
        /// <summary>
        /// Get or set the target framework property
        /// </summary>
        /// <remarks>This one does not seem to exist in the constants but the project
        /// looks for it when a new item is added.  It is not used by the
        /// help file builder project but must exist or an exception is thrown.</remarks>
        [Browsable(false)]
        public string TargetFramework
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty("TargetFramework");
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty("TargetFramework", value);
            }
        }

        /// <summary>
        /// Get or set the assembly name property
        /// </summary>
        /// <remarks>This is not used by the help file builder project but must
        /// exist or an exception is thrown.</remarks>
        [Browsable(false)]
        public string AssemblyName
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(ProjectFileConstants.AssemblyName);
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(ProjectFileConstants.AssemblyName, value);
            }
        }

        /// <summary>
        /// Get or set the root namespace property
        /// </summary>
        /// <remarks>This is not used by the help file builder project but must
        /// exist or an exception is thrown.</remarks>
        [Browsable(false)]
        public string RootNamespace
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(ProjectFileConstants.RootNamespace);
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(ProjectFileConstants.RootNamespace, value);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">The node that contains the properties to expose
        /// via the Property Browser.</param>
        public SandcastleBuilderProjectNodeProperties(SandcastleBuilderProjectNode node) : base(node)
        {
        }
        #endregion
    }
}
