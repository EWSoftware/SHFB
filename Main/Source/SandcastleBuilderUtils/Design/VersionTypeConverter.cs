//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : VersionTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/14/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that converts between a string and a
// System.Version object and vice versa.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  03/21/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter converts between a string and a
    /// <see cref="Version"/> object and vice versa.
    /// </summary>
    public class VersionTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context,
         Type sourceType)
        {
            if(sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        /// <returns>A <see cref="System.Version"/> object.  The value is
        /// always a four-part version number with any unused parts defaulted
        /// to zero.  If the new value is null or an empty string, null is
        /// returned.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture, object value)
        {
            Version v;
            string[] parts;
            string version;

            if(value is string)
            {
                version = (string)value;

                if(version == null || version.Trim().Length == 0)
                    return null;

                parts = version.Trim().Split(new char[] { '.' });

                switch(parts.Length)
                {
                    case 1:
                        v = new Version(Convert.ToInt32(parts[0],
                            CultureInfo.InvariantCulture), 0, 0, 0);
                        break;

                    case 2:
                        v = new Version(Convert.ToInt32(parts[0],
                            CultureInfo.InvariantCulture), Convert.ToInt32(
                            parts[1], CultureInfo.InvariantCulture), 0, 0);
                        break;

                    case 3:
                        v = new Version(Convert.ToInt32(parts[0],
                            CultureInfo.InvariantCulture), Convert.ToInt32(
                            parts[1], CultureInfo.InvariantCulture),
                            Convert.ToInt32(parts[2],
                            CultureInfo.InvariantCulture), 0);
                        break;

                    default:
                        v = new Version(Convert.ToInt32(parts[0],
                            CultureInfo.InvariantCulture), Convert.ToInt32(
                            parts[1], CultureInfo.InvariantCulture),
                            Convert.ToInt32(parts[2],
                            CultureInfo.InvariantCulture), Convert.ToInt32(
                            parts[3], CultureInfo.InvariantCulture));
                        break;
                }

                return v;
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture, object value, Type destinationType)
        {
            Version v;
            int parts = 4;

            if(destinationType == typeof(string) && value != null)
            {
                // Adjust count in case it isn't a four part version number
                v = (Version)value;

                if(v.Revision == -1)
                    parts--;

                if(v.Build == -1)
                    parts--;

                return v.ToString(parts);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
