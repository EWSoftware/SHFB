//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : VisibilityPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 04/17/2021
// Note    : Copyright 2017-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Visibility category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/23/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Windows;
using System.Windows.Controls;

using Sandcastle.Core;

using Sandcastle.Platform.Windows;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Visibility category properties
    /// </summary>
    public partial class VisibilityPropertiesPageContent : UserControl
    {
        #region Private data members
        //=====================================================================

        private ApiFilterCollection apiFilter;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the selected visible items values
        /// </summary>
        /// <value>The values for this page are stored in a single property so only one checkbox is bound and we
        /// get or set the values as a group.</value>
        public VisibleItems VisibleItems
        {
            get
            {
                VisibleItems items = VisibleItems.None;

                if(chkAttributes.IsChecked ?? false)
                    items |= VisibleItems.Attributes;

                if(chkExplicitInterfaceImplementations.IsChecked ?? false)
                    items |= VisibleItems.ExplicitInterfaceImplementations;

                if(chkInheritedFrameworkInternalMembers.IsChecked ?? false)
                    items |= VisibleItems.InheritedFrameworkInternalMembers;

                if(chkInheritedFrameworkMembers.IsChecked ?? false)
                    items |= VisibleItems.InheritedFrameworkMembers;

                if(chkInheritedFrameworkPrivateMembers.IsChecked ?? false)
                    items |= VisibleItems.InheritedFrameworkPrivateMembers;

                if(chkInheritedMembers.IsChecked ?? false)
                    items |= VisibleItems.InheritedMembers;

                if(chkInternals.IsChecked ?? false)
                    items |= VisibleItems.Internals;

                if(chkPrivateFields.IsChecked ?? false)
                    items |= VisibleItems.PrivateFields;

                if(chkPrivates.IsChecked ?? false)
                    items |= VisibleItems.Privates;

                if(chkProtected.IsChecked ?? false)
                    items |= VisibleItems.Protected;

                if(chkProtectedInternalAsProtected.IsChecked ?? false)
                    items |= VisibleItems.ProtectedInternalAsProtected;

                if(chkSealedProtected.IsChecked ?? false)
                    items |= VisibleItems.SealedProtected;

                if(chkNoPIATypes.IsChecked ?? false)
                    items |= VisibleItems.NoPIATypes;

                if(chkPublicCompilerGenerated.IsChecked ?? false)
                    items |= VisibleItems.PublicCompilerGenerated;

                if(chkEditorBrowsableNever.IsChecked ?? false)
                    items |= VisibleItems.EditorBrowsableNever;

                if(chkNonBrowsable.IsChecked ?? false)
                    items |= VisibleItems.NonBrowsable;

                return items;
            }
            set
            {
                chkAttributes.IsChecked = ((value & VisibleItems.Attributes) != 0);
                chkExplicitInterfaceImplementations.IsChecked = ((value & VisibleItems.ExplicitInterfaceImplementations) != 0);
                chkInheritedFrameworkInternalMembers.IsChecked = ((value & VisibleItems.InheritedFrameworkInternalMembers) != 0);
                chkInheritedFrameworkMembers.IsChecked = ((value & VisibleItems.InheritedFrameworkMembers) != 0);
                chkInheritedFrameworkPrivateMembers.IsChecked = ((value & VisibleItems.InheritedFrameworkPrivateMembers) != 0);
                chkInheritedMembers.IsChecked = ((value & VisibleItems.InheritedMembers) != 0);
                chkInternals.IsChecked = ((value & VisibleItems.Internals) != 0);
                chkPrivateFields.IsChecked = ((value & VisibleItems.PrivateFields) != 0);
                chkPrivates.IsChecked = ((value & VisibleItems.Privates) != 0);
                chkProtected.IsChecked = ((value & VisibleItems.Protected) != 0);
                chkProtectedInternalAsProtected.IsChecked = ((value & VisibleItems.ProtectedInternalAsProtected) != 0);
                chkSealedProtected.IsChecked = ((value & VisibleItems.SealedProtected) != 0);
                chkNoPIATypes.IsChecked = ((value & VisibleItems.NoPIATypes) != 0);
                chkPublicCompilerGenerated.IsChecked = ((value & VisibleItems.PublicCompilerGenerated) != 0);
                chkEditorBrowsableNever.IsChecked = ((value & VisibleItems.EditorBrowsableNever) != 0);
                chkNonBrowsable.IsChecked = ((value & VisibleItems.NonBrowsable) != 0);
            }
        }

        /// <summary>
        /// This is used to get or set the API filter
        /// </summary>
        public ApiFilterCollection ApiFilter
        {
            get => apiFilter;
            set
            {
                apiFilter = value;
                this.ApiFilterHasChanges = false;
            }
        }

        /// <summary>
        /// This is used to get or set whether the control has changes to the API filter
        /// </summary>
        public bool ApiFilterHasChanges { get; set; }

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when any of the unbound control property values changes
        /// </summary>
        public event EventHandler PropertyChanged;

        /// <summary>
        /// This event is used to indicate that the parent should apply any pending changes to the project as an
        /// action is about to occur that depends on the updated property values.
        /// </summary>
        public event EventHandler<ApplyChangesEventArgs> ApplyChanges;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public VisibilityPropertiesPageContent()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to update the API filter state label
        /// </summary>
        public void UpdateApiFilterInfo()
        {
            if(apiFilter.Count != 0)
                lblAPIFilterState.Content = "An API filter has been defined";
            else
                lblAPIFilterState.Content = "An API filter has not been defined";
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This notifies the parent control of changes to the unbound checkbox controls
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.PropertyChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedMembers_Click(object sender, RoutedEventArgs e)
        {
            if(!(chkInheritedMembers.IsChecked ?? false))
                chkInheritedFrameworkMembers.IsChecked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Framework Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedFrameworkMembers_Click(object sender, RoutedEventArgs e)
        {
            if(!(chkInheritedFrameworkMembers.IsChecked ?? false))
                chkInheritedFrameworkInternalMembers.IsChecked = chkInheritedFrameworkPrivateMembers.IsChecked = false;
            else
                chkInheritedMembers.IsChecked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Framework Internal Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedFrameworkInternalMembers_Click(object sender, RoutedEventArgs e)
        {
            if(chkInheritedFrameworkInternalMembers.IsChecked ?? false)
                chkInheritedFrameworkMembers.IsChecked = chkInheritedMembers.IsChecked = chkInternals.IsChecked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Inherited Framework Private Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInheritedFrameworkPrivateMembers_Click(object sender, RoutedEventArgs e)
        {
            if(chkInheritedFrameworkPrivateMembers.IsChecked ?? false)
                chkInheritedFrameworkMembers.IsChecked = chkInheritedMembers.IsChecked = chkPrivates.IsChecked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Internal Members state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkInternals_Click(object sender, RoutedEventArgs e)
        {
            if(!(chkInternals.IsChecked ?? false))
                chkInheritedFrameworkInternalMembers.IsChecked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Private Fields state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkPrivateFields_Click(object sender, RoutedEventArgs e)
        {
            if(chkPrivateFields.IsChecked ?? false)
                chkPrivates.IsChecked = true;
        }

        /// <summary>
        /// Update the other checkbox states based on the Privates state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkPrivates_Click(object sender, RoutedEventArgs e)
        {
            if(!(chkPrivates.IsChecked ?? false))
                chkPrivateFields.IsChecked = chkInheritedFrameworkPrivateMembers.IsChecked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Protected state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkProtected_Click(object sender, RoutedEventArgs e)
        {
            if(!(chkProtected.IsChecked ?? false))
                chkSealedProtected.IsChecked = false;
        }

        /// <summary>
        /// Update the other checkbox states based on the Sealed Protected state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkSealedProtected_Click(object sender, RoutedEventArgs e)
        {
            if(chkSealedProtected.IsChecked ?? false)
                chkProtected.IsChecked = true;
        }

        /// <summary>
        /// Edit the project's API filter
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnEditAPIFilter_Click(object sender, RoutedEventArgs e)
        {
            var args = new ApplyChangesEventArgs();

            this.ApplyChanges?.Invoke(this, args);

            if(args.ChangesApplied)
            {
                string oldFilter, newFilter;

                var dlg = new ApiFilterEditorDlg(apiFilter);

                oldFilter = apiFilter.ToXml();
                dlg.ShowModalDialog();
                newFilter= apiFilter.ToXml();

                // If it changes, mark the page as dirty and update the summary info
                if(oldFilter != newFilter)
                {
                    this.PropertyChanged?.Invoke(this, EventArgs.Empty);
                    this.ApiFilterHasChanges = true;
                    this.UpdateApiFilterInfo();
                }
            }
        }
        #endregion
    }
}
