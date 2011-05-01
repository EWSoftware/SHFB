//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : UserDefinedPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/16/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the User Defined category properties.
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Visibility category project properties
    /// </summary>
    [Guid("D9EDDED9-4825-40F5-B394-262D9153C4E2")]
    public partial class UserDefinedPropertiesPageControl : BasePropertyPage
    {
        #region Private property editor class
        //=====================================================================

        /// <summary>
        /// This is used to edit the user-defined project property items
        /// </summary>
        private sealed class PropertyItem
        {
            #region Private data members
            //=====================================================================

            private ProjectProperty projProp;
            private string name, condition, propValue;
            #endregion

            #region Properties
            //=====================================================================

            /// <summary>
            /// This is used to get or set the owning property page
            /// </summary>
            [Browsable(false)]
            public UserDefinedPropertiesPageControl Owner { get; set; }

            /// <summary>
            /// The underlying project property if any
            /// </summary>
            /// <value>This returns null for new properties</value>
            [Browsable(false)]
            public ProjectProperty UnderlyingProperty
            {
                get { return projProp; }
            }

            /// <summary>
            /// This read-only property indicates whether or not the project
            /// property was modified.
            /// </summary>
            [Browsable(false)]
            public bool WasModified { get; private set; }

            /// <summary>
            /// This is used to get or set the property name
            /// </summary>
            /// <remarks>Existing properties cannot be renamed as the MSBuild
            /// project object doesn't provide a way to do it.</remarks>
            [Category("Name"), Description("The property name")]
            public string Name
            {
                get { return name; }
                set
                {
                    this.Owner.CheckProjectIsEditable(true);

                    if(String.IsNullOrEmpty(value))
                        throw new ArgumentException("Name cannot be null or blank");

                    if(projProp == null)
                    {
                        if(!this.Owner.Project.IsValidUserDefinedPropertyName(value))
                            throw new ArgumentException("The entered name matches " +
                                "an existing project or reserved property name");

                        if(this.Owner.UserDefinedProperties.Where(
                          p => p != this && p.Name == value).Count() != 0)
                            throw new ArgumentException("The entered name matches " +
                                "an existing user-defined property name");
                    }
                    else
                        throw new InvalidOperationException(
                            "Existing properties cannot be renamed via the designer");

                    name = value;
                    this.WasModified = true;
                }
            }

            /// <summary>
            /// This is used to get or set the Condition attribute value for
            /// the property.
            /// </summary>
            [Category("Value"), Description("An optional condition used to " +
              "determine when the property value is defined"),
              Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
            public string Condition
            {
                get { return condition; }
                set
                {
                    this.Owner.CheckProjectIsEditable(true);
                    condition = value;
                    this.WasModified = true;
                }
            }

            /// <summary>
            /// This is used to get or set the value for the property
            /// </summary>
            [Category("Value"), Description("The property value"),
              Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
            public string Value
            {
                get { return propValue; }
                set
                {
                    this.Owner.CheckProjectIsEditable(true);
                    propValue = value;
                    this.WasModified = true;
                }
            }
            #endregion

            #region Constructor
            //=====================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="owner">The owning property page</param>
            /// <param name="buildProperty">The build property to edit or null
            /// for a new property</param>
            public PropertyItem(UserDefinedPropertiesPageControl owner, ProjectProperty buildProperty)
            {
                string newPropName;
                int idx = 1;

                this.Owner = owner;
                projProp = buildProperty;

                if(projProp != null)
                {
                    name = projProp.Name;
                    condition = projProp.Xml.Condition;
                    propValue = projProp.UnevaluatedValue;
                }
                else
                {
                    do
                    {
                        newPropName = "NewProperty" + idx.ToString(CultureInfo.InvariantCulture);
                        idx++;

                    } while(!this.Owner.Project.IsValidUserDefinedPropertyName(newPropName) ||
                        this.Owner.UserDefinedProperties.Where(p => p.Name == newPropName).Count() != 0);

                    name = newPropName;
                    propValue = String.Empty;
                }
            }
            #endregion

            #region Method overrides
            //=====================================================================

            /// <summary>
            /// Return the name of the build property
            /// </summary>
            public override string ToString()
            {
                return this.Name;
            }
            #endregion
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the user-defined property collection
        /// </summary>
        private Collection<PropertyItem> UserDefinedProperties { get; set; }

        /// <summary>
        /// This returns a reference to the underlying Sandcastle project
        /// </summary>
        private SandcastleProject Project { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public UserDefinedPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "User Defined";
            this.HelpKeyword = "da405a33-3eeb-4451-9aa8-a55be5026434#UserDefProps";
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                ProjectProperty p;

                if(base.ProjectMgr != null)
                {
                    foreach(PropertyItem item in lbProperties.Items)
                        if(item.WasModified)
                        {
                            p = this.ProjectMgr.BuildProject.SetProperty(item.Name, item.Value);
                            p.Xml.Condition = item.Condition;
                        }

                    this.ProjectMgr.BuildProject.ReevaluateIfNecessary();
                }

                return true;
            }
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            PropertyItem propItem;

            if(this.ProjectMgr == null)
            {
                this.Enabled = false;
                return;
            }

            this.Project = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
            this.UserDefinedProperties = new Collection<PropertyItem>();

            lbProperties.Sorted = true;

            try
            {
                foreach(ProjectProperty prop in this.Project.GetUserDefinedProperties())
                {
                    propItem = new PropertyItem(this, prop);
                    this.UserDefinedProperties.Add(propItem);
                    lbProperties.Items.Add(propItem);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to load user-defined properties.  Error " + ex.Message,
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lbProperties.Sorted = false;

            if(lbProperties.Items.Count == 0)
                pgProps.Enabled = btnRemove.Enabled = false;
            else
                lbProperties.SelectedIndex = 0;
            
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the project can be edited.  If not, abort
        /// the change by throwing an exception.
        /// </summary>
        /// <param name="throwOnNotEditable">True to throw an exception if not editable, false to just return</param>
        /// <returns>If <paramref name="throwOnNotEditable"/> is false, it returns true if the project is
        /// editable, false if not.</returns>
        private bool CheckProjectIsEditable(bool throwOnNotEditable)
        {
            if(!base.ProjectMgr.QueryEditProjectFile(false))
            {
                if(throwOnNotEditable)
                    throw new OperationCanceledException("Project cannot be edited");

                return false;
            }

            this.IsDirty = true;
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Update the property grid with the selected item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProperties_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lbProperties.SelectedItem != null)
            {
                PropertyItem item = (PropertyItem)lbProperties.SelectedItem;
                pgProps.SelectedObject = item;
                pgProps.Enabled = btnRemove.Enabled = true;
            }
            else
            {
                pgProps.SelectedObject = null;
                pgProps.Enabled = btnRemove.Enabled = false;
            }

            pgProps.Refresh();
        }

        /// <summary>
        /// Refresh the list box item when a property changes
        /// </summary>
        /// <param name="s">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pgProps_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            lbProperties.Refresh(lbProperties.SelectedIndex);
        }

        /// <summary>
        /// This is used to add a new user-defined property to the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(this.CheckProjectIsEditable(false))
            {
                PropertyItem newProp = new PropertyItem(this, null);
                this.UserDefinedProperties.Add(newProp);

                lbProperties.SelectedIndex = lbProperties.Items.Add(newProp);
            }
        }

        /// <summary>
        /// Remove a user defined property
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            int idx = lbProperties.SelectedIndex;

            if(this.CheckProjectIsEditable(false))
            {
                PropertyItem p = lbProperties.SelectedItem as PropertyItem;

                if(p != null)
                {
                    if(p.UnderlyingProperty != null)
                        this.ProjectMgr.BuildProject.RemoveProperty(p.UnderlyingProperty);

                    this.UserDefinedProperties.Remove(p);
                    lbProperties.Items.Remove(p);

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
