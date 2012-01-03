//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : VisibilityPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Visibility category properties.
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
using System.Windows.Forms;

using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Visibility category project properties
    /// </summary>
    [Guid("318912BF-0E26-485C-9183-CC35B8D47591")]
    public partial class VisibilityPropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private string apiFilter;
        private bool filterChanged;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public VisibilityPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Visibility";
            this.HelpKeyword = "3c489bd6-598c-4684-aafb-fbe9400864d3";

            apiFilter = String.Empty;

            // We are responsible for connecting the change event to all of the unbound controls
            chkExplicitInterfaceImplementations.CheckedChanged += base.OnPropertyChanged;
            chkInheritedFrameworkInternalMembers.CheckedChanged += base.OnPropertyChanged;
            chkInheritedFrameworkMembers.CheckedChanged += base.OnPropertyChanged;
            chkInheritedFrameworkPrivateMembers.CheckedChanged += base.OnPropertyChanged;
            chkInheritedMembers.CheckedChanged += base.OnPropertyChanged;
            chkInternals.CheckedChanged += base.OnPropertyChanged;
            chkPrivateFields.CheckedChanged += base.OnPropertyChanged;
            chkPrivates.CheckedChanged += base.OnPropertyChanged;
            chkProtected.CheckedChanged += base.OnPropertyChanged;
            chkProtectedInternalAsProtected.CheckedChanged += base.OnPropertyChanged;
            chkSealedProtected.CheckedChanged += base.OnPropertyChanged;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to update the API filter state label
        /// </summary>
        private void UpdateApiFilterInfo()
        {
            if(apiFilter.Length != 0)
                lblAPIFilterState.Text = "An API filter has been defined";
            else
                lblAPIFilterState.Text = "An API filter has not been defined";
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>For this page, we only need to bind one control as all the values are stored
        /// in a single property.</remarks>
        protected override bool BindControlValue(Control control)
        {
            ProjectProperty projProp;
            VisibleItems items;

            if(this.ProjectMgr == null)
                return false;

            if(control.Name == "lblAPIFilterState")
            {
                projProp = this.ProjectMgr.BuildProject.GetProperty("ApiFilter");

                if(projProp != null)
                    apiFilter = projProp.UnevaluatedValue ?? String.Empty;

                this.UpdateApiFilterInfo();
                return true;
            }

            projProp = this.ProjectMgr.BuildProject.GetProperty("VisibleItems");

            // If not found or not valid, we'll ignore it and use the defaults
            if(projProp != null && Enum.TryParse<VisibleItems>(projProp.UnevaluatedValue, out items))
            {
                chkAttributes.Checked = ((items & VisibleItems.Attributes) != 0);
                chkExplicitInterfaceImplementations.Checked = ((items & VisibleItems.ExplicitInterfaceImplementations) != 0);
                chkInheritedFrameworkInternalMembers.Checked = ((items & VisibleItems.InheritedFrameworkInternalMembers) != 0);
                chkInheritedFrameworkMembers.Checked = ((items & VisibleItems.InheritedFrameworkMembers) != 0);
                chkInheritedFrameworkPrivateMembers.Checked = ((items & VisibleItems.InheritedFrameworkPrivateMembers) != 0);
                chkInheritedMembers.Checked = ((items & VisibleItems.InheritedMembers) != 0);
                chkInternals.Checked = ((items & VisibleItems.Internals) != 0);
                chkPrivateFields.Checked = ((items & VisibleItems.PrivateFields) != 0);
                chkPrivates.Checked = ((items & VisibleItems.Privates) != 0);
                chkProtected.Checked = ((items & VisibleItems.Protected) != 0);
                chkProtectedInternalAsProtected.Checked = ((items & VisibleItems.ProtectedInternalAsProtected) != 0);
                chkSealedProtected.Checked = ((items & VisibleItems.SealedProtected) != 0);
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>For this page, we only need to bind one control as all the values are stored
        /// in a single property.</remarks>
        protected override bool StoreControlValue(Control control)
        {
            VisibleItems items = VisibleItems.None;

            if(this.ProjectMgr == null)
                return false;

            if(control.Name == "lblAPIFilterState")
            {
                if(filterChanged)
                {
                    this.ProjectMgr.SetProjectProperty("ApiFilter", apiFilter);
                    filterChanged = false;
                }

                return true;
            }

            if(chkAttributes.Checked)
                items |= VisibleItems.Attributes;

            if(chkExplicitInterfaceImplementations.Checked)
                items |= VisibleItems.ExplicitInterfaceImplementations;
            
            if(chkInheritedFrameworkInternalMembers.Checked)
                items |= VisibleItems.InheritedFrameworkInternalMembers;
            
            if(chkInheritedFrameworkMembers.Checked)
                items |= VisibleItems.InheritedFrameworkMembers;
            
            if(chkInheritedFrameworkPrivateMembers.Checked)
                items |= VisibleItems.InheritedFrameworkPrivateMembers;
            
            if(chkInheritedMembers.Checked)
                items |= VisibleItems.InheritedMembers;
            
            if(chkInternals.Checked)
                items |= VisibleItems.Internals;
            
            if(chkPrivateFields.Checked)
                items |= VisibleItems.PrivateFields;
            
            if(chkPrivates.Checked)
                items |= VisibleItems.Privates;
            
            if(chkProtected.Checked)
                items |= VisibleItems.Protected;
            
            if(chkProtectedInternalAsProtected.Checked)
                items |= VisibleItems.ProtectedInternalAsProtected;
            
            if(chkSealedProtected.Checked)
                items |= VisibleItems.SealedProtected;

            this.ProjectMgr.SetProjectProperty("VisibleItems", items.ToString());

            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Update the other checkbox states based on the Inherited Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedMembers_CheckedChanged(object sender, EventArgs e)
        {
            if(!chkInheritedMembers.Checked)
                chkInheritedFrameworkMembers.Checked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Framework Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedFrameworkMembers_CheckedChanged(object sender, EventArgs e)
        {
            if(!chkInheritedFrameworkMembers.Checked)
                chkInheritedFrameworkInternalMembers.Checked = chkInheritedFrameworkPrivateMembers.Checked = false;
            else
                chkInheritedMembers.Checked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Framework Internal Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedFrameworkInternalMembers_CheckedChanged(object sender, EventArgs e)
        {
            if(chkInheritedFrameworkInternalMembers.Checked)
                chkInheritedFrameworkMembers.Checked = chkInheritedMembers.Checked = chkInternals.Checked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Framework Private Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedFrameworkPrivateMembers_CheckedChanged(object sender, EventArgs e)
        {
            if(chkInheritedFrameworkPrivateMembers.Checked)
                chkInheritedFrameworkMembers.Checked = chkInheritedMembers.Checked = chkPrivates.Checked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Internal Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInternals_CheckedChanged(object sender, EventArgs e)
        {
            if(!chkInternals.Checked)
                chkInheritedFrameworkInternalMembers.Checked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Private Fields state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkPrivateFields_CheckedChanged(object sender, EventArgs e)
        {
            if(chkPrivateFields.Checked)
                chkPrivates.Checked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Privates state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkPrivates_CheckedChanged(object sender, EventArgs e)
        {
            if(!chkPrivates.Checked)
                chkPrivateFields.Checked = chkInheritedFrameworkPrivateMembers.Checked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Protected state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkProtected_CheckedChanged(object sender, EventArgs e)
        {
            if(!chkProtected.Checked)
                chkSealedProtected.Checked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Sealed Protected state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkSealedProtected_CheckedChanged(object sender, EventArgs e)
        {
            if(chkSealedProtected.Checked)
                chkProtected.Checked = true;
        }

        /// <summary>
        /// Edit the project's API filter
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnEditAPIFilter_Click(object sender, EventArgs e)
        {
            if(this.ProjectMgr == null)
                return;

            // Apply any pending visibility changes first
            if(this.IsDirty && ((IPropertyPage)this).Apply() != VSConstants.S_OK)
                return;

            // Create an API filter collection that we can edit
            ApiFilterCollection filter = new ApiFilterCollection(
                ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject);
            filter.FromXml(apiFilter);

            using(ApiFilterEditorDlg dlg = new ApiFilterEditorDlg(filter))
            {
                dlg.ShowDialog();

                string newFilter = filter.ToXml();

                // If it changes, mark the page as dirty and update the local copy of the filter
                if(apiFilter != newFilter)
                {
                    apiFilter = newFilter;
                    filterChanged = true;
                    this.UpdateApiFilterInfo();
                }
            }
        }
        #endregion
    }
}
