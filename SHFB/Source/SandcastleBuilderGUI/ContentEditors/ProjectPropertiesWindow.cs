//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ProjectPropertiesWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/10/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the project properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/27/2008  EFW  Created the code
// 10/28/2012  EFW  Rewrote to use the property pages from the VSPackage
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

using System;
using System.Linq;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Package.PropertyPages;
using SandcastleBuilder.Utils;

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
        /// This is used to get or set the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get => currentProject;
            set
            {
                currentProject = value;

                if(value == null || tvPropertyPages.Nodes.Count == 0)
                    this.LoadPropertyPages();
                else
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
            InitializeComponent();

            // Ensure that the custom controls are known by the base property page class
            if(!BasePropertyPage.CustomControls.ContainsKey("Sandcastle.Platform.Windows.UserControls.FilePathUserControl"))
            {
                BasePropertyPage.CustomControls.Add("Xceed.Wpf.Toolkit.IntegerUpDown", "Value");
                BasePropertyPage.CustomControls.Add("Sandcastle.Platform.Windows.UserControls.FilePathUserControl",
                    "PersistablePath");
                BasePropertyPage.CustomControls.Add("Sandcastle.Platform.Windows.UserControls.FolderPathUserControl",
                    "PersistablePath");
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================


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

                page.Visible = (currentProject != null);
                page.SetProject(currentProject);

                System.Drawing.Size minSize = (System.Drawing.Size)page.Tag;

                this.AutoScrollMinSize = new System.Drawing.Size(minSize.Width + page.Left,
                    minSize.Height + page.Top);
            }
        }

        /// <summary>
        /// Notify the main form when a property page is changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void page_DirtyChanged(object sender, EventArgs e)
        {
            MainForm.Host.UpdateFilenameInfo();
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

            if(e != null &&  e.CloseReason == CloseReason.UserClosing && this.DockState == DockState.Document)
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
            get => MainForm.Host.ProjectExplorer.IsDirty || this.Controls.OfType<BasePropertyPage>().Any(p => p.IsDirty);
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
        public override bool CanSaveContent => MainForm.Host.ProjectExplorer.CanSaveContent;

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

        /// <summary>
        /// Set the enabled state of the property page container
        /// </summary>
        /// <param name="enabled">True to enable the property pages, false to disable them</param>
        /// <remarks>This allows the property pages to be disabled during a build but still allows you to
        /// flip through them to see the active options.</remarks>
        public void SetEnabledState(bool enabled)
        {
            foreach(var c in this.Controls.OfType<BasePropertyPage>())
                c.Enabled = enabled;
        }

        /// <summary>
        /// This is used to the load property page controls
        /// </summary>
        private void LoadPropertyPages()
        {
            BasePropertyPage page;
            TreeNode node;

            // The property pages will be listed in this order
            Type[] propertyPages = new[] {
                typeof(BuildPropertiesPageControl),
                typeof(HelpFilePropertiesPageControl),
                typeof(Help1WebsitePropertiesPageControl),
                typeof(MSHelpViewerPropertiesPageControl),
                typeof(SummaryPropertiesPageControl),
                typeof(VisibilityPropertiesPageControl),
                typeof(MissingTagPropertiesPageControl),
                typeof(PathPropertiesPageControl),
                typeof(ComponentPropertiesPageControl),
                typeof(PlugInPropertiesPageControl),
                typeof(TransformArgumentsPageControl),
                typeof(UserDefinedPropertiesPageControl),
                typeof(BuildEventPropertiesPageControl)
            };

            try
            {
                // If pages already exist, dispose of them.  This gives us behavior similar to Visual Studio so
                // that when a new project is created or another project is loaded, default control values are
                // used for properties that are not present in the project.
                foreach(TreeNode n in tvPropertyPages.Nodes)
                {
                    Control c = (Control)n.Tag;

                    this.Controls.Remove(c);
                    c.Dispose();
                }

                tvPropertyPages.BeginUpdate();
                tvPropertyPages.Nodes.Clear();

                // Create the property pages
                foreach(Type pageType in propertyPages)
                {
                    page = (BasePropertyPage)Activator.CreateInstance(pageType);
                    page.Visible = false;
                    page.DirtyChanged += page_DirtyChanged;
                    page.Location = new System.Drawing.Point(226, 12);

                    // For the standalone GUI, we don't want it enforcing a minimum size.  We'll control it
                    // manually.  The controls are hosted on the property page rather than within a panel as
                    // ElementHosts within panels don't redraw correctly.
                    page.Tag = page.MinimumSize;
                    page.MinimumSize = System.Drawing.Size.Empty;

                    page.Size = new System.Drawing.Size(this.Width - page.Location.X - 12, tvPropertyPages.Height);
                    page.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

                    node = tvPropertyPages.Nodes.Add(page.Title);
                    node.Tag = page;

                    this.Controls.Add(page);
                }

                if(tvPropertyPages.Nodes.Count != 0)
                {
                    if(Settings.Default.LastUsedPropertyPage >= 0 &&
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
    }
}
