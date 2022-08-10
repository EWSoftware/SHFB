//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : EnumValueFormat.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/17/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type used to define how enumeration values are formatted
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/17/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This enumerated value defines the enumeration value formats
    /// </summary>
    [Serializable]
    public enum EnumValueFormat
    {
        /// <summary>
        /// Display as an integer value
        /// </summary>
        IntegerValue,
        /// <summary>
        /// Display as a hex value
        /// </summary>
        HexValue,
        /// <summary>
        /// Display as bit flags (binary literal)
        /// </summary>
        BitFlags
    }
}
