//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : MSHelpAttrCollectionTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/26/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter for the help attribute collection.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  03/26/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to give a more descriptive message in the property
    /// grid for the <see cref="MSHelpAttrCollection"/>.
    /// </summary>
    internal sealed class MSHelpAttrCollectionTypeConverter : TypeConverter
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
            MSHelpAttrCollection items = value as MSHelpAttrCollection;

            if(items == null || destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            if(items.Count == 0)
                return "(None)";

            return String.Format(culture, "{0} attribute(s)", items.Count);
        }
    }
}
