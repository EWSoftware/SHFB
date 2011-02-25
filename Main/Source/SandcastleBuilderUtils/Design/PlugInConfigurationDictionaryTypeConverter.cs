//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : PlugInConfigurationDictionaryTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/15/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter for the plug-in configuration
// dictionary.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.6  03/15/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;

using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to give a more descriptive message in the property
    /// grid for the <see cref="PlugInConfigurationDictionary"/>.
    /// </summary>
    internal sealed class PlugInConfigurationDictionaryTypeConverter : TypeConverter
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
            PlugInConfigurationDictionary items =
                value as PlugInConfigurationDictionary;
            int disabled = 0;

            if(items == null || destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            if(items.Count == 0)
                return "(None)";

            foreach(PlugInConfiguration pc in items.Values)
                if(!pc.Enabled)
                    disabled++;

            if(disabled == 0)
                return String.Format(culture, "{0} plug-in(s)", items.Count);

            return String.Format(culture, "{0} plug-in(s), " +
                "{1} disabled", items.Count, disabled);
        }
    }
}
