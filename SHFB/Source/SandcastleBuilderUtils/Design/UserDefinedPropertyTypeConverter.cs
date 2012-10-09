//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : UserDefinedPropertyTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/18/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter for the user-defined project properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  12/18/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to give a more descriptive message in the property
    /// grid for the <see cref="SandcastleProject.UserDefinedProperties" />
    /// project property.
    /// </summary>
    internal sealed class UserDefinedPropertyTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context,
          Type destinationType)
        {
            return destinationType == typeof(string);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture, object value, Type destinationType)
        {
            SandcastleProject project = value as SandcastleProject;
            int count;

            if(project == null || destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            try
            {
                count = project.GetUserDefinedProperties().Count;
            }
            catch(Exception ex)
            {
                return "(Error: " + ex.Message + ")";
            }

            if(count == 0)
                return "(None)";

            return String.Format(culture, "{0} user-defined project properties",
                count);
        }
    }
}
