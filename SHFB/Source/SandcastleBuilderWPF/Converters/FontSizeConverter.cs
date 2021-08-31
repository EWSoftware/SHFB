//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : FontSizeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/18/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a converter that adjusts a font size up or down a given percentage
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
using System.Windows.Data;

namespace SandcastleBuilder.WPF.Converters
{
    /// <summary>
    /// This value converter adjusts a font size up or down a given percentage
    /// </summary>
    public class FontSizeConverter : IValueConverter
    {
        /// <inheritdoc />
        /// <remarks>Positive parameter values scale up, negative parameter values scale down</remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double fontSize && parameter != null && Double.TryParse(parameter.ToString(), out double percentage))
                return fontSize + (fontSize * (percentage / 100));

            return value;
        }

        /// <inheritdoc />
        /// <remarks>This is not implemented for this converter</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
