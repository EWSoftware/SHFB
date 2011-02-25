//=============================================================================
// System  : Sandcastle Help File Builder
// File    : ProjectPropertiesWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/28/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the project properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to edit the project properties
    /// </summary>
    public partial class ProjectPropertiesWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get { return currentProject; }
            set
            {
                currentProject = value;
                pgProps.SelectedObject = value;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectPropertiesWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Update the Sandcastle path in the component configuration designer
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void pgProps_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string scPath = BuildComponentManager.SandcastlePath,
                   prjPath = currentProject.SandcastlePath;

            if((String.IsNullOrEmpty(scPath) && prjPath.Length != 0) ||
              (!String.IsNullOrEmpty(scPath) && prjPath.Length == 0) ||
              (!String.IsNullOrEmpty(scPath) && prjPath.Length != 0 &&
              scPath != prjPath))
                BuildComponentManager.SandcastlePath = prjPath;
        }
        #endregion

        #region Methods overrides and helper methods
        //=====================================================================

        /// <summary>
        /// Reset the property grid splitter position after being made visible
        /// </summary>
        /// <param name="e">The event arguments</param>
        /// <remarks>If not, it tends to be way to large</remarks>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if(this.Visible)
                pgProps.PropertyNamePaneWidth = 330;
        }

        /// <summary>
        /// This is overridden to ignore Ctrl+F4 which closes the window rather
        /// than hide it when docked as a document.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing &&
              this.DockState == DockState.Document)
            {
                this.Hide();
                e.Cancel = true;
            }
            else
                base.OnFormClosing(e);
        }

        /// <inheritdoc />
        public override bool IsDirty
        {
            get { return MainForm.Host.ProjectExplorer.IsDirty; }
            set { /* Handled by the property grid and main form */ }
        }

        /// <inheritdoc />
        public override bool CanClose
        {
            get { return MainForm.Host.ProjectExplorer.CanClose; }
        }

        /// <inheritdoc />
        public override bool CanSaveContent
        {
            get { return MainForm.Host.ProjectExplorer.CanSaveContent; }
        }

        /// <inheritdoc />
        public override bool Save()
        {
            return MainForm.Host.ProjectExplorer.Save();
        }

        /// <inheritdoc />
        public override bool SaveAs()
        {
            bool result = MainForm.Host.ProjectExplorer.SaveAs();

            if(result)
                this.RefreshProperties();

            return result;
        }

        /// <summary>
        /// This is called to refresh the property grid after a change that
        /// may have affected it.
        /// </summary>
        public void RefreshProperties()
        {
            if(currentProject != null)
                pgProps.Refresh();
        }
        #endregion
    }
}
