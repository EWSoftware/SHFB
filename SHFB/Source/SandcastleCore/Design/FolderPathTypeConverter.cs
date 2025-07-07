//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : FolderPathTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a type converter used to convert FolderPath objects to and from strings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/29/2006  EFW  Created the code
// 06/19/2025  EFW  Moved From SandcastleBuilderUtils
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;

namespace Sandcastle.Core.Design
{
    /// <summary>
    /// This type converter is used to convert a FolderPath object to and from a string so that it can be edited
    /// in a <c>System.Windows.Forms.PropertyGrid</c>.
    /// </summary>
    public sealed class FolderPathTypeConverter : ExpandableObjectConverter
    {
        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
          Type destinationType)
        {
            FolderPath folderPath = value as FolderPath;

            if(destinationType == typeof(string) && folderPath != null)
                return folderPath.PersistablePath;

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
                    provider = ((FolderPath)context.PropertyDescriptor.GetValue(context.Instance)).BasePathProvider;

                return new FolderPath(s, provider);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
