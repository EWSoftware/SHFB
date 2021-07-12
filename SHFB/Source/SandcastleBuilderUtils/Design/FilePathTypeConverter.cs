//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FilePathTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/13/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a type converter used to convert FilePath objects to and from strings.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/29/2006  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter is used to convert a FilePath object to and from a string so that it can be edited
    /// in a <c>System.Windows.Forms.PropertyGrid</c>.
    /// </summary>
    public class FilePathTypeConverter : ExpandableObjectConverter
    {
        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
          Type destinationType)
        {
            FilePath filePath = value as FilePath;

            if(destinationType == typeof(string) && filePath != null)
                return filePath.PersistablePath;

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if(sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            IBasePathProvider provider;

            if(value is string s)
            {
                // Get the base path provider from the existing instance so that the new path is kept relative to it
                if(context == null)
                    provider = null;
                else
                    provider = ((FilePath)context.PropertyDescriptor.GetValue(context.Instance)).BasePathProvider;

                return new FilePath(s, provider);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
