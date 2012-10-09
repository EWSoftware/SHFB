//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : NamespaceSummaryCollectionTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/23/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter for the namespace summary item
// collection.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to give a more descriptive message in the property
    /// grid for the <see cref="NamespaceSummaryItemCollection"/>.
    /// </summary>
    internal sealed class NamespaceSummaryItemCollectionTypeConverter : TypeConverter
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
            NamespaceSummaryItemCollection items = value as NamespaceSummaryItemCollection;
            int excluded = 0, summary = 0;

            if(items == null || destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            if(items.Count == 0)
                return "(None)";

            foreach(NamespaceSummaryItem nsi in items)
                if(!nsi.IsDocumented)
                    excluded++;
                else
                    if(!String.IsNullOrEmpty(nsi.Summary))
                        summary++;

            return String.Format(culture, "{0} with summary, {1} excluded",
                summary, excluded);
        }
    }
}
