//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : UserDefinedProperty.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2019-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to edit user-defined project properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/26/2017  EFW  Moved the class into its own file
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used to edit the user-defined project property items
    /// </summary>
    public sealed class UserDefinedProperty : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private string name, condition, propValue;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the owning property page
        /// </summary>
        [Browsable(false)]
        public UserDefinedPropertiesPageContent Owner { get; set; }

        /// <summary>
        /// The underlying project property if any
        /// </summary>
        /// <value>This returns null for new properties</value>
        [Browsable(false)]
        public ProjectProperty UnderlyingProperty { get; }

        /// <summary>
        /// This read-only property indicates whether or not the project property was modified
        /// </summary>
        [Browsable(false)]
        public bool WasModified { get; private set; }

        /// <summary>
        /// This is used to get or set the property name
        /// </summary>
        /// <remarks>Existing properties cannot be renamed as the MSBuild project object doesn't provide a
        /// way to do it.</remarks>
        [Category("Name"), Description("The property name")]
        public string Name
        {
            get => name;
            set
            {
                this.Owner.CheckProjectIsEditable(true);

                if(String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or blank");

                if(this.UnderlyingProperty == null)
                {
                    value = value.Trim();

                    if(!this.Owner.Project.IsValidUserDefinedPropertyName(value))
                        throw new ArgumentException("The entered name matches an existing project or " +
                            "reserved property name");

                    if(this.Owner.UserDefinedProperties.Any(p => p != this && p.Name == value))
                        throw new ArgumentException("The entered name matches an existing user-defined " +
                            "property name");
                }
                else
                    throw new InvalidOperationException("Existing properties cannot be renamed via the designer");

                name = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to get or set the Condition attribute value for the property
        /// </summary>
        [Category("Value"), Description("An optional condition used to determine when the property value " +
          "is defined"), Editor(typeof(LargeTextBoxEditor), typeof(LargeTextBoxEditor))]
        public string Condition
        {
            get => condition;
            set
            {
                this.Owner.CheckProjectIsEditable(true);
                condition = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to get or set the value for the property
        /// </summary>
        [Category("Value"), Description("The property value"), Editor(typeof(LargeTextBoxEditor),
          typeof(LargeTextBoxEditor))]
        public string Value
        {
            get => propValue;
            set
            {
                this.Owner.CheckProjectIsEditable(true);
                propValue = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">The owning property page</param>
        /// <param name="buildProperty">The build property to edit or null for a new property</param>
        public UserDefinedProperty(UserDefinedPropertiesPageContent owner, ProjectProperty buildProperty)
        {
            string newPropName;
            int idx = 1;

            this.Owner = owner;
            this.UnderlyingProperty = buildProperty;

            if(this.UnderlyingProperty != null)
            {
                name = this.UnderlyingProperty.Name;
                condition = this.UnderlyingProperty.Xml.Condition;
                propValue = this.UnderlyingProperty.UnevaluatedValue;
            }
            else
            {
                do
                {
                    newPropName = "NewProperty" + idx.ToString(CultureInfo.InvariantCulture);
                    idx++;

                } while(!this.Owner.Project.IsValidUserDefinedPropertyName(newPropName) ||
                    this.Owner.UserDefinedProperties.Any(p => p.Name == newPropName));

                name = newPropName;
                propValue = String.Empty;
            }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This is used to raise the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="caller">The name of the caller used as the property name</param>
        private void OnPropertyChanged([CallerMemberName]string caller = null)
        {
            this.WasModified = true;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
        #endregion
    }
}
