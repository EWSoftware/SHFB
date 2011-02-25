//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FilePathTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/18/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter used to convert FilePath objects to and
// from strings.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.4.0  12/29/2006  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter is used to convert a FilePath object to and from
    /// a string so that it can be edited in a
    /// <see cref="System.Windows.Forms.PropertyGrid" />.
    /// </summary>
    public class FilePathTypeConverter : ExpandableObjectConverter
    {
        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture, object value, Type destinationType)
        {
            FilePath filePath = value as FilePath;

            if(destinationType == typeof(string) && filePath != null)
                return filePath.PersistablePath;

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context,
          Type sourceType)
        {
            if(sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture, object value)
        {
            IBasePathProvider provider;

            if(value is string)
            {
                // Get the base path provider from the existing instance so that
                // the new path is kept relative to it.
                if(context == null)
                    provider = null;
                else
                    provider = ((FilePath)context.PropertyDescriptor.GetValue(
                        context.Instance)).BasePathProvider;

                return new FilePath((string)value, provider);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
