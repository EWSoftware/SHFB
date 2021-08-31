//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EnumerableStringConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/19/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a converter that converts an enumerable string value to a comma-separated string
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/19/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace SandcastleBuilder.WPF.Converters
{
    /// <summary>
    /// This value converter converts an enumerable string value to a comma-separated string
    /// </summary>
    public class EnumerableStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is IEnumerable<string> values)
                return String.Join(", ", values);

            return String.Empty;
        }

        /// <inheritdoc />
        /// <remarks>This is not implemented for this converter</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
