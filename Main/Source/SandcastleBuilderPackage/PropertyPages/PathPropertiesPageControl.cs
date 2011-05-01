//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PathPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/16/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Path category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Path category project properties
    /// </summary>
    [Guid("70E698CB-B4C8-4746-B36F-983D72763543")]
    public partial class PathPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PathPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Paths";
            this.HelpKeyword = "e6fcfa33-e7ee-430a-abfe-6b7962e6d068";
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                txtOutputPath.Text = txtOutputPath.Text.Trim();

                if(txtOutputPath.Text.Length == 0)
                    txtOutputPath.Text = @".\Help\";
                else
                    txtOutputPath.Text = FolderPath.TerminatePath(txtOutputPath.Text);

                return true;
            }
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            // Set the project as the base path provider so that the folder is correct
            if(base.ProjectMgr != null)
            {
                SandcastleProject project = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;

                txtHtmlHelp1xCompilerPath.Folder = new FolderPath(project);
                txtHtmlHelp2xCompilerPath.Folder = new FolderPath(project);
                txtOutputPath.Folder = new FolderPath(project);
                txtSandcastlePath.Folder = new FolderPath(project);
                txtWorkingPath.Folder = new FolderPath(project);
            }
        }

        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// If the Output Path is blank, default it to the .\Help folder under the project's path
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtOutputPath_PersistablePathChanged(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(txtOutputPath.PersistablePath))
                txtOutputPath.PersistablePath = @".\Help";
        }
        #endregion
    }
}
