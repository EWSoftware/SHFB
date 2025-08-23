//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : VisibilityConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/23/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains a converter that converts various value types to a visibility states (hidden or visible)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/18/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SandcastleBuilder.WPF.Converters
{
    /// <summary>
    /// This value converter converts various value types to a visibility states (hidden or visible)
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = Visibility.Hidden;

            if(parameter == null || !Boolean.TryParse(parameter.ToString(), out bool invertResult))
                invertResult = false;

            if((value != null && value is string url && !String.IsNullOrWhiteSpace(url)) ||
              (value is bool boolValue && boolValue) || (value is int intValue && intValue != 0) ||
              (value is long longValue && longValue != 0))
            {
                result = Visibility.Visible;
            }

            if(invertResult)
                return result == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

            return result;
        }

        /// <inheritdoc />
        /// <remarks>This is not implemented for this converter</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
