//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : UserDefinedPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 04/17/2021
// Note    : Copyright 2017-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the User Defined category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/25/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
// ==============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the User Defined category properties
    /// </summary>
    public partial class UserDefinedPropertiesPageContent : UserControl
    {
        #region Private data members
        //=====================================================================

        private readonly BindingList<UserDefinedProperty> userDefinedProperties;
        private bool isLoading;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the user-defined property collection
        /// </summary>
        public IList<UserDefinedProperty> UserDefinedProperties => userDefinedProperties;

        /// <summary>
        /// This returns a reference to the underlying Sandcastle project
        /// </summary>
        public SandcastleProject Project { get; set; }

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised to see if the containing project is editable
        /// </summary>
        public event EventHandler<CancelEventArgs> ProjectIsEditable;

        /// <summary>
        /// This event is raised when any of the unbound control property values changes
        /// </summary>
        public event EventHandler PropertyChanged;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public UserDefinedPropertiesPageContent()
        {
            InitializeComponent();

            userDefinedProperties = new BindingList<UserDefinedProperty>();
            userDefinedProperties.ListChanged += (s, e) =>
            {
                if(!isLoading)
                    this.PropertyChanged?.Invoke(s, e);
            };
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the project can be edited.  If not, abort the change by throwing an exception
        /// </summary>
        /// <param name="throwOnNotEditable">True to throw an exception if not editable, false to just return</param>
        /// <returns>If <paramref name="throwOnNotEditable"/> is false, it returns true if the project is
        /// editable, false if not.</returns>
        public bool CheckProjectIsEditable(bool throwOnNotEditable)
        {
            CancelEventArgs ce = new CancelEventArgs();

            this.ProjectIsEditable?.Invoke(this, ce);

            if(ce.Cancel)
            {
                if(throwOnNotEditable)
                    throw new OperationCanceledException("Project cannot be edited");

                return false;
            }

            return true;
        }

        /// <summary>
        /// This is used to load the user-defined properties from the project
        /// </summary>
        public void LoadUserDefinedProperties()
        {
            UserDefinedProperty propItem;

            lbProperties.ItemsSource = null;

            try
            {
                isLoading = true;

                foreach(ProjectProperty prop in this.Project.UserDefinedProperties.OrderBy(p => p.Name))
                {
                    propItem = new UserDefinedProperty(this, prop);
                    userDefinedProperties.Add(propItem);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to load user-defined properties.  Error " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoading = false;
                lbProperties.ItemsSource = userDefinedProperties;
            }

            if(lbProperties.Items.Count == 0)
                pgProps.IsEnabled = btnRemove.IsEnabled = false;
            else
                lbProperties.SelectedIndex = 0;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Update the property grid with the selected item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lbProperties.SelectedItem != null)
            {
                pgProps.SelectedObject = lbProperties.SelectedItem;
                pgProps.IsEnabled = btnRemove.IsEnabled = true;
            }
            else
            {
                pgProps.SelectedObject = null;
                pgProps.IsEnabled = btnRemove.IsEnabled = false;
            }
        }

        /// <summary>
        /// This is used to add a new user-defined property to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(this.CheckProjectIsEditable(false))
            {
                UserDefinedProperty newProp = new UserDefinedProperty(this, null);
                this.UserDefinedProperties.Add(newProp);

                lbProperties.SelectedIndex = lbProperties.Items.Count - 1;
            }
        }

        /// <summary>
        /// Remove a user defined property
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbProperties.SelectedIndex;

            if(this.CheckProjectIsEditable(false))
            {
                if(lbProperties.SelectedItem is UserDefinedProperty p)
                {
                    if(p.UnderlyingProperty != null)
                        this.Project.MSBuildProject.RemoveProperty(p.UnderlyingProperty);

                    userDefinedProperties.Remove(p);

                    if(idx >= lbProperties.Items.Count)
                        idx--;

                    if(idx >= 0)
                        lbProperties.SelectedIndex = idx;
                }
            }
        }
        #endregion
    }
}
