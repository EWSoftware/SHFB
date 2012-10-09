//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : DynamicReadOnlyPropertyDescriptor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/31/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a property descriptor that can be used to alter the
// read-only state of a property in a property grid at runtime based on other
// conditions.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/31/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This property descriptor can be used to alter the read-only state of a
    /// property in a property grid at runtime based on other conditions.
    /// </summary>
    /// <remarks>To use it, derive a new class and override the
    /// <see cref="IsReadOnly" /> property.</remarks>
    public class DynamicReadOnlyPropertyDescriptor : PropertyDescriptor
    {
        #region Private data members
        //=================================================================

        private ICustomTypeDescriptor typeDesc;
        private PropertyDescriptor propDesc;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the associated type descriptor
        /// </summary>
        public ICustomTypeDescriptor TypeDescriptor
        {
            get { return typeDesc; }
        }

        /// <summary>
        /// This is used to get the property descriptor wrapped by this class
        /// </summary>
        public PropertyDescriptor PropertyDescriptor
        {
            get { return propDesc; }
        }
        #endregion

        #region Constructor
        //=================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor</param>
        /// <param name="propertyDescriptor">The property descriptor</param>
        public DynamicReadOnlyPropertyDescriptor(
          ICustomTypeDescriptor typeDescriptor,
          PropertyDescriptor propertyDescriptor) : base(propertyDescriptor)
        {
            typeDesc = typeDescriptor;
            propDesc = propertyDescriptor;
        }
        #endregion

        #region Property descriptor implementation
        //=================================================================

        /// <inheritdoc />
        public override bool CanResetValue(object component)
        {
            return propDesc.CanResetValue(component);
        }

        /// <inheritdoc />
        public override Type ComponentType
        {
            get { return propDesc.ComponentType; }
        }

        /// <inheritdoc />
        public override object GetValue(object component)
        {
            return propDesc.GetValue(component);
        }

        /// <inheritdoc />
        /// <remarks>Derived classes must override this method</remarks>
        public override bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override Type PropertyType
        {
            get { return propDesc.PropertyType; }
        }

        /// <inheritdoc />
        public override void ResetValue(object component)
        {
            propDesc.ResetValue(component);
        }

        /// <inheritdoc />
        public override void SetValue(object component, object value)
        {
            propDesc.SetValue(component, value);
        }

        /// <inheritdoc />
        public override bool ShouldSerializeValue(object component)
        {
            return propDesc.ShouldSerializeValue(component);
        }
        #endregion
    }
}
