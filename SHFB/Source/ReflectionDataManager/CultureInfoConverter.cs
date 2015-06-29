//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : CultureInfoConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/28/2015
// Note    : Copyright 2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a value converter that converts between CultureInfo and String
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/27/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace ReflectionDataManager
{
    /// <summary>
    /// This value converter converts between <see cref="CultureInfo"/> and <see cref="String"/>
    /// </summary>
    public class CultureInfoConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
                return String.Empty;

            return ((CultureInfo)value).Name;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo convertedCulture = null;

            if(value != null)
                try
                {
                    convertedCulture = new CultureInfo((string)value);
                }
                catch
                {
                    // Ignore errors and return null
                }

            return convertedCulture;
        }
    }
}
