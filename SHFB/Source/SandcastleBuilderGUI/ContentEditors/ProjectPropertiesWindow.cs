//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ProjectPropertiesWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/23/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the project properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.8.0.0  07/27/2008  EFW  Created the code
// 1.9.6.0  10/28/2012  EFW  Rewrote to use the property pages from the VSPackage
//===============================================================================================================

using System;
using System.Drawing;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Package.PropertyPages;

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
                tvPropertyPages_AfterSelect(this, new TreeViewEventArgs(tvPropertyPages.SelectedNode));
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
            BasePropertyPage page;
            TreeNode node;

            // The property pages will be listed in this order
            Type[] propertyPages = new[] {
                typeof(BuildPropertiesPageControl),
                typeof(HelpFilePropertiesPageControl),
                typeof(Help1WebsitePropertiesPageControl),
                typeof(MSHelp2PropertiesPageControl),
                typeof(MSHelpViewerPropertiesPageControl),
                typeof(SummaryPropertiesPageControl),
                typeof(VisibilityPropertiesPageControl),
                typeof(MissingTagPropertiesPageControl),
                typeof(PathPropertiesPageControl),
                typeof(ComponentPropertiesPageControl),
                typeof(PlugInPropertiesPageControl),
                typeof(TransformArgumentsPageControl),
                typeof(UserDefinedPropertiesPageControl)
            };

            InitializeComponent();

            // Ensure that the file and folder path user controls are known by the base property page class
            if(!BasePropertyPage.CustomControls.ContainsKey(typeof(SandcastleBuilder.Utils.Controls.FilePathUserControl).Name))
            {
                BasePropertyPage.CustomControls.Add(typeof(SandcastleBuilder.Utils.Controls.FilePathUserControl).FullName,
                    "PersistablePath");
                BasePropertyPage.CustomControls.Add(typeof(SandcastleBuilder.Utils.Controls.FolderPathUserControl).FullName,
                    "PersistablePath");
            }

            try
            {
                tvPropertyPages.BeginUpdate();

                // Create the property pages
                foreach(Type pageType in propertyPages)
                {
                    page = (BasePropertyPage)Activator.CreateInstance(pageType);
                    page.Visible = false;

                    node = tvPropertyPages.Nodes.Add(page.Title);
                    node.Tag = page;

                    pnlPropertyPages.Controls.Add(page);
                }

                if(tvPropertyPages.Nodes.Count != 0)
                {
                    if(Settings.Default.LastUsedPropertyPage >=0 &&
                      Settings.Default.LastUsedPropertyPage < tvPropertyPages.Nodes.Count)
                        tvPropertyPages.SelectedNode = tvPropertyPages.Nodes[Settings.Default.LastUsedPropertyPage];
                    else
                        tvPropertyPages.SelectedNode = tvPropertyPages.Nodes[0];
                }
            }
            finally
            {
                tvPropertyPages.EndUpdate();
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Resize the property pages when the parent panel size changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pnlPropertyPages_SizeChanged(object sender, EventArgs e)
        {
            Size newSize;

            foreach(Control c in pnlPropertyPages.Controls)
            {
                newSize = pnlPropertyPages.Size;

                // If a vertical scrollbar will be shown, shrink the width to prevent an unnecessary
                // horizontal scrollbar.
                if(newSize.Height < c.MinimumSize.Height)
                    newSize.Width -= SystemInformation.VerticalScrollBarWidth;

                c.Size = newSize;
            }
        }

        /// <summary>
        /// Validate the current property page before allowing movement to another property page
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvPropertyPages_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if(tvPropertyPages.SelectedNode != null)
            {
                var page = (BasePropertyPage)tvPropertyPages.SelectedNode.Tag;

                if(!page.Apply())
                    e.Cancel = true;
                else
                    page.Visible = false;
            }
        }

        /// <summary>
        /// Switch to the new property page and bind it if necessary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvPropertyPages_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(tvPropertyPages.SelectedNode != null)
            {
                var page = (BasePropertyPage)tvPropertyPages.SelectedNode.Tag;

                page.SetProject(currentProject);
                page.Visible = (currentProject != null);
            }
        }
        #endregion

        #region Methods overrides and helper methods
        //=====================================================================

        /// <summary>
        /// This is overridden to show help for the current property page
        /// </summary>
        /// <param name="msg">The message to process</param>
        /// <param name="keyData">The key data to process</param>
        /// <returns>True if the command key is handled, false if not</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if(keyData == Keys.F1 && tvPropertyPages.SelectedNode != null)
            {
                var page = (BasePropertyPage)tvPropertyPages.SelectedNode.Tag;

                page.ShowHelp();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// This is overridden to ignore Ctrl+F4 which closes the window rather
        /// than hide it when docked as a document.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Update the last used property page index
            if(tvPropertyPages.SelectedNode != null)
                Settings.Default.LastUsedPropertyPage = tvPropertyPages.Nodes.IndexOf(tvPropertyPages.SelectedNode);

            if(e.CloseReason == CloseReason.UserClosing && this.DockState == DockState.Document)
            {
                if(this.Apply())
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
            set { /* Handled by the property pages and the main form */ }
        }

        /// <inheritdoc />
        public override bool CanClose
        {
            get
            {
                if(!this.Apply())
                    return false;

                return MainForm.Host.ProjectExplorer.CanClose;
            }
        }

        /// <inheritdoc />
        public override bool CanSaveContent
        {
            get { return MainForm.Host.ProjectExplorer.CanSaveContent; }
        }

        /// <inheritdoc />
        public override bool Save()
        {
            bool result = this.Apply();

            if(result)
            {
                result = MainForm.Host.ProjectExplorer.Save();

                if(result)
                    this.RefreshProperties();
            }
            else
                this.Activate();

            return result;
        }

        /// <inheritdoc />
        public override bool SaveAs()
        {
            bool result = this.Apply();

            if(result)
            {
                result = MainForm.Host.ProjectExplorer.SaveAs();

                if(result)
                    this.RefreshProperties();
            }
            else
                this.Activate();

            return result;
        }

        /// <summary>
        /// This is used to see if the current property page is valid and apply any pending changes to the
        /// project.
        /// </summary>
        /// <returns>True if successful or false if not</returns>
        public bool Apply()
        {
            if(tvPropertyPages.SelectedNode != null)
            {
                var page = (BasePropertyPage)tvPropertyPages.SelectedNode.Tag;

                return page.Apply();
            }

            return true;
        }

        /// <summary>
        /// This is called to refresh the property grid after a change that may have affected it.
        /// </summary>
        public void RefreshProperties()
        {
            if(currentProject != null)
                tvPropertyPages_AfterSelect(this, new TreeViewEventArgs(tvPropertyPages.SelectedNode));
        }
        #endregion
    }
}
