//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : DownloadCountConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/18/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a converter that converts download counts to a more compact form for display
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/17/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace SandcastleBuilder.WPF.Converters
{
    /// <summary>
    /// This value converter converts a download count to a more compact form for display (e.g. 1.2G, 2.5M, 523.2K)
    /// </summary>
    public class DownloadCountConverter: IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double downloadCount = (int)value;
            string convertedValue;

            // Display format is based on the behavior of the Visual Studio NuGet package manager tool window
            if(downloadCount >= 1000000000)
                convertedValue = $"{downloadCount / 1000000000:N1}G";
            else if(downloadCount >= 100000000)
                convertedValue = $"{downloadCount / 1000000:N0}M";
            else if(downloadCount >= 10000000)
                convertedValue = $"{downloadCount / 1000000:N2}M";
            else if(downloadCount >= 1000000)
                convertedValue = $"{downloadCount / 1000000:N1}M";
            else if(downloadCount >= 100000)
                convertedValue = $"{downloadCount / 1000:N0}K";
            else if(downloadCount >= 10000)
                convertedValue = $"{downloadCount / 1000:N1}K";
            else if(downloadCount >= 1000)
                convertedValue = $"{downloadCount / 1000:N2}K";
            else
                convertedValue = $"{downloadCount:N0}";

            if(convertedValue.EndsWith(".00", StringComparison.Ordinal))
                return convertedValue.Substring(0, convertedValue.Length - 3);

            if(convertedValue.EndsWith(".0", StringComparison.Ordinal))
                return convertedValue.Substring(0, convertedValue.Length - 2);

            return convertedValue;
        }

        /// <inheritdoc />
        /// <remarks>This is not implemented for this converter</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
